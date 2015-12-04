using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class Card : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<EmoteEntry> <>f__am$cache3D;
    [CompilerGenerated]
    private static Predicate<Network.Entity.Tag> <>f__am$cache3E;
    public static readonly Vector3 ABOVE_DECK_OFFSET = new Vector3(0f, 3.6f, 0f);
    public static readonly Vector3 IN_DECK_ANGLES = new Vector3(-90f, 270f, 0f);
    public static readonly Vector3 IN_DECK_HIDDEN_ANGLES = new Vector3(270f, 90f, 0f);
    public static readonly Quaternion IN_DECK_HIDDEN_ROTATION = Quaternion.Euler(IN_DECK_HIDDEN_ANGLES);
    public static readonly Vector3 IN_DECK_OFFSET = new Vector3(0f, 0f, 0.1f);
    public static readonly Quaternion IN_DECK_ROTATION = Quaternion.Euler(IN_DECK_ANGLES);
    public static readonly Vector3 IN_DECK_SCALE = new Vector3(0.81f, 0.81f, 0.81f);
    private int m_activeDeathEffectCount;
    protected Actor m_actor;
    private bool m_actorLoading;
    protected string m_actorName;
    private bool m_actorReady = true;
    protected Actor m_actorWaitingToBeReplaced;
    protected CardAudio m_announcerLine;
    protected CardEffect m_attackEffect;
    protected bool m_attacking;
    private bool m_beingDrawnByOpponent;
    protected CardDef m_cardDef;
    private bool m_cardStandInInteractive = true;
    protected Spell m_customDeathSpell;
    protected Spell m_customDeathSpellOverride;
    protected CardEffect m_customKeywordSpell;
    protected Spell m_customSpawnSpell;
    protected Spell m_customSpawnSpellOverride;
    protected Spell m_customSummonSpell;
    protected CardEffect m_deathEffect;
    private bool m_doNotSort;
    private bool m_doNotWarpToNewZone;
    private List<CardEffect> m_effects = new List<CardEffect>();
    protected List<EmoteEntry> m_emotes;
    protected Entity m_entity;
    private bool m_hasBeenGrabbedByEnemyActionHandler;
    private bool m_holdingForLinkedCardSwitch;
    private bool m_ignoreDeath;
    private bool m_inputEnabled = true;
    private bool m_isBattleCrySource;
    protected CardEffect m_lifetimeEffect;
    protected bool m_mousedOver;
    protected bool m_mousedOverByOpponent;
    protected bool m_overPlayfield;
    protected CardEffect m_playEffect;
    private int m_predictedZonePosition;
    private Zone m_prevZone;
    private bool m_secretSheathed;
    private bool m_secretTriggered;
    protected bool m_shouldShowTooltip;
    protected bool m_shown = true;
    protected bool m_showTooltip;
    private int m_spellLoadCount;
    protected List<CardEffect> m_subOptionEffects;
    private bool m_suppressActorTriggerSpell;
    private bool m_suppressDeathEffects;
    private bool m_suppressDeathSounds;
    private bool m_suppressKeywordDeaths;
    private bool m_suppressPlaySounds;
    private float m_transitionDelay;
    private bool m_transitioningZones;
    private ZoneTransitionStyle m_transitionStyle;
    protected List<CardEffect> m_triggerEffects;
    private Zone m_zone;
    private int m_zonePosition;

    [DebuggerHidden]
    private IEnumerator ActivateActorBattlecrySpell()
    {
        return new <ActivateActorBattlecrySpell>c__Iterator23E { <>f__this = this };
    }

    public Spell ActivateActorSpell(SpellType spellType)
    {
        return this.ActivateActorSpell(this.m_actor, spellType, null, null);
    }

    private Spell ActivateActorSpell(Actor actor, SpellType spellType)
    {
        return this.ActivateActorSpell(actor, spellType, null, null);
    }

    public Spell ActivateActorSpell(SpellType spellType, Spell.FinishedCallback finishedCallback)
    {
        return this.ActivateActorSpell(this.m_actor, spellType, finishedCallback, null);
    }

    private Spell ActivateActorSpell(Actor actor, SpellType spellType, Spell.FinishedCallback finishedCallback)
    {
        return this.ActivateActorSpell(actor, spellType, finishedCallback, null);
    }

    public Spell ActivateActorSpell(SpellType spellType, Spell.FinishedCallback finishedCallback, Spell.StateFinishedCallback stateFinishedCallback)
    {
        return this.ActivateActorSpell(this.m_actor, spellType, finishedCallback, stateFinishedCallback);
    }

    private Spell ActivateActorSpell(Actor actor, SpellType spellType, Spell.FinishedCallback finishedCallback, Spell.StateFinishedCallback stateFinishedCallback)
    {
        if (actor == null)
        {
            Log.Mike.Print(string.Format("{0}.ActivateActorSpell() - actor IS NULL spellType={1}", this, spellType), new object[0]);
            return null;
        }
        Spell spell = actor.GetSpell(spellType);
        if (spell == null)
        {
            Log.Mike.Print(string.Format("{0}.ActivateActorSpell() - spell IS NULL actor={1} spellType={2}", this, actor, spellType), new object[0]);
            return null;
        }
        this.ActivateSpell(spell, finishedCallback, stateFinishedCallback);
        return spell;
    }

    private bool ActivateActorSpells_HandToPlay(Actor oldActor)
    {
        bool flag;
        if (oldActor == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToPlay() - oldActor=null", this));
            return false;
        }
        if (this.m_cardDef == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToPlay() - m_cardDef=null", this));
            return false;
        }
        if (this.m_actor == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToPlay() - m_actor=null", this));
            return false;
        }
        SpellType spellType = this.m_cardDef.DetermineSummonOutSpell_HandToPlay(this);
        Spell spell = oldActor.GetSpell(spellType);
        if (spell == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToPlay() - outSpell=null outSpellType={1}", this, spellType));
            return false;
        }
        if (this.GetBestSummonSpell(out flag) == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToPlay() - inSpell=null standard={1}", this, flag));
            return false;
        }
        this.m_inputEnabled = false;
        this.ActivateSpell(spell, new Spell.FinishedCallback(this.OnSpellFinished_HandToPlay_SummonOut), null, new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroyActor));
        return true;
    }

    private bool ActivateActorSpells_HandToWeapon(Actor oldActor)
    {
        if (oldActor == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToWeapon() - oldActor=null", this));
            return false;
        }
        if (this.m_actor == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToWeapon() - m_actor=null", this));
            return false;
        }
        SpellType spellType = SpellType.SUMMON_OUT_WEAPON;
        Spell spell = oldActor.GetSpell(spellType);
        if (spell == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToWeapon() - outSpell=null outSpellType={1}", this, spellType));
            return false;
        }
        SpellType type2 = !this.m_entity.IsControlledByFriendlySidePlayer() ? SpellType.SUMMON_IN_OPPONENT : SpellType.SUMMON_IN_FRIENDLY;
        Spell actorSpell = this.GetActorSpell(type2);
        if (actorSpell == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_HandToWeapon() - inSpell=null inSpellType={1}", this, type2));
            return false;
        }
        this.m_inputEnabled = false;
        this.ActivateSpell(spell, new Spell.FinishedCallback(this.OnSpellFinished_HandToWeapon_SummonOut), actorSpell, new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroyActor));
        return true;
    }

    private bool ActivateActorSpells_PlayToHand(Actor oldActor, bool wasInGraveyard)
    {
        if (oldActor == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_PlayToHand() - oldActor=null", this));
            return false;
        }
        if (this.m_actor == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_PlayToHand() - m_actor=null", this));
            return false;
        }
        SpellType spellType = SpellType.BOUNCE_OUT;
        Spell spell = oldActor.GetSpell(spellType);
        if (spell == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_PlayToHand() - outSpell=null outSpellType={1}", this, spellType));
            return false;
        }
        SpellType type2 = SpellType.BOUNCE_IN;
        Spell actorSpell = this.GetActorSpell(type2);
        if (actorSpell == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateActorSpells_PlayToHand() - inSpell=null inSpellType={1}", this, type2));
            return false;
        }
        this.m_inputEnabled = false;
        if (this.m_entity.IsControlledByFriendlySidePlayer())
        {
            Spell.FinishedCallback finishedCallback = !wasInGraveyard ? new Spell.FinishedCallback(this.OnSpellFinished_PlayToHand_SummonOut) : new Spell.FinishedCallback(this.OnSpellFinished_PlayToHand_SummonOut_FromGraveyard);
            this.ActivateSpell(spell, finishedCallback, actorSpell, new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroyActor));
        }
        else
        {
            if (this.m_entity.IsControlledByOpposingSidePlayer())
            {
                object[] args = new object[] { this, spellType, oldActor };
                Log.FaceDownCard.Print("Card.ActivateActorSpells_PlayToHand() - {0} - {1} on {2}", args);
                object[] objArray2 = new object[] { this, type2, this.m_actor };
                Log.FaceDownCard.Print("Card.ActivateActorSpells_PlayToHand() - {0} - {1} on {2}", objArray2);
            }
            Spell.FinishedCallback callback2 = !wasInGraveyard ? null : delegate (Spell spell, object userData) {
                this.ResumeLayoutForPlayZone();
            };
            this.ActivateSpell(spell, callback2, null, new Spell.StateFinishedCallback(this.OnSpellStateFinished_PlayToHand_OldActor_SummonOut));
            this.ActivateSpell(actorSpell, new Spell.FinishedCallback(this.OnSpellFinished_PlayToHand_SummonIn));
        }
        return true;
    }

    public void ActivateCharacterAttackEffects()
    {
        this.ActivateSoundSpellList(this.m_attackEffect.GetSoundSpells(true));
    }

    public void ActivateCharacterDeathEffects()
    {
        if (!this.m_suppressDeathEffects)
        {
            if (!this.m_suppressDeathSounds)
            {
                if (<>f__am$cache3D == null)
                {
                    <>f__am$cache3D = e => (e != null) && (e.GetEmoteType() == EmoteType.DEATH_LINE);
                }
                int num = (this.m_emotes != null) ? this.m_emotes.FindIndex(<>f__am$cache3D) : -1;
                if (num >= 0)
                {
                    this.PlayEmote(EmoteType.DEATH_LINE);
                }
                else
                {
                    this.ActivateSoundSpellList(this.m_deathEffect.GetSoundSpells(true));
                }
                this.m_suppressDeathSounds = false;
            }
            this.DeactivateLifetimeEffects();
        }
    }

    public void ActivateCharacterPlayEffects()
    {
        if (!this.m_suppressPlaySounds)
        {
            this.ActivateSoundSpellList(this.m_playEffect.GetSoundSpells(true));
            this.m_suppressPlaySounds = false;
        }
        this.ActivateLifetimeEffects();
    }

    private Spell ActivateCreatorSpawnMinionSpell()
    {
        if (this.m_entity.IsControlledByFriendlySidePlayer())
        {
            return this.ActivateActorSpell(SpellType.FRIENDLY_SPAWN_MINION);
        }
        return this.ActivateActorSpell(SpellType.OPPONENT_SPAWN_MINION);
    }

    [DebuggerHidden]
    private IEnumerator ActivateCreatorSpawnMinionSpell(Entity creator, Card creatorCard)
    {
        return new <ActivateCreatorSpawnMinionSpell>c__Iterator23C { creator = creator, creatorCard = creatorCard, <$>creator = creator, <$>creatorCard = creatorCard, <>f__this = this };
    }

    private void ActivateCustomHandSpawnSpell(Spell spell, Card creatorCard)
    {
        GameObject go = (creatorCard != null) ? creatorCard.gameObject : base.gameObject;
        spell.SetSource(go);
        spell.RemoveAllTargets();
        spell.AddTarget(base.gameObject);
        spell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroySpell));
        this.SetCustomSpellParent(spell, this.m_actor);
        spell.AddFinishedCallback(new Spell.FinishedCallback(this.OnSpellFinished_CustomHandSpawn));
        spell.Activate();
    }

    public void ActivateCustomKeywordEffect()
    {
        if (this.m_customKeywordSpell != null)
        {
            Spell spell = this.m_customKeywordSpell.GetSpell(true);
            if (spell == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("Card.ActivateCustomKeywordEffect() -- failed to load custom keyword spell for card {0}", this.ToString()));
            }
            else
            {
                spell.transform.parent = this.m_actor.transform;
                spell.ActivateState(SpellStateType.BIRTH);
            }
        }
    }

    private void ActivateCustomSpawnMinionSpell(Spell spell, Card creatorCard)
    {
        GameObject go = (creatorCard != null) ? creatorCard.gameObject : base.gameObject;
        spell.SetSource(go);
        spell.RemoveAllTargets();
        spell.AddTarget(base.gameObject);
        spell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroySpell));
        this.SetCustomSpellParent(spell, this.m_actor);
        spell.AddFinishedCallback(new Spell.FinishedCallback(this.OnSpellFinished_CustomSpawnMinion));
        spell.Activate();
    }

    private Spell ActivateDeathSpell(Actor actor)
    {
        bool flag;
        Spell bestDeathSpell = this.GetBestDeathSpell(actor, out flag);
        if (bestDeathSpell == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateDeathSpell() - {1} is null", this, SpellType.DEATH));
            return null;
        }
        this.CleanUpCustomSpell(bestDeathSpell, ref this.m_customDeathSpell);
        this.CleanUpCustomSpell(bestDeathSpell, ref this.m_customDeathSpellOverride);
        this.m_activeDeathEffectCount++;
        if (flag)
        {
            if (this.m_actor != actor)
            {
                bestDeathSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroyActor));
            }
        }
        else
        {
            bestDeathSpell.SetSource(base.gameObject);
            if (this.m_actor != actor)
            {
                bestDeathSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnSpellStateFinished_CustomDeath));
            }
            this.SetCustomSpellParent(bestDeathSpell, actor);
        }
        bestDeathSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnSpellFinished_Death));
        bestDeathSpell.Activate();
        BoardEvents events = BoardEvents.Get();
        if (events != null)
        {
            events.DeathEvent(this);
        }
        return bestDeathSpell;
    }

    private Spell ActivateDefaultSpawnSpell(Spell.FinishedCallback finishedCallback)
    {
        SpellType type;
        this.m_inputEnabled = false;
        this.m_actor.ToggleForceIdle(true);
        if (this.m_entity.IsWeapon() && (this.m_zone is ZoneWeapon))
        {
            type = !this.m_entity.IsControlledByFriendlySidePlayer() ? SpellType.SUMMON_IN_OPPONENT : SpellType.SUMMON_IN_FRIENDLY;
        }
        else
        {
            type = SpellType.SUMMON_IN;
        }
        Spell spell = this.ActivateActorSpell(type, finishedCallback);
        if (spell == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.ActivateDefaultSpawnSpell() - {1} is null", this, type));
            return null;
        }
        return spell;
    }

    [DebuggerHidden]
    private IEnumerator ActivateGraveyardActorDeathSpellAfterDelay()
    {
        return new <ActivateGraveyardActorDeathSpellAfterDelay>c__Iterator250 { <>f__this = this };
    }

    private Spell ActivateHandSpawnSpell()
    {
        if (this.m_customSpawnSpellOverride == null)
        {
            return this.ActivateDefaultSpawnSpell(new Spell.FinishedCallback(this.OnSpellFinished_DefaultHandSpawn));
        }
        Entity creator = this.m_entity.GetCreator();
        Card creatorCard = null;
        if ((creator != null) && creator.IsMinion())
        {
            creatorCard = creator.GetCard();
        }
        if (creatorCard != null)
        {
            TransformUtil.CopyWorld((Component) base.transform, (Component) creatorCard.transform);
        }
        this.ActivateCustomHandSpawnSpell(this.m_customSpawnSpellOverride, creatorCard);
        return this.m_customSpawnSpellOverride;
    }

    public void ActivateLifetimeEffects()
    {
        if ((this.m_lifetimeEffect != null) && !this.m_entity.IsSilenced())
        {
            Spell spell = this.m_lifetimeEffect.GetSpell(true);
            if (spell != null)
            {
                spell.Deactivate();
                spell.ActivateState(SpellStateType.BIRTH);
            }
            if (this.m_lifetimeEffect.GetSoundSpells(true) != null)
            {
                this.ActivateSoundSpellList(this.m_lifetimeEffect.GetSoundSpells(true));
            }
        }
    }

    private void ActivateMinionSpawnEffects()
    {
        bool flag;
        Entity creator = this.m_entity.GetCreator();
        Card creatorCard = null;
        if ((creator != null) && creator.IsMinion())
        {
            creatorCard = creator.GetCard();
        }
        if (creatorCard != null)
        {
            TransformUtil.CopyWorld((Component) base.transform, (Component) creatorCard.transform);
        }
        Spell bestSpawnSpell = this.GetBestSpawnSpell(out flag);
        if (flag)
        {
            if (creatorCard == null)
            {
                this.ActivateStandardSpawnMinionSpell();
            }
            else
            {
                base.StartCoroutine(this.ActivateCreatorSpawnMinionSpell(creator, creatorCard));
            }
        }
        else
        {
            this.ActivateCustomSpawnMinionSpell(bestSpawnSpell, creatorCard);
        }
    }

    [DebuggerHidden]
    private IEnumerator ActivateReviveSpell()
    {
        return new <ActivateReviveSpell>c__Iterator23D { <>f__this = this };
    }

    public bool ActivateSoundSpell(CardSoundSpell soundSpell)
    {
        if (soundSpell == null)
        {
            return false;
        }
        if (GameState.Get().GetGameEntity().ShouldDelayCardSoundSpells())
        {
            base.StartCoroutine(this.WaitThenActivateSoundSpell(soundSpell));
        }
        else
        {
            soundSpell.Reactivate();
        }
        return true;
    }

    public bool ActivateSoundSpellList(List<CardSoundSpell> soundSpells)
    {
        if (soundSpells == null)
        {
            return false;
        }
        if (soundSpells.Count == 0)
        {
            return false;
        }
        bool flag = false;
        for (int i = 0; i < soundSpells.Count; i++)
        {
            CardSoundSpell soundSpell = soundSpells[i];
            this.ActivateSoundSpell(soundSpell);
            flag = true;
        }
        return flag;
    }

    private void ActivateSpell(Spell spell, Spell.FinishedCallback finishedCallback)
    {
        this.ActivateSpell(spell, finishedCallback, null, null, null);
    }

    private void ActivateSpell(Spell spell, Spell.FinishedCallback finishedCallback, Spell.StateFinishedCallback stateFinishedCallback)
    {
        this.ActivateSpell(spell, finishedCallback, null, stateFinishedCallback, null);
    }

    private void ActivateSpell(Spell spell, Spell.FinishedCallback finishedCallback, object finishedUserData, Spell.StateFinishedCallback stateFinishedCallback)
    {
        this.ActivateSpell(spell, finishedCallback, finishedUserData, stateFinishedCallback, null);
    }

    private void ActivateSpell(Spell spell, Spell.FinishedCallback finishedCallback, object finishedUserData, Spell.StateFinishedCallback stateFinishedCallback, object stateFinishedUserData)
    {
        if (finishedCallback != null)
        {
            spell.AddFinishedCallback(finishedCallback, finishedUserData);
        }
        if (stateFinishedCallback != null)
        {
            spell.AddStateFinishedCallback(stateFinishedCallback, stateFinishedUserData);
        }
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            spell.Activate();
        }
    }

    private void ActivateStandardSpawnMinionSpell()
    {
        if (this.m_entity.IsControlledByFriendlySidePlayer())
        {
            this.ActivateActorSpell(SpellType.FRIENDLY_SPAWN_MINION, new Spell.FinishedCallback(this.OnSpellFinished_StandardSpawnMinion));
        }
        else
        {
            this.ActivateActorSpell(SpellType.OPPONENT_SPAWN_MINION, new Spell.FinishedCallback(this.OnSpellFinished_StandardSpawnMinion));
        }
        this.ActivateCharacterPlayEffects();
    }

    private void AnimateDeckToPlay()
    {
        string handActor = ActorNames.GetHandActor(this.m_entity);
        GameObject actorObject = AssetLoader.Get().LoadActor(handActor, false, false);
        Actor component = actorObject.GetComponent<Actor>();
        this.SetupDeckToPlayActor(component, actorObject);
        SpellType spellType = this.m_cardDef.DetermineSummonOutSpell_HandToPlay(this);
        Spell outSpell = component.GetSpell(spellType);
        GameObject obj3 = AssetLoader.Get().LoadActor("Card_Hidden", false, false);
        Actor actor = obj3.GetComponent<Actor>();
        this.SetupDeckToPlayActor(actor, obj3);
        base.StartCoroutine(this.AnimateDeckToPlay(component, outSpell, actor));
    }

    [DebuggerHidden]
    private IEnumerator AnimateDeckToPlay(Actor cardFaceActor, Spell outSpell, Actor hiddenActor)
    {
        return new <AnimateDeckToPlay>c__Iterator245 { cardFaceActor = cardFaceActor, hiddenActor = hiddenActor, outSpell = outSpell, <$>cardFaceActor = cardFaceActor, <$>hiddenActor = hiddenActor, <$>outSpell = outSpell, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator AnimatePlayToDeck(Actor playActor)
    {
        return new <AnimatePlayToDeck>c__Iterator249 { playActor = playActor, <$>playActor = playActor, <>f__this = this };
    }

    [DebuggerHidden]
    public IEnumerator AnimatePlayToDeck(GameObject mover, ZoneDeck deckZone, bool hideBackSide = false)
    {
        return new <AnimatePlayToDeck>c__Iterator24A { deckZone = deckZone, hideBackSide = hideBackSide, mover = mover, <$>deckZone = deckZone, <$>hideBackSide = hideBackSide, <$>mover = mover };
    }

    private void CancelActiveSpell(Spell spell)
    {
        if (spell != null)
        {
            switch (spell.GetActiveState())
            {
                case SpellStateType.NONE:
                    return;

                case SpellStateType.CANCEL:
                    return;
            }
            spell.ActivateState(SpellStateType.CANCEL);
        }
    }

    private void CancelActiveSpells()
    {
        this.CancelActiveSpell(this.m_playEffect.GetSpell(false));
        if (this.m_triggerEffects != null)
        {
            foreach (CardEffect effect in this.m_triggerEffects)
            {
                this.CancelActiveSpell(effect.GetSpell(false));
            }
        }
    }

    public bool CanShowActorVisuals()
    {
        if (this.m_entity.IsLoadingAssets())
        {
            return false;
        }
        if (this.m_actor == null)
        {
            return false;
        }
        if (!this.m_actor.IsShown())
        {
            return false;
        }
        return true;
    }

    private bool CanShowSecret()
    {
        return ((UniversalInputManager.UsePhoneUI == null) || (this == this.m_zone.GetCardAtIndex(0)));
    }

    public bool CanShowSecretDeath()
    {
        return ((UniversalInputManager.UsePhoneUI == null) || (this.m_prevZone.GetCardCount() == 0));
    }

    public bool CanShowSecretTrigger()
    {
        return ((UniversalInputManager.UsePhoneUI == null) || this.m_zone.IsOnlyCard(this));
    }

    public bool CardStandInIsInteractive()
    {
        return this.m_cardStandInInteractive;
    }

    private void CleanUpCustomSpell(Spell chosenSpell, ref Spell customSpell)
    {
        if (customSpell != null)
        {
            if (chosenSpell == customSpell)
            {
                customSpell = null;
            }
            else
            {
                UnityEngine.Object.Destroy(customSpell.gameObject);
            }
        }
    }

    public void CreateStartingCardStateEffects()
    {
        if (((this.m_entity.GetController() == null) || this.m_entity.GetController().IsFriendlySide()) || !this.m_entity.IsObfuscated())
        {
            if (this.m_entity.HasCharge())
            {
                this.m_actor.ActivateSpell(SpellType.CHARGE);
            }
            if (this.m_entity.HasTaunt())
            {
                this.m_actor.ActivateTaunt();
            }
            if (this.m_entity.IsStealthed())
            {
                this.m_actor.ActivateSpell(SpellType.STEALTH);
            }
            else if (this.m_entity.HasTag(GAME_TAG.CANT_BE_TARGETED_BY_HERO_POWERS))
            {
                this.m_actor.ActivateSpell(SpellType.UNTARGETABLE);
            }
            if (this.m_entity.HasDivineShield())
            {
                this.m_actor.ActivateSpell(SpellType.DIVINE_SHIELD);
            }
            if (this.m_entity.HasSpellPower())
            {
                this.m_actor.ActivateSpell(SpellType.SPELL_POWER);
            }
            if (this.m_entity.HasHeroPowerDamage())
            {
                this.m_actor.ActivateSpell(SpellType.SPELL_POWER);
            }
            if (this.m_entity.IsImmune() && !this.m_entity.DontShowImmune())
            {
                this.m_actor.ActivateSpell(SpellType.IMMUNE);
            }
            if (this.m_entity.HasHealthMin())
            {
                this.m_actor.ActivateSpell(SpellType.SHOUT_BUFF);
            }
            if (this.m_entity.IsAsleep())
            {
                this.m_actor.ActivateSpell(SpellType.Zzz);
            }
            if (this.m_entity.IsEnraged())
            {
                this.m_actor.ActivateSpell(SpellType.ENRAGE);
            }
            if (this.m_entity.HasWindfury())
            {
                Spell spell = this.m_actor.GetSpell(SpellType.WINDFURY_IDLE);
                if (spell != null)
                {
                    spell.ActivateState(SpellStateType.BIRTH);
                }
            }
            if (this.m_entity.HasDeathrattle())
            {
                this.m_actor.ActivateSpell(SpellType.DEATHRATTLE_IDLE);
            }
            if (this.m_entity.IsSilenced())
            {
                this.m_actor.ActivateSpell(SpellType.SILENCE);
            }
            SpellType spellType = this.PrioritizedBauble();
            if (spellType != SpellType.NONE)
            {
                this.m_actor.ActivateSpell(spellType);
            }
            if (this.m_entity.HasAura())
            {
                this.m_actor.ActivateSpell(SpellType.AURA);
            }
            if (this.m_entity.HasTag(GAME_TAG.AI_MUST_PLAY))
            {
                this.m_actor.ActivateSpell(SpellType.AUTO_CAST);
            }
            if (this.m_entity.IsFrozen())
            {
                this.m_actor.ActivateSpell(SpellType.FROZEN);
            }
            if (this.m_entity.HasEvilGlow())
            {
                this.m_actor.ActivateSpell(SpellType.EVIL_GLOW);
            }
            if (this.m_entity.HasTag(GAME_TAG.HEAVILY_ARMORED))
            {
                this.m_actor.ActivateSpell(SpellType.HEAVILY_ARMORED);
            }
            Player controller = this.m_entity.GetController();
            if (controller != null)
            {
                if (this.m_entity.IsHeroPower())
                {
                    if (controller.HasTag(GAME_TAG.STEADY_SHOT_CAN_TARGET))
                    {
                        this.m_actor.ActivateSpell(SpellType.STEADY_SHOT_CAN_TARGET);
                    }
                    if (controller.HasTag(GAME_TAG.CURRENT_HEROPOWER_DAMAGE_BONUS) && controller.IsHeroPowerAffectedByBonusDamage())
                    {
                        this.m_actor.ActivateSpell(SpellType.CURRENT_HEROPOWER_DAMAGE_BONUS);
                    }
                    if (this.m_entity.HasTag(GAME_TAG.ELECTRIC_CHARGE_LEVEL))
                    {
                        switch (this.m_entity.GetTag(GAME_TAG.ELECTRIC_CHARGE_LEVEL))
                        {
                            case 1:
                                this.m_actor.ActivateSpell(SpellType.ELECTRIC_CHARGE_LEVEL_MEDIUM);
                                break;

                            case 2:
                                this.m_actor.ActivateSpell(SpellType.ELECTRIC_CHARGE_LEVEL_LARGE);
                                break;
                        }
                    }
                }
                if (this.m_entity.IsHero())
                {
                    if (controller.HasTag(GAME_TAG.LOCK_AND_LOAD))
                    {
                        this.m_actor.ActivateSpell(SpellType.LOCK_AND_LOAD);
                    }
                    if (controller.HasTag(GAME_TAG.SHADOWFORM))
                    {
                        this.m_actor.ActivateSpell(SpellType.SHADOWFORM);
                    }
                }
            }
            switch (this.m_entity.GetZone())
            {
                case TAG_ZONE.PLAY:
                case TAG_ZONE.SECRET:
                    if (this.m_entity.HasCustomKeywordEffect())
                    {
                        this.ActivateCustomKeywordEffect();
                    }
                    this.ShowExhaustedChange(this.m_entity.IsExhausted());
                    break;
            }
        }
    }

    private void CutoffFriendlyCardDraw()
    {
        if (!this.m_actorReady)
        {
            if (this.m_actorWaitingToBeReplaced != null)
            {
                this.m_actorWaitingToBeReplaced.Destroy();
                this.m_actorWaitingToBeReplaced = null;
            }
            this.m_actor.Show();
            this.m_actor.TurnOffCollider();
            this.m_doNotSort = false;
            this.m_actorReady = true;
            this.CreateStartingCardStateEffects();
            this.RefreshActor();
            GameState.Get().ClearCardBeingDrawn(this);
            this.m_zone.UpdateLayout();
        }
    }

    public void DeactivateBaubles()
    {
        if (!this.m_actor.IsSpellDyingOrNone(SpellType.TRIGGER))
        {
            this.m_actor.DeactivateSpell(SpellType.TRIGGER);
        }
        if (!this.m_actor.IsSpellDyingOrNone(SpellType.POISONOUS))
        {
            this.m_actor.DeactivateSpell(SpellType.POISONOUS);
        }
        if (!this.m_actor.IsSpellDyingOrNone(SpellType.INSPIRE))
        {
            this.m_actor.DeactivateSpell(SpellType.INSPIRE);
        }
    }

    public void DeactivateCustomKeywordEffect()
    {
        if (this.m_customKeywordSpell != null)
        {
            Spell spell = this.m_customKeywordSpell.GetSpell(false);
            if ((spell != null) && spell.IsActive())
            {
                spell.ActivateState(SpellStateType.DEATH);
            }
        }
    }

    public void DeactivateLifetimeEffects()
    {
        if (this.m_lifetimeEffect != null)
        {
            Spell spell = this.m_lifetimeEffect.GetSpell(true);
            if (spell != null)
            {
                SpellStateType activeState = spell.GetActiveState();
                if ((activeState != SpellStateType.NONE) && (activeState != SpellStateType.DEATH))
                {
                    spell.ActivateState(SpellStateType.DEATH);
                }
            }
        }
    }

    public void Destroy()
    {
        if (this.m_actor != null)
        {
            this.m_actor.Destroy();
        }
        this.DestroyCardDefAssets();
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void DestroyCardAsset<T>(ref T asset) where T: Component
    {
        if (((T) asset) != null)
        {
            UnityEngine.Object.Destroy(asset.gameObject);
            asset = null;
        }
    }

    private void DestroyCardAsset<T>(T asset) where T: Component
    {
        if (asset != null)
        {
            UnityEngine.Object.Destroy(asset.gameObject);
        }
    }

    private void DestroyCardAudio(ref CardAudio cardAudio)
    {
        if (cardAudio != null)
        {
            cardAudio.Clear();
            cardAudio = null;
        }
    }

    private void DestroyCardDefAssets()
    {
        this.DestroyCardEffect(ref this.m_playEffect);
        this.DestroyCardEffect(ref this.m_attackEffect);
        this.DestroyCardEffect(ref this.m_deathEffect);
        this.DestroyCardEffect(ref this.m_lifetimeEffect);
        this.DestroyCardEffectList(ref this.m_subOptionEffects);
        this.DestroyCardEffectList(ref this.m_triggerEffects);
        this.DestroyCardEffect(ref this.m_customKeywordSpell);
        this.DestroyCardAudio(ref this.m_announcerLine);
        this.DestroyEmoteList();
        this.DestroyCardAsset<Spell>(ref this.m_customSummonSpell);
        this.DestroyCardAsset<Spell>(ref this.m_customSpawnSpell);
        this.DestroyCardAsset<Spell>(ref this.m_customSpawnSpellOverride);
        this.DestroyCardAsset<Spell>(ref this.m_customDeathSpell);
        this.DestroyCardAsset<Spell>(ref this.m_customDeathSpellOverride);
    }

    private void DestroyCardEffect(ref CardEffect effect)
    {
        if (effect != null)
        {
            effect.Clear();
            effect = null;
        }
    }

    private void DestroyCardEffectList(ref List<CardEffect> effects)
    {
        if (effects != null)
        {
            foreach (CardEffect effect in effects)
            {
                effect.Clear();
            }
            effects = null;
        }
    }

    private void DestroyEmoteList()
    {
        if (this.m_emotes != null)
        {
            for (int i = 0; i < this.m_emotes.Count; i++)
            {
                this.m_emotes[i].Clear();
            }
            this.m_emotes = null;
        }
    }

    private void DestroySpellList<T>(List<T> spells) where T: Spell
    {
        if (spells != null)
        {
            for (int i = 0; i < spells.Count; i++)
            {
                this.DestroyCardAsset<T>(spells[i]);
            }
            spells = null;
        }
    }

    private void DiscardCardBeingDrawn()
    {
        if (this == GameState.Get().GetOpponentCardBeingDrawn())
        {
            this.m_actorWaitingToBeReplaced.Destroy();
            this.m_actorWaitingToBeReplaced = null;
        }
        if (this.m_actor.IsShown())
        {
            this.ActivateDeathSpell(this.m_actor);
        }
        else
        {
            GameState.Get().ClearCardBeingDrawn(this);
        }
    }

    private bool DoChoiceHighlight(GameState state)
    {
        if (state.GetChosenEntities().Contains(this.m_entity))
        {
            if (this.m_mousedOver)
            {
                this.m_actor.SetActorState(ActorStateType.CARD_PLAYABLE_MOUSE_OVER);
            }
            else
            {
                this.m_actor.SetActorState(ActorStateType.CARD_SELECTED);
            }
            return true;
        }
        int entityId = this.m_entity.GetEntityId();
        if (!state.GetFriendlyEntityChoices().Entities.Contains(entityId))
        {
            return false;
        }
        if (GameState.Get().IsMulliganManagerActive())
        {
            this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
        }
        else
        {
            this.m_actor.SetActorState(ActorStateType.CARD_SELECTABLE);
        }
        return true;
    }

    private void DoDiscardAnimation()
    {
        this.m_doNotSort = true;
        iTween.Stop(base.gameObject);
        float num = 3f;
        if (this.GetEntity().IsControlledByOpposingSidePlayer())
        {
            num = -num;
        }
        Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z + num);
        iTween.MoveTo(base.gameObject, position, 3f);
        iTween.ScaleTo(base.gameObject, (Vector3) (base.transform.localScale * 1.5f), 3f);
        base.StartCoroutine(this.ActivateGraveyardActorDeathSpellAfterDelay());
    }

    private bool DoesCardReturnFromGraveyard()
    {
        IEnumerator<PowerTaskList> enumerator = GameState.Get().GetPowerProcessor().GetPowerQueue().GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                PowerTaskList current = enumerator.Current;
                if (this.DoesTaskListReturnCardFromGraveyard(current))
                {
                    object[] args = new object[] { this.m_entity };
                    Log.JMac.PrintWarning("Found the task for returning entity {0} from graveyard!", args);
                    return true;
                }
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
        return false;
    }

    private bool DoesTaskListBlockCardDraw(PowerTaskList taskList)
    {
        if (taskList != null)
        {
            Network.HistActionStart sourceAction = taskList.GetSourceAction();
            if (sourceAction == null)
            {
                return false;
            }
            switch (sourceAction.BlockType)
            {
                case HistoryBlock.Type.POWER:
                case HistoryBlock.Type.TRIGGER:
                {
                    if (!taskList.IsComplete())
                    {
                        Entity sourceEntity = taskList.GetSourceEntity();
                        if (this.m_entity == sourceEntity)
                        {
                            return true;
                        }
                        int entityId = this.m_entity.GetEntityId();
                        List<PowerTask> list = taskList.GetTaskList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            Network.PowerHistory power = list[i].GetPower();
                            int zone = 0;
                            switch (power.Type)
                            {
                                case Network.PowerType.SHOW_ENTITY:
                                {
                                    Network.HistShowEntity entity2 = (Network.HistShowEntity) power;
                                    if (entity2.Entity.ID == entityId)
                                    {
                                        if (<>f__am$cache3E == null)
                                        {
                                            <>f__am$cache3E = currTag => currTag.Name == 0x31;
                                        }
                                        Network.Entity.Tag tag = entity2.Entity.Tags.Find(<>f__am$cache3E);
                                        if (tag != null)
                                        {
                                            zone = tag.Value;
                                        }
                                    }
                                    break;
                                }
                                case Network.PowerType.HIDE_ENTITY:
                                {
                                    Network.HistHideEntity entity3 = (Network.HistHideEntity) power;
                                    if (entity3.Entity == entityId)
                                    {
                                        zone = entity3.Zone;
                                    }
                                    break;
                                }
                                case Network.PowerType.TAG_CHANGE:
                                {
                                    Network.HistTagChange change = (Network.HistTagChange) power;
                                    if ((change.Entity == entityId) && (change.Tag == 0x31))
                                    {
                                        zone = change.Value;
                                    }
                                    break;
                                }
                            }
                            if ((zone != 0) && (zone != 3))
                            {
                                return true;
                            }
                        }
                    }
                    PowerTaskList next = taskList.GetNext();
                    return this.DoesTaskListBlockCardDraw(next);
                }
            }
        }
        return false;
    }

    private bool DoesTaskListReturnCardFromGraveyard(PowerTaskList taskList)
    {
        Network.HistActionStart sourceAction = taskList.GetSourceAction();
        if ((sourceAction != null) && (sourceAction.BlockType == HistoryBlock.Type.TRIGGER))
        {
            foreach (PowerTask task in taskList.GetTaskList())
            {
                Network.PowerHistory power = task.GetPower();
                if (power.Type == Network.PowerType.TAG_CHANGE)
                {
                    Network.HistTagChange change = power as Network.HistTagChange;
                    if ((change.Tag == 0x31) && (change.Entity == this.m_entity.GetEntityId()))
                    {
                        if (change.Value == 6)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void DoNullZoneVisuals()
    {
        this.HideCard();
    }

    private bool DoOptionHighlight(GameState state)
    {
        if (!GameState.Get().IsOption(this.m_entity))
        {
            return false;
        }
        bool flag = this.m_entity.GetZone() == TAG_ZONE.HAND;
        bool flag2 = this.m_entity.GetController().IsRealTimeComboActive();
        if ((flag && flag2) && this.m_entity.HasTag(GAME_TAG.COMBO))
        {
            this.m_actor.SetActorState(ActorStateType.CARD_COMBO);
            return true;
        }
        bool realTimePoweredUp = this.m_entity.GetRealTimePoweredUp();
        if (flag && realTimePoweredUp)
        {
            this.m_actor.SetActorState(ActorStateType.CARD_POWERED_UP);
            return true;
        }
        if (!flag && this.m_mousedOver)
        {
            this.m_actor.SetActorState(ActorStateType.CARD_PLAYABLE_MOUSE_OVER);
            return true;
        }
        this.m_actor.SetActorState(ActorStateType.CARD_PLAYABLE);
        return true;
    }

    private bool DoOptionTargetHighlight(GameState state)
    {
        Network.Options.Option.SubOption selectedNetworkSubOption = state.GetSelectedNetworkSubOption();
        int entityId = this.m_entity.GetEntityId();
        if (!selectedNetworkSubOption.Targets.Contains(entityId))
        {
            return false;
        }
        if (this.m_mousedOver)
        {
            this.m_actor.SetActorState(ActorStateType.CARD_VALID_TARGET_MOUSE_OVER);
        }
        else
        {
            this.m_actor.SetActorState(ActorStateType.CARD_VALID_TARGET);
        }
        return true;
    }

    private void DoPlayToDeckTransition(Actor playActor)
    {
        this.m_doNotSort = true;
        this.m_actor.Hide();
        base.StartCoroutine(this.AnimatePlayToDeck(playActor));
    }

    private bool DoPlayToHandTransition(Actor oldActor, bool wasInGraveyard = false)
    {
        if (this.m_entity.IsControlledByConcealedPlayer())
        {
            this.m_entity.SetTag(GAME_TAG.ATK, this.m_entity.GetEntityDef().GetATK());
            this.m_entity.SetTag(GAME_TAG.HEALTH, this.m_entity.GetEntityDef().GetHealth());
            this.m_entity.SetTag(GAME_TAG.DAMAGE, 0);
        }
        bool flag = this.ActivateActorSpells_PlayToHand(oldActor, wasInGraveyard);
        if (flag)
        {
            this.m_actor.Hide();
        }
        return flag;
    }

    private bool DoSubOptionHighlight(GameState state)
    {
        Network.Options.Option selectedNetworkOption = state.GetSelectedNetworkOption();
        int entityId = this.m_entity.GetEntityId();
        foreach (Network.Options.Option.SubOption option2 in selectedNetworkOption.Subs)
        {
            if (entityId == option2.ID)
            {
                if (this.m_mousedOver)
                {
                    this.m_actor.SetActorState(ActorStateType.CARD_PLAYABLE_MOUSE_OVER);
                }
                else
                {
                    this.m_actor.SetActorState(ActorStateType.CARD_PLAYABLE);
                }
                return true;
            }
        }
        return false;
    }

    public void DoTauntNotification()
    {
        iTween.PunchScale(this.m_actor.gameObject, new Vector3(0.2f, 0.2f, 0.2f), 0.5f);
    }

    public void DrawFriendlyCard()
    {
        base.StartCoroutine(this.DrawFriendlyCardWithTiming());
    }

    [DebuggerHidden]
    private IEnumerator DrawFriendlyCardWithTiming()
    {
        return new <DrawFriendlyCardWithTiming>c__Iterator23F { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator DrawKnownOpponentCard(ZoneHand handZone)
    {
        return new <DrawKnownOpponentCard>c__Iterator242 { handZone = handZone, <$>handZone = handZone, <>f__this = this };
    }

    private void DrawOpponentCard()
    {
        base.StartCoroutine(this.DrawOpponentCardWithTiming());
    }

    [DebuggerHidden]
    private IEnumerator DrawOpponentCardWithTiming()
    {
        return new <DrawOpponentCardWithTiming>c__Iterator240 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator DrawUnknownOpponentCard(ZoneHand handZone)
    {
        return new <DrawUnknownOpponentCard>c__Iterator241 { handZone = handZone, <$>handZone = handZone, <>f__this = this };
    }

    public void EnableAttacking(bool enable)
    {
        this.m_attacking = enable;
    }

    public void EnableTransitioningZones(bool enable)
    {
        this.m_transitioningZones = enable;
    }

    private void FinishActorLoad(Actor oldActor)
    {
        this.m_actorLoading = false;
        this.OnZoneChanged();
        this.OnActorChanged(oldActor);
        if (this.m_isBattleCrySource)
        {
            SceneUtils.SetLayer(this.m_actor.gameObject, GameLayer.IgnoreFullScreenEffects);
        }
        this.RefreshActor();
    }

    private void FinishSpellLoad()
    {
        this.m_spellLoadCount--;
        if (this.m_spellLoadCount <= 0)
        {
            this.LoadActor();
        }
    }

    public void ForceLoadHandActor()
    {
        string cardName = this.m_cardDef.DetermineActorNameForZone(this.m_entity, TAG_ZONE.HAND);
        if ((this.m_actor != null) && (this.m_actorName == cardName))
        {
            this.ShowCard();
            this.m_actor.Show();
            this.RefreshActor();
        }
        else
        {
            this.m_actorReady = false;
            this.m_actorLoading = true;
            AssetLoader.Get().LoadActor(cardName, new AssetLoader.GameObjectCallback(this.OnHandActorForceLoaded), this.m_zone, false);
        }
    }

    public Actor GetActor()
    {
        return this.m_actor;
    }

    public Spell GetActorAttackSpellForInput()
    {
        if (this.m_actor == null)
        {
            object[] args = new object[] { this };
            Log.Mike.Print("{0}.GetActorAttackSpellForInput() - m_actor IS NULL", args);
            return null;
        }
        if (this.m_zone == null)
        {
            object[] objArray2 = new object[] { this };
            Log.Mike.Print("{0}.GetActorAttackSpellForInput() - m_zone IS NULL", objArray2);
            return null;
        }
        Spell spell = this.m_actor.GetSpell(SpellType.FRIENDLY_ATTACK);
        if (spell == null)
        {
            object[] objArray3 = new object[] { this, SpellType.FRIENDLY_ATTACK };
            Log.Mike.Print("{0}.GetActorAttackSpellForInput() - {1} spell is null", objArray3);
            return null;
        }
        return spell;
    }

    public string GetActorName()
    {
        return this.m_actorName;
    }

    public Spell GetActorSpell(SpellType spellType)
    {
        if (this.m_actor == null)
        {
            Log.Mike.Print(string.Format("{0}.GetActorSpell() - m_actor IS NULL", this), new object[0]);
            return null;
        }
        Spell spell = this.m_actor.GetSpell(spellType);
        if (spell == null)
        {
            return null;
        }
        return spell;
    }

    public AudioSource GetAnnouncerLine()
    {
        return this.m_announcerLine.GetAudio();
    }

    public List<CardSoundSpell> GetAttackSoundSpells(bool loadIfNeeded = true)
    {
        if (this.m_attackEffect == null)
        {
            return null;
        }
        return this.m_attackEffect.GetSoundSpells(loadIfNeeded);
    }

    public Spell GetAttackSpell(bool loadIfNeeded = true)
    {
        if (this.m_attackEffect == null)
        {
            return null;
        }
        return this.m_attackEffect.GetSpell(loadIfNeeded);
    }

    public Spell GetBestDeathSpell()
    {
        bool flag;
        return this.GetBestDeathSpell(out flag);
    }

    public Spell GetBestDeathSpell(out bool standard)
    {
        return this.GetBestDeathSpell(this.m_actor, out standard);
    }

    private Spell GetBestDeathSpell(Actor actor)
    {
        bool flag;
        return this.GetBestDeathSpell(actor, out flag);
    }

    private Spell GetBestDeathSpell(Actor actor, out bool standard)
    {
        standard = false;
        if (!this.m_entity.IsSilenced())
        {
            if (this.m_customDeathSpellOverride != null)
            {
                return this.m_customDeathSpellOverride;
            }
            if (this.m_customDeathSpell != null)
            {
                return this.m_customDeathSpell;
            }
        }
        standard = true;
        return actor.GetSpell(SpellType.DEATH);
    }

    public Spell GetBestSpawnSpell()
    {
        bool flag;
        return this.GetBestSpawnSpell(out flag);
    }

    public Spell GetBestSpawnSpell(out bool standard)
    {
        standard = false;
        if (this.m_customSpawnSpellOverride != null)
        {
            return this.m_customSpawnSpellOverride;
        }
        if (this.m_customSpawnSpell != null)
        {
            return this.m_customSpawnSpell;
        }
        standard = true;
        if (this.m_entity.IsControlledByFriendlySidePlayer())
        {
            return this.GetActorSpell(SpellType.FRIENDLY_SPAWN_MINION);
        }
        return this.GetActorSpell(SpellType.OPPONENT_SPAWN_MINION);
    }

    public Spell GetBestSummonSpell()
    {
        bool flag;
        return this.GetBestSummonSpell(out flag);
    }

    public Spell GetBestSummonSpell(out bool standard)
    {
        if (this.m_customSummonSpell != null)
        {
            standard = false;
            return this.m_customSummonSpell;
        }
        standard = true;
        SpellType spellType = this.m_cardDef.DetermineSummonInSpell_HandToPlay(this);
        return this.GetActorSpell(spellType);
    }

    public CardDef GetCardDef()
    {
        return this.m_cardDef;
    }

    public CardFlair GetCardFlair()
    {
        if (this.m_entity == null)
        {
            return null;
        }
        return this.m_entity.GetCardFlair();
    }

    public Player GetController()
    {
        if (this.m_entity == null)
        {
            return null;
        }
        return this.m_entity.GetController();
    }

    public Player.Side GetControllerSide()
    {
        if (this.m_entity == null)
        {
            return Player.Side.NEUTRAL;
        }
        return this.m_entity.GetControllerSide();
    }

    public Spell GetCustomDeathSpell()
    {
        return this.m_customDeathSpell;
    }

    public Spell GetCustomDeathSpellOverride()
    {
        return this.m_customDeathSpellOverride;
    }

    public Spell GetCustomSpawnSpell()
    {
        return this.m_customSpawnSpell;
    }

    public Spell GetCustomSpawnSpellOverride()
    {
        return this.m_customSpawnSpellOverride;
    }

    public Spell GetCustomSummonSpell()
    {
        return this.m_customSummonSpell;
    }

    public List<CardSoundSpell> GetDeathSoundSpells(bool loadIfNeeded = true)
    {
        if (this.m_deathEffect == null)
        {
            return null;
        }
        return this.m_deathEffect.GetSoundSpells(loadIfNeeded);
    }

    private EmoteEntry GetEmoteEntry(EmoteType emoteType)
    {
        if (this.m_emotes != null)
        {
            if (SpecialEventManager.Get().IsEventActive(SpecialEventType.LUNAR_NEW_YEAR, false))
            {
                if (emoteType == EmoteType.GREETINGS)
                {
                    foreach (EmoteEntry entry in this.m_emotes)
                    {
                        if (entry.GetEmoteType() == EmoteType.EVENT_LUNAR_NEW_YEAR)
                        {
                            return entry;
                        }
                    }
                }
            }
            else if (SpecialEventManager.Get().IsEventActive(SpecialEventType.FEAST_OF_WINTER_VEIL, false) && (emoteType == EmoteType.GREETINGS))
            {
                foreach (EmoteEntry entry2 in this.m_emotes)
                {
                    if (entry2.GetEmoteType() == EmoteType.EVENT_WINTER_VEIL)
                    {
                        return entry2;
                    }
                }
            }
            foreach (EmoteEntry entry3 in this.m_emotes)
            {
                if (entry3.GetEmoteType() == emoteType)
                {
                    return entry3;
                }
            }
        }
        return null;
    }

    public Entity GetEntity()
    {
        return this.m_entity;
    }

    public Material GetGoldenMaterial()
    {
        return ((this.m_cardDef != null) ? this.m_cardDef.GetPremiumPortraitMaterial() : null);
    }

    public Entity GetHero()
    {
        Player controller = this.GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetHero();
    }

    public Card GetHeroCard()
    {
        Entity hero = this.GetHero();
        if (hero == null)
        {
            return null;
        }
        return hero.GetCard();
    }

    public Entity GetHeroPower()
    {
        Player controller = this.GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetHeroPower();
    }

    public Card GetHeroPowerCard()
    {
        Entity heroPower = this.GetHeroPower();
        if (heroPower == null)
        {
            return null;
        }
        return heroPower.GetCard();
    }

    public List<CardSoundSpell> GetLifetimeSoundSpells(bool loadIfNeeded = true)
    {
        if (this.m_lifetimeEffect == null)
        {
            return null;
        }
        return this.m_lifetimeEffect.GetSoundSpells(loadIfNeeded);
    }

    public Spell GetLifetimeSpell(bool loadIfNeeded = true)
    {
        if (this.m_lifetimeEffect == null)
        {
            return null;
        }
        return this.m_lifetimeEffect.GetSpell(loadIfNeeded);
    }

    public List<CardSoundSpell> GetPlaySoundSpells(bool loadIfNeeded = true)
    {
        if (this.m_playEffect == null)
        {
            return null;
        }
        return this.m_playEffect.GetSoundSpells(loadIfNeeded);
    }

    public Spell GetPlaySpell(bool loadIfNeeded = true)
    {
        if (this.m_playEffect == null)
        {
            return null;
        }
        return this.m_playEffect.GetSpell(loadIfNeeded);
    }

    public Texture GetPortraitTexture()
    {
        return ((this.m_cardDef != null) ? this.m_cardDef.GetPortraitTexture() : null);
    }

    public int GetPredictedZonePosition()
    {
        return this.m_predictedZonePosition;
    }

    public Zone GetPrevZone()
    {
        return this.m_prevZone;
    }

    public bool GetShouldShowTooltip()
    {
        return this.m_shouldShowTooltip;
    }

    public CardEffect GetSubOptionEffect(int index)
    {
        if (this.m_subOptionEffects == null)
        {
            return null;
        }
        if (index < 0)
        {
            return null;
        }
        if (index >= this.m_subOptionEffects.Count)
        {
            return null;
        }
        return this.m_subOptionEffects[index];
    }

    public List<CardSoundSpell> GetSubOptionSoundSpells(int index, bool loadIfNeeded = true)
    {
        CardEffect subOptionEffect = this.GetSubOptionEffect(index);
        if (subOptionEffect == null)
        {
            return null;
        }
        return subOptionEffect.GetSoundSpells(loadIfNeeded);
    }

    public Spell GetSubOptionSpell(int index, bool loadIfNeeded = true)
    {
        CardEffect subOptionEffect = this.GetSubOptionEffect(index);
        if (subOptionEffect == null)
        {
            return null;
        }
        return subOptionEffect.GetSpell(loadIfNeeded);
    }

    public float GetTransitionDelay()
    {
        return this.m_transitionDelay;
    }

    public ZoneTransitionStyle GetTransitionStyle()
    {
        return this.m_transitionStyle;
    }

    public CardEffect GetTriggerEffect(int index)
    {
        if (this.m_triggerEffects == null)
        {
            return null;
        }
        if (index < 0)
        {
            return null;
        }
        if (index >= this.m_triggerEffects.Count)
        {
            return null;
        }
        return this.m_triggerEffects[index];
    }

    public List<CardSoundSpell> GetTriggerSoundSpells(int index, bool loadIfNeeded = true)
    {
        CardEffect triggerEffect = this.GetTriggerEffect(index);
        if (triggerEffect == null)
        {
            return null;
        }
        return triggerEffect.GetSoundSpells(loadIfNeeded);
    }

    public Spell GetTriggerSpell(int index, bool loadIfNeeded = true)
    {
        CardEffect triggerEffect = this.GetTriggerEffect(index);
        if (triggerEffect == null)
        {
            return null;
        }
        return triggerEffect.GetSpell(loadIfNeeded);
    }

    public Zone GetZone()
    {
        return this.m_zone;
    }

    public int GetZonePosition()
    {
        return this.m_zonePosition;
    }

    private bool HandleGraveyardToDeck(Actor oldActor)
    {
        if (this.m_actorWaitingToBeReplaced == null)
        {
            return false;
        }
        if (oldActor != null)
        {
            oldActor.Destroy();
        }
        oldActor = this.m_actorWaitingToBeReplaced;
        this.m_actorWaitingToBeReplaced = null;
        this.DoPlayToDeckTransition(oldActor);
        return true;
    }

    private bool HandleGraveyardToHand(Actor oldActor)
    {
        if (this.m_actorWaitingToBeReplaced != null)
        {
            if ((oldActor != null) && (oldActor != this.m_actor))
            {
                oldActor.Destroy();
            }
            oldActor = this.m_actorWaitingToBeReplaced;
            this.m_actorWaitingToBeReplaced = null;
            if (this.DoPlayToHandTransition(oldActor, true))
            {
                return true;
            }
        }
        return false;
    }

    private bool HandlePlayActorDeath(Actor oldActor)
    {
        bool flag = false;
        if (!this.m_cardDef.m_SuppressDeathrattleDeath && this.m_entity.HasDeathrattle())
        {
            this.ActivateActorSpell(oldActor, SpellType.DEATHRATTLE_DEATH);
        }
        if (this.m_suppressDeathEffects)
        {
            if (oldActor != null)
            {
                oldActor.Destroy();
            }
            if (this.IsShown())
            {
                this.ShowImpl();
            }
            else
            {
                this.HideImpl();
            }
            flag = true;
            this.m_actorReady = true;
            return flag;
        }
        if (!this.m_suppressKeywordDeaths)
        {
            base.StartCoroutine(this.WaitAndPrepareForDeathAnimation(oldActor));
        }
        if (this.ActivateDeathSpell(oldActor) != null)
        {
            this.m_actor.Hide();
            flag = true;
            this.m_actorReady = true;
        }
        return flag;
    }

    public bool HasActiveEmoteSound()
    {
        if (this.m_emotes != null)
        {
            foreach (EmoteEntry entry in this.m_emotes)
            {
                CardSoundSpell spell = entry.GetSpell(false);
                if ((spell != null) && spell.IsActive())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasBeenGrabbedByEnemyActionHandler()
    {
        return this.m_hasBeenGrabbedByEnemyActionHandler;
    }

    public void HideCard()
    {
        if (this.m_shown)
        {
            this.m_shown = false;
            this.HideImpl();
        }
    }

    private void HideImpl()
    {
        if (this.m_actor != null)
        {
            this.m_actor.Hide();
        }
    }

    [DebuggerHidden]
    private IEnumerator HideRevealedOpponentCard(Actor handActor)
    {
        return new <HideRevealedOpponentCard>c__Iterator244 { handActor = handActor, <$>handActor = handActor, <>f__this = this };
    }

    public void HideTooltip()
    {
        this.m_shouldShowTooltip = false;
        if (this.m_showTooltip)
        {
            this.m_showTooltip = false;
            this.UpdateTooltip();
        }
    }

    public void IgnoreDeath(bool ignore)
    {
        this.m_ignoreDeath = ignore;
    }

    private void InitActor(string actorName, Actor actor)
    {
        Actor oldActor = this.m_actor;
        this.m_actor = actor;
        this.m_actorName = actorName;
        this.m_actor.SetEntity(this.m_entity);
        this.m_actor.SetCard(this);
        this.m_actor.SetCardDef(this.m_cardDef);
        this.m_actor.UpdateAllComponents();
        this.FinishActorLoad(oldActor);
    }

    private void InitCardDefAssets()
    {
        this.InitEffect(this.m_cardDef.m_PlayEffectDef, ref this.m_playEffect);
        this.InitEffect(this.m_cardDef.m_AttackEffectDef, ref this.m_attackEffect);
        this.InitEffect(this.m_cardDef.m_DeathEffectDef, ref this.m_deathEffect);
        this.InitEffect(this.m_cardDef.m_LifetimeEffectDef, ref this.m_lifetimeEffect);
        this.InitEffect(this.m_cardDef.m_CustomKeywordSpellPath, ref this.m_customKeywordSpell);
        this.InitEffectList(this.m_cardDef.m_SubOptionEffectDefs, ref this.m_subOptionEffects);
        this.InitEffectList(this.m_cardDef.m_TriggerEffectDefs, ref this.m_triggerEffects);
        this.InitSound(this.m_cardDef.m_AnnouncerLinePath, ref this.m_announcerLine);
        this.InitEmoteList();
    }

    private void InitEffect(CardEffectDef effectDef, ref CardEffect effect)
    {
        this.DestroyCardEffect(ref effect);
        if (effectDef != null)
        {
            effect = new CardEffect(effectDef, new CardEffect.InitSpellFunc(this.SetupSpell), new CardEffect.InitSpellFunc(this.SetupSoundSpell));
            this.m_effects.Add(effect);
            if (this.ShouldPreloadCardAssets())
            {
                effect.LoadAll();
            }
        }
    }

    private void InitEffect(string spellPath, ref CardEffect effect)
    {
        this.DestroyCardEffect(ref effect);
        if (spellPath != null)
        {
            effect = new CardEffect(spellPath, new CardEffect.InitSpellFunc(this.SetupSpell), null);
            if (this.ShouldPreloadCardAssets())
            {
                effect.LoadAll();
            }
        }
    }

    private void InitEffectList(List<CardEffectDef> effectDefs, ref List<CardEffect> effects)
    {
        this.DestroyCardEffectList(ref effects);
        if (effectDefs != null)
        {
            effects = new List<CardEffect>();
            for (int i = 0; i < effectDefs.Count; i++)
            {
                CardEffectDef def = effectDefs[i];
                CardEffect item = null;
                if (def != null)
                {
                    item = new CardEffect(def, new CardEffect.InitSpellFunc(this.SetupSpell), new CardEffect.InitSpellFunc(this.SetupSoundSpell));
                    if (this.ShouldPreloadCardAssets())
                    {
                        item.LoadAll();
                    }
                }
                effects.Add(item);
            }
        }
    }

    private void InitEmoteList()
    {
        this.DestroyEmoteList();
        if (this.m_cardDef.m_EmoteDefs != null)
        {
            this.m_emotes = new List<EmoteEntry>();
            for (int i = 0; i < this.m_cardDef.m_EmoteDefs.Count; i++)
            {
                EmoteEntryDef def = this.m_cardDef.m_EmoteDefs[i];
                if (string.IsNullOrEmpty(def.m_emoteSoundSpellPath))
                {
                    UnityEngine.Debug.LogWarning(string.Format("m_emoteSoundSpellPath not defined for {0}", this.m_entity.GetCardId()));
                }
                EmoteEntry item = new EmoteEntry(def.m_emoteType, def.m_emoteSoundSpellPath, def.m_emoteGameStringKey, new EmoteEntry.InitSpellFunc(this.SetupSoundSpell));
                if (this.ShouldPreloadCardAssets())
                {
                    item.GetSpell(true);
                }
                this.m_emotes.Add(item);
            }
        }
    }

    private void InitSound(string path, ref CardAudio audio)
    {
        this.DestroyCardAudio(ref audio);
        audio = new CardAudio(path);
        if (this.ShouldPreloadCardAssets())
        {
            audio.GetAudio();
        }
    }

    public bool IsAbleToShowTooltip()
    {
        if (this.m_entity == null)
        {
            return false;
        }
        if (this.m_actor == null)
        {
            return false;
        }
        if (BigCard.Get() == null)
        {
            return false;
        }
        return true;
    }

    public bool IsActorLoading()
    {
        return this.m_actorLoading;
    }

    public bool IsActorReady()
    {
        return this.m_actorReady;
    }

    public bool IsAllowedToShowTooltip()
    {
        if (this.m_zone == null)
        {
            return false;
        }
        if (((this.m_zone.m_ServerTag != TAG_ZONE.PLAY) && (this.m_zone.m_ServerTag != TAG_ZONE.SECRET)) && ((this.m_zone.m_ServerTag == TAG_ZONE.HAND) && (this.m_zone.m_Side != Player.Side.OPPOSING)))
        {
            return false;
        }
        if (((GameState.Get() != null) && this.m_entity.IsHero()) && !GameState.Get().GetGameEntity().ShouldShowHeroTooltips())
        {
            return false;
        }
        return true;
    }

    public bool IsAttacking()
    {
        return this.m_attacking;
    }

    public bool IsBeingDrawnByOpponent()
    {
        return this.m_beingDrawnByOpponent;
    }

    public bool IsDoNotSort()
    {
        return this.m_doNotSort;
    }

    public bool IsDoNotWarpToNewZone()
    {
        return this.m_doNotWarpToNewZone;
    }

    public bool IsHoldingForLinkedCardSwitch()
    {
        return this.m_holdingForLinkedCardSwitch;
    }

    public bool IsInputEnabled()
    {
        return this.m_inputEnabled;
    }

    public bool IsMousedOver()
    {
        return this.m_mousedOver;
    }

    public bool IsOverPlayfield()
    {
        return this.m_overPlayfield;
    }

    public bool IsShowingTooltip()
    {
        return this.m_showTooltip;
    }

    public bool IsShown()
    {
        return this.m_shown;
    }

    public bool IsTransitioningZones()
    {
        return this.m_transitioningZones;
    }

    private void LoadActor()
    {
        string cardName = this.m_cardDef.DetermineActorNameForZone(this.m_entity, this.m_zone.m_ServerTag);
        if ((this.m_actorName == cardName) || (cardName == null))
        {
            this.m_actorName = cardName;
            this.FinishActorLoad(this.m_actor);
        }
        else
        {
            AssetLoader.Get().LoadActor(cardName, new AssetLoader.GameObjectCallback(this.OnActorLoaded), null, false);
        }
    }

    private void LoadActorAndSpells()
    {
        this.m_actorLoading = true;
        List<SpellLoadRequest> list = new List<SpellLoadRequest>();
        if ((this.m_prevZone is ZoneHand) && (this.m_zone is ZonePlay))
        {
            SpellLoadRequest item = this.MakeCustomSpellLoadRequest(this.m_cardDef.m_CustomSummonSpellPath, this.m_cardDef.m_GoldenCustomSummonSpellPath, new AssetLoader.GameObjectCallback(this.OnCustomSummonSpellLoaded));
            if (item != null)
            {
                list.Add(item);
            }
        }
        if ((this.m_customDeathSpell == null) && ((this.m_zone is ZoneHand) || (this.m_zone is ZonePlay)))
        {
            SpellLoadRequest request2 = this.MakeCustomSpellLoadRequest(this.m_cardDef.m_CustomDeathSpellPath, this.m_cardDef.m_GoldenCustomDeathSpellPath, new AssetLoader.GameObjectCallback(this.OnCustomDeathSpellLoaded));
            if (request2 != null)
            {
                list.Add(request2);
            }
        }
        if ((this.m_customSpawnSpell == null) && (this.m_zone is ZonePlay))
        {
            SpellLoadRequest request3 = this.MakeCustomSpellLoadRequest(this.m_cardDef.m_CustomSpawnSpellPath, this.m_cardDef.m_GoldenCustomSpawnSpellPath, new AssetLoader.GameObjectCallback(this.OnCustomSpawnSpellLoaded));
            if (request3 != null)
            {
                list.Add(request3);
            }
        }
        this.m_spellLoadCount = list.Count;
        if (list.Count == 0)
        {
            this.LoadActor();
        }
        else
        {
            foreach (SpellLoadRequest request4 in list)
            {
                AssetLoader.Get().LoadSpell(request4.m_path, request4.m_loadCallback, null, false);
            }
        }
    }

    public void LoadCardDef(CardDef cardDef)
    {
        if (this.m_cardDef != cardDef)
        {
            this.m_cardDef = cardDef;
            this.InitCardDefAssets();
            if (this.m_actor != null)
            {
                this.m_actor.SetCardDef(this.m_cardDef);
                this.m_actor.UpdateAllComponents();
            }
        }
    }

    private SpellLoadRequest MakeCustomSpellLoadRequest(string customPath, string goldenCustomPath, AssetLoader.GameObjectCallback loadCallback)
    {
        string str = customPath;
        if ((this.m_entity.GetPremiumType() == TAG_PREMIUM.GOLDEN) && !string.IsNullOrEmpty(goldenCustomPath))
        {
            str = goldenCustomPath;
        }
        else if (string.IsNullOrEmpty(str))
        {
            return null;
        }
        return new SpellLoadRequest { m_path = str, m_loadCallback = loadCallback };
    }

    public void MarkAsGrabbedByEnemyActionHandler(bool enable)
    {
        object[] args = new object[] { this, enable };
        Log.FaceDownCard.Print("Card.MarkAsGrabbedByEnemyActionHandler() - card={0} enable={1}", args);
        this.m_hasBeenGrabbedByEnemyActionHandler = enable;
    }

    private void MillCard()
    {
        base.StartCoroutine(this.MillCardWithTiming());
    }

    [DebuggerHidden]
    private IEnumerator MillCardWithTiming()
    {
        return new <MillCardWithTiming>c__Iterator246 { <>f__this = this };
    }

    public void NotifyLeftPlayfield()
    {
        this.m_overPlayfield = false;
        this.UpdateActorState();
    }

    public void NotifyMousedOut()
    {
        this.m_mousedOver = false;
        this.UpdateActorState();
        this.UpdateProposedManaUsage();
        if (RemoteActionHandler.Get() != null)
        {
            RemoteActionHandler.Get().NotifyOpponentOfMouseOut();
        }
        if (KeywordHelpPanelManager.Get() != null)
        {
            KeywordHelpPanelManager.Get().HideKeywordHelp();
        }
        if (CardTypeBanner.Get() != null)
        {
            CardTypeBanner.Get().Hide(this.m_actor);
        }
        if (GameState.Get() != null)
        {
            GameState.Get().GetGameEntity().NotifyOfCardMousedOff(this.GetEntity());
        }
        if (((this.m_entity.HasSpellPower() && this.m_entity.IsControlledByFriendlySidePlayer()) && (this.m_entity.IsHero() || (this.m_zone is ZonePlay))) && !this.m_transitioningZones)
        {
            ZoneMgr.Get().FindZoneOfType<ZoneHand>(this.m_zone.m_Side).OnSpellPowerEntityMousedOut();
        }
        if ((this.m_entity.IsWeapon() && this.m_entity.IsExhausted()) && ((this.m_actor != null) && (this.m_actor.GetAttackObject() != null)))
        {
            this.m_actor.GetAttackObject().ScaleToZero();
        }
    }

    public void NotifyMousedOver()
    {
        this.m_mousedOver = true;
        this.UpdateActorState();
        this.UpdateProposedManaUsage();
        if ((RemoteActionHandler.Get() != null) && (TargetReticleManager.Get() != null))
        {
            RemoteActionHandler.Get().NotifyOpponentOfMouseOverEntity(this.GetEntity().GetCard());
        }
        if (GameState.Get() != null)
        {
            GameState.Get().GetGameEntity().NotifyOfCardMousedOver(this.GetEntity());
        }
        if (this.m_zone is ZoneHand)
        {
            Spell actorSpell = this.GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
            if (actorSpell != null)
            {
                actorSpell.Deactivate();
            }
            Spell spell2 = this.GetActorSpell(SpellType.SPELL_POWER_HINT_IDLE);
            if (spell2 != null)
            {
                spell2.Deactivate();
            }
            if (GameState.Get().IsMulliganManagerActive())
            {
                SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over", base.gameObject);
            }
        }
        if ((this.m_entity.IsControlledByFriendlySidePlayer() && (this.m_entity.IsHero() || (this.m_zone is ZonePlay))) && !this.m_transitioningZones)
        {
            bool flag = this.m_entity.HasSpellPower() || this.m_entity.HasSpellPowerDouble();
            bool flag2 = this.m_entity.HasHeroPowerDamage();
            if (flag || flag2)
            {
                Spell spell3 = this.GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
                if (spell3 != null)
                {
                    spell3.Reactivate();
                }
                if (flag)
                {
                    ZoneMgr.Get().FindZoneOfType<ZoneHand>(this.m_zone.m_Side).OnSpellPowerEntityMousedOver();
                }
            }
        }
        if ((this.m_entity.IsWeapon() && this.m_entity.IsExhausted()) && ((this.m_actor != null) && (this.m_actor.GetAttackObject() != null)))
        {
            this.m_actor.GetAttackObject().Enlarge(1f);
        }
    }

    public void NotifyOpponentMousedOffThisCard()
    {
        this.m_mousedOverByOpponent = false;
        this.UpdateActorState();
    }

    public void NotifyOpponentMousedOverThisCard()
    {
        this.m_mousedOverByOpponent = true;
        this.UpdateActorState();
    }

    public void NotifyOverPlayfield()
    {
        this.m_overPlayfield = true;
        this.UpdateActorState();
    }

    public void NotifyPickedUp()
    {
        this.m_transitioningZones = false;
        this.CutoffFriendlyCardDraw();
    }

    public void NotifyTargetingCanceled()
    {
        SpellStateType activeState;
        if (this.m_entity.IsHeroPower() || this.m_entity.IsSpell())
        {
            Spell spell = this.m_playEffect.GetSpell(false);
            if ((this.m_playEffect != null) && (spell != null))
            {
                activeState = spell.GetActiveState();
                if ((activeState != SpellStateType.NONE) && (activeState != SpellStateType.CANCEL))
                {
                    spell.ActivateState(SpellStateType.CANCEL);
                }
            }
            if (this.m_entity.IsSpell())
            {
                Spell actorSpell = this.GetActorSpell(SpellType.POWER_UP);
                if ((actorSpell != null) && (actorSpell.GetActiveState() != SpellStateType.CANCEL))
                {
                    actorSpell.ActivateState(SpellStateType.CANCEL);
                }
            }
        }
        else if (this.m_entity.IsCharacter() && !this.IsAttacking())
        {
            Spell actorAttackSpellForInput = this.GetActorAttackSpellForInput();
            if (actorAttackSpellForInput != null)
            {
                if (this.m_entity.HasTag(GAME_TAG.IMMUNE_WHILE_ATTACKING) && !this.m_entity.IsImmune())
                {
                    this.GetActor().DeactivateSpell(SpellType.IMMUNE);
                }
                activeState = actorAttackSpellForInput.GetActiveState();
                if ((activeState != SpellStateType.NONE) && (activeState != SpellStateType.CANCEL))
                {
                    actorAttackSpellForInput.ActivateState(SpellStateType.CANCEL);
                }
            }
        }
    }

    private void OnActorChanged(Actor oldActor)
    {
        this.HideTooltip();
        bool flag = false;
        bool flag2 = GameState.Get().IsGameCreating();
        if (oldActor == null)
        {
            bool flag3 = GameState.Get().IsMulliganPhaseNowOrPending();
            if ((this.m_zone is ZoneHand) && GameState.Get().IsBeginPhase())
            {
                bool flag4 = this.m_entity.GetCardId() == "GAME_005";
                if (flag3 && !GameState.Get().HasTheCoinBeenSpawned())
                {
                    if (flag4)
                    {
                        GameState.Get().NotifyOfCoinSpawn();
                        this.m_actor.TurnOffCollider();
                        this.m_actor.Hide();
                        this.m_actorReady = true;
                        flag = true;
                        base.transform.position = Vector3.zero;
                        this.m_doNotWarpToNewZone = true;
                        this.m_doNotSort = true;
                    }
                    else
                    {
                        Player controller = this.m_entity.GetController();
                        if ((controller.IsOpposingSide() && (this == this.m_zone.GetLastCard())) && !controller.HasTag(GAME_TAG.FIRST_PLAYER))
                        {
                            GameState.Get().NotifyOfCoinSpawn();
                            this.m_actor.TurnOffCollider();
                            this.m_actorReady = true;
                            flag = true;
                        }
                    }
                }
                if (!flag4)
                {
                    ZoneMgr.Get().FindZoneOfType<ZoneDeck>(this.m_zone.m_Side).SetCardToInDeckState(this);
                }
            }
            else if (flag2)
            {
                TransformUtil.CopyWorld((Component) base.transform, (Component) this.m_zone.transform);
                if (this.m_zone is ZonePlay)
                {
                    this.ActivateLifetimeEffects();
                }
            }
            else
            {
                if (!this.m_doNotWarpToNewZone)
                {
                    TransformUtil.CopyWorld((Component) base.transform, (Component) this.m_zone.transform);
                }
                if (this.m_zone is ZoneHand)
                {
                    if (!this.m_doNotWarpToNewZone)
                    {
                        ZoneHand hand = (ZoneHand) this.m_zone;
                        base.transform.localScale = hand.GetCardScale(this);
                        base.transform.localEulerAngles = hand.GetCardRotation(this);
                        base.transform.position = hand.GetCardPosition(this);
                    }
                    if (this.m_entity.HasTag(GAME_TAG.LINKEDCARD))
                    {
                        int tag = this.m_entity.GetTag(GAME_TAG.LINKEDCARD);
                        Entity entity = GameState.Get().GetEntity(tag);
                        if ((entity != null) && (entity.GetCard() != null))
                        {
                            this.m_actor.Hide();
                            this.m_doNotSort = true;
                            flag = true;
                        }
                    }
                    else
                    {
                        this.m_actorReady = true;
                        this.m_shown = true;
                        if (!this.m_doNotWarpToNewZone)
                        {
                            this.m_actor.Hide();
                            this.ActivateHandSpawnSpell();
                            flag = true;
                        }
                    }
                }
                if ((this.m_prevZone == null) && (this.m_zone is ZonePlay))
                {
                    if (!this.m_doNotWarpToNewZone)
                    {
                        base.transform.position = ((ZonePlay) this.m_zone).GetCardPosition(this);
                    }
                    if (this.m_entity.HasTag(GAME_TAG.LINKEDCARD))
                    {
                        this.m_transitionStyle = ZoneTransitionStyle.INSTANT;
                        this.ActivateCharacterPlayEffects();
                        this.OnSpellFinished_StandardSpawnMinion(null, null);
                    }
                    else
                    {
                        this.m_actor.Hide();
                        this.ActivateMinionSpawnEffects();
                    }
                    flag = true;
                }
                else if ((!flag3 && ((this.m_zone is ZoneHeroPower) || (this.m_zone is ZoneWeapon))) && this.IsShown())
                {
                    this.ActivateDefaultSpawnSpell(new Spell.FinishedCallback(this.OnSpellFinished_DefaultPlaySpawn));
                    flag = true;
                    this.m_actorReady = true;
                }
            }
        }
        else if ((this.m_prevZone == null) && ((this.m_zone is ZoneHeroPower) || (this.m_zone is ZoneWeapon)))
        {
            oldActor.Destroy();
            TransformUtil.CopyWorld((Component) base.transform, (Component) this.m_zone.transform);
            this.m_transitionStyle = ZoneTransitionStyle.INSTANT;
            this.ActivateDefaultSpawnSpell(new Spell.FinishedCallback(this.OnSpellFinished_DefaultPlaySpawn));
            flag = true;
            this.m_actorReady = true;
        }
        else if ((this.m_prevZone is ZoneHand) && (this.m_zone is ZonePlay))
        {
            if (this.ActivateActorSpells_HandToPlay(oldActor))
            {
                this.ActivateCharacterPlayEffects();
                this.m_actor.Hide();
                flag = true;
                if (((CardTypeBanner.Get() != null) && (CardTypeBanner.Get().GetCardDef() != null)) && (CardTypeBanner.Get().GetCardDef() == this.GetCardDef()))
                {
                    CardTypeBanner.Get().Hide();
                }
            }
        }
        else if ((this.m_prevZone is ZoneHand) && (this.m_zone is ZoneWeapon))
        {
            if (this.ActivateActorSpells_HandToWeapon(oldActor))
            {
                this.m_actor.Hide();
                flag = true;
                if (((CardTypeBanner.Get() != null) && (CardTypeBanner.Get().GetCardDef() != null)) && (CardTypeBanner.Get().GetCardDef() == this.GetCardDef()))
                {
                    CardTypeBanner.Get().Hide();
                }
            }
        }
        else if ((this.m_prevZone is ZonePlay) && (this.m_zone is ZoneHand))
        {
            if (this.DoPlayToHandTransition(oldActor, false))
            {
                flag = true;
            }
        }
        else if (((this.m_prevZone != null) && (((this.m_prevZone is ZonePlay) || (this.m_prevZone is ZoneWeapon)) || (this.m_prevZone is ZoneHeroPower))) && (this.m_zone is ZoneGraveyard))
        {
            if ((this.m_mousedOver && this.m_entity.HasSpellPower()) && (this.m_entity.IsControlledByFriendlySidePlayer() && (this.m_prevZone is ZonePlay)))
            {
                ZoneMgr.Get().FindZoneOfType<ZoneHand>(this.m_prevZone.m_Side).OnSpellPowerEntityMousedOut();
            }
            if (this.m_entity.HasTag(GAME_TAG.DEATHRATTLE_RETURN_ZONE) && this.DoesCardReturnFromGraveyard())
            {
                this.m_prevZone.AddLayoutBlocker();
                TAG_ZONE zoneTag = this.m_entity.GetTag<TAG_ZONE>(GAME_TAG.DEATHRATTLE_RETURN_ZONE);
                Zone zone = ZoneMgr.Get().FindZoneForEntityAndZoneTag(this.m_entity, zoneTag);
                if (zone is ZoneDeck)
                {
                    zone.AddLayoutBlocker();
                }
                this.m_actorWaitingToBeReplaced = oldActor;
                this.m_actor.Hide();
                flag = true;
                this.m_actorReady = true;
            }
            else if (this.HandlePlayActorDeath(oldActor))
            {
                flag = true;
            }
        }
        else if ((this.m_prevZone is ZoneDeck) && (this.m_zone is ZoneHand))
        {
            if (this.m_zone.m_Side == Player.Side.FRIENDLY)
            {
                if (GameState.Get().IsPastBeginPhase())
                {
                    this.m_actorWaitingToBeReplaced = oldActor;
                    if (!TurnStartManager.Get().IsCardDrawHandled(this))
                    {
                        this.DrawFriendlyCard();
                    }
                    flag = true;
                }
                else
                {
                    this.m_actor.TurnOffCollider();
                    this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
                }
            }
            else if (GameState.Get().IsPastBeginPhase())
            {
                if (oldActor != null)
                {
                    oldActor.Destroy();
                }
                this.DrawOpponentCard();
                flag = true;
            }
        }
        else if ((this.m_prevZone is ZoneSecret) && (this.m_zone is ZoneGraveyard))
        {
            flag = true;
            this.m_actorReady = true;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.m_shown = false;
                this.m_actor.Hide();
            }
            else
            {
                this.ShowSecretDeath(oldActor);
            }
        }
        else if ((this.m_prevZone is ZoneGraveyard) && (this.m_zone is ZonePlay))
        {
            this.m_actor.Hide();
            base.StartCoroutine(this.ActivateReviveSpell());
            flag = true;
        }
        else if ((this.m_prevZone is ZoneDeck) && (this.m_zone is ZoneGraveyard))
        {
            this.MillCard();
            flag = true;
        }
        else if ((this.m_prevZone is ZoneDeck) && (this.m_zone is ZonePlay))
        {
            this.m_doNotSort = true;
            if (oldActor != null)
            {
                oldActor.Destroy();
            }
            this.AnimateDeckToPlay();
            flag = true;
        }
        else if ((this.m_prevZone is ZonePlay) && (this.m_zone is ZoneDeck))
        {
            this.m_prevZone.AddLayoutBlocker();
            ZoneMgr.Get().FindZoneOfType<ZoneDeck>(this.m_zone.m_Side).AddLayoutBlocker();
            this.DoPlayToDeckTransition(oldActor);
            flag = true;
        }
        else if ((this.m_prevZone is ZoneGraveyard) && (this.m_zone is ZoneDeck))
        {
            if (this.HandleGraveyardToDeck(oldActor))
            {
                flag = true;
            }
        }
        else if (((this.m_prevZone is ZoneGraveyard) && (this.m_zone is ZoneHand)) && this.HandleGraveyardToHand(oldActor))
        {
            flag = true;
        }
        if (!flag && (oldActor == this.m_actor))
        {
            if (((this.m_prevZone != null) && (this.m_prevZone.m_Side != this.m_zone.m_Side)) && ((this.m_prevZone is ZoneSecret) && (this.m_zone is ZoneSecret)))
            {
                base.StartCoroutine(this.SwitchSecretSides());
                flag = true;
            }
            if (!flag)
            {
                this.m_actorReady = true;
            }
        }
        else
        {
            if (!flag && (this.m_zone is ZoneSecret))
            {
                this.m_shown = true;
                if (oldActor != null)
                {
                    oldActor.Destroy();
                }
                this.m_transitionStyle = ZoneTransitionStyle.INSTANT;
                this.ShowSecretBirth();
                flag = true;
                this.m_actorReady = true;
                if (flag2)
                {
                    this.CreateStartingCardStateEffects();
                }
            }
            if (!flag)
            {
                if (oldActor != null)
                {
                    oldActor.Destroy();
                }
                bool flag7 = ((this.m_zone.m_ServerTag == TAG_ZONE.PLAY) || (this.m_zone.m_ServerTag == TAG_ZONE.SECRET)) || (this.m_zone.m_ServerTag == TAG_ZONE.HAND);
                if (this.IsShown() && flag7)
                {
                    this.CreateStartingCardStateEffects();
                }
                this.m_actorReady = true;
                if (this.IsShown())
                {
                    this.ShowImpl();
                }
                else
                {
                    this.HideImpl();
                }
            }
        }
    }

    private void OnActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("Card.OnActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("Card.OnActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                this.InitActor(actorName, component);
            }
        }
    }

    private void OnCustomDeathSpellLoaded(string name, GameObject go, object callbackData)
    {
        this.m_customDeathSpell = this.SetupCustomSpell(go);
        if (this.m_customDeathSpell == null)
        {
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
            object[] messageArgs = new object[] { name, this };
            Error.AddDevFatal("Card.OnCustomDeathSpellLoaded() - FAILED to load \"{0}\" for card {1}", messageArgs);
        }
        this.FinishSpellLoad();
    }

    private void OnCustomSpawnSpellLoaded(string name, GameObject go, object callbackData)
    {
        this.m_customSpawnSpell = this.SetupCustomSpell(go);
        if (this.m_customSpawnSpell == null)
        {
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
            object[] messageArgs = new object[] { name, this };
            Error.AddDevFatal("Card.OnCustomSpawnSpellLoaded() - FAILED to load \"{0}\" for card {1}", messageArgs);
        }
        this.FinishSpellLoad();
    }

    private void OnCustomSummonSpellLoaded(string name, GameObject go, object callbackData)
    {
        this.m_customSummonSpell = this.SetupCustomSpell(go);
        if (this.m_customSummonSpell == null)
        {
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
            object[] messageArgs = new object[] { name, this };
            Error.AddDevFatal("Card.OnCustomSummonSpellLoaded() - FAILED to load \"{0}\" for card {1}", messageArgs);
        }
        this.FinishSpellLoad();
    }

    public void OnEnchantmentAdded(int oldEnchantmentCount, Entity enchantment)
    {
        Spell actorSpell = null;
        switch (enchantment.GetEnchantmentBirthVisual())
        {
            case TAG_ENCHANTMENT_VISUAL.POSITIVE:
                actorSpell = this.GetActorSpell(SpellType.ENCHANT_POSITIVE);
                break;

            case TAG_ENCHANTMENT_VISUAL.NEGATIVE:
                actorSpell = this.GetActorSpell(SpellType.ENCHANT_NEGATIVE);
                break;

            case TAG_ENCHANTMENT_VISUAL.NEUTRAL:
                actorSpell = this.GetActorSpell(SpellType.ENCHANT_NEUTRAL);
                break;
        }
        if (actorSpell == null)
        {
            this.UpdateEnchantments();
            this.UpdateTooltip();
        }
        else
        {
            if (enchantment.HasTriggerVisual())
            {
                this.ToggleBauble(true, SpellType.TRIGGER);
            }
            actorSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnEnchantmentSpellStateFinished));
            actorSpell.ActivateState(SpellStateType.BIRTH);
        }
    }

    public void OnEnchantmentRemoved(int oldEnchantmentCount, Entity enchantment)
    {
        Spell actorSpell = null;
        switch (enchantment.GetEnchantmentBirthVisual())
        {
            case TAG_ENCHANTMENT_VISUAL.POSITIVE:
                actorSpell = this.GetActorSpell(SpellType.ENCHANT_POSITIVE);
                break;

            case TAG_ENCHANTMENT_VISUAL.NEGATIVE:
                actorSpell = this.GetActorSpell(SpellType.ENCHANT_NEGATIVE);
                break;

            case TAG_ENCHANTMENT_VISUAL.NEUTRAL:
                actorSpell = this.GetActorSpell(SpellType.ENCHANT_NEUTRAL);
                break;
        }
        if (actorSpell == null)
        {
            this.UpdateEnchantments();
            this.UpdateTooltip();
        }
        else
        {
            if (enchantment.HasTriggerVisual() && !this.m_entity.DoEnchantmentsHaveTriggerVisuals())
            {
                this.ToggleBauble(false, SpellType.TRIGGER);
            }
            actorSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnEnchantmentSpellStateFinished));
            actorSpell.ActivateState(SpellStateType.DEATH);
        }
    }

    private void OnEnchantmentSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        if ((prevStateType == SpellStateType.BIRTH) || (prevStateType == SpellStateType.DEATH))
        {
            spell.RemoveStateFinishedCallback(new Spell.StateFinishedCallback(this.OnEnchantmentSpellStateFinished));
            this.UpdateEnchantments();
            this.UpdateTooltip();
        }
    }

    private void OnHandActorForceLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("Card.OnHandActorForceLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("Card.OnHandActorForceLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                if (this.m_actor != null)
                {
                    this.m_actor.Destroy();
                }
                this.m_actor = component;
                this.m_actorName = actorName;
                this.m_actor.SetEntity(this.m_entity);
                this.m_actor.SetCard(this);
                this.m_actor.SetCardDef(this.m_cardDef);
                this.m_actor.UpdateAllComponents();
                this.m_actorLoading = false;
                this.m_actorReady = true;
                if (this.m_shown)
                {
                    this.ShowImpl();
                }
                else
                {
                    this.HideImpl();
                }
                this.RefreshActor();
            }
        }
    }

    public void OnMetaData(Network.HistMetaData metaData)
    {
        if ((((metaData.MetaType == HistoryMeta.Type.DAMAGE) || (metaData.MetaType == HistoryMeta.Type.HEALING)) && this.CanShowActorVisuals()) && (this.m_entity.GetZone() == TAG_ZONE.PLAY))
        {
            Spell actorSpell = this.GetActorSpell(SpellType.DAMAGE);
            if (actorSpell == null)
            {
                this.UpdateActorComponents();
            }
            else
            {
                actorSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnSpellFinished_UpdateActorComponents));
                if (this.m_entity.IsCharacter())
                {
                    int damage = (metaData.MetaType != HistoryMeta.Type.HEALING) ? metaData.Data : -metaData.Data;
                    ((DamageSplatSpell) actorSpell).SetDamage(damage);
                    actorSpell.ActivateState(SpellStateType.ACTION);
                    BoardEvents events = BoardEvents.Get();
                    if (events != null)
                    {
                        if (metaData.MetaType == HistoryMeta.Type.HEALING)
                        {
                            events.HealEvent(this, (float) -metaData.Data);
                        }
                        else
                        {
                            events.DamageEvent(this, (float) metaData.Data);
                        }
                    }
                }
                else
                {
                    actorSpell.Activate();
                }
            }
        }
    }

    private void OnSpellFinished_CustomHandSpawn(Spell spell, object userData)
    {
        this.OnSpellFinished_DefaultHandSpawn(spell, userData);
        this.CleanUpCustomSpell(spell, ref this.m_customSpawnSpellOverride);
    }

    private void OnSpellFinished_CustomSpawnMinion(Spell spell, object userData)
    {
        this.OnSpellFinished_StandardSpawnMinion(spell, userData);
        this.CleanUpCustomSpell(spell, ref this.m_customSpawnSpell);
        this.CleanUpCustomSpell(spell, ref this.m_customSpawnSpellOverride);
        this.ActivateCharacterPlayEffects();
    }

    private void OnSpellFinished_Death(Spell spell, object userData)
    {
        this.m_suppressKeywordDeaths = false;
        this.m_activeDeathEffectCount--;
        GameState.Get().ClearCardBeingDrawn(this);
    }

    private void OnSpellFinished_DefaultHandSpawn(Spell spell, object userData)
    {
        this.m_actor.ToggleForceIdle(false);
        this.m_inputEnabled = true;
        this.CreateStartingCardStateEffects();
        this.RefreshActor();
        this.UpdateActorComponents();
    }

    private void OnSpellFinished_DefaultPlaySpawn(Spell spell, object userData)
    {
        this.m_actor.ToggleForceIdle(false);
        this.m_inputEnabled = true;
        this.CreateStartingCardStateEffects();
        this.RefreshActor();
        this.UpdateActorComponents();
    }

    private void OnSpellFinished_HandToPlay_SummonIn(Spell spell, object userData)
    {
        this.m_actorReady = true;
        this.m_inputEnabled = true;
        this.CreateStartingCardStateEffects();
        this.RefreshActor();
        if ((this.m_entity.HasSpellPower() || this.m_entity.HasSpellPowerDouble()) && this.m_entity.IsControlledByFriendlySidePlayer())
        {
            ZoneMgr.Get().FindZoneOfType<ZoneHand>(this.m_zone.m_Side).OnSpellPowerEntityEnteredPlay();
        }
        if (this.m_entity.HasWindfury())
        {
            this.ActivateActorSpell(SpellType.WINDFURY_BURST);
        }
        base.StartCoroutine(this.ActivateActorBattlecrySpell());
        BoardEvents events = BoardEvents.Get();
        if (events != null)
        {
            events.SummonedEvent(this);
        }
    }

    private void OnSpellFinished_HandToPlay_SummonOut(Spell spell, object userData)
    {
        bool flag;
        this.m_actor.Show();
        Spell bestSummonSpell = this.GetBestSummonSpell(out flag);
        if (!flag)
        {
            bestSummonSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroySpell));
            this.SetCustomSpellParent(bestSummonSpell, this.m_actor);
        }
        bestSummonSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnSpellFinished_HandToPlay_SummonIn));
        bestSummonSpell.Activate();
    }

    private void OnSpellFinished_HandToWeapon_SummonOut(Spell spell, object userData)
    {
        this.m_actor.Show();
        Spell spell2 = (Spell) userData;
        this.ActivateSpell(spell2, new Spell.FinishedCallback(this.OnSpellFinished_StandardCardSummon));
    }

    private void OnSpellFinished_PlayToHand_SummonIn(Spell spell, object userData)
    {
        if (this.m_entity.IsControlledByOpposingSidePlayer())
        {
            object[] args = new object[] { this };
            Log.FaceDownCard.Print("Card.OnSpellFinished_PlayToHand_SummonIn() - {0}", args);
        }
        this.OnSpellFinished_StandardCardSummon(spell, userData);
    }

    private void OnSpellFinished_PlayToHand_SummonOut(Spell spell, object userData)
    {
        Spell spell2 = (Spell) userData;
        this.ActivateSpell(spell2, new Spell.FinishedCallback(this.OnSpellFinished_StandardCardSummon));
    }

    private void OnSpellFinished_PlayToHand_SummonOut_FromGraveyard(Spell spell, object userData)
    {
        this.OnSpellFinished_PlayToHand_SummonOut(spell, userData);
        this.ResumeLayoutForPlayZone();
    }

    private void OnSpellFinished_StandardCardSummon(Spell spell, object userData)
    {
        this.m_actorReady = true;
        this.m_inputEnabled = true;
        this.CreateStartingCardStateEffects();
        this.RefreshActor();
        this.UpdateActorComponents();
    }

    private void OnSpellFinished_StandardSpawnMinion(Spell spell, object userData)
    {
        this.m_actorReady = true;
        this.m_inputEnabled = true;
        this.m_actor.Show();
        this.CreateStartingCardStateEffects();
        this.RefreshActor();
        this.UpdateActorComponents();
        BoardEvents events = BoardEvents.Get();
        if (events != null)
        {
            events.SummonedEvent(this);
        }
    }

    private void OnSpellFinished_UpdateActorComponents(Spell spell, object userData)
    {
        this.UpdateActorComponents();
    }

    private void OnSpellStateFinished_CustomDeath(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            Actor actor = SceneUtils.FindComponentInThisOrParents<Actor>(spell.gameObject);
            if (actor == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("Card.OnSpellStateFinished_CustomDeath() - spell {0} on Card {1} has no Actor ancestor", spell, this));
            }
            else
            {
                actor.Destroy();
            }
        }
    }

    private void OnSpellStateFinished_DestroyActor(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            if (Options.Get().GetBool(Option.USE_EXPERIMENTAL_CODE, true) && (this.m_zone is ZoneGraveyard))
            {
                this.PurgeInactiveSpells();
            }
            Actor actor = SceneUtils.FindComponentInThisOrParents<Actor>(spell.gameObject);
            if (actor == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("Card.OnSpellStateFinished_DestroyActor() - spell {0} on Card {1} has no Actor ancestor", spell, this));
            }
            else
            {
                actor.Destroy();
            }
        }
    }

    private void OnSpellStateFinished_DestroySpell(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            UnityEngine.Object.Destroy(spell.gameObject);
        }
    }

    private void OnSpellStateFinished_PlayToHand_OldActor_SummonOut(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (this.m_entity.IsControlledByOpposingSidePlayer())
        {
            object[] args = new object[] { this, spell.GetActiveState() };
            Log.FaceDownCard.Print("Card.OnSpellStateFinished_PlayToHand_OldActor_SummonOut() - {0} stateType={1}", args);
        }
        this.OnSpellStateFinished_DestroyActor(spell, prevStateType, userData);
    }

    public void OnTagChanged(TagDelta change)
    {
        GAME_TAG tag = (GAME_TAG) change.tag;
        switch (tag)
        {
            case GAME_TAG.DURABILITY:
            case GAME_TAG.HEALTH:
            case GAME_TAG.ATK:
            case GAME_TAG.COST:
                goto Label_076C;

            case GAME_TAG.SILENCED:
                if (this.CanShowActorVisuals())
                {
                    if (change.newValue == 1)
                    {
                        this.m_actor.ActivateSpell(SpellType.SILENCE);
                        this.DeactivateLifetimeEffects();
                    }
                    else
                    {
                        this.m_actor.DeactivateSpell(SpellType.SILENCE);
                        this.ActivateLifetimeEffects();
                    }
                    return;
                }
                return;

            case GAME_TAG.WINDFURY:
                if (this.CanShowActorVisuals())
                {
                    if (change.newValue >= 1)
                    {
                        this.m_actor.ActivateSpell(SpellType.WINDFURY_BURST);
                    }
                    Spell spell = this.m_actor.GetSpell(SpellType.WINDFURY_IDLE);
                    if (spell != null)
                    {
                        if (change.newValue >= 1)
                        {
                            spell.ActivateState(SpellStateType.BIRTH);
                        }
                        else
                        {
                            spell.ActivateState(SpellStateType.CANCEL);
                        }
                    }
                    return;
                }
                return;

            case GAME_TAG.TAUNT:
                break;

            case GAME_TAG.STEALTH:
                goto Label_0539;

            case GAME_TAG.SPELLPOWER:
            case GAME_TAG.HEROPOWER_DAMAGE:
                if (this.CanShowActorVisuals())
                {
                    if (change.newValue > 0)
                    {
                        this.m_actor.ActivateSpell(SpellType.SPELL_POWER);
                    }
                    else
                    {
                        this.m_actor.DeactivateSpell(SpellType.SPELL_POWER);
                    }
                }
                return;

            case GAME_TAG.DIVINE_SHIELD:
                if (this.CanShowActorVisuals())
                {
                    if (((this.m_entity.GetController() == null) || this.m_entity.GetController().IsFriendlySide()) || !this.m_entity.IsObfuscated())
                    {
                        if (change.newValue == 1)
                        {
                            this.m_actor.ActivateSpell(SpellType.DIVINE_SHIELD);
                        }
                        else
                        {
                            this.m_actor.DeactivateSpell(SpellType.DIVINE_SHIELD);
                        }
                    }
                    return;
                }
                return;

            case GAME_TAG.CHARGE:
                if (this.CanShowActorVisuals())
                {
                    Spell actorSpell = this.GetActorSpell(SpellType.Zzz);
                    if (actorSpell != null)
                    {
                        actorSpell.ActivateState(SpellStateType.DEATH);
                    }
                    if (change.newValue == 1)
                    {
                        this.m_actor.ActivateSpell(SpellType.CHARGE);
                    }
                    return;
                }
                return;

            case GAME_TAG.EXHAUSTED:
                if (this.CanShowActorVisuals())
                {
                    if (change.newValue != change.oldValue)
                    {
                        if (GameState.Get().IsTurnStartManagerActive() && this.m_entity.IsControlledByFriendlySidePlayer())
                        {
                            TurnStartManager.Get().NotifyOfExhaustedChange(this, change);
                        }
                        else
                        {
                            this.ShowExhaustedChange(change.newValue);
                        }
                    }
                    return;
                }
                return;

            case GAME_TAG.DAMAGE:
                if (this.CanShowActorVisuals())
                {
                    if (this.m_entity.IsEnraged())
                    {
                        if (!this.m_actor.GetSpell(SpellType.ENRAGE).IsActive())
                        {
                            this.m_actor.ActivateSpell(SpellType.ENRAGE);
                        }
                    }
                    else
                    {
                        this.m_actor.DeactivateSpell(SpellType.ENRAGE);
                    }
                    this.UpdateActorComponents();
                    return;
                }
                return;

            case GAME_TAG.CONTROLLER:
                if (this.CanShowActorVisuals())
                {
                    if (!this.m_entity.HasCharge())
                    {
                        this.m_actor.ActivateSpell(SpellType.Zzz);
                    }
                    return;
                }
                return;

            case GAME_TAG.AURA:
                if (this.CanShowActorVisuals())
                {
                    if (change.newValue == 1)
                    {
                        this.m_actor.ActivateSpell(SpellType.AURA);
                    }
                    else
                    {
                        this.m_actor.DeactivateSpell(SpellType.AURA);
                    }
                    return;
                }
                return;

            case GAME_TAG.POISONOUS:
                if (this.CanShowActorVisuals())
                {
                    this.ToggleBauble(change.newValue == 1, SpellType.POISONOUS);
                    return;
                }
                return;

            case GAME_TAG.AI_MUST_PLAY:
                if (this.CanShowActorVisuals())
                {
                    if (change.newValue == 1)
                    {
                        this.m_actor.ActivateSpell(SpellType.AUTO_CAST);
                    }
                    else
                    {
                        this.m_actor.DeactivateSpell(SpellType.AUTO_CAST);
                    }
                    return;
                }
                return;

            case GAME_TAG.OBFUSCATED:
                if (this.CanShowActorVisuals())
                {
                    if ((this.m_entity.GetController() == null) || !this.m_entity.GetController().IsFriendlySide())
                    {
                        this.UpdateActor();
                    }
                    return;
                }
                return;

            case GAME_TAG.BURNING:
                if (this.CanShowActorVisuals())
                {
                    if ((change.oldValue == 0) && (change.newValue > 0))
                    {
                        this.m_actor.ActivateSpell(SpellType.BURNING);
                    }
                    else if ((change.oldValue > 0) && (change.newValue == 0))
                    {
                        this.m_actor.DeactivateSpell(SpellType.BURNING);
                        if (!this.m_entity.IsSilenced())
                        {
                            this.m_actor.ActivateSpell(SpellType.EXPLODE);
                        }
                    }
                    return;
                }
                return;

            case GAME_TAG.EVIL_GLOW:
                if (this.CanShowActorVisuals())
                {
                    if (change.newValue == 1)
                    {
                        this.m_actor.ActivateSpell(SpellType.EVIL_GLOW);
                    }
                    else
                    {
                        this.m_actor.DeactivateSpell(SpellType.EVIL_GLOW);
                    }
                    return;
                }
                return;

            case GAME_TAG.INSPIRE:
                if (this.CanShowActorVisuals())
                {
                    this.ToggleBauble(change.newValue == 1, SpellType.INSPIRE);
                    return;
                }
                return;

            default:
                switch (tag)
                {
                    case GAME_TAG.ELECTRIC_CHARGE_LEVEL:
                        if (this.CanShowActorVisuals())
                        {
                            if (change.newValue == 1)
                            {
                                this.m_actor.DeactivateSpell(SpellType.ELECTRIC_CHARGE_LEVEL_SMALL);
                                this.m_actor.ActivateSpell(SpellType.ELECTRIC_CHARGE_LEVEL_MEDIUM);
                            }
                            else if (change.newValue == 2)
                            {
                                this.m_actor.DeactivateSpell(SpellType.ELECTRIC_CHARGE_LEVEL_SMALL);
                                this.m_actor.DeactivateSpell(SpellType.ELECTRIC_CHARGE_LEVEL_MEDIUM);
                                this.m_actor.ActivateSpell(SpellType.ELECTRIC_CHARGE_LEVEL_LARGE);
                            }
                            else
                            {
                                this.m_actor.DeactivateSpell(SpellType.ELECTRIC_CHARGE_LEVEL_LARGE);
                            }
                            return;
                        }
                        return;

                    case GAME_TAG.HEAVILY_ARMORED:
                        if (this.CanShowActorVisuals())
                        {
                            if (change.newValue == 1)
                            {
                                this.m_actor.ActivateSpell(SpellType.HEAVILY_ARMORED);
                            }
                            else
                            {
                                this.m_actor.DeactivateSpell(SpellType.HEAVILY_ARMORED);
                            }
                            return;
                        }
                        return;

                    case GAME_TAG.DONT_SHOW_IMMUNE:
                        if (this.CanShowActorVisuals())
                        {
                            if ((change.newValue == 0) && this.m_entity.IsImmune())
                            {
                                this.m_actor.ActivateSpell(SpellType.IMMUNE);
                            }
                            else
                            {
                                this.m_actor.DeactivateSpell(SpellType.IMMUNE);
                            }
                            return;
                        }
                        return;

                    case GAME_TAG.TAUNT_READY:
                        break;

                    case GAME_TAG.STEALTH_READY:
                        goto Label_0539;

                    case GAME_TAG.TRIGGER_VISUAL:
                        if (this.m_entity.IsEnchantment())
                        {
                            Entity entity = GameState.Get().GetEntity(this.m_entity.GetAttached());
                            if (((entity != null) && (entity.GetCard() != null)) && entity.GetCard().CanShowActorVisuals())
                            {
                                entity.GetCard().ToggleBauble(change.newValue == 1, SpellType.TRIGGER);
                            }
                        }
                        if (this.CanShowActorVisuals())
                        {
                            this.ToggleBauble(change.newValue == 1, SpellType.TRIGGER);
                        }
                        return;

                    case GAME_TAG.ENRAGED:
                        if (this.CanShowActorVisuals())
                        {
                            if (change.newValue == 1)
                            {
                                this.m_actor.ActivateSpell(SpellType.ENRAGE);
                            }
                            else
                            {
                                this.m_actor.DeactivateSpell(SpellType.ENRAGE);
                            }
                        }
                        return;

                    case GAME_TAG.DEATHRATTLE:
                        if (this.CanShowActorVisuals())
                        {
                            this.ToggleDeathrattle(change.newValue == 1);
                        }
                        return;

                    case GAME_TAG.CANT_PLAY:
                        if (change.newValue == 1)
                        {
                            this.CancelActiveSpells();
                        }
                        return;

                    case GAME_TAG.CANT_BE_DAMAGED:
                        if (this.CanShowActorVisuals())
                        {
                            if ((change.newValue == 1) && !this.m_entity.DontShowImmune())
                            {
                                this.m_actor.ActivateSpell(SpellType.IMMUNE);
                            }
                            else
                            {
                                this.m_actor.DeactivateSpell(SpellType.IMMUNE);
                            }
                        }
                        return;

                    case GAME_TAG.FROZEN:
                        if (this.CanShowActorVisuals())
                        {
                            if ((change.oldValue == 0) && (change.newValue > 0))
                            {
                                SoundManager.Get().LoadAndPlay("FrostBoltHit1");
                                this.m_actor.ActivateSpell(SpellType.FROZEN);
                            }
                            else if ((change.oldValue > 0) && (change.newValue == 0))
                            {
                                SoundManager.Get().LoadAndPlay("FrostArmorTarget1");
                                this.m_actor.DeactivateSpell(SpellType.FROZEN);
                            }
                        }
                        return;

                    case GAME_TAG.NUM_TURNS_IN_PLAY:
                        if (this.CanShowActorVisuals() && this.m_entity.IsAsleep())
                        {
                            this.m_actor.ActivateSpell(SpellType.Zzz);
                        }
                        return;

                    case GAME_TAG.ARMOR:
                        goto Label_076C;

                    case GAME_TAG.CANT_BE_TARGETED_BY_HERO_POWERS:
                        if (this.CanShowActorVisuals())
                        {
                            if (this.m_entity.IsStealthed())
                            {
                                return;
                            }
                            if (change.newValue == 1)
                            {
                                this.m_actor.ActivateSpell(SpellType.UNTARGETABLE);
                            }
                            else
                            {
                                this.m_actor.DeactivateSpell(SpellType.UNTARGETABLE);
                            }
                        }
                        return;

                    case GAME_TAG.HEALTH_MINIMUM:
                        if (this.CanShowActorVisuals())
                        {
                            if (change.newValue > 0)
                            {
                                this.m_actor.ActivateSpell(SpellType.SHOUT_BUFF);
                            }
                            else
                            {
                                this.m_actor.DeactivateSpell(SpellType.SHOUT_BUFF);
                            }
                        }
                        return;

                    case GAME_TAG.CUSTOM_KEYWORD_EFFECT:
                        if (this.CanShowActorVisuals())
                        {
                            if (change.newValue == 1)
                            {
                                this.ActivateCustomKeywordEffect();
                            }
                            else
                            {
                                this.DeactivateCustomKeywordEffect();
                            }
                        }
                        return;

                    default:
                        return;
                }
                break;
        }
        if (this.CanShowActorVisuals())
        {
            if (change.newValue == 1)
            {
                this.m_actor.ActivateTaunt();
            }
            else
            {
                this.m_actor.DeactivateTaunt();
            }
        }
        return;
    Label_0539:
        if (this.CanShowActorVisuals())
        {
            if (change.newValue == 1)
            {
                this.m_actor.ActivateSpell(SpellType.STEALTH);
                this.m_actor.DeactivateSpell(SpellType.UNTARGETABLE);
            }
            else
            {
                this.m_actor.DeactivateSpell(SpellType.STEALTH);
                if (this.m_entity.HasTag(GAME_TAG.CANT_BE_TARGETED_BY_HERO_POWERS))
                {
                    this.m_actor.ActivateSpell(SpellType.UNTARGETABLE);
                }
            }
            if (this.m_entity.HasTaunt())
            {
                this.m_actor.ActivateTaunt();
            }
        }
        return;
    Label_076C:
        if (!this.CanShowActorVisuals())
        {
            return;
        }
        this.UpdateActorComponents();
    }

    public void OnTagsChanged(TagDeltaSet changeSet)
    {
        bool flag = false;
        for (int i = 0; i < changeSet.Size(); i++)
        {
            TagDelta change = changeSet[i];
            GAME_TAG tag = (GAME_TAG) change.tag;
            switch (tag)
            {
                case GAME_TAG.HEALTH:
                case GAME_TAG.ATK:
                case GAME_TAG.COST:
                    break;

                default:
                    if ((tag != GAME_TAG.DURABILITY) && (tag != GAME_TAG.ARMOR))
                    {
                        goto Label_0053;
                    }
                    break;
            }
            flag = true;
            continue;
        Label_0053:
            this.OnTagChanged(change);
        }
        if ((flag && !this.m_entity.IsLoadingAssets()) && this.IsActorReady())
        {
            this.UpdateActorComponents();
        }
    }

    private void OnZoneChanged()
    {
        if ((this.m_prevZone is ZoneHand) && (this.m_zone is ZoneGraveyard))
        {
            this.DoDiscardAnimation();
        }
        else if (this.m_zone is ZoneGraveyard)
        {
            if (this.m_entity.IsHero())
            {
                this.m_doNotSort = true;
            }
        }
        else if (this.m_zone is ZoneHand)
        {
            if (!this.m_doNotSort)
            {
                this.ShowCard();
            }
            if ((this.m_prevZone is ZoneGraveyard) && this.m_entity.IsSpell())
            {
                this.m_actor.Hide();
                this.ActivateActorSpell(SpellType.SUMMON_IN);
            }
        }
        else if (((this.m_prevZone is ZoneGraveyard) || (this.m_prevZone is ZoneDeck)) && (this.m_zone.m_ServerTag == TAG_ZONE.PLAY))
        {
            this.ShowCard();
        }
    }

    public void OverrideCustomDeathSpell(Spell spell)
    {
        this.m_customDeathSpellOverride = this.SetupOverrideSpell(this.m_customDeathSpellOverride, spell);
    }

    public void OverrideCustomSpawnSpell(Spell spell)
    {
        this.m_customSpawnSpellOverride = this.SetupOverrideSpell(this.m_customSpawnSpellOverride, spell);
    }

    public void OverrideTriggerSpell(int index, Spell spell)
    {
        if (index < 0)
        {
            UnityEngine.Debug.LogError(string.Format("Card.OverrideTriggerSpell() - invalid index value {0} passed!", index));
        }
        else
        {
            for (int i = 0; i <= (index - this.m_triggerEffects.Count); i++)
            {
                this.m_triggerEffects.Add(new CardEffect(null));
            }
            CardEffect effect = this.m_triggerEffects[index];
            Spell spell2 = this.SetupOverrideSpell(effect.GetSpell(false), spell);
            effect.OverrideSpell(spell2);
        }
    }

    public CardSoundSpell PlayEmote(EmoteType emoteType)
    {
        return this.PlayEmote(emoteType, Notification.SpeechBubbleDirection.None);
    }

    public CardSoundSpell PlayEmote(EmoteType emoteType, Notification.SpeechBubbleDirection overrideDirection)
    {
        EmoteEntry emoteEntry = this.GetEmoteEntry(emoteType);
        CardSoundSpell emoteSpell = (emoteEntry != null) ? emoteEntry.GetSpell(true) : null;
        if (!this.m_entity.IsHero())
        {
            return null;
        }
        if (this.m_actor == null)
        {
            return null;
        }
        if (emoteSpell != null)
        {
            emoteSpell.Reactivate();
            if (emoteSpell.IsActive())
            {
                for (int i = 0; i < this.m_emotes.Count; i++)
                {
                    EmoteEntry entry2 = this.m_emotes[i];
                    if (entry2 != emoteEntry)
                    {
                        Spell spell = entry2.GetSpell(false);
                        if (spell != null)
                        {
                            spell.Deactivate();
                        }
                    }
                }
            }
            GameState.Get().GetGameEntity().OnEmotePlayed(this, emoteType, emoteSpell);
        }
        Notification.SpeechBubbleDirection bottomLeft = Notification.SpeechBubbleDirection.BottomLeft;
        if (this.GetEntity().IsControlledByOpposingSidePlayer())
        {
            bottomLeft = Notification.SpeechBubbleDirection.TopRight;
        }
        if (overrideDirection != Notification.SpeechBubbleDirection.None)
        {
            bottomLeft = overrideDirection;
        }
        string speechText = (emoteSpell != null) ? string.Empty : null;
        if ((emoteEntry != null) && !string.IsNullOrEmpty(emoteEntry.GetGameStringKey()))
        {
            speechText = GameStrings.Get(emoteEntry.GetGameStringKey());
        }
        if (speechText != null)
        {
            Notification notification = NotificationManager.Get().CreateSpeechBubble(speechText, bottomLeft, this.m_actor, true, true);
            NotificationManager.Get().DestroyNotification(notification, 1.5f);
        }
        return emoteSpell;
    }

    [DebuggerHidden]
    private IEnumerator PlayHeroPowerAnimation(string animation)
    {
        return new <PlayHeroPowerAnimation>c__Iterator23A { animation = animation, <$>animation = animation, <>f__this = this };
    }

    private void PrepareForDeathAnimation(Actor dyingActor)
    {
        dyingActor.ToggleCollider(false);
        dyingActor.ToggleForceIdle(true);
        dyingActor.SetActorState(ActorStateType.CARD_IDLE);
        dyingActor.DeactivateAllPreDeathSpells();
        this.DeactivateCustomKeywordEffect();
    }

    private SpellType PrioritizedBauble()
    {
        if (this.m_entity.HasTriggerVisual() || this.m_entity.DoEnchantmentsHaveTriggerVisuals())
        {
            return SpellType.TRIGGER;
        }
        if (this.m_entity.IsPoisonous())
        {
            return SpellType.POISONOUS;
        }
        if (this.m_entity.HasInspire())
        {
            return SpellType.INSPIRE;
        }
        return SpellType.NONE;
    }

    private void PurgeInactiveSpells()
    {
        foreach (CardEffect effect in this.m_effects)
        {
            effect.PurgeInactiveSpells();
        }
    }

    public void RefreshActor()
    {
        this.UpdateActorState();
        if (this.m_entity.IsEnchanted())
        {
            this.UpdateEnchantments();
        }
        this.UpdateTooltip();
    }

    private void ResumeLayoutForPlayZone()
    {
        ZonePlay play = ZoneMgr.Get().FindZoneOfType<ZonePlay>(this.m_zone.m_Side);
        play.RemoveLayoutBlocker();
        play.UpdateLayout();
    }

    [DebuggerHidden]
    private IEnumerator RevealDrawnOpponentCard(string handActorName, Actor handActor, ZoneHand handZone)
    {
        return new <RevealDrawnOpponentCard>c__Iterator243 { handActor = handActor, handActorName = handActorName, handZone = handZone, <$>handActor = handActor, <$>handActorName = handActorName, <$>handZone = handZone, <>f__this = this };
    }

    public void SetActor(Actor actor)
    {
        this.m_actor = actor;
    }

    public void SetActorName(string actorName)
    {
        this.m_actorName = actorName;
    }

    public void SetBattleCrySource(bool source)
    {
        this.m_isBattleCrySource = source;
        if (this.m_actor != null)
        {
            if (source)
            {
                SceneUtils.SetLayer(this.m_actor.gameObject, GameLayer.IgnoreFullScreenEffects);
            }
            else
            {
                SceneUtils.SetLayer(this.m_actor.gameObject, GameLayer.Default);
                SceneUtils.SetLayer(this.m_actor.GetMeshRenderer().gameObject, GameLayer.CardRaycast);
            }
        }
    }

    private void SetCustomSpellParent(Spell spell, Component c)
    {
        SpellUtils.SetCustomSpellParent(spell, c);
    }

    public void SetDoNotSort(bool on)
    {
        if (this.m_entity.IsControlledByOpposingSidePlayer())
        {
            object[] args = new object[] { this, on };
            Log.FaceDownCard.Print("Card.SetDoNotSort() - card={0} on={1}", args);
        }
        this.m_doNotSort = on;
    }

    public void SetDoNotWarpToNewZone(bool on)
    {
        this.m_doNotWarpToNewZone = on;
    }

    public void SetEntity(Entity entity)
    {
        this.m_entity = entity;
    }

    public void SetHoldingForLinkedCardSwitch(bool hold)
    {
        this.m_holdingForLinkedCardSwitch = hold;
    }

    public void SetInputEnabled(bool enabled)
    {
        this.m_inputEnabled = enabled;
        this.UpdateActorState();
    }

    public void SetPredictedZonePosition(int pos)
    {
        this.m_predictedZonePosition = pos;
    }

    public void SetSecretTriggered(bool set)
    {
        this.m_secretTriggered = set;
    }

    public void SetShouldShowTooltip()
    {
        if (this.IsAllowedToShowTooltip() && !this.m_shouldShowTooltip)
        {
            this.m_shouldShowTooltip = true;
        }
    }

    public void SetTransitionDelay(float delay)
    {
        this.m_transitionDelay = delay;
    }

    public void SetTransitionStyle(ZoneTransitionStyle style)
    {
        this.m_transitionStyle = style;
    }

    private Spell SetupCustomSpell(GameObject go)
    {
        if (go == null)
        {
            return null;
        }
        Spell component = go.GetComponent<Spell>();
        if (component == null)
        {
            return null;
        }
        this.SetupSpell(component);
        return component;
    }

    private void SetupDeckToPlayActor(Actor actor, GameObject actorObject)
    {
        actor.SetEntity(this.m_entity);
        actor.SetCardDef(this.m_cardDef);
        actor.UpdateAllComponents();
        actorObject.transform.parent = base.transform;
        actorObject.transform.localPosition = Vector3.zero;
        actorObject.transform.localScale = Vector3.one;
        actorObject.transform.localRotation = Quaternion.identity;
    }

    private Spell SetupOverrideSpell(Spell existingSpell, Spell spell)
    {
        if (existingSpell != null)
        {
            UnityEngine.Object.Destroy(existingSpell.gameObject);
        }
        this.SetupSpell(spell);
        return spell;
    }

    private AudioSource SetupSound(AudioSource sound)
    {
        sound.transform.parent = base.transform;
        TransformUtil.Identity(sound.transform);
        return sound;
    }

    private void SetupSoundSpell(Spell spell)
    {
        CardSoundSpell spell2 = spell as CardSoundSpell;
        if (spell2 == null)
        {
            object[] messageArgs = new object[] { spell, this };
            Error.AddDevFatal("Card.SetupSoundSpell() - {0} on card {1} is not a CardSoundSpell or subtype of CardSoundSpell", messageArgs);
        }
        else
        {
            spell.SetSource(base.gameObject);
            spell.transform.parent = base.transform;
            TransformUtil.Identity(spell.transform);
        }
    }

    private void SetupSpell(Spell spell)
    {
        spell.SetSource(base.gameObject);
    }

    public void SetZone(Zone zone)
    {
        this.m_zone = zone;
    }

    public void SetZonePosition(int pos)
    {
        this.m_zonePosition = pos;
    }

    private void SheatheSecret(Spell spell)
    {
        if (!this.m_secretSheathed && this.m_entity.IsExhausted())
        {
            this.m_secretSheathed = true;
            spell.ActivateState(SpellStateType.IDLE);
        }
    }

    private void SheatheWeapon()
    {
        this.m_actor.GetAttackObject().ScaleToZero();
        this.ActivateActorSpell(SpellType.SHEATHE);
    }

    private bool ShouldCardDrawWaitForTaskLists()
    {
        PowerQueue powerQueue = GameState.Get().GetPowerProcessor().GetPowerQueue();
        if (powerQueue.Count == 0)
        {
            return false;
        }
        PowerTaskList taskList = powerQueue.Peek();
        if (this.DoesTaskListBlockCardDraw(taskList))
        {
            return true;
        }
        PowerTaskList parent = taskList.GetParent();
        return this.DoesTaskListBlockCardDraw(parent);
    }

    private bool ShouldCardDrawWaitForTurnStartSpells()
    {
        SpellController spellController = TurnStartManager.Get().GetSpellController();
        if (spellController == null)
        {
            return false;
        }
        return (spellController.IsSource(this) || spellController.IsTarget(this));
    }

    private bool ShouldPreloadCardAssets()
    {
        if (ApplicationMgr.IsPublic())
        {
            return false;
        }
        return Options.Get().GetBool(Option.PRELOAD_CARD_ASSETS, false);
    }

    public void ShowCard()
    {
        if (!this.m_shown)
        {
            this.m_shown = true;
            this.ShowImpl();
        }
    }

    public void ShowExhaustedChange(bool exhausted)
    {
        if (this.m_entity.IsHeroPower())
        {
            base.StopCoroutine("PlayHeroPowerAnimation");
            if (exhausted)
            {
                base.StartCoroutine("PlayHeroPowerAnimation", (UniversalInputManager.UsePhoneUI == null) ? "HeroPower_Used" : "HeroPower_Used_phone");
                SoundManager.Get().LoadAndPlay("hero_power_icon_flip_off");
            }
            else
            {
                base.StartCoroutine("PlayHeroPowerAnimation", (UniversalInputManager.UsePhoneUI == null) ? "HeroPower_Restore" : "HeroPower_Restore_phone");
                SoundManager.Get().LoadAndPlay("hero_power_icon_flip_on");
            }
        }
        else if (this.m_entity.IsWeapon())
        {
            if (exhausted)
            {
                this.SheatheWeapon();
            }
            else
            {
                this.UnSheatheWeapon();
            }
        }
        else if (this.m_entity.IsSecret())
        {
            base.StartCoroutine(this.ShowSecretExhaustedChange(exhausted));
        }
    }

    public void ShowExhaustedChange(int val)
    {
        bool exhausted = val == 1;
        this.ShowExhaustedChange(exhausted);
    }

    private void ShowImpl()
    {
        if (this.m_actor != null)
        {
            this.m_actor.Show();
            this.RefreshActor();
        }
    }

    private void ShowSecretBirth()
    {
        Spell component = this.m_actor.GetComponent<Spell>();
        if (!this.CanShowSecret())
        {
            Spell.StateFinishedCallback callback = delegate (Spell thisSpell, SpellStateType prevStateType, object userData) {
                if ((thisSpell.GetActiveState() == SpellStateType.NONE) && !this.CanShowSecret())
                {
                    this.HideCard();
                }
            };
            component.AddStateFinishedCallback(callback);
        }
        component.ActivateState(SpellStateType.BIRTH);
    }

    public void ShowSecretDeath(Actor oldActor)
    {
        Spell component = oldActor.GetComponent<Spell>();
        if (this.m_secretTriggered)
        {
            this.m_secretTriggered = false;
            if (component.GetActiveState() == SpellStateType.NONE)
            {
                oldActor.Destroy();
            }
            else
            {
                component.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroyActor));
            }
        }
        else
        {
            component.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnSpellStateFinished_DestroyActor));
            component.ActivateState(SpellStateType.ACTION);
            oldActor.transform.parent = null;
            if (UniversalInputManager.UsePhoneUI == null)
            {
                this.m_doNotSort = true;
                iTween.Stop(base.gameObject);
                this.m_actor.Hide();
                base.StartCoroutine(this.WaitAndThenShowDestroyedSecret());
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowSecretExhaustedChange(bool exhausted)
    {
        return new <ShowSecretExhaustedChange>c__Iterator23B { exhausted = exhausted, <$>exhausted = exhausted, <>f__this = this };
    }

    public void ShowSecretTrigger()
    {
        this.m_actor.GetComponent<Spell>().ActivateState(SpellStateType.ACTION);
    }

    public void ShowTooltip()
    {
        if (!this.m_showTooltip)
        {
            this.m_showTooltip = true;
            this.UpdateTooltip();
        }
    }

    public void SuppressActorTriggerSpell(bool suppress)
    {
        this.m_suppressActorTriggerSpell = suppress;
    }

    public void SuppressDeathEffects(bool suppress)
    {
        this.m_suppressDeathEffects = suppress;
    }

    public void SuppressDeathSounds(bool suppress)
    {
        this.m_suppressDeathSounds = suppress;
    }

    public void SuppressKeywordDeaths(bool suppress)
    {
        this.m_suppressKeywordDeaths = suppress;
    }

    public void SuppressPlaySounds(bool suppress)
    {
        this.m_suppressPlaySounds = suppress;
    }

    [DebuggerHidden]
    private IEnumerator SwitchOutFriendlyLinkedDrawnCard(Card oldCard)
    {
        return new <SwitchOutFriendlyLinkedDrawnCard>c__Iterator247 { oldCard = oldCard, <$>oldCard = oldCard, <>f__this = this };
    }

    private bool SwitchOutLinkedDrawnCard()
    {
        if (!(this.m_prevZone is ZoneHand))
        {
            return false;
        }
        int tag = this.m_entity.GetTag(GAME_TAG.LINKEDCARD);
        Entity entity = GameState.Get().GetEntity(tag);
        if (entity == null)
        {
            GameState.Get().ClearCardBeingDrawn(this);
            return false;
        }
        Card card = entity.GetCard();
        if (card == null)
        {
            GameState.Get().ClearCardBeingDrawn(this);
            return false;
        }
        TransformUtil.CopyWorld((Component) card, (Component) this);
        card.m_actorReady = true;
        if (!GameState.Get().IsBeingDrawn(this))
        {
            this.m_doNotSort = false;
            this.DoNullZoneVisuals();
            card.m_actor.Show();
            card.m_doNotSort = false;
            this.m_prevZone.UpdateLayout();
            return true;
        }
        if (this.m_entity.IsControlledByFriendlySidePlayer())
        {
            card.StartCoroutine(card.SwitchOutFriendlyLinkedDrawnCard(this));
        }
        else
        {
            card.StartCoroutine(card.SwitchOutOpponentLinkedDrawnCard(this));
        }
        return true;
    }

    [DebuggerHidden]
    private IEnumerator SwitchOutOpponentLinkedDrawnCard(Card oldCard)
    {
        return new <SwitchOutOpponentLinkedDrawnCard>c__Iterator248 { oldCard = oldCard, <$>oldCard = oldCard, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator SwitchSecretSides()
    {
        return new <SwitchSecretSides>c__Iterator24C { <>f__this = this };
    }

    private void ToggleBauble(bool on, SpellType spellType)
    {
        if (on)
        {
            this.DeactivateBaubles();
            this.m_actor.ActivateSpell(spellType);
        }
        else
        {
            SpellType type = this.PrioritizedBauble();
            if (spellType != type)
            {
                this.m_actor.DeactivateSpell(spellType);
                if (type != SpellType.NONE)
                {
                    this.m_actor.ActivateSpell(type);
                }
            }
        }
    }

    private void ToggleDeathrattle(bool on)
    {
        if (on)
        {
            this.m_actor.ActivateSpell(SpellType.DEATHRATTLE_IDLE);
        }
        else
        {
            this.m_actor.DeactivateSpell(SpellType.DEATHRATTLE_IDLE);
        }
    }

    public override string ToString()
    {
        return ((this.m_entity != null) ? this.m_entity.ToString() : "UNKNOWN CARD");
    }

    public void TransitionToZone(Zone zone)
    {
        if (this.m_zone == zone)
        {
            object[] args = new object[] { this };
            Log.Mike.Print("Card.TransitionToZone() - card={0} already in target zone", args);
        }
        else if (zone == null)
        {
            this.m_zone.RemoveCard(this);
            this.m_prevZone = this.m_zone;
            this.m_zone = null;
            if (!this.SwitchOutLinkedDrawnCard())
            {
                this.DeactivateLifetimeEffects();
                this.DeactivateCustomKeywordEffect();
                this.DoNullZoneVisuals();
            }
        }
        else
        {
            this.m_prevZone = this.m_zone;
            this.m_zone = zone;
            if (this.m_prevZone != null)
            {
                this.m_prevZone.RemoveCard(this);
            }
            this.m_zone.AddCard(this);
            if ((this.m_zone is ZoneGraveyard) && GameState.Get().IsBeingDrawn(this))
            {
                this.m_actorReady = true;
                this.DiscardCardBeingDrawn();
            }
            else if ((this.m_zone is ZoneGraveyard) && this.m_ignoreDeath)
            {
                this.m_actorReady = true;
            }
            else
            {
                this.m_actorReady = false;
                this.LoadActorAndSpells();
            }
        }
    }

    private void UnSheatheSecret(Spell spell)
    {
        if (this.m_secretSheathed && !this.m_entity.IsExhausted())
        {
            this.m_secretSheathed = false;
            spell.ActivateState(SpellStateType.DEATH);
        }
    }

    private void UnSheatheWeapon()
    {
        this.m_actor.GetAttackObject().Enlarge(1f);
        this.ActivateActorSpell(SpellType.UNSHEATHE);
    }

    public void UpdateActor()
    {
        if (this.m_zone != null)
        {
            this.m_actorReady = false;
            this.LoadActorAndSpells();
        }
    }

    public void UpdateActorComponents()
    {
        if (this.m_actor != null)
        {
            this.m_actor.UpdateAllComponents();
        }
    }

    public void UpdateActorState()
    {
        if ((((this.m_actor != null) && this.m_shown) && !this.m_entity.IsBusy()) && !(this.m_zone is ZoneGraveyard))
        {
            if (!this.m_inputEnabled || ((this.m_zone != null) && !this.m_zone.IsInputEnabled()))
            {
                this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
            }
            else if (this.m_overPlayfield)
            {
                this.m_actor.SetActorState(ActorStateType.CARD_OVER_PLAYFIELD);
            }
            else
            {
                GameState state = GameState.Get();
                if ((state != null) && state.IsEntityInputEnabled(this.m_entity))
                {
                    switch (state.GetResponseMode())
                    {
                        case GameState.ResponseMode.OPTION:
                            if (!this.DoOptionHighlight(state))
                            {
                                break;
                            }
                            return;

                        case GameState.ResponseMode.SUB_OPTION:
                            if (!this.DoSubOptionHighlight(state))
                            {
                                break;
                            }
                            return;

                        case GameState.ResponseMode.OPTION_TARGET:
                            if (!this.DoOptionTargetHighlight(state))
                            {
                                break;
                            }
                            return;

                        case GameState.ResponseMode.CHOICE:
                            if (!this.DoChoiceHighlight(state))
                            {
                                break;
                            }
                            return;
                    }
                }
                if (this.m_mousedOver && !(this.m_zone is ZoneHand))
                {
                    this.m_actor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
                }
                else if (this.m_mousedOverByOpponent)
                {
                    this.m_actor.SetActorState(ActorStateType.CARD_OPPONENT_MOUSE_OVER);
                }
                else
                {
                    this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
                }
            }
        }
    }

    public void UpdateEnchantments()
    {
        List<Entity> enchantments = this.m_entity.GetEnchantments();
        Spell actorSpell = this.GetActorSpell(SpellType.ENCHANT_POSITIVE);
        Spell spell2 = this.GetActorSpell(SpellType.ENCHANT_NEGATIVE);
        Spell spell3 = this.GetActorSpell(SpellType.ENCHANT_NEUTRAL);
        Spell spell4 = null;
        if ((actorSpell != null) && (actorSpell.GetActiveState() == SpellStateType.IDLE))
        {
            spell4 = actorSpell;
        }
        else if ((spell2 != null) && (spell2.GetActiveState() == SpellStateType.IDLE))
        {
            spell4 = spell2;
        }
        else if ((spell3 != null) && (spell3.GetActiveState() == SpellStateType.IDLE))
        {
            spell4 = spell3;
        }
        if (enchantments.Count == 0)
        {
            if (spell4 != null)
            {
                spell4.ActivateState(SpellStateType.DEATH);
            }
        }
        else
        {
            int num = 0;
            bool flag = false;
            foreach (Entity entity in enchantments)
            {
                TAG_ENCHANTMENT_VISUAL enchantmentIdleVisual = entity.GetEnchantmentIdleVisual();
                switch (enchantmentIdleVisual)
                {
                    case TAG_ENCHANTMENT_VISUAL.POSITIVE:
                        num++;
                        break;

                    case TAG_ENCHANTMENT_VISUAL.NEGATIVE:
                        num--;
                        break;
                }
                if (enchantmentIdleVisual != TAG_ENCHANTMENT_VISUAL.INVALID)
                {
                    flag = true;
                }
            }
            Spell spell5 = null;
            if (num > 0)
            {
                spell5 = actorSpell;
            }
            else if (num < 0)
            {
                spell5 = spell2;
            }
            else if (flag)
            {
                spell5 = spell3;
            }
            if ((spell4 != null) && (spell4 != spell5))
            {
                spell4.Deactivate();
            }
            if (spell5 != null)
            {
                spell5.ActivateState(SpellStateType.IDLE);
            }
        }
    }

    public void UpdateProposedManaUsage()
    {
        if (GameState.Get().GetSelectedOption() == -1)
        {
            Player player = GameState.Get().GetPlayer(this.GetEntity().GetControllerId());
            if (player.IsFriendlySide() && player.HasTag(GAME_TAG.CURRENT_PLAYER))
            {
                if (this.m_mousedOver)
                {
                    bool flag = this.m_entity.GetZone() == TAG_ZONE.HAND;
                    bool flag2 = this.m_entity.IsHeroPower();
                    if ((flag || flag2) && GameState.Get().IsOption(this.m_entity))
                    {
                        player.ProposeManaCrystalUsage(this.m_entity);
                    }
                }
                else
                {
                    player.CancelAllProposedMana(this.m_entity);
                }
            }
        }
    }

    public void UpdateTooltip()
    {
        if (((this.GetShouldShowTooltip() && this.IsAllowedToShowTooltip()) && this.IsAbleToShowTooltip()) && this.m_showTooltip)
        {
            if (BigCard.Get() != null)
            {
                BigCard.Get().Show(this);
            }
        }
        else
        {
            this.m_showTooltip = false;
            this.m_shouldShowTooltip = false;
            if (BigCard.Get() != null)
            {
                BigCard.Get().Hide(this);
            }
        }
    }

    public void UpdateZoneFromTags()
    {
        this.m_zonePosition = this.m_entity.GetZonePosition();
        Zone zone = ZoneMgr.Get().FindZoneForEntity(this.m_entity);
        this.TransitionToZone(zone);
        if (zone != null)
        {
            zone.UpdateLayout();
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitAndPrepareForDeathAnimation(Actor dyingActor)
    {
        return new <WaitAndPrepareForDeathAnimation>c__Iterator24F { dyingActor = dyingActor, <$>dyingActor = dyingActor, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitAndThenShowDestroyedSecret()
    {
        return new <WaitAndThenShowDestroyedSecret>c__Iterator24B { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForCardDrawBlockingTaskLists()
    {
        return new <WaitForCardDrawBlockingTaskLists>c__Iterator24E { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForCardDrawBlockingTurnStartSpells()
    {
        return new <WaitForCardDrawBlockingTurnStartSpells>c__Iterator24D { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenActivateSoundSpell(CardSoundSpell soundSpell)
    {
        return new <WaitThenActivateSoundSpell>c__Iterator239 { soundSpell = soundSpell, <$>soundSpell = soundSpell };
    }

    public bool WasSecretTriggered()
    {
        return this.m_secretTriggered;
    }

    public bool WillIgnoreDeath()
    {
        return this.m_ignoreDeath;
    }

    public bool WillSuppressActorTriggerSpell()
    {
        return this.m_suppressActorTriggerSpell;
    }

    public bool WillSuppressDeathEffects()
    {
        return this.m_suppressDeathEffects;
    }

    public bool WillSuppressDeathSounds()
    {
        return this.m_suppressDeathSounds;
    }

    public bool WillSuppressKeywordDeaths()
    {
        return this.m_suppressKeywordDeaths;
    }

    public bool WillSuppressPlaySounds()
    {
        return this.m_suppressPlaySounds;
    }

    [CompilerGenerated]
    private sealed class <ActivateActorBattlecrySpell>c__Iterator23E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;
        internal Spell <battlecrySpell>__0;
        internal Spell <playSpell>__1;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<battlecrySpell>__0 = this.<>f__this.GetActorSpell(SpellType.BATTLECRY);
                    if (this.<battlecrySpell>__0 != null)
                    {
                        if ((!(this.<>f__this.m_zone is ZonePlay) || (InputManager.Get() == null)) || (InputManager.Get().GetBattlecrySourceCard() != this.<>f__this))
                        {
                            break;
                        }
                        this.$current = new WaitForSeconds(0.01f);
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    if (InputManager.Get() != null)
                    {
                        if (InputManager.Get().GetBattlecrySourceCard() == this.<>f__this)
                        {
                            if (this.<battlecrySpell>__0.GetActiveState() == SpellStateType.NONE)
                            {
                                this.<battlecrySpell>__0.ActivateState(SpellStateType.BIRTH);
                            }
                            this.<playSpell>__1 = this.<>f__this.GetPlaySpell(true);
                            if (this.<playSpell>__1 != null)
                            {
                                this.<playSpell>__1.ActivateState(SpellStateType.BIRTH);
                            }
                            this.$PC = -1;
                        }
                        break;
                    }
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ActivateCreatorSpawnMinionSpell>c__Iterator23C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>creator;
        internal Card <$>creatorCard;
        internal Card <>f__this;
        internal Spell <creatorSpell>__0;
        internal Entity creator;
        internal Card creatorCard;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (this.creator.IsLoadingAssets() || !this.creatorCard.IsActorReady())
                    {
                        this.$current = 0;
                        this.$PC = 1;
                        goto Label_00B4;
                    }
                    this.<creatorSpell>__0 = this.creatorCard.ActivateCreatorSpawnMinionSpell();
                    if (this.<creatorSpell>__0 != null)
                    {
                        this.$current = new WaitForSeconds(0.9f);
                        this.$PC = 2;
                        goto Label_00B4;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_00B2;
            }
            this.<>f__this.ActivateStandardSpawnMinionSpell();
            this.$PC = -1;
        Label_00B2:
            return false;
        Label_00B4:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ActivateGraveyardActorDeathSpellAfterDelay>c__Iterator250 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_actor.DeactivateAllPreDeathSpells();
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    goto Label_008F;

                case 1:
                    this.<>f__this.ActivateActorSpell(SpellType.DEATH);
                    this.$current = new WaitForSeconds(4f);
                    this.$PC = 2;
                    goto Label_008F;

                case 2:
                    this.<>f__this.m_doNotSort = false;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_008F:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ActivateReviveSpell>c__Iterator23D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (this.<>f__this.m_activeDeathEffectCount > 0)
                    {
                        this.$current = 0;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.ActivateStandardSpawnMinionSpell();
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateDeckToPlay>c__Iterator245 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>cardFaceActor;
        internal Actor <$>hiddenActor;
        internal Spell <$>outSpell;
        internal Card <>f__this;
        internal float <cardFlipTime>__1;
        internal ZonePlay <zonePlay>__0;
        internal Actor cardFaceActor;
        internal Actor hiddenActor;
        internal Spell outSpell;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                {
                    this.cardFaceActor.Hide();
                    this.<>f__this.m_actor.Hide();
                    this.hiddenActor.Hide();
                    this.<>f__this.m_inputEnabled = false;
                    SoundManager.Get().LoadAndPlay("draw_card_into_play", this.<>f__this.gameObject);
                    this.<>f__this.gameObject.transform.localEulerAngles = new Vector3(270f, 90f, 0f);
                    iTween.MoveTo(this.<>f__this.gameObject, this.<>f__this.gameObject.transform.position + Card.ABOVE_DECK_OFFSET, 0.6f);
                    object[] args = new object[] { "rotation", new Vector3(0f, 0f, 0f), "time", 0.7f, "delay", 0.6f, "easetype", iTween.EaseType.easeInOutCubic, "islocal", true };
                    iTween.RotateTo(this.<>f__this.gameObject, iTween.Hash(args));
                    this.hiddenActor.Show();
                    this.$current = new WaitForSeconds(0.4f);
                    this.$PC = 1;
                    goto Label_0532;
                }
                case 1:
                {
                    object[] objArray2 = new object[] { "position", new Vector3(0f, 3f, 0f), "time", 1f, "delay", 0f, "islocal", true };
                    iTween.MoveTo(this.hiddenActor.gameObject, iTween.Hash(objArray2));
                    this.<>f__this.m_doNotSort = false;
                    this.<zonePlay>__0 = (ZonePlay) this.<>f__this.m_zone;
                    this.<zonePlay>__0.SetTransitionTime(1.6f);
                    this.<zonePlay>__0.UpdateLayout();
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 2;
                    goto Label_0532;
                }
                case 2:
                {
                    this.<cardFlipTime>__1 = 0.35f;
                    object[] objArray3 = new object[] { "rotation", new Vector3(0f, 0f, -90f), "time", this.<cardFlipTime>__1, "delay", 0f, "easetype", iTween.EaseType.easeInCubic, "islocal", true };
                    iTween.RotateTo(this.hiddenActor.gameObject, iTween.Hash(objArray3));
                    this.$current = new WaitForSeconds(this.<cardFlipTime>__1);
                    this.$PC = 3;
                    goto Label_0532;
                }
                case 3:
                {
                    this.hiddenActor.Destroy();
                    this.cardFaceActor.Show();
                    this.cardFaceActor.gameObject.transform.localPosition = new Vector3(0f, 3f, 0f);
                    this.cardFaceActor.gameObject.transform.Rotate(new Vector3(0f, 0f, 90f));
                    object[] objArray4 = new object[] { "rotation", new Vector3(0f, 0f, 0f), "time", this.<cardFlipTime>__1, "delay", 0f, "easetype", iTween.EaseType.easeOutCubic, "islocal", true };
                    iTween.RotateTo(this.cardFaceActor.gameObject, iTween.Hash(objArray4));
                    this.<>f__this.m_actor.gameObject.transform.localPosition = new Vector3(0f, 2.86f, 0f);
                    this.cardFaceActor.gameObject.transform.localPosition = new Vector3(0f, 2.86f, 0f);
                    object[] objArray5 = new object[] { "position", Vector3.zero, "time", 1f, "delay", 0f, "islocal", true };
                    iTween.MoveTo(this.hiddenActor.gameObject, iTween.Hash(objArray5));
                    this.<>f__this.ActivateSpell(this.outSpell, new Spell.FinishedCallback(this.<>f__this.OnSpellFinished_HandToPlay_SummonOut), null, new Spell.StateFinishedCallback(this.<>f__this.OnSpellStateFinished_DestroyActor));
                    this.<>f__this.m_actor.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                    this.$PC = -1;
                    break;
                }
            }
            return false;
        Label_0532:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AnimatePlayToDeck>c__Iterator249 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>playActor;
        internal Card <>f__this;
        internal AssetLoader.GameObjectCallback <actorLoadedCallback>__3;
        internal string <actorName>__2;
        internal ZoneDeck <deckZone>__12;
        internal Actor <handActor>__0;
        internal Spell.FinishedCallback <inFinishCallback>__9;
        internal Spell <inSpell>__7;
        internal SpellType <inSpellType>__6;
        internal bool <loadingActor>__1;
        internal Spell.FinishedCallback <outFinishCallback>__11;
        internal Spell <outSpell>__5;
        internal SpellType <outSpellType>__4;
        internal Spell.StateFinishedCallback <outStateFinishCallback>__10;
        internal bool <waitForSpells>__8;
        internal Actor playActor;

        internal void <>m__201(string name, GameObject go, object callbackData)
        {
            this.<loadingActor>__1 = false;
            if (go == null)
            {
                object[] messageArgs = new object[] { name };
                Error.AddDevFatal("Card.AnimatePlayToGraveyardToDeck() - failed to load {0}", messageArgs);
            }
            else
            {
                this.<handActor>__0 = go.GetComponent<Actor>();
                if (this.<handActor>__0 == null)
                {
                    object[] objArray2 = new object[] { name };
                    Error.AddDevFatal("Card.AnimatePlayToGraveyardToDeck() - instance of {0} has no Actor component", objArray2);
                }
            }
        }

        internal void <>m__202(Spell spell, object userData)
        {
            this.<waitForSpells>__8 = false;
        }

        internal void <>m__203(Spell spell, SpellStateType prevStateType, object userData)
        {
            if (spell.GetActiveState() == SpellStateType.NONE)
            {
                this.playActor.Destroy();
            }
        }

        internal void <>m__204(Spell spell, object userData)
        {
            this.<inSpell>__7.Activate();
            this.<>f__this.ResumeLayoutForPlayZone();
        }

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<handActor>__0 = null;
                    this.<loadingActor>__1 = true;
                    this.<actorName>__2 = ActorNames.GetHandActor(this.<>f__this.m_entity);
                    this.<actorLoadedCallback>__3 = new AssetLoader.GameObjectCallback(this.<>m__201);
                    AssetLoader.Get().LoadActor(this.<actorName>__2, this.<actorLoadedCallback>__3, null, false);
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0288;

                case 3:
                    this.<handActor>__0.Destroy();
                    this.<>f__this.m_actorReady = true;
                    this.<>f__this.m_doNotSort = false;
                    this.<deckZone>__12.RemoveLayoutBlocker();
                    this.<deckZone>__12.UpdateLayout();
                    this.$PC = -1;
                    goto Label_0323;

                default:
                    goto Label_0323;
            }
            if (this.<loadingActor>__1)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0325;
            }
            if (this.<handActor>__0 == null)
            {
                this.playActor.Destroy();
                goto Label_0323;
            }
            this.<handActor>__0.SetEntity(this.<>f__this.m_entity);
            this.<handActor>__0.SetCardDef(this.<>f__this.m_cardDef);
            this.<handActor>__0.UpdateAllComponents();
            this.<handActor>__0.transform.parent = this.playActor.transform.parent;
            TransformUtil.Identity(this.<handActor>__0);
            this.<handActor>__0.Hide();
            this.<outSpellType>__4 = SpellType.SUMMON_OUT;
            this.<outSpell>__5 = this.playActor.GetSpell(this.<outSpellType>__4);
            if (this.<outSpell>__5 == null)
            {
                object[] messageArgs = new object[] { this.<>f__this, this.<outSpellType>__4 };
                Error.AddDevFatal("{0}.AnimatePlayToGraveyardToDeck() - outSpell=null outSpellType={1}", messageArgs);
                goto Label_0323;
            }
            this.<inSpellType>__6 = SpellType.SUMMON_IN;
            this.<inSpell>__7 = this.<handActor>__0.GetSpell(this.<inSpellType>__6);
            if (this.<inSpell>__7 == null)
            {
                object[] objArray2 = new object[] { this.<>f__this, this.<inSpellType>__6 };
                Error.AddDevFatal("{0}.AnimatePlayToGraveyardToDeck() - inSpell=null inSpellType={1}", objArray2);
                goto Label_0323;
            }
            this.<waitForSpells>__8 = true;
            this.<inFinishCallback>__9 = new Spell.FinishedCallback(this.<>m__202);
            this.<outStateFinishCallback>__10 = new Spell.StateFinishedCallback(this.<>m__203);
            this.<outFinishCallback>__11 = new Spell.FinishedCallback(this.<>m__204);
            this.<inSpell>__7.AddFinishedCallback(this.<inFinishCallback>__9);
            this.<outSpell>__5.AddFinishedCallback(this.<outFinishCallback>__11);
            this.<outSpell>__5.AddStateFinishedCallback(this.<outStateFinishCallback>__10);
            this.<>f__this.PrepareForDeathAnimation(this.playActor);
            this.<outSpell>__5.Activate();
        Label_0288:
            while (this.<waitForSpells>__8)
            {
                this.$current = 0;
                this.$PC = 2;
                goto Label_0325;
            }
            this.<deckZone>__12 = (ZoneDeck) this.<>f__this.m_zone;
            this.$current = this.<>f__this.StartCoroutine(this.<>f__this.AnimatePlayToDeck(this.<>f__this.gameObject, this.<deckZone>__12, false));
            this.$PC = 3;
            goto Label_0325;
        Label_0323:
            return false;
        Label_0325:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AnimatePlayToDeck>c__Iterator24A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ZoneDeck <$>deckZone;
        internal bool <$>hideBackSide;
        internal GameObject <$>mover;
        internal Vector3 <finalPos>__1;
        internal Vector3 <finalRot>__4;
        internal float <finalRotSec>__6;
        internal Vector3 <finalScale>__5;
        internal Vector3 <intermedPos>__2;
        internal Vector3 <intermedRot>__3;
        internal GameObject <targetThickness>__0;
        internal ZoneDeck deckZone;
        internal bool hideBackSide;
        internal GameObject mover;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                {
                    SoundManager.Get().LoadAndPlay("MinionToDeck_transition");
                    this.<targetThickness>__0 = this.deckZone.GetThicknessForLayout();
                    this.<finalPos>__1 = this.<targetThickness>__0.GetComponent<Renderer>().bounds.center + Card.IN_DECK_OFFSET;
                    this.<intermedPos>__2 = this.<finalPos>__1 + Card.ABOVE_DECK_OFFSET;
                    this.<intermedRot>__3 = new Vector3(0f, Card.IN_DECK_ANGLES.y, 0f);
                    this.<finalRot>__4 = Card.IN_DECK_ANGLES;
                    this.<finalScale>__5 = Card.IN_DECK_SCALE;
                    this.<finalRotSec>__6 = 0.3f;
                    if (this.hideBackSide)
                    {
                        this.<intermedRot>__3.y = this.<finalRot>__4.y = -Card.IN_DECK_ANGLES.y;
                        this.<finalRotSec>__6 = 0.5f;
                    }
                    object[] args = new object[] { "position", this.<intermedPos>__2, "delay", 0f, "time", 0.7f, "easetype", iTween.EaseType.easeInOutCubic };
                    iTween.MoveTo(this.mover, iTween.Hash(args));
                    object[] objArray2 = new object[] { "rotation", this.<intermedRot>__3, "delay", 0f, "time", 0.2f, "easetype", iTween.EaseType.easeInOutCubic };
                    iTween.RotateTo(this.mover, iTween.Hash(objArray2));
                    object[] objArray3 = new object[] { "position", this.<finalPos>__1, "delay", 0.7f, "time", 0.7f, "easetype", iTween.EaseType.easeOutCubic };
                    iTween.MoveTo(this.mover, iTween.Hash(objArray3));
                    object[] objArray4 = new object[] { "scale", this.<finalScale>__5, "delay", 0.7f, "time", 0.6f, "easetype", iTween.EaseType.easeInCubic };
                    iTween.ScaleTo(this.mover, iTween.Hash(objArray4));
                    object[] objArray5 = new object[] { "rotation", this.<finalRot>__4, "delay", 0.2f, "time", this.<finalRotSec>__6, "easetype", iTween.EaseType.easeOutCubic };
                    iTween.RotateTo(this.mover, iTween.Hash(objArray5));
                    break;
                }
                case 1:
                    break;

                default:
                    goto Label_0339;
            }
            while (iTween.HasTween(this.mover))
            {
                this.$current = 0;
                this.$PC = 1;
                return true;
            }
            this.$PC = -1;
        Label_0339:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <DrawFriendlyCardWithTiming>c__Iterator23F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;
        internal Transform <bone>__2;
        internal CardBackDisplay <cbDisplay>__1;
        internal Vector3[] <drawPath>__3;
        internal Actor <standIn>__0;
        internal ZoneHand <zoneHand>__4;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_doNotSort = true;
                    this.<>f__this.m_transitionStyle = ZoneTransitionStyle.SLOW;
                    this.<>f__this.m_cardStandInInteractive = false;
                    this.<>f__this.m_actor.Hide();
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_03CD;

                case 3:
                case 4:
                case 5:
                    goto Label_0492;

                case 6:
                    this.<>f__this.m_cardStandInInteractive = true;
                    this.<zoneHand>__4.MakeStandInInteractive(this.<>f__this);
                    goto Label_0553;

                default:
                    goto Label_055A;
            }
            if (GameState.Get().GetFriendlyCardBeingDrawn() != null)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_055C;
            }
            GameState.Get().SetFriendlyCardBeingDrawn(this.<>f__this);
            this.<standIn>__0 = Gameplay.Get().GetCardDrawStandIn();
            this.<standIn>__0.transform.parent = this.<>f__this.m_actor.transform.parent;
            this.<standIn>__0.transform.localPosition = Vector3.zero;
            this.<standIn>__0.transform.localScale = Vector3.one;
            this.<standIn>__0.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            this.<standIn>__0.Show();
            this.<cbDisplay>__1 = this.<standIn>__0.GetRootObject().GetComponentInChildren<CardBackDisplay>();
            this.<cbDisplay>__1.SetCardBack(true);
            if (this.<>f__this.m_actorWaitingToBeReplaced != null)
            {
                this.<>f__this.m_actorWaitingToBeReplaced.Destroy();
                this.<>f__this.m_actorWaitingToBeReplaced = null;
            }
            this.<bone>__2 = Board.Get().FindBone("FriendlyDrawCard");
            this.<drawPath>__3 = new Vector3[] { this.<>f__this.gameObject.transform.position, this.<>f__this.gameObject.transform.position + Card.ABOVE_DECK_OFFSET, this.<bone>__2.position };
            object[] args = new object[] { "path", this.<drawPath>__3, "time", 1.5f, "easetype", iTween.EaseType.easeInSineOutExpo };
            iTween.MoveTo(this.<>f__this.gameObject, iTween.Hash(args));
            this.<>f__this.gameObject.transform.localEulerAngles = new Vector3(270f, 270f, 0f);
            object[] objArray2 = new object[] { "rotation", new Vector3(0f, 0f, 357f), "time", 1.35f, "delay", 0.15f };
            iTween.RotateTo(this.<>f__this.gameObject, iTween.Hash(objArray2));
            object[] objArray3 = new object[] { "scale", this.<bone>__2.localScale, "time", 0.75f, "delay", 0.15f };
            iTween.ScaleTo(this.<>f__this.gameObject, iTween.Hash(objArray3));
            SoundManager.Get().LoadAndPlay("draw_card_1", this.<>f__this.gameObject);
            this.<standIn>__0.transform.parent = null;
            this.<standIn>__0.Hide();
            this.<>f__this.m_actor.Show();
            this.<>f__this.m_actor.TurnOffCollider();
        Label_03CD:
            while (iTween.Count(this.<>f__this.gameObject) > 0)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_055C;
            }
            this.<>f__this.m_actorReady = true;
            this.<zoneHand>__4 = (ZoneHand) this.<>f__this.m_zone;
            if (this.<>f__this.ShouldCardDrawWaitForTurnStartSpells())
            {
                this.$current = this.<>f__this.StartCoroutine(this.<>f__this.WaitForCardDrawBlockingTurnStartSpells());
                this.$PC = 3;
                goto Label_055C;
            }
            if (this.<>f__this.ShouldCardDrawWaitForTaskLists())
            {
                this.$current = this.<>f__this.StartCoroutine(this.<>f__this.WaitForCardDrawBlockingTaskLists());
                this.$PC = 4;
                goto Label_055C;
            }
        Label_0492:
            while (this.<>f__this.m_holdingForLinkedCardSwitch)
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_055C;
            }
            this.<>f__this.m_doNotSort = false;
            GameState.Get().ClearCardBeingDrawn(this.<>f__this);
            if (this.<>f__this.m_zone == this.<zoneHand>__4)
            {
                SoundManager.Get().LoadAndPlay("add_card_to_hand_1", this.<>f__this.gameObject);
                this.<>f__this.CreateStartingCardStateEffects();
                this.<>f__this.RefreshActor();
                this.<>f__this.m_zone.UpdateLayout();
                this.$current = new WaitForSeconds(0.3f);
                this.$PC = 6;
                goto Label_055C;
            }
        Label_0553:
            this.$PC = -1;
        Label_055A:
            return false;
        Label_055C:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <DrawKnownOpponentCard>c__Iterator242 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ZoneHand <$>handZone;
        internal Card <>f__this;
        internal AssetLoader.GameObjectCallback <actorLoadedCallback>__2;
        internal string <actorName>__3;
        internal Actor <handActor>__0;
        internal bool <loadingActor>__1;
        internal ZoneHand handZone;

        internal void <>m__1FE(string name, GameObject go, object callbackData)
        {
            this.<loadingActor>__1 = false;
            if (go == null)
            {
                object[] messageArgs = new object[] { name };
                Error.AddDevFatal("Card.DrawKnownOpponentCard() - failed to load {0}", messageArgs);
            }
            else
            {
                this.<handActor>__0 = go.GetComponent<Actor>();
                if (this.<handActor>__0 == null)
                {
                    object[] objArray2 = new object[] { name };
                    Error.AddDevFatal("Card.DrawKnownOpponentCard() - instance of {0} has no Actor component", objArray2);
                }
            }
        }

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<handActor>__0 = null;
                    this.<loadingActor>__1 = true;
                    this.<actorLoadedCallback>__2 = new AssetLoader.GameObjectCallback(this.<>m__1FE);
                    this.<actorName>__3 = ActorNames.GetHandActor(this.<>f__this.m_entity);
                    AssetLoader.Get().LoadActor(this.<actorName>__3, this.<actorLoadedCallback>__2, null, false);
                    break;

                case 1:
                    break;

                default:
                    goto Label_012C;
            }
            if (this.<loadingActor>__1)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (this.<handActor>__0 != null)
            {
                this.<handActor>__0.SetEntity(this.<>f__this.m_entity);
                this.<handActor>__0.SetCardDef(this.<>f__this.m_cardDef);
                this.<handActor>__0.UpdateAllComponents();
                this.<>f__this.StartCoroutine(this.<>f__this.RevealDrawnOpponentCard(this.<actorName>__3, this.<handActor>__0, this.handZone));
            }
            else
            {
                this.<>f__this.StartCoroutine(this.<>f__this.DrawUnknownOpponentCard(this.handZone));
            }
            this.$PC = -1;
        Label_012C:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <DrawOpponentCardWithTiming>c__Iterator240 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;
        internal ZoneHand <handZone>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_doNotSort = true;
                    this.<>f__this.m_beingDrawnByOpponent = true;
                    break;

                case 1:
                    break;

                default:
                    goto Label_0110;
            }
            if (GameState.Get().GetOpponentCardBeingDrawn() != null)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            GameState.Get().SetOpponentCardBeingDrawn(this.<>f__this);
            this.<handZone>__0 = (ZoneHand) this.<>f__this.m_zone;
            this.<handZone>__0.UpdateLayout();
            if (this.<>f__this.m_entity.HasTag(GAME_TAG.REVEALED) || this.<>f__this.m_entity.HasTag(GAME_TAG.TOPDECK))
            {
                this.<>f__this.StartCoroutine(this.<>f__this.DrawKnownOpponentCard(this.<handZone>__0));
            }
            else
            {
                this.<>f__this.StartCoroutine(this.<>f__this.DrawUnknownOpponentCard(this.<handZone>__0));
            }
            this.$PC = -1;
        Label_0110:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <DrawUnknownOpponentCard>c__Iterator241 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ZoneHand <$>handZone;
        internal Card <>f__this;
        internal Transform <bone>__0;
        internal Vector3[] <drawPath>__1;
        internal ZoneHand handZone;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                {
                    SoundManager.Get().LoadAndPlay("draw_card_and_add_to_hand_opp_1", this.<>f__this.gameObject);
                    this.<>f__this.gameObject.transform.rotation = Card.IN_DECK_HIDDEN_ROTATION;
                    this.<bone>__0 = Board.Get().FindBone("OpponentDrawCard");
                    this.<drawPath>__1 = new Vector3[] { this.<>f__this.gameObject.transform.position, this.<>f__this.gameObject.transform.position + Card.ABOVE_DECK_OFFSET, this.<bone>__0.position, this.handZone.GetCardPosition(this.<>f__this) };
                    object[] args = new object[] { "path", this.<drawPath>__1, "time", 1.75f, "easetype", iTween.EaseType.easeInOutQuart };
                    iTween.MoveTo(this.<>f__this.gameObject, iTween.Hash(args));
                    object[] objArray2 = new object[] { "rotation", this.handZone.GetCardRotation(this.<>f__this), "time", 0.7f, "delay", 0.8f, "easetype", iTween.EaseType.easeInOutCubic };
                    iTween.RotateTo(this.<>f__this.gameObject, iTween.Hash(objArray2));
                    object[] objArray3 = new object[] { "scale", this.handZone.GetCardScale(this.<>f__this), "time", 0.7f, "delay", 0.8f, "easetype", iTween.EaseType.easeInOutQuint };
                    iTween.ScaleTo(this.<>f__this.gameObject, iTween.Hash(objArray3));
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 1;
                    goto Label_02FC;
                }
                case 1:
                    this.<>f__this.m_actorReady = true;
                    this.$current = new WaitForSeconds(0.6f);
                    this.$PC = 2;
                    goto Label_02FC;

                case 2:
                case 3:
                    if (iTween.Count(this.<>f__this.gameObject) > 0)
                    {
                        this.$current = null;
                        this.$PC = 3;
                        goto Label_02FC;
                    }
                    this.<>f__this.m_doNotSort = false;
                    this.<>f__this.m_beingDrawnByOpponent = false;
                    GameState.Get().SetOpponentCardBeingDrawn(null);
                    this.handZone.UpdateLayout();
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_02FC:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <HideRevealedOpponentCard>c__Iterator244 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>handActor;
        internal Card <>f__this;
        internal Action<object> <changeActorsCallback>__3;
        internal float <flipSec>__0;
        internal float <handActorRotation>__2;
        internal float <revealSec>__1;
        internal Actor handActor;

        internal void <>m__1FF(object obj)
        {
            UnityEngine.Object.Destroy(this.handActor.gameObject);
            this.<>f__this.m_actor.Show();
        }

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                {
                    this.<flipSec>__0 = 0.5f;
                    this.<revealSec>__1 = 0.525f * this.<flipSec>__0;
                    if (!this.<>f__this.GetController().IsRevealed())
                    {
                        this.<handActorRotation>__2 = 180f;
                        TransformUtil.SetEulerAngleZ(this.<>f__this.m_actor.gameObject, -this.<handActorRotation>__2);
                        object[] objArray1 = new object[] { "z", this.<handActorRotation>__2, "time", this.<flipSec>__0, "easetype", iTween.EaseType.easeInOutCubic };
                        iTween.RotateAdd(this.handActor.gameObject, iTween.Hash(objArray1));
                        object[] objArray2 = new object[] { "z", this.<handActorRotation>__2, "time", this.<flipSec>__0, "easetype", iTween.EaseType.easeInOutCubic };
                        iTween.RotateAdd(this.<>f__this.m_actor.gameObject, iTween.Hash(objArray2));
                    }
                    this.<changeActorsCallback>__3 = new Action<object>(this.<>m__1FF);
                    object[] args = new object[] { "time", this.<revealSec>__1, "oncomplete", this.<changeActorsCallback>__3 };
                    iTween.Timer(this.<>f__this.m_actor.gameObject, iTween.Hash(args));
                    this.$current = new WaitForSeconds(this.<flipSec>__0);
                    this.$PC = 1;
                    return true;
                }
                case 1:
                    this.<>f__this.m_doNotSort = false;
                    this.<>f__this.m_beingDrawnByOpponent = false;
                    GameState.Get().SetOpponentCardBeingDrawn(null);
                    SoundManager.Get().LoadAndPlay("add_card_to_hand_1", this.<>f__this.gameObject);
                    this.<>f__this.CreateStartingCardStateEffects();
                    this.<>f__this.RefreshActor();
                    this.<>f__this.m_zone.UpdateLayout();
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <MillCardWithTiming>c__Iterator246 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;
        internal Transform <bone>__4;
        internal string <boneName>__2;
        internal Player <cardOwner>__0;
        internal Vector3[] <drawPath>__5;
        internal bool <friendly>__1;
        internal Spell <handfullSpell>__6;
        internal int <turn>__3;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<cardOwner>__0 = this.<>f__this.m_entity.GetController();
                    this.<friendly>__1 = this.<cardOwner>__0.IsFriendlySide();
                    if (!this.<friendly>__1)
                    {
                        goto Label_00BF;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_00BF;

                case 3:
                    goto Label_035E;

                default:
                    goto Label_03E2;
            }
            if (GameState.Get().GetFriendlyCardBeingDrawn() != null)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_03E4;
            }
            GameState.Get().SetFriendlyCardBeingDrawn(this.<>f__this);
            this.<boneName>__2 = "FriendlyMillCard";
            goto Label_00EE;
        Label_00BF:
            while (GameState.Get().GetOpponentCardBeingDrawn() != null)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_03E4;
            }
            GameState.Get().SetOpponentCardBeingDrawn(this.<>f__this);
            this.<boneName>__2 = "OpponentMillCard";
        Label_00EE:
            this.<turn>__3 = GameState.Get().GetTurn();
            if ((this.<turn>__3 != GameState.Get().GetLastTurnRemindedOfFullHand()) && (this.<cardOwner>__0.GetHandZone().GetCardCount() >= 10))
            {
                GameState.Get().SetLastTurnRemindedOfFullHand(this.<turn>__3);
                this.<cardOwner>__0.GetHeroCard().PlayEmote(EmoteType.ERROR_HAND_FULL);
            }
            this.<>f__this.m_actor.Show();
            this.<>f__this.m_actor.TurnOffCollider();
            this.<bone>__4 = Board.Get().FindBone(this.<boneName>__2);
            this.<drawPath>__5 = new Vector3[] { this.<>f__this.gameObject.transform.position, this.<>f__this.gameObject.transform.position + Card.ABOVE_DECK_OFFSET, this.<bone>__4.position };
            object[] args = new object[] { "path", this.<drawPath>__5, "time", 1.5f, "easetype", iTween.EaseType.easeInSineOutExpo };
            iTween.MoveTo(this.<>f__this.gameObject, iTween.Hash(args));
            this.<>f__this.gameObject.transform.localEulerAngles = new Vector3(270f, 270f, 0f);
            object[] objArray2 = new object[] { "rotation", new Vector3(0f, 0f, 357f), "time", 1.35f, "delay", 0.15f };
            iTween.RotateTo(this.<>f__this.gameObject, iTween.Hash(objArray2));
            object[] objArray3 = new object[] { "scale", this.<bone>__4.localScale, "time", 0.75f, "delay", 0.15f };
            iTween.ScaleTo(this.<>f__this.gameObject, iTween.Hash(objArray3));
        Label_035E:
            while (iTween.Count(this.<>f__this.gameObject) > 0)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_03E4;
            }
            this.<>f__this.m_actorReady = true;
            this.<>f__this.RefreshActor();
            this.<handfullSpell>__6 = this.<>f__this.m_actor.GetSpell(SpellType.HANDFULL);
            this.<handfullSpell>__6.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.<>f__this.OnSpellStateFinished_DestroyActor));
            this.<handfullSpell>__6.Activate();
            GameState.Get().ClearCardBeingDrawn(this.<>f__this);
            this.$PC = -1;
        Label_03E2:
            return false;
        Label_03E4:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <PlayHeroPowerAnimation>c__Iterator23A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>animation;
        internal Card <>f__this;
        internal MinionShake <shake>__0;
        internal Spell <spell>__1;
        internal string animation;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.SetInputEnabled(false);
                    this.<shake>__0 = this.<>f__this.m_actor.gameObject.GetComponentInChildren<MinionShake>();
                    if (this.<shake>__0 != null)
                    {
                        break;
                    }
                    goto Label_013F;

                case 1:
                    break;

                case 2:
                    goto Label_00E1;

                default:
                    goto Label_013F;
            }
            while (this.<shake>__0.isShaking())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0141;
            }
            this.<>f__this.m_actor.GetComponent<Animation>().Play(this.animation);
            this.<spell>__1 = this.<>f__this.GetPlaySpell(true);
            if (this.<spell>__1 == null)
            {
                goto Label_00F1;
            }
        Label_00E1:
            while (this.<spell>__1.GetActiveState() != SpellStateType.NONE)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0141;
            }
        Label_00F1:
            this.<>f__this.SetInputEnabled(true);
            if (this.animation.Contains("Used") && GameState.Get().IsOption(this.<>f__this.m_entity))
            {
                this.<>f__this.SetInputEnabled(false);
            }
            this.$PC = -1;
        Label_013F:
            return false;
        Label_0141:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <RevealDrawnOpponentCard>c__Iterator243 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>handActor;
        internal string <$>handActorName;
        internal ZoneHand <$>handZone;
        internal Card <>f__this;
        internal string <actorName>__3;
        internal Transform <bone>__1;
        internal string <boneName>__0;
        internal Vector3[] <drawPath>__2;
        internal Actor handActor;
        internal string handActorName;
        internal ZoneHand handZone;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                {
                    SoundManager.Get().LoadAndPlay("draw_card_1", this.<>f__this.gameObject);
                    this.handActor.transform.parent = this.<>f__this.m_actor.transform.parent;
                    TransformUtil.CopyLocal((Component) this.handActor, (Component) this.<>f__this.m_actor);
                    this.<>f__this.m_actor.Hide();
                    this.<>f__this.gameObject.transform.localEulerAngles = new Vector3(270f, 90f, 0f);
                    this.<boneName>__0 = "OpponentDrawCardAndReveal";
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<boneName>__0 = this.<boneName>__0 + "_phone";
                    }
                    this.<bone>__1 = Board.Get().FindBone(this.<boneName>__0);
                    this.<drawPath>__2 = new Vector3[] { this.<>f__this.gameObject.transform.position, this.<>f__this.gameObject.transform.position + Card.ABOVE_DECK_OFFSET, this.<bone>__1.position };
                    object[] args = new object[] { "path", this.<drawPath>__2, "time", 1.75f, "easetype", iTween.EaseType.easeInOutQuart };
                    iTween.MoveTo(this.<>f__this.gameObject, iTween.Hash(args));
                    object[] objArray2 = new object[] { "rotation", this.<bone>__1.eulerAngles, "time", 0.7f, "delay", 0.8f, "easetype", iTween.EaseType.easeInOutCubic };
                    iTween.RotateTo(this.<>f__this.gameObject, iTween.Hash(objArray2));
                    object[] objArray3 = new object[] { "scale", this.<bone>__1.localScale, "time", 0.7f, "delay", 0.8f, "easetype", iTween.EaseType.easeInOutQuint };
                    iTween.ScaleTo(this.<>f__this.gameObject, iTween.Hash(objArray3));
                    this.$current = new WaitForSeconds(1.75f);
                    this.$PC = 1;
                    goto Label_0444;
                }
                case 1:
                    this.<>f__this.m_actorReady = true;
                    this.<>f__this.m_beingDrawnByOpponent = false;
                    this.<actorName>__3 = this.<>f__this.m_actorName;
                    this.<>f__this.m_actorWaitingToBeReplaced = this.<>f__this.m_actor;
                    this.<>f__this.m_actorName = this.handActorName;
                    this.<>f__this.m_actor = this.handActor;
                    if (!this.<>f__this.ShouldCardDrawWaitForTaskLists())
                    {
                        break;
                    }
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.WaitForCardDrawBlockingTaskLists());
                    this.$PC = 2;
                    goto Label_0444;

                case 2:
                case 3:
                    break;

                case 4:
                    this.$PC = -1;
                    goto Label_0442;

                default:
                    goto Label_0442;
            }
            if (this.<>f__this.m_holdingForLinkedCardSwitch)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_0444;
            }
            if (this.<>f__this.m_zone != this.handZone)
            {
                this.<>f__this.m_doNotSort = false;
                GameState.Get().ClearCardBeingDrawn(this.<>f__this);
            }
            else
            {
                this.<>f__this.m_actor = this.<>f__this.m_actorWaitingToBeReplaced;
                this.<>f__this.m_actorName = this.<actorName>__3;
                this.<>f__this.m_actorWaitingToBeReplaced = null;
                this.<>f__this.m_beingDrawnByOpponent = true;
                this.$current = this.<>f__this.StartCoroutine(this.<>f__this.HideRevealedOpponentCard(this.handActor));
                this.$PC = 4;
                goto Label_0444;
            }
        Label_0442:
            return false;
        Label_0444:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ShowSecretExhaustedChange>c__Iterator23B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>exhausted;
        internal Card <>f__this;
        internal Spell <spell>__0;
        internal bool exhausted;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (!this.<>f__this.m_actorReady)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_00DB;
                    }
                    this.<spell>__0 = this.<>f__this.m_actor.GetComponent<Spell>();
                    break;

                case 2:
                    break;

                default:
                    goto Label_00D9;
            }
            while (this.<spell>__0.GetActiveState() != SpellStateType.NONE)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00DB;
            }
            if (this.<>f__this.CanShowSecret())
            {
                if (this.exhausted)
                {
                    this.<>f__this.SheatheSecret(this.<spell>__0);
                }
                else
                {
                    this.<>f__this.UnSheatheSecret(this.<spell>__0);
                }
                this.$PC = -1;
            }
        Label_00D9:
            return false;
        Label_00DB:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <SwitchOutFriendlyLinkedDrawnCard>c__Iterator247 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>oldCard;
        internal Card <>f__this;
        internal Card oldCard;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.oldCard.DoNullZoneVisuals();
                    this.<>f__this.m_actor.Show();
                    break;

                case 1:
                    break;

                default:
                    goto Label_00C2;
            }
            if (this.<>f__this.m_holdingForLinkedCardSwitch)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.m_doNotSort = false;
            GameState.Get().SetFriendlyCardBeingDrawn(null);
            SoundManager.Get().LoadAndPlay("add_card_to_hand_1", this.<>f__this.gameObject);
            this.<>f__this.CreateStartingCardStateEffects();
            this.<>f__this.RefreshActor();
            this.<>f__this.m_zone.UpdateLayout();
            this.$PC = -1;
        Label_00C2:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <SwitchOutOpponentLinkedDrawnCard>c__Iterator248 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>oldCard;
        internal Card <>f__this;
        internal AssetLoader.GameObjectCallback <actorLoadedCallback>__2;
        internal string <actorName>__3;
        internal Actor <handActor>__0;
        internal bool <loadingActor>__1;
        internal Card oldCard;

        internal void <>m__200(string name, GameObject go, object callbackData)
        {
            this.<loadingActor>__1 = false;
            if (go == null)
            {
                object[] messageArgs = new object[] { name };
                Error.AddDevFatal("Card.SwitchOutOpponentLinkedDrawnCard() - failed to load {0}", messageArgs);
            }
            else
            {
                this.<handActor>__0 = go.GetComponent<Actor>();
                if (this.<handActor>__0 == null)
                {
                    object[] objArray2 = new object[] { name };
                    Error.AddDevFatal("Card.SwitchOutOpponentLinkedDrawnCard() - instance of {0} has no Actor component", objArray2);
                }
            }
        }

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_beingDrawnByOpponent = true;
                    this.<handActor>__0 = null;
                    this.<loadingActor>__1 = true;
                    this.<actorLoadedCallback>__2 = new AssetLoader.GameObjectCallback(this.<>m__200);
                    this.<actorName>__3 = ActorNames.GetHandActor(this.<>f__this.m_entity);
                    AssetLoader.Get().LoadActor(this.<actorName>__3, this.<actorLoadedCallback>__2, null, false);
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0190;

                case 3:
                    this.$PC = -1;
                    goto Label_01D5;

                default:
                    goto Label_01D5;
            }
            if (this.<loadingActor>__1)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_01D7;
            }
            this.oldCard.m_actorWaitingToBeReplaced.Destroy();
            this.oldCard.m_actorWaitingToBeReplaced = null;
            this.oldCard.DoNullZoneVisuals();
            if (this.<handActor>__0 == null)
            {
                this.<>f__this.m_doNotSort = false;
                this.<>f__this.m_beingDrawnByOpponent = false;
                GameState.Get().SetOpponentCardBeingDrawn(null);
                goto Label_01D5;
            }
            this.<handActor>__0.SetEntity(this.<>f__this.m_entity);
            this.<handActor>__0.SetCardDef(this.<>f__this.m_cardDef);
            this.<handActor>__0.UpdateAllComponents();
            this.<handActor>__0.transform.parent = this.<>f__this.m_actor.transform.parent;
            TransformUtil.CopyLocal((Component) this.<handActor>__0, (Component) this.<>f__this.m_actor);
        Label_0190:
            while (this.<>f__this.m_holdingForLinkedCardSwitch)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01D7;
            }
            this.$current = this.<>f__this.StartCoroutine(this.<>f__this.HideRevealedOpponentCard(this.<handActor>__0));
            this.$PC = 3;
            goto Label_01D7;
        Label_01D5:
            return false;
        Label_01D7:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <SwitchSecretSides>c__Iterator24C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;
        internal AssetLoader.GameObjectCallback <actorLoadedCallback>__2;
        internal bool <loadingActor>__1;
        internal Actor <newActor>__0;
        internal Actor <oldActor>__3;
        internal bool <oldActorFinished>__4;
        internal Spell <oldSpell>__7;
        internal Spell.FinishedCallback <onOldSpellFinished>__5;
        internal Spell.StateFinishedCallback <onOldSpellStateFinished>__6;

        internal void <>m__205(string name, GameObject go, object callbackData)
        {
            this.<loadingActor>__1 = false;
            if (go == null)
            {
                object[] messageArgs = new object[] { name };
                Error.AddDevFatal("Card.SwitchSecretSides() - failed to load {0}", messageArgs);
            }
            else
            {
                this.<newActor>__0 = go.GetComponent<Actor>();
                if (this.<newActor>__0 == null)
                {
                    object[] objArray2 = new object[] { name };
                    Error.AddDevFatal("Card.SwitchSecretSides() - instance of {0} has no Actor component", objArray2);
                }
            }
        }

        internal void <>m__206(Spell spell, object userData)
        {
            this.<oldActorFinished>__4 = true;
        }

        internal void <>m__207(Spell spell, SpellStateType prevStateType, object userData)
        {
            if (spell.GetActiveState() == SpellStateType.NONE)
            {
                this.<oldActor>__3.Destroy();
            }
        }

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_doNotSort = true;
                    this.<newActor>__0 = null;
                    this.<loadingActor>__1 = true;
                    this.<actorLoadedCallback>__2 = new AssetLoader.GameObjectCallback(this.<>m__205);
                    AssetLoader.Get().LoadActor(this.<>f__this.m_actorName, this.<actorLoadedCallback>__2, null, false);
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0233;

                default:
                    goto Label_029F;
            }
            if (this.<loadingActor>__1)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_02A1;
            }
            if (this.<newActor>__0 == null)
            {
                goto Label_0265;
            }
            this.<oldActor>__3 = this.<>f__this.m_actor;
            this.<>f__this.m_actor = this.<newActor>__0;
            this.<>f__this.m_actor.SetEntity(this.<>f__this.m_entity);
            this.<>f__this.m_actor.SetCard(this.<>f__this);
            this.<>f__this.m_actor.SetCardDef(this.<>f__this.m_cardDef);
            this.<>f__this.m_actor.UpdateAllComponents();
            this.<>f__this.m_actor.transform.parent = this.<oldActor>__3.transform.parent;
            TransformUtil.Identity(this.<>f__this.m_actor);
            this.<>f__this.m_actor.Hide();
            if (!this.<>f__this.CanShowSecretDeath())
            {
                this.<oldActor>__3.Destroy();
                goto Label_023E;
            }
            this.<oldActor>__3.transform.parent = this.<>f__this.transform.parent;
            this.<>f__this.m_transitionStyle = ZoneTransitionStyle.INSTANT;
            this.<oldActorFinished>__4 = false;
            this.<onOldSpellFinished>__5 = new Spell.FinishedCallback(this.<>m__206);
            this.<onOldSpellStateFinished>__6 = new Spell.StateFinishedCallback(this.<>m__207);
            this.<oldSpell>__7 = this.<oldActor>__3.GetComponent<Spell>();
            this.<oldSpell>__7.AddFinishedCallback(this.<onOldSpellFinished>__5);
            this.<oldSpell>__7.AddStateFinishedCallback(this.<onOldSpellStateFinished>__6);
            this.<oldSpell>__7.ActivateState(SpellStateType.ACTION);
        Label_0233:
            while (!this.<oldActorFinished>__4)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_02A1;
            }
        Label_023E:
            this.<>f__this.m_shown = true;
            this.<>f__this.m_actor.Show();
            this.<>f__this.ShowSecretBirth();
        Label_0265:
            this.<>f__this.m_actorReady = true;
            this.<>f__this.m_doNotSort = false;
            this.<>f__this.m_zone.UpdateLayout();
            this.<>f__this.CreateStartingCardStateEffects();
            this.$PC = -1;
        Label_029F:
            return false;
        Label_02A1:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WaitAndPrepareForDeathAnimation>c__Iterator24F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>dyingActor;
        internal Card <>f__this;
        internal Actor dyingActor;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(0.6f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.PrepareForDeathAnimation(this.dyingActor);
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WaitAndThenShowDestroyedSecret>c__Iterator24B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;
        internal Vector3 <newPos>__1;
        internal float <slideAmount>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<slideAmount>__0 = 2f;
                    if (this.<>f__this.GetEntity().IsControlledByOpposingSidePlayer())
                    {
                        this.<slideAmount>__0 = -this.<slideAmount>__0;
                    }
                    this.<newPos>__1 = new Vector3(this.<>f__this.transform.position.x, this.<>f__this.transform.position.y + 1f, this.<>f__this.transform.position.z + this.<slideAmount>__0);
                    this.<>f__this.m_actor.Show();
                    iTween.MoveTo(this.<>f__this.gameObject, this.<newPos>__1, 3f);
                    this.<>f__this.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                    this.<>f__this.transform.localEulerAngles = new Vector3(0f, 0f, 357f);
                    iTween.ScaleTo(this.<>f__this.gameObject, new Vector3(1.25f, 0.2f, 1.25f), 3f);
                    this.<>f__this.StartCoroutine(this.<>f__this.ActivateGraveyardActorDeathSpellAfterDelay());
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WaitForCardDrawBlockingTaskLists>c__Iterator24E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;
        internal PowerTaskList <blockingTaskList>__3;
        internal PowerTaskList <parentTaskList>__2;
        internal PowerQueue <powerQueue>__0;
        internal PowerTaskList <taskList>__1;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<powerQueue>__0 = GameState.Get().GetPowerProcessor().GetPowerQueue();
                    goto Label_00EB;

                case 1:
                    break;

                default:
                    goto Label_0103;
            }
        Label_00D5:
            while (this.<>f__this.DoesTaskListBlockCardDraw(this.<blockingTaskList>__3))
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
        Label_00EB:
            if (this.<powerQueue>__0.Count > 0)
            {
                this.<taskList>__1 = this.<powerQueue>__0.Peek();
                this.<parentTaskList>__2 = this.<taskList>__1.GetParent();
                this.<blockingTaskList>__3 = null;
                if (this.<>f__this.DoesTaskListBlockCardDraw(this.<taskList>__1))
                {
                    this.<blockingTaskList>__3 = this.<taskList>__1;
                }
                else if (this.<>f__this.DoesTaskListBlockCardDraw(this.<parentTaskList>__2))
                {
                    this.<blockingTaskList>__3 = this.<parentTaskList>__2;
                }
                if (this.<blockingTaskList>__3 != null)
                {
                    goto Label_00D5;
                }
            }
            this.$PC = -1;
        Label_0103:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WaitForCardDrawBlockingTurnStartSpells>c__Iterator24D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (this.<>f__this.ShouldCardDrawWaitForTurnStartSpells())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WaitThenActivateSoundSpell>c__Iterator239 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardSoundSpell <$>soundSpell;
        internal GameEntity <gameEntity>__0;
        internal CardSoundSpell soundSpell;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<gameEntity>__0 = GameState.Get().GetGameEntity();
                    break;

                case 1:
                    break;

                default:
                    goto Label_006B;
            }
            if (this.<gameEntity>__0.ShouldDelayCardSoundSpells())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.soundSpell.Reactivate();
            this.$PC = -1;
        Label_006B:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    private class SpellLoadRequest
    {
        public AssetLoader.GameObjectCallback m_loadCallback;
        public string m_path;
    }
}

