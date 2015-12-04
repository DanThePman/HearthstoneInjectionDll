using System;
using UnityEngine;

public class CollectionInputMgr : MonoBehaviour
{
    private bool m_cardsDraggable;
    private Vector3 m_heldCardScreenSpace;
    public CollectionDraggableCardVisual m_heldCardVisual;
    private bool m_mouseIsOverDeck;
    private UIBScrollable m_scrollBar;
    private bool m_showingDeckTile;
    private static CollectionInputMgr s_instance;
    public Collider TooltipPlane;

    private void Awake()
    {
        s_instance = this;
        UniversalInputManager.Get().RegisterMouseOnOrOffScreenListener(new UniversalInputManager.MouseOnOrOffScreenCallback(this.OnMouseOnOrOffScreen));
    }

    private bool CanGrabItem(Actor actor)
    {
        if (this.IsDraggingScrollbar())
        {
            return false;
        }
        if (this.m_heldCardVisual.IsShown())
        {
            return false;
        }
        if (actor == null)
        {
            return false;
        }
        return true;
    }

    public void DropCard(DeckTrayDeckTileVisual deckTileToRemove)
    {
        this.DropCard(false, deckTileToRemove);
    }

    private void DropCard(bool dragCanceled, DeckTrayDeckTileVisual deckTileToRemove)
    {
        PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
        if (!dragCanceled)
        {
            if (!this.m_mouseIsOverDeck)
            {
                SoundManager.Get().LoadAndPlay("collection_manager_drop_card", this.m_heldCardVisual.gameObject);
            }
            else
            {
                switch (this.m_heldCardVisual.GetVisualType())
                {
                    case CollectionManagerDisplay.ViewMode.CARDS:
                        CollectionDeckTray.Get().AddCard(this.m_heldCardVisual.GetEntityDef(), this.m_heldCardVisual.GetCardFlair(), deckTileToRemove, true, null);
                        break;

                    case CollectionManagerDisplay.ViewMode.HERO_SKINS:
                        CollectionDeckTray.Get().GetHeroSkinContent().UpdateHeroSkin(this.m_heldCardVisual.GetEntityDef(), this.m_heldCardVisual.GetCardFlair(), true);
                        break;

                    case CollectionManagerDisplay.ViewMode.CARD_BACKS:
                    {
                        object cardBackId = this.m_heldCardVisual.GetCardBackId();
                        if (cardBackId != null)
                        {
                            CollectionDeckTray.Get().GetCardBackContent().UpdateCardBack((int) cardBackId, true, null);
                            break;
                        }
                        Debug.LogWarning("Cardback ID not set for dragging card back.");
                        break;
                    }
                }
            }
        }
        this.m_heldCardVisual.Hide();
        this.m_scrollBar.Pause(false);
    }

    public static CollectionInputMgr Get()
    {
        return s_instance;
    }

    public bool GrabCard(CollectionCardVisual cardVisual)
    {
        Actor actor = cardVisual.GetActor();
        if (!this.CanGrabItem(actor))
        {
            return false;
        }
        if (!this.m_heldCardVisual.ChangeActor(actor, cardVisual.GetVisualType()))
        {
            return false;
        }
        this.m_scrollBar.Pause(true);
        PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
        CollectionCardBack component = actor.GetComponent<CollectionCardBack>();
        this.m_heldCardVisual.SetSlot(null);
        if (component != null)
        {
            this.m_heldCardVisual.SetCardBackId(component.GetCardBackId());
        }
        this.m_heldCardVisual.transform.position = actor.transform.position;
        this.m_heldCardVisual.Show(this.m_mouseIsOverDeck);
        SoundManager.Get().LoadAndPlay("collection_manager_pick_up_card", this.m_heldCardVisual.gameObject);
        return true;
    }

    public bool GrabCard(DeckTrayDeckTileVisual deckTileVisual)
    {
        Actor actor = deckTileVisual.GetActor();
        if (!this.CanGrabItem(actor))
        {
            return false;
        }
        if (!this.m_heldCardVisual.ChangeActor(actor, CollectionManagerDisplay.ViewMode.CARDS))
        {
            return false;
        }
        this.m_scrollBar.Pause(true);
        PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
        this.m_heldCardVisual.SetSlot(deckTileVisual.GetSlot());
        this.m_heldCardVisual.transform.position = actor.transform.position;
        this.m_heldCardVisual.Show(this.m_mouseIsOverDeck);
        SoundManager.Get().LoadAndPlay("collection_manager_pick_up_card", this.m_heldCardVisual.gameObject);
        CollectionDeckTray.Get().RemoveCard(this.m_heldCardVisual.GetCardID(), this.m_heldCardVisual.GetCardFlair(), deckTileVisual.IsOwnedSlot());
        return true;
    }

    public bool HandleKeyboardInput()
    {
        if ((Input.GetKeyUp(KeyCode.Escape) && (CraftingManager.Get() != null)) && CraftingManager.Get().IsCardShowing())
        {
            Navigation.GoBack();
            return true;
        }
        return false;
    }

    public bool HasHeldCard()
    {
        return this.m_heldCardVisual.IsShown();
    }

    public bool IsDraggingScrollbar()
    {
        return ((this.m_scrollBar != null) && this.m_scrollBar.IsDragging());
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnMouseOnOrOffScreen(bool onScreen)
    {
        if (!onScreen && this.m_heldCardVisual.IsShown())
        {
            this.DropCard(true, null);
            CollectionDeckTray.Get().GetDeckBigCard().ForceHide();
        }
    }

    public void SetScrollbar(UIBScrollable scrollbar)
    {
        this.m_scrollBar = scrollbar;
    }

    private void Start()
    {
    }

    public void Unload()
    {
        UniversalInputManager.Get().UnregisterMouseOnOrOffScreenListener(new UniversalInputManager.MouseOnOrOffScreenCallback(this.OnMouseOnOrOffScreen));
    }

    private void Update()
    {
        this.UpdateHeldCard();
    }

    private void UpdateHeldCard()
    {
        RaycastHit hit;
        if (this.m_heldCardVisual.IsShown() && UniversalInputManager.Get().GetInputHitInfo(GameLayer.DragPlane.LayerBit(), out hit))
        {
            this.m_heldCardVisual.transform.position = hit.point;
            this.m_mouseIsOverDeck = CollectionDeckTray.Get().MouseIsOver();
            this.m_heldCardVisual.UpdateVisual(this.m_mouseIsOverDeck);
            if (Input.GetMouseButtonUp(0))
            {
                this.DropCard(this.m_heldCardVisual.GetDeckTileToRemove());
            }
        }
    }
}

