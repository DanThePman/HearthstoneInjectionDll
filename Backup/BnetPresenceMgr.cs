using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class BnetPresenceMgr
{
    [CompilerGenerated]
    private static System.Action <>f__am$cache8;
    [CompilerGenerated]
    private static Func<Enum, string> <>f__am$cache9;
    [CompilerGenerated]
    private static Func<BattleNet.PresenceUpdate, bool> <>f__am$cacheA;
    private Map<BnetAccountId, BnetAccount> m_accounts = new Map<BnetAccountId, BnetAccount>();
    private Map<BnetGameAccountId, BnetGameAccount> m_gameAccounts = new Map<BnetGameAccountId, BnetGameAccount>();
    private BnetGameAccountId m_myGameAccountId;
    private BnetPlayer m_myPlayer;
    private Map<BnetAccountId, BnetPlayer> m_players = new Map<BnetAccountId, BnetPlayer>();
    private List<PlayersChangedListener> m_playersChangedListeners = new List<PlayersChangedListener>();
    private static BnetPresenceMgr s_instance;

    public event Action<BattleNet.PresenceUpdate[]> OnGameAccountPresenceChange;

    private void AddChangedPlayer(BnetPlayer player, BnetPlayerChangelist changelist)
    {
        if ((player != null) && !changelist.HasChange(player))
        {
            BnetPlayerChange change = new BnetPlayerChange();
            change.SetOldPlayer(player.Clone());
            change.SetNewPlayer(player);
            changelist.AddChange(change);
        }
    }

    public bool AddPlayersChangedListener(PlayersChangedCallback callback)
    {
        return this.AddPlayersChangedListener(callback, null);
    }

    public bool AddPlayersChangedListener(PlayersChangedCallback callback, object userData)
    {
        PlayersChangedListener item = new PlayersChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_playersChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_playersChangedListeners.Add(item);
        return true;
    }

    private void CacheMyself(BnetGameAccount gameAccount, BnetPlayer player)
    {
        if ((player != this.m_myPlayer) && (gameAccount.GetId() == this.m_myGameAccountId))
        {
            this.m_myPlayer = player;
        }
    }

    private BnetPlayerChangelist ChangeGameField(BnetGameAccount hsGameAccount, uint fieldId, object val)
    {
        if (hsGameAccount == null)
        {
            return null;
        }
        BnetPlayerChange change = new BnetPlayerChange();
        change.SetOldPlayer(this.m_myPlayer.Clone());
        change.SetNewPlayer(this.m_myPlayer);
        hsGameAccount.SetGameField(fieldId, val);
        BnetPlayerChangelist changelist = new BnetPlayerChangelist();
        changelist.AddChange(change);
        return changelist;
    }

    private void CreateAccount(BnetAccountId id, BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetAccount account = new BnetAccount();
        this.m_accounts.Add(id, account);
        account.SetId(id);
        BnetPlayer player = null;
        if (!this.m_players.TryGetValue(id, out player))
        {
            player = new BnetPlayer();
            this.m_players.Add(id, player);
            BnetPlayerChange change = new BnetPlayerChange();
            change.SetNewPlayer(player);
            changelist.AddChange(change);
        }
        player.SetAccount(account);
        this.UpdateAccount(account, update, changelist);
    }

    private void CreateGameAccount(BnetGameAccountId id, BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetGameAccount account = new BnetGameAccount();
        this.m_gameAccounts.Add(id, account);
        account.SetId(id);
        this.UpdateGameAccount(account, update, changelist);
    }

    private void CreateGameInfo(BnetGameAccountId id, BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetGameAccount account = new BnetGameAccount();
        this.m_gameAccounts.Add(id, account);
        account.SetId(id);
        this.UpdateGameInfo(account, update, changelist);
    }

    private void FirePlayersChangedEvent(BnetPlayerChangelist changelist)
    {
        if ((changelist != null) && (changelist.GetChanges().Count != 0))
        {
            PlayersChangedListener[] listenerArray = this.m_playersChangedListeners.ToArray();
            for (int i = 0; i < listenerArray.Length; i++)
            {
                listenerArray[i].Fire(changelist);
            }
        }
    }

    public static BnetPresenceMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new BnetPresenceMgr();
            if (<>f__am$cache8 == null)
            {
                <>f__am$cache8 = delegate {
                    BnetPresenceMgr mgr = s_instance;
                    s_instance = new BnetPresenceMgr();
                    s_instance.m_playersChangedListeners = mgr.m_playersChangedListeners;
                    s_instance.OnGameAccountPresenceChange = mgr.OnGameAccountPresenceChange;
                };
            }
            ApplicationMgr.Get().WillReset += <>f__am$cache8;
        }
        return s_instance;
    }

    public BnetAccount GetAccount(BnetAccountId id)
    {
        if (id == null)
        {
            return null;
        }
        BnetAccount account = null;
        this.m_accounts.TryGetValue(id, out account);
        return account;
    }

    public BnetGameAccount GetGameAccount(BnetGameAccountId id)
    {
        if (id == null)
        {
            return null;
        }
        BnetGameAccount account = null;
        this.m_gameAccounts.TryGetValue(id, out account);
        return account;
    }

    public BnetGameAccountId GetMyGameAccountId()
    {
        return this.m_myGameAccountId;
    }

    public BnetPlayer GetMyPlayer()
    {
        return this.m_myPlayer;
    }

    public BnetPlayer GetPlayer(BnetAccountId id)
    {
        if (id == null)
        {
            return null;
        }
        BnetPlayer player = null;
        this.m_players.TryGetValue(id, out player);
        return player;
    }

    public BnetPlayer GetPlayer(BnetGameAccountId id)
    {
        BnetGameAccount gameAccount = this.GetGameAccount(id);
        if (gameAccount == null)
        {
            return null;
        }
        return this.GetPlayer(gameAccount.GetOwnerId());
    }

    private void HandleGameAccountChange(BnetPlayer player, BattleNet.PresenceUpdate update)
    {
        if (player != null)
        {
            player.OnGameAccountChanged(update.fieldId);
        }
    }

    public void Initialize()
    {
        Network.Get().SetPresenceHandler(new Network.PresenceHandler(this.OnPresenceUpdate));
        BnetEventMgr.Get().AddChangeListener(new BnetEventMgr.ChangeCallback(this.OnBnetEventOccurred));
        BattleNet.DllEntityId myGameAccountId = BattleNet.GetMyGameAccountId();
        this.m_myGameAccountId = BnetGameAccountId.CreateFromDll(myGameAccountId);
    }

    private void OnAccountUpdate(BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetAccountId key = BnetAccountId.CreateFromDll(update.entityId);
        BnetAccount account = null;
        if (!this.m_accounts.TryGetValue(key, out account))
        {
            this.CreateAccount(key, update, changelist);
        }
        else
        {
            this.UpdateAccount(account, update, changelist);
        }
    }

    private void OnBnetEventOccurred(BattleNet.BnetEvent bnetEvent, object userData)
    {
        if (bnetEvent == BattleNet.BnetEvent.Disconnected)
        {
            foreach (BnetAccount account in this.m_accounts.Values)
            {
                GeneralUtils.DeepReset<BnetAccount>(account);
            }
            foreach (BnetGameAccount account2 in this.m_gameAccounts.Values)
            {
                GeneralUtils.DeepReset<BnetGameAccount>(account2);
            }
        }
    }

    private void OnGameAccountUpdate(BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetGameAccountId key = BnetGameAccountId.CreateFromDll(update.entityId);
        BnetGameAccount account = null;
        if (!this.m_gameAccounts.TryGetValue(key, out account))
        {
            this.CreateGameAccount(key, update, changelist);
        }
        else
        {
            this.UpdateGameAccount(account, update, changelist);
        }
    }

    private void OnGameUpdate(BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetGameAccountId key = BnetGameAccountId.CreateFromDll(update.entityId);
        BnetGameAccount account = null;
        if (!this.m_gameAccounts.TryGetValue(key, out account))
        {
            this.CreateGameInfo(key, update, changelist);
        }
        else
        {
            this.UpdateGameInfo(account, update, changelist);
        }
    }

    private void OnPresenceUpdate(BattleNet.PresenceUpdate[] updates)
    {
        BnetPlayerChangelist changelist = new BnetPlayerChangelist();
        if (<>f__am$cacheA == null)
        {
            <>f__am$cacheA = u => ((u.programId == BnetProgramId.BNET) && (u.groupId == 2)) && (u.fieldId == 7);
        }
        IEnumerator<BattleNet.PresenceUpdate> enumerator = Enumerable.Where<BattleNet.PresenceUpdate>(updates, <>f__am$cacheA).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                BattleNet.PresenceUpdate current = enumerator.Current;
                BnetGameAccountId id = BnetGameAccountId.CreateFromDll(current.entityId);
                BnetAccountId id2 = BnetAccountId.CreateFromDll(current.entityIdVal);
                if (!id2.IsEmpty())
                {
                    if (this.GetAccount(id2) == null)
                    {
                        BattleNet.PresenceUpdate update = new BattleNet.PresenceUpdate();
                        BnetPlayerChangelist changelist2 = new BnetPlayerChangelist();
                        this.CreateAccount(id2, update, changelist2);
                    }
                    if (!id.IsEmpty() && (this.GetGameAccount(id) == null))
                    {
                        this.CreateGameAccount(id, current, changelist);
                    }
                }
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
        List<BattleNet.PresenceUpdate> list = null;
        foreach (BattleNet.PresenceUpdate update3 in updates)
        {
            if (update3.programId == BnetProgramId.BNET)
            {
                if (update3.groupId == 1)
                {
                    this.OnAccountUpdate(update3, changelist);
                }
                else if (update3.groupId == 2)
                {
                    this.OnGameAccountUpdate(update3, changelist);
                }
            }
            else if (update3.programId == BnetProgramId.HEARTHSTONE)
            {
                this.OnGameUpdate(update3, changelist);
            }
            if (((update3.programId == BnetProgramId.HEARTHSTONE) || ((update3.programId == BnetProgramId.BNET) && (update3.groupId == 2))) && (this.OnGameAccountPresenceChange != null))
            {
                if (list == null)
                {
                    list = new List<BattleNet.PresenceUpdate>();
                }
                list.Add(update3);
            }
        }
        if (list != null)
        {
            this.OnGameAccountPresenceChange(list.ToArray());
        }
        this.FirePlayersChangedEvent(changelist);
    }

    public BnetPlayer RegisterPlayer(BnetAccountId id)
    {
        BnetPlayer player = this.GetPlayer(id);
        if (player == null)
        {
            player = new BnetPlayer();
            player.SetAccountId(id);
            this.m_players[id] = player;
            BnetPlayerChange change = new BnetPlayerChange();
            change.SetNewPlayer(player);
            BnetPlayerChangelist changelist = new BnetPlayerChangelist();
            changelist.AddChange(change);
            this.FirePlayersChangedEvent(changelist);
        }
        return player;
    }

    public bool RemovePlayersChangedListener(PlayersChangedCallback callback)
    {
        return this.RemovePlayersChangedListener(callback, null);
    }

    public bool RemovePlayersChangedListener(PlayersChangedCallback callback, object userData)
    {
        PlayersChangedListener item = new PlayersChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_playersChangedListeners.Remove(item);
    }

    public bool SetGameField(uint fieldId, bool val)
    {
        BnetGameAccount account;
        if (!Network.ShouldBeConnectedToAurora())
        {
            object[] messageArgs = new object[] { fieldId, val };
            Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}={1}", messageArgs);
            return false;
        }
        if (!this.ShouldUpdateGameField(fieldId, val, out account))
        {
            return false;
        }
        if (fieldId == 2)
        {
            account.SetBusy(val);
            int num = !val ? 0 : 1;
            BattleNet.SetPresenceInt(fieldId, (long) num);
        }
        else
        {
            BattleNet.SetPresenceBool(fieldId, val);
        }
        BnetPlayerChangelist changelist = this.ChangeGameField(account, fieldId, val);
        switch (fieldId)
        {
            case 2:
                if (val)
                {
                    account.SetAway(false);
                }
                break;

            case 10:
                if (val)
                {
                    account.SetBusy(false);
                }
                break;
        }
        this.FirePlayersChangedEvent(changelist);
        return true;
    }

    public bool SetGameField(uint fieldId, int val)
    {
        BnetGameAccount account;
        if (!Network.ShouldBeConnectedToAurora())
        {
            object[] messageArgs = new object[] { fieldId, val };
            Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}={1}", messageArgs);
            return false;
        }
        if (!this.ShouldUpdateGameField(fieldId, val, out account))
        {
            return false;
        }
        BattleNet.SetPresenceInt(fieldId, (long) val);
        BnetPlayerChangelist changelist = this.ChangeGameField(account, fieldId, val);
        this.FirePlayersChangedEvent(changelist);
        return true;
    }

    public bool SetGameField(uint fieldId, string val)
    {
        BnetGameAccount account;
        if (!Network.ShouldBeConnectedToAurora())
        {
            object[] messageArgs = new object[] { fieldId, val };
            Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}={1}", messageArgs);
            return false;
        }
        if (!this.ShouldUpdateGameField(fieldId, val, out account))
        {
            return false;
        }
        BattleNet.SetPresenceString(fieldId, val);
        BnetPlayerChangelist changelist = this.ChangeGameField(account, fieldId, val);
        this.FirePlayersChangedEvent(changelist);
        return true;
    }

    public bool SetGameField(uint fieldId, byte[] val)
    {
        BnetGameAccount account;
        if (!Network.ShouldBeConnectedToAurora())
        {
            object[] messageArgs = new object[] { fieldId, (val != null) ? val.Length.ToString() : string.Empty };
            Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}=[{1}]", messageArgs);
            return false;
        }
        if (!this.ShouldUpdateGameFieldBlob(fieldId, val, out account))
        {
            return false;
        }
        BattleNet.SetPresenceBlob(fieldId, val);
        BnetPlayerChangelist changelist = this.ChangeGameField(account, fieldId, val);
        this.FirePlayersChangedEvent(changelist);
        return true;
    }

    public bool SetGameFieldBlob(uint fieldId, IProtoBuf protoMessage)
    {
        byte[] val = (protoMessage != null) ? ProtobufUtil.ToByteArray(protoMessage) : null;
        return this.SetGameField(fieldId, val);
    }

    public bool SetRichPresence(Enum[] richPresence)
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            object[] messageArgs = new object[1];
            if (<>f__am$cache9 == null)
            {
                <>f__am$cache9 = x => x.ToString();
            }
            messageArgs[0] = (richPresence != null) ? string.Join(", ", Enumerable.Select<Enum, string>(richPresence, <>f__am$cache9).ToArray<string>()) : string.Empty;
            Error.AddDevFatal("Caller should check for Battle.net connection before calling SetRichPresence {0}", messageArgs);
            return false;
        }
        if (richPresence == null)
        {
            return false;
        }
        if (richPresence.Length == 0)
        {
            return false;
        }
        BattleNet.DllRichPresenceUpdate[] updates = new BattleNet.DllRichPresenceUpdate[richPresence.Length];
        for (int i = 0; i < richPresence.Length; i++)
        {
            Enum enum2 = richPresence[i];
            System.Type type = enum2.GetType();
            FourCC rcc = RichPresence.s_streamIds[type];
            updates[i] = new BattleNet.DllRichPresenceUpdate { presenceFieldIndex = (i != 0) ? ((ulong) (0x70000 + i)) : ((ulong) 0), programId = BnetProgramId.HEARTHSTONE.GetValue(), streamId = rcc.GetValue(), index = Convert.ToUInt32(enum2) };
        }
        BattleNet.SetRichPresence(updates);
        return true;
    }

    private bool ShouldUpdateGameField(uint fieldId, object val, out BnetGameAccount hsGameAccount)
    {
        hsGameAccount = null;
        if (this.m_myPlayer != null)
        {
            hsGameAccount = this.m_myPlayer.GetHearthstoneGameAccount();
            if (hsGameAccount == null)
            {
                return true;
            }
            if (hsGameAccount.HasGameField(fieldId))
            {
                object gameField = hsGameAccount.GetGameField(fieldId);
                if (val == null)
                {
                    if (gameField == null)
                    {
                        return false;
                    }
                }
                else if (val.Equals(gameField))
                {
                    return false;
                }
            }
            else if (val == null)
            {
                return false;
            }
        }
        return true;
    }

    private bool ShouldUpdateGameFieldBlob(uint fieldId, byte[] val, out BnetGameAccount hsGameAccount)
    {
        hsGameAccount = null;
        if (this.m_myPlayer != null)
        {
            hsGameAccount = this.m_myPlayer.GetHearthstoneGameAccount();
            if (hsGameAccount == null)
            {
                return true;
            }
            if (hsGameAccount.HasGameField(fieldId))
            {
                byte[] gameFieldBytes = hsGameAccount.GetGameFieldBytes(fieldId);
                if (GeneralUtils.AreArraysEqual<byte>(val, gameFieldBytes))
                {
                    return false;
                }
            }
            else if (val == null)
            {
                return false;
            }
        }
        return true;
    }

    public void Shutdown()
    {
        Network.Get().SetPresenceHandler(null);
    }

    private void UpdateAccount(BnetAccount account, BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetPlayer player = this.m_players[account.GetId()];
        if (update.fieldId == 7)
        {
            bool boolVal = update.boolVal;
            if (boolVal != account.IsAway())
            {
                this.AddChangedPlayer(player, changelist);
                account.SetAway(boolVal);
                if (boolVal)
                {
                    account.SetBusy(false);
                }
            }
        }
        else if (update.fieldId == 8)
        {
            ulong intVal = (ulong) update.intVal;
            if (intVal != account.GetAwayTimeMicrosec())
            {
                this.AddChangedPlayer(player, changelist);
                account.SetAwayTimeMicrosec(intVal);
            }
        }
        else if (update.fieldId == 11)
        {
            bool busy = update.boolVal;
            if (busy != account.IsBusy())
            {
                this.AddChangedPlayer(player, changelist);
                account.SetBusy(busy);
                if (busy)
                {
                    account.SetAway(false);
                }
            }
        }
        else if (update.fieldId == 4)
        {
            BnetBattleTag battleTag = BnetBattleTag.CreateFromString(update.stringVal);
            if (battleTag != account.GetBattleTag())
            {
                this.AddChangedPlayer(player, changelist);
                account.SetBattleTag(battleTag);
            }
        }
        else if (update.fieldId == 1)
        {
            string stringVal = update.stringVal;
            if (stringVal == null)
            {
                object[] messageArgs = new object[] { account };
                Error.AddDevFatal("BnetPresenceMgr.UpdateAccount() - Failed to convert full name to native string for {0}.", messageArgs);
            }
            else if (stringVal != account.GetFullName())
            {
                this.AddChangedPlayer(player, changelist);
                account.SetFullName(stringVal);
            }
        }
        else if (update.fieldId == 6)
        {
            ulong microsec = (ulong) update.intVal;
            if (microsec != account.GetLastOnlineMicrosec())
            {
                this.AddChangedPlayer(player, changelist);
                account.SetLastOnlineMicrosec(microsec);
            }
        }
        else if (update.fieldId == 3)
        {
        }
    }

    private void UpdateGameAccount(BnetGameAccount gameAccount, BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetPlayer player = null;
        BnetAccountId ownerId = gameAccount.GetOwnerId();
        if (ownerId != null)
        {
            this.m_players.TryGetValue(ownerId, out player);
        }
        if (update.fieldId == 2)
        {
            int num = !gameAccount.IsBusy() ? 0 : 1;
            int intVal = (int) update.intVal;
            if (intVal != num)
            {
                this.AddChangedPlayer(player, changelist);
                bool busy = intVal == 1;
                gameAccount.SetBusy(busy);
                if (busy)
                {
                    gameAccount.SetAway(false);
                }
                this.HandleGameAccountChange(player, update);
            }
        }
        else if (update.fieldId == 10)
        {
            bool boolVal = update.boolVal;
            if (boolVal != gameAccount.IsAway())
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.SetAway(boolVal);
                if (boolVal)
                {
                    gameAccount.SetBusy(false);
                }
                this.HandleGameAccountChange(player, update);
            }
        }
        else if (update.fieldId == 11)
        {
            ulong awayTimeMicrosec = (ulong) update.intVal;
            if (awayTimeMicrosec != gameAccount.GetAwayTimeMicrosec())
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.SetAwayTimeMicrosec(awayTimeMicrosec);
                this.HandleGameAccountChange(player, update);
            }
        }
        else if (update.fieldId == 5)
        {
            BnetBattleTag battleTag = BnetBattleTag.CreateFromString(update.stringVal);
            if (battleTag != gameAccount.GetBattleTag())
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.SetBattleTag(battleTag);
                this.HandleGameAccountChange(player, update);
            }
        }
        else if (update.fieldId == 1)
        {
            bool online = update.boolVal;
            if (online != gameAccount.IsOnline())
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.SetOnline(online);
                this.HandleGameAccountChange(player, update);
            }
        }
        else if (update.fieldId == 3)
        {
            BnetProgramId programId = new BnetProgramId(update.stringVal);
            if (programId != gameAccount.GetProgramId())
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.SetProgramId(programId);
                this.HandleGameAccountChange(player, update);
            }
        }
        else if (update.fieldId == 4)
        {
            ulong microsec = (ulong) update.intVal;
            if (microsec != gameAccount.GetLastOnlineMicrosec())
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.SetLastOnlineMicrosec(microsec);
                this.HandleGameAccountChange(player, update);
            }
        }
        else if (update.fieldId == 7)
        {
            BnetAccountId id3 = BnetAccountId.CreateFromDll(update.entityIdVal);
            if (id3 != gameAccount.GetOwnerId())
            {
                this.UpdateGameAccountOwner(id3, gameAccount, changelist);
            }
        }
        else if (update.fieldId == 9)
        {
            if (update.valCleared && (gameAccount.GetRichPresence() != null))
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.SetRichPresence(null);
                this.HandleGameAccountChange(player, update);
            }
        }
        else if (update.fieldId == 0x3e8)
        {
            string stringVal = update.stringVal;
            if (stringVal == null)
            {
                stringVal = string.Empty;
            }
            if (stringVal != gameAccount.GetRichPresence())
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.SetRichPresence(stringVal);
                this.HandleGameAccountChange(player, update);
            }
        }
    }

    private void UpdateGameAccountOwner(BnetAccountId ownerId, BnetGameAccount gameAccount, BnetPlayerChangelist changelist)
    {
        BnetPlayer player = null;
        BnetAccountId key = gameAccount.GetOwnerId();
        if ((key != null) && this.m_players.TryGetValue(key, out player))
        {
            player.RemoveGameAccount(gameAccount.GetId());
            this.AddChangedPlayer(player, changelist);
        }
        BnetPlayer player2 = null;
        if (this.m_players.TryGetValue(ownerId, out player2))
        {
            this.AddChangedPlayer(player2, changelist);
        }
        else
        {
            player2 = new BnetPlayer();
            this.m_players.Add(ownerId, player2);
            BnetPlayerChange change = new BnetPlayerChange();
            change.SetNewPlayer(player2);
            changelist.AddChange(change);
        }
        gameAccount.SetOwnerId(ownerId);
        player2.AddGameAccount(gameAccount);
        this.CacheMyself(gameAccount, player2);
    }

    private void UpdateGameInfo(BnetGameAccount gameAccount, BattleNet.PresenceUpdate update, BnetPlayerChangelist changelist)
    {
        BnetPlayer player = null;
        BnetAccountId ownerId = gameAccount.GetOwnerId();
        if (ownerId != null)
        {
            this.m_players.TryGetValue(ownerId, out player);
        }
        if (update.valCleared)
        {
            if (gameAccount.HasGameField(update.fieldId))
            {
                this.AddChangedPlayer(player, changelist);
                gameAccount.RemoveGameField(update.fieldId);
                this.HandleGameAccountChange(player, update);
            }
        }
        else
        {
            switch (update.fieldId)
            {
                case 1:
                    if (update.boolVal != gameAccount.GetGameFieldBool(update.fieldId))
                    {
                        this.AddChangedPlayer(player, changelist);
                        gameAccount.SetGameField(update.fieldId, update.boolVal);
                        this.HandleGameAccountChange(player, update);
                        break;
                    }
                    return;

                case 2:
                case 3:
                case 4:
                case 0x13:
                case 20:
                    if (!(update.stringVal == gameAccount.GetGameFieldString(update.fieldId)))
                    {
                        this.AddChangedPlayer(player, changelist);
                        gameAccount.SetGameField(update.fieldId, update.stringVal);
                        this.HandleGameAccountChange(player, update);
                        break;
                    }
                    return;

                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 0x10:
                    if (((int) update.intVal) != gameAccount.GetGameFieldInt(update.fieldId))
                    {
                        this.AddChangedPlayer(player, changelist);
                        gameAccount.SetGameField(update.fieldId, (int) update.intVal);
                        this.HandleGameAccountChange(player, update);
                        break;
                    }
                    return;

                case 0x11:
                case 0x12:
                case 0x15:
                    if (!GeneralUtils.AreBytesEqual(update.blobVal, gameAccount.GetGameFieldBytes(update.fieldId)))
                    {
                        this.AddChangedPlayer(player, changelist);
                        gameAccount.SetGameField(update.fieldId, update.blobVal);
                        this.HandleGameAccountChange(player, update);
                        break;
                    }
                    return;
            }
        }
    }

    public delegate void PlayersChangedCallback(BnetPlayerChangelist changelist, object userData);

    private class PlayersChangedListener : EventListener<BnetPresenceMgr.PlayersChangedCallback>
    {
        public void Fire(BnetPlayerChangelist changelist)
        {
            base.m_callback(changelist, base.m_userData);
        }
    }
}

