using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class CollectionPageManager : MonoBehaviour
{
    [CompilerGenerated]
    private static Comparison<FilteredArtStack> <>f__am$cache3E;
    private readonly PlatformDependentValue<bool> ALWAYS_SHOW_PAGING_ARROWS;
    private readonly PlatformDependentValue<bool> ANIMATE_PAGE_TRANSITIONS;
    private static readonly string ANIMATE_TABS_COROUTINE_NAME;
    private static readonly float ARROW_SCALE_TIME;
    private static readonly Vector3 CLASS_TAB_LOCAL_EULERS;
    private static TAG_CLASS[] CLASS_TAB_ORDER;
    private static Map<TAG_CLASS, int> CLASS_TO_TAB_IDX;
    private static readonly Vector3 CURRENT_PAGE_LOCAL_POS;
    private static readonly float HIDDEN_TAB_LOCAL_Z_POS;
    private static CollectionManagerDisplay.ViewMode[] LARGE_TAB_ORDER;
    private List<CollectionClassTab> m_allTabs = new List<CollectionClassTab>();
    public CollectionClassTab m_cardBacksTab;
    private Vector3 m_cardBacksTabPos;
    private SortedCollection m_cardsSortedCollection = new SortedCollection();
    private TAG_CLASS m_classConstraint;
    public ClassFilterHeaderButton m_classFilterHeader;
    public GameObject m_classTabContainer;
    public CollectionClassTab m_classTabPrefab;
    private List<CollectionClassTab> m_classTabs = new List<CollectionClassTab>();
    private CollectionClassTab m_currentClassTab;
    private bool m_currentPageIsPageA;
    private int m_currentPageNum;
    private bool m_delayShowingArrows;
    private float m_deselectedClassTabHalfWidth;
    private bool m_fullyLoaded;
    private SortedCollection m_heroSkinsSortedCollection = new SortedCollection();
    public CollectionClassTab m_heroSkinsTab;
    private Vector3 m_heroSkinsTabPos;
    private bool m_initializedTabPositions;
    private int m_lastPageNum;
    private bool m_leftArrowShown;
    public MassDisenchant m_massDisenchant;
    private CollectionPageDisplay m_pageA;
    private CollectionPageDisplay m_pageB;
    public CollectionPageDisplay m_pageDisplayPrefab;
    public PegUIElement m_pageDraggableRegion;
    private GameObject m_pageLeftArrow;
    public GameObject m_pageLeftArrowBone;
    public PegUIElement m_pageLeftClickableRegion;
    private GameObject m_pageRightArrow;
    public GameObject m_pageRightArrowBone;
    public PegUIElement m_pageRightClickableRegion;
    private bool m_pagesCurrentlyTurning;
    public PageTurn m_pageTurn;
    private FilteredArtStack m_preSearchArtStackAnchor;
    private bool m_rightArrowShown;
    private bool m_skipNextPageTurn;
    private Map<CollectionManagerDisplay.ViewMode, SortedCollection> m_sortedCollections = new Map<CollectionManagerDisplay.ViewMode, SortedCollection>();
    public float m_spaceBetweenTabs;
    private bool m_tabsAreAnimating;
    private Map<CollectionClassTab, bool> m_tabVisibility = new Map<CollectionClassTab, bool>();
    public float m_turnLeftPageSwapTiming;
    private bool m_useLastPage;
    private bool m_wasTouchModeEnabled;
    private static readonly int MASS_DISENCHANT_PAGE_NUM;
    private static readonly Vector3 NEXT_PAGE_LOCAL_POS;
    private static readonly int NUM_PAGE_FLIPS_BEFORE_STOP_SHOWING_ARROWS;
    private static readonly Vector3 PREV_PAGE_LOCAL_POS;
    public static readonly Map<TAG_CLASS, Vector2> s_classTextureOffsets;
    public static readonly float SELECT_TAB_ANIM_TIME;
    private static readonly string SELECT_TAB_COROUTINE_NAME;
    private static readonly string SHOW_ARROWS_COROUTINE_NAME;

    static CollectionPageManager()
    {
        Map<TAG_CLASS, Vector2> map = new Map<TAG_CLASS, Vector2>();
        map.Add(TAG_CLASS.MAGE, new Vector2(0f, 0f));
        map.Add(TAG_CLASS.PALADIN, new Vector2(0.205f, 0f));
        map.Add(TAG_CLASS.PRIEST, new Vector2(0.392f, 0f));
        map.Add(TAG_CLASS.ROGUE, new Vector2(0.58f, 0f));
        map.Add(TAG_CLASS.SHAMAN, new Vector2(0.774f, 0f));
        map.Add(TAG_CLASS.WARLOCK, new Vector2(0f, -0.2f));
        map.Add(TAG_CLASS.WARRIOR, new Vector2(0.205f, -0.2f));
        map.Add(TAG_CLASS.DRUID, new Vector2(0.392f, -0.2f));
        map.Add(TAG_CLASS.HUNTER, new Vector2(0.58f, -0.2f));
        map.Add(TAG_CLASS.INVALID, new Vector2(0.774f, -0.2f));
        s_classTextureOffsets = map;
        TAG_CLASS[] tag_classArray1 = new TAG_CLASS[10];
        tag_classArray1[0] = TAG_CLASS.DRUID;
        tag_classArray1[1] = TAG_CLASS.HUNTER;
        tag_classArray1[2] = TAG_CLASS.MAGE;
        tag_classArray1[3] = TAG_CLASS.PALADIN;
        tag_classArray1[4] = TAG_CLASS.PRIEST;
        tag_classArray1[5] = TAG_CLASS.ROGUE;
        tag_classArray1[6] = TAG_CLASS.SHAMAN;
        tag_classArray1[7] = TAG_CLASS.WARLOCK;
        tag_classArray1[8] = TAG_CLASS.WARRIOR;
        CLASS_TAB_ORDER = tag_classArray1;
        CollectionManagerDisplay.ViewMode[] modeArray1 = new CollectionManagerDisplay.ViewMode[3];
        modeArray1[1] = CollectionManagerDisplay.ViewMode.CARD_BACKS;
        modeArray1[2] = CollectionManagerDisplay.ViewMode.HERO_SKINS;
        LARGE_TAB_ORDER = modeArray1;
        SELECT_TAB_ANIM_TIME = 0.2f;
        PREV_PAGE_LOCAL_POS = new Vector3(0f, 0.5f, 0f);
        CURRENT_PAGE_LOCAL_POS = new Vector3(0f, 0.25f, 0f);
        NEXT_PAGE_LOCAL_POS = Vector3.zero;
        CLASS_TAB_LOCAL_EULERS = new Vector3(0f, 180f, 0f);
        HIDDEN_TAB_LOCAL_Z_POS = -0.42f;
        ARROW_SCALE_TIME = 0.6f;
        ANIMATE_TABS_COROUTINE_NAME = "AnimateTabs";
        SELECT_TAB_COROUTINE_NAME = "SelectTabWhenReady";
        SHOW_ARROWS_COROUTINE_NAME = "WaitThenShowArrows";
        NUM_PAGE_FLIPS_BEFORE_STOP_SHOWING_ARROWS = 20;
        MASS_DISENCHANT_PAGE_NUM = 0x3e8;
        CLASS_TO_TAB_IDX = null;
    }

    public CollectionPageManager()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            iOS = true,
            Android = true,
            PC = true,
            Mac = true
        };
        this.ANIMATE_PAGE_TRANSITIONS = value2;
        value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            iOS = true,
            Android = true,
            PC = false,
            Mac = false
        };
        this.ALWAYS_SHOW_PAGING_ARROWS = value2;
    }

    private void ActivateArrows(bool leftArrow, bool rightArrow)
    {
        this.m_pageLeftClickableRegion.enabled = leftArrow;
        this.m_pageLeftClickableRegion.SetEnabled(leftArrow);
        this.m_pageRightClickableRegion.enabled = rightArrow;
        this.m_pageRightClickableRegion.SetEnabled(rightArrow);
        this.ShowArrow(this.m_pageLeftArrow, leftArrow, false);
        this.ShowArrow(this.m_pageRightArrow, rightArrow, true);
    }

    public void AddClassFilter(TAG_CLASS shownClass, bool skipPageTurn)
    {
        this.AddClassFilter(shownClass, skipPageTurn, null, null);
    }

    public void AddClassFilter(TAG_CLASS shownClass, bool skipPageTurn, DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_classConstraint = shownClass;
        this.m_skipNextPageTurn = skipPageTurn;
        List<object> values = new List<object> {
            (int) this.m_classConstraint,
            0
        };
        foreach (KeyValuePair<CollectionManagerDisplay.ViewMode, SortedCollection> pair in this.m_sortedCollections)
        {
            pair.Value.m_filterSet.AddGameFilter(GAME_TAG.CLASS, values, CollectionFilterFunc.EQUAL);
        }
        this.UpdateSortedCollectionFromFilterSet();
        if (CollectionManagerDisplay.Get().GetViewMode() == CollectionManagerDisplay.ViewMode.CARDS)
        {
            this.JumpToCollectionClassPage(shownClass, callback, callbackData);
        }
        else
        {
            this.TransitionPageWhenReady(PageTransitionType.NONE, false, null, null);
        }
    }

    private void AddGameFilter(GAME_TAG key, object val, CollectionFilterFunc func)
    {
        foreach (KeyValuePair<CollectionManagerDisplay.ViewMode, SortedCollection> pair in this.m_sortedCollections)
        {
            pair.Value.m_filterSet.AddGameFilter(key, val, func);
        }
        this.UpdateSortedCollectionFromFilterSet();
    }

    [DebuggerHidden]
    private IEnumerator AnimateTabs()
    {
        return new <AnimateTabs>c__Iterator3C { <>f__this = this };
    }

    public bool ArePagesTurning()
    {
        return this.m_pagesCurrentlyTurning;
    }

    private bool AssembleBasePage(TransitionReadyCallbackData transitionReadyCallbackData, bool emptyPage)
    {
        <AssembleBasePage>c__AnonStorey2DB storeydb;
        storeydb = new <AssembleBasePage>c__AnonStorey2DB {
            transitionReadyCallbackData = transitionReadyCallbackData,
            <>f__this = this,
            page = storeydb.transitionReadyCallbackData.m_assembledPage
        };
        bool flag = MASS_DISENCHANT_PAGE_NUM == this.m_currentPageNum;
        if (flag)
        {
            base.StartCoroutine(this.ShowMassDisenchantPage());
            storeydb.page.ActivatePageCountText(false);
        }
        else
        {
            if (this.m_massDisenchant != null)
            {
                this.m_massDisenchant.Hide();
            }
            storeydb.page.ActivatePageCountText(true);
        }
        if (emptyPage)
        {
            this.AssembleEmptyPageUI(storeydb.page, !flag);
            CollectionManagerDisplay.Get().CollectionPageContentsChanged(null, new CollectionManagerDisplay.CollectionActorsReadyCallback(storeydb.<>m__B2), null);
            return true;
        }
        return false;
    }

    private void AssembleCardBackPage(TransitionReadyCallbackData transitionReadyCallbackData)
    {
        <AssembleCardBackPage>c__AnonStorey2DD storeydd = new <AssembleCardBackPage>c__AnonStorey2DD {
            transitionReadyCallbackData = transitionReadyCallbackData,
            <>f__this = this
        };
        bool emptyPage = this.m_currentPageNum == MASS_DISENCHANT_PAGE_NUM;
        if (!this.AssembleBasePage(storeydd.transitionReadyCallbackData, emptyPage))
        {
            storeydd.page = storeydd.transitionReadyCallbackData.m_assembledPage;
            int count = this.GetCurrentDeckTrayModeCardBackIds().Count;
            int maxNumCards = CollectionPageDisplay.GetMaxNumCards();
            int max = (count / maxNumCards) + (((count % maxNumCards) <= 0) ? 0 : 1);
            this.m_currentPageNum = Mathf.Clamp(this.m_currentPageNum, 1, max);
            storeydd.page.SetCardBacks();
            storeydd.page.ShowNoMatchesFound(false);
            object[] args = new object[] { this.m_currentPageNum };
            storeydd.page.SetPageCountText(GameStrings.Format("GLUE_COLLECTION_PAGE_NUM", args));
            this.ActivateArrows(this.m_currentPageNum > 1, this.m_currentPageNum < max);
            CollectionManagerDisplay.Get().CollectionPageContentsChangedToCardBacks(this.m_currentPageNum, maxNumCards, new CollectionManagerDisplay.CollectionActorsReadyCallback(storeydd.<>m__B4), null, this.m_classConstraint == TAG_CLASS.INVALID);
        }
    }

    private void AssembleCardPage(TransitionReadyCallbackData transitionReadyCallbackData, List<FilteredArtStack> artStacksToDisplay)
    {
        <AssembleCardPage>c__AnonStorey2DC storeydc = new <AssembleCardPage>c__AnonStorey2DC {
            transitionReadyCallbackData = transitionReadyCallbackData,
            <>f__this = this
        };
        bool emptyPage = (artStacksToDisplay == null) || (artStacksToDisplay.Count == 0);
        if (!this.AssembleBasePage(storeydc.transitionReadyCallbackData, emptyPage))
        {
            storeydc.page = storeydc.transitionReadyCallbackData.m_assembledPage;
            FilteredArtStack stack = artStacksToDisplay[0];
            SortedCollection currentSortedCollection = this.GetCurrentSortedCollection();
            if (currentSortedCollection.m_filterSet.IsTextFilterEmpty())
            {
                this.m_preSearchArtStackAnchor = stack.Clone();
            }
            EntityDef entityDef = DefLoader.Get().GetEntityDef(stack.CardID);
            if (CollectionManagerDisplay.Get().GetViewMode() == CollectionManagerDisplay.ViewMode.HERO_SKINS)
            {
                storeydc.page.SetHeroSkins((CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing) == null) ? TAG_CLASS.INVALID : entityDef.GetClass());
            }
            else
            {
                storeydc.page.SetClass(new TAG_CLASS?(entityDef.GetClass()));
            }
            object[] args = new object[] { this.m_currentPageNum };
            storeydc.page.SetPageCountText(GameStrings.Format("GLUE_COLLECTION_PAGE_NUM", args));
            storeydc.page.ShowNoMatchesFound(false);
            int totalNumPages = currentSortedCollection.GetTotalNumPages();
            this.ActivateArrows(this.m_currentPageNum > 1, this.m_currentPageNum < totalNumPages);
            CollectionManagerDisplay.Get().CollectionPageContentsChanged(artStacksToDisplay, new CollectionManagerDisplay.CollectionActorsReadyCallback(storeydc.<>m__B3), null);
        }
    }

    private void AssembleEmptyPageUI(CollectionPageDisplay page, bool displayNoMatchesText)
    {
        page.SetClass(null);
        page.ShowNoMatchesFound(displayNoMatchesText);
        if (CollectionManagerDisplay.Get().GetViewMode() == CollectionManagerDisplay.ViewMode.CARDS)
        {
            this.DeselectCurrentClassTab();
        }
        page.SetPageCountText(GameStrings.Get("GLUE_COLLECTION_EMPTY_PAGE"));
        this.ActivateArrows(false, false);
    }

    private void Awake()
    {
        this.m_sortedCollections[CollectionManagerDisplay.ViewMode.CARDS] = this.m_cardsSortedCollection;
        this.m_sortedCollections[CollectionManagerDisplay.ViewMode.HERO_SKINS] = this.m_heroSkinsSortedCollection;
        this.m_cardsSortedCollection.m_filterSet.AddGameFilter(GAME_TAG.CARDTYPE, TAG_CARDTYPE.HERO, CollectionFilterFunc.NOT_EQUAL);
        this.m_heroSkinsSortedCollection.m_filterSet.AddGameFilter(GAME_TAG.CARDTYPE, TAG_CARDTYPE.HERO, CollectionFilterFunc.EQUAL);
        this.m_heroSkinsSortedCollection.m_filterSet.RemoveAccountFilter(ACCOUNT_TAG.OWNED_COUNT, 1, CollectionFilterFunc.GREATER_EQUAL);
        if (<>f__am$cache3E == null)
        {
            <>f__am$cache3E = delegate (FilteredArtStack lhs, FilteredArtStack rhs) {
                <Awake>c__AnonStorey2D9 storeyd = new <Awake>c__AnonStorey2D9();
                bool flag = lhs.Count > 0;
                bool flag2 = rhs.Count > 0;
                if (flag != flag2)
                {
                    return !flag ? 1 : -1;
                }
                storeyd.lhsDef = DefLoader.Get().GetEntityDef(lhs.CardID);
                storeyd.rhsDef = DefLoader.Get().GetEntityDef(rhs.CardID);
                int num = Array.FindIndex<TAG_CLASS>(CLASS_TAB_ORDER, new Predicate<TAG_CLASS>(storeyd.<>m__B5));
                int num2 = Array.FindIndex<TAG_CLASS>(CLASS_TAB_ORDER, new Predicate<TAG_CLASS>(storeyd.<>m__B6));
                int num3 = num - num2;
                if (num3 == 0)
                {
                    return string.Compare(lhs.CardID, rhs.CardID);
                }
                return Mathf.Clamp(num3, -1, 1);
            };
        }
        this.m_heroSkinsSortedCollection.SetSortDelegate(<>f__am$cache3E);
        if (CLASS_TO_TAB_IDX == null)
        {
            CLASS_TO_TAB_IDX = new Map<TAG_CLASS, int>();
            for (int i = 0; i < CLASS_TAB_ORDER.Length; i++)
            {
                CLASS_TO_TAB_IDX.Add(CLASS_TAB_ORDER[i], i);
            }
        }
        this.UpdateSortedCollectionFromFilterSet();
        if (this.m_massDisenchant != null)
        {
            this.m_massDisenchant.Hide();
        }
        this.m_pageLeftClickableRegion.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnPageLeftPressed));
        this.m_pageLeftClickableRegion.SetCursorOver(PegCursor.Mode.LEFTARROW);
        this.m_pageLeftClickableRegion.SetCursorDown(PegCursor.Mode.LEFTARROW);
        this.m_pageRightClickableRegion.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnPageRightPressed));
        this.m_pageRightClickableRegion.SetCursorOver(PegCursor.Mode.RIGHTARROW);
        this.m_pageRightClickableRegion.SetCursorDown(PegCursor.Mode.RIGHTARROW);
        CollectionManagerDisplay.Get().RegisterSwitchViewModeListener(new CollectionManagerDisplay.OnSwitchViewMode(this.OnCollectionManagerViewModeChanged));
        this.m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
        if (UniversalInputManager.Get().IsTouchMode())
        {
            base.gameObject.AddComponent<CollectionPageManagerTouchBehavior>();
        }
        this.m_pageA = UnityEngine.Object.Instantiate<CollectionPageDisplay>(this.m_pageDisplayPrefab);
        this.m_pageB = UnityEngine.Object.Instantiate<CollectionPageDisplay>(this.m_pageDisplayPrefab);
        TransformUtil.AttachAndPreserveLocalTransform(this.m_pageA.transform, base.transform);
        TransformUtil.AttachAndPreserveLocalTransform(this.m_pageB.transform, base.transform);
        CollectionManager manager = CollectionManager.Get();
        manager.RegisterFavoriteHeroChangedListener(new CollectionManager.FavoriteHeroChangedCallback(this.OnFavoriteHeroChanged));
        manager.RegisterDefaultCardbackChangedListener(new CollectionManager.DefaultCardbackChangedCallback(this.OnDefaultCardbackChanged));
    }

    public void ChangeSearchTextFilter(string newSearchText, bool updateVisuals = true)
    {
        this.ChangeSearchTextFilter(newSearchText, null, null, updateVisuals);
    }

    public void ChangeSearchTextFilter(string newSearchText, DelOnPageTransitionComplete callback, object callbackData, bool updateVisuals = true)
    {
        this.m_cardsSortedCollection.m_filterSet.SetTextFilterValue(newSearchText);
        this.UpdateSortedCollectionFromFilterSet();
        if (updateVisuals)
        {
            this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
        }
    }

    private void DeselectCurrentClassTab()
    {
        if (this.m_currentClassTab != null)
        {
            this.m_currentClassTab.SetSelected(false);
            this.m_currentClassTab.SetLargeTab(false);
            this.m_currentClassTab = null;
        }
    }

    private void DestroyArrow(GameObject arrow)
    {
        UnityEngine.Object.Destroy(arrow);
    }

    public void FilterByCardSet(TAG_CARD_SET? cardSet)
    {
        this.FilterByCardSet(cardSet, null, null);
    }

    public void FilterByCardSet(TAG_CARD_SET? cardSet, DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_cardsSortedCollection.m_filterSet.RemoveAllGameFiltersByTag(GAME_TAG.CARD_SET);
        if (cardSet.HasValue)
        {
            this.m_cardsSortedCollection.m_filterSet.AddGameFilter(GAME_TAG.CARD_SET, cardSet, CollectionFilterFunc.EQUAL);
        }
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
    }

    public void FilterByCardSets(List<object> cardSets)
    {
        this.FilterByCardSets(cardSets, null, null);
    }

    public void FilterByCardSets(List<object> cardSets, DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_cardsSortedCollection.m_filterSet.RemoveAllGameFiltersByTag(GAME_TAG.CARD_SET);
        if (cardSets != null)
        {
            this.m_cardsSortedCollection.m_filterSet.AddGameFilter(GAME_TAG.CARD_SET, cardSets, CollectionFilterFunc.EQUAL);
        }
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
    }

    public void FilterByManaCost(int cost)
    {
        this.FilterByManaCost(cost, null, null);
    }

    public void FilterByManaCost(int cost, DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_cardsSortedCollection.m_filterSet.RemoveAllGameFiltersByTag(GAME_TAG.COST);
        if (cost != ManaFilterTab.ALL_TAB_IDX)
        {
            CollectionFilterFunc func = (cost != ManaFilterTab.SEVEN_PLUS_TAB_IDX) ? CollectionFilterFunc.EQUAL : CollectionFilterFunc.GREATER_EQUAL;
            this.m_cardsSortedCollection.m_filterSet.AddGameFilter(GAME_TAG.COST, cost, func);
        }
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
    }

    private void FlipToPage(int newCollectionPage, DelOnPageTransitionComplete callback, object callbackData)
    {
        int num = newCollectionPage - this.m_currentPageNum;
        bool flag = num < 0;
        if (Math.Abs(num) != 1)
        {
            this.m_currentPageNum = newCollectionPage;
            this.TransitionPageWhenReady(!flag ? PageTransitionType.MANY_PAGE_RIGHT : PageTransitionType.MANY_PAGE_LEFT, true, callback, callbackData);
        }
        else if (flag)
        {
            this.PageLeft(callback, callbackData);
        }
        else
        {
            this.PageRight(callback, callbackData);
        }
    }

    private CollectionPageDisplay GetAlternatePage()
    {
        return (!this.m_currentPageIsPageA ? this.m_pageA : this.m_pageB);
    }

    public CollectionCardVisual GetCardVisual(string cardID, CardFlair cardFlair)
    {
        return this.GetCurrentPage().GetCardVisual(cardID, cardFlair);
    }

    private HashSet<int> GetCurrentDeckTrayModeCardBackIds()
    {
        return CardBackManager.Get().GetCardBackIds(this.m_classConstraint == TAG_CLASS.INVALID);
    }

    private CollectionPageDisplay GetCurrentPage()
    {
        return (!this.m_currentPageIsPageA ? this.m_pageB : this.m_pageA);
    }

    private SortedCollection GetCurrentSortedCollection()
    {
        SortedCollection sorteds = null;
        this.m_sortedCollections.TryGetValue(CollectionManagerDisplay.Get().GetViewMode(), out sorteds);
        return sorteds;
    }

    private int GetLastPageInCurrentMode()
    {
        switch (CollectionManagerDisplay.Get().GetViewMode())
        {
            case CollectionManagerDisplay.ViewMode.CARD_BACKS:
            {
                int count = this.GetCurrentDeckTrayModeCardBackIds().Count;
                int maxNumCards = CollectionPageDisplay.GetMaxNumCards();
                return ((count / maxNumCards) + (((count % maxNumCards) <= 0) ? 0 : 1));
            }
        }
        return this.GetCurrentSortedCollection().GetTotalNumPages();
    }

    public int GetMassDisenchantAmount()
    {
        return CollectionManager.Get().GetCardsToDisenchantCount();
    }

    public int GetNumNewCardsForClass(TAG_CLASS tagClass)
    {
        return this.m_cardsSortedCollection.GetNumNewCardsForClass(tagClass);
    }

    public int GetNumPagesForClass(TAG_CLASS classTag)
    {
        return this.m_cardsSortedCollection.GetNumPagesForClass(classTag);
    }

    public bool HaveCardsToMassDisenchant()
    {
        return (CollectionManager.Get().GetCardsToDisenchantCount() > 0);
    }

    public void HideMassDisenchant()
    {
        bool flag = this.IsShowingMassDisenchant();
        this.m_currentPageNum = this.m_lastPageNum;
        this.m_cardsSortedCollection.m_filterSet.SetIncludeOnlyCraftable(false);
        this.m_cardsSortedCollection.m_filterSet.ClearFlairRestriction();
        this.m_cardsSortedCollection.m_filterSet.IncludeCardsNotOwned(new List<CardFlair>());
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(!flag ? PageTransitionType.NONE : PageTransitionType.MANY_PAGE_LEFT, false, null, null);
    }

    private void HideThenDestroyArrow(GameObject arrow)
    {
        if (arrow != null)
        {
            object[] args = new object[] { "scale", Vector3.zero, "time", 0.3f, "oncomplete", "DestroyArrow", "oncompletetarget", base.gameObject, "oncompleteparams", arrow, "name", "scale" };
            Hashtable hashtable = iTween.Hash(args);
            iTween.StopByName(arrow, "scale");
            iTween.ScaleTo(arrow, hashtable);
        }
    }

    private bool IsEditingDeck()
    {
        return false;
    }

    public bool IsFullyLoaded()
    {
        return this.m_fullyLoaded;
    }

    public bool IsShowingMassDisenchant()
    {
        return (this.m_currentPageNum == MASS_DISENCHANT_PAGE_NUM);
    }

    public void JumpToCollectionClassPage(TAG_CLASS pageClass)
    {
        this.JumpToCollectionClassPage(pageClass, null, null);
    }

    public void JumpToCollectionClassPage(TAG_CLASS pageClass, DelOnPageTransitionComplete callback, object callbackData)
    {
        CollectionManagerDisplay display = CollectionManagerDisplay.Get();
        if (display.GetViewMode() != CollectionManagerDisplay.ViewMode.CARDS)
        {
            display.SetViewMode(CollectionManagerDisplay.ViewMode.CARDS, pageClass);
        }
        else
        {
            int num;
            this.GetCurrentSortedCollection().GetPageContentsForClass(pageClass, 1, true, out num);
            this.FlipToPage(num, callback, callbackData);
        }
    }

    public void JumpToPageWithCard(string cardID, CardFlair flair)
    {
        this.JumpToPageWithCard(cardID, flair, null, null);
    }

    public void JumpToPageWithCard(string cardID, CardFlair flair, DelOnPageTransitionComplete callback, object callbackData)
    {
        TAG_CLASS tag_class;
        int num;
        if ((this.GetCurrentSortedCollection().GetPageContentsForCard(cardID, flair, out tag_class, out num).Count != 0) && (this.m_currentPageNum != num))
        {
            this.FlipToPage(num, callback, callbackData);
        }
    }

    public void LoadPagingArrows()
    {
        if (!((Options.Get().GetInt(Option.PAGE_MOUSE_OVERS) >= NUM_PAGE_FLIPS_BEFORE_STOP_SHOWING_ARROWS) & (this.ALWAYS_SHOW_PAGING_ARROWS == 0)) && ((this.m_pageLeftArrow == null) || (this.m_pageRightArrow == null)))
        {
            AssetLoader.Get().LoadGameObject("PagingArrow", new AssetLoader.GameObjectCallback(this.OnPagingArrowLoaded), null, false);
        }
    }

    public void NotifyOfCollectionChanged()
    {
        this.UpdateMassDisenchant();
    }

    public void OnBookClosing()
    {
        this.DeselectCurrentClassTab();
        this.ActivateArrows(false, false);
    }

    public void OnBookOpening()
    {
        base.StopCoroutine(SHOW_ARROWS_COROUTINE_NAME);
        base.StartCoroutine(SHOW_ARROWS_COROUTINE_NAME);
        this.TransitionPageWhenReady(PageTransitionType.NONE, true, null, null);
    }

    private void OnCardBacksTabPressed(UIEvent e)
    {
        CollectionClassTab element = e.GetElement() as CollectionClassTab;
        if ((element != null) && (element != this.m_currentClassTab))
        {
            CollectionManagerDisplay.Get().SetViewMode(CollectionManagerDisplay.ViewMode.CARD_BACKS, null);
        }
    }

    private void OnClassTabPressed(UIEvent e)
    {
        CollectionClassTab element = e.GetElement() as CollectionClassTab;
        if ((element != null) && (element != this.m_currentClassTab))
        {
            TAG_CLASS pageClass = element.GetClass();
            this.JumpToCollectionClassPage(pageClass);
        }
    }

    public void OnCollectionLoaded()
    {
        this.ShowOnlyCardsIOwn();
    }

    private void OnCollectionManagerViewModeChanged(CollectionManagerDisplay.ViewMode prevMode, CollectionManagerDisplay.ViewMode mode, object userdata)
    {
        this.UpdateCraftingModeButtonDustBottleVisibility();
        if (this.m_useLastPage)
        {
            this.m_useLastPage = false;
            this.m_currentPageNum = this.GetLastPageInCurrentMode();
        }
        else
        {
            this.m_currentPageNum = 1;
            if (userdata != null)
            {
                this.GetCurrentSortedCollection().GetPageContentsForClass((TAG_CLASS) ((int) userdata), 1, true, out this.m_currentPageNum);
            }
        }
        int num = 0;
        int num2 = 0;
        for (int i = 0; i < LARGE_TAB_ORDER.Length; i++)
        {
            if (prevMode == LARGE_TAB_ORDER[i])
            {
                num = i;
            }
            if (mode == LARGE_TAB_ORDER[i])
            {
                num2 = i;
            }
        }
        if ((mode == CollectionManagerDisplay.ViewMode.CARD_BACKS) || (mode == CollectionManagerDisplay.ViewMode.HERO_SKINS))
        {
            CollectionManagerDisplay.Get().ResetFilters(false);
            this.m_cardsSortedCollection.m_filterSet.RemoveAllGameFiltersByTag(GAME_TAG.CARD_SET);
        }
        PageTransitionType transitionType = ((num2 - num) >= 0) ? PageTransitionType.SINGLE_PAGE_RIGHT : PageTransitionType.SINGLE_PAGE_LEFT;
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(transitionType, true, null, null);
    }

    public void OnDefaultCardbackChanged(int newDefaultCardBackID, object userData)
    {
        this.GetCurrentPage().UpdateFavoriteCardBack(CollectionManagerDisplay.Get().GetViewMode());
    }

    private void OnDestroy()
    {
        if (CollectionManagerDisplay.Get() != null)
        {
            CollectionManagerDisplay.Get().RemoveSwitchViewModeListener(new CollectionManagerDisplay.OnSwitchViewMode(this.OnCollectionManagerViewModeChanged));
        }
        CollectionManager manager = CollectionManager.Get();
        if (manager != null)
        {
            manager.RemoveFavoriteHeroChangedListener(new CollectionManager.FavoriteHeroChangedCallback(this.OnFavoriteHeroChanged));
            manager.RemoveDefaultCardbackChangedListener(new CollectionManager.DefaultCardbackChangedCallback(this.OnDefaultCardbackChanged));
        }
    }

    public void OnDoneEditingDeck()
    {
        this.RemoveAllClassFilters();
        this.UpdateCraftingModeButtonDustBottleVisibility();
    }

    public void OnFavoriteHeroChanged(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero, object userData)
    {
        this.GetCurrentPage().UpdateFavoriteHeroSkins(CollectionManagerDisplay.Get().GetViewMode(), this.IsShowingMassDisenchant());
    }

    private void OnHeroSkinsTabPressed(UIEvent e)
    {
        CollectionClassTab element = e.GetElement() as CollectionClassTab;
        if ((element != null) && (element != this.m_currentClassTab))
        {
            CollectionManagerDisplay.Get().SetViewMode(CollectionManagerDisplay.ViewMode.HERO_SKINS, null);
        }
    }

    private void OnLoadedMassDisenchant(string name, GameObject screen, object callbackData)
    {
        this.m_massDisenchant = screen.GetComponent<MassDisenchant>();
        this.m_massDisenchant.Show();
    }

    private void OnPageFlip()
    {
        int @int = Options.Get().GetInt(Option.PAGE_MOUSE_OVERS);
        int val = @int + 1;
        if (@int < NUM_PAGE_FLIPS_BEFORE_STOP_SHOWING_ARROWS)
        {
            Options.Get().SetInt(Option.PAGE_MOUSE_OVERS, val);
        }
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CM_PAGE_TURNED);
    }

    private void OnPageLeftPressed(UIEvent e)
    {
        this.OnPageFlip();
        SoundManager.Get().LoadAndPlay("collection_manager_book_page_flip_back");
        this.PageLeft(null, null);
    }

    private void OnPageRightPressed(UIEvent e)
    {
        this.OnPageFlip();
        SoundManager.Get().LoadAndPlay("collection_manager_book_page_flip_forward");
        this.PageRight(null, null);
    }

    private void OnPageTurnComplete(object callbackData)
    {
        Resources.UnloadUnusedAssets();
        TransitionReadyCallbackData data = callbackData as TransitionReadyCallbackData;
        CollectionPageDisplay assembledPage = data.m_assembledPage;
        CollectionPageDisplay otherPage = data.m_otherPage;
        switch (data.m_transitionType)
        {
            case PageTransitionType.SINGLE_PAGE_LEFT:
            case PageTransitionType.MANY_PAGE_LEFT:
                this.PositionCurrentPage(assembledPage);
                this.PositionNextPage(otherPage);
                break;
        }
        if (otherPage != this.GetCurrentPage())
        {
            otherPage.transform.position = new Vector3(-300f, 0f, -300f);
        }
        if (data.m_callback != null)
        {
            data.m_callback(data.m_callbackData);
        }
        if (data.m_assembledPage == this.GetCurrentPage())
        {
            this.m_pagesCurrentlyTurning = false;
        }
    }

    private void OnPagingArrowLoaded(string name, GameObject go, object callbackData)
    {
        if (go != null)
        {
            if (this.m_pageLeftArrow == null)
            {
                this.m_pageLeftArrow = go;
                this.m_pageLeftArrow.transform.parent = base.transform;
                this.m_pageLeftArrow.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
                this.m_pageLeftArrow.transform.position = this.m_pageLeftArrowBone.transform.position;
                this.m_pageLeftArrow.transform.localScale = Vector3.zero;
                bool enabled = this.m_pageLeftClickableRegion.enabled;
                SceneUtils.SetLayer(this.m_pageLeftArrow, GameLayer.TransparentFX);
                this.ShowArrow(this.m_pageLeftArrow, enabled, false);
            }
            if (this.m_pageRightArrow == null)
            {
                this.m_pageRightArrow = UnityEngine.Object.Instantiate<GameObject>(this.m_pageLeftArrow);
                this.m_pageRightArrow.transform.parent = base.transform;
                this.m_pageRightArrow.transform.localEulerAngles = Vector3.zero;
                this.m_pageRightArrow.transform.position = this.m_pageRightArrowBone.transform.position;
                this.m_pageRightArrow.transform.localScale = Vector3.zero;
                bool show = this.m_pageRightClickableRegion.enabled;
                SceneUtils.SetLayer(this.m_pageRightArrow, GameLayer.TransparentFX);
                this.ShowArrow(this.m_pageRightArrow, show, true);
            }
        }
    }

    private void OnTabOut(UIEvent e)
    {
        CollectionClassTab element = e.GetElement() as CollectionClassTab;
        if (element != null)
        {
            element.SetGlowActive(false);
        }
    }

    private void OnTabOut_Touch(UIEvent e)
    {
        if (UniversalInputManager.Get().IsTouchMode())
        {
            CollectionClassTab element = e.GetElement() as CollectionClassTab;
            if (element != this.m_currentClassTab)
            {
                element.SetLargeTab(false);
            }
        }
    }

    private void OnTabOver(UIEvent e)
    {
        CollectionClassTab element = e.GetElement() as CollectionClassTab;
        if (element != null)
        {
            element.SetGlowActive(true);
        }
    }

    private void OnTabOver_Touch(UIEvent e)
    {
        if (UniversalInputManager.Get().IsTouchMode())
        {
            (e.GetElement() as CollectionClassTab).SetLargeTab(true);
        }
    }

    private void PageLeft(DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_currentPageNum--;
        this.TransitionPageWhenReady(PageTransitionType.SINGLE_PAGE_LEFT, true, callback, callbackData);
    }

    private void PageRight(DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_currentPageNum++;
        this.TransitionPageWhenReady(PageTransitionType.SINGLE_PAGE_RIGHT, true, callback, callbackData);
    }

    private void PositionClassTabs(bool animate)
    {
        Vector3 position = this.m_classTabContainer.transform.position;
        for (int i = 0; i < CLASS_TAB_ORDER.Length; i++)
        {
            Vector3 localPosition;
            CollectionClassTab tab = this.m_classTabs[i];
            if (this.ShouldShowTab(tab))
            {
                tab.SetTargetVisibility(true);
                position.x += this.m_spaceBetweenTabs;
                position.x += this.m_deselectedClassTabHalfWidth;
                localPosition = this.m_classTabContainer.transform.InverseTransformPoint(position);
                if (tab == this.m_currentClassTab)
                {
                    localPosition.y = tab.m_SelectedLocalYPos;
                }
                position.x += this.m_deselectedClassTabHalfWidth;
            }
            else
            {
                tab.SetTargetVisibility(false);
                localPosition = tab.transform.localPosition;
                localPosition.z = HIDDEN_TAB_LOCAL_Z_POS;
            }
            if (animate)
            {
                tab.SetTargetLocalPosition(localPosition);
            }
            else
            {
                tab.SetIsVisible(tab.ShouldBeVisible());
                tab.transform.localPosition = localPosition;
            }
        }
        bool showTab = this.ShouldShowTab(this.m_heroSkinsTab);
        this.PositionFixedTab(showTab, this.m_heroSkinsTab, this.m_heroSkinsTabPos, animate);
        bool flag3 = this.ShouldShowTab(this.m_cardBacksTab);
        this.PositionFixedTab(flag3, this.m_cardBacksTab, this.m_cardBacksTabPos, animate);
        if (animate)
        {
            base.StopCoroutine(ANIMATE_TABS_COROUTINE_NAME);
            base.StartCoroutine(ANIMATE_TABS_COROUTINE_NAME);
        }
    }

    private void PositionCurrentPage(CollectionPageDisplay page)
    {
        page.transform.localPosition = CURRENT_PAGE_LOCAL_POS;
    }

    private void PositionFixedTab(bool showTab, CollectionClassTab tab, Vector3 originalPos, bool animate)
    {
        if (!showTab)
        {
            originalPos.z -= 0.5f;
        }
        tab.SetTargetVisibility(showTab);
        tab.SetTargetLocalPosition(originalPos);
        if (animate)
        {
            tab.AnimateToTargetPosition(0.4f, iTween.EaseType.easeOutQuad);
        }
        else
        {
            tab.SetIsVisible(tab.ShouldBeVisible());
            tab.transform.localPosition = originalPos;
        }
    }

    private void PositionNextPage(CollectionPageDisplay page)
    {
        page.transform.localPosition = NEXT_PAGE_LOCAL_POS;
    }

    private void PositionPreviousPage(CollectionPageDisplay page)
    {
        page.transform.localPosition = PREV_PAGE_LOCAL_POS;
    }

    public void RefreshCurrentPageContents()
    {
        this.RefreshCurrentPageContents(null, null);
    }

    public void RefreshCurrentPageContents(DelOnPageTransitionComplete callback, object callbackData)
    {
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, true, callback, callbackData);
    }

    private void RemoveAllClassFilters()
    {
        this.RemoveAllClassFilters(null, null);
    }

    private void RemoveAllClassFilters(DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_classConstraint = TAG_CLASS.INVALID;
        foreach (KeyValuePair<CollectionManagerDisplay.ViewMode, SortedCollection> pair in this.m_sortedCollections)
        {
            pair.Value.m_filterSet.RemoveAllGameFiltersByTag(GAME_TAG.CLASS);
        }
        this.UpdateSortedCollectionFromFilterSet();
        PageTransitionType transitionType = (CollectionManagerDisplay.Get().GetViewMode() != CollectionManagerDisplay.ViewMode.CARDS) ? PageTransitionType.NONE : PageTransitionType.SINGLE_PAGE_LEFT;
        this.TransitionPageWhenReady(transitionType, false, callback, callbackData);
    }

    public void RemoveSearchTextFilter()
    {
        this.RemoveSearchTextFilter(null, null);
    }

    public void RemoveSearchTextFilter(DelOnPageTransitionComplete callback, object callbackData)
    {
        bool flag = false;
        foreach (KeyValuePair<CollectionManagerDisplay.ViewMode, SortedCollection> pair in this.m_sortedCollections)
        {
            SortedCollection sorteds = pair.Value;
            if (!sorteds.m_filterSet.IsTextFilterEmpty())
            {
                flag = true;
                sorteds.m_filterSet.RemoveTextFilter();
            }
        }
        if (flag)
        {
            this.UpdateSortedCollectionFromFilterSet();
            this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
        }
    }

    [DebuggerHidden]
    private IEnumerator SelectTabWhenReady(CollectionClassTab tab)
    {
        return new <SelectTabWhenReady>c__Iterator3D { tab = tab, <$>tab = tab, <>f__this = this };
    }

    private void SetCurrentClassTab(TAG_CLASS? tabClass)
    {
        <SetCurrentClassTab>c__AnonStorey2DA storeyda = new <SetCurrentClassTab>c__AnonStorey2DA {
            tabClass = tabClass
        };
        CollectionClassTab heroSkinsTab = null;
        CollectionManagerDisplay.ViewMode viewMode = CollectionManagerDisplay.Get().GetViewMode();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_classFilterHeader.SetMode(viewMode, storeyda.tabClass);
        }
        else
        {
            switch (viewMode)
            {
                case CollectionManagerDisplay.ViewMode.CARDS:
                    if (storeyda.tabClass.HasValue)
                    {
                        heroSkinsTab = this.m_classTabs.Find(new Predicate<CollectionClassTab>(storeyda.<>m__B1));
                    }
                    break;

                case CollectionManagerDisplay.ViewMode.HERO_SKINS:
                    heroSkinsTab = this.m_heroSkinsTab;
                    break;

                case CollectionManagerDisplay.ViewMode.CARD_BACKS:
                    heroSkinsTab = this.m_cardBacksTab;
                    break;

                default:
                    heroSkinsTab = null;
                    break;
            }
            if (heroSkinsTab != this.m_currentClassTab)
            {
                this.DeselectCurrentClassTab();
                this.m_currentClassTab = heroSkinsTab;
                if (this.m_currentClassTab != null)
                {
                    base.StopCoroutine(SELECT_TAB_COROUTINE_NAME);
                    base.StartCoroutine(SELECT_TAB_COROUTINE_NAME, this.m_currentClassTab);
                }
            }
        }
    }

    private void SetUpClassTabs()
    {
        if (UniversalInputManager.UsePhoneUI == null)
        {
            bool receiveReleaseWithoutMouseDown = UniversalInputManager.Get().IsTouchMode();
            for (int i = 0; i < CLASS_TAB_ORDER.Length; i++)
            {
                TAG_CLASS tag_class = CLASS_TAB_ORDER[i];
                CollectionClassTab item = (CollectionClassTab) GameUtils.Instantiate(this.m_classTabPrefab, this.m_classTabContainer, false);
                item.Init(new TAG_CLASS?(tag_class));
                item.transform.localScale = item.m_DeselectedLocalScale;
                item.transform.localEulerAngles = CLASS_TAB_LOCAL_EULERS;
                item.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClassTabPressed));
                item.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnTabOver));
                item.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnTabOut));
                item.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnTabOver_Touch));
                item.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnTabOut_Touch));
                item.SetReceiveReleaseWithoutMouseDown(receiveReleaseWithoutMouseDown);
                item.gameObject.name = tag_class.ToString();
                this.m_allTabs.Add(item);
                this.m_classTabs.Add(item);
                this.m_tabVisibility[item] = true;
                if (i <= 0)
                {
                    this.m_deselectedClassTabHalfWidth = item.GetComponent<BoxCollider>().bounds.extents.x;
                }
            }
            if (this.m_heroSkinsTab != null)
            {
                this.m_heroSkinsTab.Init(null);
                this.m_heroSkinsTab.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnHeroSkinsTabPressed));
                this.m_heroSkinsTab.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnTabOver));
                this.m_heroSkinsTab.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnTabOut));
                this.m_heroSkinsTab.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnTabOver_Touch));
                this.m_heroSkinsTab.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnTabOut_Touch));
                this.m_heroSkinsTab.SetReceiveReleaseWithoutMouseDown(receiveReleaseWithoutMouseDown);
                this.m_allTabs.Add(this.m_heroSkinsTab);
                this.m_tabVisibility[this.m_heroSkinsTab] = true;
                this.m_heroSkinsTabPos = this.m_heroSkinsTab.transform.localPosition;
            }
            if (this.m_cardBacksTab != null)
            {
                this.m_cardBacksTab.Init(null);
                this.m_cardBacksTab.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCardBacksTabPressed));
                this.m_cardBacksTab.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnTabOver));
                this.m_cardBacksTab.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnTabOut));
                this.m_cardBacksTab.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnTabOver_Touch));
                this.m_cardBacksTab.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnTabOut_Touch));
                this.m_cardBacksTab.SetReceiveReleaseWithoutMouseDown(receiveReleaseWithoutMouseDown);
                this.m_allTabs.Add(this.m_cardBacksTab);
                this.m_tabVisibility[this.m_cardBacksTab] = true;
                this.m_cardBacksTabPos = this.m_cardBacksTab.transform.localPosition;
            }
            this.PositionClassTabs(false);
            this.m_initializedTabPositions = true;
        }
    }

    private bool ShouldShowTab(CollectionClassTab tab)
    {
        if (this.m_initializedTabPositions)
        {
            if (tab.m_tabViewMode == CollectionManagerDisplay.ViewMode.CARDS)
            {
                TAG_CLASS cardClass = tab.GetClass();
                bool flag = this.m_cardsSortedCollection.GetNumPagesForClass(cardClass) > 0;
                if ((this.m_classConstraint != TAG_CLASS.INVALID) && ((cardClass != this.m_classConstraint) && (cardClass != TAG_CLASS.INVALID)))
                {
                    return false;
                }
                return flag;
            }
            CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
            if (taggedDeck != null)
            {
                switch (tab.m_tabViewMode)
                {
                    case CollectionManagerDisplay.ViewMode.HERO_SKINS:
                        if (CollectionManager.Get().GetBestHeroesIOwn(taggedDeck.GetClass()).Count <= 1)
                        {
                            return false;
                        }
                        break;

                    case CollectionManagerDisplay.ViewMode.CARD_BACKS:
                        if (CardBackManager.Get().GetCardBacksOwned().Count <= 1)
                        {
                            return false;
                        }
                        break;
                }
            }
        }
        return true;
    }

    private void ShowArrow(GameObject arrow, bool show, bool isRightArrow)
    {
        if ((arrow != null) && !this.m_delayShowingArrows)
        {
            if (isRightArrow)
            {
                if (this.m_rightArrowShown == show)
                {
                    return;
                }
                this.m_rightArrowShown = show;
            }
            else
            {
                if (this.m_leftArrowShown == show)
                {
                    return;
                }
                this.m_leftArrowShown = show;
            }
            GameObject obj2 = !isRightArrow ? this.m_pageLeftArrowBone : this.m_pageRightArrowBone;
            Vector3 localScale = obj2.transform.localScale;
            Vector3 vector2 = !show ? Vector3.zero : localScale;
            iTween.EaseType type = !show ? iTween.EaseType.linear : iTween.EaseType.easeOutElastic;
            object[] args = new object[] { "scale", vector2, "time", ARROW_SCALE_TIME, "easetype", type, "name", "ArrowScale" };
            Hashtable hashtable = iTween.Hash(args);
            iTween.StopByName(arrow, "ArrowScale");
            iTween.ScaleTo(arrow, hashtable);
        }
    }

    public void ShowCardsNotOwned(bool includePremiums)
    {
        this.ShowCardsNotOwned(includePremiums, null, null);
    }

    public void ShowCardsNotOwned(bool includePremiums, DelOnPageTransitionComplete callback, object callbackData)
    {
        List<CardFlair> flairTypes = new List<CardFlair> {
            new CardFlair(0)
        };
        if (includePremiums)
        {
            flairTypes.Add(new CardFlair(TAG_PREMIUM.GOLDEN));
        }
        this.m_cardsSortedCollection.m_filterSet.ClearFlairRestriction();
        this.m_cardsSortedCollection.m_filterSet.IncludeCardsNotOwned(flairTypes);
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
    }

    public void ShowCraftableCardsOnly(bool showCraftableCardsOnly)
    {
        this.ShowCraftableCardsOnly(showCraftableCardsOnly, null, null);
    }

    public void ShowCraftableCardsOnly(bool showCraftableCardsOnly, DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_cardsSortedCollection.m_filterSet.SetIncludeOnlyCraftable(showCraftableCardsOnly);
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
    }

    public void ShowCraftingModeCards(bool showCraftableCardsOnly, bool showGolden)
    {
        this.ShowCraftingModeCards(showCraftableCardsOnly, showGolden, null, null);
    }

    public void ShowCraftingModeCards(bool showCraftableCardsOnly, bool showGolden, DelOnPageTransitionComplete callback, object callbackData)
    {
        SortedCollection cardsSortedCollection = this.m_cardsSortedCollection;
        cardsSortedCollection.m_filterSet.SetIncludeOnlyCraftable(showCraftableCardsOnly);
        List<CardFlair> flairTypes = new List<CardFlair>();
        CardFlair item = new CardFlair(TAG_PREMIUM.GOLDEN);
        if (showGolden)
        {
            flairTypes.Add(item);
            cardsSortedCollection.m_filterSet.SetFlairRestriction(item);
        }
        else
        {
            flairTypes.Add(new CardFlair(TAG_PREMIUM.NORMAL));
            cardsSortedCollection.m_filterSet.ClearFlairRestriction();
        }
        cardsSortedCollection.m_filterSet.IncludeCardsNotOwned(flairTypes);
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
    }

    public void ShowMassDisenchant()
    {
        this.m_lastPageNum = this.m_currentPageNum;
        this.m_currentPageNum = MASS_DISENCHANT_PAGE_NUM;
        this.TransitionPageWhenReady(PageTransitionType.MANY_PAGE_RIGHT, false, null, null);
    }

    [DebuggerHidden]
    private IEnumerator ShowMassDisenchantPage()
    {
        return new <ShowMassDisenchantPage>c__Iterator3E { <>f__this = this };
    }

    public void ShowOnlyCardsIOwn()
    {
        this.ShowOnlyCardsIOwn(null, null);
    }

    public void ShowOnlyCardsIOwn(DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_cardsSortedCollection.m_filterSet.SetIncludeOnlyCraftable(false);
        this.m_cardsSortedCollection.m_filterSet.ClearFlairRestriction();
        this.m_cardsSortedCollection.m_filterSet.IncludeCardsNotOwned(new List<CardFlair>());
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
    }

    public void ShowPremiumCardsOnly()
    {
        this.ShowPremiumCardsOnly(null, null);
    }

    public void ShowPremiumCardsOnly(DelOnPageTransitionComplete callback, object callbackData)
    {
        List<CardFlair> flairTypes = new List<CardFlair>();
        CardFlair item = new CardFlair(TAG_PREMIUM.GOLDEN);
        flairTypes.Add(item);
        this.m_cardsSortedCollection.m_filterSet.SetFlairRestriction(item);
        this.m_cardsSortedCollection.m_filterSet.IncludeCardsNotOwned(flairTypes);
        this.UpdateSortedCollectionFromFilterSet();
        this.TransitionPageWhenReady(PageTransitionType.NONE, false, callback, callbackData);
    }

    private void Start()
    {
        this.SetUpClassTabs();
        CollectionPageDisplay alternatePage = this.GetAlternatePage();
        CollectionPageDisplay currentPage = this.GetCurrentPage();
        this.AssembleEmptyPageUI(alternatePage, false);
        this.AssembleEmptyPageUI(currentPage, false);
        this.PositionNextPage(alternatePage);
        this.PositionCurrentPage(currentPage);
        this.m_fullyLoaded = true;
    }

    private void SwapCurrentAndAltPages()
    {
        this.m_currentPageIsPageA = !this.m_currentPageIsPageA;
    }

    private void TransitionPage(object callbackData)
    {
        TransitionReadyCallbackData data = callbackData as TransitionReadyCallbackData;
        CollectionPageDisplay assembledPage = data.m_assembledPage;
        CollectionPageDisplay otherPage = data.m_otherPage;
        if (this.ANIMATE_PAGE_TRANSITIONS != null)
        {
            PageTransitionType transitionType = data.m_transitionType;
            if (TavernBrawlDisplay.IsTavernBrawlViewing())
            {
                transitionType = PageTransitionType.NONE;
            }
            if (this.m_skipNextPageTurn)
            {
                transitionType = PageTransitionType.NONE;
                this.m_skipNextPageTurn = false;
            }
            switch (transitionType)
            {
                case PageTransitionType.NONE:
                    this.PositionNextPage(otherPage);
                    this.PositionCurrentPage(assembledPage);
                    this.OnPageTurnComplete(data);
                    break;

                case PageTransitionType.SINGLE_PAGE_RIGHT:
                    goto Label_00DA;

                case PageTransitionType.SINGLE_PAGE_LEFT:
                    goto Label_009D;

                case PageTransitionType.MANY_PAGE_RIGHT:
                    SoundManager.Get().LoadAndPlay("collection_manager_book_page_flip_forward");
                    goto Label_00DA;

                case PageTransitionType.MANY_PAGE_LEFT:
                    SoundManager.Get().LoadAndPlay("collection_manager_book_page_flip_back");
                    goto Label_009D;
            }
        }
        goto Label_0111;
    Label_009D:
        this.m_pageTurn.TurnLeft(assembledPage.gameObject, otherPage.gameObject, new PageTurn.DelOnPageTurnComplete(this.OnPageTurnComplete), data);
        goto Label_0111;
    Label_00DA:
        this.m_pageTurn.TurnRight(otherPage.gameObject, assembledPage.gameObject, new PageTurn.DelOnPageTurnComplete(this.OnPageTurnComplete), data);
        this.PositionCurrentPage(assembledPage);
        this.PositionNextPage(otherPage);
    Label_0111:
        this.UpdateVisibleTabs();
        if (MASS_DISENCHANT_PAGE_NUM == this.m_currentPageNum)
        {
            this.DeselectCurrentClassTab();
        }
        else
        {
            this.SetCurrentClassTab(assembledPage.GetFirstCardClass());
        }
        if (this.ANIMATE_PAGE_TRANSITIONS == null)
        {
            this.PositionNextPage(otherPage);
            this.PositionCurrentPage(assembledPage);
            this.OnPageTurnComplete(data);
        }
    }

    private void TransitionPageWhenReady(PageTransitionType transitionType, bool useCurrentPageNum, DelOnPageTransitionComplete callback, object callbackData)
    {
        this.m_pagesCurrentlyTurning = true;
        this.SwapCurrentAndAltPages();
        TransitionReadyCallbackData transitionReadyCallbackData = new TransitionReadyCallbackData {
            m_assembledPage = this.GetCurrentPage(),
            m_otherPage = this.GetAlternatePage(),
            m_transitionType = transitionType,
            m_callback = callback,
            m_callbackData = callbackData
        };
        if (CollectionManagerDisplay.Get().GetViewMode() != CollectionManagerDisplay.ViewMode.CARD_BACKS)
        {
            TAG_CLASS tag_class;
            SortedCollection currentSortedCollection = this.GetCurrentSortedCollection();
            List<FilteredArtStack> artStacksToDisplay = null;
            if (useCurrentPageNum)
            {
                artStacksToDisplay = currentSortedCollection.GetPageContents(this.m_currentPageNum, out tag_class);
            }
            else if (this.m_currentPageNum != MASS_DISENCHANT_PAGE_NUM)
            {
                if (this.m_preSearchArtStackAnchor == null)
                {
                    this.m_currentPageNum = 1;
                    artStacksToDisplay = currentSortedCollection.GetPageContents(this.m_currentPageNum, out tag_class);
                }
                else
                {
                    int num;
                    artStacksToDisplay = currentSortedCollection.GetPageContentsForCard(this.m_preSearchArtStackAnchor.CardID, this.m_preSearchArtStackAnchor.Flair, out tag_class, out num);
                    if (artStacksToDisplay.Count == 0)
                    {
                        artStacksToDisplay = currentSortedCollection.GetPageContentsForClass(DefLoader.Get().GetEntityDef(this.m_preSearchArtStackAnchor.CardID).GetClass(), 1, true, out num);
                    }
                    if (artStacksToDisplay.Count == 0)
                    {
                        artStacksToDisplay = currentSortedCollection.GetPageContents(1, out tag_class);
                        num = 1;
                    }
                    this.m_currentPageNum = (artStacksToDisplay.Count != 0) ? num : 0;
                }
            }
            if ((MASS_DISENCHANT_PAGE_NUM != this.m_currentPageNum) && ((artStacksToDisplay == null) || (artStacksToDisplay.Count == 0)))
            {
                TAG_CLASS tag_class2;
                int num2;
                artStacksToDisplay = currentSortedCollection.GetFirstNonEmptyPage(out tag_class2, out num2);
                if (artStacksToDisplay.Count > 0)
                {
                    tag_class = tag_class2;
                    this.m_currentPageNum = num2;
                }
            }
            this.AssembleCardPage(transitionReadyCallbackData, artStacksToDisplay);
        }
        else
        {
            if (this.m_currentPageNum < 1)
            {
                this.m_currentPageNum = 1;
            }
            this.AssembleCardBackPage(transitionReadyCallbackData);
        }
    }

    private void Update()
    {
        bool receiveReleaseWithoutMouseDown = UniversalInputManager.Get().IsTouchMode();
        if (this.m_wasTouchModeEnabled != receiveReleaseWithoutMouseDown)
        {
            this.m_wasTouchModeEnabled = receiveReleaseWithoutMouseDown;
            if (receiveReleaseWithoutMouseDown)
            {
                base.gameObject.AddComponent<CollectionPageManagerTouchBehavior>();
            }
            else
            {
                UnityEngine.Object.Destroy(base.gameObject.GetComponent<CollectionPageManagerTouchBehavior>());
            }
            foreach (CollectionClassTab tab in this.m_allTabs)
            {
                tab.SetReceiveReleaseWithoutMouseDown(receiveReleaseWithoutMouseDown);
            }
        }
    }

    public void UpdateClassTabNewCardCounts()
    {
        foreach (CollectionClassTab tab in this.m_classTabs)
        {
            TAG_CLASS tagClass = tab.GetClass();
            int numNewCardsForClass = this.GetNumNewCardsForClass(tagClass);
            tab.UpdateNewItemCount(numNewCardsForClass);
        }
    }

    public void UpdateCraftingModeButtonDustBottleVisibility()
    {
        bool show = ((CollectionManagerDisplay.Get().GetViewMode() == CollectionManagerDisplay.ViewMode.CARDS) && (CollectionManager.Get().GetCardsToDisenchantCount() > 0)) && (CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing) == null);
        CollectionManagerDisplay.Get().m_craftingModeButton.ShowDustBottle(show);
    }

    public void UpdateCurrentPageCardLocks(bool playSound)
    {
        this.GetCurrentPage().UpdateCurrentPageCardLocks(playSound);
    }

    public void UpdateMassDisenchant()
    {
        if (this.m_massDisenchant == null)
        {
            this.UpdateCraftingModeButtonDustBottleVisibility();
        }
        else
        {
            this.m_massDisenchant.UpdateContents(CollectionManager.Get().GetMassDisenchantArtStacks());
            if (CraftingTray.Get() != null)
            {
                CraftingTray.Get().SetMassDisenchantAmount();
            }
            this.UpdateCraftingModeButtonDustBottleVisibility();
        }
    }

    private void UpdateSortedCollectionFromFilterSet()
    {
        this.m_cardsSortedCollection.UpdateFromResults(false, false);
        this.m_heroSkinsSortedCollection.UpdateFromResults(true, true);
        this.UpdateClassTabNewCardCounts();
    }

    private void UpdateVisibleTabs()
    {
        if (UniversalInputManager.UsePhoneUI == null)
        {
            bool flag = false;
            foreach (CollectionClassTab tab in this.m_allTabs)
            {
                bool flag2 = this.m_tabVisibility[tab];
                bool flag3 = this.ShouldShowTab(tab);
                if (flag2 != flag3)
                {
                    flag = true;
                    this.m_tabVisibility[tab] = flag3;
                }
            }
            if (flag)
            {
                this.PositionClassTabs(true);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenCompletePageLeft(object callbackData)
    {
        return new <WaitThenCompletePageLeft>c__Iterator3F { callbackData = callbackData, <$>callbackData = callbackData, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenShowArrows()
    {
        return new <WaitThenShowArrows>c__Iterator40 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <AnimateTabs>c__Iterator3C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<CollectionClassTab>.Enumerator <$s_404>__3;
        internal List<CollectionClassTab>.Enumerator <$s_405>__5;
        internal List<CollectionClassTab>.Enumerator <$s_406>__7;
        internal List<CollectionClassTab>.Enumerator <$s_407>__9;
        internal CollectionPageManager <>f__this;
        internal CollectionClassTab <tab>__10;
        internal CollectionClassTab <tab>__4;
        internal CollectionClassTab <tab>__6;
        internal CollectionClassTab <tab>__8;
        internal List<CollectionClassTab> <tabsToHide>__0;
        internal List<CollectionClassTab> <tabsToMove>__2;
        internal List<CollectionClassTab> <tabsToShow>__1;

        [DebuggerHidden]
        public void Dispose()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 1:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_405>__5.Dispose();
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
                    this.<tabsToHide>__0 = new List<CollectionClassTab>();
                    this.<tabsToShow>__1 = new List<CollectionClassTab>();
                    this.<tabsToMove>__2 = new List<CollectionClassTab>();
                    this.<$s_404>__3 = this.<>f__this.m_classTabs.GetEnumerator();
                    try
                    {
                        while (this.<$s_404>__3.MoveNext())
                        {
                            this.<tab>__4 = this.<$s_404>__3.Current;
                            if (this.<tab>__4.IsVisible() || this.<tab>__4.ShouldBeVisible())
                            {
                                if (this.<tab>__4.IsVisible() && this.<tab>__4.ShouldBeVisible())
                                {
                                    this.<tabsToMove>__2.Add(this.<tab>__4);
                                }
                                else if (this.<tab>__4.IsVisible() && !this.<tab>__4.ShouldBeVisible())
                                {
                                    this.<tabsToHide>__0.Add(this.<tab>__4);
                                }
                                else
                                {
                                    this.<tabsToShow>__1.Add(this.<tab>__4);
                                }
                                this.<tab>__4.SetIsVisible(this.<tab>__4.ShouldBeVisible());
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_404>__3.Dispose();
                    }
                    this.<>f__this.m_tabsAreAnimating = true;
                    if (this.<tabsToHide>__0.Count <= 0)
                    {
                        goto Label_023C;
                    }
                    this.<$s_405>__5 = this.<tabsToHide>__0.GetEnumerator();
                    num = 0xfffffffd;
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_023C;

                case 3:
                    goto Label_02F1;

                case 4:
                    goto Label_0397;

                default:
                    goto Label_03AA;
            }
            try
            {
                switch (num)
                {
                    case 1:
                        this.<tab>__6.AnimateToTargetPosition(0.1f, iTween.EaseType.easeOutQuad);
                        break;
                }
                while (this.<$s_405>__5.MoveNext())
                {
                    this.<tab>__6 = this.<$s_405>__5.Current;
                    SoundManager.Get().LoadAndPlay("class_tab_retract", this.<tab>__6.gameObject);
                    this.$current = new WaitForSeconds(0.03f);
                    this.$PC = 1;
                    flag = true;
                    goto Label_03AC;
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_405>__5.Dispose();
            }
            this.$current = new WaitForSeconds(0.1f);
            this.$PC = 2;
            goto Label_03AC;
        Label_023C:
            if (this.<tabsToMove>__2.Count > 0)
            {
                this.<$s_406>__7 = this.<tabsToMove>__2.GetEnumerator();
                try
                {
                    while (this.<$s_406>__7.MoveNext())
                    {
                        this.<tab>__8 = this.<$s_406>__7.Current;
                        if (this.<tab>__8.WillSlide())
                        {
                            SoundManager.Get().LoadAndPlay("class_tab_slides_across_top", this.<tab>__8.gameObject);
                        }
                        this.<tab>__8.AnimateToTargetPosition(0.25f, iTween.EaseType.easeOutQuad);
                    }
                }
                finally
                {
                    this.<$s_406>__7.Dispose();
                }
                this.$current = new WaitForSeconds(0.25f);
                this.$PC = 3;
                goto Label_03AC;
            }
        Label_02F1:
            if (this.<tabsToShow>__1.Count > 0)
            {
                this.<$s_407>__9 = this.<tabsToShow>__1.GetEnumerator();
                try
                {
                    while (this.<$s_407>__9.MoveNext())
                    {
                        this.<tab>__10 = this.<$s_407>__9.Current;
                        SoundManager.Get().LoadAndPlay("class_tab_retract", this.<tab>__10.gameObject);
                        this.<tab>__10.AnimateToTargetPosition(0.4f, iTween.EaseType.easeOutBounce);
                    }
                }
                finally
                {
                    this.<$s_407>__9.Dispose();
                }
                this.$current = new WaitForSeconds(0.4f);
                this.$PC = 4;
                goto Label_03AC;
            }
        Label_0397:
            this.<>f__this.m_tabsAreAnimating = false;
            this.$PC = -1;
        Label_03AA:
            return false;
        Label_03AC:
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
    private sealed class <AssembleBasePage>c__AnonStorey2DB
    {
        internal CollectionPageManager <>f__this;
        internal CollectionPageDisplay page;
        internal CollectionPageManager.TransitionReadyCallbackData transitionReadyCallbackData;

        internal void <>m__B2(List<Actor> actorList, object data)
        {
            this.page.UpdateCollectionCards(actorList, CollectionManagerDisplay.Get().GetViewMode(), this.<>f__this.IsShowingMassDisenchant());
            this.<>f__this.TransitionPage(this.transitionReadyCallbackData);
        }
    }

    [CompilerGenerated]
    private sealed class <AssembleCardBackPage>c__AnonStorey2DD
    {
        internal CollectionPageManager <>f__this;
        internal CollectionPageDisplay page;
        internal CollectionPageManager.TransitionReadyCallbackData transitionReadyCallbackData;

        internal void <>m__B4(List<Actor> actorList, object data)
        {
            this.page.UpdateCollectionCards(actorList, CollectionManagerDisplay.Get().GetViewMode(), this.<>f__this.IsShowingMassDisenchant());
            foreach (Actor actor in actorList)
            {
                CardBackManager.Get().UpdateCardBackWithInternalCardBack(actor);
            }
            this.<>f__this.TransitionPage(this.transitionReadyCallbackData);
        }
    }

    [CompilerGenerated]
    private sealed class <AssembleCardPage>c__AnonStorey2DC
    {
        internal CollectionPageManager <>f__this;
        internal CollectionPageDisplay page;
        internal CollectionPageManager.TransitionReadyCallbackData transitionReadyCallbackData;

        internal void <>m__B3(List<Actor> actorList, object data)
        {
            this.page.UpdateCollectionCards(actorList, CollectionManagerDisplay.Get().GetViewMode(), this.<>f__this.IsShowingMassDisenchant());
            this.<>f__this.TransitionPage(this.transitionReadyCallbackData);
        }
    }

    [CompilerGenerated]
    private sealed class <Awake>c__AnonStorey2D9
    {
        internal EntityDef lhsDef;
        internal EntityDef rhsDef;

        internal bool <>m__B5(TAG_CLASS c)
        {
            return (c == this.lhsDef.GetClass());
        }

        internal bool <>m__B6(TAG_CLASS c)
        {
            return (c == this.rhsDef.GetClass());
        }
    }

    [CompilerGenerated]
    private sealed class <SelectTabWhenReady>c__Iterator3D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionClassTab <$>tab;
        internal CollectionPageManager <>f__this;
        internal CollectionClassTab tab;

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
                    if (this.<>f__this.m_tabsAreAnimating)
                    {
                        this.$current = 0;
                        this.$PC = 1;
                        return true;
                    }
                    if (this.<>f__this.m_currentClassTab == this.tab)
                    {
                        this.tab.SetSelected(true);
                        this.tab.SetLargeTab(true);
                        this.$PC = -1;
                    }
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
    private sealed class <SetCurrentClassTab>c__AnonStorey2DA
    {
        internal TAG_CLASS? tabClass;

        internal bool <>m__B1(CollectionClassTab obj)
        {
            return (obj.GetClass() == ((TAG_CLASS) this.tabClass.Value));
        }
    }

    [CompilerGenerated]
    private sealed class <ShowMassDisenchantPage>c__Iterator3E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionPageManager <>f__this;

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
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.<>f__this.m_massDisenchant != null)
                    {
                        this.<>f__this.m_massDisenchant.Show();
                        break;
                    }
                    CollectionManagerDisplay.Get().LoadCraftingManager(new AssetLoader.GameObjectCallback(this.<>f__this.OnLoadedMassDisenchant));
                    break;

                default:
                    goto Label_0085;
            }
            this.$PC = -1;
        Label_0085:
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
    private sealed class <WaitThenCompletePageLeft>c__Iterator3F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal object <$>callbackData;
        internal CollectionPageManager <>f__this;
        internal float <flipTimePercent>__0;
        internal float <waitTime>__1;
        internal object callbackData;

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
                    this.<flipTimePercent>__0 = Mathf.Clamp(this.<>f__this.m_turnLeftPageSwapTiming, 0f, 1f);
                    this.<waitTime>__1 = this.<>f__this.m_pageTurn.GetLeftTurnAnimTime() * this.<flipTimePercent>__0;
                    this.$current = new WaitForSeconds(this.<waitTime>__1);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.OnPageTurnComplete(this.callbackData);
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
    private sealed class <WaitThenShowArrows>c__Iterator40 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionPageManager <>f__this;
        internal bool <showLeftArrow>__0;
        internal bool <showRightArrow>__1;

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
                    if ((this.<>f__this.m_pageLeftArrow != null) || (this.<>f__this.m_pageRightArrow != null))
                    {
                        this.<>f__this.m_delayShowingArrows = true;
                        this.$current = new WaitForSeconds(1f);
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    this.<>f__this.m_delayShowingArrows = false;
                    this.<showLeftArrow>__0 = this.<>f__this.m_pageLeftClickableRegion.enabled;
                    this.<>f__this.ShowArrow(this.<>f__this.m_pageLeftArrow, this.<showLeftArrow>__0, false);
                    this.<showRightArrow>__1 = this.<>f__this.m_pageRightClickableRegion.enabled;
                    this.<>f__this.ShowArrow(this.<>f__this.m_pageRightArrow, this.<showRightArrow>__1, true);
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

    private enum ArrowClickType
    {
        DISABLED,
        ENABLED,
        SWITCH_MODE
    }

    public delegate void DelOnPageTransitionComplete(object callbackData);

    private enum PageTransitionType
    {
        NONE,
        SINGLE_PAGE_RIGHT,
        SINGLE_PAGE_LEFT,
        MANY_PAGE_RIGHT,
        MANY_PAGE_LEFT
    }

    private class SortedCollection
    {
        private List<FilteredArtStack> m_allResults = new List<FilteredArtStack>();
        private List<FilteredArtStack>[] m_currentResultsByClass = new List<FilteredArtStack>[CollectionPageManager.CLASS_TAB_ORDER.Length];
        public CollectionFilterSet m_filterSet = new CollectionFilterSet(true);
        private bool m_showAll;
        private Comparison<FilteredArtStack> m_sortDelegate;

        public SortedCollection()
        {
            for (int i = 0; i < this.m_currentResultsByClass.Length; i++)
            {
                this.m_currentResultsByClass[i] = new List<FilteredArtStack>();
            }
        }

        public List<FilteredArtStack> GetFirstNonEmptyPage(out TAG_CLASS pageClass, out int collectionPage)
        {
            collectionPage = 0;
            pageClass = TAG_CLASS.INVALID;
            List<FilteredArtStack> list = new List<FilteredArtStack>();
            for (int i = 0; i < CollectionPageManager.CLASS_TAB_ORDER.Length; i++)
            {
                pageClass = CollectionPageManager.CLASS_TAB_ORDER[i];
                if (this.GetNumPagesForClass(CollectionPageManager.CLASS_TAB_ORDER[i]) != 0)
                {
                    return this.GetPageContentsForClass(pageClass, 1, true, out collectionPage);
                }
            }
            return list;
        }

        public int GetNumNewCardsForClass(TAG_CLASS cardClass)
        {
            int num = 0;
            int index = CollectionPageManager.CLASS_TO_TAB_IDX[cardClass];
            List<FilteredArtStack> list = this.m_currentResultsByClass[index];
            CollectionManagerDisplay display = CollectionManagerDisplay.Get();
            foreach (FilteredArtStack stack in list)
            {
                if (display.ShouldShowNewCardGlow(stack.CardID, stack.Flair))
                {
                    num++;
                }
            }
            return num;
        }

        public int GetNumPagesForClass(TAG_CLASS cardClass)
        {
            int maxNumCards = CollectionPageDisplay.GetMaxNumCards();
            int count = this.m_currentResultsByClass[CollectionPageManager.CLASS_TO_TAB_IDX[cardClass]].Count;
            return ((count / maxNumCards) + (((count % maxNumCards) <= 0) ? 0 : 1));
        }

        public List<FilteredArtStack> GetPageContents(int page, out TAG_CLASS pageClass)
        {
            pageClass = TAG_CLASS.INVALID;
            if (this.m_showAll)
            {
                return this.GetPageContentsFromPage_AllResults(page);
            }
            List<FilteredArtStack> list = new List<FilteredArtStack>();
            if ((page >= 0) && (page <= this.GetTotalNumPages()))
            {
                int num = 0;
                for (int i = 0; i < CollectionPageManager.CLASS_TAB_ORDER.Length; i++)
                {
                    int num3 = num;
                    TAG_CLASS cardClass = CollectionPageManager.CLASS_TAB_ORDER[i];
                    num += this.GetNumPagesForClass(cardClass);
                    if (page <= num)
                    {
                        int num5;
                        pageClass = cardClass;
                        int pageWithinClass = page - num3;
                        return this.GetPageContentsForClass(cardClass, pageWithinClass, false, out num5);
                    }
                }
            }
            return list;
        }

        public List<FilteredArtStack> GetPageContentsForCard(string cardID, CardFlair flair, out TAG_CLASS pageClass, out int collectionPage)
        {
            <GetPageContentsForCard>c__AnonStorey2DE storeyde = new <GetPageContentsForCard>c__AnonStorey2DE {
                cardID = cardID,
                flair = flair
            };
            pageClass = DefLoader.Get().GetEntityDef(storeyde.cardID).GetClass();
            collectionPage = 0;
            if (this.m_showAll)
            {
                int cardIdx = this.m_allResults.FindIndex(new Predicate<FilteredArtStack>(storeyde.<>m__B7));
                return this.GetPageContentsFromCard_AllResults(cardIdx, out collectionPage);
            }
            int index = CollectionPageManager.CLASS_TO_TAB_IDX[pageClass];
            int num3 = this.m_currentResultsByClass[index].FindIndex(new Predicate<FilteredArtStack>(storeyde.<>m__B8));
            if (num3 < 0)
            {
                return new List<FilteredArtStack>();
            }
            int maxNumCards = CollectionPageDisplay.GetMaxNumCards();
            int num5 = num3 + 1;
            int pageWithinClass = (num5 / maxNumCards) + (((num5 % maxNumCards) <= 0) ? 0 : 1);
            return this.GetPageContentsForClass(pageClass, pageWithinClass, true, out collectionPage);
        }

        public List<FilteredArtStack> GetPageContentsForClass(TAG_CLASS pageClass, int pageWithinClass, bool calculateCollectionPage, out int collectionPage)
        {
            List<FilteredArtStack> range = new List<FilteredArtStack>();
            collectionPage = 0;
            if ((pageWithinClass > 0) && (pageWithinClass <= this.GetNumPagesForClass(pageClass)))
            {
                int maxNumCards = CollectionPageDisplay.GetMaxNumCards();
                int index = CollectionPageManager.CLASS_TO_TAB_IDX[pageClass];
                int num3 = (pageWithinClass - 1) * maxNumCards;
                int count = Math.Min(this.m_currentResultsByClass[index].Count - num3, maxNumCards);
                range = this.m_currentResultsByClass[index].GetRange(num3, count);
                if (!calculateCollectionPage)
                {
                    return range;
                }
                for (int i = 0; i < CollectionPageManager.CLASS_TAB_ORDER.Length; i++)
                {
                    TAG_CLASS cardClass = CollectionPageManager.CLASS_TAB_ORDER[i];
                    if (cardClass == pageClass)
                    {
                        break;
                    }
                    collectionPage += this.GetNumPagesForClass(cardClass);
                }
                collectionPage += pageWithinClass;
            }
            return range;
        }

        private List<FilteredArtStack> GetPageContentsFromCard_AllResults(int cardIdx, out int page)
        {
            int maxNumCards = CollectionPageDisplay.GetMaxNumCards();
            page = cardIdx / maxNumCards;
            cardIdx = page * maxNumCards;
            page++;
            return this.m_allResults.GetRange(cardIdx, Math.Min(CollectionPageDisplay.GetMaxNumCards(), this.m_allResults.Count - cardIdx));
        }

        private List<FilteredArtStack> GetPageContentsFromPage_AllResults(int page)
        {
            int cardIdx = (page - 1) * CollectionPageDisplay.GetMaxNumCards();
            return this.GetPageContentsFromCard_AllResults(cardIdx, out page);
        }

        public int GetTotalNumPages()
        {
            if (this.m_showAll)
            {
                int count = this.m_allResults.Count;
                int maxNumCards = CollectionPageDisplay.GetMaxNumCards();
                return ((count / maxNumCards) + (((count % maxNumCards) <= 0) ? 0 : 1));
            }
            int num3 = 0;
            foreach (TAG_CLASS tag_class in CollectionPageManager.CLASS_TAB_ORDER)
            {
                num3 += this.GetNumPagesForClass(tag_class);
            }
            return num3;
        }

        public void SetSortDelegate(Comparison<FilteredArtStack> sortDelegate)
        {
            this.m_sortDelegate = sortDelegate;
        }

        public void UpdateFromResults(bool showAll, bool preferGolden)
        {
            this.m_showAll = showAll;
            for (int i = 0; i < this.m_currentResultsByClass.Length; i++)
            {
                this.m_currentResultsByClass[i].Clear();
            }
            this.m_allResults.Clear();
            List<FilteredArtStack> list = this.m_filterSet.GenerateList();
            if (this.m_sortDelegate != null)
            {
                list.Sort(this.m_sortDelegate);
            }
            else
            {
                list.Sort();
            }
            for (int j = 0; j < list.Count; j++)
            {
                FilteredArtStack item = list[j];
                EntityDef entityDef = DefLoader.Get().GetEntityDef(item.CardID);
                if (entityDef == null)
                {
                    Log.Rachelle.Print("SortedCollection.UpdateFromResults(): null entity def!!!", new object[0]);
                }
                else
                {
                    if (preferGolden)
                    {
                        CardFlair bestHeroFlairOwned = CollectionManager.Get().GetBestHeroFlairOwned(item.CardID);
                        if ((((bestHeroFlairOwned != null) && (bestHeroFlairOwned.Premium == TAG_PREMIUM.GOLDEN)) && (item.Flair.Premium == TAG_PREMIUM.NORMAL)) || ((item.Count == 0) && (item.Flair.Premium == TAG_PREMIUM.GOLDEN)))
                        {
                            goto Label_016E;
                        }
                    }
                    TAG_CLASS key = entityDef.GetClass();
                    if (!CollectionPageManager.CLASS_TO_TAB_IDX.ContainsKey(key))
                    {
                        Log.Rachelle.Print(string.Format("CLASS_TO_TAB_IDX does not contain key {0}", key), new object[0]);
                    }
                    int index = CollectionPageManager.CLASS_TO_TAB_IDX[key];
                    this.m_currentResultsByClass[index].Add(item);
                    this.m_allResults.Add(item);
                Label_016E:;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <GetPageContentsForCard>c__AnonStorey2DE
        {
            internal string cardID;
            internal CardFlair flair;

            internal bool <>m__B7(FilteredArtStack obj)
            {
                return (obj.CardID.Equals(this.cardID) && obj.Flair.Equals(this.flair));
            }

            internal bool <>m__B8(FilteredArtStack obj)
            {
                return (obj.CardID.Equals(this.cardID) && obj.Flair.Equals(this.flair));
            }
        }
    }

    private class TransitionReadyCallbackData
    {
        public CollectionPageDisplay m_assembledPage;
        public CollectionPageManager.DelOnPageTransitionComplete m_callback;
        public object m_callbackData;
        public CollectionPageDisplay m_otherPage;
        public CollectionPageManager.PageTransitionType m_transitionType;
    }
}

