using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DeckCardBarSummonInForge : SpellImpl
{
    private static Color COMMON_COLOR = new Color(1f, 1f, 1f);
    private static Color COMMON_TINT_COLOR = new Color(0.9215686f, 0.945098f, 1f);
    private static Color EPIC_COLOR = new Color(0.4156863f, 0.1647059f, 1f);
    private static Color EPIC_TINT_COLOR = new Color(0.4156863f, 0.1647059f, 0.9921569f);
    private static Color LEGENDARY_COLOR = new Color(0.7686275f, 0.5411765f, 0.1490196f);
    private static Color LEGENDARY_TINT_COLOR = new Color(0.6666667f, 0.4745098f, 0.1294118f);
    public GameObject m_echoQuad;
    public Material m_echoQuadMaterial;
    public GameObject m_fxEvaporate;
    public Material m_fxEvaporateMaterial;
    private static Color RARE_COLOR = new Color(0.1647059f, 0.4078431f, 1f);
    private static Color RARE_TINT_COLOR = new Color(0.1647059f, 0.4078431f, 1f);

    [DebuggerHidden]
    private IEnumerator BirthState()
    {
        return new <BirthState>c__Iterator20B { <>f__this = this };
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        base.StartCoroutine(this.BirthState());
    }

    [CompilerGenerated]
    private sealed class <BirthState>c__Iterator20B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DeckCardBarSummonInForge <>f__this;
        internal Material <echoMaterial>__0;
        internal TAG_RARITY <rarity>__1;

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
                    this.<>f__this.SetAnimationTime(this.<>f__this.m_echoQuad, "Secret_AbilityEchoOut_Forge", 0f);
                    this.<>f__this.SetVisibility(this.<>f__this.m_echoQuad, true);
                    this.<echoMaterial>__0 = this.<>f__this.GetMaterial(this.<>f__this.m_echoQuad, this.<>f__this.m_echoQuadMaterial, false, 0);
                    this.<rarity>__1 = this.<>f__this.m_actor.GetRarity();
                    switch (this.<rarity>__1)
                    {
                        case TAG_RARITY.RARE:
                            this.<>f__this.SetMaterialColor(this.<>f__this.m_echoQuad, this.<echoMaterial>__0, "_Color", DeckCardBarSummonInForge.RARE_COLOR, 0);
                            this.<>f__this.SetMaterialColor(this.<>f__this.m_fxEvaporate, this.<>f__this.m_fxEvaporateMaterial, "_TintColor", DeckCardBarSummonInForge.RARE_TINT_COLOR, 0);
                            goto Label_022A;

                        case TAG_RARITY.EPIC:
                            this.<>f__this.SetMaterialColor(this.<>f__this.m_echoQuad, this.<echoMaterial>__0, "_Color", DeckCardBarSummonInForge.EPIC_COLOR, 0);
                            this.<>f__this.SetMaterialColor(this.<>f__this.m_fxEvaporate, this.<>f__this.m_fxEvaporateMaterial, "_TintColor", DeckCardBarSummonInForge.EPIC_TINT_COLOR, 0);
                            goto Label_022A;

                        case TAG_RARITY.LEGENDARY:
                            this.<>f__this.SetMaterialColor(this.<>f__this.m_echoQuad, this.<echoMaterial>__0, "_Color", DeckCardBarSummonInForge.LEGENDARY_COLOR, 0);
                            this.<>f__this.SetMaterialColor(this.<>f__this.m_fxEvaporate, this.<>f__this.m_fxEvaporateMaterial, "_TintColor", DeckCardBarSummonInForge.LEGENDARY_TINT_COLOR, 0);
                            goto Label_022A;
                    }
                    break;

                case 1:
                    this.<>f__this.SetVisibility(this.<>f__this.m_echoQuad, false);
                    this.$PC = -1;
                    goto Label_02D4;

                default:
                    goto Label_02D4;
            }
            this.<>f__this.SetMaterialColor(this.<>f__this.m_echoQuad, this.<echoMaterial>__0, "_Color", DeckCardBarSummonInForge.COMMON_COLOR, 0);
            this.<>f__this.SetMaterialColor(this.<>f__this.m_fxEvaporate, this.<>f__this.m_fxEvaporateMaterial, "_TintColor", DeckCardBarSummonInForge.COMMON_TINT_COLOR, 0);
        Label_022A:
            this.<>f__this.SetActorVisibility(true, true);
            this.<>f__this.PlayParticles(this.<>f__this.m_fxEvaporate, false);
            this.<>f__this.SetAnimationSpeed(this.<>f__this.m_echoQuad, "Secret_AbilityEchoOut_Forge", 0.2f);
            this.<>f__this.PlayAnimation(this.<>f__this.m_echoQuad, "Secret_AbilityEchoOut_Forge", PlayMode.StopAll, 0f);
            this.<>f__this.OnSpellFinished();
            this.$current = new WaitForSeconds(1f);
            this.$PC = 1;
            return true;
        Label_02D4:
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

