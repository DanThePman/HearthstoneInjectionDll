using System;
using System.Collections.Generic;
using UnityEngine;

public class DeathSpellController : SpellController
{
    private void AddDeadCardsToTargetList(PowerTaskList taskList)
    {
        List<PowerTask> list = base.m_taskList.GetTaskList();
        for (int i = 0; i < list.Count; i++)
        {
            Network.PowerHistory power = list[i].GetPower();
            if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange tagChange = power as Network.HistTagChange;
                if (GameUtils.IsCharacterDeathTagChange(tagChange))
                {
                    Entity entity = GameState.Get().GetEntity(tagChange.Entity);
                    Card card = entity.GetCard();
                    if (this.CanAddTarget(entity, card))
                    {
                        base.AddTarget(card);
                    }
                }
            }
        }
    }

    protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
    {
        this.AddDeadCardsToTargetList(taskList);
        if (base.m_targets.Count == 0)
        {
            return false;
        }
        return true;
    }

    private bool CanAddTarget(Entity entity, Card card)
    {
        if (card.WillSuppressDeathEffects())
        {
            return false;
        }
        return true;
    }

    private bool CanPlayDeathSound(Entity entity)
    {
        if (entity.HasTag(GAME_TAG.DEATHRATTLE_RETURN_ZONE))
        {
            return false;
        }
        return true;
    }

    protected override void OnProcessTaskList()
    {
        int num = this.PickDeathSoundCardIndex();
        for (int i = 0; i < base.m_targets.Count; i++)
        {
            Card card = base.m_targets[i];
            card.SuppressDeathSounds(i != num);
            card.ActivateCharacterDeathEffects();
        }
        base.OnProcessTaskList();
    }

    private int PickDeathSoundCardIndex()
    {
        if (base.m_targets.Count == 1)
        {
            Entity entity = base.m_targets[0].GetEntity();
            if (this.CanPlayDeathSound(entity))
            {
                return 0;
            }
            return -1;
        }
        if (base.m_targets.Count == 2)
        {
            Card card2 = base.m_targets[0];
            Card card3 = base.m_targets[1];
            Entity defender = card2.GetEntity();
            Entity attacker = card3.GetEntity();
            if (this.WasAttackedBy(defender, attacker))
            {
                if (this.CanPlayDeathSound(defender))
                {
                    return 0;
                }
                return 1;
            }
            if (this.WasAttackedBy(attacker, defender))
            {
                if (this.CanPlayDeathSound(attacker))
                {
                    return 1;
                }
                return 0;
            }
        }
        return this.PickRandomDeathSoundCardIndex();
    }

    private int PickRandomDeathSoundCardIndex()
    {
        List<int> list = new List<int>();
        for (int i = 0; i < base.m_targets.Count; i++)
        {
            Entity entity = base.m_targets[i].GetEntity();
            if (this.CanPlayDeathSound(entity))
            {
                list.Add(i);
            }
        }
        if (list.Count == 0)
        {
            return -1;
        }
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    private bool WasAttackedBy(Entity defender, Entity attacker)
    {
        if (!attacker.HasTag(GAME_TAG.ATTACKING))
        {
            return false;
        }
        if (!defender.HasTag(GAME_TAG.DEFENDING))
        {
            return false;
        }
        if (defender.GetTag(GAME_TAG.LAST_AFFECTED_BY) != attacker.GetEntityId())
        {
            return false;
        }
        return true;
    }
}

