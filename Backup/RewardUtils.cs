using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class RewardUtils
{
    [CompilerGenerated]
    private static Comparison<Reward> <>f__am$cache2;
    public static readonly Vector3 REWARD_HIDDEN_SCALE = new Vector3(0.001f, 0.001f, 0.001f);
    public static readonly float REWARD_HIDE_TIME = 0.25f;

    public static void AddRewardDataToList(RewardData newRewardData, List<RewardData> existingRewardDataList)
    {
        CardRewardData duplicateCardDataReward = GetDuplicateCardDataReward(newRewardData, existingRewardDataList);
        if (duplicateCardDataReward == null)
        {
            existingRewardDataList.Add(newRewardData);
        }
        else
        {
            CardRewardData other = newRewardData as CardRewardData;
            duplicateCardDataReward.Merge(other);
        }
    }

    private static CardRewardData GetDuplicateCardDataReward(RewardData newRewardData, List<RewardData> existingRewardData)
    {
        <GetDuplicateCardDataReward>c__AnonStorey323 storey = new <GetDuplicateCardDataReward>c__AnonStorey323();
        if (!(newRewardData is CardRewardData))
        {
            return null;
        }
        storey.newCardRewardData = newRewardData as CardRewardData;
        return (existingRewardData.Find(new Predicate<RewardData>(storey.<>m__153)) as CardRewardData);
    }

    public static List<RewardData> GetRewards(List<NetCache.ProfileNotice> notices)
    {
        List<RewardData> existingRewardDataList = new List<RewardData>();
        foreach (NetCache.ProfileNotice notice in notices)
        {
            RewardData newRewardData = null;
            switch (notice.Type)
            {
                case NetCache.ProfileNotice.NoticeType.REWARD_BOOSTER:
                {
                    NetCache.ProfileNoticeRewardBooster booster = notice as NetCache.ProfileNoticeRewardBooster;
                    BoosterPackRewardData data2 = new BoosterPackRewardData(booster.Id, booster.Count);
                    newRewardData = data2;
                    break;
                }
                case NetCache.ProfileNotice.NoticeType.REWARD_CARD:
                {
                    NetCache.ProfileNoticeRewardCard card = notice as NetCache.ProfileNoticeRewardCard;
                    CardRewardData data3 = new CardRewardData(card.CardID, card.Premium, card.Quantity);
                    newRewardData = data3;
                    break;
                }
                case NetCache.ProfileNotice.NoticeType.DISCONNECTED_GAME:
                case NetCache.ProfileNotice.NoticeType.PRECON_DECK:
                case NetCache.ProfileNotice.NoticeType.PURCHASE:
                {
                    continue;
                }
                case NetCache.ProfileNotice.NoticeType.REWARD_DUST:
                {
                    NetCache.ProfileNoticeRewardDust dust = notice as NetCache.ProfileNoticeRewardDust;
                    ArcaneDustRewardData data4 = new ArcaneDustRewardData(dust.Amount);
                    newRewardData = data4;
                    break;
                }
                case NetCache.ProfileNotice.NoticeType.REWARD_MOUNT:
                {
                    NetCache.ProfileNoticeRewardMount mount = notice as NetCache.ProfileNoticeRewardMount;
                    MountRewardData data5 = new MountRewardData((MountRewardData.MountType) mount.MountID);
                    newRewardData = data5;
                    break;
                }
                case NetCache.ProfileNotice.NoticeType.REWARD_FORGE:
                {
                    NetCache.ProfileNoticeRewardForge forge = notice as NetCache.ProfileNoticeRewardForge;
                    ForgeTicketRewardData data6 = new ForgeTicketRewardData(forge.Quantity);
                    newRewardData = data6;
                    break;
                }
                case NetCache.ProfileNotice.NoticeType.REWARD_GOLD:
                {
                    NetCache.ProfileNoticeRewardGold gold = notice as NetCache.ProfileNoticeRewardGold;
                    GoldRewardData data7 = new GoldRewardData((long) gold.Amount, new DateTime?(DateTime.FromFileTimeUtc(gold.Date)));
                    newRewardData = data7;
                    break;
                }
                case NetCache.ProfileNotice.NoticeType.REWARD_CARD_BACK:
                {
                    NetCache.ProfileNoticeRewardCardBack back = notice as NetCache.ProfileNoticeRewardCardBack;
                    CardBackRewardData data8 = new CardBackRewardData(back.CardBackID);
                    newRewardData = data8;
                    break;
                }
                default:
                {
                    continue;
                }
            }
            if (newRewardData != null)
            {
                newRewardData.SetOrigin(notice.Origin, notice.OriginData);
                newRewardData.AddNoticeID(notice.NoticeID);
                AddRewardDataToList(newRewardData, existingRewardDataList);
            }
        }
        return existingRewardDataList;
    }

    public static string GetRewardText(RewardData rewardData)
    {
        switch (rewardData.RewardType)
        {
            case Reward.Type.ARCANE_DUST:
            {
                ArcaneDustRewardData data3 = rewardData as ArcaneDustRewardData;
                object[] args = new object[] { data3.Amount };
                return GameStrings.Format("GLOBAL_HERO_LEVEL_REWARD_ARCANE_DUST", args);
            }
            case Reward.Type.BOOSTER_PACK:
            {
                BoosterPackRewardData data2 = rewardData as BoosterPackRewardData;
                string locString = GameDbf.Booster.GetRecord(data2.Id).GetLocString("NAME");
                object[] objArray2 = new object[] { locString };
                return GameStrings.Format("GLOBAL_HERO_LEVEL_REWARD_BOOSTER", objArray2);
            }
            case Reward.Type.CARD:
            {
                CardRewardData data = rewardData as CardRewardData;
                EntityDef entityDef = DefLoader.Get().GetEntityDef(data.CardID);
                if (data.Premium != TAG_PREMIUM.GOLDEN)
                {
                    return entityDef.GetName();
                }
                object[] objArray1 = new object[] { GameStrings.Get("GLOBAL_COLLECTION_GOLDEN"), entityDef.GetName() };
                return GameStrings.Format("GLOBAL_HERO_LEVEL_REWARD_GOLDEN_CARD", objArray1);
            }
            case Reward.Type.GOLD:
            {
                GoldRewardData data4 = rewardData as GoldRewardData;
                object[] objArray4 = new object[] { data4.Amount };
                return GameStrings.Format("GLOBAL_HERO_LEVEL_REWARD_GOLD", objArray4);
            }
        }
        return "UNKNOWN";
    }

    public static void GetViewableRewards(List<RewardData> rewardDataList, HashSet<RewardVisualTiming> rewardTimings, ref List<RewardData> rewardsToShow, ref List<Achievement> completedQuests)
    {
        if (rewardsToShow == null)
        {
            rewardsToShow = new List<RewardData>();
        }
        if (completedQuests == null)
        {
            completedQuests = new List<Achievement>();
        }
        foreach (RewardData data in rewardDataList)
        {
            object[] args = new object[] { data };
            Log.Rachelle.Print("RewardUtils.GetViewableRewards() - processing reward {0}", args);
            if (data.Origin == NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT)
            {
                <GetViewableRewards>c__AnonStorey322 storey = new <GetViewableRewards>c__AnonStorey322 {
                    completedQuest = AchieveManager.Get().GetAchievement((int) data.OriginData)
                };
                if (storey.completedQuest != null)
                {
                    List<long> noticeIDs = data.GetNoticeIDs();
                    Achievement achievement = completedQuests.Find(new Predicate<Achievement>(storey.<>m__151));
                    if (achievement != null)
                    {
                        foreach (long num in noticeIDs)
                        {
                            achievement.AddRewardNoticeID(num);
                        }
                    }
                    else
                    {
                        foreach (long num2 in noticeIDs)
                        {
                            storey.completedQuest.AddRewardNoticeID(num2);
                        }
                        if (rewardTimings.Contains(storey.completedQuest.RewardTiming))
                        {
                            completedQuests.Add(storey.completedQuest);
                        }
                    }
                }
                continue;
            }
            bool flag = false;
            switch (data.RewardType)
            {
                case Reward.Type.ARCANE_DUST:
                case Reward.Type.BOOSTER_PACK:
                case Reward.Type.GOLD:
                    flag = true;
                    goto Label_026F;

                case Reward.Type.CARD:
                {
                    CardRewardData cardReward = data as CardRewardData;
                    if (!(cardReward.CardID.Equals("HERO_08") && cardReward.Premium.Equals(TAG_PREMIUM.NORMAL)))
                    {
                        break;
                    }
                    flag = false;
                    data.AcknowledgeNotices();
                    CollectionManager.Get().AddCardReward(cardReward, false);
                    goto Label_026F;
                }
                case Reward.Type.CARD_BACK:
                    flag = NetCache.ProfileNotice.NoticeOrigin.SEASON != data.Origin;
                    goto Label_026F;

                case Reward.Type.FORGE_TICKET:
                {
                    bool flag3 = false;
                    if ((data.Origin == NetCache.ProfileNotice.NoticeOrigin.BLIZZCON) && (data.OriginData == 0x7ddL))
                    {
                        flag3 = true;
                    }
                    if (data.Origin == NetCache.ProfileNotice.NoticeOrigin.OUT_OF_BAND_LICENSE)
                    {
                        Log.Rachelle.Print(string.Format("RewardUtils.GetViewableRewards(): auto-acking notices for out of band license reward {0}", data), new object[0]);
                        flag3 = true;
                    }
                    if (flag3)
                    {
                        data.AcknowledgeNotices();
                    }
                    flag = false;
                    goto Label_026F;
                }
                default:
                    goto Label_026F;
            }
            flag = true;
        Label_026F:
            if (flag)
            {
                rewardsToShow.Add(data);
            }
        }
    }

    [DebuggerHidden]
    private static IEnumerator NotifyOfExpertPacksNeeded(Notification innkeeperQuote)
    {
        return new <NotifyOfExpertPacksNeeded>c__Iterator1B8 { innkeeperQuote = innkeeperQuote, <$>innkeeperQuote = innkeeperQuote };
    }

    private static void ShowInnkeeperQuoteForReward(Reward reward)
    {
        if (reward != null)
        {
            if (reward.RewardType == Reward.Type.CARD)
            {
                CardRewardData data = reward.Data as CardRewardData;
                switch (data.InnKeeperLine)
                {
                    case CardRewardData.InnKeeperTrigger.CORE_CLASS_SET_COMPLETE:
                    {
                        Notification innkeeperQuote = NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_BASIC_DONE1_11"), "VO_INNKEEPER_BASIC_DONE1_11", 0f, null);
                        if (!Options.Get().GetBool(Option.HAS_SEEN_ALL_BASIC_CLASS_CARDS_COMPLETE, false))
                        {
                            SceneMgr.Get().StartCoroutine(NotifyOfExpertPacksNeeded(innkeeperQuote));
                        }
                        break;
                    }
                    case CardRewardData.InnKeeperTrigger.SECOND_REWARD_EVER:
                        if (!Options.Get().GetBool(Option.HAS_BEEN_NUDGED_TO_CM, false))
                        {
                            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_NUDGE_CM_X"), "VO_INNKEEPER2_NUDGE_COLLECTION_10", 0f, null);
                            Options.Get().SetBool(Option.HAS_BEEN_NUDGED_TO_CM, true);
                        }
                        break;
                }
            }
            else if (reward.RewardType == Reward.Type.BOOSTER_PACK)
            {
                Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_RECEIVED_BOOSTER_PACK);
            }
        }
    }

    public static void ShowReward(Reward reward, bool updateCacheValues, Vector3 rewardPunchScale, Vector3 rewardScale, AnimationUtil.DelOnShownWithPunch callback, object callbackData)
    {
        ShowReward_Internal(reward, updateCacheValues, rewardPunchScale, rewardScale, string.Empty, null, callback, callbackData);
    }

    public static void ShowReward(Reward reward, bool updateCacheValues, Vector3 rewardPunchScale, Vector3 rewardScale, string callbackName = "", object callbackData = null, GameObject callbackGO = null)
    {
        ShowReward_Internal(reward, updateCacheValues, rewardPunchScale, rewardScale, callbackName, callbackGO, null, callbackData);
    }

    private static void ShowReward_Internal(Reward reward, bool updateCacheValues, Vector3 rewardPunchScale, Vector3 rewardScale, string gameObjectCallbackName, GameObject callbackGO, AnimationUtil.DelOnShownWithPunch onShowPunchCallback, object callbackData)
    {
        if (reward != null)
        {
            RenderUtils.SetAlpha(reward.gameObject, 0f);
            AnimationUtil.ShowWithPunch(reward.gameObject, REWARD_HIDDEN_SCALE, rewardPunchScale, rewardScale, gameObjectCallbackName, false, callbackGO, callbackData, onShowPunchCallback);
            reward.Show(updateCacheValues);
            ShowInnkeeperQuoteForReward(reward);
        }
    }

    public static void SortRewards(ref List<Reward> rewards)
    {
        if (rewards != null)
        {
            if (<>f__am$cache2 == null)
            {
                <>f__am$cache2 = delegate (Reward r1, Reward r2) {
                    if (r1.RewardType == r2.RewardType)
                    {
                        if (r1.RewardType != Reward.Type.CARD)
                        {
                            return 0;
                        }
                        CardRewardData data = r1.Data as CardRewardData;
                        CardRewardData data2 = r2.Data as CardRewardData;
                        EntityDef entityDef = DefLoader.Get().GetEntityDef(data.CardID);
                        EntityDef def2 = DefLoader.Get().GetEntityDef(data2.CardID);
                        bool flag = TAG_CARDTYPE.HERO == entityDef.GetCardType();
                        bool flag2 = TAG_CARDTYPE.HERO == def2.GetCardType();
                        if (flag == flag2)
                        {
                            return 0;
                        }
                        return !flag ? 1 : -1;
                    }
                    if (r1.RewardType == Reward.Type.CARD_BACK)
                    {
                        return -1;
                    }
                    if (r2.RewardType == Reward.Type.CARD_BACK)
                    {
                        return 1;
                    }
                    if (r1.RewardType == Reward.Type.CARD)
                    {
                        return -1;
                    }
                    if (r2.RewardType == Reward.Type.CARD)
                    {
                        return 1;
                    }
                    if (r1.RewardType == Reward.Type.BOOSTER_PACK)
                    {
                        return -1;
                    }
                    if (r2.RewardType == Reward.Type.BOOSTER_PACK)
                    {
                        return 1;
                    }
                    if (r1.RewardType == Reward.Type.MOUNT)
                    {
                        return -1;
                    }
                    if (r2.RewardType == Reward.Type.MOUNT)
                    {
                        return 1;
                    }
                    return 0;
                };
            }
            rewards.Sort(<>f__am$cache2);
        }
    }

    [CompilerGenerated]
    private sealed class <GetDuplicateCardDataReward>c__AnonStorey323
    {
        internal CardRewardData newCardRewardData;

        internal bool <>m__153(RewardData obj)
        {
            if (!(obj is CardRewardData))
            {
                return false;
            }
            CardRewardData data = obj as CardRewardData;
            if (!data.CardID.Equals(this.newCardRewardData.CardID))
            {
                return false;
            }
            if (!data.Premium.Equals(this.newCardRewardData.Premium))
            {
                return false;
            }
            if (!data.Origin.Equals(this.newCardRewardData.Origin))
            {
                return false;
            }
            return data.OriginData.Equals(this.newCardRewardData.OriginData);
        }
    }

    [CompilerGenerated]
    private sealed class <GetViewableRewards>c__AnonStorey322
    {
        internal Achievement completedQuest;

        internal bool <>m__151(Achievement obj)
        {
            return (this.completedQuest.ID == obj.ID);
        }
    }

    [CompilerGenerated]
    private sealed class <NotifyOfExpertPacksNeeded>c__Iterator1B8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Notification <$>innkeeperQuote;
        internal Notification innkeeperQuote;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (this.innkeeperQuote.GetAudio() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                    }
                    else
                    {
                        this.$current = new WaitForSeconds(this.innkeeperQuote.GetAudio().clip.length);
                        this.$PC = 2;
                    }
                    return true;

                case 2:
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_BASIC_DONE2_12"), "VO_INNKEEPER_BASIC_DONE2_12", 0f, null);
                    Options.Get().SetBool(Option.HAS_SEEN_ALL_BASIC_CLASS_CARDS_COMPLETE, true);
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

