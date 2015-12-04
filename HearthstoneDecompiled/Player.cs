using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    private int m_cardBackId;
    private bool m_concedeEmotePlayed;
    private BnetGameAccountId m_gameAccountId;
    private Entity m_hero;
    private Entity m_heroPower;
    private bool m_local;
    private ManaCounter m_manaCounter;
    private MedalInfoTranslator m_medalInfo;
    private string m_name;
    private int m_queuedSpentMana;
    private bool m_realTimeComboActive;
    private int m_realtimeTempMana;
    private Side m_side;
    private Entity m_startingHero;
    private int m_usedTempMana;
    private bool m_waitingForHeroEntity;

    public void AddManaCrystal(int numCrystals)
    {
        this.AddManaCrystal(numCrystals, false);
    }

    public void AddManaCrystal(int numCrystals, bool isTurnStart)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().AddManaCrystals(numCrystals, isTurnStart);
        }
    }

    public void AddTempManaCrystal(int numCrystals)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().AddTempManaCrystals(numCrystals);
        }
    }

    private void AssignPlayerBoardObjects()
    {
        foreach (ManaCounter counter in BoardStandardGame.Get().gameObject.GetComponentsInChildren<ManaCounter>(true))
        {
            if (counter.m_Side == this.m_side)
            {
                this.m_manaCounter = counter;
                this.m_manaCounter.SetPlayer(this);
                this.m_manaCounter.UpdateText();
                break;
            }
        }
        this.InitManaCrystalMgr();
        foreach (Zone zone in ZoneMgr.Get().GetZones())
        {
            if (zone.m_Side == this.m_side)
            {
                zone.SetController(this);
            }
        }
    }

    public void CancelAllProposedMana(Entity entity)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().CancelAllProposedMana(entity);
        }
    }

    public PlayErrors.PlayerInfo ConvertToPlayerInfo()
    {
        PlayErrors.PlayerInfo info = new PlayErrors.PlayerInfo();
        int numMinionsInPlay = this.GetNumMinionsInPlay();
        int numTotalMinionsInPlay = this.GetNumTotalMinionsInPlay();
        info.isCurrentPlayer = this.IsCurrentPlayer();
        info.id = this.GetPlayerId();
        info.numResources = this.GetNumAvailableResources();
        info.weaponEquipped = this.HasWeapon();
        info.enemyWeaponEquipped = GameState.Get().GetOpposingSidePlayer().HasWeapon();
        info.comboActive = this.IsComboActive();
        info.steadyShotRequiresTarget = base.HasTag(GAME_TAG.STEADY_SHOT_CAN_TARGET);
        info.numFriendlyMinionsInPlay = numMinionsInPlay;
        info.numEnemyMinionsInPlay = numTotalMinionsInPlay - numMinionsInPlay;
        info.numMinionSlotsPerPlayer = GameState.Get().GetMaxFriendlyMinionsPerPlayer();
        info.numOpenSecretSlots = GameState.Get().GetMaxSecretsPerPlayer() - this.GetSecretDefinitions().Count;
        info.numDragonsInHand = this.GetNumDragonsInHand();
        info.numFriendlyMinionsThatDiedThisTurn = base.GetTag(GAME_TAG.NUM_FRIENDLY_MINIONS_THAT_DIED_THIS_TURN);
        info.numFriendlyMinionsThatDiedThisGame = base.GetTag(GAME_TAG.NUM_FRIENDLY_MINIONS_THAT_DIED_THIS_GAME);
        return info;
    }

    public void DestroyManaCrystal(int numCrystals)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().DestroyManaCrystals(numCrystals);
        }
    }

    public void DestroyTempManaCrystal(int numCrystals)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().DestroyTempManaCrystals(numCrystals);
        }
    }

    public ZonePlay GetBattlefieldZone()
    {
        return ZoneMgr.Get().FindZoneOfType<ZonePlay>(this.GetSide());
    }

    public BnetPlayer GetBnetPlayer()
    {
        return BnetPresenceMgr.Get().GetPlayer(this.m_gameAccountId);
    }

    public int GetCardBackId()
    {
        return this.m_cardBackId;
    }

    public override string GetDebugName()
    {
        if (this.m_name != null)
        {
            return this.m_name;
        }
        if (this.IsAI())
        {
            return GameStrings.Get("GAMEPLAY_AI_OPPONENT_NAME");
        }
        return "UNKNOWN HUMAN PLAYER";
    }

    public ZoneDeck GetDeckZone()
    {
        return ZoneMgr.Get().FindZoneOfType<ZoneDeck>(this.GetSide());
    }

    public BnetGameAccountId GetGameAccountId()
    {
        return this.m_gameAccountId;
    }

    public ZoneGraveyard GetGraveyardZone()
    {
        return ZoneMgr.Get().FindZoneOfType<ZoneGraveyard>(this.GetSide());
    }

    public ZoneHand GetHandZone()
    {
        return ZoneMgr.Get().FindZoneOfType<ZoneHand>(this.GetSide());
    }

    public override Entity GetHero()
    {
        return this.m_hero;
    }

    public override Card GetHeroCard()
    {
        if (this.m_hero == null)
        {
            return null;
        }
        return this.m_hero.GetCard();
    }

    public EntityDef GetHeroEntityDef()
    {
        if (this.m_hero == null)
        {
            return null;
        }
        EntityDef entityDef = this.m_hero.GetEntityDef();
        if (entityDef == null)
        {
            return null;
        }
        return entityDef;
    }

    public override Entity GetHeroPower()
    {
        return this.m_heroPower;
    }

    public override Card GetHeroPowerCard()
    {
        if (this.m_heroPower == null)
        {
            return null;
        }
        return this.m_heroPower.GetCard();
    }

    public override string GetName()
    {
        return this.m_name;
    }

    public int GetNumAvailableResources()
    {
        int tag = base.GetTag(GAME_TAG.TEMP_RESOURCES);
        int num2 = base.GetTag(GAME_TAG.RESOURCES);
        int num3 = base.GetTag(GAME_TAG.RESOURCES_USED);
        int num4 = (((num2 + tag) - num3) - this.m_queuedSpentMana) - this.m_usedTempMana;
        return ((num4 >= 0) ? num4 : 0);
    }

    public int GetNumDragonsInHand()
    {
        int num = 0;
        foreach (Card card in this.GetHandZone().GetCards())
        {
            if (card.GetEntity().GetRace() == TAG_RACE.DRAGON)
            {
                num++;
            }
        }
        return num;
    }

    public int GetNumMinionsInPlay()
    {
        int num = 0;
        foreach (Card card in this.GetBattlefieldZone().GetCards())
        {
            Entity entity = card.GetEntity();
            if ((entity.GetControllerId() == this.GetPlayerId()) && entity.IsMinion())
            {
                num++;
            }
        }
        return num;
    }

    private int GetNumTotalMinionsInPlay()
    {
        int num = 0;
        foreach (Zone zone in ZoneMgr.Get().GetZones())
        {
            if (zone is ZonePlay)
            {
                foreach (Card card in zone.GetCards())
                {
                    if (card.GetEntity().IsMinion())
                    {
                        num++;
                    }
                }
            }
        }
        return num;
    }

    public int GetPlayerId()
    {
        return base.GetTag(GAME_TAG.PLAYER_ID);
    }

    public int GetQueuedSpentMana()
    {
        return this.m_queuedSpentMana;
    }

    public int GetQueuedUsedTempMana()
    {
        return this.m_usedTempMana;
    }

    public MedalInfoTranslator GetRank()
    {
        return this.m_medalInfo;
    }

    public int GetRealTimeTempMana()
    {
        return this.m_realtimeTempMana;
    }

    public List<string> GetSecretDefinitions()
    {
        List<string> list = new List<string>();
        foreach (Zone zone in ZoneMgr.Get().GetZones())
        {
            if ((zone is ZoneSecret) && (zone.m_Side == Side.FRIENDLY))
            {
                foreach (Card card in zone.GetCards())
                {
                    list.Add(card.GetEntity().GetCardId());
                }
            }
        }
        return list;
    }

    public ZoneSecret GetSecretZone()
    {
        return ZoneMgr.Get().FindZoneOfType<ZoneSecret>(this.GetSide());
    }

    public Side GetSide()
    {
        return this.m_side;
    }

    public Entity GetStartingHero()
    {
        return this.m_startingHero;
    }

    public override Card GetWeaponCard()
    {
        return ZoneMgr.Get().FindZoneOfType<ZoneWeapon>(this.GetSide()).GetFirstCard();
    }

    public void HandleSameTurnOverloadChanged(int crystalsChanged)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().HandleSameTurnOverloadChanged(crystalsChanged);
        }
    }

    public bool HasATauntMinion()
    {
        List<Card> cards = this.GetBattlefieldZone().GetCards();
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].GetEntity().HasTaunt())
            {
                return true;
            }
        }
        return false;
    }

    public bool HasReadyAttackers()
    {
        List<Card> cards = this.GetBattlefieldZone().GetCards();
        for (int i = 0; i < cards.Count; i++)
        {
            if (GameState.Get().HasResponse(cards[i].GetEntity()))
            {
                return true;
            }
        }
        return false;
    }

    public bool HasWeapon()
    {
        foreach (Zone zone in ZoneMgr.Get().GetZones())
        {
            if ((zone is ZoneWeapon) && (zone.m_Side == this.m_side))
            {
                return (zone.GetCards().Count > 0);
            }
        }
        return false;
    }

    private void InitManaCrystalMgr()
    {
        if (this.IsFriendlySide())
        {
            int tag = base.GetTag(GAME_TAG.TEMP_RESOURCES);
            int numCrystals = base.GetTag(GAME_TAG.RESOURCES);
            int shownChangeAmount = base.GetTag(GAME_TAG.RESOURCES_USED);
            int num4 = base.GetTag(GAME_TAG.OVERLOAD_OWED);
            ManaCrystalMgr.Get().AddManaCrystals(numCrystals, false);
            ManaCrystalMgr.Get().AddTempManaCrystals(tag);
            ManaCrystalMgr.Get().UpdateSpentMana(shownChangeAmount);
            ManaCrystalMgr.Get().MarkCrystalsOwedForOverload(num4);
        }
    }

    public void InitPlayer(Network.HistCreateGame.PlayerData netPlayer)
    {
        this.SetPlayerId(netPlayer.ID);
        this.SetGameAccountId(netPlayer.GameAccountId);
        this.SetCardBackId(netPlayer.CardBackID);
        base.SetTags(netPlayer.Player.Tags);
        GameState.Get().RegisterTurnChangedListener(new GameState.TurnChangedCallback(this.OnTurnChanged));
    }

    public bool IsAI()
    {
        if (this.m_gameAccountId == null)
        {
            return false;
        }
        return !this.m_gameAccountId.IsValid();
    }

    public bool IsBnetPlayer()
    {
        if (!this.IsHuman())
        {
            return false;
        }
        return Network.ShouldBeConnectedToAurora();
    }

    public bool IsComboActive()
    {
        return base.HasTag(GAME_TAG.COMBO_ACTIVE);
    }

    public bool IsCurrentPlayer()
    {
        return base.HasTag(GAME_TAG.CURRENT_PLAYER);
    }

    public bool IsDisplayable()
    {
        if (this.m_gameAccountId == null)
        {
            return false;
        }
        if (!this.IsBnetPlayer())
        {
            if (this.ShouldUseHeroName() && (this.GetHeroEntityDef() == null))
            {
                return false;
            }
            return true;
        }
        BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(this.m_gameAccountId);
        if (player == null)
        {
            return false;
        }
        if (!player.IsDisplayable())
        {
            return false;
        }
        if (GameUtils.ShouldShowRankedMedals())
        {
            BnetGameAccount hearthstoneGameAccount = player.GetHearthstoneGameAccount();
            if (hearthstoneGameAccount == null)
            {
                return false;
            }
            if (!hearthstoneGameAccount.HasGameField(0x12))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsFriendlySide()
    {
        return (this.m_side == Side.FRIENDLY);
    }

    public bool IsGuestPlayer()
    {
        if (!this.IsHuman())
        {
            return false;
        }
        return !Network.ShouldBeConnectedToAurora();
    }

    public bool IsHeroPowerAffectedByBonusDamage()
    {
        Card heroPowerCard = this.GetHeroPowerCard();
        if (heroPowerCard == null)
        {
            return false;
        }
        Entity entity = heroPowerCard.GetEntity();
        if (!entity.IsHeroPower())
        {
            return false;
        }
        return TextUtils.HasBonusDamage(entity.GetStringTag(GAME_TAG.CARDTEXT_INHAND));
    }

    public bool IsHuman()
    {
        if (this.m_gameAccountId == null)
        {
            return false;
        }
        return this.m_gameAccountId.IsValid();
    }

    public bool IsLocalUser()
    {
        return this.m_local;
    }

    public bool IsOpposingSide()
    {
        return (this.m_side == Side.OPPOSING);
    }

    public bool IsRealTimeComboActive()
    {
        return this.m_realTimeComboActive;
    }

    public bool IsRevealed()
    {
        return (this.IsFriendlySide() || SpectatorManager.Get().IsSpectatingPlayer(this.m_gameAccountId));
    }

    public void NotifyOfSpentMana(int spentMana)
    {
        this.m_queuedSpentMana += spentMana;
    }

    public void NotifyOfUsedTempMana(int usedMana)
    {
        this.m_usedTempMana += usedMana;
    }

    private void OnBnetPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        if ((changelist.FindChange(this.m_gameAccountId) != null) && this.IsDisplayable())
        {
            BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnBnetPlayersChanged));
            this.UpdateDisplayInfo();
        }
    }

    public void OnBoardLoaded()
    {
        this.AssignPlayerBoardObjects();
    }

    private void OnFriendlyPlayerTagChanged(TagDelta change)
    {
        GAME_TAG tag = (GAME_TAG) change.tag;
        switch (tag)
        {
            case GAME_TAG.CURRENT_SPELLPOWER:
                break;

            case GAME_TAG.TEMP_RESOURCES:
            {
                int num = change.oldValue - this.m_usedTempMana;
                int num2 = change.newValue - change.oldValue;
                if (num2 < 0)
                {
                    this.m_usedTempMana += num2;
                }
                if (this.m_usedTempMana < 0)
                {
                    this.m_usedTempMana = 0;
                }
                if (num < 0)
                {
                    num = 0;
                }
                int numCrystals = (change.newValue - num) - this.m_usedTempMana;
                if (numCrystals > 0)
                {
                    this.AddTempManaCrystal(numCrystals);
                }
                else
                {
                    this.DestroyTempManaCrystal(-numCrystals);
                }
                return;
            }
            case GAME_TAG.OVERLOAD_OWED:
                this.HandleSameTurnOverloadChanged(change.newValue - change.oldValue);
                return;

            case GAME_TAG.CURRENT_PLAYER:
                if (change.newValue == 1)
                {
                    ManaCrystalMgr.Get().OnCurrentPlayerChanged();
                    this.m_queuedSpentMana = 0;
                    if (GameState.Get().IsMainPhase())
                    {
                        TurnStartManager.Get().BeginListeningForTurnEvents();
                    }
                }
                return;

            case GAME_TAG.RESOURCES_USED:
            {
                int num4 = change.oldValue + this.m_queuedSpentMana;
                int num5 = change.newValue - change.oldValue;
                if (num5 > 0)
                {
                    this.m_queuedSpentMana -= num5;
                }
                if (this.m_queuedSpentMana < 0)
                {
                    this.m_queuedSpentMana = 0;
                }
                int shownChangeAmount = (change.newValue - num4) + this.m_queuedSpentMana;
                ManaCrystalMgr.Get().UpdateSpentMana(shownChangeAmount);
                return;
            }
            case GAME_TAG.RESOURCES:
                if (change.newValue <= change.oldValue)
                {
                    this.DestroyManaCrystal(change.oldValue - change.newValue);
                    return;
                }
                if (!GameState.Get().IsTurnStartManagerActive() || !this.IsFriendlySide())
                {
                    this.AddManaCrystal(change.newValue - change.oldValue);
                    return;
                }
                TurnStartManager.Get().NotifyOfManaCrystalGained(change.newValue - change.oldValue);
                return;

            default:
                switch (tag)
                {
                    case GAME_TAG.SPELLPOWER_DOUBLE:
                    case GAME_TAG.HEALING_DOUBLE:
                        break;

                    case GAME_TAG.MULLIGAN_STATE:
                        if ((change.newValue == 4) && (MulliganManager.Get() == null))
                        {
                            foreach (Card card in this.GetHandZone().GetCards())
                            {
                                card.GetActor().TurnOnCollider();
                            }
                        }
                        return;

                    default:
                        if ((tag == GAME_TAG.OVERLOAD_LOCKED) && (change.newValue < change.oldValue))
                        {
                            this.UnlockCrystals(change.oldValue - change.newValue);
                        }
                        return;
                }
                break;
        }
        List<Card> cards = this.GetHandZone().GetCards();
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].GetEntity().IsSpell())
            {
                cards[i].GetActor().UpdatePowersText();
            }
        }
    }

    private void OnOpposingPlayerTagChanged(TagDelta change)
    {
        switch (((GAME_TAG) change.tag))
        {
            case GAME_TAG.PLAYSTATE:
                if (change.newValue == 7)
                {
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_ANNOUNCER_DISCONNECT_45"), "VO_ANNOUNCER_DISCONNECT_45", 0f, null);
                }
                break;

            case GAME_TAG.RESOURCES:
                if (change.newValue > change.oldValue)
                {
                    GameState.Get().GetGameEntity().NotifyOfEnemyManaCrystalSpawned();
                }
                break;
        }
    }

    public override void OnRealTimeTagChanged(Network.HistTagChange change)
    {
        switch (((GAME_TAG) change.Tag))
        {
            case GAME_TAG.COMBO_ACTIVE:
                this.SetRealTimeComboActive(change.Value);
                break;

            case GAME_TAG.TEMP_RESOURCES:
                this.SetRealTimeTempMana(change.Value);
                break;
        }
    }

    public override void OnTagChanged(TagDelta change)
    {
        if (this.IsFriendlySide())
        {
            this.OnFriendlyPlayerTagChanged(change);
        }
        else
        {
            this.OnOpposingPlayerTagChanged(change);
        }
        GAME_TAG tag = (GAME_TAG) change.tag;
        switch (tag)
        {
            case GAME_TAG.LOCK_AND_LOAD:
            {
                Card heroCard = this.GetHeroCard();
                this.ToggleActorSpellOnCard(heroCard, change, SpellType.LOCK_AND_LOAD);
                return;
            }
            case GAME_TAG.SHADOWFORM:
            {
                Card card5 = this.GetHeroCard();
                this.ToggleActorSpellOnCard(card5, change, SpellType.SHADOWFORM);
                return;
            }
        }
        if ((tag != GAME_TAG.RESOURCES_USED) && (tag != GAME_TAG.RESOURCES))
        {
            if (tag == GAME_TAG.PLAYSTATE)
            {
                if (change.newValue == 8)
                {
                    this.PlayConcedeEmote();
                }
                return;
            }
            if (tag == GAME_TAG.COMBO_ACTIVE)
            {
                foreach (Card card in this.GetHandZone().GetCards())
                {
                    card.UpdateActorState();
                }
                return;
            }
            if (tag != GAME_TAG.TEMP_RESOURCES)
            {
                if (tag == GAME_TAG.MULLIGAN_STATE)
                {
                    if ((change.newValue == 4) && (MulliganManager.Get() != null))
                    {
                        MulliganManager.Get().ServerHasDealtReplacementCards(this.IsFriendlySide());
                    }
                    return;
                }
                if (tag == GAME_TAG.STEADY_SHOT_CAN_TARGET)
                {
                    Card heroPowerCard = this.GetHeroPowerCard();
                    this.ToggleActorSpellOnCard(heroPowerCard, change, SpellType.STEADY_SHOT_CAN_TARGET);
                    return;
                }
                if (tag == GAME_TAG.CURRENT_HEROPOWER_DAMAGE_BONUS)
                {
                    if (this.IsHeroPowerAffectedByBonusDamage())
                    {
                        Card card3 = this.GetHeroPowerCard();
                        this.ToggleActorSpellOnCard(card3, change, SpellType.CURRENT_HEROPOWER_DAMAGE_BONUS);
                    }
                    return;
                }
                return;
            }
        }
        if (!GameState.Get().IsTurnStartManagerActive() || !this.IsFriendlySide())
        {
            this.UpdateManaCounter();
        }
    }

    public override void OnTagsChanged(TagDeltaSet changeSet)
    {
        for (int i = 0; i < changeSet.Size(); i++)
        {
            TagDelta change = changeSet[i];
            this.OnTagChanged(change);
        }
    }

    private void OnTurnChanged(int oldTurn, int newTurn, object userData)
    {
        this.WipeZzzs();
    }

    public void PlayConcedeEmote()
    {
        if (!this.m_concedeEmotePlayed)
        {
            Card heroCard = this.GetHeroCard();
            if (heroCard != null)
            {
                heroCard.PlayEmote(EmoteType.CONCEDE);
                this.m_concedeEmotePlayed = true;
            }
        }
    }

    public void ProposeManaCrystalUsage(Entity entity)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().ProposeManaCrystalUsage(entity);
        }
    }

    public void ReadyManaCrystal(int numCrystals)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().ReadyManaCrystals(numCrystals);
        }
    }

    private void RequestPlayerPresence()
    {
        BattleNet.DllEntityId entityId = new BattleNet.DllEntityId {
            hi = this.m_gameAccountId.GetHi(),
            lo = this.m_gameAccountId.GetLo()
        };
        List<BattleNet.DllPresenceFieldKey> list = new List<BattleNet.DllPresenceFieldKey>();
        BattleNet.DllPresenceFieldKey item = new BattleNet.DllPresenceFieldKey {
            programId = BnetProgramId.BNET.GetValue(),
            groupId = 2,
            fieldId = 7,
            index = 0L
        };
        list.Add(item);
        item.programId = BnetProgramId.BNET.GetValue();
        item.groupId = 2;
        item.fieldId = 3;
        item.index = 0L;
        list.Add(item);
        item.programId = BnetProgramId.BNET.GetValue();
        item.groupId = 2;
        item.fieldId = 5;
        item.index = 0L;
        list.Add(item);
        if (GameUtils.ShouldShowRankedMedals())
        {
            BattleNet.DllPresenceFieldKey key2 = new BattleNet.DllPresenceFieldKey {
                programId = BnetProgramId.HEARTHSTONE.GetValue(),
                groupId = 2,
                fieldId = 0x12,
                index = 0L
            };
            list.Add(key2);
        }
        BattleNet.DllPresenceFieldKey[] fieldList = list.ToArray();
        BattleNet.RequestPresenceFields(true, entityId, fieldList);
    }

    public void SetCardBackId(int id)
    {
        this.m_cardBackId = id;
    }

    public void SetGameAccountId(BnetGameAccountId id)
    {
        this.m_gameAccountId = id;
        this.UpdateLocal();
        this.UpdateSide();
        if (this.IsDisplayable())
        {
            this.UpdateDisplayInfo();
        }
        else if (this.IsBnetPlayer())
        {
            BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnBnetPlayersChanged));
            if (!BnetFriendMgr.Get().IsFriend(this.m_gameAccountId) && (GameMgr.Get().IsSpectator() || (GameMgr.Get().GetReconnectType() == ReconnectType.LOGIN)))
            {
                this.RequestPlayerPresence();
            }
        }
    }

    public void SetHero(Entity hero)
    {
        this.m_hero = hero;
        if ((this.m_startingHero == null) && (hero != null))
        {
            this.m_startingHero = hero;
        }
        if (this.ShouldUseHeroName())
        {
            this.UpdateDisplayInfo();
        }
    }

    public void SetHeroPower(Entity heroPower)
    {
        this.m_heroPower = heroPower;
    }

    public void SetLocalUser(bool local)
    {
        this.m_local = local;
        this.UpdateSide();
    }

    public void SetName(string name)
    {
        this.m_name = name;
    }

    public void SetPlayerId(int playerId)
    {
        base.SetTag(GAME_TAG.PLAYER_ID, playerId);
    }

    public void SetRealTimeComboActive(bool active)
    {
        this.m_realTimeComboActive = active;
    }

    public void SetRealTimeComboActive(int tagValue)
    {
        this.SetRealTimeComboActive(tagValue == 1);
    }

    public void SetRealTimeTempMana(int tempMana)
    {
        this.m_realtimeTempMana = tempMana;
    }

    public void SetSide(Side side)
    {
        this.m_side = side;
    }

    private bool ShouldUseBogusRank()
    {
        if (this.IsBnetPlayer())
        {
            return false;
        }
        return true;
    }

    private bool ShouldUseHeroName()
    {
        if (this.IsBnetPlayer())
        {
            return false;
        }
        if (this.IsAI() && GameMgr.Get().IsPractice())
        {
            return false;
        }
        return true;
    }

    public void SpendManaCrystal(int numCrystals)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().SpendManaCrystals(numCrystals);
        }
    }

    protected void ToggleActorSpellOnCard(Card card, TagDelta change, SpellType spellType)
    {
        if ((card != null) && card.CanShowActorVisuals())
        {
            Actor actor = card.GetActor();
            if (change.newValue > 0)
            {
                actor.ActivateSpell(spellType);
            }
            else
            {
                actor.DeactivateSpell(spellType);
            }
        }
    }

    public void UnlockCrystals(int numCrystals)
    {
        if (this.IsFriendlySide())
        {
            ManaCrystalMgr.Get().UnlockCrystals(numCrystals);
        }
    }

    private void UpdateDisplayInfo()
    {
        this.UpdateName();
        this.UpdateRank();
        if (this.IsBnetPlayer() && !this.IsLocalUser())
        {
            BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(this.m_gameAccountId);
            if (BnetFriendMgr.Get().IsFriend(player))
            {
                ChatMgr.Get().AddRecentWhisperPlayerToBottom(player);
            }
        }
    }

    private void UpdateLocal()
    {
        if (this.IsBnetPlayer())
        {
            BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
            this.m_local = myGameAccountId == this.m_gameAccountId;
        }
        else
        {
            this.m_local = this.m_gameAccountId.GetLo() == 1L;
        }
    }

    public void UpdateManaCounter()
    {
        if (this.m_manaCounter != null)
        {
            this.m_manaCounter.UpdateText();
        }
    }

    private void UpdateName()
    {
        if (this.ShouldUseHeroName())
        {
            this.UpdateNameWithHeroName();
        }
        else if (this.IsAI())
        {
            this.m_name = GameStrings.Get("GAMEPLAY_AI_OPPONENT_NAME");
        }
        else if (this.IsBnetPlayer())
        {
            this.m_name = BnetPresenceMgr.Get().GetPlayer(this.m_gameAccountId).GetBestName();
        }
        else
        {
            Debug.LogError(string.Format("Player.UpdateName() - unable to determine player name", new object[0]));
        }
    }

    private void UpdateNameWithHeroName()
    {
        if (this.m_hero != null)
        {
            EntityDef entityDef = this.m_hero.GetEntityDef();
            if (entityDef != null)
            {
                this.m_name = entityDef.GetName();
            }
        }
    }

    private void UpdateRank()
    {
        if (this.ShouldUseBogusRank())
        {
            this.m_medalInfo = new MedalInfoTranslator();
        }
        else
        {
            BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(this.m_gameAccountId);
            this.m_medalInfo = RankMgr.Get().GetRankPresenceField(player);
        }
    }

    private void UpdateSide()
    {
        if (GameMgr.Get().IsSpectator())
        {
            if (this.m_gameAccountId == SpectatorManager.Get().GetSpectateeFriendlySide())
            {
                this.m_side = Side.FRIENDLY;
            }
            else
            {
                this.m_side = Side.OPPOSING;
            }
        }
        else if (this.IsLocalUser())
        {
            this.m_side = Side.FRIENDLY;
        }
        else
        {
            this.m_side = Side.OPPOSING;
        }
    }

    public void WipeZzzs()
    {
        foreach (Card card in this.GetBattlefieldZone().GetCards())
        {
            Spell actorSpell = card.GetActorSpell(SpellType.Zzz);
            if (actorSpell != null)
            {
                actorSpell.ActivateState(SpellStateType.DEATH);
            }
        }
    }

    public enum Side
    {
        NEUTRAL,
        FRIENDLY,
        OPPOSING
    }
}

