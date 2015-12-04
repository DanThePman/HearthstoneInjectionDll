using System;
using UnityEngine;

public class CardAudio
{
    private string m_path;
    private AudioSource m_source;

    public CardAudio(string path)
    {
        this.m_path = path;
    }

    public void Clear()
    {
        UnityEngine.Object.Destroy(this.m_source);
    }

    public AudioSource GetAudio()
    {
        if ((this.m_source == null) && !string.IsNullOrEmpty(this.m_path))
        {
            this.m_source = AssetLoader.Get().LoadSound(this.m_path, true, false).GetComponent<AudioSource>();
        }
        return this.m_source;
    }
}

