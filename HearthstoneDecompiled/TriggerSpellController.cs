using System;
using System.Collections.Generic;

public class TriggerSpellController : SpellController
{
    private Spell m_actorTriggerSpell;
    private int m_cardEffectsBlockingFinish;
    private int m_cardEffectsBlockingTaskListFinish;
    private List<CardSoundSpell> m_triggerSoundSpells = new List<CardSoundSpell>();
    private Spell m_triggerSpell;

    private bool ActivateActorTriggerSpell()
    {
        if (this.m_actorTriggerSpell == null)
        {
            return false;
        }
        Entity sourceEntity = base.m_taskList.GetSourceEntity();
        this.GetCardWithActorTrigger(sourceEntity).DeactivateBaubles();
        this.m_actorTriggerSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnActorTriggerSpellStateFinished));
        this.m_actorTriggerSpell.ActivateState(SpellStateType.ACTION);
        return true;
    }

    private bool ActivateCardEffects()
    {
        bool flag = this.ActivateTriggerSpell();
        bool flag2 = this.ActivateTriggerSounds();
        return (flag || flag2);
    }

    private bool ActivateInitialSpell()
    {
        return (this.ActivateActorTriggerSpell() || this.ActivateCardEffects());
    }

    private bool ActivateTriggerSounds()
    {
        if (this.m_triggerSoundSpells.Count == 0)
        {
            return false;
        }
        Card source = base.GetSource();
        foreach (CardSoundSpell spell in this.m_triggerSoundSpells)
        {
            if (spell != null)
            {
                source.ActivateSoundSpell(spell);
            }
        }
        this.CardSpellFinished();
        this.CardSpellNoneStateEntered();
        return true;
    }

    private bool ActivateTriggerSpell()
    {
        if (this.m_triggerSpell == null)
        {
            return false;
        }
        this.m_triggerSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnCardSpellFinished));
        this.m_triggerSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnCardSpellStateFinished));
        this.m_triggerSpell.ActivateState(SpellStateType.ACTION);
        return true;
    }

    protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
    {
        this.Reset();
        if (!this.HasSourceCard(taskList))
        {
            return false;
        }
        Entity sourceEntity = taskList.GetSourceEntity();
        Card card = sourceEntity.GetCard();
        Spell spell = this.DetermineTriggerSpell(card);
        bool spellInitialized = this.InitTriggerSpell(card, spell);
        List<CardSoundSpell> soundSpells = this.DetermineTriggerSounds(card);
        this.InitTriggerSounds(card, soundSpells);
        if (this.CanPlayActorTriggerSpell(sourceEntity, spell, spellInitialized))
        {
            this.m_actorTriggerSpell = this.GetActorTriggerSpell(sourceEntity);
        }
        if (((this.m_triggerSpell == null) && (this.m_triggerSoundSpells.Count == 0)) && (this.m_actorTriggerSpell == null))
        {
            return TurnStartManager.Get().IsCardDrawHandled(card);
        }
        base.SetSource(card);
        return true;
    }

    private bool CanPlayActorTriggerSpell(Entity entity, Spell spell, bool spellInitialized)
    {
        if ((!entity.HasTriggerVisual() && !entity.IsPoisonous()) && !entity.HasInspire())
        {
            return false;
        }
        if (((entity.GetController() != null) && !entity.GetController().IsFriendlySide()) && entity.IsObfuscated())
        {
            return false;
        }
        Card cardWithActorTrigger = this.GetCardWithActorTrigger(entity);
        if (cardWithActorTrigger == null)
        {
            return false;
        }
        if (cardWithActorTrigger.WillSuppressActorTriggerSpell())
        {
            return false;
        }
        if (spell != null)
        {
            if (!spellInitialized)
            {
                return false;
            }
        }
        else if (!SpellUtils.CanAddPowerTargets(base.m_taskList))
        {
            return false;
        }
        return true;
    }

    private void CardSpellFinished()
    {
        this.m_cardEffectsBlockingTaskListFinish--;
        if (this.m_cardEffectsBlockingTaskListFinish <= 0)
        {
            base.OnFinishedTaskList();
        }
    }

    private void CardSpellNoneStateEntered()
    {
        this.m_cardEffectsBlockingFinish--;
        if (this.m_cardEffectsBlockingFinish <= 0)
        {
            base.OnFinished();
        }
    }

    private List<CardSoundSpell> DetermineTriggerSounds(Card card)
    {
        if (card == null)
        {
            return null;
        }
        Network.HistActionStart sourceAction = base.m_taskList.GetSourceAction();
        return card.GetTriggerSoundSpells(sourceAction.Index, true);
    }

    private Spell DetermineTriggerSpell(Card card)
    {
        if (card == null)
        {
            return null;
        }
        Network.HistActionStart sourceAction = base.m_taskList.GetSourceAction();
        return card.GetTriggerSpell(sourceAction.Index, true);
    }

    private Spell GetActorTriggerSpell(Entity entity)
    {
        SpellType tRIGGER;
        Card cardWithActorTrigger = this.GetCardWithActorTrigger(entity);
        if (entity.HasTriggerVisual())
        {
            if (GameState.Get().IsUsingFastActorTriggers())
            {
                tRIGGER = SpellType.FAST_TRIGGER;
            }
            else
            {
                tRIGGER = SpellType.TRIGGER;
            }
        }
        else if (entity.IsPoisonous())
        {
            tRIGGER = SpellType.POISONOUS;
        }
        else if (entity.HasInspire())
        {
            tRIGGER = SpellType.INSPIRE;
        }
        else
        {
            return null;
        }
        return cardWithActorTrigger.GetActorSpell(tRIGGER);
    }

    private Card GetCardWithActorTrigger(Entity entity)
    {
        if (entity == null)
        {
            return null;
        }
        if (entity.IsEnchantment())
        {
            Entity entity2 = GameState.Get().GetEntity(entity.GetAttached());
            if (entity2 == null)
            {
                return null;
            }
            return entity2.GetCard();
        }
        return entity.GetCard();
    }

    private Card GetCardWithActorTrigger(PowerTaskList taskList)
    {
        Entity sourceEntity = taskList.GetSourceEntity();
        return this.GetCardWithActorTrigger(sourceEntity);
    }

    protected override bool HasSourceCard(PowerTaskList taskList)
    {
        if (taskList.GetSourceEntity() == null)
        {
            return false;
        }
        if (this.GetCardWithActorTrigger(taskList) == null)
        {
            return false;
        }
        return true;
    }

    private bool InitTriggerSounds(Card card, List<CardSoundSpell> soundSpells)
    {
        if (soundSpells == null)
        {
            return false;
        }
        if (soundSpells.Count == 0)
        {
            return false;
        }
        foreach (CardSoundSpell spell in soundSpells)
        {
            if (spell != null)
            {
                if (!spell.AttachPowerTaskList(base.m_taskList))
                {
                    object[] args = new object[] { base.name, spell, card };
                    Log.Power.Print("{0}.InitTriggerSounds() - FAILED to attach task list to TriggerSoundSpell {1} for Card {2}", args);
                }
                else
                {
                    this.m_triggerSoundSpells.Add(spell);
                }
            }
        }
        if (this.m_triggerSoundSpells.Count == 0)
        {
            return false;
        }
        this.m_cardEffectsBlockingFinish++;
        this.m_cardEffectsBlockingTaskListFinish++;
        return true;
    }

    private bool InitTriggerSpell(Card sourceCard, Spell spell)
    {
        if (spell == null)
        {
            return false;
        }
        if (!spell.AttachPowerTaskList(base.m_taskList))
        {
            Log.Power.Print(string.Format("{0}.InitTriggerSpell() - FAILED to add targets to spell for {1}", this, sourceCard), new object[0]);
            return false;
        }
        this.m_triggerSpell = spell;
        this.m_cardEffectsBlockingFinish++;
        this.m_cardEffectsBlockingTaskListFinish++;
        return true;
    }

    private void OnActorTriggerSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (prevStateType == SpellStateType.ACTION)
        {
            spell.RemoveStateFinishedCallback(new Spell.StateFinishedCallback(this.OnActorTriggerSpellStateFinished));
            if (!this.ActivateCardEffects())
            {
                base.OnProcessTaskList();
            }
        }
    }

    private void OnCardSpellFinished(Spell spell, object userData)
    {
        this.CardSpellFinished();
    }

    private void OnCardSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            this.CardSpellNoneStateEntered();
        }
    }

    protected override void OnProcessTaskList()
    {
        if (!this.ActivateInitialSpell())
        {
            base.OnProcessTaskList();
        }
        else if (GameState.Get().IsTurnStartManagerActive())
        {
            TurnStartManager.Get().NotifyOfTriggerVisual();
        }
    }

    private void Reset()
    {
        this.m_triggerSpell = null;
        this.m_triggerSoundSpells.Clear();
        this.m_actorTriggerSpell = null;
        this.m_cardEffectsBlockingFinish = 0;
        this.m_cardEffectsBlockingTaskListFinish = 0;
    }
}

