using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class PowerTaskList
{
    private bool m_attackDataBuilt;
    private Entity m_attacker;
    private AttackInfo m_attackInfo;
    private AttackType m_attackType;
    private Entity m_defender;
    private Network.HistActionEnd m_endAction;
    private int m_id;
    private PowerTaskList m_next;
    private PowerTaskList m_parent;
    private PowerTaskList m_previous;
    private Entity m_proposedDefender;
    private Network.HistActionStart m_sourceAction;
    private bool m_spawnedHistoryTile;
    private List<PowerTask> m_tasks = new List<PowerTask>();
    private ZoneChangeList m_zoneChangeList;

    public bool AreTasksComplete()
    {
        foreach (PowerTask task in this.m_tasks)
        {
            if (!task.IsCompleted())
            {
                return false;
            }
        }
        return true;
    }

    public bool AreZoneChangesComplete()
    {
        return ((this.m_zoneChangeList == null) || this.m_zoneChangeList.IsComplete());
    }

    private void BuildAttackData()
    {
        if (!this.m_attackDataBuilt)
        {
            AttackInfo info;
            this.m_attackInfo = this.BuildAttackInfo();
            this.m_attackType = this.DetermineAttackType(out info);
            this.m_attacker = null;
            this.m_defender = null;
            this.m_proposedDefender = null;
            switch (this.m_attackType)
            {
                case AttackType.REGULAR:
                    this.m_attacker = info.m_attacker;
                    this.m_defender = info.m_defender;
                    break;

                case AttackType.PROPOSED:
                    this.m_attacker = info.m_proposedAttacker;
                    this.m_defender = info.m_proposedDefender;
                    this.m_proposedDefender = info.m_proposedDefender;
                    break;

                case AttackType.CANCELED:
                    this.m_attacker = this.m_previous.GetAttacker();
                    this.m_proposedDefender = this.m_previous.GetProposedDefender();
                    break;

                case AttackType.ONLY_ATTACKER:
                    this.m_attacker = info.m_attacker;
                    break;

                case AttackType.ONLY_DEFENDER:
                    this.m_defender = info.m_defender;
                    break;

                case AttackType.ONLY_PROPOSED_ATTACKER:
                    this.m_attacker = info.m_proposedAttacker;
                    break;

                case AttackType.ONLY_PROPOSED_DEFENDER:
                    this.m_proposedDefender = info.m_proposedDefender;
                    this.m_defender = info.m_proposedDefender;
                    break;

                case AttackType.WAITING_ON_PROPOSED_ATTACKER:
                case AttackType.WAITING_ON_PROPOSED_DEFENDER:
                case AttackType.WAITING_ON_ATTACKER:
                case AttackType.WAITING_ON_DEFENDER:
                    this.m_attacker = this.m_previous.GetAttacker();
                    this.m_defender = this.m_previous.GetDefender();
                    break;
            }
            this.m_attackDataBuilt = true;
        }
    }

    private AttackInfo BuildAttackInfo()
    {
        GameState state = GameState.Get();
        AttackInfo info = new AttackInfo();
        bool flag = false;
        foreach (PowerTask task in this.GetTaskList())
        {
            Network.PowerHistory power = task.GetPower();
            if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange change = power as Network.HistTagChange;
                if (change.Tag == 0x24)
                {
                    info.m_defenderTagValue = new int?(change.Value);
                    if (change.Value == 1)
                    {
                        info.m_defender = state.GetEntity(change.Entity);
                    }
                    flag = true;
                }
                else if (change.Tag == 0x26)
                {
                    info.m_attackerTagValue = new int?(change.Value);
                    if (change.Value == 1)
                    {
                        info.m_attacker = state.GetEntity(change.Entity);
                    }
                    flag = true;
                }
                else if (change.Tag == 0x27)
                {
                    info.m_proposedAttackerTagValue = new int?(change.Value);
                    if (change.Value != 0)
                    {
                        info.m_proposedAttacker = state.GetEntity(change.Value);
                    }
                    flag = true;
                }
                else if (change.Tag == 0x25)
                {
                    info.m_proposedDefenderTagValue = new int?(change.Value);
                    if (change.Value != 0)
                    {
                        info.m_proposedDefender = state.GetEntity(change.Value);
                    }
                    flag = true;
                }
            }
        }
        if (flag)
        {
            return info;
        }
        return null;
    }

    public PowerTask CreateTask(Network.PowerHistory netPower)
    {
        PowerTask item = new PowerTask();
        item.SetPower(netPower);
        this.m_tasks.Add(item);
        return item;
    }

    public void DebugDump()
    {
        this.DebugDump(Log.Power);
    }

    public void DebugDump(Logger logger)
    {
        if (logger.CanPrint())
        {
            GameState state = GameState.Get();
            string indentation = string.Empty;
            int num = (this.m_parent != null) ? this.m_parent.GetId() : 0;
            int num2 = (this.m_previous != null) ? this.m_previous.GetId() : 0;
            object[] args = new object[] { this.m_id, num, num2, this.m_tasks.Count };
            logger.Print("PowerTaskList.DebugDump() - ID={0} ParentID={1} PreviousID={2} TaskCount={3}", args);
            if (this.m_sourceAction == null)
            {
                object[] objArray2 = new object[] { indentation };
                logger.Print("PowerTaskList.DebugDump() - {0}Source Action=(null)", objArray2);
                indentation = indentation + "    ";
            }
            else
            {
                state.DebugPrintPower(logger, "PowerTaskList", this.m_sourceAction, ref indentation);
            }
            for (int i = 0; i < this.m_tasks.Count; i++)
            {
                Network.PowerHistory power = this.m_tasks[i].GetPower();
                state.DebugPrintPower(logger, "PowerTaskList", power, ref indentation);
            }
            if (this.m_endAction == null)
            {
                if (indentation.Length >= "    ".Length)
                {
                    indentation = indentation.Remove(indentation.Length - "    ".Length);
                }
                object[] objArray3 = new object[] { indentation };
                logger.Print("PowerTaskList.DebugDump() - {0}End Action=(null)", objArray3);
            }
            else
            {
                state.DebugPrintPower(logger, "PowerTaskList", this.m_endAction, ref indentation);
            }
        }
    }

    private AttackType DetermineAttackType(out AttackInfo info)
    {
        info = this.m_attackInfo;
        GameState state = GameState.Get();
        GameEntity gameEntity = state.GetGameEntity();
        Entity entity = state.GetEntity(gameEntity.GetTag(GAME_TAG.PROPOSED_ATTACKER));
        Entity entity3 = state.GetEntity(gameEntity.GetTag(GAME_TAG.PROPOSED_DEFENDER));
        AttackType iNVALID = AttackType.INVALID;
        Entity attacker = null;
        Entity defender = null;
        if (this.m_previous != null)
        {
            iNVALID = this.m_previous.GetAttackType();
            attacker = this.m_previous.GetAttacker();
            defender = this.m_previous.GetDefender();
        }
        if (this.m_attackInfo != null)
        {
            if ((this.m_attackInfo.m_attacker != null) || (this.m_attackInfo.m_defender != null))
            {
                if (this.m_attackInfo.m_attacker == null)
                {
                    if ((iNVALID != AttackType.ONLY_ATTACKER) && (iNVALID != AttackType.WAITING_ON_DEFENDER))
                    {
                        return AttackType.ONLY_DEFENDER;
                    }
                    info = new AttackInfo();
                    info.m_attacker = attacker;
                    info.m_defender = this.m_attackInfo.m_defender;
                    return AttackType.REGULAR;
                }
                if (this.m_attackInfo.m_defender == null)
                {
                    if ((iNVALID != AttackType.ONLY_DEFENDER) && (iNVALID != AttackType.WAITING_ON_ATTACKER))
                    {
                        return AttackType.ONLY_ATTACKER;
                    }
                    info = new AttackInfo();
                    info.m_attacker = this.m_attackInfo.m_attacker;
                    info.m_defender = defender;
                }
                return AttackType.REGULAR;
            }
            if ((this.m_attackInfo.m_proposedAttacker != null) || (this.m_attackInfo.m_proposedDefender != null))
            {
                if (this.m_attackInfo.m_proposedAttacker == null)
                {
                    if (entity != null)
                    {
                        info = new AttackInfo();
                        info.m_proposedAttacker = entity;
                        info.m_proposedDefender = this.m_attackInfo.m_proposedDefender;
                        return AttackType.PROPOSED;
                    }
                    return AttackType.ONLY_PROPOSED_DEFENDER;
                }
                if (this.m_attackInfo.m_proposedDefender != null)
                {
                    return AttackType.PROPOSED;
                }
                if (entity3 != null)
                {
                    info = new AttackInfo();
                    info.m_proposedAttacker = this.m_attackInfo.m_proposedAttacker;
                    info.m_proposedDefender = entity3;
                    return AttackType.PROPOSED;
                }
                return AttackType.ONLY_PROPOSED_ATTACKER;
            }
            switch (iNVALID)
            {
                case AttackType.REGULAR:
                case AttackType.INVALID:
                    return AttackType.INVALID;
            }
        }
        if (iNVALID == AttackType.PROPOSED)
        {
            if (((entity == null) || (entity.GetZone() == TAG_ZONE.PLAY)) && ((entity3 == null) || (entity3.GetZone() == TAG_ZONE.PLAY)))
            {
                if ((attacker != entity) || (defender != entity3))
                {
                    info = new AttackInfo();
                    info.m_proposedAttacker = entity;
                    info.m_proposedDefender = entity3;
                    return AttackType.PROPOSED;
                }
                if (((entity != null) && (entity3 != null)) && !this.IsEndOfBlock())
                {
                    info = new AttackInfo();
                    info.m_proposedAttacker = entity;
                    info.m_proposedDefender = entity3;
                    return AttackType.PROPOSED;
                }
            }
            return AttackType.CANCELED;
        }
        if (iNVALID == AttackType.CANCELED)
        {
            return AttackType.INVALID;
        }
        if (this.IsEndOfBlock())
        {
            switch (iNVALID)
            {
                case AttackType.ONLY_ATTACKER:
                case AttackType.WAITING_ON_DEFENDER:
                    return AttackType.CANCELED;
            }
            object[] args = new object[] { iNVALID, attacker, defender };
            UnityEngine.Debug.LogWarningFormat("AttackSpellController.DetermineAttackType() - INVALID ATTACK prevAttackType={0} prevAttacker={1} prevDefender={2}", args);
            return AttackType.INVALID;
        }
        switch (iNVALID)
        {
            case AttackType.ONLY_PROPOSED_ATTACKER:
            case AttackType.WAITING_ON_PROPOSED_DEFENDER:
                return AttackType.WAITING_ON_PROPOSED_DEFENDER;

            case AttackType.ONLY_PROPOSED_DEFENDER:
            case AttackType.WAITING_ON_PROPOSED_ATTACKER:
                return AttackType.WAITING_ON_PROPOSED_ATTACKER;

            case AttackType.ONLY_ATTACKER:
            case AttackType.WAITING_ON_DEFENDER:
                return AttackType.WAITING_ON_DEFENDER;
        }
        if ((iNVALID != AttackType.ONLY_DEFENDER) && (iNVALID != AttackType.WAITING_ON_ATTACKER))
        {
            return AttackType.INVALID;
        }
        return AttackType.WAITING_ON_ATTACKER;
    }

    public bool DidBlockSpawnHistoryTile()
    {
        for (PowerTaskList list = this.GetOrigin(); list != null; list = list.m_next)
        {
            if (list.DidSpawnHistoryTile())
            {
                return true;
            }
        }
        return false;
    }

    public bool DidSpawnHistoryTile()
    {
        return this.m_spawnedHistoryTile;
    }

    public void DoAllTasks()
    {
        this.DoTasks(0, this.m_tasks.Count, null, null);
    }

    public void DoAllTasks(CompleteCallback callback)
    {
        this.DoTasks(0, this.m_tasks.Count, callback, null);
    }

    public void DoAllTasks(CompleteCallback callback, object userData)
    {
        this.DoTasks(0, this.m_tasks.Count, callback, userData);
    }

    public void DoEarlyConcedeTasks()
    {
        for (int i = 0; i < this.m_tasks.Count; i++)
        {
            this.m_tasks[i].DoEarlyConcedeTask();
        }
    }

    public bool DoesBlockHaveEndAction()
    {
        return (this.GetLast().m_endAction != null);
    }

    public bool DoesBlockHaveMetaDataTasks()
    {
        for (PowerTaskList list = this.GetOrigin(); list != null; list = list.m_next)
        {
            if (list.HasMetaDataTasks())
            {
                return true;
            }
        }
        return false;
    }

    public bool DoesBlockHaveMetaDataTasks(HistoryMeta.Type metaType)
    {
        for (PowerTaskList list = this.GetOrigin(); list != null; list = list.m_next)
        {
            if (list.HasMetaDataTasks(metaType))
            {
                return true;
            }
        }
        return false;
    }

    public void DoTasks(int startIndex, int count)
    {
        this.DoTasks(startIndex, count, null, null);
    }

    public void DoTasks(int startIndex, int count, CompleteCallback callback)
    {
        this.DoTasks(startIndex, count, callback, null);
    }

    public void DoTasks(int startIndex, int count, CompleteCallback callback, object userData)
    {
        bool flag = false;
        int taskStartIndex = -1;
        int taskEndIndex = Mathf.Min((int) ((startIndex + count) - 1), (int) (this.m_tasks.Count - 1));
        for (int i = startIndex; i <= taskEndIndex; i++)
        {
            PowerTask task = this.m_tasks[i];
            if (!task.IsCompleted())
            {
                if (taskStartIndex < 0)
                {
                    taskStartIndex = i;
                }
                if (ZoneMgr.IsHandledPower(task.GetPower()))
                {
                    flag = true;
                    break;
                }
            }
        }
        if (taskStartIndex < 0)
        {
            taskStartIndex = startIndex;
        }
        if (flag)
        {
            ZoneChangeCallbackData data = new ZoneChangeCallbackData {
                m_startIndex = startIndex,
                m_count = count,
                m_taskListCallback = callback,
                m_taskListUserData = userData
            };
            this.m_zoneChangeList = ZoneMgr.Get().AddServerZoneChanges(this, taskStartIndex, taskEndIndex, new ZoneMgr.ChangeCompleteCallback(this.OnZoneChangeComplete), data);
            if (this.m_zoneChangeList != null)
            {
                return;
            }
        }
        if (Gameplay.Get() != null)
        {
            Gameplay.Get().StartCoroutine(this.WaitForGameStateAndDoTasks(taskStartIndex, taskEndIndex, startIndex, count, callback, userData));
        }
        else
        {
            this.DoTasks(taskStartIndex, taskEndIndex, startIndex, count, callback, userData);
        }
    }

    private void DoTasks(int incompleteStartIndex, int endIndex, int startIndex, int count, CompleteCallback callback, object userData)
    {
        for (int i = incompleteStartIndex; i <= endIndex; i++)
        {
            this.m_tasks[i].DoTask();
        }
        if (callback != null)
        {
            callback(this, startIndex, count, userData);
        }
    }

    public int FindEarlierIncompleteTaskIndex(int taskIndex)
    {
        for (int i = taskIndex - 1; i >= 0; i--)
        {
            PowerTask task = this.m_tasks[i];
            if (!task.IsCompleted())
            {
                return i;
            }
        }
        return -1;
    }

    public Entity GetAttacker()
    {
        this.BuildAttackData();
        return this.m_attacker;
    }

    public AttackInfo GetAttackInfo()
    {
        this.BuildAttackData();
        return this.m_attackInfo;
    }

    public AttackType GetAttackType()
    {
        this.BuildAttackData();
        return this.m_attackType;
    }

    public DamageInfo GetDamageInfo(Entity entity)
    {
        if (entity != null)
        {
            int entityId = entity.GetEntityId();
            foreach (PowerTask task in this.m_tasks)
            {
                Network.PowerHistory power = task.GetPower();
                if (power.Type == Network.PowerType.TAG_CHANGE)
                {
                    Network.HistTagChange change = power as Network.HistTagChange;
                    if ((change.Tag == 0x2c) && (change.Entity == entityId))
                    {
                        DamageInfo info;
                        return new DamageInfo { m_entity = GameState.Get().GetEntity(change.Entity), m_damage = change.Value - info.m_entity.GetDamage() };
                    }
                }
            }
        }
        return null;
    }

    public Entity GetDefender()
    {
        this.BuildAttackData();
        return this.m_defender;
    }

    public Network.HistActionEnd GetEndAction()
    {
        return this.m_endAction;
    }

    public int GetId()
    {
        return this.m_id;
    }

    public PowerTaskList GetLast()
    {
        PowerTaskList next = this;
        while (next.m_next != null)
        {
            next = next.m_next;
        }
        return next;
    }

    public PowerTaskList GetNext()
    {
        return this.m_next;
    }

    public PowerTaskList GetOrigin()
    {
        PowerTaskList previous = this;
        while (previous.m_previous != null)
        {
            previous = previous.m_previous;
        }
        return previous;
    }

    public PowerTaskList GetParent()
    {
        return this.GetOrigin().m_parent;
    }

    public PowerTaskList GetPrevious()
    {
        return this.m_previous;
    }

    public Entity GetProposedDefender()
    {
        this.BuildAttackData();
        return this.m_proposedDefender;
    }

    public Network.HistActionStart GetSourceAction()
    {
        return this.GetOrigin().m_sourceAction;
    }

    public Entity GetSourceEntity()
    {
        Network.HistActionStart sourceAction = this.GetSourceAction();
        if (sourceAction == null)
        {
            return null;
        }
        int id = sourceAction.Entity;
        Entity entity = GameState.Get().GetEntity(id);
        if (entity == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("PowerProcessor.GetSourceEntity() - task list {0} has a source entity with id {1} but there is no entity with that id", this.m_id, id));
            return null;
        }
        return entity;
    }

    public Entity GetTargetEntity()
    {
        Network.HistActionStart sourceAction = this.GetSourceAction();
        if (sourceAction == null)
        {
            return null;
        }
        int target = sourceAction.Target;
        Entity entity = GameState.Get().GetEntity(target);
        if (entity == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("PowerProcessor.GetSourceEntity() - task list {0} has a target entity with id {1} but there is no entity with that id", this.m_id, target));
            return null;
        }
        return entity;
    }

    public List<PowerTask> GetTaskList()
    {
        return this.m_tasks;
    }

    public bool HasEarlierIncompleteTask(int taskIndex)
    {
        return (this.FindEarlierIncompleteTaskIndex(taskIndex) >= 0);
    }

    public bool HasFriendlyConcede()
    {
        for (int i = 0; i < this.m_tasks.Count; i++)
        {
            Network.PowerHistory power = this.m_tasks[i].GetPower();
            if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange tagChange = (Network.HistTagChange) power;
                if (GameUtils.IsFriendlyConcede(tagChange))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasGameOver()
    {
        for (int i = 0; i < this.m_tasks.Count; i++)
        {
            Network.PowerHistory power = this.m_tasks[i].GetPower();
            if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange change = (Network.HistTagChange) power;
                if (GameUtils.IsGameOverTag(change.Entity, change.Tag, change.Value))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasMetaDataTasks()
    {
        foreach (PowerTask task in this.m_tasks)
        {
            if (task.GetPower().Type == Network.PowerType.META_DATA)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasMetaDataTasks(HistoryMeta.Type metaType)
    {
        foreach (PowerTask task in this.m_tasks)
        {
            Network.HistMetaData power = task.GetPower() as Network.HistMetaData;
            if ((power != null) && (power.MetaType == metaType))
            {
                return true;
            }
        }
        return false;
    }

    public bool HasTargetEntity()
    {
        Network.HistActionStart sourceAction = this.GetSourceAction();
        if (sourceAction == null)
        {
            return false;
        }
        int target = sourceAction.Target;
        return (GameState.Get().GetEntity(target) != null);
    }

    public bool HasTasks()
    {
        return (this.m_tasks.Count > 0);
    }

    public bool HasTasksOfType(Network.PowerType powType)
    {
        foreach (PowerTask task in this.m_tasks)
        {
            if (task.GetPower().Type == powType)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasZoneChanges()
    {
        return (this.m_zoneChangeList != null);
    }

    public bool IsBlock()
    {
        return (this.GetOrigin().m_sourceAction != null);
    }

    public bool IsBlockUnended()
    {
        if (!this.IsBlock())
        {
            return false;
        }
        if (this.DoesBlockHaveEndAction())
        {
            return false;
        }
        return true;
    }

    public bool IsComplete()
    {
        if (!this.AreTasksComplete())
        {
            return false;
        }
        if (!this.AreZoneChangesComplete())
        {
            return false;
        }
        return true;
    }

    public bool IsDescendantOfBlock(PowerTaskList taskList)
    {
        if (taskList != null)
        {
            if (this.IsInBlock(taskList))
            {
                return true;
            }
            PowerTaskList origin = taskList.GetOrigin();
            for (PowerTaskList list2 = this.GetParent(); list2 != null; list2 = list2.m_parent)
            {
                if (list2 == origin)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsEarlierInBlockThan(PowerTaskList taskList)
    {
        if (taskList != null)
        {
            for (PowerTaskList list = taskList.m_previous; list != null; list = list.m_previous)
            {
                if (this == list)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsEmpty()
    {
        PowerTaskList origin = this.GetOrigin();
        if (origin.m_sourceAction != null)
        {
            return false;
        }
        if (origin.m_endAction != null)
        {
            return false;
        }
        if (origin.m_tasks.Count > 0)
        {
            return false;
        }
        return true;
    }

    public bool IsEndOfBlock()
    {
        if (!this.IsBlock())
        {
            return false;
        }
        return (this.m_endAction != null);
    }

    public bool IsInBlock(PowerTaskList taskList)
    {
        return ((this == taskList) || (this.IsEarlierInBlockThan(taskList) || this.IsLaterInBlockThan(taskList)));
    }

    public bool IsLaterInBlockThan(PowerTaskList taskList)
    {
        if (taskList != null)
        {
            for (PowerTaskList list = taskList.m_next; list != null; list = list.m_next)
            {
                if (this == list)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsOrigin()
    {
        return (this.m_previous == null);
    }

    public bool IsSourceActionOrigin()
    {
        return (this.m_sourceAction != null);
    }

    public bool IsSourceOfBlock()
    {
        if (!this.IsBlock())
        {
            return false;
        }
        return (this.m_sourceAction != null);
    }

    public bool IsTaskPartOfMetaData(int taskIndex, HistoryMeta.Type metaType)
    {
        for (int i = taskIndex; i >= 0; i--)
        {
            Network.PowerHistory power = this.m_tasks[i].GetPower();
            if (power.Type == Network.PowerType.META_DATA)
            {
                Network.HistMetaData data = (Network.HistMetaData) power;
                if (data.MetaType == metaType)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void NotifyHistoryOfAdditionalTargets()
    {
        List<int> list = new List<int>();
        bool flag = true;
        foreach (PowerTask task in this.GetTaskList())
        {
            Network.PowerHistory power = task.GetPower();
            if (power.Type == Network.PowerType.META_DATA)
            {
                Network.HistMetaData data = power as Network.HistMetaData;
                if (data.MetaType == HistoryMeta.Type.TARGET)
                {
                    if ((data.Info != null) && (data.Info.Count > 0))
                    {
                        for (int i = 0; i < data.Info.Count; i++)
                        {
                            HistoryManager.Get().NotifyAboutAdditionalTarget(data.Info[i]);
                        }
                    }
                }
                else if ((data.MetaType == HistoryMeta.Type.DAMAGE) || (data.MetaType == HistoryMeta.Type.HEALING))
                {
                    flag = false;
                }
                continue;
            }
            if (power.Type == Network.PowerType.SHOW_ENTITY)
            {
                Network.HistShowEntity entity = power as Network.HistShowEntity;
                bool flag2 = false;
                bool flag3 = false;
                foreach (Network.Entity.Tag tag in entity.Entity.Tags)
                {
                    if ((tag.Name == 0xca) && (tag.Value == 6))
                    {
                        flag2 = true;
                        break;
                    }
                    if ((tag.Name == 0x31) && (tag.Value == 4))
                    {
                        flag3 = true;
                    }
                }
                if (!flag2)
                {
                    if (flag3)
                    {
                        HistoryManager.Get().NotifyAboutCardDeath(GameState.Get().GetEntity(entity.Entity.ID));
                    }
                    else
                    {
                        HistoryManager.Get().NotifyAboutAdditionalTarget(entity.Entity.ID);
                    }
                }
                continue;
            }
            Entity sourceEntity = this.GetSourceEntity();
            int num2 = (sourceEntity != null) ? sourceEntity.GetEntityId() : 0;
            if (power.Type == Network.PowerType.FULL_ENTITY)
            {
                Network.HistFullEntity entity3 = power as Network.HistFullEntity;
                bool flag4 = false;
                bool flag5 = false;
                bool flag6 = false;
                foreach (Network.Entity.Tag tag2 in entity3.Entity.Tags)
                {
                    if ((tag2.Name == 0xca) && (tag2.Value == 6))
                    {
                        flag4 = true;
                        break;
                    }
                    if ((tag2.Name == 0x31) && ((tag2.Value == 1) || (tag2.Value == 7)))
                    {
                        flag5 = true;
                    }
                    if ((tag2.Name == 0x181) && (tag2.Value == num2))
                    {
                        flag6 = true;
                    }
                }
                if (!flag4 && (flag5 || flag6))
                {
                    HistoryManager.Get().NotifyAboutAdditionalTarget(entity3.Entity.ID);
                }
                continue;
            }
            if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange tagChange = power as Network.HistTagChange;
                Entity entity4 = GameState.Get().GetEntity(tagChange.Entity);
                if (tagChange.Tag == 0x2c)
                {
                    if (!list.Contains(tagChange.Entity) && !flag)
                    {
                        HistoryManager.Get().NotifyAboutDamageChanged(entity4, tagChange.Value);
                        flag = true;
                    }
                }
                else if (tagChange.Tag == 0x124)
                {
                    if (!list.Contains(tagChange.Entity))
                    {
                        HistoryManager.Get().NotifyAboutArmorChanged(entity4, tagChange.Value);
                    }
                }
                else if (tagChange.Tag == 0x13e)
                {
                    HistoryManager.Get().NotifyAboutPreDamage(entity4);
                }
                else if ((tagChange.Tag == 0x181) && (tagChange.Value == num2))
                {
                    HistoryManager.Get().NotifyAboutAdditionalTarget(tagChange.Entity);
                }
                else if (tagChange.Tag == 0x106)
                {
                    HistoryManager.Get().NotifyAboutAdditionalTarget(tagChange.Entity);
                }
                if (GameUtils.IsHistoryDeathTagChange(tagChange))
                {
                    HistoryManager.Get().NotifyAboutCardDeath(entity4);
                    list.Add(tagChange.Entity);
                }
            }
        }
    }

    private void OnZoneChangeComplete(ZoneChangeList changeList, object userData)
    {
        ZoneChangeCallbackData data = (ZoneChangeCallbackData) userData;
        if (data.m_taskListCallback != null)
        {
            data.m_taskListCallback(this, data.m_startIndex, data.m_count, data.m_taskListUserData);
        }
    }

    public void SetEndAction(Network.HistActionEnd endAction)
    {
        this.m_endAction = endAction;
    }

    public void SetId(int id)
    {
        this.m_id = id;
    }

    public void SetParent(PowerTaskList parent)
    {
        this.m_parent = parent;
    }

    public void SetPrevious(PowerTaskList taskList)
    {
        this.m_previous = taskList;
        taskList.m_next = this;
    }

    public void SetSourceAction(Network.HistActionStart startAction)
    {
        this.m_sourceAction = startAction;
    }

    public void SetSpawnedHistoryTile(bool set)
    {
        this.m_spawnedHistoryTile = set;
    }

    public override string ToString()
    {
        object[] args = new object[] { this.m_id, this.m_tasks.Count, (this.m_previous != null) ? this.m_previous.GetId() : 0, (this.m_next != null) ? this.m_next.GetId() : 0, (this.m_parent != null) ? this.m_parent.GetId() : 0 };
        return string.Format("id={0} tasks={1} prevId={2} nextId={3} parentId={4}", args);
    }

    [DebuggerHidden]
    private IEnumerator WaitForGameStateAndDoTasks(int incompleteStartIndex, int endIndex, int startIndex, int count, CompleteCallback callback, object userData)
    {
        return new <WaitForGameStateAndDoTasks>c__IteratorC4 { incompleteStartIndex = incompleteStartIndex, endIndex = endIndex, callback = callback, startIndex = startIndex, count = count, userData = userData, <$>incompleteStartIndex = incompleteStartIndex, <$>endIndex = endIndex, <$>callback = callback, <$>startIndex = startIndex, <$>count = count, <$>userData = userData, <>f__this = this };
    }

    public bool WasThePlayedSpellCountered(Entity entity)
    {
        foreach (PowerTask task in this.m_tasks)
        {
            Network.PowerHistory power = task.GetPower();
            if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange change = power as Network.HistTagChange;
                if (((change.Entity == entity.GetEntityId()) && (change.Tag == 0xe7)) && (change.Value == 1))
                {
                    return true;
                }
            }
        }
        foreach (PowerTaskList list2 in GameState.Get().GetPowerProcessor().GetPowerQueue().GetList())
        {
            foreach (PowerTask task2 in list2.GetTaskList())
            {
                Network.PowerHistory history2 = task2.GetPower();
                if (history2.Type == Network.PowerType.TAG_CHANGE)
                {
                    Network.HistTagChange change2 = history2 as Network.HistTagChange;
                    if (((change2.Entity == entity.GetEntityId()) && (change2.Tag == 0xe7)) && (change2.Value == 1))
                    {
                        return true;
                    }
                }
            }
            if ((list2.GetEndAction() != null) && (list2.GetSourceAction().BlockType == HistoryBlock.Type.PLAY))
            {
                return false;
            }
        }
        return false;
    }

    [CompilerGenerated]
    private sealed class <WaitForGameStateAndDoTasks>c__IteratorC4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PowerTaskList.CompleteCallback <$>callback;
        internal int <$>count;
        internal int <$>endIndex;
        internal int <$>incompleteStartIndex;
        internal int <$>startIndex;
        internal object <$>userData;
        internal PowerTaskList <>f__this;
        internal int <i>__0;
        internal PowerTask <task>__1;
        internal PowerTaskList.CompleteCallback callback;
        internal int count;
        internal int endIndex;
        internal int incompleteStartIndex;
        internal int startIndex;
        internal object userData;

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
                    this.<i>__0 = this.incompleteStartIndex;
                    goto Label_00B9;

                case 1:
                    break;

                case 2:
                    goto Label_009C;

                default:
                    goto Label_00FF;
            }
        Label_0075:
            while (GameState.Get().IsBusy())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0101;
            }
        Label_009C:
            while (GameState.Get().IsMulliganBlockingPowers())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0101;
            }
            this.<i>__0++;
        Label_00B9:
            if (this.<i>__0 <= this.endIndex)
            {
                this.<task>__1 = this.<>f__this.m_tasks[this.<i>__0];
                this.<task>__1.DoTask();
                goto Label_0075;
            }
            if (this.callback != null)
            {
                this.callback(this.<>f__this, this.startIndex, this.count, this.userData);
            }
            this.$PC = -1;
        Label_00FF:
            return false;
        Label_0101:
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

    public delegate void CompleteCallback(PowerTaskList taskList, int startIndex, int count, object userData);

    public class DamageInfo
    {
        public int m_damage;
        public Entity m_entity;
    }

    private class ZoneChangeCallbackData
    {
        public int m_count;
        public int m_startIndex;
        public PowerTaskList.CompleteCallback m_taskListCallback;
        public object m_taskListUserData;
    }
}

