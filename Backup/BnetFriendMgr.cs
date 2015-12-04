using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class BnetFriendMgr
{
    private List<ChangeListener> m_changeListeners = new List<ChangeListener>();
    private List<BnetPlayer> m_friends = new List<BnetPlayer>();
    private int m_maxFriends;
    private int m_maxReceivedInvites;
    private int m_maxSentInvites;
    private PendingBnetFriendChangelist m_pendingChangelist = new PendingBnetFriendChangelist();
    private List<BnetInvitation> m_receivedInvites = new List<BnetInvitation>();
    private List<BnetInvitation> m_sentInvites = new List<BnetInvitation>();
    private static BnetFriendMgr s_instance;

    public void AcceptInvite(BnetInvitationId inviteId)
    {
        Network.AcceptFriendInvite(inviteId);
    }

    public bool AddChangeListener(ChangeCallback callback)
    {
        return this.AddChangeListener(callback, null);
    }

    public bool AddChangeListener(ChangeCallback callback, object userData)
    {
        ChangeListener item = new ChangeListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_changeListeners.Contains(item))
        {
            return false;
        }
        this.m_changeListeners.Add(item);
        return true;
    }

    private void AddPendingFriend(BnetPlayer friend)
    {
        if (this.m_pendingChangelist.Add(friend) && (this.m_pendingChangelist.GetCount() == 1))
        {
            BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPendingPlayersChanged));
        }
    }

    private void Clear()
    {
        this.m_friends.Clear();
        this.m_receivedInvites.Clear();
        this.m_sentInvites.Clear();
        this.m_pendingChangelist.Clear();
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPendingPlayersChanged));
    }

    public void DeclineInvite(BnetInvitationId inviteId)
    {
        Network.DeclineFriendInvite(inviteId);
    }

    public BnetPlayer FindFriend(BnetAccountId id)
    {
        BnetPlayer player = this.FindNonPendingFriend(id);
        if (player != null)
        {
            return player;
        }
        player = this.FindPendingFriend(id);
        if (player != null)
        {
            return player;
        }
        return null;
    }

    public BnetPlayer FindFriend(BnetGameAccountId id)
    {
        BnetPlayer player = this.FindNonPendingFriend(id);
        if (player != null)
        {
            return player;
        }
        player = this.FindPendingFriend(id);
        if (player != null)
        {
            return player;
        }
        return null;
    }

    public BnetPlayer FindNonPendingFriend(BnetAccountId id)
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

    public BnetPlayer FindNonPendingFriend(BnetGameAccountId id)
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

    public BnetPlayer FindPendingFriend(BnetAccountId id)
    {
        return this.m_pendingChangelist.FindFriend(id);
    }

    public BnetPlayer FindPendingFriend(BnetGameAccountId id)
    {
        return this.m_pendingChangelist.FindFriend(id);
    }

    private void FireChangeEvent(BnetFriendChangelist changelist)
    {
        foreach (ChangeListener listener in this.m_changeListeners.ToArray())
        {
            listener.Fire(changelist);
        }
    }

    private void FirePendingFriendsChangedEvent()
    {
        BnetFriendChangelist changelist = this.m_pendingChangelist.CreateChangelist();
        if (this.m_pendingChangelist.GetCount() == 0)
        {
            BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPendingPlayersChanged));
        }
        this.FireChangeEvent(changelist);
    }

    public static BnetFriendMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new BnetFriendMgr();
            ApplicationMgr.Get().WillReset += new System.Action(s_instance.Clear);
        }
        return s_instance;
    }

    public int GetActiveOnlineFriendCount()
    {
        int num = 0;
        foreach (BnetPlayer player in this.m_friends)
        {
            if ((player.IsOnline() && (player.GetBestProgramId() != null)) && (!player.GetBestProgramId().IsPhoenix() || ((player.GetBestAwayTimeMicrosec() <= 0L) && !player.IsBusy())))
            {
                num++;
            }
        }
        return num;
    }

    public int GetFriendCount()
    {
        return this.m_friends.Count;
    }

    public List<BnetPlayer> GetFriends()
    {
        return this.m_friends;
    }

    public int GetMaxFriends()
    {
        return this.m_maxFriends;
    }

    public int GetMaxReceivedInvites()
    {
        return this.m_maxReceivedInvites;
    }

    public int GetMaxSentInvites()
    {
        return this.m_maxSentInvites;
    }

    public int GetOnlineFriendCount()
    {
        int num = 0;
        foreach (BnetPlayer player in this.m_friends)
        {
            if (player.IsOnline())
            {
                num++;
            }
        }
        return num;
    }

    public List<BnetPlayer> GetPendingFriends()
    {
        return this.m_pendingChangelist.GetFriends();
    }

    public List<BnetInvitation> GetReceivedInvites()
    {
        return this.m_receivedInvites;
    }

    public List<BnetInvitation> GetSentInvites()
    {
        return this.m_sentInvites;
    }

    public void IgnoreInvite(BnetInvitationId inviteId)
    {
        Network.IgnoreFriendInvite(inviteId);
    }

    public void Initialize()
    {
        FriendMgr.Get();
        BnetEventMgr.Get().AddChangeListener(new BnetEventMgr.ChangeCallback(this.OnBnetEventOccurred));
        Network.Get().SetFriendsHandler(new Network.FriendsHandler(this.OnFriendsUpdate));
        Network.Get().AddBnetErrorListener(BnetFeature.Friends, new Network.BnetErrorCallback(this.OnBnetError));
        this.InitMaximums();
    }

    private void InitMaximums()
    {
        BattleNet.DllFriendsInfo info = new BattleNet.DllFriendsInfo();
        BattleNet.GetFriendsInfo(ref info);
        this.m_maxFriends = info.maxFriends;
        this.m_maxReceivedInvites = info.maxRecvInvites;
        this.m_maxSentInvites = info.maxSentInvites;
    }

    public bool IsFriend(BnetAccountId id)
    {
        return (this.IsNonPendingFriend(id) || this.IsPendingFriend(id));
    }

    public bool IsFriend(BnetGameAccountId id)
    {
        return (this.IsNonPendingFriend(id) || this.IsPendingFriend(id));
    }

    public bool IsFriend(BnetPlayer player)
    {
        return (this.IsNonPendingFriend(player) || this.IsPendingFriend(player));
    }

    public bool IsNonPendingFriend(BnetAccountId id)
    {
        return (this.FindNonPendingFriend(id) != null);
    }

    public bool IsNonPendingFriend(BnetGameAccountId id)
    {
        return (this.FindNonPendingFriend(id) != null);
    }

    public bool IsNonPendingFriend(BnetPlayer player)
    {
        if (player != null)
        {
            if (this.m_friends.Contains(player))
            {
                return true;
            }
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

    public bool IsPendingFriend(BnetAccountId id)
    {
        return this.m_pendingChangelist.IsFriend(id);
    }

    public bool IsPendingFriend(BnetGameAccountId id)
    {
        return this.m_pendingChangelist.IsFriend(id);
    }

    public bool IsPendingFriend(BnetPlayer player)
    {
        return this.m_pendingChangelist.IsFriend(player);
    }

    private bool OnBnetError(BnetErrorInfo info, object userData)
    {
        object[] args = new object[] { info.GetFeatureEvent(), info.GetError() };
        Log.Mike.Print("BnetFriendMgr.OnBnetError() - event={0} error={1}", args);
        return true;
    }

    private void OnBnetEventOccurred(BattleNet.BnetEvent bnetEvent, object userData)
    {
        if (bnetEvent == BattleNet.BnetEvent.Disconnected)
        {
            this.Clear();
        }
    }

    private void OnFriendsUpdate(BattleNet.FriendsUpdate[] updates)
    {
        BnetFriendChangelist changelist = new BnetFriendChangelist();
        foreach (BattleNet.FriendsUpdate update in updates)
        {
            switch (((BattleNet.FriendsUpdate.Action) update.action))
            {
                case BattleNet.FriendsUpdate.Action.FRIEND_REMOVED:
                {
                    BnetAccountId id2 = BnetAccountId.CreateFromBnetEntityId(update.entity1);
                    BnetPlayer item = BnetPresenceMgr.Get().GetPlayer(id2);
                    this.m_friends.Remove(item);
                    changelist.AddRemovedFriend(item);
                    this.RemovePendingFriend(item);
                    break;
                }
                case BattleNet.FriendsUpdate.Action.FRIEND_INVITE:
                {
                    BnetInvitation invitation = BnetInvitation.CreateFromFriendsUpdate(update);
                    this.m_receivedInvites.Add(invitation);
                    changelist.AddAddedReceivedInvite(invitation);
                    break;
                }
                case BattleNet.FriendsUpdate.Action.FRIEND_INVITE_REMOVED:
                {
                    BnetInvitation invitation2 = BnetInvitation.CreateFromFriendsUpdate(update);
                    this.m_receivedInvites.Remove(invitation2);
                    changelist.AddRemovedReceivedInvite(invitation2);
                    break;
                }
                case BattleNet.FriendsUpdate.Action.FRIEND_SENT_INVITE:
                {
                    BnetInvitation invitation3 = BnetInvitation.CreateFromFriendsUpdate(update);
                    this.m_sentInvites.Add(invitation3);
                    changelist.AddAddedSentInvite(invitation3);
                    break;
                }
                case BattleNet.FriendsUpdate.Action.FRIEND_SENT_INVITE_REMOVED:
                {
                    BnetInvitation invitation4 = BnetInvitation.CreateFromFriendsUpdate(update);
                    this.m_sentInvites.Remove(invitation4);
                    changelist.AddRemovedSentInvite(invitation4);
                    break;
                }
                case BattleNet.FriendsUpdate.Action.FRIEND_ADDED:
                {
                    BnetAccountId id = BnetAccountId.CreateFromBnetEntityId(update.entity1);
                    BnetPlayer player = BnetPresenceMgr.Get().RegisterPlayer(id);
                    if (player.IsDisplayable())
                    {
                        this.m_friends.Add(player);
                        changelist.AddAddedFriend(player);
                    }
                    else
                    {
                        this.AddPendingFriend(player);
                    }
                    break;
                }
            }
        }
        if (!changelist.IsEmpty())
        {
            this.FireChangeEvent(changelist);
        }
    }

    private void OnPendingPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        this.ProcessPendingFriends();
    }

    private void ProcessPendingFriends()
    {
        bool flag = false;
        foreach (BnetPlayer player in this.m_pendingChangelist.GetFriends())
        {
            if (player.IsDisplayable())
            {
                flag = true;
                this.m_friends.Add(player);
            }
        }
        if (flag)
        {
            this.FirePendingFriendsChangedEvent();
        }
    }

    public bool RemoveChangeListener(ChangeCallback callback)
    {
        return this.RemoveChangeListener(callback, null);
    }

    public bool RemoveChangeListener(ChangeCallback callback, object userData)
    {
        ChangeListener item = new ChangeListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_changeListeners.Remove(item);
    }

    public bool RemoveFriend(BnetPlayer friend)
    {
        if (!this.m_friends.Contains(friend))
        {
            return false;
        }
        Network.RemoveFriend(friend.GetAccountId());
        return true;
    }

    private void RemovePendingFriend(BnetPlayer friend)
    {
        if (this.m_pendingChangelist.Remove(friend))
        {
            if (this.m_pendingChangelist.GetCount() == 0)
            {
                BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPendingPlayersChanged));
            }
            else
            {
                this.ProcessPendingFriends();
            }
        }
    }

    public void RevokeInvite(BnetInvitationId inviteId)
    {
        Network.RevokeFriendInvite(inviteId);
    }

    public void SendInvite(string name)
    {
        if (name.Contains("@"))
        {
            this.SendInviteByEmail(name);
        }
        else
        {
            this.SendInviteByBattleTag(name);
        }
    }

    public void SendInviteByBattleTag(string battleTagString)
    {
        Network.SendFriendInviteByBattleTag(BnetPresenceMgr.Get().GetMyPlayer().GetBattleTag().GetString(), battleTagString);
    }

    public void SendInviteByEmail(string email)
    {
        Network.SendFriendInviteByEmail(BnetPresenceMgr.Get().GetMyPlayer().GetFullName(), email);
    }

    public void Shutdown()
    {
        Network.Get().RemoveBnetErrorListener(BnetFeature.Friends, new Network.BnetErrorCallback(this.OnBnetError));
        Network.Get().SetFriendsHandler(null);
    }

    public delegate void ChangeCallback(BnetFriendChangelist changelist, object userData);

    private class ChangeListener : EventListener<BnetFriendMgr.ChangeCallback>
    {
        public void Fire(BnetFriendChangelist changelist)
        {
            base.m_callback(changelist, base.m_userData);
        }
    }
}

