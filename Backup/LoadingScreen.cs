using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    private long m_assetLoadEndTimestamp;
    private long m_assetLoadNextStartTimestamp;
    private long m_assetLoadStartTimestamp;
    public iTween.EaseType m_FadeInEaseType = iTween.EaseType.linear;
    public float m_FadeInSec = 1f;
    public iTween.EaseType m_FadeOutEaseType = iTween.EaseType.linear;
    public float m_FadeOutSec = 1f;
    private List<FinishedTransitionListener> m_finishedTransitionListeners = new List<FinishedTransitionListener>();
    private Camera m_fxCamera;
    private float m_originalPosX;
    private Phase m_phase;
    private bool m_previousSceneActive;
    private List<PreviousSceneDestroyedListener> m_prevSceneDestroyedListeners = new List<PreviousSceneDestroyedListener>();
    private TransitionParams m_prevTransitionParams;
    private List<GameStringCategory> m_stringCategoriesToUnload = new List<GameStringCategory>();
    private TransitionParams m_transitionParams = new TransitionParams();
    private TransitionUnfriendlyData m_transitionUnfriendlyData = new TransitionUnfriendlyData();
    private const float MIDDLE_OF_NOWHERE_X = 5000f;
    private static LoadingScreen s_instance;

    public void AddStringsToUnload(GameStringCategory category)
    {
        this.m_stringCategoriesToUnload.Add(category);
    }

    public void AddTransitionBlocker()
    {
        this.m_transitionParams.AddBlocker();
    }

    public void AddTransitionBlocker(int count)
    {
        this.m_transitionParams.AddBlocker(count);
    }

    public void AddTransitionObject(Component c)
    {
        this.m_transitionParams.AddObject(c.gameObject);
    }

    public void AddTransitionObject(GameObject go)
    {
        this.m_transitionParams.AddObject(go);
    }

    private void Awake()
    {
        s_instance = this;
        this.InitializeFxCamera();
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
    }

    private void ClearAssets(long startTimestamp, long endTimestamp)
    {
        object[] args = new object[] { startTimestamp, endTimestamp, endTimestamp - startTimestamp };
        Log.LoadingScreen.Print("LoadingScreen.ClearAssets() - START startTimestamp={0} endTimestamp={1} diff={2}", args);
        AssetCache.ClearAllCachesBetween(startTimestamp, endTimestamp);
        ApplicationMgr.Get().UnloadUnusedAssets();
    }

    private void ClearPreviousSceneAssets()
    {
        foreach (GameStringCategory category in this.m_stringCategoriesToUnload)
        {
            GameStrings.UnloadCategory(category);
        }
        this.m_stringCategoriesToUnload.Clear();
        object[] args = new object[] { this.m_assetLoadStartTimestamp, this.m_assetLoadEndTimestamp };
        Log.LoadingScreen.Print("LoadingScreen.ClearPreviousSceneAssets() - START m_assetLoadStartTimestamp={0} m_assetLoadEndTimestamp={1}", args);
        this.ClearAssets(this.m_assetLoadStartTimestamp, this.m_assetLoadEndTimestamp);
        this.m_assetLoadStartTimestamp = this.m_assetLoadNextStartTimestamp;
        this.m_assetLoadEndTimestamp = 0L;
        this.m_assetLoadNextStartTimestamp = 0L;
        object[] objArray2 = new object[] { this.m_assetLoadStartTimestamp, this.m_assetLoadEndTimestamp };
        Log.LoadingScreen.Print("LoadingScreen.ClearPreviousSceneAssets() - END m_assetLoadStartTimestamp={0} m_assetLoadEndTimestamp={1}", objArray2);
    }

    private void CutoffTransition()
    {
        if (!this.IsTransitioning())
        {
            this.m_transitionParams = new TransitionParams();
        }
        else
        {
            this.StopFading();
            this.FinishPreviousScene();
            this.FinishFxCamera();
            this.m_prevTransitionParams = null;
            this.m_transitionParams = new TransitionParams();
            this.m_phase = Phase.INVALID;
            this.FireFinishedTransitionListeners(true);
        }
    }

    private void DisableTransitionUnfriendlyStuff(GameObject mainObject)
    {
        object[] args = new object[] { mainObject };
        Log.LoadingScreen.Print("LoadingScreen.DisableTransitionUnfriendlyStuff() - {0}", args);
        AudioListener[] componentsInChildren = base.GetComponentsInChildren<AudioListener>();
        bool flag = false;
        foreach (AudioListener listener in componentsInChildren)
        {
            flag |= (listener != null) && listener.enabled;
        }
        if (flag)
        {
            AudioListener componentInChildren = mainObject.GetComponentInChildren<AudioListener>();
            this.m_transitionUnfriendlyData.SetAudioListener(componentInChildren);
        }
        Light[] lights = mainObject.GetComponentsInChildren<Light>();
        this.m_transitionUnfriendlyData.AddLights(lights);
    }

    public static bool DoesShowLoadingScreen(SceneMgr.Mode prevMode, SceneMgr.Mode nextMode)
    {
        return ((prevMode == SceneMgr.Mode.GAMEPLAY) || (nextMode == SceneMgr.Mode.GAMEPLAY));
    }

    private void DoInterruptionCleanUp()
    {
        bool flag = this.IsPreviousSceneActive();
        object[] args = new object[] { this.m_phase, flag };
        Log.LoadingScreen.Print("LoadingScreen.DoInterruptionCleanUp() - m_phase={0} previousSceneActive={1}", args);
        if (this.m_phase == Phase.WAITING_FOR_BLOCKERS)
        {
            base.StopCoroutine("HackWaitThenStartTransitionEffects");
        }
        if (this.IsFading())
        {
            this.StopFading();
            if (this.IsFadingIn())
            {
                this.m_prevTransitionParams = null;
            }
        }
        if (flag)
        {
            long assetLoadNextStartTimestamp = this.m_assetLoadNextStartTimestamp;
            long endTimestamp = TimeUtils.BinaryStamp();
            this.ClearAssets(assetLoadNextStartTimestamp, endTimestamp);
            this.m_transitionUnfriendlyData.Clear();
            this.m_transitionParams = new TransitionParams();
            this.m_phase = Phase.WAITING_FOR_SCENE_LOAD;
        }
    }

    public void EnableFadeIn(bool enable)
    {
        this.m_transitionParams.EnableFadeIn(enable);
    }

    public void EnableFadeOut(bool enable)
    {
        this.m_transitionParams.EnableFadeOut(enable);
    }

    public void EnableTransition(bool enable)
    {
        this.m_transitionParams.Enable(enable);
    }

    private void FadeIn()
    {
        <FadeIn>c__AnonStorey37B storeyb = new <FadeIn>c__AnonStorey37B();
        Log.LoadingScreen.Print("LoadingScreen.FadeIn()", new object[0]);
        this.m_phase = Phase.FADING_IN;
        if (!this.m_prevTransitionParams.IsFadeInEnabled())
        {
            this.OnFadeInComplete();
        }
        else
        {
            storeyb.cameraFade = base.GetComponent<CameraFade>();
            if (storeyb.cameraFade == null)
            {
                UnityEngine.Debug.LogError("LoadingScreen FadeIn(): Failed to find CameraFade component");
            }
            else
            {
                storeyb.cameraFade.m_Color = this.m_prevTransitionParams.GetFadeColor();
                Action<object> action = new Action<object>(storeyb.<>m__264);
                action(1f);
                object[] args = new object[] { "time", this.m_FadeInSec, "from", 1f, "to", 0f, "onupdate", action, "onupdatetarget", base.gameObject, "oncomplete", "OnFadeInComplete", "oncompletetarget", base.gameObject, "name", "Fade" };
                Hashtable hashtable = iTween.Hash(args);
                iTween.ValueTo(base.gameObject, hashtable);
            }
        }
    }

    private void FadeOut()
    {
        <FadeOut>c__AnonStorey37A storeya = new <FadeOut>c__AnonStorey37A();
        Log.LoadingScreen.Print("LoadingScreen.FadeOut()", new object[0]);
        this.m_phase = Phase.FADING_OUT;
        if (!this.m_prevTransitionParams.IsFadeOutEnabled())
        {
            this.OnFadeOutComplete();
        }
        else
        {
            storeya.cameraFade = base.GetComponent<CameraFade>();
            if (storeya.cameraFade == null)
            {
                UnityEngine.Debug.LogError("LoadingScreen FadeOut(): Failed to find CameraFade component");
            }
            else
            {
                storeya.cameraFade.m_Color = this.m_prevTransitionParams.GetFadeColor();
                Action<object> action = new Action<object>(storeya.<>m__263);
                object[] args = new object[] { "time", this.m_FadeOutSec, "from", storeya.cameraFade.m_Fade, "to", 1f, "onupdate", action, "onupdatetarget", base.gameObject, "oncomplete", "OnFadeOutComplete", "oncompletetarget", base.gameObject, "name", "Fade" };
                Hashtable hashtable = iTween.Hash(args);
                iTween.ValueTo(base.gameObject, hashtable);
            }
        }
    }

    private void FinishFxCamera()
    {
        <FinishFxCamera>c__AnonStorey37C storeyc = new <FinishFxCamera>c__AnonStorey37C {
            cameraFade = base.GetComponent<CameraFade>()
        };
        if (storeyc.cameraFade == null)
        {
            UnityEngine.Debug.LogError("LoadingScreen.FinishFxCamera(): Failed to find CameraFade component");
        }
        else if (storeyc.cameraFade.m_Fade > 0f)
        {
            Action<object> action = new Action<object>(storeyc.<>m__265);
            object[] args = new object[] { "time", 0.3f, "from", storeyc.cameraFade.m_Fade, "to", 0f, "onupdate", action, "onupdatetarget", base.gameObject, "oncompletetarget", base.gameObject, "delay", 0.5f, "name", "Fade" };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ValueTo(base.gameObject, hashtable);
        }
    }

    private void FinishPreviousScene()
    {
        Log.LoadingScreen.Print("LoadingScreen.FinishPreviousScene()", new object[0]);
        if (this.m_prevTransitionParams != null)
        {
            this.m_prevTransitionParams.DestroyObjects();
            TransformUtil.SetPosX(base.gameObject, this.m_originalPosX);
        }
        if (this.m_transitionParams.ClearPreviousAssets)
        {
            this.ClearPreviousSceneAssets();
        }
        this.m_transitionUnfriendlyData.Restore();
        this.m_transitionUnfriendlyData.Clear();
        this.m_previousSceneActive = false;
        this.FirePreviousSceneDestroyedListeners();
    }

    private void FireFinishedTransitionListeners(bool cutoff)
    {
        FinishedTransitionListener[] listenerArray = this.m_finishedTransitionListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire(cutoff);
        }
    }

    private void FirePreviousSceneDestroyedListeners()
    {
        PreviousSceneDestroyedListener[] listenerArray = this.m_prevSceneDestroyedListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire();
        }
    }

    public static LoadingScreen Get()
    {
        return s_instance;
    }

    public long GetAssetLoadStartTimestamp()
    {
        return this.m_assetLoadStartTimestamp;
    }

    public CameraFade GetCameraFade()
    {
        return base.GetComponent<CameraFade>();
    }

    public Color GetFadeColor()
    {
        return this.m_transitionParams.GetFadeColor();
    }

    public Camera GetFreezeFrameCamera()
    {
        return this.m_transitionParams.GetFreezeFrameCamera();
    }

    private FullScreenEffects GetFullScreenEffects(Camera camera)
    {
        FullScreenEffects component = camera.GetComponent<FullScreenEffects>();
        if (component != null)
        {
            return component;
        }
        return camera.gameObject.AddComponent<FullScreenEffects>();
    }

    public Camera GetFxCamera()
    {
        return this.m_fxCamera;
    }

    public Phase GetPhase()
    {
        return this.m_phase;
    }

    public AudioListener GetTransitionAudioListener()
    {
        return this.m_transitionParams.GetAudioListener();
    }

    [DebuggerHidden]
    private IEnumerator HackWaitThenStartTransitionEffects()
    {
        return new <HackWaitThenStartTransitionEffects>c__Iterator260 { <>f__this = this };
    }

    private void InitializeFxCamera()
    {
        this.m_fxCamera = base.GetComponent<Camera>();
    }

    public bool IsFading()
    {
        return (this.IsFadingOut() || this.IsFadingIn());
    }

    public bool IsFadingIn()
    {
        return (this.m_phase == Phase.FADING_IN);
    }

    public bool IsFadingOut()
    {
        return (this.m_phase == Phase.FADING_OUT);
    }

    public bool IsPreviousSceneActive()
    {
        return this.m_previousSceneActive;
    }

    public bool IsTransitionEnabled()
    {
        return this.m_transitionParams.IsEnabled();
    }

    public bool IsTransitioning()
    {
        return (this.m_phase != Phase.INVALID);
    }

    public bool IsUnloadingStrings(GameStringCategory category)
    {
        return this.m_stringCategoriesToUnload.Contains(category);
    }

    public bool IsWaiting()
    {
        switch (this.m_phase)
        {
            case Phase.WAITING_FOR_SCENE_UNLOAD:
            case Phase.WAITING_FOR_SCENE_LOAD:
            case Phase.WAITING_FOR_BLOCKERS:
                return true;
        }
        return false;
    }

    public void NotifyMainSceneObjectAwoke(GameObject mainObject)
    {
        if (this.IsPreviousSceneActive())
        {
            this.DisableTransitionUnfriendlyStuff(mainObject);
        }
    }

    public void NotifyTransitionBlockerComplete()
    {
        if (this.m_prevTransitionParams != null)
        {
            this.m_prevTransitionParams.RemoveBlocker();
            this.TransitionIfPossible();
        }
    }

    public void NotifyTransitionBlockerComplete(int count)
    {
        if (this.m_prevTransitionParams != null)
        {
            this.m_prevTransitionParams.RemoveBlocker(count);
            this.TransitionIfPossible();
        }
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    private void OnFadeInComplete()
    {
        Log.LoadingScreen.Print("LoadingScreen.OnFadeInComplete()", new object[0]);
        this.FinishFxCamera();
        this.m_prevTransitionParams = null;
        this.m_phase = Phase.INVALID;
        this.FireFinishedTransitionListeners(false);
    }

    private void OnFadeOutComplete()
    {
        Log.LoadingScreen.Print("LoadingScreen.OnFadeOutComplete()", new object[0]);
        this.FinishPreviousScene();
        this.FadeIn();
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        this.EnableTransition(false);
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        object[] args = new object[] { SceneMgr.Get().GetPrevMode(), mode };
        Log.LoadingScreen.Print("LoadingScreen.OnSceneLoaded() - prevMode={0} currMode={1}", args);
        if (mode == SceneMgr.Mode.FATAL_ERROR)
        {
            object[] objArray2 = new object[] { mode };
            Log.LoadingScreen.Print("LoadingScreen.OnSceneLoaded() - calling CutoffTransition()", objArray2);
            this.CutoffTransition();
        }
        else
        {
            if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.STARTUP)
            {
                this.m_assetLoadStartTimestamp = TimeUtils.BinaryStamp();
                object[] objArray3 = new object[] { this.m_assetLoadStartTimestamp };
                Log.LoadingScreen.Print("LoadingScreen.OnSceneLoaded() - m_assetLoadStartTimestamp={0}", objArray3);
            }
            if (this.m_phase != Phase.WAITING_FOR_SCENE_LOAD)
            {
                object[] objArray4 = new object[] { this.m_phase };
                Log.LoadingScreen.Print("LoadingScreen.OnSceneLoaded() - END - {0} != Phase.WAITING_FOR_SCENE_LOAD", objArray4);
            }
            else
            {
                this.m_phase = Phase.WAITING_FOR_BLOCKERS;
                if (!this.TransitionIfPossible())
                {
                }
            }
        }
    }

    private void OnScenePreUnload(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        object[] args = new object[] { prevMode, SceneMgr.Get().GetMode(), this.m_phase };
        Log.LoadingScreen.Print("LoadingScreen.OnScenePreUnload() - prevMode={0} nextMode={1} m_phase={2}", args);
        if (!DoesShowLoadingScreen(prevMode, SceneMgr.Get().GetMode()))
        {
            this.CutoffTransition();
        }
        else if (!this.m_transitionParams.IsEnabled())
        {
            this.CutoffTransition();
        }
        else
        {
            if (this.IsTransitioning())
            {
                this.DoInterruptionCleanUp();
            }
            this.m_assetLoadNextStartTimestamp = TimeUtils.BinaryStamp();
            if (this.IsTransitioning())
            {
                this.FireFinishedTransitionListeners(true);
                if (this.IsPreviousSceneActive())
                {
                    return;
                }
            }
            this.m_phase = Phase.WAITING_FOR_SCENE_UNLOAD;
            this.m_previousSceneActive = true;
            this.ShowFreezeFrame(this.m_transitionParams.GetFreezeFrameCamera());
        }
    }

    private void OnSceneUnloaded(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        object[] args = new object[] { prevMode, SceneMgr.Get().GetMode(), this.m_phase };
        Log.LoadingScreen.Print("LoadingScreen.OnSceneUnloaded() - prevMode={0} nextMode={1} m_phase={2}", args);
        if (this.m_phase == Phase.WAITING_FOR_SCENE_UNLOAD)
        {
            this.m_assetLoadEndTimestamp = this.m_assetLoadNextStartTimestamp;
            object[] objArray2 = new object[] { this.m_assetLoadEndTimestamp };
            Log.LoadingScreen.Print("LoadingScreen.OnSceneUnloaded() - m_assetLoadEndTimestamp={0}", objArray2);
            this.m_phase = Phase.WAITING_FOR_SCENE_LOAD;
            this.m_prevTransitionParams = this.m_transitionParams;
            this.m_transitionParams = new TransitionParams();
            this.m_transitionParams.ClearPreviousAssets = prevMode != SceneMgr.Get().GetMode();
            this.m_prevTransitionParams.AutoAddObjects();
            this.m_prevTransitionParams.FixupCameras(this.m_fxCamera);
            this.m_prevTransitionParams.PreserveObjects(base.transform);
            this.m_originalPosX = base.transform.position.x;
            TransformUtil.SetPosX(base.gameObject, 5000f);
        }
    }

    public bool RegisterFinishedTransitionListener(FinishedTransitionCallback callback)
    {
        return this.RegisterFinishedTransitionListener(callback, null);
    }

    public bool RegisterFinishedTransitionListener(FinishedTransitionCallback callback, object userData)
    {
        FinishedTransitionListener item = new FinishedTransitionListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_finishedTransitionListeners.Contains(item))
        {
            return false;
        }
        this.m_finishedTransitionListeners.Add(item);
        return true;
    }

    public bool RegisterPreviousSceneDestroyedListener(PreviousSceneDestroyedCallback callback)
    {
        return this.RegisterPreviousSceneDestroyedListener(callback, null);
    }

    public bool RegisterPreviousSceneDestroyedListener(PreviousSceneDestroyedCallback callback, object userData)
    {
        PreviousSceneDestroyedListener item = new PreviousSceneDestroyedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_prevSceneDestroyedListeners.Contains(item))
        {
            return false;
        }
        this.m_prevSceneDestroyedListeners.Add(item);
        return true;
    }

    public void RegisterSceneListeners()
    {
        SceneMgr.Get().RegisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
        SceneMgr.Get().RegisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(this.OnSceneUnloaded));
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
    }

    public void RemoveStringsToUnload(GameStringCategory category)
    {
        this.m_stringCategoriesToUnload.Remove(category);
    }

    public void SetAssetLoadStartTimestamp(long timestamp)
    {
        this.m_assetLoadStartTimestamp = Math.Min(this.m_assetLoadStartTimestamp, timestamp);
        object[] args = new object[] { this.m_assetLoadStartTimestamp };
        Log.LoadingScreen.Print("LoadingScreen.SetAssetLoadStartTimestamp() - m_assetLoadStartTimestamp={0}", args);
    }

    public void SetFadeColor(Color color)
    {
        this.m_transitionParams.SetFadeColor(color);
    }

    public void SetFreezeFrameCamera(Camera camera)
    {
        this.m_transitionParams.SetFreezeFrameCamera(camera);
    }

    public void SetTransitionAudioListener(AudioListener listener)
    {
        object[] args = new object[] { listener };
        Log.LoadingScreen.Print("LoadingScreen.SetTransitionAudioListener() - {0}", args);
        this.m_transitionParams.SetAudioListener(listener);
    }

    private void ShowFreezeFrame(Camera camera)
    {
        if (camera != null)
        {
            this.GetFullScreenEffects(camera).Freeze();
        }
    }

    private void Start()
    {
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        this.RegisterSceneListeners();
    }

    private void StopFading()
    {
        iTween.Stop(base.gameObject);
    }

    private bool TransitionIfPossible()
    {
        if (this.m_prevTransitionParams.GetBlockerCount() > 0)
        {
            return false;
        }
        base.StartCoroutine("HackWaitThenStartTransitionEffects");
        return true;
    }

    public bool UnregisterFinishedTransitionListener(FinishedTransitionCallback callback)
    {
        return this.UnregisterFinishedTransitionListener(callback, null);
    }

    public bool UnregisterFinishedTransitionListener(FinishedTransitionCallback callback, object userData)
    {
        FinishedTransitionListener item = new FinishedTransitionListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_finishedTransitionListeners.Remove(item);
    }

    public bool UnregisterPreviousSceneDestroyedListener(PreviousSceneDestroyedCallback callback)
    {
        return this.UnregisterPreviousSceneDestroyedListener(callback, null);
    }

    public bool UnregisterPreviousSceneDestroyedListener(PreviousSceneDestroyedCallback callback, object userData)
    {
        PreviousSceneDestroyedListener item = new PreviousSceneDestroyedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_prevSceneDestroyedListeners.Remove(item);
    }

    public void UnregisterSceneListeners()
    {
        SceneMgr.Get().UnregisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
        SceneMgr.Get().UnregisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(this.OnSceneUnloaded));
        SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
    }

    private void WillReset()
    {
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
    }

    [CompilerGenerated]
    private sealed class <FadeIn>c__AnonStorey37B
    {
        internal CameraFade cameraFade;

        internal void <>m__264(object amount)
        {
            this.cameraFade.m_Fade = (float) amount;
        }
    }

    [CompilerGenerated]
    private sealed class <FadeOut>c__AnonStorey37A
    {
        internal CameraFade cameraFade;

        internal void <>m__263(object amount)
        {
            this.cameraFade.m_Fade = (float) amount;
        }
    }

    [CompilerGenerated]
    private sealed class <FinishFxCamera>c__AnonStorey37C
    {
        internal CameraFade cameraFade;

        internal void <>m__265(object amount)
        {
            this.cameraFade.m_Fade = (float) amount;
        }
    }

    [CompilerGenerated]
    private sealed class <HackWaitThenStartTransitionEffects>c__Iterator260 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal LoadingScreen <>f__this;

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
                    Log.LoadingScreen.Print("LoadingScreen.HackWaitThenStartTransitionEffects() - START", new object[0]);
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    return true;

                case 1:
                {
                    if (this.<>f__this.m_phase == LoadingScreen.Phase.WAITING_FOR_BLOCKERS)
                    {
                        this.<>f__this.FadeOut();
                        this.$PC = -1;
                        break;
                    }
                    object[] args = new object[] { this.<>f__this.m_phase };
                    Log.LoadingScreen.Print("LoadingScreen.HackWaitThenStartTransitionEffects() - END - {0} != Phase.WAITING_FOR_BLOCKERS", args);
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

    public delegate void FinishedTransitionCallback(bool cutoff, object userData);

    private class FinishedTransitionListener : EventListener<LoadingScreen.FinishedTransitionCallback>
    {
        public void Fire(bool cutoff)
        {
            base.m_callback(cutoff, base.m_userData);
        }
    }

    public enum Phase
    {
        INVALID,
        WAITING_FOR_SCENE_UNLOAD,
        WAITING_FOR_SCENE_LOAD,
        WAITING_FOR_BLOCKERS,
        FADING_OUT,
        FADING_IN
    }

    public delegate void PreviousSceneDestroyedCallback(object userData);

    private class PreviousSceneDestroyedListener : EventListener<LoadingScreen.PreviousSceneDestroyedCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    private class TransitionParams
    {
        private AudioListener m_audioListener;
        private int m_blockerCount;
        private List<Camera> m_cameras = new List<Camera>();
        private bool m_clearPreviousAssets = true;
        private bool m_enabled = true;
        private Color m_fadeColor = Color.black;
        private bool m_fadeIn = true;
        private bool m_fadeOut = true;
        private Camera m_freezeFrameCamera;
        private List<Light> m_lights = new List<Light>();
        private List<GameObject> m_objects = new List<GameObject>();

        public void AddBlocker()
        {
            this.m_blockerCount++;
        }

        public void AddBlocker(int count)
        {
            this.m_blockerCount += count;
        }

        public void AddObject(Component c)
        {
            if (c != null)
            {
                this.AddObject(c.gameObject);
            }
        }

        public void AddObject(GameObject go)
        {
            if (go != null)
            {
                for (Transform transform = go.transform; transform != null; transform = transform.parent)
                {
                    if (this.m_objects.Contains(transform.gameObject))
                    {
                        return;
                    }
                }
                foreach (Camera camera in go.GetComponentsInChildren<Camera>())
                {
                    if (!this.m_cameras.Contains(camera))
                    {
                        this.m_cameras.Add(camera);
                    }
                }
                this.m_objects.Add(go);
            }
        }

        public void AutoAddObjects()
        {
            Light[] lightArray = (Light[]) UnityEngine.Object.FindObjectsOfType(typeof(Light));
            foreach (Light light in lightArray)
            {
                this.AddObject(light.gameObject);
                this.m_lights.Add(light);
            }
        }

        public void DestroyObjects()
        {
            foreach (GameObject obj2 in this.m_objects)
            {
                UnityEngine.Object.DestroyImmediate(obj2);
            }
        }

        public void Enable(bool enable)
        {
            this.m_enabled = enable;
        }

        public void EnableFadeIn(bool enable)
        {
            this.m_fadeIn = enable;
        }

        public void EnableFadeOut(bool enable)
        {
            this.m_fadeOut = enable;
        }

        public void FixupCameras(Camera fxCamera)
        {
            if (this.m_cameras.Count != 0)
            {
                Camera camera = this.m_cameras[0];
                camera.tag = "Untagged";
                float depth = camera.depth;
                for (int i = 1; i < this.m_cameras.Count; i++)
                {
                    Camera camera2 = this.m_cameras[i];
                    camera2.tag = "Untagged";
                    if (camera2.depth > depth)
                    {
                        depth = camera2.depth;
                    }
                }
                float num3 = (fxCamera.depth - 1f) - depth;
                for (int j = 0; j < this.m_cameras.Count; j++)
                {
                    Camera camera3 = this.m_cameras[j];
                    camera3.depth += num3;
                }
            }
        }

        public AudioListener GetAudioListener()
        {
            return this.m_audioListener;
        }

        public int GetBlockerCount()
        {
            return this.m_blockerCount;
        }

        public List<Camera> GetCameras()
        {
            return this.m_cameras;
        }

        public Color GetFadeColor()
        {
            return this.m_fadeColor;
        }

        public Camera GetFreezeFrameCamera()
        {
            return this.m_freezeFrameCamera;
        }

        public List<Light> GetLights()
        {
            return this.m_lights;
        }

        public bool IsEnabled()
        {
            return this.m_enabled;
        }

        public bool IsFadeInEnabled()
        {
            return this.m_fadeIn;
        }

        public bool IsFadeOutEnabled()
        {
            return this.m_fadeOut;
        }

        public void PreserveObjects(Transform parent)
        {
            foreach (GameObject obj2 in this.m_objects)
            {
                obj2.transform.parent = parent;
            }
        }

        public void RemoveBlocker()
        {
            this.m_blockerCount--;
        }

        public void RemoveBlocker(int count)
        {
            this.m_blockerCount -= count;
        }

        public void SetAudioListener(AudioListener listener)
        {
            if (listener != null)
            {
                this.m_audioListener = listener;
                this.AddObject(listener);
            }
        }

        public void SetFadeColor(Color color)
        {
            this.m_fadeColor = color;
        }

        public void SetFreezeFrameCamera(Camera camera)
        {
            if (camera != null)
            {
                this.m_freezeFrameCamera = camera;
                this.AddObject(camera.gameObject);
            }
        }

        public bool ClearPreviousAssets
        {
            get
            {
                return this.m_clearPreviousAssets;
            }
            set
            {
                this.m_clearPreviousAssets = value;
            }
        }
    }

    private class TransitionUnfriendlyData
    {
        private AudioListener m_audioListener;
        private List<Light> m_lights = new List<Light>();

        public void AddLights(Light[] lights)
        {
            foreach (Light light in lights)
            {
                if (light.enabled)
                {
                    light.enabled = false;
                    for (Transform transform = light.transform; transform.parent != null; transform = transform.parent)
                    {
                    }
                    this.m_lights.Add(light);
                }
            }
        }

        public void Clear()
        {
            this.m_audioListener = null;
            this.m_lights.Clear();
        }

        public AudioListener GetAudioListener()
        {
            return this.m_audioListener;
        }

        public List<Light> GetLights()
        {
            return this.m_lights;
        }

        public void Restore()
        {
            for (int i = 0; i < this.m_lights.Count; i++)
            {
                Light light = this.m_lights[i];
                if (light == null)
                {
                    UnityEngine.Debug.LogError(string.Format("TransitionUnfriendlyData.Restore() - light {0} is null!", i));
                }
                else
                {
                    for (Transform transform = light.transform; transform.parent != null; transform = transform.parent)
                    {
                    }
                    light.enabled = true;
                }
            }
            if (this.m_audioListener != null)
            {
                this.m_audioListener.enabled = true;
            }
        }

        public void SetAudioListener(AudioListener listener)
        {
            if ((listener != null) && listener.enabled)
            {
                this.m_audioListener = listener;
                this.m_audioListener.enabled = false;
            }
        }
    }
}

