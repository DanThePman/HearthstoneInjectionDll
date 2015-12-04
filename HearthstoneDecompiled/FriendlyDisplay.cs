using System;
using UnityEngine;

public class FriendlyDisplay : MonoBehaviour
{
    private DeckPickerTrayDisplay m_deckPickerTray;
    public GameObject m_deckPickerTrayContainer;
    private static FriendlyDisplay s_instance;

    private void Awake()
    {
        s_instance = this;
        AssetLoader.Get().LoadActor((UniversalInputManager.UsePhoneUI == null) ? "DeckPickerTray" : "DeckPickerTray_phone", delegate (string name, GameObject go, object data) {
            if (go == null)
            {
                Debug.LogError("Unable to load DeckPickerTray.");
            }
            else
            {
                this.m_deckPickerTray = go.GetComponent<DeckPickerTrayDisplay>();
                if (this.m_deckPickerTray == null)
                {
                    Debug.LogError("DeckPickerTrayDisplay component not found in DeckPickerTray object.");
                }
                else
                {
                    GameUtils.SetParent(this.m_deckPickerTray, this.m_deckPickerTrayContainer, false);
                    this.m_deckPickerTray.SetHeaderText(GameStrings.Get(!FriendChallengeMgr.Get().IsChallengeTavernBrawl() ? "GLOBAL_FRIEND_CHALLENGE_TITLE" : "GLOBAL_TAVERN_BRAWL"));
                    this.m_deckPickerTray.Init();
                    this.DisableOtherModeStuff();
                    NetCache.Get().RegisterScreenFriendly(new NetCache.NetCacheCallback(this.OnNetCacheReady));
                    MusicManager.Get().StartPlaylist(!FriendChallengeMgr.Get().IsChallengeTavernBrawl() ? MusicPlaylistType.UI_Friendly : MusicPlaylistType.UI_TavernBrawl);
                }
            }
        }, null, false);
    }

    private void DisableOtherModeStuff()
    {
        if (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY)
        {
            Camera camera = CameraUtils.FindFullScreenEffectsCamera(true);
            if (camera != null)
            {
                camera.GetComponent<FullScreenEffects>().Disable();
            }
        }
    }

    public static FriendlyDisplay Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnNetCacheReady()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        if (AchieveManager.Get().HasActiveQuests(true))
        {
            WelcomeQuests.Show(false, null, false);
        }
        else
        {
            GameToastMgr.Get().UpdateQuestProgressToasts();
        }
    }

    public void Unload()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
    }
}

