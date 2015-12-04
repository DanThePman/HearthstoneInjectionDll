using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditClass]
public class CollectionCardVisual : PegUIElement
{
    private const string ADD_CARD_TO_DECK_SOUND = "collection_manager_card_add_to_deck_instant";
    private const string CARD_LIMIT_LOCK_SOUND = "card_limit_lock";
    private const string CARD_LIMIT_UNLOCK_SOUND = "card_limit_unlock";
    private const string CARD_MOUSE_OVER_SOUND = "collection_manager_card_mouse_over";
    private const string CARD_MOVE_INVALID_OR_CLICK_SOUND = "collection_manager_card_move_invalid_or_click";
    private Actor m_actor;
    public Vector3 m_boxColliderCenter = new Vector3(0f, 0.14f, 0f);
    public Vector3 m_boxColliderSize = new Vector3(2f, 0.21f, 2.7f);
    public CollectionCardCount m_cardCount;
    public CollectionCardLock m_cardLock;
    private int m_cmRow;
    public Vector3 m_heroSkinBoxColliderCenter = new Vector3(0f, 0.14f, -0.58f);
    public Vector3 m_heroSkinBoxColliderSize = new Vector3(2f, 0.21f, 2f);
    private LockType m_lockType;
    public GameObject m_newCardCallout;
    private Vector3 m_originalPosition;
    private bool m_shown;
    private CollectionManagerDisplay.ViewMode m_visualType;

    protected override void Awake()
    {
        base.Awake();
        if (base.gameObject.GetComponent<AudioSource>() == null)
        {
            base.gameObject.AddComponent<AudioSource>();
        }
        base.SetDragTolerance(5f);
        SoundManager.Get().Load("collection_manager_card_add_to_deck_instant");
    }

