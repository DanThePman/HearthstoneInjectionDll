using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SummonInForge : SpellImpl
{
    public GameObject m_blackBits;
    public GameObject m_burnIn;
    public float m_burnInAnimationSpeed = 1f;
    public bool m_isHeroActor;
    public GameObject m_smokePuff;

    [DebuggerHidden]
    private IEnumerator BirthState()
    {
        return new <BirthState>c__Iterator228 { <>f__this = this };
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        base.StartCoroutine(this.BirthState());
    }

    [CompilerGenerated]
    private sealed class <BirthState>c__Iterator228 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SummonInForge <>f__this;
        internal GameObject <attackObject>__0;
        internal GameObject <healthObject>__1;

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
                    this.<>f__this.SetActorVisibility(false, true);
                    this.<>f__this.SetVisibility(this.<>f__this.m_burnIn, true);
                    this.<>f__this.SetAnimationSpeed(this.<>f__this.m_burnIn, "AllyInHandScryLines_Forge", this.<>f__this.m_burnInAnimationSpeed);
                    this.<>f__this.PlayAnimation(this.<>f__this.m_burnIn, "AllyInHandScryLines_Forge", PlayMode.StopAll, 0f);
                    this.<>f__this.PlayParticles(this.<>f__this.m_smokePuff, false);
                    this.<>f__this.PlayParticles(this.<>f__this.m_blackBits, false);
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 1;
                    goto Label_0182;

                case 1:
                    this.<>f__this.SetActorVisibility(true, true);
                    if (this.<>f__this.m_isHeroActor)
                    {
                        this.<attackObject>__0 = this.<>f__this.GetActorObject("AttackObject");
                        this.<healthObject>__1 = this.<>f__this.GetActorObject("HealthObject");
                        this.<>f__this.SetVisibilityRecursive(this.<attackObject>__0, false);
                        this.<>f__this.SetVisibilityRecursive(this.<healthObject>__1, false);
                    }
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 2;
                    goto Label_0182;

                case 2:
                    this.<>f__this.OnSpellFinished();
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0182:
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

