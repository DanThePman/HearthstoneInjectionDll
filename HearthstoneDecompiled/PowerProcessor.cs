using PegasusGame;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PowerProcessor
{
    private const string ATTACK_SPELL_CONTROLLER_PREFAB_NAME = "AttackSpellController";
    private const string JOUST_SPELL_CONTROLLER_PREFAB_NAME = "JoustSpellController";
    private bool m_buildingTaskList;
    private PowerTaskList m_busyTaskList;
    private PowerTaskList m_currentTaskList;
    private PowerTaskList m_earlyConcedeTaskList;
    private PowerTaskList m_gameOverTaskList;
    private bool m_handledFirstEarlyConcede;
    private bool m_historyBlocking;
    private PowerTaskList m_historyBlockingTaskList;
    private int m_nextTaskListId = 1;
    private PowerQueue m_powerQueue = new PowerQueue();
    private Stack<PowerTaskList> m_previousStack = new Stack<PowerTaskList>();
    private const string SECRET_SPELL_CONTROLLER_PREFAB_NAME = "SecretSpellController";

    private void BuildTaskList(List<Network.PowerHistory> powerList, ref int index, PowerTaskList taskList)
    {
        while (index < powerList.Count)
        {
            Network.PowerHistory netPower = powerList[index];
            Network.PowerType type = netPower.Type;
            if (type == Network.PowerType.ACTION_START)
            {
                if (!taskList.IsEmpty())
                {
                    this.EnqueueTaskList(taskList);
                }
                PowerTaskList item = new PowerTaskList();
                item.SetSourceAction((Network.HistActionStart) netPower);
                PowerTaskList origin = taskList.GetOrigin();
                if (origin.IsSourceActionOrigin())
                {
                    item.SetParent(origin);
                }
                this.m_previousStack.Push(item);
                index++;
                this.BuildTaskList(powerList, ref index, item);
                return;
            }
            if (type == Network.PowerType.ACTION_END)
            {
                taskList.SetEndAction((Network.HistActionEnd) netPower);
                if (this.m_previousStack.Count > 0)
                {
                    this.m_previousStack.Pop();
                    this.EnqueueTaskList(taskList);
                    return;
                }
                break;
            }
            taskList.CreateTask(netPower).DoRealTimeTask(powerList, index);
            index++;
        }
        if (!taskList.IsEmpty())
        {
            this.EnqueueTaskList(taskList);
        }
    }

    private void CancelSpellsForEarlyConcede(PowerTaskList taskList)
    {
        Entity sourceEntity = taskList.GetSourceEntity();
        if (sourceEntity != null)
        {
            Card card = sourceEntity.GetCard();
            if ((card != null) && (taskList.GetSourceAction().BlockType == HistoryBlock.Type.POWER))
            {
                Spell playSpell = card.GetPlaySpell(true);
                if (playSpell != null)
                {
                    SpellStateType activeState = playSpell.GetActiveState();
                    if ((activeState != SpellStateType.NONE) && (activeState != SpellStateType.CANCEL))
                    {
                        playSpell.ActivateState(SpellStateType.CANCEL);
                    }
                }
            }
        }
    }

    private bool CanEarlyConcede()
    {
        if (this.m_earlyConcedeTaskList != null)
        {
            return true;
        }
        if (!GameState.Get().IsGameOver() && GameState.Get().WasConcedeRequested())
        {
            Network.HistTagChange realTimeGameOverTagChange = GameState.Get().GetRealTimeGameOverTagChange();
            if ((realTimeGameOverTagChange != null) && (realTimeGameOverTagChange.Value != 4))
            {
                return true;
            }
        }
        return false;
    }

    public void Clear()
    {
        this.m_powerQueue.Clear();
        this.m_currentTaskList = null;
    }

    private AttackSpellController CreateAttackSpellController(PowerTaskList taskList)
    {
        return this.CreateSpellController<AttackSpellController>(taskList, "AttackSpellController");
    }

    private DeathSpellController CreateDeathSpellController(PowerTaskList taskList)
    {
        return this.CreateSpellController<DeathSpellController>(taskList, null);
    }

    private FatigueSpellController CreateFatigueSpellController(PowerTaskList taskList)
    {
        return this.CreateSpellController<FatigueSpellController>(taskList, null);
    }

    private JoustSpellController CreateJoustSpellController(PowerTaskList taskList)
    {
        return this.CreateSpellController<JoustSpellController>(taskList, "JoustSpellController");
    }

    private PowerSpellController CreatePowerSpellController(PowerTaskList taskList)
    {
        return this.CreateSpellController<PowerSpellController>(taskList, null);
    }

    private SecretSpellController CreateSecretSpellController(PowerTaskList taskList)
    {
        return this.CreateSpellController<SecretSpellController>(taskList, "SecretSpellController");
    }

    private T CreateSpellController<T>(PowerTaskList taskList, string prefabName = null) where T: SpellController
    {
        T component;
        GameObject obj2;
        if (prefabName == null)
        {
            obj2 = new GameObject();
            component = obj2.AddComponent<T>();
        }
        else
        {
            obj2 = AssetLoader.Get().LoadGameObject(prefabName, true, false);
            component = obj2.GetComponent<T>();
        }
        obj2.name = string.Format("{0} [taskListId={1}]", typeof(T), taskList.GetId());
        return component;
    }

    private TriggerSpellController CreateTriggerSpellController(PowerTaskList taskList)
    {
        return this.CreateSpellController<TriggerSpellController>(taskList, null);
    }

    private void DestroySpellController(SpellController spellController)
    {
        UnityEngine.Object.Destroy(spellController.gameObject);
    }

    private void DoCurrentTaskList()
    {
        this.m_currentTaskList.DoAllTasks((taskList, startIndex, count, userData) => this.EndCurrentTaskList());
    }

    private bool DoTaskListUsingController(SpellController spellController, PowerTaskList taskList, SpellController.FinishedCallback callback)
    {
        if (spellController == null)
        {
            Log.Power.Print("PowerProcessor.DoTaskListUsingController() - spellController=null", new object[0]);
            return false;
        }
        if (!spellController.AttachPowerTaskList(taskList))
        {
            return false;
        }
        spellController.AddFinishedTaskListCallback(new SpellController.FinishedTaskListCallback(this.OnSpellControllerFinishedTaskList));
        spellController.AddFinishedCallback(callback);
        spellController.DoPowerTaskList();
        return true;
    }

    private bool DoTaskListWithSpellController(GameState state, PowerTaskList taskList, Entity sourceEntity)
    {
        Network.HistActionStart sourceAction = taskList.GetSourceAction();
        switch (sourceAction.BlockType)
        {
            case HistoryBlock.Type.ATTACK:
            {
                AttackSpellController spellController = this.CreateAttackSpellController(taskList);
                if (!this.DoTaskListUsingController(spellController, taskList, new SpellController.FinishedCallback(this.OnSpellControllerFinished)))
                {
                    this.DestroySpellController(spellController);
                    return false;
                }
                return true;
            }
            case HistoryBlock.Type.POWER:
            {
                PowerSpellController controller2 = this.CreatePowerSpellController(taskList);
                if (!this.DoTaskListUsingController(controller2, taskList, new SpellController.FinishedCallback(this.OnSpellControllerFinished)))
                {
                    this.DestroySpellController(controller2);
                    return false;
                }
                return true;
            }
            case HistoryBlock.Type.TRIGGER:
                if (sourceEntity.IsSecret())
                {
                    SecretSpellController controller3 = this.CreateSecretSpellController(taskList);
                    if (!this.DoTaskListUsingController(controller3, taskList, new SpellController.FinishedCallback(this.OnSpellControllerFinished)))
                    {
                        this.DestroySpellController(controller3);
                        return false;
                    }
                }
                else
                {
                    TriggerSpellController controller4 = this.CreateTriggerSpellController(taskList);
                    Card card = sourceEntity.GetCard();
                    if (TurnStartManager.Get().IsCardDrawHandled(card))
                    {
                        if (!controller4.AttachPowerTaskList(taskList))
                        {
                            this.DestroySpellController(controller4);
                            return false;
                        }
                        controller4.AddFinishedTaskListCallback(new SpellController.FinishedTaskListCallback(this.OnSpellControllerFinishedTaskList));
                        controller4.AddFinishedCallback(new SpellController.FinishedCallback(this.OnSpellControllerFinished));
                        TurnStartManager.Get().NotifyOfSpellController(controller4);
                    }
                    else if (!this.DoTaskListUsingController(controller4, taskList, new SpellController.FinishedCallback(this.OnSpellControllerFinished)))
                    {
                        this.DestroySpellController(controller4);
                        return false;
                    }
                }
                return true;

            case HistoryBlock.Type.DEATHS:
            {
                DeathSpellController controller5 = this.CreateDeathSpellController(taskList);
                if (!this.DoTaskListUsingController(controller5, taskList, new SpellController.FinishedCallback(this.OnSpellControllerFinished)))
                {
                    this.DestroySpellController(controller5);
                    return false;
                }
                return true;
            }
            case HistoryBlock.Type.FATIGUE:
            {
                FatigueSpellController controller6 = this.CreateFatigueSpellController(taskList);
                if (!controller6.AttachPowerTaskList(taskList))
                {
                    this.DestroySpellController(controller6);
                    return false;
                }
                controller6.AddFinishedTaskListCallback(new SpellController.FinishedTaskListCallback(this.OnSpellControllerFinishedTaskList));
                controller6.AddFinishedCallback(new SpellController.FinishedCallback(this.OnSpellControllerFinished));
                if (state.IsTurnStartManagerActive())
                {
                    TurnStartManager.Get().NotifyOfSpellController(controller6);
                }
                else
                {
                    controller6.DoPowerTaskList();
                }
                return true;
            }
            case HistoryBlock.Type.JOUST:
            {
                JoustSpellController controller7 = this.CreateJoustSpellController(taskList);
                if (!this.DoTaskListUsingController(controller7, taskList, new SpellController.FinishedCallback(this.OnSpellControllerFinished)))
                {
                    this.DestroySpellController(controller7);
                    return false;
                }
                return true;
            }
        }
        object[] args = new object[] { sourceAction.BlockType, sourceEntity };
        Log.Power.Print("PowerProcessor.DoTaskListForCard() - actionStart has unhandled BlockType {0} for sourceEntity {1}", args);
        return false;
    }

    private void EndCurrentTaskList()
    {
        object[] args = new object[] { (this.m_currentTaskList != null) ? this.m_currentTaskList.GetId().ToString() : "null" };
        Log.Power.Print("PowerProcessor.EndCurrentTaskList() - m_currentTaskList={0}", args);
        if (this.m_currentTaskList == null)
        {
            Log.Ben.Print("PowerProcessor - EndCurrentTaskList() - m_currentTaskList is NULL", new object[0]);
            GameState.Get().OnTaskListEnded(null);
        }
        else
        {
            if (this.m_currentTaskList.GetEndAction() != null)
            {
                if (this.m_currentTaskList.IsInBlock(this.m_historyBlockingTaskList))
                {
                    this.m_historyBlockingTaskList = null;
                }
                if (this.m_currentTaskList.DidBlockSpawnHistoryTile())
                {
                    HistoryManager.Get().MarkCurrentHistoryEntryAsCompleted();
                }
            }
            GameState.Get().OnTaskListEnded(this.m_currentTaskList);
            this.m_currentTaskList = null;
        }
    }

    private void EnqueueTaskList(PowerTaskList taskList)
    {
        taskList.SetId(this.GetNextTaskListId());
        this.m_powerQueue.Enqueue(taskList);
        if (taskList.HasFriendlyConcede())
        {
            this.m_earlyConcedeTaskList = taskList;
        }
        if (taskList.HasGameOver())
        {
            this.m_gameOverTaskList = taskList;
        }
    }

    private string FindRevealedCardId(PowerTaskList taskList)
    {
        Network.HistActionStart sourceAction = taskList.GetSourceAction();
        List<PowerTask> list = taskList.GetTaskList();
        for (int i = 0; i < list.Count; i++)
        {
            PowerTask task = list[i];
            Network.HistShowEntity power = task.GetPower() as Network.HistShowEntity;
            if ((power != null) && (power.Entity.ID == sourceAction.Entity))
            {
                return power.Entity.CardID;
            }
        }
        return null;
    }

    private void ForceEarlyConcedeVisuals()
    {
        Player friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
        if (friendlySidePlayer != null)
        {
            friendlySidePlayer.PlayConcedeEmote();
        }
    }

    public void ForEachTaskList(Action<int, PowerTaskList> predicate)
    {
        if (this.m_currentTaskList != null)
        {
            predicate(-1, this.m_currentTaskList);
        }
        for (int i = 0; i < this.m_powerQueue.Count; i++)
        {
            predicate(i, this.m_powerQueue[i]);
        }
    }

    public PowerTaskList GetCurrentTaskList()
    {
        return this.m_currentTaskList;
    }

    public PowerTaskList GetEarlyConcedeTaskList()
    {
        return this.m_earlyConcedeTaskList;
    }

    public PowerTaskList GetGameOverTaskList()
    {
        return this.m_gameOverTaskList;
    }

    public PowerTaskList GetLastTaskList()
    {
        int count = this.m_powerQueue.Count;
        if (count > 0)
        {
            return this.m_powerQueue[count - 1];
        }
        return this.m_currentTaskList;
    }

    public PowerTaskList GetLatestUnendedTaskList()
    {
        int count = this.m_powerQueue.Count;
        if (count == 0)
        {
            return this.m_currentTaskList;
        }
        return this.m_powerQueue[count - 1];
    }

    private int GetNextTaskListId()
    {
        int nextTaskListId = this.m_nextTaskListId;
        this.m_nextTaskListId = (this.m_nextTaskListId != 0x7fffffff) ? (this.m_nextTaskListId + 1) : 1;
        return nextTaskListId;
    }

    public PowerQueue GetPowerQueue()
    {
        return this.m_powerQueue;
    }

    public bool HasEarlyConcedeTaskList()
    {
        return (this.m_earlyConcedeTaskList != null);
    }

    public bool HasGameOverTaskList()
    {
        return (this.m_gameOverTaskList != null);
    }

    public bool HasTaskList(PowerTaskList taskList)
    {
        if (taskList == null)
        {
            return false;
        }
        return ((this.m_currentTaskList == taskList) || this.m_powerQueue.Contains(taskList));
    }

    public bool HasTaskLists()
    {
        return ((this.m_currentTaskList != null) || (this.m_powerQueue.Count > 0));
    }

    public bool IsBuildingTaskList()
    {
        return this.m_buildingTaskList;
    }

    public bool IsBusy()
    {
        return (this.m_buildingTaskList || ((this.m_currentTaskList != null) || this.m_historyBlocking));
    }

    private void NotifyWillProcessTaskList(PowerTaskList taskList)
    {
        if (ThinkEmoteManager.Get() != null)
        {
            ThinkEmoteManager.Get().NotifyOfActivity();
        }
        if (taskList.IsSourceActionOrigin())
        {
            Network.HistActionStart sourceAction = taskList.GetSourceAction();
            if (sourceAction.BlockType == HistoryBlock.Type.PLAY)
            {
                Entity entity = GameState.Get().GetEntity(sourceAction.Entity);
                if (entity.GetController().IsOpposingSide())
                {
                    string cardId = entity.GetCardId();
                    if (string.IsNullOrEmpty(cardId))
                    {
                        cardId = this.FindRevealedCardId(taskList);
                    }
                    GameState.Get().GetGameEntity().NotifyOfOpponentWillPlayCard(cardId);
                }
            }
        }
    }

    private void OnBigCardFinished()
    {
        this.m_historyBlocking = false;
    }

    public void OnPowerHistory(List<Network.PowerHistory> powerList)
    {
        this.m_buildingTaskList = true;
        for (int i = 0; i < powerList.Count; i++)
        {
            PowerTaskList item = new PowerTaskList();
            if (this.m_previousStack.Count > 0)
            {
                item.SetPrevious(this.m_previousStack.Pop());
                this.m_previousStack.Push(item);
            }
            this.BuildTaskList(powerList, ref i, item);
        }
        this.m_buildingTaskList = false;
    }

    private void OnSpellControllerFinished(SpellController spellController)
    {
        this.DestroySpellController(spellController);
    }

    private void OnSpellControllerFinishedTaskList(SpellController spellController)
    {
        spellController.DetachPowerTaskList();
        if (this.m_currentTaskList != null)
        {
            this.DoCurrentTaskList();
        }
    }

    private void PrepareHistoryForCurrentTaskList()
    {
        object[] args = new object[] { this.m_currentTaskList.GetId() };
        Log.Power.Print("PowerProcessor.PrepareHistoryForCurrentTaskList() - m_currentTaskList={0}", args);
        Network.HistActionStart sourceAction = this.m_currentTaskList.GetSourceAction();
        if (sourceAction != null)
        {
            if (sourceAction.BlockType != HistoryBlock.Type.ATTACK)
            {
                if (sourceAction.BlockType == HistoryBlock.Type.PLAY)
                {
                    Entity playedEntity = GameState.Get().GetEntity(sourceAction.Entity);
                    if (playedEntity != null)
                    {
                        if (this.m_currentTaskList.IsSourceActionOrigin())
                        {
                            Entity targetedEntity = GameState.Get().GetEntity(sourceAction.Target);
                            HistoryManager.Get().CreateCardPlayedTile(playedEntity, targetedEntity);
                            this.m_currentTaskList.SetSpawnedHistoryTile(true);
                            if (playedEntity.IsControlledByFriendlySidePlayer())
                            {
                                GameState.Get().GetGameEntity().NotifyOfFriendlyPlayedCard(playedEntity);
                            }
                            if (this.ShouldShowPlayedBigCard(playedEntity))
                            {
                                if (playedEntity.IsControlledByOpposingSidePlayer())
                                {
                                    GameState.Get().GetGameEntity().NotifyOfOpponentPlayedCard(playedEntity);
                                }
                                bool wasCountered = this.m_currentTaskList.WasThePlayedSpellCountered(playedEntity);
                                this.m_historyBlocking = true;
                                this.m_historyBlockingTaskList = this.m_currentTaskList;
                                HistoryManager.Get().CreatePlayedBigCard(playedEntity, new HistoryManager.FinishedCallback(this.OnBigCardFinished), wasCountered);
                            }
                        }
                        this.m_currentTaskList.NotifyHistoryOfAdditionalTargets();
                    }
                }
                else if (sourceAction.BlockType == HistoryBlock.Type.POWER)
                {
                    this.m_currentTaskList.NotifyHistoryOfAdditionalTargets();
                }
                else if (sourceAction.BlockType == HistoryBlock.Type.JOUST)
                {
                    this.m_currentTaskList.NotifyHistoryOfAdditionalTargets();
                }
                else if (sourceAction.BlockType == HistoryBlock.Type.TRIGGER)
                {
                    Entity triggeredEntity = GameState.Get().GetEntity(sourceAction.Entity);
                    if (triggeredEntity != null)
                    {
                        if (triggeredEntity.IsSecret())
                        {
                            if (this.m_currentTaskList.IsSourceActionOrigin())
                            {
                                HistoryManager.Get().CreateTriggerTile(triggeredEntity);
                                this.m_currentTaskList.SetSpawnedHistoryTile(true);
                                this.m_historyBlocking = true;
                                this.m_historyBlockingTaskList = this.m_currentTaskList;
                                HistoryManager.Get().CreateSecretBigCard(triggeredEntity, new HistoryManager.FinishedCallback(this.OnBigCardFinished));
                            }
                            this.m_currentTaskList.NotifyHistoryOfAdditionalTargets();
                        }
                        else
                        {
                            if (this.m_currentTaskList.IsSourceActionOrigin())
                            {
                                PowerHistoryInfo powerHistoryInfo = triggeredEntity.GetPowerHistoryInfo(sourceAction.Index);
                                if ((powerHistoryInfo != null) && powerHistoryInfo.ShouldShowInHistory())
                                {
                                    if (triggeredEntity.HasTag(GAME_TAG.HISTORY_PROXY))
                                    {
                                        Entity entity6 = GameState.Get().GetEntity(triggeredEntity.GetTag(GAME_TAG.HISTORY_PROXY));
                                        HistoryManager.Get().CreateCardPlayedTile(entity6, null);
                                        if ((triggeredEntity.GetController() != GameState.Get().GetFriendlySidePlayer()) || !triggeredEntity.HasTag(GAME_TAG.HISTORY_PROXY_NO_BIG_CARD))
                                        {
                                            this.m_historyBlocking = true;
                                            this.m_historyBlockingTaskList = this.m_currentTaskList;
                                            HistoryManager.Get().CreateTriggeredBigCard(entity6, new HistoryManager.FinishedCallback(this.OnBigCardFinished));
                                        }
                                    }
                                    else
                                    {
                                        if (this.ShouldShowTriggeredBigCard(triggeredEntity))
                                        {
                                            this.m_historyBlocking = true;
                                            this.m_historyBlockingTaskList = this.m_currentTaskList;
                                            HistoryManager.Get().CreateTriggeredBigCard(triggeredEntity, new HistoryManager.FinishedCallback(this.OnBigCardFinished));
                                        }
                                        HistoryManager.Get().CreateTriggerTile(triggeredEntity);
                                    }
                                    this.m_currentTaskList.SetSpawnedHistoryTile(true);
                                }
                            }
                            if (this.m_currentTaskList.DidBlockSpawnHistoryTile())
                            {
                                this.m_currentTaskList.NotifyHistoryOfAdditionalTargets();
                            }
                        }
                    }
                }
                else if (sourceAction.BlockType == HistoryBlock.Type.FATIGUE)
                {
                    if (this.m_currentTaskList.IsSourceActionOrigin())
                    {
                        HistoryManager.Get().CreateFatigueTile();
                        this.m_currentTaskList.SetSpawnedHistoryTile(true);
                    }
                    this.m_currentTaskList.NotifyHistoryOfAdditionalTargets();
                }
            }
            else
            {
                AttackType attackType = this.m_currentTaskList.GetAttackType();
                Entity attacker = null;
                Entity defender = null;
                switch (attackType)
                {
                    case AttackType.REGULAR:
                        attacker = this.m_currentTaskList.GetAttacker();
                        defender = this.m_currentTaskList.GetDefender();
                        break;

                    case AttackType.CANCELED:
                        attacker = this.m_currentTaskList.GetAttacker();
                        defender = this.m_currentTaskList.GetProposedDefender();
                        break;
                }
                if ((attacker != null) && (defender != null))
                {
                    HistoryManager.Get().CreateAttackTile(attacker, defender, this.m_currentTaskList);
                    this.m_currentTaskList.SetSpawnedHistoryTile(true);
                    this.m_currentTaskList.NotifyHistoryOfAdditionalTargets();
                }
            }
        }
    }

    public void ProcessPowerQueue()
    {
        while (((this.m_powerQueue.Count > 0) && !this.IsBusy()) && !GameState.Get().IsBusy())
        {
            if (this.m_busyTaskList != null)
            {
                this.m_busyTaskList = null;
            }
            else
            {
                PowerTaskList taskList = this.m_powerQueue.Peek();
                if (this.m_historyBlockingTaskList != null)
                {
                    if (!taskList.IsDescendantOfBlock(this.m_historyBlockingTaskList))
                    {
                        break;
                    }
                }
                else if ((HistoryManager.Get() != null) && (HistoryManager.Get().GetCurrentBigCard() != null))
                {
                    break;
                }
                this.NotifyWillProcessTaskList(taskList);
                if (GameState.Get().IsBusy())
                {
                    this.m_busyTaskList = taskList;
                    break;
                }
            }
            if (this.CanEarlyConcede())
            {
                if ((this.m_earlyConcedeTaskList == null) && !this.m_handledFirstEarlyConcede)
                {
                    this.ForceEarlyConcedeVisuals();
                    this.m_handledFirstEarlyConcede = true;
                }
                while (this.m_powerQueue.Count > 0)
                {
                    this.m_currentTaskList = this.m_powerQueue.Dequeue();
                    this.m_currentTaskList.DebugDump();
                    this.CancelSpellsForEarlyConcede(this.m_currentTaskList);
                    this.m_currentTaskList.DoEarlyConcedeTasks();
                    this.m_currentTaskList = null;
                }
                break;
            }
            this.m_currentTaskList = this.m_powerQueue.Dequeue();
            this.m_currentTaskList.DebugDump();
            this.PrepareHistoryForCurrentTaskList();
            this.StartCurrentTaskList();
        }
    }

    private bool ShouldShowPlayedBigCard(Entity sourceEntity)
    {
        return (GameMgr.Get().IsSpectator() || sourceEntity.IsControlledByOpposingSidePlayer());
    }

    private bool ShouldShowTriggeredBigCard(Entity sourceEntity)
    {
        if (sourceEntity.GetZone() != TAG_ZONE.HAND)
        {
            return false;
        }
        if (!sourceEntity.HasTriggerVisual())
        {
            return false;
        }
        return true;
    }

    private void StartCurrentTaskList()
    {
        GameState state = GameState.Get();
        Network.HistActionStart sourceAction = this.m_currentTaskList.GetSourceAction();
        if (sourceAction == null)
        {
            this.DoCurrentTaskList();
        }
        else
        {
            int id = sourceAction.Entity;
            Entity sourceEntity = state.GetEntity(id);
            if (sourceEntity == null)
            {
                Debug.LogError(string.Format("PowerProcessor.StartCurrentTaskList() - WARNING got a power with a null source entity (ID={0})", id));
                this.DoCurrentTaskList();
            }
            else if (!this.DoTaskListWithSpellController(state, this.m_currentTaskList, sourceEntity))
            {
                this.DoCurrentTaskList();
            }
        }
    }
}

