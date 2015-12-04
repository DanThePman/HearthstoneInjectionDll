using System;
using UnityEngine;

public class MobileChatLogUI : IChatLogUI
{
    private ChatFrames m_chatFrames;

    public void GoBack()
    {
        if (this.IsShowing)
        {
            this.m_chatFrames.Back();
        }
    }

    public void Hide()
    {
        if (this.IsShowing)
        {
            UnityEngine.Object.Destroy(this.m_chatFrames.gameObject);
            this.m_chatFrames = null;
        }
    }

    public void ShowForPlayer(BnetPlayer player)
    {
        string name = (UniversalInputManager.UsePhoneUI == null) ? "MobileChatFrames" : "MobileChatFrames_phone";
        UnityEngine.GameObject obj2 = AssetLoader.Get().LoadGameObject(name, true, false);
        if (obj2 != null)
        {
            this.m_chatFrames = obj2.GetComponent<ChatFrames>();
            this.m_chatFrames.Receiver = player;
        }
    }

    public UnityEngine.GameObject GameObject
    {
        get
        {
            return ((this.m_chatFrames != null) ? this.m_chatFrames.gameObject : null);
        }
    }

    public bool IsShowing
    {
        get
        {
            return (this.m_chatFrames != null);
        }
    }

    public BnetPlayer Receiver
    {
        get
        {
            return ((this.m_chatFrames != null) ? this.m_chatFrames.Receiver : null);
        }
    }
}

