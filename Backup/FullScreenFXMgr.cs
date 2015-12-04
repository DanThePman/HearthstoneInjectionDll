using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class FullScreenFXMgr : MonoBehaviour
{
    private int m_ActiveEffectsCount;
    private EffectListener m_blendToColorListener;
    private EffectListener m_blurListener;
    private EffectListener m_desatListener;
    private FullScreenEffects m_FullScreenEffects;
    private int m_StdBlurVignetteCount;
    private EffectListener m_vignetteListener;
    private static FullScreenFXMgr s_instance;

    private void Awake()
    {
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
        s_instance = this;
    }

    private void BeginEffect(string name, string onUpdate, string onComplete, float start, float end, float time, iTween.EaseType easeType)
    {
        Log.FullScreenFX.Print(string.Concat(new object[] { "BeginEffect ", name, " ", start, " => ", end }), new object[0]);
        Hashtable args = new Hashtable();
        args["name"] = name;
        args["onupdate"] = onUpdate;
        args["onupdatetarget"] = base.gameObject;
        args["from"] = start;
        if (!string.IsNullOrEmpty(onComplete))
        {
            args["oncomplete"] = onComplete;
            args["oncompletetarget"] = base.gameObject;
        }
        args["to"] = end;
        args["time"] = time;
        args["easetype"] = easeType;
        iTween.StopByName(base.gameObject, name);
        iTween.ValueTo(base.gameObject, args);
    }

    public void BlendToColor(Color blendColor, float endVal, float time, iTween.EaseType easeType, EffectListener listener = null)
    {
        this.GetCurrEffects();
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.enabled = true;
            this.m_FullScreenEffects.BlendToColorEnable = true;
            this.m_FullScreenEffects.BlendToColor = blendColor;
            this.m_blendToColorListener = listener;
            this.BeginEffect("blendtocolor", "OnBlendToColor", "OnBlendToColorComplete", 0f, endVal, time, easeType);
        }
    }

    public void Blur(float blurVal, float time, iTween.EaseType easeType, EffectListener listener = null)
    {
        this.m_ActiveEffectsCount++;
        this.GetCurrEffects();
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlurEnabled = true;
            this.m_blurListener = listener;
            this.BeginEffect("blur", "OnBlur", "OnBlurComplete", this.m_FullScreenEffects.BlurBlend, blurVal, time, easeType);
        }
    }

    public void Desaturate(float endVal, float time, iTween.EaseType easeType, EffectListener listener = null)
    {
        this.GetCurrEffects();
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.DesaturationEnabled = true;
            this.m_desatListener = listener;
            this.BeginEffect("desat", "OnDesat", "OnDesatComplete", this.m_FullScreenEffects.Desaturation, endVal, time, easeType);
        }
    }

    public void DisableBlur()
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlurEnabled = false;
            this.m_FullScreenEffects.BlurBlend = 0f;
            this.m_FullScreenEffects.BlurAmount = 0f;
        }
    }

    public void EndStandardBlurVignette(float time, EffectListener listener = null)
    {
        if (this.m_StdBlurVignetteCount != 0)
        {
            this.m_StdBlurVignetteCount--;
            if (this.m_StdBlurVignetteCount == 0)
            {
                this.StopBlur(time, iTween.EaseType.easeOutCirc, null);
                this.StopVignette(time, iTween.EaseType.easeOutCirc, listener);
            }
        }
    }

    public static FullScreenFXMgr Get()
    {
        return s_instance;
    }

    private FullScreenEffects GetCurrEffects()
    {
        if (Camera.main == null)
        {
            UnityEngine.Debug.LogError("Camera.main is null!");
            return null;
        }
        FullScreenEffects component = Camera.main.GetComponent<FullScreenEffects>();
        if (component == null)
        {
            UnityEngine.Debug.LogError("fullScreenEffects is nulll!");
            return null;
        }
        this.m_FullScreenEffects = component;
        return component;
    }

    public bool isFullScreenEffectActive()
    {
        if (this.m_FullScreenEffects == null)
        {
            return false;
        }
        return this.m_FullScreenEffects.isActive();
    }

    public void OnBlendToColor(float val)
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlendToColorAmount = val;
        }
    }

    public void OnBlendToColorClear()
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlendToColorEnable = false;
            this.OnBlendToColorComplete();
        }
    }

    public void OnBlendToColorComplete()
    {
        if (this.m_blendToColorListener != null)
        {
            this.m_blendToColorListener();
        }
    }

    public void OnBlur(float val)
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlurBlend = val;
        }
    }

    public void OnBlurClear()
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlurEnabled = false;
            this.OnBlurComplete();
        }
    }

    public void OnBlurComplete()
    {
        if (this.m_blurListener != null)
        {
            this.m_blurListener();
        }
    }

    public void OnDesat(float val)
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.Desaturation = val;
        }
    }

    public void OnDesatClear()
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.DesaturationEnabled = false;
            this.OnDesatComplete();
        }
    }

    public void OnDesatComplete()
    {
        if (this.m_desatListener != null)
        {
            this.m_desatListener();
        }
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    public void OnVignette(float val)
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.VignettingIntensity = val;
        }
    }

    public void OnVignetteClear()
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.VignettingEnable = false;
            this.OnVignetteComplete();
        }
    }

    public void OnVignetteComplete()
    {
        if (this.m_vignetteListener != null)
        {
            this.m_vignetteListener();
        }
    }

    public void SetBlurAmount(float val)
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlurAmount = val;
        }
    }

    public void SetBlurBrightness(float val)
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlurBrightness = val;
        }
    }

    public void SetBlurDesaturation(float val)
    {
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.BlurDesaturation = val;
        }
    }

    public void StartStandardBlurVignette(float time)
    {
        if (this.m_StdBlurVignetteCount == 0)
        {
            this.SetBlurBrightness(1f);
            this.SetBlurDesaturation(0f);
            this.Vignette(0.4f, time, iTween.EaseType.easeOutCirc, null);
            this.Blur(1f, time, iTween.EaseType.easeOutCirc, null);
        }
        this.m_StdBlurVignetteCount++;
    }

    public void StopAllEffects(float delay = 0f)
    {
        this.GetCurrEffects();
        if ((this.m_FullScreenEffects != null) && this.m_FullScreenEffects.isActive())
        {
            Log.FullScreenFX.Print("StopAllEffects", new object[0]);
            base.StartCoroutine(this.StopAllEffectsCoroutine(this.m_FullScreenEffects, delay));
        }
    }

    [DebuggerHidden]
    private IEnumerator StopAllEffectsCoroutine(FullScreenEffects effects, float delay)
    {
        return new <StopAllEffectsCoroutine>c__Iterator27F { delay = delay, effects = effects, <$>delay = delay, <$>effects = effects, <>f__this = this };
    }

    public void StopBlendToColor(float time, iTween.EaseType easeType, EffectListener listener = null)
    {
        if ((this.m_FullScreenEffects != null) && this.m_FullScreenEffects.isActive())
        {
            this.m_FullScreenEffects.BlendToColorEnable = true;
            this.m_blendToColorListener = listener;
            this.BeginEffect("blendtocolor", "OnBlendToColor", "OnBlendToColorClear", this.m_FullScreenEffects.BlendToColorAmount, 0f, time, easeType);
        }
    }

    public void StopBlur(float time, iTween.EaseType easeType, EffectListener listener = null)
    {
        this.m_ActiveEffectsCount--;
        if (((this.m_ActiveEffectsCount <= 0) && (this.m_FullScreenEffects != null)) && this.m_FullScreenEffects.isActive())
        {
            this.m_FullScreenEffects.BlurEnabled = true;
            this.m_blurListener = listener;
            this.BeginEffect("blur", "OnBlur", "OnBlurClear", this.m_FullScreenEffects.BlurBlend, 0f, time, easeType);
        }
    }

    public void StopDesaturate(float time, iTween.EaseType easeType, EffectListener listener = null)
    {
        if ((this.m_FullScreenEffects != null) && this.m_FullScreenEffects.isActive())
        {
            this.m_FullScreenEffects.DesaturationEnabled = true;
            this.m_desatListener = listener;
            this.BeginEffect("desat", "OnDesat", "OnDesatClear", this.m_FullScreenEffects.Desaturation, 0f, time, easeType);
        }
    }

    public void StopVignette(float time, iTween.EaseType easeType, EffectListener listener = null)
    {
        if ((this.m_FullScreenEffects != null) && this.m_FullScreenEffects.isActive())
        {
            this.m_FullScreenEffects.VignettingEnable = true;
            this.m_vignetteListener = listener;
            this.BeginEffect("vignette", "OnVignette", "OnVignetteClear", this.m_FullScreenEffects.VignettingIntensity, 0f, time, easeType);
        }
    }

    public void Vignette(float endVal, float time, iTween.EaseType easeType, EffectListener listener = null)
    {
        this.GetCurrEffects();
        if (this.m_FullScreenEffects != null)
        {
            this.m_FullScreenEffects.VignettingEnable = true;
            this.m_vignetteListener = listener;
            this.BeginEffect("vignette", "OnVignette", "OnVignetteComplete", 0f, endVal, time, easeType);
        }
    }

    public void WillReset()
    {
        this.m_ActiveEffectsCount = 0;
        Camera main = Camera.main;
        if (main != null)
        {
            FullScreenEffects component = main.GetComponent<FullScreenEffects>();
            if (component != null)
            {
                component.BlurEnabled = false;
                component.VignettingEnable = false;
                component.DesaturationEnabled = false;
                component.BlendToColorEnable = false;
                this.m_StdBlurVignetteCount = 0;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <StopAllEffectsCoroutine>c__Iterator27F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal FullScreenEffects <$>effects;
        internal FullScreenFXMgr <>f__this;
        internal float <stopEffectsTime>__0;
        internal float delay;
        internal FullScreenEffects effects;

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
                    this.<stopEffectsTime>__0 = 0.25f;
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    goto Label_015B;

                case 1:
                    Log.FullScreenFX.Print("StopAllEffectsCoroutine stopping effects now", new object[0]);
                    if (this.effects != null)
                    {
                        if (this.effects.BlurEnabled)
                        {
                            this.<>f__this.StopBlur(this.<stopEffectsTime>__0, iTween.EaseType.linear, null);
                        }
                        if (this.effects.VignettingEnable)
                        {
                            this.<>f__this.StopVignette(this.<stopEffectsTime>__0, iTween.EaseType.linear, null);
                        }
                        if (this.effects.BlendToColorEnable)
                        {
                            this.<>f__this.StopBlendToColor(this.<stopEffectsTime>__0, iTween.EaseType.linear, null);
                        }
                        if (this.effects.DesaturationEnabled)
                        {
                            this.<>f__this.StopDesaturate(this.<stopEffectsTime>__0, iTween.EaseType.linear, null);
                        }
                        this.<>f__this.m_StdBlurVignetteCount = 0;
                        this.$current = new WaitForSeconds(this.<stopEffectsTime>__0);
                        this.$PC = 2;
                        goto Label_015B;
                    }
                    break;

                case 2:
                    if (this.effects != null)
                    {
                        this.effects.Disable();
                        this.$PC = -1;
                        break;
                    }
                    break;
            }
            return false;
        Label_015B:
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

    public delegate void EffectListener();
}

