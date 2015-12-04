using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class StoreAdventureDef : MonoBehaviour
{
    [CustomEditField(T=EditType.TEXTURE)]
    public string m_accentTextureName;
    public Material m_keyArt;
    [CustomEditField(T=EditType.TEXTURE)]
    public string m_logoTextureName;
    public MusicPlaylistType m_playlist;
    public int m_preorderCardBackId;
    public List<string> m_previewCards = new List<string>();
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_storeButtonPrefab;
}

