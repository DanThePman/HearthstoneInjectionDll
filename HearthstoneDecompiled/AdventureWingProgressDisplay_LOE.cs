using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class AdventureWingProgressDisplay_LOE : AdventureWingProgressDisplay
{
    private bool m_animating;
    public PegUIElement m_completeStaffHitArea;
    [CustomEditField(Sections="VO")]
    public string m_completeStaffQuotePrefab;
    [CustomEditField(Sections="VO")]
    public string m_completeStaffQuoteVOLine;
    public List<GameObject> m_emptyStaffObjects = new List<GameObject>();
    private bool m_finalWingComplete;
    public PegUIElement m_hangingSignHitArea;
    [CustomEditField(Sections="VO")]
    public string m_hangingSignQuotePrefab;
    [CustomEditField(Sections="VO")]
    public string m_hangingSignQuoteVOLine;
    public UberText m_hangingSignText;
    private bool m_headComplete;
    public List<GameObject> m_headObjects = new List<GameObject>();
    private bool m_pearlComplete;
    public List<GameObject> m_pearlObjects = new List<GameObject>();
    private bool m_rodComplete;
    public List<GameObject> m_rodObjects = new List<GameObject>();
    public List<GameObject> m_visibleStaffObjects = new List<GameObject>();
    private const string s_CompleteAnimationVarName = "AnimationComplete";
    private const string s_WingDisappearAnimateEventName = "OnWingDisappear";
    private const string s_WingReappearAnimateEventName = "OnWingReappear";

    private void Awake()
    {
        SetObjectsVisibility(this.m_emptyStaffObjects, true);
        SetObjectsVisibility(this.m_rodObjects, false);
        SetObjectsVisibility(this.m_headObjects, false);
        SetObjectsVisibility(this.m_pearlObjects, false);
        SetObjectsVisibility(this.m_visibleStaffObjects, false);
        if (this.m_hangingSignHitArea != null)
        {
            this.m_hangingSignHitArea.AddEventListener(UIEventType.RELEASE, e => this.OnHangingSignClick());
        }
        if (this.m_completeStaffHitArea != null)
        {
            this.m_completeStaffHitArea.AddEventListener(UIEventType.RELEASE, e => this.OnCompleteStaffClick());
        }
    }

    public override bool HasProgressAnimationToPlay()
    {
        if ((!this.m_rodComplete || !this.m_headComplete) || !this.m_pearlComplete)
        {
            return false;
        }
        if (this.m_finalWingComplete)
        {
            return !Options.Get().GetBool(Option.HAS_SEEN_LOE_STAFF_REAPPEAR, false);
        }
        return !Options.Get().GetBool(Option.HAS_SEEN_LOE_STAFF_DISAPPEAR, false);
    }

    private void OnCompleteStaffClick()
    {
        if ((!this.m_animating && ((this.m_rodComplete && this.m_headComplete) && (this.m_pearlComplete && this.m_finalWingComplete))) && (!string.IsNullOrEmpty(this.m_completeStaffQuotePrefab) && !string.IsNullOrEmpty(this.m_completeStaffQuoteVOLine)))
        {
            NotificationManager.Get().CreateCharacterQuote(FileUtils.GameAssetPathToName(this.m_completeStaffQuotePrefab), GameStrings.Get(this.m_completeStaffQuoteVOLine), this.m_completeStaffQuoteVOLine, true, 0f, CanvasAnchor.BOTTOM_LEFT);
        }
    }

    private void OnHangingSignClick()
    {
        if ((!this.m_animating && ((!this.m_rodComplete && !this.m_headComplete) && !this.m_pearlComplete)) && (!string.IsNullOrEmpty(this.m_hangingSignQuotePrefab) && !string.IsNullOrEmpty(this.m_hangingSignQuoteVOLine)))
        {
            NotificationManager.Get().CreateCharacterQuote(FileUtils.GameAssetPathToName(this.m_hangingSignQuotePrefab), GameStrings.Get(this.m_hangingSignQuoteVOLine), this.m_hangingSignQuoteVOLine, true, 0f, CanvasAnchor.BOTTOM_LEFT);
        }
    }

    [DebuggerHidden]
    private IEnumerator PlayCompleteAnimationCoroutine(PlayMakerFSM fsm, string eventName, AdventureWingProgressDisplay.OnAnimationComplete onAnimComplete, Option seenOption)
    {
        return new <PlayCompleteAnimationCoroutine>c__Iterator10 { fsm = fsm, eventName = eventName, seenOption = seenOption, onAnimComplete = onAnimComplete, <$>fsm = fsm, <$>eventName = eventName, <$>seenOption = seenOption, <$>onAnimComplete = onAnimComplete, <>f__this = this };
    }

    public override void PlayProgressAnimation(AdventureWingProgressDisplay.OnAnimationComplete onAnimComplete = null)
    {
        if ((!this.m_rodComplete || !this.m_headComplete) || !this.m_pearlComplete)
        {
            if (onAnimComplete != null)
            {
                onAnimComplete();
            }
        }
        else
        {
            PlayMakerFSM component = base.GetComponent<PlayMakerFSM>();
            if (component == null)
            {
                if (onAnimComplete != null)
                {
                    onAnimComplete();
                }
            }
            else if (!this.m_finalWingComplete)
            {
                if (Options.Get().GetBool(Option.HAS_SEEN_LOE_STAFF_DISAPPEAR, false))
                {
                    if (onAnimComplete != null)
                    {
                        onAnimComplete();
                    }
                }
                else
                {
                    base.StartCoroutine(this.PlayCompleteAnimationCoroutine(component, "OnWingDisappear", onAnimComplete, Option.HAS_SEEN_LOE_STAFF_DISAPPEAR));
                }
            }
            else if (Options.Get().GetBool(Option.HAS_SEEN_LOE_STAFF_REAPPEAR, false))
            {
                if (onAnimComplete != null)
                {
                    onAnimComplete();
                }
            }
            else
            {
                base.StartCoroutine(this.PlayCompleteAnimationCoroutine(component, "OnWingReappear", onAnimComplete, Option.HAS_SEEN_LOE_STAFF_REAPPEAR));
            }
        }
    }

    private static void SetObjectsVisibility(List<GameObject> objs, bool show)
    {
        foreach (GameObject obj2 in objs)
        {
            if (obj2 != null)
            {
                obj2.SetActive(show);
            }
        }
    }

    private void Update()
    {
        if (AdventureScene.Get().IsDevMode)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                base.StartCoroutine(this.PlayCompleteAnimationCoroutine(base.GetComponent<PlayMakerFSM>(), "OnWingDisappear", null, Option.INVALID));
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                base.StartCoroutine(this.PlayCompleteAnimationCoroutine(base.GetComponent<PlayMakerFSM>(), "OnWingReappear", null, Option.INVALID));
            }
        }
    }

    private void UpdatePartVisibility()
    {
        bool @bool = Options.Get().GetBool(Option.HAS_SEEN_LOE_STAFF_DISAPPEAR, false);
        if (this.m_finalWingComplete)
        {
            bool flag2 = Options.Get().GetBool(Option.HAS_SEEN_LOE_STAFF_REAPPEAR, false);
            SetObjectsVisibility(this.m_emptyStaffObjects, false);
            SetObjectsVisibility(this.m_rodObjects, this.m_rodComplete && flag2);
            SetObjectsVisibility(this.m_headObjects, this.m_headComplete && flag2);
            SetObjectsVisibility(this.m_pearlObjects, this.m_pearlComplete && flag2);
            SetObjectsVisibility(this.m_visibleStaffObjects, true);
        }
        else
        {
            bool show = this.m_rodComplete && !@bool;
            bool flag4 = this.m_headComplete && !@bool;
            bool flag5 = this.m_pearlComplete && !@bool;
            bool flag6 = (show || flag4) || flag5;
            SetObjectsVisibility(this.m_emptyStaffObjects, !flag6);
            SetObjectsVisibility(this.m_rodObjects, show);
            SetObjectsVisibility(this.m_headObjects, flag4);
            SetObjectsVisibility(this.m_pearlObjects, flag5);
            SetObjectsVisibility(this.m_visibleStaffObjects, flag6);
        }
        if (this.m_hangingSignText != null)
        {
            this.m_hangingSignText.Text = !@bool ? GameStrings.Get("GLUE_ADVENTURE_LOE_STAFF_RESERVED") : GameStrings.Get("GLUE_ADVENTURE_LOE_STAFF_DISAPPEARED");
        }
        if (this.m_completeStaffHitArea != null)
        {
            this.m_completeStaffHitArea.gameObject.SetActive(((this.m_finalWingComplete && this.m_rodComplete) && this.m_headComplete) && this.m_pearlComplete);
        }
        if (this.m_hangingSignHitArea != null)
        {
            this.m_hangingSignHitArea.SetEnabled(((!this.m_finalWingComplete && !this.m_rodComplete) && !this.m_headComplete) && !this.m_pearlComplete);
        }
    }

    public override void UpdateProgress(WingDbId wingDbId, bool normalComplete)
    {
        switch (wingDbId)
        {
            case WingDbId.LOE_TEMPLE_OF_ORSIS:
                this.m_rodComplete = normalComplete;
                break;

            case WingDbId.LOE_ULDAMAN:
                this.m_headComplete = normalComplete;
                break;

            case WingDbId.LOE_RUINED_CITY:
                this.m_pearlComplete = normalComplete;
                break;

            case WingDbId.LOE_HALL_OF_EXPLORERS:
                this.m_finalWingComplete = normalComplete;
                break;
        }
        this.UpdatePartVisibility();
    }

    [CompilerGenerated]
    private sealed class <PlayCompleteAnimationCoroutine>c__Iterator10 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>eventName;
        internal PlayMakerFSM <$>fsm;
        internal AdventureWingProgressDisplay.OnAnimationComplete <$>onAnimComplete;
        internal Option <$>seenOption;
        internal AdventureWingProgressDisplay_LOE <>f__this;
        internal FsmBool <animComplete>__0;
        internal string eventName;
        internal PlayMakerFSM fsm;
        internal AdventureWingProgressDisplay.OnAnimationComplete onAnimComplete;
        internal Option seenOption;

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
                    this.<animComplete>__0 = this.fsm.FsmVariables.FindFsmBool("AnimationComplete");
                    this.fsm.SendEvent(this.eventName);
                    this.<>f__this.m_animating = true;
                    if (this.<animComplete>__0 == null)
                    {
                        goto Label_008C;
                    }
                    break;

                case 1:
                    break;

                default:
                    goto Label_00D1;
            }
            if (!this.<animComplete>__0.Value)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
        Label_008C:
            this.<>f__this.m_animating = false;
            if (this.seenOption != Option.INVALID)
            {
                Options.Get().SetBool(this.seenOption, true);
            }
            if (this.onAnimComplete != null)
            {
                this.onAnimComplete();
            }
            this.$PC = -1;
        Label_00D1:
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

