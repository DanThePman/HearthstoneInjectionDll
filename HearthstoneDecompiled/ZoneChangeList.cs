using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ZoneChangeList
{
    private bool m_canceledChangeList;
    private List<ZoneChange> m_changes = new List<ZoneChange>();
    private bool m_complete;
    private ZoneMgr.ChangeCompleteCallback m_completeCallback;
    private object m_completeCallbackUserData;
    private HashSet<Zone> m_dirtyZones = new HashSet<Zone>();
    private int m_id;
    private bool m_ignoreCardZoneChanges;
    private int m_predictedPosition;
    private PowerTaskList m_taskList;

    public void AddChange(ZoneChange change)
    {
        this.m_changes.Add(change);
    }

    public void ClearChanges()
    {
        this.m_changes.Clear();
    }

    [Conditional("ZONE_CHANGE_DEBUG")]
    private void DebugPrint(string format, params object[] args)
    {
        string str = string.Format(format, args);
        Log.Zone.Print(str, new object[0]);
    }

    public bool DoesIgnoreCardZoneChanges()
    {
        return this.m_ignoreCardZoneChanges;
    }

    private void Finish()
    {
        this.m_complete = true;
        object[] args = new object[] { this };
        Log.Zone.Print("ZoneChangeList.Finish() - {0}", args);
    }

    [DebuggerHidden]
    private IEnumerator FinishWhenPossible()
    {
        return new <FinishWhenPossible>c__IteratorCE { <>f__this = this };
    }

    public void FireCompleteCallback()
    {
        object[] args = new object[] { this.m_id, (this.m_taskList != null) ? this.m_taskList.GetId().ToString() : "(null)", this.m_changes.Count, this.m_complete, (this.m_completeCallback != null) ? "(not null)" : "(null)" };
        this.DebugPrint("ZoneChangeList.FireCompleteCallback() - m_id={0} m_taskList={1} m_changes.Count={2} m_complete={3} m_completeCallback={4}", args);
        if (this.m_completeCallback != null)
        {
            this.m_completeCallback(this, this.m_completeCallbackUserData);
        }
    }

    public ZoneChange GetChange(int index)
    {
        return this.m_changes[index];
    }

    public List<ZoneChange> GetChanges()
    {
        return this.m_changes;
    }

    public ZoneMgr.ChangeCompleteCallback GetCompleteCallback()
    {
        return this.m_completeCallback;
    }

    public object GetCompleteCallbackUserData()
    {
        return this.m_completeCallbackUserData;
    }

    public int GetId()
    {
        return this.m_id;
    }

    public Card GetLocalTriggerCard()
    {
        ZoneChange localTriggerChange = this.GetLocalTriggerChange();
        if (localTriggerChange == null)
        {
            return null;
        }
        return localTriggerChange.GetEntity().GetCard();
    }

    public ZoneChange GetLocalTriggerChange()
    {
        if (!this.IsLocal())
        {
            return null;
        }
        return ((this.m_changes.Count <= 0) ? null : this.m_changes[0]);
    }

    public int GetPredictedPosition()
    {
        return this.m_predictedPosition;
    }

    public PowerTaskList GetTaskList()
    {
        return this.m_taskList;
    }

    public void InsertChange(int index, ZoneChange change)
    {
        this.m_changes.Insert(index, change);
    }

    public bool IsCanceledChangeList()
    {
        return this.m_canceledChangeList;
    }

    public bool IsComplete()
    {
        return this.m_complete;
    }

    private bool IsDeathBlock()
    {
        if (this.m_taskList == null)
        {
            return false;
        }
        Network.HistActionStart sourceAction = this.m_taskList.GetSourceAction();
        if (sourceAction == null)
        {
            return false;
        }
        return (sourceAction.BlockType == HistoryBlock.Type.DEATHS);
    }

    private bool IsDisplayableDyingSecret(Entity entity, Card card, Zone srcZone, Zone dstZone)
    {
        if (entity.IsSecret())
        {
            if (!(srcZone is ZoneSecret))
            {
                return false;
            }
            if (dstZone is ZoneGraveyard)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsEntityLoading(Entity entity, Card card)
    {
        return (entity.IsLoadingAssets() || ((card != null) && card.IsActorLoading()));
    }

    public bool IsLocal()
    {
        return (this.m_taskList == null);
    }

    private bool MustWaitForChoices()
    {
        if (ChoiceCardMgr.Get().HasChoices())
        {
            PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
            if (powerProcessor.HasGameOverTaskList())
            {
                return false;
            }
            foreach (int num in GameState.Get().GetPlayerMap().Keys)
            {
                PowerTaskList preChoiceTaskList = ChoiceCardMgr.Get().GetPreChoiceTaskList(num);
                if ((preChoiceTaskList != null) && !powerProcessor.HasTaskList(preChoiceTaskList))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnUpdateLayoutComplete(Zone zone, object userData)
    {
        object[] args = new object[] { this.m_id, zone };
        this.DebugPrint("ZoneChangeList.OnUpdateLayoutComplete() - m_id={0} END waiting for zone {1}", args);
        this.m_dirtyZones.Remove(zone);
    }

    [DebuggerHidden]
    public IEnumerator ProcessChanges()
    {
        return new <ProcessChanges>c__IteratorCA { <>f__this = this };
    }

    private void ProcessDyingSecrets(Map<Player, DyingSecretGroup> dyingSecretMap)
    {
        if (dyingSecretMap != null)
        {
            Map<Player, DeadSecretGroup> deadSecretMap = null;
            foreach (KeyValuePair<Player, DyingSecretGroup> pair in dyingSecretMap)
            {
                Player key = pair.Key;
                DyingSecretGroup group = pair.Value;
                Card mainCard = group.GetMainCard();
                List<Card> cards = group.GetCards();
                List<Actor> actors = group.GetActors();
                for (int i = 0; i < cards.Count; i++)
                {
                    Card card = cards[i];
                    Actor oldActor = actors[i];
                    if (card.WasSecretTriggered())
                    {
                        oldActor.Destroy();
                    }
                    else
                    {
                        DeadSecretGroup group2;
                        if ((card == mainCard) && card.CanShowSecretDeath())
                        {
                            card.ShowSecretDeath(oldActor);
                        }
                        else
                        {
                            oldActor.Destroy();
                        }
                        if (deadSecretMap == null)
                        {
                            deadSecretMap = new Map<Player, DeadSecretGroup>();
                        }
                        if (!deadSecretMap.TryGetValue(key, out group2))
                        {
                            group2 = new DeadSecretGroup();
                            group2.SetMainCard(mainCard);
                            deadSecretMap.Add(key, group2);
                        }
                        group2.AddCard(card);
                    }
                }
            }
            BigCard.Get().ShowSecretDeaths(deadSecretMap);
        }
    }

    public void SetCanceledChangeList(bool canceledChangeList)
    {
        this.m_canceledChangeList = canceledChangeList;
    }

    public void SetCompleteCallback(ZoneMgr.ChangeCompleteCallback callback)
    {
        this.m_completeCallback = callback;
    }

    public void SetCompleteCallbackUserData(object userData)
    {
        this.m_completeCallbackUserData = userData;
    }

    public void SetId(int id)
    {
        this.m_id = id;
    }

    public void SetIgnoreCardZoneChanges(bool ignore)
    {
        this.m_ignoreCardZoneChanges = ignore;
    }

    public void SetPredictedPosition(int pos)
    {
        this.m_predictedPosition = pos;
    }

    public void SetTaskList(PowerTaskList taskList)
    {
        this.m_taskList = taskList;
    }

    public void SetZoneInputBlocking(bool block)
    {
        for (int i = 0; i < this.m_changes.Count; i++)
        {
            ZoneChange change = this.m_changes[i];
            Zone sourceZone = change.GetSourceZone();
            if (sourceZone != null)
            {
                sourceZone.BlockInput(block);
            }
            Zone destinationZone = change.GetDestinationZone();
            if (destinationZone != null)
            {
                destinationZone.BlockInput(block);
            }
        }
    }

    public override string ToString()
    {
        object[] args = new object[] { this.m_id, this.m_changes.Count, this.m_complete, this.IsLocal(), this.GetLocalTriggerChange() };
        return string.Format("id={0} changes={1} complete={2} local={3} localTrigger=[{4}]", args);
    }

    [DebuggerHidden]
    private IEnumerator UpdateDirtyZones(HashSet<Entity> loadingEntities)
    {
        return new <UpdateDirtyZones>c__IteratorCC { loadingEntities = loadingEntities, <$>loadingEntities = loadingEntities, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForAndRemoveLoadingEntity(HashSet<Entity> loadingEntities, Entity entity, Card card)
    {
        return new <WaitForAndRemoveLoadingEntity>c__IteratorCB { entity = entity, card = card, loadingEntities = loadingEntities, <$>entity = entity, <$>card = card, <$>loadingEntities = loadingEntities, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ZoneHand_UpdateLayout(ZoneHand zoneHand)
    {
        return new <ZoneHand_UpdateLayout>c__IteratorCD { zoneHand = zoneHand, <$>zoneHand = zoneHand, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <FinishWhenPossible>c__IteratorCE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ZoneChangeList <>f__this;

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
                    if (this.<>f__this.m_dirtyZones.Count > 0)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_008E;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_008C;
            }
            while (GameState.Get().IsBusy())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_008E;
            }
            this.<>f__this.Finish();
            this.$PC = -1;
        Label_008C:
            return false;
        Label_008E:
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
    private sealed class <ProcessChanges>c__IteratorCA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ZoneChangeList <>f__this;
        internal Card <card>__5;
        internal ZoneChange <change>__3;
        internal Player <controller>__18;
        internal bool <controllerChanged>__15;
        internal int <dstControllerId>__10;
        internal int <dstPos>__11;
        internal Zone <dstZone>__12;
        internal TAG_ZONE <dstZoneTag>__13;
        internal DyingSecretGroup <dyingSecretGroup>__19;
        internal Map<Player, DyingSecretGroup> <dyingSecretMap>__1;
        internal Entity <entity>__4;
        internal int <i>__2;
        internal HashSet<Entity> <loadingEntities>__0;
        internal bool <posChanged>__16;
        internal PowerTask <powerTask>__6;
        internal bool <revealed>__17;
        internal bool <revealedSecret>__21;
        internal int <srcControllerId>__7;
        internal int <srcPos>__8;
        internal Zone <srcZone>__9;
        internal bool <transitionedZones>__20;
        internal bool <zoneChanged>__14;

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
                    object[] args = new object[] { this.<>f__this.m_id, (this.<>f__this.m_taskList != null) ? this.<>f__this.m_taskList.GetId().ToString() : "(null)", this.<>f__this.m_changes.Count };
                    this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - m_id={0} m_taskList={1} m_changes.Count={2}", args);
                    break;
                }
                case 1:
                    break;

                case 2:
                    goto Label_0289;

                case 3:
                    goto Label_05B8;

                case 4:
                    goto Label_0666;

                default:
                    goto Label_09BA;
            }
            if (this.<>f__this.MustWaitForChoices())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_09BC;
            }
            this.<loadingEntities>__0 = new HashSet<Entity>();
            this.<dyingSecretMap>__1 = null;
            this.<i>__2 = 0;
            while (this.<i>__2 < this.<>f__this.m_changes.Count)
            {
                object[] objArray10;
                object[] objArray7;
                object[] objArray4;
                this.<change>__3 = this.<>f__this.m_changes[this.<i>__2];
                object[] objArray2 = new object[] { this.<i>__2, this.<change>__3 };
                this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - processing index={0} change={1}", objArray2);
                this.<entity>__4 = this.<change>__3.GetEntity();
                this.<card>__5 = this.<entity>__4.GetCard();
                this.<powerTask>__6 = this.<change>__3.GetPowerTask();
                this.<srcControllerId>__7 = this.<entity>__4.GetControllerId();
                this.<srcPos>__8 = 0;
                this.<srcZone>__9 = null;
                if (this.<card>__5 != null)
                {
                    this.<srcPos>__8 = this.<card>__5.GetZonePosition();
                    this.<srcZone>__9 = this.<card>__5.GetZone();
                }
                this.<dstControllerId>__10 = this.<change>__3.GetDestinationControllerId();
                this.<dstPos>__11 = this.<change>__3.GetDestinationPosition();
                this.<dstZone>__12 = this.<change>__3.GetDestinationZone();
                this.<dstZoneTag>__13 = this.<change>__3.GetDestinationZoneTag();
                if (this.<powerTask>__6 == null)
                {
                    goto Label_02D5;
                }
                if (this.<powerTask>__6.IsCompleted())
                {
                    goto Label_0941;
                }
                if (!this.<loadingEntities>__0.Contains(this.<entity>__4))
                {
                    goto Label_02A8;
                }
                object[] objArray3 = new object[] { this.<card>__5 };
                this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - START waiting for {0} to load (powerTask=(not null))", objArray3);
                this.$current = ZoneMgr.Get().StartCoroutine(this.<>f__this.WaitForAndRemoveLoadingEntity(this.<loadingEntities>__0, this.<entity>__4, this.<card>__5));
                this.$PC = 2;
                goto Label_09BC;
            Label_0289:
                objArray4 = new object[] { this.<card>__5 };
                this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - END waiting for {0} to load (powerTask=(not null))", objArray4);
            Label_02A8:
                this.<powerTask>__6.DoTask();
                if (this.<entity>__4.IsLoadingAssets())
                {
                    this.<loadingEntities>__0.Add(this.<entity>__4);
                }
            Label_02D5:
                if ((this.<card>__5 == null) || this.<>f__this.m_ignoreCardZoneChanges)
                {
                    goto Label_0941;
                }
                this.<zoneChanged>__14 = (this.<dstZoneTag>__13 != TAG_ZONE.INVALID) && (this.<srcZone>__9 != this.<dstZone>__12);
                this.<controllerChanged>__15 = (this.<dstControllerId>__10 != 0) && (this.<srcControllerId>__7 != this.<dstControllerId>__10);
                this.<posChanged>__16 = (this.<dstPos>__11 != 0) && (this.<srcPos>__8 != this.<dstPos>__11);
                this.<revealed>__17 = (this.<powerTask>__6 != null) && (this.<powerTask>__6.GetPower().Type == Network.PowerType.SHOW_ENTITY);
                if ((UniversalInputManager.UsePhoneUI != null) && this.<>f__this.IsDisplayableDyingSecret(this.<entity>__4, this.<card>__5, this.<srcZone>__9, this.<dstZone>__12))
                {
                    if (this.<dyingSecretMap>__1 == null)
                    {
                        this.<dyingSecretMap>__1 = new Map<Player, DyingSecretGroup>();
                    }
                    this.<controller>__18 = this.<card>__5.GetController();
                    if (!this.<dyingSecretMap>__1.TryGetValue(this.<controller>__18, out this.<dyingSecretGroup>__19))
                    {
                        this.<dyingSecretGroup>__19 = new DyingSecretGroup();
                        this.<dyingSecretMap>__1.Add(this.<controller>__18, this.<dyingSecretGroup>__19);
                    }
                    this.<dyingSecretGroup>__19.AddCard(this.<card>__5);
                }
                if ((!this.<zoneChanged>__14 && !this.<controllerChanged>__15) && !this.<revealed>__17)
                {
                    goto Label_085F;
                }
                this.<transitionedZones>__20 = this.<zoneChanged>__14 || this.<controllerChanged>__15;
                this.<revealedSecret>__21 = this.<revealed>__17 && (this.<entity>__4.GetZone() == TAG_ZONE.SECRET);
                if (this.<transitionedZones>__20 || !this.<revealedSecret>__21)
                {
                    if (this.<srcZone>__9 != null)
                    {
                        this.<>f__this.m_dirtyZones.Add(this.<srcZone>__9);
                    }
                    if (this.<dstZone>__12 != null)
                    {
                        this.<>f__this.m_dirtyZones.Add(this.<dstZone>__12);
                    }
                    object[] objArray5 = new object[] { this.<card>__5, this.<dstZone>__12 };
                    this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - TRANSITIONING card {0} to {1}", objArray5);
                }
                if (!this.<loadingEntities>__0.Contains(this.<entity>__4))
                {
                    goto Label_05F3;
                }
                object[] objArray6 = new object[] { this.<card>__5, this.<zoneChanged>__14, this.<controllerChanged>__15 };
                this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - START waiting for {0} to load (zoneChanged={1} controllerChanged={2} powerTask=(not null))", objArray6);
                this.$current = ZoneMgr.Get().StartCoroutine(this.<>f__this.WaitForAndRemoveLoadingEntity(this.<loadingEntities>__0, this.<entity>__4, this.<card>__5));
                this.$PC = 3;
                goto Label_09BC;
            Label_05B8:
                objArray7 = new object[] { this.<card>__5, this.<zoneChanged>__14, this.<controllerChanged>__15 };
                this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - END waiting for {0} to load (zoneChanged={1} controllerChanged={2} powerTask=(not null))", objArray7);
            Label_05F3:
                if (this.<card>__5.IsActorReady() && !this.<card>__5.IsBeingDrawnByOpponent())
                {
                    goto Label_06C1;
                }
                object[] objArray8 = new object[] { this.<card>__5, this.<zoneChanged>__14, this.<controllerChanged>__15 };
                this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - START waiting for {0} to become ready (zoneChanged={1} controllerChanged={2} powerTask=(not null))", objArray8);
            Label_0666:
                while (!this.<card>__5.IsActorReady() || this.<card>__5.IsBeingDrawnByOpponent())
                {
                    this.$current = null;
                    this.$PC = 4;
                    goto Label_09BC;
                }
                object[] objArray9 = new object[] { this.<card>__5, this.<zoneChanged>__14, this.<controllerChanged>__15 };
                this.<>f__this.DebugPrint("ZoneChangeList.ProcessChanges() - END waiting for {0} to become ready (zoneChanged={1} controllerChanged={2} powerTask=(not null))", objArray9);
            Label_06C1:
                objArray10 = new object[] { this.<>f__this.m_id, this.<>f__this.IsLocal(), this.<card>__5, this.<srcZone>__9, this.<dstZone>__12 };
                Log.Zone.Print("ZoneChangeList.ProcessChanges() - id={0} local={1} {2} zone from {3} -> {4}", objArray10);
                if (this.<transitionedZones>__20)
                {
                    if (((this.<srcZone>__9 is ZonePlay) && (this.<srcZone>__9.m_Side == Player.Side.OPPOSING)) && ((this.<dstZone>__12 is ZoneHand) && (this.<dstZone>__12.m_Side == Player.Side.OPPOSING)))
                    {
                        object[] objArray11 = new object[] { this.<>f__this.m_id, this.<card>__5, this.<srcZone>__9, this.<dstZone>__12 };
                        Log.FaceDownCard.Print("ZoneChangeList.ProcessChanges() - id={0} {1}.TransitionToZone(): {2} -> {3}", objArray11);
                        this.<>f__this.m_taskList.DebugDump(Log.FaceDownCard);
                    }
                    this.<card>__5.TransitionToZone(this.<dstZone>__12);
                }
                else if (this.<revealed>__17)
                {
                    if (this.<card>__5.GetCardDef() == null)
                    {
                        object[] objArray12 = new object[] { this.<card>__5, this.<card>__5.GetZone(), this.<card>__5.GetCardDef() };
                        Log.Power.PrintError("ZoneMgr.ProcessChanges() - {0} cardIdChanged with null CardDef {0} zone={1} cardDef={2}", objArray12);
                    }
                    else
                    {
                        this.<card>__5.UpdateActor();
                    }
                }
                if (this.<card>__5.IsActorLoading())
                {
                    this.<loadingEntities>__0.Add(this.<entity>__4);
                }
            Label_085F:
                if (this.<posChanged>__16)
                {
                    if (((this.<srcZone>__9 != null) && !this.<zoneChanged>__14) && !this.<controllerChanged>__15)
                    {
                        this.<>f__this.m_dirtyZones.Add(this.<srcZone>__9);
                    }
                    if (this.<dstZone>__12 != null)
                    {
                        this.<>f__this.m_dirtyZones.Add(this.<dstZone>__12);
                    }
                    object[] objArray13 = new object[] { this.<>f__this.m_id, this.<>f__this.IsLocal(), this.<card>__5, this.<srcPos>__8, this.<dstPos>__11 };
                    Log.Zone.Print("ZoneChangeList.ProcessChanges() - id={0} local={1} {2} pos from {3} -> {4}", objArray13);
                    this.<card>__5.SetZonePosition(this.<dstPos>__11);
                }
            Label_0941:
                this.<i>__2++;
            }
            if (this.<>f__this.IsCanceledChangeList())
            {
                this.<>f__this.SetZoneInputBlocking(false);
            }
            this.<>f__this.ProcessDyingSecrets(this.<dyingSecretMap>__1);
            ZoneMgr.Get().StartCoroutine(this.<>f__this.UpdateDirtyZones(this.<loadingEntities>__0));
            this.$PC = -1;
        Label_09BA:
            return false;
        Label_09BC:
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
    private sealed class <UpdateDirtyZones>c__IteratorCC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HashSet<Entity> <$>loadingEntities;
        internal HashSet<Entity>.Enumerator <$s_654>__0;
        internal HashSet<Zone>.Enumerator <$s_655>__4;
        internal Zone[] <$s_656>__7;
        internal int <$s_657>__8;
        internal ZoneChangeList <>f__this;
        internal Card <card>__2;
        internal Zone <dirtyZone>__5;
        internal Zone <dirtyZone>__9;
        internal Zone[] <dirtyZones>__6;
        internal Entity <entity>__1;
        internal float <layoutDelaySec>__3;
        internal HashSet<Entity> loadingEntities;

        [DebuggerHidden]
        public void Dispose()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 1:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_654>__0.Dispose();
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
                {
                    object[] args = new object[] { this.<>f__this.m_id, this.loadingEntities.Count, this.<>f__this.m_dirtyZones.Count };
                    this.<>f__this.DebugPrint("ZoneChangeList.UpdateDirtyZones() - m_id={0} loadingEntities.Count={1} m_dirtyZones.Count={2}", args);
                    this.<$s_654>__0 = this.loadingEntities.GetEnumerator();
                    num = 0xfffffffd;
                    break;
                }
                case 1:
                    break;

                case 2:
                    goto Label_01E5;

                default:
                    goto Label_0381;
            }
            try
            {
                switch (num)
                {
                    case 1:
                        goto Label_0117;
                }
                while (this.<$s_654>__0.MoveNext())
                {
                    this.<entity>__1 = this.<$s_654>__0.Current;
                    this.<card>__2 = this.<entity>__1.GetCard();
                    object[] objArray2 = new object[] { this.<>f__this.m_id, this.<entity>__1, this.<card>__2 };
                    this.<>f__this.DebugPrint("ZoneChangeList.UpdateDirtyZones() - m_id={0} START waiting for {1} to load (card={2})", objArray2);
                Label_0117:
                    while (this.<>f__this.IsEntityLoading(this.<entity>__1, this.<card>__2))
                    {
                        this.$current = null;
                        this.$PC = 1;
                        flag = true;
                        goto Label_0383;
                    }
                    object[] objArray3 = new object[] { this.<>f__this.m_id, this.<entity>__1, this.<card>__2 };
                    this.<>f__this.DebugPrint("ZoneChangeList.UpdateDirtyZones() - m_id={0} END waiting for {1} to load (card={2})", objArray3);
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_654>__0.Dispose();
            }
            if (!this.<>f__this.IsDeathBlock())
            {
                this.<dirtyZones>__6 = new Zone[this.<>f__this.m_dirtyZones.Count];
                this.<>f__this.m_dirtyZones.CopyTo(this.<dirtyZones>__6);
                this.<$s_656>__7 = this.<dirtyZones>__6;
                this.<$s_657>__8 = 0;
                while (this.<$s_657>__8 < this.<$s_656>__7.Length)
                {
                    this.<dirtyZone>__9 = this.<$s_656>__7[this.<$s_657>__8];
                    object[] objArray4 = new object[] { this.<>f__this.m_id, this.<dirtyZone>__9 };
                    this.<>f__this.DebugPrint("ZoneChangeList.UpdateDirtyZones() - m_id={0} START waiting for zone {1}", objArray4);
                    if (this.<dirtyZone>__9 is ZoneHand)
                    {
                        ZoneMgr.Get().StartCoroutine(this.<>f__this.ZoneHand_UpdateLayout((ZoneHand) this.<dirtyZone>__9));
                    }
                    else
                    {
                        this.<dirtyZone>__9.AddUpdateLayoutCompleteCallback(new Zone.UpdateLayoutCompleteCallback(this.<>f__this.OnUpdateLayoutComplete));
                        this.<dirtyZone>__9.UpdateLayout();
                    }
                    this.<$s_657>__8++;
                }
                goto Label_0364;
            }
            this.<layoutDelaySec>__3 = ZoneMgr.Get().RemoveNextDeathBlockLayoutDelaySec();
            if (this.<layoutDelaySec>__3 >= 0f)
            {
                this.$current = new WaitForSeconds(this.<layoutDelaySec>__3);
                this.$PC = 2;
                goto Label_0383;
            }
        Label_01E5:
            this.<$s_655>__4 = this.<>f__this.m_dirtyZones.GetEnumerator();
            try
            {
                while (this.<$s_655>__4.MoveNext())
                {
                    this.<dirtyZone>__5 = this.<$s_655>__4.Current;
                    this.<dirtyZone>__5.UpdateLayout();
                }
            }
            finally
            {
                this.<$s_655>__4.Dispose();
            }
            this.<>f__this.m_dirtyZones.Clear();
        Label_0364:
            ZoneMgr.Get().StartCoroutine(this.<>f__this.FinishWhenPossible());
            this.$PC = -1;
        Label_0381:
            return false;
        Label_0383:
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
    private sealed class <WaitForAndRemoveLoadingEntity>c__IteratorCB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>card;
        internal Entity <$>entity;
        internal HashSet<Entity> <$>loadingEntities;
        internal ZoneChangeList <>f__this;
        internal Card card;
        internal Entity entity;
        internal HashSet<Entity> loadingEntities;

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
                    if (this.<>f__this.IsEntityLoading(this.entity, this.card))
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.loadingEntities.Remove(this.entity);
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
    private sealed class <ZoneHand_UpdateLayout>c__IteratorCD : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ZoneHand <$>zoneHand;
        private static Predicate<Card> <>f__am$cache6;
        internal ZoneChangeList <>f__this;
        internal Card <busyCard>__0;
        internal ZoneHand zoneHand;

        private static bool <>m__10C(Card card)
        {
            if ((TurnStartManager.Get() != null) && TurnStartManager.Get().IsCardDrawHandled(card))
            {
                return false;
            }
            return !card.IsActorReady();
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
                case 1:
                    if (<>f__am$cache6 == null)
                    {
                        <>f__am$cache6 = new Predicate<Card>(ZoneChangeList.<ZoneHand_UpdateLayout>c__IteratorCD.<>m__10C);
                    }
                    this.<busyCard>__0 = this.zoneHand.GetCards().Find(<>f__am$cache6);
                    if (this.<busyCard>__0 == null)
                    {
                        this.zoneHand.AddUpdateLayoutCompleteCallback(new Zone.UpdateLayoutCompleteCallback(this.<>f__this.OnUpdateLayoutComplete));
                        this.zoneHand.UpdateLayout();
                        this.$PC = -1;
                        break;
                    }
                    this.$current = null;
                    this.$PC = 1;
                    return true;
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

