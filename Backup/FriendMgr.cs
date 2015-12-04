using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class FriendMgr
{
    private float m_friendListScrollCamPosY;
    private bool m_friendListScrollEnabled;
    private BnetPlayer m_recentOpponent;
    private List<RecentOpponentListener> m_recentOpponentListeners = new List<RecentOpponentListener>();
    private BnetPlayer m_selectedFriend;
    private static FriendMgr s_instance;

    public void AddRecentOpponentListener(RecentOpponentCallback callback)
    {
        RecentOpponentListener item = new RecentOpponentListener();
        item.SetCallback(callback);
        item.SetUserData(null);
        if (!this.m_recentOpponentListeners.Contains(item))
        {
            this.m_recentOpponentListeners.Add(item);
        }
    }

    public void FireRecentOpponentEvent(BnetPlayer recentOpponent)
    {
        foreach (RecentOpponentListener listener in this.m_recentOpponentListeners.ToArray())
        {
            listener.Fire(recentOpponent);
        }
    }

    public static FriendMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new FriendMgr();
            s_instance.Initialize();
        }
        return s_instance;
    }

    public float GetFriendListScrollCamPosY()
    {
        return this.m_friendListScrollCamPosY;
    }

    public BnetPlayer GetRecentOpponent()
    {
        return this.m_recentOpponent;
    }

    public BnetPlayer GetSelectedFriend()
    {
        return this.m_selectedFriend;
    }

    private void Initialize()
    {
        BnetFriendMgr.Get().AddChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        Network.Get().AddBnetErrorListener(BnetFeature.Friends, new Network.BnetErrorCallback(this.OnBnetError));
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
    }

    public bool IsFriendListScrollEnabled()
    {
        return this.m_friendListScrollEnabled;
    }

    private bool OnBnetError(BnetErrorInfo info, object userData)
    {
        BnetFeature feature = info.GetFeature();
        BnetFeatureEvent featureEvent = info.GetFeatureEvent();
        if ((feature == BnetFeature.Friends) && (featureEvent == BnetFeatureEvent.Friends_OnSendInvitation))
        {
            string str;
            switch (info.GetError())
            {
                case BattleNetErrors.ERROR_OK:
                    str = GameStrings.Get("GLOBAL_ADDFRIEND_SENT_CONFIRMATION");
                    UIStatus.Get().AddInfo(str);
                    return true;

                case BattleNetErrors.ERROR_FRIENDS_FRIENDSHIP_ALREADY_EXISTS:
                    str = GameStrings.Get("GLOBAL_ADDFRIEND_ERROR_ALREADY_FRIEND");
                    UIStatus.Get().AddError(str);
                    return true;
            }
        }
        return false;
    }

    private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
    {
        List<BnetPlayer> removedFriends = changelist.GetRemovedFriends();
        if ((removedFriends != null) && removedFriends.Contains(this.m_selectedFriend))
        {
            this.m_selectedFriend = null;
        }
    }

    private void OnGameOver(object userData)
    {
        GameState.Get().UnregisterGameOverListener(new GameState.GameOverCallback(this.OnGameOver), null);
        this.UpdateRecentOpponent();
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        BnetPlayerChange change = changelist.FindChange(this.m_selectedFriend);
        if (change != null)
        {
            BnetPlayer oldPlayer = change.GetOldPlayer();
            BnetPlayer newPlayer = change.GetNewPlayer();
            if ((oldPlayer == null) || (oldPlayer.IsOnline() != newPlayer.IsOnline()))
            {
                this.m_selectedFriend = null;
            }
        }
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (mode == SceneMgr.Mode.GAMEPLAY)
        {
            GameState.Get().RegisterGameOverListener(new GameState.GameOverCallback(this.OnGameOver), null);
        }
    }

    public bool RemoveRecentOpponentListener(RecentOpponentCallback callback)
    {
        RecentOpponentListener item = new RecentOpponentListener();
        item.SetCallback(callback);
        item.SetUserData(null);
        return this.m_recentOpponentListeners.Remove(item);
    }

    public void SetFriendListScrollCamPosY(float y)
    {
        this.m_friendListScrollCamPosY = y;
    }

    public void SetFriendListScrollEnabled(bool enabled)
    {
        this.m_friendListScrollEnabled = enabled;
    }

    public void SetSelectedFriend(BnetPlayer friend)
    {
        this.m_selectedFriend = friend;
    }

    private void UpdateRecentOpponent()
    {
        if (!SpectatorManager.Get().IsInSpectatorMode() && (GameState.Get() != null))
        {
            Player opposingSidePlayer = GameState.Get().GetOpposingSidePlayer();
            if (opposingSidePlayer != null)
            {
                BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(opposingSidePlayer.GetGameAccountId());
                if ((player != null) && (this.m_recentOpponent != player))
                {
                    this.m_recentOpponent = player;
                    this.FireRecentOpponentEvent(this.m_recentOpponent);
                }
            }
        }
    }

    public delegate void RecentOpponentCallback(BnetPlayer recentOpponent, object userData);

    private class RecentOpponentListener : EventListener<FriendMgr.RecentOpponentCallback>
    {
        public void Fire(BnetPlayer recentOpponent)
        {
            base.m_callback(recentOpponent, base.m_userData);
        }
    }
}

