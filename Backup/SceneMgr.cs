using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SceneMgr : MonoBehaviour
{
    private long m_boxLoadTimestamp;
    private float m_lastMemoryWarning;
    private Mode m_mode = Mode.STARTUP;
    private Mode m_nextMode;
    private bool m_performFullCleanup;
    private Mode m_prevMode;
    private bool m_reloadMode;
    private Scene m_scene;
    private bool m_sceneLoaded;
    private List<SceneLoadedListener> m_sceneLoadedListeners = new List<SceneLoadedListener>();
    private List<ScenePreLoadListener> m_scenePreLoadListeners = new List<ScenePreLoadListener>();
    private List<ScenePreUnloadListener> m_scenePreUnloadListeners = new List<ScenePreUnloadListener>();
    private List<SceneUnloadedListener> m_sceneUnloadedListeners = new List<SceneUnloadedListener>();
    private int m_startupAssetLoads;
    public GameObject m_StartupCamera;
    private bool m_textInputGUISkinLoaded;
    private bool m_transitioning;
    private static SceneMgr s_instance;
    private const float SCENE_LOADED_DELAY = 0.15f;
    private const float SCENE_UNLOAD_DELAY = 0.15f;
    private const int TRIM_MEMORY_RUNNING_CRITICAL = 15;
    private const int TRIM_MEMORY_RUNNING_LOW = 10;
    private const int TRIM_MEMORY_RUNNING_MODERATE = 5;

    private void Awake()
    {
        s_instance = this;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        this.m_transitioning = true;
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
    }

    private void CacheModeForResume(Mode mode)
    {
        if ((PlatformSettings.OS == OSCategory.iOS) || (PlatformSettings.OS == OSCategory.Android))
        {
            switch (mode)
            {
                case Mode.HUB:
                case Mode.FRIENDLY:
                    Options.Get().SetInt(Option.LAST_SCENE_MODE, 0);
                    break;

                case Mode.COLLECTIONMANAGER:
                case Mode.TOURNAMENT:
                case Mode.DRAFT:
                case Mode.CREDITS:
                case Mode.ADVENTURE:
                case Mode.TAVERN_BRAWL:
                    Options.Get().SetInt(Option.LAST_SCENE_MODE, (int) mode);
                    break;
            }
        }
    }

    public void ClearCachesAndFreeMemory(int severity)
    {
        UnityEngine.Debug.LogWarning(string.Format("Clearing Caches; this will force assets to be reloaded off disk and may be slow! {0}", severity));
        if (StoreManager.Get() != null)
        {
            StoreManager.Get().UnloadAndFreeMemory();
        }
        if (severity > 15)
        {
            if (SpellCache.Get() != null)
            {
                SpellCache.Get().Clear();
            }
            AssetCache.ClearAllCaches(true, false);
        }
        ApplicationMgr.Get().UnloadUnusedAssets();
    }

    private void DestroyAllObjectsOnModeSwitch()
    {
        GameObject[] objArray = (GameObject[]) UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject obj2 in objArray)
        {
            if (this.ShouldDestroyOnModeSwitch(obj2))
            {
                UnityEngine.Object.DestroyImmediate(obj2);
            }
        }
    }

    private bool DoesModeShowBox(Mode mode)
    {
        Mode mode2 = mode;
        switch (mode2)
        {
            case Mode.STARTUP:
            case Mode.GAMEPLAY:
                break;

            default:
                switch (mode2)
                {
                    case Mode.FATAL_ERROR:
                    case Mode.RESET:
                        break;

                    case Mode.DRAFT:
                    case Mode.CREDITS:
                        goto Label_003A;

                    default:
                        goto Label_003A;
                }
                break;
        }
        return false;
    Label_003A:
        return true;
    }

    public void FatalMobileError(string msg)
    {
        Error.AddFatal(msg);
    }

    private void FireSceneLoadedEvent()
    {
        this.m_transitioning = false;
        foreach (SceneLoadedListener listener in this.m_sceneLoadedListeners.ToArray())
        {
            listener.Fire(this.m_mode, this.m_scene);
        }
    }

    private void FireScenePreLoadEvent()
    {
        foreach (ScenePreLoadListener listener in this.m_scenePreLoadListeners.ToArray())
        {
            listener.Fire(this.m_prevMode, this.m_mode);
        }
    }

    private void FireScenePreUnloadEvent(Scene prevScene)
    {
        foreach (ScenePreUnloadListener listener in this.m_scenePreUnloadListeners.ToArray())
        {
            listener.Fire(this.m_prevMode, prevScene);
        }
    }

    private void FireSceneUnloadedEvent(Scene prevScene)
    {
        foreach (SceneUnloadedListener listener in this.m_sceneUnloadedListeners.ToArray())
        {
            listener.Fire(this.m_prevMode, prevScene);
        }
    }

    public static SceneMgr Get()
    {
        return s_instance;
    }

    public Mode GetMode()
    {
        return this.m_mode;
    }

    public Mode GetNextMode()
    {
        return this.m_nextMode;
    }

    public Mode GetPrevMode()
    {
        return this.m_prevMode;
    }

    public Scene GetScene()
    {
        return this.m_scene;
    }

    public bool IsInGame()
    {
        return this.IsModeRequested(Mode.GAMEPLAY);
    }

    public bool IsModeRequested(Mode mode)
    {
        return ((this.m_mode == mode) || (this.m_nextMode == mode));
    }

    public bool IsSceneLoaded()
    {
        return this.m_sceneLoaded;
    }

    public bool IsTransitioning()
    {
        return this.m_transitioning;
    }

    [DebuggerHidden]
    private IEnumerator LoadAssetsWhenReady()
    {
        return new <LoadAssetsWhenReady>c__Iterator1B9 { <>f__this = this };
    }

    private void LoadBox(AssetLoader.GameObjectCallback callback)
    {
        this.m_boxLoadTimestamp = TimeUtils.BinaryStamp();
        AssetLoader.Get().LoadUIScreen("TheBox", callback, null, false);
    }

    private void LoadMode()
    {
        this.FireScenePreLoadEvent();
        Application.LoadLevelAdditiveAsync(EnumUtils.GetString<Mode>(this.m_mode));
    }

    private void LoadModeFromModeSwitch()
    {
        bool flag = this.DoesModeShowBox(this.m_prevMode);
        bool flag2 = this.DoesModeShowBox(this.m_mode);
        if (!flag && flag2)
        {
            this.LoadStringsWhenPossible(GameStringCategory.GLUE);
            if (this.m_prevMode == Mode.GAMEPLAY)
            {
                this.UnloadStringsWhenPossible(GameStringCategory.GAMEPLAY);
                this.UnloadStringsWhenPossible(GameStringCategory.MISSION);
            }
            this.LoadBox(new AssetLoader.GameObjectCallback(this.OnBoxReloaded));
        }
        else
        {
            if (flag && !flag2)
            {
                LoadingScreen.Get().SetAssetLoadStartTimestamp(this.m_boxLoadTimestamp);
                this.m_boxLoadTimestamp = 0L;
                this.UnloadStringsWhenPossible(GameStringCategory.GLUE);
            }
            if ((this.m_prevMode != Mode.GAMEPLAY) && (this.m_mode == Mode.GAMEPLAY))
            {
                this.LoadStringsWhenPossible(GameStringCategory.GAMEPLAY);
                this.LoadStringsWhenPossible(GameStringCategory.MISSION);
            }
            this.LoadMode();
        }
    }

    public void LoadShaderPreCompiler()
    {
    }

    private void LoadStartupAssets()
    {
        this.m_startupAssetLoads = 6;
        if (SoundManager.Get().GetConfig() == null)
        {
            AssetLoader.Get().LoadGameObject("SoundConfig", new AssetLoader.GameObjectCallback(this.OnSoundConfigLoaded), null, false);
        }
        else
        {
            this.m_startupAssetLoads--;
        }
        if (MusicConfig.Get() == null)
        {
            AssetLoader.Get().LoadGameObject("MusicConfig", new AssetLoader.GameObjectCallback(this.OnStartupAssetLoaded<MusicConfig>), null, false);
        }
        else
        {
            this.m_startupAssetLoads--;
        }
        if (AdventureConfig.Get() == null)
        {
            AssetLoader.Get().LoadGameObject("AdventureConfig", new AssetLoader.GameObjectCallback(this.OnStartupAssetLoaded<AdventureConfig>), null, false);
        }
        else
        {
            this.m_startupAssetLoads--;
        }
        if (CardColorSwitcher.Get() == null)
        {
            AssetLoader.Get().LoadGameObject("CardColorSwitcher", new AssetLoader.GameObjectCallback(this.OnColorSwitcherLoaded), null, false);
        }
        else
        {
            this.m_startupAssetLoads--;
        }
        if (SpecialEventVisualMgr.Get() == null)
        {
            AssetLoader.Get().LoadGameObject("SpecialEventVisualMgr", new AssetLoader.GameObjectCallback(this.OnSpecialEventVisualMgrLoaded), null, false);
        }
        else
        {
            this.m_startupAssetLoads--;
        }
        if (!this.m_textInputGUISkinLoaded)
        {
            AssetLoader.Get().LoadGameObject("TextInputGUISkin", new AssetLoader.GameObjectCallback(this.OnTextInputGUISkinLoaded), null, false);
        }
        else
        {
            this.m_startupAssetLoads--;
        }
        if (this.m_startupAssetLoads == 0)
        {
            this.OnStartupAssetFinishedLoading();
        }
    }

    private void LoadStartupStrings()
    {
        GameStrings.LoadCategory(GameStringCategory.GLUE);
        GameStrings.LoadCategory(GameStringCategory.TUTORIAL);
        GameStrings.LoadCategory(GameStringCategory.PRESENCE);
    }

    private void LoadStringsWhenPossible(GameStringCategory category)
    {
        if (LoadingScreen.Get().IsTransitioning() && LoadingScreen.Get().IsUnloadingStrings(category))
        {
            LoadingScreen.Get().RemoveStringsToUnload(category);
        }
        else
        {
            GameStrings.LoadCategory(category);
        }
    }

    public void LowMemoryWarning(string msg)
    {
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        float num2 = realtimeSinceStartup - this.m_lastMemoryWarning;
        int result = 0;
        if (!int.TryParse(msg, out result) && ((num2 <= 2f) || (num2 > 120f)))
        {
            result = 10;
            if (num2 < 0.5f)
            {
                result = 15;
            }
        }
        UnityEngine.Debug.Log(string.Concat(new object[] { "receiving low memory warning ", result, " ", num2 }));
        if (result > 5)
        {
            this.ClearCachesAndFreeMemory(result);
            this.m_lastMemoryWarning = realtimeSinceStartup;
        }
    }

    public void NotifySceneLoaded()
    {
        this.m_sceneLoaded = true;
        if (this.ShouldUseSceneLoadDelays())
        {
            base.StartCoroutine(this.WaitThenFireSceneLoadedEvent());
        }
        else
        {
            this.FireSceneLoadedEvent();
        }
    }

    private void OnBaseUILoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnBaseUILoaded() - FAILED to load \"{0}\"", name));
        }
    }

    private void OnBoxLoaded(string name, GameObject screen, object callbackData)
    {
        if (screen == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnBoxLoaded() - failed to load {0}", name));
        }
        else if (!this.IsModeRequested(Mode.FATAL_ERROR))
        {
            this.m_nextMode = Mode.LOGIN;
        }
    }

    private void OnBoxReloaded(string name, GameObject screen, object callbackData)
    {
        if (screen == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnBoxReloaded() - failed to load {0}", name));
        }
        else
        {
            this.LoadMode();
        }
    }

    private void OnColorSwitcherLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnColorSwitcherLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            go.transform.parent = base.transform;
            this.OnStartupAssetFinishedLoading();
        }
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        if (!ApplicationMgr.Get().ResetOnErrorIfNecessary())
        {
            this.SetNextMode(Mode.FATAL_ERROR);
        }
    }

    private void OnFontTableInitialized(object userData)
    {
        if (OverlayUI.Get() == null)
        {
            AssetLoader.Get().LoadUIScreen("OverlayUI", new AssetLoader.GameObjectCallback(this.OnOverlayUILoaded), null, false);
        }
        AssetLoader.Get().LoadGameObject("SplashScreen", new AssetLoader.GameObjectCallback(this.OnSplashScreenLoaded), null, false);
    }

    private void OnOverlayUILoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnOverlayUILoaded() - FAILED to load \"{0}\"", name));
        }
    }

    private void OnSoundConfigLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnSoundConfigLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            SoundConfig component = go.GetComponent<SoundConfig>();
            if (component == null)
            {
                UnityEngine.Debug.LogError(string.Format("SceneMgr.OnSoundConfigLoaded() - ERROR \"{0}\" has no {1} component", name, typeof(SoundConfig)));
            }
            else
            {
                go.transform.parent = base.transform;
                SoundManager.Get().SetConfig(component);
                this.OnStartupAssetFinishedLoading();
            }
        }
    }

    private void OnSpecialEventVisualMgrLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnSpecialEventMgrLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            go.transform.parent = base.transform;
            this.OnStartupAssetFinishedLoading();
        }
    }

    private void OnSplashScreenFinished()
    {
        this.LoadStartupAssets();
        this.m_StartupCamera.SetActive(false);
    }

    private void OnSplashScreenLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnSplashScreenLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            go.GetComponent<SplashScreen>().AddFinishedListener(new SplashScreen.FinishedHandler(this.OnSplashScreenFinished));
            if (BaseUI.Get() == null)
            {
                AssetLoader.Get().LoadUIScreen("BaseUI", new AssetLoader.GameObjectCallback(this.OnBaseUILoaded), null, false);
            }
        }
    }

    private void OnStartupAssetFinishedLoading()
    {
        if (!this.IsModeRequested(Mode.FATAL_ERROR))
        {
            this.m_startupAssetLoads--;
            if (this.m_startupAssetLoads <= 0)
            {
                this.LoadBox(new AssetLoader.GameObjectCallback(this.OnBoxLoaded));
            }
        }
    }

    private void OnStartupAssetLoaded<T>(string name, GameObject go, object callbackData) where T: UnityEngine.Component
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnStartupAssetLoaded<{0}>() - FAILED to load \"{1}\"", typeof(T).Name, name));
        }
        else if (go.GetComponent<T>() == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnStartupAssetLoaded<{0}>() - ERROR \"{1}\" has no {2} component", typeof(T).Name, name, typeof(T)));
        }
        else
        {
            go.transform.parent = base.transform;
            this.OnStartupAssetFinishedLoading();
        }
    }

    private void OnTextInputGUISkinLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SceneMgr.OnTextGUISkinLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            this.m_textInputGUISkinLoaded = true;
            GUISkinContainer component = go.GetComponent<GUISkinContainer>();
            UniversalInputManager.Get().SetGUISkin(component);
            this.OnStartupAssetFinishedLoading();
        }
    }

    private void PostUnloadCleanup()
    {
        UnityEngine.Time.captureFramerate = 0;
        if (this.m_performFullCleanup)
        {
            AssetCache.ClearAllCaches(false, true);
        }
        this.DestroyAllObjectsOnModeSwitch();
        if (this.m_performFullCleanup)
        {
            ApplicationMgr.Get().UnloadUnusedAssets();
        }
    }

    public void RegisterSceneLoadedEvent(SceneLoadedCallback callback)
    {
        this.RegisterSceneLoadedEvent(callback, null);
    }

    public void RegisterSceneLoadedEvent(SceneLoadedCallback callback, object userData)
    {
        SceneLoadedListener item = new SceneLoadedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_sceneLoadedListeners.Contains(item))
        {
            this.m_sceneLoadedListeners.Add(item);
        }
    }

    public void RegisterScenePreLoadEvent(ScenePreLoadCallback callback)
    {
        this.RegisterScenePreLoadEvent(callback, null);
    }

    public void RegisterScenePreLoadEvent(ScenePreLoadCallback callback, object userData)
    {
        ScenePreLoadListener item = new ScenePreLoadListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_scenePreLoadListeners.Contains(item))
        {
            this.m_scenePreLoadListeners.Add(item);
        }
    }

    public void RegisterScenePreUnloadEvent(ScenePreUnloadCallback callback)
    {
        this.RegisterScenePreUnloadEvent(callback, null);
    }

    public void RegisterScenePreUnloadEvent(ScenePreUnloadCallback callback, object userData)
    {
        ScenePreUnloadListener item = new ScenePreUnloadListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_scenePreUnloadListeners.Contains(item))
        {
            this.m_scenePreUnloadListeners.Add(item);
        }
    }

    public void RegisterSceneUnloadedEvent(SceneUnloadedCallback callback)
    {
        this.RegisterSceneUnloadedEvent(callback, null);
    }

    public void RegisterSceneUnloadedEvent(SceneUnloadedCallback callback, object userData)
    {
        SceneUnloadedListener item = new SceneUnloadedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_sceneUnloadedListeners.Contains(item))
        {
            this.m_sceneUnloadedListeners.Add(item);
        }
    }

    public void ReloadMode()
    {
        if (!this.IsModeRequested(Mode.FATAL_ERROR))
        {
            this.m_nextMode = this.m_mode;
            this.m_reloadMode = true;
        }
    }

    public void SetNextMode(Mode mode)
    {
        if (!this.IsModeRequested(Mode.FATAL_ERROR))
        {
            this.CacheModeForResume(mode);
            this.m_nextMode = mode;
            this.m_reloadMode = false;
        }
    }

    public void SetScene(Scene scene)
    {
        this.m_scene = scene;
    }

    private bool ShouldDestroyOnModeSwitch(GameObject go)
    {
        if (go == null)
        {
            return false;
        }
        if (go.transform.parent != null)
        {
            return false;
        }
        if (go == base.gameObject)
        {
            return false;
        }
        if ((PegUI.Get() != null) && (go == PegUI.Get().gameObject))
        {
            return false;
        }
        if ((OverlayUI.Get() != null) && (go == OverlayUI.Get().gameObject))
        {
            return false;
        }
        if (((Box.Get() != null) && (go == Box.Get().gameObject)) && this.DoesModeShowBox(this.m_mode))
        {
            return false;
        }
        if (DefLoader.Get().HasDef(go))
        {
            return false;
        }
        if (AssetLoader.Get().IsWaitingOnObject(go))
        {
            return false;
        }
        if (go == iTweenManager.Get().gameObject)
        {
            return false;
        }
        return true;
    }

    private bool ShouldUseSceneLoadDelays()
    {
        if (this.m_mode == Mode.LOGIN)
        {
            return false;
        }
        if (this.m_mode == Mode.HUB)
        {
            return false;
        }
        if (this.m_mode == Mode.FATAL_ERROR)
        {
            return false;
        }
        return true;
    }

    private bool ShouldUseSceneUnloadDelays()
    {
        if (this.m_prevMode == this.m_mode)
        {
            return false;
        }
        return true;
    }

    private void Start()
    {
        if (!this.IsModeRequested(Mode.FATAL_ERROR))
        {
            base.StartCoroutine("LoadAssetsWhenReady");
        }
    }

    [DebuggerHidden]
    private IEnumerator SwitchMode()
    {
        return new <SwitchMode>c__Iterator1BC { <>f__this = this };
    }

    private void UnloadStringsWhenPossible(GameStringCategory category)
    {
        if (LoadingScreen.Get().IsTransitioning())
        {
            LoadingScreen.Get().AddStringsToUnload(category);
        }
        else
        {
            GameStrings.UnloadCategory(category);
        }
    }

    public bool UnregisterSceneLoadedEvent(SceneLoadedCallback callback)
    {
        return this.UnregisterSceneLoadedEvent(callback, null);
    }

    public bool UnregisterSceneLoadedEvent(SceneLoadedCallback callback, object userData)
    {
        SceneLoadedListener item = new SceneLoadedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_sceneLoadedListeners.Remove(item);
    }

    public bool UnregisterScenePreLoadEvent(ScenePreLoadCallback callback)
    {
        return this.UnregisterScenePreLoadEvent(callback, null);
    }

    public bool UnregisterScenePreLoadEvent(ScenePreLoadCallback callback, object userData)
    {
        ScenePreLoadListener item = new ScenePreLoadListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_scenePreLoadListeners.Remove(item);
    }

    public bool UnregisterScenePreUnloadEvent(ScenePreUnloadCallback callback)
    {
        return this.UnregisterScenePreUnloadEvent(callback, null);
    }

    public bool UnregisterScenePreUnloadEvent(ScenePreUnloadCallback callback, object userData)
    {
        ScenePreUnloadListener item = new ScenePreUnloadListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_scenePreUnloadListeners.Remove(item);
    }

    public bool UnregisterSceneUnloadedEvent(SceneUnloadedCallback callback)
    {
        return this.UnregisterSceneUnloadedEvent(callback, null);
    }

    public bool UnregisterSceneUnloadedEvent(SceneUnloadedCallback callback, object userData)
    {
        SceneUnloadedListener item = new SceneUnloadedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_sceneUnloadedListeners.Remove(item);
    }

    private void Update()
    {
        if (!this.m_reloadMode)
        {
            if (this.m_nextMode == Mode.INVALID)
            {
                return;
            }
            if (this.m_mode == this.m_nextMode)
            {
                this.m_nextMode = Mode.INVALID;
                return;
            }
        }
        this.m_transitioning = true;
        this.m_performFullCleanup = !this.m_reloadMode;
        this.m_prevMode = this.m_mode;
        this.m_mode = this.m_nextMode;
        this.m_nextMode = Mode.INVALID;
        this.m_reloadMode = false;
        if (this.m_scene != null)
        {
            base.StopCoroutine("SwitchMode");
            base.StartCoroutine("SwitchMode");
        }
        else
        {
            this.LoadMode();
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenFireSceneLoadedEvent()
    {
        return new <WaitThenFireSceneLoadedEvent>c__Iterator1BB { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenLoadAssets()
    {
        return new <WaitThenLoadAssets>c__Iterator1BA { <>f__this = this };
    }

    private void WillReset()
    {
        Log.Reset.Print("SceneMgr.WillReset()", new object[0]);
        if (ApplicationMgr.IsPublic())
        {
            UnityEngine.Time.timeScale = 1f;
        }
        base.StopAllCoroutines();
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        this.m_mode = Mode.STARTUP;
        this.m_nextMode = Mode.INVALID;
        this.m_prevMode = Mode.INVALID;
        this.m_reloadMode = false;
        Scene prevScene = this.m_scene;
        if (prevScene != null)
        {
            prevScene.PreUnload();
        }
        this.FireScenePreUnloadEvent(prevScene);
        if (this.m_scene != null)
        {
            this.m_scene.Unload();
            this.m_scene = null;
            this.m_sceneLoaded = false;
        }
        this.FireSceneUnloadedEvent(prevScene);
        this.PostUnloadCleanup();
        base.StartCoroutine("WaitThenLoadAssets");
        Log.Reset.Print("\tSceneMgr.WillReset() completed", new object[0]);
    }

    public bool WillTransition()
    {
        if (this.m_reloadMode)
        {
            return true;
        }
        if (this.m_nextMode == Mode.INVALID)
        {
            return false;
        }
        return (this.m_nextMode != this.m_mode);
    }

    [CompilerGenerated]
    private sealed class <LoadAssetsWhenReady>c__Iterator1B9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SceneMgr <>f__this;

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
                    if (!AssetLoader.Get().IsReady())
                    {
                        this.$current = new WaitForSeconds(0.1f);
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.LoadStartupStrings();
                    FontTable.Get().AddInitializedCallback(new FontTable.InitializedCallback(this.<>f__this.OnFontTableInitialized));
                    FontTable.Get().Initialize();
                    UberText.LoadCachedData();
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
    private sealed class <SwitchMode>c__Iterator1BC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SceneMgr <>f__this;
        internal Camera <freezeFrameCamera>__1;
        internal Scene <prevScene>__0;

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
                    if (!this.<>f__this.m_scene.IsUnloading())
                    {
                        this.<prevScene>__0 = this.<>f__this.m_scene;
                        this.<prevScene>__0.PreUnload();
                        this.<>f__this.FireScenePreUnloadEvent(this.<prevScene>__0);
                        if (LoadingScreen.Get().GetPhase() == LoadingScreen.Phase.WAITING_FOR_SCENE_UNLOAD)
                        {
                            this.<freezeFrameCamera>__1 = LoadingScreen.Get().GetFreezeFrameCamera();
                            if (this.<freezeFrameCamera>__1 != null)
                            {
                                this.$current = new WaitForEndOfFrame();
                                this.$PC = 1;
                                goto Label_017D;
                            }
                        }
                        break;
                    }
                    goto Label_017B;

                case 1:
                    break;

                case 2:
                    goto Label_00F5;

                case 3:
                    goto Label_0125;

                default:
                    goto Label_017B;
            }
            if (!this.<>f__this.ShouldUseSceneUnloadDelays())
            {
                goto Label_0125;
            }
            if (Box.Get() == null)
            {
                this.$current = new WaitForSeconds(0.15f);
                this.$PC = 3;
                goto Label_017D;
            }
        Label_00F5:
            while (Box.Get().HasPendingEffects())
            {
                this.$current = 0;
                this.$PC = 2;
                goto Label_017D;
            }
        Label_0125:
            this.<>f__this.m_scene.Unload();
            this.<>f__this.m_scene = null;
            this.<>f__this.m_sceneLoaded = false;
            this.<>f__this.FireSceneUnloadedEvent(this.<prevScene>__0);
            this.<>f__this.PostUnloadCleanup();
            this.<>f__this.LoadModeFromModeSwitch();
            this.$PC = -1;
        Label_017B:
            return false;
        Label_017D:
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
    private sealed class <WaitThenFireSceneLoadedEvent>c__Iterator1BB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SceneMgr <>f__this;

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
                    this.$current = new WaitForSeconds(0.15f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.FireSceneLoadedEvent();
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
    private sealed class <WaitThenLoadAssets>c__Iterator1BA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SceneMgr <>f__this;

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
                    this.$current = 0;
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.StartCoroutine("LoadAssetsWhenReady");
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

    public enum Mode
    {
        [Description("Adventure")]
        ADVENTURE = 13,
        [Description("CollectionManager")]
        COLLECTIONMANAGER = 5,
        [Description("Credits")]
        CREDITS = 11,
        [Description("Draft")]
        DRAFT = 10,
        [Description("FatalError")]
        FATAL_ERROR = 9,
        [Description("Friendly")]
        FRIENDLY = 8,
        [Description("Gameplay")]
        GAMEPLAY = 4,
        [Description("Hub")]
        HUB = 3,
        INVALID = 0,
        [Description("Login")]
        LOGIN = 2,
        [Description("PackOpening")]
        PACKOPENING = 6,
        [Description("Reset")]
        RESET = 12,
        STARTUP = 1,
        [Description("TavernBrawl")]
        TAVERN_BRAWL = 14,
        [Description("Tournament")]
        TOURNAMENT = 7
    }

    public delegate void SceneLoadedCallback(SceneMgr.Mode mode, Scene scene, object userData);

    private class SceneLoadedListener : EventListener<SceneMgr.SceneLoadedCallback>
    {
        public void Fire(SceneMgr.Mode mode, Scene scene)
        {
            base.m_callback(mode, scene, base.m_userData);
        }
    }

    public delegate void ScenePreLoadCallback(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData);

    private class ScenePreLoadListener : EventListener<SceneMgr.ScenePreLoadCallback>
    {
        public void Fire(SceneMgr.Mode prevMode, SceneMgr.Mode mode)
        {
            base.m_callback(prevMode, mode, base.m_userData);
        }
    }

    public delegate void ScenePreUnloadCallback(SceneMgr.Mode prevMode, Scene prevScene, object userData);

    private class ScenePreUnloadListener : EventListener<SceneMgr.ScenePreUnloadCallback>
    {
        public void Fire(SceneMgr.Mode prevMode, Scene prevScene)
        {
            base.m_callback(prevMode, prevScene, base.m_userData);
        }
    }

    public delegate void SceneUnloadedCallback(SceneMgr.Mode prevMode, Scene prevScene, object userData);

    private class SceneUnloadedListener : EventListener<SceneMgr.SceneUnloadedCallback>
    {
        public void Fire(SceneMgr.Mode prevMode, Scene prevScene)
        {
            base.m_callback(prevMode, prevScene, base.m_userData);
        }
    }
}

