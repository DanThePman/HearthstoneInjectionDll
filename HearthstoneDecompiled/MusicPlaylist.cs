using System;
using System.Collections.Generic;

[Serializable]
public class MusicPlaylist
{
    [CustomEditField(ListTable=true)]
    public List<MusicTrack> m_tracks = new List<MusicTrack>();
    [CustomEditField(ListSortable=true)]
    public MusicPlaylistType m_type;

    public List<MusicTrack> GetAmbienceTracks()
    {
        return this.GetRandomizedTracks(this.m_tracks, MusicTrackType.Ambience);
    }

    public List<MusicTrack> GetMusicTracks()
    {
        return this.GetRandomizedTracks(this.m_tracks, MusicTrackType.Music);
    }

    private List<MusicTrack> GetRandomizedTracks(List<MusicTrack> trackList, MusicTrackType type)
    {
        List<MusicTrack> list = new List<MusicTrack>();
        List<MusicTrack> list2 = new List<MusicTrack>();
        foreach (MusicTrack track in trackList)
        {
            if ((type == track.m_trackType) && !string.IsNullOrEmpty(track.m_name))
            {
                if (track.m_shuffle)
                {
                    list2.Add(track.Clone());
                }
                else
                {
                    list.Add(track.Clone());
                }
            }
        }
        Random random = new Random();
        while (list2.Count > 0)
        {
            int index = random.Next(0, list2.Count);
            list.Add(list2[index]);
            list2.RemoveAt(index);
        }
        return list;
    }
}

