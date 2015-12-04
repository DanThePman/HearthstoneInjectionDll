using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class PackOpening : MonoBehaviour
{
    private bool m_autoOpenPending;
    public UIBButton m_BackButton;
    public PackOpeningBones m_Bones;
    private PackOpeningDirector m_director;
    public PackOpeningDirector m_DirectorPrefab;
    private UnopenedPack m_draggedPack;
    public GameObject m_DragPlane;
    private bool m_enableBackButton;
    private static bool m_hasAcknowledgedKoreanWarning;
    public UberText m_HeaderText;
    private Notification m_hintArrow;
    public GameObject m_InputBlocker;
    private int m_lastOpenedBoosterId;
    private bool m_loadingUnopenedPack;
    public bool m_OnePackCentered = true;
    private int m_packCount;
    private GameObject m_PackOpeningCardFX;
    private bool m_shown;
    public PackOpeningSocket m_Socket;
    public PackOpeningSocket m_SocketAccent;
    public StoreButton m_StoreButton;
    public Vector3 m_StoreButtonOffset;
    public UIBObjectSpacing m_UnopenedPackContainer;
    public float m_UnopenedPackPadding;
    private Map<int, UnopenedPack> m_unopenedPacks = new Map<int, UnopenedPack>();
    public UIBScrollable m_UnopenedPackScroller;
    private Map<int, bool> m_unopenedPacksLoading = new Map<int, bool>();
    private bool m_waitingForInitialNetData = true;
    private const int PACKS_TO_LOAD_BEFORE_CACHE_RESET = 10;
    private static PackOpening s_instance;

    private void AutomaticallyOpenPack()
    {
        if (this.CanOpenPackAutomatically())
        {
            this.HideUnopenedPackTooltip();
            UnopenedPack pack = null;
            if (!this.m_unopenedPacks.TryGetValue(this.m_lastOpenedBoosterId, out pack) || (pack.GetBoosterStack().Count == 0))
            {
                foreach (KeyValuePair<int, UnopenedPack> pair in this.m_unopenedPacks)
                {
                    if ((pair.Value != null) && (pair.Value.GetBoosterStack().Count > 0))
                    {
                        pack = pair.Value;
                        break;
                    }
                }
            }
            if ((pack != null) && pack.CanOpenPack())
            {
                this.m_draggedPack = pack.AcquireDraggedPack();
                this.PickUpBooster();
                pack.StopAlert();
                this.OpenBooster(this.m_draggedPack);
                this.DestroyDraggedPack();
                this.UpdateUIEvents();
                this.m_DragPlane.SetActive(false);
            }
        }
    }

    private void Awake()
    {
        s_instance = this;
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_VISIT_PACK_OPEN_SCREEN);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            AssetLoader.Get().LoadGameObject("PackOpeningCardFX_Phone", new AssetLoader.GameObjectCallback(this.OnPackOpeningFXLoaded), null, false);
        }
        else
        {
            AssetLoader.Get().LoadGameObject("PackOpeningCardFX", new AssetLoader.GameObjectCallback(this.OnPackOpeningFXLoaded), null, false);
        }
        this.InitializeNet();
        this.InitializeUI();
        Box.Get().AddTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxTransitionFinished));
        this.m_StoreButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnStoreButtonReleased));
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    private bool CanOpenPackAutomatically()
    {
        if (this.m_autoOpenPending)
        {
            return false;
        }
        if (!this.m_shown)
        {
            return false;
        }
        if (!GameUtils.HaveBoosters())
        {
            return false;
        }
        if (this.m_director.IsPlaying() && !this.m_director.IsDoneButtonShown())
        {
            return false;
        }
        if (this.m_DragPlane.activeSelf)
        {
            return false;
        }
        if (StoreManager.Get().IsShownOrWaitingToShow())
        {
            return false;
        }
        return true;
    }

    private void CreateDirector()
    {
        GameObject obj3 = UnityEngine.Object.Instantiate<GameObject>(this.m_DirectorPrefab.gameObject);
        this.m_director = obj3.GetComponent<PackOpeningDirector>();
        obj3.transform.parent = base.transform;
        TransformUtil.CopyWorld((Component) this.m_director, (Component) this.m_Bones.m_Director);
    }

    private void CreateDraggedPack(UnopenedPack creatorPack)
    {
        RaycastHit hit;
        this.m_draggedPack = creatorPack.AcquireDraggedPack();
        Vector3 position = this.m_draggedPack.transform.position;
        if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.DragPlane.LayerBit(), out hit))
        {
            position = hit.point;
        }
        float f = Vector3.Dot(Camera.main.transform.forward, Vector3.up);
        float num2 = -f / Mathf.Abs(f);
        Bounds bounds = this.m_draggedPack.GetComponent<Collider>().bounds;
        position.y += (num2 * bounds.extents.y) * this.m_draggedPack.transform.lossyScale.y;
        this.m_draggedPack.transform.position = position;
    }

    private void DestroyDraggedPack()
    {
        this.m_draggedPack.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDraggedPackRelease));
        this.m_draggedPack.GetCreatorPack().ReleaseDraggedPack();
        this.m_draggedPack = null;
    }

    private void DestroyHint()
    {
        if (this.m_hintArrow != null)
        {
            UnityEngine.Object.Destroy(this.m_hintArrow.gameObject);
            this.m_hintArrow = null;
        }
    }

    private void DropPack()
    {
        PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
        this.m_Socket.OnPackReleased();
        this.m_SocketAccent.OnPackReleased();
        if (UniversalInputManager.Get().InputIsOver(this.m_Socket.gameObject))
        {
            if (BattleNet.GetAccountCountry() == "KOR")
            {
                m_hasAcknowledgedKoreanWarning = true;
            }
            this.OpenBooster(this.m_draggedPack);
            this.HideHint();
        }
        else
        {
            this.PutBackBooster();
            this.DestroyHint();
        }
        this.DestroyDraggedPack();
        this.UpdateUIEvents();
        this.m_DragPlane.SetActive(false);
    }

    private void FixArrowScale(Transform parent)
    {
        Transform transform = this.m_hintArrow.transform.parent;
        this.m_hintArrow.transform.parent = parent;
        this.m_hintArrow.transform.localScale = Vector3.one;
        this.m_hintArrow.transform.parent = transform;
    }

    public static PackOpening Get()
    {
        return s_instance;
    }

    public GameObject GetPackOpeningCardEffects()
    {
        return this.m_PackOpeningCardFX;
    }

    public bool HandleKeyboardInput()
    {
        if (!Input.GetKeyUp(KeyCode.Space))
        {
            return false;
        }
        if (this.CanOpenPackAutomatically())
        {
            this.m_autoOpenPending = true;
            this.m_director.FinishPackOpen();
            base.StartCoroutine(this.OpenNextPackWhenReady());
        }
        return true;
    }

    private void Hide()
    {
        if (this.m_shown)
        {
            this.m_shown = false;
            this.DestroyHint();
            this.m_StoreButton.Unload();
            this.m_InputBlocker.SetActive(false);
            this.UnregisterUIEvents();
            this.ShutdownNet();
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
        }
    }

    [DebuggerHidden]
    private IEnumerator HideAfterNoMorePacks()
    {
        return new <HideAfterNoMorePacks>c__Iterator1AB { <>f__this = this };
    }

    private void HideHint()
    {
        if (this.m_hintArrow != null)
        {
            Options.Get().SetBool(Option.HAS_OPENED_BOOSTER, true);
            UnityEngine.Object.Destroy(this.m_hintArrow.gameObject);
            this.m_hintArrow = null;
        }
    }

    private void HideUnopenedPackTooltip()
    {
        foreach (KeyValuePair<int, UnopenedPack> pair in this.m_unopenedPacks)
        {
            if (pair.Value != null)
            {
                pair.Value.GetComponent<TooltipZone>().HideTooltip();
            }
        }
    }

    private void HoldPack(UnopenedPack selectedPack)
    {
        if (selectedPack.CanOpenPack())
        {
            this.HideUnopenedPackTooltip();
            PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
            this.m_DragPlane.SetActive(true);
            this.CreateDraggedPack(selectedPack);
            if (this.m_draggedPack != null)
            {
                KeywordHelpPanel componentInChildren = this.m_draggedPack.GetComponentInChildren<KeywordHelpPanel>();
                if (componentInChildren != null)
                {
                    UnityEngine.Object.Destroy(componentInChildren.gameObject);
                }
            }
            this.PickUpBooster();
            selectedPack.StopAlert();
            this.ShowHintOnSlot();
            this.m_Socket.OnPackHeld();
            this.m_SocketAccent.OnPackHeld();
            this.UpdateUIEvents();
        }
    }

    private void InitializeNet()
    {
        this.m_waitingForInitialNetData = true;
        NetCache.Get().RegisterScreenPackOpening(new NetCache.NetCacheCallback(this.OnNetDataReceived), new NetCache.ErrorCallback(NetCache.DefaultErrorHandler));
        Network.Get().RegisterNetHandler(BoosterContent.PacketID.ID, new Network.NetHandler(this.OnBoosterOpened), null);
    }

    private void InitializeUI()
    {
        this.m_HeaderText.Text = GameStrings.Get("GLUE_PACK_OPENING_HEADER");
        this.m_BackButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBackButtonPressed));
        this.m_DragPlane.SetActive(false);
        this.m_InputBlocker.SetActive(false);
    }

    private void LayoutPacks(bool animate = false)
    {
        List<int> sortedPackIds = GameUtils.GetSortedPackIds();
        this.m_UnopenedPackContainer.ClearObjects();
        foreach (int num in sortedPackIds)
        {
            UnopenedPack pack;
            this.m_unopenedPacks.TryGetValue(num, out pack);
            if ((pack != null) && (pack.GetBoosterStack().Count != 0))
            {
                pack.gameObject.SetActive(true);
                this.m_UnopenedPackContainer.AddObject(pack, true);
            }
        }
        if (this.m_OnePackCentered && (this.m_UnopenedPackContainer.m_Objects.Count <= 1))
        {
            this.m_UnopenedPackContainer.AddSpace(0);
        }
        this.m_UnopenedPackContainer.AddObject(this.m_StoreButton, this.m_StoreButtonOffset, true);
        if (animate)
        {
            this.m_UnopenedPackContainer.AnimateUpdatePositions(0.25f, iTween.EaseType.easeInOutQuad);
        }
        else
        {
            this.m_UnopenedPackContainer.UpdatePositions();
        }
    }

    private void NotifySceneLoadedWhenReady()
    {
        if (this.m_waitingForInitialNetData)
        {
            this.m_waitingForInitialNetData = false;
            SceneMgr.Get().NotifySceneLoaded();
        }
    }

    private void OnBackButtonPressed(UIEvent e)
    {
        Navigation.GoBack();
    }

    private void OnBoosterOpened()
    {
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_BOOSTER_OPENED);
        List<NetCache.BoosterCard> cards = Network.OpenedBooster();
        this.m_director.OnBoosterOpened(cards);
    }

    private void OnBoxTransitionFinished(object userData)
    {
        Box.Get().RemoveTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxTransitionFinished));
        this.Show();
    }

    private void OnDestroy()
    {
        if ((this.m_draggedPack != null) && (PegCursor.Get() != null))
        {
            PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
        }
        this.ShutdownNet();
        s_instance = null;
    }

    private void OnDirectorFinished(object userData)
    {
        this.m_UnopenedPackScroller.Pause(false);
        int num = 0;
        foreach (KeyValuePair<int, UnopenedPack> pair in this.m_unopenedPacks)
        {
            if (pair.Value != null)
            {
                int count = pair.Value.GetBoosterStack().Count;
                num += count;
                pair.Value.gameObject.SetActive(count > 0);
            }
        }
        if (num == 0)
        {
            base.StartCoroutine(this.HideAfterNoMorePacks());
        }
        else
        {
            this.m_InputBlocker.SetActive(false);
            this.m_packCount++;
            if (this.m_packCount == 10)
            {
                DefLoader.Get().ClearCardDefs();
                this.m_packCount = 0;
            }
            this.CreateDirector();
            this.LayoutPacks(true);
        }
        BnetBar.Get().m_currencyFrame.RefreshContents();
    }

    private void OnDraggedPackRelease(UIEvent e)
    {
        this.DropPack();
    }

    [DebuggerHidden]
    private IEnumerator OnFakeBoosterOpened()
    {
        return new <OnFakeBoosterOpened>c__Iterator1AA { <>f__this = this };
    }

    private bool OnNavigateBack()
    {
        if (!this.m_enableBackButton || this.m_InputBlocker.activeSelf)
        {
            return false;
        }
        this.Hide();
        return true;
    }

    private void OnNetDataReceived()
    {
        this.NotifySceneLoadedWhenReady();
        this.UpdatePacks();
        this.UpdateUIEvents();
    }

    private void OnPackOpeningFXLoaded(string name, GameObject gameObject, object callbackData)
    {
        this.m_PackOpeningCardFX = gameObject;
    }

    private void OnStoreButtonReleased(UIEvent e)
    {
        if (!this.m_StoreButton.IsVisualClosed())
        {
            StoreManager.Get().StartGeneralTransaction();
        }
    }

    private void OnUnopenedPackHold(UIEvent e)
    {
        this.HoldPack(e.GetElement() as UnopenedPack);
    }

    private void OnUnopenedPackLoaded(string name, GameObject go, object userData)
    {
        NetCache.BoosterStack boosterStack = (NetCache.BoosterStack) userData;
        int id = boosterStack.Id;
        this.m_unopenedPacksLoading[id] = false;
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("PackOpening.OnUnopenedPackLoaded() - FAILED to load {0}", name));
        }
        else
        {
            UnopenedPack component = go.GetComponent<UnopenedPack>();
            go.SetActive(false);
            if (component == null)
            {
                UnityEngine.Debug.LogError(string.Format("PackOpening.OnUnopenedPackLoaded() - asset {0} did not have a {1} script on it", name, typeof(UnopenedPack)));
            }
            else
            {
                this.m_unopenedPacks.Add(id, component);
                component.gameObject.SetActive(true);
                GameUtils.SetParent((Component) component, (Component) this.m_UnopenedPackContainer, false);
                component.transform.localScale = Vector3.one;
                component.AddEventListener(UIEventType.HOLD, new UIEvent.Handler(this.OnUnopenedPackHold));
                component.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnUnopenedPackRollover));
                component.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnUnopenedPackRollout));
                component.AddEventListener(UIEventType.RELEASEALL, new UIEvent.Handler(this.OnUnopenedPackReleaseAll));
                this.UpdatePack(component, boosterStack);
                if (NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>().BoosterStacks.Count < 2)
                {
                    this.ShowHintOnUnopenedPack();
                }
                this.LayoutPacks(false);
                this.UpdateUIEvents();
            }
        }
    }

    private void OnUnopenedPackReleaseAll(UIEvent e)
    {
        if (this.m_draggedPack == null)
        {
            if (!UniversalInputManager.Get().IsTouchMode())
            {
                UIReleaseAllEvent event2 = (UIReleaseAllEvent) e;
                if (event2.GetMouseIsOver())
                {
                    this.HoldPack(e.GetElement() as UnopenedPack);
                }
            }
        }
        else
        {
            this.DropPack();
        }
    }

    private void OnUnopenedPackRollout(UIEvent e)
    {
        this.HideUnopenedPackTooltip();
    }

    private void OnUnopenedPackRollover(UIEvent e)
    {
        if (!m_hasAcknowledgedKoreanWarning && (BattleNet.GetAccountCountry() == "KOR"))
        {
            TooltipZone component = (e.GetElement() as UnopenedPack).GetComponent<TooltipZone>();
            if (component != null)
            {
                component.ShowTooltip(" ", GameStrings.Get("GLUE_PACK_OPENING_TOOLTIP"), 5f, true);
            }
        }
    }

    private void OpenBooster(UnopenedPack pack)
    {
        int id = 1;
        if (!GameUtils.IsFakePackOpeningEnabled())
        {
            id = pack.GetBoosterStack().Id;
            Network.OpenBooster(id);
        }
        this.m_InputBlocker.SetActive(true);
        this.m_director.AddFinishedListener(new PackOpeningDirector.FinishedCallback(this.OnDirectorFinished));
        this.m_director.Play(id);
        this.m_lastOpenedBoosterId = id;
        BnetBar.Get().m_currencyFrame.HideTemporarily();
        if (GameUtils.IsFakePackOpeningEnabled())
        {
            base.StartCoroutine(this.OnFakeBoosterOpened());
        }
        this.m_UnopenedPackScroller.Pause(true);
    }

    [DebuggerHidden]
    private IEnumerator OpenNextPackWhenReady()
    {
        return new <OpenNextPackWhenReady>c__Iterator1AC { <>f__this = this };
    }

    private void PickUpBooster()
    {
        UnopenedPack creatorPack = this.m_draggedPack.GetCreatorPack();
        creatorPack.RemoveBooster();
        NetCache.BoosterStack boosterStack = new NetCache.BoosterStack {
            Id = creatorPack.GetBoosterStack().Id,
            Count = 1
        };
        this.m_draggedPack.SetBoosterStack(boosterStack);
    }

    private void PutBackBooster()
    {
        UnopenedPack creatorPack = this.m_draggedPack.GetCreatorPack();
        this.m_draggedPack.RemoveBooster();
        creatorPack.AddBooster();
    }

    private void Show()
    {
        if (!this.m_shown)
        {
            this.m_shown = true;
            Enum[] args = new Enum[] { PresenceStatus.PACKOPENING };
            PresenceMgr.Get().SetStatus(args);
            if (!Options.Get().GetBool(Option.HAS_SEEN_PACK_OPENING, false))
            {
                NetCache.NetCacheBoosters netObject = NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>();
                if ((netObject != null) && (netObject.GetTotalNumBoosters() > 0))
                {
                    Options.Get().SetBool(Option.HAS_SEEN_PACK_OPENING, true);
                }
            }
            MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_PackOpening);
            this.CreateDirector();
            BnetBar.Get().m_currencyFrame.RefreshContents();
            if (NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>().BoosterStacks.Count < 2)
            {
                this.ShowHintOnUnopenedPack();
            }
            this.UpdateUIEvents();
        }
    }

    private void ShowHintOnSlot()
    {
        if (!Options.Get().GetBool(Option.HAS_OPENED_BOOSTER, false))
        {
            if (this.m_hintArrow == null)
            {
                this.m_hintArrow = NotificationManager.Get().CreateBouncingArrow(false);
                this.FixArrowScale(this.m_draggedPack.transform);
            }
            Bounds bounds = this.m_hintArrow.bounceObject.GetComponent<Renderer>().bounds;
            Vector3 position = this.m_Bones.m_Hint.position;
            position.z += bounds.extents.z;
            this.m_hintArrow.transform.position = position;
        }
    }

    private void ShowHintOnUnopenedPack()
    {
        List<UnopenedPack> list = new List<UnopenedPack>();
        foreach (KeyValuePair<int, UnopenedPack> pair in this.m_unopenedPacks)
        {
            if ((pair.Value != null) && (pair.Value.GetBoosterStack().Count > 0))
            {
                list.Add(pair.Value);
            }
        }
        if ((list.Count >= 1) && (list[0] != null))
        {
            list[0].PlayAlert();
            if (!Options.Get().GetBool(Option.HAS_OPENED_BOOSTER, false))
            {
                if (this.m_hintArrow == null)
                {
                    this.m_hintArrow = NotificationManager.Get().CreateBouncingArrow(false);
                    this.FixArrowScale(list[0].transform);
                }
                Bounds bounds = list[0].GetComponent<Collider>().bounds;
                Bounds bounds2 = this.m_hintArrow.bounceObject.GetComponent<Renderer>().bounds;
                Vector3 center = bounds.center;
                center.z += bounds.extents.z + bounds2.extents.z;
                this.m_hintArrow.transform.position = center;
            }
        }
    }

    private void ShutdownNet()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetDataReceived));
        Network.Get().RemoveNetHandler(BoosterContent.PacketID.ID, new Network.NetHandler(this.OnBoosterOpened));
    }

    private void UnregisterUIEvents()
    {
        this.m_enableBackButton = false;
        this.m_BackButton.SetEnabled(false);
        this.m_StoreButton.SetEnabled(false);
        foreach (KeyValuePair<int, UnopenedPack> pair in this.m_unopenedPacks)
        {
            if (pair.Value != null)
            {
                pair.Value.SetEnabled(false);
            }
        }
    }

    private void Update()
    {
        this.UpdateDraggedPack();
    }

    private void UpdateDraggedPack()
    {
        if (this.m_draggedPack != null)
        {
            RaycastHit hit;
            Vector3 position = this.m_draggedPack.transform.position;
            if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.DragPlane.LayerBit(), out hit))
            {
                position.x = hit.point.x;
                position.z = hit.point.z;
                this.m_draggedPack.transform.position = position;
            }
            if (UniversalInputManager.Get().GetMouseButtonUp(0))
            {
                this.DropPack();
            }
        }
    }

    private void UpdatePack(UnopenedPack pack, NetCache.BoosterStack boosterStack)
    {
        pack.SetBoosterStack(boosterStack);
    }

    private void UpdatePacks()
    {
        NetCache.NetCacheBoosters netObject = NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>();
        if (netObject == null)
        {
            UnityEngine.Debug.LogError(string.Format("PackOpening.UpdatePacks() - boosters are null", new object[0]));
        }
        else
        {
            foreach (NetCache.BoosterStack stack in netObject.BoosterStacks)
            {
                int id = stack.Id;
                if (this.m_unopenedPacks.ContainsKey(id) && (this.m_unopenedPacks[id] != null))
                {
                    if (netObject.GetBoosterStack(id) == null)
                    {
                        UnityEngine.Object.Destroy(this.m_unopenedPacks[id]);
                        this.m_unopenedPacks[id] = null;
                    }
                    else
                    {
                        this.UpdatePack(this.m_unopenedPacks[id], netObject.GetBoosterStack(id));
                    }
                }
                else if ((netObject.GetBoosterStack(id) != null) && (!this.m_unopenedPacksLoading.ContainsKey(id) || !this.m_unopenedPacksLoading[id]))
                {
                    this.m_unopenedPacksLoading[id] = true;
                    string assetName = GameDbf.Booster.GetRecord(id).GetAssetName("PACK_OPENING_PREFAB");
                    if (string.IsNullOrEmpty(assetName))
                    {
                        UnityEngine.Debug.LogError(string.Format("PackOpening.UpdatePacks() - no prefab found for booster {0}!", id));
                    }
                    else
                    {
                        AssetLoader.Get().LoadActor(assetName, new AssetLoader.GameObjectCallback(this.OnUnopenedPackLoaded), stack, false);
                    }
                }
            }
            this.LayoutPacks(false);
        }
    }

    private void UpdateUIEvents()
    {
        if (!this.m_shown)
        {
            this.UnregisterUIEvents();
        }
        else if (this.m_draggedPack != null)
        {
            this.UnregisterUIEvents();
        }
        else
        {
            this.m_enableBackButton = true;
            this.m_BackButton.SetEnabled(true);
            this.m_StoreButton.SetEnabled(true);
            foreach (KeyValuePair<int, UnopenedPack> pair in this.m_unopenedPacks)
            {
                if (pair.Value != null)
                {
                    pair.Value.SetEnabled(true);
                }
            }
        }
    }

    [CompilerGenerated]
    private sealed class <HideAfterNoMorePacks>c__Iterator1AB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PackOpening <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if ((this.<>f__this.m_director == null) || (this.<>f__this.m_director.gameObject == null))
                    {
                        this.<>f__this.Hide();
                        this.$PC = -1;
                        break;
                    }
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 1;
                    return true;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <OnFakeBoosterOpened>c__Iterator1AA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PackOpening <>f__this;
        internal NetCache.BoosterCard <boosterCard>__2;
        internal List<NetCache.BoosterCard> <cards>__1;
        internal float <fakeNetDelay>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<fakeNetDelay>__0 = UnityEngine.Random.Range((float) 0f, (float) 1f);
                    this.$current = new WaitForSeconds(this.<fakeNetDelay>__0);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<cards>__1 = new List<NetCache.BoosterCard>();
                    this.<boosterCard>__2 = new NetCache.BoosterCard();
                    this.<boosterCard>__2.Def.Name = "CS1_042";
                    this.<boosterCard>__2.Def.Premium = TAG_PREMIUM.NORMAL;
                    this.<cards>__1.Add(this.<boosterCard>__2);
                    this.<boosterCard>__2 = new NetCache.BoosterCard();
                    this.<boosterCard>__2.Def.Name = "CS1_129";
                    this.<boosterCard>__2.Def.Premium = TAG_PREMIUM.NORMAL;
                    this.<cards>__1.Add(this.<boosterCard>__2);
                    this.<boosterCard>__2 = new NetCache.BoosterCard();
                    this.<boosterCard>__2.Def.Name = "EX1_050";
                    this.<boosterCard>__2.Def.Premium = TAG_PREMIUM.NORMAL;
                    this.<cards>__1.Add(this.<boosterCard>__2);
                    this.<boosterCard>__2 = new NetCache.BoosterCard();
                    this.<boosterCard>__2.Def.Name = "EX1_105";
                    this.<boosterCard>__2.Def.Premium = TAG_PREMIUM.NORMAL;
                    this.<cards>__1.Add(this.<boosterCard>__2);
                    this.<boosterCard>__2 = new NetCache.BoosterCard();
                    this.<boosterCard>__2.Def.Name = "EX1_350";
                    this.<boosterCard>__2.Def.Premium = TAG_PREMIUM.NORMAL;
                    this.<cards>__1.Add(this.<boosterCard>__2);
                    this.<>f__this.m_director.OnBoosterOpened(this.<cards>__1);
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <OpenNextPackWhenReady>c__Iterator1AC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PackOpening <>f__this;
        internal float <waitTime>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<waitTime>__0 = 0f;
                    if (this.<>f__this.m_director.IsPlaying())
                    {
                        this.<waitTime>__0 = 1f;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    this.<>f__this.m_autoOpenPending = false;
                    this.<>f__this.AutomaticallyOpenPack();
                    this.$PC = -1;
                    goto Label_00B8;

                default:
                    goto Label_00B8;
            }
            if (this.<>f__this.m_director.IsPlaying())
            {
                this.$current = null;
                this.$PC = 1;
            }
            else
            {
                this.$current = new WaitForSeconds(this.<waitTime>__0);
                this.$PC = 2;
            }
            return true;
        Label_00B8:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

