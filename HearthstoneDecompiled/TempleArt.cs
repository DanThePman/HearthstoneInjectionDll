using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TempleArt : MonoBehaviour
{
    public List<Texture2D> m_portraits;
    public float m_portraitSwapDelay = 0.5f;
    public Spell m_portraitSwapSpell;

    public void DoPortraitSwap(Actor actor, int turn)
    {
        base.StartCoroutine(this.DoPortraitSwapWithTiming(actor, turn));
    }

    [DebuggerHidden]
    private IEnumerator DoPortraitSwapWithTiming(Actor actor, int turn)
    {
        return new <DoPortraitSwapWithTiming>c__Iterator26 { actor = actor, turn = turn, <$>actor = actor, <$>turn = turn, <>f__this = this };
    }

    private Texture2D GetArtForTurn(int turn)
    {
        return this.m_portraits[turn];
    }

    [CompilerGenerated]
    private sealed class <DoPortraitSwapWithTiming>c__Iterator26 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal int <$>turn;
        private static Spell.StateFinishedCallback <>f__am$cache8;
        internal TempleArt <>f__this;
        internal Spell <spellInstance>__0;
        internal Actor actor;
        internal int turn;

        private static void <>m__7A(Spell spell, SpellStateType prevStateType, object userData)
        {
            if (spell.GetActiveState() == SpellStateType.NONE)
            {
                UnityEngine.Object.Destroy(spell.gameObject);
            }
        }

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
                    if (this.actor != null)
                    {
                        if (this.<>f__this.m_portraitSwapSpell != null)
                        {
                            this.<spellInstance>__0 = UnityEngine.Object.Instantiate<Spell>(this.<>f__this.m_portraitSwapSpell);
                            this.<spellInstance>__0.transform.parent = this.actor.transform;
                            if (<>f__am$cache8 == null)
                            {
                                <>f__am$cache8 = new Spell.StateFinishedCallback(TempleArt.<DoPortraitSwapWithTiming>c__Iterator26.<>m__7A);
                            }
                            this.<spellInstance>__0.AddStateFinishedCallback(<>f__am$cache8);
                            this.<spellInstance>__0.SetSource(this.actor.gameObject);
                            this.<spellInstance>__0.Activate();
                            this.$current = new WaitForSeconds(this.<>f__this.m_portraitSwapDelay);
                            this.$PC = 1;
                            return true;
                        }
                        break;
                    }
                    goto Label_010C;

                case 1:
                    break;

                default:
                    goto Label_010C;
            }
            this.actor.SetPortraitTextureOverride(this.<>f__this.GetArtForTurn(this.turn));
            this.$PC = -1;
        Label_010C:
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

