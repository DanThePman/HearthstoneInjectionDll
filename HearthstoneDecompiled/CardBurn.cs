using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CardBurn : Spell
{
    public string m_BurnCardAnim = "CardBurnUpFire";
    public GameObject m_BurnCardQuad;
    public ParticleSystem m_EdgeEmbers;

    [DebuggerHidden]
    private IEnumerator BirthAction()
    {
        return new <BirthAction>c__Iterator205 { <>f__this = this };
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        base.StartCoroutine(this.BirthAction());
    }

    [CompilerGenerated]
    private sealed class <BirthAction>c__Iterator205 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardBurn <>f__this;
        internal Actor <actor>__0;

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
                    if (this.<>f__this.m_BurnCardQuad != null)
                    {
                        this.<>f__this.m_BurnCardQuad.GetComponent<Renderer>().enabled = true;
                        this.<>f__this.m_BurnCardQuad.GetComponent<Animation>().Play(this.<>f__this.m_BurnCardAnim, PlayMode.StopAll);
                    }
                    if (this.<>f__this.m_EdgeEmbers != null)
                    {
                        this.<>f__this.m_EdgeEmbers.Play();
                    }
                    this.$current = new WaitForSeconds(0.15f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<actor>__0 = SceneUtils.FindComponentInThisOrParents<Actor>(this.<>f__this.gameObject);
                    if (this.<actor>__0 != null)
                    {
                        this.<actor>__0.Hide();
                        this.<>f__this.OnSpellFinished();
                        break;
                    }
                    break;

                default:
                    break;
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

