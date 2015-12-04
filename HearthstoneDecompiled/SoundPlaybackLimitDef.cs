using System;
using System.Collections.Generic;

[Serializable]
public class SoundPlaybackLimitDef
{
    [CustomEditField(Label="Clip Group")]
    public List<SoundPlaybackLimitClipDef> m_ClipDefs = new List<SoundPlaybackLimitClipDef>();
    [CustomEditField(Label="Playback Limit")]
    public int m_Limit = 1;
}

