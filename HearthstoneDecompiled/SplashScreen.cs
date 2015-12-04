using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using WTCG.BI;

[CustomEditClass]
public class SplashScreen : MonoBehaviour
{
    private const float GLOW_FADE_TIME = 1f;
    private const float KOREA_RATINGS_SCREEN_DISPLAY_TIME = 5f;
    public GameObject m_blizzardLogo;
    public GameObject m_demoDisclaimer;
    public StandardPegButtonNew m_devClearLoginButton;
    private bool m_fadingStarted;
    private List<FinishedHandler> m_finishedListeners = new List<FinishedHandler>();
    public Glow m_glow1;
    public Glow m_glow2;
    private bool m_inputCameraSet;
    private bool m_loginFinished;
    private GameObject m_logo;
    public GameObject m_logoContainer;
    private bool m_LogoFinished;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_logoPrefab;
    private bool m_patching;
    public ProgressBar m_patchingBar;
    private float m_patchingBarShownTime;
    public GameObject m_patchingFrame;
    private bool m_queueFinished;
    private Network.QueueInfo m_queueInfo;
    private bool m_queueShown;
    public GameObject m_queueSign;
    public UberText m_queueText;
    public UberText m_queueTime;
    public UberText m_queueTitle;
    public StandardPegButtonNew m_quitButton;
    public GameObject m_quitButtonParent;
    private bool m_RatingsFinished;
    private bool m_triedToken;
    private WebAuth m_webAuth;
    private bool m_webAuthHidden;
    private string m_webAuthUrl;
    private GameObject m_webLoginCanvas;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public String_MobileOverride m_webLoginCanvasPrefab;
    private string m_webToken;
    private const float MinPatchingBarDisplayTime = 3f;
    private static SplashScreen s_instance;

    public bool AddFinishedListener(FinishedHandler handler)
    {
        if (this.m_finishedListeners.Contains(handler))
        {
            return false;
        }
        this.m_finishedListeners.Add(handler);
        return true;
    }

