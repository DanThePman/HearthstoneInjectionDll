using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PowerSpellController : SpellController
{
    private int m_cardEffectsBlockingFinish;
    private int m_cardEffectsBlockingTaskListFinish;
    private List<CardSoundSpell> m_powerSoundSpells = new List<CardSoundSpell>();
    private Spell m_powerSpell;

    private bool ActivateActorBattlecrySpell()
    {
        Card source = base.GetSource();
        Entity entity = source.GetEntity();
        if (!this.CanActivateActorBattlecrySpell(entity))
        {
            return false;
        }
        Spell actorBattlecrySpell = this.GetActorBattlecrySpell(source);
        if (actorBattlecrySpell == null)
        {
            return false;
        }
        base.StartCoroutine(this.WaitThenActivateActorBattlecrySpell(actorBattlecrySpell));
        return true;
    }

    private bool ActivateCardEffects()
    {
        bool flag = this.ActivatePowerSpell();
        bool flag2 = this.ActivatePowerSounds();
        return (flag || flag2);
    }

    private bool ActivatePowerSounds()
    {
        if (this.m_powerSoundSpells.Count == 0)
        {
            return false;
        }
        Card source = base.GetSource();
        foreach (CardSoundSpell spell in this.m_powerSoundSpells)
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

    private bool ActivatePowerSpell()
    {
        if (this.m_powerSpell == null)
        {
            return false;
        }
        this.m_powerSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnCardSpellFinished));
        this.m_powerSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnCardSpellStateFinished));
        this.m_powerSpell.ActivateState(SpellStateType.ACTION);
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
        Spell spell = this.DeterminePowerSpell(sourceEntity, card);
        if (sourceEntity.IsMinion())
        {
            if (!this.InitPowerSpell(card, spell))
            {
                if (!SpellUtils.CanAddPowerTargets(taskList))
                {
                    return false;
                }
                if (this.GetActorBattlecrySpell(card) == null)
                {
                    return false;
                }
            }
        }
        else
        {
            this.InitPowerSpell(card, spell);
            List<CardSoundSpell> soundSpells = this.DeterminePowerSounds(sourceEntity, card);
            this.InitPowerSounds(card, soundSpells);
            if ((this.m_powerSpell == null) && (this.m_powerSoundSpells.Count == 0))
            {
                return false;
            }
        }
        base.SetSource(card);
        return true;
    }

    private bool CanActivateActorBattlecrySpell(Entity entity)
    {
        return (entity.HasBattlecry() || (entity.HasCombo() && entity.GetController().IsComboActive()));
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

    private List<CardSoundSpell> DeterminePowerSounds(Entity entity, Card card)
    {
        int index = base.m_taskList.GetSourceAction().Index;
        if (index >= 0)
        {
            return card.GetSubOptionSoundSpells(index, true);
        }
        return card.GetPlaySoundSpells(true);
    }

    private Spell DeterminePowerSpell(Entity entity, Card card)
    {
        int index = base.m_taskList.GetSourceAction().Index;
        if (index >= 0)
        {
            return card.GetSubOptionSpell(index, true);
        }
        return card.GetPlaySpell(true);
    }

    private Spell GetActorBattlecrySpell(Card card)
    {
        Spell actorSpell = card.GetActorSpell(SpellType.BATTLECRY);
        if (actorSpell == null)
        {
            return null;
        }
        if (!actorSpell.HasUsableState(SpellStateType.ACTION))
        {
            return null;
        }
        return actorSpell;
    }

    private bool InitPowerSounds(Card card, List<CardSoundSpell> soundSpells)
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
                    Log.Power.Print("{0}.InitPowerSounds() - FAILED to attach task list to PowerSoundSpell {1} for Card {2}", args);
                }
                else
                {
                    this.m_powerSoundSpells.Add(spell);
                }
            }
        }
        if (this.m_powerSoundSpells.Count == 0)
        {
            return false;
        }
        this.m_cardEffectsBlockingFinish++;
        this.m_cardEffectsBlockingTaskListFinish++;
        return true;
    }

    private bool InitPowerSpell(Card card, Spell spell)
    {
        if (spell == null)
        {
            return false;
        }
        if (!spell.HasUsableState(SpellStateType.ACTION))
        {
            object[] args = new object[] { base.name, spell, card, SpellStateType.ACTION };
            Log.Power.PrintWarning("{0}.InitPowerSpell() - spell {1} for Card {2} has no {3} state", args);
            return false;
        }
        if (!spell.AttachPowerTaskList(base.m_taskList))
        {
            object[] objArray2 = new object[] { base.name, spell, card };
            Log.Power.Print("{0}.InitPowerSpell() - FAILED to attach task list to spell {1} for Card {2}", objArray2);
            return false;
        }
        if (spell.GetActiveState() != SpellStateType.NONE)
        {
            spell.ActivateState(SpellStateType.NONE);
        }
        this.m_powerSpell = spell;
        this.m_cardEffectsBlockingFinish++;
        this.m_cardEffectsBlockingTaskListFinish++;
        return true;
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
        if (!this.ActivateActorBattlecrySpell() && !this.ActivateCardEffects())
        {
            base.OnProcessTaskList();
        }
    }

    private void Reset()
    {
        this.m_powerSpell = null;
        this.m_powerSoundSpells.Clear();
        this.m_cardEffectsBlockingFinish = 0;
        this.m_cardEffectsBlockingTaskListFinish = 0;
    }

    [DebuggerHidden]
    private IEnumerator WaitThenActivateActorBattlecrySpell(Spell actorBattlecrySpell)
    {
        return new <WaitThenActivateActorBattlecrySpell>c__Iterator1C5 { actorBattlecrySpell = actorBattlecrySpell, <$>actorBattlecrySpell = actorBattlecrySpell, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitThenActivateActorBattlecrySpell>c__Iterator1C5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Spell <$>actorBattlecrySpell;
        internal PowerSpellController <>f__this;
        internal Spell actorBattlecrySpell;

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
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.actorBattlecrySpell.ActivateState(SpellStateType.ACTION);
                    if (!this.<>f__this.ActivateCardEffects())
                    {
                        this.<>f__this.OnProcessTaskList();
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
}

