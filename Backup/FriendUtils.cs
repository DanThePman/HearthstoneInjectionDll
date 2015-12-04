using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FriendUtils
{
    private static int FriendNameSortCompare(BnetPlayer friend1, BnetPlayer friend2)
    {
        int num = string.Compare(GetFriendListName(friend1, false), GetFriendListName(friend2, false), true);
        if (num != 0)
        {
            return num;
        }
        long lo = (long) friend1.GetAccountId().GetLo();
        long num3 = (long) friend2.GetAccountId().GetLo();
        return (int) (lo - num3);
    }

    public static int FriendSortCompare(BnetPlayer friend1, BnetPlayer friend2)
    {
        int result = 0;
        if ((friend1 == null) || (friend2 == null))
        {
            return ((friend1 != friend2) ? ((friend1 != null) ? -1 : 1) : 0);
        }
        if (friend1.IsOnline() || friend2.IsOnline())
        {
            if (friend1.IsOnline() && !friend2.IsOnline())
            {
                return -1;
            }
            if (!friend1.IsOnline() && friend2.IsOnline())
            {
                return 1;
            }
            BnetProgramId bestProgramId = friend1.GetBestProgramId();
            BnetProgramId id2 = friend2.GetBestProgramId();
            if (FriendSortFlagCompare(friend1, friend2, bestProgramId == BnetProgramId.HEARTHSTONE, id2 == BnetProgramId.HEARTHSTONE, out result))
            {
                return result;
            }
            bool lhsflag = (bestProgramId != null) ? bestProgramId.IsGame() : false;
            bool rhsflag = (id2 != null) ? id2.IsGame() : false;
            if (FriendSortFlagCompare(friend1, friend2, lhsflag, rhsflag, out result))
            {
                return result;
            }
            bool flag3 = (bestProgramId != null) ? bestProgramId.IsPhoenix() : false;
            bool flag4 = (id2 != null) ? id2.IsPhoenix() : false;
            if (FriendSortFlagCompare(friend1, friend2, flag3, flag4, out result))
            {
                return result;
            }
        }
        return FriendNameSortCompare(friend1, friend2);
    }

    private static bool FriendSortFlagCompare(BnetPlayer lhs, BnetPlayer rhs, bool lhsflag, bool rhsflag, out int result)
    {
        if (lhsflag && !rhsflag)
        {
            result = -1;
            return true;
        }
        if (!lhsflag && rhsflag)
        {
            result = 1;
            return true;
        }
        result = 0;
        return false;
    }

    public static string GetAwayTimeString(ulong epochMicrosec)
    {
        TimeUtils.ElapsedStringSet stringSet = new TimeUtils.ElapsedStringSet {
            m_seconds = "GLOBAL_DATETIME_AFK_SECONDS",
            m_minutes = "GLOBAL_DATETIME_AFK_MINUTES",
            m_hours = "GLOBAL_DATETIME_AFK_HOURS",
            m_yesterday = "GLOBAL_DATETIME_AFK_DAY",
            m_days = "GLOBAL_DATETIME_AFK_DAYS",
            m_weeks = "GLOBAL_DATETIME_AFK_WEEKS",
            m_monthAgo = "GLOBAL_DATETIME_AFK_MONTH"
        };
        return TimeUtils.GetElapsedTimeStringFromEpochMicrosec(epochMicrosec, stringSet);
    }

    public static string GetBattleTagWithColor(BnetBattleTag battleTag, string nameColorStr)
    {
        object[] args = new object[] { nameColorStr, battleTag.GetName(), "a1a1a1ff", battleTag.GetNumber() };
        return string.Format("<color=#{0}>{1}</color><color=#{2}>#{3}</color>", args);
    }

    public static string GetFriendListName(BnetPlayer friend, bool addColorTags)
    {
        string fullName = null;
        BnetAccount account = friend.GetAccount();
        if (account != null)
        {
            fullName = account.GetFullName();
            if ((fullName == null) && (account.GetBattleTag() != null))
            {
                fullName = account.GetBattleTag().ToString();
            }
        }
        if (fullName == null)
        {
            foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> pair in friend.GetGameAccounts())
            {
                if (pair.Value.GetBattleTag() != null)
                {
                    fullName = pair.Value.GetBattleTag().ToString();
                    break;
                }
            }
        }
        if (addColorTags)
        {
            string str2 = !friend.IsOnline() ? "999999ff" : "5ecaf0ff";
            return string.Format("<color=#{0}>{1}</color>", str2, fullName);
        }
        return fullName;
    }

    public static string GetLastOnlineElapsedTimeString(ulong epochMicrosec)
    {
        if (epochMicrosec == 0)
        {
            return GameStrings.Get("GLOBAL_OFFLINE");
        }
        TimeUtils.ElapsedStringSet stringSet = new TimeUtils.ElapsedStringSet {
            m_seconds = "GLOBAL_DATETIME_LASTONLINE_SECONDS",
            m_minutes = "GLOBAL_DATETIME_LASTONLINE_MINUTES",
            m_hours = "GLOBAL_DATETIME_LASTONLINE_HOURS",
            m_yesterday = "GLOBAL_DATETIME_LASTONLINE_DAY",
            m_days = "GLOBAL_DATETIME_LASTONLINE_DAYS",
            m_weeks = "GLOBAL_DATETIME_LASTONLINE_WEEKS",
            m_monthAgo = "GLOBAL_DATETIME_LASTONLINE_MONTH"
        };
        return TimeUtils.GetElapsedTimeStringFromEpochMicrosec(epochMicrosec, stringSet);
    }

    public static string GetRequestElapsedTimeString(ulong epochMicrosec)
    {
        TimeUtils.ElapsedStringSet stringSet = new TimeUtils.ElapsedStringSet {
            m_seconds = "GLOBAL_DATETIME_FRIENDREQUEST_SECONDS",
            m_minutes = "GLOBAL_DATETIME_FRIENDREQUEST_MINUTES",
            m_hours = "GLOBAL_DATETIME_FRIENDREQUEST_HOURS",
            m_yesterday = "GLOBAL_DATETIME_FRIENDREQUEST_DAY",
            m_days = "GLOBAL_DATETIME_FRIENDREQUEST_DAYS",
            m_weeks = "GLOBAL_DATETIME_FRIENDREQUEST_WEEKS",
            m_monthAgo = "GLOBAL_DATETIME_FRIENDREQUEST_MONTH"
        };
        return TimeUtils.GetElapsedTimeStringFromEpochMicrosec(epochMicrosec, stringSet);
    }

    public static string GetUniqueName(BnetPlayer friend)
    {
        BnetBattleTag tag;
        string str;
        if (GetUniqueName(friend, out tag, out str))
        {
            return tag.ToString();
        }
        return str;
    }

    private static bool GetUniqueName(BnetPlayer friend, out BnetBattleTag battleTag, out string name)
    {
        battleTag = friend.GetBattleTag();
        name = friend.GetBestName();
        if (battleTag != null)
        {
            if (BnetNearbyPlayerMgr.Get().IsNearbyStranger(friend))
            {
                return true;
            }
            foreach (BnetPlayer player in BnetFriendMgr.Get().GetFriends())
            {
                if (player != friend)
                {
                    string bestName = player.GetBestName();
                    if (string.Compare(name, bestName, true) == 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static string GetUniqueNameWithColor(BnetPlayer friend)
    {
        BnetBattleTag tag;
        string str2;
        string nameColorStr = !friend.IsOnline() ? "999999ff" : "5ecaf0ff";
        if (GetUniqueName(friend, out tag, out str2))
        {
            return GetBattleTagWithColor(tag, nameColorStr);
        }
        return string.Format("<color=#{0}>{1}</color>", nameColorStr, str2);
    }

    public static bool IsValidEmail(string emailString)
    {
        if (emailString != null)
        {
            int index = emailString.IndexOf('@');
            if ((index >= 1) && (index < (emailString.Length - 1)))
            {
                int num2 = emailString.LastIndexOf('.');
                if ((num2 > (index + 1)) && (num2 < (emailString.Length - 1)))
                {
                    return true;
                }
            }
        }
        return false;
    }
}

