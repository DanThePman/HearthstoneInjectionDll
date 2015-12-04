using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class AdventureConfig : MonoBehaviour
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map31;
    private List<AdventureMissionSet> m_AdventureMissionSetEventList = new List<AdventureMissionSet>();
    private List<AdventureModeChange> m_AdventureModeChangeEventList = new List<AdventureModeChange>();
    private Map<ScenarioDbId, AdventureBossDef> m_CachedBossDef = new Map<ScenarioDbId, AdventureBossDef>();
    private Map<ScenarioDbId, bool> m_CachedDefeatedScenario = new Map<ScenarioDbId, bool>();
    private Map<AdventureDbId, AdventureModeDbId> m_ClientChooserAdventureModes = new Map<AdventureDbId, AdventureModeDbId>();
    private AdventureSubScenes m_CurrentSubScene;
    private Map<string, ScenarioDbId> m_LastSelectedMissions = new Map<string, ScenarioDbId>();
    private Stack<AdventureSubScenes> m_LastSubScenes = new Stack<AdventureSubScenes>();
    private AdventureDbId m_SelectedAdventure = AdventureDbId.PRACTICE;
    private AdventureModeDbId m_SelectedMode = AdventureModeDbId.NORMAL;
    private List<SelectedModeChange> m_SelectedModeChangeEventList = new List<SelectedModeChange>();
    private ScenarioDbId m_StartMission;
    private List<SubSceneChange> m_SubSceneChangeEventList = new List<SubSceneChange>();
    private Map<string, int> m_WingBossesDefeatedCache = new Map<string, int>();
    private static AdventureConfig s_instance;

    public void AddAdventureMissionSetListener(AdventureMissionSet dlg)
    {
        this.m_AdventureMissionSetEventList.Add(dlg);
    }

    public void AddAdventureModeChangeListener(AdventureModeChange dlg)
    {
        this.m_AdventureModeChangeEventList.Add(dlg);
    }

    public void AddSelectedModeChangeListener(SelectedModeChange dlg)
    {
        this.m_SelectedModeChangeEventList.Add(dlg);
    }

    public void AddSubSceneChangeListener(SubSceneChange dlg)
    {
        this.m_SubSceneChangeEventList.Add(dlg);
    }

    private void Awake()
    {
        s_instance = this;
    }

    public bool CanPlayMode(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        bool flag = AchieveManager.Get().HasUnlockedFeature(Achievement.UnlockableFeature.VANILLA_HEROES);
        if (adventureId == AdventureDbId.PRACTICE)
        {
            if (modeId == AdventureModeDbId.EXPERT)
            {
                return flag;
            }
            return true;
        }
        if (flag)
        {
            if (modeId == AdventureModeDbId.NORMAL)
            {
                return true;
            }
            int num = (int) adventureId;
            int num2 = (int) modeId;
            foreach (DbfRecord record in GameDbf.Scenario.GetRecords())
            {
                int @int = record.GetInt("ADVENTURE_ID");
                if (num == @int)
                {
                    int num4 = record.GetInt("MODE_ID");
                    if (num2 == num4)
                    {
                        int id = record.GetId();
                        if (AdventureProgressMgr.Get().CanPlayScenario(id))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void ChangeSubScene(AdventureSubScenes subscene)
    {
        if (subscene == this.m_CurrentSubScene)
        {
            Debug.Log(string.Format("Sub scene {0} is already set.", subscene));
        }
        else
        {
            this.m_LastSubScenes.Push(this.m_CurrentSubScene);
            this.m_CurrentSubScene = subscene;
            this.FireSubSceneChangeEvent(true);
            this.FireAdventureModeChangeEvent();
        }
    }

    public void ChangeSubScene(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        this.ChangeSubScene(GetSubSceneFromMode(adventureId, modeId));
    }

    public void ChangeSubSceneToSelectedAdventure()
    {
        this.ChangeSubScene(GetSubSceneFromMode(this.m_SelectedAdventure, this.m_SelectedMode));
    }

    public void ChangeToLastSubScene(bool fireevent = true)
    {
        if (this.m_LastSubScenes.Count == 0)
        {
            Debug.Log("No last sub scenes were loaded.");
        }
        else
        {
            this.m_CurrentSubScene = this.m_LastSubScenes.Pop();
            if (fireevent)
            {
                this.FireSubSceneChangeEvent(false);
            }
            this.FireAdventureModeChangeEvent();
        }
    }

    public void ClearBossDefs()
    {
        foreach (KeyValuePair<ScenarioDbId, AdventureBossDef> pair in this.m_CachedBossDef)
        {
            UnityEngine.Object.Destroy(pair.Value);
        }
        this.m_CachedBossDef.Clear();
    }

    public bool DoesMissionRequireDeck(ScenarioDbId scenario)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord((int) scenario);
        return ((record == null) || (record.GetInt("PLAYER1_DECK_ID") == 0));
    }

    public bool DoesSelectedMissionRequireDeck()
    {
        return this.DoesMissionRequireDeck(this.m_StartMission);
    }

    private void FireAdventureModeChangeEvent()
    {
        foreach (AdventureModeChange change in this.m_AdventureModeChangeEventList.ToArray())
        {
            change(this.m_SelectedAdventure, this.m_SelectedMode);
        }
    }

    private void FireSelectedModeChangeEvent()
    {
        foreach (SelectedModeChange change in this.m_SelectedModeChangeEventList.ToArray())
        {
            change(this.m_SelectedAdventure, this.m_SelectedMode);
        }
    }

    private void FireSubSceneChangeEvent(bool forward)
    {
        this.UpdatePresence();
        foreach (SubSceneChange change in this.m_SubSceneChangeEventList.ToArray())
        {
            change(this.m_CurrentSubScene, forward);
        }
    }

    public static AdventureConfig Get()
    {
        return s_instance;
    }

    public AdventureBossDef GetBossDef(ScenarioDbId mission)
    {
        AdventureBossDef def = null;
        if (!this.m_CachedBossDef.TryGetValue(mission, out def))
        {
            Debug.LogError(string.Format("Boss def for mission not loaded: {0}\nCall LoadBossDef first.", mission));
        }
        return def;
    }

    public string GetBossDefAssetPath(ScenarioDbId mission)
    {
        DbfRecord record = GameDbf.AdventureMission.GetRecord("SCENARIO_ID", (int) mission);
        if (record == null)
        {
            return null;
        }
        return record.GetAssetName("BOSS_DEF_ASSET_PATH");
    }

    public AdventureModeDbId GetClientChooserAdventureMode(AdventureDbId adventureDbId)
    {
        AdventureModeDbId id;
        if (this.m_ClientChooserAdventureModes.TryGetValue(adventureDbId, out id))
        {
            return id;
        }
        return ((this.m_SelectedAdventure != adventureDbId) ? AdventureModeDbId.NORMAL : this.m_SelectedMode);
    }

    public AdventureSubScenes GetCurrentSubScene()
    {
        return this.m_CurrentSubScene;
    }

    public ScenarioDbId GetLastSelectedMission()
    {
        string selectedAdventureAndModeString = this.GetSelectedAdventureAndModeString();
        ScenarioDbId iNVALID = ScenarioDbId.INVALID;
        this.m_LastSelectedMissions.TryGetValue(selectedAdventureAndModeString, out iNVALID);
        return iNVALID;
    }

    public ScenarioDbId GetMission()
    {
        return this.m_StartMission;
    }

    private bool GetMissionPlayableParameters(int missionId, ref int missionReqProgress, ref int wingId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return false;
        }
        DbfRecord record2 = GameDbf.Wing.GetRecord(record.GetInt("WING_ID"));
        if (record2 == null)
        {
            return false;
        }
        DbfRecord record3 = GameDbf.AdventureMission.GetRecord("SCENARIO_ID", record.GetId());
        if (record3 == null)
        {
            return false;
        }
        missionReqProgress = record3.GetInt("REQ_PROGRESS");
        wingId = record2.GetId();
        return true;
    }

    public AdventureDbId GetSelectedAdventure()
    {
        return this.m_SelectedAdventure;
    }

    public string GetSelectedAdventureAndModeString()
    {
        return string.Format("{0}_{1}", this.m_SelectedAdventure, this.m_SelectedMode);
    }

    public AdventureModeDbId GetSelectedMode()
    {
        return this.m_SelectedMode;
    }

    public static AdventureSubScenes GetSubSceneFromMode(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        AdventureSubScenes chooser = AdventureSubScenes.Chooser;
        int num = (int) adventureId;
        int num2 = (int) modeId;
        string key = GameUtils.GetAdventureDataRecord(num, num2).GetString("SUBSCENE_PREFAB");
        if (key != null)
        {
            int num3;
            if (<>f__switch$map31 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                dictionary.Add("Assets/Game/UIScreens/AdventurePractice", 0);
                dictionary.Add("Assets/Game/UIScreens/AdventureMissionDisplay", 1);
                dictionary.Add("Assets/Game/UIScreens/AdventureClassChallenge", 2);
                <>f__switch$map31 = dictionary;
            }
            if (<>f__switch$map31.TryGetValue(key, out num3))
            {
                switch (num3)
                {
                    case 0:
                        return AdventureSubScenes.Practice;

                    case 1:
                        return AdventureSubScenes.MissionDisplay;

                    case 2:
                        return AdventureSubScenes.ClassChallenge;
                }
            }
        }
        Debug.LogError(string.Format("Adventure sub scene asset not defined for {0}.{1}.", adventureId, modeId));
        return chooser;
    }

    public int GetWingBossesDefeated(AdventureDbId advId, AdventureModeDbId mode, WingDbId wing, int defaultvalue = 0)
    {
        int num = 0;
        if (this.m_WingBossesDefeatedCache.TryGetValue(this.GetWingUniqueId(advId, mode, wing), out num))
        {
            return num;
        }
        return defaultvalue;
    }

    private string GetWingUniqueId(AdventureDbId advId, AdventureModeDbId modeId, WingDbId wing)
    {
        return string.Format("{0}_{1}_{2}", advId, modeId, wing);
    }

    public bool IsFeaturedMode(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        if (!this.CanPlayMode(adventureId, modeId))
        {
            return false;
        }
        return (((adventureId == AdventureDbId.NAXXRAMAS) && (modeId == AdventureModeDbId.CLASS_CHALLENGE)) && !Options.Get().GetBool(Option.HAS_SEEN_NAXX_CLASS_CHALLENGE, false));
    }

    public bool IsMissionAvailable(int missionId)
    {
        bool flag = AdventureProgressMgr.Get().CanPlayScenario(missionId);
        if (!flag)
        {
            return false;
        }
        int missionReqProgress = 0;
        int wingId = 0;
        if (!this.GetMissionPlayableParameters(missionId, ref missionReqProgress, ref wingId))
        {
            return false;
        }
        int ack = 0;
        AdventureProgressMgr.Get().GetWingAck(wingId, out ack);
        return (flag && (missionReqProgress <= ack));
    }

    public bool IsMissionNewlyAvailableAndGetReqs(int missionId, ref int wingId, ref int missionReqProgress)
    {
        if (!this.GetMissionPlayableParameters(missionId, ref missionReqProgress, ref wingId))
        {
            return false;
        }
        bool flag = AdventureProgressMgr.Get().CanPlayScenario(missionId);
        int ack = 0;
        AdventureProgressMgr.Get().GetWingAck(wingId, out ack);
        return ((ack < missionReqProgress) && flag);
    }

    public bool IsScenarioDefeatedAndInitCache(ScenarioDbId mission)
    {
        bool flag = AdventureProgressMgr.Get().HasDefeatedScenario((int) mission);
        if (!this.m_CachedDefeatedScenario.ContainsKey(mission))
        {
            this.m_CachedDefeatedScenario[mission] = flag;
        }
        return flag;
    }

    public bool IsScenarioJustDefeated(ScenarioDbId mission)
    {
        bool flag = AdventureProgressMgr.Get().HasDefeatedScenario((int) mission);
        bool flag2 = false;
        this.m_CachedDefeatedScenario.TryGetValue(mission, out flag2);
        this.m_CachedDefeatedScenario[mission] = flag;
        return (flag != flag2);
    }

    public void LoadBossDef(ScenarioDbId mission, DelBossDefLoaded callback)
    {
        <LoadBossDef>c__AnonStorey35D storeyd = new <LoadBossDef>c__AnonStorey35D {
            callback = callback,
            mission = mission,
            <>f__this = this
        };
        AdventureBossDef def = null;
        if (this.m_CachedBossDef.TryGetValue(storeyd.mission, out def))
        {
            storeyd.callback(def, true);
        }
        else
        {
            string bossDefAssetPath = this.GetBossDefAssetPath(storeyd.mission);
            if (string.IsNullOrEmpty(bossDefAssetPath))
            {
                if (storeyd.callback != null)
                {
                    storeyd.callback(null, false);
                }
            }
            else
            {
                AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(bossDefAssetPath), new AssetLoader.GameObjectCallback(storeyd.<>m__1F2), null, false);
            }
        }
    }

    public bool MarkFeaturedMode(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        if (this.CanPlayMode(adventureId, modeId) && ((adventureId == AdventureDbId.NAXXRAMAS) && (modeId == AdventureModeDbId.CLASS_CHALLENGE)))
        {
            Options.Get().SetBool(Option.HAS_SEEN_NAXX_CLASS_CHALLENGE, true);
            return true;
        }
        return false;
    }

    public void OnAdventureSceneAwake()
    {
        this.m_SelectedAdventure = Options.Get().GetEnum<AdventureDbId>(Option.SELECTED_ADVENTURE, AdventureDbId.PRACTICE);
        this.m_SelectedMode = Options.Get().GetEnum<AdventureModeDbId>(Option.SELECTED_ADVENTURE_MODE, AdventureModeDbId.NORMAL);
    }

    public void OnAdventureSceneUnload()
    {
        this.m_SelectedAdventure = AdventureDbId.INVALID;
        this.m_SelectedMode = AdventureModeDbId.INVALID;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void RemoveAdventureMissionSetListener(AdventureMissionSet dlg)
    {
        this.m_AdventureMissionSetEventList.Remove(dlg);
    }

    public void RemoveAdventureModeChangeListener(AdventureModeChange dlg)
    {
        this.m_AdventureModeChangeEventList.Remove(dlg);
    }

    public void RemoveSelectedModeChangeListener(SelectedModeChange dlg)
    {
        this.m_SelectedModeChangeEventList.Remove(dlg);
    }

    public void RemoveSubSceneChangeListener(SubSceneChange dlg)
    {
        this.m_SubSceneChangeEventList.Remove(dlg);
    }

    public void ResetSubScene()
    {
        this.m_CurrentSubScene = AdventureSubScenes.Chooser;
        this.m_LastSubScenes.Clear();
    }

    public void ResetSubScene(AdventureSubScenes subscene)
    {
        this.m_CurrentSubScene = subscene;
        this.m_LastSubScenes.Clear();
    }

    public void SetMission(ScenarioDbId mission, bool showDetails = true)
    {
        this.m_StartMission = mission;
        string selectedAdventureAndModeString = this.GetSelectedAdventureAndModeString();
        this.m_LastSelectedMissions[selectedAdventureAndModeString] = mission;
        foreach (AdventureMissionSet set in this.m_AdventureMissionSetEventList.ToArray())
        {
            set(mission, showDetails);
        }
    }

    public void SetSelectedAdventureMode(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        this.m_SelectedAdventure = adventureId;
        this.m_SelectedMode = modeId;
        this.m_ClientChooserAdventureModes[adventureId] = modeId;
        Options.Get().SetEnum<AdventureDbId>(Option.SELECTED_ADVENTURE, this.m_SelectedAdventure);
        Options.Get().SetEnum<AdventureModeDbId>(Option.SELECTED_ADVENTURE_MODE, this.m_SelectedMode);
        this.FireSelectedModeChangeEvent();
    }

    public void SetWingAckIfGreater(int wingId, int ackProgress)
    {
        int ack = 0;
        AdventureProgressMgr.Get().GetWingAck(wingId, out ack);
        if (ackProgress > ack)
        {
            AdventureProgressMgr.Get().SetWingAck(wingId, ackProgress);
        }
    }

    public void UpdatePresence()
    {
        switch (this.m_CurrentSubScene)
        {
            case AdventureSubScenes.MissionDeckPicker:
            case AdventureSubScenes.MissionDisplay:
            case AdventureSubScenes.ClassChallenge:
                PresenceMgr.Get().SetStatus_EnteringAdventure(this.m_SelectedAdventure, this.m_SelectedMode);
                return;
        }
        if ((AdventureScene.Get() != null) && !AdventureScene.Get().IsUnloading())
        {
            Enum[] args = new Enum[] { PresenceStatus.ADVENTURE_CHOOSING_MODE };
            PresenceMgr.Get().SetStatus(args);
        }
    }

    public void UpdateWingBossesDefeated(AdventureDbId advId, AdventureModeDbId mode, WingDbId wing, int bossesDefeated)
    {
        this.m_WingBossesDefeatedCache[this.GetWingUniqueId(advId, mode, wing)] = bossesDefeated;
    }

    [CompilerGenerated]
    private sealed class <LoadBossDef>c__AnonStorey35D
    {
        internal AdventureConfig <>f__this;
        internal AdventureConfig.DelBossDefLoaded callback;
        internal ScenarioDbId mission;

        internal void <>m__1F2(string name, GameObject go, object data)
        {
            if (go == null)
            {
                Debug.LogError(string.Format("Unable to instantiate boss def: {0}", name));
                if (this.callback != null)
                {
                    this.callback(null, false);
                }
            }
            else
            {
                AdventureBossDef component = go.GetComponent<AdventureBossDef>();
                if (component == null)
                {
                    Debug.LogError(string.Format("Object does not contain AdventureBossDef component: {0}", name));
                }
                else
                {
                    this.<>f__this.m_CachedBossDef[this.mission] = component;
                }
                if (this.callback != null)
                {
                    this.callback(component, component != null);
                }
            }
        }
    }

    public delegate void AdventureMissionSet(ScenarioDbId mission, bool showDetails);

    public delegate void AdventureModeChange(AdventureDbId adventureId, AdventureModeDbId modeId);

    public delegate void DelBossDefLoaded(AdventureBossDef bossDef, bool success);

    public delegate void SelectedModeChange(AdventureDbId adventureId, AdventureModeDbId modeId);

    public delegate void SubSceneChange(AdventureSubScenes newscene, bool forward);
}

