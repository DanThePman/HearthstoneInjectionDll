#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#endregion 

#region " Referenced assemblies "
// - mscorlib v2.0.5.0
// - Assembly-CSharp-firstpass v0.0.0.0
// - System.Core v2.0.5.0
// - UnityEngine v0.0.0.0
// - PlayMaker v1.6.0.0
// - System v2.0.5.0
// - ICSharpCode.SharpZipLib v0.86.0.518
// - IFacebook v0.0.0.0
// - UnityEngine.Analytics v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - HearthstoneInjectionDll v0.0.0.0
// - Assembly-CSharp v0.0.0.0
// - HearthstoneInjectionDll v1.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class SceneMgr : MonoBehaviour
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void Awake()
    {
        s_instance = this;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        this.m_transitioning = true;
        ApplicationMgr.Get().WillReset += new Action(this.WillReset);
        HearthstoneInjectionDll.Core.OnGameInit();
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    SceneMgr()
    {
    }

    void OnDestroy()
    {
    }

    void Start()
    {
    }

    System.Collections.IEnumerator LoadAssetsWhenReady()
    {
        return default(System.Collections.IEnumerator);
    }

    void LoadShaderPreCompiler()
    {
    }

    void Update()
    {
    }

    static SceneMgr Get()
    {
        return default(SceneMgr);
    }

    void WillReset()
    {
    }

    System.Collections.IEnumerator WaitThenLoadAssets()
    {
        return default(System.Collections.IEnumerator);
    }

    void ReloadMode()
    {
    }

    Scene GetScene()
    {
        return default(Scene);
    }

    void SetScene(Scene scene)
    {
    }

    bool IsSceneLoaded()
    {
        return default(bool);
    }

    bool WillTransition()
    {
        return default(bool);
    }

    bool IsTransitioning()
    {
        return default(bool);
    }

    bool IsTransitionNowOrPending()
    {
        return default(bool);
    }

    bool IsInGame()
    {
        return default(bool);
    }

    void NotifySceneLoaded()
    {
    }


    System.Collections.IEnumerator WaitThenFireSceneLoadedEvent()
    {
        return default(System.Collections.IEnumerator);
    }

    void FireScenePreUnloadEvent(Scene prevScene)
    {
    }

    void FireSceneUnloadedEvent(Scene prevScene)
    {
    }

    void FireScenePreLoadEvent()
    {
    }

    void FireSceneLoadedEvent()
    {
    }

    void OnFontTableInitialized(object userData)
    {
    }

    void OnBaseUILoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnOverlayUILoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnSplashScreenLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnSplashScreenFinished()
    {
    }

    void LoadStartupAssets()
    {
    }

    void OnColorSwitcherLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnSpecialEventVisualMgrLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnSoundConfigLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnStartupAssetLoaded<T>(string name, UnityEngine.GameObject go, object callbackData) where T : UnityEngine.Component
    {
    }

    void OnTextInputGUISkinLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnStartupAssetFinishedLoading()
    {
    }

    void OnBoxLoaded(string name, UnityEngine.GameObject screen, object callbackData)
    {
    }

    void LoadMode()
    {
    }

    System.Collections.IEnumerator SwitchMode()
    {
        return default(System.Collections.IEnumerator);
    }

    bool ShouldUseSceneUnloadDelays()
    {
        return default(bool);
    }

    bool ShouldUseSceneLoadDelays()
    {
        return default(bool);
    }

    void PostUnloadCleanup()
    {
    }

    void DestroyAllObjectsOnModeSwitch()
    {
    }

    bool ShouldDestroyOnModeSwitch(UnityEngine.GameObject go)
    {
        return default(bool);
    }

    void LoadModeFromModeSwitch()
    {
    }

    void OnBoxReloaded(string name, UnityEngine.GameObject screen, object callbackData)
    {
    }

    void LoadBox(AssetLoader.GameObjectCallback callback)
    {
    }

    void OnFatalError(FatalErrorMessage message, object userData)
    {
    }

    void ClearCachesAndFreeMemory(int severity)
    {
    }

    void LowMemoryWarning(string msg)
    {
    }

    void FatalMobileError(string msg)
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static float SCENE_UNLOAD_DELAY;
    static float SCENE_LOADED_DELAY;
    static int TRIM_MEMORY_RUNNING_CRITICAL;
    static int TRIM_MEMORY_RUNNING_LOW;
    static int TRIM_MEMORY_RUNNING_MODERATE;
    UnityEngine.GameObject m_StartupCamera;
    static SceneMgr s_instance;
    int m_startupAssetLoads;
    bool m_reloadMode;
    Scene m_scene;
    bool m_sceneLoaded;
    bool m_transitioning;
    bool m_performFullCleanup;

    long m_boxLoadTimestamp;
    bool m_textInputGUISkinLoaded;
    float m_lastMemoryWarning;
    #endregion

}
