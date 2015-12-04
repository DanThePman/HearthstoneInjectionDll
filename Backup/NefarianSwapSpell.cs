using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NefarianSwapSpell : HeroSwapSpell
{
    private Card m_obsoleteHeroCard;
    public float m_obsoleteRemovalDelay;

    public override bool AddPowerTargets()
    {
        if (!base.AddPowerTargets())
        {
            return false;
        }
        int tag = base.m_oldHeroCard.GetEntity().GetTag(GAME_TAG.LINKEDCARD);
        if (tag != 0)
        {
            this.m_obsoleteHeroCard = GameState.Get().GetEntity(tag).GetCard();
        }
        if (this.m_obsoleteHeroCard == null)
        {
            return false;
        }
        return true;
    }

    public override void CustomizeFXProcess(Actor heroActor)
    {
        if (heroActor == base.m_newHeroCard.GetActor())
        {
            base.StartCoroutine(this.DestroyObsolete());
        }
    }

    [DebuggerHidden]
    private IEnumerator DestroyObsolete()
    {
        return new <DestroyObsolete>c__Iterator21D { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <DestroyObsolete>c__Iterator21D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal NefarianSwapSpell <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.m_obsoleteRemovalDelay);
                    this.$PC = 1;
                    return true;

                case 1:
                    UnityEngine.Object.Destroy(this.<>f__this.m_obsoleteHeroCard.GetActor().gameObject);
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

