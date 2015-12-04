using PegasusShared;
using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Login : Scene
{
    [CompilerGenerated]
    private static Spell.StateFinishedCallback <>f__am$cacheD;
    [CompilerGenerated]
    private static AlertPopup.ResponseCallback <>f__am$cacheE;
    [CompilerGenerated]
    private static AlertPopup.ResponseCallback <>f__am$cacheF;
    private List<Achievement> m_completedQuests = new List<Achievement>();
    private ExistingAccountPopup m_existingAccountPopup;
    private int m_nextMissionId;
    private int m_numRewardsToLoad;
    private List<Reward> m_rewards = new List<Reward>();
    private TutorialProgress m_skipToTutorialProgress;
    private bool m_waitingForBattleNet = true;
    private bool m_waitingForSetProgress;
    private bool m_waitingForUpdateLoginComplete = true;
    public static readonly Vector3 REWARD_LOCAL_POS = new Vector3(0.1438589f, 31.27692f, 12.97332f);
    public static PlatformDependentValue<Vector3> REWARD_PUNCH_SCALE;
    public static PlatformDependentValue<Vector3> REWARD_SCALE;
    private static Login s_instance;

    static Login()
    {
        PlatformDependentValue<Vector3> value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(15f, 15f, 15f),
            Phone = new Vector3(8f, 8f, 8f)
        };
        REWARD_SCALE = value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(15.1f, 15.1f, 15.1f),
            Phone = new Vector3(8.1f, 8.1f, 8.1f)
        };
        REWARD_PUNCH_SCALE = value2;
        s_instance = null;
    }

    private void AssetsVersionCheckCompleted()
    {
        if (!string.IsNullOrEmpty(UpdateManager.Get().GetError()) && UpdateManager.Get().UpdateIsRequired())
        {
            Error.AddFatalLoc("GLUE_PATCHING_ERROR", new object[0]);
        }
        else
        {
            if (Network.ShouldBeConnectedToAurora())
            {
                BnetPresenceMgr.Get().Initialize();
                BnetFriendMgr.Get().Initialize();
                BnetChallengeMgr.Get().Initialize();
                BnetWhisperMgr.Get().Initialize();
                BnetNearbyPlayerMgr.Get().Initialize();
                FriendChallengeMgr.Get().OnLoggedIn();
                SpectatorManager.Get().Initialize();
                if (!Options.Get().GetBool(Option.CONNECT_TO_AURORA))
                {
                    Options.Get().SetBool(Option.CONNECT_TO_AURORA, true);
                }
                TutorialProgress progress = Options.Get().GetEnum<TutorialProgress>(Option.LOCAL_TUTORIAL_PROGRESS);
                if (progress > TutorialProgress.NOTHING_COMPLETE)
                {
                    this.m_waitingForSetProgress = true;
                    ConnectAPI.SetProgress((long) progress);
                }
                if (WebAuth.GetIsNewCreatedAccount())
                {
                    AdTrackingManager.Get().TrackAccountCreated();
                    WebAuth.SetIsNewCreatedAccount(false);
                }
            }
            ConnectAPI.RequestAccountLicenses();
            ConnectAPI.RequestGameLicenses();
            Box.Get().OnLoggedIn();
            BaseUI.Get().OnLoggedIn();
            InactivePlayerKicker.Get().OnLoggedIn();
            HealthyGamingMgr.Get().OnLoggedIn();
            DefLoader.Get().Initialize();
            CollectionManager.Init();
            AdventureProgressMgr.Init();
            Tournament.Init();
            GameMgr.Get().OnLoggedIn();
            if (Network.ShouldBeConnectedToAurora())
            {
                StoreManager.Get().Init();
            }
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_LOGIN_FINISHED);
            Network.ResetConnectionFailureCount();
            if (Network.ShouldBeConnectedToAurora())
            {
                ConnectAPI.DoLoginUpdate();
            }
            else
            {
                this.m_waitingForUpdateLoginComplete = false;
            }
            Enum[] args = new Enum[] { PresenceStatus.LOGIN };
            PresenceMgr.Get().SetStatus(args);
            if (SplashScreen.Get() != null)
            {
                SplashScreen.Get().StopPatching();
                SplashScreen.Get().ShowRatings();
            }
            this.PreloadActors();
            if (!Network.ShouldBeConnectedToAurora())
            {
                base.StartCoroutine(this.RegisterScreenWhenReady());
            }
            SceneMgr.Get().LoadShaderPreCompiler();
        }
    }

    protected override void Awake()
    {
        s_instance = this;
        base.Awake();
    }

    private void ChangeMode()
    {
        MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MainTitle);
        if (this.m_skipToTutorialProgress != TutorialProgress.NOTHING_COMPLETE)
        {
            this.m_nextMissionId = GameUtils.GetNextTutorial(this.m_skipToTutorialProgress);
        }
        else
        {
            this.m_nextMissionId = GameUtils.GetNextTutorial();
        }
        if (this.m_nextMissionId == 0)
        {
            this.ChangeMode_Hub();
        }
        else
        {
            this.ChangeMode_Tutorial();
        }
    }

    private void ChangeMode_Hub()
    {
        if (Options.Get().GetBool(Option.HAS_SEEN_HUB, false))
        {
            SoundManager.Get().LoadAndPlay("VO_INNKEEPER_INTRO_01");
        }
        if (this.ShouldDoIntro())
        {
            Spell eventSpell = Box.Get().GetEventSpell(BoxEventType.STARTUP_HUB);
            eventSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnStartupHubSpellFinished));
            eventSpell.Activate();
        }
        else
        {
            this.DoSkippedBoxIntro();
            base.StartCoroutine(this.ShowUnAckedRewardsAndQuests());
        }
    }

    private void ChangeMode_Resume(SceneMgr.Mode mode)
    {
        if ((mode == SceneMgr.Mode.HUB) && Options.Get().GetBool(Option.HAS_SEEN_HUB, false))
        {
            SoundManager.Get().LoadAndPlay("VO_INNKEEPER_INTRO_01");
        }
        if (mode == SceneMgr.Mode.COLLECTIONMANAGER)
        {
            CollectionManager.Get().NotifyOfBoxTransitionStart();
        }
        SceneMgr.Get().SetNextMode(mode);
        Box.Get().m_Camera.m_EventTable.m_FadeFromBlackSpell.Activate();
    }

    private void ChangeMode_Tutorial()
    {
        Enum[] args = new Enum[] { PresenceStatus.TUTORIAL_PREGAME };
        PresenceMgr.Get().SetStatus(args);
        Box.Get().SetLightState(BoxLightStateType.TUTORIAL);
        Spell eventSpell = Box.Get().GetEventSpell(BoxEventType.STARTUP_TUTORIAL);
        eventSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnStartupTutorialSpellFinished));
        eventSpell.Activate();
    }

    private void DoSkippedBoxIntro()
    {
        Spell fadeFromBlackSpell = Box.Get().GetBoxCamera().GetEventTable().m_FadeFromBlackSpell;
        if (<>f__am$cacheD == null)
        {
            <>f__am$cacheD = delegate (Spell thisSpell, SpellStateType prevStateType, object userData) {
                float num = (float) userData;
                UnityEngine.Time.timeScale = num;
            };
        }
        Spell.StateFinishedCallback callback = <>f__am$cacheD;
        fadeFromBlackSpell.AddStateFinishedCallback(callback, UnityEngine.Time.timeScale);
        UnityEngine.Time.timeScale = SceneDebugger.Get().m_MaxTimeScale;
        fadeFromBlackSpell.Activate();
    }

    public static Login Get()
    {
        return s_instance;
    }

    private void HandleUnAckedRewardsAndCompletedQuests()
    {
        CollectionManager.Get().RegisterAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
        NetCache.NetCacheProfileNotices netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
        List<RewardData> rewardsToShow = new List<RewardData>();
        if (netObject != null)
        {
            List<RewardData> rewards = RewardUtils.GetRewards(netObject.Notices);
            HashSet<RewardVisualTiming> rewardTimings = new HashSet<RewardVisualTiming>();
            IEnumerator enumerator = Enum.GetValues(typeof(RewardVisualTiming)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    RewardVisualTiming current = (RewardVisualTiming) ((int) enumerator.Current);
                    rewardTimings.Add(current);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable == null)
                {
                }
                disposable.Dispose();
            }
            RewardUtils.GetViewableRewards(rewards, rewardTimings, ref rewardsToShow, ref this.m_completedQuests);
        }
        if (rewardsToShow.Count == 0)
        {
            this.ShowNextUnAckedRewardOrCompletedQuest();
        }
        else
        {
            this.m_numRewardsToLoad = rewardsToShow.Count;
            foreach (RewardData data in rewardsToShow)
            {
                data.LoadRewardObject(new Reward.DelOnRewardLoaded(this.OnRewardObjectLoaded));
            }
        }
    }

    public static bool IsLoginSceneActive()
    {
        return (s_instance != null);
    }

    public bool isWaitingForBattleNet()
    {
        return this.m_waitingForBattleNet;
    }

    private void LoginOk()
    {
        if (Network.ShouldBeConnectedToAurora())
        {
            Network.LoginOk();
            Network.RequestAssetsVersion();
        }
        else
        {
            this.AssetsVersionCheckCompleted();
        }
    }

    private void OnAccountLicensesResponse()
    {
        CheckAccountLicensesResponse checkAccountLicensesResponse = ConnectAPI.GetCheckAccountLicensesResponse();
        if (ApplicationMgr.IsPublic() && !checkAccountLicensesResponse.Success)
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER"),
                m_text = GameStrings.Get("GLOBAL_ERROR_ACCOUNT_LICENSES"),
                m_showAlertIcon = false,
                m_responseDisplay = AlertPopup.ResponseDisplay.OK
            };
            DialogManager.Get().ShowPopup(info);
        }
        base.StartCoroutine(this.RegisterScreenWhenReady());
    }

    private void OnAssetsVersion()
    {
        int version = !ApplicationMgr.IsPublic() ? Vars.Key("Application.AssetsVersion").GetInt(Network.GetAssetsVersion()) : Network.GetAssetsVersion();
        object[] args = new object[] { version, 0x2acc };
        Log.UpdateManager.Print("UpdataManager: assetsVersion = {0}, gameVersion = {1}", args);
        if ((version != 0) && (version > 0x2acc))
        {
            if (SplashScreen.Get() != null)
            {
                SplashScreen.Get().StartPatching();
            }
            UpdateManager.Get().StartInitialize(version, new UpdateManager.InitCallback(this.AssetsVersionCheckCompleted));
        }
        else
        {
            this.AssetsVersionCheckCompleted();
        }
    }

    private void OnCinematicFinished()
    {
        this.ReconnectOrChangeMode();
    }

    private void OnCollectionAchievesCompleted(List<Achievement> achievements)
    {
        <OnCollectionAchievesCompleted>c__AnonStorey302 storey = new <OnCollectionAchievesCompleted>c__AnonStorey302();
        using (List<Achievement>.Enumerator enumerator = achievements.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                storey.achieve = enumerator.Current;
                if (this.m_completedQuests.Find(new Predicate<Achievement>(storey.<>m__119)) == null)
                {
                    this.m_completedQuests.Add(storey.achieve);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    private bool OnExistingAccountLoadedCallback(DialogBase dialog, object userData)
    {
        this.m_existingAccountPopup = (ExistingAccountPopup) dialog;
        this.m_existingAccountPopup.gameObject.SetActive(true);
        return true;
    }

    private void OnExistingAccountPopupResponse(bool hasAccount)
    {
        this.m_existingAccountPopup.gameObject.SetActive(false);
        if (hasAccount)
        {
            ApplicationMgr.Get().ResetAndForceLogin();
        }
        else
        {
            this.StartTutorial();
        }
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        if ((eventData.m_state == FindGameState.SERVER_GAME_STARTED) && !GameMgr.Get().IsNextReconnect())
        {
            Spell eventSpell = Box.Get().GetEventSpell(BoxEventType.TUTORIAL_PLAY);
            eventSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnTutorialPlaySpellStateFinished));
            eventSpell.ActivateState(SpellStateType.BIRTH);
            return true;
        }
        return false;
    }

    private void OnGameLicensesResponse()
    {
        if (ConnectAPI.GetCheckGameLicensesResponse() != null)
        {
        }
    }

    private void OnMissionSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnMissionSceneLoaded));
        Box.Get().GetEventSpell(BoxEventType.TUTORIAL_PLAY).ActivateState(SpellStateType.ACTION);
    }

    private void OnNetCacheReady()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        base.StartCoroutine(this.WaitForAchievesThenInit());
    }

    private bool OnReconnectTimeout(object userData)
    {
        this.ChangeMode();
        return true;
    }

    private void OnRewardClicked(Reward reward, object userData)
    {
        reward.RemoveClickListener(new Reward.OnClickedCallback(this.OnRewardClicked));
        reward.Hide(true);
        this.ShowNextUnAckedRewardOrCompletedQuest();
    }

    private void OnRewardObjectLoaded(Reward reward, object callbackData)
    {
        reward.Hide(false);
        reward.transform.parent = base.transform;
        reward.transform.localRotation = Quaternion.identity;
        reward.transform.localPosition = REWARD_LOCAL_POS;
        this.m_rewards.Add(reward);
        if (reward.RewardType == Reward.Type.CARD)
        {
            (reward as CardReward).MakeActorsUnlit();
        }
        SceneUtils.SetLayer(reward.gameObject, GameLayer.Default);
        this.m_numRewardsToLoad--;
        if (this.m_numRewardsToLoad <= 0)
        {
            RewardUtils.SortRewards(ref this.m_rewards);
            this.ShowNextUnAckedRewardOrCompletedQuest();
        }
    }

    private void OnRewardShown(object callbackData)
    {
        Reward reward = callbackData as Reward;
        if (reward != null)
        {
            reward.RegisterClickListener(new Reward.OnClickedCallback(this.OnRewardClicked));
            reward.EnableClickCatcher(true);
        }
    }

    private void OnSetProgressResponse()
    {
        SetProgressResponse setProgressResponse = ConnectAPI.GetSetProgressResponse();
        switch (setProgressResponse.Result_)
        {
            case SetProgressResponse.Result.SUCCESS:
            case SetProgressResponse.Result.ALREADY_DONE:
                if (setProgressResponse.HasProgress)
                {
                    this.m_skipToTutorialProgress = (TutorialProgress) setProgressResponse.Progress;
                }
                Options.Get().DeleteOption(Option.LOCAL_TUTORIAL_PROGRESS);
                break;

            default:
                UnityEngine.Debug.LogWarning(string.Format("Login.OnSetProgressResponse(): received unexpected result {0}", setProgressResponse.Result_));
                break;
        }
        this.m_waitingForSetProgress = false;
    }

    private void OnStartButtonPressed(Box.ButtonType buttonType, object userData)
    {
        if (buttonType == Box.ButtonType.START)
        {
            if (this.m_nextMissionId == 3)
            {
                AdTrackingManager.Get().TrackTutorialProgress(TutorialProgress.NOTHING_COMPLETE.ToString());
            }
            Box.Get().RemoveButtonPressListener(new Box.ButtonPressCallback(this.OnStartButtonPressed));
            if (this.m_nextMissionId == 3)
            {
                if ((DemoMgr.Get().GetMode() == DemoMode.APPLE_STORE) || Network.ShouldBeConnectedToAurora())
                {
                    this.StartTutorial();
                }
                else
                {
                    Box.Get().m_StartButton.ChangeState(BoxStartButton.State.HIDDEN);
                    DialogManager.Get().ShowExistingAccountPopup(new ExistingAccountPopup.ResponseCallback(this.OnExistingAccountPopupResponse), new DialogManager.DialogProcessCallback(this.OnExistingAccountLoadedCallback));
                }
            }
            else
            {
                this.ShowTutorialProgressScreen();
            }
        }
    }

    private void OnStartupHubSpellFinished(Spell spell, object userData)
    {
        base.StartCoroutine(this.ShowUnAckedRewardsAndQuests());
    }

    private void OnStartupTutorialSpellFinished(Spell spell, object userData)
    {
        Box.Get().AddButtonPressListener(new Box.ButtonPressCallback(this.OnStartButtonPressed));
        Box.Get().ChangeState(Box.State.PRESS_START);
    }

    private void OnTutorialPlaySpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        SpellStateType activeState = spell.GetActiveState();
        if (prevStateType == SpellStateType.BIRTH)
        {
            LoadingScreen.Get().SetFadeColor(Color.white);
            LoadingScreen.Get().EnableFadeOut(false);
            LoadingScreen.Get().AddTransitionObject(Box.Get().gameObject);
            LoadingScreen.Get().AddTransitionBlocker();
            SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnMissionSceneLoaded));
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAMEPLAY);
        }
        else if (activeState == SpellStateType.NONE)
        {
            LoadingScreen.Get().NotifyTransitionBlockerComplete();
        }
    }

    private void OnTutorialProgressScreenCallback(string name, GameObject go, object callbackData)
    {
        TutorialProgressScreen component = go.GetComponent<TutorialProgressScreen>();
        component.SetCoinPressCallback(new HeroCoin.CoinPressCallback(this.StartTutorial));
        component.StartTutorialProgress();
    }

    private void OnUpdateLoginComplete()
    {
        ConnectAPI.GetUpdateLoginComplete();
        this.m_waitingForUpdateLoginComplete = false;
    }

    private void OnWelcomeQuestsCallback()
    {
        this.ShowAlertDialogs();
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
    }

    private void PreloadActors()
    {
    }

    private void ReconnectOrChangeMode()
    {
        if (ReconnectMgr.Get().ReconnectFromLogin())
        {
            ReconnectMgr.Get().AddTimeoutListener(new ReconnectMgr.TimeoutCallback(this.OnReconnectTimeout));
        }
        else
        {
            this.ChangeMode();
        }
    }

    [DebuggerHidden]
    private IEnumerator RegisterScreenWhenReady()
    {
        return new <RegisterScreenWhenReady>c__IteratorD7 { <>f__this = this };
    }

    private bool ShouldDoIntro()
    {
        return (ApplicationMgr.IsPublic() || Options.Get().GetBool(Option.INTRO));
    }

    private void ShowAlertDialogs()
    {
        this.ShowNerfedCards();
        this.ShowGoldCapAlert();
    }

    private void ShowGoldCapAlert()
    {
        NetCache.NetCacheGoldBalance netObject = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>();
        long cap = netObject.Cap;
        if (netObject.GetTotal() >= cap)
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
            object[] args = new object[] { cap.ToString() };
            info.m_headerText = GameStrings.Format("GLUE_GOLD_CAP_HEADER", args);
            object[] objArray2 = new object[] { cap.ToString() };
            info.m_text = GameStrings.Format("GLUE_GOLD_CAP_BODY", objArray2);
            info.m_showAlertIcon = true;
            info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
            DialogManager.Get().ShowPopup(info);
        }
    }

    private void ShowGraphicsDeviceWarning()
    {
        if (!Options.Get().GetBool(Option.SHOWN_GFX_DEVICE_WARNING, false))
        {
            Options.Get().SetBool(Option.SHOWN_GFX_DEVICE_WARNING, true);
            string str = SystemInfo.graphicsDeviceName.ToLower();
            if (str.Contains("powervr") && (str.Contains("540") || str.Contains("544")))
            {
                AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                    m_headerText = GameStrings.Get("GLUE_UNRELIABLE_GPU_WARNING_TITLE"),
                    m_text = GameStrings.Get("GLUE_UNRELIABLE_GPU_WARNING"),
                    m_showAlertIcon = true,
                    m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                    m_iconSet = AlertPopup.PopupInfo.IconSet.None,
                    m_cancelText = GameStrings.Get("GLOBAL_SUPPORT"),
                    m_confirmText = GameStrings.Get("GLOBAL_OKAY")
                };
                if (<>f__am$cacheF == null)
                {
                    <>f__am$cacheF = delegate (AlertPopup.Response response, object data) {
                        if (response == AlertPopup.Response.CANCEL)
                        {
                            Application.OpenURL(NydusLink.GetSupportLink("system-requirements", false));
                        }
                    };
                }
                info.m_responseCallback = <>f__am$cacheF;
                DialogManager.Get().ShowPopup(info);
            }
        }
    }

    private void ShowNerfedCards()
    {
        NetCache.NetCacheCardValues netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardValues>();
        HashSet<string> set = new HashSet<string>();
        foreach (KeyValuePair<NetCache.CardDefinition, NetCache.CardValue> pair in netObject.Values)
        {
            if (pair.Value.Nerfed && CollectionManager.Get().IsCardInCollection(pair.Key.Name, new CardFlair(pair.Key.Premium)))
            {
                int dbId = GameUtils.TranslateCardIdToDbId(pair.Key.Name);
                if (dbId != 0)
                {
                    int cardNerfIndex = netObject.CardNerfIndex;
                    if (!ChangedCardMgr.Get().AllowCard(cardNerfIndex, dbId))
                    {
                        continue;
                    }
                }
                set.Add(pair.Key.Name);
            }
        }
        if (set.Count != 0)
        {
            string str = string.Empty;
            foreach (string str2 in set)
            {
                str = str + DefLoader.Get().GetEntityDef(str2).GetName() + "\n";
            }
            GameStrings.PluralNumber[] numberArray1 = new GameStrings.PluralNumber[1];
            GameStrings.PluralNumber number = new GameStrings.PluralNumber {
                m_number = set.Count
            };
            numberArray1[0] = number;
            GameStrings.PluralNumber[] pluralNumbers = numberArray1;
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLUE_CARD_NERF_HEADER")
            };
            object[] args = new object[] { str };
            info.m_text = GameStrings.FormatPlurals("GLUE_CARD_NERF_BODY", pluralNumbers, args);
            info.m_showAlertIcon = false;
            info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
            DialogManager.Get().ShowPopup(info);
        }
    }

    private bool ShowNextCompletedQuest()
    {
        if (QuestToast.IsQuestActive())
        {
            QuestToast.GetCurrentToast().CloseQuestToast();
        }
        if (this.m_completedQuests.Count == 0)
        {
            return false;
        }
        Achievement quest = this.m_completedQuests[0];
        this.m_completedQuests.RemoveAt(0);
        QuestToast.ShowQuestToast(delegate (object userData) {
            this.ShowNextUnAckedRewardOrCompletedQuest();
        }, false, quest);
        return true;
    }

    private bool ShowNextUnAckedReward()
    {
        if (this.m_rewards.Count == 0)
        {
            return false;
        }
        Reward reward = this.m_rewards[0];
        this.m_rewards.RemoveAt(0);
        RewardUtils.ShowReward(reward, false, (Vector3) REWARD_PUNCH_SCALE, (Vector3) REWARD_SCALE, "OnRewardShown", reward, base.gameObject);
        return true;
    }

    private void ShowNextUnAckedRewardOrCompletedQuest()
    {
        if ((!BannerManager.Get().ShowABanner(new BannerManager.DelOnCloseBanner(this.ShowNextUnAckedRewardOrCompletedQuest)) && !this.ShowNextCompletedQuest()) && !this.ShowNextUnAckedReward())
        {
            HashSet<RewardVisualTiming> rewardVisualTimings = new HashSet<RewardVisualTiming> {
                RewardVisualTiming.IMMEDIATE
            };
            FixedRewardsMgr.DelOnAllFixedRewardsShown allRewardsShownCallback = delegate (object userData) {
                CollectionManager.Get().RemoveAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
                this.ShowWelcomeQuests();
            };
            if (!FixedRewardsMgr.Get().ShowFixedRewards(rewardVisualTimings, allRewardsShownCallback, null, (Vector3) REWARD_PUNCH_SCALE, (Vector3) REWARD_SCALE))
            {
                allRewardsShownCallback(null);
            }
        }
    }

    private void ShowTextureCompressionWarning()
    {
        if (ApplicationMgr.IsInternal() && !AndroidDeviceSettings.IsCurrentTextureFormatSupported())
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLUE_TEXTURE_COMPRESSION_WARNING_TITLE"),
                m_text = GameStrings.Get("GLUE_TEXTURE_COMPRESSION_WARNING"),
                m_showAlertIcon = true,
                m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                m_iconSet = AlertPopup.PopupInfo.IconSet.None,
                m_cancelText = GameStrings.Get("GLOBAL_OKAY"),
                m_confirmText = GameStrings.Get("GLOBAL_CANCEL")
            };
            if (<>f__am$cacheE == null)
            {
                <>f__am$cacheE = delegate (AlertPopup.Response response, object data) {
                    if (response == AlertPopup.Response.CANCEL)
                    {
                        Application.OpenURL("http://www.hearthstone.com.cn/download");
                    }
                };
            }
            info.m_responseCallback = <>f__am$cacheE;
            DialogManager.Get().ShowPopup(info);
        }
    }

    private void ShowTutorialProgressScreen()
    {
        Box.Get().m_StartButton.ChangeState(BoxStartButton.State.HIDDEN);
        AssetLoader.Get().LoadActor("TutorialProgressScreen", new AssetLoader.GameObjectCallback(this.OnTutorialProgressScreenCallback), null, false);
    }

    [DebuggerHidden]
    private IEnumerator ShowUnAckedRewardsAndQuests()
    {
        return new <ShowUnAckedRewardsAndQuests>c__IteratorD6 { <>f__this = this };
    }

    private void ShowWelcomeQuests()
    {
        if (!DemoMgr.Get().ShouldShowWelcomeQuests())
        {
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
        }
        else if (this.ShouldDoIntro())
        {
            WelcomeQuests.Show(true, new WelcomeQuests.DelOnWelcomeQuestsClosed(this.OnWelcomeQuestsCallback), true);
        }
        else
        {
            this.OnWelcomeQuestsCallback();
        }
    }

    private void Start()
    {
        Network network = Network.Get();
        network.RegisterNetHandler(CheckAccountLicensesResponse.PacketID.ID, new Network.NetHandler(this.OnAccountLicensesResponse), null);
        network.RegisterNetHandler(CheckGameLicensesResponse.PacketID.ID, new Network.NetHandler(this.OnGameLicensesResponse), null);
        network.RegisterNetHandler(SetProgressResponse.PacketID.ID, new Network.NetHandler(this.OnSetProgressResponse), null);
        network.RegisterNetHandler(AssetsVersionResponse.PacketID.ID, new Network.NetHandler(this.OnAssetsVersion), null);
        network.RegisterNetHandler(UpdateLoginComplete.PacketID.ID, new Network.NetHandler(this.OnUpdateLoginComplete), null);
        SceneMgr.Get().NotifySceneLoaded();
        Network.Get().OnLoginStarted();
    }

    private void StartTutorial()
    {
        MusicManager.Get().StopPlaylist();
        Box.Get().ChangeState(Box.State.CLOSED);
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        GameMgr.Get().FindGame(GameType.GT_TUTORIAL, this.m_nextMissionId, 0L, 0L);
    }

    public override void Unload()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        Network network = Network.Get();
        network.RemoveNetHandler(CheckAccountLicensesResponse.PacketID.ID, new Network.NetHandler(this.OnAccountLicensesResponse));
        network.RemoveNetHandler(CheckGameLicensesResponse.PacketID.ID, new Network.NetHandler(this.OnGameLicensesResponse));
        network.RemoveNetHandler(SetProgressResponse.PacketID.ID, new Network.NetHandler(this.OnSetProgressResponse));
        network.RemoveNetHandler(AssetsVersionResponse.PacketID.ID, new Network.NetHandler(this.OnAssetsVersion));
        network.RemoveNetHandler(UpdateLoginComplete.PacketID.ID, new Network.NetHandler(this.OnUpdateLoginComplete));
        GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
        if (this.m_waitingForBattleNet)
        {
            Network.BnetLoginState state = Network.BattleNetStatus();
            if ((((state == Network.BnetLoginState.BATTLE_NET_LOGGED_IN) && (BattleNet.GetAccountCountry() != null)) && (BattleNet.GetAccountRegion() != Network.BnetRegion.REGION_UNINITIALIZED)) || !Network.ShouldBeConnectedToAurora())
            {
                this.m_waitingForBattleNet = false;
                this.LoginOk();
            }
            else
            {
                switch (state)
                {
                    case Network.BnetLoginState.BATTLE_NET_LOGIN_FAILED:
                    case Network.BnetLoginState.BATTLE_NET_TIMEOUT:
                        this.m_waitingForBattleNet = false;
                        Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_LOGIN_FAILURE");
                        break;
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitForAchievesThenInit()
    {
        return new <WaitForAchievesThenInit>c__IteratorD5 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <OnCollectionAchievesCompleted>c__AnonStorey302
    {
        internal Achievement achieve;

        internal bool <>m__119(Achievement obj)
        {
            return (this.achieve.ID == obj.ID);
        }
    }

    [CompilerGenerated]
    private sealed class <RegisterScreenWhenReady>c__IteratorD7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Login <>f__this;

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
                    if (!DefLoader.Get().HasLoadedEntityDefs())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_00C9;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_0090;

                default:
                    goto Label_00C7;
            }
            while (this.<>f__this.m_waitingForSetProgress)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00C9;
            }
        Label_0090:
            while (this.<>f__this.m_waitingForUpdateLoginComplete)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_00C9;
            }
            ConnectAPI.OnStartupPacketSequenceComplete();
            NetCache.Get().RegisterScreenLogin(new NetCache.NetCacheCallback(this.<>f__this.OnNetCacheReady));
            this.$PC = -1;
        Label_00C7:
            return false;
        Label_00C9:
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

    [CompilerGenerated]
    private sealed class <ShowUnAckedRewardsAndQuests>c__IteratorD6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Login <>f__this;

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
                    DialogManager.Get().ReadyForSeasonEndPopup(true);
                    break;

                case 1:
                    break;

                default:
                    goto Label_0065;
            }
            if (DialogManager.Get().WaitingToShowSeasonEndDialog())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.HandleUnAckedRewardsAndCompletedQuests();
            this.$PC = -1;
        Label_0065:
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
    private sealed class <WaitForAchievesThenInit>c__IteratorD5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Login <>f__this;
        internal Cinematic <cine>__0;

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
                    if (AchieveManager.Get() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0178;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_008E;

                case 4:
                    goto Label_00E3;

                default:
                    goto Label_0176;
            }
            while (!AchieveManager.Get().IsReady())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0178;
            }
        Label_008E:
            while ((SplashScreen.Get() != null) && !SplashScreen.Get().IsRatingsScreenFinished())
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_0178;
            }
            FixedRewardsMgr.Get().InitStartupFixedRewards();
            if (DemoMgr.Get().GetMode() == DemoMode.APPLE_STORE)
            {
                this.$current = new WaitForSeconds(2f);
                this.$PC = 4;
                goto Label_0178;
            }
        Label_00E3:
            SplashScreen.Get().Hide();
            Log.Downloader.Print("LOADING PROCESS COMPLETE at " + UnityEngine.Time.realtimeSinceStartup, new object[0]);
            if (this.<>f__this.ShouldDoIntro() && !Options.Get().GetBool(Option.HAS_SEEN_CINEMATIC, false))
            {
                this.<cine>__0 = SceneMgr.Get().GetComponentInChildren<Cinematic>();
                this.<cine>__0.Play(new Cinematic.MovieCallback(this.<>f__this.OnCinematicFinished));
            }
            else
            {
                this.<>f__this.ReconnectOrChangeMode();
            }
            this.$PC = -1;
        Label_0176:
            return false;
        Label_0178:
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
}

