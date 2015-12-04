using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class AdventureWingDef : MonoBehaviour
{
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_AccentPrefab;
    private AdventureDbId m_AdventureId;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_CoinPrefab;
    private string m_ComingSoonLabel;
    [CustomEditField(Sections="Complete Quote")]
    public string m_CompleteQuotePrefab;
    [CustomEditField(Sections="Complete Quote")]
    public string m_CompleteQuoteVOLine;
    [CustomEditField(Sections="Rewards Preview")]
    public int m_HiddenRewardsPreviewCount;
    [CustomEditField(Sections="Loc Strings")]
    public string m_LockedLocString;
    [CustomEditField(Sections="Loc Strings")]
    public string m_LockedPurchaseLocString;
    [CustomEditField(Sections="Opening Quote")]
    public string m_OpenQuotePrefab;
    [CustomEditField(Sections="Opening Quote")]
    public string m_OpenQuoteVOLine;
    private WingDbId m_OwnershipPrereq;
    private string m_RequiresLabel;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_RewardsPrefab;
    private int m_SortOrder;
    [CustomEditField(Sections="Rewards Preview")]
    public List<string> m_SpecificRewardsPreviewCards;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_UnlockSpellPrefab;
    private WingDbId m_WingId;
    private string m_WingName;
    [CustomEditField(Sections="Wing Open Popup", T=EditType.GAME_OBJECT)]
    public string m_WingOpenPopup;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_WingPrefab;

    public AdventureDbId GetAdventureId()
    {
        return this.m_AdventureId;
    }

    public string GetComingSoonLabel()
    {
        return this.m_ComingSoonLabel;
    }

    public WingDbId GetOwnershipPrereqId()
    {
        return this.m_OwnershipPrereq;
    }

    public string GetRequiresLabel()
    {
        return this.m_RequiresLabel;
    }

    public int GetSortOrder()
    {
        return this.m_SortOrder;
    }

    public WingDbId GetWingId()
    {
        return this.m_WingId;
    }

    public string GetWingName()
    {
        return this.m_WingName;
    }

    public void Init(DbfRecord wingRecord)
    {
        this.m_AdventureId = (AdventureDbId) wingRecord.GetInt("ADVENTURE_ID");
        this.m_WingId = (WingDbId) wingRecord.GetId();
        this.m_OwnershipPrereq = (WingDbId) wingRecord.GetInt("OWNERSHIP_PREREQ_WING_ID");
        this.m_SortOrder = wingRecord.GetInt("SORT_ORDER");
        this.m_WingName = wingRecord.GetLocString("NAME");
        this.m_ComingSoonLabel = wingRecord.GetLocString("COMING_SOON_LABEL");
        this.m_RequiresLabel = wingRecord.GetLocString("REQUIRES_LABEL");
    }
}

