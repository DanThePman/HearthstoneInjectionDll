using System;
using UnityEngine;

public class RewardBanner : MonoBehaviour
{
    public UberText m_detailsText;
    public GameObject m_headlineCenterBone;
    private float m_headlineHeight;
    public UberText m_headlineText;
    public UberText m_sourceText;

    public void AlignHeadlineToCenterBone()
    {
        if (this.m_headlineCenterBone != null)
        {
            this.m_headlineText.transform.localPosition = this.m_headlineCenterBone.transform.localPosition;
        }
    }

    private void Awake()
    {
        if ((UniversalInputManager.UsePhoneUI != null) && (this.m_sourceText != null))
        {
            this.m_sourceText.gameObject.SetActive(false);
        }
        this.m_headlineHeight = this.m_headlineText.Height;
    }

    public void SetText(string headline, string details, string source)
    {
        this.m_headlineText.Text = headline;
        this.m_detailsText.Text = details;
        this.m_sourceText.Text = source;
        if (details == string.Empty)
        {
            this.AlignHeadlineToCenterBone();
            this.m_headlineText.Height = this.m_headlineHeight * 1.5f;
        }
    }

    public string DetailsText
    {
        get
        {
            return this.m_detailsText.Text;
        }
    }

    public string HeadlineText
    {
        get
        {
            return this.m_headlineText.Text;
        }
    }

    public string SourceText
    {
        get
        {
            return this.m_sourceText.Text;
        }
    }
}

