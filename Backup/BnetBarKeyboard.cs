using System;
using System.Collections.Generic;
using UnityEngine;

public class BnetBarKeyboard : PegUIElement
{
    public Color m_highlight;
    private List<OnKeyboardPressed> m_keyboardPressedListeners = new List<OnKeyboardPressed>();
    public Color m_origColor;

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.ShowHighlight(false);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        this.ShowHighlight(true);
    }

    protected override void OnPress()
    {
        W8Touch.Get().ShowKeyboard();
        foreach (OnKeyboardPressed pressed in this.m_keyboardPressedListeners.ToArray())
        {
            pressed();
        }
    }

    public void RegisterKeyboardPressedListener(OnKeyboardPressed listener)
    {
        if (!this.m_keyboardPressedListeners.Contains(listener))
        {
            this.m_keyboardPressedListeners.Add(listener);
        }
    }

    public void ShowHighlight(bool show)
    {
        if (show)
        {
            base.gameObject.GetComponent<Renderer>().material.SetColor("_Color", this.m_highlight);
        }
        else
        {
            base.gameObject.GetComponent<Renderer>().material.SetColor("_Color", this.m_origColor);
        }
    }

    public void UnregisterKeyboardPressedListener(OnKeyboardPressed listener)
    {
        this.m_keyboardPressedListeners.Remove(listener);
    }
}

