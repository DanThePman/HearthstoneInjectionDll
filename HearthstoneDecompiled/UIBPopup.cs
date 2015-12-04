using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class UIBPopup : MonoBehaviour
{
    [CustomEditField(Sections="Click Blockers")]
    public BoxCollider m_animationClickBlocker;
    protected bool m_destroyOnSceneLoad = true;
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_hideAnimationSound = "Assets/Game/Sounds/Interface/Shrink_Down";
    [CustomEditField(Sections="Animation & Positioning")]
    public float m_hideAnimTime = 0.1f;
    [CustomEditField(Sections="Animation & Positioning")]
    public Vector3 m_hidePosition = new Vector3(-1000f, 0f, 0f);
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public bool m_playHideSoundWithNoAnimation;
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public bool m_playShowSoundWithNoAnimation;
    protected CanvasScaleMode m_scaleMode = CanvasScaleMode.HEIGHT;
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_showAnimationSound = "Assets/Game/Sounds/Interface/Expand_Up";
    [CustomEditField(Sections="Animation & Positioning")]
    public float m_showAnimTime = 0.5f;
    protected bool m_shown;
    [CustomEditField(Sections="Animation & Positioning")]
    public Vector3 m_showPosition = Vector3.zero;
    [CustomEditField(Sections="Animation & Positioning")]
    public Vector3 m_showScale = Vector3.one;
    private const string s_ShowiTweenAnimationName = "SHOW_ANIMATION";

    protected void DoHideAnimation(OnAnimationComplete animationDoneCallback = null)
    {
        this.DoHideAnimation(false, animationDoneCallback);
    }

    protected void DoHideAnimation(bool disableAnimation, OnAnimationComplete animationDoneCallback = null)
    {
        <DoHideAnimation>c__AnonStorey29F storeyf;
        storeyf = new <DoHideAnimation>c__AnonStorey29F {
            animationDoneCallback = animationDoneCallback,
            <>f__this = this,
            setHidePosition = new System.Action(storeyf.<>m__24)
        };
        if (!disableAnimation && (this.m_hideAnimTime > 0f))
        {
            if (!string.IsNullOrEmpty(this.m_showAnimationSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_hideAnimationSound));
            }
            object[] args = new object[] { "scale", (Vector3) (this.m_showScale * 0.01f), "isLocal", true, "time", this.m_hideAnimTime, "easetype", iTween.EaseType.linear, "name", "SHOW_ANIMATION" };
            Hashtable hashtable = iTween.Hash(args);
            if (storeyf.animationDoneCallback != null)
            {
                hashtable.Add("oncomplete", new Action<object>(storeyf.<>m__25));
            }
            else
            {
                hashtable.Add("oncomplete", new Action<object>(storeyf.<>m__26));
            }
            iTween.StopByName(base.gameObject, "SHOW_ANIMATION");
            iTween.ScaleTo(base.gameObject, hashtable);
        }
        else
        {
            if (this.m_playHideSoundWithNoAnimation && !string.IsNullOrEmpty(this.m_hideAnimationSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_hideAnimationSound));
            }
            storeyf.setHidePosition();
            if (storeyf.animationDoneCallback != null)
            {
                storeyf.animationDoneCallback();
            }
        }
    }

    protected void DoShowAnimation(OnAnimationComplete animationDoneCallback = null)
    {
        this.DoShowAnimation(false, animationDoneCallback);
    }

    protected void DoShowAnimation(bool disableAnimation, OnAnimationComplete animationDoneCallback = null)
    {
        <DoShowAnimation>c__AnonStorey29E storeye = new <DoShowAnimation>c__AnonStorey29E {
            animationDoneCallback = animationDoneCallback,
            <>f__this = this
        };
        base.transform.localPosition = this.m_showPosition;
        CanvasScaleMode scaleMode = this.m_scaleMode;
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, this.m_destroyOnSceneLoad, scaleMode);
        this.EnableAnimationClickBlocker(true);
        if (!disableAnimation && (this.m_showAnimTime > 0f))
        {
            base.transform.localScale = (Vector3) (this.m_showScale * 0.01f);
            if (!string.IsNullOrEmpty(this.m_showAnimationSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_showAnimationSound));
            }
            object[] args = new object[] { "scale", this.m_showScale, "isLocal", false, "time", this.m_showAnimTime, "easetype", iTween.EaseType.easeOutBounce, "name", "SHOW_ANIMATION" };
            Hashtable hashtable = iTween.Hash(args);
            if (storeye.animationDoneCallback != null)
            {
                hashtable.Add("oncomplete", new Action<object>(storeye.<>m__23));
            }
            iTween.StopByName(base.gameObject, "SHOW_ANIMATION");
            iTween.ScaleTo(base.gameObject, hashtable);
        }
        else
        {
            if (this.m_playShowSoundWithNoAnimation && !string.IsNullOrEmpty(this.m_showAnimationSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_showAnimationSound));
            }
            base.transform.localScale = this.m_showScale;
            if (storeye.animationDoneCallback != null)
            {
                this.EnableAnimationClickBlocker(false);
                storeye.animationDoneCallback();
            }
        }
    }

    private void EnableAnimationClickBlocker(bool enable)
    {
        if (this.m_animationClickBlocker != null)
        {
            this.m_animationClickBlocker.gameObject.SetActive(enable);
        }
    }

    public virtual void Hide()
    {
        this.Hide(false);
    }

    protected virtual void Hide(bool animate)
    {
        if (this.m_shown)
        {
            this.m_shown = false;
            this.DoHideAnimation(!animate, new OnAnimationComplete(this.OnHidden));
        }
    }

    public virtual bool IsShown()
    {
        return this.m_shown;
    }

    protected virtual void OnHidden()
    {
    }

    public virtual void Show()
    {
        if (!this.m_shown)
        {
            CanvasScaleMode scaleMode = this.m_scaleMode;
            OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, this.m_destroyOnSceneLoad, scaleMode);
            this.m_shown = true;
            this.DoShowAnimation(null);
        }
    }

    [CompilerGenerated]
    private sealed class <DoHideAnimation>c__AnonStorey29F
    {
        internal UIBPopup <>f__this;
        internal UIBPopup.OnAnimationComplete animationDoneCallback;
        internal System.Action setHidePosition;

        internal void <>m__24()
        {
            this.<>f__this.transform.localPosition = this.<>f__this.m_hidePosition;
            this.<>f__this.transform.localScale = this.<>f__this.m_showScale;
        }

        internal void <>m__25(object o)
        {
            this.setHidePosition();
            this.animationDoneCallback();
        }

        internal void <>m__26(object o)
        {
            this.setHidePosition();
        }
    }

    [CompilerGenerated]
    private sealed class <DoShowAnimation>c__AnonStorey29E
    {
        internal UIBPopup <>f__this;
        internal UIBPopup.OnAnimationComplete animationDoneCallback;

        internal void <>m__23(object o)
        {
            this.<>f__this.EnableAnimationClickBlocker(false);
            this.animationDoneCallback();
        }
    }

    public delegate void OnAnimationComplete();
}