    private void Awake()
    {
        s_instance = this;
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        this.m_logo = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(this.m_logoPrefab), true, false);
        GameUtils.SetParent(this.m_logo, this.m_logoContainer, true);
        this.m_logo.SetActive(false);
        this.m_webLoginCanvas = null;
        this.Show();
        this.UpdateLayout();
        if (Vars.Key("Aurora.ClientCheck").GetBool(true) && BattleNetClient.needsToRun)
        {
            BattleNetClient.quitHearthstoneAndRun();
        }
        else
        {
            Network.Get().RegisterQueueInfoHandler(new Network.QueueInfoHandler(this.QueueInfoHandler));
            if (DemoMgr.Get().GetMode() == DemoMode.APPLE_STORE)
            {
                this.m_demoDisclaimer.SetActive(true);
            }
            if (ApplicationMgr.IsInternal() && (ApplicationMgr.AllowResetFromFatalError != null))
            {
                this.m_devClearLoginButton.gameObject.SetActive(true);
                this.m_devClearLoginButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ClearLogin));
            }
        }
    }

    private void ClearLogin(UIEvent e)
    {
        UnityEngine.Debug.LogWarning("Clear Login Button pressed from the Splash Screen!");
        WebAuth.ClearLoginData();
    }

    private void DestroySplashScreen()
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    [DebuggerHidden]
    private IEnumerator FadeGlowInOut(Glow glow, float timeDelay, bool shouldStartOver)
    {
        return new <FadeGlowInOut>c__Iterator1D2 { timeDelay = timeDelay, glow = glow, shouldStartOver = shouldStartOver, <$>timeDelay = timeDelay, <$>glow = glow, <$>shouldStartOver = shouldStartOver, <>f__this = this };
    }

    private void FadeGlowsIn()
    {
        this.m_fadingStarted = true;
        base.StartCoroutine(this.FadeGlowInOut(this.m_glow1, 0f, false));
        base.StartCoroutine(this.FadeGlowInOut(this.m_glow2, 1f, true));
    }

    [DebuggerHidden]
    private IEnumerator FadeInLogo()
    {
        return new <FadeInLogo>c__Iterator1D4 { <>f__this = this };
    }

    public void FinishSplashScreen()
    {
        this.m_loginFinished = true;
        base.StartCoroutine(this.FadeInLogo());
        base.StartCoroutine(this.FireFinishedEvent());
    }

    [DebuggerHidden]
    private IEnumerator FireFinishedEvent()
    {
        return new <FireFinishedEvent>c__Iterator1D3 { <>f__this = this };
    }

    public static SplashScreen Get()
    {
        return s_instance;
    }

    public bool HandleKeyboardInput()
    {
        return false;
    }

    public void Hide()
    {
        if (this.m_webAuth != null)
        {
            this.m_webAuth.Close();
        }
        this.HideWebLoginCanvas();
        base.StartCoroutine(this.HideCoroutine());
    }

    [DebuggerHidden]
    private IEnumerator HideCoroutine()
    {
        return new <HideCoroutine>c__Iterator1D1 { <>f__this = this };
    }

    public void HideLogo()
    {
        this.m_logo.SetActive(false);
    }

    public void HideWebAuth()
    {
        UnityEngine.Debug.Log("HideWebAuth");
        if (this.m_webAuth != null)
        {
            this.m_webAuth.Hide();
            this.m_webAuthHidden = true;
        }
    }

    public void HideWebLoginCanvas()
    {
        if (this.m_webLoginCanvas != null)
        {
            UnityEngine.Object.Destroy(this.m_webLoginCanvas);
            this.m_webLoginCanvas = null;
        }
    }

    public bool IsFinished()
    {
        return this.m_loginFinished;
    }

    public bool IsRatingsScreenFinished()
    {
        return this.m_RatingsFinished;
    }

    private void KoreaRatingsScreen()
    {
        AssetLoader.Get().LoadGameObject("Korean_Ratings_SplashScreen", new AssetLoader.GameObjectCallback(this.OnKoreaRatingsScreenLoaded), null, false);
    }

    [DebuggerHidden]
    private IEnumerator KoreaRatingsScreenWait(GameObject ratingsObject)
    {
        return new <KoreaRatingsScreenWait>c__Iterator1D6 { ratingsObject = ratingsObject, <$>ratingsObject = ratingsObject, <>f__this = this };
    }

    private void OnDestroy()
    {
        if (this.m_webAuth != null)
        {
            this.m_webAuth.Close();
        }
        s_instance = null;
    }

    private void OnKoreaRatingsScreenLoaded(string name, GameObject go, object callbackData)
    {
        OverlayUI.Get().AddGameObject(go, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        go.SetActive(false);
        this.m_queueSign.SetActive(false);
        this.m_queueTitle.gameObject.SetActive(false);
        this.m_queueText.gameObject.SetActive(false);
        this.m_queueTime.gameObject.SetActive(false);
        base.StartCoroutine(this.KoreaRatingsScreenWait(go));
    }

    private void QueueInfoHandler(Network.QueueInfo queueInfo)
    {
        this.m_queueInfo = queueInfo;
        if (queueInfo.position == 0)
        {
            Network.Get().RemoveQueueInfoHandler(new Network.QueueInfoHandler(this.QueueInfoHandler));
            this.m_queueFinished = true;
            this.m_queueShown = false;
            this.m_queueSign.SetActive(false);
            this.FinishSplashScreen();
        }
        else
        {
            this.ShowQueueInfo();
        }
    }

    private void QuitGame(UIEvent e)
    {
        ApplicationMgr.Get().Exit();
    }

    [DebuggerHidden]
    private IEnumerator RatingsScreen()
    {
        return new <RatingsScreen>c__Iterator1D5 { <>f__this = this };
    }

    public virtual bool RemoveFinishedListener(FinishedHandler handler)
    {
        return this.m_finishedListeners.Remove(handler);
    }

    private void RequestBreakingNews()
    {
        <RequestBreakingNews>c__AnonStorey32A storeya = new <RequestBreakingNews>c__AnonStorey32A {
            breakingNewsLocalized = GameStrings.Get("GLUE_MOBILE_SPLASH_SCREEN_BREAKING_NEWS")
        };
        BreakingNews.FetchBreakingNews(NydusLink.GetBreakingNewsLink(), new BreakingNews.BreakingNewsRecievedDelegate(storeya.<>m__15E));
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
        object[] args = new object[] { "amount", 1f, "time", 0.25f, "easeType", iTween.EaseType.easeOutCubic };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(base.gameObject, hashtable);
        if (!this.m_fadingStarted)
        {
            this.FadeGlowsIn();
        }
    }

    private void ShowQueueInfo()
    {
        if (!this.m_queueShown)
        {
            this.m_queueShown = true;
            this.m_queueTitle.Text = GameStrings.Get("GLUE_SPLASH_QUEUE_TITLE");
            this.m_queueText.Text = GameStrings.Get("GLUE_SPLASH_QUEUE_TEXT");
            this.m_quitButton.SetOriginalLocalPosition();
            this.m_quitButton.SetText(GameStrings.Get("GLOBAL_QUIT"));
            this.m_quitButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.QuitGame));
            RenderUtils.SetAlpha(this.m_queueSign, 0f);
            this.m_queueSign.SetActive(true);
            object[] args = new object[] { "amount", 1f, "time", 0.5f, "easeType", iTween.EaseType.easeOutCubic };
            Hashtable hashtable = iTween.Hash(args);
            iTween.FadeTo(this.m_queueSign, hashtable);
            object[] objArray2 = new object[] { "amount", 0f, "time", 0.5f, "includechildren", true, "easeType", iTween.EaseType.easeOutCubic };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.FadeTo(this.m_logo, hashtable2);
        }
        TimeUtils.ElapsedStringSet stringSet = TimeUtils.SPLASHSCREEN_DATETIME_STRINGSET;
        this.m_queueTime.Text = TimeUtils.GetElapsedTimeString((int) this.m_queueInfo.end, stringSet);
    }

    public void ShowRatings()
    {
        base.StartCoroutine(this.RatingsScreen());
    }

    public void StartPatching()
    {
        this.m_patching = true;
    }

    public void StopPatching()
    {
        this.m_patching = false;
        if ((this.m_patchingBar != null) && this.m_patchingBar.gameObject.activeInHierarchy)
        {
            this.m_patchingBar.SetProgressBar(1f);
        }
    }

    public void UnHideWebAuth()
    {
        UnityEngine.Debug.Log("ShowWebAuth");
        if ((this.m_webAuth != null) && (this.m_webAuth.GetStatus() >= WebAuth.Status.ReadyToDisplay))
        {
            this.m_webAuth.Show();
            this.m_webAuthHidden = false;
        }
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
        if (!this.m_queueFinished && !Network.ShouldBeConnectedToAurora())
        {
            this.m_queueFinished = true;
        }
        if (!this.m_inputCameraSet && (PegUI.Get() != null))
        {
            this.m_inputCameraSet = true;
            PegUI.Get().SetInputCamera(OverlayUI.Get().m_UICamera);
        }
        if (!PlayErrors.IsInitialized())
        {
            PlayErrors.Init();
        }
        this.UpdatePatching();
        this.HandleKeyboardInput();
    }

    public void UpdateLayout()
    {
        Bounds nearClipBounds = CameraUtils.GetNearClipBounds(OverlayUI.Get().m_UICamera);
        float num = this.m_blizzardLogo.GetComponent<Renderer>().bounds.size.x / 1.5f;
        TransformUtil.SetPosX(this.m_blizzardLogo, nearClipBounds.max.x - num);
    }

    public void UpdatePatching()
    {
        if (this.m_patching)
        {
            UpdateManager.UpdateProgress progressForCurrentFile = UpdateManager.Get().GetProgressForCurrentFile();
            if ((!this.m_patchingFrame.activeSelf && progressForCurrentFile.showProgressBar) && (progressForCurrentFile.numFilesToDownload > 0))
            {
                this.m_patchingFrame.SetActive(true);
                this.m_patchingBarShownTime = UnityEngine.Time.realtimeSinceStartup;
            }
            if (this.m_patchingFrame.activeSelf && (progressForCurrentFile.numFilesToDownload > progressForCurrentFile.numFilesDownloaded))
            {
                float progress = (((float) progressForCurrentFile.numFilesDownloaded) / ((float) progressForCurrentFile.numFilesToDownload)) + (progressForCurrentFile.downloadPercentage / ((float) progressForCurrentFile.numFilesToDownload));
                this.m_patchingBar.SetProgressBar(progress);
                object[] args = new object[] { progressForCurrentFile.numFilesDownloaded + 1, progressForCurrentFile.numFilesToDownload };
                string text = GameStrings.Format("GLUE_PATCHING_LABEL", args);
                this.m_patchingBar.SetLabel(text);
                if (!UpdateManager.Get().UpdateIsRequired() && (UnityEngine.Time.realtimeSinceStartup >= (this.m_patchingBarShownTime + progressForCurrentFile.maxPatchingBarDisplayTime)))
                {
                    Log.UpdateManager.Print("Optional update is taking too long, no longer blocking.", new object[0]);
                    UpdateManager.Get().StopWaitingForUpdate();
                }
            }
        }
        if (((this.m_patchingFrame != null) && this.m_patchingFrame.activeSelf) && (!this.m_patching && (UnityEngine.Time.realtimeSinceStartup >= (this.m_patchingBarShownTime + 3f))))
        {
            this.m_patchingFrame.SetActive(false);
        }
    }

    public void UpdateWebAuth()
    {
        if (this.m_webAuth != null)
        {
            switch (this.m_webAuth.GetStatus())
            {
                case WebAuth.Status.ReadyToDisplay:
                    if (!this.m_webLoginCanvas.activeSelf)
                    {
                        this.m_webLoginCanvas.SetActive(true);
                    }
                    if (!this.m_webAuthHidden && !this.m_webAuth.IsShown())
                    {
                        this.m_webAuth.Show();
                    }
                    break;

                case WebAuth.Status.Success:
                {
                    this.m_webToken = this.m_webAuth.GetLoginCode();
                    object[] args = new object[] { this.m_webToken };
                    Log.BattleNet.Print("Web token retrieved from web pane: {0}", args);
                    WebAuth.SetStoredToken(this.m_webToken);
                    BattleNet.ProvideWebAuthToken(this.m_webToken);
                    this.HideWebLoginCanvas();
                    this.m_webAuth.Close();
                    break;
                }
                case WebAuth.Status.Error:
                    BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_ERROR, BIReport.TelemetryEvent.EVENT_WEB_LOGIN_ERROR);
                    Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_LOGIN_FAILURE");
                    break;
            }
        }
        else if (BattleNet.CheckWebAuth(out this.m_webAuthUrl))
        {
            if (GameUtils.AreAllTutorialsComplete(Options.Get().GetEnum<TutorialProgress>(Option.LOCAL_TUTORIAL_PROGRESS)))
            {
                this.m_webAuthUrl = NydusLink.GetAccountCreationLink();
            }
            UnityEngine.Debug.Log("Web URL for auth: " + this.m_webAuthUrl);
            MobileCallbackManager.Get().m_wasBreakingNewsShown = false;
            if (!string.IsNullOrEmpty(this.m_webToken))
            {
                BattleNet.ProvideWebAuthToken(this.m_webToken);
                this.m_triedToken = true;
                this.m_webToken = null;
                this.RequestBreakingNews();
            }
            else
            {
                this.m_webLoginCanvas = (GameObject) GameUtils.InstantiateGameObject((string) this.m_webLoginCanvasPrefab, null, false);
                WebLoginCanvas component = this.m_webLoginCanvas.GetComponent<WebLoginCanvas>();
                this.m_webLoginCanvas.SetActive(false);
                Camera uICamera = OverlayUI.Get().m_UICamera;
                Vector3 vector = uICamera.WorldToScreenPoint(component.m_topLeftBone.transform.position);
                Vector3 vector2 = uICamera.WorldToScreenPoint(component.m_bottomRightBone.transform.position);
                float x = vector.x;
                float y = uICamera.pixelHeight - vector.y;
                float width = vector2.x - vector.x;
                float height = vector.y - vector2.y;
                this.m_webAuth = new WebAuth(this.m_webAuthUrl, x, y, width, height, this.m_webLoginCanvas.gameObject.name);
                string domain = (ApplicationMgr.GetMobileEnvironment() != MobileEnv.DEVELOPMENT) ? ".battle.net" : ".blizzard.net";
                this.m_webAuth.SetCountryCodeCookie(MobileDeviceLocale.GetCountryCode(), domain);
                this.m_webAuth.Load();
                this.RequestBreakingNews();
                MobileCallbackManager.Get().m_wasBreakingNewsShown = true;
                if (this.m_triedToken)
                {
                    object[] objArray2 = new object[] { this.m_webToken };
                    Log.BattleNet.Print("Submitted login token {0} but it was rejected, so deleting stored token.", objArray2);
                    WebAuth.ClearLoginData();
                }
            }
        }
    }

    [CompilerGenerated]
    private sealed class <FadeGlowInOut>c__Iterator1D2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Glow <$>glow;
        internal bool <$>shouldStartOver;
        internal float <$>timeDelay;
        internal SplashScreen <>f__this;
        internal Hashtable <args>__0;
        internal Hashtable <args2>__1;
        internal Glow glow;
        internal bool shouldStartOver;
        internal float timeDelay;

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
                    this.$current = new WaitForSeconds(this.timeDelay);
                    this.$PC = 1;
                    return true;

                case 1:
                {
                    object[] args = new object[] { "time", 1f, "easeType", iTween.EaseType.linear, "from", 0f, "to", 0.4f, "onupdate", "UpdateAlpha", "onupdatetarget", this.glow.gameObject };
                    this.<args>__0 = iTween.Hash(args);
                    iTween.ValueTo(this.glow.gameObject, this.<args>__0);
                    object[] objArray2 = new object[] { "delay", 1f, "time", 1f, "easeType", iTween.EaseType.linear, "from", 0.4f, "to", 0f, "onupdate", "UpdateAlpha", "onupdatetarget", this.glow.gameObject };
                    this.<args2>__1 = iTween.Hash(objArray2);
                    if (this.shouldStartOver)
                    {
                        this.<args2>__1.Add("oncomplete", "FadeGlowsIn");
                        this.<args2>__1.Add("oncompletetarget", this.<>f__this.gameObject);
                    }
                    iTween.ValueTo(this.glow.gameObject, this.<args2>__1);
                    this.$PC = -1;
                    break;
                }
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
    private sealed class <FadeInLogo>c__Iterator1D4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SplashScreen <>f__this;
        internal Hashtable <fadeInArgs>__0;

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
                {
                    this.<>f__this.m_logo.SetActive(true);
                    object[] args = new object[] { "amount", 1f, "time", 0.5f, "includechildren", true, "easeType", iTween.EaseType.easeInCubic };
                    this.<fadeInArgs>__0 = iTween.Hash(args);
                    iTween.FadeTo(this.<>f__this.m_logo, this.<fadeInArgs>__0);
                    this.$current = new WaitForSeconds(0.8f);
                    this.$PC = 1;
                    return true;
                }
                case 1:
                    this.<>f__this.m_LogoFinished = true;
                    break;

                default:
                    break;
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
    private sealed class <FireFinishedEvent>c__Iterator1D3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SplashScreen <>f__this;
        internal int <i>__1;
        internal SplashScreen.FinishedHandler <listener>__2;
        internal SplashScreen.FinishedHandler[] <listenersCopy>__0;

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
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<listenersCopy>__0 = this.<>f__this.m_finishedListeners.ToArray();
                    this.<i>__1 = 0;
                    while (this.<i>__1 < this.<listenersCopy>__0.Length)
                    {
                        this.<listener>__2 = this.<listenersCopy>__0[this.<i>__1];
                        this.<listener>__2();
                        this.<i>__1++;
                    }
                    break;

                default:
                    break;
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
    private sealed class <HideCoroutine>c__Iterator1D1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SplashScreen <>f__this;
        internal Hashtable <args>__0;
        internal Hashtable <g1args>__1;
        internal Hashtable <g2args>__2;

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
                    if (!this.<>f__this.m_queueFinished || !this.<>f__this.m_RatingsFinished)
                    {
                        this.$current = null;
                        this.$PC = 1;
                    }
                    else
                    {
                        BnetBar.Get().ToggleEnableButtons(true);
                        object[] args = new object[] { "amount", 0f, "delay", 0.9f, "time", 0.7f, "easeType", iTween.EaseType.linear, "oncomplete", "DestroySplashScreen", "oncompletetarget", this.<>f__this.gameObject };
                        this.<args>__0 = iTween.Hash(args);
                        iTween.FadeTo(this.<>f__this.gameObject, this.<args>__0);
                        object[] objArray2 = new object[] { "amount", 0f, "delay", 0f, "time", 0.5f, "easeType", iTween.EaseType.linear, "oncompletetarget", this.<>f__this.gameObject };
                        this.<g1args>__1 = iTween.Hash(objArray2);
                        iTween.FadeTo(this.<>f__this.m_glow1.gameObject, this.<g1args>__1);
                        object[] objArray3 = new object[] { "amount", 0f, "delay", 0f, "time", 0.5f, "easeType", iTween.EaseType.linear, "oncompletetarget", this.<>f__this.gameObject };
                        this.<g2args>__2 = iTween.Hash(objArray3);
                        iTween.FadeTo(this.<>f__this.m_glow2.gameObject, this.<g2args>__2);
                        this.$current = new WaitForSeconds(0.5f);
                        this.$PC = 2;
                    }
                    return true;

                case 2:
                    this.<>f__this.m_glow1.gameObject.SetActive(false);
                    this.<>f__this.m_glow2.gameObject.SetActive(false);
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
    private sealed class <KoreaRatingsScreenWait>c__Iterator1D6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>ratingsObject;
        internal SplashScreen <>f__this;
        internal Hashtable <args2>__0;
        internal Hashtable <fadeInRatings>__1;
        internal Hashtable <fadeOutRatings>__2;
        internal GameObject ratingsObject;

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
                {
                    object[] args = new object[] { "amount", 0f, "time", 0.5f, "includechildren", true, "easeType", iTween.EaseType.easeOutCubic };
                    this.<args2>__0 = iTween.Hash(args);
                    iTween.FadeTo(this.<>f__this.m_logo, this.<args2>__0);
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    goto Label_0215;
                }
                case 1:
                {
                    this.<>f__this.m_logo.SetActive(false);
                    this.ratingsObject.SetActive(true);
                    object[] objArray2 = new object[] { "amount", 1f, "time", 0.5f, "includechildren", true, "easeType", iTween.EaseType.easeInCubic };
                    this.<fadeInRatings>__1 = iTween.Hash(objArray2);
                    iTween.FadeTo(this.ratingsObject, this.<fadeInRatings>__1);
                    this.$current = new WaitForSeconds(5.5f);
                    this.$PC = 2;
                    goto Label_0215;
                }
                case 2:
                {
                    object[] objArray3 = new object[] { "amount", 0f, "time", 0.5f, "includechildren", true, "easeType", iTween.EaseType.easeInCubic };
                    this.<fadeOutRatings>__2 = iTween.Hash(objArray3);
                    iTween.FadeTo(this.ratingsObject, this.<fadeOutRatings>__2);
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 3;
                    goto Label_0215;
                }
                case 3:
                    this.ratingsObject.SetActive(false);
                    UnityEngine.Object.Destroy(this.ratingsObject);
                    this.<>f__this.m_RatingsFinished = true;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0215:
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
    private sealed class <RatingsScreen>c__Iterator1D5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SplashScreen <>f__this;
        internal string <accountCountry>__0;
        internal bool <useKoreanRating>__1;

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
                    if (!this.<>f__this.m_LogoFinished)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_00C6;
                    }
                    this.<accountCountry>__0 = BattleNet.GetAccountCountry();
                    this.<useKoreanRating>__1 = this.<accountCountry>__0 == "KOR";
                    if (this.<useKoreanRating>__1)
                    {
                        this.<>f__this.KoreaRatingsScreen();
                    }
                    else
                    {
                        this.<>f__this.m_RatingsFinished = true;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_00C4;
            }
            if (!this.<>f__this.m_RatingsFinished)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00C6;
            }
            this.$PC = -1;
        Label_00C4:
            return false;
        Label_00C6:
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
    private sealed class <RequestBreakingNews>c__AnonStorey32A
    {
        internal string breakingNewsLocalized;

        internal void <>m__15E(string response, bool error)
        {
            if (!error)
            {
                WebAuth.UpdateBreakingNews(this.breakingNewsLocalized, response, MobileCallbackManager.Get().name);
            }
        }
    }

    public delegate void FinishedHandler();
}