    private bool CanPickUpCard()
    {
        if (this.ShouldIgnoreAllInput())
        {
            return false;
        }
        if (CollectionManagerDisplay.Get().GetViewMode() != this.m_visualType)
        {
            return false;
        }
        if (!CollectionDeckTray.Get().CanPickupCard())
        {
            return false;
        }
        if (this.m_visualType == CollectionManagerDisplay.ViewMode.CARDS)
        {
            if (!this.IsInCollection())
            {
                return false;
            }
            if (!this.IsUnlocked())
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckCardSeen()
    {
        bool flag = this.m_actor.GetActorStateMgr().GetActiveStateType() == ActorStateType.CARD_RECENTLY_ACQUIRED;
        if (flag)
        {
            EntityDef entityDef = this.m_actor.GetEntityDef();
            if (entityDef != null)
            {
                CollectionManager.Get().MarkAllInstancesAsSeen(entityDef.GetCardId(), this.m_actor.GetCardFlair());
            }
        }
        return flag;
    }

    private void EnterCraftingMode()
    {
        CollectionManagerDisplay.ViewMode viewMode = CollectionManagerDisplay.Get().GetViewMode();
        if (this.m_visualType == viewMode)
        {
            switch (viewMode)
            {
                case CollectionManagerDisplay.ViewMode.CARDS:
                    if (CraftingManager.Get() != null)
                    {
                        CraftingManager.Get().EnterCraftMode(this);
                    }
                    break;

                case CollectionManagerDisplay.ViewMode.HERO_SKINS:
                {
                    HeroSkinInfoManager manager2 = HeroSkinInfoManager.Get();
                    if (manager2 != null)
                    {
                        manager2.EnterPreview(this);
                    }
                    break;
                }
                case CollectionManagerDisplay.ViewMode.CARD_BACKS:
                {
                    CardBackInfoManager manager3 = CardBackInfoManager.Get();
                    if (manager3 != null)
                    {
                        manager3.EnterPreview(this);
                    }
                    break;
                }
            }
            CollectionDeckTray.Get().CancelRenamingDeck();
        }
    }

    public Actor GetActor()
    {
        return this.m_actor;
    }

    public CollectionCardCount GetCardCountObject()
    {
        return this.m_cardCount;
    }

    public int GetCMRow()
    {
        return this.m_cmRow;
    }

    public CollectionManagerDisplay.ViewMode GetVisualType()
    {
        return this.m_visualType;
    }

    public void Hide()
    {
        this.m_shown = false;
        base.SetEnabled(false);
        base.GetComponent<Collider>().enabled = false;
        if (this.m_cardCount != null)
        {
            this.m_cardCount.Hide();
        }
        this.ShowLock(LockType.NONE);
        this.ShowNewItemCallout(false);
        if (this.m_actor != null)
        {
            this.m_actor.Hide();
        }
    }

    private bool IsInCollection()
    {
        if (this.m_cardCount == null)
        {
            return false;
        }
        return (this.m_cardCount.GetCount() > 0);
    }

    public bool IsShown()
    {
        return this.m_shown;
    }

    private bool IsTransactionPendingOnThisCard()
    {
        CraftingManager manager = CraftingManager.Get();
        if (manager == null)
        {
            return false;
        }
        PendingTransaction pendingTransaction = manager.GetPendingTransaction();
        if (pendingTransaction == null)
        {
            return false;
        }
        EntityDef entityDef = this.m_actor.GetEntityDef();
        if (entityDef == null)
        {
            return false;
        }
        if (pendingTransaction.CardID != entityDef.GetCardId())
        {
            return false;
        }
        if (pendingTransaction.cardFlair != this.m_actor.GetCardFlair())
        {
            return false;
        }
        return true;
    }

    private bool IsUnlocked()
    {
        return (this.m_lockType == LockType.NONE);
    }

    public void OnDoneCrafting()
    {
        this.UpdateCardCount();
    }

    protected override void OnHold()
    {
        if (this.CanPickUpCard())
        {
            CollectionInputMgr.Get().GrabCard(this);
        }
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        KeywordHelpPanelManager.Get().HideKeywordHelp();
        if (!this.ShouldIgnoreAllInput() && this.IsInCollection())
        {
            this.CheckCardSeen();
            this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
            this.ShowNewItemCallout(false);
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        if (!this.ShouldIgnoreAllInput())
        {
            EntityDef entityDef = this.m_actor.GetEntityDef();
            if (entityDef != null)
            {
                KeywordHelpPanelManager.Get().UpdateKeywordHelpForCollectionManager(entityDef, this.m_actor, this.m_cmRow > 0);
            }
            SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over", base.gameObject);
            if (this.IsInCollection())
            {
                ActorStateType stateType = ActorStateType.CARD_MOUSE_OVER;
                if (this.CheckCardSeen())
                {
                    stateType = ActorStateType.CARD_RECENTLY_ACQUIRED_MOUSE_OVER;
                }
                this.m_actor.SetActorState(stateType);
            }
        }
    }

    protected override void OnRelease()
    {
        if (!this.IsTransactionPendingOnThisCard())
        {
            if (UniversalInputManager.Get().IsTouchMode() || ((CraftingTray.Get() != null) && CraftingTray.Get().IsShown()))
            {
                this.CheckCardSeen();
                this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
                this.EnterCraftingMode();
            }
            else
            {
                Spell spell = this.m_actor.GetSpell(SpellType.DEATHREVERSE);
                if (!this.CanPickUpCard())
                {
                    SoundManager.Get().LoadAndPlay("collection_manager_card_move_invalid_or_click");
                    if (spell != null)
                    {
                        spell.ActivateState(SpellStateType.BIRTH);
                    }
                    EntityDef entityDef = this.m_actor.GetEntityDef();
                    bool isHero = (entityDef != null) && entityDef.IsHero();
                    CollectionManagerDisplay.Get().ShowInnkeeeprLClickHelp(isHero);
                }
                else if (this.m_visualType == CollectionManagerDisplay.ViewMode.CARDS)
                {
                    EntityDef cardEntityDef = this.m_actor.GetEntityDef();
                    if (cardEntityDef != null)
                    {
                        if (spell != null)
                        {
                            spell.ActivateState(SpellStateType.BIRTH);
                        }
                        CollectionDeckTray.Get().AddCard(cardEntityDef, this.m_actor.GetCardFlair(), null, false, this.m_actor);
                    }
                }
                else if (this.m_visualType == CollectionManagerDisplay.ViewMode.CARD_BACKS)
                {
                    CollectionDeckTray.Get().SetCardBack(this.m_actor);
                }
                else if (this.m_visualType == CollectionManagerDisplay.ViewMode.HERO_SKINS)
                {
                    CollectionDeckTray.Get().SetHeroSkin(this.m_actor);
                }
            }
        }
    }

    protected override void OnRightClick()
    {
        if (!this.IsTransactionPendingOnThisCard())
        {
            if (!Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, false))
            {
                Options.Get().SetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, true);
            }
            this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
            this.EnterCraftingMode();
        }
    }

    public void SetActor(Actor actor, CollectionManagerDisplay.ViewMode type = 0)
    {
        if ((this.m_actor != null) && (this.m_actor.transform.parent == base.transform))
        {
            this.m_actor.Hide();
        }
        this.m_visualType = type;
        this.m_actor = actor;
        this.UpdateCardCount();
        if (this.m_actor != null)
        {
            GameUtils.SetParent((Component) actor, (Component) this, false);
            ActorStateType activeStateType = this.m_actor.GetActorStateMgr().GetActiveStateType();
            this.ShowNewItemCallout(activeStateType == ActorStateType.CARD_RECENTLY_ACQUIRED);
        }
    }

    public void SetCMRow(int rowNum)
    {
        this.m_cmRow = rowNum;
    }

    public void SetDefaultBoxCollider()
    {
        BoxCollider component = base.GetComponent<BoxCollider>();
        component.center = this.m_boxColliderCenter;
        component.size = this.m_boxColliderSize;
    }

    public void SetHeroSkinBoxCollider()
    {
        BoxCollider component = base.GetComponent<BoxCollider>();
        component.center = this.m_heroSkinBoxColliderCenter;
        component.size = this.m_heroSkinBoxColliderSize;
    }

    private bool ShouldIgnoreAllInput()
    {
        return (((CollectionInputMgr.Get() != null) && CollectionInputMgr.Get().IsDraggingScrollbar()) || ((CraftingManager.Get() != null) && CraftingManager.Get().IsCardShowing()));
    }

    public void Show()
    {
        this.m_shown = true;
        base.SetEnabled(true);
        base.GetComponent<Collider>().enabled = true;
        if (this.m_actor != null)
        {
            bool show = false;
            if (this.m_visualType == CollectionManagerDisplay.ViewMode.CARDS)
            {
                string cardId = this.m_actor.GetEntityDef().GetCardId();
                CardFlair cardFlair = this.m_actor.GetCardFlair();
                show = CollectionManagerDisplay.Get().ShouldShowNewCardGlow(cardId, cardFlair);
                if (!show)
                {
                    CollectionManager.Get().MarkAllInstancesAsSeen(cardId, cardFlair);
                }
            }
            else if ((this.m_visualType != CollectionManagerDisplay.ViewMode.HERO_SKINS) && (this.m_visualType == CollectionManagerDisplay.ViewMode.CARD_BACKS))
            {
            }
            this.ShowNewItemCallout(show);
            this.m_actor.Show();
            ActorStateType stateType = !show ? ActorStateType.CARD_IDLE : ActorStateType.CARD_RECENTLY_ACQUIRED;
            this.m_actor.SetActorState(stateType);
            Renderer[] componentsInChildren = this.m_actor.gameObject.GetComponentsInChildren<Renderer>();
            if (componentsInChildren != null)
            {
                foreach (Renderer renderer in componentsInChildren)
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            EntityDef entityDef = this.m_actor.GetEntityDef();
            if (entityDef != null)
            {
                string tag = "FakeShadow";
                string str3 = "FakeShadowUnique";
                GameObject obj2 = SceneUtils.FindChildByTag(this.m_actor.gameObject, tag);
                GameObject obj3 = SceneUtils.FindChildByTag(this.m_actor.gameObject, str3);
                if (CollectionManager.Get().IsCardInCollection(entityDef.GetCardId(), this.m_actor.GetCardFlair()))
                {
                    if (entityDef.IsElite())
                    {
                        if (obj2 != null)
                        {
                            obj2.GetComponent<Renderer>().enabled = false;
                        }
                        if (obj3 != null)
                        {
                            obj3.GetComponent<Renderer>().enabled = true;
                        }
                    }
                    else
                    {
                        if (obj2 != null)
                        {
                            obj2.GetComponent<Renderer>().enabled = true;
                        }
                        if (obj3 != null)
                        {
                            obj3.GetComponent<Renderer>().enabled = false;
                        }
                    }
                }
                else
                {
                    if (obj2 != null)
                    {
                        obj2.GetComponent<Renderer>().enabled = false;
                    }
                    if (obj3 != null)
                    {
                        obj3.GetComponent<Renderer>().enabled = false;
                    }
                }
            }
        }
    }

    public void ShowLock(LockType type)
    {
        this.ShowLock(type, false);
    }

    public void ShowLock(LockType type, bool playSound)
    {
        if (this.m_lockType != type)
        {
            this.m_lockType = type;
            this.UpdateCardCountVisibility();
            if (this.m_cardLock != null)
            {
                if (this.m_actor != null)
                {
                    this.m_cardLock.UpdateLockVisual(this.m_actor.GetEntityDef(), type);
                }
                else
                {
                    this.m_cardLock.UpdateLockVisual(null, LockType.NONE);
                }
            }
            if (playSound)
            {
                if (type == LockType.NONE)
                {
                    SoundManager.Get().LoadAndPlay("card_limit_unlock");
                }
                else
                {
                    SoundManager.Get().LoadAndPlay("card_limit_lock");
                }
            }
        }
    }

    public void ShowNewItemCallout(bool show)
    {
        if (this.m_newCardCallout != null)
        {
            this.m_newCardCallout.SetActive(show);
        }
    }

    private void UpdateCardCount()
    {
        if (this.m_cardCount != null)
        {
            int cardCount = 0;
            if (this.m_actor != null)
            {
                EntityDef entityDef = this.m_actor.GetEntityDef();
                if (entityDef != null)
                {
                    cardCount = CollectionManager.Get().GetCollectionArtStack(entityDef.GetCardId(), this.m_actor.GetCardFlair()).Count;
                }
            }
            this.m_cardCount.SetCount(cardCount);
            this.UpdateCardCountVisibility();
        }
    }

    private void UpdateCardCountVisibility()
    {
        if (this.m_cardCount != null)
        {
            if ((this.m_lockType == LockType.NONE) && (this.m_visualType == CollectionManagerDisplay.ViewMode.CARDS))
            {
                this.m_cardCount.Show();
            }
            else
            {
                this.m_cardCount.Hide();
            }
        }
    }

    public enum LockType
    {
        NONE,
        MAX_COPIES_IN_DECK,
        NO_MORE_INSTANCES
    }
}

