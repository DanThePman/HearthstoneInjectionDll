using System;
using UnityEngine;

public class HistoryInfo
{
    public int m_armorChangeAmount;
    public Material m_bigCardGoldenMaterial;
    public Texture m_bigCardPortraitTexture;
    private bool m_cachedBonuses;
    public int m_damageChangeAmount;
    public bool m_died;
    private Entity m_duplicatedEntity;
    public HistoryInfoType m_infoType;
    private int m_originalDamageBonus;
    private int m_originalDamageBonusDouble;
    private Entity m_originalEntity;
    private int m_originalHealingDouble;

    public void CacheBonuses()
    {
        if (this.CanCacheBonuses())
        {
            this.m_originalDamageBonus = this.m_originalEntity.GetDamageBonus();
            this.m_originalDamageBonusDouble = this.m_originalEntity.GetDamageBonusDouble();
            this.m_originalHealingDouble = this.m_originalEntity.GetHealingDouble();
            this.m_cachedBonuses = true;
        }
    }

    public bool CanCacheBonuses()
    {
        if (this.m_cachedBonuses)
        {
            return false;
        }
        if (this.m_originalEntity == null)
        {
            return false;
        }
        return this.m_originalEntity.HasTag(GAME_TAG.CARDTYPE);
    }

    public bool CanDuplicateEntity()
    {
        if (this.m_originalEntity == null)
        {
            return false;
        }
        if (this.m_originalEntity.GetLoadState() != Entity.LoadState.DONE)
        {
            return false;
        }
        return (!this.m_originalEntity.IsHidden() || (this.m_originalEntity.IsSecret() && GameUtils.IsEntityHiddenAfterCurrentTasklist(this.m_originalEntity)));
    }

    public void DuplicateEntity()
    {
        if (this.m_duplicatedEntity == null)
        {
            this.CacheBonuses();
            if (this.CanDuplicateEntity())
            {
                Card card = this.GetCard();
                if (card != null)
                {
                    this.m_bigCardPortraitTexture = card.GetPortraitTexture();
                    this.m_bigCardGoldenMaterial = card.GetGoldenMaterial();
                }
                this.m_duplicatedEntity = this.m_originalEntity.CloneForHistory(this.m_originalDamageBonus, this.m_originalDamageBonusDouble, this.m_originalHealingDouble);
            }
        }
    }

    public Card GetCard()
    {
        return this.m_originalEntity.GetCard();
    }

    public Entity GetDuplicatedEntity()
    {
        return this.m_duplicatedEntity;
    }

    public Entity GetOriginalEntity()
    {
        return this.m_originalEntity;
    }

    public int GetSplatAmount()
    {
        return (this.m_damageChangeAmount + this.m_armorChangeAmount);
    }

    public void SetOriginalEntity(Entity entity)
    {
        this.m_originalEntity = entity;
        this.DuplicateEntity();
    }
}

