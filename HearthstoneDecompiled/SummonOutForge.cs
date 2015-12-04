using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SummonOutForge : SpellImpl
{
    private static Color COMMON_COLOR = new Color(0.7333333f, 0.8235294f, 1f);
    private static Color EPIC_COLOR = new Color(0.5450981f, 0.2313726f, 1f);
    private static Color LEGENDARY_COLOR = new Color(1f, 0.6666667f, 0.2f);
    public GameObject m_burstMotes;
    public GameObject m_scryLines;
    public Material m_scryLinesMaterial;
    private static Color RARE_COLOR = new Color(0.2f, 0.4745098f, 1f);

    [DebuggerHidden]
    private IEnumerator BirthState()
    {
        return new <BirthState>c__Iterator229 { <>f__this = this };
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        base.StartCoroutine(this.BirthState());
    }

    [CompilerGenerated]
    private sealed class <BirthState>c__Iterator229 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SummonOutForge <>f__this;
        internal TAG_RARITY <rarity>__0;

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
                    this.<>f__this.SetActorVisibility(true, false);
                    this.<>f__this.SetVisibility(this.<>f__this.m_scryLines, true);
                    this.<rarity>__0 = this.<>f__this.m_actor.GetRarity();
                    switch (this.<rarity>__0)
                    {
                        case TAG_RARITY.RARE:
                            this.<>f__this.m_scryLinesMaterial.SetColor("_TintColor", SummonOutForge.RARE_COLOR);
                            this.<>f__this.m_scryLines.GetComponent<Renderer>().material.SetColor("_TintColor", SummonOutForge.RARE_COLOR);
                            goto Label_019A;

                        case TAG_RARITY.EPIC:
                            this.<>f__this.m_scryLinesMaterial.SetColor("_TintColor", SummonOutForge.EPIC_COLOR);
                            this.<>f__this.m_scryLines.GetComponent<Renderer>().material.SetColor("_TintColor", SummonOutForge.EPIC_COLOR);
                            goto Label_019A;

                        case TAG_RARITY.LEGENDARY:
                            this.<>f__this.m_scryLinesMaterial.SetColor("_TintColor", SummonOutForge.LEGENDARY_COLOR);
                            this.<>f__this.m_scryLines.GetComponent<Renderer>().material.SetColor("_TintColor", SummonOutForge.LEGENDARY_COLOR);
                            goto Label_019A;
                    }
                    break;

                case 1:
                    this.<>f__this.SetVisibilityRecursive(this.<>f__this.m_rootObject, false);
                    this.<>f__this.OnSpellFinished();
                    this.$PC = -1;
                    goto Label_0217;

                default:
                    goto Label_0217;
            }
            this.<>f__this.m_scryLinesMaterial.SetColor("_TintColor", SummonOutForge.COMMON_COLOR);
            this.<>f__this.m_scryLines.GetComponent<Renderer>().material.SetColor("_TintColor", SummonOutForge.COMMON_COLOR);
        Label_019A:
            this.<>f__this.PlayAnimation(this.<>f__this.m_scryLines, "AllyInHandScryLines_ForgeOut", PlayMode.StopAll, 0f);
            this.<>f__this.PlayParticles(this.<>f__this.m_burstMotes, false);
            this.$current = new WaitForSeconds(0.16f);
            this.$PC = 1;
            return true;
        Label_0217:
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

