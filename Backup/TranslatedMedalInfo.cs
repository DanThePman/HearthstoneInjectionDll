using System;

public class TranslatedMedalInfo
{
    public int bestRank = -1;
    public bool canLoseLevel;
    public bool canLoseStars;
    public int earnedStars;
    public int legendIndex;
    public string name;
    public string nextMedalName;
    public int rank = -1;
    public int streakStars;
    public string textureName;
    public int totalStars;
    public int winStreak;

    public bool CanGetRankedRewardChest()
    {
        return (this.canLoseStars || this.IsLegendRank());
    }

    public bool IsHighestRankThatCannotBeLost()
    {
        return (this.canLoseStars && !this.canLoseLevel);
    }

    public bool IsLegendRank()
    {
        return (this.rank == 0);
    }

    public override string ToString()
    {
        object[] args = new object[] { this.totalStars, this.earnedStars, this.rank, this.name, this.canLoseStars, this.canLoseLevel };
        return string.Format("[{3} totalStars={0} earnedStars={1} rank={2} canLoseStars={4} canLoseLevel={5}]", args);
    }
}

