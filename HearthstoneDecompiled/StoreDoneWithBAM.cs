using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class StoreDoneWithBAM : UIBPopup
{
    public UberText m_headlineText;
    public UberText m_messageText;
    public UIBButton m_okayButton;
    private List<ButtonPressedListener> m_okayListeners = new List<ButtonPressedListener>();

    private void Awake()
    {
        this.m_okayButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnOkayPressed));
    }

    private void OnOkayPressed(UIEvent e)
    {
        this.Hide(true);
        foreach (ButtonPressedListener listener in this.m_okayListeners.ToArray())
        {
            listener();
        }
    }

    public void RegisterOkayListener(ButtonPressedListener listener)
    {
        if (!this.m_okayListeners.Contains(listener))
        {
            this.m_okayListeners.Add(listener);
        }
    }

    public void RemoveOkayListener(ButtonPressedListener listener)
    {
        this.m_okayListeners.Remove(listener);
    }

    public delegate void ButtonPressedListener();
}

