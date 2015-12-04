using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TavernBrawlMenuButton : BoxMenuButton
{
    private bool isPoppedUp;
    public float m_hoverDelay = 0.5f;
    public UberText m_returnsInfo;

    public void ClearHighlightAndTooltip()
    {
        base.TriggerOut();
    }

    [DebuggerHidden]
    public IEnumerator DoPopup()
    {
        return new <DoPopup>c__Iterator1E7 { <>f__this = this };
    }

    public override void TriggerOut()
    {
        if ((!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.TavernBrawl || !TavernBrawlManager.Get().HasUnlockedTavernBrawl) || TavernBrawlManager.Get().IsTavernBrawlActive)
        {
            base.TriggerOut();
        }
        else
        {
            if (!UniversalInputManager.Get().IsTouchMode())
            {
                base.StopCoroutine("DoPopup");
            }
            if (this.isPoppedUp)
            {
                if (Box.Get().m_tavernBrawlPopdownSound != string.Empty)
                {
                    SoundManager.Get().LoadAndPlay(Box.Get().m_tavernBrawlPopdownSound);
                }
                Box.Get().m_TavernBrawlButtonVisual.GetComponent<Animator>().Play("TavernBrawl_ButtonPopdown");
                this.isPoppedUp = false;
            }
        }
    }

    public override void TriggerOver()
    {
        if ((!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.TavernBrawl || !TavernBrawlManager.Get().HasUnlockedTavernBrawl) || TavernBrawlManager.Get().IsTavernBrawlActive)
        {
            base.TriggerOver();
        }
        else
        {
            this.UpdateTimeText();
            base.StartCoroutine("DoPopup");
        }
    }

    public override void TriggerPress()
    {
        if ((NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.TavernBrawl && TavernBrawlManager.Get().HasUnlockedTavernBrawl) && TavernBrawlManager.Get().IsTavernBrawlActive)
        {
            base.TriggerPress();
        }
    }

    public override void TriggerRelease()
    {
        if ((NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.TavernBrawl && TavernBrawlManager.Get().HasUnlockedTavernBrawl) && TavernBrawlManager.Get().IsTavernBrawlActive)
        {
            base.TriggerRelease();
        }
    }

    private void UpdateTimeText()
    {
        int nextTavernBrawlSeasonStart = TavernBrawlManager.Get().NextTavernBrawlSeasonStart;
        if (nextTavernBrawlSeasonStart < 0)
        {
            this.m_returnsInfo.Text = GameStrings.Get("GLUE_TAVERN_BRAWL_RETURNS_UNKNOWN");
        }
        else
        {
            TimeUtils.ElapsedStringSet stringSet = new TimeUtils.ElapsedStringSet {
                m_seconds = "GLUE_TAVERN_BRAWL_RETURNS_LESS_THAN_1_HOUR",
                m_minutes = "GLUE_TAVERN_BRAWL_RETURNS_LESS_THAN_1_HOUR",
                m_hours = "GLUE_TAVERN_BRAWL_RETURNS_HOURS",
                m_days = "GLUE_TAVERN_BRAWL_RETURNS_DAYS",
                m_weeks = "GLUE_TAVERN_BRAWL_RETURNS_WEEKS",
                m_monthAgo = "GLUE_TAVERN_BRAWL_RETURNS_OVER_1_MONTH"
            };
            string elapsedTimeString = TimeUtils.GetElapsedTimeString(nextTavernBrawlSeasonStart, stringSet);
            this.m_returnsInfo.Text = elapsedTimeString;
        }
    }

    [CompilerGenerated]
    private sealed class <DoPopup>c__Iterator1E7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TavernBrawlMenuButton <>f__this;
        internal Animator <tbAnim>__0;

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
                    if (UniversalInputManager.Get().IsTouchMode())
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_hoverDelay);
                    this.$PC = 1;
                    goto Label_00D0;

                case 1:
                    break;

                case 2:
                    this.$PC = -1;
                    goto Label_00CE;

                default:
                    goto Label_00CE;
            }
            this.<>f__this.isPoppedUp = true;
            if (Box.Get().m_tavernBrawlPopupSound != string.Empty)
            {
                SoundManager.Get().LoadAndPlay(Box.Get().m_tavernBrawlPopupSound);
            }
            this.<tbAnim>__0 = Box.Get().m_TavernBrawlButtonVisual.GetComponent<Animator>();
            this.<tbAnim>__0.Play("TavernBrawl_ButtonPopup");
            this.$current = null;
            this.$PC = 2;
            goto Label_00D0;
        Label_00CE:
            return false;
        Label_00D0:
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
}

