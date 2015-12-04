using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class FriendListFrame : MonoBehaviour
{
    [CompilerGenerated]
    private static Func<Bounds, Renderer, Bounds> <>f__am$cache24;
    [CompilerGenerated]
    private static TouchList.SelectedIndexChangingEvent <>f__am$cache25;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache26;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache27;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache28;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache29;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache2A;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache2B;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache2C;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache2D;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache2E;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache2F;
    [CompilerGenerated]
    private static Func<FriendListItem, bool> <>f__am$cache30;
    public FriendListButton addFriendButton;
    private static readonly PlatformDependentValue<bool> ALLOW_ITEM_SELECTION;
    public GameObject innerShadow;
    public TouchList items;
    public ListInfo listInfo;
    private AddFriendFrame m_addFriendFrame;
    private List<FriendListItem> m_allItems = new List<FriendListItem>();
    private bool m_editFriendsMode;
    private BnetPlayer m_friendToRemove;
    private Dictionary<MobileFriendListItem.TypeFlags, FriendListItemHeader> m_headers = new Dictionary<MobileFriendListItem.TypeFlags, FriendListItemHeader>();
    private Camera m_itemsCamera;
    private float m_lastNearbyPlayersUpdate;
    private VirtualizedFriendsListBehavior m_longListBehavior;
    private bool m_nearbyPlayersNeedUpdate;
    private List<NearbyPlayerUpdate> m_nearbyPlayerUpdates = new List<NearbyPlayerUpdate>();
    private BnetPlayerChangelist m_playersChangeList = new BnetPlayerChangelist();
    public FriendListButton m_RecruitAFriendButton;
    private RecruitAFriendFrame m_recruitAFriendFrame;
    private AlertPopup m_removeFriendPopup;
    public Me me;
    private PlayerPortrait myPortrait;
    private const float NEARBY_PLAYERS_UPDATE_TIME = 10f;
    public GameObject outerShadow;
    public GameObject portraitBackground;
    public Prefabs prefabs;
    public Material rankedBackground;
    public RecentOpponent recentOpponent;
    public FriendListButton removeFriendButton;
    public TouchListScrollbar scrollbar;
    public Material unrankedBackground;
    public NineSliceElement window;

    public event System.Action AddFriendFrameClosed;

    public event System.Action AddFriendFrameOpened;

    public event System.Action RecruitAFriendFrameClosed;

    public event System.Action RecruitAFriendFrameOpened;

    public event System.Action RemoveFriendPopupClosed;

    public event System.Action RemoveFriendPopupOpened;

    static FriendListFrame()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.Screen) {
            PC = true,
            Tablet = true,
            Phone = false
        };
        ALLOW_ITEM_SELECTION = value2;
    }

    [CompilerGenerated]
    private static <>__AnonType0<ITouchListItem, T> <GetItems`1>m__64<T>(ITouchListItem i) where T: MonoBehaviour
    {
        return new <>__AnonType0<ITouchListItem, T>(i, i.GetComponent<T>());
    }

    [CompilerGenerated]
    private static bool <GetItems`1>m__65<T>(<>__AnonType0<ITouchListItem, T> <>__TranspIdent0) where T: MonoBehaviour
    {
        return (<>__TranspIdent0.c != null);
    }

    [CompilerGenerated]
    private static T <GetItems`1>m__66<T>(<>__AnonType0<ITouchListItem, T> <>__TranspIdent0) where T: MonoBehaviour
    {
        return <>__TranspIdent0.c;
    }

    private void Awake()
    {
        this.myPortrait = this.me.portraitRef.Spawn<PlayerPortrait>();
        this.recentOpponent.button.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnRecentOpponentButtonReleased));
        this.InitButtons();
        this.RegisterFriendEvents();
        this.CreateItemsCamera();
        this.UpdateBackgroundCollider();
        bool flag = !(UniversalInputManager.Get().IsTouchMode() && (PlatformSettings.OS != OSCategory.PC));
        if (this.scrollbar != null)
        {
            this.scrollbar.gameObject.SetActive(flag);
        }
        this.me.m_Medal.m_legendIndex.RenderToTexture = false;
        this.me.m_Medal.m_legendIndex.TextColor = new Color(0.97f, 0.98f, 0.7f, 1f);
    }

    public void CloseAddFriendFrame()
    {
        if (this.m_addFriendFrame != null)
        {
            this.m_addFriendFrame.Close();
            if (this.AddFriendFrameClosed != null)
            {
                this.AddFriendFrameClosed();
            }
            this.m_addFriendFrame = null;
        }
    }

    private MobileFriendListItem CreateCurrentGameFrame(BnetPlayer friend)
    {
        FriendListCurrentGameFrame frame = UnityEngine.Object.Instantiate<FriendListCurrentGameFrame>(this.prefabs.currentGameItem);
        UberText[] objs = UberText.EnableAllTextInObject(frame.gameObject, false);
        frame.SetFriend(friend);
        frame.GetComponent<FriendListUIElement>().AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBaseFriendFrameReleased));
        MobileFriendListItem item = this.FinishCreateVisualItem<FriendListCurrentGameFrame>(frame, MobileFriendListItem.TypeFlags.CurrentGame, this.FindHeader(MobileFriendListItem.TypeFlags.CurrentGame), frame.gameObject);
        UberText.EnableAllTextObjects(objs, true);
        return item;
    }

    private MobileFriendListItem CreateFriendFrame(BnetPlayer friend)
    {
        FriendListFriendFrame frame = UnityEngine.Object.Instantiate<FriendListFriendFrame>(this.prefabs.friendItem);
        UberText[] objs = UberText.EnableAllTextInObject(frame.gameObject, false);
        frame.SetFriend(friend);
        frame.GetComponent<FriendListUIElement>().AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBaseFriendFrameReleased));
        MobileFriendListItem item = this.FinishCreateVisualItem<FriendListFriendFrame>(frame, MobileFriendListItem.TypeFlags.Friend, this.FindHeader(MobileFriendListItem.TypeFlags.Friend), frame.gameObject);
        UberText.EnableAllTextObjects(objs, true);
        return item;
    }

    private void CreateItemsCamera()
    {
        this.m_itemsCamera = new GameObject("ItemsCamera") { transform = { parent = this.items.transform, localPosition = new Vector3(0f, 0f, -100f) } }.AddComponent<Camera>();
        this.m_itemsCamera.orthographic = true;
        this.m_itemsCamera.depth = BnetBar.CameraDepth + 1;
        this.m_itemsCamera.clearFlags = CameraClearFlags.Depth;
        this.m_itemsCamera.cullingMask = GameLayer.BattleNetFriendList.LayerBit();
        this.UpdateItemsCamera();
    }

    private MobileFriendListItem CreateNearbyPlayerFrame(BnetPlayer friend)
    {
        FriendListNearbyPlayerFrame frame = UnityEngine.Object.Instantiate<FriendListNearbyPlayerFrame>(this.prefabs.nearbyPlayerItem);
        UberText[] objs = UberText.EnableAllTextInObject(frame.gameObject, false);
        frame.SetNearbyPlayer(friend);
        frame.GetComponent<FriendListUIElement>().AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnNearbyPlayerFrameReleased));
        MobileFriendListItem item = this.FinishCreateVisualItem<FriendListNearbyPlayerFrame>(frame, MobileFriendListItem.TypeFlags.NearbyPlayer, this.FindHeader(MobileFriendListItem.TypeFlags.NearbyPlayer), frame.gameObject);
        UberText.EnableAllTextObjects(objs, true);
        return item;
    }

    private MobileFriendListItem CreateRecruitFrame(Network.RecruitInfo info)
    {
        FriendListRecruitFrame frame = UnityEngine.Object.Instantiate<FriendListRecruitFrame>(this.prefabs.recruitItem);
        UberText[] objs = UberText.EnableAllTextInObject(frame.gameObject, false);
        frame.SetRecruitInfo(info);
        MobileFriendListItem item = this.FinishCreateVisualItem<FriendListRecruitFrame>(frame, MobileFriendListItem.TypeFlags.Recruit, this.FindHeader(MobileFriendListItem.TypeFlags.Recruit), frame.gameObject);
        UberText.EnableAllTextObjects(objs, true);
        return item;
    }

    private MobileFriendListItem CreateRequestFrame(BnetInvitation invite)
    {
        FriendListRequestFrame frame = UnityEngine.Object.Instantiate<FriendListRequestFrame>(this.prefabs.requestItem);
        UberText[] objs = UberText.EnableAllTextInObject(frame.gameObject, false);
        frame.SetInvite(invite);
        MobileFriendListItem item = this.FinishCreateVisualItem<FriendListRequestFrame>(frame, MobileFriendListItem.TypeFlags.Request, this.FindHeader(MobileFriendListItem.TypeFlags.Request), frame.gameObject);
        UberText.EnableAllTextObjects(objs, true);
        return item;
    }

    private void DoPlayersChanged(BnetPlayerChangelist changelist)
    {
        this.SuspendItemsLayout();
        BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
        bool flag = false;
        bool flag2 = false;
        foreach (BnetPlayerChange change in changelist.GetChanges())
        {
            BnetPlayer oldPlayer = change.GetOldPlayer();
            BnetPlayer newPlayer = change.GetNewPlayer();
            if (newPlayer == FriendMgr.Get().GetRecentOpponent())
            {
                this.UpdateRecentOpponent();
            }
            if (newPlayer == myPlayer)
            {
                this.UpdateMyself();
                BnetGameAccount hearthstoneGameAccount = newPlayer.GetHearthstoneGameAccount();
                if ((oldPlayer == null) || (oldPlayer.GetHearthstoneGameAccount() == null))
                {
                    flag = hearthstoneGameAccount.CanBeInvitedToGame();
                }
                else
                {
                    flag = oldPlayer.GetHearthstoneGameAccount().CanBeInvitedToGame() != hearthstoneGameAccount.CanBeInvitedToGame();
                }
            }
            else
            {
                if ((oldPlayer == null) || (oldPlayer.GetBestName() != newPlayer.GetBestName()))
                {
                    flag2 = true;
                }
                long persistentGameId = newPlayer.GetPersistentGameId();
                FriendListFriendFrame frame = this.FindFriendFrame(newPlayer);
                if (frame != null)
                {
                    if (persistentGameId != 0)
                    {
                        this.RemoveItem(false, MobileFriendListItem.TypeFlags.Friend, newPlayer);
                        FriendListItem item = new FriendListItem(false, MobileFriendListItem.TypeFlags.CurrentGame, newPlayer);
                        this.m_allItems.Add(item);
                        this.UpdateCurrentGamesHeader();
                        this.UpdateFriendsHeader(null);
                    }
                    else
                    {
                        frame.UpdateFriend();
                    }
                }
                else
                {
                    FriendListCurrentGameFrame frame2 = this.FindCurrentGameFrame(newPlayer);
                    if (frame2 != null)
                    {
                        if (persistentGameId == 0)
                        {
                            this.RemoveItem(false, MobileFriendListItem.TypeFlags.CurrentGame, null);
                            FriendListItem item2 = new FriendListItem(false, MobileFriendListItem.TypeFlags.Friend, newPlayer);
                            this.m_allItems.Add(item2);
                            this.UpdateCurrentGamesHeader();
                            this.UpdateFriendsHeader(null);
                        }
                        else
                        {
                            frame2.UpdateFriend();
                        }
                    }
                }
            }
        }
        if (flag)
        {
            this.UpdateItems();
        }
        else if (flag2)
        {
            this.UpdateFriendItems();
        }
        this.UpdateAllHeaders();
        this.UpdateAllHeaderBackgrounds();
        this.ResumeItemsLayout();
    }

    private FriendListBaseFriendFrame FindBaseFriendFrame(BnetPlayer friend)
    {
        <FindBaseFriendFrame>c__AnonStorey2B2 storeyb = new <FindBaseFriendFrame>c__AnonStorey2B2 {
            friend = friend
        };
        return this.FindFirstItem<FriendListBaseFriendFrame>(new Predicate<FriendListBaseFriendFrame>(storeyb.<>m__52));
    }

    private FriendListCurrentGameFrame FindCurrentGameFrame(BnetPlayer friend)
    {
        <FindCurrentGameFrame>c__AnonStorey2B3 storeyb = new <FindCurrentGameFrame>c__AnonStorey2B3 {
            friend = friend
        };
        return this.FindFirstItem<FriendListCurrentGameFrame>(new Predicate<FriendListCurrentGameFrame>(storeyb.<>m__53));
    }

    private T FindFirstItem<T>(Predicate<T> predicate) where T: MonoBehaviour
    {
        <FindFirstItem>c__AnonStorey2B8<T> storeyb = new <FindFirstItem>c__AnonStorey2B8<T> {
            predicate = predicate
        };
        ITouchListItem item = Enumerable.FirstOrDefault<ITouchListItem>(this.items, new Func<ITouchListItem, bool>(storeyb.<>m__63));
        return ((item == null) ? null : item.GetComponent<T>());
    }

    private FriendListFriendFrame FindFriendFrame(BnetAccountId id)
    {
        <FindFriendFrame>c__AnonStorey2B5 storeyb = new <FindFriendFrame>c__AnonStorey2B5 {
            id = id
        };
        return this.FindFirstItem<FriendListFriendFrame>(new Predicate<FriendListFriendFrame>(storeyb.<>m__55));
    }

    private FriendListFriendFrame FindFriendFrame(BnetPlayer friend)
    {
        <FindFriendFrame>c__AnonStorey2B4 storeyb = new <FindFriendFrame>c__AnonStorey2B4 {
            friend = friend
        };
        return this.FindFirstItem<FriendListFriendFrame>(new Predicate<FriendListFriendFrame>(storeyb.<>m__54));
    }

    private FriendListItemHeader FindHeader(MobileFriendListItem.TypeFlags type)
    {
        FriendListItemHeader header;
        type |= MobileFriendListItem.TypeFlags.Header;
        this.m_headers.TryGetValue(type, out header);
        return header;
    }

    private FriendListItemHeader FindOrAddHeader(MobileFriendListItem.TypeFlags type)
    {
        type |= MobileFriendListItem.TypeFlags.Header;
        FriendListItemHeader userdata = this.FindHeader(type);
        if (userdata == null)
        {
            FriendListItem item = new FriendListItem(true, type, null);
            userdata = UnityEngine.Object.Instantiate<FriendListItemHeader>(this.prefabs.headerItem);
            this.m_headers[type] = userdata;
            Option setoption = Option.FRIENDS_LIST_FRIEND_SECTION_HIDE;
            MobileFriendListItem.TypeFlags subType = item.SubType;
            switch (subType)
            {
                case MobileFriendListItem.TypeFlags.Recruit:
                    setoption = Option.FRIENDS_LIST_RECRUIT_SECTION_HIDE;
                    break;

                case MobileFriendListItem.TypeFlags.Friend:
                    setoption = Option.FRIENDS_LIST_FRIEND_SECTION_HIDE;
                    break;

                case MobileFriendListItem.TypeFlags.CurrentGame:
                    setoption = Option.FRIENDS_LIST_CURRENTGAME_SECTION_HIDE;
                    break;

                case MobileFriendListItem.TypeFlags.NearbyPlayer:
                    setoption = Option.FRIENDS_LIST_NEARBYPLAYER_SECTION_HIDE;
                    break;

                default:
                    if (subType == MobileFriendListItem.TypeFlags.Request)
                    {
                        setoption = Option.FRIENDS_LIST_REQUEST_SECTION_HIDE;
                    }
                    break;
            }
            userdata.SubType = item.SubType;
            userdata.Option = setoption;
            bool showHeaderSection = this.GetShowHeaderSection(setoption);
            userdata.SetInitialShowContents(showHeaderSection);
            userdata.ClearToggleListeners();
            userdata.AddToggleListener(new FriendListItemHeader.ToggleContentsFunc(this.OnHeaderSectionToggle), userdata);
            UberText[] objs = UberText.EnableAllTextInObject(userdata.gameObject, false);
            this.FinishCreateVisualItem<FriendListItemHeader>(userdata, type, null, null);
            UberText.EnableAllTextObjects(objs, true);
        }
        return userdata;
    }

    private MobileFriendListItem FinishCreateVisualItem<T>(T obj, MobileFriendListItem.TypeFlags type, ITouchListItem parent, GameObject showObj) where T: MonoBehaviour
    {
        MobileFriendListItem component = obj.gameObject.GetComponent<MobileFriendListItem>();
        if (component == null)
        {
            component = obj.gameObject.AddComponent<MobileFriendListItem>();
            BoxCollider collider = component.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.size = new Vector3(collider.size.x, collider.size.y + this.items.elementSpacing, collider.size.z);
            }
        }
        component.Type = type;
        component.SetShowObject(showObj);
        component.SetParent(parent);
        if (component.Selectable)
        {
            BnetPlayer selectedFriend = FriendMgr.Get().GetSelectedFriend();
            if (selectedFriend == null)
            {
                return component;
            }
            BnetPlayer friend = null;
            if (obj is FriendListFriendFrame)
            {
                friend = ((FriendListFriendFrame) obj).GetFriend();
            }
            else if (obj is FriendListNearbyPlayerFrame)
            {
                friend = ((FriendListNearbyPlayerFrame) obj).GetNearbyPlayer();
            }
            if ((friend != null) && (selectedFriend == friend))
            {
                component.Selected();
            }
        }
        return component;
    }

    private Transform GetBottomRightBone()
    {
        return (((this.scrollbar == null) || !this.scrollbar.gameObject.activeSelf) ? this.listInfo.bottomRight : this.listInfo.bottomRightWithScrollbar);
    }

    private IEnumerable<T> GetItems<T>() where T: MonoBehaviour
    {
        return Enumerable.Select<<>__AnonType0<ITouchListItem, T>, T>(Enumerable.Where<<>__AnonType0<ITouchListItem, T>>(Enumerable.Select<ITouchListItem, <>__AnonType0<ITouchListItem, T>>(this.items, new Func<ITouchListItem, <>__AnonType0<ITouchListItem, T>>(FriendListFrame.<GetItems`1>m__64<T>)), new Func<<>__AnonType0<ITouchListItem, T>, bool>(FriendListFrame.<GetItems`1>m__65<T>)), new Func<<>__AnonType0<ITouchListItem, T>, T>(FriendListFrame.<GetItems`1>m__66<T>));
    }

    private bool GetShowHeaderSection(Option setoption)
    {
        return !((bool) Options.Get().GetOption(setoption, false));
    }

    private bool HandleKeyboardInput()
    {
        return (FatalErrorMgr.Get().HasError() && false);
    }

    private void HandleNearbyPlayersChanged()
    {
        if (this.m_nearbyPlayersNeedUpdate)
        {
            this.UpdateNearbyPlayerItems();
            if (this.m_nearbyPlayerUpdates.Count > 0)
            {
                this.SuspendItemsLayout();
                foreach (NearbyPlayerUpdate update in this.m_nearbyPlayerUpdates)
                {
                    if (update.Change == NearbyPlayerUpdate.ChangeType.Added)
                    {
                        FriendListItem item = new FriendListItem(false, MobileFriendListItem.TypeFlags.NearbyPlayer, update.Player);
                        this.m_allItems.Add(item);
                    }
                    else
                    {
                        this.RemoveItem(false, MobileFriendListItem.TypeFlags.NearbyPlayer, update.Player);
                    }
                }
                this.m_nearbyPlayerUpdates.Clear();
                this.UpdateAllHeaders();
                this.ResumeItemsLayout();
                this.UpdateAllHeaderBackgrounds();
                this.UpdateSelectedItem();
            }
            this.m_nearbyPlayersNeedUpdate = false;
            this.m_lastNearbyPlayersUpdate = UnityEngine.Time.realtimeSinceStartup;
        }
    }

    private void InitButtons()
    {
        this.addFriendButton.SetText(GameStrings.Get("GLOBAL_FRIENDLIST_ADD_FRIEND_BUTTON"));
        this.addFriendButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnAddFriendButtonReleased));
        this.removeFriendButton.SetText(GameStrings.Get("GLOBAL_FRIENDLIST_REMOVE_FRIEND_BUTTON"));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.removeFriendButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnEditFriendsButtonReleased));
        }
        else
        {
            this.removeFriendButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnRemoveFriendButtonReleased));
        }
    }

    private void InitItems()
    {
        BnetFriendMgr mgr = BnetFriendMgr.Get();
        BnetNearbyPlayerMgr mgr2 = BnetNearbyPlayerMgr.Get();
        this.items.SelectionEnabled = true;
        if (<>f__am$cache25 == null)
        {
            <>f__am$cache25 = index => index != -1;
        }
        this.items.SelectedIndexChanging += <>f__am$cache25;
        this.SuspendItemsLayout();
        this.UpdateRequests(mgr.GetReceivedInvites(), null);
        this.UpdateAllFriends(mgr.GetFriends(), null);
        this.UpdateAllNearbyPlayers(mgr2.GetNearbyStrangers(), null);
        this.UpdateAllNearbyFriends(mgr2.GetNearbyFriends(), null);
        this.UpdateAllRecruits();
        this.UpdateAllHeaders();
        this.ResumeItemsLayout();
        this.UpdateAllHeaderBackgrounds();
        this.UpdateSelectedItem();
    }

    private int ItemsSortCompare(FriendListItem item1, FriendListItem item2)
    {
        int num = item2.ItemFlags.CompareTo(item1.ItemFlags);
        if (num != 0)
        {
            return num;
        }
        MobileFriendListItem.TypeFlags itemFlags = item1.ItemFlags;
        switch (itemFlags)
        {
            case MobileFriendListItem.TypeFlags.Friend:
                return FriendUtils.FriendSortCompare(item1.GetFriend(), item2.GetFriend());

            case MobileFriendListItem.TypeFlags.NearbyPlayer:
                return FriendUtils.FriendSortCompare(item1.GetNearbyPlayer(), item2.GetNearbyPlayer());

            case MobileFriendListItem.TypeFlags.Recruit:
                return item1.GetRecruit().ID.CompareTo(item2.GetRecruit().ID);

            case MobileFriendListItem.TypeFlags.CurrentGame:
                return FriendUtils.FriendSortCompare(item1.GetCurrentGame(), item2.GetCurrentGame());
        }
        if (itemFlags != MobileFriendListItem.TypeFlags.Request)
        {
            return 0;
        }
        BnetInvitation invite = item1.GetInvite();
        BnetInvitation invitation2 = item2.GetInvite();
        int num2 = string.Compare(invite.GetInviterName(), invitation2.GetInviterName(), true);
        if (num2 != 0)
        {
            return num2;
        }
        long lo = (long) invite.GetInviterId().GetLo();
        long num4 = (long) invitation2.GetInviterId().GetLo();
        return (int) (lo - num4);
    }

    private void OnAddFriendButtonReleased(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Small_Click");
        if (this.m_addFriendFrame != null)
        {
            this.CloseAddFriendFrame();
        }
        else
        {
            if (this.AddFriendFrameOpened != null)
            {
                this.AddFriendFrameOpened();
            }
            BnetPlayer selectedFriend = FriendMgr.Get().GetSelectedFriend();
            this.ShowAddFriendFrame(selectedFriend);
        }
    }

    private void OnBaseFriendFrameReleased(UIEvent e)
    {
        if (!this.IsInEditMode)
        {
            FriendListUIElement element = (FriendListUIElement) e.GetElement();
            BnetPlayer friend = element.GetComponent<FriendListBaseFriendFrame>().GetFriend();
            FriendMgr.Get().SetSelectedFriend(friend);
            if (ALLOW_ITEM_SELECTION != null)
            {
                this.SelectedPlayer = friend;
            }
            ChatMgr.Get().OnFriendListFriendSelected(friend);
        }
    }

    private void OnDestroy()
    {
        this.UnregisterFriendEvents();
        this.CloseAddFriendFrame();
        if ((this.m_longListBehavior != null) && (this.m_longListBehavior.FreeList != null))
        {
            foreach (MobileFriendListItem item in this.m_longListBehavior.FreeList)
            {
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
        }
        foreach (FriendListItemHeader header in this.m_headers.Values)
        {
            if (header != null)
            {
                UnityEngine.Object.Destroy(header.gameObject);
            }
        }
    }

    private void OnEditFriendsButtonReleased(UIEvent e)
    {
        this.ToggleEditFriendsMode();
    }

    private void OnEnable()
    {
        if (this.m_nearbyPlayersNeedUpdate)
        {
            this.HandleNearbyPlayersChanged();
        }
        if (this.m_playersChangeList.GetChanges().Count > 0)
        {
            this.DoPlayersChanged(this.m_playersChangeList);
            this.m_playersChangeList.GetChanges().Clear();
        }
        if (this.items.IsInitialized)
        {
            this.ResumeItemsLayout();
        }
        this.UpdateMyself();
        this.items.ResetState();
        this.m_editFriendsMode = false;
        this.m_friendToRemove = null;
    }

    private void OnFriendChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, object userData)
    {
        if (player == BnetPresenceMgr.Get().GetMyPlayer())
        {
            this.UpdateFriendItems();
        }
        else
        {
            FriendListBaseFriendFrame frame = this.FindBaseFriendFrame(player);
            if (frame != null)
            {
                frame.UpdateFriend();
            }
        }
    }

    private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
    {
        this.SuspendItemsLayout();
        this.UpdateRequests(changelist.GetAddedReceivedInvites(), changelist.GetRemovedReceivedInvites());
        this.UpdateAllFriends(changelist.GetAddedFriends(), changelist.GetRemovedFriends());
        this.UpdateAllHeaders();
        this.ResumeItemsLayout();
        this.UpdateAllHeaderBackgrounds();
        this.UpdateSelectedItem();
    }

    private void OnHeaderSectionToggle(bool show, object userdata)
    {
        <OnHeaderSectionToggle>c__AnonStorey2B7 storeyb = new <OnHeaderSectionToggle>c__AnonStorey2B7 {
            header = (FriendListItemHeader) userdata
        };
        this.SetShowHeaderSection(storeyb.header.Option, show);
        int startingLongListIndex = this.m_allItems.FindIndex(new Predicate<FriendListItem>(storeyb.<>m__62));
        this.items.RefreshList(startingLongListIndex, true);
        this.UpdateHeaderBackground(storeyb.header);
    }

    private void OnNearbyPlayerFrameReleased(UIEvent e)
    {
        FriendListUIElement element = (FriendListUIElement) e.GetElement();
        BnetPlayer nearbyPlayer = element.GetComponent<FriendListNearbyPlayerFrame>().GetNearbyPlayer();
        FriendMgr.Get().SetSelectedFriend(nearbyPlayer);
        if (ALLOW_ITEM_SELECTION != null)
        {
            this.SelectedPlayer = nearbyPlayer;
        }
    }

    private void OnNearbyPlayersChanged(BnetNearbyPlayerChangelist changelist, object userData)
    {
        this.m_nearbyPlayersNeedUpdate = true;
        if (changelist.GetAddedStrangers() != null)
        {
            foreach (BnetPlayer player in changelist.GetAddedStrangers())
            {
                this.m_nearbyPlayerUpdates.Add(new NearbyPlayerUpdate(NearbyPlayerUpdate.ChangeType.Added, player));
            }
        }
        if (changelist.GetRemovedStrangers() != null)
        {
            foreach (BnetPlayer player2 in changelist.GetRemovedStrangers())
            {
                this.m_nearbyPlayerUpdates.Add(new NearbyPlayerUpdate(NearbyPlayerUpdate.ChangeType.Removed, player2));
            }
        }
        if (changelist.GetAddedFriends() != null)
        {
            foreach (BnetPlayer player3 in changelist.GetAddedFriends())
            {
                this.m_nearbyPlayerUpdates.Add(new NearbyPlayerUpdate(NearbyPlayerUpdate.ChangeType.Added, player3));
            }
        }
        if (changelist.GetRemovedFriends() != null)
        {
            foreach (BnetPlayer player4 in changelist.GetRemovedFriends())
            {
                this.m_nearbyPlayerUpdates.Add(new NearbyPlayerUpdate(NearbyPlayerUpdate.ChangeType.Removed, player4));
            }
        }
        if (base.gameObject.activeInHierarchy && (UnityEngine.Time.realtimeSinceStartup >= (this.m_lastNearbyPlayersUpdate + 10f)))
        {
            this.HandleNearbyPlayersChanged();
        }
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        if (base.gameObject.activeInHierarchy)
        {
            this.DoPlayersChanged(changelist);
        }
        else
        {
            List<BnetPlayerChange> changes = changelist.GetChanges();
            this.m_playersChangeList.GetChanges().AddRange(changes);
        }
    }

    private void OnRecentOpponent(BnetPlayer recentOpponent, object userData)
    {
        this.UpdateRecentOpponent();
    }

    private void OnRecentOpponentButtonReleased(UIEvent e)
    {
        if (!string.IsNullOrEmpty(this.recentOpponent.nameText.Text))
        {
            BnetPlayer recentOpponent = FriendMgr.Get().GetRecentOpponent();
            this.ShowAddFriendFrame(recentOpponent);
        }
    }

    private void OnRecruitAccepted(Network.RecruitInfo recruit)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
        string nickname = recruit.Nickname;
        FriendListFriendFrame frame = this.FindFriendFrame(recruit.RecruitID);
        if (frame != null)
        {
            nickname = frame.GetFriend().GetBestName();
        }
        info.m_headerText = GameStrings.Format("GLOBAL_FRIENDLIST_RECRUIT_ACCEPTED_ALERT_HEADER", new object[0]);
        object[] args = new object[] { nickname };
        info.m_text = GameStrings.Format("GLOBAL_FRIENDLIST_RECRUIT_ACCEPTED_ALERT_MESSAGE", args);
        info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
        DialogManager.Get().ShowPopup(info);
    }

    private void OnRecruitAFriendButtonReleased(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Small_Click");
        if (this.m_recruitAFriendFrame != null)
        {
            this.RecruitAFriend_OnClosed();
        }
        else
        {
            if (this.RecruitAFriendFrameOpened != null)
            {
                this.RecruitAFriendFrameOpened();
            }
            this.ShowRecruitAFriendFrame();
        }
    }

    private void OnRecruitsChanged()
    {
        this.UpdateAllRecruits();
        this.UpdateRecruitsHeader();
    }

    private void OnRemoveFriendButtonReleased(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Small_Click");
        if (this.items.SelectedItem != null)
        {
            BnetPlayer selectedFriend = FriendMgr.Get().GetSelectedFriend();
            this.ShowRemoveFriendPopup(selectedFriend);
        }
    }

    private bool OnRemoveFriendDialogShown(DialogBase dialog, object userData)
    {
        BnetPlayer player = (BnetPlayer) userData;
        if (!BnetFriendMgr.Get().IsFriend(player))
        {
            return false;
        }
        this.m_removeFriendPopup = (AlertPopup) dialog;
        return true;
    }

    private void OnRemoveFriendPopupResponse(AlertPopup.Response response, object userData)
    {
        if ((response == AlertPopup.Response.CONFIRM) && (this.m_friendToRemove != null))
        {
            BnetFriendMgr.Get().RemoveFriend(this.m_friendToRemove);
        }
        this.m_friendToRemove = null;
        this.m_removeFriendPopup = null;
        if (this.RemoveFriendPopupClosed != null)
        {
            this.RemoveFriendPopupClosed();
        }
    }

    private void OnScenePreUnload(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        switch (SceneMgr.Get().GetMode())
        {
            case SceneMgr.Mode.FRIENDLY:
            case SceneMgr.Mode.FATAL_ERROR:
                if (ChatMgr.Get() != null)
                {
                    ChatMgr.Get().HideFriendsList();
                }
                else
                {
                    UnityEngine.Object.Destroy(base.gameObject);
                }
                break;
        }
    }

    private void RecruitAFriend_OnClosed()
    {
        if (this.m_recruitAFriendFrame != null)
        {
            this.m_recruitAFriendFrame.Close();
            if (this.RecruitAFriendFrameClosed != null)
            {
                this.RecruitAFriendFrameClosed();
            }
            this.m_recruitAFriendFrame = null;
        }
    }

    private void RegisterFriendEvents()
    {
        BnetFriendMgr.Get().AddChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        FriendChallengeMgr.Get().AddChangedListener(new FriendChallengeMgr.ChangedCallback(this.OnFriendChallengeChanged));
        BnetNearbyPlayerMgr.Get().AddChangeListener(new BnetNearbyPlayerMgr.ChangeCallback(this.OnNearbyPlayersChanged));
        RecruitListMgr.Get().AddRecruitsChangedListener(new RecruitListMgr.RecruitsChangedCallback(this.OnRecruitsChanged));
        RecruitListMgr.Get().AddRecruitAcceptedListener(new RecruitListMgr.RecruitAcceptedCallback(this.OnRecruitAccepted));
        FriendMgr.Get().AddRecentOpponentListener(new FriendMgr.RecentOpponentCallback(this.OnRecentOpponent));
        SceneMgr.Get().RegisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
        SpectatorManager.Get().OnInviteReceived += new SpectatorManager.InviteReceivedHandler(this.SpectatorManager_OnInviteReceivedOrSent);
        SpectatorManager.Get().OnInviteSent += new SpectatorManager.InviteSentHandler(this.SpectatorManager_OnInviteReceivedOrSent);
    }

    private bool RemoveItem(bool isHeader, MobileFriendListItem.TypeFlags type, object itemToRemove)
    {
        <RemoveItem>c__AnonStorey2B9 storeyb = new <RemoveItem>c__AnonStorey2B9 {
            isHeader = isHeader,
            type = type,
            itemToRemove = itemToRemove
        };
        int index = this.m_allItems.FindIndex(new Predicate<FriendListItem>(storeyb.<>m__67));
        if (index < 0)
        {
            return false;
        }
        this.m_allItems.RemoveAt(index);
        return true;
    }

    private void ResumeItemsLayout()
    {
        this.items.ResumeLayout(false);
        this.SortAndRefreshTouchList();
    }

    private void SetShowHeaderSection(Option sectionoption, bool show)
    {
        if (this.GetShowHeaderSection(sectionoption) != show)
        {
            Options.Get().SetOption(sectionoption, !show);
        }
    }

    public void SetWorldHeight(float height)
    {
        bool activeSelf = base.gameObject.activeSelf;
        base.gameObject.SetActive(true);
        this.window.SetEntireHeight(height);
        this.UpdateItemsList();
        this.UpdateItemsCamera();
        this.UpdateBackgroundCollider();
        this.UpdateDropShadow();
        base.gameObject.SetActive(activeSelf);
    }

    public void SetWorldPosition(Vector3 pos)
    {
        bool activeSelf = base.gameObject.activeSelf;
        base.gameObject.SetActive(true);
        base.transform.position = pos;
        this.UpdateItemsList();
        this.UpdateItemsCamera();
        this.UpdateBackgroundCollider();
        base.gameObject.SetActive(activeSelf);
    }

    public void SetWorldPosition(float x, float y)
    {
        this.SetWorldPosition(new Vector3(x, y));
    }

    public void SetWorldRect(float x, float y, float width, float height)
    {
        bool activeSelf = base.gameObject.activeSelf;
        base.gameObject.SetActive(true);
        this.window.SetEntireSize(width, height);
        Vector3 vector = TransformUtil.ComputeWorldPoint(TransformUtil.ComputeSetPointBounds(this.window), new Vector3(0f, 1f, 0f));
        Vector3 translation = new Vector3(x, y, vector.z) - vector;
        base.transform.Translate(translation);
        this.UpdateItemsList();
        this.UpdateItemsCamera();
        this.UpdateBackgroundCollider();
        this.UpdateDropShadow();
        base.gameObject.SetActive(activeSelf);
    }

    public void ShowAddFriendFrame(BnetPlayer player = null)
    {
        this.m_addFriendFrame = UnityEngine.Object.Instantiate<AddFriendFrame>(this.prefabs.addFriendFrame);
        this.m_addFriendFrame.Closed += new System.Action(this.CloseAddFriendFrame);
        this.RecruitAFriend_OnClosed();
        if (player != null)
        {
            this.m_addFriendFrame.SetPlayer(player);
        }
    }

    public void ShowRecruitAFriendFrame()
    {
        this.m_recruitAFriendFrame = UnityEngine.Object.Instantiate<RecruitAFriendFrame>(this.prefabs.recruitAFriendFrame);
        this.m_recruitAFriendFrame.Closed += new System.Action(this.RecruitAFriend_OnClosed);
        this.CloseAddFriendFrame();
    }

    public void ShowRemoveFriendPopup(BnetPlayer friend)
    {
        this.m_friendToRemove = friend;
        if (this.m_friendToRemove != null)
        {
            string uniqueName = FriendUtils.GetUniqueName(this.m_friendToRemove);
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
            object[] args = new object[] { uniqueName };
            info.m_text = GameStrings.Format("GLOBAL_FRIENDLIST_REMOVE_FRIEND_ALERT_MESSAGE", args);
            info.m_showAlertIcon = true;
            info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
            info.m_responseCallback = new AlertPopup.ResponseCallback(this.OnRemoveFriendPopupResponse);
            DialogManager.Get().ShowPopup(info, new DialogManager.DialogProcessCallback(this.OnRemoveFriendDialogShown), this.m_friendToRemove);
            if (this.RemoveFriendPopupOpened != null)
            {
                this.RemoveFriendPopupOpened();
            }
        }
    }

    private void SortAndRefreshTouchList()
    {
        if (!this.items.IsLayoutSuspended)
        {
            this.m_allItems.Sort(new Comparison<FriendListItem>(this.ItemsSortCompare));
            if (this.m_longListBehavior == null)
            {
                this.m_longListBehavior = new VirtualizedFriendsListBehavior(this);
                this.items.LongListBehavior = this.m_longListBehavior;
            }
            else
            {
                this.items.RefreshList(0, true);
            }
        }
    }

    private void SpectatorManager_OnInviteReceivedOrSent(OnlineEventType evt, BnetPlayer inviter)
    {
        FriendListFriendFrame frame = this.FindFriendFrame(inviter);
        if (frame != null)
        {
            frame.UpdateFriend();
        }
    }

    private void Start()
    {
        this.UpdateMyself();
        this.UpdateRecentOpponent();
        this.InitItems();
    }

    private void SuspendItemsLayout()
    {
        this.items.SuspendLayout();
    }

    private void ToggleEditFriendsMode()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_editFriendsMode = !this.m_editFriendsMode;
            this.UpdateFriendItems();
        }
    }

    private void UnregisterFriendEvents()
    {
        BnetFriendMgr.Get().RemoveChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        FriendChallengeMgr.Get().RemoveChangedListener(new FriendChallengeMgr.ChangedCallback(this.OnFriendChallengeChanged));
        BnetNearbyPlayerMgr.Get().RemoveChangeListener(new BnetNearbyPlayerMgr.ChangeCallback(this.OnNearbyPlayersChanged));
        RecruitListMgr.Get().RemoveRecruitsChangedListener(new RecruitListMgr.RecruitsChangedCallback(this.OnRecruitsChanged));
        FriendMgr.Get().RemoveRecentOpponentListener(new FriendMgr.RecentOpponentCallback(this.OnRecentOpponent));
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().UnregisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
        }
        SpectatorManager.Get().OnInviteReceived -= new SpectatorManager.InviteReceivedHandler(this.SpectatorManager_OnInviteReceivedOrSent);
        SpectatorManager.Get().OnInviteSent -= new SpectatorManager.InviteSentHandler(this.SpectatorManager_OnInviteReceivedOrSent);
    }

    private void Update()
    {
        this.HandleKeyboardInput();
        if (this.m_nearbyPlayersNeedUpdate && (UnityEngine.Time.realtimeSinceStartup >= (this.m_lastNearbyPlayersUpdate + 10f)))
        {
            this.HandleNearbyPlayersChanged();
        }
        this.UpdateButtonGlows();
    }

    private void UpdateAllFriends(List<BnetPlayer> addedList, List<BnetPlayer> removedList)
    {
        if ((removedList != null) || (addedList != null))
        {
            if (removedList != null)
            {
                foreach (BnetPlayer player in removedList)
                {
                    if (!this.RemoveItem(false, MobileFriendListItem.TypeFlags.Friend, player) && this.RemoveItem(false, MobileFriendListItem.TypeFlags.CurrentGame, player))
                    {
                    }
                }
            }
            this.UpdateFriendItems();
            if (addedList != null)
            {
                foreach (BnetPlayer player2 in addedList)
                {
                    FriendListItem item;
                    if (player2.GetPersistentGameId() == 0)
                    {
                        item = new FriendListItem(false, MobileFriendListItem.TypeFlags.Friend, player2);
                    }
                    else
                    {
                        item = new FriendListItem(false, MobileFriendListItem.TypeFlags.CurrentGame, player2);
                    }
                    this.m_allItems.Add(item);
                }
            }
            this.SortAndRefreshTouchList();
        }
    }

    private void UpdateAllHeaderBackgrounds()
    {
        this.UpdateHeaderBackground(this.FindHeader(MobileFriendListItem.TypeFlags.Request));
        this.UpdateHeaderBackground(this.FindHeader(MobileFriendListItem.TypeFlags.CurrentGame));
    }

    private void UpdateAllHeaders()
    {
        this.UpdateRequestsHeader(null);
        this.UpdateCurrentGamesHeader();
        this.UpdateNearbyPlayersHeader(null);
        this.UpdateFriendsHeader(null);
        this.UpdateRecruitsHeader();
    }

    private void UpdateAllNearbyFriends(List<BnetPlayer> addedList, List<BnetPlayer> removedList)
    {
        if (removedList != null)
        {
            foreach (BnetPlayer player in removedList)
            {
                this.RemoveItem(false, MobileFriendListItem.TypeFlags.NearbyPlayer, player);
            }
        }
        this.UpdateNearbyPlayerItems();
        if (addedList != null)
        {
            foreach (BnetPlayer player2 in addedList)
            {
                FriendListItem item = new FriendListItem(false, MobileFriendListItem.TypeFlags.NearbyPlayer, player2);
                this.m_allItems.Add(item);
            }
        }
        this.SortAndRefreshTouchList();
    }

    private void UpdateAllNearbyPlayers(List<BnetPlayer> addedList, List<BnetPlayer> removedList)
    {
        if (removedList != null)
        {
            foreach (BnetPlayer player in removedList)
            {
                this.RemoveItem(false, MobileFriendListItem.TypeFlags.NearbyPlayer, player);
            }
        }
        this.UpdateNearbyPlayerItems();
        if (addedList != null)
        {
            foreach (BnetPlayer player2 in addedList)
            {
                FriendListItem item = new FriendListItem(false, MobileFriendListItem.TypeFlags.NearbyPlayer, player2);
                this.m_allItems.Add(item);
            }
        }
        this.SortAndRefreshTouchList();
    }

    private void UpdateAllRecruits()
    {
        List<Network.RecruitInfo> recruitList = RecruitListMgr.Get().GetRecruitList();
        List<Network.RecruitInfo> list2 = new List<Network.RecruitInfo>();
        List<Network.RecruitInfo> list3 = new List<Network.RecruitInfo>();
        List<Network.RecruitInfo> list4 = new List<Network.RecruitInfo>();
        IEnumerator<FriendListRecruitFrame> enumerator = this.GetItems<FriendListRecruitFrame>().GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                list4.Add(enumerator.Current.GetRecruitInfo());
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
        foreach (Network.RecruitInfo info in list4)
        {
            if (!recruitList.Contains(info))
            {
                list3.Add(info);
            }
        }
        <UpdateAllRecruits>c__AnonStorey2B1 storeyb = new <UpdateAllRecruits>c__AnonStorey2B1();
        using (List<Network.RecruitInfo>.Enumerator enumerator3 = recruitList.GetEnumerator())
        {
            while (enumerator3.MoveNext())
            {
                storeyb.info = enumerator3.Current;
                if ((this.FindFirstItem<FriendListFriendFrame>(new Predicate<FriendListFriendFrame>(storeyb.<>m__51)) == null) && !list4.Contains(storeyb.info))
                {
                    list2.Add(storeyb.info);
                }
            }
        }
        foreach (Network.RecruitInfo info2 in list3)
        {
            this.RemoveItem(false, MobileFriendListItem.TypeFlags.Recruit, info2);
        }
        foreach (Network.RecruitInfo info3 in list2)
        {
            FriendListItem item = new FriendListItem(false, MobileFriendListItem.TypeFlags.Recruit, info3);
            this.m_allItems.Add(item);
        }
        this.UpdateRecruitItems();
        this.SortAndRefreshTouchList();
    }

    private void UpdateBackgroundCollider()
    {
        if (<>f__am$cache24 == null)
        {
            <>f__am$cache24 = delegate (Bounds aggregate, Renderer renderer) {
                aggregate.Encapsulate(renderer.bounds);
                return aggregate;
            };
        }
        Bounds bounds = Enumerable.Aggregate<Renderer, Bounds>(this.window.GetComponentsInChildren<Renderer>(), new Bounds(base.transform.position, Vector3.zero), <>f__am$cache24);
        Vector3 vector = base.transform.InverseTransformPoint(bounds.min);
        Vector3 vector2 = base.transform.InverseTransformPoint(bounds.max);
        BoxCollider component = base.GetComponent<BoxCollider>();
        if (component == null)
        {
            component = base.gameObject.AddComponent<BoxCollider>();
        }
        component.center = ((Vector3) ((vector + vector2) / 2f)) + Vector3.forward;
        component.size = vector2 - vector;
    }

    private void UpdateButtonGlows()
    {
        this.removeFriendButton.ShowActiveGlow(this.IsInEditMode);
    }

    private void UpdateCurrentGamesHeader()
    {
        if (<>f__am$cache28 == null)
        {
            <>f__am$cache28 = i => i.ItemMainType == MobileFriendListItem.TypeFlags.CurrentGame;
        }
        int num = Enumerable.Count<FriendListItem>(this.m_allItems, <>f__am$cache28);
        if (num > 0)
        {
            object[] args = new object[] { num };
            string text = GameStrings.Format("GLOBAL_FRIENDLIST_CURRENT_GAMES_HEADER", args);
            FriendListItemHeader header = this.FindOrAddHeader(MobileFriendListItem.TypeFlags.CurrentGame);
            if (<>f__am$cache29 == null)
            {
                <>f__am$cache29 = item => item.IsHeader && (item.SubType == MobileFriendListItem.TypeFlags.CurrentGame);
            }
            if (!Enumerable.Any<FriendListItem>(this.m_allItems, <>f__am$cache29))
            {
                FriendListItem item = new FriendListItem(true, MobileFriendListItem.TypeFlags.CurrentGame, null);
                this.m_allItems.Add(item);
            }
            header.SetText(text);
        }
        else
        {
            this.RemoveItem(true, MobileFriendListItem.TypeFlags.CurrentGame, null);
        }
    }

    private void UpdateDropShadow()
    {
        if (this.outerShadow != null)
        {
            this.outerShadow.SetActive(!UniversalInputManager.Get().IsTouchMode());
        }
    }

    private void UpdateFriendItems()
    {
        IEnumerator<FriendListCurrentGameFrame> enumerator = this.GetItems<FriendListCurrentGameFrame>().GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                enumerator.Current.UpdateFriend();
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
        IEnumerator<FriendListFriendFrame> enumerator2 = this.GetItems<FriendListFriendFrame>().GetEnumerator();
        try
        {
            while (enumerator2.MoveNext())
            {
                enumerator2.Current.UpdateFriend();
            }
        }
        finally
        {
            if (enumerator2 == null)
            {
            }
            enumerator2.Dispose();
        }
    }

    private void UpdateFriendsHeader(FriendListItemHeader header = null)
    {
        if (<>f__am$cache2E == null)
        {
            <>f__am$cache2E = i => i.ItemMainType == MobileFriendListItem.TypeFlags.Friend;
        }
        IEnumerable<FriendListItem> source = Enumerable.Where<FriendListItem>(this.m_allItems, <>f__am$cache2E);
        if (<>f__am$cache2F == null)
        {
            <>f__am$cache2F = i => i.GetFriend().IsOnline();
        }
        int num = Enumerable.Count<FriendListItem>(source, <>f__am$cache2F);
        int num2 = source.Count<FriendListItem>();
        string text = null;
        if (num == num2)
        {
            object[] args = new object[] { num };
            text = GameStrings.Format("GLOBAL_FRIENDLIST_FRIENDS_HEADER_ALL_ONLINE", args);
        }
        else
        {
            object[] objArray2 = new object[] { num, num2 };
            text = GameStrings.Format("GLOBAL_FRIENDLIST_FRIENDS_HEADER", objArray2);
        }
        if (header == null)
        {
            header = this.FindOrAddHeader(MobileFriendListItem.TypeFlags.Friend);
            if (<>f__am$cache30 == null)
            {
                <>f__am$cache30 = item => item.IsHeader && (item.SubType == MobileFriendListItem.TypeFlags.Friend);
            }
            if (!Enumerable.Any<FriendListItem>(this.m_allItems, <>f__am$cache30))
            {
                FriendListItem item = new FriendListItem(true, MobileFriendListItem.TypeFlags.Friend, null);
                this.m_allItems.Add(item);
            }
        }
        header.SetText(text);
    }

    private void UpdateHeaderBackground(FriendListItemHeader itemHeader)
    {
        <UpdateHeaderBackground>c__AnonStorey2B6 storeyb = new <UpdateHeaderBackground>c__AnonStorey2B6();
        if (itemHeader != null)
        {
            MobileFriendListItem component = itemHeader.GetComponent<MobileFriendListItem>();
            if ((component != null) && (((component.Type & MobileFriendListItem.TypeFlags.Request) != 0) || ((component.Type & MobileFriendListItem.TypeFlags.CurrentGame) != 0)))
            {
                TiledBackground background = null;
                if (itemHeader.Background == null)
                {
                    GameObject go = new GameObject("ItemsBackground") {
                        transform = { parent = component.transform }
                    };
                    TransformUtil.Identity(go);
                    go.layer = 0x18;
                    HeaderBackgroundInfo info = ((component.Type & MobileFriendListItem.TypeFlags.Request) == 0) ? this.listInfo.currentGameBackgroundInfo : this.listInfo.requestBackgroundInfo;
                    go.AddComponent<MeshFilter>().mesh = info.mesh;
                    go.AddComponent<MeshRenderer>().material = info.material;
                    background = go.AddComponent<TiledBackground>();
                    itemHeader.Background = go;
                }
                else
                {
                    background = itemHeader.Background.GetComponent<TiledBackground>();
                }
                background.transform.parent = null;
                storeyb.type = component.Type ^ MobileFriendListItem.TypeFlags.Header;
                Bounds bounds = Enumerable.Aggregate<ITouchListItem, Bounds>(this.items, new Bounds(component.transform.position, Vector3.zero), new Func<Bounds, ITouchListItem, Bounds>(storeyb.<>m__61));
                background.transform.parent = component.transform;
                bounds.center = component.transform.InverseTransformPoint(bounds.center);
                background.SetBounds(bounds);
                TransformUtil.SetPosZ(background.transform, 2f);
                background.gameObject.SetActive(itemHeader.IsShowingContents);
            }
        }
    }

    private void UpdateItems()
    {
        IEnumerator<FriendListRequestFrame> enumerator = this.GetItems<FriendListRequestFrame>().GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                enumerator.Current.UpdateInvite();
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
        this.UpdateFriendItems();
    }

    private void UpdateItemsCamera()
    {
        Camera bnetCamera = BaseUI.Get().GetBnetCamera();
        Transform bottomRightBone = this.GetBottomRightBone();
        Vector3 vector = bnetCamera.WorldToScreenPoint(this.listInfo.topLeft.position);
        Vector3 vector2 = bnetCamera.WorldToScreenPoint(bottomRightBone.position);
        GeneralUtils.Swap<float>(ref vector.y, ref vector2.y);
        this.m_itemsCamera.pixelRect = new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
        this.m_itemsCamera.orthographicSize = this.m_itemsCamera.rect.height * bnetCamera.orthographicSize;
    }

    private void UpdateItemsList()
    {
        Transform bottomRightBone = this.GetBottomRightBone();
        this.items.transform.position = (Vector3) ((this.listInfo.topLeft.position + bottomRightBone.position) / 2f);
        Vector3 vector = bottomRightBone.position - this.listInfo.topLeft.position;
        this.items.ClipSize = new Vector2(vector.x, Math.Abs(vector.y));
        if (this.innerShadow != null)
        {
            this.innerShadow.transform.position = this.items.transform.position;
            Vector3 vector2 = this.GetBottomRightBone().position - this.listInfo.topLeft.position;
            WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex(Mathf.Abs(vector2.x), 0), new WorldDimensionIndex(Mathf.Abs(vector2.y), 2) };
            TransformUtil.SetLocalScaleToWorldDimension(this.innerShadow, dimensions);
        }
    }

    private void UpdateMyself()
    {
        BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
        if ((myPlayer != null) && myPlayer.IsDisplayable())
        {
            BnetBattleTag battleTag = myPlayer.GetBattleTag();
            this.myPortrait.SetProgramId(BnetProgramId.HEARTHSTONE);
            this.me.nameText.Text = battleTag.GetName();
            this.me.numberText.Text = string.Format("#{0}", battleTag.GetNumber().ToString());
            this.me.statusText.Text = GameStrings.Get("GLOBAL_FRIENDLIST_MYSTATUS");
            TransformUtil.SetPoint((Component) this.me.numberText, Anchor.LEFT, (Component) this.me.nameText, Anchor.RIGHT, (Vector3) (6f * Vector3.right));
            MedalInfoTranslator medal = new MedalInfoTranslator(NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>());
            if ((medal == null) || (medal.GetCurrentMedal().rank == 0x19))
            {
                this.me.m_MedalPatch.SetActive(false);
                this.myPortrait.gameObject.SetActive(true);
                if (this.portraitBackground != null)
                {
                    Material[] materials = this.portraitBackground.GetComponent<Renderer>().materials;
                    materials[0] = this.unrankedBackground;
                    this.portraitBackground.GetComponent<Renderer>().materials = materials;
                }
            }
            else
            {
                this.myPortrait.gameObject.SetActive(false);
                this.me.m_Medal.SetEnabled(false);
                this.me.m_Medal.SetMedal(medal, false);
                this.me.m_MedalPatch.SetActive(true);
                if (this.portraitBackground != null)
                {
                    Material[] materialArray2 = this.portraitBackground.GetComponent<Renderer>().materials;
                    materialArray2[0] = this.rankedBackground;
                    this.portraitBackground.GetComponent<Renderer>().materials = materialArray2;
                }
            }
        }
        else
        {
            this.me.nameText.Text = string.Empty;
            this.me.numberText.Text = string.Empty;
            this.me.statusText.Text = string.Empty;
        }
    }

    private void UpdateNearbyPlayerItems()
    {
        IEnumerator<FriendListNearbyPlayerFrame> enumerator = this.GetItems<FriendListNearbyPlayerFrame>().GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                enumerator.Current.UpdateNearbyPlayer();
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
    }

    private void UpdateNearbyPlayersHeader(FriendListItemHeader header = null)
    {
        if (<>f__am$cache2A == null)
        {
            <>f__am$cache2A = i => i.ItemMainType == MobileFriendListItem.TypeFlags.NearbyPlayer;
        }
        int num = Enumerable.Count<FriendListItem>(this.m_allItems, <>f__am$cache2A);
        if (num > 0)
        {
            object[] args = new object[] { num };
            string text = GameStrings.Format("GLOBAL_FRIENDLIST_NEARBY_PLAYERS_HEADER", args);
            if (header == null)
            {
                header = this.FindOrAddHeader(MobileFriendListItem.TypeFlags.NearbyPlayer);
                if (<>f__am$cache2B == null)
                {
                    <>f__am$cache2B = item => item.IsHeader && (item.SubType == MobileFriendListItem.TypeFlags.NearbyPlayer);
                }
                if (!Enumerable.Any<FriendListItem>(this.m_allItems, <>f__am$cache2B))
                {
                    FriendListItem item = new FriendListItem(true, MobileFriendListItem.TypeFlags.NearbyPlayer, null);
                    this.m_allItems.Add(item);
                }
            }
            header.SetText(text);
        }
        else if (header == null)
        {
            this.RemoveItem(true, MobileFriendListItem.TypeFlags.NearbyPlayer, null);
        }
    }

    private void UpdateRecentOpponent()
    {
        BnetPlayer recentOpponent = FriendMgr.Get().GetRecentOpponent();
        if (recentOpponent == null)
        {
            this.recentOpponent.button.gameObject.SetActive(false);
        }
        else
        {
            this.recentOpponent.button.gameObject.SetActive(true);
            this.recentOpponent.nameText.Text = FriendUtils.GetUniqueNameWithColor(recentOpponent);
        }
    }

    private void UpdateRecruitItems()
    {
        IEnumerator<FriendListRecruitFrame> enumerator = this.GetItems<FriendListRecruitFrame>().GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                enumerator.Current.UpdateRecruit();
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
    }

    private void UpdateRecruitsHeader()
    {
        if (<>f__am$cache2C == null)
        {
            <>f__am$cache2C = i => i.ItemMainType == MobileFriendListItem.TypeFlags.Recruit;
        }
        int num = Enumerable.Count<FriendListItem>(this.m_allItems, <>f__am$cache2C);
        if (num > 0)
        {
            object[] args = new object[] { num };
            string text = GameStrings.Format("GLOBAL_FRIENDLIST_RECRUITS_HEADER", args);
            FriendListItemHeader header = this.FindOrAddHeader(MobileFriendListItem.TypeFlags.Recruit);
            if (<>f__am$cache2D == null)
            {
                <>f__am$cache2D = item => item.IsHeader && (item.SubType == MobileFriendListItem.TypeFlags.Recruit);
            }
            if (!Enumerable.Any<FriendListItem>(this.m_allItems, <>f__am$cache2D))
            {
                FriendListItem item = new FriendListItem(true, MobileFriendListItem.TypeFlags.Recruit, null);
                this.m_allItems.Add(item);
            }
            header.SetText(text);
        }
        else
        {
            this.RemoveItem(true, MobileFriendListItem.TypeFlags.Recruit, null);
        }
    }

    private void UpdateRequests(List<BnetInvitation> addedList, List<BnetInvitation> removedList)
    {
        if ((removedList != null) || (addedList != null))
        {
            if (removedList != null)
            {
                foreach (BnetInvitation invitation in removedList)
                {
                    this.RemoveItem(false, MobileFriendListItem.TypeFlags.Request, invitation);
                }
            }
            IEnumerator<FriendListRequestFrame> enumerator = this.GetItems<FriendListRequestFrame>().GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateInvite();
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }
            if (addedList != null)
            {
                foreach (BnetInvitation invitation2 in addedList)
                {
                    FriendListItem item = new FriendListItem(false, MobileFriendListItem.TypeFlags.Request, invitation2);
                    this.m_allItems.Add(item);
                }
            }
        }
    }

    private void UpdateRequestsHeader(FriendListItemHeader header = null)
    {
        if (<>f__am$cache26 == null)
        {
            <>f__am$cache26 = i => i.ItemMainType == MobileFriendListItem.TypeFlags.Request;
        }
        int num = Enumerable.Count<FriendListItem>(this.m_allItems, <>f__am$cache26);
        if (num > 0)
        {
            object[] args = new object[] { num };
            string text = GameStrings.Format("GLOBAL_FRIENDLIST_REQUESTS_HEADER", args);
            if (header == null)
            {
                header = this.FindOrAddHeader(MobileFriendListItem.TypeFlags.Request);
                if (<>f__am$cache27 == null)
                {
                    <>f__am$cache27 = item => item.IsHeader && (item.SubType == MobileFriendListItem.TypeFlags.Request);
                }
                if (!Enumerable.Any<FriendListItem>(this.m_allItems, <>f__am$cache27))
                {
                    FriendListItem item = new FriendListItem(true, MobileFriendListItem.TypeFlags.Request, null);
                    this.m_allItems.Add(item);
                }
            }
            header.SetText(text);
        }
        else if (header == null)
        {
            this.RemoveItem(true, MobileFriendListItem.TypeFlags.Request, null);
        }
    }

    private void UpdateSelectedItem()
    {
        BnetPlayer selectedFriend = FriendMgr.Get().GetSelectedFriend();
        FriendListBaseFriendFrame frame = this.FindBaseFriendFrame(selectedFriend);
        if (frame == null)
        {
            if (this.items.SelectedIndex != -1)
            {
                this.items.SelectedIndex = -1;
                if (this.m_removeFriendPopup != null)
                {
                    this.m_removeFriendPopup.Hide();
                    this.m_removeFriendPopup = null;
                    if (this.RemoveFriendPopupClosed != null)
                    {
                        this.RemoveFriendPopupClosed();
                    }
                }
            }
        }
        else
        {
            this.items.SelectedIndex = this.items.IndexOf(frame.GetComponent<MobileFriendListItem>());
        }
    }

    public bool IsInEditMode
    {
        get
        {
            return this.m_editFriendsMode;
        }
    }

    public BnetPlayer SelectedPlayer
    {
        get
        {
            if (this.items.SelectedItem != null)
            {
                FriendListBaseFriendFrame component = this.items.SelectedItem.GetComponent<FriendListBaseFriendFrame>();
                if (component != null)
                {
                    return component.GetFriend();
                }
                FriendListNearbyPlayerFrame frame2 = this.items.SelectedItem.GetComponent<FriendListNearbyPlayerFrame>();
                if (frame2 != null)
                {
                    return frame2.GetNearbyPlayer();
                }
            }
            return null;
        }
        set
        {
            <>c__AnonStorey2B0 storeyb = new <>c__AnonStorey2B0 {
                value = value
            };
            this.items.SelectedIndex = this.items.FindIndex(new Predicate<ITouchListItem>(storeyb.<>m__4E));
            this.UpdateItems();
        }
    }

    public bool ShowingAddFriendFrame
    {
        get
        {
            return (this.m_addFriendFrame != null);
        }
    }

    [CompilerGenerated]
    private sealed class <>c__AnonStorey2B0
    {
        internal BnetPlayer value;

        internal bool <>m__4E(ITouchListItem item)
        {
            if (item == null)
            {
                return false;
            }
            FriendListBaseFriendFrame component = item.GetComponent<FriendListBaseFriendFrame>();
            if (component != null)
            {
                return (component.GetFriend() == this.value);
            }
            FriendListNearbyPlayerFrame frame2 = item.GetComponent<FriendListNearbyPlayerFrame>();
            return ((frame2 != null) && (frame2.GetNearbyPlayer() == this.value));
        }
    }

    [CompilerGenerated]
    private sealed class <FindBaseFriendFrame>c__AnonStorey2B2
    {
        internal BnetPlayer friend;

        internal bool <>m__52(FriendListBaseFriendFrame frame)
        {
            return (frame.GetFriend() == this.friend);
        }
    }

    [CompilerGenerated]
    private sealed class <FindCurrentGameFrame>c__AnonStorey2B3
    {
        internal BnetPlayer friend;

        internal bool <>m__53(FriendListCurrentGameFrame frame)
        {
            return (frame.GetFriend() == this.friend);
        }
    }

    [CompilerGenerated]
    private sealed class <FindFirstItem>c__AnonStorey2B8<T> where T: MonoBehaviour
    {
        internal Predicate<T> predicate;

        internal bool <>m__63(ITouchListItem listItem)
        {
            T component = listItem.GetComponent<T>();
            return ((component != null) && this.predicate(component));
        }
    }

    [CompilerGenerated]
    private sealed class <FindFriendFrame>c__AnonStorey2B4
    {
        internal BnetPlayer friend;

        internal bool <>m__54(FriendListFriendFrame frame)
        {
            return (frame.GetFriend() == this.friend);
        }
    }

    [CompilerGenerated]
    private sealed class <FindFriendFrame>c__AnonStorey2B5
    {
        internal BnetAccountId id;

        internal bool <>m__55(FriendListFriendFrame frame)
        {
            return (frame.GetFriend().GetAccountId() == this.id);
        }
    }

    [CompilerGenerated]
    private sealed class <OnHeaderSectionToggle>c__AnonStorey2B7
    {
        internal FriendListItemHeader header;

        internal bool <>m__62(FriendListFrame.FriendListItem item)
        {
            return (item.IsHeader && (item.SubType == this.header.SubType));
        }
    }

    [CompilerGenerated]
    private sealed class <RemoveItem>c__AnonStorey2B9
    {
        internal bool isHeader;
        internal object itemToRemove;
        internal MobileFriendListItem.TypeFlags type;

        internal bool <>m__67(FriendListFrame.FriendListItem item)
        {
            if ((item.IsHeader != this.isHeader) || (item.SubType != this.type))
            {
                return false;
            }
            if (this.itemToRemove == null)
            {
                return true;
            }
            MobileFriendListItem.TypeFlags type = this.type;
            switch (type)
            {
                case MobileFriendListItem.TypeFlags.Recruit:
                    return (item.GetRecruit() == ((Network.RecruitInfo) this.itemToRemove));

                case MobileFriendListItem.TypeFlags.Friend:
                    return (item.GetFriend() == ((BnetPlayer) this.itemToRemove));

                case MobileFriendListItem.TypeFlags.CurrentGame:
                    return (item.GetCurrentGame() == ((BnetPlayer) this.itemToRemove));

                case MobileFriendListItem.TypeFlags.NearbyPlayer:
                    return (item.GetNearbyPlayer() == ((BnetPlayer) this.itemToRemove));
            }
            if (type != MobileFriendListItem.TypeFlags.Request)
            {
                return false;
            }
            return (item.GetInvite() == ((BnetInvitation) this.itemToRemove));
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateAllRecruits>c__AnonStorey2B1
    {
        internal Network.RecruitInfo info;

        internal bool <>m__51(FriendListFriendFrame frame)
        {
            return (frame.GetFriend().GetAccountId() == this.info.RecruitID);
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateHeaderBackground>c__AnonStorey2B6
    {
        internal MobileFriendListItem.TypeFlags type;

        internal Bounds <>m__61(Bounds aggregate, ITouchListItem listItem)
        {
            MobileFriendListItem item = listItem as MobileFriendListItem;
            if ((item.Type & this.type) != 0)
            {
                aggregate.Encapsulate(item.ComputeWorldBounds());
            }
            return aggregate;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FriendListItem
    {
        private object m_item;
        public FriendListItem(bool isHeader, MobileFriendListItem.TypeFlags itemType, object itemData)
        {
            if (!isHeader && (itemData == null))
            {
                Log.Henry.Print("FriendListItem: itemData is null! itemType=" + itemType, new object[0]);
            }
            this.m_item = itemData;
            this.ItemFlags = itemType;
            if (isHeader)
            {
                this.ItemFlags |= MobileFriendListItem.TypeFlags.Header;
            }
            else
            {
                this.ItemFlags &= ~MobileFriendListItem.TypeFlags.Header;
            }
        }

        public MobileFriendListItem.TypeFlags ItemFlags { get; private set; }
        public bool IsHeader
        {
            get
            {
                return ((this.ItemFlags & MobileFriendListItem.TypeFlags.Header) != 0);
            }
        }
        public BnetPlayer GetFriend()
        {
            if ((this.ItemFlags & MobileFriendListItem.TypeFlags.Friend) == 0)
            {
                return null;
            }
            return (BnetPlayer) this.m_item;
        }

        public BnetPlayer GetCurrentGame()
        {
            if ((this.ItemFlags & MobileFriendListItem.TypeFlags.CurrentGame) == 0)
            {
                return null;
            }
            return (BnetPlayer) this.m_item;
        }

        public BnetPlayer GetNearbyPlayer()
        {
            if ((this.ItemFlags & MobileFriendListItem.TypeFlags.NearbyPlayer) == 0)
            {
                return null;
            }
            return (BnetPlayer) this.m_item;
        }

        public BnetInvitation GetInvite()
        {
            if ((this.ItemFlags & MobileFriendListItem.TypeFlags.Request) == 0)
            {
                return null;
            }
            return (BnetInvitation) this.m_item;
        }

        public Network.RecruitInfo GetRecruit()
        {
            if ((this.ItemFlags & MobileFriendListItem.TypeFlags.Recruit) == 0)
            {
                return null;
            }
            return (Network.RecruitInfo) this.m_item;
        }

        public MobileFriendListItem.TypeFlags ItemMainType
        {
            get
            {
                if (this.IsHeader)
                {
                    return MobileFriendListItem.TypeFlags.Header;
                }
                return this.SubType;
            }
        }
        public MobileFriendListItem.TypeFlags SubType
        {
            get
            {
                return (this.ItemFlags & ~MobileFriendListItem.TypeFlags.Header);
            }
        }
        public override string ToString()
        {
            if (this.IsHeader)
            {
                return string.Format("[{0}]Header", this.SubType);
            }
            return string.Format("[{0}]{1}", this.ItemMainType, this.m_item);
        }

        public System.Type GetFrameType()
        {
            MobileFriendListItem.TypeFlags itemMainType = this.ItemMainType;
            if (itemMainType != MobileFriendListItem.TypeFlags.Header)
            {
                if (itemMainType == MobileFriendListItem.TypeFlags.Recruit)
                {
                    return typeof(FriendListRecruitFrame);
                }
                if (itemMainType == MobileFriendListItem.TypeFlags.Friend)
                {
                    return typeof(FriendListFriendFrame);
                }
                if (itemMainType == MobileFriendListItem.TypeFlags.CurrentGame)
                {
                    return typeof(FriendListCurrentGameFrame);
                }
                if (itemMainType == MobileFriendListItem.TypeFlags.NearbyPlayer)
                {
                    return typeof(FriendListNearbyPlayerFrame);
                }
                if (itemMainType == MobileFriendListItem.TypeFlags.Request)
                {
                    return typeof(FriendListRequestFrame);
                }
            }
            else
            {
                return typeof(FriendListItemHeader);
            }
            object[] objArray1 = new object[] { "Unknown ItemType: ", this.ItemFlags, " (", (int) this.ItemFlags, ")" };
            throw new Exception(string.Concat(objArray1));
        }
    }

    [Serializable]
    public class HeaderBackgroundInfo
    {
        public Material material;
        public Mesh mesh;
    }

    [Serializable]
    public class ListInfo
    {
        public Transform bottomRight;
        public Transform bottomRightWithScrollbar;
        public FriendListFrame.HeaderBackgroundInfo currentGameBackgroundInfo;
        public FriendListFrame.HeaderBackgroundInfo requestBackgroundInfo;
        public Transform topLeft;
    }

    [Serializable]
    public class Me
    {
        public TournamentMedal m_Medal;
        public GameObject m_MedalPatch;
        public UberText nameText;
        public UberText numberText;
        public Spawner portraitRef;
        public UberText statusText;
    }

    private class NearbyPlayerUpdate
    {
        public ChangeType Change;
        public BnetPlayer Player;

        public NearbyPlayerUpdate(ChangeType c, BnetPlayer p)
        {
            this.Change = c;
            this.Player = p;
        }

        public enum ChangeType
        {
            Added,
            Removed
        }
    }

    [Serializable]
    public class Prefabs
    {
        public AddFriendFrame addFriendFrame;
        public FriendListCurrentGameFrame currentGameItem;
        public FriendListFriendFrame friendItem;
        public FriendListItemHeader headerItem;
        public FriendListNearbyPlayerFrame nearbyPlayerItem;
        public RecruitAFriendFrame recruitAFriendFrame;
        public FriendListRecruitFrame recruitItem;
        public FriendListRequestFrame requestItem;
    }

    [Serializable]
    public class RecentOpponent
    {
        public PegUIElement button;
        public UberText nameText;
    }

    private class VirtualizedFriendsListBehavior : TouchList.ILongListBehavior
    {
        [CompilerGenerated]
        private static Func<KeyValuePair<MobileFriendListItem.TypeFlags, FriendListItemHeader>, bool> <>f__am$cache5;
        private HashSet<MobileFriendListItem> m_acquiredItems = new HashSet<MobileFriendListItem>();
        private Bounds[] m_boundsByType;
        private int m_cachedMaxVisibleItems = -1;
        private List<MobileFriendListItem> m_freelist;
        private FriendListFrame m_friendList;
        private const int MAX_FREELIST_ITEMS = 20;

        public VirtualizedFriendsListBehavior(FriendListFrame friendList)
        {
            this.m_friendList = friendList;
        }

        public ITouchListItem AcquireItem(int index)
        {
            MobileFriendListItem.TypeFlags flags2;
            <AcquireItem>c__AnonStorey2BA storeyba = new <AcquireItem>c__AnonStorey2BA();
            if (this.m_acquiredItems.Count >= this.MaxAcquiredItems)
            {
                object[] objArray1 = new object[] { "Bug in ILongListBehavior? there are too many acquired items! index=", index, " max=", this.MaxAcquiredItems, " maxVisible=", this.MaxVisibleItems, " minBuffer=", this.MinBuffer, " acquiredItems.Count=", this.m_acquiredItems.Count, " hasCollapsedHeaders=", this.HasCollapsedHeaders };
                throw new Exception(string.Concat(objArray1));
            }
            if ((index < 0) || (index >= this.m_friendList.m_allItems.Count))
            {
                throw new IndexOutOfRangeException(string.Format("Invalid index, {0} has {1} elements.", DebugUtils.GetHierarchyPathAndType(this.m_friendList), this.m_friendList.m_allItems.Count));
            }
            storeyba.item = this.m_friendList.m_allItems[index];
            MobileFriendListItem.TypeFlags itemMainType = storeyba.item.ItemMainType;
            storeyba.frameType = storeyba.item.GetFrameType();
            if ((this.m_freelist != null) && !storeyba.item.IsHeader)
            {
                int num = this.m_freelist.FindLastIndex(new Predicate<MobileFriendListItem>(storeyba.<>m__6B));
                if ((num >= 0) && (this.m_freelist[num] == null))
                {
                    for (int i = 0; i < this.m_freelist.Count; i++)
                    {
                        if (this.m_freelist[i] == null)
                        {
                            this.m_freelist.RemoveAt(i);
                            i--;
                        }
                    }
                    num = this.m_freelist.FindLastIndex(new Predicate<MobileFriendListItem>(storeyba.<>m__6C));
                }
                if (num >= 0)
                {
                    MobileFriendListItem item = this.m_freelist[num];
                    this.m_freelist.RemoveAt(num);
                    flags2 = itemMainType;
                    if (flags2 != MobileFriendListItem.TypeFlags.Friend)
                    {
                        if (flags2 != MobileFriendListItem.TypeFlags.NearbyPlayer)
                        {
                            if (flags2 != MobileFriendListItem.TypeFlags.Request)
                            {
                                object[] objArray2 = new object[] { "VirtualizedFriendsListBehavior.AcquireItem[reuse] frameType=", storeyba.frameType.FullName, " itemType=", itemMainType };
                                throw new NotImplementedException(string.Concat(objArray2));
                            }
                            FriendListRequestFrame component = item.GetComponent<FriendListRequestFrame>();
                            component.SetInvite(storeyba.item.GetInvite());
                            this.m_friendList.FinishCreateVisualItem<FriendListRequestFrame>(component, itemMainType, this.m_friendList.FindHeader(itemMainType), component.gameObject);
                            bool activeSelf = component.gameObject.activeSelf;
                            component.gameObject.SetActive(true);
                            component.UpdateInvite();
                            if (!activeSelf)
                            {
                                component.gameObject.SetActive(activeSelf);
                            }
                        }
                        else
                        {
                            FriendListNearbyPlayerFrame frame3 = item.GetComponent<FriendListNearbyPlayerFrame>();
                            frame3.SetNearbyPlayer(storeyba.item.GetNearbyPlayer());
                            this.m_friendList.FinishCreateVisualItem<FriendListNearbyPlayerFrame>(frame3, itemMainType, this.m_friendList.FindHeader(itemMainType), frame3.gameObject);
                            bool flag3 = frame3.gameObject.activeSelf;
                            frame3.gameObject.SetActive(true);
                            frame3.UpdateNearbyPlayer();
                            if (!flag3)
                            {
                                frame3.gameObject.SetActive(flag3);
                            }
                        }
                    }
                    else
                    {
                        FriendListFriendFrame frame = item.GetComponent<FriendListFriendFrame>();
                        frame.SetFriend(storeyba.item.GetFriend());
                        this.m_friendList.FinishCreateVisualItem<FriendListFriendFrame>(frame, itemMainType, this.m_friendList.FindHeader(itemMainType), frame.gameObject);
                        bool flag = frame.gameObject.activeSelf;
                        frame.gameObject.SetActive(true);
                        frame.UpdateFriend();
                        if (!flag)
                        {
                            frame.gameObject.SetActive(flag);
                        }
                    }
                    item.gameObject.SetActive(true);
                    this.m_acquiredItems.Add(item);
                    return item;
                }
            }
            MobileFriendListItem item2 = null;
            flags2 = itemMainType;
            if (flags2 != MobileFriendListItem.TypeFlags.Header)
            {
                if (flags2 != MobileFriendListItem.TypeFlags.Friend)
                {
                    if (flags2 != MobileFriendListItem.TypeFlags.NearbyPlayer)
                    {
                        if (flags2 != MobileFriendListItem.TypeFlags.Request)
                        {
                            throw new NotImplementedException("VirtualizedFriendsListBehavior.AcquireItem[new] type=" + storeyba.frameType.FullName);
                        }
                        item2 = this.m_friendList.CreateRequestFrame(storeyba.item.GetInvite());
                    }
                    else
                    {
                        item2 = this.m_friendList.CreateNearbyPlayerFrame(storeyba.item.GetNearbyPlayer());
                    }
                    goto Label_04BB;
                }
            }
            else
            {
                item2 = this.m_friendList.FindHeader(storeyba.item.SubType).GetComponent<MobileFriendListItem>();
                goto Label_04BB;
            }
            item2 = this.m_friendList.CreateFriendFrame(storeyba.item.GetFriend());
        Label_04BB:
            this.m_acquiredItems.Add(item2);
            return item2;
        }

        private int GetBoundsByTypeIndex(MobileFriendListItem.TypeFlags itemType)
        {
            MobileFriendListItem.TypeFlags flags = itemType;
            if (flags != MobileFriendListItem.TypeFlags.Header)
            {
                if (flags == MobileFriendListItem.TypeFlags.Recruit)
                {
                    return 5;
                }
                if (flags == MobileFriendListItem.TypeFlags.Friend)
                {
                    return 4;
                }
                if (flags == MobileFriendListItem.TypeFlags.CurrentGame)
                {
                    return 3;
                }
                if (flags == MobileFriendListItem.TypeFlags.NearbyPlayer)
                {
                    return 2;
                }
                if (flags == MobileFriendListItem.TypeFlags.Request)
                {
                    return 1;
                }
            }
            else
            {
                return 0;
            }
            object[] objArray1 = new object[] { "Unknown ItemType: ", itemType, " (", (int) itemType, ")" };
            throw new Exception(string.Concat(objArray1));
        }

        public Vector3 GetItemSize(int allItemsIndex)
        {
            if ((allItemsIndex < 0) || (allItemsIndex >= this.AllItemsCount))
            {
                return Vector3.zero;
            }
            FriendListFrame.FriendListItem item = this.m_friendList.m_allItems[allItemsIndex];
            if (this.m_boundsByType == null)
            {
                this.InitializeBoundsByTypeArray();
            }
            int boundsByTypeIndex = this.GetBoundsByTypeIndex(item.ItemMainType);
            return this.m_boundsByType[boundsByTypeIndex].size;
        }

        private Component GetPrefab(MobileFriendListItem.TypeFlags itemType)
        {
            MobileFriendListItem.TypeFlags flags = itemType;
            if (flags != MobileFriendListItem.TypeFlags.Header)
            {
                if (flags == MobileFriendListItem.TypeFlags.Recruit)
                {
                    return this.m_friendList.prefabs.recruitItem;
                }
                if (flags == MobileFriendListItem.TypeFlags.Friend)
                {
                    return this.m_friendList.prefabs.friendItem;
                }
                if (flags == MobileFriendListItem.TypeFlags.CurrentGame)
                {
                    return this.m_friendList.prefabs.currentGameItem;
                }
                if (flags == MobileFriendListItem.TypeFlags.NearbyPlayer)
                {
                    return this.m_friendList.prefabs.nearbyPlayerItem;
                }
                if (flags == MobileFriendListItem.TypeFlags.Request)
                {
                    return this.m_friendList.prefabs.requestItem;
                }
            }
            else
            {
                return this.m_friendList.prefabs.headerItem;
            }
            object[] objArray1 = new object[] { "Unknown ItemType: ", itemType, " (", (int) itemType, ")" };
            throw new Exception(string.Concat(objArray1));
        }

        private static Bounds GetPrefabBounds(GameObject prefabGameObject)
        {
            GameObject go = UnityEngine.Object.Instantiate<GameObject>(prefabGameObject);
            go.SetActive(true);
            Bounds bounds = TransformUtil.ComputeSetPointBounds(go);
            UnityEngine.Object.DestroyImmediate(go);
            return bounds;
        }

        private void InitializeBoundsByTypeArray()
        {
            Array values = Enum.GetValues(typeof(MobileFriendListItem.TypeFlags));
            this.m_boundsByType = new Bounds[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                MobileFriendListItem.TypeFlags itemType = (MobileFriendListItem.TypeFlags) ((int) values.GetValue(i));
                Component prefab = this.GetPrefab(itemType);
                int boundsByTypeIndex = this.GetBoundsByTypeIndex(itemType);
                this.m_boundsByType[boundsByTypeIndex] = (prefab != null) ? GetPrefabBounds(prefab.gameObject) : new Bounds();
            }
        }

        public bool IsItemShowable(int allItemsIndex)
        {
            if ((allItemsIndex < 0) || (allItemsIndex >= this.AllItemsCount))
            {
                return false;
            }
            FriendListFrame.FriendListItem item = this.m_friendList.m_allItems[allItemsIndex];
            if (item.IsHeader)
            {
                return true;
            }
            FriendListItemHeader header = this.m_friendList.FindHeader(item.SubType);
            return ((header != null) && header.IsShowingContents);
        }

        public void ReleaseAllItems()
        {
            if (this.m_acquiredItems.Count != 0)
            {
                if (this.m_freelist == null)
                {
                    this.m_freelist = new List<MobileFriendListItem>();
                }
                foreach (MobileFriendListItem item in this.m_acquiredItems)
                {
                    if (item.IsHeader)
                    {
                        item.gameObject.SetActive(false);
                    }
                    else if (this.m_freelist.Count >= 20)
                    {
                        UnityEngine.Object.Destroy(item.gameObject);
                    }
                    else
                    {
                        this.m_freelist.Add(item);
                        item.gameObject.SetActive(false);
                    }
                    item.Unselected();
                }
                this.m_acquiredItems.Clear();
            }
        }

        public void ReleaseItem(ITouchListItem item)
        {
            MobileFriendListItem item2 = item as MobileFriendListItem;
            if (item2 == null)
            {
                throw new ArgumentException("given item is not MobileFriendListItem: " + item);
            }
            if (this.m_freelist == null)
            {
                this.m_freelist = new List<MobileFriendListItem>();
            }
            if (item2.IsHeader)
            {
                item2.gameObject.SetActive(false);
            }
            else if (this.m_freelist.Count >= 20)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
            else
            {
                this.m_freelist.Add(item2);
                item2.gameObject.SetActive(false);
            }
            if (!this.m_acquiredItems.Remove(item2))
            {
                object[] args = new object[] { item2 };
                Log.Henry.Print("VirtualizedFriendsListBehavior.ReleaseItem item not found in m_acquiredItems: {0}", args);
            }
            item2.Unselected();
        }

        public int AllItemsCount
        {
            get
            {
                return this.m_friendList.m_allItems.Count;
            }
        }

        public List<MobileFriendListItem> FreeList
        {
            get
            {
                return this.m_freelist;
            }
        }

        private bool HasCollapsedHeaders
        {
            get
            {
                if (<>f__am$cache5 == null)
                {
                    <>f__am$cache5 = kv => !kv.Value.IsShowingContents;
                }
                return Enumerable.Any<KeyValuePair<MobileFriendListItem.TypeFlags, FriendListItemHeader>>(this.m_friendList.m_headers, <>f__am$cache5);
            }
        }

        public int MaxAcquiredItems
        {
            get
            {
                return (this.MaxVisibleItems + (2 * this.MinBuffer));
            }
        }

        public int MaxVisibleItems
        {
            get
            {
                if (this.m_cachedMaxVisibleItems < 0)
                {
                    this.m_cachedMaxVisibleItems = 0;
                    Vector2 clipSize = this.m_friendList.items.ClipSize;
                    Bounds prefabBounds = GetPrefabBounds(this.m_friendList.prefabs.requestItem.gameObject);
                    Bounds bounds2 = GetPrefabBounds(this.m_friendList.prefabs.friendItem.gameObject);
                    Bounds bounds3 = GetPrefabBounds(this.m_friendList.prefabs.nearbyPlayerItem.gameObject);
                    float num = prefabBounds.max.y - prefabBounds.min.y;
                    float num2 = bounds2.max.y - bounds2.min.y;
                    float num3 = bounds3.max.y - bounds3.min.y;
                    float[] values = new float[] { num, num2, num3 };
                    float num4 = Mathf.Min(values);
                    if (num4 > 0f)
                    {
                        int num5 = Mathf.CeilToInt(clipSize.y / num4);
                        this.m_cachedMaxVisibleItems = num5 + 3;
                    }
                }
                return this.m_cachedMaxVisibleItems;
            }
        }

        public int MinBuffer
        {
            get
            {
                return 2;
            }
        }

        [CompilerGenerated]
        private sealed class <AcquireItem>c__AnonStorey2BA
        {
            internal System.Type frameType;
            internal FriendListFrame.FriendListItem item;

            internal bool <>m__6B(MobileFriendListItem m)
            {
                return (!this.item.IsHeader ? (m.GetComponent(this.frameType) != null) : m.IsHeader);
            }

            internal bool <>m__6C(MobileFriendListItem m)
            {
                return (!this.item.IsHeader ? (m.GetComponent(this.frameType) != null) : m.IsHeader);
            }
        }
    }
}

