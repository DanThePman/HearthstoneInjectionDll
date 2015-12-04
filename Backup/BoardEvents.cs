using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class BoardEvents : MonoBehaviour
{
    private const float AI_PROCESS_INTERVAL = 3.5f;
    private const float FAST_PROCESS_INTERVAL = 0.15f;
    private LinkedList<EventData> m_events = new LinkedList<EventData>();
    private LinkedList<EventData> m_fastEvents = new LinkedList<EventData>();
    private List<EventCallback> m_friendlyHeroDamageCallacks;
    private List<EventCallback> m_friendlyHeroHealCallbacks;
    private List<EventCallback> m_friendlyMinionDamageCallacks;
    private List<EventCallback> m_friendlyMinionHealCallbacks;
    private List<EventCallback> m_frindlyLegendaryMinionDeathCallbacks;
    private List<EventCallback> m_frindlyLegendaryMinionSpawnCallbacks;
    private List<EventCallback> m_frindlyMinionDeathCallbacks;
    private List<EventCallback> m_frindlyMinionSpawnCallbacks;
    private List<LargeShakeEventDelegate> m_largeShakeEventCallbacks;
    private float m_nextFastProcessTime;
    private float m_nextProcessTime;
    private List<EventCallback> m_opponentHeroDamageCallacks;
    private List<EventCallback> m_opponentHeroHealCallbacks;
    private List<EventCallback> m_opponentLegendaryMinionDeathCallbacks;
    private List<EventCallback> m_opponentLegendaryMinionSpawnCallbacks;
    private List<EventCallback> m_opponentMinionDamageCallacks;
    private List<EventCallback> m_opponentMinionDeathCallbacks;
    private List<EventCallback> m_opponentMinionHealCallbacks;
    private List<EventCallback> m_opponentMinionSpawnCallbacks;
    private List<LinkedListNode<EventData>> m_removeEvents = new List<LinkedListNode<EventData>>();
    private Dictionary<EVENT_TYPE, float> m_weights = new Dictionary<EVENT_TYPE, float>();
    private const float PROCESS_INTERVAL = 1.25f;
    private static BoardEvents s_instance;

    private void AddWeight(EVENT_TYPE eventType, float weight)
    {
        if (this.m_weights.ContainsKey(eventType))
        {
            Dictionary<EVENT_TYPE, float> dictionary;
            EVENT_TYPE event_type;
            float num = dictionary[event_type];
            (dictionary = this.m_weights)[event_type = eventType] = num + weight;
        }
        else
        {
            this.m_weights.Add(eventType, weight);
        }
    }

    private void Awake()
    {
        s_instance = this;
    }

    private void CallbackEvent(List<EventCallback> eventList, float weight)
    {
        if (eventList != null)
        {
            for (int i = eventList.Count - 1; i >= 0; i--)
            {
                if (eventList[i] == null)
                {
                    eventList.RemoveAt(i);
                }
                else if (weight >= eventList[i].minimumWeight)
                {
                    eventList[i].callback(weight);
                }
            }
        }
    }

    public void DamageEvent(Card targetCard, float damage)
    {
        Entity entity = targetCard.GetEntity();
        if (entity != null)
        {
            EventData data = new EventData {
                m_card = targetCard,
                m_timeStamp = UnityEngine.Time.timeSinceLevelLoad
            };
            if (entity.IsHero())
            {
                if (targetCard.GetControllerSide() == Player.Side.FRIENDLY)
                {
                    data.m_eventType = EVENT_TYPE.FriendlyHeroDamage;
                }
                else
                {
                    data.m_eventType = EVENT_TYPE.OpponentHeroDamage;
                }
            }
            else if (targetCard.GetControllerSide() == Player.Side.FRIENDLY)
            {
                data.m_eventType = EVENT_TYPE.FriendlyMinionDamage;
            }
            else
            {
                data.m_eventType = EVENT_TYPE.OpponentMinionDamage;
            }
            data.m_value = damage;
            data.m_rarity = entity.GetRarity();
            this.m_events.AddLast(data);
        }
    }

    public void DeathEvent(Card card)
    {
        Entity entity = card.GetEntity();
        if (entity != null)
        {
            EventData data = new EventData {
                m_card = card,
                m_timeStamp = UnityEngine.Time.timeSinceLevelLoad
            };
            if (entity.GetRarity() == TAG_RARITY.LEGENDARY)
            {
                if (card.GetControllerSide() == Player.Side.FRIENDLY)
                {
                    data.m_eventType = EVENT_TYPE.FriendlyLegendaryMinionDeath;
                }
                else
                {
                    data.m_eventType = EVENT_TYPE.OpponentLegendaryMinionDeath;
                }
            }
            else if (card.GetControllerSide() == Player.Side.FRIENDLY)
            {
                data.m_eventType = EVENT_TYPE.FriendlyMinionDeath;
            }
            else
            {
                data.m_eventType = EVENT_TYPE.OpponentMinionDeath;
            }
            data.m_value = entity.GetOriginalCost();
            data.m_rarity = entity.GetRarity();
            this.m_events.AddLast(data);
        }
    }

    public static BoardEvents Get()
    {
        if (s_instance == null)
        {
            Board board = Board.Get();
            if (board == null)
            {
                return null;
            }
            s_instance = board.gameObject.AddComponent<BoardEvents>();
        }
        return s_instance;
    }

    public void HealEvent(Card targetCard, float health)
    {
        Entity entity = targetCard.GetEntity();
        if (entity != null)
        {
            EventData data = new EventData {
                m_card = targetCard,
                m_timeStamp = UnityEngine.Time.timeSinceLevelLoad
            };
            if (entity.IsHero())
            {
                if (targetCard.GetControllerSide() == Player.Side.FRIENDLY)
                {
                    data.m_eventType = EVENT_TYPE.FriendlyHeroHeal;
                }
                else
                {
                    data.m_eventType = EVENT_TYPE.OpponentHeroHeal;
                }
            }
            else if (targetCard.GetControllerSide() == Player.Side.FRIENDLY)
            {
                data.m_eventType = EVENT_TYPE.FriendlyMinionHeal;
            }
            else
            {
                data.m_eventType = EVENT_TYPE.OpponentMinionHeal;
            }
            data.m_value = health;
            data.m_rarity = entity.GetRarity();
            this.m_events.AddLast(data);
        }
    }

    private void LargeShakeEvent()
    {
        if (this.m_largeShakeEventCallbacks != null)
        {
            for (int i = this.m_largeShakeEventCallbacks.Count - 1; i >= 0; i--)
            {
                if (this.m_largeShakeEventCallbacks[i] == null)
                {
                    this.m_largeShakeEventCallbacks.RemoveAt(i);
                }
                else
                {
                    this.m_largeShakeEventCallbacks[i]();
                }
            }
        }
    }

    public void MinionShakeEvent(ShakeMinionIntensity shakeIntensity, float customIntensity)
    {
        if (shakeIntensity == ShakeMinionIntensity.LargeShake)
        {
            EventData data = new EventData {
                m_timeStamp = UnityEngine.Time.timeSinceLevelLoad,
                m_eventType = EVENT_TYPE.LargeMinionShake
            };
            this.m_fastEvents.AddLast(data);
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void ProcessEvents()
    {
        if (this.m_events.Count == 0)
        {
            return;
        }
        LinkedListNode<EventData> first = this.m_events.First;
        while (first != null)
        {
            EventData data = first.Value;
            LinkedListNode<EventData> item = first;
            first = first.Next;
            if ((data.m_timeStamp + 3.5f) < UnityEngine.Time.timeSinceLevelLoad)
            {
                this.m_removeEvents.Add(item);
            }
            else
            {
                this.AddWeight(data.m_eventType, data.m_value);
            }
        }
        for (int i = 0; i < this.m_removeEvents.Count; i++)
        {
            LinkedListNode<EventData> node = this.m_removeEvents[i];
            if (node != null)
            {
                this.m_events.Remove(node);
            }
        }
        this.m_removeEvents.Clear();
        if (this.m_weights.Count == 0)
        {
            return;
        }
        EVENT_TYPE? nullable = null;
        float weight = -1f;
        foreach (EVENT_TYPE event_type in this.m_weights.Keys)
        {
            if (this.m_weights[event_type] >= weight)
            {
                nullable = new EVENT_TYPE?(event_type);
                weight = this.m_weights[event_type];
            }
        }
        if (!nullable.HasValue)
        {
            return;
        }
        if (nullable.HasValue)
        {
            switch (nullable.Value)
            {
                case EVENT_TYPE.FriendlyHeroDamage:
                    this.CallbackEvent(this.m_friendlyHeroDamageCallacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.OpponentHeroDamage:
                    this.CallbackEvent(this.m_opponentHeroDamageCallacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.FriendlyHeroHeal:
                    this.CallbackEvent(this.m_friendlyHeroHealCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.OpponentHeroHeal:
                    this.CallbackEvent(this.m_opponentHeroHealCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.FriendlyLegendaryMinionSpawn:
                    this.CallbackEvent(this.m_frindlyLegendaryMinionSpawnCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.OpponentLegendaryMinionSpawn:
                    this.CallbackEvent(this.m_opponentLegendaryMinionSpawnCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.FriendlyLegendaryMinionDeath:
                    this.CallbackEvent(this.m_frindlyLegendaryMinionDeathCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.OpponentLegendaryMinionDeath:
                    this.CallbackEvent(this.m_opponentLegendaryMinionDeathCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.FriendlyMinionSpawn:
                    this.CallbackEvent(this.m_frindlyMinionSpawnCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.OpponentMinionSpawn:
                    this.CallbackEvent(this.m_opponentMinionSpawnCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.FriendlyMinionDeath:
                    this.CallbackEvent(this.m_frindlyMinionDeathCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.OpponentMinionDeath:
                    this.CallbackEvent(this.m_opponentMinionDeathCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.FriendlyMinionDamage:
                    this.CallbackEvent(this.m_friendlyMinionDamageCallacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.OpponentMinionDamage:
                    this.CallbackEvent(this.m_opponentMinionDamageCallacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.FriendlyMinionHeal:
                    this.CallbackEvent(this.m_friendlyMinionHealCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;

                case EVENT_TYPE.OpponentMinionHeal:
                    this.CallbackEvent(this.m_opponentMinionHealCallbacks, weight);
                    this.m_events.Clear();
                    goto Label_03BE;
            }
        }
        Debug.LogWarning(string.Format("BoardEvents: Event type unknown when processing event weights: {0}", nullable));
    Label_03BE:
        this.m_weights.Clear();
    }

    private void ProcessImmediateEvents()
    {
        if ((this.m_fastEvents.Count != 0) && (this.m_largeShakeEventCallbacks != null))
        {
            LinkedListNode<EventData> first = this.m_fastEvents.First;
            while (first != null)
            {
                EventData data = first.Value;
                LinkedListNode<EventData> item = first;
                first = first.Next;
                if ((data.m_timeStamp + 0.15f) < UnityEngine.Time.timeSinceLevelLoad)
                {
                    this.m_removeEvents.Add(item);
                }
                else if (data.m_eventType == EVENT_TYPE.LargeMinionShake)
                {
                    this.AddWeight(EVENT_TYPE.LargeMinionShake, 1f);
                    this.m_removeEvents.Add(item);
                }
            }
            for (int i = 0; i < this.m_removeEvents.Count; i++)
            {
                LinkedListNode<EventData> node = this.m_removeEvents[i];
                if (node != null)
                {
                    this.m_fastEvents.Remove(node);
                }
            }
            this.m_removeEvents.Clear();
            if (this.m_weights.ContainsKey(EVENT_TYPE.LargeMinionShake) && (this.m_weights[EVENT_TYPE.LargeMinionShake] > 0f))
            {
                this.LargeShakeEvent();
            }
            this.m_weights.Clear();
        }
    }

    private List<EventCallback> RegisterEvent(List<EventCallback> eventList, EventDelegate callback, float minimumWeight)
    {
        if (eventList == null)
        {
            eventList = new List<EventCallback>();
        }
        EventCallback item = new EventCallback {
            callback = callback,
            minimumWeight = minimumWeight
        };
        eventList.Add(item);
        return eventList;
    }

    public void RegisterFriendlyHeroDamageEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_friendlyHeroDamageCallacks = this.RegisterEvent(this.m_friendlyHeroDamageCallacks, callback, minimumWeight);
    }

    public void RegisterFriendlyHeroHealEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_friendlyHeroHealCallbacks = this.RegisterEvent(this.m_friendlyHeroHealCallbacks, callback, minimumWeight);
    }

    public void RegisterFriendlyLegendaryMinionDeathEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_frindlyLegendaryMinionDeathCallbacks = this.RegisterEvent(this.m_frindlyLegendaryMinionDeathCallbacks, callback, minimumWeight);
    }

    public void RegisterFriendlyLegendaryMinionSpawnEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_frindlyLegendaryMinionSpawnCallbacks = this.RegisterEvent(this.m_frindlyLegendaryMinionSpawnCallbacks, callback, minimumWeight);
    }

    public void RegisterFriendlyMinionDamageEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_friendlyMinionDamageCallacks = this.RegisterEvent(this.m_friendlyMinionDamageCallacks, callback, minimumWeight);
    }

    public void RegisterFriendlyMinionDeathEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_frindlyMinionDeathCallbacks = this.RegisterEvent(this.m_frindlyMinionDeathCallbacks, callback, minimumWeight);
    }

    public void RegisterFriendlyMinionHealEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_friendlyMinionHealCallbacks = this.RegisterEvent(this.m_friendlyMinionHealCallbacks, callback, minimumWeight);
    }

    public void RegisterFriendlyMinionSpawnEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_frindlyMinionSpawnCallbacks = this.RegisterEvent(this.m_frindlyMinionSpawnCallbacks, callback, minimumWeight);
    }

    public void RegisterLargeShakeEvent(LargeShakeEventDelegate callback)
    {
        if (this.m_largeShakeEventCallbacks == null)
        {
            this.m_largeShakeEventCallbacks = new List<LargeShakeEventDelegate>();
        }
        this.m_largeShakeEventCallbacks.Add(callback);
    }

    public void RegisterOppenentLegendaryMinionDeathEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_opponentLegendaryMinionDeathCallbacks = this.RegisterEvent(this.m_opponentLegendaryMinionDeathCallbacks, callback, minimumWeight);
    }

    public void RegisterOppenentLegendaryMinionSpawnEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_opponentLegendaryMinionSpawnCallbacks = this.RegisterEvent(this.m_opponentLegendaryMinionSpawnCallbacks, callback, minimumWeight);
    }

    public void RegisterOppenentMinionDeathEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_opponentMinionDeathCallbacks = this.RegisterEvent(this.m_opponentMinionDeathCallbacks, callback, minimumWeight);
    }

    public void RegisterOppenentMinionSpawnEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_opponentMinionSpawnCallbacks = this.RegisterEvent(this.m_opponentMinionSpawnCallbacks, callback, minimumWeight);
    }

    public void RegisterOpponentHeroDamageEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_opponentHeroDamageCallacks = this.RegisterEvent(this.m_opponentHeroDamageCallacks, callback, minimumWeight);
    }

    public void RegisterOpponentHeroHealEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_opponentHeroHealCallbacks = this.RegisterEvent(this.m_opponentHeroHealCallbacks, callback, minimumWeight);
    }

    public void RegisterOpponentMinionDamageEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_opponentMinionDamageCallacks = this.RegisterEvent(this.m_opponentMinionDamageCallacks, callback, minimumWeight);
    }

    public void RegisterOpponentMinionHealEvent(EventDelegate callback, float minimumWeight = 1)
    {
        this.m_opponentMinionHealCallbacks = this.RegisterEvent(this.m_opponentMinionHealCallbacks, callback, minimumWeight);
    }

    private void Start()
    {
    }

    public void SummonedEvent(Card minionCard)
    {
        Entity entity = minionCard.GetEntity();
        if (entity != null)
        {
            EventData data = new EventData {
                m_card = minionCard,
                m_timeStamp = UnityEngine.Time.timeSinceLevelLoad
            };
            if (entity.GetRarity() == TAG_RARITY.LEGENDARY)
            {
                if (minionCard.GetControllerSide() == Player.Side.FRIENDLY)
                {
                    data.m_eventType = EVENT_TYPE.FriendlyLegendaryMinionSpawn;
                }
                else
                {
                    data.m_eventType = EVENT_TYPE.OpponentLegendaryMinionSpawn;
                }
            }
            else if (minionCard.GetControllerSide() == Player.Side.FRIENDLY)
            {
                data.m_eventType = EVENT_TYPE.FriendlyMinionSpawn;
            }
            else
            {
                data.m_eventType = EVENT_TYPE.OpponentMinionSpawn;
            }
            data.m_value = entity.GetOriginalCost();
            data.m_rarity = entity.GetRarity();
            this.m_events.AddLast(data);
        }
    }

    private void Update()
    {
        if (UnityEngine.Time.timeSinceLevelLoad > this.m_nextFastProcessTime)
        {
            this.m_nextFastProcessTime = UnityEngine.Time.timeSinceLevelLoad + 0.15f;
            this.ProcessImmediateEvents();
        }
        else if (UnityEngine.Time.timeSinceLevelLoad > this.m_nextProcessTime)
        {
            this.m_nextProcessTime = UnityEngine.Time.timeSinceLevelLoad + 1.25f;
            this.ProcessEvents();
        }
    }

    public enum EVENT_TYPE
    {
        FriendlyHeroDamage,
        OpponentHeroDamage,
        FriendlyHeroHeal,
        OpponentHeroHeal,
        FriendlyLegendaryMinionSpawn,
        OpponentLegendaryMinionSpawn,
        FriendlyLegendaryMinionDeath,
        OpponentLegendaryMinionDeath,
        FriendlyMinionSpawn,
        OpponentMinionSpawn,
        FriendlyMinionDeath,
        OpponentMinionDeath,
        FriendlyMinionDamage,
        OpponentMinionDamage,
        FriendlyMinionHeal,
        OpponentMinionHeal,
        LargeMinionShake
    }

    public class EventCallback
    {
        public BoardEvents.EventDelegate callback;
        public float minimumWeight;
    }

    public class EventData
    {
        public Card m_card;
        public BoardEvents.EVENT_TYPE m_eventType;
        public TAG_RARITY m_rarity;
        public float m_timeStamp;
        public float m_value;
    }

    public delegate void EventDelegate(float weight);

    public delegate void LargeShakeEventDelegate();
}

