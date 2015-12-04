using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CardBurstLegendary : Spell
{
    public ParticleSystem m_Bang;
    public GameObject m_EdgeGlow;
    public string m_EdgeGlowBirthAnimation = "StandardEdgeGlowFade_Forge";
    public string m_EdgeGlowDeathAnimation = "StandardEdgeGlowFadeOut_Forge";
    public GameObject m_RaysMask;
    public GameObject m_RenderPlane;
    public ParticleSystem m_Shockwave;

    [DebuggerHidden]
    private IEnumerator DeathState()
    {
        return new <DeathState>c__Iterator207 { <>f__this = this };
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        if (this.m_RenderPlane != null)
        {
            this.m_RenderPlane.SetActive(true);
        }
        if (this.m_RaysMask != null)
        {
            this.m_RaysMask.SetActive(true);
        }
        if (this.m_EdgeGlow != null)
        {
            this.m_EdgeGlow.GetComponent<Renderer>().enabled = true;
            this.m_EdgeGlow.GetComponent<Animation>().Play(this.m_EdgeGlowBirthAnimation, PlayMode.StopAll);
        }
        if (this.m_Shockwave != null)
        {
            this.m_Shockwave.Play();
        }
        if (this.m_Bang != null)
        {
            this.m_Bang.Play();
        }
        this.OnSpellFinished();
    }

    protected override void OnDeath(SpellStateType prevStateType)
    {
        if (this.m_EdgeGlow != null)
        {
            this.m_EdgeGlow.GetComponent<Animation>().Play(this.m_EdgeGlowDeathAnimation, PlayMode.StopAll);
        }
        base.StartCoroutine(this.DeathState());
    }

    [CompilerGenerated]
    private sealed class <DeathState>c__Iterator207 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardBurstLegendary <>f__this;

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
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.<>f__this.m_EdgeGlow != null)
                    {
                        this.<>f__this.m_EdgeGlow.GetComponent<Renderer>().enabled = false;
                    }
                    this.<>f__this.OnSpellFinished();
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

