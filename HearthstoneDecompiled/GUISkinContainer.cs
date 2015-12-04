using UnityEngine;

public class GUISkinContainer : MonoBehaviour
{
    public GUISkin m_guiSkin;

    public GUISkin GetGUISkin()
    {
        return this.m_guiSkin;
    }
}

