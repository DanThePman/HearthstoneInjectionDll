using PegasusShared;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Debug1v1Button : PegUIElement
{
    public GameObject m_heroImage;
    private GameObject m_heroPowerObject;
    public int m_missionId;
    public UberText m_name;

    private void OnCardDefLoaded(string cardID, CardDef cardDef, object userData)
    {
        this.m_heroImage.GetComponent<Renderer>().material.mainTexture = cardDef.GetPortraitTexture();
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        long selectedDeckID = DeckPickerTrayDisplay.Get().GetSelectedDeckID();
        HasUsedDebugMenu = true;
        GameMgr.Get().FindGame(GameType.GT_TAVERNBRAWL, this.m_missionId, selectedDeckID, 0L);
        UnityEngine.Object.Destroy(base.transform.parent.gameObject);
    }

    private void Start()
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(this.m_missionId);
        if (record != null)
        {
            string locString = record.GetLocString("SHORT_NAME");
            if ((this.m_name != null) && !string.IsNullOrEmpty(locString))
            {
                this.m_name.Text = locString;
            }
        }
    }

    public static bool HasUsedDebugMenu
    {
        [CompilerGenerated]
        get
        {
            return <HasUsedDebugMenu>k__BackingField;
        }
        [CompilerGenerated]
        set
        {
            <HasUsedDebugMenu>k__BackingField = value;
        }
    }
}

