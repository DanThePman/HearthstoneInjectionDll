using System;
using System.Collections.Generic;

public class BnetNearbyPlayerChangelist
{
    private List<BnetPlayer> m_friendsAdded;
    private List<BnetPlayer> m_friendsRemoved;
    private List<BnetPlayer> m_friendsUpdated;
    private List<BnetPlayer> m_playersAdded;
    private List<BnetPlayer> m_playersRemoved;
    private List<BnetPlayer> m_playersUpdated;
    private List<BnetPlayer> m_strangersAdded;
    private List<BnetPlayer> m_strangersRemoved;
    private List<BnetPlayer> m_strangersUpdated;

    public bool AddAddedFriend(BnetPlayer friend)
    {
        if (this.m_friendsAdded == null)
        {
            this.m_friendsAdded = new List<BnetPlayer>();
        }
        else if (this.m_friendsAdded.Contains(friend))
        {
            return false;
        }
        this.m_friendsAdded.Add(friend);
        return true;
    }

    public bool AddAddedPlayer(BnetPlayer player)
    {
        if (this.m_playersAdded == null)
        {
            this.m_playersAdded = new List<BnetPlayer>();
        }
        else if (this.m_playersAdded.Contains(player))
        {
            return false;
        }
        this.m_playersAdded.Add(player);
        return true;
    }

    public bool AddAddedStranger(BnetPlayer stranger)
    {
        if (this.m_strangersAdded == null)
        {
            this.m_strangersAdded = new List<BnetPlayer>();
        }
        else if (this.m_strangersAdded.Contains(stranger))
        {
            return false;
        }
        this.m_strangersAdded.Add(stranger);
        return true;
    }

    public bool AddRemovedFriend(BnetPlayer friend)
    {
        if (this.m_friendsRemoved == null)
        {
            this.m_friendsRemoved = new List<BnetPlayer>();
        }
        else if (this.m_friendsRemoved.Contains(friend))
        {
            return false;
        }
        this.m_friendsRemoved.Add(friend);
        return true;
    }

    public bool AddRemovedPlayer(BnetPlayer player)
    {
        if (this.m_playersRemoved == null)
        {
            this.m_playersRemoved = new List<BnetPlayer>();
        }
        else if (this.m_playersRemoved.Contains(player))
        {
            return false;
        }
        this.m_playersRemoved.Add(player);
        return true;
    }

    public bool AddRemovedStranger(BnetPlayer stranger)
    {
        if (this.m_strangersRemoved == null)
        {
            this.m_strangersRemoved = new List<BnetPlayer>();
        }
        else if (this.m_strangersRemoved.Contains(stranger))
        {
            return false;
        }
        this.m_strangersRemoved.Add(stranger);
        return true;
    }

    public bool AddUpdatedFriend(BnetPlayer friend)
    {
        if (this.m_friendsUpdated == null)
        {
            this.m_friendsUpdated = new List<BnetPlayer>();
        }
        else if (this.m_friendsUpdated.Contains(friend))
        {
            return false;
        }
        this.m_friendsUpdated.Add(friend);
        return true;
    }

    public bool AddUpdatedPlayer(BnetPlayer player)
    {
        if (this.m_playersUpdated == null)
        {
            this.m_playersUpdated = new List<BnetPlayer>();
        }
        else if (this.m_playersUpdated.Contains(player))
        {
            return false;
        }
        this.m_playersUpdated.Add(player);
        return true;
    }

    public bool AddUpdatedStranger(BnetPlayer stranger)
    {
        if (this.m_strangersUpdated == null)
        {
            this.m_strangersUpdated = new List<BnetPlayer>();
        }
        else if (this.m_strangersUpdated.Contains(stranger))
        {
            return false;
        }
        this.m_strangersUpdated.Add(stranger);
        return true;
    }

    public void Clear()
    {
        this.ClearAddedPlayers();
        this.ClearUpdatedPlayers();
        this.ClearRemovedPlayers();
        this.ClearAddedFriends();
        this.ClearUpdatedFriends();
        this.ClearRemovedFriends();
        this.ClearAddedStrangers();
        this.ClearUpdatedStrangers();
        this.ClearRemovedStrangers();
    }

    public void ClearAddedFriends()
    {
        this.m_friendsAdded = null;
    }

    public void ClearAddedPlayers()
    {
        this.m_playersAdded = null;
    }

    public void ClearAddedStrangers()
    {
        this.m_strangersAdded = null;
    }

    public void ClearRemovedFriends()
    {
        this.m_friendsRemoved = null;
    }

