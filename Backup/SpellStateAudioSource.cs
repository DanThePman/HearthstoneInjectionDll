using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class SpellStateAudioSource
{
    public AudioSource m_AudioSource;
    public string m_Comment;
    public bool m_Enabled = true;
    public bool m_PlayGlobally;
    public float m_StartDelaySec;
    public bool m_StopOnStateChange;

    [DebuggerHidden]
    private IEnumerator DelayedPlay()
    {
        return new <DelayedPlay>c__Iterator26C { <>f__this = this };
    }

    public void Init()
    {
        if (this.m_AudioSource != null)
        {
            this.m_AudioSource.playOnAwake = false;
        }
    }

    public void Play(SpellState parent)
    {
        if (this.m_Enabled)
        {
            if (object.Equals(this.m_StartDelaySec, 0f))
            {
                this.PlayNow();
            }
            else
            {
                parent.StartCoroutine(this.DelayedPlay());
            }
        }
    }

    private void PlayNow()
    {
        if (this.m_AudioSource != null)
        {
            if (this.m_PlayGlobally)
            {
                SoundPlayClipArgs args = new SoundPlayClipArgs {
                    m_clip = this.m_AudioSource.clip,
                    m_volume = new float?(this.m_AudioSource.volume),
                    m_pitch = new float?(this.m_AudioSource.pitch),
                    m_category = new SoundCategory?(SoundManager.Get().GetCategory(this.m_AudioSource)),
                    m_parentObject = this.m_AudioSource.gameObject
                };
                SoundManager.Get().PlayClip(args);
            }
            else
            {
                SoundManager.Get().Play(this.m_AudioSource);
            }
        }
    }

    public void Stop()
    {
        if (((this.m_Enabled && (this.m_AudioSource != null)) && !this.m_PlayGlobally) && this.m_StopOnStateChange)
        {
            this.m_AudioSource.Stop();
        }
    }

    [CompilerGenerated]
    private sealed class <DelayedPlay>c__Iterator26C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SpellStateAudioSource <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.m_StartDelaySec);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.PlayNow();
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

