using System;
using UnityEngine;

[CustomEditClass]
public class AdventureBossDef : MonoBehaviour
{
    public Material m_CoinPortraitMaterial;
    public string m_IntroLine;
    public IntroLinePlayTime m_IntroLinePlayTime;
    public MusicPlaylistType m_MissionMusic;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_quotePrefabOverride;

    public enum IntroLinePlayTime
    {
        MissionSelect,
        MissionStart
    }
}

