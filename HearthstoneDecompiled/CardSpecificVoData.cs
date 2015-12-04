using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardSpecificVoData
{
    public AudioSource m_AudioSource;
    public string m_CardId;
    public SpellPlayerSide m_SideToSearch = SpellPlayerSide.TARGET;
    public List<SpellZoneTag> m_ZonesToSearch = new List<SpellZoneTag> { 1, 2, 3, 4 };
}

