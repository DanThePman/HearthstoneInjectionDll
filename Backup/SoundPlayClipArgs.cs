using System;
using UnityEngine;

public class SoundPlayClipArgs
{
    public SoundCategory? m_category;
    public AudioClip m_clip;
    public GameObject m_parentObject;
    public float? m_pitch;
    public float? m_spatialBlend;
    public AudioSource m_templateSource;
    public float? m_volume;
}

