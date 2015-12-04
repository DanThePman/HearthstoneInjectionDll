using System;
using UnityEngine;

public class RibbonButton : PegUIElement
{
    public GameObject m_highlight;

    public void OnButtonOut(UIEvent e)
    {
        if (this.m_highlight != null)
        {
            this.m_highlight.SetActive(false);
        }
    }

    public void OnButtonOver(UIEvent e)
    {
        if (this.m_highlight != null)
        {
            this.m_highlight.SetActive(true);
        }
    }

    public void Start()
    {
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnButtonOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnButtonOut));
    }
}

