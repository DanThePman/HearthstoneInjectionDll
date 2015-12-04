using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class AdventureProgressMgr
{
    private Map<int, AdventureMission> m_missions = new Map<int, AdventureMission>();
    private List<AdventureProgressUpdatedListener> m_progressUpdatedListeners = new List<AdventureProgressUpdatedListener>();
    private Map<int, int> m_wingAckState = new Map<int, int>();
    private Map<int, AdventureMission.WingProgress> m_wingProgress = new Map<int, AdventureMission.WingProgress>();
    private static AdventureProgressMgr s_instance;

    private AdventureProgressMgr()
    {
        this.LoadAdventureMissionsFromDBF();
    }

    public bool CanPlayScenario(int scenarioID)
    {
        if ((DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2015) && (scenarioID != 0x425))
        {
            return false;
        }
        if (!this.m_missions.ContainsKey(scenarioID))
        {
            return true;
        }
        AdventureMission mission = this.m_missions[scenarioID];
        if (!mission.HasRequiredProgress())
        {
            return true;
        }
        AdventureMission.WingProgress progress = this.GetProgress(mission.RequiredProgress.Wing);
        if (progress == null)
        {
            return false;
        }
        return progress.MeetsProgressAndFlagsRequirements(mission.RequiredProgress);
    }

    private void CreateOrUpdateProgress(bool isStartupAction, int wing, int progress)
    {
        if (!this.m_wingProgress.ContainsKey(wing))
        {
            this.m_wingProgress[wing] = new AdventureMission.WingProgress(wing, progress, 0L);
            object[] args = new object[] { wing, this.m_wingProgress[wing] };
            Log.Rachelle.Print("AdventureProgressMgr.CreateOrUpdateProgress: creating wing {0} : PROGRESS {1}", args);
            this.FireProgressUpdate(isStartupAction, null, this.m_wingProgress[wing]);
        }
        else
        {
            AdventureMission.WingProgress oldProgress = this.m_wingProgress[wing].Clone();
            this.m_wingProgress[wing].SetProgress(progress);
            object[] objArray2 = new object[] { wing, this.m_wingProgress[wing], oldProgress };
            Log.Rachelle.Print("AdventureProgressMgr.CreateOrUpdateProgress: updating wing {0} : PROGRESS {1} (former progress {2})", objArray2);
            this.FireProgressUpdate(isStartupAction, oldProgress, this.m_wingProgress[wing]);
        }
    }

    private void CreateOrUpdateWingAck(int wing, int ack)
    {
        this.m_wingAckState[wing] = ack;
    }

    private void CreateOrUpdateWingFlags(bool isStartupAction, int wing, ulong flags)
    {
        if (!this.m_wingProgress.ContainsKey(wing))
        {
            this.m_wingProgress[wing] = new AdventureMission.WingProgress(wing, 0, flags);
            object[] args = new object[] { wing, this.m_wingProgress[wing] };
            Log.Rachelle.Print("AdventureProgressMgr.CreateOrUpdateWingFlags: creating wing {0} : PROGRESS {1}", args);
            this.FireProgressUpdate(isStartupAction, null, this.m_wingProgress[wing]);
        }
        else
        {
            AdventureMission.WingProgress oldProgress = this.m_wingProgress[wing].Clone();
            this.m_wingProgress[wing].SetFlags(flags);
            object[] objArray2 = new object[] { wing, this.m_wingProgress[wing], oldProgress };
            Log.Rachelle.Print("AdventureProgressMgr.CreateOrUpdateWingFlags: updating wing {0} : PROGRESS {1} (former flags {2})", objArray2);
            this.FireProgressUpdate(isStartupAction, oldProgress, this.m_wingProgress[wing]);
        }
    }

    private void FireProgressUpdate(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress)
    {
        foreach (AdventureProgressUpdatedListener listener in this.m_progressUpdatedListeners.ToArray())
        {
            listener.Fire(isStartupAction, oldProgress, newProgress);
        }
    }

    public static AdventureProgressMgr Get()
    {
        return s_instance;
    }

    public AdventureOption[] GetAdventureOptions()
    {
        NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
        return ((netObject != null) ? netObject.AdventureOptions : null);
    }

    public List<AdventureMission.WingProgress> GetAllProgress()
    {
        return new List<AdventureMission.WingProgress>(this.m_wingProgress.Values);
    }

    public List<CardRewardData> GetCardRewardsForDefeatingScenario(int scenarioID, HashSet<RewardVisualTiming> rewardTimings)
    {
        AdventureMission mission;
        if (!this.m_missions.TryGetValue(scenarioID, out mission))
        {
            return new List<CardRewardData>();
        }
        List<RewardData> list = null;
        if (GameUtils.IsHeroicAdventureMission(scenarioID))
        {
            list = FixedRewardsMgr.Get().GetRewardsForWingFlags(mission.GrantedProgress.Wing, mission.GrantedProgress.Flags, rewardTimings);
        }
        else if (GameUtils.IsClassChallengeMission(scenarioID))
        {
            list = FixedRewardsMgr.Get().GetRewardsForWingFlags(mission.GrantedProgress.Wing, mission.GrantedProgress.Flags, rewardTimings);
        }
        else if (mission.GrantedProgress != null)
        {
            list = FixedRewardsMgr.Get().GetRewardsForWingProgress(mission.GrantedProgress.Wing, mission.GrantedProgress.Progress, rewardTimings);
        }
        List<CardRewardData> list2 = new List<CardRewardData>();
        if (list != null)
        {
            foreach (RewardData data in list)
            {
                if (data.RewardType == Reward.Type.CARD)
                {
                    list2.Add(data as CardRewardData);
                }
            }
        }
        return list2;
    }

    public List<CardRewardData> GetCardRewardsForWing(int wing, HashSet<RewardVisualTiming> rewardTimings)
    {
        List<RewardData> rewardsForWing = FixedRewardsMgr.Get().GetRewardsForWing(wing, rewardTimings);
        List<CardRewardData> list2 = new List<CardRewardData>();
        foreach (RewardData data in rewardsForWing)
        {
            if (data.RewardType == Reward.Type.CARD)
            {
                list2.Add(data as CardRewardData);
            }
        }
        return list2;
    }

    public List<CardRewardData> GetImmediateCardRewardsForDefeatingScenario(int scenarioID)
    {
        HashSet<RewardVisualTiming> rewardTimings = new HashSet<RewardVisualTiming> { 1 };
        return this.GetCardRewardsForDefeatingScenario(scenarioID, rewardTimings);
    }

    public int GetPlayableAdventureScenarios(AdventureDbId adventureID, AdventureModeDbId modeID)
    {
        List<DbfRecord> records = GameDbf.Wing.GetRecords();
        List<DbfRecord> list2 = GameDbf.Scenario.GetRecords();
        int num = 0;
        foreach (DbfRecord record in records)
        {
            int id = record.GetId();
            if (((record.GetInt("ADVENTURE_ID") == adventureID) && this.OwnsWing(id)) && this.IsWingOpen(id))
            {
                foreach (DbfRecord record2 in list2)
                {
                    if ((((record.GetId() == record2.GetInt("WING_ID")) && (record2.GetInt("ADVENTURE_ID") == adventureID)) && ((record2.GetInt("MODE_ID") == modeID) && !Get().HasDefeatedScenario(record2.GetId()))) && ((record2.GetInt("MODE_ID") != 3) || Get().CanPlayScenario(record2.GetId())))
                    {
                        num++;
                    }
                }
            }
        }
        return num;
    }

    public int GetPlayableClassChallenges(AdventureDbId adventureID, AdventureModeDbId modeID)
    {
        int num = 0;
        foreach (DbfRecord record in GameDbf.Scenario.GetRecords())
        {
            if (((record.GetInt("ADVENTURE_ID") == adventureID) && (record.GetInt("MODE_ID") == modeID)) && (this.CanPlayScenario(record.GetId()) && !this.HasDefeatedScenario(record.GetId())))
            {
                num++;
            }
        }
        return num;
    }

    public AdventureMission.WingProgress GetProgress(int wing)
    {
        if (!this.m_wingProgress.ContainsKey(wing))
        {
            return null;
        }
        return this.m_wingProgress[wing];
    }

    public bool GetWingAck(int wing, out int ack)
    {
        return this.m_wingAckState.TryGetValue(wing, out ack);
    }

    public string GetWingName(int wing)
    {
        DbfRecord record = GameDbf.Wing.GetRecord(wing);
        if (record == null)
        {
            Debug.LogWarning(string.Format("AdventureProgressMgr.GetWingName could not find DBF record for wing {0}", wing));
            return string.Empty;
        }
        return record.GetLocString("NAME");
    }

    public SpecialEventType GetWingOpenEvent(int wing)
    {
        SpecialEventType type;
        DbfRecord record = GameDbf.Wing.GetRecord(wing);
        if (record == null)
        {
            Debug.LogWarning(string.Format("AdventureProgressMgr.GetWingOpenEvent could not find DBF record for wing {0}, assuming it is has no open event", wing));
            return SpecialEventType.IGNORE;
        }
        string str = record.GetString("REQUIRED_EVENT");
        if (!EnumUtils.TryGetEnum<SpecialEventType>(str, out type))
        {
            Debug.LogWarning(string.Format("AdventureProgressMgr.GetWingOpenEvent wing={0} could not find SpecialEventType record for event '{1}'", wing, str));
            return SpecialEventType.IGNORE;
        }
        return type;
    }

    public bool HasDefeatedScenario(int scenarioID)
    {
        AdventureMission mission;
        if (!this.m_missions.TryGetValue(scenarioID, out mission))
        {
            return false;
        }
        if (mission.RequiredProgress == null)
        {
            return false;
        }
        AdventureMission.WingProgress progress = this.GetProgress(mission.RequiredProgress.Wing);
        if (progress == null)
        {
            return false;
        }
        if (GameUtils.IsHeroicAdventureMission(scenarioID))
        {
            return progress.MeetsFlagsRequirement(mission.GrantedProgress.Flags);
        }
        if (GameUtils.IsClassChallengeMission(scenarioID))
        {
            return progress.MeetsFlagsRequirement(mission.GrantedProgress.Flags);
        }
        return progress.MeetsProgressRequirement(mission.GrantedProgress.Progress);
    }

    public static void Init()
    {
        if (s_instance == null)
        {
            s_instance = new AdventureProgressMgr();
            Network.Get().RegisterNetHandler(AdventureProgressResponse.PacketID.ID, new Network.NetHandler(s_instance.OnAdventureProgress), null);
            NetCache.Get().RegisterNewNoticesListener(new NetCache.DelNewNoticesListener(s_instance.OnNewNotices));
            ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
        }
        Network.RequestAdventureProgress();
    }

    public bool IsWingComplete(AdventureDbId adventureID, AdventureModeDbId modeID, WingDbId wingId)
    {
        List<DbfRecord> records = GameDbf.Scenario.GetRecords();
        int num = 0;
        int num2 = 0;
        foreach (DbfRecord record in records)
        {
            if (((record.GetInt("ADVENTURE_ID") == adventureID) && (record.GetInt("MODE_ID") == modeID)) && (record.GetInt("WING_ID") == wingId))
            {
                num2++;
                if (this.HasDefeatedScenario(record.GetId()))
                {
                    num++;
                }
            }
        }
        return (num == num2);
    }

    public bool IsWingLocked(int wingId)
    {
        if (wingId == 14)
        {
            bool flag = this.IsWingComplete(AdventureDbId.LOE, AdventureModeDbId.NORMAL, WingDbId.LOE_TEMPLE_OF_ORSIS);
            bool flag2 = this.IsWingComplete(AdventureDbId.LOE, AdventureModeDbId.NORMAL, WingDbId.LOE_ULDAMAN);
            bool flag3 = this.IsWingComplete(AdventureDbId.LOE, AdventureModeDbId.NORMAL, WingDbId.LOE_RUINED_CITY);
            return !((flag && flag2) && flag3);
        }
        return false;
    }

    public bool IsWingOpen(int wing)
    {
        SpecialEventType wingOpenEvent = this.GetWingOpenEvent(wing);
        return SpecialEventManager.Get().IsEventActive(wingOpenEvent, false);
    }

    private void LoadAdventureMissionsFromDBF()
    {
        foreach (DbfRecord record in GameDbf.AdventureMission.GetRecords())
        {
            int @int = record.GetInt("SCENARIO_ID");
            if (this.m_missions.ContainsKey(@int))
            {
                Debug.LogWarning(string.Format("AdventureProgressMgr.LoadAdventureMissionsFromDBF(): duplicate entry found for scenario ID {0}", @int));
            }
            else
            {
                string description = record.GetString("NOTE_DESC");
                AdventureMission.WingProgress requiredProgress = new AdventureMission.WingProgress(record.GetInt("REQ_WING_ID"), record.GetInt("REQ_PROGRESS"), record.GetULong("REQ_FLAGS"));
                AdventureMission.WingProgress grantedProgress = new AdventureMission.WingProgress(record.GetInt("GRANTS_WING_ID"), record.GetInt("GRANTS_PROGRESS"), record.GetULong("GRANTS_FLAGS"));
                this.m_missions[@int] = new AdventureMission(@int, description, requiredProgress, grantedProgress);
            }
        }
    }

    private void OnAdventureProgress()
    {
        foreach (Network.AdventureProgress progress in Network.GetAdventureProgressResponse())
        {
            this.CreateOrUpdateProgress(true, progress.Wing, progress.Progress);
            this.CreateOrUpdateWingFlags(true, progress.Wing, progress.Flags);
            this.CreateOrUpdateWingAck(progress.Wing, progress.Ack);
        }
    }

    private void OnNewNotices(List<NetCache.ProfileNotice> newNotices)
    {
        List<long> list = new List<long>();
        foreach (NetCache.ProfileNotice notice in newNotices)
        {
            if (notice.Type == NetCache.ProfileNotice.NoticeType.ADVENTURE_PROGRESS)
            {
                NetCache.ProfileNoticeAdventureProgress progress = notice as NetCache.ProfileNoticeAdventureProgress;
                if (progress.Progress.HasValue)
                {
                    this.CreateOrUpdateProgress(false, progress.Wing, progress.Progress.Value);
                }
                if (progress.Flags.HasValue)
                {
                    this.CreateOrUpdateWingFlags(false, progress.Wing, progress.Flags.Value);
                }
                list.Add(notice.NoticeID);
            }
        }
        foreach (long num in list)
        {
            Network.AckNotice(num);
        }
    }

    public bool OwnsWing(int wing)
    {
        if (!this.m_wingProgress.ContainsKey(wing))
        {
            return false;
        }
        return this.m_wingProgress[wing].IsOwned();
    }

    public bool RegisterProgressUpdatedListener(AdventureProgressUpdatedCallback callback)
    {
        return this.RegisterProgressUpdatedListener(callback, null);
    }

    public bool RegisterProgressUpdatedListener(AdventureProgressUpdatedCallback callback, object userData)
    {
        if (callback == null)
        {
            return false;
        }
        AdventureProgressUpdatedListener item = new AdventureProgressUpdatedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_progressUpdatedListeners.Contains(item))
        {
            return false;
        }
        this.m_progressUpdatedListeners.Add(item);
        return true;
    }

    public bool RemoveProgressUpdatedListener(AdventureProgressUpdatedCallback callback)
    {
        return this.RemoveProgressUpdatedListener(callback, null);
    }

    public bool RemoveProgressUpdatedListener(AdventureProgressUpdatedCallback callback, object userData)
    {
        if (callback == null)
        {
            return false;
        }
        AdventureProgressUpdatedListener item = new AdventureProgressUpdatedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_progressUpdatedListeners.Contains(item))
        {
            return false;
        }
        this.m_progressUpdatedListeners.Remove(item);
        return true;
    }

    public void SetAdventureOptions(int id, ulong options)
    {
        if (id != 0)
        {
            NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
            if ((netObject != null) && (netObject.AdventureOptions != null))
            {
                foreach (AdventureOption option in netObject.AdventureOptions)
                {
                    if (option.AdventureID == id)
                    {
                        option.Options = options;
                        NetCache.Get().NetCacheChanged<NetCache.NetCacheProfileProgress>();
                        break;
                    }
                }
            }
            Network.SetAdventureOptions(id, options);
        }
    }

    public bool SetWingAck(int wing, int ackId)
    {
        int num;
        if (this.m_wingAckState.TryGetValue(wing, out num))
        {
            if (ackId < num)
            {
                return false;
            }
            if (ackId == num)
            {
                return true;
            }
        }
        this.m_wingAckState[wing] = ackId;
        Network.AckWingProgress(wing, ackId);
        return true;
    }

    private void WillReset()
    {
        this.m_wingProgress.Clear();
        this.m_wingAckState.Clear();
        this.m_progressUpdatedListeners.Clear();
    }

    public delegate void AdventureProgressUpdatedCallback(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress, object userData);

    private class AdventureProgressUpdatedListener : EventListener<AdventureProgressMgr.AdventureProgressUpdatedCallback>
    {
        public void Fire(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress)
        {
            base.m_callback(isStartupAction, oldProgress, newProgress, base.m_userData);
        }
    }
}

