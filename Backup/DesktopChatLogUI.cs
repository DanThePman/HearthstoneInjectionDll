using System;
using UnityEngine;

public class DesktopChatLogUI : IChatLogUI
{
    private QuickChatFrame m_quickChatFrame;

    public void GoBack()
    {
    }

    public void Hide()
    {
        if (this.m_quickChatFrame != null)
        {
            UnityEngine.Object.Destroy(this.m_quickChatFrame.gameObject);
            this.m_quickChatFrame = null;
        }
    }

    public void ShowForPlayer(BnetPlayer player)
    {
        if (this.m_quickChatFrame == null)
        {
            UnityEngine.GameObject obj2 = AssetLoader.Get().LoadGameObject("QuickChatFrame", true, false);
            if (obj2 != null)
            {
                this.m_quickChatFrame = obj2.GetComponent<QuickChatFrame>();
                this.m_quickChatFrame.SetReceiver(player);
            }
        }
    }

    public UnityEngine.GameObject GameObject
    {
        get
        {
            return ((this.m_quickChatFrame != null) ? this.m_quickChatFrame.gameObject : null);
        }
    }

    public bool IsShowing
    {
        get
        {
            return (this.m_quickChatFrame != null);
        }
    }

    public BnetPlayer Receiver
    {
        get
        {
            return ((this.m_quickChatFrame != null) ? this.m_quickChatFrame.GetReceiver() : null);
        }
    }
}

