using System;
using System.Collections.Generic;

public class PowerTask
{
    private bool m_completed;
    private Network.PowerHistory m_power;

    public void DoEarlyConcedeTask()
    {
        if (!this.m_completed)
        {
            GameState state = GameState.Get();
            switch (this.m_power.Type)
            {
                case Network.PowerType.SHOW_ENTITY:
                {
                    Network.HistShowEntity power = (Network.HistShowEntity) this.m_power;
                    state.OnEarlyConcedeShowEntity(power);
                    break;
                }
                case Network.PowerType.HIDE_ENTITY:
                {
                    Network.HistHideEntity hideEntity = (Network.HistHideEntity) this.m_power;
                    state.OnEarlyConcedeHideEntity(hideEntity);
                    break;
                }
                case Network.PowerType.TAG_CHANGE:
                {
                    Network.HistTagChange netChange = (Network.HistTagChange) this.m_power;
                    state.OnEarlyConcedeTagChange(netChange);
                    break;
                }
            }
            this.m_completed = true;
        }
    }

    public void DoRealTimeTask(List<Network.PowerHistory> powerList, int index)
    {
        GameState state = GameState.Get();
        switch (this.m_power.Type)
        {
            case Network.PowerType.FULL_ENTITY:
            {
                Network.HistFullEntity power = (Network.HistFullEntity) this.m_power;
                state.OnRealTimeFullEntity(power);
                break;
            }
            case Network.PowerType.SHOW_ENTITY:
            {
                Network.HistShowEntity showEntity = (Network.HistShowEntity) this.m_power;
                state.OnRealTimeShowEntity(showEntity);
                break;
            }
            case Network.PowerType.TAG_CHANGE:
            {
                Network.HistTagChange change = (Network.HistTagChange) this.m_power;
                state.OnRealTimeTagChange(change);
                break;
            }
            case Network.PowerType.CREATE_GAME:
            {
                Network.HistCreateGame createGame = (Network.HistCreateGame) this.m_power;
                state.OnRealTimeCreateGame(powerList, index, createGame);
                break;
            }
        }
    }

    public void DoTask()
    {
        if (!this.m_completed)
        {
            GameState state = GameState.Get();
            switch (this.m_power.Type)
            {
                case Network.PowerType.FULL_ENTITY:
                {
                    Network.HistFullEntity power = (Network.HistFullEntity) this.m_power;
                    state.OnFullEntity(power);
                    break;
                }
                case Network.PowerType.SHOW_ENTITY:
                {
                    Network.HistShowEntity showEntity = (Network.HistShowEntity) this.m_power;
                    state.OnShowEntity(showEntity);
                    break;
                }
                case Network.PowerType.HIDE_ENTITY:
                {
                    Network.HistHideEntity hideEntity = (Network.HistHideEntity) this.m_power;
                    state.OnHideEntity(hideEntity);
                    break;
                }
                case Network.PowerType.TAG_CHANGE:
                {
                    Network.HistTagChange netChange = (Network.HistTagChange) this.m_power;
                    state.OnTagChange(netChange);
                    break;
                }
                case Network.PowerType.META_DATA:
                {
                    Network.HistMetaData metaData = (Network.HistMetaData) this.m_power;
                    state.OnMetaData(metaData);
                    break;
                }
            }
            this.m_completed = true;
        }
    }

    public Network.PowerHistory GetPower()
    {
        return this.m_power;
    }

    private string GetPrintableEntity(Network.Entity netEntity)
    {
        Entity entity = GameState.Get().GetEntity(netEntity.ID);
        string str = (entity != null) ? entity.GetName() : null;
        if (str == null)
        {
            return string.Format("[id={0} cardId={2}]", netEntity.ID, netEntity.CardID);
        }
        return string.Format("[id={0} cardId={1} name={2}]", netEntity.ID, netEntity.CardID, str);
    }

    private string GetPrintableEntity(int entityId)
    {
        Entity entity = GameState.Get().GetEntity(entityId);
        if (entity == null)
        {
            return entityId.ToString();
        }
        string name = entity.GetName();
        if (name == null)
        {
            return string.Format("[id={0} cardId={1}]", entityId, entity.GetCardId());
        }
        return string.Format("[id={0} cardId={1} name={2}]", entityId, entity.GetCardId(), name);
    }

    public bool IsCompleted()
    {
        return this.m_completed;
    }

    public void SetCompleted(bool complete)
    {
        this.m_completed = complete;
    }

    public void SetPower(Network.PowerHistory power)
    {
        this.m_power = power;
    }

    public override string ToString()
    {
        string str = "null";
        if (this.m_power != null)
        {
            switch (this.m_power.Type)
            {
                case Network.PowerType.FULL_ENTITY:
                {
                    Network.HistFullEntity power = (Network.HistFullEntity) this.m_power;
                    str = string.Format("type={0} entity={1} tags={2}", this.m_power.Type, this.GetPrintableEntity(power.Entity), power.Entity.Tags);
                    break;
                }
                case Network.PowerType.SHOW_ENTITY:
                {
                    Network.HistShowEntity entity2 = (Network.HistShowEntity) this.m_power;
                    str = string.Format("type={0} entity={1} tags={2}", this.m_power.Type, this.GetPrintableEntity(entity2.Entity), entity2.Entity.Tags);
                    break;
                }
                case Network.PowerType.HIDE_ENTITY:
                {
                    Network.HistHideEntity entity3 = (Network.HistHideEntity) this.m_power;
                    str = string.Format("type={0} entity={1} zone={2}", this.m_power.Type, this.GetPrintableEntity(entity3.Entity), entity3.Zone);
                    break;
                }
                case Network.PowerType.TAG_CHANGE:
                {
                    Network.HistTagChange change = (Network.HistTagChange) this.m_power;
                    str = string.Format("type={0} entity={1} {2}", this.m_power.Type, this.GetPrintableEntity(change.Entity), Tags.DebugTag(change.Tag, change.Value));
                    break;
                }
                case Network.PowerType.CREATE_GAME:
                    str = ((Network.HistCreateGame) this.m_power).ToString();
                    break;

                case Network.PowerType.META_DATA:
                    str = ((Network.HistMetaData) this.m_power).ToString();
                    break;
            }
        }
        return string.Format("power=[{0}] complete={1}", str, this.m_completed);
    }
}

