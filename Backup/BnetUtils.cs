using System;

public static class BnetUtils
{
    public static bool CanReceiveChallengeFrom(BnetGameAccountId id)
    {
        return (BnetFriendMgr.Get().IsFriend(id) || BnetNearbyPlayerMgr.Get().IsNearbyStranger(id));
    }

    public static bool CanReceiveWhisperFrom(BnetGameAccountId id)
    {
        if (BnetPresenceMgr.Get().GetMyPlayer().IsBusy())
        {
            return false;
        }
        return BnetFriendMgr.Get().IsFriend(id);
    }

    public static string GetInviterBestName(PartyInvite invite)
    {
        if ((invite != null) && !string.IsNullOrEmpty(invite.InviterName))
        {
            return invite.InviterName;
        }
        BnetPlayer player = (invite != null) ? GetPlayer(invite.InviterId) : null;
        string str = (player != null) ? player.GetBestName() : null;
        if (string.IsNullOrEmpty(str))
        {
            str = GameStrings.Get("GLOBAL_PLAYER_PLAYER");
        }
        return str;
    }

    public static BnetPlayer GetPlayer(BnetGameAccountId id)
    {
        if (id == null)
        {
            return null;
        }
        BnetPlayer player = BnetNearbyPlayerMgr.Get().FindNearbyStranger(id);
        if (player == null)
        {
            player = BnetPresenceMgr.Get().GetPlayer(id);
        }
        return player;
    }

    public static string GetPlayerBestName(BnetGameAccountId id)
    {
        BnetPlayer player = GetPlayer(id);
        string str = (player != null) ? player.GetBestName() : null;
        if (string.IsNullOrEmpty(str))
        {
            str = GameStrings.Get("GLOBAL_PLAYER_PLAYER");
        }
        return str;
    }

    public static bool HasPlayerBestNamePresence(BnetGameAccountId id)
    {
        BnetPlayer player = GetPlayer(id);
        string str = (player != null) ? player.GetBestName() : null;
        return !string.IsNullOrEmpty(str);
    }
}

