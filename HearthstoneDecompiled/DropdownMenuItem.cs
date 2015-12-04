using System;
using UnityEngine;

public class DropdownMenuItem : PegUIElement
{
    public GameObject m_selected;
    public UberText m_text;
    private object m_value;

    public object GetValue()
    {
        return this.m_value;
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_text.TextColor = Color.black;
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        this.m_text.TextColor = Color.white;
    }

    public void SetSelected(bool selected)
    {
        if (this.m_selected != null)
        {
            this.m_selected.SetActive(selected);
        }
    }

    public void SetValue(object val, string text)
    {
        this.m_value = val;
        this.m_text.Text = text;
    }
}

