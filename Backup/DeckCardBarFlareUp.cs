using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DeckCardBarFlareUp : SpellImpl
{
    public GameObject m_fuseQuad;
    public GameObject m_fxSparks;

    [DebuggerHidden]
    private IEnumerator BirthState()
    {
        return new <BirthState>c__Iterator209 { <>f__this = this };
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        if (base.gameObject.activeSelf)
        {
            base.StartCoroutine(this.BirthState());
        }
    }

    [CompilerGenerated]
    private sealed class <BirthState>c__Iterator209 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DeckCardBarFlareUp <>f__this;

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
                    this.<>f__this.SetVisibility(this.<>f__this.m_fuseQuad, true);
                    this.<>f__this.PlayParticles(this.<>f__this.m_fxSparks, false);
                    this.<>f__this.PlayAnimation(this.<>f__this.m_fuseQuad, "DeckCardBar_FuseInOut", PlayMode.StopAll, 0f);
                    this.<>f__this.OnSpellFinished();
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.SetVisibility(this.<>f__this.m_fuseQuad, false);
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

