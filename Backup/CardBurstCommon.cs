using System;
using UnityEngine;

public class CardBurstCommon : Spell
{
    public ParticleSystem m_BurstMotes;
    public GameObject m_EdgeGlow;

    protected override void OnBirth(SpellStateType prevStateType)
    {
        if (this.m_BurstMotes != null)
        {
            this.m_BurstMotes.Play();
        }
        if (this.m_EdgeGlow != null)
        {
            this.m_EdgeGlow.GetComponent<Renderer>().enabled = true;
        }
        this.OnSpellFinished();
    }
}

