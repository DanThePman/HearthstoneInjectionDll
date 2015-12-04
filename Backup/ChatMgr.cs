using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ChatMgr : MonoBehaviour
{
    private Rect keyboardArea = new Rect(0f, 0f, 0f, 0f);
    private KeyboardState keyboardState;
    private List<ChatBubbleFrame> m_chatBubbleFrames = new List<ChatBubbleFrame>();
    public ChatMgrBubbleInfo m_ChatBubbleInfo;
    private bool m_chatLogFrameShown;
    private IChatLogUI m_chatLogUI;
    public float m_chatLogXOffset;
    private PegUIElement m_closeCatcher;
    private FriendListFrame m_friendListFrame;
    public Float_MobileOverride m_friendsListHeightPadding;
    public Float_MobileOverride m_friendsListWidth;
    public Float_MobileOverride m_friendsListWidthPadding;
    public Float_MobileOverride m_friendsListXOffset;
    public Float_MobileOverride m_friendsListYOffset;
    private List<PlayerChatInfoChangedListener> m_playerChatInfoChangedListeners = new List<PlayerChatInfoChangedListener>();
    private Map<BnetPlayer, PlayerChatInfo> m_playerChatInfos = new Map<BnetPlayer, PlayerChatInfo>();
    public ChatMgrPrefabs m_Prefabs;
    private List<BnetPlayer> m_recentWhisperPlayers = new List<BnetPlayer>();
    private static ChatMgr s_instance;

    public void AddPlayerChatInfoChangedListener(PlayerChatInfoChangedCallback callback)
    {
        this.AddPlayerChatInfoChangedListener(callback, null);
    }

    public void AddPlayerChatInfoChangedListener(PlayerChatInfoChangedCallback callback, object userData)
    {
        PlayerChatInfoChangedListener item = new PlayerChatInfoChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_playerChatInfoChangedListeners.Contains(item))
        {
            this.m_playerChatInfoChangedListeners.Add(item);
        }
    }

    public void AddRecentWhisperPlayerToBottom(BnetPlayer player)
    {
        if (!this.m_recentWhisperPlayers.Contains(player))
        {
            if (this.m_recentWhisperPlayers.Count == 10)
            {
                this.m_recentWhisperPlayers.RemoveAt(this.m_recentWhisperPlayers.Count - 1);
            }
            this.m_recentWhisperPlayers.Add(player);
        }
    }

    public void AddRecentWhisperPlayerToTop(BnetPlayer player)
    {
        <AddRecentWhisperPlayerToTop>c__AnonStorey2AD storeyad = new <AddRecentWhisperPlayerToTop>c__AnonStorey2AD {
            player = player
        };
        int index = this.m_recentWhisperPlayers.FindIndex(new Predicate<BnetPlayer>(storeyad.<>m__4A));
        if (index < 0)
        {
            if (this.m_recentWhisperPlayers.Count == 10)
            {
                this.m_recentWhisperPlayers.RemoveAt(this.m_recentWhisperPlayers.Count - 1);
            }
        }
        else
        {
            this.m_recentWhisperPlayers.RemoveAt(index);
        }
        this.m_recentWhisperPlayers.Insert(0, storeyad.player);
    }

    private void Awake()
    {
        s_instance = this;
        BnetWhisperMgr.Get().AddWhisperListener(new BnetWhisperMgr.WhisperCallback(this.OnWhisper));
        BnetFriendMgr.Get().AddChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        W8Touch.Get().VirtualKeyboardDidShow += new System.Action(this.OnKeyboardShow);
        W8Touch.Get().VirtualKeyboardDidHide += new System.Action(this.OnKeyboardHide);
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
        this.InitCloseCatcher();
        this.InitChatLogUI();
    }

    public void CleanUp()
    {
        this.DestroyFriendListFrame();
    }

    public void CloseChatUI()
    {
        if (this.m_chatLogUI.IsShowing)
        {
            this.m_chatLogUI.Hide();
        }
        this.DestroyFriendListFrame();
    }

    private KeyboardState ComputeKeyboardState()
    {
        if (this.keyboardArea.height > 0f)
        {
            float y = this.keyboardArea.y;
            float num2 = Screen.height - this.keyboardArea.yMax;
            return ((y <= num2) ? KeyboardState.Above : KeyboardState.Below);
        }
        return KeyboardState.None;
    }

    private ChatBubbleFrame CreateChatBubble(BnetWhisper whisper)
    {
        ChatBubbleFrame c = this.InstantiateChatBubble(this.m_Prefabs.m_ChatBubbleOneLineFrame, whisper);
        if (!c.DoesMessageFit())
        {
            UnityEngine.Object.Destroy(c.gameObject);
            c = this.InstantiateChatBubble(this.m_Prefabs.m_ChatBubbleSmallFrame, whisper);
        }
        SceneUtils.SetLayer(c, GameLayer.BattleNetDialog);
        return c;
    }

    private FriendListFrame CreateFriendsListUI()
    {
        string name = (UniversalInputManager.UsePhoneUI == null) ? "FriendListFrame" : "FriendListFrame_phone";
        GameObject obj2 = AssetLoader.Get().LoadGameObject(name, true, false);
        if (obj2 == null)
        {
            return null;
        }
        obj2.transform.parent = base.transform;
        return obj2.GetComponent<FriendListFrame>();
    }

    private void DestroyFriendListFrame()
    {
        this.HideFriendsList();
        if (this.m_friendListFrame != null)
        {
            UnityEngine.Object.Destroy(this.m_friendListFrame.gameObject);
            this.m_friendListFrame = null;
        }
    }

    private void FireChatInfoChangedEvent(PlayerChatInfo chatInfo)
    {
        foreach (PlayerChatInfoChangedListener listener in this.m_playerChatInfoChangedListeners.ToArray())
        {
            listener.Fire(chatInfo);
        }
    }

    public static ChatMgr Get()
    {
        return s_instance;
    }

    private BnetPlayer GetMostRecentWhisperedPlayer()
    {
        return ((this.m_recentWhisperPlayers.Count <= 0) ? null : this.m_recentWhisperPlayers[0]);
    }

    public PlayerChatInfo GetPlayerChatInfo(BnetPlayer player)
    {
        PlayerChatInfo info = null;
        this.m_playerChatInfos.TryGetValue(player, out info);
        return info;
    }

    public List<BnetPlayer> GetRecentWhisperPlayers()
    {
        return this.m_recentWhisperPlayers;
    }

    public void GoBack()
    {
        if (this.IsFriendListShowing())
        {
            this.CloseChatUI();
        }
        else if (this.m_chatLogUI.IsShowing)
        {
            this.m_chatLogUI.Hide();
            this.ShowFriendsList();
        }
    }

    public void HandleGUIInput()
    {
        if (!FatalErrorMgr.Get().HasError() && !this.IsMobilePlatform())
        {
            this.HandleGUIInputForQuickChat();
        }
    }

    private void HandleGUIInputForQuickChat()
    {
        if (!this.m_chatLogUI.IsShowing)
        {
            if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return))
            {
                this.ShowChatForPlayer(this.GetMostRecentWhisperedPlayer());
            }
        }
        else if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.Escape))
        {
            this.m_chatLogUI.Hide();
        }
    }

    public bool HandleKeyboardInput()
    {
        if (!FatalErrorMgr.Get().HasError())
        {
            if (Input.GetKeyUp(KeyCode.Escape) && this.m_chatLogUI.IsShowing)
            {
                this.m_chatLogUI.Hide();
                return true;
            }
            if ((this.IsMobilePlatform() && this.m_chatLogUI.IsShowing) && Input.GetKeyUp(KeyCode.Escape))
            {
                this.m_chatLogUI.GoBack();
                return true;
            }
        }
        return false;
    }

    public void HideFriendsList()
    {
        if (this.IsFriendListShowing())
        {
            this.m_friendListFrame.gameObject.SetActive(false);
        }
        if (this.m_closeCatcher != null)
        {
            this.m_closeCatcher.gameObject.SetActive(false);
        }
    }

    private void InitChatLogUI()
    {
        if (this.IsMobilePlatform())
        {
            this.m_chatLogUI = new MobileChatLogUI();
        }
        else
        {
            this.m_chatLogUI = new DesktopChatLogUI();
        }
    }

    private void InitCloseCatcher()
    {
        this.m_closeCatcher = CameraUtils.CreateInputBlocker(BaseUI.Get().GetBnetCamera(), "CloseCatcher", this).AddComponent<PegUIElement>();
        this.m_closeCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCloseCatcherRelease));
        TransformUtil.SetPosZ(this.m_closeCatcher, base.transform.position.z + 100f);
        this.m_closeCatcher.gameObject.SetActive(false);
    }

    private ChatBubbleFrame InstantiateChatBubble(ChatBubbleFrame prefab, BnetWhisper whisper)
    {
        ChatBubbleFrame frame = UnityEngine.Object.Instantiate<ChatBubbleFrame>(prefab);
        frame.SetWhisper(whisper);
        frame.GetComponent<PegUIElement>().AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnChatBubbleReleased));
        return frame;
    }

    public bool IsChatLogFrameShown()
    {
        if (this.IsMobilePlatform())
        {
            return this.m_chatLogUI.IsShowing;
        }
        return this.m_chatLogFrameShown;
    }

    public bool IsFriendListShowing()
    {
        return ((this.m_friendListFrame != null) ? this.m_friendListFrame.gameObject.activeSelf : false);
    }

    public bool IsMobilePlatform()
    {
        return (UniversalInputManager.Get().IsTouchMode() && (PlatformSettings.OS != OSCategory.PC));
    }

    private void MoveChatBubbles(ChatBubbleFrame newBubbleFrame)
    {
        Anchor dstAnchor = Anchor.TOP_LEFT;
        Anchor srcAnchor = Anchor.BOTTOM_LEFT;
        if ((UniversalInputManager.UsePhoneUI != null) && (this.m_ChatBubbleInfo.m_Parent.transform.localPosition.y > -900f))
        {
            dstAnchor = Anchor.BOTTOM_LEFT;
            srcAnchor = Anchor.TOP_LEFT;
        }
        TransformUtil.SetPoint((Component) newBubbleFrame, srcAnchor, (Component) this.m_ChatBubbleInfo.m_Parent, dstAnchor, Vector3.zero);
        int count = this.m_chatBubbleFrames.Count;
        if (count != 1)
        {
            Vector3[] vectorArray = new Vector3[count - 1];
            Component dst = newBubbleFrame;
            for (int i = count - 2; i >= 0; i--)
            {
                ChatBubbleFrame src = this.m_chatBubbleFrames[i];
                vectorArray[i] = src.transform.position;
                TransformUtil.SetPoint(src, srcAnchor, dst, dstAnchor, Vector3.zero);
                dst = src;
            }
            for (int j = count - 2; j >= 0; j--)
            {
                ChatBubbleFrame frame2 = this.m_chatBubbleFrames[j];
                object[] args = new object[] { "islocal", true, "position", frame2.transform.localPosition, "time", this.m_ChatBubbleInfo.m_MoveOverSec, "easeType", this.m_ChatBubbleInfo.m_MoveOverEaseType };
                Hashtable hashtable = iTween.Hash(args);
                frame2.transform.position = vectorArray[j];
                iTween.Stop(frame2.gameObject, "move");
                iTween.MoveTo(frame2.gameObject, hashtable);
            }
        }
    }

    private void OnChatBubbleFadeOutComplete(ChatBubbleFrame bubbleFrame)
    {
        UnityEngine.Object.Destroy(bubbleFrame.gameObject);
        this.m_chatBubbleFrames.Remove(bubbleFrame);
    }

    private void OnChatBubbleReleased(UIEvent e)
    {
        BnetPlayer theirPlayer = e.GetElement().GetComponent<ChatBubbleFrame>().GetWhisper().GetTheirPlayer();
        this.ShowChatForPlayer(theirPlayer);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.RemoveAllChatBubbles();
        }
    }

    private void OnChatBubbleScaleInComplete(ChatBubbleFrame bubbleFrame)
    {
        object[] args = new object[] { "amount", 0f, "delay", this.m_ChatBubbleInfo.m_HoldSec, "time", this.m_ChatBubbleInfo.m_FadeOutSec, "easeType", this.m_ChatBubbleInfo.m_FadeOutEaseType, "oncomplete", "OnChatBubbleFadeOutComplete", "oncompleteparams", bubbleFrame, "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(bubbleFrame.gameObject, hashtable);
    }

    public void OnChatFramesMoved()
    {
        this.UpdateChatBubbleParentLayout();
    }

    public void OnChatLogFrameHidden()
    {
        this.m_chatLogFrameShown = false;
    }

    public void OnChatLogFrameShown()
    {
        this.m_chatLogFrameShown = true;
    }

    public void OnChatReceiverChanged(BnetPlayer player)
    {
        this.UpdatePlayerFocusTime(player);
    }

    private void OnCloseCatcherRelease(UIEvent e)
    {
        this.CloseChatUI();
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        W8Touch.Get().VirtualKeyboardDidShow -= new System.Action(this.OnKeyboardShow);
        W8Touch.Get().VirtualKeyboardDidHide -= new System.Action(this.OnKeyboardHide);
        s_instance = null;
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        this.CleanUp();
    }

    public void OnFriendListClosed()
    {
        if (W8Touch.Get().IsVirtualKeyboardVisible())
        {
            this.OnKeyboardShow();
        }
        else
        {
            this.UpdateChatBubbleParentLayout();
        }
    }

    public void OnFriendListFriendSelected(BnetPlayer friend)
    {
        this.ShowChatForPlayer(friend);
    }

    public void OnFriendListOpened()
    {
        if (W8Touch.Get().IsVirtualKeyboardVisible())
        {
            this.OnKeyboardShow();
        }
        else
        {
            this.UpdateChatBubbleParentLayout();
        }
    }

    private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
    {
        List<BnetPlayer> removedFriends = changelist.GetRemovedFriends();
        if (removedFriends != null)
        {
            <OnFriendsChanged>c__AnonStorey2AE storeyae = new <OnFriendsChanged>c__AnonStorey2AE();
            using (List<BnetPlayer>.Enumerator enumerator = removedFriends.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    storeyae.friend = enumerator.Current;
                    int index = this.m_recentWhisperPlayers.FindIndex(new Predicate<BnetPlayer>(storeyae.<>m__4B));
                    if (index >= 0)
                    {
                        this.m_recentWhisperPlayers.RemoveAt(index);
                    }
                }
            }
        }
    }

    public void OnKeyboardHide()
    {
        this.UpdateLayout();
        this.UpdateChatBubbleLayout();
    }

    public void OnKeyboardShow()
    {
        if (this.m_chatLogUI.IsShowing && (BaseUI.Get().m_Bones.m_QuickChatVirtualKeyboard.position != this.m_chatLogUI.GameObject.transform.position))
        {
            W8Touch.Get().VirtualKeyboardDidShow -= new System.Action(this.OnKeyboardShow);
            W8Touch.Get().VirtualKeyboardDidHide -= new System.Action(this.OnKeyboardHide);
            this.m_chatLogUI.Hide();
            this.m_chatLogUI.ShowForPlayer(this.GetMostRecentWhisperedPlayer());
            W8Touch.Get().VirtualKeyboardDidShow += new System.Action(this.OnKeyboardShow);
            W8Touch.Get().VirtualKeyboardDidHide += new System.Action(this.OnKeyboardHide);
        }
        Vector2 vector = new Vector2(0f, (float) (Screen.height - 150));
        GameObject gameObject = BnetBarFriendButton.Get().gameObject;
        TransformUtil.SetPoint(this.m_ChatBubbleInfo.m_Parent, Anchor.BOTTOM_LEFT, gameObject, Anchor.BOTTOM_RIGHT, (Vector3) vector);
        int count = this.m_chatBubbleFrames.Count;
        if (count != 0)
        {
            Component parent = this.m_ChatBubbleInfo.m_Parent;
            for (int i = count - 1; i >= 0; i--)
            {
                ChatBubbleFrame src = this.m_chatBubbleFrames[i];
                TransformUtil.SetPoint(src, Anchor.TOP_LEFT, parent, Anchor.BOTTOM_LEFT, Vector3.zero);
                parent = src;
            }
        }
    }

    private void OnWhisper(BnetWhisper whisper, object userData)
    {
        BnetPlayer theirPlayer = whisper.GetTheirPlayer();
        this.AddRecentWhisperPlayerToTop(theirPlayer);
        PlayerChatInfo chatInfo = this.RegisterPlayerChatInfo(whisper.GetTheirPlayer());
        try
        {
            if (this.m_chatLogUI.IsShowing && whisper.IsSpeakerOrReceiver(this.m_chatLogUI.Receiver))
            {
                if (this.IsMobilePlatform())
                {
                    chatInfo.SetLastSeenWhisper(whisper);
                }
            }
            else
            {
                this.PopupNewChatBubble(whisper);
            }
        }
        finally
        {
            this.FireChatInfoChangedEvent(chatInfo);
        }
    }

    private void PopupNewChatBubble(BnetWhisper whisper)
    {
        ChatBubbleFrame item = this.CreateChatBubble(whisper);
        this.m_chatBubbleFrames.Add(item);
        item.transform.parent = this.m_ChatBubbleInfo.m_Parent.transform;
        item.transform.localScale = (Vector3) item.m_ScaleOverride;
        SoundManager.Get().LoadAndPlay("receive_message");
        object[] args = new object[] { "scale", item.m_VisualRoot.transform.localScale, "time", this.m_ChatBubbleInfo.m_ScaleInSec, "easeType", this.m_ChatBubbleInfo.m_ScaleInEaseType, "oncomplete", "OnChatBubbleScaleInComplete", "oncompleteparams", item, "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        item.m_VisualRoot.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
        iTween.ScaleTo(item.m_VisualRoot, hashtable);
        this.MoveChatBubbles(item);
    }

    public PlayerChatInfo RegisterPlayerChatInfo(BnetPlayer player)
    {
        PlayerChatInfo info;
        if (!this.m_playerChatInfos.TryGetValue(player, out info))
        {
            info = new PlayerChatInfo();
            info.SetPlayer(player);
            this.m_playerChatInfos.Add(player, info);
        }
        return info;
    }

    private void RemoveAllChatBubbles()
    {
        foreach (ChatBubbleFrame frame in this.m_chatBubbleFrames)
        {
            UnityEngine.Object.Destroy(frame.gameObject);
        }
        this.m_chatBubbleFrames.Clear();
    }

    public bool RemovePlayerChatInfoChangedListener(PlayerChatInfoChangedCallback callback)
    {
        return this.RemovePlayerChatInfoChangedListener(callback, null);
    }

    public bool RemovePlayerChatInfoChangedListener(PlayerChatInfoChangedCallback callback, object userData)
    {
        PlayerChatInfoChangedListener item = new PlayerChatInfoChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_playerChatInfoChangedListeners.Remove(item);
    }

    private void ShowChatForPlayer(BnetPlayer player)
    {
        <ShowChatForPlayer>c__AnonStorey2AF storeyaf = new <ShowChatForPlayer>c__AnonStorey2AF {
            player = player
        };
        if (storeyaf.player != null)
        {
            this.AddRecentWhisperPlayerToTop(storeyaf.player);
            PlayerChatInfo chatInfo = this.RegisterPlayerChatInfo(storeyaf.player);
            List<BnetWhisper> whispersWithPlayer = BnetWhisperMgr.Get().GetWhispersWithPlayer(storeyaf.player);
            if (whispersWithPlayer != null)
            {
                chatInfo.SetLastSeenWhisper(Enumerable.LastOrDefault<BnetWhisper>(whispersWithPlayer, new Func<BnetWhisper, bool>(storeyaf.<>m__4C)));
                this.FireChatInfoChangedEvent(chatInfo);
            }
        }
        if (this.m_chatLogUI.IsShowing)
        {
            this.m_chatLogUI.Hide();
        }
        if (!this.m_chatLogUI.IsShowing)
        {
            if ((OptionsMenu.Get() != null) && OptionsMenu.Get().IsShown())
            {
                OptionsMenu.Get().Hide(true);
            }
            if ((GameMenu.Get() != null) && GameMenu.Get().IsShown())
            {
                GameMenu.Get().Hide();
            }
            this.m_chatLogUI.ShowForPlayer(this.GetMostRecentWhisperedPlayer());
            this.UpdateLayout();
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.HideFriendsList();
            }
        }
    }

    public void ShowFriendsList()
    {
        if (this.m_friendListFrame == null)
        {
            this.m_friendListFrame = this.CreateFriendsListUI();
        }
        this.m_friendListFrame.gameObject.SetActive(true);
        this.m_closeCatcher.gameObject.SetActive(true);
        this.UpdateLayout();
    }

    private void Start()
    {
        SoundManager.Get().Load("receive_message");
        this.UpdateLayout();
        if (W8Touch.Get().IsVirtualKeyboardVisible())
        {
            this.OnKeyboardShow();
        }
    }

    private void Update()
    {
        Rect keyboardArea = this.keyboardArea;
        this.keyboardArea = TextField.KeyboardArea;
        if (this.keyboardArea != keyboardArea)
        {
            this.UpdateLayout();
        }
    }

    private void UpdateChatBubbleLayout()
    {
        int count = this.m_chatBubbleFrames.Count;
        if (count != 0)
        {
            Component parent = this.m_ChatBubbleInfo.m_Parent;
            for (int i = count - 1; i >= 0; i--)
            {
                ChatBubbleFrame src = this.m_chatBubbleFrames[i];
                Anchor dstAnchor = (UniversalInputManager.UsePhoneUI == null) ? Anchor.TOP_LEFT : Anchor.BOTTOM_LEFT;
                Anchor srcAnchor = (UniversalInputManager.UsePhoneUI == null) ? Anchor.BOTTOM_LEFT : Anchor.TOP_LEFT;
                TransformUtil.SetPoint(src, srcAnchor, parent, dstAnchor, Vector3.zero);
                parent = src;
            }
        }
    }

    private void UpdateChatBubbleParentLayout()
    {
        if (BaseUI.Get().GetChatBubbleBone() != null)
        {
            this.m_ChatBubbleInfo.m_Parent.transform.position = BaseUI.Get().GetChatBubbleBone().transform.position;
        }
    }

    public void UpdateLayout()
    {
        if ((this.m_friendListFrame != null) || this.m_chatLogUI.IsShowing)
        {
            this.UpdateLayoutForOnScreenKeyboard();
        }
        this.UpdateChatBubbleParentLayout();
    }

    private void UpdateLayoutForOnScreenKeyboard()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.UpdateLayoutForOnScreenKeyboardOnPhone();
        }
        else
        {
            this.keyboardState = this.ComputeKeyboardState();
            bool flag = this.IsMobilePlatform();
            Camera bnetCamera = BaseUI.Get().GetBnetCamera();
            float num = bnetCamera.orthographicSize * 2f;
            float num2 = num * bnetCamera.aspect;
            float num3 = bnetCamera.transform.position.y + (num / 2f);
            float num4 = bnetCamera.transform.position.x - (num2 / 2f);
            float num5 = 0f;
            if ((this.keyboardState != KeyboardState.None) && flag)
            {
                num5 = (num * this.keyboardArea.height) / ((float) Screen.height);
            }
            float num6 = 0f;
            if (this.m_friendListFrame != null)
            {
                OrientedBounds bounds = TransformUtil.ComputeOrientedWorldBounds(BaseUI.Get().m_BnetBar.m_friendButton.gameObject, true);
                if (flag)
                {
                    float num7 = (this.keyboardState != KeyboardState.Below) ? (bounds.Extents[1].y * 2f) : num5;
                    this.m_friendListFrame.SetWorldHeight(num - num7);
                }
                OrientedBounds bounds2 = TransformUtil.ComputeOrientedWorldBounds(this.m_friendListFrame.gameObject, true);
                if (!flag || (this.keyboardState != KeyboardState.Below))
                {
                    float x = ((num4 + bounds2.Extents[0].x) + bounds2.CenterOffset.x) + this.m_friendsListXOffset;
                    float y = ((bounds.GetTrueCenterPosition().y + bounds.Extents[1].y) + bounds2.Extents[1].y) + bounds2.CenterOffset.y;
                    this.m_friendListFrame.SetWorldPosition(x, y);
                }
                else if (flag && (this.keyboardState == KeyboardState.Below))
                {
                    float num10 = ((num4 + bounds2.Extents[0].x) + bounds2.CenterOffset.x) + this.m_friendsListXOffset;
                    float num11 = (((bnetCamera.transform.position.y - (num / 2f)) + num5) + bounds2.Extents[1].y) + bounds2.CenterOffset.y;
                    this.m_friendListFrame.SetWorldPosition(num10, num11);
                }
                num6 = bounds2.Extents[0].magnitude * 2f;
            }
            if (this.m_chatLogUI.IsShowing)
            {
                ChatFrames component = this.m_chatLogUI.GameObject.GetComponent<ChatFrames>();
                if (component != null)
                {
                    float num12 = num3;
                    if (this.keyboardState == KeyboardState.Above)
                    {
                        num12 -= num5;
                    }
                    float height = num - num5;
                    float num14 = num4;
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        num14 += (num6 + this.m_friendsListXOffset) + this.m_chatLogXOffset;
                    }
                    float width = num2;
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        width -= (num6 + this.m_friendsListXOffset) + this.m_chatLogXOffset;
                    }
                    component.chatLogFrame.SetWorldRect(num14, num12, width, height);
                }
            }
            this.OnChatFramesMoved();
        }
    }

    private void UpdateLayoutForOnScreenKeyboardOnPhone()
    {
        this.keyboardState = this.ComputeKeyboardState();
        bool flag = UniversalInputManager.Get().IsTouchMode();
        Camera bnetCamera = BaseUI.Get().GetBnetCamera();
        float num = bnetCamera.orthographicSize * 2f;
        float num2 = num * bnetCamera.aspect;
        float num3 = bnetCamera.transform.position.y + (num / 2f);
        float num4 = bnetCamera.transform.position.x - (num2 / 2f);
        float num5 = 0f;
        float num6 = 0f;
        if ((this.keyboardState != KeyboardState.None) && flag)
        {
            num5 = (num * this.keyboardArea.height) / ((float) Screen.height);
            num6 = (num2 * this.keyboardArea.width) / ((float) Screen.width);
        }
        if (this.m_friendListFrame != null)
        {
            float x = num4 + this.m_friendsListXOffset;
            float y = num3 + this.m_friendsListYOffset;
            float width = (float) (this.m_friendsListWidth + this.m_friendsListWidthPadding);
            float height = num + this.m_friendsListHeightPadding;
            this.m_friendListFrame.SetWorldRect(x, y, width, height);
        }
        if (this.m_chatLogUI.IsShowing)
        {
            ChatFrames component = this.m_chatLogUI.GameObject.GetComponent<ChatFrames>();
            if (component != null)
            {
                float num11 = num3;
                if (this.keyboardState == KeyboardState.Above)
                {
                    num11 -= num5;
                }
                float num12 = num - num5;
                float num13 = num4;
                if (UniversalInputManager.UsePhoneUI == null)
                {
                    num13 += this.m_friendsListWidth;
                }
                float num14 = (num6 != 0f) ? num6 : num2;
                if (UniversalInputManager.UsePhoneUI == null)
                {
                    num14 -= this.m_friendsListWidth;
                }
                component.chatLogFrame.SetWorldRect(num13, num11, num14, num12);
            }
        }
        this.OnChatFramesMoved();
    }

    private void UpdatePlayerFocusTime(BnetPlayer player)
    {
        PlayerChatInfo chatInfo = this.RegisterPlayerChatInfo(player);
        chatInfo.SetLastFocusTime(UnityEngine.Time.realtimeSinceStartup);
        this.FireChatInfoChangedEvent(chatInfo);
    }

    private void WillReset()
    {
        this.CleanUp();
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
    }

    public FriendListFrame FriendListFrame
    {
        get
        {
            return this.m_friendListFrame;
        }
    }

    public Rect KeyboardRect
    {
        get
        {
            return this.keyboardArea;
        }
    }

    [CompilerGenerated]
    private sealed class <AddRecentWhisperPlayerToTop>c__AnonStorey2AD
    {
        internal BnetPlayer player;

        internal bool <>m__4A(BnetPlayer currPlayer)
        {
            return (currPlayer == this.player);
        }
    }

    [CompilerGenerated]
    private sealed class <OnFriendsChanged>c__AnonStorey2AE
    {
        internal BnetPlayer friend;

        internal bool <>m__4B(BnetPlayer player)
        {
            return (this.friend == player);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowChatForPlayer>c__AnonStorey2AF
    {
        internal BnetPlayer player;

        internal bool <>m__4C(BnetWhisper whisper)
        {
            return whisper.IsSpeaker(this.player);
        }
    }

    private enum KeyboardState
    {
        None,
        Below,
        Above
    }

    public delegate void PlayerChatInfoChangedCallback(PlayerChatInfo chatInfo, object userData);

    private class PlayerChatInfoChangedListener : EventListener<ChatMgr.PlayerChatInfoChangedCallback>
    {
        public void Fire(PlayerChatInfo chatInfo)
        {
            base.m_callback(chatInfo, base.m_userData);
        }
    }
}

