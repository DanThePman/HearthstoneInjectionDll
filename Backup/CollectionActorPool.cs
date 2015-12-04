using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class CollectionActorPool
{
    private readonly TAG_CARDTYPE[] ACTOR_CARD_TYPES = new TAG_CARDTYPE[] { TAG_CARDTYPE.MINION, TAG_CARDTYPE.SPELL, TAG_CARDTYPE.WEAPON };
    private Map<Actor, ActorKey> m_actorKeyMap;
    private Map<ActorKey, List<Actor>> m_actorMap;
    private int m_actorsPreloaded;
    private int m_actorsToPreload;
    private readonly PlatformDependentValue<bool> PRELOAD_ACTORS;

    public CollectionActorPool()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            PC = false,
            Mac = false,
            iOS = false,
            Android = false
        };
        this.PRELOAD_ACTORS = value2;
        this.m_actorMap = new Map<ActorKey, List<Actor>>();
        this.m_actorKeyMap = new Map<Actor, ActorKey>();
    }

    public bool AcquireActor(EntityDef entityDef, CardFlair flair, AcquireActorCallback callback, object callbackData)
    {
        if (entityDef == null)
        {
            Debug.LogError("Cannot acquire actor; entityDef is null!");
            return false;
        }
        bool owned = CollectionManager.Get().IsCardInCollection(entityDef.GetCardId(), flair);
        ActorKey key = this.MakeActorKey(entityDef, flair, owned);
        string heroSkinOrHandActor = ActorNames.GetHeroSkinOrHandActor(entityDef.GetCardType(), flair.Premium);
        ActorLoadCallbackData data = new ActorLoadCallbackData {
            m_key = key,
            m_owned = owned,
            m_entityDef = entityDef,
            m_cardFlair = flair,
            m_callback = callback,
            m_callbackData = callbackData
        };
        AssetLoader.Get().LoadActor(heroSkinOrHandActor, new AssetLoader.GameObjectCallback(this.OnActorLoaded), data, false);
        return true;
    }

    private ActorKey FindActorKey(Actor actor)
    {
        ActorKey key;
        this.m_actorKeyMap.TryGetValue(actor, out key);
        return key;
    }

    public void Initialize()
    {
        foreach (TAG_CARDTYPE tag_cardtype in this.ACTOR_CARD_TYPES)
        {
            IEnumerator enumerator = Enum.GetValues(typeof(TAG_PREMIUM)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    TAG_PREMIUM current = (TAG_PREMIUM) ((int) enumerator.Current);
                    ActorKey key = this.MakeActorKey(tag_cardtype, current, true);
                    this.m_actorMap.Add(key, new List<Actor>());
                    ActorKey key2 = this.MakeActorKey(tag_cardtype, current, false);
                    this.m_actorMap.Add(key2, new List<Actor>());
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable == null)
                {
                }
                disposable.Dispose();
            }
        }
        if (this.PRELOAD_ACTORS != null)
        {
            this.Preload(TAG_CARDTYPE.MINION, TAG_PREMIUM.NORMAL, true, 1);
            this.Preload(TAG_CARDTYPE.SPELL, TAG_PREMIUM.NORMAL, true, 1);
            this.Preload(TAG_CARDTYPE.WEAPON, TAG_PREMIUM.NORMAL, true, 1);
        }
    }

    public bool IsReady()
    {
        return (this.m_actorsPreloaded >= this.m_actorsToPreload);
    }

    private ActorKey MakeActorKey(EntityDef entityDef, CardFlair flair, bool owned)
    {
        return this.MakeActorKey(entityDef.GetCardType(), CardFlair.GetPremiumType(flair), owned);
    }

    private ActorKey MakeActorKey(TAG_CARDTYPE cardType, TAG_PREMIUM premiumType, bool owned)
    {
        return new ActorKey { m_cardType = cardType, m_owned = owned, m_premiumType = premiumType };
    }

    private void MissingCardDisplay(Actor actor, string cardID, TAG_PREMIUM premium)
    {
        NetCache.CardValue value2;
        int buy = -1;
        NetCache.NetCacheCardValues netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardValues>();
        NetCache.CardDefinition key = new NetCache.CardDefinition {
            Name = cardID,
            Premium = premium
        };
        if (netObject.Values.TryGetValue(key, out value2))
        {
            buy = value2.Buy;
        }
        long balance = NetCache.Get().GetNetObject<NetCache.NetCacheArcaneDustBalance>().Balance;
        if (((buy > 0) && (buy <= balance)) && FixedRewardsMgr.Get().CanCraftCard(cardID, premium))
        {
            if (!actor.isGhostCard())
            {
                actor.GhostCardEffect(true);
            }
        }
        else
        {
            CollectionManagerDisplay display = CollectionManagerDisplay.Get();
            if (!actor.MissingCardEffect())
            {
                if (premium == TAG_PREMIUM.GOLDEN)
                {
                    actor.OverrideAllMeshMaterials(display.GetGoldenCardNotOwnedMeshMaterial());
                }
                else
                {
                    actor.OverrideAllMeshMaterials(display.GetCardNotOwnedMeshMaterial());
                }
            }
        }
    }

    private void OnActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            Debug.LogWarning(string.Format("CollectionActorPool.OnActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                Debug.LogWarning(string.Format("CollectionActorPool.OnActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                component.TurnOffCollider();
                ActorLoadCallbackData data = (ActorLoadCallbackData) callbackData;
                component.SetEntityDef(data.m_entityDef);
                component.SetCardFlair(data.m_cardFlair);
                if (!data.m_owned)
                {
                    string cardId = data.m_entityDef.GetCardId();
                    this.MissingCardDisplay(component, cardId, data.m_cardFlair.Premium);
                }
                data.m_callback(component, data.m_callbackData);
            }
        }
    }

    private void OnActorPreloaded(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_actorsPreloaded++;
        if (actorObject == null)
        {
            Debug.LogWarning(string.Format("CollectionActorPool.OnActorPreloaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                Debug.LogWarning(string.Format("CollectionActorPool.OnActorPreloaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                actorObject.transform.position = new Vector3(-9999f, -9999f, 9999f);
                component.TurnOffCollider();
                ActorLoadCallbackData data = (ActorLoadCallbackData) callbackData;
                component.SetCardFlair(data.m_cardFlair);
                this.m_actorKeyMap.Add(component, data.m_key);
                if (!data.m_owned)
                {
                    string cardId = data.m_entityDef.GetCardId();
                    this.MissingCardDisplay(component, cardId, data.m_cardFlair.Premium);
                }
                this.ReleaseActor(component);
            }
        }
    }

    public void Preload(TAG_CARDTYPE type, TAG_PREMIUM premium, bool owned, int num)
    {
    }

    public void ReleaseActor(Actor actor)
    {
        UnityEngine.Object.Destroy(actor.gameObject);
    }

    public void Unload()
    {
    }

    public delegate void AcquireActorCallback(Actor actor, object callbackData);

    private class ActorKey
    {
        public TAG_CARDTYPE m_cardType;
        public bool m_owned;
        public TAG_PREMIUM m_premiumType;

        public bool Equals(CollectionActorPool.ActorKey other)
        {
            if (other == null)
            {
                return false;
            }
            return (((this.m_cardType == other.m_cardType) && (this.m_owned == other.m_owned)) && (this.m_premiumType == other.m_premiumType));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            CollectionActorPool.ActorKey other = obj as CollectionActorPool.ActorKey;
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            int num = 0x17;
            num = (num * 0x11) + this.m_cardType.GetHashCode();
            num = (num * 0x11) + this.m_owned.GetHashCode();
            return ((num * 0x11) + this.m_premiumType.GetHashCode());
        }

        public static bool operator ==(CollectionActorPool.ActorKey a, CollectionActorPool.ActorKey b)
        {
            return (object.ReferenceEquals(a, b) || (((a != null) && (b != null)) && a.Equals(b)));
        }

        public static bool operator !=(CollectionActorPool.ActorKey a, CollectionActorPool.ActorKey b)
        {
            return !(a == b);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ActorLoadCallbackData
    {
        public CollectionActorPool.ActorKey m_key;
        public bool m_owned;
        public EntityDef m_entityDef;
        public CardFlair m_cardFlair;
        public CollectionActorPool.AcquireActorCallback m_callback;
        public object m_callbackData;
    }
}

