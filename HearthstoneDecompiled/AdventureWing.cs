using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureWing : MonoBehaviour
{
    [CustomEditField(Sections="Random Background Properties")]
    public List<float> m_BackgroundOffsets = new List<float>();
    [CustomEditField(Sections="Random Background Properties", ListTable=true)]
    public List<BackgroundRandomization> m_BackgroundRenderers = new List<BackgroundRandomization>();
    [CustomEditField(Sections="Wing Rewards")]
    public PegUIElement m_BigChest;
    private List<Boss> m_BossCoins = new List<Boss>();
    private List<BossSelected> m_BossSelectedListeners = new List<BossSelected>();
    private BringToFocusCallback m_BringToFocusCallback;
    [CustomEditField(Sections="UI")]
    public PegUIElement m_BuyButton;
    [CustomEditField(Sections="UI")]
    public MeshRenderer m_BuyButtonMesh;
    [CustomEditField(Sections="UI")]
    public UberText m_BuyButtonText;
    [CustomEditField(Sections="Containers & Bones")]
    public GameObject m_CoinContainer;
    [SerializeField]
    private Vector3 m_CoinsChestOffset = Vector3.zero;
    [SerializeField]
    private Vector3 m_CoinsOffset = Vector3.zero;
    [SerializeField]
    private float m_CoinSpacing = 25f;
    [CustomEditField(Sections="Containers & Bones")]
    public GameObject m_ContentsContainer;
    private AdventureWingDef m_DependsOnWingDef;
    private bool m_EventStartDetected;
    private List<HideCardRewards> m_HideCardRewardsListeners = new List<HideCardRewards>();
    [CustomEditField(Sections="Lock Plate")]
    public GameObject m_LockPlate;
    [CustomEditField(Sections="Lock Plate")]
    public GameObject m_LockPlateFXContainer;
    private List<OpenPlateEnd> m_OpenPlateEndListeners = new List<OpenPlateEnd>();
    private List<OpenPlateStart> m_OpenPlateStartListeners = new List<OpenPlateStart>();
    private bool m_Owned;
    [CustomEditField(Sections="Containers & Bones")]
    public GameObject m_PlateAccentContainer;
    private GameObject m_PlateAccentObject;
    private bool m_Playable;
    [CustomEditField(Sections="UI")]
    public GameObject m_PurchasedBanner;
    [CustomEditField(Sections="UI")]
    public UberText m_ReleaseLabelText;
    [CustomEditField(Sections="UI")]
    public PegUIElement m_RewardsPreviewButton;
    private List<ShowCardRewards> m_ShowCardRewardsListeners = new List<ShowCardRewards>();
    private List<ShowRewardsPreview> m_ShowRewardsPreviewListeners = new List<ShowRewardsPreview>();
    private List<TryPurchaseWing> m_TryPurchaseWingListeners = new List<TryPurchaseWing>();
    [CustomEditField(Sections="UI")]
    public PegUIElement m_UnlockButton;
    [CustomEditField(Sections="Special UI/LOE")]
    public float m_UnlockButtonHighlightIntensityOut = 1.52f;
    [CustomEditField(Sections="Special UI/LOE")]
    public float m_UnlockButtonHighlightIntensityOver = 2f;
    [CustomEditField(Sections="Special UI/LOE")]
    public MeshRenderer m_UnlockButtonHighlightMesh_LOE;
    private Spell m_UnlockSpell;
    [CustomEditField(Sections="Containers & Bones")]
    public GameObject m_WallAccentContainer;
    private GameObject m_WallAccentObject;
    private AdventureWingDef m_WingDef;
    [CustomEditField(Sections="Wing Event Table")]
    public AdventureWingEventTable m_WingEventTable;
    [CustomEditField(Sections="UI")]
    public List<UberText> m_WingTitles = new List<UberText>();
    private static List<int> s_LastRandomNumbers = new List<int>();

    public void AddBossSelectedListener(BossSelected dlg)
    {
        this.m_BossSelectedListeners.Add(dlg);
    }

    public void AddHideCardRewardsListener(HideCardRewards dlg)
    {
        this.m_HideCardRewardsListeners.Add(dlg);
    }

    public void AddOpenPlateEndListener(OpenPlateEnd dlg)
    {
        this.m_OpenPlateEndListeners.Add(dlg);
    }

    public void AddOpenPlateStartListener(OpenPlateStart dlg)
    {
        this.m_OpenPlateStartListeners.Add(dlg);
    }

    public void AddShowCardRewardsListener(ShowCardRewards dlg)
    {
        this.m_ShowCardRewardsListeners.Add(dlg);
    }

    public void AddShowRewardsPreviewListeners(ShowRewardsPreview dlg)
    {
        this.m_ShowRewardsPreviewListeners.Add(dlg);
    }

    public void AddTryPurchaseWingListener(TryPurchaseWing dlg)
    {
        this.m_TryPurchaseWingListeners.Add(dlg);
    }

    [DebuggerHidden]
    private IEnumerator AnimateCoinsAndChests(List<Boss> thingsToFlip, float delaySeconds, DelOnCoinAnimateCallback dlg)
    {
        return new <AnimateCoinsAndChests>c__IteratorD { delaySeconds = delaySeconds, dlg = dlg, thingsToFlip = thingsToFlip, <$>delaySeconds = delaySeconds, <$>dlg = dlg, <$>thingsToFlip = thingsToFlip, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator AnimateOneCoinAndChest(Boss boss)
    {
        return new <AnimateOneCoinAndChest>c__IteratorE { boss = boss, <$>boss = boss };
    }

    private void Awake()
    {
        if (this.m_BigChest != null)
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.m_BigChest.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ShowBigChestRewards));
            }
            else
            {
                this.m_BigChest.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.ShowBigChestRewards));
                this.m_BigChest.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.HideBigChestRewards));
            }
        }
    }

    public void BigChestStayOpen()
    {
        this.m_WingEventTable.BigChestStayOpen();
    }

    public AdventureBossCoin CreateBoss(string coinPrefab, string rewardsPrefab, ScenarioDbId mission, bool enabled)
    {
        <CreateBoss>c__AnonStorey2A7 storeya = new <CreateBoss>c__AnonStorey2A7 {
            mission = mission,
            <>f__this = this,
            newcoin = GameUtils.LoadGameObjectWithComponent<AdventureBossCoin>(coinPrefab),
            newchest = GameUtils.LoadGameObjectWithComponent<AdventureRewardsChest>(rewardsPrefab)
        };
        storeya.newcoin.gameObject.name = string.Format("AdventureBossCoin({0})", (int) storeya.mission);
        if (storeya.newchest != null)
        {
            storeya.newchest.gameObject.name = string.Format("AdventureRewardsChest({0})", (int) storeya.mission);
            this.UpdateBossChest(storeya.newchest, storeya.mission);
        }
        if (this.m_CoinContainer != null)
        {
            GameUtils.SetParent(storeya.newcoin, this.m_CoinContainer, false);
            if (storeya.newchest != null)
            {
                GameUtils.SetParent(storeya.newchest, this.m_CoinContainer, false);
                TransformUtil.SetLocalPosY(storeya.newchest.transform, 0.01f);
            }
        }
        storeya.newcoin.Enable(enabled, false);
        storeya.newcoin.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storeya.<>m__38));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            storeya.newchest.Enable(false);
            if (storeya.newcoin.m_DisabledCollider != null)
            {
                storeya.newcoin.m_DisabledCollider.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(storeya.<>m__39));
            }
        }
        else
        {
            storeya.newchest.AddChestEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(storeya.<>m__3A));
            storeya.newchest.AddChestEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(storeya.<>m__3B));
        }
        if (this.m_BossCoins.Count == 0)
        {
            storeya.newcoin.ShowConnector(false);
        }
        Boss item = new Boss {
            m_Mission = storeya.mission,
            m_Coin = storeya.newcoin,
            m_Chest = storeya.newchest
        };
        this.m_BossCoins.Add(item);
        this.UpdateCoinPositions();
        return storeya.newcoin;
    }

    [DebuggerHidden]
    private IEnumerator DoUnlockPlate(float startDelay)
    {
        return new <DoUnlockPlate>c__IteratorF { startDelay = startDelay, <$>startDelay = startDelay, <>f__this = this };
    }

    private void FireBossSelectedEvent(AdventureBossCoin coin, ScenarioDbId mission)
    {
        foreach (BossSelected selected in this.m_BossSelectedListeners.ToArray())
        {
            selected(coin, mission);
        }
    }

    private void FireHideCardRewardsEvent(List<CardRewardData> rewards)
    {
        foreach (HideCardRewards rewards2 in this.m_HideCardRewardsListeners.ToArray())
        {
            rewards2(rewards);
        }
    }

    private void FireOpenPlateEndEvent(Spell s)
    {
        if (this.m_UnlockSpell != null)
        {
            this.m_UnlockSpell.gameObject.SetActive(false);
        }
        foreach (OpenPlateEnd end in this.m_OpenPlateEndListeners.ToArray())
        {
            end(this);
        }
    }

    private void FireOpenPlateStartEvent()
    {
        foreach (OpenPlateStart start in this.m_OpenPlateStartListeners.ToArray())
        {
            start(this);
        }
    }

    private void FireShowCardRewardsEvent(List<CardRewardData> rewards, Vector3 origin)
    {
        foreach (ShowCardRewards rewards2 in this.m_ShowCardRewardsListeners.ToArray())
        {
            rewards2(rewards, origin);
        }
    }

    private void FireShowRewardsPreviewEvent()
    {
        foreach (ShowRewardsPreview preview in this.m_ShowRewardsPreviewListeners.ToArray())
        {
            preview();
        }
    }

    private void FireTryPurchaseWingEvent()
    {
        foreach (TryPurchaseWing wing in this.m_TryPurchaseWingListeners.ToArray())
        {
            wing();
        }
    }

    public AdventureDbId GetAdventureId()
    {
        return this.m_WingDef.GetAdventureId();
    }

    public List<CardRewardData> GetBigChestRewards()
    {
        return ((this.m_BigChest == null) ? null : ((List<CardRewardData>) this.m_BigChest.GetData()));
    }

    public List<AdventureRewardsChest> GetChests()
    {
        List<AdventureRewardsChest> list = new List<AdventureRewardsChest>();
        foreach (Boss boss in this.m_BossCoins)
        {
            list.Add(boss.m_Chest);
        }
        return list;
    }

    public int GetProductData()
    {
        return (int) this.GetWingId();
    }

    public ProductType GetProductType()
    {
        return StoreManager.GetAdventureProductType(this.GetAdventureId());
    }

    public AdventureWingDef GetWingDef()
    {
        return this.m_WingDef;
    }

    public WingDbId GetWingId()
    {
        return this.m_WingDef.GetWingId();
    }

    public string GetWingName()
    {
        return this.m_WingDef.GetWingName();
    }

    public bool HasBigChestRewards()
    {
        return ((this.m_BigChest != null) && (this.m_BigChest.GetData() != null));
    }

    public bool HasRewards()
    {
        List<CardRewardData> bigChestRewards = this.GetBigChestRewards();
        if ((bigChestRewards != null) && (bigChestRewards.Count > 0))
        {
            return true;
        }
        foreach (Boss boss in this.m_BossCoins)
        {
            if (AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario((int) boss.m_Mission).Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void HideBigChest()
    {
        this.m_WingEventTable.BigChestCover();
    }

    private void HideBigChestRewards(UIEvent e)
    {
        List<CardRewardData> bigChestRewards = this.GetBigChestRewards();
        if (bigChestRewards != null)
        {
            this.FireHideCardRewardsEvent(bigChestRewards);
        }
    }

    private void HideBossRewards(ScenarioDbId mission)
    {
        List<CardRewardData> immediateCardRewardsForDefeatingScenario = AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario((int) mission);
        this.FireHideCardRewardsEvent(immediateCardRewardsForDefeatingScenario);
    }

    public void Initialize(AdventureWingDef wingDef, AdventureWingDef dependsOnWingDef)
    {
        int num;
        this.m_WingDef = wingDef;
        this.m_DependsOnWingDef = dependsOnWingDef;
        base.gameObject.name = string.Format("AdventureWing({0})", wingDef.GetWingId());
        foreach (UberText text in this.m_WingTitles)
        {
            if (text != null)
            {
                text.Text = this.m_WingDef.GetWingName();
            }
        }
        if (!string.IsNullOrEmpty(wingDef.m_UnlockSpellPrefab) && (this.m_LockPlateFXContainer != null))
        {
            this.m_UnlockSpell = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(wingDef.m_UnlockSpellPrefab), true, false).GetComponent<Spell>();
            GameUtils.SetParent(this.m_UnlockSpell, this.m_LockPlateFXContainer, false);
            this.m_UnlockSpell.gameObject.SetActive(false);
        }
        this.SetAccent(wingDef.m_AccentPrefab);
        this.m_Owned = AdventureProgressMgr.Get().OwnsWing((int) wingDef.GetWingId());
        this.m_Playable = this.m_Owned && AdventureProgressMgr.Get().IsWingOpen((int) wingDef.GetWingId());
        this.UpdatePurchasedBanner();
        bool flag = AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.HEROIC;
        AdventureProgressMgr.Get().GetWingAck((int) this.m_WingDef.GetWingId(), out num);
        bool flag2 = num > 0;
        if (!AdventureScene.Get().IsDevMode)
        {
            if (this.m_Playable && (flag2 || flag))
            {
                this.m_WingEventTable.PlateDeactivate();
            }
            else
            {
                bool flag3 = AdventureProgressMgr.Get().IsWingLocked((int) wingDef.GetWingId());
                this.UpdateBuyButton(StoreManager.Get().IsOpen(), null);
                StoreManager.Get().RegisterStatusChangedListener(new StoreManager.StatusChangedCallback(this.UpdateBuyButton));
                this.m_WingEventTable.PlateActivate();
                if (!flag3)
                {
                    bool flag4 = (dependsOnWingDef == null) || AdventureProgressMgr.Get().OwnsWing((int) dependsOnWingDef.GetWingId());
                    this.m_EventStartDetected = AdventureProgressMgr.Get().IsWingOpen((int) this.m_WingDef.GetWingId());
                    if (!this.m_EventStartDetected)
                    {
                        if (this.m_ReleaseLabelText != null)
                        {
                            this.m_ReleaseLabelText.Text = this.m_WingDef.GetComingSoonLabel();
                        }
                    }
                    else if (!this.m_Owned && flag4)
                    {
                        this.m_WingEventTable.PlateBuy(true);
                    }
                    else if (this.m_Owned && (num == 0))
                    {
                        this.m_WingEventTable.PlateKey(true);
                    }
                    else if (this.m_ReleaseLabelText != null)
                    {
                        this.m_ReleaseLabelText.Text = this.m_WingDef.GetRequiresLabel();
                    }
                }
                else if (this.m_ReleaseLabelText != null)
                {
                    this.m_ReleaseLabelText.Text = !this.m_Owned ? GameStrings.Get(this.m_WingDef.m_LockedPurchaseLocString) : GameStrings.Get(this.m_WingDef.m_LockedLocString);
                }
            }
        }
        else
        {
            int devModeSetting = AdventureScene.Get().DevModeSetting;
            this.m_WingEventTable.PlateActivate();
            switch (devModeSetting)
            {
                case 1:
                    this.m_WingEventTable.PlateKey(true);
                    break;

                case 2:
                    this.m_WingEventTable.PlateInitialText();
                    break;
            }
            if (this.m_ReleaseLabelText != null)
            {
                this.m_ReleaseLabelText.Text = this.m_WingDef.GetComingSoonLabel();
            }
        }
        this.m_UnlockButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.UnlockPlate));
        this.m_UnlockButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnUnlockButtonOut));
        this.m_UnlockButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnUnlockButtonOver));
        this.m_BuyButton.AddEventListener(UIEventType.RELEASE, delegate (UIEvent e) {
            if (AdventureScene.Get().IsDevMode)
            {
                this.m_WingEventTable.PlateKey(false);
            }
            else
            {
                this.FireTryPurchaseWingEvent();
            }
        });
        if (this.m_RewardsPreviewButton != null)
        {
            this.m_RewardsPreviewButton.AddEventListener(UIEventType.RELEASE, e => this.FireShowRewardsPreviewEvent());
        }
    }

    private void OnDestroy()
    {
        if (StoreManager.Get() != null)
        {
            StoreManager.Get().RemoveStatusChangedListener(new StoreManager.StatusChangedCallback(this.UpdateBuyButton));
        }
    }

    private void OnUnlockButtonOut(UIEvent e)
    {
        if (this.m_UnlockButtonHighlightMesh_LOE != null)
        {
            this.m_UnlockButtonHighlightMesh_LOE.material.SetFloat("_Intensity", this.m_UnlockButtonHighlightIntensityOut);
        }
    }

    private void OnUnlockButtonOver(UIEvent e)
    {
        if (this.m_UnlockButtonHighlightMesh_LOE != null)
        {
            this.m_UnlockButtonHighlightMesh_LOE.material.SetFloat("_Intensity", this.m_UnlockButtonHighlightIntensityOver);
        }
    }

    private void OnUnlockSpellFinished(Spell spell, object userData)
    {
        Vector3 position = (UniversalInputManager.UsePhoneUI == null) ? NotificationManager.ALT_ADVENTURE_SCREEN_POS : NotificationManager.PHONE_CHARACTER_POS;
        if ((this.m_WingDef != null) && !string.IsNullOrEmpty(this.m_WingDef.m_OpenQuoteVOLine))
        {
            string openQuotePrefab = this.m_WingDef.m_OpenQuotePrefab;
            if (string.IsNullOrEmpty(openQuotePrefab))
            {
                openQuotePrefab = AdventureScene.Get().GetAdventureDef(this.GetAdventureId()).m_DefaultQuotePrefab;
            }
            bool allowRepeatDuringSession = (AdventureScene.Get() != null) && AdventureScene.Get().IsDevMode;
            NotificationManager.Get().CreateCharacterQuote(FileUtils.GameAssetPathToName(openQuotePrefab), position, GameStrings.Get(this.m_WingDef.m_OpenQuoteVOLine), this.m_WingDef.m_OpenQuoteVOLine, allowRepeatDuringSession, 0f, null, CanvasAnchor.BOTTOM_LEFT);
        }
    }

    public void OpenBigChest()
    {
        this.m_WingEventTable.BigChestOpen();
        if (this.m_BigChest != null)
        {
            this.m_BigChest.RemoveEventListener(UIEventType.PRESS, new UIEvent.Handler(this.ShowBigChestRewards));
            this.m_BigChest.RemoveEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.ShowBigChestRewards));
            this.m_BigChest.RemoveEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.HideBigChestRewards));
        }
    }

    public void RandomizeBackground()
    {
        if (this.m_BackgroundOffsets.Count != 0)
        {
            int num;
            do
            {
                num = UnityEngine.Random.Range(0, this.m_BackgroundOffsets.Count);
            }
            while (s_LastRandomNumbers.Contains(num));
            s_LastRandomNumbers.Add(num);
            if (s_LastRandomNumbers.Count >= this.m_BackgroundOffsets.Count)
            {
                s_LastRandomNumbers.RemoveAt(0);
            }
            foreach (BackgroundRandomization randomization in this.m_BackgroundRenderers)
            {
                if ((randomization.m_backgroundRenderer != null) && !string.IsNullOrEmpty(randomization.m_materialTextureName))
                {
                    Material material = randomization.m_backgroundRenderer.materials[0];
                    Vector2 textureOffset = material.GetTextureOffset(randomization.m_materialTextureName);
                    textureOffset.y = this.m_BackgroundOffsets[num];
                    material.SetTextureOffset(randomization.m_materialTextureName, textureOffset);
                }
            }
        }
    }

    public void SetAccent(string accentPrefab)
    {
        if (this.m_WallAccentObject != null)
        {
            UnityEngine.Object.Destroy(this.m_WallAccentObject);
        }
        if (this.m_PlateAccentObject != null)
        {
            UnityEngine.Object.Destroy(this.m_PlateAccentObject);
        }
        if (!string.IsNullOrEmpty(accentPrefab))
        {
            if (this.m_WallAccentContainer != null)
            {
                this.m_WallAccentObject = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(accentPrefab), true, false);
                GameUtils.SetParent(this.m_WallAccentObject, this.m_WallAccentContainer, false);
            }
            if (this.m_PlateAccentContainer != null)
            {
                this.m_PlateAccentObject = UnityEngine.Object.Instantiate<GameObject>(this.m_WallAccentObject);
                GameUtils.SetParent(this.m_PlateAccentObject, this.m_PlateAccentContainer, false);
            }
        }
    }

    public void SetBigChestRewards(WingDbId wingId)
    {
        if (AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.NORMAL)
        {
            HashSet<RewardVisualTiming> rewardTimings = new HashSet<RewardVisualTiming> {
                RewardVisualTiming.ADVENTURE_CHEST
            };
            List<CardRewardData> cardRewardsForWing = AdventureProgressMgr.Get().GetCardRewardsForWing((int) wingId, rewardTimings);
            if (this.m_BigChest != null)
            {
                this.m_BigChest.SetData(cardRewardsForWing);
            }
        }
    }

    public void SetBringToFocusCallback(BringToFocusCallback dlg)
    {
        this.m_BringToFocusCallback = dlg;
    }

    private void ShowBigChestRewards(UIEvent e)
    {
        List<CardRewardData> bigChestRewards = this.GetBigChestRewards();
        if (bigChestRewards != null)
        {
            this.FireShowCardRewardsEvent(bigChestRewards, this.m_BigChest.transform.position);
        }
    }

    private void ShowBossRewards(ScenarioDbId mission, Vector3 origin)
    {
        List<CardRewardData> immediateCardRewardsForDefeatingScenario = AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario((int) mission);
        this.FireShowCardRewardsEvent(immediateCardRewardsForDefeatingScenario, origin);
    }

    private void UnlockPlate(UIEvent e)
    {
        this.m_UnlockButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.UnlockPlate));
        float time = 0f;
        if (this.m_BringToFocusCallback != null)
        {
            time = 0.5f;
            this.m_BringToFocusCallback(this.m_UnlockButton.transform.position, time);
        }
        base.StartCoroutine(this.DoUnlockPlate(time));
    }

    private void Update()
    {
        if ((!this.m_ContentsContainer.activeSelf && !this.m_EventStartDetected) && !AdventureScene.Get().IsDevMode)
        {
            bool flag = AdventureProgressMgr.Get().OwnsWing((int) this.m_WingDef.GetWingId());
            if (AdventureProgressMgr.Get().IsWingOpen((int) this.m_WingDef.GetWingId()) && !flag)
            {
                this.UpdatePlateState();
            }
        }
        if (AdventureScene.Get().IsDevMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                this.m_ContentsContainer.SetActive(false);
                this.m_LockPlate.SetActive(true);
                this.m_WingEventTable.PlateKey(true);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                this.m_ContentsContainer.SetActive(false);
                this.m_LockPlate.SetActive(true);
                this.m_WingEventTable.PlateBuy(true);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                this.m_ContentsContainer.SetActive(false);
                this.m_LockPlate.SetActive(true);
                this.m_WingEventTable.PlateInitialText();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                this.m_ContentsContainer.SetActive(true);
                this.m_LockPlate.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                this.m_ContentsContainer.SetActive(false);
                this.m_LockPlate.SetActive(true);
                AdventureWingUnlockSpell component = this.m_UnlockSpell.GetComponent<AdventureWingUnlockSpell>();
                this.m_WingEventTable.PlateOpen((component == null) ? 0f : component.m_UnlockDelay);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                foreach (Boss boss in this.m_BossCoins)
                {
                    boss.m_Chest.SlamInCheckmark();
                }
            }
        }
    }

    public bool UpdateAndAnimateCoinsAndChests(float startDelay, bool forceCoinAnimation, DelOnCoinAnimateCallback dlg)
    {
        if (!this.m_WingEventTable.IsPlateInOrGoingToAnActiveState())
        {
            List<Boss> thingsToFlip = new List<Boss>();
            AdventureConfig config = AdventureConfig.Get();
            List<KeyValuePair<int, int>> list2 = new List<KeyValuePair<int, int>>();
            foreach (Boss boss in this.m_BossCoins)
            {
                int wingId = 0;
                int missionReqProgress = 0;
                bool flag = config.IsMissionNewlyAvailableAndGetReqs((int) boss.m_Mission, ref wingId, ref missionReqProgress);
                if ((forceCoinAnimation || flag) && (!forceCoinAnimation || AdventureProgressMgr.Get().CanPlayScenario((int) boss.m_Mission)))
                {
                    list2.Add(new KeyValuePair<int, int>(wingId, missionReqProgress));
                    Boss item = new Boss {
                        m_Mission = boss.m_Mission,
                        m_Coin = boss.m_Coin
                    };
                    if (AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario((int) boss.m_Mission).Count > 0)
                    {
                        item.m_Chest = boss.m_Chest;
                    }
                    thingsToFlip.Add(item);
                }
            }
            foreach (KeyValuePair<int, int> pair in list2)
            {
                config.SetWingAckIfGreater(pair.Key, pair.Value);
            }
            if (thingsToFlip.Count > 0)
            {
                base.StartCoroutine(this.AnimateCoinsAndChests(thingsToFlip, startDelay, dlg));
                return true;
            }
        }
        return false;
    }

    private void UpdateBossChest(AdventureRewardsChest chest, ScenarioDbId mission)
    {
        AdventureConfig config = AdventureConfig.Get();
        if (config.IsScenarioDefeatedAndInitCache(mission))
        {
            if (config.IsScenarioJustDefeated(mission))
            {
                chest.SlamInCheckmark();
            }
            else
            {
                chest.ShowCheckmark();
            }
        }
        else if (AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario((int) mission).Count == 0)
        {
            chest.HideAll();
        }
    }

    private void UpdateBuyButton(bool isOpen, object userData)
    {
        float num = 0f;
        bool flag = true;
        string gameStringTag = "GLUE_STORE_MONEY_BUTTON_TOOLTIP_HEADLINE";
        if (!isOpen)
        {
            num = 1f;
            flag = false;
            gameStringTag = "GLUE_ADVENTURE_LABEL_SHOP_CLOSED";
        }
        this.m_BuyButtonMesh.materials[0].SetFloat("_Desaturate", num);
        this.m_BuyButton.GetComponent<Collider>().enabled = flag;
        this.m_BuyButtonText.SetGameStringText(gameStringTag);
    }

    private void UpdateCoinPositions()
    {
        int num = 0;
        foreach (Boss boss in this.m_BossCoins)
        {
            boss.m_Coin.transform.localPosition = this.m_CoinsOffset;
            TransformUtil.SetLocalPosX(boss.m_Coin, this.m_CoinsOffset.x + (num * this.m_CoinSpacing));
            if (boss.m_Chest != null)
            {
                boss.m_Chest.transform.localPosition = this.m_CoinsOffset;
                TransformUtil.SetLocalPosX(boss.m_Chest, this.m_CoinsOffset.x + (num * this.m_CoinSpacing));
                Transform transform = boss.m_Chest.transform;
                transform.localPosition += this.m_CoinsChestOffset;
            }
            num++;
        }
    }

    public void UpdatePlateState()
    {
        this.UpdatePurchasedBanner();
        bool flag = AdventureProgressMgr.Get().IsWingLocked((int) this.m_WingDef.GetWingId());
        bool flag2 = AdventureProgressMgr.Get().OwnsWing((int) this.m_WingDef.GetWingId());
        bool flag3 = flag2 && AdventureProgressMgr.Get().IsWingOpen((int) this.m_WingDef.GetWingId());
        if (!flag3 || !this.m_Playable)
        {
            if ((flag3 != this.m_Playable) && !flag)
            {
                this.m_WingEventTable.PlateKey(false);
            }
            else if (!this.m_WingEventTable.IsPlateBuy())
            {
                if (!flag)
                {
                    bool flag4 = (this.m_DependsOnWingDef == null) || AdventureProgressMgr.Get().OwnsWing((int) this.m_DependsOnWingDef.GetWingId());
                    this.m_EventStartDetected = AdventureProgressMgr.Get().IsWingOpen((int) this.m_WingDef.GetWingId());
                    if (!this.m_EventStartDetected)
                    {
                        if (this.m_ReleaseLabelText != null)
                        {
                            this.m_ReleaseLabelText.Text = this.m_WingDef.GetComingSoonLabel();
                        }
                    }
                    else if (!flag2 && flag4)
                    {
                        this.m_WingEventTable.PlateBuy(false);
                    }
                    else if (this.m_ReleaseLabelText != null)
                    {
                        this.m_ReleaseLabelText.Text = this.m_WingDef.GetRequiresLabel();
                    }
                }
                else if (this.m_ReleaseLabelText != null)
                {
                    this.m_ReleaseLabelText.Text = !flag2 ? GameStrings.Get(this.m_WingDef.m_LockedPurchaseLocString) : GameStrings.Get(this.m_WingDef.m_LockedLocString);
                }
            }
            this.m_Playable = flag3;
            this.m_Owned = flag2;
        }
    }

    private void UpdatePurchasedBanner()
    {
        if (this.m_PurchasedBanner != null)
        {
            bool flag = AdventureProgressMgr.Get().OwnsWing((int) this.m_WingDef.GetWingId());
            bool flag2 = AdventureProgressMgr.Get().IsWingOpen((int) this.m_WingDef.GetWingId());
            this.m_PurchasedBanner.SetActive(flag && !flag2);
        }
    }

    public void UpdateRewardsPreviewCover()
    {
        if (!this.HasRewards())
        {
            this.m_WingEventTable.PlateCoverPreviewChest();
        }
    }

    public Vector3 CoinsChestOffset
    {
        get
        {
            return this.m_CoinsChestOffset;
        }
        set
        {
            this.m_CoinsChestOffset = value;
            this.UpdateCoinPositions();
        }
    }

    public Vector3 CoinsOffset
    {
        get
        {
            return this.m_CoinsOffset;
        }
        set
        {
            this.m_CoinsOffset = value;
            this.UpdateCoinPositions();
        }
    }

    public float CoinSpacing
    {
        get
        {
            return this.m_CoinSpacing;
        }
        set
        {
            this.m_CoinSpacing = value;
            this.UpdateCoinPositions();
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateCoinsAndChests>c__IteratorD : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delaySeconds;
        internal AdventureWing.DelOnCoinAnimateCallback <$>dlg;
        internal List<AdventureWing.Boss> <$>thingsToFlip;
        internal AdventureWing <>f__this;
        internal AdventureWing.Boss <boss>__1;
        internal int <i>__0;
        internal float delaySeconds;
        internal AdventureWing.DelOnCoinAnimateCallback dlg;
        internal List<AdventureWing.Boss> thingsToFlip;

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
                    if (this.delaySeconds <= 0f)
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(this.delaySeconds);
                    this.$PC = 1;
                    goto Label_012C;

                case 1:
                    break;

                case 2:
                    goto Label_00E3;

                case 3:
                    this.$PC = -1;
                    goto Label_012A;

                default:
                    goto Label_012A;
            }
            if (this.dlg != null)
            {
                this.dlg(this.thingsToFlip[0].m_Coin.transform.position);
            }
            this.<i>__0 = 0;
            while (this.<i>__0 < this.thingsToFlip.Count)
            {
                this.<boss>__1 = this.thingsToFlip[this.<i>__0];
                this.<>f__this.StartCoroutine(this.<>f__this.AnimateOneCoinAndChest(this.<boss>__1));
                this.$current = new WaitForSeconds(0.2f);
                this.$PC = 2;
                goto Label_012C;
            Label_00E3:
                this.<i>__0++;
            }
            this.$current = new WaitForSeconds(0.5f);
            this.$PC = 3;
            goto Label_012C;
        Label_012A:
            return false;
        Label_012C:
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
    private sealed class <AnimateOneCoinAndChest>c__IteratorE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AdventureWing.Boss <$>boss;
        internal AdventureWing.Boss boss;

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
                    if ((this.boss.m_Chest != null) && !AdventureProgressMgr.Get().HasDefeatedScenario((int) this.boss.m_Mission))
                    {
                        this.boss.m_Chest.BlinkChest();
                    }
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    goto Label_00C8;

                case 1:
                    this.boss.m_Coin.Enable(true, true);
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 2;
                    goto Label_00C8;

                case 2:
                    this.boss.m_Coin.ShowNewLookGlow();
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00C8:
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
    private sealed class <CreateBoss>c__AnonStorey2A7
    {
        internal AdventureWing <>f__this;
        internal ScenarioDbId mission;
        internal AdventureRewardsChest newchest;
        internal AdventureBossCoin newcoin;

        internal void <>m__38(UIEvent e)
        {
            this.<>f__this.FireBossSelectedEvent(this.newcoin, this.mission);
        }

        internal void <>m__39(UIEvent e)
        {
            this.<>f__this.ShowBossRewards(this.mission, this.newcoin.transform.position);
        }

        internal void <>m__3A(UIEvent e)
        {
            this.<>f__this.ShowBossRewards(this.mission, this.newchest.transform.position);
        }

        internal void <>m__3B(UIEvent e)
        {
            this.<>f__this.HideBossRewards(this.mission);
        }
    }

    [CompilerGenerated]
    private sealed class <DoUnlockPlate>c__IteratorF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>startDelay;
        internal AdventureWing <>f__this;
        internal float <unlockDelay>__0;
        internal AdventureWingUnlockSpell <wingUnlockSpell>__1;
        internal float startDelay;

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
                    this.<>f__this.FireOpenPlateStartEvent();
                    if (this.startDelay <= 0f)
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(this.startDelay);
                    this.$PC = 1;
                    return true;

                case 1:
                    break;

                default:
                    goto Label_01B4;
            }
            this.<>f__this.m_WingEventTable.AddOpenPlateEndEventListener(new StateEventTable.StateEventTrigger(this.<>f__this.FireOpenPlateEndEvent), true);
            this.<>f__this.m_UnlockButton.GetComponent<Collider>().enabled = false;
            this.<unlockDelay>__0 = 0f;
            if (this.<>f__this.m_UnlockSpell != null)
            {
                this.<wingUnlockSpell>__1 = this.<>f__this.m_UnlockSpell.GetComponent<AdventureWingUnlockSpell>();
                this.<unlockDelay>__0 = (this.<wingUnlockSpell>__1 == null) ? 0f : this.<wingUnlockSpell>__1.m_UnlockDelay;
            }
            this.<>f__this.m_WingEventTable.PlateOpen(this.<unlockDelay>__0);
            this.<>f__this.m_ContentsContainer.SetActive(true);
            if (this.<>f__this.m_UnlockSpell != null)
            {
                this.<>f__this.m_UnlockSpell.gameObject.SetActive(true);
                this.<>f__this.m_UnlockSpell.AddFinishedCallback(new Spell.FinishedCallback(this.<>f__this.OnUnlockSpellFinished));
                this.<>f__this.m_UnlockSpell.Activate();
            }
            else
            {
                this.<>f__this.OnUnlockSpellFinished(null, null);
            }
            this.<>f__this.m_UnlockButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.UnlockPlate));
            this.$PC = -1;
        Label_01B4:
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

    [Serializable]
    public class BackgroundRandomization
    {
        public MeshRenderer m_backgroundRenderer;
        public string m_materialTextureName = "_MainTex";
    }

    protected class Boss
    {
        public AdventureRewardsChest m_Chest;
        public AdventureBossCoin m_Coin;
        public ScenarioDbId m_Mission;
    }

    public delegate void BossSelected(AdventureBossCoin coin, ScenarioDbId mission);

    public delegate void BringToFocusCallback(Vector3 position, float time);

    public delegate void DelOnCoinAnimateCallback(Vector3 coinPosition);

    public delegate void HideCardRewards(List<CardRewardData> rewards);

    public delegate void OpenPlateEnd(AdventureWing wing);

    public delegate void OpenPlateStart(AdventureWing wing);

    public delegate void ShowCardRewards(List<CardRewardData> rewards, Vector3 origin);

    public delegate void ShowRewardsPreview();

    public delegate void TryPurchaseWing();
}

