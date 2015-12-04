using System;

[Serializable]
public class EmoteEntryDef
{
    public string m_emoteGameStringKey;
    [CustomEditField(T=EditType.CARD_SOUND_SPELL)]
    public string m_emoteSoundSpellPath;
    public EmoteType m_emoteType;
}

