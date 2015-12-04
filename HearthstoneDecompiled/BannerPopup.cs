using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class BannerPopup : MonoBehaviour
{
    public Spell m_HideSpell;
    private PegUIElement m_inputBlocker;
    public Spell m_LoopingSpell;
    private BannerManager.DelOnCloseBanner m_onCloseBannerPopup;
    public GameObject m_root;
    public Spell m_ShowSpell;
    private bool m_showSpellComplete = true;
    public UberText m_text;

    private void Awake()
    {
        base.gameObject.SetActive(false);
    }

    private void CloseBannerPopup(UIEvent e)
    {
        this.FadeEffectsOut();
        this.m_inputBlocker.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.CloseBannerPopup));
        object[] args = new object[] { "scale", Vector3.zero, "time", 0.5f, "oncompletetarget", base.gameObject, "oncomplete", "DestroyAdventurePopup" };
        iTween.ScaleTo(base.gameObject, iTween.Hash(args));
        SoundManager.Get().LoadAndPlay("new_quest_click_and_shrink");
        ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>();
        if (componentsInChildren != null)
        {
            foreach (ParticleSystem system in componentsInChildren)
            {
                system.gameObject.SetActive(false);
            }
        }
        if (this.m_LoopingSpell != null)
        {
            this.m_LoopingSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnLoopingSpellFinished));
            this.m_LoopingSpell.ActivateState(SpellStateType.DEATH);
        }
        else if (this.m_HideSpell != null)
        {
            this.m_HideSpell.Activate();
        }
    }

    private void DestroyAdventurePopup()
    {
        this.m_onCloseBannerPopup();
        base.StartCoroutine(this.DestroyPopupObject());
    }

    [DebuggerHidden]
    private IEnumerator DestroyPopupObject()
    {
        return new <DestroyPopupObject>c__Iterator235 { <>f__this = this };
    }

    private void EnableClickHandler()
    {
        this.m_inputBlocker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.CloseBannerPopup));
    }

    private void FadeEffectsIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.SetBlurBrightness(1f);
        mgr.SetBlurDesaturation(0f);
        mgr.Vignette(0.4f, 0.2f, iTween.EaseType.easeOutCirc, null);
        mgr.Blur(1f, 0.2f, iTween.EaseType.easeOutCirc, null);
    }

    private void FadeEffectsOut()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, null);
        mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
    }

    private void OnLoopingSpellFinished(Spell spell, object userData)
    {
        if (this.m_HideSpell != null)
        {
            this.m_HideSpell.Activate();
        }
    }

    private void OnShowSpellFinished(Spell spell, object userData)
    {
        this.m_showSpellComplete = true;
        if (this.m_LoopingSpell == null)
        {
            this.OnLoopingSpellFinished(null, null);
        }
        else
        {
            this.m_LoopingSpell.ActivateState(SpellStateType.ACTION);
        }
    }

    public void Show(string bannerText, BannerManager.DelOnCloseBanner onCloseCallback = null)
    {
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, true, CanvasScaleMode.HEIGHT);
        this.m_text.Text = bannerText;
        this.m_onCloseBannerPopup = onCloseCallback;
        base.gameObject.SetActive(true);
        this.m_root.GetComponent<Animation>().Play();
        GameObject go = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "ClosedSignInputBlocker", this);
        SceneUtils.SetLayer(go, base.gameObject.layer);
        this.m_inputBlocker = go.AddComponent<PegUIElement>();
        object[] args = new object[] { "scale", new Vector3(0.01f, 0.01f, 0.01f), "time", 0.25f, "oncompletetarget", base.gameObject, "oncomplete", "EnableClickHandler" };
        iTween.ScaleFrom(base.gameObject, iTween.Hash(args));
        this.FadeEffectsIn();
        this.m_showSpellComplete = false;
    }

    private void Start()
    {
        if (this.m_ShowSpell == null)
        {
            this.OnShowSpellFinished(null, null);
        }
        else
        {
            this.m_ShowSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnShowSpellFinished));
            this.m_ShowSpell.Activate();
        }
    }

    [CompilerGenerated]
    private sealed class <DestroyPopupObject>c__Iterator235 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal BannerPopup <>f__this;

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
                    if (!this.<>f__this.m_showSpellComplete)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    UnityEngine.Object.Destroy(this.<>f__this.gameObject);
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

