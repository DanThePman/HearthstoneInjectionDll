using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class StateEventTable : MonoBehaviour
{
    [CustomEditField(Sections="Event Table", ListTable=true)]
    public List<StateEvent> m_Events = new List<StateEvent>();
    private string m_LastState;
    private QueueList<QueueStateEvent> m_QueuedEvents = new QueueList<QueueStateEvent>();
    private Map<string, List<StateEventTrigger>> m_StateEventEndListeners = new Map<string, List<StateEventTrigger>>();
    private Map<string, List<StateEventTrigger>> m_StateEventEndOnceListeners = new Map<string, List<StateEventTrigger>>();
    private Map<string, List<StateEventTrigger>> m_StateEventStartListeners = new Map<string, List<StateEventTrigger>>();
    private Map<string, List<StateEventTrigger>> m_StateEventStartOnceListeners = new Map<string, List<StateEventTrigger>>();

    public void AddStateEventEndListener(string eventName, StateEventTrigger dlg, bool once = false)
    {
        this.AddStateEventListener(!once ? this.m_StateEventEndListeners : this.m_StateEventEndOnceListeners, eventName, dlg);
    }

    private void AddStateEventListener(Map<string, List<StateEventTrigger>> listenerDict, string eventName, StateEventTrigger dlg)
    {
        List<StateEventTrigger> list;
        if (!listenerDict.TryGetValue(eventName, out list))
        {
            list = new List<StateEventTrigger>();
            listenerDict[eventName] = list;
        }
        list.Add(dlg);
    }

    public void AddStateEventStartListener(string eventName, StateEventTrigger dlg, bool once = false)
    {
        this.AddStateEventListener(!once ? this.m_StateEventStartListeners : this.m_StateEventStartOnceListeners, eventName, dlg);
    }

    public void CancelQueuedStates()
    {
        this.m_QueuedEvents.Clear();
    }

    private void FireStateEventFinishedEvent(Map<string, List<StateEventTrigger>> listenerDict, QueueStateEvent stateEvt, bool clear = false)
    {
        List<StateEventTrigger> list;
        if (listenerDict.TryGetValue(stateEvt.GetEventName(), out list))
        {
            foreach (StateEventTrigger trigger in list.ToArray())
            {
                trigger(stateEvt.m_StateEvent.m_Event);
            }
            if (clear)
            {
                list.Clear();
            }
        }
    }

    public PlayMakerFSM GetFSMFromEvent(string evtName)
    {
        Spell spellEvent = this.GetSpellEvent(evtName);
        if (spellEvent != null)
        {
            return spellEvent.GetComponent<PlayMakerFSM>();
        }
        return null;
    }

    public string GetLastState()
    {
        return this.m_LastState;
    }

    public Spell GetSpellEvent(string eventName)
    {
        StateEvent stateEvent = this.GetStateEvent(eventName);
        if (stateEvent != null)
        {
            return stateEvent.m_Event;
        }
        return null;
    }

    protected StateEvent GetStateEvent(string eventName)
    {
        <GetStateEvent>c__AnonStorey2AA storeyaa = new <GetStateEvent>c__AnonStorey2AA {
            eventName = eventName
        };
        return this.m_Events.Find(new Predicate<StateEvent>(storeyaa.<>m__40));
    }

    public bool HasState(string eventName)
    {
        <HasState>c__AnonStorey2A8 storeya = new <HasState>c__AnonStorey2A8 {
            eventName = eventName
        };
        return (this.m_Events.Find(new Predicate<StateEvent>(storeya.<>m__3C)) != null);
    }

    private void QueueNextState(Spell spell, SpellStateType prevStateType, object thisStateEvent)
    {
        if (this.m_QueuedEvents.Count != 0)
        {
            this.m_QueuedEvents.Dequeue();
            this.StartNextQueuedState((QueueStateEvent) thisStateEvent);
        }
    }

    public void RemoveStateEventEndListener(string eventName, StateEventTrigger dlg)
    {
        this.RemoveStateEventListener(this.m_StateEventEndListeners, eventName, dlg);
    }

    private void RemoveStateEventListener(Map<string, List<StateEventTrigger>> listenerDict, string eventName, StateEventTrigger dlg)
    {
        List<StateEventTrigger> list;
        if (listenerDict.TryGetValue(eventName, out list))
        {
            list.Remove(dlg);
        }
    }

    public void RemoveStateEventStartListener(string eventName, StateEventTrigger dlg)
    {
        this.RemoveStateEventListener(this.m_StateEventStartListeners, eventName, dlg);
    }

    public void SetBoolVar(string eventName, string varName, bool value)
    {
        PlayMakerFSM fSMFromEvent = this.GetFSMFromEvent(eventName);
        if (fSMFromEvent != null)
        {
            fSMFromEvent.FsmVariables.GetFsmBool(varName).Value = value;
        }
    }

    public void SetFloatVar(string eventName, string varName, float value)
    {
        PlayMakerFSM fSMFromEvent = this.GetFSMFromEvent(eventName);
        if (fSMFromEvent != null)
        {
            fSMFromEvent.FsmVariables.GetFsmFloat(varName).Value = value;
        }
    }

    public void SetGameObjectVar(string eventName, string varName, Component value)
    {
        PlayMakerFSM fSMFromEvent = this.GetFSMFromEvent(eventName);
        if (fSMFromEvent != null)
        {
            fSMFromEvent.FsmVariables.GetFsmGameObject(varName).Value = value.gameObject;
        }
    }

    public void SetGameObjectVar(string eventName, string varName, GameObject value)
    {
        PlayMakerFSM fSMFromEvent = this.GetFSMFromEvent(eventName);
        if (fSMFromEvent != null)
        {
            fSMFromEvent.FsmVariables.GetFsmGameObject(varName).Value = value;
        }
    }

    public void SetIntVar(string eventName, string varName, int value)
    {
        PlayMakerFSM fSMFromEvent = this.GetFSMFromEvent(eventName);
        if (fSMFromEvent != null)
        {
            fSMFromEvent.FsmVariables.GetFsmInt(varName).Value = value;
        }
    }

    public void SetVar(string eventName, string varName, object value)
    {
        <SetVar>c__AnonStorey2A9 storeya = new <SetVar>c__AnonStorey2A9 {
            eventName = eventName,
            varName = varName,
            value = value,
            <>f__this = this
        };
        if (storeya.value is GameObject)
        {
            this.SetGameObjectVar(storeya.eventName, storeya.varName, (GameObject) storeya.value);
        }
        else if (storeya.value is Component)
        {
            this.SetGameObjectVar(storeya.eventName, storeya.varName, (Component) storeya.value);
        }
        else
        {
            System.Action action;
            Map<System.Type, System.Action> map2 = new Map<System.Type, System.Action>();
            map2.Add(typeof(float), new System.Action(storeya.<>m__3D));
            map2.Add(typeof(int), new System.Action(storeya.<>m__3E));
            map2.Add(typeof(bool), new System.Action(storeya.<>m__3F));
            Map<System.Type, System.Action> map = map2;
            if (map.TryGetValue(storeya.value.GetType(), out action))
            {
                action();
            }
            else
            {
                Debug.LogError(string.Format("Set var type ({0}) not supported.", storeya.value.GetType()));
            }
        }
    }

    public void SetVector3Var(string eventName, string varName, Vector3 value)
    {
        PlayMakerFSM fSMFromEvent = this.GetFSMFromEvent(eventName);
        if (fSMFromEvent != null)
        {
            fSMFromEvent.FsmVariables.GetFsmVector3(varName).Value = value;
        }
    }

    private void StartNextQueuedState(QueueStateEvent lastEvt)
    {
        if (this.m_QueuedEvents.Count == 0)
        {
            if (lastEvt != null)
            {
                this.FireStateEventFinishedEvent(this.m_StateEventEndListeners, lastEvt, false);
                this.FireStateEventFinishedEvent(this.m_StateEventEndOnceListeners, lastEvt, true);
            }
        }
        else
        {
            QueueStateEvent userData = this.m_QueuedEvents.Peek();
            StateEvent stateEvent = userData.m_StateEvent;
            if (userData.m_SaveAsLastState)
            {
                this.m_LastState = userData.GetEventName();
            }
            stateEvent.m_Event.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.QueueNextState), userData);
            this.FireStateEventFinishedEvent(this.m_StateEventStartListeners, userData, false);
            this.FireStateEventFinishedEvent(this.m_StateEventStartOnceListeners, userData, true);
            stateEvent.m_Event.Activate();
        }
    }

    public void TriggerState(string eventName, bool saveLastState = true, string nameOverride = null)
    {
        StateEvent stateEvent = this.GetStateEvent(eventName);
        if (stateEvent == null)
        {
            Debug.LogError(string.Format("{0} not defined in event table.", eventName), base.gameObject);
        }
        else
        {
            QueueStateEvent item = new QueueStateEvent {
                m_StateEvent = stateEvent,
                m_NameOverride = nameOverride,
                m_SaveAsLastState = saveLastState
            };
            this.m_QueuedEvents.Enqueue(item);
            if (this.m_QueuedEvents.Count == 1)
            {
                this.StartNextQueuedState(null);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <GetStateEvent>c__AnonStorey2AA
    {
        internal string eventName;

        internal bool <>m__40(StateEventTable.StateEvent e)
        {
            return (e.m_Name == this.eventName);
        }
    }

    [CompilerGenerated]
    private sealed class <HasState>c__AnonStorey2A8
    {
        internal string eventName;

        internal bool <>m__3C(StateEventTable.StateEvent e)
        {
            return (e.m_Name == this.eventName);
        }
    }

    [CompilerGenerated]
    private sealed class <SetVar>c__AnonStorey2A9
    {
        internal StateEventTable <>f__this;
        internal string eventName;
        internal object value;
        internal string varName;

        internal void <>m__3D()
        {
            this.<>f__this.SetFloatVar(this.eventName, this.varName, (float) this.value);
        }

        internal void <>m__3E()
        {
            this.<>f__this.SetIntVar(this.eventName, this.varName, (int) this.value);
        }

        internal void <>m__3F()
        {
            this.<>f__this.SetBoolVar(this.eventName, this.varName, (bool) this.value);
        }
    }

    protected class QueueStateEvent
    {
        public string m_NameOverride;
        public bool m_SaveAsLastState = true;
        public StateEventTable.StateEvent m_StateEvent;

        public string GetEventName()
        {
            return (!string.IsNullOrEmpty(this.m_NameOverride) ? this.m_NameOverride : this.m_StateEvent.m_Name);
        }
    }

    [Serializable]
    public class StateEvent
    {
        public Spell m_Event;
        public string m_Name;
    }

    public delegate void StateEventTrigger(Spell evt);
}

