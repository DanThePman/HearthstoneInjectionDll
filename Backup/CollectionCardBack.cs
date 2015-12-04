using System;
using UnityEngine;

public class CollectionCardBack : MonoBehaviour
{
    private int m_cardBackId = -1;
    public GameObject m_favoriteBanner;
    public UberText m_name;
    public GameObject m_nameFrame;
    private int m_seasonId = -1;

    public void Awake()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_nameFrame.SetActive(false);
        }
    }

    public int GetCardBackId()
    {
        return this.m_cardBackId;
    }

    public int GetSeasonId()
    {
        return this.m_seasonId;
    }

    public void SetCardBackId(int id)
    {
        this.m_cardBackId = id;
    }

    public void SetCardBackName(string name)
    {
        if (this.m_name != null)
        {
            this.m_name.Text = name;
        }
    }

    public void SetSeasonId(int seasonId)
    {
        this.m_seasonId = seasonId;
    }

    public void ShowFavoriteBanner(bool show)
    {
        if (this.m_favoriteBanner != null)
        {
            this.m_favoriteBanner.SetActive(show);
        }
    }
}

