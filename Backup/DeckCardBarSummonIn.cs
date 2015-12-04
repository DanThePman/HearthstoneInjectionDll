using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DeckCardBarSummonIn : SpellImpl
{
    public GameObject m_echoQuad;
    public GameObject m_fxEvaporate;

    [DebuggerHidden]
    private IEnumerator BirthState()
    {
        return new <BirthState>c__Iterator20A { <>f__this = this };
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        base.StartCoroutine(this.BirthState());
    }

    private void OnDisable()
    {
        this.m_echoQuad.GetComponent<Renderer>().material.color = Color.clear;
        this.m_fxEvaporate.GetComponent<ParticleSystem>().Clear();
    }

    [CompilerGenerated]
    private sealed class <BirthState>c__Iterator20A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DeckCardBarSummonIn <>f__this;
        internal GameObject <frame>__0;

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
                    this.<>f__this.InitActorVariables();
                    this.<frame>__0 = this.<>f__this.GetActorObject("Frame");
                    this.<>f__this.SetVisibilityRecursive(this.<frame>__0, false);
                    this.<>f__this.SetVisibility(this.<>f__this.m_echoQuad, true);
                    this.<>f__this.SetVisibilityRecursive(this.<frame>__0, true);
                    this.<>f__this.PlayParticles(this.<>f__this.m_fxEvaporate, false);
                    this.<>f__this.SetAnimationSpeed(this.<>f__this.m_echoQuad, "Secret_AbilityEchoFade", 0.5f);
                    this.<>f__this.PlayAnimation(this.<>f__this.m_echoQuad, "Secret_AbilityEchoFade", PlayMode.StopAll, 0f);
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.OnSpellFinished();
                    this.<>f__this.SetVisibility(this.<>f__this.m_echoQuad, false);
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

