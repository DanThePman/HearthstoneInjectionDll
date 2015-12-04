using System;
using System.Collections.Generic;
using UnityEngine;

public class QuickChatFrame : MonoBehaviour
{
    public GameObject m_Background;
    public QuickChatFrameBones m_Bones;
    public PegUIElement m_ChatLogButton;
    private ChatLogFrame m_chatLogFrame;
    private float m_initialLastMessageShadowScaleZ;
    private float m_initialLastMessageTextHeight;
    private PegUIElement m_inputBlocker;
    public Font m_InputFont;
    public GameObject m_LastMessageShadow;
    public UberText m_LastMessageText;
    private Font m_localizedInputFont;
    public QuickChatFramePrefabs m_Prefabs;
    private BnetPlayer m_receiver;
    public UberText m_ReceiverNameText;
    private DropdownControl m_recentPlayerDropdown;
    private List<BnetPlayer> m_recentPlayers = new List<BnetPlayer>();

    private void Awake()
    {
        this.InitRecentPlayers();
        if (!this.InitReceiver())
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
        else
        {
            BnetWhisperMgr.Get().AddWhisperListener(new BnetWhisperMgr.WhisperCallback(this.OnWhisper));
            BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
            this.InitTransform();
            this.InitInputBlocker();
            this.InitLastMessage();
            this.InitChatLogFrame();
            this.InitInput();
            this.ShowInput(true);
        }
    }

    private void CycleNextReceiver()
    {
        BnetPlayer player;
        int num = this.m_recentPlayers.FindIndex(currReceiver => this.m_receiver == currReceiver);
        if (num == (this.m_recentPlayers.Count - 1))
        {
            player = this.m_recentPlayers[0];
        }
        else
        {
            player = this.m_recentPlayers[num + 1];
        }
        this.SetReceiver(player);
    }

    private void CyclePrevReceiver()
    {
        BnetPlayer player;
        int num = this.m_recentPlayers.FindIndex(currReceiver => this.m_receiver == currReceiver);
        if (num == 0)
        {
            player = this.m_recentPlayers[this.m_recentPlayers.Count - 1];
        }
        else
        {
            player = this.m_recentPlayers[num - 1];
        }
        this.SetReceiver(player);
    }

    private void DefaultChatTransform()
    {
        base.transform.position = BaseUI.Get().m_Bones.m_QuickChat.position;
        base.transform.localScale = BaseUI.Get().m_Bones.m_QuickChat.localScale;
        if (this.m_chatLogFrame != null)
        {
            this.m_chatLogFrame.UpdateLayout();
        }
    }

    private BnetWhisper FindLastWhisperFromReceiver()
    {
        List<BnetWhisper> whispersWithPlayer = BnetWhisperMgr.Get().GetWhispersWithPlayer(this.m_receiver);
        if (whispersWithPlayer != null)
        {
            for (int i = whispersWithPlayer.Count - 1; i >= 0; i--)
            {
                BnetWhisper whisper = whispersWithPlayer[i];
                if (whisper.IsSpeaker(this.m_receiver))
                {
                    return whisper;
                }
            }
        }
        return null;
    }

    public ChatLogFrame GetChatLogFrame()
    {
        return this.m_chatLogFrame;
    }

    public BnetPlayer GetReceiver()
    {
        return this.m_receiver;
    }

    private void HideChatLogFrame()
    {
        UnityEngine.Object.Destroy(this.m_chatLogFrame.gameObject);
        this.m_chatLogFrame = null;
        ChatMgr.Get().OnChatLogFrameHidden();
    }

    private void HideLastMessage()
    {
        this.m_ReceiverNameText.gameObject.SetActive(false);
        this.m_LastMessageText.gameObject.SetActive(false);
        this.m_LastMessageShadow.SetActive(false);
    }

