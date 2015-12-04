#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;

#endregion 

#region " Referenced assemblies "
// - mscorlib v2.0.5.0
// - Assembly-CSharp-firstpass v0.0.0.0
// - System.Core v2.0.5.0
// - UnityEngine v0.0.0.0
// - PlayMaker v1.6.0.0
// - System v2.0.5.0
// - ICSharpCode.SharpZipLib v0.86.0.518
// - IFacebook v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - HearthstoneInjectionDll v0.0.0.0
// - Assembly-CSharp v0.0.0.0
// - HearthstoneInjectionDll v1.0.0.0
// - HearthstoneInjectionDll v0.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class CollectionManager
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void OnDeck()
    {
        Network.DeckContents deckContents = Network.GetDeckContents();
        if (this.m_pendingRequestDeckContents != null)
        {
            this.m_pendingRequestDeckContents.Remove(deckContents.Deck);
        }
        CollectionDeck deck = this.GetDeck(deckContents.Deck);
        CollectionDeck baseDeck = this.GetBaseDeck(deckContents.Deck);
        if ((deck == null) || (baseDeck == null))
        {
            //Debug.LogError(string.Format("No contents returned for deck {0}!", deckContents.Deck));
        }
        else
        {
            HearthstoneInjectionDll.DeckContainer.currentDeckId = deck.ID;
            HearthstoneInjectionDll.DeckContainer.currentDeckName = deck.Name;

            deck.ClearSlotContents();
            baseDeck.ClearSlotContents();
            foreach (Network.CardUserData data in deckContents.Cards)
            {
                string cardID = GameUtils.TranslateDbIdToCardId(data.DbId);
                if (cardID == null)
                {
                    //Debug.LogError(string.Format("CollectionManager.OnDeck(): Could not find card with asset ID {0} in our card manifest", data.DbId));
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        HearthstoneInjectionDll.DeckContainer.LoadCard(cardID);
                        deck.AddCard(cardID, data.Premium);
                        baseDeck.AddCard(cardID, data.Premium);
                    }
                }
            }
            HearthstoneInjectionDll.DeckManager.CreateDeckFile();
            deck.MarkNetworkContentsLoaded();
        }
        this.FireDeckContentsEvent(deckContents.Deck);
        foreach (CollectionDeck deck3 in this.GetDecks().Values)
        {
            if (!deck3.NetworkContentsLoaded())
            {
                return;
            }
        }
        this.FireAllDeckContentsEvent();
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    CollectionManager()
    {
    }

    void RegisterCardStacks()
    {
    }

    void OnCardSale()
    {
    }

    void OnMassDisenchantResponse()
    {
    }

    void OnSetFavoriteHeroResponse()
    {
    }

    void NetCache_OnFavoriteHeroesReceived()
    {
    }

    void UpdateFavoriteHero(TAG_CLASS heroClass, string heroCardId, TAG_PREMIUM premium)
    {
    }

    void OnDefaultCardBackSet()
    {
    }

    void OnDBAction()
    {
    }

    void OnDeckCreated()
    {
    }

    void OnDeckDeleted()
    {
    }

    void OnDeckRenamed()
    {
    }

    static void Init()
    {
    }

    static CollectionManager Get()
    {
        return default(CollectionManager);
    }

    bool IsFullyLoaded()
    {
        return default(bool);
    }

    void RegisterCollectionNetHandlers()
    {
    }

    void RemoveCollectionNetHandlers()
    {
    }

    int GetMaxNumCustomDecks()
    {
        return default(int);
    }

    bool HasVisitedCollection()
    {
        return default(bool);
    }

    void SetHasVisitedCollection(bool enable)
    {
    }

    bool IsWaitingForBoxTransition()
    {
        return default(bool);
    }

    void NotifyOfBoxTransitionStart()
    {
    }

    void OnBoxTransitionFinished(object userData)
    {
    }

    bool CanDisplayCards()
    {
        return default(bool);
    }

    void AddCardReward(CardRewardData cardReward, bool markAsNew)
    {
    }

    void AddCardRewards(System.Collections.Generic.List<CardRewardData> cardRewards, bool markAsNew)
    {
    }

    int EntityDefSortComparison(EntityDef entityDef1, EntityDef entityDef2)
    {
        return default(int);
    }

    int GetCardTypeSortOrder(EntityDef entityDef)
    {
        return default(int);
    }

    int GetBasicCardsIOwn(TAG_CLASS cardClass)
    {
        return default(int);
    }

    bool AllCardsInSetOwned(System.Nullable<TAG_CARD_SET> cardSet, System.Nullable<TAG_CLASS> cardClass, System.Nullable<TAG_RARITY> cardRarity, System.Nullable<TAG_RACE> cardRace, CardFlair flair, bool allCopiesRequired)
    {
        return default(bool);
    }

    System.Collections.Generic.Dictionary<string, CollectionCardStack> GetCollectibleStacks()
    {
        return default(System.Collections.Generic.Dictionary<string, CollectionCardStack>);
    }

    System.Collections.Generic.List<CollectionCardStack> GetOwnedCardStacks()
    {
        return default(System.Collections.Generic.List<CollectionCardStack>);
    }

    void GetOwnedCardCount(string cardId, ref int standard, ref int golden)
    {
    }

    System.Collections.Generic.List<TAG_CARD_SET> GetDisplayableCardSets()
    {
        return default(System.Collections.Generic.List<TAG_CARD_SET>);
    }

    bool IsCardInCollection(string cardID, CardFlair cardFlair)
    {
        return default(bool);
    }

    int GetNumCopiesInCollection(string cardID, TAG_PREMIUM premium)
    {
        return default(int);
    }

    int GetCardsToDisenchantCount()
    {
        return default(int);
    }

    void MarkAllInstancesAsSeen(string cardID, CardFlair cardFlair)
    {
    }

    void OnCardRewardOpened(string cardID, TAG_PREMIUM premium, int count)
    {
    }

    System.Collections.Generic.SortedDictionary<long, CollectionDeck> GetDecks()
    {
        return default(System.Collections.Generic.SortedDictionary<long, CollectionDeck>);
    }

    System.Collections.Generic.List<CollectionDeck> GetDecks(PegasusShared.DeckType deckType)
    {
        return default(System.Collections.Generic.List<CollectionDeck>);
    }

    CollectionDeck GetDeck(long id)
    {
        return default(CollectionDeck);
    }

    bool IsInEditMode()
    {
        return default(bool);
    }

    void DoneEditing()
    {
    }

    void SendCreateDeck(PegasusShared.DeckType deckType, string name, string heroCardID)
    {
    }

    void SendDeleteDeck(long id)
    {
    }

    void RequestDeckContents(long id)
    {
    }

    CollectionDeck GetBaseDeck(long id)
    {
        return default(CollectionDeck);
    }

    string AutoGenerateDeckName(TAG_CLASS classTag)
    {
        return default(string);
    }

    string GetVanillaHeroCardIDFromClass(TAG_CLASS heroClass)
    {
        return default(string);
    }

    string GetVanillaHeroCardID(EntityDef HeroSkinEntityDef)
    {
        return default(string);
    }

    void InitImpl()
    {
    }

    void WillReset()
    {
    }

    void OnCollectionChanged()
    {
    }

    System.Collections.Generic.List<string> GetCardIDsInSet(System.Nullable<TAG_CARD_SET> cardSet, System.Nullable<TAG_CLASS> cardClass, System.Nullable<TAG_RARITY> cardRarity, System.Nullable<TAG_RACE> cardRace)
    {
        return default(System.Collections.Generic.List<string>);
    }

    int NumCardsOwnedInSet(TAG_CARD_SET cardSet)
    {
        return default(int);
    }

    CollectionCardStack GetCollectionStack(string cardID)
    {
        return default(CollectionCardStack);
    }

    void RegisterHeroCardStack(EntityDef entityDef, string cardID, CardFlair cardFlair, System.DateTime insertDate, int count, int numSeen)
    {
    }

    void RegisterHeroCardFlair(string heroCardID, CardFlair cardFlair)
    {
    }

    CollectionCardStack RegisterEmptyCardStack(string cardID)
    {
        return default(CollectionCardStack);
    }

    bool RegisterCardStack(EntityDef entityDef, string cardID, CardFlair cardFlair, System.DateTime insertDate, int count, int numSeen)
    {
        return default(bool);
    }

    void AddPreconDeck(TAG_CLASS heroClass, long deckID, CardFlair heroFlair)
    {
    }

    void RemoveDeck(long id)
    {
    }

    bool IsDeckNameTaken(string name)
    {
        return default(bool);
    }

    void FireDeckContentsEvent(long id)
    {
    }

    void FireAllDeckContentsEvent()
    {
    }

    void OnNetCacheReady()
    {
    }

    void UpdateShowAdvancedCMOption()
    {
    }

    bool HasUnlockedAllHeores()
    {
        return default(bool);
    }

    void UpdateDeckHeroArt(string heroCardID)
    {
    }

    void InsertNewCollectionCard(string cardID, CardFlair cardFlair, System.DateTime insertDate, int count, bool seenBefore)
    {
    }

    void InsertNewCollectionCards(System.Collections.Generic.List<string> cardIDs, System.Collections.Generic.List<CardFlair> cardFlairs, System.Collections.Generic.List<System.DateTime> insertDates, System.Collections.Generic.List<int> counts, bool seenBefore)
    {
    }

    void RemoveCollectionCard(string cardID, CardFlair cardFlair, int count)
    {
    }

    void OnActiveAchievesUpdated(object userData)
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static int NUM_CARDS_GRANTED_POST_TUTORIAL;
    static int NUM_CARDS_TO_UNLOCK_ADVANCED_CM;
    static int NUM_EXPERT_CARDS_TO_UNLOCK_CRAFTING;
    static int NUM_EXPERT_CARDS_TO_UNLOCK_FORGE;
    static int NUM_BASIC_CARDS_PER_CLASS;
    static CollectionManager s_instance;
    bool m_unloading;
    bool m_cardStacksRegistered;
    bool m_collectionLoaded;
    Map<string, System.Collections.Generic.List<CardFlair>> m_heroFlair;
    System.Collections.Generic.Dictionary<string, CollectionCardStack> m_collectibleStacks;
    Map<long, CollectionDeck> m_decks;
    Map<long, CollectionDeck> m_baseDecks;
    System.Collections.Generic.List<TAG_CARD_SET> m_displayableCardSets;
    Map<long, float> m_pendingRequestDeckContents;

    bool m_waitingForBoxTransition;
    bool m_hasVisitedCollection;
    bool m_editMode;
    #endregion

}
