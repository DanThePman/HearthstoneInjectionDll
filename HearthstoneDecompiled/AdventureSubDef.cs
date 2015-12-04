using System;
using UnityEngine;

[CustomEditClass]
public class AdventureSubDef : MonoBehaviour
{
    private AdventureModeDbId m_AdventureModeId;
    [CustomEditField(Sections="Chooser", T=EditType.GAME_OBJECT)]
    public String_MobileOverride m_ChooserDescriptionPrefab;
    private string m_CompleteBannerText;
    private string m_Description;
    private string m_RequirementsDescription;
    private string m_ShortName;
    private int m_SortOrder;
    [CustomEditField(Sections="Chooser", T=EditType.TEXTURE)]
    public string m_Texture;
    [CustomEditField(Sections="Chooser")]
    public Vector2 m_TextureOffset = Vector2.zero;
    [CustomEditField(Sections="Chooser")]
    public Vector2 m_TextureTiling = Vector2.one;
    [CustomEditField(Sections="Mission Display", T=EditType.TEXTURE)]
    public string m_WatermarkTexture;

    public AdventureModeDbId GetAdventureModeId()
    {
        return this.m_AdventureModeId;
    }

    public string GetCompleteBannerText()
    {
        return this.m_CompleteBannerText;
    }

    public string GetDescription()
    {
        return this.m_Description;
    }

    public string GetRequirementsDescription()
    {
        return this.m_RequirementsDescription;
    }

    public string GetShortName()
    {
        return this.m_ShortName;
    }

    public int GetSortOrder()
    {
        return this.m_SortOrder;
    }

    public void Init(DbfRecord advDataRecord)
    {
        this.m_AdventureModeId = (AdventureModeDbId) advDataRecord.GetInt("MODE_ID");
        this.m_SortOrder = advDataRecord.GetInt("SORT_ORDER");
        this.m_ShortName = advDataRecord.GetLocString("SHORT_NAME");
        this.m_Description = advDataRecord.GetLocString((UniversalInputManager.UsePhoneUI == null) ? "DESCRIPTION" : "SHORT_DESCRIPTION");
        this.m_RequirementsDescription = advDataRecord.GetLocString("REQUIREMENTS_DESCRIPTION");
        this.m_CompleteBannerText = advDataRecord.GetLocString("COMPLETE_BANNER_TEXT");
    }
}

