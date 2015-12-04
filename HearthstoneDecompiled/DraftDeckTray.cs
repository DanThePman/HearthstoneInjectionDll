using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class DraftDeckTray : MonoBehaviour
{
    public DeckTrayCardListContent m_cardsContent;
    public UberText m_countLabelText;
    public UberText m_countText;
    public DeckBigCard m_deckBigCard;
    public TooltipZone m_deckHeaderTooltip;
    public GameObject m_headerLabel;
    public UIBScrollable m_scrollbar;
    private static DraftDeckTray s_instance;

    public void AddCard(string cardID, Actor animateFromActor = null)
    {
        this.m_cardsContent.UpdateCardList(cardID, (bool) animateFromActor, null);
    }

    private void Awake()
    {
        s_instance = this;
        if (this.m_scrollbar != null)
        {
            this.m_scrollbar.Enable(false);
            this.m_scrollbar.AddTouchScrollStartedListener(new UIBScrollable.OnTouchScrollStarted(this.OnTouchScrollStarted));
        }
        this.m_cardsContent.SetInArena(true);
        this.m_cardsContent.RegisterCardTilePressListener(new DeckTrayCardListContent.CardTilePress(this.OnCardTilePress));
        this.m_cardsContent.RegisterCardTileOverListener(new DeckTrayCardListContent.CardTileOver(this.OnCardTileOver));
        this.m_cardsContent.RegisterCardTileOutListener(new DeckTrayCardListContent.CardTileOut(this.OnCardTileOut));
        this.m_cardsContent.RegisterCardTileReleaseListener(new DeckTrayCardListContent.CardTileRelease(this.OnCardTileRelease));
        this.m_cardsContent.RegisterCardCountUpdated(new DeckTrayCardListContent.CardCountChanged(this.OnCardCountUpdated));
        DraftManager.Get().RegisterDraftDeckSetListener(new DraftManager.DraftDeckSet(this.OnDraftDeckInitialized));
    }

    [DebuggerHidden]
    private IEnumerator DelayCardCountUpdate(string count)
    {
        return new <DelayCardCountUpdate>c__Iterator63 { count = count, <$>count = count, <>f__this = this };
    }

    public static DraftDeckTray Get()
    {
        return s_instance;
    }

    public DeckTrayCardListContent GetCardsContent()
    {
        return this.m_cardsContent;
    }

    public TooltipZone GetTooltipZone()
    {
        return this.m_deckHeaderTooltip;
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

    public void Initialize()
    {
        CollectionDeck draftDeck = DraftManager.Get().GetDraftDeck();
        if (draftDeck != null)
        {
            this.OnDraftDeckInitialized(draftDeck);
        }
    }

    public bool MouseIsOver()
    {
        return UniversalInputManager.Get().InputIsOver(base.gameObject);
    }

    private void OnCardCountUpdated(int cardCount)
    {
        string str = string.Empty;
        string count = string.Empty;
        if (cardCount > 0)
        {
            if (this.m_headerLabel != null)
            {
                this.m_headerLabel.SetActive(true);
            }
            if (cardCount < 30)
            {
                str = GameStrings.Get("GLUE_DECK_TRAY_CARD_COUNT_LABEL");
                object[] args = new object[] { cardCount, 30 };
                count = GameStrings.Format("GLUE_DECK_TRAY_COUNT", args);
            }
        }
        this.m_countLabelText.Text = str;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            base.StartCoroutine(this.DelayCardCountUpdate(count));
        }
        else
        {
            this.m_countText.Text = count;
        }
    }

    private void OnCardTileOut(DeckTrayDeckTileVisual cardTile)
    {
        this.HideDeckBigCard(cardTile, false);
    }

    private void OnCardTileOver(DeckTrayDeckTileVisual cardTile)
    {
        if (!UniversalInputManager.Get().IsTouchMode())
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
        if (UniversalInputManager.Get().IsTouchMode())
        {
            this.HideDeckBigCard(cardTile, false);
        }
    }

    private void OnDestroy()
    {
        DraftManager.Get().RemoveDraftDeckSetListener(new DraftManager.DraftDeckSet(this.OnDraftDeckInitialized));
        CollectionManager.Get().ClearTaggedDeck(CollectionManager.DeckTag.Arena);
        s_instance = null;
    }

    private void OnDraftDeckInitialized(CollectionDeck draftDeck)
    {
        if (draftDeck == null)
        {
            UnityEngine.Debug.LogError("Draft deck is null.");
        }
        else
        {
            CollectionManager.Get().SetTaggedDeck(CollectionManager.DeckTag.Arena, draftDeck, null);
            this.OnCardCountUpdated(draftDeck.GetTotalCardCount());
            this.m_cardsContent.UpdateCardList(true, null);
        }
    }

    private void OnTouchScrollStarted()
    {
        if (this.m_deckBigCard != null)
        {
            this.m_deckBigCard.ForceHide();
        }
    }

    private void ShowDeckBigCard(DeckTrayDeckTileVisual cardTile, float delay = 0)
    {
        CollectionDeckTileActor actor = cardTile.GetActor();
        if (this.m_deckBigCard != null)
        {
            EntityDef entityDef = actor.GetEntityDef();
            CardDef cardDef = CollectionCardCache.Get().GetCardDef(entityDef.GetCardId());
            this.m_deckBigCard.Show(entityDef, actor.GetCardFlair(), cardDef, actor.gameObject.transform.position, false, delay);
            if (UniversalInputManager.Get().IsTouchMode())
            {
                cardTile.SetHighlight(true);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <DelayCardCountUpdate>c__Iterator63 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>count;
        internal DraftDeckTray <>f__this;
        internal string count;

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
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_countText.Text = this.count;
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
}

