using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class LOE_DeckTakeEvent : MonoBehaviour
{
    private bool m_animIsPlaying;
    public Renderer m_friendlyDeckRenderer;
    public Animator m_replacementDeckAnimator;
    public string m_replacementDeckAnimName = "CardsToPlayerDeck";
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_replacementDeckSoundPrefab;
    public Animator m_takeDeckAnimator;
    public string m_takeDeckAnimName = "LOE_TakeDeck";
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_takeDeckSoundPrefab;

    public bool AnimIsPlaying()
    {
        return this.m_animIsPlaying;
    }

    [DebuggerHidden]
    public IEnumerator PlayReplacementDeckAnim()
    {
        return new <PlayReplacementDeckAnim>c__Iterator153 { <>f__this = this };
    }

    [DebuggerHidden]
    public IEnumerator PlayTakeDeckAnim()
    {
        return new <PlayTakeDeckAnim>c__Iterator152 { <>f__this = this };
    }

    private void Start()
    {
        CardBackManager.Get().SetCardBackTexture(this.m_friendlyDeckRenderer, 0, true);
    }

    [CompilerGenerated]
    private sealed class <PlayReplacementDeckAnim>c__Iterator153 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal LOE_DeckTakeEvent <>f__this;
        internal float <animDuration>__2;
        internal string <replacementDeckSoundName>__0;
        internal AnimatorStateInfo <state>__1;

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
                    this.<>f__this.m_animIsPlaying = true;
                    this.<>f__this.m_replacementDeckAnimator.enabled = true;
                    this.<>f__this.m_replacementDeckAnimator.Play(this.<>f__this.m_replacementDeckAnimName);
                    if (!string.IsNullOrEmpty(this.<>f__this.m_replacementDeckSoundPrefab))
                    {
                        this.<replacementDeckSoundName>__0 = FileUtils.GameAssetPathToName(this.<>f__this.m_replacementDeckSoundPrefab);
                        SoundManager.Get().LoadAndPlay(this.<replacementDeckSoundName>__0);
                    }
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    goto Label_0133;

                case 1:
                    this.<state>__1 = this.<>f__this.m_replacementDeckAnimator.GetCurrentAnimatorStateInfo(0);
                    this.<animDuration>__2 = this.<state>__1.length;
                    Log.JMac.Print("Take Deck anim duration: " + this.<animDuration>__2, new object[0]);
                    this.$current = new WaitForSeconds(this.<animDuration>__2);
                    this.$PC = 2;
                    goto Label_0133;

                case 2:
                    this.<>f__this.m_animIsPlaying = false;
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0133:
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
    private sealed class <PlayTakeDeckAnim>c__Iterator152 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal LOE_DeckTakeEvent <>f__this;
        internal float <animDuration>__2;
        internal AnimatorStateInfo <state>__1;
        internal string <takeDeckSoundName>__0;

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
                    this.<>f__this.m_animIsPlaying = true;
                    this.<>f__this.m_takeDeckAnimator.enabled = true;
                    this.<>f__this.m_takeDeckAnimator.Play(this.<>f__this.m_takeDeckAnimName);
                    if (!string.IsNullOrEmpty(this.<>f__this.m_takeDeckSoundPrefab))
                    {
                        this.<takeDeckSoundName>__0 = FileUtils.GameAssetPathToName(this.<>f__this.m_takeDeckSoundPrefab);
                        SoundManager.Get().LoadAndPlay(this.<takeDeckSoundName>__0);
                    }
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    goto Label_0133;

                case 1:
                    this.<state>__1 = this.<>f__this.m_takeDeckAnimator.GetCurrentAnimatorStateInfo(0);
                    this.<animDuration>__2 = this.<state>__1.length;
                    Log.JMac.Print("Take Deck anim duration: " + this.<animDuration>__2, new object[0]);
                    this.$current = new WaitForSeconds(this.<animDuration>__2);
                    this.$PC = 2;
                    goto Label_0133;

                case 2:
                    this.<>f__this.m_animIsPlaying = false;
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0133:
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

