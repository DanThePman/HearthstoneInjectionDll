using PegasusShared;
using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class AdventureMissionDisplay : MonoBehaviour
{
    [CustomEditField(Sections="UI")]
    public UberText m_AdventureTitle;
    private int m_AssetsLoading;
    [CustomEditField(Sections="UI")]
    public PegUIElement m_BackButton;
    private Actor m_BossActor;
    [CustomEditField(Sections="Boss Info")]
    public UberText m_BossDescription;
    private Map<ScenarioDbId, BossInfo> m_BossInfoCache = new Map<ScenarioDbId, BossInfo>();
    private bool m_BossJustDefeated;
    [CustomEditField(Sections="UI")]
    public GameObject m_BossPortraitContainer;
    private Map<ScenarioDbId, FullDef> m_BossPortraitDefCache = new Map<ScenarioDbId, FullDef>();
    private Actor m_BossPowerActor;
    private Actor m_BossPowerBigCard;
    [CustomEditField(Sections="UI")]
    public float m_BossPowerCardScale = 1f;
    [CustomEditField(Sections="UI")]
    public GameObject m_BossPowerContainer;
    private Map<ScenarioDbId, FullDef> m_BossPowerDefCache = new Map<ScenarioDbId, FullDef>();
    [CustomEditField(Sections="UI")]
    public PegUIElement m_BossPowerHoverArea;
    [CustomEditField(Sections="Boss Info")]
    public UberText m_BossTitle;
    private GameObject m_BossWingBorder;
    [CustomEditField(Sections="Boss Layout Settings")]
    public GameObject m_BossWingContainer;
    [SerializeField]
    private float m_BossWingHeight = 15f;
    [SerializeField]
    private Vector3 m_BossWingOffset = Vector3.zero;
    private List<AdventureWing> m_BossWings = new List<AdventureWing>();
    [CustomEditField(Sections="UI")]
    public PlayButton m_ChooseButton;
    private int m_ClassChallengeUnlockShowing;
    [CustomEditField(Sections="UI")]
    public GameObject m_ClickBlocker;
    [CustomEditField(Sections="UI/Animation")]
    public float m_CoinFlipAnimationTime = 1f;
    [CustomEditField(Sections="UI/Animation")]
    public float m_CoinFlipDelayTime = 1.25f;
    private int m_DisableSelectionCount;
    private MusicPlaylistType m_mainMusic;
    [CustomEditField(Sections="UI/Preview Pane")]
    public AdventureRewardsPreview m_PreviewPane;
    private AdventureWingProgressDisplay m_progressDisplay;
    [CustomEditField(Sections="UI")]
    public AdventureRewardsDisplayArea m_RewardsDisplay;
    [CustomEditField(Sections="UI/Scroll Bar")]
    public UIBScrollable m_ScrollBar;
    private AdventureBossCoin m_SelectedCoin;
    private FullDef m_SelectedHeroPowerFullDef;
    private bool m_ShowingRewardsPreview;
    private int m_TotalBosses;
    private int m_TotalBossesDefeated;
    private bool m_WaitingForClassChallengeUnlocks;
    [CustomEditField(Sections="UI", T=EditType.GAME_OBJECT)]
    public MeshRenderer m_WatermarkIcon;
    private List<AdventureWing> m_WingsToGiveBigChest = new List<AdventureWing>();
    private static readonly Vector3 REWARD_PUNCH_SCALE = new Vector3(1.2f, 1.2f, 1.2f);
    private static readonly Vector3 REWARD_SCALE = Vector3.one;
    private static AdventureMissionDisplay s_instance;
    private const float s_ScreenBackTransitionDelay = 1.8f;

    private void AddAssetToLoad(int assetCount = 1)
    {
        this.m_AssetsLoading += assetCount;
    }

    [DebuggerHidden]
    private IEnumerator AnimateFancyCheckmarksEffects()
    {
        return new <AnimateFancyCheckmarksEffects>c__Iterator6 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator AnimateProgressDisplay()
    {
        return new <AnimateProgressDisplay>c__Iterator5 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator AnimateWingBigChestsAndProgressDisplay()
    {
        return new <AnimateWingBigChestsAndProgressDisplay>c__Iterator4 { <>f__this = this };
    }

    private void AssetLoadCompleted()
    {
        if (this.m_AssetsLoading > 0)
        {
            this.m_AssetsLoading--;
            if (this.m_AssetsLoading == 0)
            {
                if (((this.m_BossPowerBigCard != null) && (this.m_BossPowerActor != null)) && (this.m_BossPowerBigCard.transform.parent != this.m_BossPowerActor.transform))
                {
                    GameUtils.SetParent(this.m_BossPowerBigCard, this.m_BossPowerActor.gameObject, false);
                }
                this.OnSubSceneLoaded();
            }
        }
        else
        {
            UnityEngine.Debug.LogError("AssetLoadCompleted() called when no assets left.", base.gameObject);
        }
    }

    private void Awake()
    {
        s_instance = this;
        this.m_mainMusic = MusicManager.Get().GetCurrentPlaylist();
        AdventureConfig config = AdventureConfig.Get();
        AdventureDbId selectedAdventure = config.GetSelectedAdventure();
        AdventureModeDbId selectedMode = config.GetSelectedMode();
        string locString = GameUtils.GetAdventureDataRecord((int) selectedAdventure, (int) selectedMode).GetLocString("NAME");
        this.m_AdventureTitle.Text = locString;
        List<WingCreateParams> list = this.BuildWingCreateParamsList();
        this.m_WingsToGiveBigChest.Clear();
        AdventureDef adventureDef = AdventureScene.Get().GetAdventureDef(selectedAdventure);
        AdventureSubDef subDef = adventureDef.GetSubDef(selectedMode);
        if (!string.IsNullOrEmpty(adventureDef.m_WingBottomBorderPrefab))
        {
            this.m_BossWingBorder = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(adventureDef.m_WingBottomBorderPrefab), true, false);
            GameUtils.SetParent(this.m_BossWingBorder, this.m_BossWingContainer, false);
        }
        this.AddAssetToLoad(3);
        foreach (WingCreateParams @params in list)
        {
            this.AddAssetToLoad(@params.m_BossCreateParams.Count * 2);
        }
        this.m_TotalBosses = 0;
        this.m_TotalBossesDefeated = 0;
        int num = 0;
        int num2 = list.Count - 1;
        if (!string.IsNullOrEmpty((string) adventureDef.m_ProgressDisplayPrefab))
        {
            this.m_progressDisplay = GameUtils.LoadGameObjectWithComponent<AdventureWingProgressDisplay>((string) adventureDef.m_ProgressDisplayPrefab);
            if (this.m_progressDisplay != null)
            {
                num++;
                num2++;
                if (this.m_BossWingContainer != null)
                {
                    GameUtils.SetParent(this.m_progressDisplay, this.m_BossWingContainer, false);
                }
            }
        }
        foreach (WingCreateParams params2 in list)
        {
            <Awake>c__AnonStorey29A storeya = new <Awake>c__AnonStorey29A {
                <>f__this = this
            };
            WingDbId wingId = params2.m_WingDef.GetWingId();
            AdventureWingDef wingDef = params2.m_WingDef;
            storeya.wing = GameUtils.LoadGameObjectWithComponent<AdventureWing>(wingDef.m_WingPrefab);
            if (storeya.wing != null)
            {
                if (this.m_BossWingContainer != null)
                {
                    GameUtils.SetParent(storeya.wing, this.m_BossWingContainer, false);
                }
                AdventureWingDef dependsOnWingDef = AdventureScene.Get().GetWingDef(params2.m_WingDef.GetOwnershipPrereqId());
                storeya.wing.Initialize(wingDef, dependsOnWingDef);
                storeya.wing.SetBigChestRewards(wingId);
                storeya.wing.AddBossSelectedListener(new AdventureWing.BossSelected(storeya.<>m__C));
                storeya.wing.AddOpenPlateStartListener(new AdventureWing.OpenPlateStart(this.OnStartUnlockPlate));
                storeya.wing.AddOpenPlateEndListener(new AdventureWing.OpenPlateEnd(this.OnEndUnlockPlate));
                storeya.wing.AddTryPurchaseWingListener(new AdventureWing.TryPurchaseWing(storeya.<>m__D));
                storeya.wing.AddShowCardRewardsListener(new AdventureWing.ShowCardRewards(storeya.<>m__E));
                storeya.wing.AddHideCardRewardsListener(new AdventureWing.HideCardRewards(storeya.<>m__F));
                storeya.wingScenarios = new List<int>();
                int defaultvalue = 0;
                foreach (BossCreateParams params3 in params2.m_BossCreateParams)
                {
                    <Awake>c__AnonStorey29B storeyb = new <Awake>c__AnonStorey29B {
                        <>f__this = this
                    };
                    bool enabled = config.IsMissionAvailable((int) params3.m_MissionId);
                    storeyb.coin = storeya.wing.CreateBoss(wingDef.m_CoinPrefab, wingDef.m_RewardsPrefab, params3.m_MissionId, enabled);
                    AdventureConfig.Get().LoadBossDef(params3.m_MissionId, new AdventureConfig.DelBossDefLoaded(storeyb.<>m__10));
                    if (AdventureConfig.Get().GetLastSelectedMission() == params3.m_MissionId)
                    {
                        base.StartCoroutine(this.RememberLastBossSelection(storeyb.coin, params3.m_MissionId));
                    }
                    if (AdventureProgressMgr.Get().HasDefeatedScenario((int) params3.m_MissionId))
                    {
                        defaultvalue++;
                        this.m_TotalBossesDefeated++;
                    }
                    this.m_TotalBosses++;
                    DefLoader.Get().LoadFullDef(params3.m_CardDefId, new DefLoader.LoadDefCallback<FullDef>(this.OnHeroFullDefLoaded), params3.m_MissionId);
                    storeya.wingScenarios.Add((int) params3.m_MissionId);
                }
                int num4 = config.GetWingBossesDefeated(selectedAdventure, selectedMode, wingId, defaultvalue);
                if (num4 != defaultvalue)
                {
                    this.m_BossJustDefeated = true;
                }
                bool flag2 = (defaultvalue == params2.m_BossCreateParams.Count) || AdventureScene.Get().IsDevMode;
                if (!storeya.wing.HasBigChestRewards())
                {
                    storeya.wing.HideBigChest();
                }
                else if (flag2)
                {
                    if (num4 != defaultvalue)
                    {
                        this.m_WingsToGiveBigChest.Add(storeya.wing);
                    }
                    else
                    {
                        storeya.wing.BigChestStayOpen();
                    }
                }
                if (this.m_progressDisplay != null)
                {
                    bool normalComplete = GameUtils.IsWingComplete((int) selectedAdventure, 1, (int) wingId);
                    this.m_progressDisplay.UpdateProgress(params2.m_WingDef.GetWingId(), normalComplete);
                }
                config.UpdateWingBossesDefeated(selectedAdventure, selectedMode, wingId, defaultvalue);
                storeya.wing.AddShowRewardsPreviewListeners(new AdventureWing.ShowRewardsPreview(storeya.<>m__11));
                storeya.wing.UpdateRewardsPreviewCover();
                storeya.wing.RandomizeBackground();
                storeya.focusScrollPos = ((float) num++) / ((float) num2);
                storeya.wing.SetBringToFocusCallback(new AdventureWing.BringToFocusCallback(storeya.<>m__12));
                this.m_BossWings.Add(storeya.wing);
            }
        }
        AssetLoader.Get().LoadActor("Card_Play_Hero", new AssetLoader.GameObjectCallback(this.OnHeroActorLoaded), null, false);
        AssetLoader.Get().LoadActor("Card_Play_HeroPower", new AssetLoader.GameObjectCallback(this.OnHeroPowerActorLoaded), null, false);
        AssetLoader.Get().LoadActor("History_HeroPower_Opponent", new AssetLoader.GameObjectCallback(this.OnHeroPowerBigCardLoaded), null, false);
        if (this.m_BossPowerHoverArea != null)
        {
            this.m_BossPowerHoverArea.AddEventListener(UIEventType.ROLLOVER, e => this.ShowHeroPowerBigCard());
            this.m_BossPowerHoverArea.AddEventListener(UIEventType.ROLLOUT, e => this.HideHeroPowerBigCard());
        }
        this.m_BackButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBackButtonPress));
        this.m_ChooseButton.AddEventListener(UIEventType.RELEASE, e => this.ChangeToDeckPicker());
        this.UpdateWingPositions();
        this.m_ChooseButton.Disable();
        this.DoAutoPurchaseWings(selectedAdventure, selectedMode);
        StoreManager.Get().RegisterStoreShownListener(new StoreManager.StoreShownCallback(this.OnStoreShown));
        StoreManager.Get().RegisterStoreHiddenListener(new StoreManager.StoreHiddenCallback(this.OnStoreHidden));
        AdventureProgressMgr.Get().RegisterProgressUpdatedListener(new AdventureProgressMgr.AdventureProgressUpdatedCallback(this.OnAdventureProgressUpdate));
        if (this.m_ScrollBar != null)
        {
            this.m_ScrollBar.LoadScroll(AdventureConfig.Get().GetSelectedAdventureAndModeString());
        }
        if ((this.m_WatermarkIcon != null) && !string.IsNullOrEmpty(subDef.m_WatermarkTexture))
        {
            string name = FileUtils.GameAssetPathToName(subDef.m_WatermarkTexture);
            Texture texture = AssetLoader.Get().LoadTexture(name, false);
            if (texture != null)
            {
                this.m_WatermarkIcon.material.mainTexture = texture;
            }
            else
            {
                UnityEngine.Debug.LogWarning(string.Format("Adventure Watermark texture is null: {0}", subDef.m_WatermarkTexture));
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning(string.Format("Adventure Watermark texture is null: m_WatermarkIcon: {0},  advSubDef.m_WatermarkTexture: {1}", this.m_WatermarkIcon, subDef.m_WatermarkTexture));
        }
        Navigation.PushUnique(new Navigation.NavigateBackHandler(AdventureMissionDisplay.OnNavigateBack));
        this.m_BackButton.gameObject.SetActive(true);
        this.m_PreviewPane.AddHideListener(new AdventureRewardsPreview.OnHide(this.OnHideRewardsPreview));
    }

    private int BossCreateParamsSortComparison(BossCreateParams params1, BossCreateParams params2)
    {
        return GameUtils.MissionSortComparison(params1.m_ScenarioRecord, params2.m_ScenarioRecord);
    }

    private void BringWingToFocus(float scrollPos)
    {
        if (this.m_ScrollBar != null)
        {
            this.m_ScrollBar.SetScroll(scrollPos, false, true);
        }
    }

    private List<WingCreateParams> BuildWingCreateParamsList()
    {
        AdventureConfig config = AdventureConfig.Get();
        AdventureDbId selectedAdventure = config.GetSelectedAdventure();
        AdventureModeDbId selectedMode = config.GetSelectedMode();
        int num = (int) selectedAdventure;
        int num2 = (int) selectedMode;
        List<WingCreateParams> list = new List<WingCreateParams>();
        int num3 = 0;
        foreach (DbfRecord record in GameDbf.Scenario.GetRecords())
        {
            int num4;
            int num5;
            <BuildWingCreateParamsList>c__AnonStorey29C storeyc = new <BuildWingCreateParamsList>c__AnonStorey29C();
            if ((record.TryGetInt("ADVENTURE_ID", out num4) && (num4 == num)) && (record.TryGetInt("MODE_ID", out num5) && (num5 == num2)))
            {
                ScenarioDbId id3;
                int id = record.GetId();
                if (EnumUtils.TryCast<ScenarioDbId>(id, out id3) && EnumUtils.TryCast<WingDbId>(record.GetInt("WING_ID"), out storeyc.wingId))
                {
                    int @int = record.GetInt("CLIENT_PLAYER2_HERO_CARD_ID");
                    if (@int == 0)
                    {
                        @int = record.GetInt("PLAYER2_HERO_CARD_ID");
                    }
                    AdventureWingDef wingDef = AdventureScene.Get().GetWingDef(storeyc.wingId);
                    if (wingDef == null)
                    {
                        UnityEngine.Debug.LogError(string.Format("Unable to find wing record for scenario {0} with ID: {1}", id, storeyc.wingId));
                    }
                    else
                    {
                        DbfRecord record2 = GameDbf.Card.GetRecord(@int);
                        WingCreateParams item = list.Find(new Predicate<WingCreateParams>(storeyc.<>m__19));
                        if (item == null)
                        {
                            item = new WingCreateParams {
                                m_WingDef = wingDef
                            };
                            if (item.m_WingDef == null)
                            {
                                object[] messageArgs = new object[] { selectedAdventure, storeyc.wingId };
                                Error.AddDevFatal("AdventureDisplay.BuildWingCreateParamsMap() - failed to find a WingDef for adventure {0} wing {1}", messageArgs);
                                continue;
                            }
                            list.Add(item);
                        }
                        BossCreateParams params2 = new BossCreateParams {
                            m_ScenarioRecord = record,
                            m_MissionId = id3,
                            m_CardDefId = record2.GetString("NOTE_MINI_GUID")
                        };
                        if (!this.m_BossInfoCache.ContainsKey(id3))
                        {
                            BossInfo info = new BossInfo {
                                m_Title = record.GetLocString("SHORT_NAME"),
                                m_Description = record.GetLocString("DESCRIPTION")
                            };
                            this.m_BossInfoCache[id3] = info;
                        }
                        item.m_BossCreateParams.Add(params2);
                        num3++;
                    }
                }
            }
        }
        if (num3 == 0)
        {
            UnityEngine.Debug.LogError(string.Format("Unable to find any bosses associated with wing {0} and mode {1}.\nCheck if the scenario DBF has valid entries!", selectedAdventure, selectedMode));
        }
        list.Sort(new Comparison<WingCreateParams>(this.WingCreateParamsSortComparison));
        foreach (WingCreateParams params3 in list)
        {
            params3.m_BossCreateParams.Sort(new Comparison<BossCreateParams>(this.BossCreateParamsSortComparison));
        }
        return list;
    }

    private void ChangeToDeckPicker()
    {
        ScenarioDbId mission = AdventureConfig.Get().GetMission();
        AdventureBossDef bossDef = AdventureConfig.Get().GetBossDef(mission);
        if ((bossDef != null) && (bossDef.m_IntroLinePlayTime == AdventureBossDef.IntroLinePlayTime.MissionStart))
        {
            this.PlayMissionQuote(bossDef, this.DetermineCharacterQuotePos(this.m_ChooseButton.gameObject));
        }
        if (AdventureConfig.Get().DoesSelectedMissionRequireDeck())
        {
            this.m_ChooseButton.Disable();
            this.DisableSelection(true);
            AdventureConfig.Get().ChangeSubScene(AdventureSubScenes.MissionDeckPicker);
        }
        else
        {
            GameMgr.Get().FindGame(GameType.GT_VS_AI, (int) AdventureConfig.Get().GetMission(), 0L, 0L);
        }
    }

    private Vector3 DetermineCharacterQuotePos(GameObject coin)
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            return NotificationManager.PHONE_CHARACTER_POS;
        }
        Bounds boundsOfChildren = TransformUtil.GetBoundsOfChildren(coin);
        Vector3 center = boundsOfChildren.center;
        center.y -= boundsOfChildren.extents.y;
        Camera camera = Box.Get().GetCamera();
        Vector3 vector2 = camera.WorldToScreenPoint(center);
        float num2 = 0.4f * camera.pixelHeight;
        if (vector2.y < num2)
        {
            return NotificationManager.ALT_ADVENTURE_SCREEN_POS;
        }
        return NotificationManager.DEFAULT_CHARACTER_POS;
    }

    private void DisableSelection(bool yes)
    {
        if (this.m_ClickBlocker != null)
        {
            this.m_DisableSelectionCount += !yes ? -1 : 1;
            bool flag = this.IsDisabledSelection();
            if (this.m_ClickBlocker.gameObject.activeSelf != flag)
            {
                this.m_ClickBlocker.gameObject.SetActive(flag);
                this.m_ScrollBar.Enable(!flag);
            }
        }
    }

    private void DoAutoPurchaseWings(AdventureDbId selectedAdv, AdventureModeDbId selectedMode)
    {
        if (selectedMode == AdventureModeDbId.NORMAL)
        {
            ProductType adventureProductType = StoreManager.GetAdventureProductType(selectedAdv);
            if (adventureProductType != ProductType.PRODUCT_TYPE_UNKNOWN)
            {
                StoreManager.Get().DoZeroCostTransactionIfPossible(StoreType.ADVENTURE_STORE, new Store.ExitCallback(this.OnZeroCostTransactionStoreExit), null, adventureProductType, 0, 0);
            }
        }
    }

    public static AdventureMissionDisplay Get()
    {
        return s_instance;
    }

    private void HideHeroPowerBigCard()
    {
        iTween.Stop(this.m_BossPowerBigCard.gameObject);
        this.m_BossPowerBigCard.Hide();
    }

    public bool IsDisabledSelection()
    {
        return (this.m_DisableSelectionCount > 0);
    }

    private Actor OnActorLoaded(string actorName, GameObject actorObject, GameObject container)
    {
        Actor component = actorObject.GetComponent<Actor>();
        if (component != null)
        {
            if (container != null)
            {
                GameUtils.SetParent(component, container, false);
            }
            SceneUtils.SetLayer(component, container.layer);
            component.SetUnlit();
            component.Hide();
            return component;
        }
        UnityEngine.Debug.LogWarning(string.Format("ERROR actor \"{0}\" has no Actor component", actorName));
        return component;
    }

    private void OnAdventureProgressUpdate(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress, object userData)
    {
        bool flag = (oldProgress != null) && oldProgress.IsOwned();
        bool flag2 = (newProgress != null) && newProgress.IsOwned();
        if (flag != flag2)
        {
            base.StartCoroutine(this.UpdateWingPlateStates());
        }
    }

    private void OnBackButtonPress(UIEvent e)
    {
        Navigation.GoBack();
    }

    private void OnBossSelected(AdventureBossCoin coin, ScenarioDbId mission, bool showDetails = true)
    {
        if (!this.IsDisabledSelection())
        {
            if (this.m_SelectedCoin != null)
            {
                this.m_SelectedCoin.Select(false);
            }
            this.m_SelectedCoin = coin;
            this.m_SelectedCoin.Select(true);
            if (this.m_ChooseButton != null)
            {
                if (!this.m_ChooseButton.IsEnabled())
                {
                    this.m_ChooseButton.Enable();
                }
                string newText = GameStrings.Get(!AdventureConfig.Get().DoesMissionRequireDeck(mission) ? "GLOBAL_PLAY" : "GLUE_CHOOSE");
                this.m_ChooseButton.SetText(newText);
            }
            this.ShowBossFrame(mission);
            AdventureConfig.Get().SetMission(mission, showDetails);
            AdventureBossDef bossDef = AdventureConfig.Get().GetBossDef(mission);
            if ((bossDef.m_MissionMusic != MusicPlaylistType.Invalid) && !MusicManager.Get().StartPlaylist(bossDef.m_MissionMusic))
            {
                this.ResumeMainMusic();
            }
            if (bossDef.m_IntroLinePlayTime == AdventureBossDef.IntroLinePlayTime.MissionSelect)
            {
                this.PlayMissionQuote(bossDef, this.DetermineCharacterQuotePos(coin.gameObject));
            }
        }
    }

    private void OnDestroy()
    {
        AdventureProgressMgr.Get().RemoveProgressUpdatedListener(new AdventureProgressMgr.AdventureProgressUpdatedCallback(this.OnAdventureProgressUpdate));
        StoreManager.Get().RemoveStoreHiddenListener(new StoreManager.StoreHiddenCallback(this.OnStoreHidden));
        StoreManager.Get().RemoveStoreShownListener(new StoreManager.StoreShownCallback(this.OnStoreShown));
        if ((this.m_ScrollBar != null) && (AdventureConfig.Get() != null))
        {
            this.m_ScrollBar.SaveScroll(AdventureConfig.Get().GetSelectedAdventureAndModeString());
        }
        s_instance = null;
    }

    private void OnEndUnlockPlate(AdventureWing wing)
    {
        <OnEndUnlockPlate>c__AnonStorey29D storeyd = new <OnEndUnlockPlate>c__AnonStorey29D {
            wing = wing,
            <>f__this = this
        };
        this.DisableSelection(false);
        if (!string.IsNullOrEmpty(storeyd.wing.GetWingDef().m_WingOpenPopup))
        {
            AdventureWingOpenBanner banner = GameUtils.LoadGameObjectWithComponent<AdventureWingOpenBanner>(storeyd.wing.GetWingDef().m_WingOpenPopup);
            if (banner != null)
            {
                banner.ShowBanner(new AdventureWingOpenBanner.OnBannerHidden(storeyd.<>m__1A));
            }
        }
        else
        {
            List<AdventureWing> wings = new List<AdventureWing> {
                storeyd.wing
            };
            base.StartCoroutine(this.UpdateAndAnimateWingCoinsAndChests(wings, false, true));
        }
    }

    private void OnHeroActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_BossActor = this.OnActorLoaded(actorName, actorObject, this.m_BossPortraitContainer);
        if ((this.m_BossActor != null) && (this.m_BossActor.GetHealthObject() != null))
        {
            this.m_BossActor.GetHealthObject().Hide();
        }
        this.AssetLoadCompleted();
    }

    private void OnHeroFullDefLoaded(string cardId, FullDef def, object userData)
    {
        if (def == null)
        {
            UnityEngine.Debug.LogError(string.Format("Unable to load {0} hero def for Adventure boss.", cardId), base.gameObject);
            this.AssetLoadCompleted();
        }
        else
        {
            ScenarioDbId id = (ScenarioDbId) ((int) userData);
            this.m_BossPortraitDefCache[id] = def;
            string heroPowerCardIdFromHero = GameUtils.GetHeroPowerCardIdFromHero(def.GetEntityDef().GetCardId());
            if (heroPowerCardIdFromHero == null)
            {
                object[] args = new object[] { cardId, def.GetEntityDef().GetCardId(), id, (int) id };
                UnityEngine.Debug.LogError(string.Format("Unable to load hero power ID from {0} (ID: {1}) for ScenarioDbId={2} ({3}).", args), base.gameObject);
            }
            else
            {
                this.AddAssetToLoad(1);
                DefLoader.Get().LoadFullDef(heroPowerCardIdFromHero, new DefLoader.LoadDefCallback<FullDef>(this.OnHeroPowerFullDefLoaded), id);
            }
            this.AssetLoadCompleted();
        }
    }

    private void OnHeroPowerActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_BossPowerActor = this.OnActorLoaded(actorName, actorObject, this.m_BossPowerContainer);
        this.AssetLoadCompleted();
    }

    private void OnHeroPowerBigCardLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_BossPowerBigCard = this.OnActorLoaded(actorName, actorObject, (this.m_BossPowerActor != null) ? this.m_BossPowerActor.gameObject : null);
        if (this.m_BossPowerBigCard != null)
        {
            this.m_BossPowerBigCard.TurnOffCollider();
        }
        this.AssetLoadCompleted();
    }

    private void OnHeroPowerFullDefLoaded(string cardId, FullDef def, object userData)
    {
        if (def == null)
        {
            UnityEngine.Debug.LogError(string.Format("Unable to load {0} hero power def for Adventure boss.", cardId), base.gameObject);
            this.AssetLoadCompleted();
        }
        else
        {
            ScenarioDbId id = (ScenarioDbId) ((int) userData);
            this.m_BossPowerDefCache[id] = def;
            this.AssetLoadCompleted();
        }
    }

    private void OnHideRewardsPreview()
    {
        if (this.m_ClickBlocker != null)
        {
            this.m_ClickBlocker.SetActive(false);
        }
        this.m_ShowingRewardsPreview = false;
    }

    private static bool OnNavigateBack()
    {
        AdventureConfig.Get().ChangeToLastSubScene(true);
        return true;
    }

    private void OnRewardObjectLoaded(Reward reward, object callbackData)
    {
        this.PositionReward(reward);
        reward.Show(false);
    }

    private void OnStartUnlockPlate(AdventureWing wing)
    {
        this.UnselectBoss();
        this.DisableSelection(true);
    }

    private void OnStoreHidden(object userData)
    {
        this.DisableSelection(false);
    }

    private void OnStoreShown(object userData)
    {
        this.DisableSelection(true);
    }

    private void OnSubSceneLoaded()
    {
        AdventureSubScene component = base.GetComponent<AdventureSubScene>();
        if (component != null)
        {
            component.AddSubSceneTransitionFinishedListener(new AdventureSubScene.SubSceneTransitionFinished(this.OnSubSceneTransitionComplete));
            component.SetIsLoaded(true);
        }
    }

    private void OnSubSceneTransitionComplete()
    {
        AdventureSubScene component = base.GetComponent<AdventureSubScene>();
        if (component != null)
        {
            component.RemoveSubSceneTransitionFinishedListener(new AdventureSubScene.SubSceneTransitionFinished(this.OnSubSceneTransitionComplete));
        }
        base.StartCoroutine(this.UpdateAndAnimateWingCoinsAndChests(this.m_BossWings, true, false));
    }

    private void OnZeroCostTransactionStoreExit(bool authorizationBackButtonPressed, object userData)
    {
        if (authorizationBackButtonPressed)
        {
            this.OnBackButtonPress(null);
        }
    }

    private void PlayMissionQuote(AdventureBossDef bossDef, Vector3 position)
    {
        if ((bossDef != null) && !string.IsNullOrEmpty(bossDef.m_IntroLine))
        {
            AdventureDef adventureDef = AdventureScene.Get().GetAdventureDef(AdventureConfig.Get().GetSelectedAdventure());
            string defaultQuotePrefab = null;
            if (adventureDef != null)
            {
                defaultQuotePrefab = adventureDef.m_DefaultQuotePrefab;
            }
            if (!string.IsNullOrEmpty(bossDef.m_quotePrefabOverride))
            {
                defaultQuotePrefab = bossDef.m_quotePrefabOverride;
            }
            if (!string.IsNullOrEmpty(defaultQuotePrefab))
            {
                bool allowRepeatDuringSession = (AdventureScene.Get() != null) && AdventureScene.Get().IsDevMode;
                NotificationManager.Get().CreateCharacterQuote(FileUtils.GameAssetPathToName(defaultQuotePrefab), position, GameStrings.Get(bossDef.m_IntroLine), bossDef.m_IntroLine, allowRepeatDuringSession, 0f, null, CanvasAnchor.BOTTOM_LEFT);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator PlayWingNotifications()
    {
        return new <PlayWingNotifications>c__Iterator9 { <>f__this = this };
    }

    private void PositionReward(Reward reward)
    {
        GameUtils.SetParent((Component) reward, (Component) base.transform, false);
    }

    [DebuggerHidden]
    private IEnumerator RememberLastBossSelection(AdventureBossCoin coin, ScenarioDbId mission)
    {
        return new <RememberLastBossSelection>c__Iterator8 { coin = coin, mission = mission, <$>coin = coin, <$>mission = mission, <>f__this = this };
    }

    private void ResumeMainMusic()
    {
        if (this.m_mainMusic != MusicPlaylistType.Invalid)
        {
            MusicManager.Get().StartPlaylist(this.m_mainMusic);
        }
    }

    private void ShowAdventureComplete()
    {
        AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
        AdventureModeDbId selectedMode = AdventureConfig.Get().GetSelectedMode();
        bool isBadlyHurt = false;
        AdventureDef adventureDef = AdventureScene.Get().GetAdventureDef(selectedAdventure);
        switch (selectedMode)
        {
            case AdventureModeDbId.HEROIC:
            case AdventureModeDbId.CLASS_CHALLENGE:
                isBadlyHurt = true;
                break;
        }
        this.DisableSelection(true);
        AdventureSubDef subDef = adventureDef.GetSubDef(selectedMode);
        AdventureDef.BannerRewardType bannerRewardType = adventureDef.m_BannerRewardType;
        if (bannerRewardType == AdventureDef.BannerRewardType.AdventureCompleteReward)
        {
            new AdventureCompleteRewardData(FileUtils.GameAssetPathToName(adventureDef.m_BannerRewardPrefab), subDef.GetCompleteBannerText(), isBadlyHurt).LoadRewardObject(delegate (Reward reward, object data) {
                reward.RegisterHideListener(userData => this.DisableSelection(false));
                this.OnRewardObjectLoaded(reward, data);
            });
        }
        else if (bannerRewardType == AdventureDef.BannerRewardType.BannerManagerPopup)
        {
            BannerManager.Get().ShowCustomBanner(adventureDef.m_BannerRewardPrefab, subDef.GetCompleteBannerText(), () => this.DisableSelection(false));
        }
    }

    private void ShowAdventureStore(AdventureWing selectedWing)
    {
        StoreManager.Get().StartAdventureTransaction(selectedWing.GetProductType(), selectedWing.GetProductData(), null, null);
    }

    private void ShowBossFrame(ScenarioDbId mission)
    {
        BossInfo info;
        FullDef def;
        if (this.m_BossInfoCache.TryGetValue(mission, out info))
        {
            this.m_BossTitle.Text = info.m_Title;
            if (this.m_BossDescription != null)
            {
                this.m_BossDescription.Text = info.m_Description;
            }
        }
        if (this.m_BossPortraitDefCache.TryGetValue(mission, out def))
        {
            this.m_BossActor.SetCardFlair(new CardFlair(TAG_PREMIUM.NORMAL));
            this.m_BossActor.SetEntityDef(def.GetEntityDef());
            this.m_BossActor.SetCardDef(def.GetCardDef());
            this.m_BossActor.UpdateAllComponents();
            this.m_BossActor.SetUnlit();
            this.m_BossActor.Show();
        }
        if (this.m_BossPowerDefCache.TryGetValue(mission, out def))
        {
            this.m_BossPowerActor.SetCardFlair(new CardFlair(TAG_PREMIUM.NORMAL));
            this.m_BossPowerActor.SetEntityDef(def.GetEntityDef());
            this.m_BossPowerActor.SetCardDef(def.GetCardDef());
            this.m_BossPowerActor.UpdateAllComponents();
            this.m_BossPowerActor.SetUnlit();
            this.m_BossPowerActor.Show();
            this.m_SelectedHeroPowerFullDef = def;
            if ((this.m_BossPowerContainer != null) && !this.m_BossPowerContainer.activeSelf)
            {
                this.m_BossPowerContainer.SetActive(true);
            }
        }
    }

    public void ShowClassChallengeUnlock(List<int> classChallengeUnlocks)
    {
        if ((classChallengeUnlocks == null) || (classChallengeUnlocks.Count == 0))
        {
            this.m_WaitingForClassChallengeUnlocks = false;
        }
        else
        {
            foreach (int num in classChallengeUnlocks)
            {
                this.m_ClassChallengeUnlockShowing++;
                new ClassChallengeUnlockData(num).LoadRewardObject(delegate (Reward reward, object data) {
                    reward.RegisterHideListener(delegate (object userData) {
                        this.m_ClassChallengeUnlockShowing--;
                        if (this.m_ClassChallengeUnlockShowing == 0)
                        {
                            this.m_WaitingForClassChallengeUnlocks = false;
                        }
                    });
                    this.OnRewardObjectLoaded(reward, data);
                });
            }
        }
    }

    private void ShowHeroPowerBigCard()
    {
        FullDef selectedHeroPowerFullDef = this.m_SelectedHeroPowerFullDef;
        if (selectedHeroPowerFullDef != null)
        {
            CardDef cardDef = selectedHeroPowerFullDef.GetCardDef();
            if (cardDef != null)
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    NotificationManager.Get().DestroyActiveQuote(0.2f);
                }
                this.m_BossPowerBigCard.SetCardDef(cardDef);
                this.m_BossPowerBigCard.SetEntityDef(selectedHeroPowerFullDef.GetEntityDef());
                this.m_BossPowerBigCard.UpdateAllComponents();
                this.m_BossPowerBigCard.Show();
                this.m_BossPowerBigCard.transform.localScale = (Vector3) (Vector3.one * this.m_BossPowerCardScale);
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    this.m_BossPowerBigCard.transform.localPosition = new Vector3(-7.77f, 1.56f, 0.39f);
                    this.TweenPower(this.m_BossPowerBigCard.gameObject, new Vector3?(this.m_BossPowerActor.gameObject.transform.position));
                }
                else
                {
                    this.m_BossPowerBigCard.transform.localPosition = !UniversalInputManager.Get().IsTouchMode() ? new Vector3(0.019f, 0.54f, -1.12f) : new Vector3(-3.18f, 0.54f, 0.1f);
                    this.TweenPower(this.m_BossPowerBigCard.gameObject, null);
                }
            }
        }
    }

    private void ShowRewardsPreview(AdventureWing wing, int[] scenarioids, List<CardRewardData> wingRewards, string wingName)
    {
        if (!this.m_ShowingRewardsPreview)
        {
            if (this.m_ClickBlocker != null)
            {
                this.m_ClickBlocker.SetActive(true);
            }
            this.m_ShowingRewardsPreview = true;
            this.m_PreviewPane.Reset();
            this.m_PreviewPane.SetHeaderText(wingName);
            List<string> specificRewardsPreviewCards = wing.GetWingDef().m_SpecificRewardsPreviewCards;
            int hiddenRewardsPreviewCount = wing.GetWingDef().m_HiddenRewardsPreviewCount;
            if ((specificRewardsPreviewCards != null) && (specificRewardsPreviewCards.Count > 0))
            {
                this.m_PreviewPane.AddSpecificCards(specificRewardsPreviewCards);
            }
            else
            {
                foreach (int num2 in scenarioids)
                {
                    this.m_PreviewPane.AddCardBatch(num2);
                }
                if ((wingRewards != null) && (wingRewards.Count > 0))
                {
                    this.m_PreviewPane.AddCardBatch(wingRewards);
                }
            }
            this.m_PreviewPane.SetHiddenCardCount(hiddenRewardsPreviewCount);
            this.m_PreviewPane.Show(true);
        }
    }

    private void TweenPower(GameObject go, Vector3? origin = new Vector3?())
    {
        Vector3 a = (!UniversalInputManager.Get().IsTouchMode() || (UniversalInputManager.UsePhoneUI != null)) ? new Vector3(0.1f, 0.1f, 0.1f) : new Vector3(0f, 0.1f, 0.1f);
        if (!origin.HasValue)
        {
            iTween.ScaleFrom(go, (Vector3) (go.transform.localScale * 0.5f), 0.15f);
            object[] args = new object[] { "position", go.transform.localPosition + a, "isLocal", true, "time", 10 };
            iTween.MoveTo(go, iTween.Hash(args));
        }
        else
        {
            Vector3 b = TransformUtil.ComputeWorldScale(go.transform.parent);
            Vector3 driftOffset = Vector3.Scale(a, b);
            AnimationUtil.GrowThenDrift(go, origin.Value, driftOffset);
        }
    }

    private void UnselectBoss()
    {
        if (this.m_BossTitle != null)
        {
            this.m_BossTitle.Text = string.Empty;
        }
        if (this.m_BossDescription != null)
        {
            this.m_BossDescription.Text = string.Empty;
        }
        this.m_BossActor.Hide();
        if (this.m_BossPowerContainer != null)
        {
            this.m_BossPowerContainer.SetActive(false);
        }
        if (this.m_SelectedCoin != null)
        {
            this.m_SelectedCoin.Select(false);
        }
        this.m_SelectedCoin = null;
        AdventureConfig.Get().SetMission(ScenarioDbId.INVALID, true);
        if (this.m_ChooseButton.IsEnabled())
        {
            this.m_ChooseButton.Disable();
        }
    }

    private void Update()
    {
        if (AdventureScene.Get().IsDevMode && (AdventureScene.Get().DevModeSetting == 2))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                base.StartCoroutine(this.AnimateFancyCheckmarksEffects());
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                this.ShowAdventureComplete();
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateAndAnimateWingCoinsAndChests(List<AdventureWing> wings, bool scrollToCoin, bool forceCoinAnimation = false)
    {
        return new <UpdateAndAnimateWingCoinsAndChests>c__Iterator3 { wings = wings, scrollToCoin = scrollToCoin, forceCoinAnimation = forceCoinAnimation, <$>wings = wings, <$>scrollToCoin = scrollToCoin, <$>forceCoinAnimation = forceCoinAnimation, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator UpdateWingPlateStates()
    {
        return new <UpdateWingPlateStates>c__Iterator7 { <>f__this = this };
    }

    private void UpdateWingPositions()
    {
        int num = 0;
        if (this.m_progressDisplay != null)
        {
            this.m_progressDisplay.transform.localPosition = this.m_BossWingOffset;
            TransformUtil.SetLocalPosZ(this.m_progressDisplay, this.m_BossWingOffset.z - (num++ * this.m_BossWingHeight));
        }
        foreach (AdventureWing wing in this.m_BossWings)
        {
            wing.transform.localPosition = this.m_BossWingOffset;
            TransformUtil.SetLocalPosZ(wing, this.m_BossWingOffset.z - (num++ * this.m_BossWingHeight));
        }
        if (this.m_BossWingBorder != null)
        {
            this.m_BossWingBorder.transform.localPosition = this.m_BossWingOffset;
            TransformUtil.SetLocalPosZ(this.m_BossWingBorder, this.m_BossWingOffset.z - (num++ * this.m_BossWingHeight));
        }
    }

    private int WingCreateParamsSortComparison(WingCreateParams params1, WingCreateParams params2)
    {
        return (params1.m_WingDef.GetSortOrder() - params2.m_WingDef.GetSortOrder());
    }

    [CustomEditField(Sections="Boss Layout Settings")]
    public float BossWingHeight
    {
        get
        {
            return this.m_BossWingHeight;
        }
        set
        {
            this.m_BossWingHeight = value;
            this.UpdateWingPositions();
        }
    }

    [CustomEditField(Sections="Boss Layout Settings")]
    public Vector3 BossWingOffset
    {
        get
        {
            return this.m_BossWingOffset;
        }
        set
        {
            this.m_BossWingOffset = value;
            this.UpdateWingPositions();
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateFancyCheckmarksEffects>c__Iterator6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<AdventureWing>.Enumerator <$s_43>__3;
        internal List<AdventureRewardsChest>.Enumerator <$s_44>__6;
        internal List<KeyValuePair<AdventureRewardsChest, float>>.Enumerator <$s_45>__9;
        internal AdventureMissionDisplay <>f__this;
        internal float <animateTime>__1;
        internal AdventureRewardsChest <chest>__7;
        internal List<KeyValuePair<AdventureRewardsChest, float>> <chestAnimates>__0;
        internal List<AdventureRewardsChest> <chests>__5;
        internal KeyValuePair<AdventureRewardsChest, float> <kvpair>__10;
        internal float <startScroll>__8;
        internal float <totalAnimTime>__2;
        internal AdventureWing <wing>__4;

        [DebuggerHidden]
        public void Dispose()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 2:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_45>__9.Dispose();
                    }
                    break;
            }
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            bool flag = false;
            switch (num)
            {
                case 0:
                    if ((this.<>f__this.m_TotalBosses == this.<>f__this.m_TotalBossesDefeated) && this.<>f__this.m_BossJustDefeated)
                    {
                        this.<chestAnimates>__0 = new List<KeyValuePair<AdventureRewardsChest, float>>();
                        this.<animateTime>__1 = 0.7f;
                        this.<totalAnimTime>__2 = 0f;
                        this.<$s_43>__3 = this.<>f__this.m_BossWings.GetEnumerator();
                        try
                        {
                            while (this.<$s_43>__3.MoveNext())
                            {
                                this.<wing>__4 = this.<$s_43>__3.Current;
                                this.<chests>__5 = this.<wing>__4.GetChests();
                                this.<$s_44>__6 = this.<chests>__5.GetEnumerator();
                                try
                                {
                                    while (this.<$s_44>__6.MoveNext())
                                    {
                                        this.<chest>__7 = this.<$s_44>__6.Current;
                                        this.<animateTime>__1 *= 0.9f;
                                        if (this.<animateTime>__1 < 0.1f)
                                        {
                                            this.<animateTime>__1 = 0.1f;
                                        }
                                        this.<totalAnimTime>__2 += this.<animateTime>__1;
                                        this.<chestAnimates>__0.Add(new KeyValuePair<AdventureRewardsChest, float>(this.<chest>__7, this.<animateTime>__1));
                                    }
                                    continue;
                                }
                                finally
                                {
                                    this.<$s_44>__6.Dispose();
                                }
                            }
                        }
                        finally
                        {
                            this.<$s_43>__3.Dispose();
                        }
                        this.<>f__this.DisableSelection(true);
                        this.<startScroll>__8 = 0f;
                        if (this.<>f__this.m_progressDisplay != null)
                        {
                            this.<totalAnimTime>__2 -= this.<animateTime>__1;
                            this.<startScroll>__8 = 1f / ((float) this.<>f__this.m_BossWings.Count);
                        }
                        this.<>f__this.m_ScrollBar.SetScroll(this.<startScroll>__8, iTween.EaseType.easeOutSine, 0.25f, true, true);
                        this.$current = new WaitForSeconds(0.3f);
                        this.$PC = 1;
                        goto Label_02F4;
                    }
                    goto Label_02F2;

                case 1:
                    this.<>f__this.m_ScrollBar.SetScroll(1f, iTween.EaseType.easeInQuart, this.<totalAnimTime>__2 - 0.1f, true, true);
                    this.<$s_45>__9 = this.<chestAnimates>__0.GetEnumerator();
                    num = 0xfffffffd;
                    break;

                case 2:
                    break;

                default:
                    goto Label_02F2;
            }
            try
            {
                while (this.<$s_45>__9.MoveNext())
                {
                    this.<kvpair>__10 = this.<$s_45>__9.Current;
                    this.<kvpair>__10.Key.BurstCheckmark();
                    this.$current = new WaitForSeconds(this.<kvpair>__10.Value);
                    this.$PC = 2;
                    flag = true;
                    goto Label_02F4;
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_45>__9.Dispose();
            }
            this.<>f__this.DisableSelection(false);
            this.<>f__this.ShowAdventureComplete();
            this.$PC = -1;
        Label_02F2:
            return false;
        Label_02F4:
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
    private sealed class <AnimateProgressDisplay>c__Iterator5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AdventureMissionDisplay <>f__this;
        internal bool <isAnimComplete>__0;

        internal void <>m__20()
        {
            this.<>f__this.DisableSelection(false);
            this.<isAnimComplete>__0 = true;
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
                    if (this.<>f__this.m_progressDisplay == null)
                    {
                        goto Label_00BA;
                    }
                    goto Label_00A5;

                case 1:
                    break;

                default:
                    goto Label_00D8;
            }
        Label_009A:
            while (!this.<isAnimComplete>__0)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
        Label_00A5:
            if (this.<>f__this.m_progressDisplay.HasProgressAnimationToPlay())
            {
                this.<>f__this.m_ScrollBar.SetScroll(0f, false, true);
                this.<>f__this.DisableSelection(true);
                this.<isAnimComplete>__0 = false;
                this.<>f__this.m_progressDisplay.PlayProgressAnimation(new AdventureWingProgressDisplay.OnAnimationComplete(this.<>m__20));
                goto Label_009A;
            }
        Label_00BA:
            this.<>f__this.StartCoroutine(this.<>f__this.AnimateFancyCheckmarksEffects());
            this.$PC = -1;
        Label_00D8:
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
    private sealed class <AnimateWingBigChestsAndProgressDisplay>c__Iterator4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<AdventureWing>.Enumerator <$s_40>__1;
        internal List<AdventureWing>.Enumerator <$s_41>__4;
        internal List<AdventureWing>.Enumerator <$s_42>__7;
        internal AdventureMissionDisplay <>f__this;
        internal int <animDone>__0;
        internal HashSet<RewardVisualTiming> <rewardTimings>__6;
        internal AdventureWing <wing>__2;
        internal AdventureWing <wing>__5;
        internal AdventureWing <wing>__8;
        internal List<int> <wingIds>__3;

        internal void <>m__1E(Spell s)
        {
            this.<animDone>__0--;
        }

        internal void <>m__1F(object userData)
        {
            this.<>f__this.ShowClassChallengeUnlock(this.<wingIds>__3);
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
                    if (this.<>f__this.m_WingsToGiveBigChest.Count == 0)
                    {
                        goto Label_0361;
                    }
                    this.<>f__this.DisableSelection(true);
                    if (!AdventureScene.Get().IsInitialScreen())
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(1.8f);
                    this.$PC = 1;
                    goto Label_0381;

                case 1:
                    break;

                case 2:
                    goto Label_013C;

                case 3:
                    goto Label_0258;

                default:
                    goto Label_037F;
            }
            this.<animDone>__0 = 0;
            this.<$s_40>__1 = this.<>f__this.m_WingsToGiveBigChest.GetEnumerator();
            try
            {
                while (this.<$s_40>__1.MoveNext())
                {
                    this.<wing>__2 = this.<$s_40>__1.Current;
                    this.<animDone>__0++;
                    this.<wing>__2.m_WingEventTable.AddOpenChestEndEventListener(new StateEventTable.StateEventTrigger(this.<>m__1E), true);
                    this.<wing>__2.OpenBigChest();
                    this.<>f__this.m_ScrollBar.CenterWorldPosition(this.<wing>__2.transform.position);
                }
            }
            finally
            {
                this.<$s_40>__1.Dispose();
            }
        Label_013C:
            while (this.<animDone>__0 > 0)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0381;
            }
            this.<>f__this.StartCoroutine(this.<>f__this.PlayWingNotifications());
            this.<wingIds>__3 = new List<int>();
            this.<$s_41>__4 = this.<>f__this.m_WingsToGiveBigChest.GetEnumerator();
            try
            {
                while (this.<$s_41>__4.MoveNext())
                {
                    this.<wing>__5 = this.<$s_41>__4.Current;
                    this.<wingIds>__3.Add((int) this.<wing>__5.GetWingId());
                }
            }
            finally
            {
                this.<$s_41>__4.Dispose();
            }
            HashSet<RewardVisualTiming> set = new HashSet<RewardVisualTiming> {
                RewardVisualTiming.ADVENTURE_CHEST
            };
            this.<rewardTimings>__6 = set;
            this.<>f__this.m_WaitingForClassChallengeUnlocks = true;
            if (!FixedRewardsMgr.Get().ShowFixedRewards(this.<rewardTimings>__6, new FixedRewardsMgr.DelOnAllFixedRewardsShown(this.<>m__1F), new FixedRewardsMgr.DelPositionNonToastReward(this.<>f__this.PositionReward), AdventureMissionDisplay.REWARD_PUNCH_SCALE, AdventureMissionDisplay.REWARD_SCALE))
            {
                this.<>f__this.ShowClassChallengeUnlock(this.<wingIds>__3);
            }
        Label_0258:
            while (this.<>f__this.m_WaitingForClassChallengeUnlocks)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_0381;
            }
            this.<$s_42>__7 = this.<>f__this.m_WingsToGiveBigChest.GetEnumerator();
            try
            {
                while (this.<$s_42>__7.MoveNext())
                {
                    this.<wing>__8 = this.<$s_42>__7.Current;
                    if (!string.IsNullOrEmpty(this.<wing>__8.GetWingDef().m_CompleteQuotePrefab) && !string.IsNullOrEmpty(this.<wing>__8.GetWingDef().m_CompleteQuoteVOLine))
                    {
                        NotificationManager.Get().CreateCharacterQuote(FileUtils.GameAssetPathToName(this.<wing>__8.GetWingDef().m_CompleteQuotePrefab), GameStrings.Get(this.<wing>__8.GetWingDef().m_CompleteQuoteVOLine), this.<wing>__8.GetWingDef().m_CompleteQuoteVOLine, true, 0f, CanvasAnchor.BOTTOM_LEFT);
                    }
                    this.<wing>__8.BigChestStayOpen();
                }
            }
            finally
            {
                this.<$s_42>__7.Dispose();
            }
            this.<>f__this.m_WingsToGiveBigChest.Clear();
            this.<>f__this.DisableSelection(false);
        Label_0361:
            this.<>f__this.StartCoroutine(this.<>f__this.AnimateProgressDisplay());
            this.$PC = -1;
        Label_037F:
            return false;
        Label_0381:
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
    private sealed class <Awake>c__AnonStorey29A
    {
        internal AdventureMissionDisplay <>f__this;
        internal float focusScrollPos;
        internal AdventureWing wing;
        internal List<int> wingScenarios;

        internal void <>m__11()
        {
            this.<>f__this.ShowRewardsPreview(this.wing, this.wingScenarios.ToArray(), this.wing.GetBigChestRewards(), this.wing.GetWingName());
        }

        internal void <>m__12(Vector3 position, float time)
        {
            this.<>f__this.BringWingToFocus(this.focusScrollPos);
        }

        internal void <>m__C(AdventureBossCoin c, ScenarioDbId m)
        {
            this.<>f__this.OnBossSelected(c, m, true);
        }

        internal void <>m__D()
        {
            this.<>f__this.ShowAdventureStore(this.wing);
        }

        internal void <>m__E(List<CardRewardData> r, Vector3 o)
        {
            this.<>f__this.m_RewardsDisplay.ShowCards(r, o, null);
        }

        internal void <>m__F(List<CardRewardData> r)
        {
            this.<>f__this.m_RewardsDisplay.HideCardRewards();
        }
    }

    [CompilerGenerated]
    private sealed class <Awake>c__AnonStorey29B
    {
        internal AdventureMissionDisplay <>f__this;
        internal AdventureBossCoin coin;

        internal void <>m__10(AdventureBossDef bossDef, bool y)
        {
            if ((bossDef != null) && (bossDef.m_CoinPortraitMaterial != null))
            {
                this.coin.SetPortraitMaterial(bossDef.m_CoinPortraitMaterial);
            }
            this.<>f__this.AssetLoadCompleted();
        }
    }

    [CompilerGenerated]
    private sealed class <BuildWingCreateParamsList>c__AnonStorey29C
    {
        internal WingDbId wingId;

        internal bool <>m__19(AdventureMissionDisplay.WingCreateParams currParams)
        {
            return (this.wingId == currParams.m_WingDef.GetWingId());
        }
    }

    [CompilerGenerated]
    private sealed class <OnEndUnlockPlate>c__AnonStorey29D
    {
        internal AdventureMissionDisplay <>f__this;
        internal AdventureWing wing;

        internal void <>m__1A()
        {
            this.<>f__this.StartCoroutine(this.<>f__this.UpdateAndAnimateWingCoinsAndChests(new List<AdventureWing> { this.wing }, false, true));
        }
    }

    [CompilerGenerated]
    private sealed class <PlayWingNotifications>c__Iterator9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<AdventureWing>.Enumerator <$s_52>__0;
        internal AdventureMissionDisplay <>f__this;
        internal AdventureWing <wing>__1;

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
                    this.$current = new WaitForSeconds(3f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<$s_52>__0 = this.<>f__this.m_WingsToGiveBigChest.GetEnumerator();
                    try
                    {
                        while (this.<$s_52>__0.MoveNext())
                        {
                            this.<wing>__1 = this.<$s_52>__0.Current;
                            if ((this.<wing>__1.GetAdventureId() == AdventureDbId.NAXXRAMAS) && (this.<wing>__1.GetWingId() == WingDbId.NAXX_ARACHNID))
                            {
                                NotificationManager.Get().CreateKTQuote("VO_KT_MAEXXNA5_50", "VO_KT_MAEXXNA5_50", true);
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_52>__0.Dispose();
                    }
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
    private sealed class <RememberLastBossSelection>c__Iterator8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AdventureBossCoin <$>coin;
        internal ScenarioDbId <$>mission;
        internal AdventureMissionDisplay <>f__this;
        internal AdventureBossCoin coin;
        internal ScenarioDbId mission;

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
                    if (this.<>f__this.m_AssetsLoading > 0)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.OnBossSelected(this.coin, this.mission, false);
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
    private sealed class <UpdateAndAnimateWingCoinsAndChests>c__Iterator3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>forceCoinAnimation;
        internal bool <$>scrollToCoin;
        internal List<AdventureWing> <$>wings;
        internal List<AdventureWing>.Enumerator <$s_39>__3;
        internal AdventureMissionDisplay <>f__this;
        internal AdventureWing.DelOnCoinAnimateCallback <func>__5;
        internal float <scrollPos>__6;
        internal AdventureWing <wing>__4;
        internal int <wingIdx>__1;
        internal int <wingIdxMax>__2;
        internal int <wingsUpdated>__0;
        internal bool forceCoinAnimation;
        internal bool scrollToCoin;
        internal List<AdventureWing> wings;

        internal void <>m__1D(Vector3 p)
        {
            this.<>f__this.m_ScrollBar.SetScroll(this.<scrollPos>__6, false, true);
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
                    this.<>f__this.DisableSelection(true);
                    if (!AdventureScene.Get().IsInitialScreen())
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(1.8f);
                    this.$PC = 1;
                    goto Label_0200;

                case 1:
                    break;

                case 2:
                    goto Label_01C3;

                case 3:
                    this.$PC = -1;
                    goto Label_01FE;

                default:
                    goto Label_01FE;
            }
            this.<wingsUpdated>__0 = 0;
            this.<wingIdx>__1 = 0;
            this.<wingIdxMax>__2 = this.wings.Count - 1;
            if (this.<>f__this.m_progressDisplay != null)
            {
                this.<wingIdx>__1++;
                this.<wingIdxMax>__2++;
            }
            this.<$s_39>__3 = this.wings.GetEnumerator();
            try
            {
                while (this.<$s_39>__3.MoveNext())
                {
                    this.<wing>__4 = this.<$s_39>__3.Current;
                    this.<func>__5 = null;
                    if (this.scrollToCoin)
                    {
                        this.<scrollPos>__6 = ((float) this.<wingIdx>__1++) / ((float) this.<wingIdxMax>__2);
                        this.<func>__5 = new AdventureWing.DelOnCoinAnimateCallback(this.<>m__1D);
                    }
                    if (this.<wing>__4.UpdateAndAnimateCoinsAndChests(this.<>f__this.m_CoinFlipDelayTime * this.<wingsUpdated>__0, this.forceCoinAnimation, this.<func>__5))
                    {
                        this.<wingsUpdated>__0++;
                    }
                }
            }
            finally
            {
                this.<$s_39>__3.Dispose();
            }
            if (this.<wingsUpdated>__0 > 0)
            {
                this.$current = new WaitForSeconds((this.<>f__this.m_CoinFlipDelayTime * this.<wingsUpdated>__0) + this.<>f__this.m_CoinFlipAnimationTime);
                this.$PC = 2;
                goto Label_0200;
            }
        Label_01C3:
            this.<>f__this.DisableSelection(false);
            this.$current = this.<>f__this.StartCoroutine(this.<>f__this.AnimateWingBigChestsAndProgressDisplay());
            this.$PC = 3;
            goto Label_0200;
        Label_01FE:
            return false;
        Label_0200:
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
    private sealed class <UpdateWingPlateStates>c__Iterator7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<AdventureWing>.Enumerator <$s_49>__0;
        internal AdventureMissionDisplay <>f__this;
        internal AdventureWing <wing>__1;

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
                    if (StoreManager.Get().IsShown())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<$s_49>__0 = this.<>f__this.m_BossWings.GetEnumerator();
                    try
                    {
                        while (this.<$s_49>__0.MoveNext())
                        {
                            this.<wing>__1 = this.<$s_49>__0.Current;
                            this.<wing>__1.UpdatePlateState();
                        }
                    }
                    finally
                    {
                        this.<$s_49>__0.Dispose();
                    }
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

    protected class BossCreateParams
    {
        public string m_CardDefId;
        public ScenarioDbId m_MissionId;
        public DbfRecord m_ScenarioRecord;
    }

    protected class BossInfo
    {
        public string m_Description;
        public string m_Title;
    }

    protected class WingCreateParams
    {
        [CustomEditField(ListTable=true)]
        public List<AdventureMissionDisplay.BossCreateParams> m_BossCreateParams = new List<AdventureMissionDisplay.BossCreateParams>();
        public AdventureWingDef m_WingDef;
    }
}

