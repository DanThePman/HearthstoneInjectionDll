using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CardSoundSpell : Spell
{
    protected AudioSource m_activeAudioSource;
    public CardSoundData m_CardSoundData = new CardSoundData();
    protected bool m_forceDefaultAudioSource;

    [DebuggerHidden]
    protected IEnumerator DelayedPlay()
    {
        return new <DelayedPlay>c__Iterator1F6 { <>f__this = this };
    }

    public virtual AudioSource DetermineBestAudioSource()
    {
        return this.m_CardSoundData.m_AudioSource;
    }

    public void ForceDefaultAudioSource()
    {
        this.m_forceDefaultAudioSource = true;
    }

    public AudioSource GetActiveAudioSource()
    {
        return this.m_activeAudioSource;
    }

    public bool HasActiveAudioSource()
    {
        return (this.m_activeAudioSource != null);
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        base.OnBirth(prevStateType);
        this.Play();
    }

    protected override void OnNone(SpellStateType prevStateType)
    {
        base.OnNone(prevStateType);
        this.Stop();
    }

    protected virtual void Play()
    {
        this.Stop();
        this.m_activeAudioSource = !this.m_forceDefaultAudioSource ? this.DetermineBestAudioSource() : this.m_CardSoundData.m_AudioSource;
        if (this.m_activeAudioSource == null)
        {
            this.OnStateFinished();
        }
        else
        {
            base.StartCoroutine("DelayedPlay");
        }
    }

    protected virtual void PlayNow()
    {
        SoundManager.Get().Play(this.m_activeAudioSource);
        base.StartCoroutine("WaitForSourceThenFinishState");
    }

    protected virtual void Stop()
    {
        base.StopCoroutine("DelayedPlay");
        base.StopCoroutine("WaitForSourceThenFinishState");
        SoundManager.Get().Stop(this.m_activeAudioSource);
    }

    [DebuggerHidden]
    protected IEnumerator WaitForSourceThenFinishState()
    {
        return new <WaitForSourceThenFinishState>c__Iterator1F7 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <DelayedPlay>c__Iterator1F6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardSoundSpell <>f__this;

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
                    if (this.<>f__this.m_CardSoundData.m_DelaySec <= 0f)
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_CardSoundData.m_DelaySec);
                    this.$PC = 1;
                    return true;

                case 1:
                    break;

                default:
                    goto Label_0074;
            }
            this.<>f__this.PlayNow();
            this.$PC = -1;
        Label_0074:
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
    private sealed class <WaitForSourceThenFinishState>c__Iterator1F7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardSoundSpell <>f__this;

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
                    this.<>f__this.OnStateFinished();
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

