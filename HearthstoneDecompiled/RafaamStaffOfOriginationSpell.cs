using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RafaamStaffOfOriginationSpell : Spell
{
    public Spell m_CustomSpawnSpell;
    private int m_metaDataIndex;
    private int m_spawnTaskIndex;

    public override bool AddPowerTargets()
    {
        if (!base.m_taskList.DoesBlockHaveMetaDataTasks())
        {
            return false;
        }
        List<PowerTask> taskList = base.m_taskList.GetTaskList();
        if (!base.AddMultiplePowerTargets_FromMetaData(taskList))
        {
            return false;
        }
        return true;
    }

    protected override void AddTargetFromMetaData(int metaDataIndex, Card targetCard)
    {
        this.m_metaDataIndex = metaDataIndex;
        this.m_spawnTaskIndex = this.FindSpawnTaskIndex();
        base.AddTargetFromMetaData(metaDataIndex, targetCard);
    }

    private void ApplyCustomSpawnOverride()
    {
        foreach (GameObject obj2 in base.m_targets)
        {
            Card component = obj2.GetComponent<Card>();
            Spell spell = UnityEngine.Object.Instantiate<Spell>(this.m_CustomSpawnSpell);
            component.OverrideCustomSpawnSpell(spell);
        }
    }

    private void DoTasksUntilSpawn()
    {
        PowerTaskList.CompleteCallback callback = (taskList, startIndex, count, userData) => base.StartCoroutine(this.WaitThenFinish());
        base.m_taskList.DoTasks(0, this.m_spawnTaskIndex, callback);
    }

    private int FindSpawnTaskIndex()
    {
        List<PowerTask> taskList = base.m_taskList.GetTaskList();
        for (int i = this.m_metaDataIndex - 1; i >= 0; i--)
        {
            PowerTask task = taskList[i];
            if (task.GetPower() is Network.HistFullEntity)
            {
                return i;
            }
        }
        return -1;
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.OnAction(prevStateType);
        this.ApplyCustomSpawnOverride();
        this.DoTasksUntilSpawn();
    }

    [DebuggerHidden]
    private IEnumerator WaitThenFinish()
    {
        return new <WaitThenFinish>c__Iterator222 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitThenFinish>c__Iterator222 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal RafaamStaffOfOriginationSpell <>f__this;
        internal Spell <electricSpell>__4;
        internal Network.HistFullEntity <fullEntity>__1;
        internal Card <heroPowerCard>__3;
        internal Entity <spawnedEntity>__2;
        internal PowerTask <spawnTask>__0;

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
                    this.<spawnTask>__0 = this.<>f__this.m_taskList.GetTaskList()[this.<>f__this.m_spawnTaskIndex];
                    this.<fullEntity>__1 = (Network.HistFullEntity) this.<spawnTask>__0.GetPower();
                    this.<spawnedEntity>__2 = GameState.Get().GetEntity(this.<fullEntity>__1.Entity.ID);
                    this.<heroPowerCard>__3 = this.<spawnedEntity>__2.GetHeroPowerCard();
                    this.<electricSpell>__4 = this.<heroPowerCard>__3.GetActorSpell(SpellType.ELECTRIC_CHARGE_LEVEL_LARGE);
                    break;

                case 1:
                    break;

                default:
                    goto Label_00DB;
            }
            if (!this.<electricSpell>__4.IsFinished())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.OnStateFinished();
            this.$PC = -1;
        Label_00DB:
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

