using System;
using UnityEngine;

public class QuestProgressToast : GameToast
{
    public GameObject m_background;
    public UberText m_questDescription;
    public UberText m_questProgressCount;
    public GameObject m_questProgressCountBg;
    public UberText m_questTitle;

    private void Awake()
    {
        base.m_intensityMaterials.Add(this.m_questProgressCountBg.GetComponent<Renderer>().material);
        base.m_intensityMaterials.Add(this.m_background.GetComponent<Renderer>().material);
    }

    public void UpdateDisplay(string title, string description, int progress, int maxProgress)
    {
        if (maxProgress > 1)
        {
            this.m_questProgressCountBg.SetActive(true);
            object[] args = new object[] { progress, maxProgress };
            this.m_questProgressCount.Text = GameStrings.Format("GLOBAL_QUEST_PROGRESS_COUNT", args);
        }
        else
        {
            this.m_questProgressCountBg.SetActive(false);
        }
        this.m_questTitle.Text = title;
        this.m_questDescription.Text = description;
    }
}

