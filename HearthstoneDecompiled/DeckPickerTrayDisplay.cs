using PegasusShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class DeckPickerTrayDisplay : MonoBehaviour
{
    private string gameMode;
    private readonly List<TAG_CLASS> HERO_CLASSES = new List<TAG_CLASS> { 10, 8, 7, 5, 3, 2, 9, 4, 6 };
    public static readonly PlatformDependentValue<bool> HighlightSelectedDeck;
    [CustomEditField(Sections="Phone Only")]
    public Mesh m_alternateDetailsTrayMesh;
    private Material m_arrowButtonMaterial;
    [CustomEditField(Sections="Phone Only")]
    public Material m_arrowButtonShadowMaterial;
    public UIBButton m_backButton;
    private bool m_buttonAchievementsInitialized;
    public float m_customDeckHorizontalSpacing;
    private List<CollectionDeckBoxVisual> m_customDecks = new List<CollectionDeckBoxVisual>();
    public Vector3 m_customDeckScale;
    public Vector3 m_customDeckStart;
    public float m_customDeckVerticalSpacing;
    public GameObject m_customTray;
    private Texture m_customTrayTexture;
    public GameObject m_deckboxCoverPrefab;
    public CollectionDeckBoxVisual m_deckboxPrefab;
    public Vector3 m_deckCoverOffset;
    public List<GameObject> m_deckCovers = new List<GameObject>();
    private List<DeckTrayLoaded> m_DeckTrayLoadedListeners = new List<DeckTrayLoaded>();
    private bool m_delayButtonAnims;
    [CustomEditField(Sections="Phone Only")]
    public GameObject m_detailsTrayFrame;
    public Texture m_emptyHeroTexture;
    public BoxCollider m_expoClickBlocker;
    private Notification m_expoIntroQuote;
    private Notification m_expoThankQuote;
    private PegUIElement m_goldenHeroPower;
    private Actor m_goldenHeroPowerActor;
    private Actor m_goldenHeroPowerBigCard;
    public Transform m_Hero_Bone;
    public Transform m_Hero_BoneDown;
    private Actor m_heroActor;
    private List<HeroPickerButton> m_heroButtons = new List<HeroPickerButton>();
    private int m_heroDefsLoading = 0x7fffffff;
    public UberText m_heroName;
    public float m_heroPickerButtonHorizontalSpacing;
    public Vector3 m_heroPickerButtonScale;
    public Vector3 m_heroPickerButtonStart;
    public float m_heroPickerButtonVerticalSpacing;
    private PegUIElement m_heroPower;
    public Transform m_HeroPower_Bone;
    public Transform m_HeroPower_BoneDown;
    private Actor m_heroPowerActor;
    private Actor m_heroPowerBigCard;
    private Hashtable m_heroPowerDefs = new Hashtable();
    private int m_heroPowerDefsLoading = 0x7fffffff;
    public GameObject m_heroPowerShadowQuad;
    public GameObject m_heroPrefab;
    public GameObject m_hierarchyDeckTray;
    public GameObject m_hierarchyDetails;
    private UnityEngine.Vector2 m_keyholeTextureOffset;
    public GameObject m_labelDecoration;
    public ArrowModeButton m_leftArrow;
    private bool m_Loaded;
    [CustomEditField(Sections="Phone Only")]
    public Transform m_medalBone_phone;
    public UberText m_modeLabel;
    public GameObject m_modeLabelBg;
    public UberText m_modeName;
    private int m_numCustomDecks;
    public PlayButton m_playButton;
    public GameObject m_randomDeckPickerTray;
    public GameObject m_randomDecksHiddenBone;
    public GameObject m_randomDecksShownBone;
    public GameObject m_randomTray;
    [CustomEditField(Sections="Phone Only")]
    public SlidingTray m_rankedDetailsTray;
    private RankedPlayDisplay m_rankedPlayButtons;
    public Transform m_rankedPlayButtonsBone;
    public UberText m_rankedWins;
    public GameObject m_rankedWinsPlate;
    public ArrowModeButton m_rightArrow;
    private CollectionDeckBoxVisual m_selectedCustomDeckBox;
    private HeroPickerButton m_selectedHeroButton;
    private string m_selectedHeroName;
    private FullDef m_selectedHeroPowerFullDef;
    private bool m_showingCustomDecks;
    private SlidingTray m_slidingTray;
    public GameObject m_suckedInRandomDecksBone;
    private KeywordHelpPanel m_tooltip;
    public Transform m_tooltipBone;
    public GameObject m_tooltipPrefab;
    public GameObject m_trayFrame;
    private HeroXPBar m_xpBar;
    public HeroXPBar m_xpBarPrefab;
    private const int MAX_CUSTOM_DECKS_TO_DISPLAY = 9;
    private const int MAX_PRECON_DECKS_TO_DISPLAY = 9;
    private static DeckPickerTrayDisplay s_instance;
    private const float TRAY_SINK_TIME = 0.2f;
    private const float TRAY_SLIDE_TIME = 0.25f;

    static DeckPickerTrayDisplay()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.Screen) {
            Phone = false,
            Tablet = true,
            PC = true
        };
        HighlightSelectedDeck = value2;
    }

    public void AddDeckTrayLoadedListener(DeckTrayLoaded dlg)
    {
        this.m_DeckTrayLoadedListeners.Add(dlg);
    }

    private bool AreAllCustomDecksReady()
    {
        foreach (CollectionDeckBoxVisual visual in this.m_customDecks)
        {
            if ((visual.GetFullDef() == null) && (visual.GetDeckID() > 0L))
            {
                return false;
            }
        }
        return true;
    }

    [DebuggerHidden]
    private IEnumerator ArrowDelayedActivate(ArrowModeButton arrow, float delay)
    {
        return new <ArrowDelayedActivate>c__Iterator5E { delay = delay, arrow = arrow, <$>delay = delay, <$>arrow = arrow };
    }

    private void Awake()
    {
        this.m_randomDeckPickerTray.transform.localPosition = this.m_randomDecksShownBone.transform.localPosition;
        SoundManager.Get().Load("hero_panel_slide_on");
        SoundManager.Get().Load("hero_panel_slide_off");
        SceneMgr.Mode mode = SceneMgr.Get().GetMode();
        if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
        {
            this.m_delayButtonAnims = true;
            LoadingScreen.Get().RegisterFinishedTransitionListener(new LoadingScreen.FinishedTransitionCallback(this.OnTransitionFromGameplayFinished));
        }
        DeckPickerTray.Get().RegisterHandlers();
        s_instance = this;
        if (this.m_heroPowerShadowQuad != null)
        {
            this.m_heroPowerShadowQuad.SetActive(false);
        }
        this.LoadHero();
        if (this.ShouldShowHeroPower())
        {
            this.LoadHeroPower();
            this.LoadGoldenHeroPower();
        }
        this.m_heroName.RichText = false;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            Renderer component = this.m_leftArrow.GetComponent<Renderer>();
            this.m_arrowButtonMaterial = component.material;
        }
        if (this.m_backButton != null)
        {
            this.m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
            this.m_backButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.BackButtonPress));
        }
        this.m_playButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.PlayGameButtonRelease));
        this.EnablePlayButton(false);
        this.m_leftArrow.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnShowPreconDecks));
        this.m_rightArrow.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnShowCustomDecks));
        this.m_heroName.Text = string.Empty;
        switch (mode)
        {
            case SceneMgr.Mode.COLLECTIONMANAGER:
            case SceneMgr.Mode.ADVENTURE:
                this.m_playButton.SetText(GameStrings.Get("GLUE_CHOOSE"));
                break;

            case SceneMgr.Mode.TOURNAMENT:
                AssetLoader.Get().LoadGameObject((UniversalInputManager.UsePhoneUI == null) ? "RankedPlayButtons" : "RankButtons_phone", new AssetLoader.GameObjectCallback(this.RankedPlayButtonsLoaded), null, false);
                break;

            case SceneMgr.Mode.FRIENDLY:
                if (!this.IsChoosingHeroForTavernBrawlChallenge())
                {
                    this.m_playButton.SetText(GameStrings.Get("GLUE_CHOOSE"));
                    break;
                }
                this.SetHeaderForTavernBrawl();
                break;

            case SceneMgr.Mode.TAVERN_BRAWL:
                this.SetHeaderForTavernBrawl();
                break;
        }
        switch (mode)
        {
            case SceneMgr.Mode.COLLECTIONMANAGER:
                AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_Custom_Tournament" : "HeroPicker_TournamentAndCustom_phone", new AssetLoader.ObjectCallback(this.SetCustomTrayTexture), null, false);
                AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_CreateDeck" : "HeroPicker_TournamentAndCustom_phone", new AssetLoader.ObjectCallback(this.SetTrayTexture), null, false);
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    this.m_detailsTrayFrame.GetComponent<MeshFilter>().mesh = this.m_alternateDetailsTrayMesh;
                    AssetLoader.Get().LoadTexture("DeckBuild_DeckHeroTray_phone", new AssetLoader.ObjectCallback(this.SetDetailsTrayTexture), null, false);
                }
                this.m_keyholeTextureOffset = new UnityEngine.Vector2(0f, 0f);
                break;

            case SceneMgr.Mode.TOURNAMENT:
                AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_Custom_Tournament" : "HeroPicker_TournamentAndCustom_phone", new AssetLoader.ObjectCallback(this.SetCustomTrayTexture), null, false);
                AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_Tournament" : "HeroPicker_TournamentAndCustom_phone", new AssetLoader.ObjectCallback(this.SetTrayTexture), null, false);
                this.m_keyholeTextureOffset = new UnityEngine.Vector2(0f, 0f);
                this.m_rankedWinsPlate.SetActive(true);
                break;

            case SceneMgr.Mode.FRIENDLY:
                if (!this.IsChoosingHeroForTavernBrawlChallenge())
                {
                    AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_Custom_Friendly" : "HeroPicker_FriendlyAndCustom_phone", new AssetLoader.ObjectCallback(this.SetCustomTrayTexture), null, false);
                    AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_Friendly" : "HeroPicker_FriendlyAndCustom_phone", new AssetLoader.ObjectCallback(this.SetTrayTexture), null, false);
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.m_detailsTrayFrame.GetComponent<MeshFilter>().mesh = this.m_alternateDetailsTrayMesh;
                        AssetLoader.Get().LoadTexture("Friendly_DeckHeroTray_phone", new AssetLoader.ObjectCallback(this.SetDetailsTrayTexture), null, false);
                    }
                    this.m_keyholeTextureOffset = new UnityEngine.Vector2(0f, 0.61f);
                    break;
                }
                this.SetTexturesForTavernBrawl();
                break;

            case SceneMgr.Mode.ADVENTURE:
                AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_Custom_Practice" : "HeroPicker_PracticeAndCustom_phone", new AssetLoader.ObjectCallback(this.SetCustomTrayTexture), null, false);
                AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_Practice" : "HeroPicker_PracticeAndCustom_phone", new AssetLoader.ObjectCallback(this.SetTrayTexture), null, false);
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    this.m_detailsTrayFrame.GetComponent<MeshFilter>().mesh = this.m_alternateDetailsTrayMesh;
                    AssetLoader.Get().LoadTexture("Practice_DeckHeroTray_phone", new AssetLoader.ObjectCallback(this.SetDetailsTrayTexture), null, false);
                }
                this.m_keyholeTextureOffset = new UnityEngine.Vector2(0.5f, 0f);
                break;

            case SceneMgr.Mode.TAVERN_BRAWL:
                this.SetTexturesForTavernBrawl();
                break;
        }
        this.m_xpBar = UnityEngine.Object.Instantiate<HeroXPBar>(this.m_xpBarPrefab);
        this.m_xpBar.m_soloLevelLimit = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>().XPSoloLimit;
        Navigation.PushUnique(new Navigation.NavigateBackHandler(DeckPickerTrayDisplay.OnNavigateBack));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_slidingTray = base.gameObject.GetComponentInChildren<SlidingTray>();
            this.m_slidingTray.RegisterTrayToggleListener(new SlidingTray.TrayToggledListener(this.OnSlidingTrayToggled));
        }
        base.StartCoroutine(this.InitModeWhenReady());
    }

    private void BackButtonPress(UIEvent e)
    {
        Navigation.GoBack();
    }

    private static void BackOutToHub()
    {
        if (!IsBackingOut())
        {
            if (Get() != null)
            {
                FriendChallengeMgr.Get().RemoveChangedListener(new FriendChallengeMgr.ChangedCallback(Get().OnFriendChallengeChanged));
            }
            if ((Get() != null) && Get().m_showingCustomDecks)
            {
                Get().SuckInPreconDecks();
            }
            else
            {
                SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
            }
            if (UniversalInputManager.UsePhoneUI != null)
            {
                Get().m_slidingTray.ToggleTraySlider(false, null, true);
            }
        }
    }

    private void Deselect()
    {
        if ((this.m_selectedHeroButton != null) || (this.m_selectedCustomDeckBox != null))
        {
            this.EnablePlayButton(false);
            if (this.m_selectedCustomDeckBox != null)
            {
                this.m_selectedCustomDeckBox.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
                this.m_selectedCustomDeckBox.SetEnabled(true);
                this.m_selectedCustomDeckBox = null;
            }
            this.m_heroActor.SetEntityDef(null);
            this.m_heroActor.SetCardDef(null);
            this.m_heroActor.Hide();
            if (this.m_selectedHeroButton != null)
            {
                this.m_selectedHeroButton.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
                this.m_selectedHeroButton.SetSelected(false);
                this.m_selectedHeroButton = null;
            }
            if (this.ShouldShowHeroPower())
            {
                this.m_heroPowerActor.SetCardDef(null);
                this.m_heroPowerActor.SetEntityDef(null);
                this.m_heroPowerActor.Hide();
                this.m_goldenHeroPowerActor.SetCardDef(null);
                this.m_goldenHeroPowerActor.SetEntityDef(null);
                this.m_goldenHeroPowerActor.Hide();
                this.m_heroPower.GetComponent<Collider>().enabled = false;
                this.m_goldenHeroPower.GetComponent<Collider>().enabled = false;
                if (this.m_heroPowerShadowQuad != null)
                {
                    this.m_heroPowerShadowQuad.SetActive(false);
                }
            }
            this.m_selectedHeroPowerFullDef = null;
            if (this.m_heroPowerBigCard != null)
            {
                iTween.Stop(this.m_heroPowerBigCard.gameObject);
                this.m_heroPowerBigCard.Hide();
            }
            if (this.m_goldenHeroPowerBigCard != null)
            {
                iTween.Stop(this.m_goldenHeroPowerBigCard.gameObject);
                this.m_goldenHeroPowerBigCard.Hide();
            }
            this.m_selectedHeroName = null;
            this.m_heroName.Text = string.Empty;
        }
    }

    private void DeselectLastSelectedHero()
    {
        if (this.m_selectedHeroButton != null)
        {
            this.m_selectedHeroButton.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
            this.m_selectedHeroButton.SetSelected(false);
        }
    }

    private void DisableHeroButtons()
    {
        foreach (HeroPickerButton button in this.m_heroButtons)
        {
            button.SetEnabled(false);
        }
    }

    private void EnableBackButton(bool enable)
    {
        if (DemoMgr.Get().IsExpoDemo())
        {
            if (enable)
            {
                return;
            }
            enable = false;
        }
        this.m_backButton.SetEnabled(enable);
    }

    private void EnableExpoClickBlocker(bool enable)
    {
        if (this.m_expoClickBlocker != null)
        {
            FullScreenFXMgr mgr = FullScreenFXMgr.Get();
            if (enable)
            {
                mgr.SetBlurBrightness(1f);
                mgr.SetBlurDesaturation(0f);
                mgr.Vignette(0.4f, 0.4f, iTween.EaseType.easeOutCirc, null);
                mgr.Blur(1f, 0.4f, iTween.EaseType.easeOutCirc, null);
            }
            else
            {
                mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, null);
                mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
            }
            this.m_expoClickBlocker.gameObject.SetActive(enable);
        }
    }

    private void EnableHeroButtons()
    {
        foreach (HeroPickerButton button in this.m_heroButtons)
        {
            button.SetEnabled(true);
        }
    }

    private void EnablePlayButton(bool enable)
    {
        if (enable)
        {
            if ((SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY) || FriendChallengeMgr.Get().HasChallenge())
            {
                this.m_playButton.Enable();
            }
        }
        else
        {
            this.m_playButton.Disable();
        }
    }

    private void FireDeckTrayLoadedEvent()
    {
        foreach (DeckTrayLoaded loaded in this.m_DeckTrayLoadedListeners.ToArray())
        {
            loaded();
        }
    }

    public static DeckPickerTrayDisplay Get()
    {
        return s_instance;
    }

    private CollectionDeckBoxVisual GetDeckboxWithDeckID(long deckID)
    {
        if (deckID > 0L)
        {
            foreach (CollectionDeckBoxVisual visual in this.m_customDecks)
            {
                if (visual.GetDeckID() == deckID)
                {
                    return visual;
                }
            }
        }
        return null;
    }

    private DeckPickerMode GetInitialMode()
    {
        if (!DemoMgr.Get().ShowOnlyCustomDecks())
        {
            if (((SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER) || (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL)) || this.IsChoosingHeroForTavernBrawlChallenge())
            {
                return DeckPickerMode.PRECON;
            }
            if (this.m_numCustomDecks == 0)
            {
                return DeckPickerMode.PRECON;
            }
            if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
            {
                return Options.Get().GetEnum<DeckPickerMode>(Option.DECK_PICKER_MODE, DeckPickerMode.CUSTOM);
            }
        }
        return DeckPickerMode.CUSTOM;
    }

    private HeroPickerButton GetPreconButtonForClass(TAG_CLASS classType)
    {
        foreach (HeroPickerButton button in this.m_heroButtons)
        {
            if (button.GetFullDef().GetEntityDef().GetClass() == classType)
            {
                return button;
            }
        }
        return null;
    }

    public long GetSelectedDeckID()
    {
        if (this.m_showingCustomDecks)
        {
            return ((this.m_selectedCustomDeckBox != null) ? this.m_selectedCustomDeckBox.GetDeckID() : 0L);
        }
        return ((this.m_selectedHeroButton != null) ? this.m_selectedHeroButton.GetPreconDeckID() : 0L);
    }

    public int GetSelectedHeroLevel()
    {
        if (this.m_selectedHeroButton == null)
        {
            return 0;
        }
        return GameUtils.GetHeroLevel(this.m_selectedHeroButton.GetFullDef().GetEntityDef().GetClass()).CurrentLevel.Level;
    }

    private void GoBackUntilOnNavigateBackCalled()
    {
        while (Navigation.BackStackContainsHandler(new Navigation.NavigateBackHandler(DeckPickerTrayDisplay.OnNavigateBack)))
        {
            if (!Navigation.GoBack())
            {
                break;
            }
        }
    }

    public void HandleGameStartupFailure()
    {
        this.EnablePlayButton(true);
        this.EnableBackButton(true);
        this.EnableHeroButtons();
        SceneMgr.Mode mode = SceneMgr.Get().GetMode();
        SceneMgr.Mode mode2 = mode;
        if (mode2 != SceneMgr.Mode.TOURNAMENT)
        {
            if (mode2 != SceneMgr.Mode.ADVENTURE)
            {
                UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.HandleGameStartupFailure(): don't know how to handle mode {0}", mode));
            }
            else if (AdventureConfig.Get().GetCurrentSubScene() == AdventureSubScenes.Practice)
            {
                PracticePickerTrayDisplay.Get().OnGameDenied();
            }
        }
        else
        {
            PresenceMgr.Get().SetPrevStatus();
        }
    }

    private void HeroPressed(UIEvent e)
    {
        HeroPickerButton element = (HeroPickerButton) e.GetElement();
        this.SelectHero(element, true);
        SoundManager.Get().LoadAndPlay("tournament_screen_select_hero");
        this.HideDemoQuotes();
    }

    private void HideAllPreconHighlights()
    {
        foreach (HeroPickerButton button in this.m_heroButtons)
        {
            button.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
        }
    }

    private void HideDemoQuotes()
    {
        if (DemoMgr.Get().IsExpoDemo())
        {
            base.StopCoroutine("ShowDemoQuotes");
            if (this.m_expoThankQuote != null)
            {
                NotificationManager.Get().DestroyNotification(this.m_expoThankQuote, 0f);
                this.m_expoThankQuote = null;
                FullScreenFXMgr mgr = FullScreenFXMgr.Get();
                mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, null);
                mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
            }
            this.HideIntroQuote();
        }
    }

    private void HideIntroQuote()
    {
        if (this.m_expoIntroQuote != null)
        {
            NotificationManager.Get().DestroyNotification(this.m_expoIntroQuote, 0f);
            this.m_expoIntroQuote = null;
        }
    }

    public void Init()
    {
        this.InitHeroPickerButtons();
    }

    [DebuggerHidden]
    private IEnumerator InitButtonAchievements()
    {
        return new <InitButtonAchievements>c__Iterator5C { <>f__this = this };
    }

    private void InitCustomDecks()
    {
        int num = 0;
        Vector3 customDeckStart = this.m_customDeckStart;
        float customDeckHorizontalSpacing = this.m_customDeckHorizontalSpacing;
        float customDeckVerticalSpacing = this.m_customDeckVerticalSpacing;
        while (num < 9)
        {
            <InitCustomDecks>c__AnonStorey2EC storeyec = new <InitCustomDecks>c__AnonStorey2EC {
                <>f__this = this
            };
            GameObject obj2 = new GameObject {
                name = "DeckParent" + num
            };
            obj2.transform.parent = this.m_customTray.transform;
            if (num == 0)
            {
                obj2.transform.localPosition = customDeckStart;
            }
            else
            {
                float x = customDeckStart.x - ((num % 3) * customDeckHorizontalSpacing);
                float z = (Mathf.CeilToInt((float) (num / 3)) * customDeckVerticalSpacing) + customDeckStart.z;
                obj2.transform.localPosition = new Vector3(x, customDeckStart.y, z);
            }
            storeyec.deckBox = UnityEngine.Object.Instantiate<CollectionDeckBoxVisual>(this.m_deckboxPrefab);
            storeyec.deckBox.name = storeyec.deckBox.name + " - " + num;
            storeyec.deckBox.transform.parent = obj2.transform;
            storeyec.deckBox.transform.localPosition = Vector3.zero;
            storeyec.deckBox.SetOriginalButtonPosition();
            obj2.transform.localScale = this.m_customDeckScale;
            storeyec.deckBox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storeyec.<>m__DC));
            storeyec.deckBox.SetEnabled(true);
            this.m_customDecks.Add(storeyec.deckBox);
            GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.m_deckboxCoverPrefab);
            item.transform.parent = this.m_customTray.transform;
            item.transform.localScale = this.m_customDeckScale;
            item.transform.position = obj2.transform.position + this.m_deckCoverOffset;
            item.transform.GetChild(0).GetComponent<Renderer>().materials[0].mainTexture = this.m_customTrayTexture;
            this.m_deckCovers.Add(item);
            num++;
        }
        int num6 = 0;
        this.m_numCustomDecks = 0;
        foreach (CollectionDeck deck in CollectionManager.Get().GetDecks().Values)
        {
            if (deck.Type == DeckType.NORMAL_DECK)
            {
                this.m_numCustomDecks++;
                CollectionDeckBoxVisual visual = this.m_customDecks[num6];
                visual.SetDeckName(deck.Name);
                visual.SetDeckID(deck.ID);
                visual.SetHeroCardID(deck.HeroCardID);
                visual.SetIsValid(deck.IsTourneyValid);
                object[] args = new object[] { GameUtils.GetCardSetFromCardID(deck.HeroCardID) == TAG_CARD_SET.HERO_SKINS };
                Log.Kyle.Print("InitCustomDecks - is Hero Skin: {0}", args);
                visual.Show();
                this.m_deckCovers[num6].SetActive(false);
                num6++;
                if (num6 >= this.m_customDecks.Count)
                {
                    break;
                }
            }
        }
        while (num6 < this.m_customDecks.Count)
        {
            this.m_customDecks[num6].Hide();
            this.m_deckCovers[num6].SetActive(true);
            num6++;
        }
    }

    private void InitExpoDemoMode()
    {
        if (DemoMgr.Get().IsExpoDemo())
        {
            this.m_leftArrow.Activate(false);
            this.m_rightArrow.Activate(false);
            if (this.m_modeLabel != null)
            {
                this.m_modeLabel.gameObject.SetActive(false);
            }
            this.EnableBackButton(false);
            base.StartCoroutine("ShowDemoQuotes");
        }
    }

    private void InitHeroPickerButtons()
    {
        int num = 0;
        Vector3 heroPickerButtonStart = this.m_heroPickerButtonStart;
        float heroPickerButtonHorizontalSpacing = this.m_heroPickerButtonHorizontalSpacing;
        float heroPickerButtonVerticalSpacing = this.m_heroPickerButtonVerticalSpacing;
        while (num < 9)
        {
            GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(this.m_heroPrefab);
            obj2.name = string.Format("HeroButtonParent{0} (formerly {1})", num, obj2.name);
            obj2.transform.parent = this.m_randomDeckPickerTray.transform;
            obj2.transform.localScale = this.m_heroPickerButtonScale;
            if (num == 0)
            {
                obj2.transform.localPosition = heroPickerButtonStart;
            }
            else
            {
                float x = heroPickerButtonStart.x - ((num % 3) * heroPickerButtonHorizontalSpacing);
                float z = (Mathf.CeilToInt((float) (num / 3)) * heroPickerButtonVerticalSpacing) + heroPickerButtonStart.z;
                obj2.transform.localPosition = new Vector3(x, heroPickerButtonStart.y, z);
            }
            HeroPickerButton component = obj2.transform.FindChild("HeroPremade_Frame").gameObject.GetComponent<HeroPickerButton>();
            int index = (UniversalInputManager.UsePhoneUI == null) ? 0 : 1;
            component.m_buttonFrame.GetComponent<Renderer>().materials[index].mainTextureOffset = this.m_keyholeTextureOffset;
            this.m_heroButtons.Add(component);
            num++;
        }
        int num7 = 0;
        this.m_heroDefsLoading = this.m_heroButtons.Count;
        this.m_heroPowerDefsLoading = this.m_heroButtons.Count;
        foreach (HeroPickerButton button2 in this.m_heroButtons)
        {
            if (num7 >= this.HERO_CLASSES.Count)
            {
                Log.Derek.Print("TournamentDisplay - more buttons than heroes", new object[0]);
                break;
            }
            button2.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.HeroPressed));
            button2.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.MouseOverHero));
            button2.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.MouseOutHero));
            button2.SetOriginalLocalPosition();
            button2.Lock();
            button2.SetProgress(0, 0, 1);
            NetCache.CardDefinition favoriteHero = CollectionManager.Get().GetFavoriteHero(this.HERO_CLASSES[num7]);
            HeroFullDefLoadedCallbackData userData = new HeroFullDefLoadedCallbackData(button2, new CardFlair(favoriteHero.Premium));
            DefLoader.Get().LoadFullDef(favoriteHero.Name, new DefLoader.LoadDefCallback<FullDef>(this.OnHeroFullDefLoaded), userData);
            num7++;
        }
    }

    private void InitMode()
    {
        this.InitRichPresence();
        if (this.GetInitialMode() == DeckPickerMode.PRECON)
        {
            this.ShowPreconDecks();
        }
        else
        {
            this.ShowCustomDecks();
        }
        if (((SceneMgr.Get().GetMode() != SceneMgr.Mode.COLLECTIONMANAGER) && (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL)) && !this.IsChoosingHeroForTavernBrawlChallenge())
        {
            this.SetSelectionFromOptions();
        }
        this.InitExpoDemoMode();
    }

    [DebuggerHidden]
    private IEnumerator InitModeWhenReady()
    {
        return new <InitModeWhenReady>c__Iterator60 { <>f__this = this };
    }

    private void InitRichPresence()
    {
        PresenceStatus? nullable = null;
        switch (SceneMgr.Get().GetMode())
        {
            case SceneMgr.Mode.TOURNAMENT:
                nullable = 11;
                break;

            case SceneMgr.Mode.FRIENDLY:
                nullable = 0x16;
                if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
                {
                    nullable = 0x2a;
                }
                break;

            case SceneMgr.Mode.ADVENTURE:
                if (AdventureConfig.Get().GetCurrentSubScene() == AdventureSubScenes.Practice)
                {
                    nullable = 14;
                    break;
                }
                break;

            case SceneMgr.Mode.TAVERN_BRAWL:
                if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
                {
                    nullable = 0x2a;
                }
                break;
        }
        if (nullable.HasValue)
        {
            Enum[] args = new Enum[] { nullable.Value };
            PresenceMgr.Get().SetStatus(args);
        }
    }

    private static bool IsBackingOut()
    {
        return SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB);
    }

    private bool IsChoosingHeroForTavernBrawlChallenge()
    {
        return ((SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY) && FriendChallengeMgr.Get().IsChallengeTavernBrawl());
    }

    public bool IsLoaded()
    {
        return this.m_Loaded;
    }

    public bool IsShowingCustomDecks()
    {
        return this.m_showingCustomDecks;
    }

    private void LoadGoldenHeroPower()
    {
        AssetLoader.Get().LoadActor(ActorNames.GetNameWithPremiumType("Card_Play_HeroPower", TAG_PREMIUM.GOLDEN), new AssetLoader.GameObjectCallback(this.OnGoldenHeroPowerActorLoaded), null, false);
    }

    private void LoadGoldenHeroPowerCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.LoadHeroPowerCallback() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.LoadHeroPowerCallback() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                component.transform.parent = this.m_heroPower.transform;
                component.TurnOffCollider();
                SceneUtils.SetLayer(component.gameObject, this.m_heroPower.gameObject.layer);
                UberText powersText = component.GetPowersText();
                if (powersText != null)
                {
                    TransformUtil.SetLocalPosY(powersText.gameObject, powersText.transform.localPosition.y + 0.1f);
                }
                this.m_goldenHeroPowerBigCard = component;
                this.ShowGoldenHeroPowerBigCard();
            }
        }
    }

    private void LoadHero()
    {
        AssetLoader.Get().LoadActor("Card_Play_Hero", new AssetLoader.GameObjectCallback(this.OnHeroActorLoaded), null, false);
    }

    private void LoadHeroPower()
    {
        AssetLoader.Get().LoadActor("Card_Play_HeroPower", new AssetLoader.GameObjectCallback(this.OnHeroPowerActorLoaded), null, false);
    }

    private void LoadHeroPowerCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.LoadHeroPowerCallback() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.LoadHeroPowerCallback() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                component.transform.parent = this.m_heroPower.transform;
                component.TurnOffCollider();
                SceneUtils.SetLayer(component.gameObject, this.m_heroPower.gameObject.layer);
                UberText powersText = component.GetPowersText();
                if (powersText != null)
                {
                    TransformUtil.SetLocalPosY(powersText.gameObject, powersText.transform.localPosition.y + 0.1f);
                }
                this.m_heroPowerBigCard = component;
                this.ShowHeroPowerBigCard();
            }
        }
    }

    private void LowerHero()
    {
        this.m_xpBar.SetEnabled(false);
        if (this.m_heroActor.gameObject.activeInHierarchy)
        {
            object[] args = new object[] { "position", this.m_Hero_BoneDown.localPosition, "time", 0.25f, "easeType", iTween.EaseType.easeOutExpo, "islocal", true };
            Hashtable hashtable = iTween.Hash(args);
            iTween.MoveTo(this.m_heroActor.gameObject, hashtable);
            object[] objArray2 = new object[] { "position", this.m_HeroPower_BoneDown.localPosition, "time", 0.25f, "easeType", iTween.EaseType.easeOutExpo, "islocal", true };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            if (this.ShouldShowHeroPower())
            {
                iTween.MoveTo(this.m_heroPowerActor.gameObject, hashtable2);
                this.m_heroPower.GetComponent<Collider>().enabled = false;
                if (this.m_goldenHeroPowerActor != null)
                {
                    object[] objArray3 = new object[] { "position", this.m_HeroPower_BoneDown.localPosition, "time", 0.25f, "easeType", iTween.EaseType.easeOutExpo, "islocal", true };
                    Hashtable hashtable3 = iTween.Hash(objArray3);
                    iTween.MoveTo(this.m_goldenHeroPowerActor.gameObject, hashtable3);
                    this.m_goldenHeroPower.GetComponent<Collider>().enabled = false;
                }
            }
        }
    }

    private void LowerHeroButtons()
    {
        foreach (HeroPickerButton button in this.m_heroButtons)
        {
            if (button.gameObject.activeSelf)
            {
                button.Lower();
            }
        }
        this.m_customTray.GetComponent<Renderer>().enabled = true;
        for (int i = 0; i < this.m_numCustomDecks; i++)
        {
            if (i < this.m_customDecks.Count)
            {
                this.m_customDecks[i].Show();
            }
        }
    }

    private void MouseOutHero(UIEvent e)
    {
        HeroPickerButton element = (HeroPickerButton) e.GetElement();
        if ((UniversalInputManager.UsePhoneUI == null) || !element.IsSelected())
        {
            element.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
        }
    }

    private void MouseOutHeroPower(UIEvent e)
    {
        if (this.m_heroPowerBigCard != null)
        {
            iTween.Stop(this.m_heroPowerBigCard.gameObject);
            this.m_heroPowerBigCard.Hide();
        }
        if (this.m_goldenHeroPowerBigCard != null)
        {
            iTween.Stop(this.m_goldenHeroPowerBigCard.gameObject);
            this.m_goldenHeroPowerBigCard.Hide();
        }
    }

    private void MouseOverHero(UIEvent e)
    {
        if (e != null)
        {
            ((HeroPickerButton) e.GetElement()).SetHighlightState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
            SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over");
        }
    }

    private void MouseOverHeroPower(UIEvent e)
    {
        if (this.m_heroActor.GetCardFlair().Premium == TAG_PREMIUM.GOLDEN)
        {
            if (this.m_goldenHeroPowerBigCard == null)
            {
                AssetLoader.Get().LoadActor(ActorNames.GetNameWithPremiumType("History_HeroPower", TAG_PREMIUM.GOLDEN), new AssetLoader.GameObjectCallback(this.LoadGoldenHeroPowerCallback), null, false);
            }
            else
            {
                this.ShowGoldenHeroPowerBigCard();
            }
        }
        else if (this.m_heroPowerBigCard == null)
        {
            AssetLoader.Get().LoadActor("History_HeroPower", new AssetLoader.GameObjectCallback(this.LoadHeroPowerCallback), null, false);
        }
        else
        {
            this.ShowHeroPowerBigCard();
        }
    }

    public void OnApplicationPause(bool pauseStatus)
    {
        if (GameMgr.Get().IsFindingGame())
        {
            GameMgr.Get().CancelFindGame();
        }
    }

    private void OnBoxTransitionFinished(object userData)
    {
        this.m_randomDeckPickerTray.SetActive(true);
        Box.Get().RemoveTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxTransitionFinished));
    }

    private void OnDestroy()
    {
        this.HideDemoQuotes();
        DeckPickerTray.Get().UnregisterHandlers();
        if (TournamentDisplay.Get() != null)
        {
            TournamentDisplay.Get().RemoveMedalChangedListener(new TournamentDisplay.DelMedalChanged(this.OnMedalChanged));
        }
        s_instance = null;
    }

    private void OnFriendChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, object userData)
    {
        if (challengeEvent == FriendChallengeEvent.SELECTED_DECK)
        {
            if (((SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL) && (player != BnetPresenceMgr.Get().GetMyPlayer())) && FriendChallengeMgr.Get().DidISelectDeck())
            {
                FriendlyChallengeHelper.Get().HideFriendChallengeWaitingForOpponentDialog();
                FriendlyChallengeHelper.Get().WaitForFriendChallengeToStart();
            }
        }
        else if ((challengeEvent == FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE) || (challengeEvent == FriendChallengeEvent.OPPONENT_REMOVED_FROM_FRIENDS))
        {
            FriendlyChallengeHelper.Get().StopWaitingForFriendChallenge();
            this.GoBackUntilOnNavigateBackCalled();
        }
    }

    private void OnFriendChallengeWaitingForOpponentDialogResponse(AlertPopup.Response response, object userData)
    {
        if (response == AlertPopup.Response.CANCEL)
        {
            this.EnableHeroButtons();
            this.Deselect();
            FriendChallengeMgr.Get().DeselectDeck();
            FriendlyChallengeHelper.Get().StopWaitingForFriendChallenge();
        }
    }

    private void OnGoldenHeroPowerActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.OnHeroPowerActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            this.m_goldenHeroPowerActor = actorObject.GetComponent<Actor>();
            if (this.m_goldenHeroPowerActor == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.OnHeroPowerActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                this.m_goldenHeroPower = actorObject.AddComponent<PegUIElement>();
                actorObject.AddComponent<BoxCollider>();
                actorObject.transform.parent = this.m_hierarchyDetails.transform;
                actorObject.transform.localScale = this.m_HeroPower_Bone.localScale;
                actorObject.transform.localPosition = this.m_HeroPower_Bone.localPosition;
                this.m_goldenHeroPowerActor.SetUnlit();
                this.m_goldenHeroPowerActor.SetCardFlair(new CardFlair(TAG_PREMIUM.GOLDEN));
                this.m_goldenHeroPower.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.MouseOverHeroPower));
                this.m_goldenHeroPower.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.MouseOutHeroPower));
                this.m_goldenHeroPowerActor.Hide();
                this.m_goldenHeroPower.GetComponent<Collider>().enabled = false;
                this.m_heroName.Text = string.Empty;
            }
        }
    }

    private void OnHeroActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.OnHeroActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            this.m_heroActor = actorObject.GetComponent<Actor>();
            if (this.m_heroActor == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.OnHeroActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                actorObject.transform.parent = this.m_hierarchyDetails.transform;
                actorObject.transform.localScale = this.m_Hero_Bone.localScale;
                actorObject.transform.localPosition = this.m_Hero_Bone.localPosition;
                this.m_heroActor.SetUnlit();
                UnityEngine.Object.Destroy(this.m_heroActor.m_healthObject);
                UnityEngine.Object.Destroy(this.m_heroActor.m_attackObject);
                this.m_xpBar.transform.parent = this.m_heroActor.GetRootObject().transform;
                this.m_xpBar.transform.localScale = new Vector3(0.89f, 0.89f, 0.89f);
                this.m_xpBar.transform.localPosition = new Vector3(-0.1776525f, 0.2245596f, -0.7309282f);
                this.m_xpBar.m_isOnDeck = false;
                this.m_heroActor.Hide();
            }
        }
    }

    private void OnHeroFullDefLoaded(string cardId, FullDef fullDef, object userData)
    {
        EntityDef entityDef = fullDef.GetEntityDef();
        HeroFullDefLoadedCallbackData data = userData as HeroFullDefLoadedCallbackData;
        CardFlair cardFlair = (fullDef.GetEntityDef().GetCardSet() != TAG_CARD_SET.HERO_SKINS) ? CollectionManager.Get().GetBestHeroFlairOwned(cardId) : new CardFlair(TAG_PREMIUM.GOLDEN);
        if (cardFlair == null)
        {
            cardFlair = data.HeroFlair;
        }
        data.HeroPickerButton.UpdateDisplay(fullDef, cardFlair);
        data.HeroPickerButton.SetOriginalLocalPosition();
        string heroPowerCardIdFromHero = GameUtils.GetHeroPowerCardIdFromHero(entityDef.GetCardId());
        DefLoader.Get().LoadFullDef(heroPowerCardIdFromHero, new DefLoader.LoadDefCallback<FullDef>(this.OnHeroPowerFullDefLoaded));
        this.m_heroDefsLoading--;
        if (this.m_heroDefsLoading <= 0)
        {
            base.StartCoroutine(this.InitButtonAchievements());
        }
    }

    private void OnHeroPowerActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.OnHeroPowerActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            this.m_heroPowerActor = actorObject.GetComponent<Actor>();
            if (this.m_heroPowerActor == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.OnHeroPowerActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                this.m_heroPower = actorObject.AddComponent<PegUIElement>();
                actorObject.AddComponent<BoxCollider>();
                actorObject.transform.parent = this.m_hierarchyDetails.transform;
                actorObject.transform.localScale = this.m_HeroPower_Bone.localScale;
                actorObject.transform.localPosition = this.m_HeroPower_Bone.localPosition;
                this.m_heroPowerActor.SetUnlit();
                this.m_heroPower.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.MouseOverHeroPower));
                this.m_heroPower.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.MouseOutHeroPower));
                this.m_heroPowerActor.Hide();
                this.m_heroPower.GetComponent<Collider>().enabled = false;
                this.m_heroName.Text = string.Empty;
                base.StartCoroutine(this.UpdateHeroSkinHeroPower());
            }
        }
    }

    private void OnHeroPowerFullDefLoaded(string cardId, FullDef def, object userData)
    {
        this.m_heroPowerDefs[cardId] = def;
        this.m_heroPowerDefsLoading--;
    }

    private void OnMedalChanged(NetCache.NetCacheMedalInfo medalInfo)
    {
        this.m_rankedPlayButtons.SetRankedMedal(medalInfo);
    }

    private static bool OnNavigateBack()
    {
        if ((Get() != null) && !Get().m_backButton.IsEnabled())
        {
            return false;
        }
        SceneMgr.Mode mode = SceneMgr.Get().GetMode();
        switch (mode)
        {
            case SceneMgr.Mode.COLLECTIONMANAGER:
            case SceneMgr.Mode.TAVERN_BRAWL:
                if (CollectionDeckTray.Get() != null)
                {
                    CollectionDeckTray.Get().GetDecksContent().CreateNewDeckCancelled();
                }
                HeroPickerDisplay.Get().HideTray(0f);
                PresenceMgr.Get().SetPrevStatus();
                if (mode == SceneMgr.Mode.TAVERN_BRAWL)
                {
                    TavernBrawlDisplay.Get().EnablePlayButton();
                }
                break;

            case SceneMgr.Mode.TOURNAMENT:
                BackOutToHub();
                GameMgr.Get().CancelFindGame();
                break;

            case SceneMgr.Mode.FRIENDLY:
                BackOutToHub();
                FriendChallengeMgr.Get().CancelChallenge();
                break;

            case SceneMgr.Mode.ADVENTURE:
                AdventureConfig.Get().ChangeToLastSubScene(true);
                if (AdventureConfig.Get().GetCurrentSubScene() == AdventureSubScenes.Practice)
                {
                    PracticePickerTrayDisplay.Get().gameObject.SetActive(false);
                }
                GameMgr.Get().CancelFindGame();
                break;
        }
        return true;
    }

    public void OnServerGameCanceled()
    {
        if ((SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY) && !TavernBrawlManager.IsInTavernBrawlFriendlyChallenge())
        {
            this.HandleGameStartupFailure();
        }
    }

    public void OnServerGameStarted()
    {
        FriendChallengeMgr.Get().RemoveChangedListener(new FriendChallengeMgr.ChangedCallback(this.OnFriendChallengeChanged));
    }

    private void OnShowCustomDecks(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("hero_panel_slide_off");
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_TOGGLE_DECK_TYPE);
        this.ShowCustomDecks();
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.SetSelectionFromOptions();
        }
    }

    private void OnShowPreconDecks(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("hero_panel_slide_on");
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_TOGGLE_DECK_TYPE);
        this.ShowPreconDecks();
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.SetSelectionFromOptions();
        }
    }

    private void OnSlidingTrayToggled(bool isShowing)
    {
        if ((!isShowing && (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE)) && ((AdventureConfig.Get().GetCurrentSubScene() == AdventureSubScenes.Practice) && (PracticePickerTrayDisplay.Get() != null)))
        {
            PracticePickerTrayDisplay.Get().Hide();
        }
    }

    private void OnTransitionFromGameplayFinished(bool cutoff, object userData)
    {
        if ((SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY) && !FriendChallengeMgr.Get().HasChallenge())
        {
            this.GoBackUntilOnNavigateBackCalled();
        }
        LoadingScreen.Get().UnregisterFinishedTransitionListener(new LoadingScreen.FinishedTransitionCallback(this.OnTransitionFromGameplayFinished));
        this.m_delayButtonAnims = false;
    }

    private void PlayGame()
    {
        if ((UniversalInputManager.UsePhoneUI != null) && ((SceneMgr.Get().GetMode() != SceneMgr.Mode.ADVENTURE) || (AdventureConfig.Get().GetCurrentSubScene() != AdventureSubScenes.Practice)))
        {
            this.m_slidingTray.ToggleTraySlider(false, null, true);
        }
        switch (SceneMgr.Get().GetMode())
        {
            case SceneMgr.Mode.COLLECTIONMANAGER:
                this.SelectHeroForCollectionManager();
                break;

            case SceneMgr.Mode.TOURNAMENT:
            {
                long selectedDeckID = this.GetSelectedDeckID();
                if (selectedDeckID != 0)
                {
                    Network.TrackWhat what;
                    GameType type;
                    this.EnableBackButton(false);
                    if (Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE))
                    {
                        what = !this.m_showingCustomDecks ? Network.TrackWhat.TRACK_PLAY_TOURNAMENT_WITH_PRECON_DECK : Network.TrackWhat.TRACK_PLAY_TOURNAMENT_WITH_CUSTOM_DECK;
                        type = GameType.GT_RANKED;
                    }
                    else
                    {
                        what = !this.m_showingCustomDecks ? Network.TrackWhat.TRACK_PLAY_CASUAL_WITH_PRECON_DECK : Network.TrackWhat.TRACK_PLAY_CASUAL_WITH_CUSTOM_DECK;
                        type = GameType.GT_UNRANKED;
                    }
                    Network.TrackClient(Network.TrackLevel.LEVEL_INFO, what);
                    GameMgr.Get().FindGame(type, 2, selectedDeckID, 0L);
                    Enum[] args = new Enum[] { PresenceStatus.PLAY_QUEUE };
                    PresenceMgr.Get().SetStatus(args);
                    break;
                }
                UnityEngine.Debug.LogError("Trying to play game with deck ID 0!");
                return;
            }
            case SceneMgr.Mode.FRIENDLY:
            {
                long deckId = this.GetSelectedDeckID();
                if (deckId != 0)
                {
                    FriendChallengeMgr.Get().SelectDeck(deckId);
                    FriendlyChallengeHelper.Get().StartChallengeOrWaitForOpponent("GLOBAL_FRIEND_CHALLENGE_OPPONENT_WAITING_DECK", new AlertPopup.ResponseCallback(this.OnFriendChallengeWaitingForOpponentDialogResponse));
                    break;
                }
                UnityEngine.Debug.LogError("Trying to play friendly game with deck ID 0!");
                return;
            }
            case SceneMgr.Mode.ADVENTURE:
            {
                long num3 = this.GetSelectedDeckID();
                AdventureConfig config = AdventureConfig.Get();
                if ((config.GetMission() == ScenarioDbId.NAXX_ANUBREKHAN) && !Options.Get().GetBool(Option.HAS_PLAYED_NAXX))
                {
                    AdTrackingManager.Get().TrackAdventureProgress(Option.HAS_PLAYED_NAXX.ToString());
                    Options.Get().SetBool(Option.HAS_PLAYED_NAXX, true);
                }
                switch (config.GetCurrentSubScene())
                {
                    case AdventureSubScenes.Practice:
                        PracticePickerTrayDisplay.Get().Show();
                        this.LowerHero();
                        break;

                    case AdventureSubScenes.MissionDeckPicker:
                        if (DemoMgr.Get().GetMode() != DemoMode.BLIZZCON_2015)
                        {
                            config.ChangeToLastSubScene(false);
                        }
                        GameMgr.Get().FindGame(GameType.GT_VS_AI, (int) config.GetMission(), num3, 0L);
                        return;
                }
                break;
            }
            case SceneMgr.Mode.TAVERN_BRAWL:
            {
                if (!TavernBrawlManager.Get().SelectHeroBeforeMission())
                {
                    this.SelectHeroForCollectionManager();
                    break;
                }
                long num4 = this.GetSelectedDeckID();
                if (num4 != 0)
                {
                    if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
                    {
                        FriendChallengeMgr.Get().SelectDeck(num4);
                        FriendlyChallengeHelper.Get().StartChallengeOrWaitForOpponent("GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_OPPONENT_WAITING_READY", new AlertPopup.ResponseCallback(this.OnFriendChallengeWaitingForOpponentDialogResponse));
                    }
                    else
                    {
                        TavernBrawlManager.Get().StartGame(num4);
                    }
                    break;
                }
                UnityEngine.Debug.LogError("Trying to play Tavern Brawl game with deck ID 0!");
                return;
            }
        }
    }

    private void PlayGameButtonRelease(UIEvent e)
    {
        this.HideDemoQuotes();
        this.m_playButton.SetEnabled(false);
        this.DisableHeroButtons();
        this.PlayGame();
    }

    public void PreUnload()
    {
        if (this.m_showingCustomDecks && this.m_randomDeckPickerTray.activeSelf)
        {
            this.m_randomDeckPickerTray.SetActive(false);
        }
    }

    private void RaiseHero()
    {
        this.m_xpBar.SetEnabled(true);
        object[] args = new object[] { "position", this.m_Hero_Bone.localPosition, "time", 0.25f, "easeType", iTween.EaseType.easeOutExpo, "islocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_heroActor.gameObject, hashtable);
        object[] objArray2 = new object[] { "position", this.m_HeroPower_Bone.localPosition, "time", 0.25f, "easeType", iTween.EaseType.easeOutExpo, "islocal", true };
        Hashtable hashtable2 = iTween.Hash(objArray2);
        if (this.ShouldShowHeroPower())
        {
            iTween.MoveTo(this.m_heroPowerActor.gameObject, hashtable2);
            this.m_heroPower.GetComponent<Collider>().enabled = true;
            if (this.m_goldenHeroPowerActor != null)
            {
                object[] objArray3 = new object[] { "position", this.m_HeroPower_Bone.localPosition, "time", 0.25f, "easeType", iTween.EaseType.easeOutExpo, "islocal", true };
                Hashtable hashtable3 = iTween.Hash(objArray3);
                iTween.MoveTo(this.m_goldenHeroPowerActor.gameObject, hashtable3);
                this.m_goldenHeroPower.GetComponent<Collider>().enabled = true;
            }
        }
    }

    private void RaiseHeroButtons()
    {
        foreach (HeroPickerButton button in this.m_heroButtons)
        {
            if (button.gameObject.activeSelf)
            {
                button.Raise();
            }
        }
        this.m_customTray.GetComponent<Renderer>().enabled = false;
        for (int i = 0; i < this.m_numCustomDecks; i++)
        {
            if (i < this.m_customDecks.Count)
            {
                this.m_customDecks[i].Hide();
            }
        }
    }

    private void RankedPlayButtonsLoaded(string name, GameObject go, object callbackData)
    {
        this.m_rankedPlayButtons = go.GetComponent<RankedPlayDisplay>();
        this.m_rankedPlayButtons.transform.parent = this.m_hierarchyDetails.transform;
        this.m_rankedPlayButtons.transform.localScale = this.m_rankedPlayButtonsBone.localScale;
        this.m_rankedPlayButtons.transform.localPosition = this.m_rankedPlayButtonsBone.localPosition;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_rankedPlayButtons.SetRankedMedalTransform(this.m_medalBone_phone);
        }
        this.m_rankedPlayButtons.UpdateMode();
        base.StartCoroutine(this.SetRankedMedalWhenReady());
    }

    public void RemoveDeckTrayLoadedListener(DeckTrayLoaded dlg)
    {
        this.m_DeckTrayLoadedListeners.Remove(dlg);
    }

    public void ResetCurrentMode()
    {
        if (this.m_showingCustomDecks)
        {
            if (this.m_selectedCustomDeckBox != null)
            {
                this.EnablePlayButton(true);
                this.RaiseHero();
            }
            else
            {
                if (this.m_selectedHeroButton != null)
                {
                    this.RaiseHero();
                }
                this.EnablePlayButton(false);
            }
        }
        else if (this.m_selectedHeroButton != null)
        {
            this.EnablePlayButton(true);
            this.RaiseHero();
        }
        else
        {
            this.EnablePlayButton(false);
        }
        this.EnableHeroButtons();
    }

    private void SelectCustomDeck(CollectionDeckBoxVisual deckbox, bool showTrayForPhone = true)
    {
        this.HideDemoQuotes();
        if (deckbox.IsValid())
        {
            Options.Get().SetLong(Option.LAST_CUSTOM_DECK_CHOSEN, deckbox.GetDeckID());
            if (HighlightSelectedDeck != null)
            {
                deckbox.SetHighlightState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
            }
            if (UniversalInputManager.UsePhoneUI == null)
            {
                deckbox.SetEnabled(false);
            }
            if ((this.m_selectedCustomDeckBox != null) && (this.m_selectedCustomDeckBox != deckbox))
            {
                this.m_selectedCustomDeckBox.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
                this.m_selectedCustomDeckBox.SetEnabled(true);
            }
            this.m_selectedCustomDeckBox = deckbox;
            this.UpdateHeroInfo(deckbox);
            this.ShowPreconHero(true);
            this.EnablePlayButton(true);
            this.UpdateHeroLockedTooltip(false);
            if ((UniversalInputManager.UsePhoneUI != null) && showTrayForPhone)
            {
                this.m_slidingTray.ToggleTraySlider(true, null, true);
            }
        }
    }

    private void SelectHero(HeroPickerButton button, bool showTrayForPhone = true)
    {
        if ((button != this.m_selectedHeroButton) || (UniversalInputManager.UsePhoneUI != null))
        {
            this.DeselectLastSelectedHero();
            if (HighlightSelectedDeck != null)
            {
                button.SetHighlightState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
            }
            else
            {
                button.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
            }
            this.m_selectedHeroButton = button;
            this.UpdateHeroInfo(button);
            button.SetSelected(true);
            if (((SceneMgr.Get().GetMode() != SceneMgr.Mode.COLLECTIONMANAGER) && (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL)) && (!this.IsChoosingHeroForTavernBrawlChallenge() && !button.IsLocked()))
            {
                TAG_CLASS tag_class = button.GetFullDef().GetEntityDef().GetClass();
                Options.Get().SetInt(Option.LAST_PRECON_HERO_CHOSEN, (int) tag_class);
            }
            this.ShowPreconHero(true);
            if (this.m_tooltip != null)
            {
                UnityEngine.Object.DestroyImmediate(this.m_tooltip.gameObject);
            }
            bool isLocked = button.IsLocked();
            this.EnablePlayButton(!isLocked);
            this.UpdateHeroLockedTooltip(isLocked);
            if ((UniversalInputManager.UsePhoneUI != null) && showTrayForPhone)
            {
                this.m_slidingTray.ToggleTraySlider(true, null, true);
            }
        }
    }

    private void SelectHeroForCollectionManager()
    {
        SoundManager.Get().LoadAndPlay("hero_panel_slide_off");
        HeroPickerDisplay.Get().HideTray((UniversalInputManager.UsePhoneUI == null) ? 0f : 0.25f);
        FullDef fullDef = this.m_selectedHeroButton.GetFullDef();
        CollectionDeckTray.Get().GetDecksContent().CreateNewDeckFromUserSelection(fullDef.GetEntityDef().GetClass(), fullDef.GetEntityDef().GetCardId(), null);
        this.m_backButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.BackButtonPress));
        Navigation.PopUnique(new Navigation.NavigateBackHandler(DeckPickerTrayDisplay.OnNavigateBack));
    }

    private void SetArrowButtonSelected(ArrowModeButton arrow, bool selected)
    {
        arrow.SetEnabled(!selected);
        arrow.ActivateHighlight(selected);
        arrow.GetComponent<Renderer>().material = !selected ? this.m_arrowButtonMaterial : this.m_arrowButtonShadowMaterial;
    }

    private void SetCustomTrayTexture(string name, UnityEngine.Object go, object callbackData)
    {
        Texture texture = go as Texture;
        this.m_customTray.GetComponent<Renderer>().materials[0].mainTexture = texture;
        this.m_customTrayTexture = texture;
        this.InitCustomDecks();
    }

    private void SetDetailsTrayTexture(string name, UnityEngine.Object go, object callbackData)
    {
        Texture texture = go as Texture;
        this.m_detailsTrayFrame.GetComponent<Renderer>().materials[0].mainTexture = texture;
    }

    private void SetHeaderForTavernBrawl()
    {
        if (this.m_labelDecoration != null)
        {
            this.m_labelDecoration.SetActive(false);
        }
        string key = "GLUE_CHOOSE";
        if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
        {
            key = "GLUE_BRAWL_FRIEND";
        }
        else if (TavernBrawlManager.Get().SelectHeroBeforeMission())
        {
            key = "GLUE_BRAWL";
        }
        this.m_playButton.SetText(GameStrings.Get(key));
    }

    public void SetHeaderText(string text)
    {
        this.m_modeName.Text = text;
    }

    public void SetPlayButtonText(string text)
    {
        this.m_playButton.SetText(text);
    }

    [DebuggerHidden]
    private IEnumerator SetRankedMedalWhenReady()
    {
        return new <SetRankedMedalWhenReady>c__Iterator5F { <>f__this = this };
    }

    private void SetSelectionFromOptions()
    {
        if ((UniversalInputManager.UsePhoneUI == null) || (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY))
        {
            if (this.m_showingCustomDecks)
            {
                CollectionDeckBoxVisual deckboxWithDeckID = this.GetDeckboxWithDeckID(Options.Get().GetLong(Option.LAST_CUSTOM_DECK_CHOSEN));
                if (deckboxWithDeckID != null)
                {
                    this.SelectCustomDeck(deckboxWithDeckID, true);
                }
            }
            else
            {
                HeroPickerButton preconButtonForClass = this.GetPreconButtonForClass((TAG_CLASS) Options.Get().GetInt(Option.LAST_PRECON_HERO_CHOSEN));
                if (preconButtonForClass != null)
                {
                    this.SelectHero(preconButtonForClass, true);
                }
            }
        }
    }

    private void SetTexturesForTavernBrawl()
    {
        AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_Custom_Friendly" : "HeroPicker_TavernBrawl_phone", new AssetLoader.ObjectCallback(this.SetCustomTrayTexture), null, false);
        AssetLoader.Get().LoadTexture((UniversalInputManager.UsePhoneUI == null) ? "HeroPicker_TavernBrawl" : "HeroPicker_TavernBrawl_phone", new AssetLoader.ObjectCallback(this.SetTrayTexture), null, false);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_detailsTrayFrame.GetComponent<MeshFilter>().mesh = this.m_alternateDetailsTrayMesh;
            AssetLoader.Get().LoadTexture("TavernBrawl_DeckHeroTray_phone", new AssetLoader.ObjectCallback(this.SetDetailsTrayTexture), null, false);
        }
        this.m_keyholeTextureOffset = new UnityEngine.Vector2(0.5f, 0.61f);
    }

    private void SetTrayTexture(string name, UnityEngine.Object go, object callbackData)
    {
        Texture texture = go as Texture;
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_trayFrame.GetComponentInChildren<Renderer>().materials[0].mainTexture = texture;
        }
        this.m_randomTray.GetComponent<Renderer>().materials[0].mainTexture = texture;
    }

    private bool ShouldHandleBoxTransition()
    {
        if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
        {
            return false;
        }
        if (!Box.Get().IsBusy() && (Box.Get().GetState() != Box.State.LOADING))
        {
            return false;
        }
        return true;
    }

    private bool ShouldShowHeroPower()
    {
        return ((((UniversalInputManager.UsePhoneUI == null) || (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)) || (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL)) || this.IsChoosingHeroForTavernBrawlChallenge());
    }

    private void ShowCustomDecks()
    {
        if (iTween.Count(this.m_randomDeckPickerTray) <= 0)
        {
            this.m_customTray.SetActive(true);
            this.HideAllPreconHighlights();
            this.LowerHeroButtons();
            if (this.ShouldHandleBoxTransition())
            {
                Box.Get().AddTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxTransitionFinished));
                this.m_randomDeckPickerTray.SetActive(false);
                this.m_randomDeckPickerTray.transform.localPosition = this.m_randomDecksHiddenBone.transform.localPosition;
            }
            else
            {
                object[] args = new object[] { "time", 0.25f, "position", this.m_randomDecksHiddenBone.transform.localPosition, "isLocal", true, "delay", 0.2f };
                iTween.MoveTo(this.m_randomDeckPickerTray, iTween.Hash(args));
            }
            this.m_showingCustomDecks = true;
            if (this.m_selectedCustomDeckBox != null)
            {
                this.ShowPreconHero(true);
                this.EnablePlayButton(true);
            }
            else
            {
                this.EnablePlayButton(false);
            }
            if (this.m_modeLabel != null)
            {
                this.m_modeLabel.Text = GameStrings.Get("GLUE_SHOW_CUSTOM_DECKS");
            }
            if (!DemoMgr.Get().ShowOnlyCustomDecks())
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    this.SetArrowButtonSelected(this.m_leftArrow, false);
                    this.SetArrowButtonSelected(this.m_rightArrow, true);
                }
                else
                {
                    base.StartCoroutine(this.ArrowDelayedActivate(this.m_leftArrow, 0.25f));
                    this.m_rightArrow.Activate(false);
                }
            }
            Options.Get().SetEnum<DeckPickerMode>(Option.DECK_PICKER_MODE, DeckPickerMode.CUSTOM);
            Options.Get().SetBool(Option.HAS_SEEN_CUSTOM_DECK_PICKER, true);
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowDemoQuotes()
    {
        return new <ShowDemoQuotes>c__Iterator5D { <>f__this = this };
    }

    private void ShowGoldenHeroPowerBigCard()
    {
        FullDef selectedHeroPowerFullDef = this.m_selectedHeroPowerFullDef;
        if (selectedHeroPowerFullDef != null)
        {
            CardDef cardDef = selectedHeroPowerFullDef.GetCardDef();
            if (cardDef != null)
            {
                this.m_goldenHeroPowerBigCard.SetCardDef(cardDef);
                this.m_goldenHeroPowerBigCard.SetEntityDef(selectedHeroPowerFullDef.GetEntityDef());
                this.m_goldenHeroPowerBigCard.SetCardFlair(new CardFlair(TAG_PREMIUM.GOLDEN));
                this.m_goldenHeroPowerBigCard.UpdateAllComponents();
                this.m_goldenHeroPowerBigCard.Show();
                if (this.m_heroPowerBigCard != null)
                {
                    this.m_heroPowerBigCard.Hide();
                }
                float x = 1f;
                float num2 = 1.5f;
                Vector3 vector = !UniversalInputManager.Get().IsTouchMode() ? new Vector3(0.019f, 0.54f, -1.12f) : new Vector3(0.019f, 0.54f, 3f);
                GameObject gameObject = this.m_goldenHeroPowerBigCard.gameObject;
                gameObject.transform.localPosition = vector;
                this.m_goldenHeroPowerBigCard.transform.localScale = new Vector3(x, x, x);
                iTween.ScaleTo(gameObject, new Vector3(num2, num2, num2), 0.15f);
                Vector3 vector2 = !UniversalInputManager.Get().IsTouchMode() ? new Vector3(0.1f, 0.1f, 0.1f) : new Vector3(0f, 0.1f, 0.1f);
                object[] args = new object[] { "position", vector + vector2, "isLocal", true, "time", 10 };
                iTween.MoveTo(gameObject, iTween.Hash(args));
            }
        }
    }

    private void ShowHero()
    {
        if (this.IsShowingCustomDecks())
        {
            this.UpdateHeroInfo(this.m_selectedCustomDeckBox);
        }
        else
        {
            this.UpdateHeroInfo(this.m_selectedHeroButton);
        }
        this.m_heroActor.Show();
        if (this.ShouldShowHeroPower())
        {
            if (this.m_heroActor.GetCardFlair().Premium == TAG_PREMIUM.GOLDEN)
            {
                this.m_heroPowerActor.Hide();
                this.m_goldenHeroPowerActor.Show();
                this.m_goldenHeroPower.GetComponent<Collider>().enabled = true;
            }
            else
            {
                this.m_goldenHeroPowerActor.Hide();
                this.m_heroPowerActor.Show();
                this.m_heroPower.GetComponent<Collider>().enabled = true;
            }
        }
        if (this.m_selectedHeroName == null)
        {
            this.m_heroName.Text = string.Empty;
        }
    }

    private void ShowHeroPowerBigCard()
    {
        FullDef selectedHeroPowerFullDef = this.m_selectedHeroPowerFullDef;
        if (selectedHeroPowerFullDef != null)
        {
            CardDef cardDef = selectedHeroPowerFullDef.GetCardDef();
            if (cardDef != null)
            {
                this.m_heroPowerBigCard.SetCardDef(cardDef);
                this.m_heroPowerBigCard.SetEntityDef(selectedHeroPowerFullDef.GetEntityDef());
                this.m_heroPowerBigCard.UpdateAllComponents();
                this.m_heroPowerBigCard.Show();
                if (this.m_goldenHeroPowerBigCard != null)
                {
                    this.m_goldenHeroPowerBigCard.Hide();
                }
                this.UpdateCustomHeroPowerBigCard(this.m_heroPowerBigCard.gameObject);
                float num = 1f;
                float num2 = 1.5f;
                Vector3 vector = !UniversalInputManager.Get().IsTouchMode() ? new Vector3(0.019f, 0.54f, -1.12f) : new Vector3(0.019f, 0.54f, 3f);
                GameObject gameObject = this.m_heroPowerBigCard.gameObject;
                Vector3 vector2 = (!UniversalInputManager.Get().IsTouchMode() || (UniversalInputManager.UsePhoneUI != null)) ? new Vector3(0.1f, 0.1f, 0.1f) : new Vector3(0f, 0.1f, 0.1f);
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    gameObject.transform.localPosition = new Vector3(-11.4f, 0.6f, -0.14f);
                    gameObject.transform.localScale = (Vector3) (Vector3.one * 3.2f);
                    Vector3 b = TransformUtil.ComputeWorldScale(gameObject.transform.parent);
                    Vector3 driftOffset = Vector3.Scale((Vector3) (vector2 * 2f), b);
                    AnimationUtil.GrowThenDrift(gameObject, this.m_HeroPower_Bone.transform.position, driftOffset);
                }
                else
                {
                    gameObject.transform.localPosition = vector;
                    gameObject.transform.localScale = (Vector3) (Vector3.one * num);
                    iTween.ScaleTo(gameObject, (Vector3) (Vector3.one * num2), 0.15f);
                    object[] args = new object[] { "position", vector + vector2, "isLocal", true, "time", 10 };
                    iTween.MoveTo(gameObject, iTween.Hash(args));
                }
            }
        }
    }

    private void ShowIntroQuote()
    {
        this.HideIntroQuote();
        string text = Vars.Key("Demo.IntroQuote").GetStr(string.Empty).Replace(@"\n", "\n");
        if (DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2015)
        {
            this.m_expoIntroQuote = NotificationManager.Get().CreateCharacterQuote("Reno_Quote", new Vector3(0f, NotificationManager.DEPTH, -54.22f), text, string.Empty, true, 0f, null, CanvasAnchor.CENTER);
        }
        else
        {
            this.m_expoIntroQuote = NotificationManager.Get().CreateInnkeeperQuote(new Vector3(147.6f, NotificationManager.DEPTH, 23.1f), text, string.Empty, 0f, null);
        }
    }

    private void ShowPreconDecks()
    {
        if (iTween.Count(this.m_randomDeckPickerTray) <= 0)
        {
            this.ShowPreconHighlights();
            this.m_showingCustomDecks = false;
            if (this.m_selectedHeroButton != null)
            {
                this.ShowPreconHero(true);
                this.EnablePlayButton(true);
            }
            else
            {
                this.EnablePlayButton(false);
            }
            SceneMgr.Mode mode = SceneMgr.Get().GetMode();
            if (((mode == SceneMgr.Mode.COLLECTIONMANAGER) || (mode == SceneMgr.Mode.TAVERN_BRAWL)) || this.IsChoosingHeroForTavernBrawlChallenge())
            {
                SoundManager.Get().LoadAndPlay("hero_panel_slide_on");
                this.m_leftArrow.Activate(false);
                this.m_rightArrow.Activate(false);
                if (this.m_modeLabelBg != null)
                {
                    this.m_modeLabelBg.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
                }
                object[] args = new object[] { "time", 0.25f, "position", this.m_randomDecksShownBone.transform.localPosition, "isLocal", true, "oncomplete", "RaiseHeroButtons", "oncompletetarget", base.gameObject };
                iTween.MoveTo(this.m_randomDeckPickerTray, iTween.Hash(args));
            }
            else if (((mode == SceneMgr.Mode.ADVENTURE) || (mode == SceneMgr.Mode.TOURNAMENT)) || (mode == SceneMgr.Mode.FRIENDLY))
            {
                if (this.m_modeLabel != null)
                {
                    this.m_modeLabel.Text = GameStrings.Get("GLUE_SHOW_PRECON_DECKS");
                }
                bool flag = !DemoMgr.Get().IsExpoDemo() && (this.m_numCustomDecks > 0);
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    this.SetArrowButtonSelected(this.m_leftArrow, true);
                    if (flag)
                    {
                        this.SetArrowButtonSelected(this.m_rightArrow, false);
                    }
                    else
                    {
                        this.m_rightArrow.Activate(false);
                    }
                }
                else
                {
                    this.m_leftArrow.Activate(false);
                    if (flag)
                    {
                        base.StartCoroutine(this.ArrowDelayedActivate(this.m_rightArrow, 0.25f));
                        bool highlightOn = !Options.Get().GetBool(Option.HAS_SEEN_CUSTOM_DECK_PICKER, false);
                        this.m_rightArrow.ActivateHighlight(highlightOn);
                    }
                    else
                    {
                        this.m_rightArrow.Activate(false);
                    }
                }
                object[] objArray2 = new object[] { "time", 0.25f, "position", this.m_randomDecksShownBone.transform.localPosition, "isLocal", true, "oncomplete", "RaiseHeroButtons", "oncompletetarget", base.gameObject };
                iTween.MoveTo(this.m_randomDeckPickerTray, iTween.Hash(objArray2));
            }
            Options.Get().SetEnum<DeckPickerMode>(Option.DECK_PICKER_MODE, DeckPickerMode.PRECON);
        }
    }

    private void ShowPreconHero(bool show)
    {
        if ((!show || (SceneMgr.Get().GetMode() != SceneMgr.Mode.ADVENTURE)) || ((AdventureConfig.Get().GetCurrentSubScene() != AdventureSubScenes.Practice) || !PracticePickerTrayDisplay.Get().IsShown()))
        {
            if (show)
            {
                this.ShowHero();
            }
            else
            {
                if (this.m_heroActor != null)
                {
                    this.m_heroActor.Hide();
                }
                if (this.m_heroPowerActor != null)
                {
                    this.m_heroPowerActor.Hide();
                }
                if (this.m_goldenHeroPowerActor != null)
                {
                    this.m_goldenHeroPowerActor.Hide();
                }
                if (this.m_heroPower != null)
                {
                    this.m_heroPower.GetComponent<Collider>().enabled = false;
                }
                if (this.m_goldenHeroPower != null)
                {
                    this.m_goldenHeroPower.GetComponent<Collider>().enabled = false;
                }
                this.m_heroName.Text = string.Empty;
            }
        }
    }

    private void ShowPreconHighlights()
    {
        if (HighlightSelectedDeck != null)
        {
            foreach (HeroPickerButton button in this.m_heroButtons)
            {
                if (button == this.m_selectedHeroButton)
                {
                    button.SetHighlightState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
                }
            }
        }
    }

    private void Start()
    {
        if (this.m_customTray != null)
        {
            this.m_customTray.SetActive(false);
        }
    }

    public void SuckInFinished()
    {
        this.m_randomDeckPickerTray.SetActive(false);
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
    }

    private void SuckInPreconDecks()
    {
        object[] args = new object[] { "time", 0.25f, "position", this.m_suckedInRandomDecksBone.transform.localPosition, "isLocal", true, "oncomplete", "SuckInFinished", "oncompletetarget", base.gameObject };
        iTween.MoveTo(this.m_randomDeckPickerTray, iTween.Hash(args));
    }

    public void ToggleRankedDetailsTray(bool shown)
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_rankedDetailsTray.ToggleTraySlider(shown, null, true);
        }
    }

    public void Unload()
    {
        DeckPickerTray.Get().UnregisterHandlers();
    }

    private void UpdateCustomHeroPowerBigCard(GameObject heroPowerBigCard)
    {
        if (this.m_heroActor.GetCardDef() == null)
        {
            UnityEngine.Debug.LogWarning("DeckPickerTrayDisplay.UpdateCustomHeroPowerBigCard heroCardDef = null!");
        }
        else
        {
            Actor componentInChildren = heroPowerBigCard.GetComponentInChildren<Actor>();
            if (this.m_heroActor.GetEntityDef().GetCardSet() == TAG_CARD_SET.HERO_SKINS)
            {
                componentInChildren.GetCardDef().m_AlwaysRenderPremiumPortrait = true;
            }
            else
            {
                componentInChildren.GetCardDef().m_AlwaysRenderPremiumPortrait = false;
            }
            componentInChildren.UpdateMaterials();
        }
    }

    private void UpdateHeroInfo(CollectionDeckBoxVisual deckBox)
    {
        FullDef fullDef = deckBox.GetFullDef();
        string text = deckBox.GetDeckNameText().Text;
        this.UpdateHeroInfo(fullDef, text, CollectionManager.Get().GetBestHeroFlairOwned(deckBox.GetHeroCardID()));
    }

    private void UpdateHeroInfo(HeroPickerButton button)
    {
        FullDef fullDef = button.GetFullDef();
        string name = fullDef.GetEntityDef().GetName();
        CardFlair cardFlair = button.GetCardFlair();
        this.UpdateHeroInfo(fullDef, name, cardFlair);
    }

    private void UpdateHeroInfo(FullDef fullDef, string heroName, CardFlair cardFlair)
    {
        EntityDef entityDef = fullDef.GetEntityDef();
        CardDef cardDef = fullDef.GetCardDef();
        this.m_heroName.Text = heroName;
        this.m_selectedHeroName = entityDef.GetName();
        this.m_heroActor.SetCardFlair(cardFlair);
        this.m_heroActor.SetEntityDef(entityDef);
        this.m_heroActor.SetCardDef(cardDef);
        this.m_heroActor.UpdateAllComponents();
        this.m_heroActor.SetUnlit();
        this.m_xpBar.m_heroLevel = GameUtils.GetHeroLevel(entityDef.GetClass());
        this.m_xpBar.UpdateDisplay();
        string heroPowerCardIdFromHero = GameUtils.GetHeroPowerCardIdFromHero(entityDef.GetCardId());
        if (this.ShouldShowHeroPower())
        {
            this.UpdateHeroPowerInfo((FullDef) this.m_heroPowerDefs[heroPowerCardIdFromHero], cardFlair);
        }
        this.UpdateRankedClassWinsPlate();
    }

    private void UpdateHeroLockedTooltip(bool isLocked)
    {
        if (this.m_tooltip != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_tooltip.gameObject);
        }
        if (isLocked)
        {
            GameObject go = UnityEngine.Object.Instantiate<GameObject>(this.m_tooltipPrefab);
            SceneUtils.SetLayer(go, GameLayer.Default);
            this.m_tooltip = go.GetComponent<KeywordHelpPanel>();
            this.m_tooltip.Reset();
            this.m_tooltip.Initialize(GameStrings.Get("GLUE_HERO_LOCKED_NAME"), GameStrings.Get("GLUE_HERO_LOCKED_DESC"));
            GameUtils.SetParent((Component) this.m_tooltip, (Component) this.m_tooltipBone, false);
        }
    }

    private void UpdateHeroPowerInfo(FullDef def, CardFlair cardFlair)
    {
        this.m_heroPowerActor.SetCardDef(def.GetCardDef());
        this.m_heroPowerActor.SetEntityDef(def.GetEntityDef());
        this.m_heroPowerActor.UpdateAllComponents();
        this.m_selectedHeroPowerFullDef = def;
        this.m_heroPowerActor.SetUnlit();
        this.m_heroPowerActor.GetCardDef().m_AlwaysRenderPremiumPortrait = false;
        this.m_heroPowerActor.UpdateMaterials();
        this.m_goldenHeroPowerActor.SetCardDef(def.GetCardDef());
        this.m_goldenHeroPowerActor.SetEntityDef(def.GetEntityDef());
        this.m_goldenHeroPowerActor.UpdateAllComponents();
        this.m_selectedHeroPowerFullDef = def;
        this.m_goldenHeroPowerActor.SetUnlit();
        if (cardFlair.Premium == TAG_PREMIUM.GOLDEN)
        {
            this.m_heroPowerActor.Hide();
            this.m_goldenHeroPowerActor.Show();
            this.m_goldenHeroPower.GetComponent<Collider>().enabled = true;
        }
        else
        {
            this.m_goldenHeroPowerActor.Hide();
            this.m_heroPowerActor.Show();
            this.m_heroPower.GetComponent<Collider>().enabled = true;
            if (this.m_heroActor.GetEntityDef().GetCardSet() == TAG_CARD_SET.HERO_SKINS)
            {
                base.StartCoroutine(this.UpdateHeroSkinHeroPower());
            }
        }
        if (this.m_heroPowerShadowQuad != null)
        {
            this.m_heroPowerShadowQuad.SetActive(true);
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateHeroSkinHeroPower()
    {
        return new <UpdateHeroSkinHeroPower>c__Iterator61 { <>f__this = this };
    }

    public bool UpdateRankedClassWinsPlate()
    {
        if (((SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT) && Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE)) && (this.m_heroActor.GetEntityDef() != null))
        {
            string cardId = this.m_heroActor.GetEntityDef().GetCardId();
            if (this.m_heroActor.GetEntityDef().GetCardSet() == TAG_CARD_SET.HERO_SKINS)
            {
                cardId = CollectionManager.Get().GetVanillaHeroCardID(this.m_heroActor.GetEntityDef());
            }
            Achievement unlockGoldenHeroAchievement = AchieveManager.Get().GetUnlockGoldenHeroAchievement(cardId, TAG_PREMIUM.GOLDEN);
            int progress = unlockGoldenHeroAchievement.Progress;
            if ((progress == 0) || (progress >= unlockGoldenHeroAchievement.MaxProgress))
            {
                this.m_rankedWinsPlate.SetActive(false);
                return false;
            }
            object[] args = new object[] { progress, unlockGoldenHeroAchievement.MaxProgress };
            this.m_rankedWins.Text = GameStrings.Format((UniversalInputManager.UsePhoneUI == null) ? "GLOBAL_HERO_WINS" : "GLOBAL_HERO_WINS_PHONE", args);
            this.m_rankedWinsPlate.SetActive(true);
            return true;
        }
        this.m_rankedWinsPlate.SetActive(false);
        return false;
    }

    public void UpdateRankedPlayDisplay()
    {
        this.m_rankedPlayButtons.UpdateMode();
    }

    [CompilerGenerated]
    private sealed class <ArrowDelayedActivate>c__Iterator5E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ArrowModeButton <$>arrow;
        internal float <$>delay;
        internal ArrowModeButton arrow;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.arrow.Activate(true);
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
    private sealed class <InitButtonAchievements>c__Iterator5C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Achievement>.Enumerator <$s_449>__1;
        internal List<Achievement>.Enumerator <$s_450>__6;
        internal DeckPickerTrayDisplay <>f__this;
        internal Achievement <achievement>__2;
        internal Achievement <achievement>__7;
        internal HeroPickerButton <button>__3;
        internal HeroPickerButton <button>__8;
        internal CollectionManager.PreconDeck <preconDeck>__4;
        internal long <preconDeckID>__5;
        internal List<Achievement> <unlockHeroAchieves>__0;

        internal bool <>m__DD(HeroPickerButton obj)
        {
            return (obj.GetFullDef().GetEntityDef().GetClass() == ((TAG_CLASS) this.<achievement>__2.ClassRequirement.Value));
        }

        internal bool <>m__DE(HeroPickerButton obj)
        {
            return (obj.GetFullDef().GetEntityDef().GetClass() == ((TAG_CLASS) this.<achievement>__7.ClassRequirement.Value));
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
                    this.<unlockHeroAchieves>__0 = AchieveManager.Get().GetAchievesInGroup(Achievement.Group.UNLOCK_HERO);
                    this.<$s_449>__1 = this.<unlockHeroAchieves>__0.GetEnumerator();
                    try
                    {
                        while (this.<$s_449>__1.MoveNext())
                        {
                            this.<achievement>__2 = this.<$s_449>__1.Current;
                            this.<button>__3 = this.<>f__this.m_heroButtons.Find(new Predicate<HeroPickerButton>(this.<>m__DD));
                            if (this.<button>__3 == null)
                            {
                                UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.InitButtonAchievementsWhenReady() - could not find hero picker button matching UnlockHeroAchievement with class {0}", this.<achievement>__2.ClassRequirement.Value));
                            }
                            else
                            {
                                if (((TAG_CLASS) this.<achievement>__2.ClassRequirement.Value) == TAG_CLASS.MAGE)
                                {
                                    this.<achievement>__2.AckCurrentProgressAndRewardNotices();
                                }
                                this.<button>__3.SetProgress(this.<achievement>__2.AcknowledgedProgress, this.<achievement>__2.Progress, this.<achievement>__2.MaxProgress, false);
                                this.<preconDeck>__4 = CollectionManager.Get().GetPreconDeck(this.<achievement>__2.ClassRequirement.Value);
                                this.<preconDeckID>__5 = 0L;
                                if (this.<preconDeck>__4 != null)
                                {
                                    this.<preconDeckID>__5 = this.<preconDeck>__4.ID;
                                }
                                this.<button>__3.SetPreconDeckID(this.<preconDeckID>__5);
                                if (this.<achievement>__2.IsCompleted() && (this.<preconDeckID>__5 == 0))
                                {
                                    UnityEngine.Debug.LogError(string.Format("DeckPickerTrayDisplay.InitButtonAchievementsWhenReady(): preconDeckID = 0 for achievement {0}", this.<achievement>__2));
                                }
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_449>__1.Dispose();
                    }
                    this.<>f__this.m_buttonAchievementsInitialized = true;
                    break;

                case 1:
                    break;

                default:
                    goto Label_0318;
            }
            while (this.<>f__this.m_delayButtonAnims)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<$s_450>__6 = this.<unlockHeroAchieves>__0.GetEnumerator();
            try
            {
                while (this.<$s_450>__6.MoveNext())
                {
                    this.<achievement>__7 = this.<$s_450>__6.Current;
                    this.<button>__8 = this.<>f__this.m_heroButtons.Find(new Predicate<HeroPickerButton>(this.<>m__DE));
                    if (this.<button>__8 == null)
                    {
                        UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.InitButtonAchievementsWhenReady() - could not find hero picker button matching UnlockHeroAchievement with class {0}", this.<achievement>__7.ClassRequirement.Value));
                    }
                    else
                    {
                        if (this.<>f__this.IsShowingCustomDecks())
                        {
                            this.<button>__8.SetProgress(this.<achievement>__7.AcknowledgedProgress, this.<achievement>__7.Progress, this.<achievement>__7.MaxProgress, false);
                        }
                        else
                        {
                            this.<button>__8.SetProgress(this.<achievement>__7.AcknowledgedProgress, this.<achievement>__7.Progress, this.<achievement>__7.MaxProgress);
                        }
                        this.<achievement>__7.AckCurrentProgressAndRewardNotices();
                    }
                }
            }
            finally
            {
                this.<$s_450>__6.Dispose();
            }
            this.$PC = -1;
        Label_0318:
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
    private sealed class <InitCustomDecks>c__AnonStorey2EC
    {
        internal DeckPickerTrayDisplay <>f__this;
        internal CollectionDeckBoxVisual deckBox;

        internal void <>m__DC(UIEvent e)
        {
            this.<>f__this.SelectCustomDeck(this.deckBox, true);
        }
    }

    [CompilerGenerated]
    private sealed class <InitModeWhenReady>c__Iterator60 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DeckPickerTrayDisplay <>f__this;
        internal PlayGameScene <scene>__0;

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
                    if (((((!this.<>f__this.m_buttonAchievementsInitialized || (this.<>f__this.m_heroDefsLoading > 0)) || ((this.<>f__this.m_heroPowerDefsLoading > 0) || (this.<>f__this.m_heroActor == null))) || !this.<>f__this.AreAllCustomDecksReady()) || (((this.<>f__this.m_heroPowerActor == null) || (this.<>f__this.m_goldenHeroPowerActor == null)) && this.<>f__this.ShouldShowHeroPower())) || ((this.<>f__this.m_customTrayTexture == null) || ((SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT) && (this.<>f__this.m_rankedPlayButtons == null))))
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.m_Loaded = true;
                    this.<>f__this.InitMode();
                    this.<scene>__0 = SceneMgr.Get().GetScene() as PlayGameScene;
                    if (this.<scene>__0 != null)
                    {
                        this.<scene>__0.OnDeckPickerLoaded();
                    }
                    this.<>f__this.FireDeckTrayLoadedEvent();
                    if ((SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY) || TavernBrawlManager.IsInTavernBrawlFriendlyChallenge())
                    {
                        if (FriendChallengeMgr.Get().HasChallenge())
                        {
                            FriendChallengeMgr.Get().AddChangedListener(new FriendChallengeMgr.ChangedCallback(this.<>f__this.OnFriendChallengeChanged));
                        }
                        else
                        {
                            this.<>f__this.GoBackUntilOnNavigateBackCalled();
                        }
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
    private sealed class <SetRankedMedalWhenReady>c__Iterator5F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DeckPickerTrayDisplay <>f__this;

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
                    if (TournamentDisplay.Get().GetCurrentMedalInfo() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.OnMedalChanged(TournamentDisplay.Get().GetCurrentMedalInfo());
                    TournamentDisplay.Get().RegisterMedalChangedListener(new TournamentDisplay.DelMedalChanged(this.<>f__this.OnMedalChanged));
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
    private sealed class <ShowDemoQuotes>c__Iterator5D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DeckPickerTrayDisplay <>f__this;
        internal bool <showThankQuote>__2;
        internal string <thankQuote>__0;
        internal int <thankQuoteTime>__1;

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
                    this.<thankQuote>__0 = Vars.Key("Demo.ThankQuote").GetStr(string.Empty);
                    this.<thankQuoteTime>__1 = Vars.Key("Demo.ThankQuoteMsTime").GetInt(0);
                    this.<thankQuote>__0 = this.<thankQuote>__0.Replace(@"\n", "\n");
                    this.<showThankQuote>__2 = !string.IsNullOrEmpty(this.<thankQuote>__0) && (this.<thankQuoteTime>__1 > 0);
                    if (!this.<showThankQuote>__2)
                    {
                        goto Label_0175;
                    }
                    if (DemoMgr.Get().GetMode() != DemoMode.BLIZZCON_2015)
                    {
                        this.<>f__this.m_expoThankQuote = NotificationManager.Get().CreateInnkeeperQuote(new Vector3(158.1f, NotificationManager.DEPTH, 80.2f), this.<thankQuote>__0, string.Empty, ((float) this.<thankQuoteTime>__1) / 1000f, null);
                        break;
                    }
                    this.<>f__this.m_expoThankQuote = NotificationManager.Get().CreateCharacterQuote("Reno_Quote", new Vector3(0f, NotificationManager.DEPTH, 0f), this.<thankQuote>__0, string.Empty, true, ((float) this.<thankQuoteTime>__1) / 1000f, null, CanvasAnchor.CENTER);
                    break;

                case 1:
                    this.<>f__this.EnableExpoClickBlocker(false);
                    goto Label_0175;

                default:
                    goto Label_0187;
            }
            this.<>f__this.EnableExpoClickBlocker(true);
            this.$current = new WaitForSeconds(((float) this.<thankQuoteTime>__1) / 1000f);
            this.$PC = 1;
            return true;
        Label_0175:
            this.<>f__this.ShowIntroQuote();
            this.$PC = -1;
        Label_0187:
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
    private sealed class <UpdateHeroSkinHeroPower>c__Iterator61 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DeckPickerTrayDisplay <>f__this;
        internal TAG_CARD_SET <cardSet>__2;
        internal CardDef <heroCardDef>__0;
        internal HeroSkinHeroPower <hshp>__1;

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
                    if (this.<>f__this.m_heroActor == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_014B;
                    }
                    this.<heroCardDef>__0 = this.<>f__this.m_heroActor.GetCardDef();
                    break;

                case 2:
                    break;

                default:
                    goto Label_0149;
            }
            while (this.<heroCardDef>__0 == null)
            {
                this.<heroCardDef>__0 = this.<>f__this.m_heroActor.GetCardDef();
                this.$current = null;
                this.$PC = 2;
                goto Label_014B;
            }
            this.<hshp>__1 = this.<>f__this.m_heroPowerActor.gameObject.GetComponentInChildren<HeroSkinHeroPower>();
            if (this.<hshp>__1 != null)
            {
                this.<cardSet>__2 = this.<>f__this.m_heroActor.GetEntityDef().GetCardSet();
                if (this.<cardSet>__2 == TAG_CARD_SET.HERO_SKINS)
                {
                    this.<hshp>__1.m_Actor.GetCardDef().m_AlwaysRenderPremiumPortrait = true;
                }
                else
                {
                    this.<hshp>__1.m_Actor.GetCardDef().m_AlwaysRenderPremiumPortrait = false;
                }
                this.<hshp>__1.m_Actor.UpdateMaterials();
                this.$PC = -1;
            }
        Label_0149:
            return false;
        Label_014B:
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

    public delegate void DeckTrayLoaded();

    private class HeroFullDefLoadedCallbackData
    {
        public HeroFullDefLoadedCallbackData(HeroPickerButton button, CardFlair heroFlair)
        {
            this.HeroPickerButton = button;
            this.HeroFlair = heroFlair;
        }

        public CardFlair HeroFlair { get; private set; }

        public HeroPickerButton HeroPickerButton { get; private set; }
    }
}

