using System;

[Serializable]
public class SpellTableEntry
{
    [CustomEditField(Hide=true)]
    public Spell m_Spell;
    [CustomEditField(T=EditType.SPELL)]
    public string m_SpellPrefabName = string.Empty;
    public SpellType m_Type;
}

