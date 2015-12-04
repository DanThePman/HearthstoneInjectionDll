using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class MusicConfig : MonoBehaviour
{
    [CustomEditField(Sections="Playlists")]
    public List<MusicPlaylist> m_playlists = new List<MusicPlaylist>();
    private static MusicConfig s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public MusicPlaylist FindPlaylist(MusicPlaylistType type)
    {
        for (int i = 0; i < this.m_playlists.Count; i++)
        {
            MusicPlaylist playlist = this.m_playlists[i];
            if (playlist.m_type == type)
            {
                return playlist;
            }
        }
        return null;
    }

    public static MusicConfig Get()
    {
        return s_instance;
    }

    public MusicPlaylist GetPlaylist(MusicPlaylistType type)
    {
        MusicPlaylist playlist1 = this.FindPlaylist(type);
        if (playlist1 != null)
        {
            return playlist1;
        }
        return new MusicPlaylist();
    }

    private void OnDestroy()
    {
        s_instance = null;
    }
}

