using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using WTCG.BI;

public class ApplicationMgr : MonoBehaviour
{
    public static readonly PlatformDependentValue<bool> AllowResetFromFatalError;
    private const float AUTO_RESET_ON_ERROR_TIMEOUT = 1f;
    public static readonly PlatformDependentValue<bool> CanQuitGame;
    private const ApplicationMode DEFAULT_MODE = ApplicationMode.INTERNAL;
    private bool m_exiting;
    private List<FocusChangedListener> m_focusChangedListeners = new List<FocusChangedListener>();
    private bool m_focused = true;
    private bool m_hasResetSinceLastResume;
    private float m_lastPauseTime;
    private float m_lastResetTime;
    private float m_lastResumeTime = -999999f;
    private bool m_resetting;
    private LinkedList<SchedulerContext> m_schedulerContexts;
    private static ApplicationMgr s_instance;
    private static ApplicationMode s_mode;

    public event System.Action Paused;

    public event System.Action Resetting;

    public event System.Action Unpaused;

    public event System.Action WillReset;

    static ApplicationMgr()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            PC = true,
            Mac = true,
            Android = false,
            iOS = false
        };
        CanQuitGame = value2;
        value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            PC = false,
            Mac = false,
            Android = true,
            iOS = true
        };
        AllowResetFromFatalError = value2;
    }

    public bool AddFocusChangedListener(FocusChangedCallback callback)
    {
        return this.AddFocusChangedListener(callback, null);
    }

    public bool AddFocusChangedListener(FocusChangedCallback callback, object userData)
    {
        FocusChangedListener item = new FocusChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_focusChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_focusChangedListeners.Add(item);
        return true;
    }

    private void Awake()
    {
        s_instance = this;
        this.Initialize();
    }

    public bool CancelScheduledCallback(ScheduledCallback cb, object userData = null)
    {
        if (GeneralUtils.IsCallbackValid(cb))
        {
            if (this.m_schedulerContexts == null)
            {
                return false;
            }
            if (this.m_schedulerContexts.Count == 0)
            {
                return false;
            }
            for (LinkedListNode<SchedulerContext> node = this.m_schedulerContexts.First; node != null; node = node.Next)
            {
                SchedulerContext context = node.Value;
                if ((context.m_callback == cb) && (context.m_userData == userData))
                {
                    this.m_schedulerContexts.Remove(node);
                    return true;
                }
            }
        }
        return false;
    }

    public void Exit()
    {
        this.m_exiting = true;
        if (ErrorReporter.Get().busy)
        {
            base.StartCoroutine(this.WaitThenExit());
        }
        else
        {
            GeneralUtils.ExitApplication();
        }
    }

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string className, string windowName);
    private void FireFocusChangedEvent()
    {
        FocusChangedListener[] listenerArray = this.m_focusChangedListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire(this.m_focused);
        }
    }

    public static ApplicationMgr Get()
    {
        return s_instance;
    }

    public static AndroidStore GetAndroidStore()
    {
        return AndroidStore.NONE;
    }

    public static MobileEnv GetMobileEnvironment()
    {
        if (Vars.Key("Mobile.Mode").GetStr("undefined") == "Production")
        {
            return MobileEnv.PRODUCTION;
        }
        return MobileEnv.DEVELOPMENT;
    }

    public static ApplicationMode GetMode()
    {
        InitializeMode();
        return s_mode;
    }

    public bool HasFocus()
    {
        return this.m_focused;
    }

    private void Initialize()
    {
        LogArchive.Init();
        InitializeMode();
        this.InitializeUnity();
        this.InitializeGame();
        this.InitializeWindowTitle();
        this.InitializeOptionValues();
        this.WillReset = (System.Action) Delegate.Combine(this.WillReset, new System.Action(GameStrings.WillReset));
    }

    private void InitializeGame()
    {
        GameDbf.Load();
        DemoMgr.Get().Initialize();
        LocalOptions.Get().Initialize();
        if (DemoMgr.Get().GetMode() == DemoMode.APPLE_STORE)
        {
            DemoMgr.Get().ApplyAppleStoreDemoDefaults();
        }
        if (Network.TUTORIALS_WITHOUT_ACCOUNT != null)
        {
            Network.SetShouldBeConnectedToAurora(Options.Get().GetBool(Option.CONNECT_TO_AURORA));
        }
        Network.Initialize();
        Localization.Initialize();
        GameStrings.LoadCategory(GameStringCategory.GLOBAL);
        if (!PlayErrors.Init())
        {
            UnityEngine.Debug.LogError(string.Format("{0} failed to load!", "PlayErrors32"));
        }
        GameMgr.Get().Initialize();
        ChangedCardMgr.Get().Initialize();
        TavernBrawlManager.Init();
    }

    private static void InitializeMode()
    {
        if (s_mode == ApplicationMode.INVALID)
        {
            s_mode = ApplicationMode.PUBLIC;
        }
    }

    private void InitializeOptionValues()
    {
        if (IsPublic())
        {
            Options.Get().SetOption(Option.SOUND, OptionDataTables.s_defaultsMap[Option.SOUND]);
            Options.Get().SetOption(Option.MUSIC, OptionDataTables.s_defaultsMap[Option.MUSIC]);
        }
    }

    private void InitializeUnity()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 30;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    private void InitializeWindowTitle()
    {
        IntPtr hWnd = FindWindow(null, "Hearthstone");
        if (hWnd != IntPtr.Zero)
        {
            SetWindowTextW(hWnd, GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE"));
        }
    }

    public bool IsExiting()
    {
        return this.m_exiting;
    }

    public static bool IsInternal()
    {
        return (GetMode() == ApplicationMode.INTERNAL);
    }

    public static bool IsPublic()
    {
        return (GetMode() == ApplicationMode.PUBLIC);
    }

    public bool IsResetting()
    {
        return this.m_resetting;
    }

    public float LastResetTime()
    {
        return this.m_lastResetTime;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (this.m_focused != focus)
        {
            this.m_focused = focus;
            this.FireFocusChangedEvent();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (UnityEngine.Time.frameCount != 0)
        {
            if (pauseStatus)
            {
                this.m_lastPauseTime = UnityEngine.Time.realtimeSinceStartup;
                if (this.Paused != null)
                {
                    this.Paused();
                }
                UberText.StoreCachedData();
                Network.ApplicationPaused();
            }
            else
            {
                this.m_hasResetSinceLastResume = false;
                float num = UnityEngine.Time.realtimeSinceStartup - this.m_lastPauseTime;
                UnityEngine.Debug.Log("Time spent paused: " + num);
                if ((DemoMgr.Get().GetMode() == DemoMode.APPLE_STORE) && (num > 180f))
                {
                    this.ResetImmediately(false, false);
                }
                this.m_lastResumeTime = UnityEngine.Time.realtimeSinceStartup;
                if (this.Unpaused != null)
                {
                    this.Unpaused();
                }
                Network.ApplicationUnpaused();
                if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FATAL_ERROR))
                {
                    this.ResetImmediately(false, false);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        UberText.StoreCachedData();
        Network.AppQuit();
        W8Touch.AppQuit();
        this.UnloadUnusedAssets();
    }

    private void OnDestroy()
    {
    }

    private void OnGUI()
    {
        UnityEngine.Debug.developerConsoleVisible = false;
        BugReporter.Get().OnGUI();
    }

    private void ProcessScheduledCallbacks()
    {
        if ((this.m_schedulerContexts != null) && (this.m_schedulerContexts.Count != 0))
        {
            LinkedList<SchedulerContext> list = null;
            float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            LinkedListNode<SchedulerContext> first = this.m_schedulerContexts.First;
            while (first != null)
            {
                SchedulerContext context = first.Value;
                if (context.m_realTime)
                {
                    context.m_secondsWaited = realtimeSinceStartup - context.m_startTime;
                }
                else
                {
                    context.m_secondsWaited += UnityEngine.Time.deltaTime;
                }
                if (context.m_secondsWaited >= context.m_secondsToWait)
                {
                    if (list == null)
                    {
                        list = new LinkedList<SchedulerContext>();
                    }
                    list.AddLast(context);
                    LinkedListNode<SchedulerContext> next = first.Next;
                    this.m_schedulerContexts.Remove(first);
                    first = next;
                }
                else if (!GeneralUtils.IsCallbackValid(context.m_callback))
                {
                    LinkedListNode<SchedulerContext> node3 = first.Next;
                    this.m_schedulerContexts.Remove(first);
                    first = node3;
                }
                else
                {
                    first = first.Next;
                }
            }
            if (list != null)
            {
                foreach (SchedulerContext context2 in list)
                {
                    context2.m_callback(context2.m_userData);
                }
            }
        }
    }

    public bool RemoveFocusChangedListener(FocusChangedCallback callback)
    {
        return this.RemoveFocusChangedListener(callback, null);
    }

    public bool RemoveFocusChangedListener(FocusChangedCallback callback, object userData)
    {
        FocusChangedListener item = new FocusChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_focusChangedListeners.Remove(item);
    }

    public void Reset()
    {
        base.StartCoroutine(this.WaitThenReset(false, false));
    }

    public void ResetAndForceLogin()
    {
        ConnectAPI.ResetSubscription();
        base.StartCoroutine(this.WaitThenReset(true, false));
    }

    public void ResetAndGoBackToNoAccountTutorial()
    {
        ConnectAPI.ResetSubscription();
        base.StartCoroutine(this.WaitThenReset(false, true));
    }

    private void ResetImmediately(bool forceLogin, bool forceNoAccountTutorial = false)
    {
        Log.Reset.Print(string.Concat(new object[] { "ApplicationMgr.ResetImmediately - forceLogin? ", forceLogin, "  Stack trace: ", Environment.StackTrace }), new object[0]);
        BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_INFO, !forceLogin ? BIReport.TelemetryEvent.EVENT_ON_RESET : BIReport.TelemetryEvent.EVENT_ON_RESET_WITH_LOGIN);
        if (this.WillReset != null)
        {
            this.WillReset();
        }
        this.m_resetting = true;
        this.m_lastResetTime = UnityEngine.Time.realtimeSinceStartup;
        if (DialogManager.Get() != null)
        {
            DialogManager.Get().Suppress(true);
            DialogManager.Get().Suppress(false);
        }
        if (Network.TUTORIALS_WITHOUT_ACCOUNT != null)
        {
            Network.SetShouldBeConnectedToAurora(forceLogin || Options.Get().GetBool(Option.CONNECT_TO_AURORA));
        }
        FatalErrorMgr.Get().ClearAllErrors();
        this.m_hasResetSinceLastResume = true;
        if (forceNoAccountTutorial)
        {
            Options.Get().SetBool(Option.CONNECT_TO_AURORA, false);
            Network.SetShouldBeConnectedToAurora(false);
        }
        if (this.Resetting != null)
        {
            this.Resetting();
        }
        Network.Reset();
        Navigation.Clear();
        this.m_resetting = false;
        Log.Reset.Print("\tApplicationMgr.ResetImmediately completed", new object[0]);
    }

    public bool ResetOnErrorIfNecessary()
    {
        if (!this.m_hasResetSinceLastResume && (UnityEngine.Time.realtimeSinceStartup < (this.m_lastResumeTime + 1f)))
        {
            base.StartCoroutine(this.WaitThenReset(false, false));
            return true;
        }
        return false;
    }

    public bool ScheduleCallback(float secondsToWait, bool realTime, ScheduledCallback cb, object userData = null)
    {
        if (!GeneralUtils.IsCallbackValid(cb))
        {
            return false;
        }
        if (this.m_schedulerContexts == null)
        {
            this.m_schedulerContexts = new LinkedList<SchedulerContext>();
        }
        else
        {
            foreach (SchedulerContext context in this.m_schedulerContexts)
            {
                if ((context.m_callback == cb) && (context.m_userData == userData))
                {
                    return false;
                }
            }
        }
        SchedulerContext context2 = new SchedulerContext {
            m_startTime = UnityEngine.Time.realtimeSinceStartup,
            m_secondsToWait = secondsToWait,
            m_realTime = realTime,
            m_callback = cb,
            m_userData = userData
        };
        float num = context2.EstimateTargetTime();
        bool flag = false;
        for (LinkedListNode<SchedulerContext> node = this.m_schedulerContexts.Last; node != null; node = node.Previous)
        {
            if (node.Value.EstimateTargetTime() <= num)
            {
                flag = true;
                this.m_schedulerContexts.AddAfter(node, context2);
                break;
            }
        }
        if (!flag)
        {
            this.m_schedulerContexts.AddFirst(context2);
        }
        return true;
    }

    [DllImport("user32.dll")]
    public static extern int SetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string text);
    private void Start()
    {
        AutomationInterpretor.Get().Start();
    }

    public void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
    }

    private void Update()
    {
        this.ProcessScheduledCallbacks();
        Network.Heartbeat();
        AutomationInterpretor.Get().Update();
    }

    [DebuggerHidden]
    private IEnumerator WaitThenExit()
    {
        return new <WaitThenExit>c__Iterator22C();
    }

    [DebuggerHidden]
    private IEnumerator WaitThenReset(bool forceLogin, bool forceNoAccountTutorial = false)
    {
        return new <WaitThenReset>c__Iterator22B { forceLogin = forceLogin, forceNoAccountTutorial = forceNoAccountTutorial, <$>forceLogin = forceLogin, <$>forceNoAccountTutorial = forceNoAccountTutorial, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitThenExit>c__Iterator22C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                    if (ErrorReporter.Get().busy)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    GeneralUtils.ExitApplication();
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
    private sealed class <WaitThenReset>c__Iterator22B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>forceLogin;
        internal bool <$>forceNoAccountTutorial;
        internal ApplicationMgr <>f__this;
        internal bool forceLogin;
        internal bool forceNoAccountTutorial;

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
                    this.<>f__this.m_resetting = true;
                    Navigation.Clear();
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.ResetImmediately(this.forceLogin, this.forceNoAccountTutorial);
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

    public delegate void FocusChangedCallback(bool focused, object userData);

    private class FocusChangedListener : EventListener<ApplicationMgr.FocusChangedCallback>
    {
        public void Fire(bool focused)
        {
            base.m_callback(focused, base.m_userData);
        }
    }

    public delegate void ScheduledCallback(object userData);

    private class SchedulerContext
    {
        public ApplicationMgr.ScheduledCallback m_callback;
        public bool m_realTime;
        public float m_secondsToWait;
        public float m_secondsWaited;
        public float m_startTime;
        public object m_userData;

        public float EstimateTargetTime()
        {
            return (this.m_startTime + (!this.m_realTime ? (this.m_secondsToWait * UnityEngine.Time.timeScale) : this.m_secondsToWait));
        }
    }
}

