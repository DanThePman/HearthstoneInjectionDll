using SpectatorProto;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class BnetGameAccount
{
    private bool m_away;
    private ulong m_awayTimeMicrosec;
    private BnetBattleTag m_battleTag;
    private bool m_busy;
    private Map<uint, object> m_gameFields = new Map<uint, object>();
    private BnetGameAccountId m_id;
    private ulong m_lastOnlineMicrosec;
    private bool m_online;
    private BnetAccountId m_ownerId;
    private BnetProgramId m_programId;
    private string m_richPresence;

    public bool CanBeInvitedToGame()
    {
        return this.GetGameFieldBool(1);
    }

    public BnetGameAccount Clone()
    {
        BnetGameAccount account = (BnetGameAccount) base.MemberwiseClone();
        if (this.m_id != null)
        {
            account.m_id = this.m_id.Clone();
        }
        if (this.m_ownerId != null)
        {
            account.m_ownerId = this.m_ownerId.Clone();
        }
        if (this.m_programId != null)
        {
            account.m_programId = this.m_programId.Clone();
        }
        if (this.m_battleTag != null)
        {
            account.m_battleTag = this.m_battleTag.Clone();
        }
        account.m_gameFields = new Map<uint, object>();
        foreach (KeyValuePair<uint, object> pair in this.m_gameFields)
        {
            account.m_gameFields.Add(pair.Key, pair.Value);
        }
        return account;
    }

    public bool Equals(BnetGameAccountId other)
    {
        if (other == null)
        {
            return false;
        }
        return this.m_id.Equals((BnetEntityId) other);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        BnetGameAccount account = obj as BnetGameAccount;
        if (account == null)
        {
            return false;
        }
        return this.m_id.Equals((BnetEntityId) account.m_id);
    }

    public string GetArenaRecord()
    {
        return this.GetGameFieldString(3);
    }

    public ulong GetAwayTimeMicrosec()
    {
        return this.m_awayTimeMicrosec;
    }

    public BnetBattleTag GetBattleTag()
    {
        return this.m_battleTag;
    }

    public string GetCardsOpened()
    {
        return this.GetGameFieldString(4);
    }

    public string GetClientEnv()
    {
        return this.GetGameFieldString(20);
    }

    public string GetClientVersion()
    {
        return this.GetGameFieldString(0x13);
    }

    public int GetCollectionEvent()
    {
        return this.GetGameFieldInt(0x10);
    }

    public string GetDebugString()
    {
        return this.GetGameFieldString(2);
    }

    public int GetDruidLevel()
    {
        return this.GetGameFieldInt(5);
    }

    public int GetGainMedal()
    {
        return this.GetGameFieldInt(14);
    }

    public object GetGameField(uint fieldId)
    {
        object obj2 = null;
        this.m_gameFields.TryGetValue(fieldId, out obj2);
        return obj2;
    }

    public bool GetGameFieldBool(uint fieldId)
    {
        object obj2 = null;
        return (this.m_gameFields.TryGetValue(fieldId, out obj2) && ((bool) obj2));
    }

    public byte[] GetGameFieldBytes(uint fieldId)
    {
        object obj2 = null;
        if (this.m_gameFields.TryGetValue(fieldId, out obj2))
        {
            return (byte[]) obj2;
        }
        return null;
    }

    public int GetGameFieldInt(uint fieldId)
    {
        object obj2 = null;
        if (this.m_gameFields.TryGetValue(fieldId, out obj2))
        {
            return (int) obj2;
        }
        return 0;
    }

    public Map<uint, object> GetGameFields()
    {
        return this.m_gameFields;
    }

    public string GetGameFieldString(uint fieldId)
    {
        object obj2 = null;
        if (this.m_gameFields.TryGetValue(fieldId, out obj2))
        {
            return (string) obj2;
        }
        return null;
    }

    public override int GetHashCode()
    {
        return this.m_id.GetHashCode();
    }

    public int GetHunterLevel()
    {
        return this.GetGameFieldInt(6);
    }

    public BnetGameAccountId GetId()
    {
        return this.m_id;
    }

    public ulong GetLastOnlineMicrosec()
    {
        return this.m_lastOnlineMicrosec;
    }

    public int GetMageLevel()
    {
        return this.GetGameFieldInt(7);
    }

    public BnetAccountId GetOwnerId()
    {
        return this.m_ownerId;
    }

    public int GetPaladinLevel()
    {
        return this.GetGameFieldInt(8);
    }

    public int GetPriestLevel()
    {
        return this.GetGameFieldInt(9);
    }

    public BnetProgramId GetProgramId()
    {
        return this.m_programId;
    }

    public string GetRichPresence()
    {
        return this.m_richPresence;
    }

    public int GetRogueLevel()
    {
        return this.GetGameFieldInt(10);
    }

    public int GetShamanLevel()
    {
        return this.GetGameFieldInt(11);
    }

    public JoinInfo GetSpectatorJoinInfo()
    {
        byte[] gameFieldBytes = this.GetGameFieldBytes(0x15);
        if ((gameFieldBytes != null) && (gameFieldBytes.Length > 0))
        {
            return ProtobufUtil.ParseFrom<JoinInfo>(gameFieldBytes, 0, -1);
        }
        return null;
    }

    public int GetTutorialBeaten()
    {
        return this.GetGameFieldInt(15);
    }

    public int GetWarlockLevel()
    {
        return this.GetGameFieldInt(12);
    }

    public int GetWarriorLevel()
    {
        return this.GetGameFieldInt(13);
    }

    public bool HasGameField(uint fieldId)
    {
        return this.m_gameFields.ContainsKey(fieldId);
    }

    public bool IsAway()
    {
        return this.m_away;
    }

    public bool IsBusy()
    {
        return this.m_busy;
    }

    public bool IsOnline()
    {
        return this.m_online;
    }

    public bool IsSpectatorSlotAvailable()
    {
        return IsSpectatorSlotAvailable(this.GetSpectatorJoinInfo());
    }

    public static bool IsSpectatorSlotAvailable(JoinInfo info)
    {
        if (info == null)
        {
            return false;
        }
        if (!info.HasPartyId)
        {
            if (!info.HasServerIpAddress || !info.HasSecretKey)
            {
                return false;
            }
            if (string.IsNullOrEmpty(info.SecretKey))
            {
                return false;
            }
        }
        if (info.HasIsJoinable && !info.IsJoinable)
        {
            return false;
        }
        if ((info.HasMaxNumSpectators && info.HasCurrentNumSpectators) && (info.CurrentNumSpectators >= info.MaxNumSpectators))
        {
            return false;
        }
        return true;
    }

    public static bool operator ==(BnetGameAccount a, BnetGameAccount b)
    {
        return (object.ReferenceEquals(a, b) || (((a != null) && (b != null)) && (a.m_id == b.m_id)));
    }

    public static bool operator !=(BnetGameAccount a, BnetGameAccount b)
    {
        return !(a == b);
    }

    public bool RemoveGameField(uint fieldId)
    {
        return this.m_gameFields.Remove(fieldId);
    }

    public void SetAway(bool away)
    {
        this.m_away = away;
    }

    public void SetAwayTimeMicrosec(ulong awayTimeMicrosec)
    {
        this.m_awayTimeMicrosec = awayTimeMicrosec;
    }

    public void SetBattleTag(BnetBattleTag battleTag)
    {
        this.m_battleTag = battleTag;
    }

    public void SetBusy(bool busy)
    {
        this.m_busy = busy;
    }

    public void SetGameField(uint fieldId, object val)
    {
        this.m_gameFields[fieldId] = val;
    }

    public void SetId(BnetGameAccountId id)
    {
        this.m_id = id;
    }

    public void SetLastOnlineMicrosec(ulong microsec)
    {
        this.m_lastOnlineMicrosec = microsec;
    }

    public void SetOnline(bool online)
    {
        this.m_online = online;
    }

    public void SetOwnerId(BnetAccountId id)
    {
        this.m_ownerId = id;
    }

    public void SetProgramId(BnetProgramId programId)
    {
        this.m_programId = programId;
    }

    public void SetRichPresence(string richPresence)
    {
        this.m_richPresence = richPresence;
    }

    public override string ToString()
    {
        if (this.m_id == null)
        {
            return "UNKNOWN GAME ACCOUNT";
        }
        object[] args = new object[] { this.m_id, this.m_programId, this.m_battleTag, this.m_online };
        return string.Format("[id={0} programId={1} battleTag={2} online={3}]", args);
    }

    public bool TryGetGameField(uint fieldId, out object val)
    {
        return this.m_gameFields.TryGetValue(fieldId, out val);
    }

    public bool TryGetGameFieldBool(uint fieldId, out bool val)
    {
        val = false;
        object obj2 = null;
        if (!this.m_gameFields.TryGetValue(fieldId, out obj2))
        {
            return false;
        }
        val = (bool) obj2;
        return true;
    }

    public bool TryGetGameFieldBytes(uint fieldId, out byte[] val)
    {
        val = null;
        object obj2 = null;
        if (!this.m_gameFields.TryGetValue(fieldId, out obj2))
        {
            return false;
        }
        val = (byte[]) obj2;
        return true;
    }

    public bool TryGetGameFieldInt(uint fieldId, out int val)
    {
        val = 0;
        object obj2 = null;
        if (!this.m_gameFields.TryGetValue(fieldId, out obj2))
        {
            return false;
        }
        val = (int) obj2;
        return true;
    }

    public bool TryGetGameFieldString(uint fieldId, out string val)
    {
        val = null;
        object obj2 = null;
        if (!this.m_gameFields.TryGetValue(fieldId, out obj2))
        {
            return false;
        }
        val = (string) obj2;
        return true;
    }
}

