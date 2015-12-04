using System;
using System.Collections.Generic;

public abstract class EntityBase
{
    protected TagSet m_tags = new TagSet();

    protected EntityBase()
    {
    }

    public bool CanAttack()
    {
        return !this.HasTag(GAME_TAG.CANT_ATTACK);
    }

    public bool CanBeAttacked()
    {
        return !this.HasTag(GAME_TAG.CANT_BE_ATTACKED);
    }

    public bool CanBeDamaged()
    {
        return ((!this.HasDivineShield() && !this.IsImmune()) && !this.HasTag(GAME_TAG.CANT_BE_DAMAGED));
    }

    public bool CanBeTargetedByAbilities()
    {
        return !this.HasTag(GAME_TAG.CANT_BE_TARGETED_BY_ABILITIES);
    }

    public bool CanBeTargetedByBattlecries()
    {
        return !this.HasTag(GAME_TAG.CANT_BE_TARGETED_BY_BATTLECRIES);
    }

    public bool CanBeTargetedByHeroPowers()
    {
        return !this.HasTag(GAME_TAG.CANT_BE_TARGETED_BY_HERO_POWERS);
    }

    public bool CanBeTargetedByOpponents()
    {
        return !this.HasTag(GAME_TAG.CANT_BE_TARGETED_BY_OPPONENTS);
    }

    public bool CannotAttackHeroes()
    {
        return this.HasTag(GAME_TAG.CANNOT_ATTACK_HEROES);
    }

    public bool DontShowImmune()
    {
        return this.HasTag(GAME_TAG.DONT_SHOW_IMMUNE);
    }

    public int GetArmor()
    {
        return this.GetTag(GAME_TAG.ARMOR);
    }

    public int GetATK()
    {
        return this.GetTag(GAME_TAG.ATK);
    }

    public int GetAttached()
    {
        return this.GetTag(GAME_TAG.ATTACHED);
    }

    public TAG_CARD_SET GetCardSet()
    {
        return (TAG_CARD_SET) this.GetTag(GAME_TAG.CARD_SET);
    }

    public TAG_CARDTYPE GetCardType()
    {
        return (TAG_CARDTYPE) this.GetTag(GAME_TAG.CARDTYPE);
    }

    public int GetControllerId()
    {
        return this.GetTag(GAME_TAG.CONTROLLER);
    }

    public int GetCost()
    {
        return this.GetTag(GAME_TAG.COST);
    }

    public int GetCreatorId()
    {
        return this.GetTag(GAME_TAG.CREATOR);
    }

    public int GetDamage()
    {
        return this.GetTag(GAME_TAG.DAMAGE);
    }

    public int GetDisplayedCreatorId()
    {
        return this.GetTag(GAME_TAG.DISPLAYED_CREATOR);
    }

    public int GetDurability()
    {
        return this.GetTag(GAME_TAG.DURABILITY);
    }

    public int GetEntityId()
    {
        return this.GetTag(GAME_TAG.ENTITY_ID);
    }

    public int GetFatigue()
    {
        return this.GetTag(GAME_TAG.FATIGUE);
    }

    public int GetHealth()
    {
        if (this.IsWeapon())
        {
            return this.GetTag(GAME_TAG.DURABILITY);
        }
        return this.GetTag(GAME_TAG.HEALTH);
    }

    public int GetNumAttacksThisTurn()
    {
        return this.GetTag(GAME_TAG.NUM_ATTACKS_THIS_TURN);
    }

    public int GetNumTurnsInPlay()
    {
        return this.GetTag(GAME_TAG.NUM_TURNS_IN_PLAY);
    }

    public int GetReferencedTag(GAME_TAG enumTag)
    {
        return this.GetReferencedTag((int) enumTag);
    }

    public abstract int GetReferencedTag(int tag);
    public int GetRemainingHP()
    {
        return ((this.GetHealth() + this.GetArmor()) - this.GetDamage());
    }

    public int GetSpellPower()
    {
        return this.GetTag(GAME_TAG.SPELLPOWER);
    }

    public string GetStringTag(GAME_TAG enumTag)
    {
        return this.GetStringTag((int) enumTag);
    }

    public abstract string GetStringTag(int tag);
    public int GetTag(GAME_TAG enumTag)
    {
        int tag = (int) enumTag;
        return this.m_tags.GetTag(tag);
    }

