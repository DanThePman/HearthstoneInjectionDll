using System;
using UnityEngine;

public class FriendListUIElement : PegUIElement
{
    public GameObject m_Highlight;
    public FriendListUIElement m_ParentElement;
    private bool m_selected;

    protected override void Awake()
    {
        base.Awake();
        this.UpdateHighlight();
    }

    public bool IsSelected()
    {
        return this.m_selected;
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.UpdateHighlight();
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        this.UpdateHighlight();
    }

    public void SetSelected(bool enable)
    {
        if (enable != this.m_selected)
        {
            this.m_selected = enable;
            this.UpdateHighlight();
        }
    }

    protected virtual bool ShouldBeHighlighted()
    {
        return (this.m_selected || (base.GetInteractionState() == PegUIElement.InteractionState.Over));
    }

    protected bool ShouldChildBeHighlighted()
    {
        foreach (FriendListUIElement element in SceneUtils.GetComponentsInChildrenOnly<FriendListUIElement>(this, true))
        {
            if (element.ShouldBeHighlighted())
            {
                return true;
            }
        }
        return false;
    }

    protected void UpdateHighlight()
    {
        bool shouldHighlight = this.ShouldBeHighlighted();
        if (!shouldHighlight)
        {
            shouldHighlight = this.ShouldChildBeHighlighted();
        }
        this.UpdateSelfHighlight(shouldHighlight);
        if (this.m_ParentElement != null)
        {
            this.m_ParentElement.UpdateHighlight();
        }
    }

    protected void UpdateSelfHighlight(bool shouldHighlight)
    {
        if ((this.m_Highlight != null) && (this.m_Highlight.activeSelf != shouldHighlight))
        {
            this.m_Highlight.SetActive(shouldHighlight);
        }
    }
}

