using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MineCartRushArt : MonoBehaviour
{
    public List<Texture2D> m_portraits = new List<Texture2D>();
    public float m_portraitSwapDelay = 0.5f;
    public Spell m_portraitSwapSpell;

    public void DoPortraitSwap(Actor actor)
    {
        base.StartCoroutine(this.DoPortraitSwapWithTiming(actor));
    }

    [DebuggerHidden]
    private IEnumerator DoPortraitSwapWithTiming(Actor actor)
    {
        return new <DoPortraitSwapWithTiming>c__Iterator27 { actor = actor, <$>actor = actor, <>f__this = this };
    }

    private Texture2D GetNextPortrait()
    {
        if (this.m_portraits.Count == 0)
        {
            return null;
        }
        if (this.m_portraits.Count != 1)
        {
            Texture2D textured = this.m_portraits[0];
            int num = UnityEngine.Random.Range(1, this.m_portraits.Count);
            this.m_portraits[0] = this.m_portraits[num];
            this.m_portraits[num] = textured;
        }
        return this.m_portraits[0];
    }

    [CompilerGenerated]
    private sealed class <DoPortraitSwapWithTiming>c__Iterator27 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        private static Spell.StateFinishedCallback <>f__am$cache6;
        internal MineCartRushArt <>f__this;
        internal Spell <spellInstance>__0;
        internal Actor actor;

        private static void <>m__7B(Spell spell, SpellStateType prevStateType, object userData)
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
                            if (<>f__am$cache6 == null)
                            {
                                <>f__am$cache6 = new Spell.StateFinishedCallback(MineCartRushArt.<DoPortraitSwapWithTiming>c__Iterator27.<>m__7B);
                            }
                            this.<spellInstance>__0.AddStateFinishedCallback(<>f__am$cache6);
                            this.<spellInstance>__0.SetSource(this.actor.gameObject);
                            this.<spellInstance>__0.Activate();
                            this.$current = new WaitForSeconds(this.<>f__this.m_portraitSwapDelay);
                            this.$PC = 1;
                            return true;
                        }
                        break;
                    }
                    goto Label_0106;

                case 1:
                    break;

                default:
                    goto Label_0106;
            }
            this.actor.SetPortraitTextureOverride(this.<>f__this.GetNextPortrait());
            this.$PC = -1;
        Label_0106:
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

