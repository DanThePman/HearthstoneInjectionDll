using System;
using UnityEngine;

public class SeasonTimeRemainingToast : GameToast
{
    public GameObject m_background;
    public UberText m_seasonTitle;
    public UberText m_timeRemaining;

    private void Awake()
    {
        base.m_intensityMaterials.Add(this.m_background.GetComponent<Renderer>().material);
    }

    public void UpdateDisplay(string title, string timeRemaining)
    {
        this.m_seasonTitle.Text = title;
        this.m_timeRemaining.Text = timeRemaining;
    }
}

