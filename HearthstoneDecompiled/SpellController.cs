using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpellController : MonoBehaviour
{
    private List<FinishedCallback> m_finishedListeners = new List<FinishedCallback>();
    private List<FinishedTaskListCallback> m_finishedTaskListListeners = new List<FinishedTaskListCallback>();
    protected bool m_pendingFinish;
    protected bool m_processingTaskList;
    protected Card m_source;
    protected List<Card> m_targets = new List<Card>();
    protected PowerTaskList m_taskList;

    public void AddFinishedCallback(FinishedCallback callback)
    {
        if (!this.m_finishedListeners.Contains(callback))
        {
            this.m_finishedListeners.Add(callback);
        }
    }

    public void AddFinishedTaskListCallback(FinishedTaskListCallback callback)
    {
        if (!this.m_finishedTaskListListeners.Contains(callback))
        {
            this.m_finishedTaskListListeners.Add(callback);
        }
    }

    protected virtual bool AddPowerSourceAndTargets(PowerTaskList taskList)
    {
        if (!this.HasSourceCard(taskList))
        {
            return false;
        }
        if (!SpellUtils.CanAddPowerTargets(taskList))
        {
            return false;
        }
        Card card = taskList.GetSourceEntity().GetCard();
        this.SetSource(card);
        List<PowerTask> list = this.m_taskList.GetTaskList();
        for (int i = 0; i < list.Count; i++)
        {
            PowerTask task = list[i];
            Card targetCardFromPowerTask = this.GetTargetCardFromPowerTask(task);
            if (((targetCardFromPowerTask != null) && (card != targetCardFromPowerTask)) && !this.IsTarget(targetCardFromPowerTask))
            {
                this.AddTarget(targetCardFromPowerTask);
            }
        }
        return ((card != null) || (this.m_targets.Count > 0));
    }

    public void AddTarget(Card card)
    {
        this.m_targets.Add(card);
    }

    public bool AttachPowerTaskList(PowerTaskList taskList)
    {
        if (this.m_taskList != taskList)
        {
            this.DetachPowerTaskList();
            this.m_taskList = taskList;
        }
        return this.AddPowerSourceAndTargets(taskList);
    }

    public PowerTaskList DetachPowerTaskList()
    {
        PowerTaskList taskList = this.m_taskList;
        this.RemoveSource();
        this.RemoveAllTargets();
        this.m_taskList = null;
        return taskList;
    }

    public void DoPowerTaskList()
    {
        this.m_processingTaskList = true;
        base.gameObject.SetActive(true);
        GameState.Get().AddServerBlockingSpellController(this);
        base.StartCoroutine(this.WaitForCardsThenDoTaskList());
    }

    private void FireFinishedCallbacks()
    {
        FinishedCallback[] callbackArray = this.m_finishedListeners.ToArray();
        this.m_finishedListeners.Clear();
        for (int i = 0; i < callbackArray.Length; i++)
        {
            callbackArray[i](this);
        }
    }

    private void FireFinishedTaskListCallbacks()
    {
        FinishedTaskListCallback[] callbackArray = this.m_finishedTaskListListeners.ToArray();
        this.m_finishedTaskListListeners.Clear();
        for (int i = 0; i < callbackArray.Length; i++)
        {
            callbackArray[i](this);
        }
    }

    public PowerTaskList GetPowerTaskList()
    {
        return this.m_taskList;
    }

    public Card GetSource()
    {
        return this.m_source;
    }

    public Card GetTarget()
    {
        return ((this.m_targets.Count != 0) ? this.m_targets[0] : null);
    }

    protected Card GetTargetCardFromPowerTask(PowerTask task)
    {
        Network.PowerHistory power = task.GetPower();
        if (power.Type != Network.PowerType.TAG_CHANGE)
        {
            return null;
        }
        Network.HistTagChange change = power as Network.HistTagChange;
        Entity entity = GameState.Get().GetEntity(change.Entity);
        if (entity == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {1} but there is no entity with that id", this, change.Entity));
            return null;
        }
        return entity.GetCard();
    }

    public List<Card> GetTargets()
    {
        return this.m_targets;
    }

    protected virtual bool HasSourceCard(PowerTaskList taskList)
    {
        Entity sourceEntity = taskList.GetSourceEntity();
        if (sourceEntity == null)
        {
            return false;
        }
        if (sourceEntity.GetCard() == null)
        {
            return false;
        }
        return true;
    }

    protected bool IsCardBusy(Card card)
    {
        Entity entity = card.GetEntity();
        if (this.WillEntityLoadCard(entity))
        {
            return false;
        }
        if (entity.IsLoadingAssets())
        {
            return true;
        }
        if ((TurnStartManager.Get() != null) && TurnStartManager.Get().IsCardDrawHandled(card))
        {
            return false;
        }
        return !card.IsActorReady();
    }

    public bool IsProcessingTaskList()
    {
        return this.m_processingTaskList;
    }

    public bool IsSource(Card card)
    {
        return (this.m_source == card);
    }

    public bool IsTarget(Card card)
    {
        foreach (Card card2 in this.m_targets)
        {
            if (card2 == card)
            {
                return true;
            }
        }
        return false;
    }

    public void OnFinished()
    {
        if (this.m_processingTaskList)
        {
            this.m_pendingFinish = true;
        }
        else
        {
            base.gameObject.SetActive(false);
            this.FireFinishedCallbacks();
        }
    }

    public void OnFinishedTaskList()
    {
        GameState.Get().RemoveServerBlockingSpellController(this);
        this.m_processingTaskList = false;
        this.FireFinishedTaskListCallbacks();
        if (this.m_pendingFinish)
        {
            this.m_pendingFinish = false;
            this.OnFinished();
        }
    }

    protected virtual void OnProcessTaskList()
    {
        this.OnFinishedTaskList();
        this.OnFinished();
    }

    public void RemoveAllTargets()
    {
        this.m_targets.Clear();
    }

    public void RemoveSource()
    {
        this.m_source = null;
    }

    public void RemoveTarget(Card card)
    {
        this.m_targets.Remove(card);
    }

    public void SetPowerTaskList(PowerTaskList taskList)
    {
        if (this.m_taskList != taskList)
        {
            this.DetachPowerTaskList();
            this.m_taskList = taskList;
        }
    }

    public void SetSource(Card card)
    {
        this.m_source = card;
    }

    [DebuggerHidden]
    private IEnumerator WaitForCardsThenDoTaskList()
    {
        return new <WaitForCardsThenDoTaskList>c__Iterator1BD { <>f__this = this };
    }

    private bool WillEntityLoadCard(Entity entity)
    {
        int entityId = entity.GetEntityId();
        foreach (PowerTask task in this.m_taskList.GetTaskList())
        {
            Network.PowerHistory power = task.GetPower();
            switch (power.Type)
            {
                case Network.PowerType.FULL_ENTITY:
                {
                    Network.HistFullEntity entity2 = power as Network.HistFullEntity;
                    if (entityId == entity2.Entity.ID)
                    {
                        return true;
                    }
                    break;
                }
                case Network.PowerType.SHOW_ENTITY:
                {
                    Network.HistShowEntity entity3 = power as Network.HistShowEntity;
                    if (entityId != entity3.Entity.ID)
                    {
                        break;
                    }
                    return true;
                }
            }
        }
        return false;
    }

    [CompilerGenerated]
    private sealed class <WaitForCardsThenDoTaskList>c__Iterator1BD : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Card>.Enumerator <$s_955>__2;
        internal SpellController <>f__this;
        internal float <currWaitSec>__0;
        internal Card <sourceCard>__1;
        internal Card <targetCard>__3;

        [DebuggerHidden]
        public void Dispose()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 2:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_955>__2.Dispose();
                    }
                    break;
            }
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            bool flag = false;
            switch (num)
            {
                case 0:
                    this.<currWaitSec>__0 = 0f;
                    this.<sourceCard>__1 = this.<>f__this.GetSource();
                    if (this.<sourceCard>__1 == null)
                    {
                        goto Label_00C4;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_00DD;

                default:
                    goto Label_01E3;
            }
            if (this.<>f__this.IsCardBusy(this.<sourceCard>__1))
            {
                if (this.<currWaitSec>__0 >= 7f)
                {
                    UnityEngine.Debug.LogError(string.Format("{0}.WaitForCardsThenDoTaskList() - sourceCard {1} exceeded the max wait time. Printing task list.", this.<>f__this, this.<sourceCard>__1));
                }
                else
                {
                    this.<currWaitSec>__0 += UnityEngine.Time.deltaTime;
                    this.$current = null;
                    this.$PC = 1;
                    goto Label_01E5;
                }
            }
        Label_00C4:
            this.<$s_955>__2 = this.<>f__this.m_targets.GetEnumerator();
            num = 0xfffffffd;
        Label_00DD:
            try
            {
                switch (num)
                {
                    case 2:
                        goto Label_0171;
                }
                while (this.<$s_955>__2.MoveNext())
                {
                    this.<targetCard>__3 = this.<$s_955>__2.Current;
                    if (this.<targetCard>__3 == null)
                    {
                        continue;
                    }
                Label_0171:
                    while (this.<>f__this.IsCardBusy(this.<targetCard>__3))
                    {
                        if (this.<currWaitSec>__0 >= 7f)
                        {
                            UnityEngine.Debug.LogError(string.Format("{0}.WaitForCardsThenDoTaskList() - targetCard {1} exceeded the max wait time. Printing task list.", this.<>f__this, this.<targetCard>__3));
                            continue;
                        }
                        this.<currWaitSec>__0 += UnityEngine.Time.deltaTime;
                        this.$current = null;
                        this.$PC = 2;
                        flag = true;
                        goto Label_01E5;
                    }
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_955>__2.Dispose();
            }
            if (this.<currWaitSec>__0 >= 7f)
            {
                GameState.Get().OnSpellControllerPassedMaxWaitSec(this.<>f__this);
            }
            this.<>f__this.OnProcessTaskList();
            this.$PC = -1;
        Label_01E3:
            return false;
        Label_01E5:
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

    public delegate void FinishedCallback(SpellController spellController);

    public delegate void FinishedTaskListCallback(SpellController spellController);
}

