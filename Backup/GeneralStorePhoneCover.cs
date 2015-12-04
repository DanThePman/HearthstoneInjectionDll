using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStorePhoneCover : MonoBehaviour
{
    [CompilerGenerated]
    private static UIEvent.Handler <>f__am$cacheA;
    [CustomEditField(Sections="UI Blockers")]
    public GameObject m_animationClickBlocker;
    [CustomEditField(Sections="Animation")]
    public Animator m_animationController;
    [CustomEditField(Sections="General UI")]
    public PegUIElement m_backToCoverButton;
    [CustomEditField(Sections="Animation")]
    public string m_buttonEnterAnimation = string.Empty;
    [CustomEditField(Sections="Animation")]
    public List<ModeAnimation> m_buttonExitAnimations = new List<ModeAnimation>();
    [CustomEditField(Sections="UI Blockers")]
    public GameObject m_coverClickArea;
    [CustomEditField(Sections="General UI")]
    public GeneralStore m_parentStore;
    [CustomEditField(Sections="Aspect Ratio Positioning")]
    public float m_yPos16to9;
    [CustomEditField(Sections="Aspect Ratio Positioning")]
    public float m_yPos3to2;
    private const string s_coverAnimationCoroutine = "PlayAndWaitForAnimation";
    private static GeneralStorePhoneCover s_instance;

    private void Awake()
    {
        s_instance = this;
        StoreManager.Get().RegisterStoreShownListener(new StoreManager.StoreShownCallback(this.UpdateCoverPosition));
        this.m_parentStore.RegisterModeChangedListener(new GeneralStore.ModeChanged(this.OnGeneralStoreModeChanged));
        if (<>f__am$cacheA == null)
        {
            <>f__am$cacheA = e => Navigation.GoBack();
        }
        this.m_backToCoverButton.AddEventListener(UIEventType.RELEASE, <>f__am$cacheA);
    }

    public void HideCover(GeneralStoreMode selectedMode)
    {
        <HideCover>c__AnonStorey347 storey = new <HideCover>c__AnonStorey347 {
            selectedMode = selectedMode
        };
        base.StartCoroutine(this.PushBackMethodWhenShown());
        ModeAnimation animation = this.m_buttonExitAnimations.Find(new Predicate<ModeAnimation>(storey.<>m__19A));
        if (animation == null)
        {
            UnityEngine.Debug.LogError(string.Format("Unable to find animation for {0} mode.", storey.selectedMode));
        }
        else if (string.IsNullOrEmpty(animation.m_playAnimationName))
        {
            UnityEngine.Debug.LogError(string.Format("Animation name not defined for {0} mode.", storey.selectedMode));
        }
        else
        {
            base.StopCoroutine("PlayAndWaitForAnimation");
            base.StartCoroutine("PlayAndWaitForAnimation", animation.m_playAnimationName);
            this.m_coverClickArea.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
        StoreManager.Get().RemoveStoreShownListener(new StoreManager.StoreShownCallback(this.UpdateCoverPosition));
    }

    private void OnGeneralStoreModeChanged(GeneralStoreMode oldMode, GeneralStoreMode newMode)
    {
        if (newMode != GeneralStoreMode.NONE)
        {
            this.HideCover(newMode);
        }
        else
        {
            this.ShowCover();
        }
    }

    public static bool OnNavigateBack()
    {
        if (s_instance == null)
        {
            return false;
        }
        s_instance.m_parentStore.SetMode(GeneralStoreMode.NONE);
        return true;
    }

    [DebuggerHidden]
    private IEnumerator PlayAndWaitForAnimation(string animationName)
    {
        return new <PlayAndWaitForAnimation>c__Iterator1E0 { animationName = animationName, <$>animationName = animationName, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator PushBackMethodWhenShown()
    {
        return new <PushBackMethodWhenShown>c__Iterator1DF { <>f__this = this };
    }

    public void ShowCover()
    {
        base.StopCoroutine("PlayAndWaitForAnimation");
        base.StartCoroutine("PlayAndWaitForAnimation", this.m_buttonEnterAnimation);
        this.m_coverClickArea.SetActive(true);
    }

    private void Start()
    {
        this.ShowCover();
    }

    private void UpdateCoverPosition(object data)
    {
        TransformUtil.SetLocalPosY(this, TransformUtil.GetAspectRatioDependentValue(this.m_yPos3to2, this.m_yPos16to9));
    }

    [CompilerGenerated]
    private sealed class <HideCover>c__AnonStorey347
    {
        internal GeneralStoreMode selectedMode;

        internal bool <>m__19A(GeneralStorePhoneCover.ModeAnimation o)
        {
            return (o.m_mode == this.selectedMode);
        }
    }

    [CompilerGenerated]
    private sealed class <PlayAndWaitForAnimation>c__Iterator1E0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>animationName;
        internal GeneralStorePhoneCover <>f__this;
        internal AnimatorStateInfo <stateInfo>__0;
        internal string animationName;

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
                    this.<>f__this.m_animationController.enabled = true;
                    this.<>f__this.m_animationController.StopPlayback();
                    this.<>f__this.m_animationClickBlocker.SetActive(true);
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    goto Label_00EC;

                case 1:
                    this.<>f__this.m_animationController.Play(this.animationName);
                    break;

                case 2:
                    break;

                default:
                    goto Label_00EA;
            }
            this.<stateInfo>__0 = this.<>f__this.m_animationController.GetCurrentAnimatorStateInfo(0);
            if (this.<stateInfo>__0.normalizedTime < 1f)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00EC;
            }
            this.<>f__this.m_animationClickBlocker.SetActive(false);
            this.$PC = -1;
        Label_00EA:
            return false;
        Label_00EC:
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
    private sealed class <PushBackMethodWhenShown>c__Iterator1DF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GeneralStorePhoneCover <>f__this;

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
                    if (!this.<>f__this.m_parentStore.IsShown())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    Navigation.Push(new Navigation.NavigateBackHandler(GeneralStorePhoneCover.OnNavigateBack));
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

    public delegate void AnimationCallback();

    [Serializable]
    public class ModeAnimation
    {
        public GeneralStoreMode m_mode;
        public string m_playAnimationName;
    }
}

