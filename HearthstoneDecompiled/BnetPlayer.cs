using System;
using System.Collections.Generic;

public class BnetPlayer
{
    private BnetAccount m_account;
    private BnetAccountId m_accountId;
    private BnetGameAccount m_bestGameAccount;
    private Map<BnetGameAccountId, BnetGameAccount> m_gameAccounts = new Map<BnetGameAccountId, BnetGameAccount>();
    private BnetGameAccount m_hsGameAccount;

    public void AddGameAccount(BnetGameAccount gameAccount)
    {
        BnetGameAccountId key = gameAccount.GetId();
        if (!this.m_gameAccounts.ContainsKey(key))
        {
            this.m_gameAccounts.Add(key, gameAccount);
            this.CacheSpecialGameAccounts();
        }
    }

    private void CacheSpecialGameAccounts()
    {
        this.m_hsGameAccount = null;
        this.m_bestGameAccount = null;
        ulong lastOnlineMicrosec = 0L;
        foreach (BnetGameAccount account in this.m_gameAccounts.Values)
        {
            BnetProgramId programId = account.GetProgramId();
            if (programId != null)
            {
                if (programId == BnetProgramId.HEARTHSTONE)
                {
                    this.m_hsGameAccount = account;
                    if (account.IsOnline() || !BnetFriendMgr.Get().IsFriend(account.GetId()))
                    {
                        this.m_bestGameAccount = account;
                    }
                    break;
                }
                if (this.m_bestGameAccount == null)
                {
                    this.m_bestGameAccount = account;
                    lastOnlineMicrosec = this.m_bestGameAccount.GetLastOnlineMicrosec();
                }
                else
                {
                    BnetProgramId id2 = this.m_bestGameAccount.GetProgramId();
                    if (programId.IsGame() && !id2.IsGame())
                    {
                        this.m_bestGameAccount = account;
                        lastOnlineMicrosec = this.m_bestGameAccount.GetLastOnlineMicrosec();
                    }
                    else if ((account.IsOnline() && programId.IsGame()) && id2.IsGame())
                    {
                        ulong num2 = account.GetLastOnlineMicrosec();
                        if (num2 > lastOnlineMicrosec)
                        {
                            this.m_bestGameAccount = account;
                            lastOnlineMicrosec = num2;
                        }
                    }
                }
            }
        }
    }

