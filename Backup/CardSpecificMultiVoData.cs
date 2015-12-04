using System;
using System.Collections.Generic;

[Serializable]
public class CardSpecificMultiVoData
{
    public string m_CardId;
    public CardSpecificMultiVoLine[] m_Lines;
    public SpellPlayerSide m_SideToSearch = SpellPlayerSide.TARGET;
    public List<SpellZoneTag> m_ZonesToSearch = new List<SpellZoneTag> { 1, 2, 3, 4 };
}

