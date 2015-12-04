using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class BnetNearbyPlayerMgr
{
    private const float INACTIVITY_TIMEOUT = 60f;
    private bool m_availability;
    private string m_bnetEnvironment;
    private string m_bnetVersion;
    private List<ChangeListener> m_changeListeners = new List<ChangeListener>();
    private UdpClient m_client;
    private bool m_enabled = true;
    private string m_idString;
    private float m_lastCallTime;
    private bool m_listening;
    private object m_mutex = new object();
    private ulong m_myGameAccountLo;
    private List<NearbyPlayer> m_nearbyAdds = new List<NearbyPlayer>();
    private List<BnetPlayer> m_nearbyBnetPlayers = new List<BnetPlayer>();
    private List<BnetPlayer> m_nearbyFriends = new List<BnetPlayer>();
    private List<NearbyPlayer> m_nearbyPlayers = new List<NearbyPlayer>();
    private List<BnetPlayer> m_nearbyStrangers = new List<BnetPlayer>();
    private List<NearbyPlayer> m_nearbyUpdates = new List<NearbyPlayer>();
    private int m_port;
    private static BnetNearbyPlayerMgr s_instance;
    private const int UDP_PORT = 0x4cc;
    private const float UPDATE_INTERVAL = 12f;

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

    private void BeginListening()
    {
        if (!this.m_listening)
        {
            this.m_listening = true;
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 0x4cc);
            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(localEP);
            this.m_port = 0x4cc;
            this.m_client = client;
            UdpState state = new UdpState {
                e = localEP,
                u = this.m_client
            };
            this.m_lastCallTime = UnityEngine.Time.realtimeSinceStartup;
            this.m_client.BeginReceive(new AsyncCallback(this.OnUdpReceive), state);
        }
    }

    private void Broadcast()
    {
        string s = this.CreateBroadcastString();
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, this.m_port);
        UdpClient client = new UdpClient {
            EnableBroadcast = true
        };
        try
        {
            client.Send(bytes, bytes.Length, endPoint);
        }
        catch
        {
        }
        finally
        {
            client.Close();
        }
    }

    private void CacheMyAccountInfo()
    {
        if (this.m_idString == null)
        {
            BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
            if (myGameAccountId != null)
            {
                BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
                if (myPlayer != null)
                {
                    BnetAccountId accountId = myPlayer.GetAccountId();
                    if (accountId != null)
                    {
                        BnetBattleTag battleTag = myPlayer.GetBattleTag();
                        if (battleTag != null)
                        {
                            this.m_myGameAccountLo = myGameAccountId.GetLo();
                            StringBuilder builder = new StringBuilder();
                            builder.Append(accountId.GetHi());
                            builder.Append(',');
                            builder.Append(accountId.GetLo());
                            builder.Append(',');
                            builder.Append(myGameAccountId.GetHi());
                            builder.Append(',');
                            builder.Append(myGameAccountId.GetLo());
                            builder.Append(',');
                            builder.Append(battleTag.GetName());
                            builder.Append(',');
                            builder.Append(battleTag.GetNumber());
                            builder.Append(',');
                            builder.Append(BattleNet.GetVersion());
                            builder.Append(',');
                            builder.Append(BattleNet.GetEnvironment());
                            this.m_idString = builder.ToString();
                        }
                    }
                }
            }
        }
    }

    private bool CheckIntervalAndBroadcast()
    {
        float num = UnityEngine.Time.realtimeSinceStartup - this.m_lastCallTime;
        if (num < 12f)
        {
            return false;
        }
        this.m_lastCallTime = UnityEngine.Time.realtimeSinceStartup;
        this.Broadcast();
        return true;
    }

    private void Clear()
    {
        object mutex = this.m_mutex;
        lock (mutex)
        {
            this.m_nearbyPlayers.Clear();
            this.m_nearbyBnetPlayers.Clear();
            this.m_nearbyFriends.Clear();
            this.m_nearbyStrangers.Clear();
            this.m_nearbyAdds.Clear();
            this.m_nearbyUpdates.Clear();
        }
    }

    private string CreateBroadcastString()
    {
        ulong sessionStartTime = HealthyGamingMgr.Get().GetSessionStartTime();
        StringBuilder builder = new StringBuilder();
        builder.Append(this.m_idString);
        builder.Append(',');
        builder.Append(!this.m_availability ? "0" : "1");
        builder.Append(',');
        builder.Append(sessionStartTime);
        return builder.ToString();
    }

    public BnetPlayer FindNearbyFriend(BnetAccountId id)
    {
        return this.FindNearbyPlayer(id, this.m_nearbyFriends);
    }

    public BnetPlayer FindNearbyFriend(BnetGameAccountId id)
    {
        return this.FindNearbyPlayer(id, this.m_nearbyFriends);
    }

    public BnetPlayer FindNearbyFriend(BnetPlayer player)
    {
        return this.FindNearbyPlayer(player, this.m_nearbyFriends);
    }

    public BnetPlayer FindNearbyPlayer(BnetAccountId id)
    {
        return this.FindNearbyPlayer(id, this.m_nearbyBnetPlayers);
    }

    public BnetPlayer FindNearbyPlayer(BnetGameAccountId id)
    {
        return this.FindNearbyPlayer(id, this.m_nearbyBnetPlayers);
    }

    public BnetPlayer FindNearbyPlayer(BnetPlayer player)
    {
        return this.FindNearbyPlayer(player, this.m_nearbyBnetPlayers);
    }

    private BnetPlayer FindNearbyPlayer(BnetAccountId id, List<BnetPlayer> bnetPlayers)
    {
        int num = this.FindNearbyPlayerIndex(id, bnetPlayers);
        if (num < 0)
        {
            return null;
        }
        return bnetPlayers[num];
    }

    private BnetPlayer FindNearbyPlayer(BnetGameAccountId id, List<BnetPlayer> bnetPlayers)
    {
        int num = this.FindNearbyPlayerIndex(id, bnetPlayers);
        if (num < 0)
        {
            return null;
        }
        return bnetPlayers[num];
    }

    private BnetPlayer FindNearbyPlayer(BnetPlayer bnetPlayer, List<BnetPlayer> bnetPlayers)
    {
        if (bnetPlayer == null)
        {
            return null;
        }
        BnetAccountId accountId = bnetPlayer.GetAccountId();
        if (accountId != null)
        {
            return this.FindNearbyPlayer(accountId, bnetPlayers);
        }
        BnetGameAccountId hearthstoneGameAccountId = bnetPlayer.GetHearthstoneGameAccountId();
        return this.FindNearbyPlayer(hearthstoneGameAccountId, bnetPlayers);
    }

    private int FindNearbyPlayerIndex(BnetAccountId id, List<BnetPlayer> bnetPlayers)
    {
        if (id != null)
        {
            for (int i = 0; i < bnetPlayers.Count; i++)
            {
                BnetPlayer player = bnetPlayers[i];
                if (id == player.GetAccountId())
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private int FindNearbyPlayerIndex(BnetGameAccountId id, List<BnetPlayer> bnetPlayers)
    {
        if (id != null)
        {
            for (int i = 0; i < bnetPlayers.Count; i++)
            {
                BnetPlayer player = bnetPlayers[i];
                if (id == player.GetHearthstoneGameAccountId())
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private int FindNearbyPlayerIndex(BnetPlayer bnetPlayer, List<BnetPlayer> bnetPlayers)
    {
        if (bnetPlayer == null)
        {
            return -1;
        }
        BnetAccountId accountId = bnetPlayer.GetAccountId();
        if (accountId != null)
        {
            return this.FindNearbyPlayerIndex(accountId, bnetPlayers);
        }
        BnetGameAccountId hearthstoneGameAccountId = bnetPlayer.GetHearthstoneGameAccountId();
        return this.FindNearbyPlayerIndex(hearthstoneGameAccountId, bnetPlayers);
    }

    public BnetPlayer FindNearbyStranger(BnetAccountId id)
    {
        return this.FindNearbyPlayer(id, this.m_nearbyStrangers);
    }

    public BnetPlayer FindNearbyStranger(BnetGameAccountId id)
    {
        return this.FindNearbyPlayer(id, this.m_nearbyStrangers);
    }

    public BnetPlayer FindNearbyStranger(BnetPlayer player)
    {
        return this.FindNearbyPlayer(player, this.m_nearbyStrangers);
    }

    private void FireChangeEvent(BnetNearbyPlayerChangelist changelist)
    {
        if (!changelist.IsEmpty())
        {
            foreach (ChangeListener listener in this.m_changeListeners.ToArray())
            {
                listener.Fire(changelist);
            }
        }
    }

    public static BnetNearbyPlayerMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new BnetNearbyPlayerMgr();
            ApplicationMgr.Get().WillReset += new System.Action(s_instance.Clear);
        }
        return s_instance;
    }

    public bool GetAvailability()
    {
        return this.m_availability;
    }

    public List<BnetPlayer> GetNearbyFriends()
    {
        return this.m_nearbyFriends;
    }

    public List<BnetPlayer> GetNearbyPlayers()
    {
        return this.m_nearbyBnetPlayers;
    }

    public bool GetNearbySessionStartTime(BnetPlayer bnetPlayer, out ulong sessionStartTime)
    {
        <GetNearbySessionStartTime>c__AnonStorey391 storey = new <GetNearbySessionStartTime>c__AnonStorey391 {
            bnetPlayer = bnetPlayer
        };
        sessionStartTime = 0L;
        if (storey.bnetPlayer == null)
        {
            return false;
        }
        NearbyPlayer player = null;
        object mutex = this.m_mutex;
        lock (mutex)
        {
            player = this.m_nearbyPlayers.Find(new Predicate<NearbyPlayer>(storey.<>m__295));
        }
        if (player == null)
        {
            return false;
        }
        sessionStartTime = player.m_sessionStartTime;
        return true;
    }

    public List<BnetPlayer> GetNearbyStrangers()
    {
        return this.m_nearbyStrangers;
    }

    public void Initialize()
    {
        this.m_bnetVersion = BattleNet.GetVersion();
        this.m_bnetEnvironment = BattleNet.GetEnvironment();
        this.UpdateEnabled();
        Options.Get().RegisterChangedListener(Option.NEARBY_PLAYERS, new Options.ChangedCallback(this.OnEnabledOptionChanged));
        BnetFriendMgr.Get().AddChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
    }

    public bool IsEnabled()
    {
        if (!Options.Get().GetBool(Option.NEARBY_PLAYERS))
        {
            return false;
        }
        if (!this.m_enabled)
        {
            return false;
        }
        return true;
    }

    public bool IsNearbyFriend(BnetAccountId id)
    {
        return (this.FindNearbyFriend(id) != null);
    }

    public bool IsNearbyFriend(BnetGameAccountId id)
    {
        return (this.FindNearbyFriend(id) != null);
    }

    public bool IsNearbyFriend(BnetPlayer player)
    {
        return (this.FindNearbyFriend(player) != null);
    }

    public bool IsNearbyPlayer(BnetAccountId id)
    {
        return (this.FindNearbyPlayer(id) != null);
    }

    public bool IsNearbyPlayer(BnetGameAccountId id)
    {
        return (this.FindNearbyPlayer(id) != null);
    }

    public bool IsNearbyPlayer(BnetPlayer player)
    {
        return (this.FindNearbyPlayer(player) != null);
    }

    public bool IsNearbyStranger(BnetAccountId id)
    {
        return (this.FindNearbyStranger(id) != null);
    }

    public bool IsNearbyStranger(BnetGameAccountId id)
    {
        return (this.FindNearbyStranger(id) != null);
    }

    public bool IsNearbyStranger(BnetPlayer player)
    {
        return (this.FindNearbyStranger(player) != null);
    }

    private void OnEnabledOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        this.UpdateEnabled();
    }

    private void OnFriendsChanged(BnetFriendChangelist friendChangelist, object userData)
    {
        List<BnetPlayer> addedFriends = friendChangelist.GetAddedFriends();
        List<BnetPlayer> removedFriends = friendChangelist.GetRemovedFriends();
        bool flag = (addedFriends != null) && (addedFriends.Count > 0);
        bool flag2 = (removedFriends != null) && (removedFriends.Count > 0);
        if (flag || flag2)
        {
            BnetNearbyPlayerChangelist changelist = new BnetNearbyPlayerChangelist();
            object mutex = this.m_mutex;
            lock (mutex)
            {
                if (addedFriends != null)
                {
                    foreach (BnetPlayer player in addedFriends)
                    {
                        int index = this.FindNearbyPlayerIndex(player, this.m_nearbyStrangers);
                        if (index >= 0)
                        {
                            BnetPlayer item = this.m_nearbyStrangers[index];
                            this.m_nearbyStrangers.RemoveAt(index);
                            this.m_nearbyFriends.Add(item);
                            changelist.AddAddedFriend(item);
                            changelist.AddRemovedStranger(item);
                        }
                    }
                }
                if (removedFriends != null)
                {
                    foreach (BnetPlayer player3 in removedFriends)
                    {
                        int num2 = this.FindNearbyPlayerIndex(player3, this.m_nearbyFriends);
                        if (num2 >= 0)
                        {
                            BnetPlayer player4 = this.m_nearbyFriends[num2];
                            this.m_nearbyFriends.RemoveAt(num2);
                            this.m_nearbyStrangers.Add(player4);
                            changelist.AddAddedStranger(player4);
                            changelist.AddRemovedFriend(player4);
                        }
                    }
                }
            }
            this.FireChangeEvent(changelist);
        }
    }

    private void OnUdpReceive(IAsyncResult ar)
    {
        string str2;
        string str3;
        string str4;
        UdpClient u = ((UdpState) ar.AsyncState).u;
        IPEndPoint e = ((UdpState) ar.AsyncState).e;
        byte[] bytes = u.EndReceive(ar, ref e);
        u.BeginReceive(new AsyncCallback(this.OnUdpReceive), ar.AsyncState);
        char[] separator = new char[] { ',' };
        string[] strArray = Encoding.UTF8.GetString(bytes).Split(separator);
        ulong result = 0L;
        ulong num2 = 0L;
        ulong num3 = 0L;
        ulong num4 = 0L;
        int num5 = 0;
        bool val = false;
        ulong num6 = 0L;
        int num7 = 0;
        if (num7 < strArray.Length)
        {
            if (!ulong.TryParse(strArray[num7++], out result))
            {
                return;
            }
            if (num7 >= strArray.Length)
            {
                return;
            }
            if (!ulong.TryParse(strArray[num7++], out num2))
            {
                return;
            }
            if (num7 >= strArray.Length)
            {
                return;
            }
            if (!ulong.TryParse(strArray[num7++], out num3))
            {
                return;
            }
            if (num7 >= strArray.Length)
            {
                return;
            }
            if (!ulong.TryParse(strArray[num7++], out num4))
            {
                return;
            }
            if (this.m_myGameAccountLo == num4)
            {
                return;
            }
            if (num7 >= strArray.Length)
            {
                return;
            }
            str2 = strArray[num7++];
            if (num7 >= strArray.Length)
            {
                return;
            }
            if (!int.TryParse(strArray[num7++], out num5))
            {
                return;
            }
            if (num7 >= strArray.Length)
            {
                return;
            }
            str3 = strArray[num7++];
            if (string.IsNullOrEmpty(str3) || (str3 != this.m_bnetVersion))
            {
                return;
            }
            if (num7 >= strArray.Length)
            {
                return;
            }
            str4 = strArray[num7++];
            if (string.IsNullOrEmpty(str4) || (str4 != this.m_bnetEnvironment))
            {
                return;
            }
            if (num7 < strArray.Length)
            {
                switch (strArray[num7++])
                {
                    case "1":
                        val = true;
                        goto Label_020F;

                    case "0":
                        val = false;
                        goto Label_020F;
                }
            }
        }
        return;
    Label_020F:
        if (num7 >= strArray.Length)
        {
            return;
        }
        if (ulong.TryParse(strArray[num7++], out num6))
        {
            BnetBattleTag battleTag = new BnetBattleTag();
            battleTag.SetName(str2);
            battleTag.SetNumber(num5);
            BnetAccountId id = new BnetAccountId();
            id.SetHi(result);
            id.SetLo(num2);
            BnetAccount account = new BnetAccount();
            account.SetId(id);
            account.SetBattleTag(battleTag);
            BnetGameAccountId id2 = new BnetGameAccountId();
            id2.SetHi(num3);
            id2.SetLo(num4);
            BnetGameAccount gameAccount = new BnetGameAccount();
            gameAccount.SetId(id2);
            gameAccount.SetBattleTag(battleTag);
            gameAccount.SetOnline(true);
            gameAccount.SetProgramId(BnetProgramId.HEARTHSTONE);
            gameAccount.SetGameField(1, val);
            gameAccount.SetGameField(0x13, str3);
            gameAccount.SetGameField(20, str4);
            BnetPlayer player = new BnetPlayer();
            player.SetAccount(account);
            player.AddGameAccount(gameAccount);
            NearbyPlayer other = new NearbyPlayer {
                m_bnetPlayer = player,
                m_availability = val,
                m_sessionStartTime = num6
            };
            object mutex = this.m_mutex;
            lock (mutex)
            {
                if (this.m_listening)
                {
                    foreach (NearbyPlayer player3 in this.m_nearbyAdds)
                    {
                        if (player3.Equals(other))
                        {
                            this.UpdateNearbyPlayer(player3, val, num6);
                            goto Label_0480;
                        }
                    }
                    foreach (NearbyPlayer player4 in this.m_nearbyUpdates)
                    {
                        if (player4.Equals(other))
                        {
                            this.UpdateNearbyPlayer(player4, val, num6);
                            goto Label_0480;
                        }
                    }
                    foreach (NearbyPlayer player5 in this.m_nearbyPlayers)
                    {
                        if (player5.Equals(other))
                        {
                            this.UpdateNearbyPlayer(player5, val, num6);
                            this.m_nearbyUpdates.Add(player5);
                            goto Label_0480;
                        }
                    }
                    this.m_nearbyAdds.Add(other);
                }
            Label_0480:;
            }
        }
    }

    private void ProcessAddedPlayers(BnetNearbyPlayerChangelist changelist)
    {
        if (this.m_nearbyAdds.Count != 0)
        {
            for (int i = 0; i < this.m_nearbyAdds.Count; i++)
            {
                NearbyPlayer item = this.m_nearbyAdds[i];
                item.m_lastReceivedTime = UnityEngine.Time.realtimeSinceStartup;
                this.m_nearbyPlayers.Add(item);
                this.m_nearbyBnetPlayers.Add(item.m_bnetPlayer);
                changelist.AddAddedPlayer(item.m_bnetPlayer);
                if (item.IsFriend())
                {
                    this.m_nearbyFriends.Add(item.m_bnetPlayer);
                    changelist.AddAddedFriend(item.m_bnetPlayer);
                }
                else
                {
                    this.m_nearbyStrangers.Add(item.m_bnetPlayer);
                    changelist.AddAddedStranger(item.m_bnetPlayer);
                }
            }
            this.m_nearbyAdds.Clear();
        }
    }

    private void ProcessPlayerChanges()
    {
        BnetNearbyPlayerChangelist changelist = new BnetNearbyPlayerChangelist();
        object mutex = this.m_mutex;
        lock (mutex)
        {
            this.ProcessAddedPlayers(changelist);
            this.ProcessUpdatedPlayers(changelist);
            this.RemoveInactivePlayers(changelist);
        }
        this.FireChangeEvent(changelist);
    }

    private void ProcessUpdatedPlayers(BnetNearbyPlayerChangelist changelist)
    {
        if (this.m_nearbyUpdates.Count != 0)
        {
            for (int i = 0; i < this.m_nearbyUpdates.Count; i++)
            {
                NearbyPlayer player = this.m_nearbyUpdates[i];
                player.m_lastReceivedTime = UnityEngine.Time.realtimeSinceStartup;
                changelist.AddUpdatedPlayer(player.m_bnetPlayer);
                if (player.IsFriend())
                {
                    changelist.AddUpdatedFriend(player.m_bnetPlayer);
                }
                else
                {
                    changelist.AddUpdatedStranger(player.m_bnetPlayer);
                }
            }
            this.m_nearbyUpdates.Clear();
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

    private void RemoveInactivePlayers(BnetNearbyPlayerChangelist changelist)
    {
        List<NearbyPlayer> list = null;
        for (int i = 0; i < this.m_nearbyPlayers.Count; i++)
        {
            NearbyPlayer item = this.m_nearbyPlayers[i];
            float num2 = UnityEngine.Time.realtimeSinceStartup - item.m_lastReceivedTime;
            if (num2 >= 60f)
            {
                if (list == null)
                {
                    list = new List<NearbyPlayer>();
                }
                list.Add(item);
            }
        }
        if (list != null)
        {
            foreach (NearbyPlayer player2 in list)
            {
                this.m_nearbyPlayers.Remove(player2);
                if (this.m_nearbyBnetPlayers.Remove(player2.m_bnetPlayer))
                {
                    changelist.AddRemovedPlayer(player2.m_bnetPlayer);
                }
                if (this.m_nearbyFriends.Remove(player2.m_bnetPlayer))
                {
                    changelist.AddRemovedFriend(player2.m_bnetPlayer);
                }
                if (this.m_nearbyStrangers.Remove(player2.m_bnetPlayer))
                {
                    changelist.AddRemovedStranger(player2.m_bnetPlayer);
                }
            }
        }
    }

    public void SetAvailability(bool av)
    {
        this.m_availability = av;
    }

    public void SetEnabled(bool enabled)
    {
        this.m_enabled = enabled;
        this.UpdateEnabled();
    }

    public void Shutdown()
    {
        if (this.m_listening)
        {
            this.m_client.Close();
        }
        Options.Get().UnregisterChangedListener(Option.NEARBY_PLAYERS, new Options.ChangedCallback(this.OnEnabledOptionChanged));
        BnetFriendMgr.Get().RemoveChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
    }

    private void StopListening()
    {
        if (this.m_listening)
        {
            this.m_listening = false;
            this.m_client.Close();
            BnetNearbyPlayerChangelist changelist = new BnetNearbyPlayerChangelist();
            object mutex = this.m_mutex;
            lock (mutex)
            {
                foreach (BnetPlayer player in this.m_nearbyBnetPlayers)
                {
                    changelist.AddRemovedPlayer(player);
                }
                foreach (BnetPlayer player2 in this.m_nearbyFriends)
                {
                    changelist.AddRemovedFriend(player2);
                }
                foreach (BnetPlayer player3 in this.m_nearbyStrangers)
                {
                    changelist.AddRemovedStranger(player3);
                }
                this.m_nearbyPlayers.Clear();
                this.m_nearbyBnetPlayers.Clear();
                this.m_nearbyFriends.Clear();
                this.m_nearbyStrangers.Clear();
                this.m_nearbyAdds.Clear();
                this.m_nearbyUpdates.Clear();
            }
            this.FireChangeEvent(changelist);
        }
    }

    public void Update()
    {
        if (this.m_listening)
        {
            this.CacheMyAccountInfo();
            this.CheckIntervalAndBroadcast();
            this.ProcessPlayerChanges();
        }
    }

    private void UpdateEnabled()
    {
        bool flag = this.IsEnabled();
        if (flag != this.m_listening)
        {
            if (flag)
            {
                this.BeginListening();
            }
            else
            {
                this.StopListening();
            }
        }
    }

    private void UpdateNearbyPlayer(NearbyPlayer player, bool available, ulong sessionStartTime)
    {
        player.GetGameAccount().SetGameField(1, available);
        player.m_sessionStartTime = sessionStartTime;
    }

    [CompilerGenerated]
    private sealed class <GetNearbySessionStartTime>c__AnonStorey391
    {
        internal BnetPlayer bnetPlayer;

        internal bool <>m__295(BnetNearbyPlayerMgr.NearbyPlayer obj)
        {
            return (obj.m_bnetPlayer.GetAccountId() == this.bnetPlayer.GetAccountId());
        }
    }

    public delegate void ChangeCallback(BnetNearbyPlayerChangelist changelist, object userData);

    private class ChangeListener : EventListener<BnetNearbyPlayerMgr.ChangeCallback>
    {
        public void Fire(BnetNearbyPlayerChangelist changelist)
        {
            base.m_callback(changelist, base.m_userData);
        }
    }

    private class NearbyPlayer : IEquatable<BnetNearbyPlayerMgr.NearbyPlayer>
    {
        public bool m_availability;
        public BnetPlayer m_bnetPlayer;
        public float m_lastReceivedTime;
        public ulong m_sessionStartTime;

        public bool Equals(BnetNearbyPlayerMgr.NearbyPlayer other)
        {
            if (other == null)
            {
                return false;
            }
            return (this.GetGameAccountId() == other.GetGameAccountId());
        }

        public BnetAccountId GetAccountId()
        {
            return this.m_bnetPlayer.GetAccountId();
        }

        public BnetGameAccount GetGameAccount()
        {
            return this.m_bnetPlayer.GetHearthstoneGameAccount();
        }

        public BnetGameAccountId GetGameAccountId()
        {
            return this.m_bnetPlayer.GetHearthstoneGameAccountId();
        }

        public bool IsFriend()
        {
            BnetAccountId accountId = this.GetAccountId();
            return BnetFriendMgr.Get().IsFriend(accountId);
        }
    }

    private class UdpState
    {
        public IPEndPoint e;
        public UdpClient u;
    }
}

