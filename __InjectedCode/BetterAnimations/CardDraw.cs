#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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

class Card : MonoBehaviour
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void DrawFriendlyCard()
    {
        var currentRarity = this.m_entity.GetRarity();

        switch (currentRarity)
        {
            case TAG_RARITY.COMMON:
                this.m_actor.ActivateSpell(SpellType.BURST_COMMON);
                this.m_actor.DeactivateSpell(SpellType.BURST_COMMON);
                break;
            case TAG_RARITY.RARE:
                this.m_actor.ActivateSpell(SpellType.BURST_RARE);
                this.m_actor.DeactivateSpell(SpellType.BURST_RARE);
                break;
            case TAG_RARITY.EPIC:
                this.m_actor.ActivateSpell(SpellType.BURST_EPIC);
                this.m_actor.DeactivateSpell(SpellType.BURST_EPIC);
                break;
            case TAG_RARITY.LEGENDARY:
                this.m_actor.ActivateSpell(SpellType.BURST_LEGENDARY);
                this.m_actor.DeactivateSpell(SpellType.BURST_LEGENDARY);
                break;
        }
        
        base.StartCoroutine(this.DrawFriendlyCardWithTiming());
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    Card()
    {
    }

    static Card()
    {
    }

    string ToString()
    {
        return default(string);
    }

    Entity GetEntity()
    {
        return default(Entity);
    }

    void SetEntity(Entity entity)
    {
    }

    void Destroy()
    {
    }

    Player GetController()
    {
        return default(Player);
    }

    Entity GetHero()
    {
        return default(Entity);
    }

    Card GetHeroCard()
    {
        return default(Card);
    }

    Entity GetHeroPower()
    {
        return default(Entity);
    }

    Card GetHeroPowerCard()
    {
        return default(Card);
    }

    CardFlair GetCardFlair()
    {
        return default(CardFlair);
    }

    bool IsOverPlayfield()
    {
        return default(bool);
    }

    void NotifyOverPlayfield()
    {
    }

    void NotifyLeftPlayfield()
    {
    }

    void NotifyMousedOver()
    {
    }

    void NotifyMousedOut()
    {
    }

    bool IsMousedOver()
    {
        return default(bool);
    }

    void NotifyOpponentMousedOverThisCard()
    {
    }

    void NotifyOpponentMousedOffThisCard()
    {
    }

    void NotifyPickedUp()
    {
    }

    void NotifyTargetingCanceled()
    {
    }

    bool IsInputEnabled()
    {
        return default(bool);
    }

    void SetInputEnabled(bool enabled)
    {
    }

    bool IsAllowedToShowTooltip()
    {
        return default(bool);
    }

    bool IsAbleToShowTooltip()
    {
        return default(bool);
    }

    bool GetShouldShowTooltip()
    {
        return default(bool);
    }

    void SetShouldShowTooltip()
    {
    }

    void ShowTooltip()
    {
    }

    void HideTooltip()
    {
    }

    bool IsShowingTooltip()
    {
        return default(bool);
    }

    void UpdateTooltip()
    {
    }

    bool IsAttacking()
    {
        return default(bool);
    }

    void EnableAttacking(bool enable)
    {
    }

    bool WillIgnoreDeath()
    {
        return default(bool);
    }

    void IgnoreDeath(bool ignore)
    {
    }

    bool WillSuppressDeathEffects()
    {
        return default(bool);
    }

    void SuppressDeathEffects(bool suppress)
    {
    }

    bool WillSuppressKeywordDeaths()
    {
        return default(bool);
    }

    void SuppressKeywordDeaths(bool suppress)
    {
    }

    bool WillSuppressDeathSounds()
    {
        return default(bool);
    }

    void SuppressDeathSounds(bool suppress)
    {
    }

    bool WillSuppressActorTriggerSpell()
    {
        return default(bool);
    }

    void SuppressActorTriggerSpell(bool suppress)
    {
    }

    bool WillSuppressPlaySounds()
    {
        return default(bool);
    }

    void SuppressPlaySounds(bool suppress)
    {
    }

    bool IsShown()
    {
        return default(bool);
    }

    void ShowCard()
    {
    }

    void ShowImpl()
    {
    }

    void HideCard()
    {
    }

    void HideImpl()
    {
    }

    void SetBattleCrySource(bool source)
    {
    }

    void DoTauntNotification()
    {
    }

    void UpdateProposedManaUsage()
    {
    }

    CardDef GetCardDef()
    {
        return default(CardDef);
    }

    void LoadCardDef(CardDef cardDef)
    {
    }

    void InitPlaySpell(Spell spell)
    {
    }

    void InitPlaySoundSpell(int index, Spell spell)
    {
    }

    void InitAttackSpell(Spell spell)
    {
    }

    void InitAttackSoundSpell(int index, Spell spell)
    {
    }

    void InitDeathSpell(Spell spell)
    {
    }

    void InitDeathSoundSpell(int index, Spell spell)
    {
    }

    void InitLifetimeSpell(Spell spell)
    {
    }

    void InitLifetimeSoundSpell(int index, Spell spell)
    {
    }

    void InitSubOptionSpell(int index, Spell spell)
    {
    }

    void InitSubOptionSoundSpell(int index, int soundSpellIndex, Spell spell)
    {
    }

    void InitTriggerSpell(int index, Spell spell)
    {
    }

    void InitTriggerSoundSpell(int index, int soundSpellIndex, Spell spell)
    {
    }

    void OverrideCustomSpawnSpell(Spell spell)
    {
    }

    void OverrideCustomDeathSpell(Spell spell)
    {
    }

    void InitCustomKeywordSpell(Spell spell)
    {
    }

    void InitAnnouncerLine(UnityEngine.AudioSource sound)
    {
    }

    void InitEmoteSoundSpell(EmoteType emoteType, CardSoundSpell spell)
    {
    }

    UnityEngine.Texture GetPortraitTexture()
    {
        return default(UnityEngine.Texture);
    }

    UnityEngine.Material GetGoldenMaterial()
    {
        return default(UnityEngine.Material);
    }

    Spell GetPlaySpell()
    {
        return default(Spell);
    }

    System.Collections.Generic.List<CardSoundSpell> GetPlaySoundSpells()
    {
        return default(System.Collections.Generic.List<CardSoundSpell>);
    }

    Spell GetAttackSpell()
    {
        return default(Spell);
    }

    System.Collections.Generic.List<CardSoundSpell> GetAttackSoundSpells()
    {
        return default(System.Collections.Generic.List<CardSoundSpell>);
    }

    System.Collections.Generic.List<CardSoundSpell> GetDeathSoundSpells()
    {
        return default(System.Collections.Generic.List<CardSoundSpell>);
    }

    Spell GetLifetimeSpell()
    {
        return default(Spell);
    }

    System.Collections.Generic.List<CardSoundSpell> GetLifetimeSoundSpells()
    {
        return default(System.Collections.Generic.List<CardSoundSpell>);
    }

    CardEffect GetSubOptionEffect(int index)
    {
        return default(CardEffect);
    }

    Spell GetSubOptionSpell(int index)
    {
        return default(Spell);
    }

    System.Collections.Generic.List<CardSoundSpell> GetSubOptionSoundSpells(int index)
    {
        return default(System.Collections.Generic.List<CardSoundSpell>);
    }

    CardEffect GetTriggerEffect(int index)
    {
        return default(CardEffect);
    }

    Spell GetTriggerSpell(int index)
    {
        return default(Spell);
    }

    System.Collections.Generic.List<CardSoundSpell> GetTriggerSoundSpells(int index)
    {
        return default(System.Collections.Generic.List<CardSoundSpell>);
    }

    Spell GetCustomSummonSpell()
    {
        return default(Spell);
    }

    Spell GetCustomSpawnSpell()
    {
        return default(Spell);
    }

    Spell GetCustomSpawnSpellOverride()
    {
        return default(Spell);
    }

    Spell GetCustomDeathSpell()
    {
        return default(Spell);
    }

    Spell GetCustomDeathSpellOverride()
    {
        return default(Spell);
    }

    UnityEngine.AudioSource GetAnnouncerLine()
    {
        return default(UnityEngine.AudioSource);
    }

    EmoteEntry GetEmoteEntry(EmoteType emoteType)
    {
        return default(EmoteEntry);
    }

    Spell GetBestSummonSpell()
    {
        return default(Spell);
    }

    Spell GetBestSummonSpell(ref bool standard)
    {
        return default(Spell);
    }

    Spell GetBestSpawnSpell()
    {
        return default(Spell);
    }

    Spell GetBestSpawnSpell(ref bool standard)
    {
        return default(Spell);
    }

    Spell GetBestDeathSpell()
    {
        return default(Spell);
    }

    Spell GetBestDeathSpell(ref bool standard)
    {
        return default(Spell);
    }

    Spell GetBestDeathSpell(Actor actor)
    {
        return default(Spell);
    }

    Spell GetBestDeathSpell(Actor actor, ref bool standard)
    {
        return default(Spell);
    }

    void ActivateCharacterPlayEffects()
    {
    }

    void ActivateCharacterAttackEffects()
    {
    }

    void ActivateCharacterDeathEffects()
    {
    }

    void ActivateLifetimeEffects()
    {
    }

    void DeactivateLifetimeEffects()
    {
    }

    void ActivateCustomKeywordEffect()
    {
    }

    void DeactivateCustomKeywordEffect()
    {
    }

    bool ActivateSoundSpellList(System.Collections.Generic.List<CardSoundSpell> soundSpells)
    {
        return default(bool);
    }

    bool ActivateSoundSpell(CardSoundSpell soundSpell)
    {
        return default(bool);
    }

    bool HasActiveEmoteSound()
    {
        return default(bool);
    }

    CardSoundSpell PlayEmote(EmoteType emoteType)
    {
        return default(CardSoundSpell);
    }

    void InitCardDefAssets()
    {
    }

    void InitEffect(CardEffectDef effectDef, ref CardEffect effect)
    {
    }

    System.Collections.Generic.List<CardSoundSpell> InitSoundSpellList(System.Collections.Generic.List<string> paths)
    {
        return default(System.Collections.Generic.List<CardSoundSpell>);
    }

    void InitSpell(ref Spell spell)
    {
    }

    void InitSound(ref UnityEngine.AudioSource source)
    {
    }

    void InitEmoteList()
    {
    }

    Spell SetupSpell(Spell spell)
    {
        return default(Spell);
    }

    Spell SetupCustomSpell(UnityEngine.GameObject go)
    {
        return default(Spell);
    }

    Spell SetupOverrideSpell(Spell existingSpell, Spell spell)
    {
        return default(Spell);
    }

    CardSoundSpell SetupSoundSpell(Spell spell)
    {
        return default(CardSoundSpell);
    }

    UnityEngine.AudioSource SetupSound(UnityEngine.AudioSource sound)
    {
        return default(UnityEngine.AudioSource);
    }

    void SetCustomSpellParent(Spell spell, UnityEngine.Component c)
    {
    }

    void DestroyCardDefAssets()
    {
    }

    void DestroyCardEffect(ref CardEffect effect)
    {
    }

    void DestroyCardAsset<T>(ref T asset) where T : UnityEngine.Component
    {
    }

    void DestroyCardAsset<T>(T asset) where T : UnityEngine.Component
    {
    }

    void DestroySpellList<T>(System.Collections.Generic.List<T> spells) where T : Spell
    {
    }

    void DestroyEmoteList()
    {
    }

    void CancelActiveSpells()
    {
    }

    void CancelActiveSpell(Spell spell)
    {
    }

    System.Collections.IEnumerator WaitThenActivateSoundSpell(CardSoundSpell soundSpell)
    {
        return default(System.Collections.IEnumerator);
    }

    void OnTagsChanged(TagDeltaSet changeSet)
    {
    }


    void OnTagChanged(TagDelta change)
    {
    }

    bool CanShowActorVisuals()
    {
        return default(bool);
    }

    void CreateStartingCardStateEffects()
    {
    }

    void ToggleDeathrattle(bool on)
    {
    }

    void ToggleBauble(bool on, SpellType spellType)
    {
    }

    void DeactivateBaubles()
    {
    }

    SpellType PrioritizedBauble()
    {
        return default(SpellType);
    }

    void ShowExhaustedChange(int val)
    {
    }

    void ShowExhaustedChange(bool exhausted)
    {
    }

    System.Collections.IEnumerator PlayHeroPowerAnimation(string animation)
    {
        return default(System.Collections.IEnumerator);
    }

    void SheatheWeapon()
    {
    }

    void UnSheatheWeapon()
    {
    }

    System.Collections.IEnumerator ShowSecretExhaustedChange(bool exhausted)
    {
        return default(System.Collections.IEnumerator);
    }

    void SheatheSecret(Spell spell)
    {
    }

    void UnSheatheSecret(Spell spell)
    {
    }

    void OnEnchantmentAdded(int oldEnchantmentCount, Entity enchantment)
    {
    }

    void OnEnchantmentRemoved(int oldEnchantmentCount, Entity enchantment)
    {
    }

    void OnEnchantmentSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
    }

    void UpdateEnchantments()
    {
    }

    Spell GetActorSpell(SpellType spellType)
    {
        return default(Spell);
    }

    Spell ActivateActorSpell(SpellType spellType)
    {
        return default(Spell);
    }

    Spell ActivateActorSpell(Actor actor, SpellType spellType)
    {
        return default(Spell);
    }


    Spell GetActorAttackSpellForInput()
    {
        return default(Spell);
    }

    Spell ActivateDeathSpell(Actor actor)
    {
        return default(Spell);
    }

    void ActivateMinionSpawnEffects()
    {
    }

    System.Collections.IEnumerator ActivateCreatorSpawnMinionSpell(Entity creator, Card creatorCard)
    {
        return default(System.Collections.IEnumerator);
    }

    Spell ActivateCreatorSpawnMinionSpell()
    {
        return default(Spell);
    }

    void ActivateStandardSpawnMinionSpell()
    {
    }

    void ActivateCustomSpawnMinionSpell(Spell spell, Card creatorCard)
    {
    }

    System.Collections.IEnumerator ActivateReviveSpell()
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator ActivateActorBattlecrySpell()
    {
        return default(System.Collections.IEnumerator);
    }

    void CleanUpCustomSpell(Spell chosenSpell, ref Spell customSpell)
    {
    }

    void OnSpellFinished_StandardSpawnMinion(Spell spell, object userData)
    {
    }

    void OnSpellFinished_CustomSpawnMinion(Spell spell, object userData)
    {
    }

    void OnSpellFinished_DefaultHandSpawn(Spell spell, object userData)
    {
    }

    void OnSpellFinished_DefaultPlaySpawn(Spell spell, object userData)
    {
    }

    void OnSpellFinished_StandardCardSummon(Spell spell, object userData)
    {
    }

    void OnSpellFinished_UpdateActorComponents(Spell spell, object userData)
    {
    }

    void OnSpellFinished_Death(Spell spell, object userData)
    {
    }

    void OnSpellStateFinished_DestroyActor(Spell spell, SpellStateType prevStateType, object userData)
    {
    }

    void OnSpellStateFinished_DestroySpell(Spell spell, SpellStateType prevStateType, object userData)
    {
    }

    void OnSpellStateFinished_CustomDeath(Spell spell, SpellStateType prevStateType, object userData)
    {
    }

    void UpdateActorState()
    {
    }

    bool DoChoiceHighlight(GameState state)
    {
        return default(bool);
    }

    bool DoOptionHighlight(GameState state)
    {
        return default(bool);
    }

    bool DoSubOptionHighlight(GameState state)
    {
        return default(bool);
    }

    bool DoOptionTargetHighlight(GameState state)
    {
        return default(bool);
    }

    Actor GetActor()
    {
        return default(Actor);
    }

    void SetActor(Actor actor)
    {
    }

    string GetActorName()
    {
        return default(string);
    }

    void SetActorName(string actorName)
    {
    }

    bool IsActorReady()
    {
        return default(bool);
    }

    bool IsActorLoading()
    {
        return default(bool);
    }

    void UpdateActorComponents()
    {
    }

    void RefreshActor()
    {
    }

    Zone GetZone()
    {
        return default(Zone);
    }

    Zone GetPrevZone()
    {
        return default(Zone);
    }

    void SetZone(Zone zone)
    {
    }

    int GetZonePosition()
    {
        return default(int);
    }

    void SetZonePosition(int pos)
    {
    }

    int GetPredictedZonePosition()
    {
        return default(int);
    }

    void SetPredictedZonePosition(int pos)
    {
    }

    ZoneTransitionStyle GetTransitionStyle()
    {
        return default(ZoneTransitionStyle);
    }

    void SetTransitionStyle(ZoneTransitionStyle style)
    {
    }

    bool IsTransitioningZones()
    {
        return default(bool);
    }

    void EnableTransitioningZones(bool enable)
    {
    }

    bool HasBeenGrabbedByEnemyActionHandler()
    {
        return default(bool);
    }

    void MarkAsGrabbedByEnemyActionHandler(bool enable)
    {
    }

    bool IsDoNotSort()
    {
        return default(bool);
    }

    void SetDoNotSort(bool on)
    {
    }

    bool IsDoNotWarpToNewZone()
    {
        return default(bool);
    }

    void SetDoNotWarpToNewZone(bool on)
    {
    }

    float GetTransitionDelay()
    {
        return default(float);
    }

    void SetTransitionDelay(float delay)
    {
    }

    bool IsHoldingForLinkedCardSwitch()
    {
        return default(bool);
    }

    void SetHoldingForLinkedCardSwitch(bool hold)
    {
    }

    void UpdateZoneFromTags()
    {
    }

    void TransitionToZone(Zone zone)
    {
    }

    void UpdateActor()
    {
    }

    void LoadActorAndSpells()
    {
    }

    void OnCustomSummonSpellLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnCustomDeathSpellLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnCustomSpawnSpellLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void FinishSpellLoad()
    {
    }

    void LoadActor()
    {
    }

    void OnActorLoaded(string actorName, UnityEngine.GameObject actorObject, object callbackData)
    {
    }

    void InitActor(string actorName, Actor actor)
    {
    }

    void FinishActorLoad(Actor oldActor)
    {
    }

    void ForceLoadHandActor()
    {
    }

    void OnHandActorForceLoaded(string actorName, UnityEngine.GameObject actorObject, object callbackData)
    {
    }

    void OnZoneChanged()
    {
    }

    void OnActorChanged(Actor oldActor)
    {
    }

    bool HandleGraveyardToDeck(Actor oldActor)
    {
        return default(bool);
    }

    bool HandleGraveyardToHand(Actor oldActor)
    {
        return default(bool);
    }

    bool CardStandInIsInteractive()
    {
        return default(bool);
    }

    System.Collections.IEnumerator DrawFriendlyCardWithTiming()
    {
        return default(System.Collections.IEnumerator);
    }

    bool IsBeingDrawnByOpponent()
    {
        return default(bool);
    }

    void DrawOpponentCard()
    {
    }

    System.Collections.IEnumerator DrawOpponentCardWithTiming()
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator DrawUnknownOpponentCard(ZoneHand handZone)
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator DrawKnownOpponentCard(ZoneHand handZone)
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator RevealDrawnOpponentCard(string handActorName, Actor handActor, ZoneHand handZone)
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator HideRevealedOpponentCard(Actor handActor)
    {
        return default(System.Collections.IEnumerator);
    }

    void OnDeckToPlayActorLoaded(string name, UnityEngine.GameObject actorObject, object userData)
    {
    }

    void OnDeckToPlayHiddenCardLoaded(string name, UnityEngine.GameObject actorObject, object userData)
    {
    }

    System.Collections.IEnumerator AnimateDeckToPlay(Actor cardFaceActor, Actor hiddenActor)
    {
        return default(System.Collections.IEnumerator);
    }

    void MillCard()
    {
    }

    System.Collections.IEnumerator MillCardWithTiming()
    {
        return default(System.Collections.IEnumerator);
    }

    bool ActivateActorSpells_HandToPlay(Actor oldActor)
    {
        return default(bool);
    }

    void OnSpellFinished_HandToPlay_SummonOut(Spell spell, object userData)
    {
    }

    void OnSpellFinished_HandToPlay_SummonIn(Spell spell, object userData)
    {
    }

    bool ActivateActorSpells_HandToWeapon(Actor oldActor)
    {
        return default(bool);
    }

    void OnSpellFinished_HandToWeapon_SummonOut(Spell spell, object userData)
    {
    }

    void DiscardCardBeingDrawn()
    {
    }

    void DoDiscardAnimation()
    {
    }

    bool SwitchOutLinkedDrawnCard()
    {
        return default(bool);
    }

    System.Collections.IEnumerator SwitchOutFriendlyLinkedDrawnCard(Card oldCard)
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator SwitchOutOpponentLinkedDrawnCard(Card oldCard)
    {
        return default(System.Collections.IEnumerator);
    }

    bool DoPlayToHandTransition(Actor oldActor, bool wasInGraveyard)
    {
        return default(bool);
    }

    bool ActivateActorSpells_PlayToHand(Actor oldActor, bool wasInGraveyard)
    {
        return default(bool);
    }

    void OnSpellFinished_PlayToHand_SummonOut(Spell spell, object userData)
    {
    }

    void OnSpellFinished_PlayToHand_SummonOut_FromGraveyard(Spell spell, object userData)
    {
    }

    void ResumeLayoutForPlayZone()
    {
    }

    void OnSpellStateFinished_PlayToHand_OldActor_SummonOut(Spell spell, SpellStateType prevStateType, object userData)
    {
    }

    void OnSpellFinished_PlayToHand_SummonIn(Spell spell, object userData)
    {
    }

    void DoPlayToDeckTransition(Actor playActor)
    {
    }

    System.Collections.IEnumerator AnimatePlayToDeck(Actor playActor)
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator AnimatePlayToDeck(UnityEngine.GameObject mover, ZoneDeck deckZone, bool hideBackSide)
    {
        return default(System.Collections.IEnumerator);
    }

    void SetSecretTriggered(bool @set)
    {
    }

    bool WasSecretTriggered()
    {
        return default(bool);
    }

    bool CanShowSecretTrigger()
    {
        return default(bool);
    }

    void ShowSecretTrigger()
    {
    }

    bool CanShowSecret()
    {
        return default(bool);
    }

    void ShowSecretBirth()
    {
    }

    bool CanShowSecretDeath()
    {
        return default(bool);
    }

    void ShowSecretDeath(Actor oldActor)
    {
    }

    System.Collections.IEnumerator WaitAndThenShowDestroyedSecret()
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator SwitchSecretSides()
    {
        return default(System.Collections.IEnumerator);
    }

    bool ShouldCardDrawWaitForTurnStartSpells()
    {
        return default(bool);
    }

    System.Collections.IEnumerator WaitForCardDrawBlockingTurnStartSpells()
    {
        return default(System.Collections.IEnumerator);
    }

    bool ShouldCardDrawWaitForTaskLists()
    {
        return default(bool);
    }

    System.Collections.IEnumerator WaitForCardDrawBlockingTaskLists()
    {
        return default(System.Collections.IEnumerator);
    }

    bool DoesTaskListBlockCardDraw(PowerTaskList taskList)
    {
        return default(bool);
    }

    void CutoffFriendlyCardDraw()
    {
    }

    System.Collections.IEnumerator WaitAndPrepareForDeathAnimation(Actor dyingActor)
    {
        return default(System.Collections.IEnumerator);
    }

    void PrepareForDeathAnimation(Actor dyingActor)
    {
    }

    System.Collections.IEnumerator ActivateGraveyardActorDeathSpellAfterDelay()
    {
        return default(System.Collections.IEnumerator);
    }

    bool HandlePlayActorDeath(Actor oldActor)
    {
        return default(bool);
    }

    bool DoesCardReturnFromGraveyard()
    {
        return default(bool);
    }

    bool DoesTaskListReturnCardFromGraveyard(PowerTaskList taskList)
    {
        return default(bool);
    }

    void DoNullZoneVisuals()
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static UnityEngine.Vector3 ABOVE_DECK_OFFSET;
    static UnityEngine.Vector3 IN_DECK_OFFSET;
    static UnityEngine.Vector3 IN_DECK_SCALE;
    static UnityEngine.Vector3 IN_DECK_ANGLES;
    static UnityEngine.Quaternion IN_DECK_ROTATION;
    static UnityEngine.Vector3 IN_DECK_HIDDEN_ANGLES;
    static UnityEngine.Quaternion IN_DECK_HIDDEN_ROTATION;
    Entity m_entity;
    CardDef m_cardDef;
    CardEffect m_playEffect;
    CardEffect m_attackEffect;
    CardEffect m_deathEffect;
    CardEffect m_lifetimeEffect;
    System.Collections.Generic.List<CardEffect> m_subOptionEffects;
    System.Collections.Generic.List<CardEffect> m_triggerEffects;
    Spell m_customKeywordSpell;
    UnityEngine.AudioSource m_announcerLine;
    System.Collections.Generic.List<EmoteEntry> m_emotes;
    Spell m_customSummonSpell;
    Spell m_customSpawnSpell;
    Spell m_customSpawnSpellOverride;
    Spell m_customDeathSpell;
    Spell m_customDeathSpellOverride;
    int m_spellLoadCount;
    string m_actorName;
    Actor m_actor;
    Actor m_actorWaitingToBeReplaced;
    bool m_actorReady;
    bool m_actorLoading;
    bool m_transitioningZones;
    bool m_hasBeenGrabbedByEnemyActionHandler;
    Zone m_zone;
    Zone m_prevZone;
    int m_zonePosition;
    int m_predictedZonePosition;
    bool m_doNotSort;
    bool m_beingDrawnByOpponent;
    bool m_cardStandInInteractive;
    ZoneTransitionStyle m_transitionStyle;
    bool m_doNotWarpToNewZone;
    float m_transitionDelay;
    bool m_holdingForLinkedCardSwitch;
    bool m_shouldShowTooltip;
    bool m_showTooltip;
    bool m_overPlayfield;
    bool m_mousedOver;
    bool m_mousedOverByOpponent;
    bool m_shown;
    bool m_inputEnabled;
    bool m_attacking;
    int m_activeDeathEffectCount;
    bool m_ignoreDeath;
    bool m_suppressDeathEffects;
    bool m_suppressKeywordDeaths;
    bool m_suppressDeathSounds;
    bool m_suppressActorTriggerSpell;
    bool m_suppressPlaySounds;
    bool m_isBattleCrySource;
    bool m_secretTriggered;
    bool m_secretSheathed;
    #endregion

}
