using PegasusShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FixedRewardsMgr
{
    [CompilerGenerated]
    private static Comparison<RewardMapIDToShow> <>f__am$cache9;
    private HashSet<NetCache.CardDefinition> m_craftableCardRewards = new HashSet<NetCache.CardDefinition>();
    private Map<int, FixedMetaActionReward> m_earnedMetaActionRewards = new Map<int, FixedMetaActionReward>();
    private bool m_registeredForAccountLicenseUpdates;
    private bool m_registeredForAdventureProgressUpdates;
    private bool m_registeredForCompletedAchieves;
    private bool m_registeredForProfileNotices;
    private HashSet<int> m_rewardMapIDsAwarded = new HashSet<int>();
    private Map<RewardVisualTiming, HashSet<RewardMapIDToShow>> m_rewardMapIDsToShow = new Map<RewardVisualTiming, HashSet<RewardMapIDToShow>>();
    private static FixedRewardsMgr s_instance;

    public bool CanCraftCard(string cardID, TAG_PREMIUM premium)
    {
        NetCache.CardDefinition item = new NetCache.CardDefinition {
            Name = cardID,
            Premium = premium
        };
        if (!this.m_craftableCardRewards.Contains(item) && GameUtils.FixedRewardExistsForCraftingCard(cardID, premium))
        {
            return false;
        }
        return true;
    }

    public bool Cheat_ShowFixedReward(int fixedRewardMapID, DelPositionNonToastReward positionNonToastRewardCallback, Vector3 rewardPunchScale, Vector3 rewardScale)
    {
        if (!ApplicationMgr.IsInternal())
        {
            return false;
        }
        DbfRecord record = GameDbf.FixedRewardMap.GetRecord(fixedRewardMapID);
        int sortOrder = (record != null) ? record.GetInt("SORT_ORDER") : 0;
        OnAllFixedRewardsShownCallbackInfo callbackInfo = new OnAllFixedRewardsShownCallbackInfo {
            m_rewardMapIDsToShow = new List<RewardMapIDToShow> { new RewardMapIDToShow(fixedRewardMapID, RewardMapIDToShow.NO_ACHIEVE_ID, sortOrder) },
            m_positionNonToastRewardCallback = positionNonToastRewardCallback,
            m_rewardPunchScale = rewardPunchScale,
            m_rewardScale = rewardScale,
            m_showingCheatRewards = true
        };
        this.ShowFixedRewards_Internal(callbackInfo);
        return true;
    }

    public void CheckForTutorialComplete()
    {
        List<CardRewardData> cardRewards = new List<CardRewardData>();
        this.CheckForTutorialComplete(cardRewards);
        CollectionManager.Get().AddCardRewards(cardRewards, false);
    }

    public void CheckForTutorialComplete(List<CardRewardData> cardRewards)
    {
        NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
        if (netObject == null)
        {
            Debug.LogWarning(string.Format("FixedRewardsMgr.CheckForTutorialComplete(): null == NetCache.NetCacheProfileProgress", new object[0]));
        }
        else
        {
            this.TriggerTutorialProgressAction(false, (int) netObject.CampaignProgress, cardRewards);
        }
    }

    public static FixedRewardsMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new FixedRewardsMgr();
            ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
        }
        if (!s_instance.m_registeredForAdventureProgressUpdates)
        {
            s_instance.m_registeredForAdventureProgressUpdates = AdventureProgressMgr.Get().RegisterProgressUpdatedListener(new AdventureProgressMgr.AdventureProgressUpdatedCallback(s_instance.OnAdventureProgressUpdate));
        }
        if (!s_instance.m_registeredForProfileNotices)
        {
            NetCache.Get().RegisterNewNoticesListener(new NetCache.DelNewNoticesListener(s_instance.OnNewNotices));
            s_instance.m_registeredForProfileNotices = true;
        }
        if (!s_instance.m_registeredForCompletedAchieves)
        {
            AchieveManager.Get().RegisterNewlyCompletedAchievesListener(new AchieveManager.NewlyAcompletedAchievesCallback(s_instance.OnNewlyCompletedAchieves));
            s_instance.m_registeredForCompletedAchieves = true;
        }
        if (!s_instance.m_registeredForAccountLicenseUpdates)
        {
            AccountLicenseMgr.Get().RegisterAccountLicensesChangedListener(new AccountLicenseMgr.AccountLicensesChangedCallback(s_instance.OnAccountLicensesUpdate));
            s_instance.m_registeredForAccountLicenseUpdates = true;
        }
        return s_instance;
    }

    private NetCache.CardDefinition GetCardDefinition(DbfRecord dbfRecReward)
    {
        TAG_PREMIUM tag_premium;
        string str = GameUtils.TranslateDbIdToCardId(dbfRecReward.GetInt("CARD_ID"));
        if (str == null)
        {
            return null;
        }
        if (!EnumUtils.TryCast<TAG_PREMIUM>(dbfRecReward.GetInt("CARD_PREMIUM"), out tag_premium))
        {
            return null;
        }
        return new NetCache.CardDefinition { Name = str, Premium = tag_premium };
    }

    private FixedMetaActionReward GetEarnedMetaActionReward(int metaActionID)
    {
        if (!this.m_earnedMetaActionRewards.ContainsKey(metaActionID))
        {
            this.m_earnedMetaActionRewards[metaActionID] = new FixedMetaActionReward(metaActionID);
        }
        return this.m_earnedMetaActionRewards[metaActionID];
    }

    private FixedReward GetFixedReward(int rewardID, int rewardCount)
    {
        FixedReward reward = new FixedReward();
        DbfRecord dbfRecReward = GameDbf.FixedReward.GetRecord(rewardID);
        if (dbfRecReward != null)
        {
            FixedRewardType type;
            if (!EnumUtils.TryGetEnum<FixedRewardType>(dbfRecReward.GetString("TYPE"), out type))
            {
                return reward;
            }
            switch (type)
            {
                case FixedRewardType.CARD:
                {
                    NetCache.CardDefinition cardDefinition = this.GetCardDefinition(dbfRecReward);
                    if (cardDefinition != null)
                    {
                        reward.FixedCardRewardData = new CardRewardData(cardDefinition.Name, cardDefinition.Premium, rewardCount);
                    }
                    return reward;
                }
                case FixedRewardType.CARD_BACK:
                {
                    int @int = dbfRecReward.GetInt("CARD_BACK_ID");
                    reward.FixedCardBackRewardData = new CardBackRewardData(@int);
                    return reward;
                }
                case FixedRewardType.CRAFTABLE_CARD:
                {
                    NetCache.CardDefinition definition2 = this.GetCardDefinition(dbfRecReward);
                    if (definition2 != null)
                    {
                        reward.FixedCraftableCardRewardData = definition2;
                    }
                    return reward;
                }
                case FixedRewardType.META_ACTION_FLAGS:
                {
                    int metaActionID = dbfRecReward.GetInt("META_ACTION_ID");
                    ulong uLong = dbfRecReward.GetULong("META_ACTION_FLAGS");
                    reward.FixedMetaActionRewardData = new FixedMetaActionReward(metaActionID);
                    reward.FixedMetaActionRewardData.UpdateFlags(uLong, 0L);
                    return reward;
                }
            }
        }
        return reward;
    }

    private List<RewardData> GetRewardsForAction(int actionID, HashSet<RewardVisualTiming> rewardTimings)
    {
        List<RewardData> list = new List<RewardData>();
        foreach (DbfRecord record in GameUtils.GetFixedRewardMapRecordsForAction(actionID))
        {
            RewardVisualTiming timing;
            int @int = record.GetInt("REWARD_COUNT");
            if (((@int > 0) && EnumUtils.TryGetEnum<RewardVisualTiming>(record.GetString("REWARD_TIMING"), out timing)) && rewardTimings.Contains(timing))
            {
                int rewardID = record.GetInt("REWARD_ID");
                FixedReward fixedReward = this.GetFixedReward(rewardID, @int);
                if (fixedReward.FixedCardRewardData != null)
                {
                    list.Add(fixedReward.FixedCardRewardData);
                }
                else if (fixedReward.FixedCardBackRewardData != null)
                {
                    list.Add(fixedReward.FixedCardBackRewardData);
                }
            }
        }
        return list;
    }

    public List<RewardData> GetRewardsForWing(int wingID, HashSet<RewardVisualTiming> rewardTimings)
    {
        List<RewardData> list = new List<RewardData>();
        List<DbfRecord> list2 = new List<DbfRecord>();
        list2.AddRange(GameUtils.GetFixedActionRecords(FixedActionType.WING_PROGRESS));
        list2.AddRange(GameUtils.GetFixedActionRecords(FixedActionType.WING_FLAGS));
        foreach (DbfRecord record in list2)
        {
            if (record.GetInt("WING_ID") == wingID)
            {
                list.AddRange(this.GetRewardsForAction(record.GetId(), rewardTimings));
            }
        }
        return list;
    }

    public List<RewardData> GetRewardsForWingFlags(int wingID, ulong flags, HashSet<RewardVisualTiming> rewardTimings)
    {
        List<RewardData> list = new List<RewardData>();
        foreach (DbfRecord record in GameUtils.GetFixedActionRecords(FixedActionType.WING_FLAGS))
        {
            if ((record.GetInt("WING_ID") == wingID) && (record.GetULong("WING_FLAGS") == flags))
            {
                list.AddRange(this.GetRewardsForAction(record.GetId(), rewardTimings));
            }
        }
        return list;
    }

    public List<RewardData> GetRewardsForWingProgress(int wingID, int progress, HashSet<RewardVisualTiming> rewardTimings)
    {
        List<RewardData> list = new List<RewardData>();
        foreach (DbfRecord record in GameUtils.GetFixedActionRecords(FixedActionType.WING_PROGRESS))
        {
            if ((record.GetInt("WING_ID") == wingID) && (record.GetInt("WING_PROGRESS") == progress))
            {
                list.AddRange(this.GetRewardsForAction(record.GetId(), rewardTimings));
            }
        }
        return list;
    }

    private void GrantAchieveRewards(List<CardRewardData> cardRewards)
    {
        AchieveManager manager = AchieveManager.Get();
        if (manager == null)
        {
            Debug.LogWarning(string.Format("FixedRewardsMgr.GrantAchieveRewards(): null == AchieveManager.Get()", new object[0]));
        }
        else
        {
            foreach (Achievement achievement in manager.GetCompletedAchieves())
            {
                bool showRewardVisual = achievement.AcknowledgedProgress < achievement.Progress;
                this.TriggerAchieveAction(showRewardVisual, achievement.ID, cardRewards);
            }
        }
    }

    private void GrantHeroLevelRewards(List<CardRewardData> cardRewards)
    {
        NetCache.NetCacheHeroLevels netObject = NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>();
        if (netObject == null)
        {
            Debug.LogWarning(string.Format("FixedRewardsMgr.GrantHeroUnlockRewards(): null == NetCache.NetCacheHeroLevels", new object[0]));
        }
        else
        {
            foreach (NetCache.HeroLevel level in netObject.Levels)
            {
                this.TriggerHeroLevelAction(false, (int) level.Class, level.CurrentLevel.Level, cardRewards);
            }
        }
    }

    public void InitStartupFixedRewards()
    {
        List<CardRewardData> cardRewards = new List<CardRewardData>();
        foreach (AdventureMission.WingProgress progress in AdventureProgressMgr.Get().GetAllProgress())
        {
            if (progress.MeetsFlagsRequirement(1L))
            {
                this.TriggerWingProgressAction(false, progress.Wing, progress.Progress, cardRewards);
                this.TriggerWingFlagsAction(false, progress.Wing, progress.Flags, cardRewards);
            }
        }
        this.CheckForTutorialComplete(cardRewards);
        this.GrantAchieveRewards(cardRewards);
        this.GrantHeroLevelRewards(cardRewards);
        foreach (AccountLicenseInfo info in AccountLicenseMgr.Get().GetAllOwnedAccountLicenseInfo())
        {
            this.TriggerAccountLicenseFlagsAction(false, info.License, info.Flags_, cardRewards);
        }
        CollectionManager.Get().AddCardRewards(cardRewards, false);
    }

    private void OnAccountLicensesUpdate(List<AccountLicenseInfo> changedAccountLicenses, object userData)
    {
        List<CardRewardData> cardRewards = new List<CardRewardData>();
        foreach (AccountLicenseInfo info in changedAccountLicenses)
        {
            if (AccountLicenseMgr.Get().OwnsAccountLicense(info))
            {
                this.TriggerAccountLicenseFlagsAction(true, info.License, info.Flags_, cardRewards);
            }
        }
        CollectionManager.Get().AddCardRewards(cardRewards, false);
    }

    private void OnAdventureProgressUpdate(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress, object userData)
    {
        List<CardRewardData> cardRewards = new List<CardRewardData>();
        if ((!isStartupAction && (newProgress != null)) && newProgress.IsOwned())
        {
            if (oldProgress == null)
            {
                this.TriggerWingProgressAction(true, newProgress.Wing, newProgress.Progress, cardRewards);
                this.TriggerWingFlagsAction(true, newProgress.Wing, newProgress.Flags, cardRewards);
            }
            else
            {
                bool flag = !oldProgress.IsOwned() && newProgress.IsOwned();
                if (flag || (oldProgress.Progress != newProgress.Progress))
                {
                    this.TriggerWingProgressAction(!flag, newProgress.Wing, newProgress.Progress, cardRewards);
                }
                if (oldProgress.Flags != newProgress.Flags)
                {
                    this.TriggerWingFlagsAction(true, newProgress.Wing, newProgress.Flags, cardRewards);
                }
            }
            CollectionManager.Get().AddCardRewards(cardRewards, false);
        }
    }

    private void OnNewlyCompletedAchieves(List<Achievement> achieves, object userData)
    {
        List<CardRewardData> cardRewards = new List<CardRewardData>();
        foreach (Achievement achievement in achieves)
        {
            this.TriggerAchieveAction(true, achievement.ID, cardRewards);
        }
        CollectionManager.Get().AddCardRewards(cardRewards, false);
    }

    private void OnNewNotices(List<NetCache.ProfileNotice> newNotices)
    {
        List<CardRewardData> cardRewards = new List<CardRewardData>();
        foreach (NetCache.ProfileNotice notice in newNotices)
        {
            if (notice.Type == NetCache.ProfileNotice.NoticeType.HERO_LEVEL_UP)
            {
                NetCache.ProfileNoticeLevelUp up = notice as NetCache.ProfileNoticeLevelUp;
                Get().TriggerHeroLevelAction(true, up.HeroClass, up.NewLevel, cardRewards);
                Network.AckNotice(notice.NoticeID);
            }
        }
        CollectionManager.Get().AddCardRewards(cardRewards, false);
    }

    private void OnNonToastRewardClicked(Reward reward, object userData)
    {
        OnAllFixedRewardsShownCallbackInfo info = userData as OnAllFixedRewardsShownCallbackInfo;
        reward.RemoveClickListener(new Reward.OnClickedCallback(this.OnNonToastRewardClicked), info);
        reward.Hide(true);
        this.ShowFixedRewards_Internal(info);
    }

    private bool QueueRewardVisual(DbfRecord fixedRewardMapRecord, int achieveID)
    {
        RewardVisualTiming iMMEDIATE;
        int id = fixedRewardMapRecord.GetId();
        if (!EnumUtils.TryGetEnum<RewardVisualTiming>(fixedRewardMapRecord.GetString("REWARD_TIMING"), out iMMEDIATE))
        {
            Debug.LogWarning(string.Format("QueueRewardVisual rewardMapID={0} no enum value for reward visual timing {1}, check fixed rewards map", id, iMMEDIATE));
            iMMEDIATE = RewardVisualTiming.IMMEDIATE;
        }
        Log.Achievements.Print(string.Concat(new object[] { "QueueRewardVisual ", achieveID, " ", fixedRewardMapRecord.GetString("NOTE_DESC") }), new object[0]);
        if (iMMEDIATE == RewardVisualTiming.NEVER)
        {
            return false;
        }
        if (!this.m_rewardMapIDsToShow.ContainsKey(iMMEDIATE))
        {
            this.m_rewardMapIDsToShow[iMMEDIATE] = new HashSet<RewardMapIDToShow>();
        }
        int @int = fixedRewardMapRecord.GetInt("SORT_ORDER");
        this.m_rewardMapIDsToShow[iMMEDIATE].Add(new RewardMapIDToShow(fixedRewardMapRecord.GetId(), achieveID, @int));
        return true;
    }

    public bool ShowFixedRewards(HashSet<RewardVisualTiming> rewardVisualTimings, DelOnAllFixedRewardsShown allRewardsShownCallback, DelPositionNonToastReward positionNonToastRewardCallback, Vector3 rewardPunchScale, Vector3 rewardScale)
    {
        return this.ShowFixedRewards(rewardVisualTimings, allRewardsShownCallback, positionNonToastRewardCallback, rewardPunchScale, rewardScale, null);
    }

    public bool ShowFixedRewards(HashSet<RewardVisualTiming> rewardVisualTimings, DelOnAllFixedRewardsShown allRewardsShownCallback, DelPositionNonToastReward positionNonToastRewardCallback, Vector3 rewardPunchScale, Vector3 rewardScale, object userData)
    {
        OnAllFixedRewardsShownCallbackInfo callbackInfo = new OnAllFixedRewardsShownCallbackInfo {
            m_rewardMapIDsToShow = new List<RewardMapIDToShow>(),
            m_onAllRewardsShownCallback = allRewardsShownCallback,
            m_positionNonToastRewardCallback = positionNonToastRewardCallback,
            m_rewardPunchScale = rewardPunchScale,
            m_rewardScale = rewardScale,
            m_userData = userData
        };
        foreach (RewardVisualTiming timing in rewardVisualTimings)
        {
            if (this.m_rewardMapIDsToShow.ContainsKey(timing))
            {
                callbackInfo.m_rewardMapIDsToShow.AddRange(this.m_rewardMapIDsToShow[timing]);
                this.m_rewardMapIDsToShow[timing].Clear();
            }
        }
        if (callbackInfo.m_rewardMapIDsToShow.Count == 0)
        {
            return false;
        }
        if (<>f__am$cache9 == null)
        {
            <>f__am$cache9 = delegate (RewardMapIDToShow a, RewardMapIDToShow b) {
                if (a.SortOrder < b.SortOrder)
                {
                    return -1;
                }
                if (a.SortOrder > b.SortOrder)
                {
                    return 1;
                }
                return 0;
            };
        }
        callbackInfo.m_rewardMapIDsToShow.Sort(<>f__am$cache9);
        this.ShowFixedRewards_Internal(callbackInfo);
        return true;
    }

    private void ShowFixedRewards_Internal(OnAllFixedRewardsShownCallbackInfo callbackInfo)
    {
        <ShowFixedRewards_Internal>c__AnonStorey377 storey = new <ShowFixedRewards_Internal>c__AnonStorey377 {
            callbackInfo = callbackInfo,
            <>f__this = this
        };
        if (storey.callbackInfo.m_rewardMapIDsToShow.Count == 0)
        {
            if (storey.callbackInfo.m_onAllRewardsShownCallback != null)
            {
                storey.callbackInfo.m_onAllRewardsShownCallback(storey.callbackInfo.m_userData);
            }
        }
        else
        {
            RewardMapIDToShow show = storey.callbackInfo.m_rewardMapIDsToShow[0];
            storey.callbackInfo.m_rewardMapIDsToShow.RemoveAt(0);
            DbfRecord record = GameDbf.FixedRewardMap.GetRecord(show.RewardMapID);
            int @int = record.GetInt("REWARD_ID");
            int rewardCount = record.GetInt("REWARD_COUNT");
            FixedReward fixedReward = this.GetFixedReward(@int, rewardCount);
            RewardData rewardData = null;
            if (fixedReward.FixedCardRewardData != null)
            {
                rewardData = fixedReward.FixedCardRewardData;
            }
            else if (fixedReward.FixedCardBackRewardData != null)
            {
                rewardData = fixedReward.FixedCardBackRewardData;
            }
            if (rewardData == null)
            {
                this.ShowFixedRewards_Internal(storey.callbackInfo);
            }
            else
            {
                if (storey.callbackInfo.m_showingCheatRewards)
                {
                    rewardData.MarkAsDummyReward();
                }
                if (show.AchieveID != RewardMapIDToShow.NO_ACHIEVE_ID)
                {
                    Achievement achievement = AchieveManager.Get().GetAchievement(show.AchieveID);
                    if (achievement != null)
                    {
                        achievement.AckCurrentProgressAndRewardNotices();
                    }
                }
                if (record.GetBool("USE_QUEST_TOAST"))
                {
                    string locString = record.GetLocString("TOAST_NAME");
                    string description = record.GetLocString("TOAST_DESCRIPTION");
                    QuestToast.ShowFixedRewardQuestToast(new QuestToast.DelOnCloseQuestToast(storey.<>m__25F), rewardData, locString, description);
                }
                else
                {
                    rewardData.LoadRewardObject(new Reward.DelOnRewardLoaded(storey.<>m__260));
                }
            }
        }
    }

    private void TriggerAccountLicenseFlagsAction(bool showRewardVisual, long license, ulong flags, List<CardRewardData> cardRewards)
    {
        foreach (DbfRecord record in GameUtils.GetFixedActionRecords(FixedActionType.ACCOUNT_LICENSE_FLAGS))
        {
            if (record.GetLong("ACCOUNT_LICENSE_ID") == license)
            {
                ulong uLong = record.GetULong("ACCOUNT_LICENSE_FLAGS");
                if ((uLong & flags) == uLong)
                {
                    this.TriggerRewardsForAction(record.GetId(), showRewardVisual, cardRewards);
                }
            }
        }
    }

    private void TriggerAchieveAction(bool showRewardVisual, int achieveId, List<CardRewardData> cardRewards)
    {
        foreach (DbfRecord record in GameUtils.GetFixedActionRecords(FixedActionType.ACHIEVE))
        {
            if (record.GetInt("ACHIEVE_ID") == achieveId)
            {
                this.TriggerRewardsForAction(record.GetId(), showRewardVisual, cardRewards, achieveId);
            }
        }
    }

    private void TriggerHeroLevelAction(bool showRewardVisual, int classID, int heroLevel, List<CardRewardData> cardRewards)
    {
        foreach (DbfRecord record in GameUtils.GetFixedActionRecords(FixedActionType.HERO_LEVEL))
        {
            if ((record.GetInt("CLASS_ID") == classID) && (record.GetInt("HERO_LEVEL") <= heroLevel))
            {
                this.TriggerRewardsForAction(record.GetId(), showRewardVisual, cardRewards);
            }
        }
    }

    private void TriggerMetaActionFlagsAction(bool showRewardVisual, int metaActionID, List<CardRewardData> cardRewards)
    {
        DbfRecord record = GameDbf.FixedRewardAction.GetRecord(metaActionID);
        if (record != null)
        {
            ulong uLong = record.GetULong("META_ACTION_FLAGS");
            if (this.GetEarnedMetaActionReward(metaActionID).HasAllRequiredFlags(uLong))
            {
                this.TriggerRewardsForAction(metaActionID, showRewardVisual, cardRewards);
            }
        }
    }

    private void TriggerRewardsForAction(int actionID, bool showRewardVisual, List<CardRewardData> cardRewards)
    {
        this.TriggerRewardsForAction(actionID, showRewardVisual, cardRewards, RewardMapIDToShow.NO_ACHIEVE_ID);
    }

    private void TriggerRewardsForAction(int actionID, bool showRewardVisual, List<CardRewardData> cardRewards, int achieveID)
    {
        foreach (DbfRecord record in GameUtils.GetFixedRewardMapRecordsForAction(actionID))
        {
            int id = record.GetId();
            if (!this.m_rewardMapIDsAwarded.Contains(id))
            {
                this.m_rewardMapIDsAwarded.Add(id);
                int @int = record.GetInt("REWARD_COUNT");
                if (@int > 0)
                {
                    int rewardID = record.GetInt("REWARD_ID");
                    FixedReward fixedReward = this.GetFixedReward(rewardID, @int);
                    if ((fixedReward.FixedCardRewardData != null) && (!showRewardVisual || !this.QueueRewardVisual(record, achieveID)))
                    {
                        cardRewards.Add(fixedReward.FixedCardRewardData);
                    }
                    if ((fixedReward.FixedCardBackRewardData != null) && (!showRewardVisual || !this.QueueRewardVisual(record, achieveID)))
                    {
                        CardBackManager.Get().AddNewCardBack(fixedReward.FixedCardBackRewardData.CardBackID);
                    }
                    if (fixedReward.FixedCraftableCardRewardData != null)
                    {
                        this.m_craftableCardRewards.Add(fixedReward.FixedCraftableCardRewardData);
                    }
                    if (fixedReward.FixedMetaActionRewardData != null)
                    {
                        this.UpdateEarnedMetaActionFlags(fixedReward.FixedMetaActionRewardData.MetaActionID, fixedReward.FixedMetaActionRewardData.MetaActionFlags, 0L);
                        this.TriggerMetaActionFlagsAction(showRewardVisual, fixedReward.FixedMetaActionRewardData.MetaActionID, cardRewards);
                    }
                }
            }
        }
    }

    private void TriggerTutorialProgressAction(bool showRewardVisual, int tutorialProgress, List<CardRewardData> cardRewards)
    {
        foreach (DbfRecord record in GameUtils.GetFixedActionRecords(FixedActionType.TUTORIAL_PROGRESS))
        {
            if (record.GetInt("TUTORIAL_PROGRESS") <= tutorialProgress)
            {
                this.TriggerRewardsForAction(record.GetId(), showRewardVisual, cardRewards);
            }
        }
    }

    private void TriggerWingFlagsAction(bool showRewardVisual, int wingID, ulong flags, List<CardRewardData> cardRewards)
    {
        foreach (DbfRecord record in GameUtils.GetFixedActionRecords(FixedActionType.WING_FLAGS))
        {
            if (record.GetInt("WING_ID") == wingID)
            {
                ulong uLong = record.GetULong("WING_FLAGS");
                if ((uLong & flags) == uLong)
                {
                    this.TriggerRewardsForAction(record.GetId(), showRewardVisual, cardRewards);
                }
            }
        }
    }

    private void TriggerWingProgressAction(bool showRewardVisual, int wingID, int progress, List<CardRewardData> cardRewards)
    {
        foreach (DbfRecord record in GameUtils.GetFixedActionRecords(FixedActionType.WING_PROGRESS))
        {
            if ((record.GetInt("WING_ID") == wingID) && (record.GetInt("WING_PROGRESS") <= progress))
            {
                this.TriggerRewardsForAction(record.GetId(), showRewardVisual, cardRewards);
            }
        }
    }

    private void UpdateEarnedMetaActionFlags(int metaActionID, ulong addFlags, ulong removeFlags)
    {
        this.GetEarnedMetaActionReward(metaActionID).UpdateFlags(addFlags, removeFlags);
    }

    private void WillReset()
    {
        this.m_craftableCardRewards.Clear();
        this.m_earnedMetaActionRewards.Clear();
        this.m_rewardMapIDsToShow.Clear();
        this.m_rewardMapIDsAwarded.Clear();
        AdventureProgressMgr.Get().RemoveProgressUpdatedListener(new AdventureProgressMgr.AdventureProgressUpdatedCallback(this.OnAdventureProgressUpdate));
        this.m_registeredForAdventureProgressUpdates = false;
        NetCache.Get().RemoveNewNoticesListener(new NetCache.DelNewNoticesListener(this.OnNewNotices));
        this.m_registeredForProfileNotices = false;
        AchieveManager.Get().RemoveNewlyCompletedAchievesListener(new AchieveManager.NewlyAcompletedAchievesCallback(this.OnNewlyCompletedAchieves));
        this.m_registeredForCompletedAchieves = false;
        AccountLicenseMgr.Get().RemoveAccountLicensesChangedListener(new AccountLicenseMgr.AccountLicensesChangedCallback(this.OnAccountLicensesUpdate));
        this.m_registeredForAccountLicenseUpdates = false;
    }

    [CompilerGenerated]
    private sealed class <ShowFixedRewards_Internal>c__AnonStorey377
    {
        internal FixedRewardsMgr <>f__this;
        internal FixedRewardsMgr.OnAllFixedRewardsShownCallbackInfo callbackInfo;

        internal void <>m__25F(object userData)
        {
            this.<>f__this.ShowFixedRewards_Internal(this.callbackInfo);
        }

        internal void <>m__260(Reward reward, object callbackData)
        {
            <ShowFixedRewards_Internal>c__AnonStorey378 storey = new <ShowFixedRewards_Internal>c__AnonStorey378 {
                <>f__ref$887 = this,
                reward = reward
            };
            if (this.callbackInfo.m_positionNonToastRewardCallback != null)
            {
                this.callbackInfo.m_positionNonToastRewardCallback(storey.reward);
            }
            RewardUtils.ShowReward(storey.reward, true, this.callbackInfo.m_rewardPunchScale, this.callbackInfo.m_rewardScale, new AnimationUtil.DelOnShownWithPunch(storey.<>m__261), null);
        }

        private sealed class <ShowFixedRewards_Internal>c__AnonStorey378
        {
            internal FixedRewardsMgr.<ShowFixedRewards_Internal>c__AnonStorey377 <>f__ref$887;
            internal Reward reward;

            internal void <>m__261(object showRewardUserData)
            {
                this.reward.RegisterClickListener(new Reward.OnClickedCallback(this.<>f__ref$887.<>f__this.OnNonToastRewardClicked), this.<>f__ref$887.callbackInfo);
                this.reward.EnableClickCatcher(true);
            }
        }
    }

    public delegate void DelOnAllFixedRewardsShown(object userData);

    public delegate void DelPositionNonToastReward(Reward reward);

    private class FixedMetaActionReward
    {
        public FixedMetaActionReward(int metaActionID)
        {
            this.MetaActionID = metaActionID;
            this.MetaActionFlags = 0L;
        }

        public bool HasAllRequiredFlags(ulong requiredFlags)
        {
            return ((this.MetaActionFlags & requiredFlags) == requiredFlags);
        }

        public void UpdateFlags(ulong addFlags, ulong removeFlags)
        {
            this.MetaActionFlags |= addFlags;
            this.MetaActionFlags &= ~removeFlags;
        }

        public ulong MetaActionFlags { get; private set; }

        public int MetaActionID { get; private set; }
    }

    private class FixedReward
    {
        public CardBackRewardData FixedCardBackRewardData = null;
        public CardRewardData FixedCardRewardData = null;
        public NetCache.CardDefinition FixedCraftableCardRewardData = null;
        public FixedRewardsMgr.FixedMetaActionReward FixedMetaActionRewardData = null;
    }

    private class OnAllFixedRewardsShownCallbackInfo
    {
        public FixedRewardsMgr.DelOnAllFixedRewardsShown m_onAllRewardsShownCallback;
        public FixedRewardsMgr.DelPositionNonToastReward m_positionNonToastRewardCallback;
        public List<FixedRewardsMgr.RewardMapIDToShow> m_rewardMapIDsToShow;
        public Vector3 m_rewardPunchScale;
        public Vector3 m_rewardScale;
        public bool m_showingCheatRewards;
        public object m_userData;
    }

    private class RewardMapIDToShow
    {
        public int AchieveID;
        public static readonly int NO_ACHIEVE_ID;
        public int RewardMapID;
        public int SortOrder;

        public RewardMapIDToShow(int rewardMapID, int achieveID, int sortOrder)
        {
            this.RewardMapID = rewardMapID;
            this.AchieveID = achieveID;
            this.SortOrder = sortOrder;
        }

        public override bool Equals(object obj)
        {
            FixedRewardsMgr.RewardMapIDToShow show = obj as FixedRewardsMgr.RewardMapIDToShow;
            if (show == null)
            {
                return false;
            }
            return (this.RewardMapID == show.RewardMapID);
        }

        public override int GetHashCode()
        {
            return this.RewardMapID.GetHashCode();
        }
    }
}

