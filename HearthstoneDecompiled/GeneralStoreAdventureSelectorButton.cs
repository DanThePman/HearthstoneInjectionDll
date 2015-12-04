using PegasusUtil;
using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class GeneralStoreAdventureSelectorButton : PegUIElement
{
    public UberText m_adventureTitle;
    private AdventureDbId m_adventureType;
    public HighlightState m_highlight;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_mouseOverSound;
    public GameObject m_preorderRibbon;
    private bool m_selected;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_selectSound;
    public TooltipZone m_unavailableTooltip;
    public GameLayer m_unavailableTooltipLayer = GameLayer.PerspectiveUI;
    public float m_unavailableTooltipScale = 20f;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_unselectSound;

    public AdventureDbId GetAdventureType()
    {
        return this.m_adventureType;
    }

    public bool IsAvailable()
    {
        Network.Bundle bundle = null;
        bool productExists = false;
        StoreManager.Get().GetAvailableAdventureBundle(this.m_adventureType, out bundle, out productExists);
        return productExists;
    }

    public bool IsPreorder()
    {
        Network.Bundle bundle = null;
        StoreManager.Get().GetAvailableAdventureBundle(this.m_adventureType, out bundle);
        return ((bundle != null) && bundle.IsPreOrder());
    }

    public bool IsPurchasable()
    {
        ProductType adventureProductType = StoreManager.GetAdventureProductType(this.m_adventureType);
        if (adventureProductType == ProductType.PRODUCT_TYPE_UNKNOWN)
        {
            return false;
        }
        bool productExists = false;
        List<Network.Bundle> list = StoreManager.Get().GetAvailableBundlesForProduct(adventureProductType, out productExists, 0, 0);
        return ((list != null) && (list.Count > 0));
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        base.OnOut(oldState);
        if (this.IsAvailable())
        {
            this.m_highlight.ChangeState(!this.m_selected ? ActorStateType.NONE : ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        }
        else if (this.m_unavailableTooltip != null)
        {
            this.m_unavailableTooltip.HideTooltip();
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        base.OnOver(oldState);
        if (this.IsAvailable())
        {
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_SECONDARY_ACTIVE);
            if (!string.IsNullOrEmpty(this.m_mouseOverSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_mouseOverSound));
            }
        }
        else if (this.m_unavailableTooltip != null)
        {
            SceneUtils.SetLayer(this.m_unavailableTooltip.ShowTooltip(GameStrings.Get("GLUE_STORE_ADVENTURE_BUTTON_UNAVAILABLE_HEADLINE"), GameStrings.Get("GLUE_STORE_ADVENTURE_BUTTON_UNAVAILABLE_DESCRIPTION"), this.m_unavailableTooltipScale, true), this.m_unavailableTooltipLayer);
        }
    }

    protected override void OnPress()
    {
        base.OnPress();
        if (this.IsAvailable())
        {
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        }
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        if (this.IsAvailable())
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

    public void SetAdventureType(AdventureDbId adventure)
    {
        if (this.m_adventureTitle != null)
        {
            DbfRecord record = GameDbf.Adventure.GetRecord((int) adventure);
            if (record != null)
            {
                this.m_adventureTitle.Text = record.GetLocString("STORE_BUY_BUTTON_LABEL");
            }
        }
        this.m_adventureType = adventure;
        this.UpdateState();
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

    public void UpdateState()
    {
        if (this.m_preorderRibbon != null)
        {
            this.m_preorderRibbon.SetActive(this.IsPreorder());
        }
    }
}

