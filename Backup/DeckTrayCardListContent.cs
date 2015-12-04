using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class DeckTrayCardListContent : DeckTrayContent
{
    private const string ADD_CARD_TO_DECK_SOUND = "collection_manager_card_add_to_deck_instant";
    private const float CARD_MOVEMENT_TIME = 0.3f;
    private const float DECK_HELP_BUTTON_EMPTY_DECK_Y_LOCAL_POS = -0.01194457f;
    private const float DECK_HELP_BUTTON_Y_TILE_OFFSET = -0.04915909f;
    private bool m_animating;
    private List<CardCountChanged> m_cardCountChangedListeners = new List<CardCountChanged>();
    [CustomEditField(Sections="Card Tile Settings")]
    public float m_cardHelpButtonHeight = 3f;
    [CustomEditField(Sections="Card Tile Settings")]
    public float m_cardTileHeight = 2.45f;
    private List<CardTileHeld> m_cardTileHeldListeners = new List<CardTileHeld>();
    [CustomEditField(Sections="Card Tile Settings")]
    public Vector3 m_cardTileOffset = Vector3.zero;
    private List<CardTileOut> m_cardTileOutListeners = new List<CardTileOut>();
    private List<CardTileOver> m_cardTileOverListeners = new List<CardTileOver>();
    private List<CardTilePress> m_cardTilePressListeners = new List<CardTilePress>();
    private List<CardTileRelease> m_cardTileReleaseListeners = new List<CardTileRelease>();
    private List<CardTileRightClicked> m_cardTileRightClickedListeners = new List<CardTileRightClicked>();
    private List<DeckTrayDeckTileVisual> m_cardTiles = new List<DeckTrayDeckTileVisual>();
    [CustomEditField(Sections="Card Tile Settings")]
    public float m_cardTileSlotLocalHeight;
    [CustomEditField(Sections="Card Tile Settings")]
    public Vector3 m_cardTileSlotLocalScaleVec3 = new Vector3(0.01f, 0.02f, 0.01f);
    [CustomEditField(Sections="Card Tile Settings")]
    public float m_deckCardBarFlareUpInterval = 0.075f;
    [CustomEditField(Sections="Other Objects")]
    public GameObject m_deckCompleteHighlight;
    [CustomEditField(Sections="Deck Help")]
    public UIBButton m_deckHelpButton;
    [CustomEditField(Sections="Deck Type Settings")]
    public CollectionManager.DeckTag m_deckType;
    private bool m_inArena;
    private bool m_loading;
    private Vector3 m_originalLocalPosition;
    [CustomEditField(Sections="Card Tile Settings")]
    public GameObject m_phoneDeckTileBone;
    [CustomEditField(Sections="Scroll Settings")]
    public UIBScrollable m_scrollbar;

    public bool AddCard(EntityDef cardEntityDef, CardFlair cardFlair, DeckTrayDeckTileVisual deckTileToRemove, bool playSound, Actor animateFromActor = null)
    {
        if (!base.IsModeActive())
        {
            return false;
        }
        if (cardEntityDef == null)
        {
            UnityEngine.Debug.LogError("Trying to add card EntityDef that is null.");
            return false;
        }
        string cardId = cardEntityDef.GetCardId();
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(this.m_deckType);
        if (taggedDeck == null)
        {
            return false;
        }
        if (playSound)
        {
            SoundManager.Get().LoadAndPlay("collection_manager_place_card_in_deck", base.gameObject);
        }
        if (taggedDeck.GetTotalCardCount() == 30)
        {
            if (deckTileToRemove == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("CollectionDeckTray.AddCard(): Cannot add card {0} (flair {1}) without removing one first.", cardEntityDef.GetCardId(), cardFlair));
                return false;
            }
            string cardID = deckTileToRemove.GetCardID();
            CardFlair flair = deckTileToRemove.GetCardFlair();
            if (!taggedDeck.RemoveCard(cardID, flair.Premium, deckTileToRemove.IsOwnedSlot()))
            {
                object[] args = new object[] { cardId, cardFlair, cardID, flair };
                UnityEngine.Debug.LogWarning(string.Format("CollectionDeckTray.AddCard({0},{1}): Tried to remove card {2} with flair {3}, but it failed!", args));
                return false;
            }
        }
        if (!taggedDeck.AddCard(cardEntityDef, cardFlair.Premium))
        {
            UnityEngine.Debug.LogWarning(string.Format("CollectionDeckTray.AddCard({0},{1}): deck.AddCard failed!", cardId, cardFlair));
            return false;
        }
        if (taggedDeck.GetTotalOwnedCardCount() == 30)
        {
            DeckHelper.Get().Hide();
        }
        this.UpdateCardList(cardEntityDef, true, animateFromActor);
        CollectionManagerDisplay.Get().UpdateCurrentPageCardLocks(true);
        if ((!Options.Get().GetBool(Option.HAS_ADDED_CARDS_TO_DECK, false) && (taggedDeck.GetTotalCardCount() >= 2)) && (!DeckHelper.Get().IsActive() && (taggedDeck.GetTotalCardCount() < 15)))
        {
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_CM_PAGEFLIP_28"), "VO_INNKEEPER_CM_PAGEFLIP_28", 0f, null);
            Options.Get().SetBool(Option.HAS_ADDED_CARDS_TO_DECK, true);
        }
        return true;
    }

    public override bool AnimateContentEntranceEnd()
    {
        if (this.m_animating)
        {
            return false;
        }
        this.FireCardCountChangedEvent();
        return true;
    }

    public override bool AnimateContentEntranceStart()
    {
        if (this.m_loading)
        {
            return false;
        }
        this.m_animating = true;
        Action<object> action = delegate (object _1) {
            this.UpdateDeckCompleteHighlight();
            this.m_animating = false;
        };
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(this.m_deckType);
        if (taggedDeck != null)
        {
            base.transform.localPosition = this.GetOffscreenLocalPosition();
            iTween.StopByName(base.gameObject, "position");
            object[] args = new object[] { "position", this.m_originalLocalPosition, "isLocal", true, "time", 0.3f, "easeType", iTween.EaseType.easeOutQuad, "oncomplete", action, "name", "position" };
            iTween.MoveTo(base.gameObject, iTween.Hash(args));
            if (taggedDeck.GetTotalCardCount() > 0)
            {
                SoundManager.Get().LoadAndPlay("collection_manager_new_deck_moves_up_tray", base.gameObject);
            }
            this.UpdateCardList(false, null);
        }
        else
        {
            action(null);
        }
        return true;
    }

    public override bool AnimateContentExitEnd()
    {
        return !this.m_animating;
    }

    public override bool AnimateContentExitStart()
    {
        if (this.m_animating)
        {
            return false;
        }
        this.m_animating = true;
        if (this.m_deckCompleteHighlight != null)
        {
            this.m_deckCompleteHighlight.SetActive(false);
        }
        iTween.StopByName(base.gameObject, "position");
        object[] args = new object[] { "position", this.GetOffscreenLocalPosition(), "isLocal", true, "time", 0.3f, "easeType", iTween.EaseType.easeInQuad, "name", "position" };
        iTween.MoveTo(base.gameObject, iTween.Hash(args));
        SoundManager.Get().LoadAndPlay("panel_slide_off_deck_creation_screen", base.gameObject);
        ApplicationMgr.Get().ScheduleCallback(0.5f, false, delegate (object o) {
            this.m_animating = false;
        }, null);
        return true;
    }

    private void Awake()
    {
        this.m_deckHelpButton.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.OnDeckHelpButtonPress));
        this.m_deckHelpButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnDeckHelpButtonOver));
        this.m_deckHelpButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnDeckHelpButtonOut));
        this.m_originalLocalPosition = base.transform.localPosition;
    }

    [DebuggerHidden]
    private IEnumerator FinishPhoneMovingCardTile(GameObject obj, Actor movingCardTile, float delay)
    {
        return new <FinishPhoneMovingCardTile>c__Iterator252 { delay = delay, movingCardTile = movingCardTile, obj = obj, <$>delay = delay, <$>movingCardTile = movingCardTile, <$>obj = obj };
    }

    private void FireCardCountChangedEvent()
    {
        CardCountChanged[] changedArray = this.m_cardCountChangedListeners.ToArray();
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(this.m_deckType);
        int cardCount = 0;
        if (taggedDeck != null)
        {
            cardCount = (SceneMgr.Get().GetMode() != SceneMgr.Mode.COLLECTIONMANAGER) ? taggedDeck.GetTotalCardCount() : taggedDeck.GetTotalOwnedCardCount();
        }
        foreach (CardCountChanged changed in changedArray)
        {
            changed(cardCount);
        }
    }

    private void FireCardTileHeldEvent(DeckTrayDeckTileVisual cardTile)
    {
        foreach (CardTileHeld held in this.m_cardTileHeldListeners.ToArray())
        {
            held(cardTile);
        }
    }

    private void FireCardTileOutEvent(DeckTrayDeckTileVisual cardTile)
    {
        foreach (CardTileOut @out in this.m_cardTileOutListeners.ToArray())
        {
            @out(cardTile);
        }
    }

    private void FireCardTileOverEvent(DeckTrayDeckTileVisual cardTile)
    {
        foreach (CardTileOver over in this.m_cardTileOverListeners.ToArray())
        {
            over(cardTile);
        }
    }

    private void FireCardTilePressEvent(DeckTrayDeckTileVisual cardTile)
    {
        foreach (CardTilePress press in this.m_cardTilePressListeners.ToArray())
        {
            press(cardTile);
        }
    }

    private void FireCardTileReleaseEvent(DeckTrayDeckTileVisual cardTile)
    {
        foreach (CardTileRelease release in this.m_cardTileReleaseListeners.ToArray())
        {
            release(cardTile);
        }
    }

    private void FireCardTileRightClickedEvent(DeckTrayDeckTileVisual cardTile)
    {
        foreach (CardTileRightClicked clicked in this.m_cardTileRightClickedListeners.ToArray())
        {
            clicked(cardTile);
        }
    }

    public List<DeckTrayDeckTileVisual> GetCardTiles()
    {
        return this.m_cardTiles;
    }

    public DeckTrayDeckTileVisual GetCardTileVisual(int index)
    {
        if (index < this.m_cardTiles.Count)
        {
            return this.m_cardTiles[index];
        }
        return null;
    }

    public DeckTrayDeckTileVisual GetCardTileVisual(string cardID)
    {
        foreach (DeckTrayDeckTileVisual visual in this.m_cardTiles)
        {
            if ((((visual != null) && (visual.GetActor() != null)) && (visual.GetActor().GetEntityDef() != null)) && (visual.GetActor().GetEntityDef().GetCardId() == cardID))
            {
                return visual;
            }
        }
        return null;
    }

    public DeckTrayDeckTileVisual GetCardTileVisual(string cardID, TAG_PREMIUM premType)
    {
        foreach (DeckTrayDeckTileVisual visual in this.m_cardTiles)
        {
            if ((((visual != null) && (visual.GetActor() != null)) && (visual.GetActor().GetEntityDef() != null)) && ((visual.GetActor().GetEntityDef().GetCardId() == cardID) && (visual.GetActor().GetCardFlair().Premium == premType)))
            {
                return visual;
            }
        }
        return null;
    }

    public DeckTrayDeckTileVisual GetCardTileVisualOrLastVisible(string cardID)
    {
        int num = 0;
        foreach (DeckTrayDeckTileVisual visual in this.m_cardTiles)
        {
            num++;
            if (((visual != null) && (visual.GetActor() != null)) && (visual.GetActor().GetEntityDef() != null))
            {
                if (num > 20)
                {
                    return visual;
                }
                if (visual.GetActor().GetEntityDef().GetCardId() == cardID)
                {
                    return visual;
                }
            }
        }
        return null;
    }

    public Vector3 GetCardVisualExtents()
    {
        return new Vector3(this.m_cardTileHeight, this.m_cardTileHeight, this.m_cardTileHeight);
    }

    private Vector3 GetOffscreenLocalPosition()
    {
        Vector3 originalLocalPosition = this.m_originalLocalPosition;
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(this.m_deckType);
        int num = (taggedDeck == null) ? 0 : (taggedDeck.GetSlotCount() + 2);
        originalLocalPosition.z -= this.m_cardTileHeight * num;
        return originalLocalPosition;
    }

    public DeckTrayDeckTileVisual GetOrAddCardTileVisual(int index)
    {
        <GetOrAddCardTileVisual>c__AnonStorey36C storeyc = new <GetOrAddCardTileVisual>c__AnonStorey36C {
            <>f__this = this,
            newTileVisual = this.GetCardTileVisual(index)
        };
        if (storeyc.newTileVisual == null)
        {
            GameObject child = new GameObject("DeckTileVisual" + index);
            GameUtils.SetParent(child, this, false);
            child.transform.localScale = this.m_cardTileSlotLocalScaleVec3;
            storeyc.newTileVisual = child.AddComponent<DeckTrayDeckTileVisual>();
            storeyc.newTileVisual.AddEventListener(UIEventType.HOLD, new UIEvent.Handler(storeyc.<>m__233));
            storeyc.newTileVisual.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(storeyc.<>m__234));
            storeyc.newTileVisual.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(storeyc.<>m__235));
            storeyc.newTileVisual.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(storeyc.<>m__236));
            storeyc.newTileVisual.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storeyc.<>m__237));
            storeyc.newTileVisual.AddEventListener(UIEventType.RIGHTCLICK, new UIEvent.Handler(storeyc.<>m__238));
            this.m_cardTiles.Insert(index, storeyc.newTileVisual);
            Vector3 extents = new Vector3(this.m_cardTileHeight, this.m_cardTileHeight, this.m_cardTileHeight);
            if (this.m_scrollbar != null)
            {
                this.m_scrollbar.AddVisibleAffectedObject(child, extents, true, new UIBScrollable.VisibleAffected(this.IsCardTileVisible));
            }
        }
        return storeyc.newTileVisual;
    }

    public override void Hide(bool hideAll = false)
    {
        foreach (DeckTrayDeckTileVisual visual in this.m_cardTiles)
        {
            if (hideAll || !visual.IsInUse())
            {
                visual.Hide();
            }
        }
    }

    private void IsCardTileVisible(GameObject obj, bool visible)
    {
        if (obj.activeSelf != visible)
        {
            obj.SetActive(visible && obj.GetComponent<DeckTrayDeckTileVisual>().IsInUse());
        }
    }

    private void LoadCardPrefabs(List<CollectionDeckSlot> deckSlots)
    {
        <LoadCardPrefabs>c__AnonStorey36D storeyd = new <LoadCardPrefabs>c__AnonStorey36D {
            <>f__this = this
        };
        if (deckSlots.Count != 0)
        {
            storeyd.prefabsToLoad = deckSlots.Count;
            this.m_loading = true;
            for (int i = 0; i < deckSlots.Count; i++)
            {
                CollectionDeckSlot slot = deckSlots[i];
                if (slot.Count == 0)
                {
                    Log.Rachelle.Print(string.Format("CollectionDeckTray.LoadCardPrefabs(): Slot {0} of deck is empty! Skipping...", i), new object[0]);
                }
                else
                {
                    CollectionCardCache.Get().LoadCardDef(slot.CardID, new CollectionCardCache.LoadCardDefCallback(storeyd.<>m__239), null, new CardPortraitQuality(1, false));
                }
            }
        }
    }

    private void OnDeckHelpButtonOut(UIEvent e)
    {
        HighlightState componentInChildren = this.m_deckHelpButton.GetComponentInChildren<HighlightState>();
        if (componentInChildren != null)
        {
            if (!Options.Get().GetBool(Option.HAS_FINISHED_A_DECK, false))
            {
                componentInChildren.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
            }
            else
            {
                componentInChildren.ChangeState(ActorStateType.NONE);
            }
        }
    }

    private void OnDeckHelpButtonOver(UIEvent e)
    {
        HighlightState componentInChildren = this.m_deckHelpButton.GetComponentInChildren<HighlightState>();
        if (componentInChildren != null)
        {
            if (!Options.Get().GetBool(Option.HAS_FINISHED_A_DECK, false))
            {
                componentInChildren.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
            }
            else
            {
                componentInChildren.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
            }
        }
        SoundManager.Get().LoadAndPlay("Small_Mouseover", base.gameObject);
    }

    private void OnDeckHelpButtonPress(UIEvent e)
    {
        this.ShowDeckHelper();
    }

    public override void OnTaggedDeckChanged(CollectionManager.DeckTag tag, CollectionDeck newDeck, CollectionDeck oldDeck, bool isNewDeck)
    {
        if ((tag == CollectionManager.DeckTag.Editing) && (newDeck != null))
        {
            List<CollectionDeckSlot> slots = newDeck.GetSlots();
            this.LoadCardPrefabs(slots);
            if (base.IsModeActive())
            {
                this.ShowDeckHelpButtonIfNeeded();
            }
        }
    }

    public void RegisterCardCountUpdated(CardCountChanged dlg)
    {
        this.m_cardCountChangedListeners.Add(dlg);
    }

    public void RegisterCardTileHeldListener(CardTileHeld dlg)
    {
        this.m_cardTileHeldListeners.Add(dlg);
    }

    public void RegisterCardTileOutListener(CardTileOut dlg)
    {
        this.m_cardTileOutListeners.Add(dlg);
    }

    public void RegisterCardTileOverListener(CardTileOver dlg)
    {
        this.m_cardTileOverListeners.Add(dlg);
    }

    public void RegisterCardTilePressListener(CardTilePress dlg)
    {
        this.m_cardTilePressListeners.Add(dlg);
    }

    public void RegisterCardTileReleaseListener(CardTileRelease dlg)
    {
        this.m_cardTileReleaseListeners.Add(dlg);
    }

    public void RegisterCardTileRightClickedListener(CardTileRightClicked dlg)
    {
        this.m_cardTileRightClickedListeners.Add(dlg);
    }

    public void RemoveClosestTemplateCard(EntityDef entityDef)
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(this.m_deckType);
        int cost = entityDef.GetCost();
        int num2 = 0x7fffffff;
        string cardID = string.Empty;
        foreach (CollectionDeckSlot slot in taggedDeck.GetSlots())
        {
            if (!slot.Owned)
            {
                EntityDef def = DefLoader.Get().GetEntityDef(slot.CardID);
                int num3 = Mathf.Abs((int) (cost - def.GetCost()));
                if (num3 < num2)
                {
                    num2 = num3;
                    cardID = slot.CardID;
                }
            }
        }
        if (!string.IsNullOrEmpty(cardID))
        {
            taggedDeck.RemoveCard(cardID, TAG_PREMIUM.NORMAL, false);
        }
        this.UpdateCardList(true, null);
    }

    public void SetInArena(bool inArena)
    {
        this.m_inArena = inArena;
    }

    public override void Show(bool showAll = false)
    {
        foreach (DeckTrayDeckTileVisual visual in this.m_cardTiles)
        {
            if (showAll || visual.IsInUse())
            {
                visual.Show();
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowAddCardAnimationAfterTrayLoads(Actor cardToAnimate)
    {
        return new <ShowAddCardAnimationAfterTrayLoads>c__Iterator251 { cardToAnimate = cardToAnimate, <$>cardToAnimate = cardToAnimate, <>f__this = this };
    }

    public void ShowDeckCompleteEffects()
    {
        base.StartCoroutine(this.ShowDeckCompleteEffectsWithInterval(this.m_deckCardBarFlareUpInterval));
    }

    [DebuggerHidden]
    private IEnumerator ShowDeckCompleteEffectsWithInterval(float interval)
    {
        return new <ShowDeckCompleteEffectsWithInterval>c__Iterator253 { interval = interval, <$>interval = interval, <>f__this = this };
    }

    public void ShowDeckHelpButtonIfNeeded()
    {
        bool flag = false;
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(this.m_deckType);
        if (TavernBrawlDisplay.IsTavernBrawlViewing() || TavernBrawlDisplay.IsTavernBrawlEditing())
        {
            flag = false;
        }
        else if (((taggedDeck != null) && (DeckHelper.Get() != null)) && (taggedDeck.GetTotalOwnedCardCount() < 30))
        {
            flag = true;
            Vector3 cardTileOffset = this.m_cardTileOffset;
            cardTileOffset.y -= this.m_cardTileSlotLocalHeight * taggedDeck.GetSlots().Count;
            this.m_deckHelpButton.transform.localPosition = cardTileOffset;
        }
        this.m_deckHelpButton.gameObject.SetActive(flag);
        if (!Options.Get().GetBool(Option.HAS_FINISHED_A_DECK, false))
        {
            HighlightState componentInChildren = this.m_deckHelpButton.GetComponentInChildren<HighlightState>();
            if (componentInChildren != null)
            {
                componentInChildren.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
            }
        }
    }

    public void ShowDeckHelper()
    {
        if (CollectionManager.Get().IsInEditMode() && (DeckHelper.Get() != null))
        {
            DeckHelper.Get().Show();
        }
    }

    public void TriggerCardCountUpdate()
    {
        this.FireCardCountChangedEvent();
    }

    public void UnregisterCardCountUpdated(CardCountChanged dlg)
    {
        this.m_cardCountChangedListeners.Remove(dlg);
    }

    public void UnregisterCardTileHeldListener(CardTileHeld dlg)
    {
        this.m_cardTileHeldListeners.Remove(dlg);
    }

    public void UnregisterCardTileOutListener(CardTileOut dlg)
    {
        this.m_cardTileOutListeners.Remove(dlg);
    }

    public void UnregisterCardTileOverListener(CardTileOver dlg)
    {
        this.m_cardTileOverListeners.Remove(dlg);
    }

    public void UnregisterCardTilePressListener(CardTilePress dlg)
    {
        this.m_cardTilePressListeners.Remove(dlg);
    }

    public void UnregisterCardTileReleaseListener(CardTileRelease dlg)
    {
        this.m_cardTileReleaseListeners.Remove(dlg);
    }

    public void UnregisterCardTileRightClickedListener(CardTileRightClicked dlg)
    {
        this.m_cardTileRightClickedListeners.Remove(dlg);
    }

    [ContextMenu("Update Card List")]
    public void UpdateCardList(bool updateHighlight = true, Actor animateFromActor = null)
    {
        this.UpdateCardList(string.Empty, updateHighlight, animateFromActor);
    }

    public void UpdateCardList(EntityDef justChangedCardEntityDef, bool updateHighlight = true, Actor animateFromActor = null)
    {
        this.UpdateCardList((justChangedCardEntityDef == null) ? string.Empty : justChangedCardEntityDef.GetCardId(), updateHighlight, animateFromActor);
    }

    public void UpdateCardList(string justChangedCardID, bool updateHighlight = true, Actor animateFromActor = null)
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(this.m_deckType);
        if (taggedDeck == null)
        {
            UnityEngine.Debug.LogError("No current deck set for DeckTrayCardListContent.");
        }
        else
        {
            foreach (DeckTrayDeckTileVisual visual in this.m_cardTiles)
            {
                visual.MarkAsUnused();
            }
            List<CollectionDeckSlot> slots = taggedDeck.GetSlots();
            int num = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                CollectionDeckSlot s = slots[i];
                if (s.Count == 0)
                {
                    Log.Rachelle.Print(string.Format("CollectionDeckTray.UpdateCardList(): Slot {0} of deck is empty! Skipping...", i), new object[0]);
                }
                else
                {
                    num += s.Count;
                    DeckTrayDeckTileVisual orAddCardTileVisual = this.GetOrAddCardTileVisual(i);
                    orAddCardTileVisual.SetInArena(this.m_inArena);
                    orAddCardTileVisual.gameObject.transform.localPosition = new Vector3(this.m_cardTileOffset.x, this.m_cardTileOffset.y + (this.m_cardTileSlotLocalHeight * -i), this.m_cardTileOffset.z);
                    orAddCardTileVisual.MarkAsUsed();
                    orAddCardTileVisual.Show();
                    orAddCardTileVisual.SetSlot(s, justChangedCardID.Equals(s.CardID));
                }
            }
            this.Hide(false);
            this.ShowDeckHelpButtonIfNeeded();
            this.FireCardCountChangedEvent();
            this.m_scrollbar.UpdateScroll();
            if (updateHighlight)
            {
                this.UpdateDeckCompleteHighlight();
            }
            if (animateFromActor != null)
            {
                base.StartCoroutine(this.ShowAddCardAnimationAfterTrayLoads(animateFromActor));
            }
        }
    }

    private void UpdateDeckCompleteHighlight()
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(this.m_deckType);
        bool flag = (taggedDeck != null) && (taggedDeck.GetTotalOwnedCardCount() == 30);
        if (this.m_deckCompleteHighlight != null)
        {
            this.m_deckCompleteHighlight.SetActive(flag);
        }
        if (flag && !Options.Get().GetBool(Option.HAS_FINISHED_A_DECK, false))
        {
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_CM_DECK_FINISH_30"), "VO_INNKEEPER_CM_DECK_FINISH_30", 3f, null);
            Options.Get().SetBool(Option.HAS_FINISHED_A_DECK, true);
        }
    }

    [CompilerGenerated]
    private sealed class <FinishPhoneMovingCardTile>c__Iterator252 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal Actor <$>movingCardTile;
        internal GameObject <$>obj;
        internal float delay;
        internal Actor movingCardTile;
        internal GameObject obj;

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
                    this.movingCardTile.Hide();
                    UnityEngine.Object.Destroy(this.obj);
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
    private sealed class <GetOrAddCardTileVisual>c__AnonStorey36C
    {
        internal DeckTrayCardListContent <>f__this;
        internal DeckTrayDeckTileVisual newTileVisual;

        internal void <>m__233(UIEvent e)
        {
            this.<>f__this.FireCardTileHeldEvent(this.newTileVisual);
        }

        internal void <>m__234(UIEvent e)
        {
            this.<>f__this.FireCardTilePressEvent(this.newTileVisual);
        }

        internal void <>m__235(UIEvent e)
        {
            this.<>f__this.FireCardTileOverEvent(this.newTileVisual);
        }

        internal void <>m__236(UIEvent e)
        {
            this.<>f__this.FireCardTileOutEvent(this.newTileVisual);
        }

        internal void <>m__237(UIEvent e)
        {
            this.<>f__this.FireCardTileReleaseEvent(this.newTileVisual);
        }

        internal void <>m__238(UIEvent e)
        {
            this.<>f__this.FireCardTileRightClickedEvent(this.newTileVisual);
        }
    }

    [CompilerGenerated]
    private sealed class <LoadCardPrefabs>c__AnonStorey36D
    {
        internal DeckTrayCardListContent <>f__this;
        internal int prefabsToLoad;

        internal void <>m__239(string _1, CardDef _2, object _3)
        {
            this.prefabsToLoad--;
            if (this.prefabsToLoad == 0)
            {
                this.<>f__this.m_loading = false;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ShowAddCardAnimationAfterTrayLoads>c__Iterator251 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>cardToAnimate;
        internal DeckTrayCardListContent <>f__this;
        internal CardFlair <cardFlair>__1;
        internal string <cardID>__0;
        internal Vector3 <cardPos>__3;
        internal GameObject <cardTileObject>__4;
        internal CollectionDeck <currentDeck>__6;
        internal Actor <movingCardTile>__5;
        internal Vector3[] <newPath>__7;
        internal Vector3 <startSpot>__8;
        internal DeckTrayDeckTileVisual <tile>__2;
        internal Actor cardToAnimate;

        internal void <>m__23A(object v)
        {
            this.<tile>__2.Show();
            this.<tile>__2.GetActor().GetSpell(SpellType.SUMMON_IN).ActivateState(SpellStateType.BIRTH);
            this.<>f__this.StartCoroutine(this.<>f__this.FinishPhoneMovingCardTile(this.<cardTileObject>__4, this.<movingCardTile>__5, 1f));
        }

        internal void <>m__23B(object val)
        {
            Vector3 position = this.<tile>__2.transform.position;
            this.<newPath>__7[1] = new Vector3((this.<startSpot>__8.x + position.x) * 0.5f, ((this.<startSpot>__8.y + position.y) * 0.5f) + 60f, (this.<startSpot>__8.z + position.z) * 0.5f);
            this.<newPath>__7[2] = position;
            iTween.PutOnPath(this.<cardTileObject>__4, this.<newPath>__7, (float) val);
        }

        internal void <>m__23C(object v)
        {
            this.<tile>__2.Show();
            this.<tile>__2.GetActor().GetSpell(SpellType.SUMMON_IN).ActivateState(SpellStateType.BIRTH);
            this.<movingCardTile>__5.Hide();
            UnityEngine.Object.Destroy(this.<cardTileObject>__4);
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
                    this.<cardID>__0 = this.cardToAnimate.GetEntityDef().GetCardId();
                    this.<cardFlair>__1 = (this.cardToAnimate.GetCardFlair() == null) ? new CardFlair(TAG_PREMIUM.NORMAL) : this.cardToAnimate.GetCardFlair();
                    this.<tile>__2 = this.<>f__this.GetCardTileVisual(this.<cardID>__0, this.<cardFlair>__1.Premium);
                    this.<cardPos>__3 = this.cardToAnimate.transform.position;
                    break;

                case 1:
                    this.<tile>__2 = this.<>f__this.GetCardTileVisual(this.<cardID>__0, this.<cardFlair>__1.Premium);
                    break;

                default:
                    goto Label_040B;
            }
            if (this.<tile>__2 == null)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<cardTileObject>__4 = UnityEngine.Object.Instantiate<GameObject>(this.<tile>__2.GetActor().gameObject);
            this.<movingCardTile>__5 = this.<cardTileObject>__4.GetComponent<Actor>();
            this.<currentDeck>__6 = CollectionManager.Get().GetTaggedDeck(this.<>f__this.m_deckType);
            if (this.<currentDeck>__6.GetCardCount(this.<cardID>__0, this.<cardFlair>__1) == 1)
            {
                this.<tile>__2.Hide();
            }
            else
            {
                this.<tile>__2.Show();
            }
            this.<movingCardTile>__5.transform.position = new Vector3(this.<cardPos>__3.x, this.<cardPos>__3.y + 2.5f, this.<cardPos>__3.z);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.<movingCardTile>__5.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            }
            else
            {
                this.<movingCardTile>__5.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }
            this.<movingCardTile>__5.DeactivateAllSpells();
            this.<movingCardTile>__5.ActivateSpell(SpellType.SUMMON_IN_LARGE);
            if ((UniversalInputManager.UsePhoneUI != null) && (this.<>f__this.m_phoneDeckTileBone != null))
            {
                object[] args = new object[] { "position", this.<>f__this.m_phoneDeckTileBone.transform.position, "time", 0.5f, "easetype", iTween.EaseType.easeInCubic, "oncomplete", new Action<object>(this.<>m__23A) };
                iTween.MoveTo(this.<cardTileObject>__4, iTween.Hash(args));
                object[] objArray2 = new object[] { "scale", new Vector3(0.5f, 1.1f, 1.1f), "time", 0.5f, "easetype", iTween.EaseType.easeInCubic };
                iTween.ScaleTo(this.<cardTileObject>__4, iTween.Hash(objArray2));
            }
            else
            {
                this.<newPath>__7 = new Vector3[3];
                this.<startSpot>__8 = this.<movingCardTile>__5.transform.position;
                this.<newPath>__7[0] = this.<startSpot>__8;
                object[] objArray3 = new object[] { "from", 0f, "to", 1f, "time", 0.75f, "easetype", iTween.EaseType.easeOutCirc, "onupdate", new Action<object>(this.<>m__23B), "oncomplete", new Action<object>(this.<>m__23C) };
                iTween.ValueTo(this.<cardTileObject>__4, iTween.Hash(objArray3));
            }
            SoundManager.Get().LoadAndPlay("collection_manager_card_add_to_deck_instant", this.<>f__this.gameObject);
            this.$PC = -1;
        Label_040B:
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
    private sealed class <ShowDeckCompleteEffectsWithInterval>c__Iterator253 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>interval;
        internal List<DeckTrayDeckTileVisual>.Enumerator <$s_1411>__1;
        internal DeckTrayCardListContent <>f__this;
        internal bool <needScroll>__0;
        internal DeckTrayDeckTileVisual <tile>__2;
        internal float interval;

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
                        this.<$s_1411>__1.Dispose();
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
                    if (this.<>f__this.m_scrollbar != null)
                    {
                        this.<needScroll>__0 = this.<>f__this.m_scrollbar.IsScrollNeeded();
                        if (!this.<needScroll>__0)
                        {
                            break;
                        }
                        this.<>f__this.m_scrollbar.Enable(false);
                        this.<>f__this.m_scrollbar.ForceVisibleAffectedObjectsShow(true);
                        this.<>f__this.m_scrollbar.SetScroll(0f, iTween.EaseType.easeOutSine, 0.25f, true, true);
                        this.$current = new WaitForSeconds(0.3f);
                        this.$PC = 1;
                        goto Label_01E2;
                    }
                    goto Label_01E0;

                case 1:
                    this.<>f__this.m_scrollbar.SetScroll(1f, iTween.EaseType.easeInOutQuart, this.interval * this.<>f__this.m_cardTiles.Count, true, true);
                    break;

                case 2:
                    goto Label_0108;

                default:
                    goto Label_01E0;
            }
            this.<$s_1411>__1 = this.<>f__this.m_cardTiles.GetEnumerator();
            num = 0xfffffffd;
        Label_0108:
            try
            {
                while (this.<$s_1411>__1.MoveNext())
                {
                    this.<tile>__2 = this.<$s_1411>__1.Current;
                    if ((this.<tile>__2 != null) && this.<tile>__2.IsInUse())
                    {
                        this.<tile>__2.GetActor().ActivateSpell(SpellType.SUMMON_IN_FORGE);
                        this.$current = new WaitForSeconds(this.interval);
                        this.$PC = 2;
                        flag = true;
                        goto Label_01E2;
                    }
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_1411>__1.Dispose();
            }
            if (this.<needScroll>__0)
            {
                this.<>f__this.m_scrollbar.ForceVisibleAffectedObjectsShow(false);
                this.<>f__this.m_scrollbar.EnableIfNeeded();
            }
            this.$PC = -1;
        Label_01E0:
            return false;
        Label_01E2:
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

    public delegate void CardCountChanged(int cardCount);

    public delegate void CardTileHeld(DeckTrayDeckTileVisual cardTile);

    public delegate void CardTileOut(DeckTrayDeckTileVisual cardTile);

    public delegate void CardTileOver(DeckTrayDeckTileVisual cardTile);

    public delegate void CardTilePress(DeckTrayDeckTileVisual cardTile);

    public delegate void CardTileRelease(DeckTrayDeckTileVisual cardTile);

    public delegate void CardTileRightClicked(DeckTrayDeckTileVisual cardTile);
}

