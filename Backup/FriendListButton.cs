using System;
using UnityEngine;

public class FriendListButton : FriendListUIElement
{
    public GameObject m_ActiveGlow;
    public GameObject m_Background;
    public UberText m_Text;

    public string GetText()
    {
        return this.m_Text.Text;
    }

    public void SetText(string text)
    {
        this.m_Text.Text = text;
        this.UpdateAll();
    }

    public void ShowActiveGlow(bool show)
    {
        if (this.m_ActiveGlow != null)
        {
            HighlightState componentInChildren = this.m_ActiveGlow.GetComponentInChildren<HighlightState>();
            if (componentInChildren != null)
            {
                if (show)
                {
                    componentInChildren.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
                }
                else
                {
                    componentInChildren.ChangeState(ActorStateType.HIGHLIGHT_OFF);
                }
            }
        }
    }

    private void UpdateAll()
    {
        base.UpdateHighlight();
    }
}

