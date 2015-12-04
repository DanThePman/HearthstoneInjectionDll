using System;
using UnityEngine;

public class DeckTrayDeckTileVisual : PegUIElement
{
    private readonly Vector3 BOX_COLLIDER_CENTER = new Vector3(-1.4f, 0f, 0f);
    private readonly Vector3 BOX_COLLIDER_SIZE = new Vector3(25.34f, 2.14f, 3.68f);
    protected const int DEFAULT_PORTRAIT_QUALITY = 1;
    public static readonly GameLayer LAYER = GameLayer.CardRaycast;
    protected CollectionDeckTileActor m_actor;
    protected BoxCollider m_collider;
    protected bool m_inArena;
    protected bool m_isInUse;
    protected CollectionDeckSlot m_slot;
    protected bool m_useSliderAnimations;

    protected override void Awake()
    {
        base.Awake();
        string name = (UniversalInputManager.UsePhoneUI == null) ? "DeckCardBar" : "DeckCardBar_phone";
        GameObject obj2 = AssetLoader.Get().LoadActor(name, false, false);
        if (obj2 == null)
        {
            Debug.LogWarning(string.Format("DeckTrayDeckTileVisual.OnDeckTileActorLoaded() - FAILED to load actor \"{0}\"", name));
        }
        else
        {
            this.m_actor = obj2.GetComponent<CollectionDeckTileActor>();
            if (this.m_actor == null)
            {
                Debug.LogWarning(string.Format("DeckTrayDeckTileVisual.OnDeckTileActorLoaded() - ERROR game object \"{0}\" has no CollectionDeckTileActor component", name));
            }
            else
            {
                GameUtils.SetParent((Component) this.m_actor, (Component) this, false);
                this.m_actor.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
                UIBScrollableItem component = this.m_actor.GetComponent<UIBScrollableItem>();
                if (component != null)
                {
                    component.SetCustomActiveState(new UIBScrollableItem.ActiveStateCallback(this.IsInUse));
                }
                this.SetUpActor();
                if (base.gameObject.GetComponent<BoxCollider>() == null)
                {
                    this.m_collider = base.gameObject.AddComponent<BoxCollider>();
                    this.m_collider.size = this.BOX_COLLIDER_SIZE;
                    this.m_collider.center = this.BOX_COLLIDER_CENTER;
                }
                this.Hide();
                SceneUtils.SetLayer(base.gameObject, LAYER);
                base.SetDragTolerance(5f);
            }
        }
    }

    public CollectionDeckTileActor GetActor()
    {
        return this.m_actor;
    }

    public Bounds GetBounds()
    {
        return this.m_collider.bounds;
    }

    public CardFlair GetCardFlair()
    {
        return this.m_actor.GetCardFlair();
    }

    public string GetCardID()
    {
        return this.m_actor.GetEntityDef().GetCardId();
    }

    public CollectionDeckSlot GetSlot()
    {
        return this.m_slot;
    }

    public void Hide()
    {
        base.gameObject.SetActive(false);
    }

    public bool IsInUse()
    {
        return this.m_isInUse;
    }

    public bool IsOwnedSlot()
    {
        return this.m_slot.Owned;
    }

    public void MarkAsUnused()
    {
        this.m_isInUse = false;
        if (this.m_actor != null)
        {
            this.m_actor.UpdateDeckCardProperties(false, 1, false);
        }
    }

    public void MarkAsUsed()
    {
        this.m_isInUse = true;
    }

    public void SetHighlight(bool highlight)
    {
        if (this.m_actor.m_highlight != null)
        {
            this.m_actor.m_highlight.SetActive(highlight);
        }
    }

    public void SetInArena(bool inArena)
    {
        this.m_inArena = inArena;
    }

    public void SetSlot(CollectionDeckSlot s, bool useSliderAnimations)
    {
        this.m_slot = s;
        this.m_useSliderAnimations = useSliderAnimations;
        this.SetUpActor();
    }

    private void SetUpActor()
    {
        if (((this.m_actor != null) && (this.m_slot != null)) && !string.IsNullOrEmpty(this.m_slot.CardID))
        {
            EntityDef entityDef = DefLoader.Get().GetEntityDef(this.m_slot.CardID);
            bool flag = SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER;
            this.m_actor.SetDisablePremiumPortrait(true);
            this.m_actor.SetGhosted(flag && !this.m_slot.Owned);
            this.m_actor.SetEntityDef(entityDef);
            this.m_actor.SetCardFlair(new CardFlair(this.m_slot.Premium));
            bool cardIsUnique = (entityDef != null) ? entityDef.IsElite() : false;
            if ((cardIsUnique && this.m_inArena) && (this.m_slot.Count > 1))
            {
                cardIsUnique = false;
            }
            this.m_actor.UpdateDeckCardProperties(cardIsUnique, this.m_slot.Count, this.m_useSliderAnimations);
            CollectionCardCache.Get().LoadCardDef(entityDef.GetCardId(), delegate (string cardID, CardDef cardDef, object data) {
                if ((this.m_actor != null) && cardID.Equals(this.m_actor.GetEntityDef().GetCardId()))
                {
                    this.m_actor.SetCardDef(cardDef);
                    this.m_actor.UpdateAllComponents();
                    this.m_actor.UpdateMaterial(cardDef.GetDeckCardBarPortrait());
                    this.m_actor.UpdateGhostTileEffect();
                }
            }, null, new CardPortraitQuality(1, this.m_slot.Premium));
        }
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
    }
}

