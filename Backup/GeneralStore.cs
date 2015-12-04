using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStore : Store
{
    private static readonly float FLIP_BUY_PANEL_ANIM_TIME = 0.1f;
    [CustomEditField(Sections="General Store")]
    public MeshRenderer m_accentIcon;
    [CustomEditField(Sections="General Store")]
    public GameObject m_buyEmptyPanel;
    [CustomEditField(Sections="General Store")]
    public GameObject m_buyGoldPanel;
    [CustomEditField(Sections="General Store")]
    public GameObject m_buyMoneyPanel;
    private BuyPanelState m_buyPanelState;
    [CustomEditField(Sections="General Store/Text")]
    public GameObject m_chooseArrowContainer;
    [CustomEditField(Sections="General Store/Text")]
    public UberText m_chooseArrowText;
    [CustomEditField(Sections="General Store/Content")]
    public float m_contentFlipAnimationTime = 0.5f;
    [CustomEditField(Sections="General Store/Content")]
    public iTween.EaseType m_contentFlipEaseType = iTween.EaseType.easeOutBounce;
    [CustomEditField(Sections="General Store/Sounds", T=EditType.SOUND_PREFAB)]
    public string m_contentFlipSound;
    private int m_currentContentPositionIdx;
    private GeneralStoreMode m_currentMode;
    [CustomEditField(Sections="General Store/Panes")]
    public GeneralStorePane m_defaultPane;
    [CustomEditField(Sections="General Store/Text")]
    public UberText m_goldCostText;
    [CustomEditField(Sections="General Store/Text")]
    public float m_koreanProductDetailsExtendedHeight = 10.5f;
    [CustomEditField(Sections="General Store/Text")]
    public float m_koreanProductDetailsRegularHeight = 8f;
    [CustomEditField(Sections="General Store/Text")]
    public UberText m_koreanProductDetailsText;
    [CustomEditField(Sections="General Store/Text")]
    public UberText m_koreanWarningText;
    private float m_lastShakeyAmount;
    [CustomEditField(Sections="General Store")]
    public GameObject m_mainPanel;
    [CustomEditField(Sections="General Store/Mode Buttons")]
    public GameObject m_modeButtonBlocker;
    private List<ModeChanged> m_modeChangedListeners = new List<ModeChanged>();
    [CustomEditField(Sections="General Store", ListTable=true)]
    public List<ModeObjects> m_modeObjects = new List<ModeObjects>();
    [CustomEditField(Sections="General Store/Text")]
    public UberText m_moneyCostText;
    [CustomEditField(Sections="General Store/Shake Store")]
    public float m_multipleShakeTolerance = 2.5f;
    [CustomEditField(Sections="General Store/Panes")]
    public UIBScrollable m_paneScrollbar;
    private Map<GeneralStoreMode, Vector3> m_paneStartPositions = new Map<GeneralStoreMode, Vector3>();
    [CustomEditField(Sections="General Store/Panes")]
    public float m_paneSwapAnimationTime = 1f;
    [CustomEditField(Sections="General Store/Panes")]
    public Vector3 m_paneSwapInOffset = new Vector3(0f, -0.05f, 0f);
    [CustomEditField(Sections="General Store/Panes")]
    public Vector3 m_paneSwapOutOffset = new Vector3(0.05f, 0f, 0f);
    private MusicPlaylistType m_prevPlaylist;
    [CustomEditField(Sections="General Store/Text")]
    public MultiSliceElement m_productDetailsContainer;
    [CustomEditField(Sections="General Store/Text")]
    public float m_productDetailsExtendedHeight = 15.5f;
    [CustomEditField(Sections="General Store/Text")]
    public UberText m_productDetailsHeadlineText;
    [CustomEditField(Sections="General Store/Text")]
    public float m_productDetailsRegularHeight = 13f;
    [CustomEditField(Sections="General Store/Text")]
    public UberText m_productDetailsText;
    private int m_settingNewModeCount;
    [CustomEditField(Sections="General Store/Shake Store")]
    public GameObject m_shakeyObject;
    private Vector3 m_shakeyObjectOriginalLocalRotation = Vector3.zero;
    private bool m_staticTextResized;
    private bool m_stillShaking;
    private static readonly Vector3 MAIN_PANEL_ANGLE_TO_ROTATE = new Vector3(0.3333333f, 0f, 0f);
    private static readonly int MIN_GOLD_FOR_CHANGE_QTY_TOOLTIP = 500;
    private static readonly GeneralStoreMode[] s_ContentOrdering = new GeneralStoreMode[] { GeneralStoreMode.ADVENTURE, GeneralStoreMode.CARDS };
    private static readonly Vector3[] s_ContentTriangularPositions = new Vector3[] { new Vector3(0f, 0.125f, 0f), new Vector3(0f, -0.064f, -0.109f), new Vector3(0f, -0.064f, 0.109f) };
    private static readonly Vector3[] s_ContentTriangularRotations = new Vector3[] { new Vector3(-60f, 0f, -180f), new Vector3(0f, -180f, 0f), new Vector3(60f, 0f, 180f) };
    private static GeneralStore s_instance;
    private static readonly Vector3[] s_MainPanelTriangularRotations = new Vector3[] { new Vector3(0f, 0f, 0f), new Vector3(-240f, 0f, 0f), new Vector3(-120f, 0f, 0f) };

    [DebuggerHidden]
    private IEnumerator AnimateAndUpdateStoreMode(GeneralStoreMode oldMode, GeneralStoreMode newMode)
    {
        return new <AnimateAndUpdateStoreMode>c__Iterator1D7 { oldMode = oldMode, newMode = newMode, <$>oldMode = oldMode, <$>newMode = newMode, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator AnimateAndUpdateStorePane(GeneralStoreMode oldMode, GeneralStoreMode newMode)
    {
        return new <AnimateAndUpdateStorePane>c__Iterator1D8 { oldMode = oldMode, newMode = newMode, <$>oldMode = oldMode, <$>newMode = newMode, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator AnimateShakeyObjectCoroutine(float xRotationAmount, float shakeTime, float delay)
    {
        return new <AnimateShakeyObjectCoroutine>c__Iterator1D9 { xRotationAmount = xRotationAmount, delay = delay, shakeTime = shakeTime, <$>xRotationAmount = xRotationAmount, <$>delay = delay, <$>shakeTime = shakeTime, <>f__this = this };
    }

    protected override void Awake()
    {
        s_instance = this;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            base.m_scaleMode = CanvasScaleMode.WIDTH;
        }
        base.Awake();
        if (this.m_shakeyObject != null)
        {
            this.m_shakeyObjectOriginalLocalRotation = this.m_shakeyObject.transform.localEulerAngles;
        }
        base.m_buyWithMoneyButton.SetText(GameStrings.Get("GLUE_STORE_BUY_TEXT"));
        base.m_buyWithGoldButton.SetText(GameStrings.Get("GLUE_STORE_BUY_TEXT"));
        foreach (ModeObjects objects in this.m_modeObjects)
        {
            <Awake>c__AnonStorey32E storeye = new <Awake>c__AnonStorey32E {
                <>f__this = this
            };
            GeneralStoreContent content = objects.m_content;
            UIBButton button = objects.m_button;
            storeye.mode = objects.m_mode;
            GeneralStorePane pane = objects.m_pane;
            if (content != null)
            {
                content.SetParentStore(this);
                content.RegisterCurrentBundleChanged(new GeneralStoreContent.BundleChanged(storeye.<>m__164));
            }
            if (button != null)
            {
                button.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(storeye.<>m__165));
            }
            if (pane != null)
            {
                pane.transform.localPosition = this.m_paneSwapOutOffset;
                this.m_paneStartPositions[storeye.mode] = pane.m_paneContainer.transform.localPosition;
            }
        }
        if (this.m_defaultPane != null)
        {
            this.m_defaultPane.transform.localPosition = this.m_paneSwapOutOffset;
        }
    }

    protected override void BuyWithGold(UIEvent e)
    {
        <BuyWithGold>c__AnonStorey333 storey = new <BuyWithGold>c__AnonStorey333 {
            <>f__this = this
        };
        GeneralStoreContent currentContent = this.GetCurrentContent();
        storey.bundle = currentContent.GetCurrentGoldBundle();
        if (storey.bundle == null)
        {
            UnityEngine.Debug.LogWarning("GeneralStore.OnBuyWithGoldPressed(): SelectedGoldPrice is null");
        }
        else
        {
            GeneralStoreContent.BuyEvent successBuyCB = new GeneralStoreContent.BuyEvent(storey.<>m__16C);
            currentContent.TryBuyWithGold(successBuyCB, successBuyCB);
        }
    }

    protected override void BuyWithMoney(UIEvent e)
    {
        <BuyWithMoney>c__AnonStorey332 storey = new <BuyWithMoney>c__AnonStorey332 {
            <>f__this = this
        };
        GeneralStoreContent currentContent = this.GetCurrentContent();
        storey.bundle = currentContent.GetCurrentMoneyBundle();
        if (storey.bundle == null)
        {
            UnityEngine.Debug.LogWarning("GeneralStore.OnBuyWithMoneyPressed(): SelectedBundle is null");
        }
        else
        {
            GeneralStoreContent.BuyEvent successBuyCB = new GeneralStoreContent.BuyEvent(storey.<>m__16B);
            currentContent.TryBuyWithMoney(successBuyCB, null);
        }
    }

    public override void Close()
    {
        if (base.m_shown && (this.m_settingNewModeCount == 0))
        {
            Navigation.GoBack();
        }
    }

    public void Close(bool closeWithAnimation)
    {
        if (base.m_shown)
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                Navigation.PopUnique(new Navigation.NavigateBackHandler(GeneralStorePhoneCover.OnNavigateBack));
            }
            Navigation.Pop();
            this.CloseImpl(closeWithAnimation);
        }
    }

    private void CloseImpl(bool closeWithAnimation)
    {
        if (this.m_settingNewModeCount <= 0)
        {
            PresenceMgr.Get().SetPrevStatus();
            this.Hide(closeWithAnimation);
            SoundManager.Get().LoadAndPlay("Store_window_shrink", base.gameObject);
            base.EnableFullScreenEffects(false);
            base.FireExitEvent(false);
        }
    }

    private void FireModeChangedEvent(GeneralStoreMode oldMode, GeneralStoreMode newMode)
    {
        foreach (ModeChanged changed in this.m_modeChangedListeners.ToArray())
        {
            changed(oldMode, newMode);
        }
    }

    public static GeneralStore Get()
    {
        return s_instance;
    }

    private GameObject GetBuyPanelObject(BuyPanelState buyPanelState)
    {
        BuyPanelState state = buyPanelState;
        if (state != BuyPanelState.BUY_GOLD)
        {
            if (state == BuyPanelState.BUY_MONEY)
            {
                return this.m_buyMoneyPanel;
            }
            return this.m_buyEmptyPanel;
        }
        return this.m_buyGoldPanel;
    }

    public GeneralStoreContent GetContent(GeneralStoreMode mode)
    {
        <GetContent>c__AnonStorey32F storeyf = new <GetContent>c__AnonStorey32F {
            mode = mode
        };
        ModeObjects objects = this.m_modeObjects.Find(new Predicate<ModeObjects>(storeyf.<>m__166));
        return ((objects == null) ? null : objects.m_content);
    }

    private void GetContentPositionIndex(bool clockwise, out Vector3 contentPosition, out Vector3 contentRotation, out Vector3 lastPanelRotation, out Vector3 newPanelRotation)
    {
        lastPanelRotation = s_MainPanelTriangularRotations[this.m_currentContentPositionIdx];
        if (clockwise)
        {
            this.m_currentContentPositionIdx = (this.m_currentContentPositionIdx + 1) % s_ContentTriangularPositions.Length;
        }
        else
        {
            this.m_currentContentPositionIdx--;
            if (this.m_currentContentPositionIdx < 0)
            {
                this.m_currentContentPositionIdx = s_ContentTriangularPositions.Length - 1;
            }
        }
        contentPosition = s_ContentTriangularPositions[this.m_currentContentPositionIdx];
        contentRotation = s_ContentTriangularRotations[this.m_currentContentPositionIdx];
        newPanelRotation = s_MainPanelTriangularRotations[this.m_currentContentPositionIdx];
    }

    public GeneralStoreContent GetCurrentContent()
    {
        return this.GetContent(this.m_currentMode);
    }

    public GeneralStorePane GetCurrentPane()
    {
        return this.GetPane(this.m_currentMode);
    }

    public GeneralStoreMode GetMode()
    {
        return this.m_currentMode;
    }

    protected override string GetOwnedTooltipString()
    {
        switch (this.m_currentMode)
        {
            case GeneralStoreMode.CARDS:
                return GameStrings.Get("GLUE_STORE_PACK_BUTTON_TEXT_PURCHASED");

            case GeneralStoreMode.ADVENTURE:
                return GameStrings.Get("GLUE_STORE_DUNGEON_BUTTON_TEXT_PURCHASED");

            case GeneralStoreMode.HEROES:
                return GameStrings.Get("GLUE_STORE_HERO_BUTTON_TEXT_PURCHASED");
        }
        return string.Empty;
    }

    public GeneralStorePane GetPane(GeneralStoreMode mode)
    {
        <GetPane>c__AnonStorey330 storey = new <GetPane>c__AnonStorey330 {
            mode = mode
        };
        ModeObjects objects = this.m_modeObjects.Find(new Predicate<ModeObjects>(storey.<>m__167));
        if ((objects != null) && (objects.m_pane != null))
        {
            return objects.m_pane;
        }
        return this.m_defaultPane;
    }

    protected override void Hide(bool animate)
    {
        if (this.m_settingNewModeCount <= 0)
        {
            if (ShownUIMgr.Get() != null)
            {
                ShownUIMgr.Get().ClearShownUI();
            }
            FriendChallengeMgr.Get().OnStoreClosed();
            this.ResumePreviousMusicPlaylist();
            base.DoHideAnimation(!animate, new UIBPopup.OnAnimationComplete(this.OnHidden));
        }
    }

    public void HideAccentTexture()
    {
        if (this.m_accentIcon != null)
        {
            this.m_accentIcon.gameObject.SetActive(false);
        }
    }

    public void HideChooseDescription()
    {
        if (this.m_chooseArrowContainer != null)
        {
            this.m_chooseArrowContainer.SetActive(false);
        }
    }

    public void HideDescription()
    {
        if (this.m_productDetailsContainer != null)
        {
            this.m_productDetailsContainer.gameObject.SetActive(false);
        }
    }

    private bool IsContentFlipClockwise(GeneralStoreMode oldMode, GeneralStoreMode newMode)
    {
        int num = 0;
        int num2 = 0;
        for (int i = 0; i < s_ContentOrdering.Length; i++)
        {
            if (s_ContentOrdering[i] == oldMode)
            {
                num = i;
            }
            else if (s_ContentOrdering[i] == newMode)
            {
                num2 = i;
            }
        }
        return (num < num2);
    }

    public override bool IsReady()
    {
        return true;
    }

    private void OnClosePressed(UIEvent e)
    {
        if (base.m_shown)
        {
            this.Close();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StoreManager.Get().RemoveSuccessfulPurchaseAckListener(new StoreManager.SuccessfulPurchaseAckCallback(this.SuccessfulPurchaseAckEvent));
        StoreManager.Get().RemoveStoreAchievesListener(new StoreManager.StoreAchievesCallback(this.SuccessfulPurchaseEvent));
        this.m_mainPanel = null;
        s_instance = null;
    }

    public override void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
    {
        this.UpdateGoldButtonState(balance);
    }

    public override void OnGoldSpent()
    {
        NetCache.Get().RefreshNetObject<NetCache.NetCacheBoosters>();
        NetCache.Get().RegisterBoosterTallyUpdate(() => this.GetCurrentPane().OnPurchaseFinished());
    }

    protected override void OnHidden()
    {
        base.m_shown = false;
        foreach (ModeObjects objects in this.m_modeObjects)
        {
            GeneralStorePane pane = objects.m_pane;
            GeneralStoreContent content = objects.m_content;
            if (pane != null)
            {
                pane.StoreHidden(this.GetCurrentPane() == pane);
            }
            if (content != null)
            {
                content.StoreHidden(this.GetCurrentContent() == content);
            }
        }
    }

    public override void OnMoneySpent()
    {
        NetCache.Get().RefreshNetObject<NetCache.NetCacheBoosters>();
        NetCache.Get().RegisterBoosterTallyUpdate(() => this.GetCurrentPane().OnPurchaseFinished());
        GeneralStoreContent currentContent = this.GetCurrentContent();
        if (currentContent != null)
        {
            currentContent.Refresh();
        }
        GeneralStorePane currentPane = this.GetCurrentPane();
        if (currentPane != null)
        {
            currentPane.Refresh();
        }
    }

    private bool OnNavigateBack()
    {
        this.CloseImpl(true);
        return true;
    }

    private void OnStopShaking(object obj)
    {
        this.m_stillShaking = false;
    }

    private void PreRender()
    {
        if (!this.m_staticTextResized)
        {
            base.m_buyWithMoneyButton.m_ButtonText.UpdateNow();
            base.m_buyWithGoldButton.m_ButtonText.UpdateNow();
            this.m_staticTextResized = true;
        }
        this.RefreshContent();
    }

    public void RefreshContent()
    {
        GeneralStoreContent currentContent = this.GetCurrentContent();
        GeneralStorePane currentPane = this.GetCurrentPane();
        StoreManager manager = StoreManager.Get();
        base.ActivateCover(manager.TransactionInProgress() || manager.IsPromptShowing());
        if (currentContent != null)
        {
            currentContent.Refresh();
        }
        if (currentPane != null)
        {
            currentPane.Refresh();
        }
    }

    public void RegisterModeChangedListener(ModeChanged dlg)
    {
        this.m_modeChangedListeners.Add(dlg);
    }

    public void ResumePreviousMusicPlaylist()
    {
        if (this.m_prevPlaylist != MusicPlaylistType.Invalid)
        {
            MusicManager.Get().StartPlaylist(this.m_prevPlaylist);
        }
    }

    public void SetAccentTexture(Texture texture)
    {
        if (this.m_accentIcon != null)
        {
            bool flag = texture != null;
            this.m_accentIcon.gameObject.SetActive(flag);
            if (flag)
            {
                this.m_accentIcon.material.mainTexture = texture;
            }
        }
    }

    public void SetChooseDescription(string chooseText)
    {
        this.HideDescription();
        this.SetAccentTexture(null);
        if (this.m_chooseArrowContainer != null)
        {
            this.m_chooseArrowContainer.SetActive(true);
        }
        if (this.m_chooseArrowText != null)
        {
            this.m_chooseArrowText.Text = chooseText;
        }
    }

    public void SetDescription(string title, string desc, string warning = null)
    {
        this.HideChooseDescription();
        if (this.m_productDetailsContainer != null)
        {
            this.m_productDetailsContainer.gameObject.SetActive(true);
        }
        bool flag = StoreManager.Get().IsKoreanCustomer();
        bool flag2 = !string.IsNullOrEmpty(title);
        this.m_productDetailsHeadlineText.gameObject.SetActive(flag2);
        this.m_productDetailsText.gameObject.SetActive(!flag);
        this.m_koreanProductDetailsText.gameObject.SetActive(flag);
        this.m_koreanWarningText.gameObject.SetActive(flag);
        this.m_productDetailsText.Height = !flag2 ? this.m_productDetailsExtendedHeight : this.m_productDetailsRegularHeight;
        this.m_productDetailsHeadlineText.Text = title;
        this.m_koreanProductDetailsText.Text = desc;
        this.m_productDetailsText.Text = desc;
        this.m_koreanProductDetailsText.Height = !flag2 ? this.m_koreanProductDetailsExtendedHeight : this.m_koreanProductDetailsRegularHeight;
        this.m_koreanWarningText.Text = (warning != null) ? warning : string.Empty;
        if (this.m_productDetailsContainer != null)
        {
            this.m_productDetailsContainer.UpdateSlices();
        }
    }

    public void SetMode(GeneralStoreMode mode)
    {
        base.StartCoroutine(this.AnimateAndUpdateStoreMode(this.m_currentMode, mode));
    }

    public void ShakeStore(float xRotationAmount, float shakeTime, float delay = 0f)
    {
        if (this.m_shakeyObject != null)
        {
            base.StartCoroutine(this.AnimateShakeyObjectCoroutine(xRotationAmount, shakeTime, delay));
        }
    }

    private void ShowBuyPanel(BuyPanelState setPanelState)
    {
        <ShowBuyPanel>c__AnonStorey334 storey = new <ShowBuyPanel>c__AnonStorey334();
        if (this.m_buyPanelState != setPanelState)
        {
            GameObject buyPanelObject = this.GetBuyPanelObject(setPanelState);
            storey.oldPanelObject = this.GetBuyPanelObject(this.m_buyPanelState);
            this.m_buyPanelState = setPanelState;
            iTween.StopByName(buyPanelObject, "rotation");
            iTween.StopByName(storey.oldPanelObject, "rotation");
            buyPanelObject.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            storey.oldPanelObject.transform.localEulerAngles = Vector3.zero;
            buyPanelObject.SetActive(true);
            object[] args = new object[] { "rotation", new Vector3(0f, 0f, 180f), "isLocal", true, "time", FLIP_BUY_PANEL_ANIM_TIME, "easeType", iTween.EaseType.linear, "oncomplete", new Action<object>(storey.<>m__16D), "name", "rotation" };
            iTween.RotateTo(storey.oldPanelObject, iTween.Hash(args));
            object[] objArray2 = new object[] { "rotation", new Vector3(0f, 0f, 0f), "isLocal", true, "time", FLIP_BUY_PANEL_ANIM_TIME, "easeType", iTween.EaseType.linear, "name", "rotation" };
            iTween.RotateTo(buyPanelObject, iTween.Hash(objArray2));
            SoundManager.Get().LoadAndPlay((setPanelState != BuyPanelState.BUY_GOLD) ? "gold_spend_plate_flip_off" : "gold_spend_plate_flip_on");
        }
    }

    protected override void ShowImpl(Store.DelOnStoreShown onStoreShownCB, bool isTotallyFake)
    {
        <ShowImpl>c__AnonStorey331 storey = new <ShowImpl>c__AnonStorey331 {
            onStoreShownCB = onStoreShownCB
        };
        if (!base.m_shown)
        {
            this.m_prevPlaylist = MusicManager.Get().GetCurrentPlaylist();
            foreach (ModeObjects objects in this.m_modeObjects)
            {
                GeneralStoreContent content = objects.m_content;
                GeneralStorePane pane = objects.m_pane;
                if (content != null)
                {
                    content.StoreShown(this.GetCurrentContent() == content);
                }
                if (pane != null)
                {
                    pane.StoreShown(this.GetCurrentPane() == pane);
                }
            }
            ShownUIMgr.Get().SetShownUI(ShownUIMgr.UI_WINDOW.GENERAL_STORE);
            FriendChallengeMgr.Get().OnStoreOpened();
            this.PreRender();
            Enum[] args = new Enum[] { PresenceStatus.STORE };
            PresenceMgr.Get().SetStatus(args);
            if (((UniversalInputManager.UsePhoneUI == null) && !Options.Get().GetBool(Option.HAS_SEEN_GOLD_QTY_INSTRUCTION, false)) && (NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>().GetTotal() >= MIN_GOLD_FOR_CHANGE_QTY_TOOLTIP))
            {
                AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                    m_headerText = GameStrings.Get("GLUE_STORE_GOLD_QTY_CHANGE_HEADER")
                };
                if (UniversalInputManager.Get().IsTouchMode())
                {
                    info.m_text = GameStrings.Get("GLUE_STORE_GOLD_QTY_CHANGE_DESC_TOUCH");
                }
                else
                {
                    info.m_text = GameStrings.Get("GLUE_STORE_GOLD_QTY_CHANGE_DESC");
                }
                info.m_showAlertIcon = false;
                info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
                DialogManager.Get().ShowPopup(info);
                Options.Get().SetBool(Option.HAS_SEEN_GOLD_QTY_INSTRUCTION, true);
            }
            this.UpdateGoldButtonState();
            base.m_shown = true;
            Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
            base.EnableFullScreenEffects(true);
            SoundManager.Get().LoadAndPlay("Store_window_expand", base.gameObject);
            base.DoShowAnimation(new UIBPopup.OnAnimationComplete(storey.<>m__16A));
        }
    }

    protected override void Start()
    {
        base.Start();
        StoreManager.Get().RegisterStoreAchievesListener(new StoreManager.StoreAchievesCallback(this.SuccessfulPurchaseEvent));
        StoreManager.Get().RegisterSuccessfulPurchaseAckListener(new StoreManager.SuccessfulPurchaseAckCallback(this.SuccessfulPurchaseAckEvent));
        SoundManager.Get().Load("gold_spend_plate_flip_on");
        SoundManager.Get().Load("gold_spend_plate_flip_off");
        this.UpdateModeButtons(this.m_currentMode);
        foreach (ModeObjects objects in this.m_modeObjects)
        {
            if (objects.m_content != null)
            {
                objects.m_content.gameObject.SetActive(objects.m_mode == this.m_currentMode);
            }
        }
        if (base.m_offClicker != null)
        {
            base.m_offClicker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClosePressed));
        }
    }

    private void SuccessfulPurchaseAckEvent(Network.Bundle bundle, PaymentMethod paymentMethod, object userData)
    {
        if (this.IsShown() && (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE))
        {
            this.Close();
        }
        else
        {
            this.RefreshContent();
        }
    }

    private void SuccessfulPurchaseEvent(Network.Bundle bundle, PaymentMethod paymentMethod, object userData)
    {
        this.RefreshContent();
    }

    public void UnregisterModeChangedListener(ModeChanged dlg)
    {
        this.m_modeChangedListeners.Remove(dlg);
    }

    private void UpdateCostAndButtonState(NoGTAPPTransactionData goldBundle, Network.Bundle moneyBundle)
    {
        if ((moneyBundle != null) && !StoreManager.Get().IsProductAlreadyOwned(moneyBundle))
        {
            this.UpdateCostDisplay(moneyBundle);
            this.UpdateMoneyButtonState();
        }
        else if (goldBundle != null)
        {
            this.UpdateCostDisplay(goldBundle);
            this.UpdateGoldButtonState();
        }
        else
        {
            GeneralStoreContent currentContent = this.GetCurrentContent();
            if ((currentContent == null) || currentContent.IsPurchaseDisabled())
            {
                this.UpdateCostDisplay(BuyPanelState.DISABLED, string.Empty);
            }
            else
            {
                this.UpdateCostDisplay(BuyPanelState.BUY_MONEY, currentContent.GetMoneyDisplayOwnedText());
                this.UpdateMoneyButtonState();
            }
        }
    }

    private void UpdateCostDisplay(Network.Bundle moneyBundle)
    {
        if (moneyBundle == null)
        {
            this.UpdateCostDisplay(BuyPanelState.BUY_MONEY, GameStrings.Get("GLUE_STORE_DUNGEON_BUTTON_COST_OWNED_TEXT"));
        }
        else
        {
            this.UpdateCostDisplay(BuyPanelState.BUY_MONEY, StoreManager.Get().FormatCostBundle(moneyBundle));
        }
    }

    private void UpdateCostDisplay(NoGTAPPTransactionData goldBundle)
    {
        if (goldBundle == null)
        {
            this.UpdateCostDisplay(BuyPanelState.BUY_GOLD, StoreManager.Get().FormatCostText(0.0));
        }
        else
        {
            this.UpdateCostDisplay(BuyPanelState.BUY_GOLD, StoreManager.Get().GetGoldCostNoGTAPP(goldBundle).ToString());
        }
    }

    private void UpdateCostDisplay(BuyPanelState newPanelState, string costText = "")
    {
        if (newPanelState == BuyPanelState.BUY_MONEY)
        {
            this.m_moneyCostText.Text = costText;
            this.m_moneyCostText.UpdateNow();
        }
        else if (newPanelState == BuyPanelState.BUY_GOLD)
        {
            this.m_goldCostText.Text = costText;
            this.m_goldCostText.UpdateNow();
        }
        this.ShowBuyPanel(newPanelState);
    }

    private void UpdateGoldButtonState()
    {
        NetCache.NetCacheGoldBalance netObject = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>();
        this.UpdateGoldButtonState(netObject);
    }

    private void UpdateGoldButtonState(NetCache.NetCacheGoldBalance balance)
    {
        Store.BuyButtonState eNABLED = Store.BuyButtonState.ENABLED;
        GeneralStoreContent currentContent = this.GetCurrentContent();
        if (currentContent != null)
        {
            NoGTAPPTransactionData currentGoldBundle = currentContent.GetCurrentGoldBundle();
            if (currentGoldBundle == null)
            {
                eNABLED = Store.BuyButtonState.DISABLED;
            }
            else
            {
                long goldCostNoGTAPP = StoreManager.Get().GetGoldCostNoGTAPP(currentGoldBundle);
                if (!StoreManager.Get().IsOpen())
                {
                    eNABLED = Store.BuyButtonState.DISABLED;
                }
                else if (!StoreManager.Get().IsBuyWithGoldFeatureEnabled())
                {
                    eNABLED = Store.BuyButtonState.DISABLED_FEATURE;
                }
                else if (balance == null)
                {
                    eNABLED = Store.BuyButtonState.DISABLED;
                }
                else if (balance.GetTotal() < goldCostNoGTAPP)
                {
                    eNABLED = Store.BuyButtonState.DISABLED_NOT_ENOUGH_GOLD;
                }
            }
            base.SetGoldButtonState(eNABLED);
        }
    }

    private void UpdateModeButtons(GeneralStoreMode mode)
    {
        foreach (ModeObjects objects in this.m_modeObjects)
        {
            if (objects.m_button != null)
            {
                UIBHighlight component = objects.m_button.GetComponent<UIBHighlight>();
                if (component != null)
                {
                    if (mode == objects.m_mode)
                    {
                        component.SelectNoSound();
                    }
                    else
                    {
                        component.Reset();
                    }
                }
            }
        }
    }

    private void UpdateMoneyButtonState()
    {
        Store.BuyButtonState eNABLED = Store.BuyButtonState.ENABLED;
        if (!StoreManager.Get().IsOpen())
        {
            eNABLED = Store.BuyButtonState.DISABLED;
        }
        else if (!StoreManager.Get().IsBattlePayFeatureEnabled())
        {
            eNABLED = Store.BuyButtonState.DISABLED_FEATURE;
        }
        else
        {
            Network.Bundle currentMoneyBundle = this.GetCurrentContent().GetCurrentMoneyBundle();
            if ((currentMoneyBundle == null) || StoreManager.Get().IsProductAlreadyOwned(currentMoneyBundle))
            {
                eNABLED = Store.BuyButtonState.DISABLED_OWNED;
            }
        }
        base.SetMoneyButtonState(eNABLED);
    }

    [CompilerGenerated]
    private sealed class <AnimateAndUpdateStoreMode>c__Iterator1D7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GeneralStoreMode <$>newMode;
        internal GeneralStoreMode <$>oldMode;
        internal GeneralStore <>f__this;
        internal bool <clockwise>__2;
        internal Action<object> <completeAnimation>__10;
        internal Vector3 <contentPosition>__3;
        internal Vector3 <contentRotation>__4;
        internal float <direction>__9;
        internal float <flipAnimTime>__8;
        internal Vector3 <lastPanelRotation>__5;
        internal Vector3 <newPanelRotation>__6;
        internal GeneralStoreContent <nextContent>__1;
        internal GeneralStoreContent <prevContent>__0;
        internal bool <rotationDone>__7;
        internal GeneralStoreMode newMode;
        internal GeneralStoreMode oldMode;

        internal void <>m__16E(object o)
        {
            this.<>f__this.m_mainPanel.transform.localEulerAngles = this.<newPanelRotation>__6;
            this.<rotationDone>__7 = true;
            if (this.<prevContent>__0 != null)
            {
                this.<prevContent>__0.gameObject.SetActive(false);
            }
        }

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
                    if (this.<>f__this.m_settingNewModeCount > 0)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_055C;
                    }
                    this.<>f__this.FireModeChangedEvent(this.oldMode, this.newMode);
                    if (this.<>f__this.m_currentMode == this.newMode)
                    {
                        goto Label_055A;
                    }
                    this.<>f__this.m_settingNewModeCount++;
                    if (this.<>f__this.m_modeButtonBlocker != null)
                    {
                        this.<>f__this.m_modeButtonBlocker.SetActive(true);
                    }
                    this.<>f__this.UpdateModeButtons(this.newMode);
                    this.<>f__this.m_currentMode = this.newMode;
                    this.<>f__this.StartCoroutine(this.<>f__this.AnimateAndUpdateStorePane(this.oldMode, this.newMode));
                    this.<prevContent>__0 = this.<>f__this.GetContent(this.oldMode);
                    this.<nextContent>__1 = this.<>f__this.GetContent(this.newMode);
                    if (this.<prevContent>__0 == null)
                    {
                        goto Label_01B9;
                    }
                    this.<prevContent>__0.SetContentActive(false);
                    this.<prevContent>__0.PreStoreFlipOut();
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_01A9;

                case 4:
                    goto Label_0402;

                case 5:
                    goto Label_0457;

                case 6:
                    goto Label_047F;

                case 7:
                    goto Label_0505;

                default:
                    goto Label_055A;
            }
            while (!this.<prevContent>__0.AnimateExitStart())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_055C;
            }
        Label_01A9:
            while (!this.<prevContent>__0.AnimateExitEnd())
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_055C;
            }
        Label_01B9:
            this.<clockwise>__2 = this.<>f__this.IsContentFlipClockwise(this.oldMode, this.newMode);
            this.<>f__this.GetContentPositionIndex(this.<clockwise>__2, out this.<contentPosition>__3, out this.<contentRotation>__4, out this.<lastPanelRotation>__5, out this.<newPanelRotation>__6);
            if (this.<nextContent>__1 != null)
            {
                this.<nextContent>__1.transform.localPosition = this.<contentPosition>__3;
                this.<nextContent>__1.transform.localEulerAngles = this.<contentRotation>__4;
                this.<nextContent>__1.gameObject.SetActive(true);
            }
            iTween.StopByName(this.<>f__this.m_mainPanel, "PANEL_ROTATION");
            this.<>f__this.m_mainPanel.transform.localEulerAngles = this.<lastPanelRotation>__5;
            this.<rotationDone>__7 = false;
            this.<flipAnimTime>__8 = this.<>f__this.m_contentFlipAnimationTime;
            this.<direction>__9 = !this.<clockwise>__2 ? -1f : 1f;
            this.<>f__this.ShakeStore(10f * this.<direction>__9, 1.5f, this.<flipAnimTime>__8 * 0.3f);
            if (!string.IsNullOrEmpty(this.<>f__this.m_contentFlipSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.<>f__this.m_contentFlipSound));
            }
            this.<completeAnimation>__10 = new Action<object>(this.<>m__16E);
            if (this.<flipAnimTime>__8 > 0f)
            {
                object[] args = new object[] { "name", "PANEL_ROTATION", "amount", (Vector3) (GeneralStore.MAIN_PANEL_ANGLE_TO_ROTATE * this.<direction>__9), "time", this.<flipAnimTime>__8, "easetype", this.<>f__this.m_contentFlipEaseType, "oncomplete", this.<completeAnimation>__10 };
                iTween.RotateBy(this.<>f__this.m_mainPanel, iTween.Hash(args));
            }
            else
            {
                this.<completeAnimation>__10(null);
            }
            if (this.<nextContent>__1 != null)
            {
                this.<nextContent>__1.PreStoreFlipIn();
            }
        Label_0402:
            while (!this.<rotationDone>__7)
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_055C;
            }
            if (this.<nextContent>__1 == null)
            {
                goto Label_04B3;
            }
            this.<>f__this.UpdateCostAndButtonState(this.<nextContent>__1.GetCurrentGoldBundle(), this.<nextContent>__1.GetCurrentMoneyBundle());
        Label_0457:
            while (!this.<nextContent>__1.AnimateEntranceStart())
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_055C;
            }
        Label_047F:
            while (!this.<nextContent>__1.AnimateEntranceEnd())
            {
                this.$current = null;
                this.$PC = 6;
                goto Label_055C;
            }
            this.<nextContent>__1.SetContentActive(true);
            this.<nextContent>__1.PostStoreFlipIn(this.<flipAnimTime>__8 > 0f);
        Label_04B3:
            if (this.<prevContent>__0 != null)
            {
                this.<prevContent>__0.PostStoreFlipOut();
            }
            this.<>f__this.m_settingNewModeCount--;
            this.<>f__this.RefreshContent();
        Label_0505:
            while (this.<>f__this.m_settingNewModeCount > 0)
            {
                this.$current = null;
                this.$PC = 7;
                goto Label_055C;
            }
            if (this.<>f__this.m_modeButtonBlocker != null)
            {
                this.<>f__this.m_modeButtonBlocker.SetActive(false);
            }
            if (this.newMode == GeneralStoreMode.NONE)
            {
                this.<>f__this.ResumePreviousMusicPlaylist();
            }
            this.$PC = -1;
        Label_055A:
            return false;
        Label_055C:
            return true;
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
    private sealed class <AnimateAndUpdateStorePane>c__Iterator1D8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GeneralStoreMode <$>newMode;
        internal GeneralStoreMode <$>oldMode;
        internal GeneralStore <>f__this;
        internal float <delayNextPane>__3;
        internal GeneralStorePane <nextPane>__1;
        internal Vector3 <paneStartPos>__4;
        internal GeneralStorePane <prevPane>__0;
        internal int <swapCount>__2;
        internal GeneralStoreMode newMode;
        internal GeneralStoreMode oldMode;

        internal void <>m__16F(object o)
        {
            this.<swapCount>__2--;
        }

        internal void <>m__170(object o)
        {
            this.<swapCount>__2--;
        }

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
                    this.<prevPane>__0 = this.<>f__this.GetPane(this.oldMode);
                    this.<nextPane>__1 = this.<>f__this.GetPane(this.newMode);
                    if (this.oldMode != this.newMode)
                    {
                        this.<>f__this.m_settingNewModeCount++;
                        if (this.<>f__this.m_paneScrollbar != null)
                        {
                            this.<>f__this.m_paneScrollbar.SaveScroll("STORE_MODE_" + this.oldMode);
                            this.<>f__this.m_paneScrollbar.m_ScrollObject = null;
                        }
                        if (this.<prevPane>__0 == null)
                        {
                            goto Label_014B;
                        }
                        this.<prevPane>__0.PrePaneSwappedOut();
                        break;
                    }
                    goto Label_04A3;

                case 1:
                    break;

                case 2:
                    goto Label_0130;

                case 3:
                    goto Label_0321;

                case 4:
                    goto Label_0446;

                case 5:
                    goto Label_046E;

                default:
                    goto Label_04A3;
            }
            while (!this.<prevPane>__0.AnimateExitStart())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_04A5;
            }
        Label_0130:
            while (!this.<prevPane>__0.AnimateExitEnd())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_04A5;
            }
            this.<prevPane>__0.PostPaneSwappedOut();
        Label_014B:
            if (this.<>f__this.m_paneSwapAnimationTime <= 0f)
            {
                this.<prevPane>__0.transform.localPosition = this.<>f__this.m_paneSwapOutOffset;
                this.<nextPane>__1.transform.localPosition = Vector3.zero;
                goto Label_0362;
            }
            this.<swapCount>__2 = 0;
            this.<delayNextPane>__3 = 0f;
            if (this.<prevPane>__0 != null)
            {
                this.<swapCount>__2++;
                this.<prevPane>__0.transform.localPosition = Vector3.zero;
                object[] args = new object[] { "position", this.<>f__this.m_paneSwapOutOffset, "islocal", true, "time", this.<>f__this.m_paneSwapAnimationTime, "easetype", iTween.EaseType.linear, "oncomplete", new Action<object>(this.<>m__16F) };
                iTween.MoveTo(this.<prevPane>__0.gameObject, iTween.Hash(args));
                this.<delayNextPane>__3 = this.<>f__this.m_paneSwapAnimationTime;
            }
            if (this.<nextPane>__1 != null)
            {
                this.<swapCount>__2++;
                this.<nextPane>__1.transform.localPosition = this.<>f__this.m_paneSwapInOffset;
                object[] objArray2 = new object[] { "position", Vector3.zero, "islocal", true, "time", this.<>f__this.m_paneSwapAnimationTime, "delay", this.<delayNextPane>__3, "oncomplete", new Action<object>(this.<>m__170) };
                iTween.MoveTo(this.<nextPane>__1.gameObject, iTween.Hash(objArray2));
            }
        Label_0321:
            while (this.<swapCount>__2 > 0)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_04A5;
            }
        Label_0362:
            if ((this.<>f__this.m_paneScrollbar != null) && (this.<nextPane>__1.m_paneContainer != null))
            {
                this.<>f__this.m_paneStartPositions.TryGetValue(this.newMode, out this.<paneStartPos>__4);
                this.<>f__this.m_paneScrollbar.m_ScrollObject = this.<nextPane>__1.m_paneContainer;
                this.<>f__this.m_paneScrollbar.ResetScrollStartPosition(this.<paneStartPos>__4);
                this.<>f__this.m_paneScrollbar.LoadScroll("STORE_MODE_" + this.newMode);
                this.<>f__this.m_paneScrollbar.EnableIfNeeded();
            }
            if (this.<nextPane>__1 == null)
            {
                goto Label_0489;
            }
            this.<nextPane>__1.PrePaneSwappedIn();
        Label_0446:
            while (!this.<nextPane>__1.AnimateEntranceStart())
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_04A5;
            }
        Label_046E:
            while (!this.<nextPane>__1.AnimateEntranceEnd())
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_04A5;
            }
            this.<nextPane>__1.PostPaneSwappedIn();
        Label_0489:
            this.<>f__this.m_settingNewModeCount--;
            this.$PC = -1;
        Label_04A3:
            return false;
        Label_04A5:
            return true;
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
    private sealed class <AnimateShakeyObjectCoroutine>c__Iterator1D9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal float <$>shakeTime;
        internal float <$>xRotationAmount;
        internal GeneralStore <>f__this;
        internal float <absRotation>__0;
        internal float delay;
        internal float shakeTime;
        internal float xRotationAmount;

        internal void <>m__171(object o)
        {
            this.<>f__this.m_shakeyObject.transform.localEulerAngles = this.<>f__this.m_shakeyObjectOriginalLocalRotation;
            this.<>f__this.m_lastShakeyAmount = 0f;
        }

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
                    this.<absRotation>__0 = Mathf.Abs(this.xRotationAmount);
                    if (((this.<absRotation>__0 - this.<>f__this.m_lastShakeyAmount) >= this.<>f__this.m_multipleShakeTolerance) || !this.<>f__this.m_stillShaking)
                    {
                        if (this.delay > 0f)
                        {
                            this.$current = new WaitForSeconds(this.delay);
                            this.$PC = 1;
                            return true;
                        }
                        break;
                    }
                    goto Label_01A4;

                case 1:
                    break;

                default:
                    goto Label_01A4;
            }
            this.<>f__this.m_lastShakeyAmount = this.<absRotation>__0;
            this.<>f__this.m_stillShaking = true;
            ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.<>f__this.OnStopShaking), null);
            ApplicationMgr.Get().ScheduleCallback(this.shakeTime * 0.25f, false, new ApplicationMgr.ScheduledCallback(this.<>f__this.OnStopShaking), null);
            iTween.Stop(this.<>f__this.m_shakeyObject);
            this.<>f__this.m_shakeyObject.transform.localEulerAngles = this.<>f__this.m_shakeyObjectOriginalLocalRotation;
            object[] args = new object[] { "x", this.xRotationAmount, "time", this.shakeTime, "delay", 0.001f, "oncomplete", new Action<object>(this.<>m__171) };
            iTween.PunchRotation(this.<>f__this.m_shakeyObject, iTween.Hash(args));
            this.$PC = -1;
        Label_01A4:
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
    private sealed class <Awake>c__AnonStorey32E
    {
        internal GeneralStore <>f__this;
        internal GeneralStoreMode mode;

        internal void <>m__164(NoGTAPPTransactionData goldBundle, Network.Bundle moneyBundle)
        {
            this.<>f__this.UpdateCostAndButtonState(goldBundle, moneyBundle);
        }

        internal void <>m__165(UIEvent e)
        {
            this.<>f__this.SetMode(this.mode);
        }
    }

    [CompilerGenerated]
    private sealed class <BuyWithGold>c__AnonStorey333
    {
        internal GeneralStore <>f__this;
        internal NoGTAPPTransactionData bundle;

        internal void <>m__16C()
        {
            this.<>f__this.FireBuyWithGoldEventNoGTAPP(this.bundle);
        }
    }

    [CompilerGenerated]
    private sealed class <BuyWithMoney>c__AnonStorey332
    {
        internal GeneralStore <>f__this;
        internal Network.Bundle bundle;

        internal void <>m__16B()
        {
            this.<>f__this.FireBuyWithMoneyEvent(this.bundle.ProductID, 1);
        }
    }

    [CompilerGenerated]
    private sealed class <GetContent>c__AnonStorey32F
    {
        internal GeneralStoreMode mode;

        internal bool <>m__166(GeneralStore.ModeObjects obj)
        {
            return (obj.m_mode == this.mode);
        }
    }

    [CompilerGenerated]
    private sealed class <GetPane>c__AnonStorey330
    {
        internal GeneralStoreMode mode;

        internal bool <>m__167(GeneralStore.ModeObjects obj)
        {
            return (obj.m_mode == this.mode);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowBuyPanel>c__AnonStorey334
    {
        internal GameObject oldPanelObject;

        internal void <>m__16D(object o)
        {
            this.oldPanelObject.SetActive(false);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowImpl>c__AnonStorey331
    {
        internal Store.DelOnStoreShown onStoreShownCB;

        internal void <>m__16A()
        {
            if (this.onStoreShownCB != null)
            {
                this.onStoreShownCB();
            }
        }
    }

    private enum BuyPanelState
    {
        DISABLED,
        BUY_GOLD,
        BUY_MONEY
    }

    public delegate void ModeChanged(GeneralStoreMode oldMode, GeneralStoreMode newMode);

    [Serializable]
    public class ModeObjects
    {
        public UIBButton m_button;
        public GeneralStoreContent m_content;
        public GeneralStoreMode m_mode;
        public GeneralStorePane m_pane;
    }
}

