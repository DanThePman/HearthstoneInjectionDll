using PegasusUtil;
using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class GeneralStorePackSelectorButton : PegUIElement
{
    private BoosterDbId m_boosterId;
    public bool m_checkNewPlayer;
    public HighlightState m_highlight;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_mouseOverSound;
    public UberText m_packText;
    [CustomEditField(Parent="m_checkNewPlayer")]
    public int m_recommendedMinimumBuyCount = 10;
    public GameObject m_ribbonIndicator;
    public UberText m_ribbonIndicatorText;
    private bool m_selected;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_selectSound;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_unselectSound;

    public BoosterDbId GetBoosterId()
    {
        return this.m_boosterId;
    }

    public bool IsLatestExpansion()
    {
        return (!this.IsPreorder() && (this.m_boosterId == BoosterDbId.THE_GRAND_TOURNAMENT));
    }

    public bool IsPreorder()
    {
        Network.Bundle preOrderBundle = null;
        return StoreManager.Get().IsBoosterPreorderActive((int) this.m_boosterId, out preOrderBundle);
    }

    public bool IsPurchasable()
    {
        List<Network.Bundle> list = StoreManager.Get().GetAllBundlesForProduct(ProductType.PRODUCT_TYPE_BOOSTER, (int) this.m_boosterId, 0);
        return ((list != null) && (list.Count > 0));
    }

    public bool IsRecommendedForNewPlayer()
    {
        return (this.m_checkNewPlayer && (StoreManager.Get().GetTotalBoostersAcquired() < this.m_recommendedMinimumBuyCount));
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        base.OnOut(oldState);
        this.m_highlight.ChangeState(!this.m_selected ? ActorStateType.NONE : ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        base.OnOver(oldState);
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_SECONDARY_ACTIVE);
        if (!string.IsNullOrEmpty(this.m_mouseOverSound))
        {
            SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_mouseOverSound));
        }
    }

    protected override void OnPress()
    {
        base.OnPress();
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_SECONDARY_ACTIVE);
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

    public void SetBoosterId(BoosterDbId boosterId)
    {
        this.m_boosterId = boosterId;
        if (this.m_packText != null)
        {
            this.m_packText.Text = GameDbf.Booster.GetRecord((int) this.m_boosterId).GetLocString("NAME");
        }
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

    public void UpdateRibbonIndicator()
    {
        if ((this.m_ribbonIndicator != null) && (this.m_boosterId != BoosterDbId.INVALID))
        {
            bool flag = false;
            if (this.IsRecommendedForNewPlayer())
            {
                flag = true;
                this.m_ribbonIndicatorText.Text = GameStrings.Get("GLUE_STORE_PACKBUY_SUGGESTION");
            }
            else if (this.IsPreorder())
            {
                flag = true;
                this.m_ribbonIndicatorText.Text = GameStrings.Get("GLUE_STORE_PACKS_PREORDER_TEXT");
            }
            else if (this.IsLatestExpansion())
            {
                flag = true;
                this.m_ribbonIndicatorText.Text = GameStrings.Get("GLUE_STORE_PACKS_LATEST_EXPANSION");
            }
            this.m_ribbonIndicator.SetActive(flag);
        }
    }
}