    public BnetPlayer Clone()
    {
        BnetPlayer player = (BnetPlayer) base.MemberwiseClone();
        if (this.m_accountId != null)
        {
            player.m_accountId = this.m_accountId.Clone();
        }
        if (this.m_account != null)
        {
            player.m_account = this.m_account.Clone();
        }
        if (this.m_hsGameAccount != null)
        {
            player.m_hsGameAccount = this.m_hsGameAccount.Clone();
        }
        if (this.m_bestGameAccount != null)
        {
            player.m_bestGameAccount = this.m_bestGameAccount.Clone();
        }
        player.m_gameAccounts = new Map<BnetGameAccountId, BnetGameAccount>();
        foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> pair in this.m_gameAccounts)
        {
            BnetGameAccountId key = pair.Key.Clone();
            player.m_gameAccounts.Add(key, pair.Value.Clone());
        }
        return player;
    }

    public BnetAccount GetAccount()
    {
        return this.m_account;
    }

    public BnetAccountId GetAccountId()
    {
        if (this.m_accountId != null)
        {
            return this.m_accountId;
        }
        BnetGameAccount firstGameAccount = this.GetFirstGameAccount();
        if (firstGameAccount != null)
        {
            return firstGameAccount.GetOwnerId();
        }
        return null;
    }

    public BnetBattleTag GetBattleTag()
    {
        if ((this.m_account != null) && (this.m_account.GetBattleTag() != null))
        {
            return this.m_account.GetBattleTag();
        }
        BnetGameAccount firstGameAccount = this.GetFirstGameAccount();
        if (firstGameAccount != null)
        {
            return firstGameAccount.GetBattleTag();
        }
        return null;
    }

    public ulong GetBestAwayTimeMicrosec()
    {
        ulong num = 0L;
        if ((this.m_account != null) && this.m_account.IsAway())
        {
            num = Math.Max(this.m_account.GetAwayTimeMicrosec(), this.m_account.GetLastOnlineMicrosec());
            if (num != 0)
            {
                return num;
            }
        }
        if ((this.m_bestGameAccount != null) && this.m_bestGameAccount.IsAway())
        {
            num = Math.Max(this.m_bestGameAccount.GetAwayTimeMicrosec(), this.m_bestGameAccount.GetLastOnlineMicrosec());
            if (num != 0)
            {
                return num;
            }
        }
        return num;
    }

    public BnetGameAccount GetBestGameAccount()
    {
        return this.m_bestGameAccount;
    }

    public BnetGameAccountId GetBestGameAccountId()
    {
        if (this.m_bestGameAccount == null)
        {
            return null;
        }
        return this.m_bestGameAccount.GetId();
    }

    public ulong GetBestLastOnlineMicrosec()
    {
        ulong lastOnlineMicrosec = 0L;
        if (this.m_account != null)
        {
            lastOnlineMicrosec = this.m_account.GetLastOnlineMicrosec();
            if (lastOnlineMicrosec != 0)
            {
                return lastOnlineMicrosec;
            }
        }
        if (this.m_bestGameAccount != null)
        {
            lastOnlineMicrosec = this.m_bestGameAccount.GetLastOnlineMicrosec();
            if (lastOnlineMicrosec != 0)
            {
                return lastOnlineMicrosec;
            }
        }
        return lastOnlineMicrosec;
    }

    public string GetBestName()
    {
        if (this == BnetPresenceMgr.Get().GetMyPlayer())
        {
            if (this.m_hsGameAccount == null)
            {
                return null;
            }
            return ((this.m_hsGameAccount.GetBattleTag() != null) ? this.m_hsGameAccount.GetBattleTag().GetName() : null);
        }
        if (this.m_account != null)
        {
            string fullName = this.m_account.GetFullName();
            if (fullName != null)
            {
                return fullName;
            }
            if (this.m_account.GetBattleTag() != null)
            {
                return this.m_account.GetBattleTag().GetName();
            }
        }
        foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> pair in this.m_gameAccounts)
        {
            if (pair.Value.GetBattleTag() != null)
            {
                return pair.Value.GetBattleTag().GetName();
            }
        }
        return null;
    }

    public BnetProgramId GetBestProgramId()
    {
        if (this.m_bestGameAccount == null)
        {
            return null;
        }
        return this.m_bestGameAccount.GetProgramId();
    }

    public string GetBestRichPresence()
    {
        if (this.m_bestGameAccount == null)
        {
            return null;
        }
        return this.m_bestGameAccount.GetRichPresence();
    }

    public BnetGameAccount GetFirstGameAccount()
    {
        using (Map<BnetGameAccountId, BnetGameAccount>.ValueCollection.Enumerator enumerator = this.m_gameAccounts.Values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
        }
        return null;
    }

    public string GetFullName()
    {
        return ((this.m_account != null) ? this.m_account.GetFullName() : null);
    }

    public BnetGameAccount GetGameAccount(BnetGameAccountId id)
    {
        BnetGameAccount account = null;
        this.m_gameAccounts.TryGetValue(id, out account);
        return account;
    }

    public Map<BnetGameAccountId, BnetGameAccount> GetGameAccounts()
    {
        return this.m_gameAccounts;
    }

    public BnetGameAccount GetHearthstoneGameAccount()
    {
        return this.m_hsGameAccount;
    }

    public BnetGameAccountId GetHearthstoneGameAccountId()
    {
        if (this.m_hsGameAccount == null)
        {
            return null;
        }
        return this.m_hsGameAccount.GetId();
    }

    public int GetNumOnlineGameAccounts()
    {
        int num = 0;
        foreach (BnetGameAccount account in this.m_gameAccounts.Values)
        {
            if (account.IsOnline())
            {
                num++;
            }
        }
        return num;
    }

    public List<BnetGameAccount> GetOnlineGameAccounts()
    {
        List<BnetGameAccount> list = new List<BnetGameAccount>();
        foreach (BnetGameAccount account in this.m_gameAccounts.Values)
        {
            if (account.IsOnline())
            {
                list.Add(account);
            }
        }
        return list;
    }

    public long GetPersistentGameId()
    {
        return 0L;
    }

    public bool HasAccount(BnetEntityId id)
    {
        if (id != null)
        {
            if (this.m_accountId == id)
            {
                return true;
            }
            foreach (BnetGameAccountId id2 in this.m_gameAccounts.Keys)
            {
                if (id2 == id)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasGameAccount(BnetGameAccountId id)
    {
        return this.m_gameAccounts.ContainsKey(id);
    }

    public bool HasMultipleOnlineGameAccounts()
    {
        bool flag = false;
        foreach (BnetGameAccount account in this.m_gameAccounts.Values)
        {
            if (account.IsOnline())
            {
                if (flag)
                {
                    return true;
                }
                flag = true;
            }
        }
        return false;
    }

    public bool IsAway()
    {
        return (((this.m_account != null) && this.m_account.IsAway()) || ((this.m_bestGameAccount != null) && this.m_bestGameAccount.IsAway()));
    }

    public bool IsBusy()
    {
        return (((this.m_account != null) && this.m_account.IsBusy()) || ((this.m_bestGameAccount != null) && this.m_bestGameAccount.IsBusy()));
    }

    public bool IsDisplayable()
    {
        return (this.GetBestName() != null);
    }

    public bool IsOnline()
    {
        foreach (BnetGameAccount account in this.m_gameAccounts.Values)
        {
            if (account.IsOnline())
            {
                return true;
            }
        }
        return false;
    }

    public void OnGameAccountChanged(uint fieldId)
    {
        if (((fieldId == 3) || (fieldId == 1)) || (fieldId == 4))
        {
            this.CacheSpecialGameAccounts();
        }
    }

    public bool RemoveGameAccount(BnetGameAccountId id)
    {
        if (!this.m_gameAccounts.Remove(id))
        {
            return false;
        }
        this.CacheSpecialGameAccounts();
        return true;
    }

    public void SetAccount(BnetAccount account)
    {
        this.m_account = account;
        this.m_accountId = account.GetId();
    }

    public void SetAccountId(BnetAccountId accountId)
    {
        this.m_accountId = accountId;
    }

    public override string ToString()
    {
        BnetAccountId accountId = this.GetAccountId();
        BnetBattleTag battleTag = this.GetBattleTag();
        if ((accountId == null) && (battleTag == null))
        {
            return "UNKNOWN PLAYER";
        }
        return string.Format("[account={0} battleTag={1} numGameAccounts={2}]", accountId, battleTag, this.m_gameAccounts.Count);
    }
}

