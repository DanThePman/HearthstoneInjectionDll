using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private MusicPlaylistType m_currentPlaylist;
    private bool m_fadingInUnderlayVolumes;
    private bool m_hasOverlayPlaylist;
    private List<AudioSource> m_overlayTracks = new List<AudioSource>();
    private float m_underlayAmbienceVolume;
    private float m_underlayMusicVolume;
    private static MusicManager s_instance;

    private bool AreTracksEqual(List<MusicTrack> lhsTracks, List<MusicTrack> rhsTracks)
    {
        if (lhsTracks.Count != rhsTracks.Count)
        {
            return false;
        }
        <AreTracksEqual>c__AnonStorey385 storey = new <AreTracksEqual>c__AnonStorey385();
        using (List<MusicTrack>.Enumerator enumerator = lhsTracks.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                storey.lhs = enumerator.Current;
                if (rhsTracks.Find(new Predicate<MusicTrack>(storey.<>m__27A)) == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void Awake()
    {
        s_instance = this;
    }

    private void FadeInExistingTracks(float fadeTime)
    {
        SoundManager manager = SoundManager.Get();
        AudioSource currentMusicTrack = manager.GetCurrentMusicTrack();
        AudioSource currentAmbienceTrack = manager.GetCurrentAmbienceTrack();
        this.m_fadingInUnderlayVolumes = true;
        System.Action onComplete = () => this.m_fadingInUnderlayVolumes = false;
        this.FadeTrack(currentMusicTrack, this.m_underlayMusicVolume, fadeTime, fadeTime * 0.5f, onComplete);
        this.FadeTrack(currentAmbienceTrack, this.m_underlayAmbienceVolume, fadeTime, fadeTime * 0.5f, null);
    }

    private void FadeOutExistingTracks(float fadeTime)
    {
        SoundManager manager = SoundManager.Get();
        AudioSource currentMusicTrack = manager.GetCurrentMusicTrack();
        AudioSource currentAmbienceTrack = manager.GetCurrentAmbienceTrack();
        if (!this.m_fadingInUnderlayVolumes)
        {
            this.m_underlayMusicVolume = manager.GetVolume(currentMusicTrack);
            this.m_underlayAmbienceVolume = manager.GetVolume(currentAmbienceTrack);
        }
        this.FadeTrack(currentMusicTrack, 0f, fadeTime, 0f, null);
        this.FadeTrack(currentAmbienceTrack, 0f, fadeTime, 0f, null);
    }

    private void FadeTrack(AudioSource source, float volume, float fadeTime, float delay, System.Action onComplete = null)
    {
        <FadeTrack>c__AnonStorey386 storey = new <FadeTrack>c__AnonStorey386 {
            source = source,
            onComplete = onComplete
        };
        if (storey.source != null)
        {
            storey.sm = SoundManager.Get();
            float num = SoundManager.Get().GetVolume(storey.source);
            iTween.StopByName(storey.source.gameObject, "VOLUME_CONTROL");
            object[] args = new object[] { "from", num, "to", volume, "delay", delay, "time", fadeTime, "onupdate", new Action<object>(storey.<>m__27C), "name", "VOLUME_CONTROL", "oncomplete", new Action<object>(storey.<>m__27D) };
            iTween.ValueTo(storey.source.gameObject, iTween.Hash(args));
        }
    }

    private MusicPlaylist FindPlaylist(MusicPlaylistType type)
    {
        MusicConfig config = MusicConfig.Get();
        if (config == null)
        {
            Debug.LogError("MusicManager.FindPlaylist() - MusicConfig does not exist.");
            return null;
        }
        MusicPlaylist playlist = config.FindPlaylist(type);
        if (playlist == null)
        {
            Debug.LogWarning(string.Format("MusicManager.FindPlaylist() - {0} playlist is not defined.", type));
            return null;
        }
        return playlist;
    }

    public static MusicManager Get()
    {
        return s_instance;
    }

    public MusicPlaylistType GetCurrentPlaylist()
    {
        return this.m_currentPlaylist;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void OverlayPlaylist(MusicPlaylistType type, float fadeTime = 3f)
    {
        <OverlayPlaylist>c__AnonStorey383 storey = new <OverlayPlaylist>c__AnonStorey383 {
            fadeTime = fadeTime,
            <>f__this = this
        };
        if (SoundManager.Get() == null)
        {
            Debug.LogError("MusicManager.StartPlaylist() - SoundManager does not exist.");
        }
        else
        {
            MusicPlaylist playlist = this.FindPlaylist(type);
            if (playlist == null)
            {
                Debug.LogWarning(string.Format("MusicManager.StartPlaylist() - failed to find playlist for type {0}", type));
            }
            else
            {
                if (!this.m_hasOverlayPlaylist)
                {
                    this.FadeOutExistingTracks(storey.fadeTime);
                }
                this.m_hasOverlayPlaylist = true;
                storey.sm = SoundManager.Get();
                foreach (MusicTrack track in playlist.m_tracks)
                {
                    AssetLoader.Get().LoadSound(FileUtils.GameAssetPathToName(track.m_name), new AssetLoader.GameObjectCallback(storey.<>m__278), null, false, null);
                }
            }
        }
    }

    private void Start()
    {
        if (ApplicationMgr.Get() != null)
        {
            ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
        }
    }

    public bool StartPlaylist(MusicPlaylistType type)
    {
        if (this.m_currentPlaylist != type)
        {
            SoundManager manager = SoundManager.Get();
            if (manager == null)
            {
                Debug.LogError("MusicManager.StartPlaylist() - SoundManager does not exist.");
                return false;
            }
            MusicPlaylist playlist = this.FindPlaylist(type);
            if (playlist == null)
            {
                Debug.LogWarning(string.Format("MusicManager.StartPlaylist() - failed to find playlist for type {0}", type));
                return false;
            }
            List<MusicTrack> musicTracks = playlist.GetMusicTracks();
            List<MusicTrack> currentMusicTracks = manager.GetCurrentMusicTracks();
            if (!this.AreTracksEqual(musicTracks, currentMusicTracks))
            {
                manager.NukeMusicAndStopPlayingCurrentTrack();
                if ((musicTracks != null) && (musicTracks.Count > 0))
                {
                    manager.AddMusicTracks(musicTracks);
                }
            }
            List<MusicTrack> ambienceTracks = playlist.GetAmbienceTracks();
            List<MusicTrack> currentAmbienceTracks = manager.GetCurrentAmbienceTracks();
            if (!this.AreTracksEqual(ambienceTracks, currentAmbienceTracks))
            {
                manager.NukeAmbienceAndStopPlayingCurrentTrack();
                if ((ambienceTracks != null) && (ambienceTracks.Count > 0))
                {
                    manager.AddAmbienceTracks(ambienceTracks);
                }
            }
            this.m_currentPlaylist = playlist.m_type;
        }
        return true;
    }

    public void StopOverlayPlaylists(float fadeTime = 3f)
    {
        if (this.m_hasOverlayPlaylist)
        {
            this.m_hasOverlayPlaylist = false;
            foreach (AudioSource source in this.m_overlayTracks)
            {
                <StopOverlayPlaylists>c__AnonStorey384 storey = new <StopOverlayPlaylists>c__AnonStorey384();
                if (source != null)
                {
                    storey.destroyTrack = source;
                    this.FadeTrack(source, 0f, fadeTime, 0f, new System.Action(storey.<>m__279));
                }
            }
            this.FadeInExistingTracks(fadeTime);
            this.m_overlayTracks.Clear();
        }
    }

    public bool StopPlaylist()
    {
        SoundManager manager = SoundManager.Get();
        if (manager == null)
        {
            Debug.LogError("MusicManager.StopPlaylist() - SoundManager does not exist.");
            return false;
        }
        if (this.m_currentPlaylist == MusicPlaylistType.Invalid)
        {
            return false;
        }
        this.m_currentPlaylist = MusicPlaylistType.Invalid;
        manager.NukePlaylistsAndStopPlayingCurrentTracks();
        return true;
    }

    private void WillReset()
    {
        SoundManager manager = SoundManager.Get();
        if (manager == null)
        {
            Debug.LogError("MusicManager.WillReset() - SoundManager does not exist.");
        }
        else
        {
            this.m_currentPlaylist = MusicPlaylistType.Invalid;
            manager.ImmediatelyKillMusicAndAmbience();
        }
    }

    [CompilerGenerated]
    private sealed class <AreTracksEqual>c__AnonStorey385
    {
        internal MusicTrack lhs;

        internal bool <>m__27A(MusicTrack rhs)
        {
            return (rhs.m_name == this.lhs.m_name);
        }
    }

    [CompilerGenerated]
    private sealed class <FadeTrack>c__AnonStorey386
    {
        internal System.Action onComplete;
        internal SoundManager sm;
        internal AudioSource source;

        internal void <>m__27C(object val)
        {
            this.sm.SetVolume(this.source, (float) val);
        }

        internal void <>m__27D(object e)
        {
            if (this.onComplete != null)
            {
                this.onComplete();
            }
        }
    }

    [CompilerGenerated]
    private sealed class <OverlayPlaylist>c__AnonStorey383
    {
        internal MusicManager <>f__this;
        internal float fadeTime;
        internal SoundManager sm;

        internal void <>m__278(string name, GameObject go, object data)
        {
            AudioSource component = go.GetComponent<AudioSource>();
            if (component == null)
            {
                Debug.LogError(string.Format("Unable to find audio source on music track: {0}", name));
            }
            else
            {
                float volume = this.sm.GetVolume(component);
                this.sm.SetVolume(component, 0f);
                this.sm.PlayPreloaded(component);
                this.<>f__this.FadeTrack(component, volume, this.fadeTime, this.fadeTime * 0.5f, null);
                this.<>f__this.m_overlayTracks.Add(component);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <StopOverlayPlaylists>c__AnonStorey384
    {
        internal AudioSource destroyTrack;

        internal void <>m__279()
        {
            SoundManager.Get().Destroy(this.destroyTrack);
        }
    }
}

