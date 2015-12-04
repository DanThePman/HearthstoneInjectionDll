using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class CollectionNewDeckButton : PegUIElement
{
    private const float BUTTON_POP_SPEED = 2.5f;
    private readonly string DECKBOX_POPDOWN_ANIM_NAME = "NewDeck_PopDown";
    private readonly string DECKBOX_POPUP_ANIM_NAME = "NewDeck_PopUp";
    public UberText m_buttonText;
    public HighlightState m_highlightState;
    private bool m_isPoppedUp;
    private bool m_isUsable;

    protected override void Awake()
    {
        base.Awake();
        base.SetEnabled(false);
        this.m_buttonText.Text = (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL) ? GameStrings.Get("GLUE_COLLECTION_NEW_DECK") : string.Empty;
        UIBScrollableItem component = base.GetComponent<UIBScrollableItem>();
        if (component != null)
        {
            component.SetCustomActiveState(new UIBScrollableItem.ActiveStateCallback(this.IsUsable));
        }
    }

    public void FlipHalfOverAndHide(float animTime, DelOnAnimationFinished finished = null)
    {
        <FlipHalfOverAndHide>c__AnonStorey2D8 storeyd = new <FlipHalfOverAndHide>c__AnonStorey2D8 {
            finished = finished,
            <>f__this = this
        };
        if (!this.m_isPoppedUp)
        {
            UnityEngine.Debug.LogWarning("Can't flip over and hide button. It is currently not popped up.");
        }
        else
        {
            iTween.StopByName(base.gameObject, "rotation");
            object[] args = new object[] { "rotation", new Vector3(270f, 0f, 0f), "isLocal", true, "time", animTime, "easeType", iTween.EaseType.easeInCubic, "oncomplete", new Action<object>(storeyd.<>m__AF), "name", "rotation" };
            iTween.RotateTo(base.gameObject, iTween.Hash(args));
            this.m_isPoppedUp = false;
        }
    }

    public bool IsUsable()
    {
        return this.m_isUsable;
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_highlightState.ChangeState(ActorStateType.NONE);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        SoundManager.Get().LoadAndPlay("Hub_Mouseover");
        this.m_highlightState.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
    }

    private void PlayAnimation(string animationName)
    {
        this.PlayAnimation(animationName, null, null);
    }

    private void PlayAnimation(string animationName, DelOnAnimationFinished callback, object callbackData)
    {
        base.GetComponent<Animation>().Play(animationName);
        OnPopAnimationFinishedCallbackData data = new OnPopAnimationFinishedCallbackData {
            m_callback = callback,
            m_callbackData = callbackData,
            m_animationName = animationName
        };
        base.StopCoroutine("WaitThenCallAnimationCallback");
        base.StartCoroutine("WaitThenCallAnimationCallback", data);
    }

    public void PlayPopDownAnimation()
    {
        this.PlayPopDownAnimation(null);
    }

    public void PlayPopDownAnimation(DelOnAnimationFinished callback)
    {
        this.PlayPopDownAnimation(callback, null, null);
    }

    public void PlayPopDownAnimation(DelOnAnimationFinished callback, object callbackData, float? speed = new float?())
    {
        base.gameObject.SetActive(true);
        if (!this.m_isPoppedUp)
        {
            if (callback != null)
            {
                callback(callbackData);
            }
        }
        else
        {
            this.m_isPoppedUp = false;
            base.GetComponent<Animation>()[this.DECKBOX_POPDOWN_ANIM_NAME].time = 0f;
            base.GetComponent<Animation>()[this.DECKBOX_POPDOWN_ANIM_NAME].speed = !speed.HasValue ? 2.5f : speed.Value;
            this.PlayAnimation(this.DECKBOX_POPDOWN_ANIM_NAME, callback, callbackData);
        }
    }

    public void PlayPopUpAnimation()
    {
        this.PlayPopUpAnimation(null);
    }

    public void PlayPopUpAnimation(DelOnAnimationFinished callback)
    {
        this.PlayPopUpAnimation(callback, null, null);
    }

    public void PlayPopUpAnimation(DelOnAnimationFinished callback, object callbackData, float? speed = new float?())
    {
        base.gameObject.SetActive(true);
        if (this.m_isPoppedUp)
        {
            if (callback != null)
            {
                callback(callbackData);
            }
        }
        else
        {
            this.m_isPoppedUp = true;
            base.GetComponent<Animation>()[this.DECKBOX_POPUP_ANIM_NAME].time = 0f;
            base.GetComponent<Animation>()[this.DECKBOX_POPUP_ANIM_NAME].speed = !speed.HasValue ? 2.5f : speed.Value;
            this.PlayAnimation(this.DECKBOX_POPUP_ANIM_NAME, callback, callbackData);
        }
    }

    public void SetIsUsable(bool isUsable)
    {
        this.m_isUsable = isUsable;
    }

    [DebuggerHidden]
    private IEnumerator WaitThenCallAnimationCallback(OnPopAnimationFinishedCallbackData callbackData)
    {
        return new <WaitThenCallAnimationCallback>c__Iterator3B { callbackData = callbackData, <$>callbackData = callbackData, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <FlipHalfOverAndHide>c__AnonStorey2D8
    {
        internal CollectionNewDeckButton <>f__this;
        internal CollectionNewDeckButton.DelOnAnimationFinished finished;

        internal void <>m__AF(object _1)
        {
            if (this.finished != null)
            {
                this.finished(this.<>f__this);
            }
            this.<>f__this.gameObject.SetActive(false);
            this.<>f__this.transform.localEulerAngles = Vector3.zero;
        }
    }

    [CompilerGenerated]
    private sealed class <WaitThenCallAnimationCallback>c__Iterator3B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionNewDeckButton.OnPopAnimationFinishedCallbackData <$>callbackData;
        internal CollectionNewDeckButton <>f__this;
        internal bool <enableInput>__0;
        internal CollectionNewDeckButton.OnPopAnimationFinishedCallbackData callbackData;

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
                    this.$current = new WaitForSeconds(this.<>f__this.GetComponent<Animation>()[this.callbackData.m_animationName].length / this.<>f__this.GetComponent<Animation>()[this.callbackData.m_animationName].speed);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<enableInput>__0 = this.callbackData.m_animationName.Equals(this.<>f__this.DECKBOX_POPUP_ANIM_NAME);
                    this.<>f__this.SetEnabled(this.<enableInput>__0);
                    if (this.callbackData.m_callback != null)
                    {
                        this.callbackData.m_callback(this.callbackData.m_callbackData);
                        this.$PC = -1;
                        break;
                    }
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

    public delegate void DelOnAnimationFinished(object callbackData);

    private class OnPopAnimationFinishedCallbackData
    {
        public string m_animationName;
        public CollectionNewDeckButton.DelOnAnimationFinished m_callback;
        public object m_callbackData;
    }
}

