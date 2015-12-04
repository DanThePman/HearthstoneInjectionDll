using System;
using UnityEngine;

public class MedalInfoTranslator
{
    public const int LEGEND_RANK_VALUE = 0;
    public const int LOWEST_LEGEND_VALUE = -1;
    private TranslatedMedalInfo m_currMedalInfo;
    private TranslatedMedalInfo m_prevMedalInfo;
    private bool m_vaidPrevMedal;
    public const int MAX_RANK = 0x1a;
    public const string MEDAL_NAME_PREFIX = "GLOBAL_MEDAL_";
    public const string MEDAL_TEXTURE_PREFIX = "Medal_Ranked_";
    public const int WORST_DISPLAYABLE_RANK = 0x19;

    public MedalInfoTranslator()
    {
        new MedalInfoTranslator(-1, 0);
    }

    public MedalInfoTranslator(NetCache.NetCacheMedalInfo currMedalInfo)
    {
        this.m_currMedalInfo = this.Translate(currMedalInfo);
    }

    public MedalInfoTranslator(NetCache.NetCacheMedalInfo currMedalInfo, NetCache.NetCacheMedalInfo prevMedalInfo)
    {
        this.m_currMedalInfo = this.Translate(currMedalInfo);
        this.m_vaidPrevMedal = prevMedalInfo != null;
        this.m_prevMedalInfo = !this.m_vaidPrevMedal ? this.Translate(currMedalInfo) : this.Translate(prevMedalInfo);
    }

    public MedalInfoTranslator(int rank, int legendIndex)
    {
        TranslatedMedalInfo info = new TranslatedMedalInfo();
        rank = Mathf.Clamp(rank, 0, 0x19);
        legendIndex = Mathf.Clamp(legendIndex, -1, legendIndex);
        info.rank = rank;
        info.legendIndex = legendIndex;
        info.textureName = "Medal_Ranked_" + info.rank;
        info.name = GameStrings.Get("GLOBAL_MEDAL_" + info.rank);
        string key = "GLOBAL_MEDAL_" + (info.rank - 1);
        string str2 = GameStrings.Get(key);
        if (str2 != key)
        {
            info.nextMedalName = str2;
        }
        else
        {
            info.nextMedalName = string.Empty;
        }
        this.m_currMedalInfo = info;
    }

    public RankChangeType GetChangeType()
    {
        if ((this.m_prevMedalInfo == null) || (this.m_currMedalInfo == null))
        {
            return RankChangeType.UNKNOWN;
        }
        if (this.m_prevMedalInfo.rank < this.m_currMedalInfo.rank)
        {
            return RankChangeType.RANK_DOWN;
        }
        if (this.m_currMedalInfo.rank < this.m_prevMedalInfo.rank)
        {
            return RankChangeType.RANK_UP;
        }
        return RankChangeType.RANK_SAME;
    }

    public TranslatedMedalInfo GetCurrentMedal()
    {
        return this.m_currMedalInfo;
    }

    public TranslatedMedalInfo GetPreviousMedal()
    {
        return this.m_prevMedalInfo;
    }

    public bool IsDisplayable()
    {
        if (this.m_currMedalInfo == null)
        {
            return false;
        }
        if (this.m_currMedalInfo.rank == -1)
        {
            return false;
        }
        return true;
    }

    public bool IsPreviousMedalValid()
    {
        return this.m_vaidPrevMedal;
    }

    public bool ShowRewardChest()
    {
        if ((this.m_prevMedalInfo == null) || (this.m_currMedalInfo == null))
        {
            return false;
        }
        if (this.m_currMedalInfo.rank == 0)
        {
            return false;
        }
        return ((this.m_currMedalInfo.bestRank < this.m_prevMedalInfo.bestRank) && this.m_currMedalInfo.CanGetRankedRewardChest());
    }

    public void TestSetMedalInfo(TranslatedMedalInfo currMedal, TranslatedMedalInfo prevMedal)
    {
        this.m_currMedalInfo = currMedal;
        this.m_prevMedalInfo = prevMedal;
        this.m_vaidPrevMedal = true;
    }

    public TranslatedMedalInfo Translate(NetCache.NetCacheMedalInfo medalInfo)
    {
        TranslatedMedalInfo info = new TranslatedMedalInfo();
        if (medalInfo != null)
        {
            info.rank = 0x1a - medalInfo.StarLevel;
            info.bestRank = 0x1a - medalInfo.BestStarLevel;
            info.legendIndex = medalInfo.LegendIndex;
            info.totalStars = medalInfo.GainLevelStars - medalInfo.StartStars;
            info.earnedStars = medalInfo.Stars;
            if (medalInfo.StarLevel != 1)
            {
                info.earnedStars -= medalInfo.StartStars - 1;
            }
            info.winStreak = medalInfo.Streak;
            info.textureName = "Medal_Ranked_" + info.rank;
            info.name = GameStrings.Get("GLOBAL_MEDAL_" + info.rank);
            info.canLoseStars = medalInfo.CanLoseStars;
            info.canLoseLevel = medalInfo.CanLoseLevel;
            string key = "GLOBAL_MEDAL_" + (info.rank - 1);
            string str2 = GameStrings.Get(key);
            if (str2 != key)
            {
                info.nextMedalName = str2;
                return info;
            }
            info.nextMedalName = string.Empty;
        }
        return info;
    }
}

