using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class CollectionCardCache
{
    private List<CardCacheItem> m_cardCache = new List<CardCacheItem>();
    private Map<string, CardCacheRequest> m_cardCacheRequests = new Map<string, CardCacheRequest>();
    private const int MAX_CARD_CACHE_SIZE = 60;
    private static CollectionCardCache s_instance;

    private CollectionCardCache()
    {
    }

    private void AddCard(string cardID, CardDef card)
    {
        if (this.GetCardDef(cardID) != null)
        {
            Log.Rachelle.Print(string.Format("CollectionCardCache.AddCard(): somehow the card def for {0} was already in the cache...", cardID), new object[0]);
        }
        else
        {
            CardCacheItem item = new CardCacheItem {
                m_cardId = cardID,
                m_cardDef = card
            };
            item.m_lastRequestTimestamp = TimeUtils.BinaryStamp();
            this.m_cardCache.Add(item);
            this.CleanCache();
        }
    }

    private bool CanClearItem(CardCacheItem item, CollectionDeck currentlyOpenDeck, List<string> deckHelperCardChoices)
    {
        if ((currentlyOpenDeck != null) && (currentlyOpenDeck.GetCardIdCount(item.m_cardId) > 0))
        {
            return false;
        }
        if ((deckHelperCardChoices != null) && deckHelperCardChoices.Contains(item.m_cardId))
        {
            return false;
        }
        return true;
    }

    private void CleanCache()
    {
        int num = this.m_cardCache.Count - 60;
        if (num > 0)
        {
            CollectionDeck currentlyOpenDeck = null;
            if (CollectionManager.Get() != null)
            {
                currentlyOpenDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
            }
            List<string> deckHelperCardChoices = (DeckHelper.Get() != null) ? DeckHelper.Get().GetCardIDChoices() : null;
            List<string> names = new List<string>();
            int index = 0;
            while (index < this.m_cardCache.Count)
            {
                CardCacheItem item = this.m_cardCache[index];
                if (!this.CanClearItem(item, currentlyOpenDeck, deckHelperCardChoices))
                {
                    index++;
                }
                else
                {
                    names.Add(item.m_cardId);
                    this.m_cardCache.RemoveAt(index);
                    DefLoader.Get().ClearCardDef(item.m_cardId);
                    if (names.Count == num)
                    {
                        break;
                    }
                }
            }
            AssetCache.ClearCardPrefabs(names);
        }
    }

    public static CollectionCardCache Get()
    {
        if (s_instance == null)
        {
            s_instance = new CollectionCardCache();
        }
        return s_instance;
    }

    public CardDef GetCardDef(string cardId)
    {
        for (int i = 0; i < this.m_cardCache.Count; i++)
        {
            CardCacheItem item = this.m_cardCache[i];
            if (item.m_cardId.Equals(cardId))
            {
                item.m_lastRequestTimestamp = TimeUtils.BinaryStamp();
                this.m_cardCache.RemoveAt(i);
                this.m_cardCache.Add(item);
                return item.m_cardDef;
            }
        }
        return null;
    }

    public void LoadCardDef(string cardID, LoadCardDefCallback callback, object callbackData = null, CardPortraitQuality portraitQuality = null)
    {
        if (portraitQuality == null)
        {
            portraitQuality = CardPortraitQuality.GetDefault();
        }
        CardDef cardDef = this.GetCardDef(cardID);
        if ((cardDef != null) && (cardDef.GetPortraitQuality() >= portraitQuality))
        {
            callback(cardID, cardDef, callbackData);
        }
        else
        {
            DefLoader.Get().LoadCardDef(cardID, new DefLoader.LoadDefCallback<CardDef>(this.OnCardPrefabLoaded), new CallbackData(callback, callbackData), portraitQuality);
        }
    }

    private void OnCardPrefabLoaded(string cardID, CardDef cardDef, object callbackData)
    {
        if (cardDef == null)
        {
            Debug.LogError(string.Format("CollectionCardCache.OnCardPrefabLoaded() - asset for card {0} has no CardDef!", cardID));
        }
        else
        {
            this.AddCard(cardID, cardDef);
            CallbackData data = (CallbackData) callbackData;
            data.callback(cardID, cardDef, data.callbackData);
        }
    }

    public void Unload()
    {
        this.m_cardCacheRequests.Clear();
        List<string> names = new List<string>();
        foreach (CardCacheItem item in this.m_cardCache)
        {
            names.Add(item.m_cardId);
        }
        this.m_cardCache.Clear();
        AssetCache.ClearCardPrefabs(names);
    }

    protected class CallbackData
    {
        public CollectionCardCache.LoadCardDefCallback callback;
        public object callbackData;

        public CallbackData(CollectionCardCache.LoadCardDefCallback cb, object data)
        {
            this.callback = cb;
            this.callbackData = data;
        }
    }

    private class CardCacheItem
    {
        public CardDef m_cardDef;
        public string m_cardId;
        public long m_lastRequestTimestamp;
    }

    private class CardCacheRequest
    {
        public List<Requester> m_requesters = new List<Requester>();

        public void AddRequester(CollectionCardCache.LoadCardDefCallback callback, object callbackData)
        {
            if (callback != null)
            {
                Requester item = new Requester {
                    m_callback = callback,
                    m_callbackData = callbackData
                };
                this.m_requesters.Add(item);
            }
        }

        public class Requester
        {
            public CollectionCardCache.LoadCardDefCallback m_callback;
            public object m_callbackData;
        }
    }

    public delegate void LoadCardDefCallback(string cardID, CardDef cardDef, object callbackData);
}

