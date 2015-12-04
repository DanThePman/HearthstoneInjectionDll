using System;

[CustomEditClass]
public class GeneralStoreHeroesSelectorButton : PegUIElement
{
    private CardDef m_currentCardDef;
    private EntityDef m_currentEntityDef;
    public Actor m_heroActor;
    private int m_heroDbId = -1;
    private string m_heroId;
    public UberText m_heroName;
    public HighlightState m_highlight;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_mouseOverSound;
    private bool m_purchased;
    private bool m_selected;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_selectSound;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_unselectSound;

    protected override void Awake()
    {
        base.Awake();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.WIDTH);
        }
        else
        {
            OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        }
    }

    public int GetHeroDbId()
    {
        return this.m_heroDbId;
    }

    public string GetHeroId()
    {
        return this.m_heroId;
    }

    public bool GetPurchased()
    {
        return this.m_purchased;
    }

    public bool IsAvailable()
    {
        return true;
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        base.OnOut(oldState);
        if ((this.m_highlight != null) && this.IsAvailable())
        {
            this.m_highlight.ChangeState(!this.m_selected ? ActorStateType.NONE : ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        base.OnOver(oldState);
        if ((this.m_highlight != null) && this.IsAvailable())
        {
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_SECONDARY_ACTIVE);
            if (!string.IsNullOrEmpty(this.m_mouseOverSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_mouseOverSound));
            }
        }
    }

    protected override void OnPress()
    {
        base.OnPress();
        if ((this.m_highlight != null) && this.IsAvailable())
        {
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        }
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        if ((this.m_highlight != null) && this.IsAvailable())
        {
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_SECONDARY_ACTIVE);
        }
    }

    public void Select()
    {
        if (!this.m_selected)
        {
            this.m_selected = true;
            this.m_highlight.ChangeState((base.GetInteractionState() != PegUIElement.InteractionState.Up) ? ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE : ActorStateType.HIGHLIGHT_SECONDARY_ACTIVE);
            if (!string.IsNullOrEmpty(this.m_selectSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_selectSound));
            }
        }
    }

    public void SetHeroIds(int heroDbId, string heroId)
    {
        this.m_heroDbId = heroDbId;
        this.m_heroId = heroId;
    }

    public void SetPurchased(bool purchased)
    {
        this.m_purchased = purchased;
    }

    public void Unselect()
    {
        if (this.m_selected)
        {
            this.m_selected = false;
            this.m_highlight.ChangeState(ActorStateType.NONE);
            if (!string.IsNullOrEmpty(this.m_unselectSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_unselectSound));
            }
        }
    }

    public void UpdateName(GeneralStoreHeroesSelectorButton rhs)
    {
        this.UpdateName(rhs.m_heroName.Text);
    }

    public void UpdateName(string name)
    {
        if (this.m_heroName != null)
        {
            this.m_heroName.Text = name;
        }
    }

    public void UpdatePortrait(GeneralStoreHeroesSelectorButton rhs)
    {
        this.UpdatePortrait(rhs.m_currentEntityDef, rhs.m_currentCardDef);
    }

    public void UpdatePortrait(EntityDef entityDef, CardDef cardDef)
    {
        this.m_heroActor.SetEntityDef(entityDef);
        this.m_heroActor.SetCardDef(cardDef);
        this.m_heroActor.UpdateAllComponents();
        this.m_heroActor.SetUnlit();
        this.m_currentEntityDef = entityDef;
        this.m_currentCardDef = cardDef;
    }
}

