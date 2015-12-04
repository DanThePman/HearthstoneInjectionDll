using System.Collections.Generic;
using UnityEngine;

public class ClassSpecificVoSpell : CardSoundSpell
{
    public ClassSpecificVoData m_ClassSpecificVoData = new ClassSpecificVoData();

    public override AudioSource DetermineBestAudioSource()
    {
        AudioSource source = this.SearchForClassSpecificVo();
        if (source != null)
        {
            return source;
        }
        return base.DetermineBestAudioSource();
    }

    private AudioSource SearchForClassSpecificVo()
    {
        foreach (SpellZoneTag tag in this.m_ClassSpecificVoData.m_ZonesToSearch)
        {
            List<Zone> zones = SpellUtils.FindZonesFromTag(this, tag, this.m_ClassSpecificVoData.m_SideToSearch);
            AudioSource source = this.SearchForClassSpecificVo(zones);
            if (source != null)
            {
                return source;
            }
        }
        return null;
    }

    private AudioSource SearchForClassSpecificVo(List<Zone> zones)
    {
        if (zones != null)
        {
            foreach (Zone zone in zones)
            {
                foreach (Card card in zone.GetCards())
                {
                    SpellClassTag tag = SpellUtils.ConvertClassTagToSpellEnum(card.GetEntity().GetClass());
                    if (tag != SpellClassTag.NONE)
                    {
                        foreach (ClassSpecificVoLine line in this.m_ClassSpecificVoData.m_Lines)
                        {
                            if (line.m_Class == tag)
                            {
                                return line.m_AudioSource;
                            }
                        }
                    }
                }
            }
        }
        return null;
    }
}

