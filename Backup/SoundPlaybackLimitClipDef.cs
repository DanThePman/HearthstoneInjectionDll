using System;

[Serializable]
public class SoundPlaybackLimitClipDef
{
    [CustomEditField(Range="0.0-1.0")]
    public float m_ExclusivePlaybackThreshold = 0.1f;
    [CustomEditField(Label="Clip", T=EditType.AUDIO_CLIP)]
    public string m_Path;
    public int m_Priority;
}

