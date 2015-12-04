using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class SuperSpell : Spell
{
    public SpellActionInfo m_ActionInfo;
    public SpellChainInfo m_ChainInfo;
    protected int m_currentTargetIndex;
    protected int m_effectsPendingFinish;
    public SpellAreaEffectInfo m_FriendlyAreaEffectInfo;
    public SpellImpactInfo m_ImpactInfo;
    public bool m_MakeClones = true;
    public SpellMissileInfo m_MissileInfo;
    public SpellAreaEffectInfo m_OpponentAreaEffectInfo;
    protected bool m_pendingNoneStateChange;
    protected bool m_pendingSpellFinish;
    protected bool m_settingUpAction;
    public SpellStartInfo m_StartInfo;
    protected Spell m_startSpell;
    public SpellTargetInfo m_TargetInfo = new SpellTargetInfo();
    protected Map<int, int> m_targetToMetaDataMap = new Map<int, int>();
    protected List<GameObject> m_visualTargets = new List<GameObject>();
    protected Map<int, int> m_visualToTargetIndexMap = new Map<int, int>();

    private void AddAllTargetsAsVisualTargets()
    {
        for (int i = 0; i < base.m_targets.Count; i++)
        {
            int count = this.m_visualTargets.Count;
            this.m_visualToTargetIndexMap[count] = i;
            this.AddVisualTarget(base.m_targets[i]);
        }
    }

    private void AddChosenTargetAsVisualTarget()
    {
        Card powerTargetCard = base.GetPowerTargetCard();
        if (powerTargetCard == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.AddChosenTargetAsVisualTarget() - there is no chosen target", this));
        }
        else
        {
            this.AddVisualTarget(powerTargetCard.gameObject);
        }
    }

    public override bool AddPowerTargets()
    {
        this.m_visualToTargetIndexMap.Clear();
        this.m_targetToMetaDataMap.Clear();
        if (!base.CanAddPowerTargets())
        {
            return false;
        }
        if (this.HasChain() && !this.AddPrimaryChainTarget())
        {
            return false;
        }
        if (!base.AddMultiplePowerTargets())
        {
            return false;
        }
        if (base.m_targets.Count <= 0)
        {
            Network.HistActionStart sourceAction = base.m_taskList.GetSourceAction();
            if ((sourceAction != null) && (sourceAction.Target != 0))
            {
                return base.AddSinglePowerTarget_FromSourceAction(sourceAction);
            }
        }
        return true;
    }

    private bool AddPrimaryChainTarget()
    {
        Network.HistActionStart sourceAction = base.m_taskList.GetSourceAction();
        if (sourceAction == null)
        {
            return false;
        }
        return base.AddSinglePowerTarget_FromSourceAction(sourceAction);
    }

    protected override void AddTargetFromMetaData(int metaDataIndex, Card targetCard)
    {
        int count = base.m_targets.Count;
        this.m_targetToMetaDataMap[count] = metaDataIndex;
        this.AddTarget(targetCard.gameObject);
    }

    public override void AddVisualTarget(GameObject go)
    {
        this.m_visualTargets.Add(go);
    }

    public override void AddVisualTargets(List<GameObject> targets)
    {
        this.m_visualTargets.AddRange(targets);
    }

    protected bool AreEffectsActive()
    {
        return (this.m_effectsPendingFinish > 0);
    }

    private bool CheckAndWaitForGameEventsThenDoAction()
    {
        if (base.m_taskList != null)
        {
            if (this.m_ActionInfo.m_ShowSpellVisuals == SpellVisualShowTime.DURING_GAME_EVENTS)
            {
                return this.DoActionDuringGameEvents();
            }
            if (this.m_ActionInfo.m_ShowSpellVisuals == SpellVisualShowTime.AFTER_GAME_EVENTS)
            {
                this.DoActionAfterGameEvents();
                return true;
            }
        }
        return false;
    }

    private bool CheckAndWaitForStartDelayThenDoAction()
    {
        if (Mathf.Min(this.m_ActionInfo.m_StartDelayMax, this.m_ActionInfo.m_StartDelayMin) <= Mathf.Epsilon)
        {
            return false;
        }
        this.m_effectsPendingFinish++;
        base.StartCoroutine(this.WaitForStartDelayThenDoAction());
        return true;
    }

    private bool CheckAndWaitForStartPrefabThenDoAction()
    {
        if (!this.HasStart())
        {
            return false;
        }
        if ((this.m_startSpell != null) && (this.m_startSpell.GetActiveState() == SpellStateType.IDLE))
        {
            return false;
        }
        if (this.m_startSpell == null)
        {
            this.SpawnStart();
        }
        this.m_startSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnStartSpellBirthStateFinished));
        if (this.m_startSpell.GetActiveState() != SpellStateType.BIRTH)
        {
            this.m_startSpell.SafeActivateState(SpellStateType.BIRTH);
            if (this.m_startSpell.GetActiveState() == SpellStateType.NONE)
            {
                this.m_startSpell = null;
                return false;
            }
        }
        return true;
    }

    protected virtual void CleanUp()
    {
        foreach (GameObject obj2 in this.m_visualTargets)
        {
            if (obj2.GetComponent<SpellGeneratedTarget>() != null)
            {
                UnityEngine.Object.Destroy(obj2);
            }
        }
        this.m_visualTargets.Clear();
    }

    protected Spell CloneSpell(Spell prefab)
    {
        Spell spell;
        if (this.IsMakingClones())
        {
            spell = UnityEngine.Object.Instantiate<Spell>(prefab);
            spell.AddStateStartedCallback(new Spell.StateStartedCallback(this.OnCloneSpellStateStarted));
            spell.transform.parent = base.transform;
        }
        else
        {
            spell = prefab;
            spell.RemoveAllTargets();
        }
        spell.AddFinishedCallback(new Spell.FinishedCallback(this.OnCloneSpellFinished));
        return spell;
    }

    [DebuggerHidden]
    protected IEnumerator CompleteTasksFromMetaData(int metaDataIndex, float delaySec)
    {
        return new <CompleteTasksFromMetaData>c__Iterator203 { delaySec = delaySec, metaDataIndex = metaDataIndex, <$>delaySec = delaySec, <$>metaDataIndex = metaDataIndex, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator CompleteTasksUntilMetaData(int metaDataIndex)
    {
        return new <CompleteTasksUntilMetaData>c__Iterator201 { metaDataIndex = metaDataIndex, <$>metaDataIndex = metaDataIndex, <>f__this = this };
    }

    private float ComputeBoxPickChance(int[] boxUsageCounts, int index)
    {
        int num = boxUsageCounts[index];
        float num3 = boxUsageCounts.Length * 0.25f;
        float num4 = ((float) num) / num3;
        return (1f - num4);
    }

    private SpellAreaEffectInfo DetermineAreaEffectInfo()
    {
        Card sourceCard = base.GetSourceCard();
        if (sourceCard != null)
        {
            Player controller = sourceCard.GetController();
            if (controller != null)
            {
                if (controller.IsFriendlySide() && this.HasFriendlyAreaEffect())
                {
                    return this.m_FriendlyAreaEffectInfo;
                }
                if (!controller.IsFriendlySide() && this.HasOpponentAreaEffect())
                {
                    return this.m_OpponentAreaEffectInfo;
                }
            }
        }
        if (this.HasFriendlyAreaEffect())
        {
            return this.m_FriendlyAreaEffectInfo;
        }
        if (this.HasOpponentAreaEffect())
        {
            return this.m_OpponentAreaEffectInfo;
        }
        return null;
    }

    private Spell DetermineImpactPrefab(GameObject targetObject)
    {
        if (this.m_ImpactInfo.m_DamageAmountImpacts.Length == 0)
        {
            return this.m_ImpactInfo.m_Prefab;
        }
        Spell prefab = this.m_ImpactInfo.m_DamageAmountImpacts[0].m_Prefab;
        if (base.m_taskList != null)
        {
            Card component = targetObject.GetComponent<Card>();
            if (component == null)
            {
                return prefab;
            }
            PowerTaskList.DamageInfo damageInfo = base.m_taskList.GetDamageInfo(component.GetEntity());
            if (damageInfo == null)
            {
                return prefab;
            }
            foreach (SpellImpactPrefabs prefabs in this.m_ImpactInfo.m_DamageAmountImpacts)
            {
                if ((damageInfo.m_damage >= prefabs.m_MinDamage) && (damageInfo.m_damage <= prefabs.m_MaxDamage))
                {
                    prefab = prefabs.m_Prefab;
                }
            }
        }
        return prefab;
    }

    protected QueueList<PowerTask> DetermineTasksToWaitFor(int startIndex, int count)
    {
        if (count == 0)
        {
            return null;
        }
        int num = startIndex + count;
        QueueList<PowerTask> list = new QueueList<PowerTask>();
        List<PowerTask> taskList = base.m_taskList.GetTaskList();
        for (int i = startIndex; i < num; i++)
        {
            <DetermineTasksToWaitFor>c__AnonStorey34D storeyd = new <DetermineTasksToWaitFor>c__AnonStorey34D();
            PowerTask task = taskList[i];
            storeyd.entity = this.GetEntityFromZoneChangePowerTask(task);
            if ((storeyd.entity != null) && (this.m_visualTargets.Find(new Predicate<GameObject>(storeyd.<>m__1B0)) != null))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    PowerTask task2 = list[j];
                    Entity entityFromZoneChangePowerTask = this.GetEntityFromZoneChangePowerTask(task2);
                    if (storeyd.entity == entityFromZoneChangePowerTask)
                    {
                        list.RemoveAt(j);
                        break;
                    }
                }
                list.Enqueue(task);
            }
        }
        return list;
    }

    private void DoAction()
    {
        if ((!this.CheckAndWaitForGameEventsThenDoAction() && !this.CheckAndWaitForStartDelayThenDoAction()) && !this.CheckAndWaitForStartPrefabThenDoAction())
        {
            this.DoActionNow();
        }
    }

    private void DoActionAfterGameEvents()
    {
        this.m_effectsPendingFinish++;
        PowerTaskList.CompleteCallback callback = delegate (PowerTaskList taskList, int startIndex, int count, object userData) {
            this.m_effectsPendingFinish--;
            if (!this.CheckAndWaitForStartDelayThenDoAction() && !this.CheckAndWaitForStartPrefabThenDoAction())
            {
                this.DoActionNow();
            }
        };
        base.m_taskList.DoAllTasks(callback);
    }

    private bool DoActionDuringGameEvents()
    {
        base.m_taskList.DoAllTasks();
        if (base.m_taskList.IsComplete())
        {
            return false;
        }
        QueueList<PowerTask> tasksToWaitFor = this.DetermineTasksToWaitFor(0, base.m_taskList.GetTaskList().Count);
        if (tasksToWaitFor.Count == 0)
        {
            return false;
        }
        base.StartCoroutine(this.DoDelayedActionDuringGameEvents(tasksToWaitFor));
        return true;
    }

    protected virtual void DoActionNow()
    {
        SpellAreaEffectInfo info = this.DetermineAreaEffectInfo();
        if (info != null)
        {
            this.SpawnAreaEffect(info);
        }
        bool flag = this.HasMissile();
        bool flag2 = this.HasImpact();
        bool flag3 = this.HasChain();
        if ((this.GetVisualTargetCount() > 0) && ((flag || flag2) || flag3))
        {
            if (flag)
            {
                if (flag3)
                {
                    this.SpawnChainMissile();
                }
                else if (this.m_MissileInfo.m_SpawnInSequence)
                {
                    this.SpawnMissileInSequence();
                }
                else
                {
                    this.SpawnAllMissiles();
                }
            }
            else
            {
                if (flag2)
                {
                    if (flag3)
                    {
                        this.SpawnImpact(this.m_currentTargetIndex);
                    }
                    else
                    {
                        this.SpawnAllImpacts();
                    }
                }
                if (flag3)
                {
                    this.SpawnChain();
                }
                this.DoStartSpellAction();
            }
        }
        else
        {
            this.DoStartSpellAction();
        }
        this.FinishIfPossible();
    }

    [DebuggerHidden]
    private IEnumerator DoDelayedActionDuringGameEvents(QueueList<PowerTask> tasksToWaitFor)
    {
        return new <DoDelayedActionDuringGameEvents>c__Iterator1FA { tasksToWaitFor = tasksToWaitFor, <$>tasksToWaitFor = tasksToWaitFor, <>f__this = this };
    }

    private void DoStartSpellAction()
    {
        if (this.m_startSpell != null)
        {
            if (!this.m_startSpell.HasUsableState(SpellStateType.ACTION))
            {
                this.m_startSpell.UpdateTransform();
                this.m_startSpell.SafeActivateState(SpellStateType.DEATH);
            }
            else
            {
                this.m_startSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnStartSpellActionFinished));
                this.m_startSpell.ActivateState(SpellStateType.ACTION);
            }
            this.m_startSpell = null;
        }
    }

    protected void FinishIfPossible()
    {
        if (!this.m_settingUpAction && !this.AreEffectsActive())
        {
            if (this.m_pendingSpellFinish)
            {
                this.OnSpellFinished();
                this.m_pendingSpellFinish = false;
            }
            if (this.m_pendingNoneStateChange)
            {
                this.OnStateFinished();
                this.m_pendingNoneStateChange = false;
            }
            this.CleanUp();
        }
    }

    private void FireMissileOnPath(Spell missile, int targetIndex, bool reverse)
    {
        Vector3[] vectorArray = this.GenerateMissilePath(missile);
        float num = UnityEngine.Random.Range(this.m_MissileInfo.m_PathDurationMin, this.m_MissileInfo.m_PathDurationMax);
        object[] args = new object[] { "path", vectorArray, "time", num, "easetype", this.m_MissileInfo.m_PathEaseType, "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        if (reverse)
        {
            hashtable.Add("oncomplete", "OnReverseMissileTargetReached");
            hashtable.Add("oncompleteparams", missile);
        }
        else
        {
            object[] objArray2 = new object[] { "missile", missile, "targetIndex", targetIndex };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            hashtable.Add("oncomplete", "OnMissileTargetReached");
            hashtable.Add("oncompleteparams", hashtable2);
        }
        if (!object.Equals(vectorArray[0], vectorArray[2]))
        {
            hashtable.Add("orienttopath", this.m_MissileInfo.m_OrientToPath);
        }
        if (this.m_MissileInfo.m_TargetJoint.Length > 0)
        {
            GameObject target = SceneUtils.FindChildBySubstring(missile.gameObject, this.m_MissileInfo.m_TargetJoint);
            if (target != null)
            {
                missile.transform.LookAt(missile.GetTarget().transform, this.m_MissileInfo.m_JointUpVector);
                vectorArray[2].y += this.m_MissileInfo.m_TargetHeightOffset;
                iTween.MoveTo(target, hashtable);
                return;
            }
        }
        iTween.MoveTo(missile.gameObject, hashtable);
    }

    private void GenerateCreatedCardsTargets()
    {
        foreach (PowerTask task in base.m_taskList.GetTaskList())
        {
            Network.PowerHistory power = task.GetPower();
            if (power.Type == Network.PowerType.FULL_ENTITY)
            {
                Network.HistFullEntity entity = power as Network.HistFullEntity;
                int iD = entity.Entity.ID;
                Entity entity2 = GameState.Get().GetEntity(iD);
                if (entity2.GetTag(GAME_TAG.ZONE) != 6)
                {
                    if (entity2 == null)
                    {
                        UnityEngine.Debug.LogWarning(string.Format("{0}.GenerateCreatedCardsTargets() - WARNING trying to target entity with id {1} but there is no entity with that id", this, iD));
                    }
                    else
                    {
                        Card card = entity2.GetCard();
                        if (card == null)
                        {
                            UnityEngine.Debug.LogWarning(string.Format("{0}.GenerateCreatedCardsTargets() - WARNING trying to target entity.GetCard() with id {1} but there is no card with that id", this, iD));
                        }
                        else
                        {
                            this.m_visualTargets.Add(card.gameObject);
                        }
                    }
                }
            }
        }
    }

    private Vector3[] GenerateMissilePath(Spell missile)
    {
        Vector3[] path = new Vector3[3];
        path[0] = missile.transform.position;
        path[2] = missile.GetTarget().transform.position;
        path[1] = this.GenerateMissilePathCenterPoint(path);
        return path;
    }

    private Vector3 GenerateMissilePathCenterPoint(Vector3[] path)
    {
        Vector3 vector = path[0];
        Vector3 vector2 = path[2];
        Vector3 vector3 = vector2 - vector;
        float magnitude = vector3.magnitude;
        Vector3 vector4 = vector;
        bool flag = magnitude <= Mathf.Epsilon;
        if (!flag)
        {
            vector4 = vector + ((Vector3) (vector3 * (this.m_MissileInfo.m_CenterOffsetPercent * 0.01f)));
        }
        float num2 = magnitude / this.m_MissileInfo.m_DistanceScaleFactor;
        if (flag)
        {
            if ((this.m_MissileInfo.m_CenterPointHeightMin <= Mathf.Epsilon) && (this.m_MissileInfo.m_CenterPointHeightMax <= Mathf.Epsilon))
            {
                vector4.y += 2f;
            }
            else
            {
                vector4.y += UnityEngine.Random.Range(this.m_MissileInfo.m_CenterPointHeightMin, this.m_MissileInfo.m_CenterPointHeightMax);
            }
        }
        else
        {
            vector4.y += num2 * UnityEngine.Random.Range(this.m_MissileInfo.m_CenterPointHeightMin, this.m_MissileInfo.m_CenterPointHeightMax);
        }
        float num3 = 1f;
        if (vector.z > vector2.z)
        {
            num3 = -1f;
        }
        bool flag2 = GeneralUtils.RandomBool();
        if ((this.m_MissileInfo.m_RightMin == 0f) && (this.m_MissileInfo.m_RightMax == 0f))
        {
            flag2 = false;
        }
        if ((this.m_MissileInfo.m_LeftMin == 0f) && (this.m_MissileInfo.m_LeftMax == 0f))
        {
            flag2 = true;
        }
        if (flag2)
        {
            if ((this.m_MissileInfo.m_RightMin == this.m_MissileInfo.m_RightMax) || this.m_MissileInfo.m_DebugForceMax)
            {
                vector4.x += (this.m_MissileInfo.m_RightMax * num2) * num3;
                return vector4;
            }
            vector4.x += UnityEngine.Random.Range((float) (this.m_MissileInfo.m_RightMin * num2), (float) (this.m_MissileInfo.m_RightMax * num2)) * num3;
            return vector4;
        }
        if ((this.m_MissileInfo.m_LeftMin == this.m_MissileInfo.m_LeftMax) || this.m_MissileInfo.m_DebugForceMax)
        {
            vector4.x -= (this.m_MissileInfo.m_LeftMax * num2) * num3;
            return vector4;
        }
        vector4.x -= UnityEngine.Random.Range((float) (this.m_MissileInfo.m_LeftMin * num2), (float) (this.m_MissileInfo.m_LeftMax * num2)) * num3;
        return vector4;
    }

    protected void GenerateRandomBoardVisualTargets()
    {
        ZonePlay play = SpellUtils.FindFriendlyPlayZone(this);
        ZonePlay play2 = SpellUtils.FindOpponentPlayZone(this);
        Bounds bounds = play.GetComponent<Collider>().bounds;
        Bounds bounds2 = play2.GetComponent<Collider>().bounds;
        Vector3 vector = Vector3.Min(bounds.min, bounds2.min);
        Vector3 vector2 = Vector3.Max(bounds.max, bounds2.max);
        Vector3 center = (Vector3) (0.5f * (vector2 + vector));
        Vector3 vector4 = vector2 - vector;
        float x = Mathf.Abs(vector4.x);
        float y = Mathf.Abs(vector4.y);
        Vector3 size = new Vector3(x, y, Mathf.Abs(vector4.z));
        Bounds bounds3 = new Bounds(center, size);
        this.GenerateRandomVisualTargets(bounds3);
    }

    protected void GenerateRandomPlayZoneVisualTargets(ZonePlay zonePlay)
    {
        this.GenerateRandomVisualTargets(zonePlay.GetComponent<Collider>().bounds);
    }

    private void GenerateRandomVisualTargets(Bounds bounds)
    {
        int num = UnityEngine.Random.Range(this.m_TargetInfo.m_RandomTargetCountMin, this.m_TargetInfo.m_RandomTargetCountMax + 1);
        if (num != 0)
        {
            float x = bounds.min.x;
            float z = bounds.max.z;
            float min = bounds.min.z;
            float num5 = bounds.size.x / ((float) num);
            int[] boxUsageCounts = new int[num];
            int[] numArray2 = new int[num];
            for (int i = 0; i < num; i++)
            {
                boxUsageCounts[i] = 0;
                numArray2[i] = -1;
            }
            for (int j = 0; j < num; j++)
            {
                float num8 = UnityEngine.Random.Range((float) 0f, (float) 1f);
                int max = 0;
                for (int k = 0; k < num; k++)
                {
                    if (this.ComputeBoxPickChance(boxUsageCounts, k) >= num8)
                    {
                        numArray2[max++] = k;
                    }
                }
                int index = numArray2[UnityEngine.Random.Range(0, max)];
                boxUsageCounts[index]++;
                float num13 = x + (index * num5);
                float num14 = num13 + num5;
                Vector3 position = new Vector3 {
                    x = UnityEngine.Random.Range(num13, num14),
                    y = bounds.center.y,
                    z = UnityEngine.Random.Range(min, z)
                };
                this.GenerateVisualTarget(position, j, index);
            }
        }
    }

    private void GenerateVisualTarget(Vector3 position, int index, int boxIndex)
    {
        GameObject go = new GameObject {
            name = string.Format("{0} Target {1} (box {2})", this, index, boxIndex)
        };
        go.transform.position = position;
        go.AddComponent<SpellGeneratedTarget>();
        this.AddVisualTarget(go);
    }

    private Entity GetEntityFromZoneChangePowerTask(PowerTask task)
    {
        Entity entity;
        int num;
        this.GetZoneChangeFromPowerTask(task, out entity, out num);
        return entity;
    }

    protected int GetMetaDataIndexForTarget(int visualTargetIndex)
    {
        int num;
        int num2;
        if (!this.m_visualToTargetIndexMap.TryGetValue(visualTargetIndex, out num))
        {
            return -1;
        }
        if (!this.m_targetToMetaDataMap.TryGetValue(num, out num2))
        {
            return -1;
        }
        return num2;
    }

    private GameObject GetPrimaryTarget()
    {
        return this.m_visualTargets[this.GetPrimaryTargetIndex()];
    }

    private int GetPrimaryTargetIndex()
    {
        return 0;
    }

    public override GameObject GetVisualTarget()
    {
        return ((this.m_visualTargets.Count != 0) ? this.m_visualTargets[0] : null);
    }

    public override Card GetVisualTargetCard()
    {
        GameObject visualTarget = this.GetVisualTarget();
        if (visualTarget == null)
        {
            return null;
        }
        return visualTarget.GetComponent<Card>();
    }

    private int GetVisualTargetCount()
    {
        if (this.IsMakingClones())
        {
            return this.m_visualTargets.Count;
        }
        return Mathf.Min(1, this.m_visualTargets.Count);
    }

    public override List<GameObject> GetVisualTargets()
    {
        return this.m_visualTargets;
    }

    private bool GetZoneChangeFromPowerTask(PowerTask task, out Entity entity, out int zoneTag)
    {
        entity = null;
        zoneTag = 0;
        Entity entity2 = null;
        Network.PowerHistory power = task.GetPower();
        switch (power.Type)
        {
            case Network.PowerType.FULL_ENTITY:
            {
                Network.HistFullEntity entity3 = (Network.HistFullEntity) power;
                entity2 = GameState.Get().GetEntity(entity3.Entity.ID);
                if (entity2.GetCard() != null)
                {
                    foreach (Network.Entity.Tag tag in entity3.Entity.Tags)
                    {
                        if (tag.Name == 0x31)
                        {
                            entity = entity2;
                            zoneTag = tag.Value;
                            return true;
                        }
                    }
                    break;
                }
                return false;
            }
            case Network.PowerType.SHOW_ENTITY:
            {
                Network.HistShowEntity entity4 = (Network.HistShowEntity) power;
                entity2 = GameState.Get().GetEntity(entity4.Entity.ID);
                if (entity2.GetCard() != null)
                {
                    foreach (Network.Entity.Tag tag2 in entity4.Entity.Tags)
                    {
                        if (tag2.Name == 0x31)
                        {
                            entity = entity2;
                            zoneTag = tag2.Value;
                            return true;
                        }
                    }
                    break;
                }
                return false;
            }
            case Network.PowerType.TAG_CHANGE:
            {
                Network.HistTagChange change = (Network.HistTagChange) power;
                entity2 = GameState.Get().GetEntity(change.Entity);
                if (entity2.GetCard() != null)
                {
                    if (change.Tag == 0x31)
                    {
                        entity = entity2;
                        zoneTag = change.Value;
                        return true;
                    }
                    break;
                }
                return false;
            }
        }
        return false;
    }

    private bool HasAreaEffect()
    {
        return (this.HasFriendlyAreaEffect() || this.HasOpponentAreaEffect());
    }

    private bool HasChain()
    {
        return (((this.m_ChainInfo != null) && this.m_ChainInfo.m_Enabled) && (this.m_ChainInfo.m_Prefab != null));
    }

    private bool HasFriendlyAreaEffect()
    {
        return (((this.m_FriendlyAreaEffectInfo != null) && this.m_FriendlyAreaEffectInfo.m_Enabled) && (this.m_FriendlyAreaEffectInfo.m_Prefab != null));
    }

    private bool HasImpact()
    {
        return (((this.m_ImpactInfo != null) && this.m_ImpactInfo.m_Enabled) && ((this.m_ImpactInfo.m_Prefab != null) || (this.m_ImpactInfo.m_DamageAmountImpacts.Length > 0)));
    }

    protected bool HasMetaDataTargets()
    {
        return (this.m_targetToMetaDataMap.Count > 0);
    }

    private bool HasMissile()
    {
        return (((this.m_MissileInfo != null) && this.m_MissileInfo.m_Enabled) && ((this.m_MissileInfo.m_Prefab != null) || (this.m_MissileInfo.m_ReversePrefab != null)));
    }

    private bool HasOpponentAreaEffect()
    {
        return (((this.m_OpponentAreaEffectInfo != null) && this.m_OpponentAreaEffectInfo.m_Enabled) && (this.m_OpponentAreaEffectInfo.m_Prefab != null));
    }

    private bool HasStart()
    {
        return (((this.m_StartInfo != null) && this.m_StartInfo.m_Enabled) && (this.m_StartInfo.m_Prefab != null));
    }

    protected bool IsMakingClones()
    {
        return true;
    }

    public override bool IsVisualTarget(GameObject go)
    {
        return this.m_visualTargets.Contains(go);
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        this.m_settingUpAction = true;
        this.UpdateVisualTargets();
        base.UpdatePosition();
        base.UpdateOrientation();
        this.m_currentTargetIndex = this.GetPrimaryTargetIndex();
        this.UpdatePendingStateChangeFlags(SpellStateType.ACTION);
        this.DoAction();
        base.OnAction(prevStateType);
        this.m_settingUpAction = false;
        this.FinishIfPossible();
    }

    protected override void OnBirth(SpellStateType prevStateType)
    {
        base.UpdatePosition();
        base.UpdateOrientation();
        this.m_currentTargetIndex = 0;
        if (this.HasStart())
        {
            this.SpawnStart();
            this.m_startSpell.SafeActivateState(SpellStateType.BIRTH);
            if (this.m_startSpell.GetActiveState() == SpellStateType.NONE)
            {
                this.m_startSpell = null;
            }
        }
        base.OnBirth(prevStateType);
    }

    protected override void OnCancel(SpellStateType prevStateType)
    {
        this.UpdatePendingStateChangeFlags(SpellStateType.CANCEL);
        if (this.m_startSpell != null)
        {
            this.m_startSpell.SafeActivateState(SpellStateType.CANCEL);
            this.m_startSpell = null;
        }
        base.OnCancel(prevStateType);
        this.FinishIfPossible();
    }

    private void OnCloneSpellFinished(Spell spell, object userData)
    {
        this.m_effectsPendingFinish--;
        this.FinishIfPossible();
    }

    private void OnCloneSpellStateStarted(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            UnityEngine.Object.Destroy(spell.gameObject);
        }
    }

    public override void OnFsmStateStarted(FsmState state, SpellStateType stateType)
    {
        if (base.m_activeStateChange != stateType)
        {
            if ((stateType == SpellStateType.NONE) && this.AreEffectsActive())
            {
                this.m_pendingSpellFinish = true;
                this.m_pendingNoneStateChange = true;
            }
            else
            {
                base.OnFsmStateStarted(state, stateType);
            }
        }
    }

    protected void OnMetaDataTasksComplete(PowerTaskList taskList, int startIndex, int count, object userData)
    {
        this.m_effectsPendingFinish--;
        this.FinishIfPossible();
    }

    private void OnMissileSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (prevStateType == SpellStateType.BIRTH)
        {
            spell.RemoveStateFinishedCallback(new Spell.StateFinishedCallback(this.OnMissileSpellStateFinished), userData);
            int targetIndex = (int) userData;
            bool reverse = targetIndex < 0;
            this.FireMissileOnPath(spell, targetIndex, reverse);
        }
    }

    private void OnMissileTargetReached(Hashtable args)
    {
        Spell spell = (Spell) args["missile"];
        int targetIndex = (int) args["targetIndex"];
        if (this.HasImpact())
        {
            this.SpawnImpact(targetIndex);
        }
        if (this.HasChain())
        {
            this.SpawnChain();
        }
        else if (this.m_MissileInfo.m_SpawnInSequence)
        {
            this.SpawnMissileInSequence();
        }
        spell.ActivateState(SpellStateType.DEATH);
    }

    private void OnReverseMissileTargetReached(Spell missile)
    {
        missile.ActivateState(SpellStateType.DEATH);
    }

    public override void OnSpellFinished()
    {
        if (this.AreEffectsActive())
        {
            this.m_pendingSpellFinish = true;
        }
        else
        {
            base.OnSpellFinished();
        }
    }

    private void OnStartSpellActionFinished(Spell spell, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.ACTION)
        {
            spell.SafeActivateState(SpellStateType.DEATH);
        }
    }

    private void OnStartSpellBirthStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (prevStateType == SpellStateType.BIRTH)
        {
            spell.RemoveStateFinishedCallback(new Spell.StateFinishedCallback(this.OnStartSpellBirthStateFinished), userData);
            this.DoActionNow();
        }
    }

    public override void OnStateFinished()
    {
        if ((base.GuessNextStateType() == SpellStateType.NONE) && this.AreEffectsActive())
        {
            this.m_pendingNoneStateChange = true;
        }
        else
        {
            base.OnStateFinished();
        }
    }

    public override void RemoveAllVisualTargets()
    {
        this.m_visualTargets.Clear();
    }

    public override bool RemoveVisualTarget(GameObject go)
    {
        return this.m_visualTargets.Remove(go);
    }

    protected bool ShouldCompleteTasksUntilMetaData(int metaDataIndex)
    {
        if (!base.m_taskList.HasEarlierIncompleteTask(metaDataIndex))
        {
            return false;
        }
        return true;
    }

    private void SpawnAllImpacts()
    {
        for (int i = 0; i < this.GetVisualTargetCount(); i++)
        {
            this.SpawnImpact(i);
        }
    }

    private void SpawnAllMissiles()
    {
        for (int i = 0; i < this.GetVisualTargetCount(); i++)
        {
            this.SpawnMissile(i);
        }
        this.DoStartSpellAction();
    }

    private void SpawnAreaEffect(SpellAreaEffectInfo info)
    {
        this.m_effectsPendingFinish++;
        base.StartCoroutine(this.WaitAndSpawnAreaEffect(info));
    }

    private void SpawnChain()
    {
        if (this.GetVisualTargetCount() > 1)
        {
            this.m_effectsPendingFinish++;
            base.StartCoroutine(this.WaitAndSpawnChain());
        }
    }

    private void SpawnChainMissile()
    {
        this.SpawnMissile(this.GetPrimaryTargetIndex());
        this.DoStartSpellAction();
    }

    private void SpawnImpact(int targetIndex)
    {
        this.m_effectsPendingFinish++;
        base.StartCoroutine(this.WaitAndSpawnImpact(targetIndex));
    }

    private void SpawnMissile(int targetIndex)
    {
        base.StartCoroutine(this.WaitAndSpawnMissile(targetIndex));
    }

    private void SpawnMissileInSequence()
    {
        if (this.m_currentTargetIndex < this.GetVisualTargetCount())
        {
            this.SpawnMissile(this.m_currentTargetIndex);
            this.m_currentTargetIndex++;
            if (this.m_startSpell != null)
            {
                if (this.m_StartInfo.m_DeathAfterAllMissilesFire)
                {
                    if (this.m_currentTargetIndex < this.GetVisualTargetCount())
                    {
                        if (this.m_startSpell.HasUsableState(SpellStateType.ACTION))
                        {
                            this.m_startSpell.ActivateState(SpellStateType.ACTION);
                        }
                    }
                    else
                    {
                        this.DoStartSpellAction();
                    }
                }
                else
                {
                    this.DoStartSpellAction();
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator SpawnReverseMissile(Spell cloneSpell, GameObject sourceObject, GameObject targetObject, float delay)
    {
        return new <SpawnReverseMissile>c__Iterator1FD { delay = delay, cloneSpell = cloneSpell, sourceObject = sourceObject, targetObject = targetObject, <$>delay = delay, <$>cloneSpell = cloneSpell, <$>sourceObject = sourceObject, <$>targetObject = targetObject, <>f__this = this };
    }

    private void SpawnStart()
    {
        this.m_effectsPendingFinish++;
        this.m_startSpell = this.CloneSpell(this.m_StartInfo.m_Prefab);
        this.m_startSpell.SetSource(base.GetSource());
        this.m_startSpell.AddTargets(base.GetTargets());
        if (this.m_StartInfo.m_UseSuperSpellLocation)
        {
            this.m_startSpell.SetPosition(base.transform.position);
        }
    }

    private void UpdatePendingStateChangeFlags(SpellStateType stateType)
    {
        if (!base.HasStateContent(stateType))
        {
            this.m_pendingNoneStateChange = true;
            this.m_pendingSpellFinish = true;
        }
        else
        {
            this.m_pendingNoneStateChange = false;
            this.m_pendingSpellFinish = false;
        }
    }

    protected virtual void UpdateVisualTargets()
    {
        switch (this.m_TargetInfo.m_Behavior)
        {
            case SpellTargetBehavior.FRIENDLY_PLAY_ZONE_CENTER:
            {
                ZonePlay play = SpellUtils.FindFriendlyPlayZone(this);
                this.AddVisualTarget(play.gameObject);
                break;
            }
            case SpellTargetBehavior.FRIENDLY_PLAY_ZONE_RANDOM:
            {
                ZonePlay zonePlay = SpellUtils.FindFriendlyPlayZone(this);
                this.GenerateRandomPlayZoneVisualTargets(zonePlay);
                break;
            }
            case SpellTargetBehavior.OPPONENT_PLAY_ZONE_CENTER:
            {
                ZonePlay play3 = SpellUtils.FindOpponentPlayZone(this);
                this.AddVisualTarget(play3.gameObject);
                break;
            }
            case SpellTargetBehavior.OPPONENT_PLAY_ZONE_RANDOM:
            {
                ZonePlay play4 = SpellUtils.FindOpponentPlayZone(this);
                this.GenerateRandomPlayZoneVisualTargets(play4);
                break;
            }
            case SpellTargetBehavior.BOARD_CENTER:
            {
                Board board = Board.Get();
                this.AddVisualTarget(board.FindBone("CenterPointBone").gameObject);
                break;
            }
            case SpellTargetBehavior.UNTARGETED:
                this.AddVisualTarget(base.GetSource());
                break;

            case SpellTargetBehavior.CHOSEN_TARGET_ONLY:
                this.AddChosenTargetAsVisualTarget();
                break;

            case SpellTargetBehavior.BOARD_RANDOM:
                this.GenerateRandomBoardVisualTargets();
                break;

            case SpellTargetBehavior.TARGET_ZONE_CENTER:
            {
                Zone zone = SpellUtils.FindTargetZone(this);
                this.AddVisualTarget(zone.gameObject);
                break;
            }
            case SpellTargetBehavior.NEW_CREATED_CARDS:
                this.GenerateCreatedCardsTargets();
                break;

            default:
                this.AddAllTargetsAsVisualTargets();
                break;
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitAndSpawnAreaEffect(SpellAreaEffectInfo info)
    {
        return new <WaitAndSpawnAreaEffect>c__Iterator200 { info = info, <$>info = info, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitAndSpawnChain()
    {
        return new <WaitAndSpawnChain>c__Iterator1FF { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitAndSpawnImpact(int targetIndex)
    {
        return new <WaitAndSpawnImpact>c__Iterator1FE { targetIndex = targetIndex, <$>targetIndex = targetIndex, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitAndSpawnMissile(int targetIndex)
    {
        return new <WaitAndSpawnMissile>c__Iterator1FC { targetIndex = targetIndex, <$>targetIndex = targetIndex, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForStartDelayThenDoAction()
    {
        return new <WaitForStartDelayThenDoAction>c__Iterator1FB { <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator WaitForTasks(QueueList<PowerTask> tasksToWaitFor)
    {
        return new <WaitForTasks>c__Iterator202 { tasksToWaitFor = tasksToWaitFor, <$>tasksToWaitFor = tasksToWaitFor, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <CompleteTasksFromMetaData>c__Iterator203 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delaySec;
        internal int <$>metaDataIndex;
        internal SuperSpell <>f__this;
        internal float delaySec;
        internal int metaDataIndex;

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
                    this.<>f__this.m_effectsPendingFinish++;
                    this.$current = new WaitForSeconds(this.delaySec);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.CompleteMetaDataTasks(this.metaDataIndex, new PowerTaskList.CompleteCallback(this.<>f__this.OnMetaDataTasksComplete));
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
    private sealed class <CompleteTasksUntilMetaData>c__Iterator201 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>metaDataIndex;
        internal SuperSpell <>f__this;
        internal QueueList<PowerTask> <tasks>__0;
        internal int metaDataIndex;

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
                    this.<>f__this.m_effectsPendingFinish++;
                    this.<>f__this.m_taskList.DoTasks(0, this.metaDataIndex);
                    this.<tasks>__0 = this.<>f__this.DetermineTasksToWaitFor(0, this.metaDataIndex);
                    if ((this.<tasks>__0 == null) || (this.<tasks>__0.Count <= 0))
                    {
                        break;
                    }
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.WaitForTasks(this.<tasks>__0));
                    this.$PC = 1;
                    return true;

                case 1:
                    break;

                default:
                    goto Label_00C7;
            }
            this.<>f__this.m_effectsPendingFinish--;
            this.$PC = -1;
        Label_00C7:
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
    private sealed class <DetermineTasksToWaitFor>c__AnonStorey34D
    {
        internal Entity entity;

        internal bool <>m__1B0(GameObject currTargetObject)
        {
            Card component = currTargetObject.GetComponent<Card>();
            return (this.entity.GetCard() == component);
        }
    }

    [CompilerGenerated]
    private sealed class <DoDelayedActionDuringGameEvents>c__Iterator1FA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal QueueList<PowerTask> <$>tasksToWaitFor;
        internal SuperSpell <>f__this;
        internal QueueList<PowerTask> tasksToWaitFor;

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
                    this.<>f__this.m_effectsPendingFinish++;
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.WaitForTasks(this.tasksToWaitFor));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_effectsPendingFinish--;
                    if (!this.<>f__this.CheckAndWaitForStartDelayThenDoAction())
                    {
                        if (!this.<>f__this.CheckAndWaitForStartPrefabThenDoAction())
                        {
                            this.<>f__this.DoActionNow();
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
    private sealed class <SpawnReverseMissile>c__Iterator1FD : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Spell <$>cloneSpell;
        internal float <$>delay;
        internal GameObject <$>sourceObject;
        internal GameObject <$>targetObject;
        internal SuperSpell <>f__this;
        internal Spell <additionalMissile>__0;
        internal Spell cloneSpell;
        internal float delay;
        internal GameObject sourceObject;
        internal GameObject targetObject;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<additionalMissile>__0 = this.<>f__this.CloneSpell(this.cloneSpell);
                    this.<additionalMissile>__0.SetSource(this.sourceObject);
                    this.<additionalMissile>__0.AddTarget(this.targetObject);
                    this.<additionalMissile>__0.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.<>f__this.OnMissileSpellStateFinished), -1);
                    this.<additionalMissile>__0.ActivateState(SpellStateType.BIRTH);
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
    private sealed class <WaitAndSpawnAreaEffect>c__Iterator200 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SpellAreaEffectInfo <$>info;
        internal SuperSpell <>f__this;
        internal Spell <areaEffect>__1;
        internal float <spawnDelaySec>__0;
        internal SpellAreaEffectInfo info;

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
                    this.<spawnDelaySec>__0 = UnityEngine.Random.Range(this.info.m_SpawnDelaySecMin, this.info.m_SpawnDelaySecMax);
                    this.$current = new WaitForSeconds(this.<spawnDelaySec>__0);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<areaEffect>__1 = this.<>f__this.CloneSpell(this.info.m_Prefab);
                    this.<areaEffect>__1.SetSource(this.<>f__this.GetSource());
                    this.<areaEffect>__1.AttachPowerTaskList(this.<>f__this.m_taskList);
                    if (this.info.m_UseSuperSpellLocation)
                    {
                        this.<areaEffect>__1.SetPosition(this.<>f__this.transform.position);
                    }
                    this.<areaEffect>__1.m_Facing = this.info.m_Facing;
                    this.<areaEffect>__1.m_FacingOptions = this.info.m_FacingOptions;
                    this.<areaEffect>__1.UpdateOrientation();
                    this.<areaEffect>__1.Activate();
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
    private sealed class <WaitAndSpawnChain>c__Iterator1FF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<GameObject>.Enumerator <$s_1188>__3;
        internal SuperSpell <>f__this;
        internal Spell <chain>__1;
        internal GameObject <sourceObject>__2;
        internal float <spawnDelaySec>__0;
        internal GameObject <targetObject>__4;

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
                    this.<spawnDelaySec>__0 = UnityEngine.Random.Range(this.<>f__this.m_ChainInfo.m_SpawnDelayMin, this.<>f__this.m_ChainInfo.m_SpawnDelayMax);
                    this.$current = new WaitForSeconds(this.<spawnDelaySec>__0);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<chain>__1 = this.<>f__this.CloneSpell(this.<>f__this.m_ChainInfo.m_Prefab);
                    this.<sourceObject>__2 = this.<>f__this.GetPrimaryTarget();
                    this.<chain>__1.SetSource(this.<sourceObject>__2);
                    this.<$s_1188>__3 = this.<>f__this.m_visualTargets.GetEnumerator();
                    try
                    {
                        while (this.<$s_1188>__3.MoveNext())
                        {
                            this.<targetObject>__4 = this.<$s_1188>__3.Current;
                            if (this.<targetObject>__4 != this.<sourceObject>__2)
                            {
                                this.<chain>__1.AddTarget(this.<targetObject>__4);
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_1188>__3.Dispose();
                    }
                    this.<chain>__1.ActivateState(SpellStateType.ACTION);
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
    private sealed class <WaitAndSpawnImpact>c__Iterator1FE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>targetIndex;
        internal SuperSpell <>f__this;
        internal float <gameDelaySec>__2;
        internal Spell <impact>__6;
        internal Spell <impactPrefab>__5;
        internal int <metaDataIndex>__1;
        internal GameObject <sourceObject>__3;
        internal float <spawnDelaySec>__0;
        internal GameObject <targetObject>__4;
        internal int targetIndex;

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
                    this.<spawnDelaySec>__0 = UnityEngine.Random.Range(this.<>f__this.m_ImpactInfo.m_SpawnDelaySecMin, this.<>f__this.m_ImpactInfo.m_SpawnDelaySecMax);
                    this.$current = new WaitForSeconds(this.<spawnDelaySec>__0);
                    this.$PC = 1;
                    goto Label_0244;

                case 1:
                    this.<metaDataIndex>__1 = this.<>f__this.GetMetaDataIndexForTarget(this.targetIndex);
                    if (this.<metaDataIndex>__1 < 0)
                    {
                        goto Label_0122;
                    }
                    if (!this.<>f__this.ShouldCompleteTasksUntilMetaData(this.<metaDataIndex>__1))
                    {
                        break;
                    }
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.CompleteTasksUntilMetaData(this.<metaDataIndex>__1));
                    this.$PC = 2;
                    goto Label_0244;

                case 2:
                    break;

                default:
                    goto Label_0242;
            }
            this.<gameDelaySec>__2 = UnityEngine.Random.Range(this.<>f__this.m_ImpactInfo.m_GameDelaySecMin, this.<>f__this.m_ImpactInfo.m_GameDelaySecMax);
            this.<>f__this.StartCoroutine(this.<>f__this.CompleteTasksFromMetaData(this.<metaDataIndex>__1, this.<gameDelaySec>__2));
        Label_0122:
            this.<sourceObject>__3 = this.<>f__this.GetSource();
            this.<targetObject>__4 = this.<>f__this.m_visualTargets[this.targetIndex];
            this.<impactPrefab>__5 = this.<>f__this.DetermineImpactPrefab(this.<targetObject>__4);
            this.<impact>__6 = this.<>f__this.CloneSpell(this.<impactPrefab>__5);
            this.<impact>__6.SetSource(this.<sourceObject>__3);
            this.<impact>__6.AddTarget(this.<targetObject>__4);
            if (this.<>f__this.m_ImpactInfo.m_UseSuperSpellLocation)
            {
                this.<impact>__6.SetPosition(this.<>f__this.transform.position);
            }
            else
            {
                if (this.<>f__this.IsMakingClones())
                {
                    this.<impact>__6.m_Location = this.<>f__this.m_ImpactInfo.m_Location;
                    this.<impact>__6.m_SetParentToLocation = this.<>f__this.m_ImpactInfo.m_SetParentToLocation;
                }
                this.<impact>__6.UpdatePosition();
            }
            this.<impact>__6.UpdateOrientation();
            this.<impact>__6.Activate();
            this.$PC = -1;
        Label_0242:
            return false;
        Label_0244:
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
    private sealed class <WaitAndSpawnMissile>c__Iterator1FC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>targetIndex;
        internal SuperSpell <>f__this;
        internal int <metaDataIndex>__0;
        internal Spell <missile>__3;
        internal GameObject <sourceObject>__1;
        internal GameObject <targetObject>__2;
        internal int targetIndex;

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
                    this.<>f__this.m_effectsPendingFinish++;
                    this.<metaDataIndex>__0 = this.<>f__this.GetMetaDataIndexForTarget(this.targetIndex);
                    if (!this.<>f__this.ShouldCompleteTasksUntilMetaData(this.<metaDataIndex>__0))
                    {
                        break;
                    }
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.CompleteTasksUntilMetaData(this.<metaDataIndex>__0));
                    this.$PC = 1;
                    return true;

                case 1:
                    break;

                default:
                    goto Label_020D;
            }
            this.<sourceObject>__1 = this.<>f__this.GetSource();
            this.<targetObject>__2 = this.<>f__this.m_visualTargets[this.targetIndex];
            if (this.<>f__this.m_MissileInfo.m_Prefab != null)
            {
                this.<missile>__3 = this.<>f__this.CloneSpell(this.<>f__this.m_MissileInfo.m_Prefab);
                this.<missile>__3.SetSource(this.<sourceObject>__1);
                this.<missile>__3.AddTarget(this.<targetObject>__2);
                if (this.<>f__this.m_MissileInfo.m_UseSuperSpellLocation)
                {
                    this.<missile>__3.SetPosition(this.<>f__this.transform.position);
                }
                this.<missile>__3.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.<>f__this.OnMissileSpellStateFinished), this.targetIndex);
                this.<missile>__3.ActivateState(SpellStateType.BIRTH);
            }
            else
            {
                this.<>f__this.m_effectsPendingFinish--;
            }
            if (this.<>f__this.m_MissileInfo.m_ReversePrefab != null)
            {
                this.<>f__this.m_effectsPendingFinish++;
                this.<>f__this.StartCoroutine(this.<>f__this.SpawnReverseMissile(this.<>f__this.m_MissileInfo.m_ReversePrefab, this.<targetObject>__2, this.<sourceObject>__1, this.<>f__this.m_MissileInfo.m_reverseDelay));
            }
            this.$PC = -1;
        Label_020D:
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
    private sealed class <WaitForStartDelayThenDoAction>c__Iterator1FB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SuperSpell <>f__this;
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
                    this.<delaySec>__0 = UnityEngine.Random.Range(this.<>f__this.m_ActionInfo.m_StartDelayMin, this.<>f__this.m_ActionInfo.m_StartDelayMax);
                    this.$current = new WaitForSeconds(this.<delaySec>__0);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_effectsPendingFinish--;
                    if (!this.<>f__this.CheckAndWaitForStartPrefabThenDoAction())
                    {
                        this.<>f__this.DoActionNow();
                        this.$PC = -1;
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
    private sealed class <WaitForTasks>c__Iterator202 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal QueueList<PowerTask> <$>tasksToWaitFor;
        internal SuperSpell <>f__this;
        internal Card <card>__3;
        internal Entity <entity>__1;
        internal PowerTask <task>__0;
        internal Zone <zone>__4;
        internal int <zoneTag>__2;
        internal QueueList<PowerTask> tasksToWaitFor;

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
                    goto Label_0119;

                case 2:
                    break;

                case 3:
                    goto Label_00FD;

                default:
                    goto Label_0131;
            }
        Label_00CA:
            while (this.<card>__3.GetZone() != this.<zone>__4)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0133;
            }
        Label_00FD:
            while (this.<card>__3.IsActorLoading())
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_0133;
            }
            this.tasksToWaitFor.Dequeue();
        Label_0119:
            if (this.tasksToWaitFor.Count > 0)
            {
                this.<task>__0 = this.tasksToWaitFor.Peek();
                if (!this.<task>__0.IsCompleted())
                {
                    this.$current = null;
                    this.$PC = 1;
                    goto Label_0133;
                }
                this.<>f__this.GetZoneChangeFromPowerTask(this.<task>__0, out this.<entity>__1, out this.<zoneTag>__2);
                this.<card>__3 = this.<entity>__1.GetCard();
                this.<zone>__4 = ZoneMgr.Get().FindZoneForEntityAndZoneTag(this.<entity>__1, (TAG_ZONE) this.<zoneTag>__2);
                goto Label_00CA;
            }
            this.$PC = -1;
        Label_0131:
            return false;
        Label_0133:
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
}

