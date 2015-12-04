using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class CollectionDeckTray : MonoBehaviour
{
    public GameObject m_backArrow;
    public DeckTrayCardBackContent m_cardBackContent;
    public DeckTrayCardListContent m_cardsContent;
    private Map<DeckContentTypes, DeckTrayContent> m_contents = new Map<DeckContentTypes, DeckTrayContent>();
    private DeckContentTypes m_contentToSet = DeckContentTypes.INVALID;
    public UberText m_countLabelText;
    public UberText m_countText;
    private DeckContentTypes m_currentContent = DeckContentTypes.INVALID;
    public DeckBigCard m_deckBigCard;
    public TooltipZone m_deckHeaderTooltip;
    public DeckTrayDeckListContent m_decksContent;
    public UIBButton m_doneButton;
    public DeckTrayHeroSkinContent m_heroSkinContent;
    public GameObject m_inputBlocker;
    private List<ModeSwitched> m_modeSwitchedListeners = new List<ModeSwitched>();
    public UberText m_myDecksLabel;
    public List<DeckContentScroll> m_scrollables = new List<DeckContentScroll>();
    public UIBScrollable m_scrollbar;
    private bool m_settingNewMode;
    public GameObject m_topCardPositionBone;
    private static CollectionDeckTray s_instance;

    public bool AddCard(EntityDef cardEntityDef, CardFlair cardFlair, DeckTrayDeckTileVisual deckTileToRemove, bool playSound, Actor animateActor = null)
    {
        return this.GetCardsContent().AddCard(cardEntityDef, cardFlair, deckTileToRemove, playSound, animateActor);
    }

    public void AllowInput(bool allowed)
    {
        this.m_inputBlocker.SetActive(!allowed);
    }

    [DebuggerHidden]
    private IEnumerator AutoAddCardsWithTiming(List<RDMDeckEntry> newCards)
    {
        return new <AutoAddCardsWithTiming>c__Iterator31 { newCards = newCards, <$>newCards = newCards, <>f__this = this };
    }

    private void Awake()
    {
        s_instance = this;
        if (base.gameObject.GetComponent<AudioSource>() == null)
        {
            base.gameObject.AddComponent<AudioSource>();
        }
        if (this.m_scrollbar != null)
        {
            if ((SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL) && (UniversalInputManager.UsePhoneUI == null))
            {
                Vector3 center = this.m_scrollbar.m_ScrollBounds.center;
                center.z = 3f;
                this.m_scrollbar.m_ScrollBounds.center = center;
                Vector3 size = this.m_scrollbar.m_ScrollBounds.size;
                size.z = 47.67f;
                this.m_scrollbar.m_ScrollBounds.size = size;
                if ((this.m_cardsContent != null) && (this.m_cardsContent.m_deckCompleteHighlight != null))
                {
                    Vector3 localPosition = this.m_cardsContent.m_deckCompleteHighlight.transform.localPosition;
                    localPosition.z = -34.15f;
                    this.m_cardsContent.m_deckCompleteHighlight.transform.localPosition = localPosition;
                }
            }
            this.m_scrollbar.Enable(false);
            this.m_scrollbar.AddTouchScrollStartedListener(new UIBScrollable.OnTouchScrollStarted(this.OnTouchScrollStarted));
            this.m_scrollbar.AddTouchScrollEndedListener(new UIBScrollable.OnTouchScrollEnded(this.OnTouchScrollEnded));
        }
        this.m_contents[DeckContentTypes.Decks] = this.m_decksContent;
        this.m_contents[DeckContentTypes.Cards] = this.m_cardsContent;
        if (this.m_heroSkinContent != null)
        {
            this.m_contents[DeckContentTypes.HeroSkin] = this.m_heroSkinContent;
            this.m_heroSkinContent.RegisterHeroAssignedListener(new DeckTrayHeroSkinContent.HeroAssigned(this.OnHeroAssigned));
        }
        if (this.m_cardBackContent != null)
        {
            this.m_contents[DeckContentTypes.CardBack] = this.m_cardBackContent;
        }
        this.m_cardsContent.RegisterCardTileHeldListener(new DeckTrayCardListContent.CardTileHeld(this.OnCardTileHeld));
        this.m_cardsContent.RegisterCardTilePressListener(new DeckTrayCardListContent.CardTilePress(this.OnCardTilePress));
        this.m_cardsContent.RegisterCardTileOverListener(new DeckTrayCardListContent.CardTileOver(this.OnCardTileOver));
        this.m_cardsContent.RegisterCardTileOutListener(new DeckTrayCardListContent.CardTileOut(this.OnCardTileOut));
        this.m_cardsContent.RegisterCardTileReleaseListener(new DeckTrayCardListContent.CardTileRelease(this.OnCardTileRelease));
        this.m_cardsContent.RegisterCardCountUpdated(new DeckTrayCardListContent.CardCountChanged(this.OnCardCountUpdated));
        this.m_decksContent.RegisterDeckCountUpdated(new DeckTrayDeckListContent.DeckCountChanged(this.OnDeckCountUpdated));
        this.m_decksContent.RegisterBusyWithDeck(new DeckTrayDeckListContent.BusyWithDeck(this.OnBusyWithDeck));
        this.SetMyDecksLabelText(GameStrings.Get((SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL) ? "GLUE_COLLECTION_MY_DECKS" : "GLUE_COLLECTION_DECK"));
        this.m_doneButton.SetText(GameStrings.Get("GLOBAL_BACK"));
        this.m_doneButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.DoneButtonPress));
        CollectionManager.Get().RegisterTaggedDeckChanged(new CollectionManager.OnTaggedDeckChanged(this.OnTaggedDeckChanged));
        CollectionInputMgr.Get().SetScrollbar(this.m_scrollbar);
        CollectionManagerDisplay.Get().UpdateCurrentPageCardLocks(true);
        CollectionManagerDisplay.Get().RegisterSwitchViewModeListener(new CollectionManagerDisplay.OnSwitchViewMode(this.OnCMViewModeChanged));
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnBackOutOfCollectionScreen));
        foreach (DeckContentScroll scroll in this.m_scrollables)
        {
            scroll.SaveStartPosition();
        }
    }

    public void CancelRenamingDeck()
    {
        this.m_decksContent.CancelRenameEditingDeck();
    }

    public bool CanPickupCard()
    {
        DeckContentTypes currentContentType = this.GetCurrentContentType();
        CollectionManagerDisplay.ViewMode viewMode = CollectionManagerDisplay.Get().GetViewMode();
        return ((((currentContentType == DeckContentTypes.Cards) && (viewMode == CollectionManagerDisplay.ViewMode.CARDS)) || ((currentContentType == DeckContentTypes.CardBack) && (viewMode == CollectionManagerDisplay.ViewMode.CARD_BACKS))) || ((currentContentType == DeckContentTypes.HeroSkin) && (viewMode == CollectionManagerDisplay.ViewMode.HERO_SKINS)));
    }

    public void ClearCountLabels()
    {
        this.m_countLabelText.Text = string.Empty;
        this.m_countText.Text = string.Empty;
    }

    public void DeleteEditingDeck(bool popNavigation = true)
    {
        if (popNavigation)
        {
            Navigation.Pop();
        }
        this.m_decksContent.DeleteEditingDeck();
        this.SetTrayMode(DeckContentTypes.Decks);
    }

    [DebuggerHidden]
    private IEnumerator DestroyAfterSeconds(GameObject go)
    {
        return new <DestroyAfterSeconds>c__Iterator33 { go = go, <$>go = go };
    }

    private void DoneButtonPress(UIEvent e)
    {
        Navigation.GoBack();
    }

    public void EnterEditDeckModeForTavernBrawl()
    {
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnBackOutOfDeckContents));
        this.UpdateDoneButtonText(this.m_currentContent, DeckContentTypes.Cards);
    }

    public void ExitEditDeckModeForTavernBrawl()
    {
        this.UpdateDoneButtonText(this.m_currentContent, DeckContentTypes.Cards);
    }

    private void FinishMyDeckPress()
    {
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_AUTO_COMPLETE_DECK_CLICKED);
        RDM_Deck currentDeck = RandomDeckMaker.ConvertCollectionDeckToRDMDeck(CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing));
        int count = currentDeck.deckList.Count;
        List<RDMDeckEntry> deckList = RandomDeckMaker.FinishMyDeck(currentDeck).deckList;
        List<RDMDeckEntry> range = deckList.GetRange(count, deckList.Count - count);
        base.StartCoroutine(this.AutoAddCardsWithTiming(range));
    }

    private void FireModeSwitchedEvent()
    {
        ModeSwitched[] switchedArray = this.m_modeSwitchedListeners.ToArray();
        for (int i = 0; i < switchedArray.Length; i++)
        {
            switchedArray[i]();
        }
    }

    public static CollectionDeckTray Get()
    {
        return s_instance;
    }

    public DeckTrayCardBackContent GetCardBackContent()
    {
        return this.m_cardBackContent;
    }

    public DeckTrayCardListContent GetCardsContent()
    {
        return this.m_cardsContent;
    }

    public DeckTrayDeckTileVisual GetCardTileVisual(string cardID)
    {
        return this.m_cardsContent.GetCardTileVisual(cardID);
    }

    public DeckTrayDeckTileVisual GetCardTileVisual(string cardID, TAG_PREMIUM premType)
    {
        return this.m_cardsContent.GetCardTileVisual(cardID, premType);
    }

    private DeckContentTypes GetContentTypeFromViewMode(CollectionManagerDisplay.ViewMode viewMode)
    {
        CollectionManagerDisplay.ViewMode mode = viewMode;
        if (mode == CollectionManagerDisplay.ViewMode.HERO_SKINS)
        {
            return DeckContentTypes.HeroSkin;
        }
        if (mode != CollectionManagerDisplay.ViewMode.CARD_BACKS)
        {
            return DeckContentTypes.Cards;
        }
        return DeckContentTypes.CardBack;
    }

    public DeckTrayContent GetCurrentContent()
    {
        return this.m_contents[this.m_currentContent];
    }

    public DeckContentTypes GetCurrentContentType()
    {
        return this.m_currentContent;
    }

    public DeckBigCard GetDeckBigCard()
    {
        return this.m_deckBigCard;
    }

    public DeckTrayDeckListContent GetDecksContent()
    {
        return this.m_decksContent;
    }

    public DeckTrayHeroSkinContent GetHeroSkinContent()
    {
        return this.m_heroSkinContent;
    }

    public DeckTrayDeckTileVisual GetOrAddCardTileVisual(int index, bool affectedByScrollbar = true)
    {
        DeckTrayDeckTileVisual orAddCardTileVisual = this.m_cardsContent.GetOrAddCardTileVisual(index);
        if (orAddCardTileVisual == null)
        {
            orAddCardTileVisual = this.m_cardsContent.GetOrAddCardTileVisual(index);
            if (affectedByScrollbar)
            {
                this.m_scrollbar.AddVisibleAffectedObject(orAddCardTileVisual.gameObject, this.m_cardsContent.GetCardVisualExtents(), true, new UIBScrollable.VisibleAffected(CollectionDeckTray.OnDeckTrayTileScrollVisibleAffected));
            }
        }
        return orAddCardTileVisual;
    }

    public TooltipZone GetTooltipZone()
    {
        return this.m_deckHeaderTooltip;
    }

    public bool HandleDeletedCardDeckUpdate(string cardID, CardFlair flair)
    {
        if (!this.IsShowingDeckContents())
        {
            return false;
        }
        this.GetCardsContent().UpdateCardList(cardID, true, null);
        CollectionManagerDisplay.Get().UpdateCurrentPageCardLocks(true);
        return true;
    }

    private void HideDeckBigCard(DeckTrayDeckTileVisual cardTile, bool force = false)
    {
        CollectionDeckTileActor actor = cardTile.GetActor();
        if (this.m_deckBigCard != null)
        {
            if (force)
            {
                this.m_deckBigCard.ForceHide();
            }
            else
            {
                this.m_deckBigCard.Hide(actor.GetEntityDef(), actor.GetCardFlair());
            }
            if (UniversalInputManager.Get().IsTouchMode())
            {
                cardTile.SetHighlight(false);
            }
        }
    }

    public bool IsShowingDeckContents()
    {
        return (this.GetCurrentContentType() != DeckContentTypes.Decks);
    }

    public bool IsWaitingToDeleteDeck()
    {
        return this.m_decksContent.IsWaitingToDeleteDeck();
    }

    public bool MouseIsOver()
    {
        return UniversalInputManager.Get().InputIsOver(base.gameObject);
    }

    private bool OnBackOutOfCollectionScreen()
    {
        if ((this.GetCurrentContentType() != DeckContentTypes.INVALID) && !this.GetCurrentContent().IsModeActive())
        {
            return false;
        }
        if (this.IsShowingDeckContents() && (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL))
        {
            return false;
        }
        AnimationUtil.DelayedActivate(base.gameObject, 0.25f, false);
        CollectionManagerDisplay.Get().Exit();
        return true;
    }

    private bool OnBackOutOfDeckContents()
    {
        if ((this.GetCurrentContentType() != DeckContentTypes.INVALID) && !this.GetCurrentContent().IsModeActive())
        {
            return false;
        }
        if (!this.IsShowingDeckContents())
        {
            return false;
        }
        DeckHelper.Get().Hide();
        CollectionDeckViolationDeckSize collectionDeckViolationDeckOverflow = CollectionDeckValidator.GetCollectionDeckViolationDeckOverflow(CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing));
        if (collectionDeckViolationDeckOverflow != null)
        {
            int numMissingCards = 30 - collectionDeckViolationDeckOverflow.TotalOwnedCardCount;
            this.PopupMissingCardsConfirmation(numMissingCards);
        }
        else
        {
            this.SaveCurrentDeckAndEnterDeckListMode();
        }
        return true;
    }

    private void OnBusyWithDeck(bool busy)
    {
        this.m_inputBlocker.SetActive(busy);
    }

    private void OnCardCountUpdated(int cardCount)
    {
        string str = GameStrings.Get("GLUE_DECK_TRAY_CARD_COUNT_LABEL");
        object[] args = new object[] { cardCount, 30 };
        string str2 = GameStrings.Format("GLUE_DECK_TRAY_COUNT", args);
        this.m_countLabelText.Text = str;
        this.m_countText.Text = str2;
    }

    private void OnCardTileHeld(DeckTrayDeckTileVisual cardTile)
    {
        if (((CollectionInputMgr.Get() != null) && !TavernBrawlDisplay.IsTavernBrawlViewing()) && (CollectionInputMgr.Get().GrabCard(cardTile) && (this.m_deckBigCard != null)))
        {
            this.HideDeckBigCard(cardTile, true);
        }
    }

    private void OnCardTileOut(DeckTrayDeckTileVisual cardTile)
    {
        this.HideDeckBigCard(cardTile, false);
    }

    private void OnCardTileOver(DeckTrayDeckTileVisual cardTile)
    {
        if (!UniversalInputManager.Get().IsTouchMode() && ((CollectionInputMgr.Get() == null) || !CollectionInputMgr.Get().HasHeldCard()))
        {
            this.ShowDeckBigCard(cardTile, 0f);
        }
    }

    private void OnCardTilePress(DeckTrayDeckTileVisual cardTile)
    {
        if (UniversalInputManager.Get().IsTouchMode())
        {
            this.ShowDeckBigCard(cardTile, 0.2f);
        }
        else if (CollectionInputMgr.Get() != null)
        {
            this.HideDeckBigCard(cardTile, false);
        }
    }

    private void OnCardTileRelease(DeckTrayDeckTileVisual cardTile)
    {
        if (!cardTile.IsOwnedSlot())
        {
            this.m_cardsContent.ShowDeckHelper();
        }
        else if (UniversalInputManager.Get().IsTouchMode())
        {
            this.HideDeckBigCard(cardTile, false);
        }
        else if ((CollectionInputMgr.Get() != null) && !TavernBrawlDisplay.IsTavernBrawlViewing())
        {
            CollectionDeckTileActor actor = cardTile.GetActor();
            GameObject go = UnityEngine.Object.Instantiate<GameObject>(actor.GetSpell(SpellType.SUMMON_IN).gameObject);
            go.transform.position = actor.transform.position + new Vector3(-2f, 0f, 0f);
            go.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            go.GetComponent<Spell>().ActivateState(SpellStateType.BIRTH);
            base.StartCoroutine(this.DestroyAfterSeconds(go));
            if (Get() != null)
            {
                Get().RemoveCard(cardTile.GetCardID(), cardTile.GetCardFlair(), cardTile.IsOwnedSlot());
            }
            iTween.MoveTo(go, new Vector3(go.transform.position.x - 10f, go.transform.position.y + 10f, go.transform.position.z), 4f);
            SoundManager.Get().LoadAndPlay("collection_manager_card_remove_from_deck_instant", base.gameObject);
        }
    }

    private void OnCMViewModeChanged(CollectionManagerDisplay.ViewMode prevMode, CollectionManagerDisplay.ViewMode mode, object userdata)
    {
        if (this.m_currentContent != DeckContentTypes.Decks)
        {
            DeckContentTypes contentTypeFromViewMode = this.GetContentTypeFromViewMode(mode);
            this.SetTrayMode(contentTypeFromViewMode);
        }
    }

    private void OnDeckCountUpdated(int deckCount)
    {
        string str = GameStrings.Get("GLUE_DECK_TRAY_DECK_COUNT_LABEL");
        object[] args = new object[] { deckCount, CollectionManager.Get().GetMaxNumCustomDecks() };
        string str2 = GameStrings.Format("GLUE_DECK_TRAY_COUNT", args);
        this.m_countLabelText.Text = str;
        this.m_countText.Text = str2;
    }

    public static void OnDeckTrayTileScrollVisibleAffected(GameObject obj, bool visible)
    {
        DeckTrayDeckTileVisual component = obj.GetComponent<DeckTrayDeckTileVisual>();
        if (((component != null) && component.IsInUse()) && (visible != component.gameObject.activeSelf))
        {
            component.gameObject.SetActive(visible);
        }
    }

    private void OnDestroy()
    {
        CollectionManager manager = CollectionManager.Get();
        if (manager != null)
        {
            manager.RemoveTaggedDeckChanged(new CollectionManager.OnTaggedDeckChanged(this.OnTaggedDeckChanged));
            manager.DoneEditing();
        }
        s_instance = null;
    }

    private void OnHeroAssigned(string cardID)
    {
        this.m_decksContent.UpdateEditingDeckBoxVisual(cardID);
    }

    private void OnTaggedDeckChanged(CollectionManager.DeckTag tag, CollectionDeck newDeck, CollectionDeck oldDeck, object callbackData)
    {
        bool isNewDeck = ((callbackData != null) && (callbackData is bool)) && ((bool) callbackData);
        foreach (KeyValuePair<DeckContentTypes, DeckTrayContent> pair in this.m_contents)
        {
            pair.Value.OnTaggedDeckChanged(tag, newDeck, oldDeck, isNewDeck);
        }
    }

    private void OnTouchScrollEnded()
    {
    }

    private void OnTouchScrollStarted()
    {
        if (this.m_deckBigCard != null)
        {
            this.m_deckBigCard.ForceHide();
        }
    }

    public void PopulateDeck(RDM_Deck newDeck)
    {
        <PopulateDeck>c__AnonStorey2C5 storeyc = new <PopulateDeck>c__AnonStorey2C5 {
            newDeck = newDeck,
            <>f__this = this
        };
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_PASTE_POPUP_HEADER"),
            m_text = GameStrings.Format("GLUE_COLLECTION_DECK_PASTE_POPUP_MESSAGE", new object[0]),
            m_showAlertIcon = true,
            m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL
        };
        AlertPopup.ResponseCallback callback = new AlertPopup.ResponseCallback(storeyc.<>m__8E);
        info.m_responseCallback = callback;
        DialogManager.Get().ShowPopup(info);
    }

    private void PopupMissingCardsConfirmation(int numMissingCards)
    {
        CollectionManagerDisplay.Get().SetViewMode(CollectionManagerDisplay.ViewMode.CARDS, null);
        AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_INVALID_POPUP_HEADER")
        };
        object[] args = new object[] { numMissingCards };
        info2.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_INVALID_POPUP_MESSAGE", args);
        info2.m_cancelText = GameStrings.Get("GLUE_COLLECTION_DECK_SAVE_ANYWAY");
        info2.m_confirmText = GameStrings.Get("GLUE_COLLECTION_DECK_FINISH_FOR_ME");
        info2.m_showAlertIcon = true;
        info2.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
        info2.m_responseCallback = delegate (AlertPopup.Response response, object userData) {
            if (response == AlertPopup.Response.CANCEL)
            {
                this.SaveCurrentDeckAndEnterDeckListMode();
            }
            else
            {
                Navigation.Push(new Navigation.NavigateBackHandler(this.OnBackOutOfDeckContents));
                this.FinishMyDeckPress();
            }
        };
        AlertPopup.PopupInfo info = info2;
        DialogManager.Get().ShowPopup(info);
    }

    public void RegisterModeSwitchedListener(ModeSwitched callback)
    {
        this.m_modeSwitchedListeners.Add(callback);
    }

    public void RemoveCard(string cardID, CardFlair flair, bool owned)
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        if (taggedDeck != null)
        {
            taggedDeck.RemoveCard(cardID, flair.Premium, owned);
            this.HandleDeletedCardDeckUpdate(cardID, flair);
        }
    }

    private void SaveCurrentDeckAndEnterDeckListMode()
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        if (taggedDeck != null)
        {
            taggedDeck.SendChanges();
        }
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL)
        {
            if (TavernBrawlDisplay.Get() != null)
            {
                TavernBrawlDisplay.Get().BackFromDeckEdit(true);
            }
        }
        else
        {
            this.SetTrayMode(DeckContentTypes.Decks);
            CollectionManager.Get().DoneEditing();
            if (CollectionManagerDisplay.Get() != null)
            {
                CollectionManagerDisplay.Get().OnDoneEditingDeck();
            }
        }
    }

    private void SaveScrollbarPosition(DeckContentTypes contentType)
    {
        <SaveScrollbarPosition>c__AnonStorey2C6 storeyc = new <SaveScrollbarPosition>c__AnonStorey2C6 {
            contentType = contentType
        };
        DeckContentScroll scroll = this.m_scrollables.Find(new Predicate<DeckContentScroll>(storeyc.<>m__90));
        if ((scroll != null) && scroll.m_saveScrollPosition)
        {
            scroll.SaveCurrentScroll(this.m_scrollbar.GetScroll());
        }
    }

    public bool SetCardBack(Actor actor)
    {
        CollectionCardBack component = actor.gameObject.GetComponent<CollectionCardBack>();
        if (component == null)
        {
            return false;
        }
        return this.GetCardBackContent().SetNewCardBack(component.GetCardBackId(), actor.gameObject);
    }

    public void SetHeroSkin(Actor actor)
    {
        this.GetHeroSkinContent().SetNewHeroSkin(actor);
    }

    public void SetMyDecksLabelText(string text)
    {
        this.m_myDecksLabel.Text = text;
    }

    public void SetTrayMode(DeckContentTypes contentType)
    {
        this.m_contentToSet = contentType;
        if (!this.m_settingNewMode && (this.m_currentContent != contentType))
        {
            base.StartCoroutine(this.UpdateTrayMode());
        }
    }

    public void ShowDeck(long deckID, bool isNewDeck)
    {
        CollectionManager.Get().StartEditingDeck(CollectionManager.DeckTag.Editing, deckID, isNewDeck);
        CollectionManagerDisplay.ViewMode viewMode = CollectionManagerDisplay.Get().GetViewMode();
        if (((viewMode == CollectionManagerDisplay.ViewMode.HERO_SKINS) && !CollectionManagerDisplay.Get().CanViewHeroSkins()) || ((viewMode == CollectionManagerDisplay.ViewMode.CARD_BACKS) && !CollectionManagerDisplay.Get().CanViewCardBacks()))
        {
            viewMode = CollectionManagerDisplay.ViewMode.CARDS;
            CollectionManagerDisplay.Get().SetViewMode(CollectionManagerDisplay.ViewMode.CARDS, null);
        }
        DeckContentTypes contentTypeFromViewMode = this.GetContentTypeFromViewMode(viewMode);
        this.SetTrayMode(contentTypeFromViewMode);
        if (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL)
        {
            Navigation.Push(new Navigation.NavigateBackHandler(this.OnBackOutOfDeckContents));
        }
    }

    private void ShowDeckBigCard(DeckTrayDeckTileVisual cardTile, float delay = 0)
    {
        CollectionDeckTileActor actor = cardTile.GetActor();
        if (this.m_deckBigCard != null)
        {
            EntityDef entityDef = actor.GetEntityDef();
            CardDef cardDef = CollectionCardCache.Get().GetCardDef(entityDef.GetCardId());
            this.m_deckBigCard.Show(entityDef, actor.GetCardFlair(), cardDef, actor.gameObject.transform.position, !cardTile.IsOwnedSlot(), delay);
            if (UniversalInputManager.Get().IsTouchMode())
            {
                cardTile.SetHighlight(true);
            }
        }
    }

    private void Start()
    {
        SoundManager.Get().Load("panel_slide_off_deck_creation_screen");
        this.SetTrayMode(DeckContentTypes.Decks);
    }

    private void TryDisableScrollbar()
    {
        if ((this.m_scrollbar != null) && (this.m_scrollbar.m_ScrollObject != null))
        {
            this.m_scrollbar.Enable(false);
            this.m_scrollbar.m_ScrollObject = null;
        }
    }

    private void TryEnableScrollbar()
    {
        if ((this.m_scrollbar != null) && (this.GetCurrentContent() != null))
        {
            DeckContentScroll scroll = this.m_scrollables.Find(type => this.GetCurrentContentType() == type.m_contentType);
            if ((scroll == null) || (scroll.m_scrollObject == null))
            {
                UnityEngine.Debug.LogWarning("No scrollable object defined.");
            }
            else
            {
                this.m_scrollbar.m_ScrollObject = scroll.m_scrollObject;
                this.m_scrollbar.ResetScrollStartPosition(scroll.GetStartPosition());
                if (scroll.m_saveScrollPosition)
                {
                    this.m_scrollbar.SetScrollSnap(scroll.GetCurrentScroll(), true);
                }
                this.m_scrollbar.EnableIfNeeded();
            }
        }
    }

    public void Unload()
    {
        CollectionInputMgr.Get().SetScrollbar(null);
    }

    public void UnregisterModeSwitchedListener(ModeSwitched callback)
    {
        this.m_modeSwitchedListeners.Remove(callback);
    }

    private void UpdateDoneButtonText(DeckContentTypes oldContentType, DeckContentTypes newContentType)
    {
        bool flag = (oldContentType == DeckContentTypes.INVALID) || ((oldContentType == DeckContentTypes.Cards) && (newContentType == DeckContentTypes.Decks));
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL)
        {
            flag = !TavernBrawlDisplay.Get().IsInDeckEditMode() && (UniversalInputManager.UsePhoneUI == 0);
        }
        bool flag2 = this.m_backArrow != null;
        if (flag)
        {
            this.m_doneButton.SetText(!flag2 ? GameStrings.Get("GLOBAL_BACK") : string.Empty);
            if (flag2)
            {
                this.m_backArrow.gameObject.SetActive(true);
            }
        }
        else
        {
            this.m_doneButton.SetText(GameStrings.Get("GLOBAL_DONE"));
            if (flag2)
            {
                this.m_backArrow.gameObject.SetActive(false);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateTrayMode()
    {
        return new <UpdateTrayMode>c__Iterator32 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <AutoAddCardsWithTiming>c__Iterator31 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<RDMDeckEntry> <$>newCards;
        internal CollectionDeckTray <>f__this;
        internal bool <added>__2;
        internal int <i>__0;
        internal RDMDeckEntry <newCard>__1;
        internal List<RDMDeckEntry> newCards;

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
                    this.<>f__this.AllowInput(false);
                    if (!CollectionManager.Get().IsInEditMode())
                    {
                        goto Label_017B;
                    }
                    this.<i>__0 = 0;
                    goto Label_0165;

                case 1:
                    break;

                default:
                    goto Label_018E;
            }
        Label_0157:
            this.<i>__0++;
        Label_0165:
            if (this.<i>__0 < this.newCards.Count)
            {
                this.<newCard>__1 = this.newCards[this.<i>__0];
                this.<added>__2 = false;
                this.<added>__2 |= this.<>f__this.AddCard(this.<newCard>__1.EntityDef, this.<newCard>__1.Flair, null, true, null);
                if (!this.<added>__2 && (this.<newCard>__1.Flair.Premium != TAG_PREMIUM.NORMAL))
                {
                    this.<added>__2 |= this.<>f__this.AddCard(this.<newCard>__1.EntityDef, new CardFlair(TAG_PREMIUM.NORMAL), null, true, null);
                }
                if (!this.<added>__2 && (this.<newCard>__1.Flair.Premium != TAG_PREMIUM.GOLDEN))
                {
                    this.<added>__2 |= this.<>f__this.AddCard(this.<newCard>__1.EntityDef, new CardFlair(TAG_PREMIUM.GOLDEN), null, true, null);
                }
                if (this.<added>__2)
                {
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 1;
                    return true;
                }
                goto Label_0157;
            }
        Label_017B:
            this.<>f__this.AllowInput(true);
            this.$PC = -1;
        Label_018E:
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
    private sealed class <DestroyAfterSeconds>c__Iterator33 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>go;
        internal GameObject go;

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
                    this.$current = new WaitForSeconds(5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    UnityEngine.Object.Destroy(this.go);
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
    private sealed class <PopulateDeck>c__AnonStorey2C5
    {
        internal CollectionDeckTray <>f__this;
        internal RDM_Deck newDeck;

        internal void <>m__8E(AlertPopup.Response response, object userdata)
        {
            if (response == AlertPopup.Response.CONFIRM)
            {
                CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing).ClearSlotContents();
                this.<>f__this.GetCardsContent().UpdateCardList(true, null);
                this.<>f__this.StartCoroutine(this.<>f__this.AutoAddCardsWithTiming(this.newDeck.deckList));
            }
        }
    }

    [CompilerGenerated]
    private sealed class <SaveScrollbarPosition>c__AnonStorey2C6
    {
        internal CollectionDeckTray.DeckContentTypes contentType;

        internal bool <>m__90(CollectionDeckTray.DeckContentScroll type)
        {
            return (this.contentType == type.m_contentType);
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateTrayMode>c__Iterator32 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionDeckTray <>f__this;
        internal DeckTrayContent <newContent>__3;
        internal CollectionDeckTray.DeckContentTypes <newContentType>__1;
        internal DeckTrayContent <oldContent>__2;
        internal CollectionDeckTray.DeckContentTypes <oldContentType>__0;

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
                    this.<oldContentType>__0 = this.<>f__this.m_currentContent;
                    this.<newContentType>__1 = this.<>f__this.m_contentToSet;
                    if ((!this.<>f__this.m_settingNewMode && (this.<>f__this.m_currentContent != this.<>f__this.m_contentToSet)) && (this.<>f__this.m_contentToSet != CollectionDeckTray.DeckContentTypes.INVALID))
                    {
                        this.<>f__this.UpdateDoneButtonText(this.<oldContentType>__0, this.<newContentType>__1);
                        this.<>f__this.m_contentToSet = CollectionDeckTray.DeckContentTypes.INVALID;
                        this.<>f__this.m_currentContent = CollectionDeckTray.DeckContentTypes.INVALID;
                        this.<>f__this.m_settingNewMode = true;
                        this.<oldContent>__2 = null;
                        this.<newContent>__3 = null;
                        this.<>f__this.m_contents.TryGetValue(this.<oldContentType>__0, out this.<oldContent>__2);
                        this.<>f__this.m_contents.TryGetValue(this.<newContentType>__1, out this.<newContent>__3);
                        if (this.<oldContent>__2 != null)
                        {
                            break;
                        }
                        goto Label_015C;
                    }
                    goto Label_0397;

                case 1:
                    break;

                case 2:
                    goto Label_0185;

                case 3:
                    goto Label_01E6;

                case 4:
                    goto Label_020E;

                case 5:
                    goto Label_0264;

                case 6:
                    goto Label_028C;

                case 7:
                    goto Label_02E8;

                case 8:
                    goto Label_0321;

                default:
                    goto Label_0397;
            }
            while (!this.<oldContent>__2.PreAnimateContentExit())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0399;
            }
        Label_015C:
            if (this.<newContent>__3 == null)
            {
                goto Label_0195;
            }
        Label_0185:
            while (!this.<newContent>__3.PreAnimateContentEntrance())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0399;
            }
        Label_0195:
            this.<>f__this.SaveScrollbarPosition(this.<oldContentType>__0);
            this.<>f__this.TryDisableScrollbar();
            if (this.<oldContent>__2 == null)
            {
                goto Label_021E;
            }
            this.<oldContent>__2.SetModeActive(false);
        Label_01E6:
            while (!this.<oldContent>__2.AnimateContentExitStart())
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_0399;
            }
        Label_020E:
            while (!this.<oldContent>__2.AnimateContentExitEnd())
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_0399;
            }
        Label_021E:
            this.<>f__this.m_currentContent = this.<newContentType>__1;
            if (this.<newContent>__3 == null)
            {
                goto Label_02B4;
            }
            this.<newContent>__3.SetModeTrying(true);
        Label_0264:
            while (!this.<newContent>__3.AnimateContentEntranceStart())
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_0399;
            }
        Label_028C:
            while (!this.<newContent>__3.AnimateContentEntranceEnd())
            {
                this.$current = null;
                this.$PC = 6;
                goto Label_0399;
            }
            this.<newContent>__3.SetModeActive(true);
            this.<newContent>__3.SetModeTrying(false);
        Label_02B4:
            this.<>f__this.TryEnableScrollbar();
            if (this.<newContent>__3 == null)
            {
                goto Label_02F8;
            }
        Label_02E8:
            while (!this.<newContent>__3.PostAnimateContentEntrance())
            {
                this.$current = null;
                this.$PC = 7;
                goto Label_0399;
            }
        Label_02F8:
            if (this.<oldContent>__2 == null)
            {
                goto Label_0331;
            }
        Label_0321:
            while (!this.<oldContent>__2.PostAnimateContentExit())
            {
                this.$current = null;
                this.$PC = 8;
                goto Label_0399;
            }
        Label_0331:
            if (this.<>f__this.m_currentContent != CollectionDeckTray.DeckContentTypes.Decks)
            {
                this.<>f__this.m_cardsContent.TriggerCardCountUpdate();
            }
            this.<>f__this.m_settingNewMode = false;
            this.<>f__this.FireModeSwitchedEvent();
            if (this.<>f__this.m_contentToSet != CollectionDeckTray.DeckContentTypes.INVALID)
            {
                this.<>f__this.StartCoroutine(this.<>f__this.UpdateTrayMode());
            }
            this.$PC = -1;
        Label_0397:
            return false;
        Label_0399:
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

    [Serializable]
    public class DeckContentScroll
    {
        public CollectionDeckTray.DeckContentTypes m_contentType;
        private float m_currentScroll;
        public bool m_saveScrollPosition;
        public GameObject m_scrollObject;
        private Vector3 m_startPos;

        public Vector3 GetCurrentPosition()
        {
            return ((this.m_scrollObject == null) ? Vector3.zero : this.m_scrollObject.transform.localPosition);
        }

        public float GetCurrentScroll()
        {
            return this.m_currentScroll;
        }

        public Vector3 GetStartPosition()
        {
            return this.m_startPos;
        }

        public void SaveCurrentScroll(float scroll)
        {
            this.m_currentScroll = scroll;
        }

        public void SaveStartPosition()
        {
            if (this.m_scrollObject != null)
            {
                this.m_startPos = this.m_scrollObject.transform.localPosition;
            }
        }
    }

    public enum DeckContentTypes
    {
        Decks,
        Cards,
        HeroSkin,
        CardBack,
        INVALID
    }

    public delegate void ModeSwitched();
}

