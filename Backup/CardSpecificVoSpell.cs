using System;
using System.Collections.Generic;
using UnityEngine;

public class CardSpecificVoSpell : CardSoundSpell
{
    public CardSpecificVoData m_CardSpecificVoData = new CardSpecificVoData();

    public override AudioSource DetermineBestAudioSource()
    {
        if (this.SearchForCard())
        {
            return this.m_CardSpecificVoData.m_AudioSource;
        }
        return base.DetermineBestAudioSource();
    }

    private bool IsCardInZones(List<Zone> zones)
    {
        if (zones != null)
        {
            foreach (Zone zone in zones)
            {
                foreach (Card card in zone.GetCards())
                {
                    if (card.GetEntity().GetCardId() == this.m_CardSpecificVoData.m_CardId)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool SearchForCard()
    {
        if (!string.IsNullOrEmpty(this.m_CardSpecificVoData.m_CardId))
        {
            foreach (SpellZoneTag tag in this.m_CardSpecificVoData.m_ZonesToSearch)
            {
                List<Zone> zones = SpellUtils.FindZonesFromTag(this, tag, this.m_CardSpecificVoData.m_SideToSearch);
                if (this.IsCardInZones(zones))
                {
                    return true;
                }
            }
        }
        return false;
    }
}

