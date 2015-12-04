using System;
using UnityEngine;

[ExecuteInEditMode]
public class ShowFPS : MonoBehaviour
{
    private int frames;
    private string m_fpsText;
    private int m_FrameCount;
    private bool m_FrameCountActive;
    private float m_FrameCountLastTime;
    private float m_FrameCountTime;
    private GUIText m_GuiText;
    private double m_LastInterval;
    private float m_UpdateInterval = 0.5f;
    private bool m_verbose;
    private static ShowFPS s_instance;

    private void Awake()
    {
        s_instance = this;
        if (ApplicationMgr.IsPublic())
        {
            UnityEngine.Object.DestroyImmediate(base.gameObject);
        }
    }

    [ContextMenu("Clear Frame Count")]
    public void ClearFrameCount()
    {
        this.m_FrameCountLastTime = 0f;
        this.m_FrameCountTime = 0f;
        this.m_FrameCount = 0;
        this.m_FrameCountActive = false;
    }

    public static ShowFPS Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnDisable()
    {
        if (this.m_GuiText != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_GuiText.gameObject);
        }
        UnityEngine.Time.captureFramerate = 0;
    }

    private void OnHudOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        this.UpdateEnabled();
    }

    private void Start()
    {
        this.m_LastInterval = UnityEngine.Time.realtimeSinceStartup;
        this.frames = 0;
        this.UpdateEnabled();
        Options.Get().RegisterChangedListener(Option.HUD, new Options.ChangedCallback(this.OnHudOptionChanged));
    }

    [ContextMenu("Start Frame Count")]
    public void StartFrameCount()
    {
        this.m_FrameCountLastTime = UnityEngine.Time.realtimeSinceStartup;
        this.m_FrameCountTime = 0f;
        this.m_FrameCount = 0;
        this.m_FrameCountActive = true;
    }

    [ContextMenu("Stop Frame Count")]
    public void StopFrameCount()
    {
        this.m_FrameCountActive = false;
    }

    private void Update()
    {
        bool flag = false;
        foreach (Camera camera in Camera.allCameras)
        {
            FullScreenEffects component = camera.GetComponent<FullScreenEffects>();
            if ((component != null) && component.enabled)
            {
                flag = true;
            }
        }
        if (this.m_GuiText == null)
        {
            GameObject obj2 = new GameObject("FPS") {
                transform = { position = Vector3.zero }
            };
            this.m_GuiText = obj2.AddComponent<GUIText>();
            SceneUtils.SetHideFlags(obj2, HideFlags.HideAndDontSave);
            this.m_GuiText.pixelOffset = new Vector2(Screen.width * 0.7f, 15f);
        }
        this.frames++;
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        if (realtimeSinceStartup > (this.m_LastInterval + this.m_UpdateInterval))
        {
            float num3 = ((float) this.frames) / (realtimeSinceStartup - ((float) this.m_LastInterval));
            if (this.m_verbose)
            {
                this.m_fpsText = string.Format("{0:f2} - {1} frames over {2}sec", num3, this.frames, this.m_UpdateInterval);
            }
            else
            {
                this.m_fpsText = string.Format("{0:f2}", num3);
            }
            this.frames = 0;
            this.m_LastInterval = realtimeSinceStartup;
        }
        string fpsText = this.m_fpsText;
        if (this.m_FrameCountActive || (this.m_FrameCount > 0))
        {
            if (this.m_FrameCountActive)
            {
                this.m_FrameCountTime += ((realtimeSinceStartup - this.m_FrameCountLastTime) / 60f) * UnityEngine.Time.timeScale;
                if (this.m_FrameCountLastTime == 0f)
                {
                    this.m_FrameCountLastTime = realtimeSinceStartup;
                }
                this.m_FrameCount = Mathf.CeilToInt(this.m_FrameCountTime * 60f);
            }
            fpsText = string.Format("{0} - Frame Count: {1}", fpsText, this.m_FrameCount);
        }
        if (flag)
        {
            fpsText = string.Format("{0} - FSE", fpsText);
        }
        if (ScreenEffectsMgr.Get() != null)
        {
            int activeScreenEffectsCount = ScreenEffectsMgr.Get().GetActiveScreenEffectsCount();
            if ((activeScreenEffectsCount > 0) && ScreenEffectsMgr.Get().gameObject.activeSelf)
            {
                fpsText = string.Format("{0} - ScreenEffects Active: {1}", fpsText, activeScreenEffectsCount);
            }
        }
        this.m_GuiText.text = fpsText;
    }

    private void UpdateEnabled()
    {
        base.enabled = Options.Get().GetBool(Option.HUD);
    }
}

