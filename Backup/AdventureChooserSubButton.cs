using System;
using UnityEngine;

[CustomEditClass]
public class AdventureChooserSubButton : AdventureGenericButton
{
    private bool m_Glow;
    [CustomEditField(Sections="Progress UI")]
    public GameObject m_heroicSkull;
    [CustomEditField(Sections="Progress UI")]
    public GameObject m_progressCounter;
    [CustomEditField(Sections="Progress UI")]
    public UberText m_progressCounterText;
    [CustomEditField(Sections="Event Table")]
    public StateEventTable m_StateTable;
    private AdventureDbId m_TargetAdventure;
    private AdventureModeDbId m_TargetMode;
    private const string s_EventFlash = "Flash";

    public void Flash()
    {
        this.m_StateTable.TriggerState("Flash", true, null);
    }

    public AdventureDbId GetAdventure()
    {
        return this.m_TargetAdventure;
    }

    public AdventureModeDbId GetMode()
    {
        return this.m_TargetMode;
    }

    public bool IsReady()
    {
        UIBHighlightStateControl component = base.GetComponent<UIBHighlightStateControl>();
        return ((component != null) && component.IsReady());
    }

    public void SetAdventure(AdventureDbId id, AdventureModeDbId mode)
    {
        this.m_TargetAdventure = id;
        this.m_TargetMode = mode;
        this.ShowRemainingProgressCount();
    }

    public void SetHighlight(bool enable)
    {
        UIBHighlightStateControl component = base.GetComponent<UIBHighlightStateControl>();
        if (component != null)
        {
            if (this.m_Glow)
            {
                component.Select(true, true);
            }
            else
            {
                component.Select(enable, false);
            }
        }
        UIBHighlight highlight = base.GetComponent<UIBHighlight>();
        if (highlight != null)
        {
            highlight.AlwaysOver = enable;
        }
    }

    public void SetNewGlow(bool enable)
    {
        this.m_Glow = enable;
        UIBHighlightStateControl component = base.GetComponent<UIBHighlightStateControl>();
        if (component != null)
        {
            component.Select(enable, true);
        }
    }

    private void ShowRemainingProgressCount()
    {
        int playableClassChallenges = 0;
        if (this.m_TargetMode == AdventureModeDbId.CLASS_CHALLENGE)
        {
            playableClassChallenges = AdventureProgressMgr.Get().GetPlayableClassChallenges(this.m_TargetAdventure, this.m_TargetMode);
        }
        if ((this.m_TargetMode == AdventureModeDbId.NORMAL) || (this.m_TargetMode == AdventureModeDbId.HEROIC))
        {
            playableClassChallenges = AdventureProgressMgr.Get().GetPlayableAdventureScenarios(this.m_TargetAdventure, this.m_TargetMode);
        }
        if (this.m_TargetMode == AdventureModeDbId.HEROIC)
        {
            if (playableClassChallenges > 0)
            {
                this.m_heroicSkull.SetActive(true);
            }
            else
            {
                this.m_heroicSkull.SetActive(false);
            }
            this.m_progressCounter.SetActive(false);
        }
        else
        {
            this.m_heroicSkull.SetActive(false);
            if (playableClassChallenges > 0)
            {
                this.m_progressCounter.SetActive(true);
                this.m_progressCounterText.Text = playableClassChallenges.ToString();
            }
            else
            {
                this.m_progressCounter.SetActive(false);
            }
        }
    }
}

