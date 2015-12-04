using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Gameplay : Scene
{
    private float m_boardProgress;
    private Actor m_cardDrawStandIn;
    private bool m_criticalAssetsLoaded;
    private bool m_handleLastFatalBnetErrorNow;
    private BnetErrorInfo m_lastFatalBnetErrorInfo;
    private NameBannerGamePlayPhone m_nameBannerGamePlayPhone;
    private List<NameBanner> m_nameBanners = new List<NameBanner>();
    private Queue<List<Network.PowerHistory>> m_queuedPowerHistory = new Queue<List<Network.PowerHistory>>();
    private bool m_unloading;
    private static Gameplay s_instance;
    private const float TIME_TO_SET_ASYNC = 10f;

    public void AddGamePlayNameBannerPhone()
    {
        if (this.m_nameBannerGamePlayPhone == null)
        {
            AssetLoader.Get().LoadGameObject("NameBannerGamePlay_phone", new AssetLoader.GameObjectCallback(this.OnPlayerBannerLoaded), Player.Side.OPPOSING, false);
        }
    }

    private bool AreCriticalAssetsLoaded()
    {
        return this.m_criticalAssetsLoaded;
    }

    protected override void Awake()
    {
        Log.LoadingScreen.Print("Gameplay.Awake()", new object[0]);
        base.Awake();
        s_instance = this;
        GameState state = GameState.Initialize();
        if (this.ShouldHandleDisconnect())
        {
            Log.LoadingScreen.Print(LogLevel.Warning, "Gameplay.Awake() - DISCONNECTED", new object[0]);
            this.HandleDisconnect();
        }
        else
        {
            Network.Get().SetGameServerDisconnectEventListener(new Network.GameServerDisconnectEvent(this.OnDisconnect));
            CheatMgr.Get().RegisterCheatHandler("saveme", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_saveme), null, null, null);
            state.RegisterCreateGameListener(new GameState.CreateGameCallback(this.OnCreateGame));
            AssetLoader.Get().LoadGameObject("InputManager", new AssetLoader.GameObjectCallback(this.OnInputManagerLoaded), null, false);
            AssetLoader.Get().LoadGameObject("MulliganManager", new AssetLoader.GameObjectCallback(this.OnMulliganManagerLoaded), null, false);
            AssetLoader.Get().LoadGameObject("ThinkEmoteController", true, false);
            AssetLoader.Get().LoadActor("Card_Hidden", new AssetLoader.GameObjectCallback(this.OnCardDrawStandinLoaded), null, false);
            AssetLoader.Get().LoadGameObject("TurnStartManager", new AssetLoader.GameObjectCallback(this.OnTurnStartManagerLoaded), null, false);
            AssetLoader.Get().LoadGameObject("TargetReticleManager", new AssetLoader.GameObjectCallback(this.OnTargetReticleManagerLoaded), null, false);
            AssetLoader.Get().LoadGameObject("GameplayErrorManager", new AssetLoader.GameObjectCallback(this.OnGameplayErrorManagerLoaded), null, false);
            AssetLoader.Get().LoadGameObject("RemoteActionHandler", new AssetLoader.GameObjectCallback(this.OnRemoteActionHandlerLoaded), null, false);
            AssetLoader.Get().LoadGameObject("ChoiceCardMgr", new AssetLoader.GameObjectCallback(this.OnChoiceCardMgrLoaded), null, false);
            LoadingScreen.Get().RegisterFinishedTransitionListener(new LoadingScreen.FinishedTransitionCallback(this.OnTransitionFinished));
            this.m_boardProgress = -1f;
            this.ProcessGameSetupPacket();
        }
    }

    private bool CheckCriticalAssetLoads()
    {
        if (!this.m_criticalAssetsLoaded)
        {
            if (Board.Get() == null)
            {
                return false;
            }
            if (BoardCameras.Get() == null)
            {
                return false;
            }
            if (BoardStandardGame.Get() == null)
            {
                return false;
            }
            if (GameMgr.Get().IsTutorial() && (BoardTutorial.Get() == null))
            {
                return false;
            }
            if (MulliganManager.Get() == null)
            {
                return false;
            }
            if (TurnStartManager.Get() == null)
            {
                return false;
            }
            if (TargetReticleManager.Get() == null)
            {
                return false;
            }
            if (GameplayErrorManager.Get() == null)
            {
                return false;
            }
            if (EndTurnButton.Get() == null)
            {
                return false;
            }
            if (BigCard.Get() == null)
            {
                return false;
            }
            if (CardTypeBanner.Get() == null)
            {
                return false;
            }
            if (TurnTimer.Get() == null)
            {
                return false;
            }
            if (CardColorSwitcher.Get() == null)
            {
                return false;
            }
            if (RemoteActionHandler.Get() == null)
            {
                return false;
            }
            if (ChoiceCardMgr.Get() == null)
            {
                return false;
            }
            if (InputManager.Get() == null)
            {
                return false;
            }
            this.m_criticalAssetsLoaded = true;
            this.ProcessQueuedPowerHistory();
        }
        return true;
    }

    public static Gameplay Get()
    {
        return s_instance;
    }

    public Actor GetCardDrawStandIn()
    {
        return this.m_cardDrawStandIn;
    }

    private void HandleDisconnect()
    {
        Log.GameMgr.PrintWarning("Gameplay is handling a game disconnect.", new object[0]);
        if (!ReconnectMgr.Get().ReconnectFromGameplay() && !SpectatorManager.Get().HandleDisconnectFromGameplay())
        {
            DisconnectMgr.Get().DisconnectFromGameplay();
        }
    }

    private void HandleLastFatalBnetError()
    {
        if (this.m_lastFatalBnetErrorInfo != null)
        {
            if (this.m_handleLastFatalBnetErrorNow)
            {
                Network.Get().OnFatalBnetError(this.m_lastFatalBnetErrorInfo);
                this.m_handleLastFatalBnetErrorNow = false;
            }
            else
            {
                string key = (ApplicationMgr.AllowResetFromFatalError == null) ? "GAMEPLAY_DISCONNECT_BODY" : "GAMEPLAY_DISCONNECT_BODY_RESET";
                if (GameMgr.Get().IsSpectator())
                {
                    key = (ApplicationMgr.AllowResetFromFatalError == null) ? "GAMEPLAY_SPECTATOR_DISCONNECT_BODY" : "GAMEPLAY_SPECTATOR_DISCONNECT_BODY_RESET";
                }
                AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                    m_headerText = GameStrings.Get("GAMEPLAY_DISCONNECT_HEADER"),
                    m_text = GameStrings.Get(key),
                    m_showAlertIcon = true,
                    m_responseDisplay = AlertPopup.ResponseDisplay.OK,
                    m_responseCallback = new AlertPopup.ResponseCallback(this.OnBnetErrorResponse)
                };
                DialogManager.Get().ShowPopup(info);
            }
            this.m_lastFatalBnetErrorInfo = null;
        }
    }

    private void InitCardBacks()
    {
        int cardBackId = GameState.Get().GetFriendlySidePlayer().GetCardBackId();
        int opponentCardBackID = GameState.Get().GetOpposingSidePlayer().GetCardBackId();
        CardBackManager.Get().SetGameCardBackIDs(cardBackId, opponentCardBackID);
    }

    private bool IsDoneUpdatingGame()
    {
        if (!this.m_handleLastFatalBnetErrorNow)
        {
            if (Network.IsConnectedToGameServer())
            {
                return false;
            }
            if (GameState.Get().IsProcessingPowers())
            {
                return false;
            }
            if (!GameState.Get().IsGameOver())
            {
                return false;
            }
        }
        return true;
    }

    private bool IsHandlingNetworkProblem()
    {
        return (this.ShouldHandleDisconnect() || this.m_handleLastFatalBnetErrorNow);
    }

    private bool IsLeavingGameUnfinished()
    {
        if ((GameState.Get() != null) && GameState.Get().IsGameOver())
        {
            return false;
        }
        if (GameMgr.Get().IsReconnect())
        {
            return false;
        }
        if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FATAL_ERROR))
        {
            return false;
        }
        return true;
    }

    public override bool IsUnloading()
    {
        return this.m_unloading;
    }

    private void LayoutProgressGUI()
    {
        if (this.m_boardProgress >= 0f)
        {
            Vector2 vector = new Vector2(150f, 30f);
            Vector2 vector2 = new Vector2((Screen.width * 0.5f) - (vector.x * 0.5f), (Screen.height * 0.5f) - (vector.y * 0.5f));
            GUI.Box(new Rect(vector2.x, vector2.y, vector.x, vector.y), string.Empty);
            GUI.Box(new Rect(vector2.x, vector2.y, this.m_boardProgress * vector.x, vector.y), string.Empty);
            GUI.TextField(new Rect(vector2.x, vector2.y, vector.x, vector.y), string.Format("{0:0}%", this.m_boardProgress * 100f));
        }
    }

    private void LayoutSpellControllerKillTimeGUI()
    {
        DateTime? latestSpellControllerKillTime = GameState.Get().GetLatestSpellControllerKillTime();
        if (latestSpellControllerKillTime.HasValue)
        {
            Vector2 vector = new Vector2(150f, 40f);
            Vector2 vector3 = new Vector2(Screen.width - ((Screen.width * 0.01f) + vector.x), Screen.height - (vector.y + (Screen.height * 0.1f)));
            Color contentColor = GUI.contentColor;
            GUI.contentColor = Color.red;
            string text = string.Format("Last spell kill:\n{0}", latestSpellControllerKillTime.Value.ToShortTimeString());
            GUI.Box(new Rect(vector3.x, vector3.y, vector.x, vector.y), text);
            GUI.contentColor = contentColor;
        }
    }

    private void LoadBoard(Network.GameSetup setup)
    {
        string assetName;
        string board = Cheats.Get().GetBoard();
        if (string.IsNullOrEmpty(board))
        {
            assetName = GameDbf.Board.GetRecord(setup.Board).GetAssetName("PREFAB");
        }
        else
        {
            assetName = board;
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            assetName = string.Format("{0}_phone", assetName);
        }
        AssetLoader.Get().LoadBoard(assetName, new AssetLoader.GameObjectCallback(this.OnBoardLoaded), null, false);
    }

    [DebuggerHidden]
    private IEnumerator NotifyPlayersOfBoardLoad()
    {
        return new <NotifyPlayersOfBoardLoad>c__Iterator91();
    }

    private void OnAllOptions()
    {
        Network.Options options = Network.GetOptions();
        object[] args = new object[] { options.ID };
        Log.LoadingScreen.Print("Gameplay.OnAllOptions() - id={0}", args);
        GameState.Get().OnAllOptions(options);
    }

    private bool OnBnetError(BnetErrorInfo info, object userData)
    {
        if (!Network.Get().OnIgnorableBnetError(info))
        {
            if (this.m_handleLastFatalBnetErrorNow)
            {
                return true;
            }
            this.m_lastFatalBnetErrorInfo = info;
            switch (info.GetError())
            {
                case BattleNetErrors.ERROR_PARENTAL_CONTROL_RESTRICTION:
                case BattleNetErrors.ERROR_SESSION_DUPLICATE:
                    this.m_handleLastFatalBnetErrorNow = true;
                    break;
            }
        }
        return true;
    }

    private void OnBnetErrorResponse(AlertPopup.Response response, object userData)
    {
        if (ApplicationMgr.AllowResetFromFatalError != null)
        {
            ApplicationMgr.Get().Reset();
        }
        else
        {
            ApplicationMgr.Get().Exit();
        }
    }

    private void OnBoardCamerasLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnBoardCamerasLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = Board.Get().transform;
            PegUI.Get().SetInputCamera(Camera.main);
            AssetLoader.Get().LoadActor("CardTypeBanner", false, false);
            AssetLoader.Get().LoadActor("BigCard", false, false);
        }
    }

    private void OnBoardLoaded(string name, GameObject go, object callbackData)
    {
        this.m_boardProgress = -1f;
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnBoardLoaded() - FAILED to load board \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.GetComponent<Board>().SetBoardDbId(GameMgr.Get().GetGameSetup().Board);
            string str = (UniversalInputManager.UsePhoneUI == null) ? "BoardCameras" : "BoardCameras_phone";
            AssetLoader.Get().LoadGameObject(str, new AssetLoader.GameObjectCallback(this.OnBoardCamerasLoaded), null, false);
            AssetLoader.Get().LoadGameObject("BoardStandardGame", new AssetLoader.GameObjectCallback(this.OnBoardStandardGameLoaded), null, false);
            if (GameMgr.Get().IsTutorial())
            {
                AssetLoader.Get().LoadGameObject("BoardTutorial", new AssetLoader.GameObjectCallback(this.OnBoardTutorialLoaded), null, false);
            }
        }
    }

    private void OnBoardProgressUpdate(string name, float progress, object callbackData)
    {
        this.m_boardProgress = progress;
    }

    private void OnBoardStandardGameLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnBoardStandardGameLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = Board.Get().transform;
            AssetLoader.Get().LoadActor("EndTurnButton", new AssetLoader.GameObjectCallback(this.OnEndTurnButtonLoaded), null, false);
            AssetLoader.Get().LoadActor("TurnTimer", new AssetLoader.GameObjectCallback(this.OnTurnTimerLoaded), null, false);
        }
    }

    private void OnBoardTutorialLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnBoardTutorialLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = Board.Get().transform;
        }
    }

    private void OnCardDrawStandinLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnCardDrawStandinLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            this.m_cardDrawStandIn = go.GetComponent<Actor>();
            go.GetComponentInChildren<CardBackDisplay>().SetCardBack(true);
            this.m_cardDrawStandIn.Hide();
        }
    }

    private void OnChoiceCardMgrLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnChoiceCardMgrLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = base.transform;
        }
    }

    private void OnCreateGame(GameState.CreateGamePhase phase, object userData)
    {
        if (phase == GameState.CreateGamePhase.CREATING)
        {
            this.InitCardBacks();
            base.StartCoroutine(this.NotifyPlayersOfBoardLoad());
        }
        else if (phase == GameState.CreateGamePhase.CREATED)
        {
            CardBackManager.Get().UpdateAllCardBacks();
        }
    }

    private void OnDestroy()
    {
        Log.LoadingScreen.Print("Gameplay.OnDestroy()", new object[0]);
        s_instance = null;
    }

    private void OnDisconnect(BattleNetErrors error)
    {
        if (this.ShouldHandleDisconnect() && (error == BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED))
        {
            Network.Get().RemoveGameServerDisconnectEventListener(new Network.GameServerDisconnectEvent(this.OnDisconnect));
            this.HandleDisconnect();
        }
    }

    private void OnEndTurnButtonLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnEndTurnButtonLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            EndTurnButton component = go.GetComponent<EndTurnButton>();
            if (component == null)
            {
                UnityEngine.Debug.LogError(string.Format("Gameplay.OnEndTurnButtonLoaded() - ERROR \"{0}\" has no {1} component", name, typeof(EndTurnButton)));
            }
            else
            {
                component.transform.position = Board.Get().FindBone("EndTurnButton").position;
                foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.gameObject.GetComponent<TextMesh>() == null)
                    {
                        renderer.material.color = Board.Get().m_EndTurnButtonColor;
                    }
                }
            }
        }
    }

    private void OnEntitiesChosen()
    {
        Network.EntitiesChosen entitiesChosen = Network.GetEntitiesChosen();
        GameState.Get().OnEntitiesChosen(entitiesChosen);
    }

    private void OnEntityChoices()
    {
        Network.EntityChoices entityChoices = Network.GetEntityChoices();
        object[] args = new object[] { entityChoices.ID };
        Log.LoadingScreen.Print("Gameplay.OnEntityChoices() - id={0}", args);
        GameState.Get().OnEntityChoices(entityChoices);
    }

    private void OnGameplayErrorManagerLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.GameplayErrorManagerLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = base.transform;
        }
    }

    private void OnGUI()
    {
        this.LayoutProgressGUI();
        if ((GameState.Get() != null) && Options.Get().GetBool(Option.HUD))
        {
            this.LayoutSpellControllerKillTimeGUI();
        }
    }

    private void OnInputManagerLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnInputManagerLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = base.transform;
        }
    }

    private void OnMulliganManagerLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnMulliganManagerLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = base.transform;
        }
    }

    private void OnOptionRejected()
    {
        int nAckOption = Network.GetNAckOption();
        GameState.Get().OnOptionRejected(nAckOption);
    }

    private void OnPlayerBannerLoaded(string name, GameObject go, object callbackData)
    {
        Player.Side side = (Player.Side) ((int) callbackData);
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnPlayerBannerLoaded() - FAILED to load \"{0}\" side={1}", name, side.ToString()));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else if (UniversalInputManager.UsePhoneUI != null)
        {
            NameBannerGamePlayPhone component = go.GetComponent<NameBannerGamePlayPhone>();
            if (component != null)
            {
                this.m_nameBannerGamePlayPhone = component;
                this.m_nameBannerGamePlayPhone.SetPlayerSide(side);
            }
            else
            {
                NameBanner item = go.GetComponent<NameBanner>();
                item.SetPlayerSide(side);
                this.m_nameBanners.Add(item);
            }
        }
        else
        {
            NameBanner banner2 = go.GetComponent<NameBanner>();
            banner2.SetPlayerSide(side);
            this.m_nameBanners.Add(banner2);
        }
    }

    private void OnPowerHistory()
    {
        List<Network.PowerHistory> powerHistory = Network.GetPowerHistory();
        object[] args = new object[] { powerHistory.Count };
        Log.LoadingScreen.Print("Gameplay.OnPowerHistory() - powerList={0}", args);
        if (this.AreCriticalAssetsLoaded())
        {
            GameState.Get().OnPowerHistory(powerHistory);
        }
        else
        {
            this.m_queuedPowerHistory.Enqueue(powerHistory);
        }
    }

    private bool OnProcessCheat_saveme(string func, string[] args, string rawArgs)
    {
        GameState.Get().DebugNukeServerBlocks();
        return true;
    }

    private void OnRemoteActionHandlerLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnRemoteActionHandlerLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = base.transform;
        }
    }

    private void OnSpectatorNotify()
    {
        SpectatorNotify spectatorNotify = Network.GetSpectatorNotify();
        GameState.Get().OnSpectatorNotifyEvent(spectatorNotify);
    }

    private void OnTargetReticleManagerLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnTargetReticleManagerLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = base.transform;
            TargetReticleManager.Get().PreloadTargetArrows();
        }
    }

    private void OnTransitionFinished(bool cutoff, object userData)
    {
        LoadingScreen.Get().UnregisterFinishedTransitionListener(new LoadingScreen.FinishedTransitionCallback(this.OnTransitionFinished));
        if (!cutoff && !this.IsHandlingNetworkProblem())
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                if (GameState.Get().IsMulliganPhase())
                {
                    AssetLoader.Get().LoadGameObject("NameBannerRight_phone", new AssetLoader.GameObjectCallback(this.OnPlayerBannerLoaded), Player.Side.FRIENDLY, false);
                    AssetLoader.Get().LoadGameObject("NameBanner_phone", new AssetLoader.GameObjectCallback(this.OnPlayerBannerLoaded), Player.Side.OPPOSING, false);
                }
                else if (!GameMgr.Get().IsTutorial())
                {
                    this.AddGamePlayNameBannerPhone();
                }
            }
            else
            {
                string name = "NameBanner";
                if (!string.IsNullOrEmpty(GameState.Get().GetGameEntity().GetAlternatePlayerName()))
                {
                    name = "NameBannerLong";
                }
                AssetLoader.Get().LoadGameObject(name, new AssetLoader.GameObjectCallback(this.OnPlayerBannerLoaded), Player.Side.FRIENDLY, false);
                AssetLoader.Get().LoadGameObject("NameBanner", new AssetLoader.GameObjectCallback(this.OnPlayerBannerLoaded), Player.Side.OPPOSING, false);
            }
        }
    }

    private void OnTurnStartManagerLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnTurnStartManagerLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            go.transform.parent = base.transform;
        }
    }

    private void OnTurnTimerLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("Gameplay.OnTurnTimerLoaded() - FAILED to load \"{0}\"", name));
        }
        else if (this.IsHandlingNetworkProblem())
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            TurnTimer component = go.GetComponent<TurnTimer>();
            if (component == null)
            {
                UnityEngine.Debug.LogError(string.Format("Gameplay.OnTurnTimerLoaded() - ERROR \"{0}\" has no {1} component", name, typeof(TurnTimer)));
            }
            else
            {
                component.transform.position = Board.Get().FindBone("TurnTimerBone").position;
            }
        }
    }

    private void OnTurnTimerUpdate()
    {
        Network.TurnTimerInfo turnTimerInfo = Network.GetTurnTimerInfo();
        GameState.Get().OnTurnTimerUpdate(turnTimerInfo);
    }

    private void OnUserUI()
    {
        if (RemoteActionHandler.Get() != null)
        {
            RemoteActionHandler.Get().HandleAction(Network.GetUserUI());
        }
    }

    public override void PreUnload()
    {
        if ((Board.Get() != null) && (BoardCameras.Get() != null))
        {
            LoadingScreen.Get().SetFreezeFrameCamera(Camera.main);
            LoadingScreen.Get().SetTransitionAudioListener(BoardCameras.Get().GetAudioListener());
        }
    }

    private void ProcessGameSetupPacket()
    {
        Network.GameSetup gameSetup = GameMgr.Get().GetGameSetup();
        this.LoadBoard(gameSetup);
        GameState.Get().OnGameSetup(gameSetup);
    }

    private void ProcessQueuedPowerHistory()
    {
        while (this.m_queuedPowerHistory.Count > 0)
        {
            List<Network.PowerHistory> powerList = this.m_queuedPowerHistory.Dequeue();
            GameState.Get().OnPowerHistory(powerList);
        }
    }

    public void RemoveClassNames()
    {
        foreach (NameBanner banner in this.m_nameBanners)
        {
            banner.FadeClass();
        }
    }

    public void RemoveGamePlayNameBannerPhone()
    {
        if (this.m_nameBannerGamePlayPhone != null)
        {
            this.m_nameBannerGamePlayPhone.Unload();
        }
    }

    public void RemoveNameBanners()
    {
        foreach (NameBanner banner in this.m_nameBanners)
        {
            UnityEngine.Object.Destroy(banner.gameObject);
        }
        this.m_nameBanners.Clear();
    }

    public void SetGameStateBusy(bool busy, float delay)
    {
        if (delay <= Mathf.Epsilon)
        {
            GameState.Get().SetBusy(busy);
        }
        else
        {
            base.StartCoroutine(this.SetGameStateBusyWithDelay(busy, delay));
        }
    }

    [DebuggerHidden]
    private IEnumerator SetGameStateBusyWithDelay(bool busy, float delay)
    {
        return new <SetGameStateBusyWithDelay>c__Iterator92 { delay = delay, busy = busy, <$>delay = delay, <$>busy = busy };
    }

    private bool ShouldHandleDisconnect()
    {
        if (Network.IsConnectedToGameServer())
        {
            return false;
        }
        if (Network.WasGameConceded())
        {
            return false;
        }
        if (((!Network.WasDisconnectRequested() || (GameMgr.Get() == null)) || (!GameMgr.Get().IsSpectator() || GameState.Get().IsGameOverNowOrPending())) && ((GameState.Get() != null) && GameState.Get().IsGameOverNowOrPending()))
        {
            return false;
        }
        return true;
    }

    private void Start()
    {
        Log.LoadingScreen.Print("Gameplay.Start()", new object[0]);
        Network network = Network.Get();
        network.AddBnetErrorListener(new Network.BnetErrorCallback(this.OnBnetError));
        network.RegisterNetHandler(PegasusGame.PowerHistory.PacketID.ID, new Network.NetHandler(this.OnPowerHistory), null);
        network.RegisterNetHandler(AllOptions.PacketID.ID, new Network.NetHandler(this.OnAllOptions), null);
        network.RegisterNetHandler(PegasusGame.EntityChoices.PacketID.ID, new Network.NetHandler(this.OnEntityChoices), null);
        network.RegisterNetHandler(PegasusGame.EntitiesChosen.PacketID.ID, new Network.NetHandler(this.OnEntitiesChosen), null);
        network.RegisterNetHandler(PegasusGame.UserUI.PacketID.ID, new Network.NetHandler(this.OnUserUI), null);
        network.RegisterNetHandler(NAckOption.PacketID.ID, new Network.NetHandler(this.OnOptionRejected), null);
        network.RegisterNetHandler(PegasusGame.TurnTimer.PacketID.ID, new Network.NetHandler(this.OnTurnTimerUpdate), null);
        network.RegisterNetHandler(SpectatorNotify.PacketID.ID, new Network.NetHandler(this.OnSpectatorNotify), null);
        network.GetGameState();
    }

    public void SwapCardBacks()
    {
        int cardBackId = GameState.Get().GetOpposingSidePlayer().GetCardBackId();
        int id = GameState.Get().GetFriendlySidePlayer().GetCardBackId();
        GameState.Get().GetOpposingSidePlayer().SetCardBackId(id);
        GameState.Get().GetFriendlySidePlayer().SetCardBackId(cardBackId);
        CardBackManager.Get().SetGameCardBackIDs(cardBackId, id);
    }

    public override void Unload()
    {
        Log.LoadingScreen.Print("Gameplay.Unload()", new object[0]);
        this.m_unloading = true;
        bool flag = this.IsLeavingGameUnfinished();
        GameState.Shutdown();
        Network network = Network.Get();
        network.RemoveGameServerDisconnectEventListener(new Network.GameServerDisconnectEvent(this.OnDisconnect));
        network.RemoveBnetErrorListener(new Network.BnetErrorCallback(this.OnBnetError));
        network.RemoveNetHandler(PegasusGame.PowerHistory.PacketID.ID, new Network.NetHandler(this.OnPowerHistory));
        network.RemoveNetHandler(AllOptions.PacketID.ID, new Network.NetHandler(this.OnAllOptions));
        network.RemoveNetHandler(PegasusGame.EntityChoices.PacketID.ID, new Network.NetHandler(this.OnEntityChoices));
        network.RemoveNetHandler(PegasusGame.EntitiesChosen.PacketID.ID, new Network.NetHandler(this.OnEntitiesChosen));
        network.RemoveNetHandler(PegasusGame.UserUI.PacketID.ID, new Network.NetHandler(this.OnUserUI));
        network.RemoveNetHandler(NAckOption.PacketID.ID, new Network.NetHandler(this.OnOptionRejected));
        network.RemoveNetHandler(PegasusGame.TurnTimer.PacketID.ID, new Network.NetHandler(this.OnTurnTimerUpdate));
        network.RemoveNetHandler(SpectatorNotify.PacketID.ID, new Network.NetHandler(this.OnSpectatorNotify));
        CheatMgr.Get().UnregisterCheatHandler("saveme", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_saveme));
        if (flag)
        {
            if (GameMgr.Get().IsPendingAutoConcede())
            {
                Network.AutoConcede();
                GameMgr.Get().SetPendingAutoConcede(false);
            }
            Network.DisconnectFromGameServer();
        }
        foreach (NameBanner banner in this.m_nameBanners)
        {
            banner.Unload();
        }
        if (this.m_nameBannerGamePlayPhone != null)
        {
            this.m_nameBannerGamePlayPhone.Unload();
        }
        DefLoader.Get().ClearCardDefs();
        this.m_unloading = false;
    }

    private void Update()
    {
        this.CheckCriticalAssetLoads();
        Network.Get().ProcessNetwork();
        if (this.IsDoneUpdatingGame())
        {
            this.HandleLastFatalBnetError();
        }
        else if (this.AreCriticalAssetsLoaded())
        {
            GameState.Get().Update();
        }
    }

    public void UpdateEnemySideNameBannerName(string newName)
    {
        foreach (NameBanner banner in this.m_nameBanners)
        {
            if (banner.GetPlayerSide() == Player.Side.OPPOSING)
            {
                banner.SetName(newName);
            }
        }
    }

    public void UpdateFriendlySideMedalChange(MedalInfoTranslator medalInfo)
    {
        foreach (NameBanner banner in this.m_nameBanners)
        {
            if (banner.GetPlayerSide() == Player.Side.FRIENDLY)
            {
                banner.UpdateMedalChange(medalInfo);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <NotifyPlayersOfBoardLoad>c__Iterator91 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Map<int, Player>.ValueCollection.Enumerator <$s_560>__1;
        internal Player <player>__2;
        internal Map<int, Player> <playerMap>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (BoardStandardGame.Get() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<playerMap>__0 = GameState.Get().GetPlayerMap();
                    this.<$s_560>__1 = this.<playerMap>__0.Values.GetEnumerator();
                    try
                    {
                        while (this.<$s_560>__1.MoveNext())
                        {
                            this.<player>__2 = this.<$s_560>__1.Current;
                            this.<player>__2.OnBoardLoaded();
                        }
                    }
                    finally
                    {
                        this.<$s_560>__1.Dispose();
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <SetGameStateBusyWithDelay>c__Iterator92 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>busy;
        internal float <$>delay;
        internal bool busy;
        internal float delay;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    GameState.Get().SetBusy(this.busy);
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

