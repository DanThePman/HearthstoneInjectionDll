#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;
#endregion 

#region " Referenced assemblies "
// - mscorlib v2.0.5.0
// - Assembly-CSharp-firstpass v0.0.0.0
// - System.Core v2.0.5.0
// - UnityEngine v0.0.0.0
// - PlayMaker v1.6.0.0
// - System v2.0.5.0
// - ICSharpCode.SharpZipLib v0.86.0.518
// - IFacebook v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - Assembly-CSharp v0.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class Player
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void OnTurnChanged(int oldTurn, int newTurn, object userData)
    {
        HearthstoneInjectionDll.Player.OnNewTurn();
        this.WipeZzzs();
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    Player()
    {
    }

    string GetName()
    {
        return default(string);
    }

    MedalInfoTranslator GetRank()
    {
        return default(MedalInfoTranslator);
    }

    string GetDebugName()
    {
        return default(string);
    }

    void SetName(string name)
    {
    }

    void SetGameAccountId(BnetGameAccountId id)
    {
    }

    void RequestPlayerPresence()
    {
    }

    bool IsLocalUser()
    {
        return default(bool);
    }

    void SetLocalUser(bool local)
    {
    }

    bool IsAI()
    {
        return default(bool);
    }

    bool IsHuman()
    {
        return default(bool);
    }

    bool IsBnetPlayer()
    {
        return default(bool);
    }

    bool IsGuestPlayer()
    {
        return default(bool);
    }

    bool IsFriendlySide()
    {
        return default(bool);
    }

    bool IsOpposingSide()
    {
        return default(bool);
    }

    bool IsRevealed()
    {
        return default(bool);
    }

    int GetCardBackId()
    {
        return default(int);
    }

    void SetCardBackId(int id)
    {
    }

    int GetPlayerId()
    {
        return default(int);
    }

    void SetPlayerId(int playerId)
    {
    }

    System.Collections.Generic.List<string> GetSecretDefinitions()
    {
        return default(System.Collections.Generic.List<string>);
    }

    bool IsCurrentPlayer()
    {
        return default(bool);
    }

    bool IsComboActive()
    {
        return default(bool);
    }

    bool IsRealTimeComboActive()
    {
        return default(bool);
    }

    void SetRealTimeComboActive(int tagValue)
    {
    }

    void SetRealTimeComboActive(bool active)
    {
    }

    int GetNumAvailableResources()
    {
        return default(int);
    }

    bool HasWeapon()
    {
        return default(bool);
    }

    void SetHero(Entity hero)
    {
    }

    Entity GetStartingHero()
    {
        return default(Entity);
    }

    Entity GetHero()
    {
        return default(Entity);
    }

    EntityDef GetHeroEntityDef()
    {
        return default(EntityDef);
    }

    Card GetHeroCard()
    {
        return default(Card);
    }

    void SetHeroPower(Entity heroPower)
    {
    }

    Entity GetHeroPower()
    {
        return default(Entity);
    }

    Card GetHeroPowerCard()
    {
        return default(Card);
    }

    bool IsHeroPowerAffectedByBonusDamage()
    {
        return default(bool);
    }

    Card GetWeaponCard()
    {
        return default(Card);
    }

    ZoneHand GetHandZone()
    {
        return default(ZoneHand);
    }

    ZonePlay GetBattlefieldZone()
    {
        return default(ZonePlay);
    }

    ZoneDeck GetDeckZone()
    {
        return default(ZoneDeck);
    }

    ZoneGraveyard GetGraveyardZone()
    {
        return default(ZoneGraveyard);
    }

    ZoneSecret GetSecretZone()
    {
        return default(ZoneSecret);
    }

    int GetNumMinionsInPlay()
    {
        return default(int);
    }

    int GetNumDragonsInHand()
    {
        return default(int);
    }

    bool HasReadyAttackers()
    {
        return default(bool);
    }

    bool HasATauntMinion()
    {
        return default(bool);
    }

    int GetNumTotalMinionsInPlay()
    {
        return default(int);
    }

    void PlayConcedeEmote()
    {
    }

    BnetGameAccountId GetGameAccountId()
    {
        return default(BnetGameAccountId);
    }

    BnetPlayer GetBnetPlayer()
    {
        return default(BnetPlayer);
    }

    bool IsDisplayable()
    {
        return default(bool);
    }

    void AddManaCrystal(int numCrystals, bool isTurnStart)
    {
    }

    void AddManaCrystal(int numCrystals)
    {
    }

    void DestroyManaCrystal(int numCrystals)
    {
    }

    void AddTempManaCrystal(int numCrystals)
    {
    }

    void DestroyTempManaCrystal(int numCrystals)
    {
    }

    void SpendManaCrystal(int numCrystals)
    {
    }

    void ReadyManaCrystal(int numCrystals)
    {
    }

    void HandleSameTurnRecallChanged(int crystalsChanged)
    {
    }

    void UnlockCrystals(int numCrystals)
    {
    }

    void CancelAllProposedMana(Entity entity)
    {
    }

    void ProposeManaCrystalUsage(Entity entity)
    {
    }

    void UpdateManaCounter()
    {
    }

    void NotifyOfSpentMana(int spentMana)
    {
    }

    void NotifyOfUsedTempMana(int usedMana)
    {
    }

    int GetQueuedUsedTempMana()
    {
        return default(int);
    }

    int GetQueuedSpentMana()
    {
        return default(int);
    }

    void SetRealTimeTempMana(int tempMana)
    {
    }

    int GetRealTimeTempMana()
    {
        return default(int);
    }

    void OnBoardLoaded()
    {
    }

    void OnTagsChanged(TagDeltaSet changeSet)
    {
    }

    void OnTagChanged(TagDelta change)
    {
    }

    void OnFriendlyPlayerTagChanged(TagDelta change)
    {
    }

    void OnOpposingPlayerTagChanged(TagDelta change)
    {
    }

    void ToggleActorSpellOnCard(Card card, TagDelta change, SpellType spellType)
    {
    }

    void UpdateName()
    {
    }

    bool ShouldUseHeroName()
    {
        return default(bool);
    }

    void UpdateNameWithHeroName()
    {
    }

    bool ShouldUseBogusRank()
    {
        return default(bool);
    }

    void UpdateRank()
    {
    }

    void UpdateDisplayInfo()
    {
    }

    void OnBnetPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
    }

    void UpdateLocal()
    {
    }

    void UpdateSide()
    {
    }

    void AssignPlayerBoardObjects()
    {
    }

    void InitManaCrystalMgr()
    {
    }

    void WipeZzzs()
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    BnetGameAccountId m_gameAccountId;
    bool m_waitingForHeroEntity;
    string m_name;
    bool m_local;
    int m_cardBackId;
    ManaCounter m_manaCounter;
    Entity m_startingHero;
    Entity m_hero;
    Entity m_heroPower;
    int m_queuedSpentMana;
    int m_usedTempMana;
    int m_realtimeTempMana;
    bool m_realTimeComboActive;
    MedalInfoTranslator m_medalInfo;
    bool m_concedeEmotePlayed;
    #endregion

}