    private void InitChatLogFrame()
    {
        this.m_ChatLogButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnChatLogButtonReleased));
    }

    private void InitInput()
    {
        FontDef fontDef = FontTable.Get().GetFontDef(this.m_InputFont);
        if (fontDef == null)
        {
            this.m_localizedInputFont = this.m_InputFont;
        }
        else
        {
            this.m_localizedInputFont = fontDef.m_Font;
        }
    }

    private void InitInputBlocker()
    {
        Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
        float worldOffset = this.m_Bones.m_InputBlocker.position.z - base.transform.position.z;
        this.m_inputBlocker = CameraUtils.CreateInputBlocker(camera, "QuickChatInputBlocker", this, worldOffset).AddComponent<PegUIElement>();
        this.m_inputBlocker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnInputBlockerReleased));
    }

    private void InitLastMessage()
    {
        this.m_initialLastMessageTextHeight = this.m_LastMessageText.GetTextWorldSpaceBounds().size.y;
        this.m_initialLastMessageShadowScaleZ = this.m_LastMessageShadow.transform.localScale.z;
    }

    private bool InitReceiver()
    {
        this.m_receiver = null;
        if (this.m_recentPlayers.Count == 0)
        {
            string str;
            if (BnetFriendMgr.Get().GetOnlineFriendCount() == 0)
            {
                str = GameStrings.Get("GLOBAL_CHAT_NO_FRIENDS_ONLINE");
            }
            else
            {
                str = GameStrings.Get("GLOBAL_CHAT_NO_RECENT_CONVERSATIONS");
            }
            UIStatus.Get().AddError(str);
            return false;
        }
        this.m_receiver = this.m_recentPlayers[0];
        return true;
    }

    private void InitRecentPlayerDropdown()
    {
        this.m_recentPlayerDropdown = UnityEngine.Object.Instantiate<DropdownControl>(this.m_Prefabs.m_Dropdown);
        this.m_recentPlayerDropdown.transform.parent = base.transform;
        this.m_recentPlayerDropdown.transform.position = this.m_Bones.m_RecentPlayerDropdown.position;
        this.m_recentPlayerDropdown.setItemTextCallback(new DropdownControl.itemTextCallback(this.OnRecentPlayerDropdownText));
        this.m_recentPlayerDropdown.setItemChosenCallback(new DropdownControl.itemChosenCallback(this.OnRecentPlayerDropdownItemChosen));
        this.UpdateRecentPlayerDropdown();
        this.m_recentPlayerDropdown.setSelection(this.m_receiver);
    }

    private void InitRecentPlayers()
    {
        this.UpdateRecentPlayers();
    }

    private void InitTransform()
    {
        base.transform.parent = BaseUI.Get().transform;
        this.DefaultChatTransform();
        if ((UniversalInputManager.Get().IsTouchMode() && W8Touch.s_isWindows8OrGreater) || W8Touch.Get().IsVirtualKeyboardVisible())
        {
            this.TransformChatForKeyboard();
        }
    }

    private void OnChatLogButtonReleased(UIEvent e)
    {
        if (ChatMgr.Get().IsChatLogFrameShown())
        {
            this.HideChatLogFrame();
        }
        else
        {
            this.ShowChatLogFrame();
        }
        this.UpdateReceiver();
        UniversalInputManager.Get().FocusTextInput(base.gameObject);
    }

    private void OnDestroy()
    {
        BnetWhisperMgr.Get().RemoveWhisperListener(new BnetWhisperMgr.WhisperCallback(this.OnWhisper));
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        if (UniversalInputManager.Get() != null)
        {
            UniversalInputManager.Get().CancelTextInput(base.gameObject, false);
        }
    }

    private void OnInputBlockerReleased(UIEvent e)
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void OnInputCanceled(bool userRequested, GameObject requester)
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void OnInputComplete(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            if (this.m_receiver.IsOnline())
            {
                BnetWhisperMgr.Get().SendWhisper(this.m_receiver, input);
                ChatMgr.Get().AddRecentWhisperPlayerToTop(this.m_receiver);
            }
            else if (ChatMgr.Get().IsChatLogFrameShown())
            {
                if (!BnetWhisperMgr.Get().SendWhisper(this.m_receiver, input))
                {
                    this.m_chatLogFrame.m_chatLog.OnWhisperFailed();
                }
                ChatMgr.Get().AddRecentWhisperPlayerToTop(this.m_receiver);
            }
            else
            {
                object[] args = new object[] { this.m_receiver.GetBestName() };
                string message = GameStrings.Format("GLOBAL_CHAT_RECEIVER_OFFLINE", args);
                UIStatus.Get().AddError(message);
            }
        }
        if (ChatMgr.Get().IsChatLogFrameShown())
        {
            this.ShowInput(false);
        }
        else
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private bool OnInputPreprocess(Event e)
    {
        if (this.m_recentPlayers.Count < 2)
        {
            return false;
        }
        if (e.type != EventType.KeyDown)
        {
            return false;
        }
        KeyCode keyCode = e.keyCode;
        bool flag = (e.modifiers & EventModifiers.Shift) != EventModifiers.None;
        if ((keyCode == KeyCode.UpArrow) || ((keyCode == KeyCode.Tab) && flag))
        {
            this.CyclePrevReceiver();
            return true;
        }
        if ((keyCode != KeyCode.DownArrow) && (keyCode != KeyCode.Tab))
        {
            return false;
        }
        this.CycleNextReceiver();
        return true;
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        BnetPlayerChange change = changelist.FindChange(this.m_receiver);
        if (change != null)
        {
            BnetPlayer oldPlayer = change.GetOldPlayer();
            BnetPlayer newPlayer = change.GetNewPlayer();
            if ((oldPlayer == null) || (oldPlayer.IsOnline() != newPlayer.IsOnline()))
            {
                this.UpdateReceiver();
            }
        }
    }

    private void OnRecentPlayerDropdownItemChosen(object selection, object prevSelection)
    {
        BnetPlayer player = (BnetPlayer) selection;
        this.SetReceiver(player);
    }

    private string OnRecentPlayerDropdownText(object val)
    {
        BnetPlayer friend = (BnetPlayer) val;
        return FriendUtils.GetUniqueName(friend);
    }

    private void OnWhisper(BnetWhisper whisper, object userData)
    {
        if ((this.m_receiver != null) && whisper.IsSpeaker(this.m_receiver))
        {
            this.UpdateReceiver();
        }
    }

    public void SetReceiver(BnetPlayer player)
    {
        UniversalInputManager.Get().FocusTextInput(base.gameObject);
        if (this.m_receiver != player)
        {
            this.m_receiver = player;
            this.UpdateReceiver();
            this.m_recentPlayerDropdown.setSelection(player);
            ChatMgr.Get().OnChatReceiverChanged(player);
        }
    }

    private void ShowChatLogFrame()
    {
        this.m_chatLogFrame = UnityEngine.Object.Instantiate<ChatLogFrame>(this.m_Prefabs.m_ChatLogFrame);
        bool flag = base.transform.localScale == BaseUI.Get().m_Bones.m_QuickChatVirtualKeyboard.localScale;
        if ((((UniversalInputManager.Get().IsTouchMode() && W8Touch.s_isWindows8OrGreater) || W8Touch.Get().IsVirtualKeyboardVisible()) && flag) || flag)
        {
            this.DefaultChatTransform();
        }
        this.m_chatLogFrame.transform.parent = base.transform;
        this.m_chatLogFrame.transform.position = this.m_Bones.m_ChatLog.position;
        if ((((UniversalInputManager.Get().IsTouchMode() && W8Touch.s_isWindows8OrGreater) || W8Touch.Get().IsVirtualKeyboardVisible()) && flag) || flag)
        {
            this.TransformChatForKeyboard();
        }
        ChatMgr.Get().OnChatLogFrameShown();
    }

    private void ShowInput(bool fromAwake)
    {
        Rect rect = CameraUtils.CreateGUIViewportRect(BaseUI.Get().GetBnetCamera(), (Component) this.m_Bones.m_InputTopLeft, (Component) this.m_Bones.m_InputBottomRight);
        UniversalInputManager.TextInputParams parms = new UniversalInputManager.TextInputParams {
            m_owner = base.gameObject,
            m_rect = rect,
            m_preprocessCallback = new UniversalInputManager.TextInputPreprocessCallback(this.OnInputPreprocess),
            m_completedCallback = new UniversalInputManager.TextInputCompletedCallback(this.OnInputComplete),
            m_canceledCallback = new UniversalInputManager.TextInputCanceledCallback(this.OnInputCanceled),
            m_font = this.m_localizedInputFont,
            m_maxCharacters = 0x200,
            m_touchScreenKeyboardHideInput = true,
            m_showVirtualKeyboard = fromAwake,
            m_hideVirtualKeyboardOnComplete = !fromAwake ? false : true
        };
        UniversalInputManager.Get().UseTextInput(parms, false);
    }

    private void Start()
    {
        this.InitRecentPlayerDropdown();
        if (ChatMgr.Get().IsChatLogFrameShown())
        {
            this.ShowChatLogFrame();
        }
        this.UpdateReceiver();
        ChatMgr.Get().OnChatReceiverChanged(this.m_receiver);
    }

    private void TransformChatForKeyboard()
    {
        base.transform.position = BaseUI.Get().m_Bones.m_QuickChatVirtualKeyboard.position;
        base.transform.localScale = BaseUI.Get().m_Bones.m_QuickChatVirtualKeyboard.localScale;
        this.m_Prefabs.m_Dropdown.transform.localScale = new Vector3(50f, 50f, 50f);
        if (this.m_chatLogFrame != null)
        {
            this.m_chatLogFrame.UpdateLayout();
        }
    }

    private void UpdateLastMessage()
    {
        if (this.m_chatLogFrame != null)
        {
            this.HideLastMessage();
        }
        else
        {
            BnetWhisper whisper = this.FindLastWhisperFromReceiver();
            if (whisper == null)
            {
                this.HideLastMessage();
            }
            else
            {
                this.m_LastMessageText.gameObject.SetActive(true);
                this.m_LastMessageText.Text = ChatUtils.GetMessage(whisper);
                TransformUtil.SetPoint((Component) this.m_LastMessageText, Anchor.BOTTOM_LEFT, (Component) this.m_Bones.m_LastMessage, Anchor.TOP_LEFT);
                this.m_ReceiverNameText.gameObject.SetActive(true);
                if (this.m_receiver.IsOnline())
                {
                    this.m_ReceiverNameText.TextColor = GameColors.PLAYER_NAME_ONLINE;
                }
                else
                {
                    this.m_ReceiverNameText.TextColor = GameColors.PLAYER_NAME_OFFLINE;
                }
                this.m_ReceiverNameText.Text = FriendUtils.GetUniqueName(this.m_receiver);
                TransformUtil.SetPoint((Component) this.m_ReceiverNameText, Anchor.BOTTOM_LEFT, (Component) this.m_LastMessageText, Anchor.TOP_LEFT);
                this.m_LastMessageShadow.SetActive(true);
                Bounds textWorldSpaceBounds = this.m_LastMessageText.GetTextWorldSpaceBounds();
                Bounds bounds2 = this.m_ReceiverNameText.GetTextWorldSpaceBounds();
                float num = Mathf.Max(textWorldSpaceBounds.max.y, bounds2.max.y);
                float num2 = Mathf.Min(textWorldSpaceBounds.min.y, bounds2.min.y);
                float num3 = num - num2;
                float z = (num3 * this.m_initialLastMessageShadowScaleZ) / this.m_initialLastMessageTextHeight;
                TransformUtil.SetLocalScaleZ(this.m_LastMessageShadow, z);
            }
        }
    }

    public void UpdateLayout()
    {
        if (this.m_chatLogFrame != null)
        {
            this.m_chatLogFrame.UpdateLayout();
        }
    }

    private void UpdateReceiver()
    {
        this.UpdateLastMessage();
        if (this.m_chatLogFrame != null)
        {
            this.m_chatLogFrame.Receiver = this.m_receiver;
        }
    }

    private void UpdateRecentPlayerDropdown()
    {
        this.m_recentPlayerDropdown.clearItems();
        for (int i = 0; i < this.m_recentPlayers.Count; i++)
        {
            this.m_recentPlayerDropdown.addItem(this.m_recentPlayers[i]);
        }
    }

    private void UpdateRecentPlayers()
    {
        this.m_recentPlayers.Clear();
        List<BnetPlayer> recentWhisperPlayers = ChatMgr.Get().GetRecentWhisperPlayers();
        for (int i = 0; i < recentWhisperPlayers.Count; i++)
        {
            BnetPlayer item = recentWhisperPlayers[i];
            this.m_recentPlayers.Add(item);
        }
    }
}

