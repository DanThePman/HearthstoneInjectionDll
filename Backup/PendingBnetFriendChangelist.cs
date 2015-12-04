using System;
using System.Collections.Generic;

public class PendingBnetFriendChangelist
{
    private List<BnetPlayer> m_friends = new List<BnetPlayer>();

    public bool Add(BnetPlayer friend)
    {
        if (this.m_friends.Contains(friend))
        {
            return false;
        }
        this.m_friends.Add(friend);
        return true;
    }

    public void Clear()
    {
        this.m_friends.Clear();
    }

    public BnetFriendChangelist CreateChangelist()
    {
        BnetFriendChangelist changelist = new BnetFriendChangelist();
        for (int i = this.m_friends.Count - 1; i >= 0; i--)
        {
            BnetPlayer friend = this.m_friends[i];
            if (friend.IsDisplayable())
            {
                changelist.AddAddedFriend(friend);
                this.m_friends.RemoveAt(i);
            }
        }
        return changelist;
    }

    public BnetPlayer FindFriend(BnetAccountId id)
    {
        foreach (BnetPlayer player in this.m_friends)
        {
            if (player.GetAccountId() == id)
            {
                return player;
            }
        }
        return null;
    }

    public BnetPlayer FindFriend(BnetGameAccountId id)
    {
        foreach (BnetPlayer player in this.m_friends)
        {
            if (player.HasGameAccount(id))
            {
                return player;
            }
        }
        return null;
    }

    public int GetCount()
    {
        return this.m_friends.Count;
    }

    public List<BnetPlayer> GetFriends()
    {
        return this.m_friends;
    }

    public bool IsFriend(BnetAccountId id)
    {
        return (this.FindFriend(id) != null);
    }

    public bool IsFriend(BnetGameAccountId id)
    {
        return (this.FindFriend(id) != null);
    }

    public bool IsFriend(BnetPlayer player)
    {
        if (this.m_friends.Contains(player))
        {
            return true;
        }
        if (player != null)
        {
            BnetAccountId accountId = player.GetAccountId();
            if (accountId != null)
            {
                return this.IsFriend(accountId);
            }
            foreach (BnetGameAccountId id2 in player.GetGameAccounts().Keys)
            {
                if (this.IsFriend(id2))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool Remove(BnetPlayer friend)
    {
        return this.m_friends.Remove(friend);
    }
}

