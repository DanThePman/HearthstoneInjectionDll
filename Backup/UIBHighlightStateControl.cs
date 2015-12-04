using System;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class UIBHighlightStateControl : MonoBehaviour
{
    [CustomEditField(Sections="Behavior Settings")]
    public bool m_AllowSelection;
    [CustomEditField(Sections="Behavior Settings")]
    public bool m_EnableResponse = true;
    [CustomEditField(Sections="Highlight State Reference")]
    public HighlightState m_HighlightState;
    private bool m_MouseOver;
    [CustomEditField(Sections="Highlight State Type")]
    public ActorStateType m_MouseOverStateType = ActorStateType.HIGHLIGHT_MOUSE_OVER;
    private PegUIElement m_PegUIElement;
    [CustomEditField(Sections="Highlight State Type")]
    public ActorStateType m_PrimarySelectedStateType = ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE;
    [CustomEditField(Sections="Highlight State Type")]
    public ActorStateType m_SecondarySelectedStateType = ActorStateType.HIGHLIGHT_SECONDARY_ACTIVE;
    [CustomEditField(Sections="Behavior Settings")]
    public bool m_UseMouseOver;

    private void Awake()
    {
        PegUIElement component = base.gameObject.GetComponent<PegUIElement>();
        if (component != null)
        {
            component.AddEventListener(UIEventType.ROLLOVER, delegate (UIEvent e) {
                if (this.m_EnableResponse)
                {
                    this.OnRollOver();
                }
            });
            component.AddEventListener(UIEventType.ROLLOUT, delegate (UIEvent e) {
                if (this.m_EnableResponse)
                {
                    this.OnRollOut();
                }
            });
            component.AddEventListener(UIEventType.RELEASE, delegate (UIEvent e) {
                if (this.m_EnableResponse)
                {
                    this.OnRelease();
                }
            });
        }
    }

    public bool IsReady()
    {
        return this.m_HighlightState.IsReady();
    }

    private void OnRelease()
    {
        if (this.m_AllowSelection)
        {
            this.Select(true, false);
        }
        else if (this.m_MouseOver)
        {
            this.m_HighlightState.ChangeState(this.m_MouseOverStateType);
        }
        else
        {
            this.m_HighlightState.ChangeState(ActorStateType.NONE);
        }
    }

    private void OnRollOut()
    {
        if (this.m_UseMouseOver)
        {
            this.m_MouseOver = false;
            if (!this.m_AllowSelection)
            {
                this.m_HighlightState.ChangeState(ActorStateType.NONE);
            }
        }
    }

    private void OnRollOver()
    {
        if (this.m_UseMouseOver)
        {
            this.m_MouseOver = true;
            this.m_HighlightState.ChangeState(this.m_MouseOverStateType);
        }
    }

    public void Select(bool selected, bool primary = false)
    {
        if (selected)
        {
            this.m_HighlightState.ChangeState(!primary ? this.m_SecondarySelectedStateType : this.m_PrimarySelectedStateType);
        }
        else if (this.m_MouseOver)
        {
            this.m_HighlightState.ChangeState(this.m_MouseOverStateType);
        }
        else
        {
            this.m_HighlightState.ChangeState(ActorStateType.NONE);
        }
    }
}

