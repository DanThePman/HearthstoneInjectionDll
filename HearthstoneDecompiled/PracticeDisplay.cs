using System;
using UnityEngine;

public class PracticeDisplay : MonoBehaviour
{
    private DeckPickerTrayDisplay m_deckPickerTray;
    public GameObject m_deckPickerTrayContainer;
    private PracticePickerTrayDisplay m_practicePickerTray;
    public GameObject m_practicePickerTrayContainer;
    public Vector3_MobileOverride m_practicePickerTrayHideOffset;
    public GameObject_MobileOverride m_practicePickerTrayPrefab;
    private Vector3 m_practicePickerTrayShowPos;
    private static PracticeDisplay s_instance;

    private void Awake()
    {
        s_instance = this;
        this.m_practicePickerTray = ((GameObject) GameUtils.Instantiate((GameObject) this.m_practicePickerTrayPrefab, this.m_practicePickerTrayContainer, false)).GetComponent<PracticePickerTrayDisplay>();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            SceneUtils.SetLayer(this.m_practicePickerTray, GameLayer.IgnoreFullScreenEffects);
        }
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
                    if (this.m_deckPickerTrayContainer != null)
                    {
                        GameUtils.SetParent(this.m_deckPickerTray, this.m_deckPickerTrayContainer, false);
                    }
                    AdventureSubScene component = base.GetComponent<AdventureSubScene>();
                    if (component != null)
                    {
                        this.m_practicePickerTray.AddTrayLoadedListener(delegate {
                            this.OnTrayPartLoaded();
                            this.m_practicePickerTray.gameObject.SetActive(false);
                        });
                        this.m_deckPickerTray.AddDeckTrayLoadedListener(new DeckPickerTrayDisplay.DeckTrayLoaded(this.OnTrayPartLoaded));
                        if (this.m_practicePickerTray.IsLoaded() && this.m_deckPickerTray.IsLoaded())
                        {
                            component.SetIsLoaded(true);
                        }
                    }
                    this.InitializeTrays();
                    CheatMgr.Get().RegisterCheatHandler("replaymissions", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_replaymissions), null, null, null);
                    CheatMgr.Get().RegisterCheatHandler("replaymission", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_replaymissions), null, null, null);
                    NetCache.Get().RegisterScreenPractice(new NetCache.NetCacheCallback(this.OnNetCacheReady));
                }
            }
        }, null, false);
    }

    public static PracticeDisplay Get()
    {
        return s_instance;
    }

    public Vector3 GetPracticePickerHidePosition()
    {
        return (this.m_practicePickerTrayShowPos + this.m_practicePickerTrayHideOffset);
    }

    public Vector3 GetPracticePickerShowPosition()
    {
        return this.m_practicePickerTrayShowPos;
    }

    private void InitializeTrays()
    {
        int selectedAdventure = (int) AdventureConfig.Get().GetSelectedAdventure();
        int selectedMode = (int) AdventureConfig.Get().GetSelectedMode();
        string locString = GameUtils.GetAdventureDataRecord(selectedAdventure, selectedMode).GetLocString("NAME");
        this.m_deckPickerTray.SetHeaderText(locString);
        this.m_deckPickerTray.Init();
        this.m_practicePickerTray.Init();
        this.m_practicePickerTrayShowPos = this.m_practicePickerTray.transform.localPosition;
        this.m_practicePickerTray.transform.localPosition = this.GetPracticePickerHidePosition();
    }

    public bool IsLoaded()
    {
        return (this.m_practicePickerTray.IsLoaded() && this.m_deckPickerTray.IsLoaded());
    }

    private void OnDestroy()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        if (CheatMgr.Get() != null)
        {
            CheatMgr.Get().UnregisterCheatHandler("replaymissions", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_replaymissions));
            CheatMgr.Get().UnregisterCheatHandler("replaymission", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_replaymissions));
        }
        s_instance = null;
    }

    private void OnNetCacheReady()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Practice)
        {
            if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
            {
                SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_PRACTICE", new object[0]);
            }
        }
        else if (AchieveManager.Get().HasActiveQuests(true))
        {
            WelcomeQuests.Show(false, null, false);
        }
        else
        {
            GameToastMgr.Get().UpdateQuestProgressToasts();
        }
    }

    private bool OnProcessCheat_replaymissions(string func, string[] args, string rawArgs)
    {
        AssetLoader.Get().LoadGameObject("ReplayTutorialDebug", true, false);
        return true;
    }

    private void OnTrayPartLoaded()
    {
        AdventureSubScene component = base.GetComponent<AdventureSubScene>();
        if (component != null)
        {
            component.SetIsLoaded(this.IsLoaded());
        }
    }
}