    public TagEnum GetTag<TagEnum>(GAME_TAG enumTag)
    {
        int tag = this.GetTag(enumTag);
        return (TagEnum) Enum.ToObject(typeof(TagEnum), tag);
    }

    public int GetTag(int tag)
    {
        return this.m_tags.GetTag(tag);
    }

    public TagSet GetTags()
    {
        return this.m_tags;
    }

    public int GetWindfury()
    {
        return this.GetTag(GAME_TAG.WINDFURY);
    }

    public TAG_ZONE GetZone()
    {
        return (TAG_ZONE) this.GetTag(GAME_TAG.ZONE);
    }

    public int GetZonePosition()
    {
        return this.GetTag(GAME_TAG.ZONE_POSITION);
    }

    public bool HasAura()
    {
        return this.HasTag(GAME_TAG.AURA);
    }

    public bool HasBattlecry()
    {
        return this.HasTag(GAME_TAG.BATTLECRY);
    }

    public bool HasCharge()
    {
        return this.HasTag(GAME_TAG.CHARGE);
    }

    public bool HasCombo()
    {
        return this.HasTag(GAME_TAG.COMBO);
    }

    public bool HasCustomKeywordEffect()
    {
        return this.HasTag(GAME_TAG.CUSTOM_KEYWORD_EFFECT);
    }

    public bool HasDeathrattle()
    {
        return this.HasTag(GAME_TAG.DEATHRATTLE);
    }

    public bool HasDivineShield()
    {
        return this.HasTag(GAME_TAG.DIVINE_SHIELD);
    }

    public bool HasEvilGlow()
    {
        return this.HasTag(GAME_TAG.EVIL_GLOW);
    }

    public bool HasHealthMin()
    {
        return (this.GetTag(GAME_TAG.HEALTH_MINIMUM) > 0);
    }

    public bool HasHeroPowerDamage()
    {
        return this.HasTag(GAME_TAG.HEROPOWER_DAMAGE);
    }

    public bool HasInspire()
    {
        return this.HasTag(GAME_TAG.INSPIRE);
    }

    public bool HasOverload()
    {
        return this.HasTag(GAME_TAG.OVERLOAD);
    }

    public bool HasReferencedTag(GAME_TAG enumTag)
    {
        return (this.GetReferencedTag(enumTag) > 0);
    }

    public bool HasReferencedTag(int tag)
    {
        return (this.GetReferencedTag(tag) > 0);
    }

    public bool HasSpellPower()
    {
        return this.HasTag(GAME_TAG.SPELLPOWER);
    }

    public bool HasSpellPowerDouble()
    {
        return this.HasTag(GAME_TAG.SPELLPOWER_DOUBLE);
    }

    public bool HasStringTag(GAME_TAG enumTag)
    {
        return (this.GetStringTag(enumTag) != null);
    }

    public bool HasStringTag(int tag)
    {
        return (this.GetStringTag(tag) != null);
    }

    public bool HasTag(GAME_TAG tag)
    {
        return (this.GetTag(tag) > 0);
    }

    public bool HasTaunt()
    {
        return this.HasTag(GAME_TAG.TAUNT);
    }

    public bool HasTriggerVisual()
    {
        return this.HasTag(GAME_TAG.TRIGGER_VISUAL);
    }

    public bool HasWindfury()
    {
        return (this.GetTag(GAME_TAG.WINDFURY) > 0);
    }

    public bool IsAffectedBySpellPower()
    {
        return this.HasTag(GAME_TAG.AFFECTED_BY_SPELL_POWER);
    }

    public bool IsAsleep()
    {
        return ((this.GetNumTurnsInPlay() == 0) && !this.HasCharge());
    }

    public bool IsAttached()
    {
        return this.HasTag(GAME_TAG.ATTACHED);
    }

    public bool IsBasicCardUnlock()
    {
        if (this.IsHero())
        {
            return false;
        }
        return (((TAG_CARD_SET) this.GetTag<TAG_CARD_SET>(GAME_TAG.CARD_SET)) == TAG_CARD_SET.CORE);
    }

    public bool IsCharacter()
    {
        return (this.IsHero() || this.IsMinion());
    }

