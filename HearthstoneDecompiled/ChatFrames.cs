using System;
using UnityEngine;

public class ChatFrames : MonoBehaviour
{
    public MobileChatLogFrame chatLogFrame;
    private bool wasShowingDialog;

    private void Awake()
    {
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        BnetEventMgr.Get().AddChangeListener(new BnetEventMgr.ChangeCallback(this.OnBnetEventOccurred));
        this.chatLogFrame.CloseButtonReleased += new System.Action(this.OnCloseButtonReleased);
    }

    public void Back()
    {
        if (!DialogManager.Get().ShowingDialog())
        {
            if (ChatMgr.Get().FriendListFrame.ShowingAddFriendFrame)
            {
                ChatMgr.Get().FriendListFrame.CloseAddFriendFrame();
            }
            else if (this.Receiver != null)
            {
                this.Receiver = null;
            }
            else
            {
                ChatMgr.Get().CloseChatUI();
            }
        }
    }

    public void Hide()
    {
        base.gameObject.SetActive(false);
    }

    private void OnBnetEventOccurred(BattleNet.BnetEvent bnetEvent, object userData)
    {
        if (bnetEvent == BattleNet.BnetEvent.Disconnected)
        {
            ChatMgr.Get().CleanUp();
        }
    }

    private void OnCloseButtonReleased()
    {
        ChatMgr.Get().CloseChatUI();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            ChatMgr.Get().ShowFriendsList();
        }
    }

    private void OnDestroy()
    {
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        }
        if (BnetEventMgr.Get() != null)
        {
            BnetEventMgr.Get().RemoveChangeListener(new BnetEventMgr.ChangeCallback(this.OnBnetEventOccurred));
        }
        this.OnFramesMoved();
    }

    private void OnFramesMoved()
    {
        if (ChatMgr.Get() != null)
        {
            ChatMgr.Get().OnChatFramesMoved();
        }
    }

    private void OnPopupClosed()
    {
        if (this.Receiver != null)
        {
            this.chatLogFrame.Focus(true);
        }
    }

    private void OnPopupOpened()
    {
        if (this.chatLogFrame.HasFocus)
        {
            this.chatLogFrame.Focus(false);
        }
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (mode == SceneMgr.Mode.FATAL_ERROR)
        {
            ChatMgr.Get().CleanUp();
        }
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
    }

    private void Update()
    {
        bool flag = DialogManager.Get().ShowingDialog();
        if (flag != this.wasShowingDialog)
        {
            if (flag && this.chatLogFrame.HasFocus)
            {
                this.OnPopupOpened();
            }
            else if ((!flag && !ChatMgr.Get().FriendListFrame.ShowingAddFriendFrame) && !this.chatLogFrame.HasFocus)
            {
                this.OnPopupClosed();
            }
            this.wasShowingDialog = flag;
        }
    }

    public BnetPlayer Receiver
    {
        get
        {
            return this.chatLogFrame.Receiver;
        }
        set
        {
            this.chatLogFrame.Receiver = value;
            if (this.chatLogFrame.Receiver == null)
            {
                ChatMgr.Get().CloseChatUI();
            }
            this.OnFramesMoved();
        }
    }
}

