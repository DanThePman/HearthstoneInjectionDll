using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GhostCardEffect : Spell
{
    public GameObject m_Glow;
    public GameObject m_GlowUnique;

    [DebuggerHidden]
    private IEnumerator GhostEffect(SpellStateType prevStateType)
    {
        return new <GhostEffect>c__Iterator212 { prevStateType = prevStateType, <$>prevStateType = prevStateType, <>f__this = this };
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        if (this.m_Glow != null)
        {
            this.m_Glow.GetComponent<Renderer>().enabled = false;
        }
        if (this.m_GlowUnique != null)
        {
            this.m_GlowUnique.GetComponent<Renderer>().enabled = false;
        }
        base.StartCoroutine(this.GhostEffect(prevStateType));
    }

    protected override void OnDeath(SpellStateType prevStateType)
    {
        if (this.m_Glow != null)
        {
            this.m_Glow.GetComponent<Renderer>().enabled = false;
        }
        if (this.m_GlowUnique != null)
        {
            this.m_GlowUnique.GetComponent<Renderer>().enabled = false;
        }
        base.OnDeath(prevStateType);
        this.OnSpellFinished();
    }

    [CompilerGenerated]
    private sealed class <GhostEffect>c__Iterator212 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SpellStateType <$>prevStateType;
        internal GhostCardEffect <>f__this;
        internal Actor <actor>__0;
        internal GhostCard <ghostCard>__1;
        internal GameObject <glow>__2;
        internal RenderToTexture <r2t>__3;
        internal SpellStateType prevStateType;

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
                    this.<actor>__0 = SceneUtils.FindComponentInParents<Actor>(this.<>f__this.gameObject);
                    if (this.<actor>__0 != null)
                    {
                        this.<ghostCard>__1 = this.<>f__this.gameObject.GetComponentInChildren<GhostCard>();
                        if (this.<ghostCard>__1 != null)
                        {
                            if (this.<>f__this.m_Glow != null)
                            {
                                this.<glow>__2 = this.<>f__this.m_Glow;
                                if (this.<actor>__0.IsElite() && (this.<>f__this.m_GlowUnique != null))
                                {
                                    this.<glow>__2 = this.<>f__this.m_GlowUnique;
                                }
                                this.<glow>__2.GetComponent<Renderer>().enabled = true;
                            }
                            KeywordHelpPanelManager.Get().HideKeywordHelp();
                            this.<ghostCard>__1.RenderGhostCard();
                            this.$current = new WaitForEndOfFrame();
                            this.$PC = 1;
                            return true;
                        }
                        UnityEngine.Debug.LogWarning("GhostCardEffect GhostCard is null");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("GhostCardEffect actor is null");
                    }
                    goto Label_01F2;

                case 1:
                    this.<r2t>__3 = this.<>f__this.gameObject.GetComponentInChildren<RenderToTexture>();
                    if (this.<r2t>__3 == null)
                    {
                        goto Label_019D;
                    }
                    if ((GraphicsManager.Get().RenderQualityLevel != GraphicsQuality.High) || (this.<actor>__0.GetCardFlair().Premium != TAG_PREMIUM.GOLDEN))
                    {
                        this.<r2t>__3.m_RealtimeRender = false;
                        break;
                    }
                    this.<r2t>__3.m_RealtimeRender = true;
                    break;

                default:
                    goto Label_01F2;
            }
            this.<r2t>__3.m_LateUpdate = true;
        Label_019D:
            this.<ghostCard>__1.RenderGhostCard(true);
            this.<actor>__0.Show();
            KeywordHelpPanelManager.Get().HideKeywordHelp();
            this.<r2t>__3.Render();
            this.<>f__this.OnBirth(this.prevStateType);
            this.<>f__this.OnSpellFinished();
            goto Label_01F2;
            this.$PC = -1;
        Label_01F2:
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

