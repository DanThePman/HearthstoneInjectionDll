using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Entity : EntityBase
{
    public const string HIDDEN_CARD_ASSET_NAME = "HiddenCard";
    private List<Entity> m_attachments = new List<Entity>();
    private Card m_card;
    private int m_cardAssetLoadCount;
    private string m_cardId;
    private bool m_duplicateForHistory;
    private EntityDef m_entityDef = new EntityDef();
    private int m_historyDamageBonus;
    private int m_historyDamageBonusDouble;
    private int m_historyHealingDouble;
    private LoadState m_loadState;
    private List<PowerHistoryInfo> m_powerHistoryInfoOverrides = new List<PowerHistoryInfo>();
    private int m_realTimeArmor;
    private int m_realTimeAttack;
    private int m_realTimeCost = -1;
    private int m_realTimeDamage;
    private int m_realTimeHealth;
    private bool m_realTimePoweredUp;
    private List<int> m_subCardIDs = new List<int>();
    private bool m_useBattlecryPower;
    public const string PLACEHOLDER_CARD_ASSET_NAME = "PlaceholderCard";

    public void AddAttachment(Entity entity)
    {
        int count = this.m_attachments.Count;
        if (this.m_attachments.Contains(entity))
        {
            Log.Mike.Print(string.Format("Entity.AddAttachment() - {0} is already an attachment of {1}", entity, this), new object[0]);
        }
        else
        {
            this.m_attachments.Add(entity);
            if (this.m_card != null)
            {
                this.m_card.OnEnchantmentAdded(count, entity);
            }
        }
    }

    public void AddPowerHistoryInfoOverride(PowerHistoryInfo info)
    {
        if (info != null)
        {
            for (int i = 0; i < this.m_powerHistoryInfoOverrides.Count; i++)
            {
                if (this.m_powerHistoryInfoOverrides[i].GetEffectIndex() == info.GetEffectIndex())
                {
                    this.m_powerHistoryInfoOverrides[i] = info;
                    return;
                }
            }
            this.m_powerHistoryInfoOverrides.Add(info);
        }
    }

    public void AddSubCard(Entity entity)
    {
        if (!this.m_subCardIDs.Contains(entity.GetEntityId()))
        {
            this.m_subCardIDs.Add(entity.GetEntityId());
        }
    }

    public void ClearBattlecryFlag()
    {
        this.m_useBattlecryPower = false;
    }

    public void ClearTags()
    {
        base.m_tags.Clear();
    }

    public Entity CloneForHistory(int damageBonus, int damageBonusDouble, int healingDouble)
    {
        Entity entity = new Entity {
            m_duplicateForHistory = true,
            m_entityDef = this.m_entityDef,
            m_card = this.m_card,
            m_cardId = this.m_cardId
        };
        entity.ReplaceTags(base.m_tags);
        entity.SetTag<TAG_ZONE>(GAME_TAG.ZONE, TAG_ZONE.HAND);
        entity.m_historyDamageBonus = damageBonus;
        entity.m_historyDamageBonusDouble = damageBonusDouble;
        entity.m_historyHealingDouble = healingDouble;
        return entity;
    }

    public Entity CloneForZoneMgr()
    {
        Entity entity = new Entity {
            m_entityDef = this.m_entityDef,
            m_card = this.m_card,
            m_cardId = this.m_cardId
        };
        entity.ReplaceTags(base.m_tags);
        return entity;
    }

    public PlayErrors.SourceEntityInfo ConvertToSourceInfo(PlayErrors.PlayRequirementInfo playRequirementInfo, Entity parent)
    {
        List<string> entourageCardIDs = this.GetEntityDef().GetEntourageCardIDs();
        List<string> list2 = new List<string>();
        int num = 0;
        ZoneMgr mgr = ZoneMgr.Get();
        if (mgr != null)
        {
            ZonePlay play = mgr.FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
            if (play != null)
            {
                foreach (Card card in play.GetCards())
                {
                    Entity entity = card.GetEntity();
                    if (entity != null)
                    {
                        list2.Add(entity.GetCardId());
                    }
                }
            }
            ZonePlay play2 = mgr.FindZoneOfType<ZonePlay>(Player.Side.OPPOSING);
            if (play2 != null)
            {
                foreach (Card card2 in play2.GetCards())
                {
                    Entity entity2 = card2.GetEntity();
                    if ((entity2 != null) && entity2.IsMinion())
                    {
                        num++;
                    }
                }
            }
        }
        PlayErrors.SourceEntityInfo info = new PlayErrors.SourceEntityInfo {
            requirementsMap = playRequirementInfo.requirementsMap,
            id = base.GetEntityId(),
            cost = base.GetCost(),
            attack = base.GetATK(),
            minAttackRequirement = playRequirementInfo.paramMinAtk,
            maxAttackRequirement = playRequirementInfo.paramMaxAtk,
            raceRequirement = playRequirementInfo.paramRace,
            numMinionSlotsRequirement = playRequirementInfo.paramNumMinionSlots,
            numMinionSlotsWithTargetRequirement = playRequirementInfo.paramNumMinionSlotsWithTarget,
            minTotalMinionsRequirement = playRequirementInfo.paramMinNumTotalMinions,
            minFriendlyMinionsRequirement = playRequirementInfo.paramMinNumFriendlyMinions,
            minEnemyMinionsRequirement = playRequirementInfo.paramMinNumEnemyMinions,
            numTurnsInPlay = base.GetNumTurnsInPlay(),
            numAttacksThisTurn = base.GetNumAttacksThisTurn(),
            numWindfury = base.GetWindfury(),
            cardType = base.GetCardType(),
            zone = base.GetZone(),
            isSecret = base.IsSecret(),
            isDuplicateSecret = false
        };
        if (info.isSecret)
        {
            Player player = GameState.Get().GetPlayer(base.GetControllerId());
            if (player != null)
            {
                foreach (string str in player.GetSecretDefinitions())
                {
                    if (this.GetCardId().Equals(str, StringComparison.OrdinalIgnoreCase))
                    {
                        info.isDuplicateSecret = true;
                        break;
                    }
                }
            }
        }
        info.isExhausted = base.IsExhausted();
        info.isMasterPower = (base.GetZone() == TAG_ZONE.HAND) || base.IsHeroPower();
        info.isActionPower = TAG_ZONE.HAND == base.GetZone();
        info.isActivatePower = (base.GetZone() == TAG_ZONE.PLAY) || base.IsHeroPower();
        info.isAttackPower = base.IsHero() || (!base.IsHeroPower() && (TAG_ZONE.PLAY == base.GetZone()));
        info.isFrozen = base.IsFrozen();
        info.hasBattlecry = base.HasBattlecry();
        info.canAttack = base.CanAttack();
        info.entireEntourageInPlay = false;
        if (entourageCardIDs.Count > 0)
        {
            info.entireEntourageInPlay = list2.Count > 0;
            <ConvertToSourceInfo>c__AnonStorey2F3 storeyf = new <ConvertToSourceInfo>c__AnonStorey2F3();
            using (List<string>.Enumerator enumerator4 = entourageCardIDs.GetEnumerator())
            {
                while (enumerator4.MoveNext())
                {
                    storeyf.entourageCardID = enumerator4.Current;
                    if (list2.Find(new Predicate<string>(storeyf.<>m__F1)) == null)
                    {
                        info.entireEntourageInPlay = false;
                        goto Label_03C8;
                    }
                }
            }
        }
    Label_03C8:
        info.hasCharge = base.HasCharge();
        info.isChoiceMinion = (parent != null) && parent.IsMinion();
        info.cannotAttackHeroes = base.CannotAttackHeroes();
        return info;
    }

    public PlayErrors.TargetEntityInfo ConvertToTargetInfo()
    {
        return new PlayErrors.TargetEntityInfo { 
            id = base.GetEntityId(), owningPlayerID = base.GetControllerId(), damage = base.GetDamage(), attack = base.GetATK(), race = (int) this.m_entityDef.GetRace(), rarity = (int) this.m_entityDef.GetRarity(), isImmune = base.IsImmune(), canBeAttacked = base.CanBeAttacked(), canBeTargetedByOpponents = base.CanBeTargetedByOpponents(), canBeTargetedBySpells = base.CanBeTargetedByAbilities(), canBeTargetedByHeroPowers = base.CanBeTargetedByHeroPowers(), canBeTargetedByBattlecries = base.CanBeTargetedByBattlecries(), cardType = base.GetCardType(), isFrozen = base.IsFrozen(), isEnchanted = this.IsEnchanted(), isStealthed = base.IsStealthed(), 
            isTaunter = base.HasTaunt(), isMagnet = base.IsMagnet(), hasCharge = base.HasCharge(), hasAttackedThisTurn = base.GetNumAttacksThisTurn() > 0, hasBattlecry = base.HasBattlecry(), hasDeathrattle = base.HasDeathrattle()
         };
    }

    public void Destroy()
    {
        if (this.m_card != null)
        {
            this.m_card.Destroy();
        }
    }

    public bool DoEnchantmentsHaveTriggerVisuals()
    {
        foreach (Entity entity in this.m_attachments)
        {
            if (entity.HasTriggerVisual())
            {
                return true;
            }
        }
        return false;
    }

    private void FinishCardLoad()
    {
        if (this.IsCopyingDeathrattleEffect())
        {
            this.LoadDeathrattleCopyEffect();
        }
        this.m_loadState = LoadState.DONE;
    }

    public List<Entity> GetAttachments()
    {
        return this.m_attachments;
    }

    public Power GetAttackPower()
    {
        return this.m_entityDef.GetAttackPower();
    }

    public Card GetCard()
    {
        return this.m_card;
    }

    public CardFlair GetCardFlair()
    {
        return new CardFlair(this.GetPremiumType());
    }

    public string GetCardId()
    {
        return this.m_cardId;
    }

    public TAG_CARD_SET GetCardSet()
    {
        return this.m_entityDef.GetCardSet();
    }

    public string GetCardTextInHand()
    {
        if (base.IsEnchantment() && this.IsCopyingDeathrattle())
        {
            Entity entity = GameState.Get().GetEntity(base.GetTag(GAME_TAG.COPY_DEATHRATTLE));
            object[] args = new object[] { entity.GetName() };
            return TextUtils.TransformCardText(this.GetDamageBonus(), this.GetDamageBonusDouble(), this.GetHealingDouble(), GameStrings.Format("GAMEPLAY_COPY_DEATHRATTLE", args));
        }
        return TextUtils.TransformCardText(this, GAME_TAG.CARDTEXT_INHAND);
    }

    public string GetCardTextInHistory()
    {
        int historyDamageBonus = this.GetHistoryDamageBonus();
        int historyDamageBonusDouble = this.GetHistoryDamageBonusDouble();
        int historyHealingDouble = this.GetHistoryHealingDouble();
        return TextUtils.TransformCardText(historyDamageBonus, historyDamageBonusDouble, historyHealingDouble, base.GetStringTag(GAME_TAG.CARDTEXT_INHAND));
    }

    public TAG_CLASS GetClass()
    {
        if (base.IsSecret())
        {
            return base.GetTag<TAG_CLASS>(GAME_TAG.CLASS);
        }
        return this.m_entityDef.GetClass();
    }

    public Player GetController()
    {
        return GameState.Get().GetPlayer(base.GetControllerId());
    }

    public Player.Side GetControllerSide()
    {
        Player controller = this.GetController();
        if (controller == null)
        {
            return Player.Side.NEUTRAL;
        }
        return controller.GetSide();
    }

    public Entity GetCreator()
    {
        return GameState.Get().GetEntity(base.GetCreatorId());
    }

    public int GetDamageBonus()
    {
        Player controller = this.GetController();
        if (controller != null)
        {
            if (base.IsSpell())
            {
                int tag = controller.GetTag(GAME_TAG.CURRENT_SPELLPOWER);
                if (base.HasTag(GAME_TAG.RECEIVES_DOUBLE_SPELLDAMAGE_BONUS))
                {
                    tag *= 2;
                }
                return tag;
            }
            if (base.IsHeroPower())
            {
                return controller.GetTag(GAME_TAG.CURRENT_HEROPOWER_DAMAGE_BONUS);
            }
        }
        return 0;
    }

    public int GetDamageBonusDouble()
    {
        Player controller = this.GetController();
        if (controller != null)
        {
            if (base.IsSpell())
            {
                return controller.GetTag(GAME_TAG.SPELLPOWER_DOUBLE);
            }
            if (base.IsHeroPower())
            {
                return controller.GetTag(GAME_TAG.HERO_POWER_DOUBLE);
            }
        }
        return 0;
    }

    public virtual string GetDebugName()
    {
        string stringTag = base.GetStringTag(GAME_TAG.CARDNAME);
        if (stringTag != null)
        {
            object[] objArray1 = new object[] { stringTag, base.GetEntityId(), base.GetZone(), base.GetZonePosition(), this.m_cardId, base.GetControllerId() };
            return string.Format("[name={0} id={1} zone={2} zonePos={3} cardId={4} player={5}]", objArray1);
        }
        if (this.m_cardId != null)
        {
            object[] objArray2 = new object[] { base.GetEntityId(), this.m_cardId, base.GetCardType(), base.GetZone(), base.GetZonePosition(), base.GetControllerId() };
            return string.Format("[id={0} cardId={1} type={2} zone={3} zonePos={4} player={5}]", objArray2);
        }
        object[] args = new object[] { base.GetEntityId(), base.GetCardType(), base.GetZone(), base.GetZonePosition() };
        return string.Format("UNKNOWN ENTITY [id={0} type={1} zone={2} zonePos={3}]", args);
    }

    public Entity GetDisplayedCreator()
    {
        return GameState.Get().GetEntity(base.GetDisplayedCreatorId());
    }

    public TAG_ENCHANTMENT_VISUAL GetEnchantmentBirthVisual()
    {
        return this.m_entityDef.GetEnchantmentBirthVisual();
    }

    public TAG_ENCHANTMENT_VISUAL GetEnchantmentIdleVisual()
    {
        return this.m_entityDef.GetEnchantmentIdleVisual();
    }

    public List<Entity> GetEnchantments()
    {
        return this.GetAttachments();
    }

    public EntityDef GetEntityDef()
    {
        return this.m_entityDef;
    }

    public int GetHealingDouble()
    {
        Player controller = this.GetController();
        if (controller != null)
        {
            if (base.IsSpell())
            {
                return controller.GetTag(GAME_TAG.HEALING_DOUBLE);
            }
            if (base.IsHeroPower())
            {
                return controller.GetTag(GAME_TAG.HERO_POWER_DOUBLE);
            }
        }
        return 0;
    }

    public virtual Entity GetHero()
    {
        if (base.IsHero())
        {
            return this;
        }
        Player controller = this.GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetHero();
    }

    public virtual Card GetHeroCard()
    {
        if (base.IsHero())
        {
            return this.GetCard();
        }
        Player controller = this.GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetHeroCard();
    }

    public virtual Entity GetHeroPower()
    {
        if (base.IsHeroPower())
        {
            return this;
        }
        Player controller = this.GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetHeroPower();
    }

    public virtual Card GetHeroPowerCard()
    {
        if (base.IsHeroPower())
        {
            return this.GetCard();
        }
        Player controller = this.GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetHeroPowerCard();
    }

    public int GetHistoryDamageBonus()
    {
        return this.m_historyDamageBonus;
    }

    public int GetHistoryDamageBonusDouble()
    {
        return this.m_historyDamageBonusDouble;
    }

    public int GetHistoryHealingDouble()
    {
        return this.m_historyHealingDouble;
    }

    public LoadState GetLoadState()
    {
        return this.m_loadState;
    }

    public Power GetMasterPower()
    {
        return this.m_entityDef.GetMasterPower();
    }

    public virtual string GetName()
    {
        string stringTag = base.GetStringTag(GAME_TAG.CARDNAME);
        if (stringTag != null)
        {
            return stringTag;
        }
        return this.GetDebugName();
    }

    public int GetOriginalATK()
    {
        return this.m_entityDef.GetATK();
    }

    public bool GetOriginalCharge()
    {
        return this.m_entityDef.HasTag(GAME_TAG.CHARGE);
    }

    public int GetOriginalCost()
    {
        return this.m_entityDef.GetCost();
    }

    public int GetOriginalDurability()
    {
        return this.m_entityDef.GetDurability();
    }

    public int GetOriginalHealth()
    {
        return this.m_entityDef.GetHealth();
    }

    public PowerHistoryInfo GetPowerHistoryInfo(int effectIndex)
    {
        foreach (PowerHistoryInfo info in this.m_powerHistoryInfoOverrides)
        {
            if (effectIndex == info.GetEffectIndex())
            {
                return info;
            }
        }
        return this.m_entityDef.GetPowerHistoryInfo(effectIndex);
    }

    public TAG_PREMIUM GetPremiumType()
    {
        return (TAG_PREMIUM) base.GetTag(GAME_TAG.PREMIUM);
    }

    public TAG_RACE GetRace()
    {
        return this.m_entityDef.GetRace();
    }

    public string GetRaceText()
    {
        return this.m_entityDef.GetRaceText();
    }

    public TAG_RARITY GetRarity()
    {
        return this.m_entityDef.GetRarity();
    }

    public int GetRealTimeAttack()
    {
        return this.m_realTimeAttack;
    }

    public int GetRealTimeCost()
    {
        if (this.m_realTimeCost == -1)
        {
            return base.GetCost();
        }
        return this.m_realTimeCost;
    }

    public bool GetRealTimePoweredUp()
    {
        return this.m_realTimePoweredUp;
    }

    public int GetRealTimeRemainingHP()
    {
        return ((this.m_realTimeHealth + this.m_realTimeArmor) - this.m_realTimeDamage);
    }

    public override int GetReferencedTag(int tag)
    {
        return this.m_entityDef.GetReferencedTag(tag);
    }

    public override string GetStringTag(int tag)
    {
        return this.m_entityDef.GetStringTag(tag);
    }

    public List<int> GetSubCardIDs()
    {
        return this.m_subCardIDs;
    }

    public string GetTargetingArrowText()
    {
        return TextUtils.TransformCardText(this, GAME_TAG.TARGETING_ARROW_TEXT);
    }

    public virtual Card GetWeaponCard()
    {
        if (base.IsWeapon())
        {
            return this.GetCard();
        }
        Player controller = this.GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetWeaponCard();
    }

    private void HandleCopyDeathrattleChange()
    {
        if (this.IsCopyingDeathrattleEffect())
        {
            this.LoadDeathrattleCopyEffect();
        }
    }

    private void HandleTagChange(TagDelta change)
    {
        switch (((GAME_TAG) change.tag))
        {
            case GAME_TAG.COPY_DEATHRATTLE:
            case GAME_TAG.COPY_DEATHRATTLE_INDEX:
                this.HandleCopyDeathrattleChange();
                break;

            case GAME_TAG.ATTACHED:
            {
                Entity entity = GameState.Get().GetEntity(change.oldValue);
                if (entity != null)
                {
                    entity.RemoveAttachment(this);
                }
                Entity entity3 = GameState.Get().GetEntity(change.newValue);
                if (entity3 != null)
                {
                    entity3.AddAttachment(this);
                }
                break;
            }
            case GAME_TAG.ZONE:
                this.UpdateUseBattlecryFlag(false);
                if ((GameState.Get().IsTurnStartManagerActive() && (change.oldValue == 2)) && (change.newValue == 3))
                {
                    PowerTaskList currentTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
                    if ((currentTaskList != null) && (currentTaskList.GetSourceEntity() == GameState.Get().GetFriendlySidePlayer()))
                    {
                        TurnStartManager.Get().NotifyOfCardDrawn(this);
                    }
                }
                if (change.newValue == 1)
                {
                    if (base.IsHero())
                    {
                        this.GetController().SetHero(this);
                        break;
                    }
                    if (base.IsHeroPower())
                    {
                        this.GetController().SetHeroPower(this);
                    }
                }
                break;

            case GAME_TAG.PARENT_CARD:
            {
                Entity entity4 = GameState.Get().GetEntity(change.oldValue);
                if (entity4 != null)
                {
                    entity4.RemoveSubCard(this);
                }
                Entity entity5 = GameState.Get().GetEntity(change.newValue);
                if (entity5 != null)
                {
                    entity5.AddSubCard(this);
                }
                break;
            }
        }
    }

    public Card InitCard()
    {
        this.m_card = new GameObject().AddComponent<Card>();
        this.m_card.SetEntity(this);
        this.UpdateCardName();
        return this.m_card;
    }

    public void InitRealTimeValues(Network.Entity netEntity)
    {
        this.SetRealTimeCost(base.GetCost());
        this.SetRealTimeAttack(base.GetATK());
        this.SetRealTimeHealth(base.GetHealth());
        this.SetRealTimeDamage(base.GetDamage());
        this.SetRealTimeArmor(base.GetArmor());
        this.SetRealTimePoweredUp(base.GetTag(GAME_TAG.POWERED_UP));
        foreach (Network.Entity.Tag tag in netEntity.Tags)
        {
            GAME_TAG name = (GAME_TAG) tag.Name;
            switch (name)
            {
                case GAME_TAG.DAMAGE:
                {
                    this.SetRealTimeDamage(tag.Value);
                    continue;
                }
                case GAME_TAG.HEALTH:
                    break;

                case GAME_TAG.ATK:
                {
                    this.SetRealTimeAttack(tag.Value);
                    continue;
                }
                case GAME_TAG.COST:
                {
                    this.SetRealTimeCost(tag.Value);
                    continue;
                }
                default:
                    if (name != GAME_TAG.DURABILITY)
                    {
                        if (name == GAME_TAG.ARMOR)
                        {
                            goto Label_00F4;
                        }
                        if (name == GAME_TAG.POWERED_UP)
                        {
                            goto Label_0105;
                        }
                        continue;
                    }
                    break;
            }
            this.SetRealTimeHealth(tag.Value);
            continue;
        Label_00F4:
            this.SetRealTimeArmor(tag.Value);
            continue;
        Label_0105:
            this.SetRealTimePoweredUp(tag.Value);
        }
    }

    public bool IsBusy()
    {
        return (this.IsLoadingAssets() || ((this.m_card != null) && !this.m_card.IsActorReady()));
    }

    public bool IsControlledByConcealedPlayer()
    {
        return !this.IsControlledByRevealedPlayer();
    }

    public bool IsControlledByFriendlySidePlayer()
    {
        return this.GetController().IsFriendlySide();
    }

    public bool IsControlledByLocalUser()
    {
        return this.GetController().IsLocalUser();
    }

    public bool IsControlledByOpposingSidePlayer()
    {
        return this.GetController().IsOpposingSide();
    }

    public bool IsControlledByRevealedPlayer()
    {
        return this.GetController().IsRevealed();
    }

    private bool IsCopyingDeathrattle()
    {
        if (GameState.Get().GetEntity(base.GetTag(GAME_TAG.COPY_DEATHRATTLE)) == null)
        {
            return false;
        }
        return true;
    }

    private bool IsCopyingDeathrattleEffect()
    {
        if (!this.IsCopyingDeathrattle())
        {
            return false;
        }
        if (base.GetTag(GAME_TAG.COPY_DEATHRATTLE_INDEX) <= 0)
        {
            return false;
        }
        return true;
    }

    public bool IsEnchanted()
    {
        return (this.m_attachments.Count > 0);
    }

    public bool IsHidden()
    {
        return string.IsNullOrEmpty(this.m_cardId);
    }

    public bool IsHistoryDupe()
    {
        return this.m_duplicateForHistory;
    }

    public bool IsLoadingAssets()
    {
        return (this.m_loadState == LoadState.LOADING);
    }

    private bool IsNameChange(TagDelta change)
    {
        GAME_TAG tag = (GAME_TAG) change.tag;
        if (((tag != GAME_TAG.ZONE) && (tag != GAME_TAG.ENTITY_ID)) && (tag != GAME_TAG.ZONE_POSITION))
        {
            return false;
        }
        return true;
    }

    public void LoadCard(string cardId)
    {
        this.LoadEntityDef(cardId);
        this.m_loadState = LoadState.LOADING;
        if (string.IsNullOrEmpty(cardId))
        {
            DefLoader.Get().LoadCardDef("HiddenCard", new DefLoader.LoadDefCallback<CardDef>(this.OnCardDefLoaded), null, null);
        }
        else
        {
            DefLoader.Get().LoadCardDef(cardId, new DefLoader.LoadDefCallback<CardDef>(this.OnCardDefLoaded), null, null);
        }
    }

    private void LoadDeathrattleCopyEffect()
    {
        Entity entity = GameState.Get().GetEntity(base.GetTag(GAME_TAG.COPY_DEATHRATTLE));
        if (entity == null)
        {
            Debug.LogError(string.Format("Entity.LoadDeathrattleCopy() - COPY_DEATHRATTLE tag does not point to a valid Entity.", new object[0]));
        }
        else
        {
            int tag = base.GetTag(GAME_TAG.COPY_DEATHRATTLE_INDEX);
            if (tag <= 0)
            {
                Debug.LogError(string.Format("Entity.LoadDeathrattleCopy() - Invalid COPY_DEATHRATTLE_INDEX tag.", new object[0]));
            }
            else
            {
                while (entity.IsCopyingDeathrattleEffect() && (entity.GetTag(GAME_TAG.COPY_DEATHRATTLE_INDEX) == tag))
                {
                    entity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.COPY_DEATHRATTLE));
                    if (entity == null)
                    {
                        Debug.LogError(string.Format("Entity.LoadDeathrattleCopy() - COPY_DEATHRATTLE tag points to an Entity that is trying to copy a deathrattle from an invalid Entity.", new object[0]));
                        return;
                    }
                }
                tag--;
                this.AddPowerHistoryInfoOverride(entity.GetPowerHistoryInfo(tag));
                Card card = entity.GetCard();
                if (card == null)
                {
                    Debug.LogError(string.Format("Entity.LoadDeathrattleCopy() - Copy Entity does not have a Card.", new object[0]));
                }
                else
                {
                    CardDef cardDef = card.GetCardDef();
                    if (cardDef != null)
                    {
                        List<CardEffectDef> triggerEffectDefs = cardDef.m_TriggerEffectDefs;
                        if ((triggerEffectDefs == null) || (triggerEffectDefs.Count <= tag))
                        {
                            Debug.LogError(string.Format("Entity.LoadDeathrattleCopy() - Copy Entity's Card does not have an CardEffectDef for index {0}.", tag));
                        }
                        else
                        {
                            CardEffectDef def2 = triggerEffectDefs[tag];
                            if ((def2 != null) && !string.IsNullOrEmpty(def2.m_SpellPath))
                            {
                                GameObject obj2 = AssetLoader.Get().LoadSpell(def2.m_SpellPath, true, false);
                                if (obj2 != null)
                                {
                                    Spell component = obj2.GetComponent<Spell>();
                                    if (component != null)
                                    {
                                        this.OnDeathrattleCopyEffectLoaded(tag, component);
                                        if ((this.m_card.GetActor() != null) && this.m_card.IsActorReady())
                                        {
                                            this.m_card.GetActor().UpdateAllComponents();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void LoadEntityDef(string cardId)
    {
        if (this.m_cardId != cardId)
        {
            this.m_cardId = cardId;
        }
        if (!string.IsNullOrEmpty(cardId))
        {
            this.m_entityDef = DefLoader.Get().GetEntityDef(cardId);
            if (this.m_entityDef == null)
            {
                object[] messageArgs = new object[] { cardId };
                Error.AddDevFatal("Failed to load a card xml for {0}", messageArgs);
            }
            else
            {
                this.UpdateCardName();
            }
        }
    }

    private void OnCardDefLoaded(string cardId, CardDef cardDef, object callbackData)
    {
        if (cardDef == null)
        {
            if (cardId == "PlaceholderCard")
            {
                Debug.LogError(string.Format("Entity.OnCardDefLoaded() - {0} does not have an asset!", "PlaceholderCard"));
                this.FinishCardLoad();
            }
            else
            {
                Debug.LogWarning(string.Format("Entity.OnCardDefLoaded() - MISSING ASSET for card {0}. Falling back to {1}", cardId, "PlaceholderCard"));
                DefLoader.Get().LoadCardDef("PlaceholderCard", new DefLoader.LoadDefCallback<CardDef>(this.OnCardDefLoaded), null, null);
            }
        }
        else
        {
            if (this.m_card != null)
            {
                this.m_card.LoadCardDef(cardDef);
            }
            this.UpdateUseBattlecryFlag(false);
            this.FinishCardLoad();
        }
    }

    private void OnDeathrattleCopyEffectLoaded(int index, Spell spell)
    {
        this.m_card.OverrideTriggerSpell(index, spell);
    }

    public void OnFullEntity(Network.HistFullEntity fullEntity)
    {
        this.m_loadState = LoadState.PENDING;
        this.LoadCard(fullEntity.Entity.CardID);
        int tag = base.GetTag(GAME_TAG.ATTACHED);
        if (tag != 0)
        {
            GameState.Get().GetEntity(tag).AddAttachment(this);
        }
        int id = base.GetTag(GAME_TAG.PARENT_CARD);
        if (id != 0)
        {
            GameState.Get().GetEntity(id).AddSubCard(this);
        }
        if (base.GetZone() == TAG_ZONE.PLAY)
        {
            if (base.IsHero())
            {
                this.GetController().SetHero(this);
            }
            else if (base.IsHeroPower())
            {
                this.GetController().SetHeroPower(this);
            }
        }
    }

    public virtual void OnMetaData(Network.HistMetaData metaData)
    {
        if (this.m_card != null)
        {
            this.m_card.OnMetaData(metaData);
        }
    }

    public virtual void OnRealTimeFullEntity(Network.HistFullEntity fullEntity)
    {
        base.SetTags(fullEntity.Entity.Tags);
        this.InitRealTimeValues(fullEntity.Entity);
        if (!base.IsEnchantment())
        {
            this.InitCard();
        }
        this.LoadEntityDef(fullEntity.Entity.CardID);
    }

    public virtual void OnRealTimeShowEntity(Network.HistShowEntity showEntity)
    {
        this.InitRealTimeValues(showEntity.Entity);
        if (base.IsEnchantment() && (this.m_card != null))
        {
            this.m_card.Destroy();
        }
    }

    public virtual void OnRealTimeTagChanged(Network.HistTagChange change)
    {
        switch (((GAME_TAG) change.Tag))
        {
            case GAME_TAG.DAMAGE:
                this.SetRealTimeDamage(change.Value);
                return;

            case GAME_TAG.HEALTH:
            case GAME_TAG.DURABILITY:
                this.SetRealTimeHealth(change.Value);
                return;

            case GAME_TAG.ATK:
                this.SetRealTimeAttack(change.Value);
                return;

            case GAME_TAG.COST:
                this.SetRealTimeCost(change.Value);
                return;

            case GAME_TAG.ARMOR:
                this.SetRealTimeArmor(change.Value);
                return;

            case GAME_TAG.POWERED_UP:
                this.SetRealTimePoweredUp(change.Value);
                return;
        }
    }

    public void OnShowEntity(Network.HistShowEntity showEntity)
    {
        TagDeltaSet changeSet = base.m_tags.CreateDeltas(showEntity.Entity.Tags);
        base.SetTags(showEntity.Entity.Tags);
        this.LoadCard(showEntity.Entity.CardID);
        this.OnTagsChanged(changeSet);
    }

    public virtual void OnTagChanged(TagDelta change)
    {
        this.HandleTagChange(change);
        if (this.m_card != null)
        {
            if (this.IsNameChange(change))
            {
                this.UpdateCardName();
            }
            this.m_card.OnTagChanged(change);
        }
    }

    public virtual void OnTagsChanged(TagDeltaSet changeSet)
    {
        bool flag = false;
        for (int i = 0; i < changeSet.Size(); i++)
        {
            TagDelta change = changeSet[i];
            if (this.IsNameChange(change))
            {
                flag = true;
            }
            this.HandleTagChange(change);
        }
        if (this.m_card != null)
        {
            if (flag)
            {
                this.UpdateCardName();
            }
            this.m_card.OnTagsChanged(changeSet);
        }
    }

    public void RemoveAttachment(Entity entity)
    {
        int count = this.m_attachments.Count;
        if (!this.m_attachments.Remove(entity))
        {
            Log.Mike.Print(string.Format("Entity.RemoveAttachment() - {0} is not an attachment of {1}", entity, this), new object[0]);
        }
        else if (this.m_card != null)
        {
            this.m_card.OnEnchantmentRemoved(count, entity);
        }
    }

    public void RemoveSubCard(Entity entity)
    {
        this.m_subCardIDs.Remove(entity.GetEntityId());
    }

    public void SetCard(Card card)
    {
        this.m_card = card;
    }

    public void SetCardId(string cardId)
    {
        this.m_cardId = cardId;
    }

    public void SetRealTimeArmor(int newArmor)
    {
        this.m_realTimeArmor = newArmor;
    }

    public void SetRealTimeAttack(int newAttack)
    {
        this.m_realTimeAttack = newAttack;
    }

    public void SetRealTimeCost(int newCost)
    {
        this.m_realTimeCost = newCost;
    }

    public void SetRealTimeDamage(int newDamage)
    {
        this.m_realTimeDamage = newDamage;
    }

    public void SetRealTimeHealth(int newHealth)
    {
        this.m_realTimeHealth = newHealth;
    }

    public void SetRealTimePoweredUp(int poweredUp)
    {
        this.m_realTimePoweredUp = poweredUp == 1;
    }

    public void SetTagAndHandleChange<TagEnum>(GAME_TAG tag, TagEnum tagValue)
    {
        this.SetTagAndHandleChange((int) tag, Convert.ToInt32(tagValue));
    }

    public TagDelta SetTagAndHandleChange(int tag, int tagValue)
    {
        int num = base.m_tags.GetTag(tag);
        base.SetTag(tag, tagValue);
        TagDelta change = new TagDelta {
            tag = tag,
            oldValue = num,
            newValue = tagValue
        };
        this.OnTagChanged(change);
        return change;
    }

    public void SetTagsAndHandleChanges(Map<GAME_TAG, int> tagMap)
    {
        TagDeltaSet changeSet = base.m_tags.CreateDeltas(tagMap);
        base.m_tags.SetTags(tagMap);
        this.OnTagsChanged(changeSet);
    }

    public bool ShouldUseBattlecryPower()
    {
        return this.m_useBattlecryPower;
    }

    public override string ToString()
    {
        return this.GetDebugName();
    }

    public void UpdateCardName()
    {
        if (this.m_card != null)
        {
            string stringTag = base.GetStringTag(GAME_TAG.CARDNAME);
            if (stringTag != null)
            {
                if (string.IsNullOrEmpty(this.m_cardId))
                {
                    object[] args = new object[] { stringTag, base.GetEntityId(), base.GetZone(), base.GetZonePosition() };
                    this.m_card.gameObject.name = string.Format("{0} [id={1} zone={2} zonePos={3}]", args);
                }
                else
                {
                    object[] objArray2 = new object[] { stringTag, base.GetEntityId(), this.GetCardId(), base.GetZone(), base.GetZonePosition() };
                    this.m_card.gameObject.name = string.Format("{0} [id={1} cardId={2} zone={3} zonePos={4}]", objArray2);
                }
            }
            else
            {
                this.m_card.gameObject.name = string.Format("Hidden Entity [id={0} zone={1} zonePos={2}]", base.GetEntityId(), base.GetZone(), base.GetZonePosition());
            }
        }
    }

    public void UpdateUseBattlecryFlag(bool fromGameState)
    {
        if (base.IsMinion())
        {
            bool flag = fromGameState || GameState.Get().EntityHasTargets(this);
            if ((base.GetZone() == TAG_ZONE.HAND) && flag)
            {
                this.m_useBattlecryPower = true;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ConvertToSourceInfo>c__AnonStorey2F3
    {
        internal string entourageCardID;

        internal bool <>m__F1(string otherCardID)
        {
            return otherCardID.Equals(this.entourageCardID, StringComparison.OrdinalIgnoreCase);
        }
    }

    public enum LoadState
    {
        INVALID,
        PENDING,
        LOADING,
        DONE
    }
}

