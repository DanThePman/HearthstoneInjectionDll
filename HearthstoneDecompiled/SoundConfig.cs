using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class SoundConfig : MonoBehaviour
{
    [CustomEditField(Sections="Ducking")]
    public List<SoundDuckingDef> m_DuckingDefs = new List<SoundDuckingDef>();
    [CustomEditField(Sections="System Audio Sources")]
    public AudioSource m_PlaceholderSound;
    [CustomEditField(Sections="Playback Limiting")]
    public List<SoundPlaybackLimitDef> m_PlaybackLimitDefs = new List<SoundPlaybackLimitDef>();
    [CustomEditField(Sections="System Audio Sources")]
    public AudioSource m_PlayClipTemplate;
    [CustomEditField(Sections="Music")]
    public float m_SecondsBetweenMusicTracks = 10f;
}

