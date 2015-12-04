using System;

[Serializable]
public class MusicTrack
{
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_name;
    public bool m_shuffle = true;
    [CustomEditField(ListSortable=true)]
    public MusicTrackType m_trackType;
    public float m_volume = 1f;

    public MusicTrack Clone()
    {
        return (MusicTrack) base.MemberwiseClone();
    }
}

