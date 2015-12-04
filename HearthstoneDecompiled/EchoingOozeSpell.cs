using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EchoingOozeSpell : Spell
{
    public Spell m_CustomSpawnSpell;
    public float m_PostSpawnDelayMax;
    public float m_PostSpawnDelayMin;

    private void DoEffect(Card targetCard)
    {
        Spell spell = UnityEngine.Object.Instantiate<Spell>(this.m_CustomSpawnSpell);
        targetCard.OverrideCustomSpawnSpell(spell);
        this.DoTasksUntilSpawn(targetCard);
        base.StartCoroutine(this.WaitThenFinish());
    }

    private void DoTasksUntilSpawn(Card targetCard)
    {
        int entityId = targetCard.GetEntity().GetEntityId();
        List<PowerTask> taskList = base.m_taskList.GetTaskList();
        int num2 = 0;
        for (int i = 0; i < taskList.Count; i++)
        {
            PowerTask task = taskList[i];
            Network.HistFullEntity power = task.GetPower() as Network.HistFullEntity;
            if ((power != null) && (power.Entity.ID == entityId))
            {
                num2 = i;
                break;
            }
        }
        base.m_taskList.DoTasks(0, num2 + 1);
    }

    protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
    {
        Network.HistFullEntity power = task.GetPower() as Network.HistFullEntity;
        if (power == null)
        {
            return null;
        }
        Network.Entity entity = power.Entity;
        Entity entity3 = GameState.Get().GetEntity(entity.ID);
        if (entity3 == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {1} but there is no entity with that id", this, entity.ID));
            return null;
        }
        return entity3.GetCard();
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.OnAction(prevStateType);
        Card targetCard = base.GetTargetCard();
        if (targetCard == null)
        {
            this.OnStateFinished();
        }
        else
        {
            this.DoEffect(targetCard);
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenFinish()
    {
        return new <WaitThenFinish>c__Iterator20F { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitThenFinish>c__Iterator20F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal EchoingOozeSpell <>f__this;
        internal float <delaySec>__0;

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
                    this.<delaySec>__0 = UnityEngine.Random.Range(this.<>f__this.m_PostSpawnDelayMin, this.<>f__this.m_PostSpawnDelayMax);
                    if (object.Equals(this.<delaySec>__0, 0f))
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(this.<delaySec>__0);
                    this.$PC = 1;
                    return true;

                case 1:
                    break;

                default:
                    goto Label_0090;
            }
            this.<>f__this.OnStateFinished();
            this.$PC = -1;
        Label_0090:
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

