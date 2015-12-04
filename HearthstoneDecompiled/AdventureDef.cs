using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureDef : MonoBehaviour
{
    [CompilerGenerated]
    private static Comparison<AdventureSubDef> <>f__am$cacheE;
    private AdventureDbId m_AdventureId;
    private string m_AdventureName;
    [CustomEditField(Sections="Reward Banners", T=EditType.GAME_OBJECT)]
    public string m_BannerRewardPrefab;
    [CustomEditField(Sections="Reward Banners")]
    public BannerRewardType m_BannerRewardType;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public string m_ChooserButtonPrefab;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public string m_ChooserSubButtonPrefab;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public string m_DefaultQuotePrefab;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public String_MobileOverride m_ProgressDisplayPrefab;
    private int m_SortOrder;
    private Map<AdventureModeDbId, AdventureSubDef> m_SubDefs = new Map<AdventureModeDbId, AdventureSubDef>();
    [CustomEditField(Sections="Chooser Button", T=EditType.TEXTURE)]
    public string m_Texture;
    [CustomEditField(Sections="Chooser Button")]
    public Vector2 m_TextureOffset = Vector2.zero;
    [CustomEditField(Sections="Chooser Button")]
    public Vector2 m_TextureTiling = Vector2.one;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public string m_WingBottomBorderPrefab;

    public AdventureDbId GetAdventureId()
    {
        return this.m_AdventureId;
    }

    public string GetAdventureName()
    {
        return this.m_AdventureName;
    }

    public List<AdventureSubDef> GetSortedSubDefs()
    {
        List<AdventureSubDef> list = new List<AdventureSubDef>(this.m_SubDefs.Values);
        if (<>f__am$cacheE == null)
        {
            <>f__am$cacheE = (l, r) => l.GetSortOrder() - r.GetSortOrder();
        }
        list.Sort(<>f__am$cacheE);
        return list;
    }

    public int GetSortOrder()
    {
        return this.m_SortOrder;
    }

    public AdventureSubDef GetSubDef(AdventureModeDbId modeId)
    {
        AdventureSubDef def = null;
        this.m_SubDefs.TryGetValue(modeId, out def);
        return def;
    }

    public void Init(DbfRecord advRecord, List<DbfRecord> advDataRecords)
    {
        this.m_AdventureId = (AdventureDbId) advRecord.GetId();
        this.m_AdventureName = advRecord.GetLocString("NAME");
        this.m_SortOrder = advRecord.GetInt("SORT_ORDER");
        foreach (DbfRecord record in advDataRecords)
        {
            if (record.GetInt("ADVENTURE_ID") == this.m_AdventureId)
            {
                string assetPath = record.GetAssetPath("ADVENTURE_SUB_DEF_PREFAB");
                if (!string.IsNullOrEmpty(assetPath))
                {
                    GameObject obj2 = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(assetPath), true, false);
                    if (obj2 != null)
                    {
                        AdventureSubDef component = obj2.GetComponent<AdventureSubDef>();
                        if (component == null)
                        {
                            Debug.LogError(string.Format("{0} object does not contain AdventureSubDef component.", assetPath));
                            UnityEngine.Object.Destroy(obj2);
                        }
                        else
                        {
                            component.Init(record);
                            this.m_SubDefs.Add(component.GetAdventureModeId(), component);
                        }
                    }
                }
            }
        }
    }

    public bool IsActiveAndPlayable()
    {
        foreach (DbfRecord record in GameDbf.Wing.GetRecords())
        {
            if ((record.GetInt("ADVENTURE_ID") == this.GetAdventureId()) && AdventureProgressMgr.Get().IsWingOpen(record.GetId()))
            {
                return true;
            }
        }
        return false;
    }

    public enum BannerRewardType
    {
        AdventureCompleteReward,
        BannerManagerPopup
    }
}

