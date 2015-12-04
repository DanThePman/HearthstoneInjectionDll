using PegasusGame;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class ZoneMgr : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<ZoneChange> <>f__am$cacheD;
    private List<ZoneChangeList> m_activeLocalChangeLists;
    private ZoneChangeList m_activeServerChangeList;
    private QueueList<ZoneChangeList> m_localChangeListHistory;
    private float m_nextDeathBlockLayoutDelaySec;
    private int m_nextLocalChangeListId;
    private int m_nextServerChangeListId;
    private List<ZoneChangeList> m_pendingLocalChangeLists;
    private Queue<ZoneChangeList> m_pendingServerChangeLists;
    private Map<int, Entity> m_tempEntityMap;
    private Map<Zone, TempZone> m_tempZoneMap;
    private Map<System.Type, string> m_tweenNames;
    private List<Zone> m_zones;
    private static ZoneMgr s_instance;

    public ZoneMgr()
    {
        Map<System.Type, string> map = new Map<System.Type, string>();
        map.Add(typeof(ZoneHand), "ZoneHandUpdateLayout");
        map.Add(typeof(ZonePlay), "ZonePlayUpdateLayout");
        map.Add(typeof(ZoneWeapon), "ZoneWeaponUpdateLayout");
        this.m_tweenNames = map;
        this.m_zones = new List<Zone>();
        this.m_nextLocalChangeListId = 1;
        this.m_nextServerChangeListId = 1;
        this.m_pendingServerChangeLists = new Queue<ZoneChangeList>();
        this.m_tempEntityMap = new Map<int, Entity>();
        this.m_tempZoneMap = new Map<Zone, TempZone>();
        this.m_activeLocalChangeLists = new List<ZoneChangeList>();
        this.m_pendingLocalChangeLists = new List<ZoneChangeList>();
        this.m_localChangeListHistory = new QueueList<ZoneChangeList>();
    }

    public ZoneChangeList AddLocalZoneChange(Card triggerCard, TAG_ZONE zoneTag)
    {
        Entity entity = triggerCard.GetEntity();
        Zone destinationZone = this.FindZoneForEntityAndZoneTag(entity, zoneTag);
        return this.AddLocalZoneChange(triggerCard, destinationZone, zoneTag, 0, null, null);
    }

    public ZoneChangeList AddLocalZoneChange(Card triggerCard, TAG_ZONE zoneTag, int destinationPos)
    {
        Zone destinationZone = this.FindZoneForEntityAndZoneTag(triggerCard.GetEntity(), zoneTag);
        return this.AddLocalZoneChange(triggerCard, destinationZone, zoneTag, destinationPos, null, null);
    }

    public ZoneChangeList AddLocalZoneChange(Card triggerCard, Zone destinationZone, int destinationPos)
    {
        if (destinationZone == null)
        {
            Debug.LogWarning(string.Format("ZoneMgr.AddLocalZoneChange() - illegal zone change to null zone for card {0}", triggerCard));
            return null;
        }
        return this.AddLocalZoneChange(triggerCard, destinationZone, destinationZone.m_ServerTag, destinationPos, null, null);
    }

    public ZoneChangeList AddLocalZoneChange(Card triggerCard, TAG_ZONE zoneTag, int destinationPos, ChangeCompleteCallback callback, object userData)
    {
        Zone destinationZone = this.FindZoneForEntityAndZoneTag(triggerCard.GetEntity(), zoneTag);
        return this.AddLocalZoneChange(triggerCard, destinationZone, zoneTag, destinationPos, callback, userData);
    }

    public ZoneChangeList AddLocalZoneChange(Card triggerCard, Zone destinationZone, int destinationPos, ChangeCompleteCallback callback, object userData)
    {
        if (destinationZone == null)
        {
            Debug.LogWarning(string.Format("ZoneMgr.AddLocalZoneChange() - illegal zone change to null zone for card {0}", triggerCard));
            return null;
        }
        return this.AddLocalZoneChange(triggerCard, destinationZone, destinationZone.m_ServerTag, destinationPos, callback, userData);
    }

    public ZoneChangeList AddLocalZoneChange(Card triggerCard, Zone destinationZone, TAG_ZONE destinationZoneTag, int destinationPos, ChangeCompleteCallback callback, object userData)
    {
        if (destinationZoneTag == TAG_ZONE.INVALID)
        {
            Debug.LogWarning(string.Format("ZoneMgr.AddLocalZoneChange() - illegal zone change to {0} for card {1}", destinationZoneTag, triggerCard));
            return null;
        }
        if (((destinationZone is ZonePlay) || (destinationZone is ZoneHand)) && (destinationPos <= 0))
        {
            Debug.LogWarning(string.Format("ZoneMgr.AddLocalZoneChange() - destinationPos {0} is too small for zone {1}, min is 1", destinationPos, destinationZone));
            return null;
        }
        ZoneChangeList changeList = this.CreateLocalChangeList(triggerCard, destinationZone, destinationZoneTag, destinationPos, callback, userData);
        this.ProcessOrEnqueueLocalChangeList(changeList);
        this.m_localChangeListHistory.Enqueue(changeList);
        return changeList;
    }

    public ZoneChangeList AddPredictedLocalZoneChange(Card triggerCard, Zone destinationZone, int destinationPos, int predictedPos)
    {
        if (triggerCard == null)
        {
            Debug.LogWarning(string.Format("ZoneMgr.AddPredictedLocalZoneChange() - triggerCard is null", new object[0]));
            return null;
        }
        ZoneChangeList list = this.AddLocalZoneChange(triggerCard, destinationZone, destinationPos);
        if (list == null)
        {
            return null;
        }
        triggerCard.SetPredictedZonePosition(predictedPos);
        list.SetPredictedPosition(predictedPos);
        return list;
    }

    public ZoneChangeList AddServerZoneChanges(PowerTaskList taskList, int taskStartIndex, int taskEndIndex, ChangeCompleteCallback callback, object userData)
    {
        int nextServerChangeListId = this.GetNextServerChangeListId();
        ZoneChangeList parentList = new ZoneChangeList();
        parentList.SetId(nextServerChangeListId);
        parentList.SetTaskList(taskList);
        parentList.SetCompleteCallback(callback);
        parentList.SetCompleteCallbackUserData(userData);
        object[] args = new object[] { taskList.GetId(), nextServerChangeListId, taskStartIndex, taskEndIndex };
        Log.Zone.Print("ZoneMgr.AddServerZoneChanges() - taskListId={0} changeListId={1} taskStart={2} taskEnd={3}", args);
        List<PowerTask> list2 = taskList.GetTaskList();
        for (int i = taskStartIndex; i <= taskEndIndex; i++)
        {
            PowerTask powerTask = list2[i];
            Network.PowerHistory power = powerTask.GetPower();
            Network.PowerType type = power.Type;
            ZoneChange change = null;
            switch (type)
            {
                case Network.PowerType.FULL_ENTITY:
                    change = this.CreateZoneChangeFromFullEntity(power as Network.HistFullEntity);
                    break;

                case Network.PowerType.SHOW_ENTITY:
                    change = this.CreateZoneChangeFromShowEntity(power as Network.HistShowEntity);
                    break;

                case Network.PowerType.HIDE_ENTITY:
                    change = this.CreateZoneChangeFromHideEntity(power as Network.HistHideEntity);
                    break;

                case Network.PowerType.TAG_CHANGE:
                    change = this.CreateZoneChangeFromTagChange(power as Network.HistTagChange);
                    break;

                case Network.PowerType.CREATE_GAME:
                    change = this.CreateZoneChangeFromCreateGame(power as Network.HistCreateGame);
                    break;

                case Network.PowerType.META_DATA:
                    change = this.CreateZoneChangeFromMetaData(power as Network.HistMetaData);
                    break;

                default:
                    Debug.LogError(string.Format("ZoneMgr.AddServerZoneChanges() - id={0} received unhandled power of type {1}", parentList.GetId(), type));
                    return null;
            }
            if (change != null)
            {
                change.SetParentList(parentList);
                change.SetPowerTask(powerTask);
                parentList.AddChange(change);
            }
        }
        this.m_tempEntityMap.Clear();
        this.m_pendingServerChangeLists.Enqueue(parentList);
        return parentList;
    }

    private void AutoCorrectZones()
    {
        ZoneChangeList changeList = null;
        foreach (Zone zone in this.FindZonesOfType<Zone>(Player.Side.FRIENDLY))
        {
            foreach (Card card in zone.GetCards())
            {
                Entity entity = card.GetEntity();
                TAG_ZONE tag = entity.GetZone();
                int controllerId = entity.GetControllerId();
                int zonePosition = entity.GetZonePosition();
                TAG_ZONE serverTag = zone.m_ServerTag;
                int num3 = zone.GetControllerId();
                int num4 = card.GetZonePosition();
                bool flag = tag == serverTag;
                bool flag2 = controllerId == num3;
                bool flag3 = (zonePosition == 0) || (zonePosition == num4);
                if ((!flag || !flag2) || !flag3)
                {
                    if (changeList == null)
                    {
                        int nextLocalChangeListId = this.GetNextLocalChangeListId();
                        changeList = new ZoneChangeList();
                        changeList.SetId(nextLocalChangeListId);
                    }
                    ZoneChange change = new ZoneChange();
                    change.SetEntity(entity);
                    change.SetDestinationZoneTag(tag);
                    change.SetDestinationZone(this.FindZoneForEntity(entity));
                    change.SetDestinationControllerId(controllerId);
                    change.SetDestinationPosition(zonePosition);
                    changeList.AddChange(change);
                }
            }
        }
        if (changeList != null)
        {
            this.ProcessLocalChangeList(changeList);
        }
    }

    private void AutoCorrectZonesAfterLocalChange()
    {
        if ((((((!this.HasActiveLocalChange() && !this.HasPendingLocalChange()) && !this.HasActiveServerChange()) && !this.HasPendingServerChange()) && !this.HasPredictedSecrets()) && !this.HasPredictedWeapons()) && (InputManager.Get().GetBattlecrySourceCard() == null))
        {
            this.AutoCorrectZones();
        }
    }

    private void AutoCorrectZonesAfterServerChange()
    {
        if ((((!this.HasActiveLocalChange() && !this.HasPendingLocalChange()) && !this.HasActiveServerChange()) && !this.HasPendingServerChange()) && !this.HasPredictedCards())
        {
            this.AutoCorrectZones();
        }
    }

    private void Awake()
    {
        s_instance = this;
        foreach (Zone zone in base.gameObject.GetComponentsInChildren<Zone>())
        {
            this.m_zones.Add(zone);
        }
        if (GameState.Get() != null)
        {
            GameState.Get().RegisterCurrentPlayerChangedListener(new GameState.CurrentPlayerChangedCallback(this.OnCurrentPlayerChanged));
        }
    }

    private TempZone BuildTempZone(Zone zone)
    {
        TempZone zone2 = new TempZone();
        zone2.SetZone(zone);
        List<Card> cards = zone.GetCards();
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            if (card.GetPredictedZonePosition() == 0)
            {
                Entity entity = card.GetEntity();
                Entity entity2 = this.RegisterTempEntity(entity);
                zone2.AddInitialEntity(entity2);
            }
        }
        return zone2;
    }

    public ZoneChangeList CancelLocalZoneChange(ZoneChangeList changeList, ChangeCompleteCallback callback = null, object userData = null)
    {
        if (changeList == null)
        {
            Debug.LogWarning(string.Format("ZoneMgr.CancelLocalZoneChange() - changeList is null", new object[0]));
            return null;
        }
        if (!this.m_localChangeListHistory.Remove(changeList))
        {
            Debug.LogWarning(string.Format("ZoneMgr.CancelLocalZoneChange() - changeList {0} is not in history", changeList.GetId()));
            return null;
        }
        ZoneChange localTriggerChange = changeList.GetLocalTriggerChange();
        Card triggerCard = localTriggerChange.GetEntity().GetCard();
        Zone sourceZone = localTriggerChange.GetSourceZone();
        int sourcePosition = localTriggerChange.GetSourcePosition();
        ZoneChangeList list = this.CreateLocalChangeList(triggerCard, sourceZone, sourceZone.m_ServerTag, sourcePosition, callback, userData);
        list.SetCanceledChangeList(true);
        list.SetZoneInputBlocking(true);
        this.ProcessOrEnqueueLocalChangeList(list);
        return list;
    }

    private bool CheckAndIgnoreServerChangeList(ZoneChangeList serverChangeList)
    {
        Network.HistActionStart sourceAction = serverChangeList.GetTaskList().GetSourceAction();
        if (sourceAction == null)
        {
            return false;
        }
        if (sourceAction.BlockType != HistoryBlock.Type.PLAY)
        {
            return false;
        }
        ZoneChangeList list2 = this.FindLocalChangeListMatchingServerChangeList(serverChangeList);
        if (list2 == null)
        {
            return false;
        }
        serverChangeList.SetIgnoreCardZoneChanges(true);
        while (this.m_localChangeListHistory.Count > 0)
        {
            ZoneChangeList list3 = this.m_localChangeListHistory.Dequeue();
            if (list2 == list3)
            {
                list2.GetLocalTriggerCard().SetPredictedZonePosition(0);
                break;
            }
        }
        return true;
    }

    private ZoneChangeList CreateLocalChangeList(Card triggerCard, Zone destinationZone, TAG_ZONE destinationZoneTag, int destinationPos, ChangeCompleteCallback callback, object userData)
    {
        int nextLocalChangeListId = this.GetNextLocalChangeListId();
        object[] args = new object[] { nextLocalChangeListId };
        Log.Zone.Print("ZoneMgr.CreateLocalChangeList() - changeListId={0}", args);
        ZoneChangeList parentList = new ZoneChangeList();
        parentList.SetId(nextLocalChangeListId);
        parentList.SetCompleteCallback(callback);
        parentList.SetCompleteCallbackUserData(userData);
        Entity entity = triggerCard.GetEntity();
        Zone zone = triggerCard.GetZone();
        TAG_ZONE tag = (zone != null) ? zone.m_ServerTag : TAG_ZONE.INVALID;
        int zonePosition = triggerCard.GetZonePosition();
        ZoneChange change = new ZoneChange();
        change.SetParentList(parentList);
        change.SetEntity(entity);
        change.SetSourceZone(zone);
        change.SetSourceZoneTag(tag);
        change.SetSourcePosition(zonePosition);
        change.SetDestinationZone(destinationZone);
        change.SetDestinationZoneTag(destinationZoneTag);
        change.SetDestinationPosition(destinationPos);
        parentList.AddChange(change);
        return parentList;
    }

    private void CreateLocalChangesFromTrigger(ZoneChangeList changeList, ZoneChange triggerChange)
    {
        object[] args = new object[] { changeList };
        Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - {0}", args);
        Entity triggerEntity = triggerChange.GetEntity();
        Zone sourceZone = triggerChange.GetSourceZone();
        TAG_ZONE sourceZoneTag = triggerChange.GetSourceZoneTag();
        int sourcePosition = triggerChange.GetSourcePosition();
        Zone destinationZone = triggerChange.GetDestinationZone();
        TAG_ZONE destinationZoneTag = triggerChange.GetDestinationZoneTag();
        int destinationPosition = triggerChange.GetDestinationPosition();
        if (sourceZoneTag != destinationZoneTag)
        {
            this.CreateLocalChangesFromTrigger(changeList, triggerEntity, sourceZone, sourceZoneTag, sourcePosition, destinationZone, destinationZoneTag, destinationPosition);
        }
        else if (sourcePosition != destinationPosition)
        {
            this.CreateLocalPosOnlyChangesFromTrigger(changeList, triggerEntity, sourceZone, sourcePosition, destinationPosition);
        }
    }

    private void CreateLocalChangesFromTrigger(ZoneChangeList changeList, Entity triggerEntity, Zone sourceZone, TAG_ZONE sourceZoneTag, int sourcePos, Zone destinationZone, TAG_ZONE destinationZoneTag, int destinationPos)
    {
        object[] args = new object[] { triggerEntity, sourceZoneTag, sourcePos, destinationZoneTag, destinationPos };
        Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - triggerEntity={0} srcZone={1} srcPos={2} dstZone={3} dstPos={4}", args);
        if (sourcePos != destinationPos)
        {
            object[] objArray2 = new object[] { sourcePos, destinationPos };
            Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - srcPos={0} destPos={1}", objArray2);
        }
        if (sourceZone != null)
        {
            List<Card> cards = sourceZone.GetCards();
            for (int i = sourcePos; i < cards.Count; i++)
            {
                Card card = cards[i];
                Entity entity = card.GetEntity();
                ZoneChange change = new ZoneChange();
                change.SetParentList(changeList);
                change.SetEntity(entity);
                int pos = i;
                change.SetSourcePosition(card.GetZonePosition());
                change.SetDestinationPosition(pos);
                object[] objArray3 = new object[] { card, card.GetZonePosition(), pos };
                Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - srcZone card {0} zonePos {1} -> {2}", objArray3);
                changeList.AddChange(change);
            }
        }
        if ((destinationZone != null) && !(destinationZone is ZoneSecret))
        {
            if (destinationZone is ZoneWeapon)
            {
                List<Card> list2 = destinationZone.GetCards();
                if (list2.Count > 0)
                {
                    Entity entity2 = list2[0].GetEntity();
                    ZoneChange change2 = new ZoneChange();
                    change2.SetParentList(changeList);
                    change2.SetEntity(entity2);
                    change2.SetDestinationZone(this.FindZoneOfType<ZoneGraveyard>(destinationZone.m_Side));
                    change2.SetDestinationZoneTag(TAG_ZONE.GRAVEYARD);
                    changeList.AddChange(change2);
                }
            }
            else if ((destinationZone is ZonePlay) || (destinationZone is ZoneHand))
            {
                List<Card> list3 = destinationZone.GetCards();
                for (int j = destinationPos - 1; j < list3.Count; j++)
                {
                    Card card3 = list3[j];
                    Entity entity3 = card3.GetEntity();
                    int num4 = j + 2;
                    ZoneChange change3 = new ZoneChange();
                    change3.SetParentList(changeList);
                    change3.SetEntity(entity3);
                    change3.SetDestinationPosition(num4);
                    object[] objArray4 = new object[] { card3, entity3.GetZonePosition(), num4 };
                    Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - dstZone card {0} zonePos {1} -> {2}", objArray4);
                    changeList.AddChange(change3);
                }
            }
            else
            {
                Debug.LogError(string.Format("ZoneMgr.CreateLocalChangesFromTrigger() - don't know how to predict zone position changes for zone {0}", destinationZone));
            }
        }
    }

    private void CreateLocalPosOnlyChangesFromTrigger(ZoneChangeList changeList, Entity triggerEntity, Zone sourceZone, int sourcePos, int destinationPos)
    {
        List<Card> cards = sourceZone.GetCards();
        if (sourcePos < destinationPos)
        {
            for (int i = sourcePos; i < destinationPos; i++)
            {
                Card card = cards[i];
                Entity entity = card.GetEntity();
                ZoneChange change = new ZoneChange();
                change.SetParentList(changeList);
                change.SetEntity(entity);
                int pos = i;
                change.SetSourcePosition(card.GetZonePosition());
                change.SetDestinationPosition(pos);
                changeList.AddChange(change);
            }
        }
        else
        {
            for (int j = destinationPos - 1; j < (sourcePos - 1); j++)
            {
                Card card2 = cards[j];
                Entity entity2 = card2.GetEntity();
                ZoneChange change2 = new ZoneChange();
                change2.SetParentList(changeList);
                change2.SetEntity(entity2);
                int num4 = j + 2;
                change2.SetSourcePosition(card2.GetZonePosition());
                change2.SetDestinationPosition(num4);
                changeList.AddChange(change2);
            }
        }
    }

    private ZoneChange CreateZoneChangeFromCreateGame(Network.HistCreateGame createGame)
    {
        ZoneChange change = new ZoneChange();
        change.SetEntity(GameState.Get().GetGameEntity());
        return change;
    }

    private ZoneChange CreateZoneChangeFromFullEntity(Network.HistFullEntity fullEntity)
    {
        Network.Entity entity = fullEntity.Entity;
        Entity entity2 = GameState.Get().GetEntity(entity.ID);
        if (entity2 == null)
        {
            Debug.LogWarning(string.Format("ZoneMgr.CreateZoneChangeFromFullEntity() - WARNING entity {0} DOES NOT EXIST!", entity.ID));
            return null;
        }
        ZoneChange change = new ZoneChange();
        change.SetEntity(entity2);
        if (entity2.GetCard() != null)
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            foreach (Network.Entity.Tag tag in entity.Tags)
            {
                if (tag.Name == 0x31)
                {
                    change.SetDestinationZoneTag((TAG_ZONE) tag.Value);
                    flag = true;
                    if (!flag2 || !flag3)
                    {
                        continue;
                    }
                    break;
                }
                if (tag.Name == 0x107)
                {
                    change.SetDestinationPosition(tag.Value);
                    flag2 = true;
                    if (!flag || !flag3)
                    {
                        continue;
                    }
                    break;
                }
                if (tag.Name == 50)
                {
                    change.SetDestinationControllerId(tag.Value);
                    flag3 = true;
                    if (flag && flag2)
                    {
                        break;
                    }
                }
            }
            if (flag || flag3)
            {
                change.SetDestinationZone(this.FindZoneForEntity(entity2));
            }
        }
        return change;
    }

    private ZoneChange CreateZoneChangeFromHideEntity(Network.HistHideEntity hideEntity)
    {
        Entity entity = GameState.Get().GetEntity(hideEntity.Entity);
        if (entity == null)
        {
            Debug.LogWarning(string.Format("ZoneMgr.CreateZoneChangeFromHideEntity() - WARNING entity {0} DOES NOT EXIST! zone={1}", hideEntity.Entity, hideEntity.Zone));
            return null;
        }
        ZoneChange change = new ZoneChange();
        change.SetEntity(entity);
        if (entity.GetCard() != null)
        {
            Entity entity2 = this.RegisterTempEntity(hideEntity.Entity, entity);
            entity2.SetTag(GAME_TAG.ZONE, hideEntity.Zone);
            TAG_ZONE zone = (TAG_ZONE) hideEntity.Zone;
            change.SetDestinationZoneTag(zone);
            change.SetDestinationZone(this.FindZoneForEntity(entity2));
        }
        return change;
    }

    private ZoneChange CreateZoneChangeFromMetaData(Network.HistMetaData metaData)
    {
        Entity entity = GameState.Get().GetEntity(metaData.Info[0]);
        if (entity == null)
        {
            Debug.LogError(string.Format("ZoneMgr.CreateZoneChangeFromMetaData() - Entity {0} does not exist", metaData.Info[0]));
            return null;
        }
        ZoneChange change = new ZoneChange();
        change.SetEntity(entity);
        return change;
    }

    private ZoneChange CreateZoneChangeFromShowEntity(Network.HistShowEntity showEntity)
    {
        Network.Entity entity = showEntity.Entity;
        Entity entity2 = GameState.Get().GetEntity(entity.ID);
        if (entity2 == null)
        {
            Debug.LogWarning(string.Format("ZoneMgr.CreateZoneChangeFromShowEntity() - WARNING entity {0} DOES NOT EXIST!", entity.ID));
            return null;
        }
        ZoneChange change = new ZoneChange();
        change.SetEntity(entity2);
        if (entity2.GetCard() != null)
        {
            Entity entity3 = this.RegisterTempEntity(entity.ID, entity2);
            foreach (Network.Entity.Tag tag in entity.Tags)
            {
                entity3.SetTag(tag.Name, tag.Value);
            }
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            foreach (Network.Entity.Tag tag2 in entity.Tags)
            {
                entity3.SetTag(tag2.Name, tag2.Value);
                if (tag2.Name == 0x31)
                {
                    change.SetDestinationZoneTag((TAG_ZONE) tag2.Value);
                    flag = true;
                    if (!flag2 || !flag3)
                    {
                        continue;
                    }
                    break;
                }
                if (tag2.Name == 0x107)
                {
                    change.SetDestinationPosition(tag2.Value);
                    flag2 = true;
                    if (!flag || !flag3)
                    {
                        continue;
                    }
                    break;
                }
                if (tag2.Name == 50)
                {
                    change.SetDestinationControllerId(tag2.Value);
                    flag3 = true;
                    if (flag && flag2)
                    {
                        break;
                    }
                }
            }
            if (flag || flag3)
            {
                change.SetDestinationZone(this.FindZoneForEntity(entity3));
            }
        }
        return change;
    }

    private ZoneChange CreateZoneChangeFromTagChange(Network.HistTagChange tagChange)
    {
        Entity entity = GameState.Get().GetEntity(tagChange.Entity);
        if (entity == null)
        {
            Debug.LogError(string.Format("ZoneMgr.CreateZoneChangeFromTagChange() - Entity {0} does not exist", tagChange.Entity));
            return null;
        }
        ZoneChange change = new ZoneChange();
        change.SetEntity(entity);
        Card card = entity.GetCard();
        if (card != null)
        {
            Entity entity2 = this.RegisterTempEntity(tagChange.Entity, entity);
            entity2.SetTag(tagChange.Tag, tagChange.Value);
            if (tagChange.Tag == 0x31)
            {
                if (card == null)
                {
                    Debug.LogError(string.Format("ZoneMgr.CreateZoneChangeFromTagChange() - {0} does not have a card visual", entity));
                    return null;
                }
                TAG_ZONE tag = (TAG_ZONE) tagChange.Value;
                change.SetDestinationZoneTag(tag);
                change.SetDestinationZone(this.FindZoneForEntity(entity2));
                return change;
            }
            if (tagChange.Tag == 0x107)
            {
                if (card == null)
                {
                    Debug.LogError(string.Format("ZoneMgr.CreateZoneChangeFromTagChange() - {0} does not have a card visual", entity));
                    return null;
                }
                change.SetDestinationPosition(tagChange.Value);
                return change;
            }
            if (tagChange.Tag != 50)
            {
                return change;
            }
            if (card == null)
            {
                Debug.LogError(string.Format("ZoneMgr.CreateZoneChangeFromTagChange() - {0} does not have a card visual", entity));
                return null;
            }
            int controllerId = tagChange.Value;
            change.SetDestinationControllerId(controllerId);
            change.SetDestinationZone(this.FindZoneForEntity(entity2));
        }
        return change;
    }

    private int FindBestInsertionPosition(TempZone tempZone, int leftPos, int rightPos)
    {
        int num3;
        int lastPos;
        Zone zone = tempZone.GetZone();
        int pos = 0;
        for (int i = leftPos - 1; i >= 0; i--)
        {
            pos = tempZone.FindEntityPosWithReplacements(zone.GetCardAtIndex(i).GetEntity().GetEntityId());
            if (pos != 0)
            {
                break;
            }
        }
        if (pos == 0)
        {
            num3 = 1;
        }
        else
        {
            int entityId = tempZone.GetEntityAtPos(pos).GetEntityId();
            num3 = pos + 1;
            while (num3 < tempZone.GetLastPos())
            {
                Entity entityAtPos = tempZone.GetEntityAtPos(num3);
                if ((entityAtPos.GetCreatorId() != entityId) || zone.ContainsCard(entityAtPos.GetCard()))
                {
                    break;
                }
                num3++;
            }
        }
        int num5 = 0;
        for (int j = rightPos - 1; j < zone.GetCardCount(); j++)
        {
            num5 = tempZone.FindEntityPosWithReplacements(zone.GetCardAtIndex(j).GetEntity().GetEntityId());
            if (num5 != 0)
            {
                break;
            }
        }
        if (num5 == 0)
        {
            lastPos = tempZone.GetLastPos();
        }
        else
        {
            int num8 = tempZone.GetEntityAtPos(num5).GetEntityId();
            lastPos = num5 - 1;
            while (lastPos > 0)
            {
                Entity entity6 = tempZone.GetEntityAtPos(lastPos);
                if ((entity6.GetCreatorId() != num8) || zone.ContainsCard(entity6.GetCard()))
                {
                    break;
                }
                lastPos--;
            }
            lastPos++;
        }
        return Mathf.CeilToInt(0.5f * (num3 + lastPos));
    }

    private ZoneChangeList FindLocalChangeListMatchingServerChangeList(ZoneChangeList serverChangeList)
    {
        IEnumerator<ZoneChangeList> enumerator = this.m_localChangeListHistory.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                ZoneChangeList current = enumerator.Current;
                int predictedPosition = current.GetPredictedPosition();
                foreach (ZoneChange change in current.GetChanges())
                {
                    Entity entity = change.GetEntity();
                    TAG_ZONE destinationZoneTag = change.GetDestinationZoneTag();
                    if (destinationZoneTag != TAG_ZONE.INVALID)
                    {
                        List<ZoneChange> changes = serverChangeList.GetChanges();
                        for (int i = 0; i < changes.Count; i++)
                        {
                            ZoneChange change2 = changes[i];
                            Entity entity2 = change2.GetEntity();
                            if (entity == entity2)
                            {
                                TAG_ZONE tag_zone2 = change2.GetDestinationZoneTag();
                                if (destinationZoneTag == tag_zone2)
                                {
                                    ZoneChange change3 = this.FindNextDstPosChange(serverChangeList, i, entity2);
                                    int num3 = (change3 != null) ? change3.GetDestinationPosition() : entity2.GetZonePosition();
                                    if (predictedPosition == num3)
                                    {
                                        return current;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
        return null;
    }

    private ZoneChange FindNextDstPosChange(ZoneChangeList changeList, int index, Entity entity)
    {
        List<ZoneChange> changes = changeList.GetChanges();
        for (int i = index; i < changes.Count; i++)
        {
            ZoneChange change = changes[i];
            if (change.HasDestinationZoneChange() && (i != index))
            {
                return null;
            }
            if (change.HasDestinationPosition())
            {
                if (change.GetEntity() != entity)
                {
                    return null;
                }
                return change;
            }
        }
        return null;
    }

    private ZoneChangeList FindRejectedLocalZoneChange(Entity triggerEntity)
    {
        List<ZoneChangeList> list = this.m_localChangeListHistory.GetList();
        for (int i = 0; i < list.Count; i++)
        {
            ZoneChangeList list2 = list[i];
            List<ZoneChange> changes = list2.GetChanges();
            for (int j = 0; j < changes.Count; j++)
            {
                ZoneChange change = changes[j];
                if ((change.GetEntity() == triggerEntity) && (change.GetDestinationZoneTag() == TAG_ZONE.PLAY))
                {
                    return list2;
                }
            }
        }
        return null;
    }

    private TempZone FindTempZoneForZone(Zone zone)
    {
        if (zone == null)
        {
            return null;
        }
        TempZone zone2 = null;
        this.m_tempZoneMap.TryGetValue(zone, out zone2);
        return zone2;
    }

    public ZoneChangeList FindTriggeredActiveLocalChange(Card card)
    {
        int num = this.FindTriggeredActiveLocalChangeIndex(card);
        if (num < 0)
        {
            return null;
        }
        return this.m_pendingLocalChangeLists[num];
    }

    public ZoneChangeList FindTriggeredActiveLocalChange(Entity entity)
    {
        return this.FindTriggeredActiveLocalChange(entity.GetCard());
    }

    private int FindTriggeredActiveLocalChangeIndex(Card card)
    {
        for (int i = 0; i < this.m_activeLocalChangeLists.Count; i++)
        {
            ZoneChangeList list = this.m_activeLocalChangeLists[i];
            if (list.GetLocalTriggerCard() == card)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindTriggeredActiveLocalChangeIndex(Entity entity)
    {
        return this.FindTriggeredActiveLocalChangeIndex(entity.GetCard());
    }

    public ZoneChangeList FindTriggeredPendingLocalChange(Card card)
    {
        int num = this.FindTriggeredPendingLocalChangeIndex(card);
        if (num < 0)
        {
            return null;
        }
        return this.m_pendingLocalChangeLists[num];
    }

    public ZoneChangeList FindTriggeredPendingLocalChange(Entity entity)
    {
        return this.FindTriggeredPendingLocalChange(entity.GetCard());
    }

    private int FindTriggeredPendingLocalChangeIndex(Card card)
    {
        for (int i = 0; i < this.m_pendingLocalChangeLists.Count; i++)
        {
            ZoneChangeList list = this.m_pendingLocalChangeLists[i];
            if (list.GetLocalTriggerCard() == card)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindTriggeredPendingLocalChangeIndex(Entity entity)
    {
        return this.FindTriggeredPendingLocalChangeIndex(entity.GetCard());
    }

    public Zone FindZoneForEntity(Entity entity)
    {
        if (entity.GetZone() != TAG_ZONE.INVALID)
        {
            foreach (Zone zone in this.m_zones)
            {
                if (zone.CanAcceptTags(entity.GetControllerId(), entity.GetZone(), entity.GetCardType()))
                {
                    return zone;
                }
            }
        }
        return null;
    }

    public Zone FindZoneForEntityAndController(Entity entity, int controllerId)
    {
        foreach (Zone zone in this.m_zones)
        {
            if (zone.CanAcceptTags(controllerId, entity.GetZone(), entity.GetCardType()))
            {
                return zone;
            }
        }
        return null;
    }

    public Zone FindZoneForEntityAndZoneTag(Entity entity, TAG_ZONE zoneTag)
    {
        if (zoneTag != TAG_ZONE.INVALID)
        {
            foreach (Zone zone in this.m_zones)
            {
                if (zone.CanAcceptTags(entity.GetControllerId(), zoneTag, entity.GetCardType()))
                {
                    return zone;
                }
            }
        }
        return null;
    }

    public Zone FindZoneForFullEntity(Network.HistFullEntity fullEntity)
    {
        int controllerId = 0;
        TAG_ZONE iNVALID = TAG_ZONE.INVALID;
        TAG_CARDTYPE cardType = TAG_CARDTYPE.INVALID;
        foreach (Network.Entity.Tag tag in fullEntity.Entity.Tags)
        {
            GAME_TAG name = (GAME_TAG) tag.Name;
            if (name != GAME_TAG.ZONE)
            {
                if (name == GAME_TAG.CONTROLLER)
                {
                    goto Label_0061;
                }
                if (name == GAME_TAG.CARDTYPE)
                {
                    goto Label_006E;
                }
            }
            else
            {
                iNVALID = (TAG_ZONE) tag.Value;
            }
            continue;
        Label_0061:
            controllerId = tag.Value;
            continue;
        Label_006E:
            cardType = (TAG_CARDTYPE) tag.Value;
        }
        foreach (Zone zone in this.m_zones)
        {
            if (zone.CanAcceptTags(controllerId, iNVALID, cardType))
            {
                return zone;
            }
        }
        return null;
    }

    public Zone FindZoneForShowEntity(Entity entity, Network.HistShowEntity showEntity)
    {
        int controllerId = entity.GetControllerId();
        TAG_ZONE zoneTag = entity.GetZone();
        TAG_CARDTYPE cardType = entity.GetCardType();
        foreach (Network.Entity.Tag tag in showEntity.Entity.Tags)
        {
            GAME_TAG name = (GAME_TAG) tag.Name;
            if (name != GAME_TAG.ZONE)
            {
                if (name == GAME_TAG.CONTROLLER)
                {
                    goto Label_0070;
                }
                if (name == GAME_TAG.CARDTYPE)
                {
                    goto Label_007D;
                }
            }
            else
            {
                zoneTag = (TAG_ZONE) tag.Value;
            }
            continue;
        Label_0070:
            controllerId = tag.Value;
            continue;
        Label_007D:
            cardType = (TAG_CARDTYPE) tag.Value;
        }
        foreach (Zone zone in this.m_zones)
        {
            if (zone.CanAcceptTags(controllerId, zoneTag, cardType))
            {
                return zone;
            }
        }
        return null;
    }

    public Zone FindZoneForTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType)
    {
        if (controllerId != 0)
        {
            if (zoneTag == TAG_ZONE.INVALID)
            {
                return null;
            }
            foreach (Zone zone in this.m_zones)
            {
                if (zone.CanAcceptTags(controllerId, zoneTag, cardType))
                {
                    return zone;
                }
            }
        }
        return null;
    }

    public T FindZoneOfType<T>(Player.Side side) where T: Zone
    {
        System.Type type = typeof(T);
        foreach (Zone zone in this.m_zones)
        {
            if ((zone.GetType() == type) && (zone.m_Side == side))
            {
                return (T) zone;
            }
        }
        return null;
    }

    public List<Zone> FindZonesForTag(TAG_ZONE zoneTag)
    {
        List<Zone> list = new List<Zone>();
        foreach (Zone zone in this.m_zones)
        {
            if (zone.m_ServerTag == zoneTag)
            {
                list.Add(zone);
            }
        }
        return list;
    }

    public List<ReturnType> FindZonesOfType<ReturnType, ArgType>() where ReturnType: Zone where ArgType: Zone
    {
        List<ReturnType> list = new List<ReturnType>();
        System.Type type = typeof(ArgType);
        foreach (Zone zone in this.m_zones)
        {
            if (zone.GetType() == type)
            {
                list.Add((ReturnType) zone);
            }
        }
        return list;
    }

    public List<T> FindZonesOfType<T>() where T: Zone
    {
        return this.FindZonesOfType<T, T>();
    }

    public List<ReturnType> FindZonesOfType<ReturnType, ArgType>(Player.Side side) where ReturnType: Zone where ArgType: Zone
    {
        List<ReturnType> list = new List<ReturnType>();
        foreach (Zone zone in this.m_zones)
        {
            if ((zone is ArgType) && (zone.m_Side == side))
            {
                list.Add((ReturnType) zone);
            }
        }
        return list;
    }

    public List<T> FindZonesOfType<T>(Player.Side side) where T: Zone
    {
        return this.FindZonesOfType<T, T>(side);
    }

    public static ZoneMgr Get()
    {
        return s_instance;
    }

    private int GetNextLocalChangeListId()
    {
        int nextLocalChangeListId = this.m_nextLocalChangeListId;
        this.m_nextLocalChangeListId = (this.m_nextLocalChangeListId != 0x7fffffff) ? (this.m_nextLocalChangeListId + 1) : 1;
        return nextLocalChangeListId;
    }

    private int GetNextServerChangeListId()
    {
        int nextServerChangeListId = this.m_nextServerChangeListId;
        this.m_nextServerChangeListId = (this.m_nextServerChangeListId != 0x7fffffff) ? (this.m_nextServerChangeListId + 1) : 1;
        return nextServerChangeListId;
    }

    public string GetTweenName<T>() where T: Zone
    {
        System.Type key = typeof(T);
        string str = string.Empty;
        this.m_tweenNames.TryGetValue(key, out str);
        return str;
    }

    public Map<System.Type, string> GetTweenNames()
    {
        return this.m_tweenNames;
    }

    public List<Zone> GetZones()
    {
        return this.m_zones;
    }

    public bool HasActiveLocalChange()
    {
        return (this.m_activeLocalChangeLists.Count > 0);
    }

    public bool HasActiveServerChange()
    {
        return (this.m_activeServerChangeList != null);
    }

    private bool HasLocalChangeExitingZone(Entity entity, Zone zone)
    {
        return (this.HasLocalChangeExitingZone(entity, zone, this.m_activeLocalChangeLists) || this.HasLocalChangeExitingZone(entity, zone, this.m_pendingLocalChangeLists));
    }

    private bool HasLocalChangeExitingZone(Entity entity, Zone zone, List<ZoneChangeList> changeLists)
    {
        TAG_ZONE serverTag = zone.m_ServerTag;
        foreach (ZoneChangeList list in changeLists)
        {
            foreach (ZoneChange change in list.GetChanges())
            {
                if (((entity == change.GetEntity()) && (serverTag == change.GetSourceZoneTag())) && (serverTag != change.GetDestinationZoneTag()))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasPendingLocalChange()
    {
        return (this.m_pendingLocalChangeLists.Count > 0);
    }

    public bool HasPendingServerChange()
    {
        return (this.m_pendingServerChangeLists.Count > 0);
    }

    public bool HasPredictedCards()
    {
        return (this.HasPredictedPositions() || (this.HasPredictedWeapons() || this.HasPredictedSecrets()));
    }

    public bool HasPredictedPositions()
    {
        foreach (Zone zone in this.FindZonesOfType<Zone>(Player.Side.FRIENDLY))
        {
            foreach (Card card in zone.GetCards())
            {
                if (card.GetPredictedZonePosition() != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasPredictedSecrets()
    {
        foreach (ZoneSecret secret in this.FindZonesOfType<ZoneSecret>(Player.Side.FRIENDLY))
        {
            foreach (Card card in secret.GetCards())
            {
                if (card.GetEntity().GetZone() != TAG_ZONE.SECRET)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasPredictedWeapons()
    {
        foreach (ZoneWeapon weapon in this.FindZonesOfType<ZoneWeapon>(Player.Side.FRIENDLY))
        {
            foreach (Card card in weapon.GetCards())
            {
                if (card.GetEntity().GetZone() != TAG_ZONE.PLAY)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasTriggeredActiveLocalChange(Card card)
    {
        return (this.FindTriggeredActiveLocalChangeIndex(card) >= 0);
    }

    public bool HasTriggeredActiveLocalChange(Entity entity)
    {
        return this.HasTriggeredActiveLocalChange(entity.GetCard());
    }

    public bool HasTriggeredPendingLocalChange(Card card)
    {
        return (this.FindTriggeredPendingLocalChangeIndex(card) >= 0);
    }

    public bool HasTriggeredPendingLocalChange(Entity entity)
    {
        return this.HasTriggeredPendingLocalChange(entity.GetCard());
    }

    public bool IsChangeInLocalHistory(ZoneChangeList changeList)
    {
        return this.m_localChangeListHistory.GetList().Contains(changeList);
    }

    public static bool IsHandledPower(Network.PowerHistory power)
    {
        switch (power.Type)
        {
            case Network.PowerType.FULL_ENTITY:
            {
                Network.HistFullEntity entity = power as Network.HistFullEntity;
                bool flag = false;
                foreach (Network.Entity.Tag tag in entity.Entity.Tags)
                {
                    if (tag.Name == 0xca)
                    {
                        if (tag.Value == 1)
                        {
                            return false;
                        }
                        if (tag.Value == 2)
                        {
                            return false;
                        }
                    }
                    else if (((tag.Name == 0x31) || (tag.Name == 0x107)) || (tag.Name == 50))
                    {
                        flag = true;
                    }
                }
                return flag;
            }
            case Network.PowerType.SHOW_ENTITY:
                return true;

            case Network.PowerType.HIDE_ENTITY:
                return true;

            case Network.PowerType.TAG_CHANGE:
            {
                Network.HistTagChange change = power as Network.HistTagChange;
                if (((change.Tag != 0x31) && (change.Tag != 0x107)) && (change.Tag != 50))
                {
                    return false;
                }
                Entity entity2 = GameState.Get().GetEntity(change.Entity);
                if (entity2 != null)
                {
                    if (entity2.IsPlayer())
                    {
                        return false;
                    }
                    if (entity2.IsGame())
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }

    private bool IsZoneInLocalHistory(Zone zone)
    {
        IEnumerator<ZoneChangeList> enumerator = this.m_localChangeListHistory.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                ZoneChangeList current = enumerator.Current;
                foreach (ZoneChange change in current.GetChanges())
                {
                    Zone sourceZone = change.GetSourceZone();
                    Zone destinationZone = change.GetDestinationZone();
                    if ((zone == sourceZone) || (zone == destinationZone))
                    {
                        return true;
                    }
                }
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
        return false;
    }

    private bool MergeServerChangeList(ZoneChangeList serverChangeList)
    {
        foreach (Zone zone in this.m_zones)
        {
            if (this.IsZoneInLocalHistory(zone))
            {
                TempZone zone2 = this.BuildTempZone(zone);
                this.m_tempZoneMap[zone] = zone2;
                zone2.PreprocessChanges();
            }
        }
        List<ZoneChange> changes = serverChangeList.GetChanges();
        for (int i = 0; i < changes.Count; i++)
        {
            ZoneChange change = changes[i];
            this.TempApplyZoneChange(change);
        }
        bool flag = false;
        foreach (TempZone zone3 in this.m_tempZoneMap.Values)
        {
            zone3.Sort();
            zone3.PostprocessChanges();
            Zone zone4 = zone3.GetZone();
            for (int j = 1; j < zone4.GetLastPos(); j++)
            {
                Card cardAtPos = zone4.GetCardAtPos(j);
                Entity entity = cardAtPos.GetEntity();
                if (cardAtPos.GetPredictedZonePosition() != 0)
                {
                    int pos = this.FindBestInsertionPosition(zone3, j - 1, j + 1);
                    zone3.InsertEntityAtPos(pos, entity);
                }
            }
            if (zone3.IsModified())
            {
                flag = true;
                for (int k = 1; k < zone3.GetLastPos(); k++)
                {
                    Entity entity3 = zone3.GetEntityAtPos(k).GetCard().GetEntity();
                    ZoneChange change2 = new ZoneChange();
                    change2.SetEntity(entity3);
                    change2.SetDestinationZone(zone4);
                    change2.SetDestinationZoneTag(zone4.m_ServerTag);
                    change2.SetDestinationPosition(k);
                    serverChangeList.AddChange(change2);
                }
            }
        }
        this.m_tempZoneMap.Clear();
        this.m_tempEntityMap.Clear();
        return flag;
    }

    private void OnCurrentPlayerChanged(Player player, object userData)
    {
        if (player.IsLocalUser())
        {
            this.m_localChangeListHistory.Clear();
        }
    }

    private void OnDestroy()
    {
        if (GameState.Get() != null)
        {
            GameState.Get().UnregisterCurrentPlayerChangedListener(new GameState.CurrentPlayerChangedCallback(this.OnCurrentPlayerChanged));
        }
        s_instance = null;
    }

    public ZoneChangeList OnLocalZoneChangeRejected(Entity triggerEntity)
    {
        ZoneChangeList changeList = this.FindRejectedLocalZoneChange(triggerEntity);
        if (changeList == null)
        {
            object[] args = new object[] { triggerEntity };
            Log.Zone.Print("ZoneMgr.OnLocalZoneChangeRejected() - did not find a zone change to reject for {0}", args);
            return null;
        }
        triggerEntity.GetCard().SetPredictedZonePosition(0);
        return this.CancelLocalZoneChange(changeList, null, null);
    }

    private void PostProcessServerChangeList(ZoneChangeList serverChangeList)
    {
        if ((this.ShouldPostProcessServerChangeList(serverChangeList) && !this.CheckAndIgnoreServerChangeList(serverChangeList)) && !this.ReplaceRemoteWeaponInServerChangeList(serverChangeList))
        {
            this.MergeServerChangeList(serverChangeList);
        }
    }

    private void PredictZoneByApplyingTag(TempZone tempZone, Entity tempEntity, GAME_TAG tag, int val)
    {
        if ((tag != GAME_TAG.ZONE) && (tag != GAME_TAG.CONTROLLER))
        {
            tempEntity.SetTag(tag, val);
        }
        else
        {
            Zone zone = tempZone.GetZone();
            bool flag2 = tempEntity.GetZone() == zone.m_ServerTag;
            bool flag3 = tempEntity.GetControllerId() == zone.GetControllerId();
            if (flag2 && flag3)
            {
                tempZone.RemoveEntity(tempEntity);
            }
            tempEntity.SetTag(tag, val);
            flag2 = tempEntity.GetZone() == zone.m_ServerTag;
            flag3 = tempEntity.GetControllerId() == zone.GetControllerId();
            if (flag2 && flag3)
            {
                tempZone.AddEntity(tempEntity);
            }
        }
    }

    private void PredictZoneFromFullEntity(TempZone tempZone, Network.HistFullEntity fullEntity)
    {
        Entity entity = this.RegisterTempEntity(fullEntity.Entity);
        Zone zone = tempZone.GetZone();
        bool flag = entity.GetZone() == zone.m_ServerTag;
        bool flag2 = entity.GetControllerId() == zone.GetControllerId();
        if (flag && flag2)
        {
            tempZone.AddEntity(entity);
        }
    }

    private void PredictZoneFromHideEntity(TempZone tempZone, Network.HistHideEntity hideEntity)
    {
        Entity tempEntity = this.RegisterTempEntity(hideEntity.Entity);
        this.PredictZoneByApplyingTag(tempZone, tempEntity, GAME_TAG.ZONE, hideEntity.Zone);
    }

    private void PredictZoneFromPower(TempZone tempZone, Network.PowerHistory power)
    {
        switch (power.Type)
        {
            case Network.PowerType.FULL_ENTITY:
                this.PredictZoneFromFullEntity(tempZone, (Network.HistFullEntity) power);
                break;

            case Network.PowerType.SHOW_ENTITY:
                this.PredictZoneFromShowEntity(tempZone, (Network.HistShowEntity) power);
                break;

            case Network.PowerType.HIDE_ENTITY:
                this.PredictZoneFromHideEntity(tempZone, (Network.HistHideEntity) power);
                break;

            case Network.PowerType.TAG_CHANGE:
                this.PredictZoneFromTagChange(tempZone, (Network.HistTagChange) power);
                break;
        }
    }

    private void PredictZoneFromPowerProcessor(TempZone tempZone)
    {
        <PredictZoneFromPowerProcessor>c__AnonStorey2FC storeyfc = new <PredictZoneFromPowerProcessor>c__AnonStorey2FC {
            tempZone = tempZone,
            <>f__this = this
        };
        PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
        storeyfc.tempZone.PreprocessChanges();
        powerProcessor.ForEachTaskList(new Action<int, PowerTaskList>(storeyfc.<>m__10D));
        storeyfc.tempZone.Sort();
        storeyfc.tempZone.PostprocessChanges();
    }

    private void PredictZoneFromPowerTaskList(TempZone tempZone, PowerTaskList taskList)
    {
        List<PowerTask> list = taskList.GetTaskList();
        for (int i = 0; i < list.Count; i++)
        {
            Network.PowerHistory power = list[i].GetPower();
            this.PredictZoneFromPower(tempZone, power);
        }
    }

    private void PredictZoneFromShowEntity(TempZone tempZone, Network.HistShowEntity showEntity)
    {
        Entity tempEntity = this.RegisterTempEntity(showEntity.Entity);
        foreach (Network.Entity.Tag tag in showEntity.Entity.Tags)
        {
            this.PredictZoneByApplyingTag(tempZone, tempEntity, (GAME_TAG) tag.Name, tag.Value);
        }
    }

    private void PredictZoneFromTagChange(TempZone tempZone, Network.HistTagChange tagChange)
    {
        Entity tempEntity = this.RegisterTempEntity(tagChange.Entity);
        this.PredictZoneByApplyingTag(tempZone, tempEntity, (GAME_TAG) tagChange.Tag, tagChange.Value);
    }

    public int PredictZonePosition(Zone zone, int pos)
    {
        TempZone tempZone = this.BuildTempZone(zone);
        this.PredictZoneFromPowerProcessor(tempZone);
        int num = this.FindBestInsertionPosition(tempZone, pos - 1, pos);
        this.m_tempZoneMap.Clear();
        this.m_tempEntityMap.Clear();
        return num;
    }

    private void ProcessLocalChangeList(ZoneChangeList changeList)
    {
        object[] args = new object[] { changeList };
        Log.Zone.Print("ZoneMgr.ProcessLocalChangeList() - [{0}]", args);
        this.m_activeLocalChangeLists.Add(changeList);
        base.StartCoroutine(changeList.ProcessChanges());
    }

    private void ProcessOrEnqueueLocalChangeList(ZoneChangeList changeList)
    {
        ZoneChange localTriggerChange = changeList.GetLocalTriggerChange();
        Card card = localTriggerChange.GetEntity().GetCard();
        if (this.HasTriggeredActiveLocalChange(card))
        {
            this.m_pendingLocalChangeLists.Add(changeList);
        }
        else
        {
            this.CreateLocalChangesFromTrigger(changeList, localTriggerChange);
            this.ProcessLocalChangeList(changeList);
        }
    }

    private Entity RegisterTempEntity(Entity entity)
    {
        return this.RegisterTempEntity(entity.GetEntityId(), entity);
    }

    private Entity RegisterTempEntity(Network.Entity netEnt)
    {
        Entity entity = GameState.Get().GetEntity(netEnt.ID);
        return this.RegisterTempEntity(netEnt.ID, entity);
    }

    private Entity RegisterTempEntity(int id)
    {
        Entity entity = GameState.Get().GetEntity(id);
        return this.RegisterTempEntity(id, entity);
    }

    private Entity RegisterTempEntity(int id, Entity entity)
    {
        Entity entity2;
        if (!this.m_tempEntityMap.TryGetValue(id, out entity2))
        {
            entity2 = entity.CloneForZoneMgr();
            this.m_tempEntityMap.Add(id, entity2);
        }
        return entity2;
    }

    public float RemoveNextDeathBlockLayoutDelaySec()
    {
        float nextDeathBlockLayoutDelaySec = this.m_nextDeathBlockLayoutDelaySec;
        this.m_nextDeathBlockLayoutDelaySec = 0f;
        return nextDeathBlockLayoutDelaySec;
    }

    private bool ReplaceRemoteWeaponInServerChangeList(ZoneChangeList serverChangeList)
    {
        if (<>f__am$cacheD == null)
        {
            <>f__am$cacheD = delegate (ZoneChange change) {
                Zone destinationZone = change.GetDestinationZone();
                if (!(destinationZone is ZoneWeapon))
                {
                    return false;
                }
                if (destinationZone.GetController().IsFriendlySide())
                {
                    return false;
                }
                return true;
            };
        }
        ZoneChange change = serverChangeList.GetChanges().Find(<>f__am$cacheD);
        if (change == null)
        {
            return false;
        }
        Zone zone = change.GetDestinationZone();
        if (zone.GetCardCount() == 0)
        {
            return false;
        }
        Entity entity = zone.GetCardAtIndex(0).GetEntity();
        int controllerId = entity.GetControllerId();
        Zone zone2 = this.FindZoneForTags(controllerId, TAG_ZONE.GRAVEYARD, TAG_CARDTYPE.WEAPON);
        ZoneChange change2 = new ZoneChange();
        change2.SetEntity(entity);
        change2.SetDestinationZone(zone2);
        change2.SetDestinationZoneTag(TAG_ZONE.GRAVEYARD);
        change2.SetDestinationPosition(0);
        serverChangeList.AddChange(change2);
        return true;
    }

    public void RequestNextDeathBlockLayoutDelaySec(float sec)
    {
        this.m_nextDeathBlockLayoutDelaySec = Mathf.Max(this.m_nextDeathBlockLayoutDelaySec, sec);
    }

    private bool ShouldPostProcessServerChangeList(ZoneChangeList changeList)
    {
        List<ZoneChange> changes = changeList.GetChanges();
        for (int i = 0; i < changes.Count; i++)
        {
            ZoneChange change = changes[i];
            if (change.HasDestinationData())
            {
                return true;
            }
        }
        return false;
    }

    private void Start()
    {
        InputManager manager = InputManager.Get();
        if (manager != null)
        {
            manager.StartWatchingForInput();
        }
    }

    private void TempApplyZoneChange(ZoneChange change)
    {
        Network.PowerHistory power = change.GetPowerTask().GetPower();
        Entity entity = change.GetEntity();
        Entity entity2 = this.RegisterTempEntity(entity);
        if (!change.HasDestinationZoneChange())
        {
            GameUtils.ApplyPower(entity2, power);
        }
        else
        {
            Zone zone = this.FindZoneForEntity(entity2);
            TempZone zone2 = this.FindTempZoneForZone(zone);
            if (zone2 != null)
            {
                zone2.RemoveEntity(entity2);
            }
            GameUtils.ApplyPower(entity2, power);
            Zone destinationZone = change.GetDestinationZone();
            TempZone zone4 = this.FindTempZoneForZone(destinationZone);
            if (zone4 != null)
            {
                zone4.AddEntity(entity2);
            }
        }
    }

    private void Update()
    {
        this.UpdateLocalChangeLists();
        this.UpdateServerChangeLists();
    }

    private void UpdateLocalChangeLists()
    {
        List<ZoneChangeList> list = null;
        int index = 0;
        while (index < this.m_activeLocalChangeLists.Count)
        {
            ZoneChangeList item = this.m_activeLocalChangeLists[index];
            if (!item.IsComplete())
            {
                index++;
            }
            else
            {
                item.FireCompleteCallback();
                this.m_activeLocalChangeLists.RemoveAt(index);
                if (list == null)
                {
                    list = new List<ZoneChangeList>();
                }
                list.Add(item);
            }
        }
        if (list != null)
        {
            bool flag = false;
            for (int i = 0; i < list.Count; i++)
            {
                ZoneChangeList list3 = list[i];
                Entity entity = list3.GetLocalTriggerChange().GetEntity();
                Card card = entity.GetCard();
                if (list3.IsCanceledChangeList())
                {
                    flag = true;
                    card.SetPredictedZonePosition(0);
                }
                int num3 = this.FindTriggeredPendingLocalChangeIndex(entity);
                if (num3 >= 0)
                {
                    ZoneChangeList changeList = this.m_pendingLocalChangeLists[num3];
                    this.m_pendingLocalChangeLists.RemoveAt(num3);
                    this.CreateLocalChangesFromTrigger(changeList, changeList.GetLocalTriggerChange());
                    this.ProcessLocalChangeList(changeList);
                }
            }
            if (flag)
            {
                this.AutoCorrectZonesAfterLocalChange();
            }
        }
    }

    private void UpdateServerChangeLists()
    {
        if ((this.m_activeServerChangeList != null) && this.m_activeServerChangeList.IsComplete())
        {
            this.m_activeServerChangeList.FireCompleteCallback();
            this.m_activeServerChangeList = null;
            this.AutoCorrectZonesAfterServerChange();
        }
        if (this.HasPendingServerChange() && !this.HasActiveServerChange())
        {
            this.m_activeServerChangeList = this.m_pendingServerChangeLists.Dequeue();
            this.PostProcessServerChangeList(this.m_activeServerChangeList);
            base.StartCoroutine(this.m_activeServerChangeList.ProcessChanges());
        }
    }

    [CompilerGenerated]
    private sealed class <PredictZoneFromPowerProcessor>c__AnonStorey2FC
    {
        internal ZoneMgr <>f__this;
        internal ZoneMgr.TempZone tempZone;

        internal void <>m__10D(int queueIndex, PowerTaskList taskList)
        {
            this.<>f__this.PredictZoneFromPowerTaskList(this.tempZone, taskList);
        }
    }

    public delegate void ChangeCompleteCallback(ZoneChangeList changeList, object userData);

    private class TempZone
    {
        private List<Entity> m_entities = new List<Entity>();
        private bool m_modified;
        private List<Entity> m_prevEntities = new List<Entity>();
        private Map<int, int> m_replacedEntities = new Map<int, int>();
        private Zone m_zone;

        public void AddEntity(Entity entity)
        {
            if (this.CanAcceptEntity(entity) && !this.m_entities.Contains(entity))
            {
                this.m_entities.Add(entity);
                this.m_modified = true;
            }
        }

        public void AddInitialEntity(Entity entity)
        {
            this.m_entities.Add(entity);
        }

        public bool CanAcceptEntity(Entity entity)
        {
            return (ZoneMgr.Get().FindZoneForEntityAndZoneTag(entity, this.m_zone.m_ServerTag) == this.m_zone);
        }

        public void ClearEntities()
        {
            this.m_entities.Clear();
        }

        public bool ContainsEntity(Entity entity)
        {
            return (this.FindEntityPos(entity) > 0);
        }

        public bool ContainsEntity(int entityId)
        {
            return (this.FindEntityPos(entityId) > 0);
        }

        public int FindEntityPos(Entity entity)
        {
            <FindEntityPos>c__AnonStorey2FD storeyfd = new <FindEntityPos>c__AnonStorey2FD {
                entity = entity
            };
            return (1 + this.m_entities.FindIndex(new Predicate<Entity>(storeyfd.<>m__10F)));
        }

        public int FindEntityPos(int entityId)
        {
            <FindEntityPos>c__AnonStorey2FE storeyfe = new <FindEntityPos>c__AnonStorey2FE {
                entityId = entityId
            };
            return (1 + this.m_entities.FindIndex(new Predicate<Entity>(storeyfe.<>m__110)));
        }

        public int FindEntityPosWithReplacements(int entityId)
        {
            <FindEntityPosWithReplacements>c__AnonStorey2FF storeyff = new <FindEntityPosWithReplacements>c__AnonStorey2FF {
                entityId = entityId
            };
            while (storeyff.entityId != 0)
            {
                int num = 1 + this.m_entities.FindIndex(new Predicate<Entity>(storeyff.<>m__111));
                if (num > 0)
                {
                    return num;
                }
                this.m_replacedEntities.TryGetValue(storeyff.entityId, out storeyff.entityId);
            }
            return 0;
        }

        public List<Entity> GetEntities()
        {
            return this.m_entities;
        }

        public Entity GetEntityAtIndex(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (index >= this.m_entities.Count)
            {
                return null;
            }
            return this.m_entities[index];
        }

        public Entity GetEntityAtPos(int pos)
        {
            return this.GetEntityAtIndex(pos - 1);
        }

        public int GetEntityCount()
        {
            return this.m_entities.Count;
        }

        public int GetLastPos()
        {
            return (this.m_entities.Count + 1);
        }

        public Zone GetZone()
        {
            return this.m_zone;
        }

        public void InsertEntityAtIndex(int index, Entity entity)
        {
            if (((this.CanAcceptEntity(entity) && (index >= 0)) && (index <= this.m_entities.Count)) && ((index >= this.m_entities.Count) || (this.m_entities[index] != entity)))
            {
                this.m_entities.Insert(index, entity);
                this.m_modified = true;
            }
        }

        public void InsertEntityAtPos(int pos, Entity entity)
        {
            int index = pos - 1;
            this.InsertEntityAtIndex(index, entity);
        }

        public bool IsModified()
        {
            return this.m_modified;
        }

        public void PostprocessChanges()
        {
            for (int i = 0; i < this.m_prevEntities.Count; i++)
            {
                <PostprocessChanges>c__AnonStorey300 storey = new <PostprocessChanges>c__AnonStorey300();
                if (i >= this.m_entities.Count)
                {
                    break;
                }
                storey.prevEntity = this.m_prevEntities[i];
                if (this.m_entities.FindIndex(new Predicate<Entity>(storey.<>m__112)) < 0)
                {
                    Entity item = this.m_entities[i];
                    if (!this.m_prevEntities.Contains(item))
                    {
                        this.m_replacedEntities[storey.prevEntity.GetEntityId()] = item.GetEntityId();
                    }
                }
            }
        }

        public void PreprocessChanges()
        {
            this.m_prevEntities.Clear();
            for (int i = 0; i < this.m_entities.Count; i++)
            {
                this.m_prevEntities.Add(this.m_entities[i]);
            }
        }

        public bool RemoveEntity(Entity entity)
        {
            if (!this.m_entities.Remove(entity))
            {
                return false;
            }
            this.m_modified = true;
            return true;
        }

        public void SetZone(Zone zone)
        {
            this.m_zone = zone;
        }

        public void Sort()
        {
            if (this.m_modified)
            {
                this.m_entities.Sort(new Comparison<Entity>(this.SortComparison));
            }
            else
            {
                Entity[] entityArray = this.m_entities.ToArray();
                this.m_entities.Sort(new Comparison<Entity>(this.SortComparison));
                for (int i = 0; i < this.m_entities.Count; i++)
                {
                    if (entityArray[i] != this.m_entities[i])
                    {
                        this.m_modified = true;
                        break;
                    }
                }
            }
        }

        private int SortComparison(Entity entity1, Entity entity2)
        {
            int zonePosition = entity1.GetZonePosition();
            int num2 = entity2.GetZonePosition();
            return (zonePosition - num2);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1} entities)", this.m_zone, this.m_entities.Count);
        }

        [CompilerGenerated]
        private sealed class <FindEntityPos>c__AnonStorey2FD
        {
            internal Entity entity;

            internal bool <>m__10F(Entity currEntity)
            {
                return (currEntity == this.entity);
            }
        }

        [CompilerGenerated]
        private sealed class <FindEntityPos>c__AnonStorey2FE
        {
            internal int entityId;

            internal bool <>m__110(Entity currEntity)
            {
                return (currEntity.GetEntityId() == this.entityId);
            }
        }

        [CompilerGenerated]
        private sealed class <FindEntityPosWithReplacements>c__AnonStorey2FF
        {
            internal int entityId;

            internal bool <>m__111(Entity currEntity)
            {
                return (currEntity.GetEntityId() == this.entityId);
            }
        }

        [CompilerGenerated]
        private sealed class <PostprocessChanges>c__AnonStorey300
        {
            internal Entity prevEntity;

            internal bool <>m__112(Entity currEntity)
            {
                return (currEntity == this.prevEntity);
            }
        }
    }
}