    public bool IsDamaged()
    {
        return (this.GetDamage() > 0);
    }

    public bool IsElite()
    {
        return (this.GetTag(GAME_TAG.ELITE) > 0);
    }

    public bool IsEnchantment()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 6);
    }

    public bool IsEnraged()
    {
        return (this.HasTag(GAME_TAG.ENRAGED) && (this.GetDamage() > 0));
    }

    public bool IsExhausted()
    {
        return this.HasTag(GAME_TAG.EXHAUSTED);
    }

    public bool IsFreeze()
    {
        return this.HasTag(GAME_TAG.FREEZE);
    }

    public bool IsFrozen()
    {
        return this.HasTag(GAME_TAG.FROZEN);
    }

    public bool IsGame()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 1);
    }

    public bool IsHero()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 3);
    }

    public bool IsHeroPower()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 10);
    }

    public bool IsImmune()
    {
        return this.HasTag(GAME_TAG.CANT_BE_DAMAGED);
    }

    public bool IsItem()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 8);
    }

    public bool IsMagnet()
    {
        return this.HasTag(GAME_TAG.MAGNET);
    }

    public bool IsMinion()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 4);
    }

    public bool IsObfuscated()
    {
        return this.HasTag(GAME_TAG.OBFUSCATED);
    }

    public bool IsPlayer()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 2);
    }

    public bool IsPoisonous()
    {
        return this.HasTag(GAME_TAG.POISONOUS);
    }

    public bool IsRecentlyArrived()
    {
        return this.HasTag(GAME_TAG.RECENTLY_ARRIVED);
    }

    public bool IsSecret()
    {
        return this.HasTag(GAME_TAG.SECRET);
    }

    public bool IsSilenced()
    {
        return this.HasTag(GAME_TAG.SILENCED);
    }

    public bool IsSpell()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 5);
    }

    public bool IsStealthed()
    {
        return this.HasTag(GAME_TAG.STEALTH);
    }

    public bool IsToken()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 9);
    }

    public bool IsWeapon()
    {
        return (this.GetTag(GAME_TAG.CARDTYPE) == 7);
    }

    public bool ReferencesBattlecry()
    {
        return this.HasReferencedTag(GAME_TAG.BATTLECRY);
    }

    public bool ReferencesCharge()
    {
        return this.HasReferencedTag(GAME_TAG.CHARGE);
    }

    public bool ReferencesDeathrattle()
    {
        return this.HasReferencedTag(GAME_TAG.DEATHRATTLE);
    }

    public bool ReferencesDivineShield()
    {
        return this.HasReferencedTag(GAME_TAG.DIVINE_SHIELD);
    }

    public bool ReferencesImmune()
    {
        return this.HasReferencedTag(GAME_TAG.CANT_BE_DAMAGED);
    }

    public bool ReferencesSecret()
    {
        return this.HasReferencedTag(GAME_TAG.SECRET);
    }

    public bool ReferencesSpellPower()
    {
        return this.HasReferencedTag(GAME_TAG.SPELLPOWER);
    }

    public bool ReferencesStealth()
    {
        return this.HasReferencedTag(GAME_TAG.STEALTH);
    }

    public bool ReferencesTaunt()
    {
        return this.HasReferencedTag(GAME_TAG.TAUNT);
    }

    public bool ReferencesWindfury()
    {
        return this.HasReferencedTag(GAME_TAG.WINDFURY);
    }

    public void ReplaceTags(List<Network.Entity.Tag> tags)
    {
        this.m_tags.Replace(tags);
    }

    public void ReplaceTags(TagSet tags)
    {
        this.m_tags.Replace(tags);
    }

    public void SetTag(GAME_TAG tag, int tagValue)
    {
        this.SetTag((int) tag, tagValue);
    }

    public void SetTag<TagEnum>(GAME_TAG tag, TagEnum tagValue)
    {
        this.SetTag((int) tag, Convert.ToInt32(tagValue));
    }

    public void SetTag(int tag, int tagValue)
    {
        this.m_tags.SetTag(tag, tagValue);
    }

    public void SetTags(Map<GAME_TAG, int> tagMap)
    {
        this.m_tags.SetTags(tagMap);
    }

    public void SetTags(List<Network.Entity.Tag> tags)
    {
        this.m_tags.SetTags(tags);
    }
}

