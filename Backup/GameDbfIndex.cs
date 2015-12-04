using System;
using System.Collections.Generic;
using UnityEngine;

public class GameDbfIndex
{
    private List<int> m_allCardDbIds;
    private List<string> m_allCardIds;
    private Map<string, DbfRecord> m_cardsByCardId;
    private Map<int, DbfRecord> m_cardsByDbId;
    private int m_collectibleCardCount;
    private List<int> m_collectibleCardDbIds;
    private List<string> m_collectibleCardIds;
    private Map<FixedActionType, List<DbfRecord>> m_fixedActionRecordsByType;
    private Map<int, List<DbfRecord>> m_fixedRewardsByAction;

    public GameDbfIndex()
    {
        this.Initialize();
    }

    public List<int> GetAllCardDbIds()
    {
        return this.m_allCardDbIds;
    }

    public List<string> GetAllCardIds()
    {
        return this.m_allCardIds;
    }

    public DbfRecord GetCardRecord(int dbId)
    {
        DbfRecord record = null;
        this.m_cardsByDbId.TryGetValue(dbId, out record);
        return record;
    }

    public DbfRecord GetCardRecord(string cardId)
    {
        DbfRecord record = null;
        this.m_cardsByCardId.TryGetValue(cardId, out record);
        return record;
    }

    public Map<string, DbfRecord> GetCardsByCardId()
    {
        return this.m_cardsByCardId;
    }

    public Map<int, DbfRecord> GetCardsByDbId()
    {
        return this.m_cardsByDbId;
    }

    public int GetCollectibleCardCount()
    {
        return this.m_collectibleCardCount;
    }

    public List<int> GetCollectibleCardDbIds()
    {
        return this.m_collectibleCardDbIds;
    }

    public List<string> GetCollectibleCardIds()
    {
        return this.m_collectibleCardIds;
    }

    public List<DbfRecord> GetFixedActionRecordsForType(FixedActionType type)
    {
        List<DbfRecord> list = null;
        if (!this.m_fixedActionRecordsByType.TryGetValue(type, out list))
        {
            list = new List<DbfRecord>();
        }
        return list;
    }

    public List<DbfRecord> GetFixedRewardMapRecordsForAction(int actionId)
    {
        List<DbfRecord> list = null;
        if (!this.m_fixedRewardsByAction.TryGetValue(actionId, out list))
        {
            list = new List<DbfRecord>();
        }
        return list;
    }

    public void Initialize()
    {
        this.m_cardsByDbId = new Map<int, DbfRecord>();
        this.m_cardsByCardId = new Map<string, DbfRecord>();
        this.m_allCardIds = new List<string>();
        this.m_allCardDbIds = new List<int>();
        this.m_collectibleCardIds = new List<string>();
        this.m_collectibleCardDbIds = new List<int>();
        this.m_collectibleCardCount = 0;
        this.m_fixedRewardsByAction = new Map<int, List<DbfRecord>>();
        this.m_fixedActionRecordsByType = new Map<FixedActionType, List<DbfRecord>>();
    }

    public void OnCardAdded(DbfRecord cardRecord)
    {
        int id = cardRecord.GetId();
        bool @bool = cardRecord.GetBool("IS_COLLECTIBLE");
        string item = cardRecord.GetString("NOTE_MINI_GUID");
        this.m_cardsByDbId[id] = cardRecord;
        this.m_cardsByCardId[item] = cardRecord;
        this.m_allCardDbIds.Add(id);
        this.m_allCardIds.Add(item);
        if (@bool)
        {
            this.m_collectibleCardCount++;
            this.m_collectibleCardIds.Add(item);
            this.m_collectibleCardDbIds.Add(id);
        }
    }

    public void OnFixedRewardActionAdded(DbfRecord record)
    {
        FixedActionType type;
        List<DbfRecord> list;
        string str = record.GetString("TYPE");
        try
        {
            type = EnumUtils.GetEnum<FixedActionType>(str);
        }
        catch
        {
            Debug.LogError("Error parsing FixedRewardAction, type did not match a FixedRewardType");
            return;
        }
        if (!this.m_fixedActionRecordsByType.TryGetValue(type, out list))
        {
            list = new List<DbfRecord>();
            this.m_fixedActionRecordsByType.Add(type, list);
        }
        list.Add(record);
    }

    public void OnFixedRewardMapAdded(DbfRecord record)
    {
        List<DbfRecord> list;
        int @int = record.GetInt("ACTION_ID");
        if (!this.m_fixedRewardsByAction.TryGetValue(@int, out list))
        {
            list = new List<DbfRecord>();
            this.m_fixedRewardsByAction.Add(@int, list);
        }
        list.Add(record);
    }
}

