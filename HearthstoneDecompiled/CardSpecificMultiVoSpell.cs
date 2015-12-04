using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CardSpecificMultiVoSpell : CardSoundSpell
{
    private int m_ActiveAudioIndex;
    public CardSpecificMultiVoData m_CardSpecificVoData = new CardSpecificMultiVoData();
    private bool m_SpecificCardFound;

    [DebuggerHidden]
    protected IEnumerator DelayedPlayMulti()
    {
        return new <DelayedPlayMulti>c__Iterator1F8 { <>f__this = this };
    }

    public override AudioSource DetermineBestAudioSource()
    {
        if (!this.m_SpecificCardFound)
        {
            return base.DetermineBestAudioSource();
        }
        if (this.m_ActiveAudioIndex < this.m_CardSpecificVoData.m_Lines.Length)
        {
            return this.m_CardSpecificVoData.m_Lines[this.m_ActiveAudioIndex].m_AudioSource;
        }
        return null;
    }

    private bool IsCardInZones(List<Zone> zones)
    {
        if (zones != null)
        {
            foreach (Zone zone in zones)
            {
                foreach (Card card in zone.GetCards())
                {
                    if (card.GetEntity().GetCardId() == this.m_CardSpecificVoData.m_CardId)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    protected override void Play()
    {
        if (!base.m_forceDefaultAudioSource)
        {
            this.m_SpecificCardFound = this.SearchForCard();
        }
        if (this.m_SpecificCardFound)
        {
            this.Stop();
            this.m_ActiveAudioIndex = 0;
            base.m_activeAudioSource = !base.m_forceDefaultAudioSource ? this.DetermineBestAudioSource() : base.m_CardSoundData.m_AudioSource;
            if (base.m_activeAudioSource == null)
            {
                this.OnStateFinished();
            }
            else
            {
                base.StartCoroutine("DelayedPlayMulti");
            }
        }
        else
        {
            base.Play();
        }
    }

    protected virtual void PlayNowMulti()
    {
        SoundManager.Get().Play(base.m_activeAudioSource);
        base.StartCoroutine("WaitForSourceThenContinue");
    }

    private bool SearchForCard()
    {
        if (!string.IsNullOrEmpty(this.m_CardSpecificVoData.m_CardId))
        {
            foreach (SpellZoneTag tag in this.m_CardSpecificVoData.m_ZonesToSearch)
            {
                List<Zone> zones = SpellUtils.FindZonesFromTag(this, tag, this.m_CardSpecificVoData.m_SideToSearch);
                if (this.IsCardInZones(zones))
                {
                    return true;
                }
            }
        }
        return false;
    }

    protected override void Stop()
    {
        base.StopCoroutine("WaitForSourceThenContinue");
        base.Stop();
    }

    [DebuggerHidden]
    protected IEnumerator WaitForSourceThenContinue()
    {
        return new <WaitForSourceThenContinue>c__Iterator1F9 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <DelayedPlayMulti>c__Iterator1F8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardSpecificMultiVoSpell <>f__this;
        internal float <delaySec>__0;

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
                    this.<delaySec>__0 = this.<>f__this.m_CardSpecificVoData.m_Lines[this.<>f__this.m_ActiveAudioIndex].m_DelaySec;
                    if (this.<delaySec>__0 <= 0f)
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(this.<delaySec>__0);
                    this.$PC = 1;
                    return true;

                case 1:
                    break;

                default:
                    goto Label_0087;
            }
            this.<>f__this.PlayNowMulti();
            this.$PC = -1;
        Label_0087:
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
    private sealed class <WaitForSourceThenContinue>c__Iterator1F9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardSpecificMultiVoSpell <>f__this;

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
                    if (SoundManager.Get().IsActive(this.<>f__this.m_activeAudioSource))
                    {
                        this.$current = 0;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.m_ActiveAudioIndex++;
                    this.<>f__this.m_activeAudioSource = this.<>f__this.DetermineBestAudioSource();
                    if (this.<>f__this.m_activeAudioSource != null)
                    {
                        this.<>f__this.StartCoroutine("DelayedPlayMulti");
                    }
                    else
                    {
                        this.<>f__this.OnStateFinished();
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
}

