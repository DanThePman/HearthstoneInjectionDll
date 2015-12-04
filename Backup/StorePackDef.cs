using System;
using UnityEngine;

[CustomEditClass]
public class StorePackDef : MonoBehaviour
{
    [CustomEditField(T=EditType.TEXTURE)]
    public string m_accentTextureName;
    public Material m_background;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_buttonPrefab;
    [CustomEditField(T=EditType.TEXTURE)]
    public string m_logoTextureGlowName;
    [CustomEditField(T=EditType.TEXTURE)]
    public string m_logoTextureName;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_lowPolyPrefab;
    public MusicPlaylistType m_playlist;
    public string m_preorderAvailableDateString;
}

