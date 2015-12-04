using System;
using UnityEngine;

public class StoreDisclaimer : MonoBehaviour
{
    public UberText m_detailsText;
    public UberText m_headlineText;
    public GameObject m_root;
    public UberText m_warningText;

    private void Awake()
    {
        this.m_headlineText.Text = GameStrings.Get("GLUE_STORE_DISCLAIMER_HEADLINE");
        this.m_warningText.Text = GameStrings.Get("GLUE_STORE_DISCLAIMER_WARNING");
        this.m_detailsText.Text = string.Empty;
    }

    public void SetDetailsText(string detailsText)
    {
        this.m_detailsText.Text = detailsText;
    }

    public void UpdateTextSize()
    {
        this.m_headlineText.UpdateNow();
        this.m_warningText.UpdateNow();
        this.m_detailsText.UpdateNow();
    }
}

