using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class CollectionManagerDisplay : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<CardSetIconMatOffset> <>f__am$cache34;
    private const float CRAFTING_TRAY_SLIDE_IN_TIME = 0.25f;
    public GameObject m_activeSearchBone;
    public GameObject m_activeSearchBone_Win8;
    private CollectionActorPool m_actorPool = new CollectionActorPool();
    private Map<NetCache.CardDefinition, Actor> m_actorsToDisplay = new Map<NetCache.CardDefinition, Actor>();
    [CustomEditField(Sections="Tavern Brawl Changes")]
    public GameObject m_bookBack;
    private List<Actor> m_cardBackActors = new List<Actor>();
    public Material m_cardNotOwnedMeshMaterial;
    public CollectionCardVisual m_cardVisualPrefab;
    private List<Achievement> m_completeAchievesToDisplay = new List<Achievement>();
    [CustomEditField(Sections="Tavern Brawl Changes", T=EditType.TEXTURE)]
    public string m_corkBackTexture;
    public CollectionCoverDisplay m_cover;
    public CraftingModeButton m_craftingModeButton;
    private CraftingTray m_craftingTray;
    public GameObject m_craftingTrayHiddenBone;
    public GameObject m_craftingTrayShownBone;
    private List<Actor> m_currActorList;
    private ViewMode m_currentViewMode;
    private List<CollectionDeckBoxVisual> m_deckBoxes;
    private Notification m_deckHelpPopup;
    private int m_displayRequestID;
    private List<NetCache.CardDefinition> m_failedToLoadActors = new List<NetCache.CardDefinition>();
    public ActiveFilterButton m_filterButton;
    public Material m_goldenCardNotOwnedMeshMaterial;
    private Notification m_innkeeperLClickReminder;
    public PegUIElement m_inputBlocker;
    private bool m_isCoverLoading;
    private List<Actor> m_lastActorList;
    private Map<TAG_CLASS, Texture> m_loadedClassTextures = new Map<TAG_CLASS, Texture>();
    [CustomEditField(ListTable=true)]
    public List<CardSetIconMatOffset> m_manaFilterCardSetIcons = new List<CardSetIconMatOffset>();
    private List<FilterStateListener> m_manaFilterListeners = new List<FilterStateListener>();
    public ManaFilterTabManager m_manaTabManager;
    private bool m_netCacheReady;
    private List<Actor> m_obsoleteActors;
    public CollectionPageLayoutSettings m_pageLayoutSettings = new CollectionPageLayoutSettings();
    public CollectionPageManager m_pageManager;
    private List<Actor> m_previousCardBackActors;
    private Map<TAG_CLASS, TextureRequests> m_requestedClassTextures = new Map<TAG_CLASS, TextureRequests>();
    public CollectionSearch m_search;
    private List<FilterStateListener> m_searchFilterListeners = new List<FilterStateListener>();
    private bool m_selectingNewDeckHero;
    public CollectionSetFilterDropdown m_setFilterDropdown;
    private List<FilterStateListener> m_setFilterListeners = new List<FilterStateListener>();
    private long m_showDeckContentsRequest;
    private bool m_sortRibbonsInitialized;
    private List<OnSwitchViewMode> m_switchViewModeListeners = new List<OnSwitchViewMode>();
    [CustomEditField(Sections="Tavern Brawl Changes")]
    public Mesh m_tavernBrawlBookBackMesh;
    [CustomEditField(Sections="Tavern Brawl Changes")]
    public Material m_tavernBrawlElements;
    [CustomEditField(Sections="Tavern Brawl Changes")]
    public List<GameObject> m_tavernBrawlObjectsToSwap = new List<GameObject>();
    private bool m_unloading;
    private static CollectionManagerDisplay s_instance;
    private readonly Vector3 TUTORIAL_CRAFT_DECK_SCALE = ((Vector3) (14f * Vector3.one));
    private readonly Vector3 TUTORIAL_CRAFT_DECK_SCALE_PHONE = ((Vector3) (18f * Vector3.one));

    private void Awake()
    {
        s_instance = this;
        if ((GraphicsManager.Get().RenderQualityLevel != GraphicsQuality.Low) && (this.m_cover == null))
        {
            this.m_isCoverLoading = true;
            AssetLoader.Get().LoadGameObject("CollectionBookCover", new AssetLoader.GameObjectCallback(this.OnCoverLoaded), null, false);
        }
        CheatMgr.Get().RegisterCheatHandler("collectionhelp", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_collectionhelp), null, null, null);
        this.m_actorPool.Initialize();
        this.LoadAllClassTextures();
        this.EnableInput(true);
        base.StartCoroutine(this.InitCollectionWhenReady());
        this.SetTavernBrawlTexturesIfNecessary();
    }

    public bool CanViewCardBacks()
    {
        return (CardBackManager.Get().GetCardBacksOwned().Count > 1);
    }

    public bool CanViewHeroSkins()
    {
        CollectionManager manager = CollectionManager.Get();
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        return ((taggedDeck == null) || (manager.GetBestHeroesIOwn(taggedDeck.GetClass()).Count > 1));
    }

    private void CheckAndFinishDisplayList(DisplayCallbackData displayCallbackData)
    {
        if (displayCallbackData.m_requestID != this.m_displayRequestID)
        {
            Log.Rachelle.Print("CollectionManagerDisplay.CheckAndFinishDisplayList(): bailing early; outdated request ID.", new object[0]);
        }
        else
        {
            List<Actor> actorList = new List<Actor>();
            for (int i = 0; i < displayCallbackData.m_requestedCardDefinitions.Count; i++)
            {
                NetCache.CardDefinition key = displayCallbackData.m_requestedCardDefinitions[i];
                if (this.m_actorsToDisplay.ContainsKey(key))
                {
                    if (this.m_actorsToDisplay[key] == null)
                    {
                        return;
                    }
                    actorList.Add(this.m_actorsToDisplay[key]);
                }
                else if (!this.m_failedToLoadActors.Contains(key))
                {
                    return;
                }
            }
            this.UpdateCurrentActorList(actorList);
            displayCallbackData.m_callback(actorList, displayCallbackData.m_callbackData);
        }
    }

    public void CollectionPageContentsChanged(List<FilteredArtStack> artStacksToDisplay, CollectionActorsReadyCallback callback, object callbackData)
    {
        this.SetDisplayList(artStacksToDisplay, callback, callbackData);
    }

    public void CollectionPageContentsChangedToCardBacks(int pageNumber, int numCardBacksPerPage, CollectionActorsReadyCallback callback, object callbackData, bool showAll)
    {
        <CollectionPageContentsChangedToCardBacks>c__AnonStorey2D4 storeyd = new <CollectionPageContentsChangedToCardBacks>c__AnonStorey2D4 {
            callback = callback,
            callbackData = callbackData,
            <>f__this = this
        };
        CardBackManager manager = CardBackManager.Get();
        storeyd.result = new List<Actor>();
        List<CardBackManager.OwnedCardBack> orderedEnabledCardBacks = manager.GetOrderedEnabledCardBacks(!showAll);
        if (orderedEnabledCardBacks.Count == 0)
        {
            if (storeyd.callback != null)
            {
                storeyd.callback(storeyd.result, storeyd.callbackData);
            }
        }
        else
        {
            int index = (pageNumber - 1) * numCardBacksPerPage;
            int count = Mathf.Min(orderedEnabledCardBacks.Count - index, numCardBacksPerPage);
            orderedEnabledCardBacks = orderedEnabledCardBacks.GetRange(index, count);
            storeyd.numCardBacksToLoad = orderedEnabledCardBacks.Count;
            storeyd.cbLoadedCallback = new Action<int, CardBackManager.OwnedCardBack, Actor>(storeyd.<>m__A8);
            if (this.m_previousCardBackActors != null)
            {
                foreach (Actor actor in this.m_previousCardBackActors)
                {
                    UnityEngine.Object.Destroy(actor.gameObject);
                }
                this.m_previousCardBackActors.Clear();
            }
            this.m_previousCardBackActors = this.m_cardBackActors;
            this.m_cardBackActors = new List<Actor>();
            for (int i = 0; i < orderedEnabledCardBacks.Count; i++)
            {
                <CollectionPageContentsChangedToCardBacks>c__AnonStorey2D5 storeyd2 = new <CollectionPageContentsChangedToCardBacks>c__AnonStorey2D5 {
                    <>f__ref$724 = storeyd,
                    <>f__this = this,
                    currIndex = i,
                    cardBackLoad = orderedEnabledCardBacks[i]
                };
                int cardBackId = storeyd2.cardBackLoad.m_cardBackId;
                storeyd.result.Add(null);
                if (!manager.LoadCardBackByIndex(cardBackId, new CardBackManager.LoadCardBackData.LoadCardBackCallback(storeyd2.<>m__A9), "Collection_Card_Back"))
                {
                    storeyd.cbLoadedCallback(storeyd2.currIndex, storeyd2.cardBackLoad, null);
                }
            }
        }
    }

    private void DoBookClosingAnimations()
    {
        if (this.m_cover != null)
        {
            this.m_cover.Close();
        }
        this.m_manaTabManager.ActivateTabs(false);
    }

    [DebuggerHidden]
    private IEnumerator DoBookOpeningAnimations()
    {
        return new <DoBookOpeningAnimations>c__Iterator37 { <>f__this = this };
    }

    private void DoEnterCollectionManagerEvents()
    {
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_COLLECTION_MANAGER);
        if (CollectionManager.Get().HasVisitedCollection() || (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL))
        {
            this.EnableInput(true);
            this.OpenBookImmediately();
        }
        else
        {
            CollectionManager.Get().SetHasVisitedCollection(true);
            this.EnableInput(false);
            base.StartCoroutine(this.OpenBookWhenReady());
        }
    }

    private void EnableInput(bool enable)
    {
        this.m_inputBlocker.gameObject.SetActive(!enable);
    }

    private void EnableSearchUI(bool enabled)
    {
        this.m_manaTabManager.Enable(enabled);
        this.m_search.SetEnabled(enabled);
        if (this.m_setFilterDropdown != null)
        {
            this.m_setFilterDropdown.m_showDropDownButton.SetEnabled(enabled);
            this.m_setFilterDropdown.m_showDropDownButton.SetEnabledVisual(enabled);
        }
        if (this.m_filterButton != null)
        {
            this.m_filterButton.SetEnabled(enabled);
        }
    }

    public void EnterSelectNewDeckHeroMode()
    {
        if (!this.m_selectingNewDeckHero)
        {
            this.EnableInput(false);
            this.m_selectingNewDeckHero = true;
            AssetLoader.Get().LoadActor("HeroPicker", false, false);
            NotificationManager.Get().DestroyAllPopUps();
        }
    }

    public void Exit()
    {
        this.EnableInput(false);
        NotificationManager.Get().DestroyNotification(this.m_deckHelpPopup, 0f);
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
    }

    public void ExitSelectNewDeckHeroMode()
    {
        this.EnableInput(true);
        this.m_selectingNewDeckHero = false;
    }

    public void FilterByManaCost(int cost)
    {
        bool active = cost != ManaFilterTab.ALL_TAB_IDX;
        string str = (cost >= 7) ? (cost.ToString() + "+") : cost.ToString();
        this.NotifyFilterUpdate(this.m_manaFilterListeners, active, str);
        this.m_pageManager.FilterByManaCost(cost);
    }

    public static CollectionManagerDisplay Get()
    {
        return s_instance;
    }

    public Material GetCardNotOwnedMeshMaterial()
    {
        return this.m_cardNotOwnedMeshMaterial;
    }

    public CollectionCardVisual GetCardVisualPrefab()
    {
        return this.m_cardVisualPrefab;
    }

    public void GetClassTexture(TAG_CLASS classTag, DelTextureLoaded callback, object callbackData)
    {
        if (this.m_loadedClassTextures.ContainsKey(classTag))
        {
            callback(classTag, this.m_loadedClassTextures[classTag], callbackData);
        }
        else
        {
            TextureRequests requests;
            if (this.m_requestedClassTextures.ContainsKey(classTag))
            {
                requests = this.m_requestedClassTextures[classTag];
            }
            else
            {
                requests = new TextureRequests();
                this.m_requestedClassTextures[classTag] = requests;
            }
            TextureRequests.Request item = new TextureRequests.Request {
                m_callback = callback,
                m_callbackData = callbackData
            };
            requests.m_requests.Add(item);
        }
    }

    public static string GetClassTextureName(TAG_CLASS classTag)
    {
        switch (classTag)
        {
            case TAG_CLASS.DEATHKNIGHT:
                return "DeathKnight";

            case TAG_CLASS.DRUID:
                return "Druid";

            case TAG_CLASS.HUNTER:
                return "Hunter";

            case TAG_CLASS.MAGE:
                return "Mage";

            case TAG_CLASS.PALADIN:
                return "Paladin";

            case TAG_CLASS.PRIEST:
                return "Priest";

            case TAG_CLASS.ROGUE:
                return "Rogue";

            case TAG_CLASS.SHAMAN:
                return "Shaman";

            case TAG_CLASS.WARLOCK:
                return "Warlock";

            case TAG_CLASS.WARRIOR:
                return "Warrior";
        }
        return string.Empty;
    }

    public CollectionPageLayoutSettings.Variables GetCurrentPageLayoutSettings()
    {
        return this.m_pageLayoutSettings.GetVariables(this.m_currentViewMode);
    }

    private TAG_CLASS GetDeckHeroClass(long deckID)
    {
        CollectionDeck deck = CollectionManager.Get().GetDeck(deckID);
        if (deck == null)
        {
            Log.Derek.Print(string.Format("CollectionManagerDisplay no deck with ID {0}!", deckID), new object[0]);
            return TAG_CLASS.INVALID;
        }
        EntityDef entityDef = DefLoader.Get().GetEntityDef(deck.HeroCardID);
        if (entityDef == null)
        {
            Log.Derek.Print(string.Format("CollectionManagerDisplay: CollectionManager doesn't have an entity def for {0}!", deck.HeroCardID), new object[0]);
            return TAG_CLASS.INVALID;
        }
        return entityDef.GetClass();
    }

    public Material GetGoldenCardNotOwnedMeshMaterial()
    {
        return this.m_goldenCardNotOwnedMeshMaterial;
    }

    public RDM_Deck GetRDMDeck()
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        RDM_Deck deck2 = new RDM_Deck(DefLoader.Get().GetEntityDef(taggedDeck.HeroCardID));
        foreach (CollectionDeckSlot slot in taggedDeck.GetSlots())
        {
            for (int i = 0; i < slot.Count; i++)
            {
                if (slot.Owned)
                {
                    RDMDeckEntry item = new RDMDeckEntry(DefLoader.Get().GetEntityDef(slot.CardID), new CardFlair(slot.Premium));
                    deck2.deckList.Add(item);
                }
            }
        }
        return deck2;
    }

    public ViewMode GetViewMode()
    {
        return this.m_currentViewMode;
    }

    public void GoToPageWithCard(string cardID, CardFlair flair)
    {
        this.m_pageManager.JumpToPageWithCard(cardID, flair);
    }

    public void HideAllTips()
    {
        if (this.m_innkeeperLClickReminder != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.m_innkeeperLClickReminder);
        }
        if (this.m_deckHelpPopup != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.m_deckHelpPopup);
        }
    }

    public void HideCraftingTray()
    {
        object[] args = new object[] { "position", this.m_craftingTrayHiddenBone.transform.localPosition, "isLocal", true, "time", 0.25f, "easeType", iTween.EaseType.easeOutBounce, "oncomplete", o => this.m_craftingTray.gameObject.SetActive(false) };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_craftingTray.gameObject, hashtable);
        this.m_craftingModeButton.ShowActiveGlow(false);
    }

    public void HideDeckHelpPopup()
    {
        if (this.m_deckHelpPopup != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.m_deckHelpPopup);
        }
    }

    [DebuggerHidden]
    private IEnumerator InitCollectionWhenReady()
    {
        return new <InitCollectionWhenReady>c__Iterator35 { <>f__this = this };
    }

    public bool IsReady()
    {
        return this.m_actorPool.IsReady();
    }

    private bool LoadActorsForCardDefinition(NetCache.CardDefinition cardDefinition, DisplayCallbackData displayCallbackData)
    {
        if (this.m_actorsToDisplay.Count == CollectionPageDisplay.GetMaxNumCards())
        {
            return false;
        }
        EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDefinition.Name);
        AcquireActorCallbackData callbackData = new AcquireActorCallbackData {
            m_cardDefinition = cardDefinition,
            m_displayCallbackData = displayCallbackData
        };
        this.m_actorsToDisplay.Add(cardDefinition, null);
        return this.m_actorPool.AcquireActor(entityDef, new CardFlair(cardDefinition.Premium), new CollectionActorPool.AcquireActorCallback(this.OnActorAcquired), callbackData);
    }

    private void LoadAllClassTextures()
    {
        IEnumerator enumerator = Enum.GetValues(typeof(TAG_CLASS)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                TAG_CLASS current = (TAG_CLASS) ((int) enumerator.Current);
                AssetLoader.Get().LoadTexture(GetClassTextureName(current), new AssetLoader.ObjectCallback(this.OnClassTextureLoaded), current, false);
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
    }

    public void LoadCard(string cardID, CollectionCardCache.LoadCardDefCallback callback, object callbackData, CardPortraitQuality quality = null)
    {
        CollectionCardCache.Get().LoadCardDef(cardID, callback, callbackData, quality);
    }

    public void LoadCraftingManager(AssetLoader.GameObjectCallback callback)
    {
        callback(string.Empty, null, null);
    }

    private void LoadDisplayCards(DisplayCallbackData displayCallbackData)
    {
        if (!this.m_unloading && (displayCallbackData.m_requestID == this.m_displayRequestID))
        {
            foreach (NetCache.CardDefinition definition in displayCallbackData.m_requestedCardDefinitions)
            {
                if (!this.LoadActorsForCardDefinition(definition, displayCallbackData))
                {
                    UnityEngine.Debug.LogError(string.Format("LoadActorsForCardDefinition() failed to load actor for {0}", definition));
                    this.m_failedToLoadActors.Add(definition);
                }
            }
        }
    }

    private bool NewDeckButtonAvailable()
    {
        return !this.m_selectingNewDeckHero;
    }

    private void NotifyFilterUpdate(List<FilterStateListener> listeners, bool active, object value)
    {
        foreach (FilterStateListener listener in listeners)
        {
            listener(active, value);
        }
    }

    [DebuggerHidden]
    private IEnumerator NotifySceneLoadedWhenReady()
    {
        return new <NotifySceneLoadedWhenReady>c__Iterator34 { <>f__this = this };
    }

    private void OnActorAcquired(Actor actor, object callbackData)
    {
        if (!this.m_unloading)
        {
            AcquireActorCallbackData data = callbackData as AcquireActorCallbackData;
            if (data.m_displayCallbackData.m_requestID != this.m_displayRequestID)
            {
                this.ReleaseActor(actor);
            }
            else if (actor == null)
            {
                this.m_failedToLoadActors.Add(data.m_cardDefinition);
            }
            else
            {
                LoadCardCallbackData data2 = new LoadCardCallbackData {
                    m_cardDefinition = data.m_cardDefinition,
                    m_actor = actor,
                    m_displayCallbackData = data.m_displayCallbackData
                };
                string cardId = actor.GetEntityDef().GetCardId();
                this.LoadCard(cardId, new CollectionCardCache.LoadCardDefCallback(this.OnCardDefLoaded), data2, new CardPortraitQuality(1, actor.GetCardFlair().Premium));
            }
        }
    }

    private void OnCardDefLoaded(string cardID, CardDef cardDef, object callbackData)
    {
        if (!this.m_unloading)
        {
            LoadCardCallbackData data = callbackData as LoadCardCallbackData;
            if (data.m_displayCallbackData.m_requestID != this.m_displayRequestID)
            {
                this.ReleaseActor(data.m_actor);
            }
            else
            {
                Actor actor = data.m_actor;
                actor.SetCardDef(cardDef);
                actor.UpdateAllComponents();
                this.m_actorsToDisplay[data.m_cardDefinition] = actor;
                this.CheckAndFinishDisplayList(data.m_displayCallbackData);
            }
        }
    }

    private void OnCardRewardInserted(string cardID, CardFlair flair)
    {
        this.m_pageManager.RefreshCurrentPageContents();
    }

    private void OnCardTileRightClicked(DeckTrayDeckTileVisual cardTile)
    {
        this.GoToPageWithCard(cardTile.GetCardID(), cardTile.GetCardFlair());
    }

    private void OnClassTextureLoaded(string assetName, UnityEngine.Object asset, object callbackData)
    {
        if (asset == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CollectionManagerDisplay.OnClassTextureLoaded(): asset for {0} is null!", assetName));
        }
        else
        {
            TAG_CLASS key = (TAG_CLASS) ((int) callbackData);
            Texture classTexture = asset as Texture;
            if (classTexture == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("CollectionManagerDisplay.OnClassTextureLoaded(): classTexture for {0} is null (asset is not a texture)!", assetName));
            }
            else if (this.m_loadedClassTextures.ContainsKey(key))
            {
                UnityEngine.Debug.LogWarning(string.Format("CollectionManagerDisplay.OnClassTextureLoaded(): classTexture for {0} ({1}) has already been loaded!", key, assetName));
            }
            else
            {
                this.m_loadedClassTextures[key] = classTexture;
                if (this.m_requestedClassTextures.ContainsKey(key))
                {
                    TextureRequests requests = this.m_requestedClassTextures[key];
                    this.m_requestedClassTextures.Remove(key);
                    foreach (TextureRequests.Request request in requests.m_requests)
                    {
                        request.m_callback(key, classTexture, request.m_callbackData);
                    }
                }
            }
        }
    }

    private void OnCollectionAchievesCompleted(List<Achievement> achievements)
    {
        <OnCollectionAchievesCompleted>c__AnonStorey2D6 storeyd = new <OnCollectionAchievesCompleted>c__AnonStorey2D6();
        using (List<Achievement>.Enumerator enumerator = achievements.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                storeyd.achieve = enumerator.Current;
                if (this.m_completeAchievesToDisplay.Find(new Predicate<Achievement>(storeyd.<>m__AA)) == null)
                {
                    this.m_completeAchievesToDisplay.Add(storeyd.achieve);
                }
            }
        }
        if (QuestToast.GetCurrentToast() == null)
        {
            this.ShowCompleteAchieve(null);
        }
    }

    private void OnCollectionChanged()
    {
        if (!this.m_pageManager.IsShowingMassDisenchant())
        {
            this.m_pageManager.NotifyOfCollectionChanged();
        }
    }

    private void OnCollectionLoaded()
    {
        this.m_pageManager.OnCollectionLoaded();
    }

    private void OnCoverLoaded(string name, GameObject go, object userData)
    {
        this.m_isCoverLoading = false;
        this.m_cover = go.GetComponent<CollectionCoverDisplay>();
    }

    private void OnCoverOpened()
    {
        this.EnableInput(true);
    }

    private void OnCraftingModeButtonReleased(UIEvent e)
    {
        if (this.m_craftingTray.IsShown())
        {
            this.m_craftingTray.Hide();
        }
        else
        {
            this.ShowCraftingTray();
        }
    }

    private void OnCraftingTrayLoaded(string name, GameObject go, object userData)
    {
        go.SetActive(false);
        this.m_craftingTray = go.GetComponent<CraftingTray>();
        go.transform.parent = base.transform;
        go.transform.localPosition = this.m_craftingTrayHiddenBone.transform.localPosition;
        go.transform.localScale = this.m_craftingTrayHiddenBone.transform.localScale;
        this.m_pageManager.UpdateMassDisenchant();
    }

    private void OnDeckContents(long deckID)
    {
        if (deckID == this.m_showDeckContentsRequest)
        {
            this.m_showDeckContentsRequest = 0L;
            this.ShowDeck(deckID, false);
        }
    }

    private void OnDeckCreated(long deckID)
    {
        this.ShowDeck(deckID, true);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void OnDoneEditingDeck()
    {
        this.m_pageManager.OnDoneEditingDeck();
    }

    private void OnInputBlockerRelease(UIEvent e)
    {
        this.m_search.Deactivate();
    }

    private void OnLoadedMassDisenchant(string name, GameObject screen, object callbackData)
    {
        this.m_pageManager.m_massDisenchant = screen.GetComponent<MassDisenchant>();
        this.m_pageManager.m_massDisenchant.Hide();
    }

    private void OnNetCacheReady()
    {
        if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection.Manager)
        {
            if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
            {
                SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_COLLECTION", new object[0]);
            }
        }
        else
        {
            this.m_netCacheReady = true;
        }
    }

    private void OnNewCardSeen(string cardID, CardFlair flair)
    {
        this.m_pageManager.UpdateClassTabNewCardCounts();
    }

    private bool OnProcessCheat_collectionhelp(string func, string[] args, string rawArgs)
    {
        RandomDeckChoices choices = RandomDeckMaker.GetChoices(this.GetRDMDeck(), 3);
        if (choices != null)
        {
            RDMDeckEntry entry = choices.choices[0];
            CollectionDeckTray.Get().AddCard(entry.EntityDef, entry.Flair, null, true, null);
            UnityEngine.Debug.Log(choices.displayString);
        }
        return true;
    }

    private void OnSearchActivated()
    {
        this.EnableInput(false);
    }

    private void OnSearchCleared(bool updateVisuals)
    {
        this.NotifyFilterUpdate(this.m_searchFilterListeners, false, string.Empty);
        this.m_pageManager.ChangeSearchTextFilter(string.Empty, updateVisuals);
    }

    private void OnSearchDeactivated(string oldSearchText, string newSearchText)
    {
        this.EnableInput(true);
        if (oldSearchText != newSearchText)
        {
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CM_CARDS_SEARCHED);
            this.NotifyFilterUpdate(this.m_searchFilterListeners, !string.IsNullOrEmpty(newSearchText), newSearchText);
            this.m_pageManager.ChangeSearchTextFilter(newSearchText, true);
        }
    }

    private void OnShowAdvancedCMChanged(Option option, object prevValue, bool existed, object userData)
    {
        bool @bool = Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, false);
        if (@bool)
        {
            Options.Get().UnregisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, new Options.ChangedCallback(this.OnShowAdvancedCMChanged));
        }
        this.ShowAdvancedCollectionManager(@bool);
        this.m_manaTabManager.ActivateTabs(true);
    }

    private void OnSwitchViewModeResponse(ViewMode prevMode, ViewMode newMode, object userdata)
    {
        foreach (OnSwitchViewMode mode in this.m_switchViewModeListeners.ToArray())
        {
            mode(prevMode, newMode, userdata);
        }
        bool enabled = newMode == ViewMode.CARDS;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_craftingModeButton.Enable(enabled);
        }
        this.EnableSearchUI(enabled);
    }

    private void OpenBookImmediately()
    {
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
        {
            Enum[] args = new Enum[] { PresenceStatus.COLLECTION };
            PresenceMgr.Get().SetStatus(args);
        }
        base.StartCoroutine(this.SetBookToOpen());
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
        {
            base.StartCoroutine(this.ShowCollectionTipsIfNeeded());
        }
    }

    [DebuggerHidden]
    private IEnumerator OpenBookWhenReady()
    {
        return new <OpenBookWhenReady>c__Iterator39 { <>f__this = this };
    }

    public void RegisterManaFilterListener(FilterStateListener listener)
    {
        this.m_manaFilterListeners.Add(listener);
    }

    public void RegisterSearchFilterListener(FilterStateListener listener)
    {
        this.m_searchFilterListeners.Add(listener);
    }

    public void RegisterSetFilterListener(FilterStateListener listener)
    {
        this.m_setFilterListeners.Add(listener);
    }

    public void RegisterSwitchViewModeListener(OnSwitchViewMode listener)
    {
        this.m_switchViewModeListeners.Add(listener);
    }

    public void ReleaseActor(Actor actor)
    {
        actor.SetCardDef(null);
        actor.SetActorState(ActorStateType.CARD_IDLE);
        actor.Hide();
        this.m_actorPool.ReleaseActor(actor);
    }

    private void ReleaseObsoleteActors()
    {
        if (this.m_obsoleteActors != null)
        {
            foreach (Actor actor in this.m_obsoleteActors)
            {
                this.ReleaseActor(actor);
            }
        }
    }

    public void RemoveSwitchViewModeListener(OnSwitchViewMode listener)
    {
        this.m_switchViewModeListeners.Remove(listener);
    }

    public void RequestContentsToShowDeck(long deckID)
    {
        this.m_showDeckContentsRequest = deckID;
        CollectionManager.Get().RequestDeckContents(this.m_showDeckContentsRequest);
    }

    public void ResetFilters(bool updateVisuals = true)
    {
        this.m_search.ClearFilter(updateVisuals);
        this.m_manaTabManager.ClearFilter();
        if (Get().m_setFilterDropdown != null)
        {
            this.m_setFilterDropdown.ClearFilter();
        }
    }

    [DebuggerHidden]
    private IEnumerator SetBookToOpen()
    {
        return new <SetBookToOpen>c__Iterator38 { <>f__this = this };
    }

    private void SetDisplayList(List<FilteredArtStack> artStacksToDisplay, CollectionActorsReadyCallback callback, object callbackData)
    {
        if (this.m_displayRequestID == 0x7fffffff)
        {
            this.m_displayRequestID = 0;
        }
        else
        {
            this.m_displayRequestID++;
        }
        this.m_actorsToDisplay.Clear();
        this.m_failedToLoadActors.Clear();
        bool flag = false;
        if (artStacksToDisplay == null)
        {
            Log.Rachelle.Print("artStacksToDisplay is null!", new object[0]);
            flag = true;
        }
        else if (artStacksToDisplay.Count == 0)
        {
            Log.Rachelle.Print("artStacksToDisplay has a count of 0!", new object[0]);
            flag = true;
        }
        if (flag)
        {
            List<Actor> actorList = new List<Actor>();
            this.UpdateCurrentActorList(actorList);
            callback(actorList, callbackData);
        }
        else
        {
            DisplayCallbackData displayCallbackData = new DisplayCallbackData {
                m_requestID = this.m_displayRequestID,
                m_callback = callback,
                m_callbackData = callbackData
            };
            foreach (FilteredArtStack stack in artStacksToDisplay)
            {
                displayCallbackData.m_requestedCardDefinitions.Add(stack.GetCardDefinition());
            }
            base.StartCoroutine(this.WaitThenLoadDisplayCards(displayCallbackData));
        }
    }

    private void SetTavernBrawlTexturesIfNecessary()
    {
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL)
        {
            if (((this.m_bookBack != null) && !string.IsNullOrEmpty(this.m_corkBackTexture)) && (this.m_tavernBrawlBookBackMesh != null))
            {
                this.m_bookBack.GetComponent<MeshFilter>().mesh = this.m_tavernBrawlBookBackMesh;
                string name = FileUtils.GameAssetPathToName(this.m_corkBackTexture);
                Texture texture = AssetLoader.Get().LoadTexture(name, false);
                this.m_bookBack.GetComponent<MeshRenderer>().material.SetTexture(0, texture);
            }
            if (UniversalInputManager.UsePhoneUI == null)
            {
                foreach (GameObject obj2 in this.m_tavernBrawlObjectsToSwap)
                {
                    obj2.GetComponent<Renderer>().material = this.m_tavernBrawlElements;
                }
            }
        }
    }

    public void SetViewMode(ViewMode mode, object userdata = null)
    {
        if (this.m_currentViewMode != mode)
        {
            ViewMode currentViewMode = this.m_currentViewMode;
            this.m_currentViewMode = mode;
            this.OnSwitchViewModeResponse(currentViewMode, mode, userdata);
        }
    }

    public bool ShouldShowNewCardGlow(string cardID, CardFlair cardFlair)
    {
        CollectionCardStack.ArtStack collectionArtStack = CollectionManager.Get().GetCollectionArtStack(cardID, cardFlair);
        if ((collectionArtStack == null) || (collectionArtStack.Count == 0))
        {
            return false;
        }
        if (collectionArtStack.NumSeen == collectionArtStack.Count)
        {
            return false;
        }
        int num = !DefLoader.Get().GetEntityDef(cardID).IsElite() ? 2 : 1;
        return (collectionArtStack.NumSeen < num);
    }

    private void ShowAdvancedCollectionManager(bool show)
    {
        show |= UniversalInputManager.UsePhoneUI;
        this.m_search.gameObject.SetActive(show);
        this.m_manaTabManager.gameObject.SetActive(show);
        if (this.m_setFilterDropdown != null)
        {
            this.m_setFilterDropdown.m_showDropDownButton.gameObject.SetActive(show);
        }
        if (this.m_craftingTray == null)
        {
            AssetLoader.Get().LoadGameObject((UniversalInputManager.UsePhoneUI == null) ? "CraftingTray" : "CraftingTray_phone", new AssetLoader.GameObjectCallback(this.OnCraftingTrayLoaded), null, false);
        }
        this.m_craftingModeButton.gameObject.SetActive(true);
        this.m_craftingModeButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCraftingModeButtonReleased));
        if (((this.m_setFilterDropdown != null) && show) && !this.m_sortRibbonsInitialized)
        {
            if (<>f__am$cache34 == null)
            {
                <>f__am$cache34 = t => t.m_cardSet == TAG_CARD_SET.INVALID;
            }
            CardSetIconMatOffset offset = this.m_manaFilterCardSetIcons.Find(<>f__am$cache34);
            this.m_setFilterDropdown.AddItem(GameStrings.Get("GLUE_COLLECTION_ALL_SETS"), new Vector2?(offset.m_offset), new CollectionSetFilterDropdown.ItemSelectedCallback(this.ShowSet), null);
            <ShowAdvancedCollectionManager>c__AnonStorey2D7 storeyd = new <ShowAdvancedCollectionManager>c__AnonStorey2D7();
            using (List<TAG_CARD_SET>.Enumerator enumerator = CollectionManager.Get().GetDisplayableCardSets().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    storeyd.cardSet = enumerator.Current;
                    List<object> data = new List<object>();
                    CardSetIconMatOffset offset2 = this.m_manaFilterCardSetIcons.Find(new Predicate<CardSetIconMatOffset>(storeyd.<>m__AC));
                    Vector2? materialOffset = null;
                    if (offset2 != null)
                    {
                        materialOffset = new Vector2?(offset2.m_offset);
                    }
                    if (storeyd.cardSet != TAG_CARD_SET.PROMO)
                    {
                        data.Add(storeyd.cardSet);
                        if (storeyd.cardSet == TAG_CARD_SET.REWARD)
                        {
                            data.Add(TAG_CARD_SET.PROMO);
                        }
                        this.m_setFilterDropdown.AddItem(GameStrings.GetCardSetNameShortened(storeyd.cardSet), materialOffset, new CollectionSetFilterDropdown.ItemSelectedCallback(this.ShowSet), data);
                    }
                }
            }
            this.m_setFilterDropdown.Select(0);
            this.m_sortRibbonsInitialized = true;
        }
        if (show)
        {
            this.m_manaTabManager.SetUpTabs();
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowCollectionTipsIfNeeded()
    {
        return new <ShowCollectionTipsIfNeeded>c__Iterator3A { <>f__this = this };
    }

    private void ShowCompleteAchieve(object userData)
    {
        if (this.m_completeAchievesToDisplay.Count != 0)
        {
            Achievement quest = this.m_completeAchievesToDisplay[0];
            this.m_completeAchievesToDisplay.RemoveAt(0);
            QuestToast.ShowQuestToast(new QuestToast.DelOnCloseQuestToast(this.ShowCompleteAchieve), true, quest);
        }
    }

    private void ShowCraftingTipIfNeeded()
    {
        if (!Options.Get().GetBool(Option.TIP_CRAFTING_UNLOCKED, false))
        {
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_DISENCHANT_31"), "VO_INNKEEPER_DISENCHANT_31", 0f, null);
            Options.Get().SetBool(Option.TIP_CRAFTING_UNLOCKED, true);
        }
    }

    public void ShowCraftingTray()
    {
        CollectionDeckTray tray = CollectionDeckTray.Get();
        if (tray != null)
        {
            DeckTrayDeckListContent decksContent = tray.GetDecksContent();
            if (decksContent != null)
            {
                decksContent.CancelRenameEditingDeck();
            }
        }
        this.HideDeckHelpPopup();
        this.m_craftingTray.gameObject.SetActive(true);
        this.m_craftingTray.Show();
        object[] args = new object[] { "position", this.m_craftingTrayShownBone.transform.localPosition, "isLocal", true, "time", 0.25f, "easeType", iTween.EaseType.easeOutBounce };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_craftingTray.gameObject, hashtable);
        this.m_craftingModeButton.ShowActiveGlow(true);
    }

    private void ShowDeck(long deckID, bool isNewDeck)
    {
        if (CollectionManager.Get().GetDeck(deckID) != null)
        {
            CollectionDeckTray.Get().ShowDeck(deckID, isNewDeck);
            TAG_CLASS deckHeroClass = this.GetDeckHeroClass(deckID);
            this.m_pageManager.AddClassFilter(deckHeroClass, isNewDeck);
            this.m_pageManager.UpdateCraftingModeButtonDustBottleVisibility();
            if (((this.m_currentViewMode == ViewMode.HERO_SKINS) && !this.CanViewHeroSkins()) || ((this.m_currentViewMode == ViewMode.CARD_BACKS) && !this.CanViewCardBacks()))
            {
                this.SetViewMode(ViewMode.CARDS, null);
            }
        }
    }

    public void ShowInnkeeeprLClickHelp(bool isHero)
    {
        if (!CollectionDeckTray.Get().IsShowingDeckContents())
        {
            if (isHero)
            {
                this.m_innkeeperLClickReminder = NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_CM_LCLICK_HERO"), string.Empty, 3f, null);
            }
            else
            {
                this.m_innkeeperLClickReminder = NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_CM_LCLICK"), string.Empty, 3f, null);
            }
        }
    }

    public void ShowOnlyCardsIOwn()
    {
        this.ShowOnlyCardsIOwn(null);
    }

    public void ShowOnlyCardsIOwn(object obj)
    {
        this.m_pageManager.ShowOnlyCardsIOwn();
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CM_SHOW_CARDS_I_DONT_OWN_TURNED_OFF);
    }

    public void ShowPremiumCardsNotOwned(bool show)
    {
        this.m_pageManager.ShowCardsNotOwned(show);
        if (show)
        {
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CM_SHOW_PREMIUMS_NOT_OWNED);
        }
        else
        {
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CM_HIDE_PREMIUMS_NOT_OWNED);
        }
    }

    public void ShowPremiumCardsOnly()
    {
        this.m_pageManager.ShowPremiumCardsOnly();
    }

    public void ShowSet(object data)
    {
        List<object> cardSets = (List<object>) data;
        this.m_pageManager.FilterByCardSets(cardSets);
        this.NotifyFilterUpdate(this.m_setFilterListeners, data != null, null);
    }

    public void ShowTavernBrawlDeck(long deckID)
    {
        CollectionDeckTray.Get().GetDecksContent().SetEditingTraySection(0);
        CollectionDeckTray.Get().SetTrayMode(CollectionDeckTray.DeckContentTypes.Decks);
        Get().RequestContentsToShowDeck(deckID);
    }

    private void Start()
    {
        NetCache.Get().RegisterScreenCollectionManager(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        CollectionManager.Get().RegisterCollectionNetHandlers();
        CollectionManager.Get().RegisterCollectionLoadedListener(new CollectionManager.DelOnCollectionLoaded(this.OnCollectionLoaded));
        CollectionManager.Get().RegisterCollectionChangedListener(new CollectionManager.DelOnCollectionChanged(this.OnCollectionChanged));
        CollectionManager.Get().RegisterDeckCreatedListener(new CollectionManager.DelOnDeckCreated(this.OnDeckCreated));
        CollectionManager.Get().RegisterDeckContentsListener(new CollectionManager.DelOnDeckContents(this.OnDeckContents));
        CollectionManager.Get().RegisterNewCardSeenListener(new CollectionManager.DelOnNewCardSeen(this.OnNewCardSeen));
        CollectionManager.Get().RegisterCardRewardInsertedListener(new CollectionManager.DelOnCardRewardInserted(this.OnCardRewardInserted));
        this.m_inputBlocker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnInputBlockerRelease));
        this.m_search.RegisterActivatedListener(new CollectionSearch.ActivatedListener(this.OnSearchActivated));
        this.m_search.RegisterDeactivatedListener(new CollectionSearch.DeactivatedListener(this.OnSearchDeactivated));
        this.m_search.RegisterClearedListener(new CollectionSearch.ClearedListener(this.OnSearchCleared));
        this.m_pageManager.LoadPagingArrows();
        bool @bool = Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, false);
        this.ShowAdvancedCollectionManager(@bool);
        if (!@bool)
        {
            Options.Get().RegisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, new Options.ChangedCallback(this.OnShowAdvancedCMChanged));
        }
        CollectionManager.Get().RegisterAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
        this.DoEnterCollectionManagerEvents();
        if (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL)
        {
            MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_CollectionManager);
        }
        base.StartCoroutine(this.NotifySceneLoadedWhenReady());
    }

    public void Unload()
    {
        this.m_unloading = true;
        DefLoader.Get().ClearCardDefs();
        CollectionCardCache.Get().Unload();
        this.m_actorPool.Unload();
        this.UnloadAllClassTextures();
        CollectionDeckTray.Get().GetCardsContent().UnregisterCardTileRightClickedListener(new DeckTrayCardListContent.CardTileRightClicked(this.OnCardTileRightClicked));
        CollectionDeckTray.Get().Unload();
        CollectionInputMgr.Get().Unload();
        CollectionManager.Get().RemoveCollectionLoadedListener(new CollectionManager.DelOnCollectionLoaded(this.OnCollectionLoaded));
        CollectionManager.Get().RemoveCollectionChangedListener(new CollectionManager.DelOnCollectionChanged(this.OnCollectionChanged));
        CollectionManager.Get().RemoveDeckCreatedListener(new CollectionManager.DelOnDeckCreated(this.OnDeckCreated));
        CollectionManager.Get().RemoveDeckContentsListener(new CollectionManager.DelOnDeckContents(this.OnDeckContents));
        CollectionManager.Get().RemoveNewCardSeenListener(new CollectionManager.DelOnNewCardSeen(this.OnNewCardSeen));
        CollectionManager.Get().RemoveCardRewardInsertedListener(new CollectionManager.DelOnCardRewardInserted(this.OnCardRewardInserted));
        CollectionManager.Get().RemoveAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
        CollectionManager.Get().RemoveCollectionNetHandlers();
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        CheatMgr.Get().UnregisterCheatHandler("collectionhelp", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_collectionhelp));
        Options.Get().UnregisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, new Options.ChangedCallback(this.OnShowAdvancedCMChanged));
        this.m_unloading = false;
    }

    private void UnloadAllClassTextures()
    {
        if (this.m_loadedClassTextures.Count != 0)
        {
            List<string> names = new List<string>();
            foreach (TAG_CLASS tag_class in this.m_loadedClassTextures.Keys)
            {
                names.Add(GetClassTextureName(tag_class));
            }
            AssetCache.ClearTextures(names);
            this.m_loadedClassTextures.Clear();
        }
    }

    public void UnregisterManaFilterListener(FilterStateListener listener)
    {
        this.m_manaFilterListeners.Remove(listener);
    }

    public void UnregisterSearchFilterListener(FilterStateListener listener)
    {
        this.m_searchFilterListeners.Remove(listener);
    }

    public void UnregisterSetFilterListener(FilterStateListener listener)
    {
        this.m_setFilterListeners.Remove(listener);
    }

    private void Update()
    {
        if (ApplicationMgr.IsInternal())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                this.SetViewMode(ViewMode.HERO_SKINS, null);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                this.SetViewMode(ViewMode.CARDS, null);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                this.SetViewMode(ViewMode.CARD_BACKS, null);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                this.OnCraftingModeButtonReleased(null);
            }
        }
    }

    private void UpdateCurrentActorList(List<Actor> actorList)
    {
        this.m_obsoleteActors = this.m_lastActorList;
        this.m_lastActorList = this.m_currActorList;
        this.m_currActorList = actorList;
        this.ReleaseObsoleteActors();
    }

    public void UpdateCurrentPageCardLocks(bool playSound = false)
    {
        this.m_pageManager.UpdateCurrentPageCardLocks(playSound);
    }

    [DebuggerHidden]
    private IEnumerator WaitThenLoadDisplayCards(DisplayCallbackData displayCallbackData)
    {
        return new <WaitThenLoadDisplayCards>c__Iterator36 { displayCallbackData = displayCallbackData, <$>displayCallbackData = displayCallbackData, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <CollectionPageContentsChangedToCardBacks>c__AnonStorey2D4
    {
        internal CollectionManagerDisplay <>f__this;
        internal CollectionManagerDisplay.CollectionActorsReadyCallback callback;
        internal object callbackData;
        internal Action<int, CardBackManager.OwnedCardBack, Actor> cbLoadedCallback;
        internal int numCardBacksToLoad;
        internal List<Actor> result;

        internal void <>m__A8(int index, CardBackManager.OwnedCardBack cardBack, Actor actor)
        {
            if (actor != null)
            {
                this.result[index] = actor;
                actor.SetCardbackUpdateIgnore(true);
                if (!cardBack.m_owned)
                {
                    actor.MissingCardEffect();
                }
                CollectionCardBack component = actor.GetComponent<CollectionCardBack>();
                if (component != null)
                {
                    component.SetCardBackId(cardBack.m_cardBackId);
                    component.SetCardBackName(CardBackManager.Get().GetCardBackName(cardBack.m_cardBackId));
                }
                else
                {
                    UnityEngine.Debug.LogError("CollectionCardBack component does not exist on actor!");
                }
            }
            this.numCardBacksToLoad--;
            if ((this.numCardBacksToLoad == 0) && (this.callback != null))
            {
                this.callback(this.result, this.callbackData);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <CollectionPageContentsChangedToCardBacks>c__AnonStorey2D5
    {
        internal CollectionManagerDisplay.<CollectionPageContentsChangedToCardBacks>c__AnonStorey2D4 <>f__ref$724;
        internal CollectionManagerDisplay <>f__this;
        internal CardBackManager.OwnedCardBack cardBackLoad;
        internal int currIndex;

        internal void <>m__A9(CardBackManager.LoadCardBackData cardBackData)
        {
            GameObject gameObject = cardBackData.m_GameObject;
            gameObject.transform.parent = this.<>f__this.transform;
            gameObject.name = "CARD_BACK_" + cardBackData.m_CardBackIndex;
            Actor component = gameObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                GameObject cardMesh = component.m_cardMesh;
                component.SetCardbackUpdateIgnore(true);
                component.SetUnlit();
                if (cardMesh != null)
                {
                    Material material = cardMesh.GetComponent<Renderer>().material;
                    if (material.HasProperty("_SpecularIntensity"))
                    {
                        material.SetFloat("_SpecularIntensity", 0f);
                    }
                }
                this.<>f__this.m_cardBackActors.Add(component);
            }
            this.<>f__ref$724.cbLoadedCallback(this.currIndex, this.cardBackLoad, component);
        }
    }

    [CompilerGenerated]
    private sealed class <DoBookOpeningAnimations>c__Iterator37 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionManagerDisplay <>f__this;

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
                    if (this.<>f__this.m_isCoverLoading)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (this.<>f__this.m_cover != null)
                    {
                        this.<>f__this.m_cover.Open(new CollectionCoverDisplay.DelOnOpened(this.<>f__this.OnCoverOpened));
                    }
                    else
                    {
                        this.<>f__this.OnCoverOpened();
                    }
                    this.<>f__this.m_manaTabManager.ActivateTabs(true);
                    break;

                default:
                    break;
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
    private sealed class <InitCollectionWhenReady>c__Iterator35 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionManagerDisplay <>f__this;

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
                    if (!CollectionManager.Get().CanDisplayCards())
                    {
                        this.$current = 0;
                        this.$PC = 1;
                        goto Label_00D3;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_00D1;
            }
            while (!this.<>f__this.m_pageManager.IsFullyLoaded())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00D3;
            }
            AssetLoader.Get().LoadUIScreen((UniversalInputManager.UsePhoneUI == null) ? "MassDisenchant" : "MassDisenchant_phone", new AssetLoader.GameObjectCallback(this.<>f__this.OnLoadedMassDisenchant), null, false);
            this.<>f__this.m_pageManager.OnCollectionLoaded();
            this.$PC = -1;
        Label_00D1:
            return false;
        Label_00D3:
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
    private sealed class <NotifySceneLoadedWhenReady>c__Iterator34 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionManagerDisplay <>f__this;
        internal CollectionDeckTray <deckTray>__0;

        internal void <>m__AE()
        {
            this.<>f__this.UpdateCurrentPageCardLocks(false);
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
                    if (!CollectionManager.Get().CanDisplayCards())
                    {
                        this.$current = 0;
                        this.$PC = 1;
                        goto Label_00E5;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_00E3;
            }
            while (!this.<>f__this.m_netCacheReady)
            {
                this.$current = 0;
                this.$PC = 2;
                goto Label_00E5;
            }
            this.<deckTray>__0 = CollectionDeckTray.Get();
            this.<deckTray>__0.RegisterModeSwitchedListener(new CollectionDeckTray.ModeSwitched(this.<>m__AE));
            this.<deckTray>__0.GetCardsContent().RegisterCardTileRightClickedListener(new DeckTrayCardListContent.CardTileRightClicked(this.<>f__this.OnCardTileRightClicked));
            if (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL)
            {
                SceneMgr.Get().NotifySceneLoaded();
            }
            this.$PC = -1;
        Label_00E3:
            return false;
        Label_00E5:
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
    private sealed class <OnCollectionAchievesCompleted>c__AnonStorey2D6
    {
        internal Achievement achieve;

        internal bool <>m__AA(Achievement obj)
        {
            return (obj.ID == this.achieve.ID);
        }
    }

    [CompilerGenerated]
    private sealed class <OpenBookWhenReady>c__Iterator39 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionManagerDisplay <>f__this;

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
                    if (CollectionManager.Get().IsWaitingForBoxTransition())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
                    {
                        Enum[] args = new Enum[] { PresenceStatus.COLLECTION };
                        PresenceMgr.Get().SetStatus(args);
                    }
                    this.<>f__this.m_pageManager.OnBookOpening();
                    this.<>f__this.StartCoroutine(this.<>f__this.DoBookOpeningAnimations());
                    if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
                    {
                        this.<>f__this.StartCoroutine(this.<>f__this.ShowCollectionTipsIfNeeded());
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
    private sealed class <SetBookToOpen>c__Iterator38 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionManagerDisplay <>f__this;

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
                    if (this.<>f__this.m_isCoverLoading)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (this.<>f__this.m_cover != null)
                    {
                        this.<>f__this.m_cover.SetOpenState();
                    }
                    this.<>f__this.m_manaTabManager.ActivateTabs(true);
                    break;

                default:
                    break;
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
    private sealed class <ShowAdvancedCollectionManager>c__AnonStorey2D7
    {
        internal TAG_CARD_SET cardSet;

        internal bool <>m__AC(CollectionManagerDisplay.CardSetIconMatOffset t)
        {
            return (t.m_cardSet == this.cardSet);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowCollectionTipsIfNeeded>c__Iterator3A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionManagerDisplay <>f__this;
        internal Vector3 <newDeckSpot>__0;
        internal Vector3 <popupSpot>__1;

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
                    if (Options.Get().GetBool(Option.HAS_SEEN_PRACTICE_MODE, false))
                    {
                        Options.Get().SetBool(Option.HAS_SEEN_COLLECTIONMANAGER_AFTER_PRACTICE, true);
                    }
                    if (Options.Get().GetBool(Option.HAS_STARTED_A_DECK, false))
                    {
                        goto Label_01E4;
                    }
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<newDeckSpot>__0 = CollectionDeckTray.Get().GetDecksContent().GetNewDeckButtonPosition();
                    this.<popupSpot>__1 = new Vector3(this.<newDeckSpot>__0.x, this.<newDeckSpot>__0.y, this.<newDeckSpot>__0.z);
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        this.<popupSpot>__1.x -= 17f;
                        this.<>f__this.m_deckHelpPopup = NotificationManager.Get().CreatePopupText(this.<popupSpot>__1, this.<>f__this.TUTORIAL_CRAFT_DECK_SCALE, GameStrings.Get("GLUE_COLLECTION_TUTORIAL01"), true);
                        break;
                    }
                    this.<popupSpot>__1.x -= 25f;
                    this.<>f__this.m_deckHelpPopup = NotificationManager.Get().CreatePopupText(this.<popupSpot>__1, this.<>f__this.TUTORIAL_CRAFT_DECK_SCALE_PHONE, GameStrings.Get("GLUE_COLLECTION_TUTORIAL01"), true);
                    break;

                default:
                    goto Label_01E4;
            }
            this.<>f__this.m_deckHelpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
            this.<>f__this.m_deckHelpPopup.PulseReminderEveryXSeconds(3f);
            if (!Options.Get().GetBool(Option.HAS_SEEN_COLLECTIONMANAGER, false))
            {
                Options.Get().SetBool(Option.HAS_SEEN_COLLECTIONMANAGER, true);
                if (this.<>f__this.NewDeckButtonAvailable())
                {
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("TUTORIAL_INNKEEPER_01"), "VO_ANNOUNCER_CM_MAKE_DECK_51", 0f, null);
                    this.$PC = -1;
                }
            }
        Label_01E4:
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
    private sealed class <WaitThenLoadDisplayCards>c__Iterator36 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionManagerDisplay.DisplayCallbackData <$>displayCallbackData;
        internal CollectionManagerDisplay <>f__this;
        internal CollectionManager <collectionMgr>__0;
        internal CollectionManagerDisplay.DisplayCallbackData displayCallbackData;

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
                    this.<collectionMgr>__0 = CollectionManager.Get();
                    break;

                case 1:
                    break;

                default:
                    goto Label_006C;
            }
            if (!this.<collectionMgr>__0.CanDisplayCards())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.LoadDisplayCards(this.displayCallbackData);
            this.$PC = -1;
        Label_006C:
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

    private class AcquireActorCallbackData
    {
        public NetCache.CardDefinition m_cardDefinition;
        public CollectionManagerDisplay.DisplayCallbackData m_displayCallbackData;
    }

    [Serializable]
    public class CardSetIconMatOffset
    {
        public TAG_CARD_SET m_cardSet;
        public Vector2 m_offset;
    }

    public delegate void CollectionActorsReadyCallback(List<Actor> actors, object callbackData);

    public delegate void DelTextureLoaded(TAG_CLASS classTag, Texture classTexture, object callbackData);

    private class DisplayCallbackData
    {
        public CollectionManagerDisplay.CollectionActorsReadyCallback m_callback;
        public object m_callbackData;
        public List<NetCache.CardDefinition> m_requestedCardDefinitions = new List<NetCache.CardDefinition>();
        public int m_requestID = 0;
    }

    public delegate void FilterStateListener(bool filterActive, object value);

    private class LoadCardCallbackData
    {
        public Actor m_actor;
        public NetCache.CardDefinition m_cardDefinition;
        public CollectionManagerDisplay.DisplayCallbackData m_displayCallbackData;
    }

    public delegate void OnSwitchViewMode(CollectionManagerDisplay.ViewMode prevMode, CollectionManagerDisplay.ViewMode mode, object userdata);

    private class TextureRequests
    {
        public List<Request> m_requests = new List<Request>();

        public class Request
        {
            public CollectionManagerDisplay.DelTextureLoaded m_callback;
            public object m_callbackData;
        }
    }

    public enum ViewMode
    {
        CARDS,
        HERO_SKINS,
        CARD_BACKS,
        COUNT
    }
}

