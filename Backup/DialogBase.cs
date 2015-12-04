using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class DialogBase : MonoBehaviour
{
    protected List<HideListener> m_hideListeners = new List<HideListener>();
    protected Vector3 m_originalPosition;
    protected Vector3 m_originalScale;
    protected ShowAnimState m_showAnimState;
    protected bool m_shown;
    protected Vector3 PUNCH_SCALE = ((Vector3) (1.2f * Vector3.one));
    protected readonly Vector3 START_SCALE = ((Vector3) (0.01f * Vector3.one));

    public bool AddHideListener(HideCallback callback)
    {
        return this.AddHideListener(callback, null);
    }

    public bool AddHideListener(HideCallback callback, object userData)
    {
        HideListener item = new HideListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_hideListeners.Contains(item))
        {
            return false;
        }
        this.m_hideListeners.Add(item);
        return true;
    }

    protected virtual void Awake()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.PUNCH_SCALE = (Vector3) (1.08f * Vector3.one);
        }
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        this.m_originalPosition = base.transform.position;
        this.m_originalScale = base.transform.localScale;
        this.SetHiddenPosition(null);
    }

    protected void DoHideAnimation()
    {
        AnimationUtil.ScaleFade(base.gameObject, this.START_SCALE, "OnHideAnimFinished");
    }

    protected void DoShowAnimation()
    {
        this.m_showAnimState = ShowAnimState.IN_PROGRESS;
        AnimationUtil.ShowWithPunch(base.gameObject, this.START_SCALE, Vector3.Scale(this.PUNCH_SCALE, this.m_originalScale), this.m_originalScale, "OnShowAnimFinished", false, null, null, null);
    }

    protected void FireHideListeners()
    {
        HideListener[] listenerArray = this.m_hideListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire(this);
        }
    }

    public virtual void GoBack()
    {
    }

    public virtual bool HandleKeyboardInput()
    {
        return false;
    }

    public virtual void Hide()
    {
        this.m_shown = false;
        base.StartCoroutine(this.HideWhenAble());
    }

    [DebuggerHidden]
    private IEnumerator HideWhenAble()
    {
        return new <HideWhenAble>c__Iterator85 { <>f__this = this };
    }

    public virtual bool IsShown()
    {
        return this.m_shown;
    }

    protected virtual void OnHideAnimFinished()
    {
        this.SetHiddenPosition(null);
        UniversalInputManager.Get().SetSystemDialogActive(false);
        this.FireHideListeners();
    }

    protected virtual void OnShowAnimFinished()
    {
        this.m_showAnimState = ShowAnimState.FINISHED;
    }

    public bool RemoveHideListener(HideCallback callback)
    {
        return this.RemoveHideListener(callback, null);
    }

    public bool RemoveHideListener(HideCallback callback, object userData)
    {
        HideListener item = new HideListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_hideListeners.Remove(item);
    }

    protected void SetHiddenPosition(Camera referenceCamera = null)
    {
        if (referenceCamera == null)
        {
            referenceCamera = PegUI.Get().orthographicUICam;
        }
        base.transform.position = referenceCamera.transform.TransformPoint(0f, 0f, -1000f);
    }

    protected void SetShownPosition()
    {
        base.transform.position = this.m_originalPosition;
    }

    public virtual void Show()
    {
        this.m_shown = true;
        this.SetShownPosition();
    }

    [CompilerGenerated]
    private sealed class <HideWhenAble>c__Iterator85 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DialogBase <>f__this;

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
                    if (this.<>f__this.m_showAnimState == DialogBase.ShowAnimState.IN_PROGRESS)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.DoHideAnimation();
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

    public delegate void HideCallback(DialogBase dialog, object userData);

    protected class HideListener : EventListener<DialogBase.HideCallback>
    {
        public void Fire(DialogBase dialog)
        {
            base.m_callback(dialog, base.m_userData);
        }
    }

    protected enum ShowAnimState
    {
        NOT_CALLED,
        IN_PROGRESS,
        FINISHED
    }
}