    public void ClearRemovedPlayers()
    {
        this.m_playersRemoved = null;
    }

    public void ClearRemovedStrangers()
    {
        this.m_strangersRemoved = null;
    }

    public void ClearUpdatedFriends()
    {
        this.m_friendsUpdated = null;
    }

    public void ClearUpdatedPlayers()
    {
        this.m_playersUpdated = null;
    }

    public void ClearUpdatedStrangers()
    {
        this.m_strangersUpdated = null;
    }

    public List<BnetPlayer> GetAddedFriends()
    {
        return this.m_friendsAdded;
    }

    public List<BnetPlayer> GetAddedPlayers()
    {
        return this.m_playersAdded;
    }

    public List<BnetPlayer> GetAddedStrangers()
    {
        return this.m_strangersAdded;
    }

    public List<BnetPlayer> GetRemovedFriends()
    {
        return this.m_friendsRemoved;
    }

    public List<BnetPlayer> GetRemovedPlayers()
    {
        return this.m_playersRemoved;
    }

    public List<BnetPlayer> GetRemovedStrangers()
    {
        return this.m_strangersRemoved;
    }

    public List<BnetPlayer> GetUpdatedFriends()
    {
        return this.m_friendsUpdated;
    }

    public List<BnetPlayer> GetUpdatedPlayers()
    {
        return this.m_playersUpdated;
    }

    public List<BnetPlayer> GetUpdatedStrangers()
    {
        return this.m_strangersUpdated;
    }

    public bool IsEmpty()
    {
        if ((this.m_playersAdded != null) && (this.m_playersAdded.Count > 0))
        {
            return false;
        }
        if ((this.m_playersUpdated != null) && (this.m_playersUpdated.Count > 0))
        {
            return false;
        }
        if ((this.m_playersRemoved != null) && (this.m_playersRemoved.Count > 0))
        {
            return false;
        }
        if ((this.m_friendsAdded != null) && (this.m_friendsAdded.Count > 0))
        {
            return false;
        }
        if ((this.m_friendsUpdated != null) && (this.m_friendsUpdated.Count > 0))
        {
            return false;
        }
        if ((this.m_friendsRemoved != null) && (this.m_friendsRemoved.Count > 0))
        {
            return false;
        }
        if ((this.m_strangersAdded != null) && (this.m_strangersAdded.Count > 0))
        {
            return false;
        }
        if ((this.m_strangersUpdated != null) && (this.m_strangersUpdated.Count > 0))
        {
            return false;
        }
        if ((this.m_strangersRemoved != null) && (this.m_strangersRemoved.Count > 0))
        {
            return false;
        }
        return true;
    }

    public bool RemoveAddedFriend(BnetPlayer friend)
    {
        if (this.m_friendsAdded == null)
        {
            return false;
        }
        return this.m_friendsAdded.Remove(friend);
    }

    public bool RemoveAddedPlayer(BnetPlayer player)
    {
        if (this.m_playersAdded == null)
        {
            return false;
        }
        return this.m_playersAdded.Remove(player);
    }

    public bool RemoveAddedStranger(BnetPlayer stranger)
    {
        if (this.m_strangersAdded == null)
        {
            return false;
        }
        return this.m_strangersAdded.Remove(stranger);
    }

    public bool RemoveRemovedFriend(BnetPlayer friend)
    {
        if (this.m_friendsRemoved == null)
        {
            return false;
        }
        return this.m_friendsRemoved.Remove(friend);
    }

    public bool RemoveRemovedPlayer(BnetPlayer player)
    {
        if (this.m_playersRemoved == null)
        {
            return false;
        }
        return this.m_playersRemoved.Remove(player);
    }

    public bool RemoveRemovedStranger(BnetPlayer stranger)
    {
        if (this.m_strangersRemoved == null)
        {
            return false;
        }
        return this.m_strangersRemoved.Remove(stranger);
    }

    public bool RemoveUpdatedFriend(BnetPlayer friend)
    {
        if (this.m_friendsUpdated == null)
        {
            return false;
        }
        return this.m_friendsUpdated.Remove(friend);
    }

    public bool RemoveUpdatedPlayer(BnetPlayer player)
    {
        if (this.m_playersUpdated == null)
        {
            return false;
        }
        return this.m_playersUpdated.Remove(player);
    }

    public bool RemoveUpdatedStranger(BnetPlayer stranger)
    {
        if (this.m_strangersUpdated == null)
        {
            return false;
        }
        return this.m_strangersUpdated.Remove(stranger);
    }
}

