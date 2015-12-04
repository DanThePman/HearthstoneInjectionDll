using System;
using UnityEngine;

public class CollectionDraggableCardVisual : MonoBehaviour
{
    private static readonly Vector3 CARD_ACTOR_LOCAL_SCALE;
    private static readonly Vector3 DECK_TILE_LOCAL_SCALE;
    private static readonly Vector3 HERO_SKIN_ACTOR_LOCAL_SCALE;
    private Actor m_activeActor;
    private HandActorCache m_actorCache;
    private bool m_actorCacheInit;
    private Actor m_cardActor;
    private string m_cardActorName;
    private Actor m_cardBackActor;
    private int m_cardBackId;
    private CardDef m_cardDef;
    public DragRotatorInfo m_CardDragRotatorInfo;
    private CardFlair m_cardFlair;
    private CardBack m_currentCardBack;
    private CollectionDeckTileActor m_deckTile;
    private DeckTrayDeckTileVisual m_deckTileToRemove;
    private EntityDef m_entityDef;
    private CollectionDeckSlot m_slot;
    private CollectionManagerDisplay.ViewMode m_visualType;

    static CollectionDraggableCardVisual()
    {
        PlatformDependentValue<Vector3> value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(0.6f, 0.6f, 0.6f),
            Phone = new Vector3(0.9f, 0.9f, 0.9f)
        };
        DECK_TILE_LOCAL_SCALE = (Vector3) value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(6f, 6f, 6f),
            Phone = new Vector3(6.9f, 6.9f, 6.9f)
        };
        CARD_ACTOR_LOCAL_SCALE = (Vector3) value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(7.5f, 7.5f, 7.5f),
            Phone = new Vector3(8.2f, 8.2f, 8.2f)
        };
        HERO_SKIN_ACTOR_LOCAL_SCALE = (Vector3) value2;
    }

    public CollectionDraggableCardVisual()
    {
        DragRotatorInfo info = new DragRotatorInfo();
        DragRotatorAxisInfo info2 = new DragRotatorAxisInfo {
            m_ForceMultiplier = 3f,
            m_MinDegrees = -55f,
            m_MaxDegrees = 55f,
            m_RestSeconds = 2f
        };
        info.m_PitchInfo = info2;
        info2 = new DragRotatorAxisInfo {
            m_ForceMultiplier = 4.5f,
            m_MinDegrees = -60f,
            m_MaxDegrees = 60f,
            m_RestSeconds = 2f
        };
        info.m_RollInfo = info2;
        this.m_CardDragRotatorInfo = info;
        this.m_actorCache = new HandActorCache();
    }

    private void Awake()
    {
        base.gameObject.SetActive(false);
        this.LoadDeckTile();
        this.LoadCardBack();
        if (base.gameObject.GetComponent<AudioSource>() == null)
        {
            base.gameObject.AddComponent<AudioSource>();
        }
    }

    public bool ChangeActor(Actor actor, CollectionManagerDisplay.ViewMode vtype)
    {
        if (!this.m_actorCacheInit)
        {
            this.m_actorCacheInit = true;
            this.m_actorCache.AddActorLoadedListener(new HandActorCache.ActorLoadedCallback(this.OnCardActorLoaded));
            this.m_actorCache.Initialize();
        }
        if (!this.m_actorCache.IsInitializing())
        {
            this.m_visualType = vtype;
            if (this.m_visualType != CollectionManagerDisplay.ViewMode.CARD_BACKS)
            {
                EntityDef entityDef = actor.GetEntityDef();
                CardFlair cardFlair = actor.GetCardFlair();
                bool flag = entityDef != this.m_entityDef;
                bool flag2 = !cardFlair.Equals(this.m_cardFlair);
                if (flag || flag2)
                {
                    this.m_entityDef = entityDef;
                    this.m_cardFlair = cardFlair;
                    this.m_cardActor = this.m_actorCache.GetActor(entityDef, cardFlair);
                    if (this.m_cardActor == null)
                    {
                        return false;
                    }
                    if (flag)
                    {
                        CollectionCardCache.Get().LoadCardDef(this.m_entityDef.GetCardId(), new CollectionCardCache.LoadCardDefCallback(this.OnCardDefLoaded), new CardPortraitQuality(1, this.m_cardFlair.Premium), null);
                    }
                    else
                    {
                        this.InitDeckTileActor();
                        this.InitCardActor();
                    }
                }
                return true;
            }
            if (actor != null)
            {
                this.m_entityDef = null;
                this.m_cardFlair = null;
                this.m_currentCardBack = actor.GetComponentInChildren<CardBack>();
                this.m_cardActor = this.m_cardBackActor;
                this.m_cardBackActor.SetCardbackUpdateIgnore(true);
                return true;
            }
        }
        return false;
    }

    public int GetCardBackId()
    {
        return this.m_cardBackId;
    }

    public CardFlair GetCardFlair()
    {
        return this.m_cardFlair;
    }

    public string GetCardID()
    {
        return this.m_entityDef.GetCardId();
    }

    public DeckTrayDeckTileVisual GetDeckTileToRemove()
    {
        return this.m_deckTileToRemove;
    }

    public EntityDef GetEntityDef()
    {
        return this.m_entityDef;
    }

    public CollectionDeckSlot GetSlot()
    {
        return this.m_slot;
    }

    public CollectionManagerDisplay.ViewMode GetVisualType()
    {
        return this.m_visualType;
    }

    public void Hide()
    {
        base.gameObject.SetActive(false);
        if (this.m_activeActor != null)
        {
            this.m_activeActor.Hide();
            this.m_activeActor.gameObject.SetActive(false);
            this.m_activeActor = null;
        }
    }

    private void InitActor(Actor actor)
    {
        actor.SetEntityDef(this.m_entityDef);
        actor.SetCardDef(this.m_cardDef);
        actor.SetCardFlair(this.m_cardFlair);
        actor.UpdateAllComponents();
    }

    private void InitCardActor()
    {
        this.InitActor(this.m_cardActor);
        this.m_cardActor.transform.localRotation = Quaternion.identity;
    }

    private void InitDeckTileActor()
    {
        this.InitActor(this.m_deckTile);
        this.m_deckTile.SetCardDef(this.m_cardDef);
        this.m_deckTile.SetDisablePremiumPortrait(true);
        this.m_deckTile.UpdateAllComponents();
        this.m_deckTile.UpdateMaterial(this.m_cardDef.GetDeckCardBarPortrait());
        this.m_deckTile.UpdateDeckCardProperties(this.m_entityDef.IsElite(), 1, false);
    }

    public bool IsShown()
    {
        return base.gameObject.activeSelf;
    }

    private void LoadCardBack()
    {
        GameObject child = AssetLoader.Get().LoadActor("Card_Hidden", false, false);
        GameUtils.SetParent(child, this, false);
        this.m_cardBackActor = child.GetComponent<Actor>();
        this.m_cardBackActor.transform.localScale = CARD_ACTOR_LOCAL_SCALE;
        this.m_cardBackActor.TurnOffCollider();
        this.m_cardBackActor.Hide();
        child.AddComponent<DragRotator>().SetInfo(this.m_CardDragRotatorInfo);
    }

    private void LoadDeckTile()
    {
        GameObject obj2 = AssetLoader.Get().LoadActor("DeckCardBar", false, false);
        if (obj2 == null)
        {
            Debug.LogWarning(string.Format("CollectionDraggableCardVisual.OnDeckTileActorLoaded() - FAILED to load actor \"{0}\"", "DeckCardBar"));
        }
        else
        {
            this.m_deckTile = obj2.GetComponent<CollectionDeckTileActor>();
            if (this.m_deckTile == null)
            {
                Debug.LogWarning(string.Format("CollectionDraggableCardVisual.OnDeckTileActorLoaded() - ERROR game object \"{0}\" has no CollectionDeckTileActor component", "DeckCardBar"));
            }
            else
            {
                this.m_deckTile.Hide();
                this.m_deckTile.transform.parent = base.transform;
                this.m_deckTile.transform.localPosition = new Vector3(2.194931f, 0f, 0f);
                this.m_deckTile.transform.localScale = DECK_TILE_LOCAL_SCALE;
                this.m_deckTile.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            }
        }
    }

    private void OnCardActorLoaded(string name, Actor actor, object callbackData)
    {
        if (actor == null)
        {
            Debug.LogWarning(string.Format("CollectionDraggableCardVisual.OnCardActorLoaded() - FAILED to load {0}", name));
        }
        else
        {
            actor.GetType();
            actor.TurnOffCollider();
            actor.Hide();
            if (name == "Card_Hero_Skin")
            {
                actor.transform.localScale = HERO_SKIN_ACTOR_LOCAL_SCALE;
            }
            else
            {
                actor.transform.localScale = CARD_ACTOR_LOCAL_SCALE;
            }
            actor.transform.parent = base.transform;
            actor.transform.localPosition = Vector3.zero;
            actor.gameObject.AddComponent<DragRotator>().SetInfo(this.m_CardDragRotatorInfo);
        }
    }

    private void OnCardDefLoaded(string cardID, CardDef cardDef, object callbackData)
    {
        if ((this.m_entityDef != null) && (this.m_entityDef.GetCardId() == cardID))
        {
            this.m_cardDef = cardDef;
            this.InitDeckTileActor();
            this.InitCardActor();
        }
    }

    public void SetCardBackId(int cardBackId)
    {
        this.m_cardBackId = cardBackId;
    }

    public void SetSlot(CollectionDeckSlot slot)
    {
        this.m_slot = slot;
    }

    public void Show(bool isOverDeck)
    {
        base.gameObject.SetActive(true);
        this.UpdateVisual(isOverDeck);
        if ((this.m_deckTile != null) && (this.m_entityDef != null))
        {
            this.m_deckTile.UpdateDeckCardProperties(this.m_entityDef.IsElite(), 1, false);
        }
    }

    private void Update()
    {
        this.m_deckTileToRemove = null;
        if (this.m_activeActor == this.m_deckTile)
        {
            RaycastHit hit;
            CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
            if (((taggedDeck != null) && (taggedDeck.GetTotalCardCount() == 30)) && UniversalInputManager.Get().GetInputHitInfo(DeckTrayDeckTileVisual.LAYER.LayerBit(), out hit))
            {
                DeckTrayDeckTileVisual component = hit.collider.gameObject.GetComponent<DeckTrayDeckTileVisual>();
                if ((component != null) && (component != this.m_deckTileToRemove))
                {
                    this.m_deckTileToRemove = component;
                }
            }
        }
    }

    public void UpdateVisual(bool isOverDeck)
    {
        Actor activeActor = this.m_activeActor;
        SpellType nONE = SpellType.NONE;
        if (this.m_visualType == CollectionManagerDisplay.ViewMode.CARDS)
        {
            if ((isOverDeck && (this.m_entityDef != null)) && !this.m_entityDef.IsHero())
            {
                this.m_activeActor = this.m_deckTile;
                nONE = SpellType.SUMMON_IN;
            }
            else
            {
                this.m_activeActor = this.m_cardActor;
                nONE = SpellType.DEATHREVERSE;
            }
        }
        else
        {
            this.m_activeActor = this.m_cardActor;
            nONE = SpellType.DEATHREVERSE;
        }
        if (activeActor != this.m_activeActor)
        {
            if (activeActor != null)
            {
                activeActor.Hide();
                activeActor.gameObject.SetActive(false);
            }
            if (this.m_activeActor != null)
            {
                this.m_activeActor.gameObject.SetActive(true);
                this.m_activeActor.Show();
                if ((this.m_visualType == CollectionManagerDisplay.ViewMode.CARD_BACKS) && (this.m_currentCardBack != null))
                {
                    CardBackManager.Get().UpdateCardBack(this.m_activeActor, this.m_currentCardBack);
                }
                Spell spell = this.m_activeActor.GetSpell(nONE);
                if (spell != null)
                {
                    spell.ActivateState(SpellStateType.BIRTH);
                }
                if ((this.m_entityDef != null) && this.m_entityDef.IsHero())
                {
                    CollectionHeroSkin component = this.m_activeActor.gameObject.GetComponent<CollectionHeroSkin>();
                    component.SetClass(this.m_entityDef.GetClass());
                    component.ShowSocketFX();
                }
            }
        }
    }
}

