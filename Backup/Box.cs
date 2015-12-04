using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class Box : MonoBehaviour
{
    public AudioListener m_AudioListener;
    public BoxSpinner m_BottomSpinner;
    private List<ButtonPressListener> m_buttonPressListeners = new List<ButtonPressListener>();
    public BoxCamera m_Camera;
    public BoxMenuButton m_CollectionButton;
    public Color m_DisabledDrawerMaterial;
    public Color m_DisabledMaterial;
    public BoxDisk m_Disk;
    public BoxDrawer m_Drawer;
    public GameObject m_EmptyFourthButton;
    public Color m_EnabledDrawerMaterial;
    public Color m_EnabledMaterial;
    public BoxEventMgr m_EventMgr;
    public BoxMenuButton m_ForgeButton;
    public BoxDoor m_LeftDoor;
    public GameObject m_letterboxingContainer;
    public BoxLightMgr m_LightMgr;
    public BoxLogo m_Logo;
    public Camera m_NoFxCamera;
    public PackOpeningButton m_OpenPacksButton;
    private GameLayer m_originalDrawerLayer;
    private GameLayer m_originalLeftDoorLayer;
    private GameLayer m_originalRightDoorLayer;
    public GameObject m_OuterFrame;
    private int m_pendingEffects;
    private QuestLog m_questLog;
    public QuestLogButton m_QuestLogButton;
    private bool m_questLogLoading;
    private DataState m_questLogNetCacheDataState;
    private ButtonType? m_queuedButtonFire;
    public RibbonButtonsUI m_ribbonButtons;
    public BoxDoor m_RightDoor;
    public GameObject m_rootObject;
    private bool m_showRibbonButtons;
    public BoxMenuButton m_SoloAdventuresButton;
    public BoxStartButton m_StartButton;
    private State m_state = State.STARTUP;
    private BoxStateConfig[] m_stateConfigs;
    public BoxStateInfoList m_StateInfoList;
    private Queue<State> m_stateQueue = new Queue<State>();
    public StoreButton m_StoreButton;
    public GameObject m_tableTop;
    public string m_tavernBrawlActivateSound;
    public TavernBrawlMenuButton m_TavernBrawlButton;
    public GameObject m_TavernBrawlButtonActivateFX;
    public GameObject m_TavernBrawlButtonDeactivateFX;
    public GameObject m_TavernBrawlButtonVisual;
    public string m_tavernBrawlDeactivateSound;
    public string m_tavernBrawlPopdownSound;
    public string m_tavernBrawlPopupSound;
    private GameObject m_tempInputBlocker;
    public Texture2D m_textureCompressionTest;
    public BoxSpinner m_TopSpinner;
    public BoxMenuButton m_TournamentButton;
    private List<TransitionFinishedListener> m_transitionFinishedListeners = new List<TransitionFinishedListener>();
    private bool m_transitioningToSceneMode;
    private bool m_waitingForNetData;
    private bool m_waitingForSceneLoad;
    private static Box s_instance;
    private const string SHOW_LOG_COROUTINE = "ShowQuestLogWhenReady";

    public void AddButtonPressListener(ButtonPressCallback callback)
    {
        this.AddButtonPressListener(callback, null);
    }

    public void AddButtonPressListener(ButtonPressCallback callback, object userData)
    {
        ButtonPressListener item = new ButtonPressListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_buttonPressListeners.Contains(item))
        {
            this.m_buttonPressListeners.Add(item);
        }
    }

    public void AddTransitionFinishedListener(TransitionFinishedCallback callback)
    {
        this.AddTransitionFinishedListener(callback, null);
    }

    public void AddTransitionFinishedListener(TransitionFinishedCallback callback, object userData)
    {
        TransitionFinishedListener item = new TransitionFinishedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_transitionFinishedListeners.Contains(item))
        {
            this.m_transitionFinishedListeners.Add(item);
        }
    }

    private void Awake()
    {
        Log.LoadingScreen.Print("Box.Awake()", new object[0]);
        s_instance = this;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        this.InitializeStateConfigs();
        if (LoadingScreen.Get() != null)
        {
            LoadingScreen.Get().NotifyMainSceneObjectAwoke(base.gameObject);
        }
        this.m_originalLeftDoorLayer = (GameLayer) this.m_LeftDoor.gameObject.layer;
        this.m_originalRightDoorLayer = (GameLayer) this.m_RightDoor.gameObject.layer;
        this.m_originalDrawerLayer = (GameLayer) this.m_Drawer.gameObject.layer;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            if (TransformUtil.PhoneAspectRatioScale() < 0.99f)
            {
                GameUtils.InstantiateGameObject("Letterboxing", this.m_letterboxingContainer, false);
            }
            GameObject child = AssetLoader.Get().LoadGameObject("RibbonButtons_Phone", true, false);
            this.m_ribbonButtons = child.GetComponent<RibbonButtonsUI>();
            this.m_ribbonButtons.Toggle(false);
            GameUtils.SetParent(child, this.m_rootObject, false);
            AssetLoader.Get().LoadTexture("TheBox_Top_phone", new AssetLoader.ObjectCallback(this.OnBoxTopPhoneTextureLoaded), null, false);
        }
    }

    private bool CanEnableUIEvents()
    {
        if (this.HasPendingEffects())
        {
            return false;
        }
        if (this.m_stateQueue.Count > 0)
        {
            return false;
        }
        if (this.m_state == State.INVALID)
        {
            return false;
        }
        if (this.m_state == State.STARTUP)
        {
            return false;
        }
        if (this.m_state == State.LOADING)
        {
            return false;
        }
        if (this.m_state == State.OPEN)
        {
            return false;
        }
        return true;
    }

    public void ChangeLightState(BoxLightStateType stateType)
    {
        this.m_LightMgr.ChangeState(stateType);
    }

    public bool ChangeState(State state)
    {
        if (state == State.INVALID)
        {
            return false;
        }
        if (this.m_state == state)
        {
            return false;
        }
        if (this.HasPendingEffects())
        {
            this.QueueStateChange(state);
        }
        else
        {
            this.ChangeStateNow(state);
        }
        return true;
    }

    private void ChangeState_Closed()
    {
        this.m_state = State.CLOSED;
        this.ChangeStateUsingConfig();
    }

    private void ChangeState_Error()
    {
        this.m_state = State.ERROR;
        this.ChangeStateUsingConfig();
    }

    private void ChangeState_Hub()
    {
        this.m_state = State.HUB;
        this.HackRequestNetFeatures();
        this.ChangeStateUsingConfig();
    }

    private void ChangeState_HubWithDrawer()
    {
        this.m_state = State.HUB_WITH_DRAWER;
        this.m_Camera.EnableAccelerometer();
        this.HackRequestNetData();
        this.ChangeStateUsingConfig();
    }

    private void ChangeState_Loading()
    {
        this.m_state = State.LOADING;
        this.ChangeStateUsingConfig();
    }

    private void ChangeState_Open()
    {
        this.m_state = State.OPEN;
        this.ChangeStateUsingConfig();
    }

    private void ChangeState_PressStart()
    {
        this.m_state = State.PRESS_START;
        this.ChangeStateUsingConfig();
    }

    private void ChangeState_Startup()
    {
        this.m_state = State.STARTUP;
        this.ChangeStateUsingConfig();
    }

    private void ChangeStateNow(State state)
    {
        this.m_state = state;
        if (state == State.STARTUP)
        {
            this.ChangeState_Startup();
        }
        else if (state == State.PRESS_START)
        {
            this.ChangeState_PressStart();
        }
        else if (state == State.LOADING)
        {
            this.ChangeState_Loading();
        }
        else if (state == State.HUB)
        {
            this.ChangeState_Hub();
        }
        else if (state == State.HUB_WITH_DRAWER)
        {
            this.ChangeState_HubWithDrawer();
        }
        else if (state == State.OPEN)
        {
            this.ChangeState_Open();
        }
        else if (state == State.CLOSED)
        {
            this.ChangeState_Closed();
        }
        else if (state == State.ERROR)
        {
            this.ChangeState_Error();
        }
        else
        {
            UnityEngine.Debug.LogError(string.Format("Box.ChangeStateNow() - unhandled state {0}", state));
        }
        this.UpdateUIEvents();
    }

    private void ChangeStateQueued()
    {
        State state = this.m_stateQueue.Dequeue();
        this.ChangeStateNow(state);
    }

    private void ChangeStateToReflectSceneMode(SceneMgr.Mode mode)
    {
        State state = this.TranslateSceneModeToBoxState(mode);
        if (this.ChangeState(state))
        {
            this.m_transitioningToSceneMode = true;
        }
        BoxLightStateType stateType = this.TranslateSceneModeToLightState(mode);
        this.m_LightMgr.ChangeState(stateType);
    }

    private void ChangeStateUsingConfig()
    {
        BoxStateConfig config = this.m_stateConfigs[(int) this.m_state];
        if (!config.m_logoState.m_ignore)
        {
            this.m_Logo.ChangeState(config.m_logoState.m_state);
        }
        if (!config.m_startButtonState.m_ignore)
        {
            this.m_StartButton.ChangeState(config.m_startButtonState.m_state);
        }
        if (!config.m_doorState.m_ignore)
        {
            this.m_LeftDoor.ChangeState(config.m_doorState.m_state);
            this.m_RightDoor.ChangeState(config.m_doorState.m_state);
        }
        if (!config.m_diskState.m_ignore)
        {
            this.m_Disk.ChangeState(config.m_diskState.m_state);
        }
        if (!config.m_drawerState.m_ignore)
        {
            if (UniversalInputManager.UsePhoneUI == null)
            {
                this.m_Drawer.ChangeState(config.m_drawerState.m_state);
            }
            else
            {
                bool show = this.m_state == State.HUB_WITH_DRAWER;
                if (!show && (show != this.m_showRibbonButtons))
                {
                    this.ToggleRibbonUI(show);
                }
            }
        }
        if (!config.m_camState.m_ignore)
        {
            this.m_Camera.ChangeState(config.m_camState.m_state);
        }
        if (!config.m_fullScreenBlackState.m_ignore)
        {
            this.FullScreenBlack_ChangeState(config.m_fullScreenBlackState.m_state);
        }
    }

    private void ClearQueuedButtonFireEvent()
    {
        this.m_queuedButtonFire = null;
    }

    private int ComputeBoosterCount()
    {
        return NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>().GetTotalNumBoosters();
    }

    private void DestroyQuestLog()
    {
        base.StopCoroutine("ShowQuestLogWhenReady");
        if (this.m_questLog != null)
        {
            UnityEngine.Object.Destroy(this.m_questLog.gameObject);
        }
    }

    public void DisableButton(PegUIElement button)
    {
        button.SetEnabled(false);
        TooltipZone component = button.GetComponent<TooltipZone>();
        if (component != null)
        {
            component.HideTooltip();
        }
    }

    private void DoTavernBrawlButtonInitialization()
    {
        TavernBrawlManager manager = TavernBrawlManager.Get();
        NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
        if (netObject == null)
        {
            NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheFeatures), new System.Action(this.DoTavernBrawlButtonInitialization));
        }
        else
        {
            NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheFeatures), new System.Action(this.DoTavernBrawlButtonInitialization));
            if ((((netObject != null) && netObject.Games.TavernBrawl) && manager.HasUnlockedTavernBrawl) && manager.IsTavernBrawlInfoReady)
            {
                manager.OnTavernBrawlUpdated -= new System.Action(this.DoTavernBrawlButtonInitialization);
                if (!manager.IsTavernBrawlActive || manager.IsFirstTimeSeeingCurrentSeason)
                {
                    this.PlayTavernBrawlButtonActivation(false, true);
                }
            }
        }
    }

    public void EnableButton(PegUIElement button)
    {
        button.SetEnabled(true);
    }

    private void FireButtonPressEvent(ButtonType buttonType)
    {
        if (this.m_waitingForSceneLoad)
        {
            this.m_queuedButtonFire = new ButtonType?(buttonType);
        }
        else
        {
            foreach (ButtonPressListener listener in this.m_buttonPressListeners.ToArray())
            {
                listener.Fire(buttonType);
            }
        }
    }

    private void FireTransitionFinishedEvent()
    {
        foreach (TransitionFinishedListener listener in this.m_transitionFinishedListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private void FullScreenBlack_ChangeState(bool enable)
    {
        this.FullScreenBlack_UpdateState(enable);
    }

    private void FullScreenBlack_UpdateState(bool enable)
    {
        FullScreenEffects component = this.m_Camera.GetComponent<FullScreenEffects>();
        component.BlendToColorEnable = enable;
        if (enable)
        {
            component.BlendToColor = Color.black;
            component.BlendToColorAmount = 1f;
        }
    }

    public static Box Get()
    {
        return s_instance;
    }

    public AudioListener GetAudioListener()
    {
        return this.m_AudioListener;
    }

    public BoxCamera GetBoxCamera()
    {
        return this.m_Camera;
    }

    public Camera GetCamera()
    {
        return this.m_Camera.GetComponent<Camera>();
    }

    public BoxEventMgr GetEventMgr()
    {
        return this.m_EventMgr;
    }

    public Spell GetEventSpell(BoxEventType eventType)
    {
        return this.m_EventMgr.GetEventSpell(eventType);
    }

    public BoxLightMgr GetLightMgr()
    {
        return this.m_LightMgr;
    }

    public BoxLightStateType GetLightState()
    {
        return this.m_LightMgr.GetActiveState();
    }

    public Camera GetNoFxCamera()
    {
        return this.m_NoFxCamera;
    }

    public State GetState()
    {
        return this.m_state;
    }

    public Texture2D GetTextureCompressionTestTexture()
    {
        return this.m_textureCompressionTest;
    }

    private void HackRequestNetData()
    {
        this.m_waitingForNetData = true;
        this.UpdateUI(false);
        NetCache.Get().ReloadNetObject<NetCache.NetCacheBoosters>();
        NetCache.Get().ReloadNetObject<NetCache.NetCacheFeatures>();
    }

    private void HackRequestNetFeatures()
    {
        this.m_waitingForNetData = true;
        this.UpdateUI(false);
        NetCache.Get().ReloadNetObject<NetCache.NetCacheFeatures>();
    }

    public bool HasPendingEffects()
    {
        return (this.m_pendingEffects > 0);
    }

    private void HighlightButton(BoxMenuButton button, bool highlightOn)
    {
        if (button.m_HighlightState == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("Box.HighlighButton {0} - highlight state is null", button));
        }
        else
        {
            ActorStateType stateType = !highlightOn ? ActorStateType.HIGHLIGHT_OFF : ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE;
            button.m_HighlightState.ChangeState(stateType);
        }
    }

    private void InitializeComponents()
    {
        this.m_Logo.SetParent(this);
        this.m_Logo.SetInfo(this.m_StateInfoList.m_LogoInfo);
        this.m_StartButton.SetParent(this);
        this.m_StartButton.SetInfo(this.m_StateInfoList.m_StartButtonInfo);
        this.m_LeftDoor.SetParent(this);
        this.m_LeftDoor.SetInfo(this.m_StateInfoList.m_LeftDoorInfo);
        this.m_RightDoor.SetParent(this);
        this.m_RightDoor.SetInfo(this.m_StateInfoList.m_RightDoorInfo);
        this.m_RightDoor.EnableMaster(true);
        this.m_Disk.SetParent(this);
        this.m_Disk.SetInfo(this.m_StateInfoList.m_DiskInfo);
        this.m_TopSpinner.SetParent(this);
        this.m_TopSpinner.SetInfo(this.m_StateInfoList.m_SpinnerInfo);
        this.m_BottomSpinner.SetParent(this);
        this.m_BottomSpinner.SetInfo(this.m_StateInfoList.m_SpinnerInfo);
        this.m_Drawer.SetParent(this);
        this.m_Drawer.SetInfo(this.m_StateInfoList.m_DrawerInfo);
        this.m_Camera.SetParent(this);
        this.m_Camera.SetInfo(this.m_StateInfoList.m_CameraInfo);
        this.m_Camera.GetComponent<FullScreenEffects>().BlendToColorEnable = false;
    }

    public void InitializeNet()
    {
        if (SceneMgr.Get() != null)
        {
            this.m_waitingForNetData = true;
            if (SceneMgr.Get().GetMode() != SceneMgr.Mode.STARTUP)
            {
                NetCache.Get().RegisterScreenBox(new NetCache.NetCacheCallback(this.OnNetCacheReady));
            }
        }
    }

    private void InitializeState()
    {
        this.m_state = State.STARTUP;
        bool flag = GameMgr.Get().WasTutorial() && !GameMgr.Get().WasSpectator();
        SceneMgr mgr = SceneMgr.Get();
        if (mgr != null)
        {
            if (flag)
            {
                this.m_state = State.LOADING;
            }
            else
            {
                mgr.RegisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
                mgr.RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
                this.m_state = this.TranslateSceneModeToBoxState(mgr.GetMode());
            }
        }
        this.UpdateState();
        this.m_TopSpinner.Spin();
        this.m_BottomSpinner.Spin();
        if (flag)
        {
            LoadingScreen.Get().RegisterPreviousSceneDestroyedListener(new LoadingScreen.PreviousSceneDestroyedCallback(this.OnTutorialSceneDestroyed));
        }
        if (this.m_state == State.HUB_WITH_DRAWER)
        {
            this.ToggleRibbonUI(true);
        }
    }

    private void InitializeStateConfigs()
    {
        int length = Enum.GetValues(typeof(State)).Length;
        this.m_stateConfigs = new BoxStateConfig[length];
        this.m_stateConfigs[1] = new BoxStateConfig { m_logoState = { m_state = BoxLogo.State.HIDDEN }, m_startButtonState = { m_state = BoxStartButton.State.HIDDEN }, m_fullScreenBlackState = { m_state = true } };
        this.m_stateConfigs[2] = new BoxStateConfig { m_fullScreenBlackState = { m_ignore = true } };
        this.m_stateConfigs[3] = new BoxStateConfig { m_logoState = { m_state = BoxLogo.State.HIDDEN }, m_startButtonState = { m_state = BoxStartButton.State.HIDDEN }, m_drawerState = { m_state = BoxDrawer.State.OPENED }, m_camState = { m_state = BoxCamera.State.CLOSED_WITH_DRAWER }, m_fullScreenBlackState = { m_ignore = true } };
        this.m_stateConfigs[4] = new BoxStateConfig { m_logoState = { m_state = BoxLogo.State.HIDDEN }, m_startButtonState = { m_state = BoxStartButton.State.HIDDEN }, m_diskState = { m_state = BoxDisk.State.MAINMENU }, m_fullScreenBlackState = { m_ignore = true } };
        this.m_stateConfigs[5] = new BoxStateConfig { m_logoState = { m_state = BoxLogo.State.HIDDEN }, m_startButtonState = { m_state = BoxStartButton.State.HIDDEN }, m_diskState = { m_state = BoxDisk.State.MAINMENU }, m_drawerState = { m_state = BoxDrawer.State.OPENED }, m_camState = { m_state = BoxCamera.State.CLOSED_WITH_DRAWER }, m_fullScreenBlackState = { m_ignore = true } };
        this.m_stateConfigs[6] = new BoxStateConfig { m_logoState = { m_state = BoxLogo.State.HIDDEN }, m_startButtonState = { m_state = BoxStartButton.State.HIDDEN }, m_doorState = { m_state = BoxDoor.State.OPENED }, m_drawerState = { m_state = BoxDrawer.State.OPENED }, m_camState = { m_state = BoxCamera.State.OPENED }, m_fullScreenBlackState = { m_ignore = true } };
        this.m_stateConfigs[7] = new BoxStateConfig { m_logoState = { m_state = BoxLogo.State.HIDDEN }, m_startButtonState = { m_state = BoxStartButton.State.HIDDEN } };
        this.m_stateConfigs[8] = new BoxStateConfig { m_logoState = { m_state = BoxLogo.State.HIDDEN }, m_startButtonState = { m_state = BoxStartButton.State.HIDDEN } };
    }

    private void InitializeUI()
    {
        PegUI.Get().SetInputCamera(this.m_Camera.GetComponent<Camera>());
        this.m_StartButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnStartButtonPressed));
        switch (InputUtil.GetInputScheme())
        {
            case InputScheme.TOUCH:
                this.m_StartButton.SetText(GameStrings.Get("GLUE_START_TOUCH"));
                break;

            case InputScheme.GAMEPAD:
                this.m_StartButton.SetText(GameStrings.Get("GLUE_START_PRESS"));
                break;

            case InputScheme.KEYBOARD_MOUSE:
                this.m_StartButton.SetText(GameStrings.Get("GLUE_START_CLICK"));
                break;
        }
        this.m_TournamentButton.SetText(GameStrings.Get("GLUE_TOURNAMENT"));
        this.m_SoloAdventuresButton.SetText(GameStrings.Get("GLUE_ADVENTURE"));
        this.m_ForgeButton.SetText(GameStrings.Get("GLUE_FORGE"));
        this.m_TavernBrawlButton.SetText(GameStrings.Get("GLOBAL_TAVERN_BRAWL"));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_ribbonButtons.m_collectionManagerRibbon.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCollectionButtonPressed));
            this.m_ribbonButtons.m_packOpeningRibbon.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnOpenPacksButtonPressed));
            this.m_ribbonButtons.m_questLogRibbon.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnQuestButtonPressed));
            this.m_ribbonButtons.m_storeRibbon.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnStoreButtonReleased));
        }
        else
        {
            this.m_OpenPacksButton.SetText(GameStrings.Get("GLUE_OPEN_PACKS"));
            this.m_CollectionButton.SetText(GameStrings.Get("GLUE_MY_COLLECTION"));
            this.m_QuestLogButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnQuestButtonPressed));
            this.m_StoreButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnStoreButtonReleased));
        }
        this.RegisterButtonEvents(this.m_TournamentButton);
        this.RegisterButtonEvents(this.m_SoloAdventuresButton);
        this.RegisterButtonEvents(this.m_ForgeButton);
        this.RegisterButtonEvents(this.m_TavernBrawlButton);
        this.RegisterButtonEvents(this.m_OpenPacksButton);
        this.RegisterButtonEvents(this.m_CollectionButton);
        this.UpdateUI(true);
    }

    public bool IsBusy()
    {
        return (this.HasPendingEffects() || (this.m_stateQueue.Count > 0));
    }

    public bool IsTransitioningToSceneMode()
    {
        return this.m_transitioningToSceneMode;
    }

    public void OnAnimFinished()
    {
        this.m_pendingEffects--;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_OuterFrame.SetActive(false);
        }
        if (!this.HasPendingEffects())
        {
            if (this.m_stateQueue.Count == 0)
            {
                this.UpdateUIEvents();
                if (this.m_transitioningToSceneMode)
                {
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        bool show = this.m_state == State.HUB_WITH_DRAWER;
                        if (show != this.m_showRibbonButtons)
                        {
                            this.ToggleRibbonUI(show);
                        }
                    }
                    this.FireTransitionFinishedEvent();
                    this.m_transitioningToSceneMode = false;
                    if ((SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB) && AchieveManager.Get().HasActiveQuests(true))
                    {
                        WelcomeQuests.Show(false, null, false);
                    }
                }
            }
            else
            {
                this.ChangeStateQueued();
            }
        }
    }

    public void OnAnimStarted()
    {
        this.m_pendingEffects++;
    }

    private void OnBoxTopPhoneTextureLoaded(string name, UnityEngine.Object obj, object callbackData)
    {
        Texture texture = obj as Texture;
        foreach (MeshRenderer renderer in base.gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            Material sharedMaterial = renderer.sharedMaterial;
            if (sharedMaterial != null)
            {
                Texture texture2 = sharedMaterial.GetTexture(0);
                if ((texture2 != null) && texture2.name.Equals("TheBox_Top"))
                {
                    renderer.material.SetTexture(0, texture);
                }
            }
        }
    }

    private void OnButtonMouseOut(UIEvent e)
    {
        TooltipZone component = e.GetElement().gameObject.GetComponent<TooltipZone>();
        if (component != null)
        {
            component.HideTooltip();
        }
    }

    private void OnButtonMouseOver(UIEvent e)
    {
        TooltipZone component = e.GetElement().gameObject.GetComponent<TooltipZone>();
        if (component != null)
        {
            bool flag = (AchieveManager.Get() != null) && AchieveManager.Get().HasUnlockedFeature(Achievement.UnlockableFeature.FORGE);
            string str = GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
            string bodytext = str;
            NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
            bool tournament = netObject.Games.Tournament;
            bool practice = netObject.Games.Practice;
            bool flag4 = netObject.Games.Forge && flag;
            if (flag4)
            {
                flag4 = HealthyGamingMgr.Get().isArenaEnabled();
            }
            bool manager = netObject.Collection.Manager;
            if ((component.targetObject == this.m_TournamentButton.gameObject) && tournament)
            {
                bodytext = GameStrings.Get("GLUE_TOOLTIP_BUTTON_TOURNAMENT_DESC");
            }
            else if ((component.targetObject == this.m_SoloAdventuresButton.gameObject) && practice)
            {
                bodytext = GameStrings.Get("GLUE_TOOLTIP_BUTTON_ADVENTURE_DESC");
            }
            else if (component.targetObject == this.m_ForgeButton.gameObject)
            {
                if (flag4)
                {
                    bodytext = GameStrings.Get("GLUE_TOOLTIP_BUTTON_FORGE_DESC");
                }
                else if (!flag)
                {
                    object[] args = new object[] { 20 };
                    bodytext = GameStrings.Format("GLUE_TOOLTIP_BUTTON_FORGE_NOT_UNLOCKED", args);
                }
            }
            else if (component.targetObject == this.m_TavernBrawlButton.gameObject)
            {
                if (netObject.Games.TavernBrawl)
                {
                    if (TavernBrawlManager.Get().HasUnlockedTavernBrawl)
                    {
                        bodytext = GameStrings.Get("GLUE_TOOLTIP_BUTTON_TAVERN_BRAWL_DESC");
                    }
                    else
                    {
                        bodytext = GameStrings.Get("GLUE_TOOLTIP_BUTTON_TAVERN_BRAWL_NOT_UNLOCKED");
                    }
                }
                else
                {
                    bodytext = str;
                }
            }
            else if (component.targetObject == this.m_OpenPacksButton.gameObject)
            {
                bodytext = GameStrings.Get("GLUE_TOOLTIP_BUTTON_PACKOPEN_DESC");
            }
            else if ((component.targetObject == this.m_CollectionButton.gameObject) && manager)
            {
                bodytext = GameStrings.Get("GLUE_TOOLTIP_BUTTON_COLLECTION_DESC");
            }
            if (component.targetObject == this.m_TournamentButton.gameObject)
            {
                component.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_TOURNAMENT_HEADLINE"), bodytext);
            }
            else if (component.targetObject == this.m_SoloAdventuresButton.gameObject)
            {
                component.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_ADVENTURE_HEADLINE"), bodytext);
            }
            else if (component.targetObject == this.m_ForgeButton.gameObject)
            {
                component.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_FORGE_HEADLINE"), bodytext);
            }
            else if (component.targetObject == this.m_TavernBrawlButton.gameObject)
            {
                component.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_TAVERN_BRAWL_HEADLINE"), bodytext);
            }
            else if (component.targetObject == this.m_OpenPacksButton.gameObject)
            {
                component.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_PACKOPEN_HEADLINE"), bodytext);
            }
            else if (component.targetObject == this.m_CollectionButton.gameObject)
            {
                component.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_COLLECTION_HEADLINE"), bodytext);
            }
        }
    }

    private void OnButtonPressed(UIEvent e)
    {
        PegUIElement element = e.GetElement();
        NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
        bool tournament = netObject.Games.Tournament;
        bool practice = netObject.Games.Practice;
        bool flag3 = (netObject.Games.Forge && (AchieveManager.Get() != null)) && AchieveManager.Get().HasUnlockedFeature(Achievement.UnlockableFeature.FORGE);
        if (flag3)
        {
            flag3 = HealthyGamingMgr.Get().isArenaEnabled();
        }
        bool manager = netObject.Collection.Manager;
        bool flag5 = netObject.Games.TavernBrawl && TavernBrawlManager.Get().HasUnlockedTavernBrawl;
        BoxMenuButton button = (BoxMenuButton) element;
        if (button == this.m_StartButton)
        {
            this.OnStartButtonPressed(e);
        }
        else if ((button == this.m_TournamentButton) && tournament)
        {
            this.OnTournamentButtonPressed(e);
        }
        else if ((button == this.m_SoloAdventuresButton) && practice)
        {
            this.OnSoloAdventuresButtonPressed(e);
        }
        else if ((button == this.m_ForgeButton) && flag3)
        {
            this.OnForgeButtonPressed(e);
        }
        else if ((button == this.m_TavernBrawlButton) && flag5)
        {
            this.OnTavernBrawlButtonPressed(e);
        }
        else if (button == this.m_OpenPacksButton)
        {
            this.OnOpenPacksButtonPressed(e);
        }
        else if ((button == this.m_CollectionButton) && manager)
        {
            this.OnCollectionButtonPressed(e);
        }
    }

    public void OnCollectionButtonPressed(UIEvent e)
    {
        if (SceneMgr.Get() == null)
        {
            this.ChangeState(State.OPEN);
        }
        else
        {
            this.FireButtonPressEvent(ButtonType.COLLECTION);
        }
    }

    private void OnDestroy()
    {
        Log.LoadingScreen.Print("Box.OnDestroy()", new object[0]);
        TavernBrawlManager.Get().OnTavernBrawlUpdated -= new System.Action(this.DoTavernBrawlButtonInitialization);
        NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheFeatures), new System.Action(this.DoTavernBrawlButtonInitialization));
        this.ShutdownState();
        this.ShutdownNet();
        s_instance = null;
    }

    private void OnForgeButtonPressed(UIEvent e)
    {
        if (SceneMgr.Get() == null)
        {
            this.ChangeState(State.OPEN);
        }
        else
        {
            AchieveManager.Get().NotifyOfClick(Achievement.ClickTriggerType.BUTTON_ARENA);
            this.FireButtonPressEvent(ButtonType.FORGE);
        }
    }

    public void OnLoggedIn()
    {
        this.InitializeNet();
    }

    private void OnNetCacheReady()
    {
        this.m_waitingForNetData = false;
        if (Network.ShouldBeConnectedToAurora())
        {
            RankMgr.Get().SetRankPresenceField(NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>());
        }
        this.UpdateUI(false);
    }

    private void OnOpenPacksButtonPressed(UIEvent e)
    {
        if (SceneMgr.Get() == null)
        {
            this.ChangeState(State.OPEN);
        }
        else
        {
            this.FireButtonPressEvent(ButtonType.OPEN_PACKS);
        }
    }

    public void OnQuestButtonPressed(UIEvent e)
    {
        if (!ShownUIMgr.Get().HasShownUI())
        {
            ShownUIMgr.Get().SetShownUI(ShownUIMgr.UI_WINDOW.QUEST_LOG);
            SoundManager.Get().LoadAndPlay("Small_Click", base.gameObject);
            this.m_tempInputBlocker = CameraUtils.CreateInputBlocker(Get().GetCamera(), "QuestLogInputBlocker", null, (float) 30f);
            SceneUtils.SetLayer(this.m_tempInputBlocker, GameLayer.IgnoreFullScreenEffects);
            this.m_tempInputBlocker.AddComponent<PegUIElement>();
            base.StopCoroutine("ShowQuestLogWhenReady");
            base.StartCoroutine("ShowQuestLogWhenReady");
        }
    }

    private void OnQuestLogLoaded(string name, GameObject go, object callbackData)
    {
        this.m_questLogLoading = false;
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("QuestLogButton.OnQuestLogLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            this.m_questLog = go.GetComponent<QuestLog>();
            if (this.m_questLog == null)
            {
                UnityEngine.Debug.LogError(string.Format("QuestLogButton.OnQuestLogLoaded() - ERROR \"{0}\" has no {1} component", name, typeof(QuestLog)));
            }
        }
    }

    private void OnQuestLogNetCacheReady()
    {
        if (this.m_questLogNetCacheDataState != DataState.UNLOADING)
        {
            this.m_questLogNetCacheDataState = DataState.RECEIVED;
        }
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (!TavernBrawlManager.Get().IsTavernBrawlActive && (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.STARTUP))
        {
            this.PlayTavernBrawlButtonActivation(false, true);
        }
        this.ChangeStateToReflectSceneMode(mode);
        if (this.m_waitingForSceneLoad)
        {
            this.m_waitingForSceneLoad = false;
            if (this.m_queuedButtonFire.HasValue)
            {
                this.FireButtonPressEvent(this.m_queuedButtonFire.Value);
                this.m_queuedButtonFire = null;
            }
        }
    }

    private void OnScenePreUnload(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        SceneMgr.Mode mode = SceneMgr.Get().GetMode();
        switch (mode)
        {
            case SceneMgr.Mode.GAMEPLAY:
            case SceneMgr.Mode.STARTUP:
            case SceneMgr.Mode.RESET:
                return;
        }
        if (prevMode == SceneMgr.Mode.HUB)
        {
            this.ChangeState(State.LOADING);
            this.m_StoreButton.Unload();
            this.UnloadQuestLog();
        }
        else if (mode == SceneMgr.Mode.HUB)
        {
            this.ChangeStateToReflectSceneMode(mode);
            this.m_waitingForSceneLoad = true;
        }
        else
        {
            this.ChangeState(State.LOADING);
        }
        this.ClearQueuedButtonFireEvent();
        this.UpdateUIEvents();
    }

    private void OnSoloAdventuresButtonPressed(UIEvent e)
    {
        if (SceneMgr.Get() == null)
        {
            this.ChangeState(State.OPEN);
        }
        else
        {
            this.FireButtonPressEvent(ButtonType.ADVENTURE);
        }
    }

    private void OnStartButtonPressed(UIEvent e)
    {
        if (SceneMgr.Get() == null)
        {
            this.ChangeState(State.HUB);
        }
        else
        {
            this.FireButtonPressEvent(ButtonType.START);
        }
    }

    private void OnStoreButtonReleased(UIEvent e)
    {
        if (!FriendChallengeMgr.Get().HasChallenge() && !this.m_StoreButton.IsVisualClosed())
        {
            if (!StoreManager.Get().IsOpen())
            {
                SoundManager.Get().LoadAndPlay("Store_closed_button_click", base.gameObject);
            }
            else
            {
                FriendChallengeMgr.Get().OnStoreOpened();
                SoundManager.Get().LoadAndPlay("Small_Click", base.gameObject);
                StoreManager.Get().RegisterStoreShownListener(new StoreManager.StoreShownCallback(this.OnStoreShown));
                StoreManager.Get().StartGeneralTransaction();
            }
        }
    }

    private void OnStoreShown(object userData)
    {
        StoreManager.Get().RemoveStoreShownListener(new StoreManager.StoreShownCallback(this.OnStoreShown));
    }

    private void OnTavernBrawlButtonPressed(UIEvent e)
    {
        if (SceneMgr.Get() == null)
        {
            this.ChangeState(State.OPEN);
        }
        else
        {
            this.FireButtonPressEvent(ButtonType.TAVERN_BRAWL);
        }
    }

    private void OnTournamentButtonPressed(UIEvent e)
    {
        if (SceneMgr.Get() == null)
        {
            this.ChangeState(State.OPEN);
        }
        else
        {
            if (!Options.Get().HasOption(Option.HAS_CLICKED_TOURNAMENT))
            {
                Options.Get().SetBool(Option.HAS_CLICKED_TOURNAMENT, true);
            }
            AchieveManager.Get().NotifyOfClick(Achievement.ClickTriggerType.BUTTON_PLAY);
            this.FireButtonPressEvent(ButtonType.TOURNAMENT);
        }
    }

    private void OnTutorialPlaySpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            SceneMgr mgr = SceneMgr.Get();
            mgr.RegisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
            mgr.RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
            this.ChangeStateToReflectSceneMode(SceneMgr.Get().GetMode());
        }
    }

    private void OnTutorialSceneDestroyed(object userData)
    {
        LoadingScreen.Get().UnregisterPreviousSceneDestroyedListener(new LoadingScreen.PreviousSceneDestroyedCallback(this.OnTutorialSceneDestroyed));
        Spell eventSpell = this.GetEventSpell(BoxEventType.TUTORIAL_PLAY);
        eventSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnTutorialPlaySpellStateFinished));
        eventSpell.ActivateState(SpellStateType.DEATH);
    }

    public void PlayTavernBrawlButtonActivation(bool activate, bool isInitialization = false)
    {
        Animator component = this.m_TavernBrawlButtonVisual.GetComponent<Animator>();
        component.StopPlayback();
        if (activate)
        {
            component.Play("TavernBrawl_ButtonActivate");
            if (!isInitialization)
            {
                this.m_TavernBrawlButtonActivateFX.GetComponent<ParticleSystem>().Play();
            }
        }
        else
        {
            if (!isInitialization)
            {
                this.m_TavernBrawlButton.ClearHighlightAndTooltip();
            }
            component.Play("TavernBrawl_ButtonDeactivate");
            if (!isInitialization)
            {
                this.m_TavernBrawlButtonDeactivateFX.GetComponent<ParticleSystem>().Play();
            }
        }
        this.IsTavernBrawlButtonDeactivated = !activate;
    }

    private void QueueStateChange(State state)
    {
        this.m_stateQueue.Enqueue(state);
    }

    private void RegisterButtonEvents(PegUIElement button)
    {
        button.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnButtonPressed));
        button.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnButtonMouseOver));
        button.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnButtonMouseOut));
    }

    public bool RemoveButtonPressListener(ButtonPressCallback callback)
    {
        return this.RemoveButtonPressListener(callback, null);
    }

    public bool RemoveButtonPressListener(ButtonPressCallback callback, object userData)
    {
        ButtonPressListener item = new ButtonPressListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_buttonPressListeners.Remove(item);
    }

    public bool RemoveTransitionFinishedListener(TransitionFinishedCallback callback)
    {
        return this.RemoveTransitionFinishedListener(callback, null);
    }

    public bool RemoveTransitionFinishedListener(TransitionFinishedCallback callback, object userData)
    {
        TransitionFinishedListener item = new TransitionFinishedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_transitionFinishedListeners.Remove(item);
    }

    public void SetLightState(BoxLightStateType stateType)
    {
        this.m_LightMgr.SetState(stateType);
    }

    private void SetPackCount(int n)
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_ribbonButtons.SetPackCount(n);
        }
        else
        {
            this.m_OpenPacksButton.SetPackCount(n);
        }
    }

    public void SetToIgnoreFullScreenEffects(bool ignoreEffects)
    {
        if (ignoreEffects)
        {
            SceneUtils.ReplaceLayer(this.m_LeftDoor.gameObject, GameLayer.IgnoreFullScreenEffects, this.m_originalLeftDoorLayer);
            SceneUtils.ReplaceLayer(this.m_RightDoor.gameObject, GameLayer.IgnoreFullScreenEffects, this.m_originalRightDoorLayer);
            SceneUtils.ReplaceLayer(this.m_Drawer.gameObject, GameLayer.IgnoreFullScreenEffects, this.m_originalDrawerLayer);
        }
        else
        {
            SceneUtils.ReplaceLayer(this.m_LeftDoor.gameObject, this.m_originalLeftDoorLayer, GameLayer.IgnoreFullScreenEffects);
            SceneUtils.ReplaceLayer(this.m_RightDoor.gameObject, this.m_originalRightDoorLayer, GameLayer.IgnoreFullScreenEffects);
            SceneUtils.ReplaceLayer(this.m_Drawer.gameObject, this.m_originalDrawerLayer, GameLayer.IgnoreFullScreenEffects);
        }
    }

    private bool ShouldRequestData(DataState state)
    {
        return ((state == DataState.NONE) || (state == DataState.UNLOADING));
    }

    [DebuggerHidden]
    private IEnumerator ShowQuestLogWhenReady()
    {
        return new <ShowQuestLogWhenReady>c__Iterator1C { <>f__this = this };
    }

    private void ShutdownNet()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
    }

    private void ShutdownState()
    {
        if (this.m_StoreButton != null)
        {
            this.m_StoreButton.Unload();
        }
        this.UnloadQuestLog();
        SceneMgr mgr = SceneMgr.Get();
        if (mgr != null)
        {
            mgr.UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
            mgr.UnregisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
        }
    }

    private void Start()
    {
        this.InitializeNet();
        this.InitializeComponents();
        this.InitializeState();
        this.InitializeUI();
        if (DemoMgr.Get().IsExpoDemo())
        {
            this.m_StoreButton.gameObject.SetActive(false);
            this.m_Drawer.gameObject.SetActive(false);
            this.m_QuestLogButton.gameObject.SetActive(false);
        }
        TavernBrawlManager manager = TavernBrawlManager.Get();
        if (manager.IsTavernBrawlInfoReady)
        {
            this.DoTavernBrawlButtonInitialization();
        }
        else
        {
            manager.OnTavernBrawlUpdated += new System.Action(this.DoTavernBrawlButtonInitialization);
        }
    }

    private void ToggleButtonTextureState(bool enabled, BoxMenuButton button)
    {
        if (enabled)
        {
            button.m_TextMesh.TextColor = this.m_EnabledMaterial;
        }
        else
        {
            button.m_TextMesh.TextColor = this.m_DisabledMaterial;
        }
    }

    private void ToggleDrawerButtonState(bool enabled, BoxMenuButton button)
    {
        if (enabled)
        {
            button.m_TextMesh.TextColor = this.m_EnabledDrawerMaterial;
        }
        else
        {
            button.m_TextMesh.TextColor = this.m_DisabledDrawerMaterial;
        }
    }

    private void ToggleRibbonUI(bool show)
    {
        if (this.m_ribbonButtons != null)
        {
            this.m_ribbonButtons.Toggle(show);
            this.m_showRibbonButtons = show;
        }
    }

    private State TranslateSceneModeToBoxState(SceneMgr.Mode mode)
    {
        if (mode == SceneMgr.Mode.STARTUP)
        {
            return State.STARTUP;
        }
        if (mode == SceneMgr.Mode.LOGIN)
        {
            return State.INVALID;
        }
        if (mode == SceneMgr.Mode.HUB)
        {
            return State.HUB_WITH_DRAWER;
        }
        if (mode != SceneMgr.Mode.TOURNAMENT)
        {
            if (mode == SceneMgr.Mode.ADVENTURE)
            {
                return State.OPEN;
            }
            if (mode == SceneMgr.Mode.FRIENDLY)
            {
                return State.OPEN;
            }
            if (mode == SceneMgr.Mode.DRAFT)
            {
                return State.OPEN;
            }
            if (mode == SceneMgr.Mode.COLLECTIONMANAGER)
            {
                return State.OPEN;
            }
            if (mode == SceneMgr.Mode.TAVERN_BRAWL)
            {
                return State.OPEN;
            }
            if (mode == SceneMgr.Mode.PACKOPENING)
            {
                return State.OPEN;
            }
            if (mode == SceneMgr.Mode.GAMEPLAY)
            {
                return State.INVALID;
            }
            if (mode == SceneMgr.Mode.FATAL_ERROR)
            {
                return State.ERROR;
            }
        }
        return State.OPEN;
    }

    private BoxLightStateType TranslateSceneModeToLightState(SceneMgr.Mode mode)
    {
        if (mode == SceneMgr.Mode.LOGIN)
        {
            return BoxLightStateType.INVALID;
        }
        if (mode == SceneMgr.Mode.TOURNAMENT)
        {
            return BoxLightStateType.TOURNAMENT;
        }
        if (mode == SceneMgr.Mode.COLLECTIONMANAGER)
        {
            return BoxLightStateType.COLLECTION;
        }
        if (mode == SceneMgr.Mode.TAVERN_BRAWL)
        {
            return BoxLightStateType.COLLECTION;
        }
        if (mode == SceneMgr.Mode.PACKOPENING)
        {
            return BoxLightStateType.PACK_OPENING;
        }
        if (mode == SceneMgr.Mode.GAMEPLAY)
        {
            return BoxLightStateType.INVALID;
        }
        if (mode == SceneMgr.Mode.DRAFT)
        {
            return BoxLightStateType.FORGE;
        }
        if (mode == SceneMgr.Mode.ADVENTURE)
        {
            return BoxLightStateType.ADVENTURE;
        }
        if (mode == SceneMgr.Mode.FRIENDLY)
        {
            return BoxLightStateType.ADVENTURE;
        }
        return BoxLightStateType.DEFAULT;
    }

    public void UnloadQuestLog()
    {
        this.m_questLogNetCacheDataState = DataState.UNLOADING;
        this.DestroyQuestLog();
    }

    public void UpdateState()
    {
        if (this.m_state == State.STARTUP)
        {
            this.UpdateState_Startup();
        }
        else if (this.m_state == State.PRESS_START)
        {
            this.UpdateState_PressStart();
        }
        else if (this.m_state == State.LOADING)
        {
            this.UpdateState_Loading();
        }
        else if (this.m_state == State.HUB)
        {
            this.UpdateState_Hub();
        }
        else if (this.m_state == State.HUB_WITH_DRAWER)
        {
            this.UpdateState_HubWithDrawer();
        }
        else if (this.m_state == State.OPEN)
        {
            this.UpdateState_Open();
        }
        else if (this.m_state == State.CLOSED)
        {
            this.UpdateState_Closed();
        }
        else if (this.m_state == State.ERROR)
        {
            this.UpdateState_Error();
        }
        else
        {
            UnityEngine.Debug.LogError(string.Format("Box.UpdateState() - unhandled state {0}", this.m_state));
        }
    }

    private void UpdateState_Closed()
    {
        this.m_state = State.CLOSED;
        this.UpdateStateUsingConfig();
    }

    private void UpdateState_Error()
    {
        this.m_state = State.ERROR;
        this.UpdateStateUsingConfig();
    }

    private void UpdateState_Hub()
    {
        this.m_state = State.HUB;
        this.HackRequestNetFeatures();
        this.UpdateStateUsingConfig();
    }

    private void UpdateState_HubWithDrawer()
    {
        this.m_state = State.HUB_WITH_DRAWER;
        this.m_Camera.EnableAccelerometer();
        this.HackRequestNetData();
        this.UpdateStateUsingConfig();
    }

    private void UpdateState_Loading()
    {
        this.m_state = State.LOADING;
        this.UpdateStateUsingConfig();
    }

    private void UpdateState_Open()
    {
        this.m_state = State.OPEN;
        this.UpdateStateUsingConfig();
    }

    private void UpdateState_PressStart()
    {
        this.m_state = State.PRESS_START;
        this.UpdateStateUsingConfig();
    }

    private void UpdateState_Startup()
    {
        this.m_state = State.STARTUP;
        this.UpdateStateUsingConfig();
    }

    private void UpdateStateUsingConfig()
    {
        BoxStateConfig config = this.m_stateConfigs[(int) this.m_state];
        if (!config.m_logoState.m_ignore)
        {
            this.m_Logo.UpdateState(config.m_logoState.m_state);
        }
        if (!config.m_startButtonState.m_ignore)
        {
            this.m_StartButton.UpdateState(config.m_startButtonState.m_state);
        }
        if (!config.m_doorState.m_ignore)
        {
            this.m_LeftDoor.ChangeState(config.m_doorState.m_state);
            this.m_RightDoor.ChangeState(config.m_doorState.m_state);
        }
        if (!config.m_diskState.m_ignore)
        {
            this.m_Disk.UpdateState(config.m_diskState.m_state);
        }
        this.m_TopSpinner.Reset();
        this.m_BottomSpinner.Reset();
        if (!config.m_drawerState.m_ignore)
        {
            this.m_Drawer.UpdateState(config.m_drawerState.m_state);
        }
        if (!config.m_camState.m_ignore)
        {
            this.m_Camera.UpdateState(config.m_camState.m_state);
        }
        if (!config.m_fullScreenBlackState.m_ignore)
        {
            this.FullScreenBlack_UpdateState(config.m_fullScreenBlackState.m_state);
        }
    }

    public bool UpdateTavernBrawlButtonState(bool highlightAllowed)
    {
        NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
        TavernBrawlManager manager = TavernBrawlManager.Get();
        bool enabled = ((netObject != null) && netObject.Games.TavernBrawl) && manager.HasUnlockedTavernBrawl;
        bool highlightOn = ((highlightAllowed && enabled) && (manager.IsFirstTimeSeeingThisFeature && !manager.HasSeenTavernBrawlScreen)) && !this.IsTavernBrawlButtonDeactivated;
        this.HighlightButton(this.m_TavernBrawlButton, highlightOn);
        this.ToggleButtonTextureState(enabled, this.m_TavernBrawlButton);
        return enabled;
    }

    public void UpdateUI(bool isInitialization = false)
    {
        this.UpdateUIState(isInitialization);
        this.UpdateUIEvents();
    }

    private void UpdateUIEvents()
    {
        if (this.CanEnableUIEvents() && (this.m_state == State.PRESS_START))
        {
            this.EnableButton(this.m_StartButton);
        }
        else
        {
            this.DisableButton(this.m_StartButton);
        }
        if (this.CanEnableUIEvents() && ((this.m_state == State.HUB) || (this.m_state == State.HUB_WITH_DRAWER)))
        {
            if (this.m_waitingForNetData)
            {
                this.DisableButton(this.m_TournamentButton);
                this.DisableButton(this.m_SoloAdventuresButton);
                this.DisableButton(this.m_ForgeButton);
                this.DisableButton(this.m_TavernBrawlButton);
                this.DisableButton(this.m_QuestLogButton);
                this.DisableButton(this.m_StoreButton);
            }
            else
            {
                this.EnableButton(this.m_TournamentButton);
                this.EnableButton(this.m_SoloAdventuresButton);
                this.EnableButton(this.m_ForgeButton);
                this.EnableButton(this.m_TavernBrawlButton);
                this.EnableButton(this.m_QuestLogButton);
                this.EnableButton(this.m_StoreButton);
            }
            if (this.m_state == State.HUB_WITH_DRAWER)
            {
                if (this.m_waitingForNetData)
                {
                    this.DisableButton(this.m_OpenPacksButton);
                    this.DisableButton(this.m_CollectionButton);
                }
                else
                {
                    this.EnableButton(this.m_OpenPacksButton);
                    this.EnableButton(this.m_CollectionButton);
                }
            }
            else
            {
                this.DisableButton(this.m_OpenPacksButton);
                this.DisableButton(this.m_CollectionButton);
            }
        }
        else
        {
            this.DisableButton(this.m_TournamentButton);
            this.DisableButton(this.m_SoloAdventuresButton);
            this.DisableButton(this.m_ForgeButton);
            this.DisableButton(this.m_TavernBrawlButton);
            this.DisableButton(this.m_OpenPacksButton);
            this.DisableButton(this.m_CollectionButton);
            this.DisableButton(this.m_QuestLogButton);
            this.DisableButton(this.m_StoreButton);
        }
        if (DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2013)
        {
            this.DisableButton(this.m_TournamentButton);
            this.DisableButton(this.m_SoloAdventuresButton);
            this.DisableButton(this.m_OpenPacksButton);
            this.DisableButton(this.m_CollectionButton);
            this.DisableButton(this.m_QuestLogButton);
            this.DisableButton(this.m_StoreButton);
        }
        else if (DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2014)
        {
            this.DisableButton(this.m_SoloAdventuresButton);
            this.DisableButton(this.m_ForgeButton);
            this.DisableButton(this.m_TavernBrawlButton);
            this.DisableButton(this.m_OpenPacksButton);
            this.DisableButton(this.m_CollectionButton);
            this.DisableButton(this.m_QuestLogButton);
            this.DisableButton(this.m_StoreButton);
        }
        else if (DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2015)
        {
            this.DisableButton(this.m_TournamentButton);
            this.DisableButton(this.m_ForgeButton);
            this.DisableButton(this.m_TavernBrawlButton);
            this.DisableButton(this.m_OpenPacksButton);
            this.DisableButton(this.m_CollectionButton);
            this.DisableButton(this.m_QuestLogButton);
            this.DisableButton(this.m_StoreButton);
        }
    }

    private void UpdateUIState(bool isInitialization)
    {
        if (this.m_waitingForNetData)
        {
            this.SetPackCount(-1);
            this.HighlightButton(this.m_OpenPacksButton, false);
            this.HighlightButton(this.m_TournamentButton, false);
            this.HighlightButton(this.m_SoloAdventuresButton, false);
            this.HighlightButton(this.m_CollectionButton, false);
            this.HighlightButton(this.m_ForgeButton, false);
            this.HighlightButton(this.m_TavernBrawlButton, false);
        }
        else
        {
            NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
            if (DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2013)
            {
                netObject.Games.Practice = false;
                netObject.Games.Tournament = false;
            }
            int n = this.ComputeBoosterCount();
            this.SetPackCount(n);
            bool highlightOn = (n > 0) && !Options.Get().GetBool(Option.HAS_SEEN_PACK_OPENING, false);
            this.HighlightButton(this.m_OpenPacksButton, highlightOn);
            bool flag2 = netObject.Games.Practice && (!Options.Get().GetBool(Option.HAS_SEEN_PRACTICE_MODE, false) || Options.Get().GetBool(Option.BUNDLE_JUST_PURCHASE_IN_HUB, false));
            this.HighlightButton(this.m_SoloAdventuresButton, flag2);
            this.ToggleButtonTextureState(netObject.Games.Practice, this.m_SoloAdventuresButton);
            bool flag3 = false;
            if (AchieveManager.Get() != null)
            {
                foreach (Achievement achievement in AchieveManager.Get().GetActiveQuests(false))
                {
                    if (achievement.ID == 11)
                    {
                        flag3 = !flag2 && netObject.Games.Tournament;
                    }
                }
            }
            this.HighlightButton(this.m_TournamentButton, flag3);
            this.ToggleButtonTextureState(netObject.Games.Tournament, this.m_TournamentButton);
            bool flag4 = (!flag2 && netObject.Collection.Manager) && !Options.Get().GetBool(Option.HAS_SEEN_COLLECTIONMANAGER_AFTER_PRACTICE, false);
            this.HighlightButton(this.m_CollectionButton, flag4);
            this.ToggleDrawerButtonState(netObject.Collection.Manager, this.m_CollectionButton);
            bool enabled = (netObject.Games.Forge && (AchieveManager.Get() != null)) && AchieveManager.Get().HasUnlockedFeature(Achievement.UnlockableFeature.FORGE);
            if (enabled)
            {
                enabled = HealthyGamingMgr.Get().isArenaEnabled();
            }
            bool flag6 = (!flag2 && enabled) && !Options.Get().GetBool(Option.HAS_SEEN_FORGE, false);
            this.HighlightButton(this.m_ForgeButton, flag6);
            this.ToggleButtonTextureState(enabled, this.m_ForgeButton);
            this.UpdateTavernBrawlButtonState(true);
            if (SpecialEventVisualMgr.GetActiveEventType() == SpecialEventType.SPECIAL_EVENT_PRE_TAVERN_BRAWL)
            {
                this.m_TavernBrawlButton.gameObject.SetActive(false);
                this.m_EmptyFourthButton.SetActive(true);
            }
        }
    }

    public bool IsTavernBrawlButtonDeactivated { get; private set; }

    [CompilerGenerated]
    private sealed class <ShowQuestLogWhenReady>c__Iterator1C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Box <>f__this;

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
                    if ((this.<>f__this.m_questLog == null) && !this.<>f__this.m_questLogLoading)
                    {
                        this.<>f__this.m_questLogLoading = true;
                        if (UniversalInputManager.UsePhoneUI == null)
                        {
                            AssetLoader.Get().LoadGameObject("QuestLog", new AssetLoader.GameObjectCallback(this.<>f__this.OnQuestLogLoaded), null, false);
                            break;
                        }
                        AssetLoader.Get().LoadGameObject("QuestLog_phone", new AssetLoader.GameObjectCallback(this.<>f__this.OnQuestLogLoaded), null, false);
                    }
                    break;

                case 1:
                    goto Label_010F;

                case 2:
                    goto Label_013D;

                case 3:
                    UnityEngine.Object.Destroy(this.<>f__this.m_tempInputBlocker);
                    this.$PC = -1;
                    goto Label_0191;

                default:
                    goto Label_0191;
            }
            if (this.<>f__this.ShouldRequestData(this.<>f__this.m_questLogNetCacheDataState))
            {
                this.<>f__this.m_questLogNetCacheDataState = Box.DataState.REQUEST_SENT;
                NetCache.Get().RegisterScreenQuestLog(new NetCache.NetCacheCallback(this.<>f__this.OnQuestLogNetCacheReady));
            }
        Label_010F:
            while (this.<>f__this.m_questLog == null)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0193;
            }
        Label_013D:
            while (this.<>f__this.m_questLogNetCacheDataState != Box.DataState.RECEIVED)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0193;
            }
            this.<>f__this.m_questLog.Show();
            this.$current = new WaitForSeconds(0.5f);
            this.$PC = 3;
            goto Label_0193;
        Label_0191:
            return false;
        Label_0193:
            return true;
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

    private class BoxStateConfig
    {
        public Part<BoxCamera.State> m_camState = new Part<BoxCamera.State>();
        public Part<BoxDisk.State> m_diskState = new Part<BoxDisk.State>();
        public Part<BoxDoor.State> m_doorState = new Part<BoxDoor.State>();
        public Part<BoxDrawer.State> m_drawerState = new Part<BoxDrawer.State>();
        public Part<bool> m_fullScreenBlackState = new Part<bool>();
        public Part<BoxLogo.State> m_logoState = new Part<BoxLogo.State>();
        public Part<BoxStartButton.State> m_startButtonState = new Part<BoxStartButton.State>();

        public class Part<T>
        {
            public bool m_ignore;
            public T m_state;
        }
    }

    public delegate void ButtonPressCallback(Box.ButtonType buttonType, object userData);

    private class ButtonPressListener : EventListener<Box.ButtonPressCallback>
    {
        public void Fire(Box.ButtonType buttonType)
        {
            base.m_callback(buttonType, base.m_userData);
        }
    }

    public enum ButtonType
    {
        START,
        TOURNAMENT,
        ADVENTURE,
        FORGE,
        OPEN_PACKS,
        COLLECTION,
        TAVERN_BRAWL
    }

    private enum DataState
    {
        NONE,
        REQUEST_SENT,
        RECEIVED,
        UNLOADING
    }

    public enum State
    {
        INVALID,
        STARTUP,
        PRESS_START,
        LOADING,
        HUB,
        HUB_WITH_DRAWER,
        OPEN,
        CLOSED,
        ERROR
    }

    public delegate void TransitionFinishedCallback(object userData);

    private class TransitionFinishedListener : EventListener<Box.TransitionFinishedCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }
}

