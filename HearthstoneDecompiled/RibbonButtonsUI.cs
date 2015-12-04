using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RibbonButtonsUI : MonoBehaviour
{
    public PegUIElement m_collectionManagerRibbon;
    public float m_EaseInTime = 1f;
    public float m_EaseOutTime = 0.4f;
    public Transform m_LeftBones;
    public float m_minAspectRatioAdjustment = 0.24f;
    public UberText m_packCount;
    public GameObject m_packCountFrame;
    public PegUIElement m_packOpeningRibbon;
    public PegUIElement m_questLogRibbon;
    public List<RibbonButtonObject> m_Ribbons;
    public Transform m_RightBones;
    public GameObject m_rootObject;
    private bool m_shown = true;
    public PegUIElement m_storeRibbon;

    public void Awake()
    {
        this.m_rootObject.SetActive(false);
        float num = 1f - TransformUtil.PhoneAspectRatioScale();
        float num2 = num * this.m_minAspectRatioAdjustment;
        TransformUtil.SetLocalPosX(this.m_LeftBones, this.m_LeftBones.localPosition.x + num2);
        TransformUtil.SetLocalPosX(this.m_RightBones, this.m_RightBones.localPosition.x - num2);
    }

    [DebuggerHidden]
    private IEnumerator HideRibbons()
    {
        return new <HideRibbons>c__Iterator55 { <>f__this = this };
    }

    public void SetPackCount(int packs)
    {
        if (packs <= 0)
        {
            this.m_packCount.Text = string.Empty;
            this.m_packCountFrame.SetActive(false);
        }
        else
        {
            object[] args = new object[] { packs };
            this.m_packCount.Text = GameStrings.Format("GLUE_PACK_OPENING_BOOSTER_COUNT", args);
            this.m_packCountFrame.SetActive(true);
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowRibbons()
    {
        return new <ShowRibbons>c__Iterator54 { <>f__this = this };
    }

    public void Toggle(bool show)
    {
        this.m_shown = show;
        if (show)
        {
            base.StartCoroutine(this.ShowRibbons());
        }
        else
        {
            base.StartCoroutine(this.HideRibbons());
        }
    }

    [CompilerGenerated]
    private sealed class <HideRibbons>c__Iterator55 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<RibbonButtonsUI.RibbonButtonObject>.Enumerator <$s_448>__0;
        internal RibbonButtonsUI <>f__this;
        internal Hashtable <args>__2;
        internal RibbonButtonsUI.RibbonButtonObject <ribbon>__1;

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
                    this.<$s_448>__0 = this.<>f__this.m_Ribbons.GetEnumerator();
                    try
                    {
                        while (this.<$s_448>__0.MoveNext())
                        {
                            this.<ribbon>__1 = this.<$s_448>__0.Current;
                            this.<ribbon>__1.m_Ribbon.transform.position = this.<ribbon>__1.m_ShownBone.position;
                            iTween.Stop(this.<ribbon>__1.m_Ribbon.gameObject);
                            object[] args = new object[] { "position", this.<ribbon>__1.m_HiddenBone.position, "delay", 0f, "time", this.<>f__this.m_EaseOutTime, "easeType", iTween.EaseType.easeInOutBack };
                            this.<args>__2 = iTween.Hash(args);
                            iTween.MoveTo(this.<ribbon>__1.m_Ribbon.gameObject, this.<args>__2);
                        }
                    }
                    finally
                    {
                        this.<$s_448>__0.Dispose();
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_EaseOutTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (!this.<>f__this.m_shown)
                    {
                        this.<>f__this.m_rootObject.SetActive(false);
                    }
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

    [CompilerGenerated]
    private sealed class <ShowRibbons>c__Iterator54 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<RibbonButtonsUI.RibbonButtonObject>.Enumerator <$s_446>__1;
        internal List<RibbonButtonsUI.RibbonButtonObject>.Enumerator <$s_447>__3;
        internal RibbonButtonsUI <>f__this;
        internal Hashtable <args>__5;
        internal RibbonButtonsUI.RibbonButtonObject <ribbon>__2;
        internal RibbonButtonsUI.RibbonButtonObject <ribbon>__4;
        internal float <startDelay>__0;

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
                    this.<>f__this.m_rootObject.SetActive(false);
                    this.<startDelay>__0 = 1f;
                    this.<$s_446>__1 = this.<>f__this.m_Ribbons.GetEnumerator();
                    try
                    {
                        while (this.<$s_446>__1.MoveNext())
                        {
                            this.<ribbon>__2 = this.<$s_446>__1.Current;
                            if (this.<ribbon>__2.m_AnimateInDelay < this.<startDelay>__0)
                            {
                                this.<startDelay>__0 = this.<ribbon>__2.m_AnimateInDelay;
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_446>__1.Dispose();
                    }
                    this.$current = new WaitForSeconds(this.<startDelay>__0);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_rootObject.SetActive(true);
                    this.<$s_447>__3 = this.<>f__this.m_Ribbons.GetEnumerator();
                    try
                    {
                        while (this.<$s_447>__3.MoveNext())
                        {
                            this.<ribbon>__4 = this.<$s_447>__3.Current;
                            this.<ribbon>__4.m_Ribbon.transform.position = this.<ribbon>__4.m_HiddenBone.position;
                            iTween.Stop(this.<ribbon>__4.m_Ribbon.gameObject);
                            object[] args = new object[] { "position", this.<ribbon>__4.m_ShownBone.position, "delay", this.<ribbon>__4.m_AnimateInDelay - this.<startDelay>__0, "time", this.<>f__this.m_EaseInTime, "easeType", iTween.EaseType.easeOutBack };
                            this.<args>__5 = iTween.Hash(args);
                            iTween.MoveTo(this.<ribbon>__4.m_Ribbon.gameObject, this.<args>__5);
                        }
                    }
                    finally
                    {
                        this.<$s_447>__3.Dispose();
                    }
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

    [Serializable]
    public class RibbonButtonObject
    {
        public float m_AnimateInDelay;
        public Transform m_HiddenBone;
        public bool m_LeftSide;
        public PegUIElement m_Ribbon;
        public Transform m_ShownBone;
    }
}

