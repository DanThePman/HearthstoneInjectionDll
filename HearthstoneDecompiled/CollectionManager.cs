using PegasusShared;
using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class CollectionManager
{
    [CompilerGenerated]
    private static Func<KeyValuePair<string, CollectionCardStack>, string> <>f__am$cache1C;
    [CompilerGenerated]
    private static Func<KeyValuePair<string, CollectionCardStack>, CollectionCardStack> <>f__am$cache1D;
    [CompilerGenerated]
    private static Predicate<NetCache.ProfileNotice> <>f__am$cache1E;
    private List<DelOnAchievesCompleted> m_achievesCompletedListeners = new List<DelOnAchievesCompleted>();
    private List<DelOnAllDeckContents> m_allDeckContentsListeners = new List<DelOnAllDeckContents>();
    private Map<long, CollectionDeck> m_baseDecks = new Map<long, CollectionDeck>();
    private List<DelOnCardRewardInserted> m_cardRewardListeners = new List<DelOnCardRewardInserted>();
    private bool m_cardStacksRegistered;
    private Dictionary<string, CollectionCardStack> m_collectibleStacks = new Dictionary<string, CollectionCardStack>();
    private List<DelOnCollectionChanged> m_collectionChangedListeners = new List<DelOnCollectionChanged>();
    private bool m_collectionLoaded;
    private List<DelOnCollectionLoaded> m_collectionLoadedListeners = new List<DelOnCollectionLoaded>();
    private List<DelOnDeckContents> m_deckContentsListeners = new List<DelOnDeckContents>();
    private List<DelOnDeckCreated> m_deckCreatedListeners = new List<DelOnDeckCreated>();
    private List<DelOnDeckDeleted> m_deckDeletedListeners = new List<DelOnDeckDeleted>();
    private Map<long, CollectionDeck> m_decks = new Map<long, CollectionDeck>();
    private List<DefaultCardbackChangedListener> m_defaultCardbackChangedListeners = new List<DefaultCardbackChangedListener>();
    private List<TAG_CARD_SET> m_displayableCardSets = new List<TAG_CARD_SET>();
    private bool m_editMode;
    private List<FavoriteHeroChangedListener> m_favoriteHeroChangedListeners = new List<FavoriteHeroChangedListener>();
    private bool m_hasVisitedCollection;
    private Map<string, List<CardFlair>> m_heroFlair = new Map<string, List<CardFlair>>();
    private List<OnMassDisenchant> m_massDisenchantListeners = new List<OnMassDisenchant>();
    private List<DelOnNewCardSeen> m_newCardSeenListeners = new List<DelOnNewCardSeen>();
    private Map<long, float> m_pendingRequestDeckContents;
    private Map<TAG_CLASS, PreconDeck> m_preconDecks = new Map<TAG_CLASS, PreconDeck>();
    private List<OnTaggedDeckChanged> m_taggedDeckChangedListeners = new List<OnTaggedDeckChanged>();
    private Map<DeckTag, CollectionDeck> m_taggedDecks = new Map<DeckTag, CollectionDeck>();
    private bool m_unloading;
    private bool m_waitingForBoxTransition;
    public const int NUM_BASIC_CARDS_PER_CLASS = 20;
    private const int NUM_CARDS_GRANTED_POST_TUTORIAL = 0x60;
    private const int NUM_CARDS_TO_UNLOCK_ADVANCED_CM = 0x74;
    private const int NUM_EXPERT_CARDS_TO_UNLOCK_CRAFTING = 20;
    public const int NUM_EXPERT_CARDS_TO_UNLOCK_FORGE = 20;
    private static CollectionManager s_instance;

    public void AddCardReward(CardRewardData cardReward, bool markAsNew)
    {
        List<CardRewardData> cardRewards = new List<CardRewardData> {
            cardReward
        };
        this.AddCardRewards(cardRewards, markAsNew);
    }

    public void AddCardRewards(List<CardRewardData> cardRewards, bool markAsNew)
    {
        List<string> cardIDs = new List<string>();
        List<CardFlair> cardFlairs = new List<CardFlair>();
        List<DateTime> insertDates = new List<DateTime>();
        List<int> counts = new List<int>();
        DateTime now = DateTime.Now;
        foreach (CardRewardData data in cardRewards)
        {
            CardFlair item = new CardFlair(data.Premium);
            cardIDs.Add(data.CardID);
            cardFlairs.Add(item);
            insertDates.Add(now);
            counts.Add(data.Count);
            foreach (DelOnCardRewardInserted inserted in this.m_cardRewardListeners.ToArray())
            {
                inserted(data.CardID, item);
            }
        }
        this.InsertNewCollectionCards(cardIDs, cardFlairs, insertDates, counts, !markAsNew);
        AchieveManager.Get().ValidateAchievesNow(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnActiveAchievesUpdated));
    }

    private CollectionDeck AddDeck(NetCache.DeckHeader deckHeader)
    {
        return this.AddDeck(deckHeader, true);
    }

    private CollectionDeck AddDeck(NetCache.DeckHeader deckHeader, bool updateNetCache)
    {
        if ((deckHeader.Type != DeckType.NORMAL_DECK) && (deckHeader.Type != DeckType.TAVERN_BRAWL_DECK))
        {
            Debug.LogWarning(string.Format("CollectionManager.AddDeck(): deckHeader {0} is not of type NORMAL_DECK or TAVERN_BRAWL_DECK", deckHeader));
            return null;
        }
        CollectionDeck deck3 = new CollectionDeck {
            ID = deckHeader.ID,
            Type = deckHeader.Type,
            Name = deckHeader.Name,
            HeroCardID = deckHeader.Hero,
            HeroCardFlair = new CardFlair(deckHeader.HeroPremium),
            HeroOverridden = deckHeader.HeroOverridden,
            CardBackID = deckHeader.CardBack,
            CardBackOverridden = deckHeader.CardBackOverridden,
            IsTourneyValid = deckHeader.IsTourneyValid,
            SeasonId = deckHeader.SeasonId
        };
        CollectionDeck deck = deck3;
        this.m_decks.Add(deckHeader.ID, deck);
        deck3 = new CollectionDeck {
            ID = deckHeader.ID,
            Type = deckHeader.Type,
            Name = deckHeader.Name,
            HeroCardID = deckHeader.Hero,
            HeroCardFlair = new CardFlair(deckHeader.HeroPremium),
            HeroOverridden = deckHeader.HeroOverridden,
            CardBackID = deckHeader.CardBack,
            CardBackOverridden = deckHeader.CardBackOverridden,
            IsTourneyValid = deckHeader.IsTourneyValid,
            SeasonId = deckHeader.SeasonId
        };
        CollectionDeck deck2 = deck3;
        this.m_baseDecks.Add(deckHeader.ID, deck2);
        if (updateNetCache)
        {
            NetCache.Get().GetNetObject<NetCache.NetCacheDecks>().Decks.Add(deckHeader);
        }
        return deck;
    }

    private void AddPreconDeck(TAG_CLASS heroClass, long deckID, CardFlair heroFlair)
    {
        if (this.m_preconDecks.ContainsKey(heroClass))
        {
            Debug.LogWarning(string.Format("CollectionManager.AddPreconDeck(): Already have a precon deck for class {0}, cannot add deckID {1} flair {2}", heroClass, deckID, heroFlair));
        }
        else
        {
            Log.Rachelle.Print(string.Format("CollectionManager.AddPreconDeck() heroClass={0} deckID={1} heroFlair={2}", heroClass, deckID, heroFlair), new object[0]);
            this.m_preconDecks[heroClass] = new PreconDeck(deckID, heroFlair);
        }
    }

    private void AddPreconDeckFromNotice(NetCache.ProfileNoticePreconDeck preconDeckNotice)
    {
        EntityDef entityDef = DefLoader.Get().GetEntityDef(preconDeckNotice.HeroAsset);
        if (entityDef != null)
        {
            this.AddPreconDeck(entityDef.GetClass(), preconDeckNotice.DeckID, new CardFlair(TAG_PREMIUM.NORMAL));
            NetCache.NetCacheDecks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>();
            if (netObject != null)
            {
                NetCache.DeckHeader item = new NetCache.DeckHeader {
                    ID = preconDeckNotice.DeckID,
                    Name = "precon",
                    Hero = entityDef.GetCardId(),
                    HeroPower = GameUtils.GetHeroPowerCardIdFromHero(preconDeckNotice.HeroAsset),
                    IsTourneyValid = true,
                    Type = DeckType.PRECON_DECK
                };
                netObject.Decks.Add(item);
            }
        }
    }

    public bool AllCardsInSetOwned(TAG_CARD_SET? cardSet, TAG_CLASS? cardClass, TAG_RARITY? cardRarity, TAG_RACE? cardRace, CardFlair flair, bool allCopiesRequired)
    {
        foreach (string str in this.GetCardIDsInSet(cardSet, cardClass, cardRarity, cardRace))
        {
            int num = 1;
            if (allCopiesRequired)
            {
                num = !DefLoader.Get().GetEntityDef(str).IsElite() ? 2 : 1;
            }
            CollectionCardStack collectionStack = this.GetCollectionStack(str);
            int num2 = (flair != null) ? collectionStack.GetArtStack(flair).Count : collectionStack.GetTotalCount();
            if (num2 < num)
            {
                return false;
            }
        }
        return true;
    }

    public string AutoGenerateDeckName(TAG_CLASS classTag)
    {
        string str2;
        string className = GameStrings.GetClassName(classTag);
        int num = 1;
        do
        {
            if (num == 1)
            {
                object[] args = new object[] { className, string.Empty };
                str2 = GameStrings.Format("GLUE_COLLECTION_CUSTOM_DECKNAME_TEMPLATE", args);
            }
            else
            {
                object[] objArray2 = new object[] { className, num };
                str2 = GameStrings.Format("GLUE_COLLECTION_CUSTOM_DECKNAME_TEMPLATE", objArray2);
            }
            num++;
        }
        while (this.IsDeckNameTaken(str2));
        return str2;
    }

    public bool CanDisplayCards()
    {
        return (this.m_cardStacksRegistered && CollectionManagerDisplay.Get().IsReady());
    }

    public void ClearTaggedDeck(DeckTag tag)
    {
        this.SetTaggedDeck(tag, (CollectionDeck) null, null);
    }

    public void DoneEditing()
    {
        bool editMode = this.m_editMode;
        this.m_editMode = false;
        if (editMode && (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL))
        {
            PresenceMgr.Get().SetPrevStatus();
        }
        this.ClearTaggedDeck(DeckTag.Editing);
    }

    public int EntityDefSortComparison(EntityDef entityDef1, EntityDef entityDef2)
    {
        int cost = entityDef1.GetCost();
        int num2 = entityDef2.GetCost();
        if (cost != num2)
        {
            return (cost - num2);
        }
        int cardTypeSortOrder = this.GetCardTypeSortOrder(entityDef1);
        int num4 = this.GetCardTypeSortOrder(entityDef2);
        if (cardTypeSortOrder != num4)
        {
            return (cardTypeSortOrder - num4);
        }
        return string.Compare(entityDef1.GetName(), entityDef2.GetName(), true);
    }

    private void FireAllDeckContentsEvent()
    {
        DelOnAllDeckContents[] contentsArray = this.m_allDeckContentsListeners.ToArray();
        this.m_allDeckContentsListeners.Clear();
        foreach (DelOnAllDeckContents contents in contentsArray)
        {
            contents();
        }
    }

    private void FireDeckContentsEvent(long id)
    {
        foreach (DelOnDeckContents contents in this.m_deckContentsListeners.ToArray())
        {
            contents(id);
        }
    }

    public static CollectionManager Get()
    {
        return s_instance;
    }

    public CollectionDeck GetBaseDeck(long id)
    {
        CollectionDeck deck;
        if (this.m_baseDecks.TryGetValue(id, out deck))
        {
            return deck;
        }
        return null;
    }

    public int GetBasicCardsIOwn(TAG_CLASS cardClass)
    {
        return NetCache.Get().GetNetObject<NetCache.NetCacheCollection>().BasicCardsUnlockedPerClass[cardClass];
    }

    public List<NetCache.CardDefinition> GetBestHeroesIOwn(TAG_CLASS heroClass)
    {
        List<NetCache.CardDefinition> heroesIOwn = this.GetHeroesIOwn(heroClass);
        List<NetCache.CardDefinition> list2 = new List<NetCache.CardDefinition>();
        <GetBestHeroesIOwn>c__AnonStorey2CD storeycd = new <GetBestHeroesIOwn>c__AnonStorey2CD();
        using (List<NetCache.CardDefinition>.Enumerator enumerator = heroesIOwn.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                storeycd.hero = enumerator.Current;
                NetCache.CardDefinition definition = list2.Find(new Predicate<NetCache.CardDefinition>(storeycd.<>m__9D));
                if (definition != null)
                {
                    if (storeycd.hero.Premium == TAG_PREMIUM.GOLDEN)
                    {
                        definition.Premium = TAG_PREMIUM.GOLDEN;
                    }
                }
                else
                {
                    list2.Add(storeycd.hero);
                }
            }
        }
        return list2;
    }

    public CardFlair GetBestHeroFlairOwned(string heroCardID)
    {
        if (!this.m_heroFlair.ContainsKey(heroCardID))
        {
            return null;
        }
        if (this.m_heroFlair[heroCardID].Count == 0)
        {
            return null;
        }
        return this.m_heroFlair[heroCardID][0];
    }

    private List<string> GetCardIDsInSet(TAG_CARD_SET? cardSet, TAG_CLASS? cardClass, TAG_RARITY? cardRarity, TAG_RACE? cardRace)
    {
        List<string> nonHeroCollectibleCardIds = GameUtils.GetNonHeroCollectibleCardIds();
        List<string> list2 = new List<string>();
        foreach (string str in nonHeroCollectibleCardIds)
        {
            EntityDef entityDef = DefLoader.Get().GetEntityDef(str);
            if (((!cardClass.HasValue || (((TAG_CLASS) cardClass.Value) == entityDef.GetClass())) && (!cardRarity.HasValue || (((TAG_RARITY) cardRarity.Value) == entityDef.GetRarity()))) && ((!cardSet.HasValue || (((TAG_CARD_SET) cardSet.Value) == entityDef.GetCardSet())) && (!cardRace.HasValue || (((TAG_RACE) cardRace.Value) == entityDef.GetRace()))))
            {
                list2.Add(str);
            }
        }
        return list2;
    }

    public int GetCardsToDisenchantCount()
    {
        return this.GetMassDisenchantArtStacks().Count;
    }

    public int GetCardTypeSortOrder(EntityDef entityDef)
    {
        switch (entityDef.GetCardType())
        {
            case TAG_CARDTYPE.MINION:
                return 3;

            case TAG_CARDTYPE.SPELL:
                return 2;

            case TAG_CARDTYPE.WEAPON:
                return 1;
        }
        return 0;
    }

    public Dictionary<string, CollectionCardStack> GetCollectibleStacks()
    {
        return this.m_collectibleStacks;
    }

    private CollectionCardStack.ArtStack GetCollectionArtStack(int dbId, CardFlair cardFlair)
    {
        string cardID = GameUtils.TranslateDbIdToCardId(dbId);
        if (cardID == null)
        {
            Debug.LogError(string.Format("CollectionManager.GetCollectionArtStack() - Could not find assetID {0} in manifest cards.", dbId));
            return null;
        }
        return this.GetCollectionArtStack(cardID, cardFlair);
    }

    public CollectionCardStack.ArtStack GetCollectionArtStack(string cardID, CardFlair cardFlair)
    {
        CollectionCardStack collectionStack = this.GetCollectionStack(cardID);
        if (collectionStack == null)
        {
            Debug.LogError(string.Format("CollectionManager.GetCollectionArtStack() - could not find collection stack for card {0}", cardID));
            return null;
        }
        return collectionStack.GetArtStack(cardFlair);
    }

    private CollectionCardStack GetCollectionStack(string cardID)
    {
        CollectionCardStack stack = null;
        this.m_collectibleStacks.TryGetValue(cardID, out stack);
        return stack;
    }

    public CollectionDeck GetDeck(long id)
    {
        CollectionDeck deck;
        if (this.m_decks.TryGetValue(id, out deck))
        {
            if ((deck == null) || (deck.Type != DeckType.TAVERN_BRAWL_DECK))
            {
                return deck;
            }
            TavernBrawlMission mission = !TavernBrawlManager.Get().IsTavernBrawlActive ? null : TavernBrawlManager.Get().CurrentMission();
            if ((mission != null) && (deck.SeasonId == mission.seasonId))
            {
                return deck;
            }
        }
        return null;
    }

    public SortedDictionary<long, CollectionDeck> GetDecks()
    {
        SortedDictionary<long, CollectionDeck> dictionary = new SortedDictionary<long, CollectionDeck>();
        foreach (KeyValuePair<long, CollectionDeck> pair in this.m_decks)
        {
            if ((pair.Value != null) && ((pair.Value.Type != DeckType.TAVERN_BRAWL_DECK) || (TavernBrawlManager.Get().IsTavernBrawlActive && (pair.Value.SeasonId == TavernBrawlManager.Get().CurrentMission().seasonId))))
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }
        return dictionary;
    }

    public List<CollectionDeck> GetDecks(DeckType deckType)
    {
        List<CollectionDeck> list = new List<CollectionDeck>();
        foreach (CollectionDeck deck in this.m_decks.Values)
        {
            if (deck.Type == deckType)
            {
                if (deckType == DeckType.TAVERN_BRAWL_DECK)
                {
                    TavernBrawlMission mission = !TavernBrawlManager.Get().IsTavernBrawlActive ? null : TavernBrawlManager.Get().CurrentMission();
                    if ((mission == null) || (deck.SeasonId != mission.seasonId))
                    {
                        continue;
                    }
                }
                list.Add(deck);
            }
        }
        list.Sort(new DeckSort());
        return list;
    }

    public List<TAG_CARD_SET> GetDisplayableCardSets()
    {
        return this.m_displayableCardSets;
    }

    public NetCache.CardDefinition GetFavoriteHero(TAG_CLASS heroClass)
    {
        NetCache.NetCacheFavoriteHeroes netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFavoriteHeroes>();
        if (netObject == null)
        {
            return null;
        }
        if (!netObject.FavoriteHeroes.ContainsKey(heroClass))
        {
            return null;
        }
        return netObject.FavoriteHeroes[heroClass];
    }

    public List<NetCache.CardDefinition> GetHeroesIOwn(TAG_CLASS heroClass)
    {
        <GetHeroesIOwn>c__AnonStorey2CC storeycc = new <GetHeroesIOwn>c__AnonStorey2CC {
            heroClass = heroClass
        };
        List<CollectionCardStack> list2 = this.GetOwnedCardStacks().FindAll(new Predicate<CollectionCardStack>(storeycc.<>m__9C));
        List<NetCache.CardDefinition> list3 = new List<NetCache.CardDefinition>();
        foreach (CollectionCardStack stack in list2)
        {
            foreach (CollectionCardStack.ArtStack stack2 in stack.GetArtStacks().Values)
            {
                NetCache.CardDefinition item = new NetCache.CardDefinition {
                    Name = stack2.CardID,
                    Premium = stack2.Flair.Premium
                };
                list3.Add(item);
            }
        }
        return list3;
    }

    public List<CollectionCardStack.ArtStack> GetMassDisenchantArtStacks()
    {
        List<CollectionCardStack.ArtStack> list = new List<CollectionCardStack.ArtStack>();
        foreach (CollectionCardStack stack in this.GetOwnedCardStacks())
        {
            int num = !DefLoader.Get().GetEntityDef(stack.CardID).IsElite() ? 2 : 1;
            foreach (CollectionCardStack.ArtStack stack2 in stack.GetArtStacks().Values)
            {
                int count = stack2.Count - num;
                if (count > 0)
                {
                    list.Add(new CollectionCardStack.ArtStack(stack2.CardID, stack2.Flair, stack2.NewestInsertDate, count, 0));
                }
            }
        }
        return list;
    }

    public int GetMaxNumCustomDecks()
    {
        return NetCache.Get().GetNetObject<NetCache.NetCacheDeckLimit>().DeckLimit;
    }

    public int GetNumCopiesInCollection(string cardID, TAG_PREMIUM premium)
    {
        return this.GetCollectionArtStack(cardID, new CardFlair(premium)).Count;
    }

    public void GetOwnedCardCount(string cardId, out int standard, out int golden)
    {
        standard = 0;
        golden = 0;
        CollectionCardStack stack = null;
        if (this.m_collectibleStacks.TryGetValue(cardId, out stack))
        {
            CollectionCardStack.ArtStack stack2 = null;
            Map<CardFlair, CollectionCardStack.ArtStack> artStacks = stack.GetArtStacks();
            if (artStacks.TryGetValue(new CardFlair(TAG_PREMIUM.NORMAL), out stack2) && (stack2 != null))
            {
                standard = stack2.Count;
            }
            if (artStacks.TryGetValue(new CardFlair(TAG_PREMIUM.GOLDEN), out stack2) && (stack2 != null))
            {
                golden = stack2.Count;
            }
        }
    }

    public List<CollectionCardStack> GetOwnedCardStacks()
    {
        List<CollectionCardStack> list = new List<CollectionCardStack>();
        foreach (CollectionCardStack stack in this.GetCollectibleStacks().Values)
        {
            if (stack.GetTotalCount() != 0)
            {
                list.Add(stack);
            }
        }
        return list;
    }

    public PreconDeck GetPreconDeck(TAG_CLASS heroClass)
    {
        if (!this.m_preconDecks.ContainsKey(heroClass))
        {
            return null;
        }
        return this.m_preconDecks[heroClass];
    }

    public CollectionDeck GetTaggedDeck(DeckTag tag)
    {
        CollectionDeck deck = null;
        if ((!this.m_taggedDecks.TryGetValue(tag, out deck) || (deck == null)) || (deck.Type != DeckType.TAVERN_BRAWL_DECK))
        {
            return deck;
        }
        TavernBrawlMission mission = !TavernBrawlManager.Get().IsTavernBrawlActive ? null : TavernBrawlManager.Get().CurrentMission();
        if ((mission != null) && (deck.SeasonId == mission.seasonId))
        {
            return deck;
        }
        return null;
    }

    public string GetVanillaHeroCardID(EntityDef HeroSkinEntityDef)
    {
        TAG_CLASS heroClass = HeroSkinEntityDef.GetClass();
        return Get().GetVanillaHeroCardIDFromClass(heroClass);
    }

    public string GetVanillaHeroCardIDFromClass(TAG_CLASS heroClass)
    {
        switch (heroClass)
        {
            case TAG_CLASS.DRUID:
                return "HERO_06";

            case TAG_CLASS.HUNTER:
                return "HERO_05";

            case TAG_CLASS.MAGE:
                return "HERO_08";

            case TAG_CLASS.PALADIN:
                return "HERO_04";

            case TAG_CLASS.PRIEST:
                return "HERO_09";

            case TAG_CLASS.ROGUE:
                return "HERO_03";

            case TAG_CLASS.SHAMAN:
                return "HERO_02";

            case TAG_CLASS.WARLOCK:
                return "HERO_07";

            case TAG_CLASS.WARRIOR:
                return "HERO_01";
        }
        return string.Empty;
    }

    private bool HasUnlockedAllHeores()
    {
        return (AchieveManager.Get().GetNumAchievesInGroup(Achievement.Group.UNLOCK_HERO) == this.m_preconDecks.Count);
    }

    public bool HasVisitedCollection()
    {
        return this.m_hasVisitedCollection;
    }

    public static void Init()
    {
        if (s_instance == null)
        {
            s_instance = new CollectionManager();
            ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
            NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheFavoriteHeroes), new System.Action(s_instance.NetCache_OnFavoriteHeroesReceived));
        }
        s_instance.InitImpl();
    }

    private void InitImpl()
    {
        foreach (string str in GameUtils.GetAllCollectibleCardIds())
        {
            if (DefLoader.Get().GetEntityDef(str) == null)
            {
                object[] messageArgs = new object[] { str };
                Error.AddDevFatal("Failed to find an EntityDef for collectible card {0}", messageArgs);
                return;
            }
            this.RegisterEmptyCardStack(str);
        }
        Network.Get().RegisterNetHandler(PegasusUtil.DeckContents.PacketID.ID, new Network.NetHandler(this.OnDeck), null);
        NetCache.Get().RegisterCollectionManager(new NetCache.NetCacheCallback(this.OnNetCacheReady));
    }

    private void InsertNewCollectionCard(string cardID, CardFlair cardFlair, DateTime insertDate, int count, bool seenBefore)
    {
        EntityDef entityDef = DefLoader.Get().GetEntityDef(cardID);
        int numSeen = !seenBefore ? 0 : count;
        if (entityDef.IsHero())
        {
            this.RegisterHeroCardStack(entityDef, cardID, cardFlair, insertDate, count, numSeen);
            if (cardFlair.Premium > TAG_PREMIUM.NORMAL)
            {
                this.UpdateDeckHeroArt(cardID);
            }
        }
        else
        {
            CollectionCardStack collectionStack = this.GetCollectionStack(cardID);
            if (collectionStack == null)
            {
                object[] messageArgs = new object[] { cardID };
                Error.AddDevFatal("Collection stack for card {0} is NULL!", messageArgs);
            }
            else
            {
                collectionStack.AddCards(cardFlair, insertDate, count, numSeen);
                NetCache.CardDefinition cardDef = new NetCache.CardDefinition {
                    Name = cardID,
                    Premium = cardFlair.Premium
                };
                this.NotifyNetCacheOfNewCards(cardDef, insertDate.Ticks, count, seenBefore);
                AchieveManager.Get().NotifyOfCardGained(entityDef, cardFlair, collectionStack.GetArtStack(cardFlair).Count);
                this.UpdateShowAdvancedCMOption();
            }
        }
    }

    private void InsertNewCollectionCards(List<string> cardIDs, List<CardFlair> cardFlairs, List<DateTime> insertDates, List<int> counts, bool seenBefore)
    {
        List<EntityDef> entityDefs = new List<EntityDef>();
        for (int i = 0; i < cardIDs.Count; i++)
        {
            string cardId = cardIDs[i];
            CardFlair cardFlair = cardFlairs[i];
            DateTime insertDate = insertDates[i];
            int count = counts[i];
            EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
            int numSeen = !seenBefore ? 0 : count;
            if (entityDef.IsHero())
            {
                this.RegisterHeroCardStack(entityDef, cardId, cardFlair, insertDate, count, numSeen);
                if (cardFlair.Premium > TAG_PREMIUM.NORMAL)
                {
                    this.UpdateDeckHeroArt(cardId);
                }
            }
            else
            {
                CollectionCardStack collectionStack = this.GetCollectionStack(cardId);
                if (collectionStack == null)
                {
                    object[] messageArgs = new object[] { cardId };
                    Error.AddDevFatal("Collection stack for card {0} is NULL!", messageArgs);
                    return;
                }
                collectionStack.AddCards(cardFlair, insertDate, count, numSeen);
                NetCache.CardDefinition cardDef = new NetCache.CardDefinition {
                    Name = cardId,
                    Premium = cardFlair.Premium
                };
                this.NotifyNetCacheOfNewCards(cardDef, insertDate.Ticks, count, seenBefore);
                entityDefs.Add(entityDef);
                this.UpdateShowAdvancedCMOption();
            }
        }
        AchieveManager.Get().NotifyOfCardsGained(entityDefs, cardFlairs);
    }

    public bool IsCardInCollection(string cardID, CardFlair cardFlair)
    {
        CollectionCardStack.ArtStack collectionArtStack = this.GetCollectionArtStack(cardID, cardFlair);
        if (collectionArtStack == null)
        {
            Debug.LogError(string.Format("CollectionManager.IsCardInCollection() - could not find collection stack for card {0}", cardID));
            return false;
        }
        return (collectionArtStack.Count > 0);
    }

    private bool IsDeckNameTaken(string name)
    {
        foreach (CollectionDeck deck in this.GetDecks().Values)
        {
            if (deck.Name.Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsFullyLoaded()
    {
        return this.m_collectionLoaded;
    }

    public bool IsInEditMode()
    {
        return this.m_editMode;
    }

    public bool IsWaitingForBoxTransition()
    {
        return this.m_waitingForBoxTransition;
    }

    public void LoadAllDeckContents(DelOnAllDeckContents callback)
    {
        bool flag = false;
        foreach (CollectionDeck deck in this.GetDecks().Values)
        {
            if (!deck.NetworkContentsLoaded())
            {
                if (!flag && !this.m_allDeckContentsListeners.Contains(callback))
                {
                    this.m_allDeckContentsListeners.Add(callback);
                }
                this.RequestDeckContents(deck.ID);
                flag = true;
            }
        }
        if (!flag)
        {
            callback();
        }
    }

    public void MarkAllInstancesAsSeen(string cardID, CardFlair cardFlair)
    {
        <MarkAllInstancesAsSeen>c__AnonStorey2CE storeyce = new <MarkAllInstancesAsSeen>c__AnonStorey2CE();
        NetCache.NetCacheCollection netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCollection>();
        int assetID = GameUtils.TranslateCardIdToDbId(cardID);
        if (assetID != 0)
        {
            storeyce.artStack = this.GetCollectionArtStack(cardID, cardFlair);
            if (storeyce.artStack.NumSeen != storeyce.artStack.Count)
            {
                Network.AckCardSeenBefore(assetID, cardFlair);
                storeyce.artStack.MarkStackAsSeen();
                NetCache.CardStack stack = netObject.Stacks.Find(new Predicate<NetCache.CardStack>(storeyce.<>m__9E));
                if (stack != null)
                {
                    stack.NumSeen = stack.Count;
                }
                foreach (DelOnNewCardSeen seen in this.m_newCardSeenListeners)
                {
                    seen(cardID, cardFlair);
                }
            }
        }
    }

    private void NetCache_OnFavoriteHeroesReceived()
    {
        NetCache.NetCacheFavoriteHeroes netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFavoriteHeroes>();
        if ((netObject != null) && (netObject.FavoriteHeroes != null))
        {
            foreach (KeyValuePair<TAG_CLASS, NetCache.CardDefinition> pair in netObject.FavoriteHeroes)
            {
                TAG_CLASS key = pair.Key;
                NetCache.CardDefinition definition = pair.Value;
                this.UpdateFavoriteHero(key, definition.Name, definition.Premium);
            }
        }
    }

    private void NotifyNetCacheOfNewCards(NetCache.CardDefinition cardDef, long insertDate, int count, bool seenBefore)
    {
        <NotifyNetCacheOfNewCards>c__AnonStorey2D2 storeyd = new <NotifyNetCacheOfNewCards>c__AnonStorey2D2 {
            cardDef = cardDef
        };
        NetCache.NetCacheCollection netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCollection>();
        NetCache.CardStack item = netObject.Stacks.Find(new Predicate<NetCache.CardStack>(storeyd.<>m__A6));
        if (item == null)
        {
            item = new NetCache.CardStack {
                Def = storeyd.cardDef,
                Date = insertDate,
                Count = count,
                NumSeen = !seenBefore ? 0 : count
            };
            netObject.Stacks.Add(item);
        }
        else
        {
            if (insertDate > item.Date)
            {
                item.Date = insertDate;
            }
            item.Count += count;
            if (seenBefore)
            {
                item.NumSeen += count;
            }
        }
        this.UpdateCardCounts(netObject, storeyd.cardDef, count, item.Count);
    }

    private void NotifyNetCacheOfRemovedCards(NetCache.CardDefinition cardDef, int count)
    {
        <NotifyNetCacheOfRemovedCards>c__AnonStorey2D1 storeyd = new <NotifyNetCacheOfRemovedCards>c__AnonStorey2D1 {
            cardDef = cardDef
        };
        NetCache.NetCacheCollection netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCollection>();
        NetCache.CardStack item = netObject.Stacks.Find(new Predicate<NetCache.CardStack>(storeyd.<>m__A5));
        if (item == null)
        {
            Debug.LogError("CollectionManager.NotifyNetCacheOfRemovedCards() - trying to remove a card from an empty stack!");
        }
        else
        {
            item.Count -= count;
            if (item.Count <= 0)
            {
                netObject.Stacks.Remove(item);
            }
            this.UpdateCardCounts(netObject, storeyd.cardDef, -count, item.Count);
        }
    }

    public void NotifyOfBoxTransitionStart()
    {
        Box.Get().AddTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxTransitionFinished));
        this.m_waitingForBoxTransition = true;
    }

    private int NumCardsOwnedInSet(TAG_CARD_SET cardSet)
    {
        <NumCardsOwnedInSet>c__AnonStorey2CF storeycf = new <NumCardsOwnedInSet>c__AnonStorey2CF {
            cardSet = cardSet
        };
        List<CollectionCardStack> list2 = this.GetOwnedCardStacks().FindAll(new Predicate<CollectionCardStack>(storeycf.<>m__A1));
        int num = 0;
        foreach (CollectionCardStack stack in list2)
        {
            num += stack.GetTotalCount();
        }
        return num;
    }

    private void OnActiveAchievesUpdated(object userData)
    {
        List<Achievement> newCompletedAchieves = AchieveManager.Get().GetNewCompletedAchieves();
        foreach (DelOnAchievesCompleted completed in this.m_achievesCompletedListeners)
        {
            completed(newCompletedAchieves);
        }
    }

    public void OnBoosterOpened(List<NetCache.BoosterCard> cards)
    {
        if (!Options.Get().GetBool(Option.FAKE_PACK_OPENING))
        {
            List<string> cardIDs = new List<string>();
            List<CardFlair> cardFlairs = new List<CardFlair>();
            List<DateTime> insertDates = new List<DateTime>();
            List<int> counts = new List<int>();
            foreach (NetCache.BoosterCard card in cards)
            {
                cardIDs.Add(card.Def.Name);
                cardFlairs.Add(new CardFlair(card.Def.Premium));
                insertDates.Add(new DateTime(card.Date));
                counts.Add(1);
            }
            this.InsertNewCollectionCards(cardIDs, cardFlairs, insertDates, counts, false);
            AchieveManager.Get().ValidateAchievesNow(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnActiveAchievesUpdated));
            this.OnCollectionChanged();
        }
    }

    public void OnBoxTransitionFinished(object userData)
    {
        Box.Get().RemoveTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxTransitionFinished));
        this.m_waitingForBoxTransition = false;
    }

    public void OnCardRewardOpened(string cardID, TAG_PREMIUM premium, int count)
    {
        this.InsertNewCollectionCard(cardID, new CardFlair(premium), DateTime.Now, count, false);
        AchieveManager.Get().ValidateAchievesNow(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnActiveAchievesUpdated));
        this.OnCollectionChanged();
    }

    private void OnCardSale()
    {
        Network.CardSaleResult cardSaleResult = Network.GetCardSaleResult();
        bool flag = false;
        switch (cardSaleResult.Action)
        {
            case Network.CardSaleResult.SaleResult.GENERIC_FAILURE:
                CraftingManager.Get().OnCardGenericError(cardSaleResult);
                flag = false;
                break;

            case Network.CardSaleResult.SaleResult.CARD_WAS_SOLD:
                for (int i = 1; i <= cardSaleResult.Count; i++)
                {
                    this.RemoveCollectionCard(cardSaleResult.AssetName, new CardFlair(cardSaleResult.Premium), 1);
                }
                CraftingManager.Get().OnCardDisenchanted(cardSaleResult);
                AchieveManager.Get().UpdateActiveAchieves(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnActiveAchievesUpdated));
                flag = true;
                break;

            case Network.CardSaleResult.SaleResult.CARD_WAS_BOUGHT:
                for (int j = 1; j <= cardSaleResult.Count; j++)
                {
                    this.InsertNewCollectionCard(cardSaleResult.AssetName, new CardFlair(cardSaleResult.Premium), DateTime.Now, 1, true);
                }
                CraftingManager.Get().OnCardCreated(cardSaleResult);
                AchieveManager.Get().ValidateAchievesNow(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnActiveAchievesUpdated));
                flag = true;
                break;

            case Network.CardSaleResult.SaleResult.SOULBOUND:
                CraftingManager.Get().OnCardDisenchantSoulboundError(cardSaleResult);
                flag = false;
                break;

            case Network.CardSaleResult.SaleResult.FAILED_WRONG_SELL_PRICE:
            {
                NetCache.CardValue cardValue = CraftingManager.Get().GetCardValue(cardSaleResult.AssetName, cardSaleResult.Premium);
                if (cardValue != null)
                {
                    cardValue.Sell = cardSaleResult.UnitSellPrice;
                    cardValue.Nerfed = cardSaleResult.Nerfed;
                }
                flag = false;
                break;
            }
            case Network.CardSaleResult.SaleResult.FAILED_WRONG_BUY_PRICE:
            {
                NetCache.CardValue value3 = CraftingManager.Get().GetCardValue(cardSaleResult.AssetName, cardSaleResult.Premium);
                if (value3 != null)
                {
                    value3.Buy = cardSaleResult.UnitBuyPrice;
                    value3.Nerfed = cardSaleResult.Nerfed;
                }
                flag = false;
                break;
            }
            case Network.CardSaleResult.SaleResult.FAILED_NO_PERMISSION:
                CraftingManager.Get().OnCardPermissionError(cardSaleResult);
                flag = false;
                break;

            case Network.CardSaleResult.SaleResult.FAILED_EVENT_NOT_ACTIVE:
                CraftingManager.Get().OnCardCraftingEventNotActiveError(cardSaleResult);
                flag = false;
                break;

            default:
                CraftingManager.Get().OnCardUnknownError(cardSaleResult);
                flag = false;
                break;
        }
        if (!flag)
        {
            object[] args = new object[] { cardSaleResult.Action, cardSaleResult.AssetName, cardSaleResult.AssetID, cardSaleResult.Premium };
            Debug.LogWarning(string.Format("CollectionManager.OnCardSale {0} for card {1} (asset {2}) premium {3}", args));
        }
        else
        {
            this.OnCollectionChanged();
        }
    }

    private void OnCollectionChanged()
    {
        foreach (DelOnCollectionChanged changed in this.m_collectionChangedListeners.ToArray())
        {
            changed();
        }
    }

    private void OnDBAction()
    {
        <OnDBAction>c__AnonStorey2CA storeyca = new <OnDBAction>c__AnonStorey2CA();
        Network.DBAction deckResponse = Network.GetDeckResponse();
        Log.Rachelle.Print(string.Format("MetaData:{0} DBAction:{1} Result:{2}", deckResponse.MetaData, deckResponse.Action, deckResponse.Result), new object[0]);
        bool flag = false;
        bool flag2 = false;
        switch (deckResponse.Action)
        {
            case Network.DBAction.ActionType.CREATE_DECK:
                if ((deckResponse.Result != Network.DBAction.ResultType.SUCCESS) && (CollectionDeckTray.Get() != null))
                {
                    CollectionDeckTray.Get().GetDecksContent().CreateNewDeckCancelled();
                }
                break;

            case Network.DBAction.ActionType.RENAME_DECK:
                flag = true;
                break;

            case Network.DBAction.ActionType.SET_DECK:
                flag2 = true;
                break;
        }
        if (flag || flag2)
        {
            storeyca.deckID = deckResponse.MetaData;
            CollectionDeck deck = this.GetDeck(storeyca.deckID);
            CollectionDeck baseDeck = this.GetBaseDeck(storeyca.deckID);
            if (deckResponse.Result == Network.DBAction.ResultType.SUCCESS)
            {
                List<CollectionDeckViolation> deckViolations = CollectionDeckValidator.GetDeckViolations(deck);
                deck.IsTourneyValid = deckViolations.Count == 0;
                Log.Rachelle.Print(string.Format("CollectionManager.OnDBAction(): overwriting baseDeck with {0} updated deck ({1}:{2})", !deck.IsTourneyValid ? "INVALID" : "valid", deck.ID, deck.Name), new object[0]);
                baseDeck.CopyFrom(deck);
                NetCache.DeckHeader header = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>().Decks.Find(new Predicate<NetCache.DeckHeader>(storeyca.<>m__9A));
                if (header != null)
                {
                    header.HeroOverridden = deck.HeroOverridden;
                    header.CardBackOverridden = deck.CardBackOverridden;
                    header.IsTourneyValid = deck.IsTourneyValid;
                    header.SeasonId = deck.SeasonId;
                }
            }
            else
            {
                Log.Rachelle.Print(string.Format("CollectionManager.OnDBAction(): overwriting deck that failed to update with base deck ({0}:{1})", baseDeck.ID, baseDeck.Name), new object[0]);
                deck.CopyFrom(baseDeck);
            }
            if (flag)
            {
                deck.OnNameChangeComplete();
            }
            if (flag2)
            {
                deck.OnContentChangesComplete();
            }
        }
    }

    private void OnDeck()
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
            Debug.LogError(string.Format("No contents returned for deck {0}!", deckContents.Deck));
        }
        else
        {
            deck.ClearSlotContents();
            baseDeck.ClearSlotContents();
            foreach (Network.CardUserData data in deckContents.Cards)
            {
                string cardID = GameUtils.TranslateDbIdToCardId(data.DbId);
                if (cardID == null)
                {
                    Debug.LogError(string.Format("CollectionManager.OnDeck(): Could not find card with asset ID {0} in our card manifest", data.DbId));
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        deck.AddCard(cardID, data.Premium);
                        baseDeck.AddCard(cardID, data.Premium);
                    }
                }
            }
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

    private void OnDeckCreated()
    {
        NetCache.DeckHeader createdDeck = Network.GetCreatedDeck();
        Log.Rachelle.Print(string.Format("DeckCreated:{0} ID:{1} Hero:{2}", createdDeck.Name, createdDeck.ID, createdDeck.Hero), new object[0]);
        this.AddDeck(createdDeck).MarkNetworkContentsLoaded();
        foreach (DelOnDeckCreated created in this.m_deckCreatedListeners)
        {
            created(createdDeck.ID);
        }
    }

    private void OnDeckDeleted()
    {
        Log.Rachelle.Print("CollectionManager.OnDeckDeleted", new object[0]);
        long deletedDeckID = Network.GetDeletedDeckID();
        Log.Rachelle.Print(string.Format("DeckDeleted:{0}", deletedDeckID), new object[0]);
        this.RemoveDeck(deletedDeckID);
        if (CollectionDeckTray.Get() != null)
        {
            foreach (DelOnDeckDeleted deleted in this.m_deckDeletedListeners.ToArray())
            {
                deleted(deletedDeckID);
            }
        }
    }

    private void OnDeckRenamed()
    {
        <OnDeckRenamed>c__AnonStorey2CB storeycb = new <OnDeckRenamed>c__AnonStorey2CB();
        Network.DeckName renamedDeck = Network.GetRenamedDeck();
        Log.Rachelle.Print(string.Format("OnDeckRenamed {0}", renamedDeck.Deck), new object[0]);
        storeycb.id = renamedDeck.Deck;
        string name = renamedDeck.Name;
        this.GetBaseDeck(storeycb.id).Name = name;
        CollectionDeck deck = this.GetDeck(storeycb.id);
        deck.Name = name;
        NetCache.DeckHeader header = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>().Decks.Find(new Predicate<NetCache.DeckHeader>(storeycb.<>m__9B));
        if (header != null)
        {
            header.Name = name;
        }
        deck.OnNameChangeComplete();
    }

    private void OnDefaultCardBackSet()
    {
        Network.CardBackResponse cardBackReponse = ConnectAPI.GetCardBackReponse();
        if (!cardBackReponse.Success)
        {
            object[] args = new object[] { cardBackReponse.CardBack };
            Log.Rachelle.Print("SetCardBack FAILED (cardBack = {0})", args);
        }
        else
        {
            NetCache.NetCacheCardBacks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
            NetCache.NetCacheDecks decks = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>();
            netObject.DefaultCardBack = cardBackReponse.CardBack;
            foreach (NetCache.DeckHeader header in decks.Decks)
            {
                if (!header.CardBackOverridden)
                {
                    header.CardBack = cardBackReponse.CardBack;
                    CollectionDeck deck = this.GetDeck(header.ID);
                    if (deck != null)
                    {
                        deck.CardBackID = header.CardBack;
                    }
                    CollectionDeck baseDeck = this.GetBaseDeck(header.ID);
                    if (baseDeck != null)
                    {
                        baseDeck.CardBackID = header.CardBack;
                    }
                }
            }
            foreach (DefaultCardbackChangedListener listener in this.m_defaultCardbackChangedListeners.ToArray())
            {
                listener.Fire(netObject.DefaultCardBack);
            }
        }
    }

    private void OnMassDisenchantResponse()
    {
        Network.MassDisenchantResponse massDisenchantResponse = Network.GetMassDisenchantResponse();
        if (massDisenchantResponse.Amount == 0)
        {
            Debug.LogError("CollectionManager.OnMassDisenchantResponse(): Amount is 0. This means the backend failed to mass disenchant correctly.");
        }
        else
        {
            NetCache.Get().OnArcaneDustBalanceChanged((long) massDisenchantResponse.Amount);
            AchieveManager.Get().UpdateActiveAchieves(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnActiveAchievesUpdated));
            foreach (OnMassDisenchant disenchant in this.m_massDisenchantListeners.ToArray())
            {
                disenchant(massDisenchantResponse.Amount);
            }
            foreach (CollectionCardStack.ArtStack stack in this.GetMassDisenchantArtStacks())
            {
                this.RemoveCollectionCard(stack.CardID, stack.Flair, stack.Count);
            }
            this.OnCollectionChanged();
        }
    }

    private void OnNetCacheReady()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        Log.Rachelle.Print("CollectionManager.OnNetCacheReady", new object[0]);
        foreach (string str in GameUtils.GetNonHeroCollectibleCardIds())
        {
            TAG_CARD_SET cardSet = DefLoader.Get().GetEntityDef(str).GetCardSet();
            if (!this.m_displayableCardSets.Contains(cardSet))
            {
                this.m_displayableCardSets.Add(cardSet);
            }
        }
        AchieveManager.Init();
        this.RegisterCardStacks();
        foreach (NetCache.DeckHeader header in NetCache.Get().GetNetObject<NetCache.NetCacheDecks>().Decks)
        {
            switch (header.Type)
            {
                case DeckType.NORMAL_DECK:
                case DeckType.TAVERN_BRAWL_DECK:
                {
                    this.AddDeck(header, false);
                    continue;
                }
                case DeckType.PRECON_DECK:
                {
                    this.AddPreconDeck(DefLoader.Get().GetEntityDef(header.Hero).GetClass(), header.ID, new CardFlair(header.HeroPremium));
                    continue;
                }
            }
            Debug.LogWarning(string.Format("CollectionManager.OnNetCacheReady(): don't know how to handle deck type {0}", header.Type));
        }
        this.UpdateShowAdvancedCMOption();
        foreach (DelOnCollectionLoaded loaded in this.m_collectionLoadedListeners.ToArray())
        {
            loaded();
        }
        NetCache.Get().RegisterNewNoticesListener(new NetCache.DelNewNoticesListener(this.OnNewNotices));
        this.m_collectionLoaded = true;
    }

    private void OnNewNotices(List<NetCache.ProfileNotice> newNotices)
    {
        if (<>f__am$cache1E == null)
        {
            <>f__am$cache1E = obj => obj.Type == NetCache.ProfileNotice.NoticeType.PRECON_DECK;
        }
        foreach (NetCache.ProfileNotice notice in newNotices.FindAll(<>f__am$cache1E))
        {
            NetCache.ProfileNoticePreconDeck preconDeckNotice = notice as NetCache.ProfileNoticePreconDeck;
            this.AddPreconDeckFromNotice(preconDeckNotice);
        }
    }

    private void OnSetFavoriteHeroResponse()
    {
        Network.SetFavoriteHeroResponse setFavoriteHeroResponse = Network.GetSetFavoriteHeroResponse();
        if (setFavoriteHeroResponse.Success)
        {
            if ((setFavoriteHeroResponse.HeroClass == TAG_CLASS.INVALID) || (setFavoriteHeroResponse.Hero == null))
            {
                Debug.LogWarning(string.Format("CollectionManager.OnSetFavoriteHeroResponse: setting hero was a success, but message contains invalid class ({0}) and/or hero ({1})", setFavoriteHeroResponse.HeroClass, setFavoriteHeroResponse.Hero));
            }
            else
            {
                NetCache.NetCacheFavoriteHeroes netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFavoriteHeroes>();
                if (netObject != null)
                {
                    netObject.FavoriteHeroes[setFavoriteHeroResponse.HeroClass] = setFavoriteHeroResponse.Hero;
                    object[] args = new object[] { setFavoriteHeroResponse.HeroClass, setFavoriteHeroResponse.Hero };
                    Log.Rachelle.Print("CollectionManager.OnSetFavoriteHeroResponse: favorite hero for class {0} updated to {1}", args);
                }
                this.UpdateFavoriteHero(setFavoriteHeroResponse.HeroClass, setFavoriteHeroResponse.Hero.Name, setFavoriteHeroResponse.Hero.Premium);
            }
        }
    }

    public void RegisterAchievesCompletedListener(DelOnAchievesCompleted listener)
    {
        if (!this.m_achievesCompletedListeners.Contains(listener))
        {
            this.m_achievesCompletedListeners.Add(listener);
        }
    }

    public void RegisterCardRewardInsertedListener(DelOnCardRewardInserted listener)
    {
        if (!this.m_cardRewardListeners.Contains(listener))
        {
            this.m_cardRewardListeners.Add(listener);
        }
    }

    private bool RegisterCardStack(NetCache.CardStack netStack, EntityDef entityDef)
    {
        return this.RegisterCardStack(entityDef, netStack.Def.Name, new CardFlair(netStack.Def.Premium), new DateTime(netStack.Date), netStack.Count, netStack.NumSeen);
    }

    private bool RegisterCardStack(EntityDef entityDef, string cardID, CardFlair cardFlair, DateTime insertDate, int count, int numSeen)
    {
        if (entityDef == null)
        {
            Debug.LogError(string.Format("CollectionManager.RegisterCardStack(): DefLoader failed to get entity def for {0}", cardID));
            return false;
        }
        CollectionCardStack stack = this.RegisterEmptyCardStack(cardID);
        if (stack.ContainsArtStack(cardFlair))
        {
            Debug.LogWarning(string.Format("CollectionManager.RegisterCardStack(): Already have a registered art stack for {0} {1}", cardID, cardFlair));
            return false;
        }
        stack.AddCards(cardFlair, insertDate, count, numSeen);
        return true;
    }

    private void RegisterCardStacks()
    {
        if (!this.m_cardStacksRegistered)
        {
            foreach (NetCache.CardStack stack in NetCache.Get().GetNetObject<NetCache.NetCacheCollection>().Stacks)
            {
                string name = stack.Def.Name;
                if (GameUtils.IsCardCollectible(name))
                {
                    EntityDef entityDef = DefLoader.Get().GetEntityDef(name);
                    if (entityDef.IsHero())
                    {
                        this.RegisterHeroCardStack(stack, entityDef);
                    }
                    else
                    {
                        this.RegisterCardStack(stack, entityDef);
                    }
                }
            }
            this.m_cardStacksRegistered = true;
        }
    }

    public void RegisterCollectionChangedListener(DelOnCollectionChanged listener)
    {
        if (!this.m_collectionChangedListeners.Contains(listener))
        {
            this.m_collectionChangedListeners.Add(listener);
        }
    }

    public void RegisterCollectionLoadedListener(DelOnCollectionLoaded listener)
    {
        if (!this.m_collectionLoadedListeners.Contains(listener))
        {
            this.m_collectionLoadedListeners.Add(listener);
        }
    }

    public void RegisterCollectionNetHandlers()
    {
        Network network = Network.Get();
        network.RegisterNetHandler(PegasusUtil.DBAction.PacketID.ID, new Network.NetHandler(this.OnDBAction), null);
        network.RegisterNetHandler(DeckCreated.PacketID.ID, new Network.NetHandler(this.OnDeckCreated), null);
        network.RegisterNetHandler(DeckDeleted.PacketID.ID, new Network.NetHandler(this.OnDeckDeleted), null);
        network.RegisterNetHandler(DeckRenamed.PacketID.ID, new Network.NetHandler(this.OnDeckRenamed), null);
        network.RegisterNetHandler(BoughtSoldCard.PacketID.ID, new Network.NetHandler(this.OnCardSale), null);
        network.RegisterNetHandler(PegasusUtil.MassDisenchantResponse.PacketID.ID, new Network.NetHandler(this.OnMassDisenchantResponse), null);
        network.RegisterNetHandler(PegasusUtil.SetFavoriteHeroResponse.PacketID.ID, new Network.NetHandler(this.OnSetFavoriteHeroResponse), null);
        network.RegisterNetHandler(SetCardBackResponse.PacketID.ID, new Network.NetHandler(this.OnDefaultCardBackSet), null);
    }

    public void RegisterDeckContentsListener(DelOnDeckContents listener)
    {
        if (!this.m_deckContentsListeners.Contains(listener))
        {
            this.m_deckContentsListeners.Add(listener);
        }
    }

    public void RegisterDeckCreatedListener(DelOnDeckCreated listener)
    {
        if (!this.m_deckCreatedListeners.Contains(listener))
        {
            this.m_deckCreatedListeners.Add(listener);
        }
    }

    public void RegisterDeckDeletedListener(DelOnDeckDeleted listener)
    {
        if (!this.m_deckDeletedListeners.Contains(listener))
        {
            this.m_deckDeletedListeners.Add(listener);
        }
    }

    public bool RegisterDefaultCardbackChangedListener(DefaultCardbackChangedCallback callback)
    {
        return this.RegisterDefaultCardbackChangedListener(callback, null);
    }

    public bool RegisterDefaultCardbackChangedListener(DefaultCardbackChangedCallback callback, object userData)
    {
        DefaultCardbackChangedListener item = new DefaultCardbackChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_defaultCardbackChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_defaultCardbackChangedListeners.Add(item);
        return true;
    }

    private CollectionCardStack RegisterEmptyCardStack(string cardID)
    {
        CollectionCardStack collectionStack = this.GetCollectionStack(cardID);
        if (collectionStack == null)
        {
            collectionStack = new CollectionCardStack(cardID);
            this.m_collectibleStacks.Add(collectionStack.CardID, collectionStack);
        }
        return collectionStack;
    }

    public bool RegisterFavoriteHeroChangedListener(FavoriteHeroChangedCallback callback)
    {
        return this.RegisterFavoriteHeroChangedListener(callback, null);
    }

    public bool RegisterFavoriteHeroChangedListener(FavoriteHeroChangedCallback callback, object userData)
    {
        FavoriteHeroChangedListener item = new FavoriteHeroChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_favoriteHeroChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_favoriteHeroChangedListeners.Add(item);
        return true;
    }

    private void RegisterHeroCardFlair(string heroCardID, CardFlair cardFlair)
    {
        List<CardFlair> list = null;
        if (!this.m_heroFlair.TryGetValue(heroCardID, out list))
        {
            list = new List<CardFlair>();
            this.m_heroFlair[heroCardID] = list;
        }
        if (!list.Contains(cardFlair))
        {
            list.Add(cardFlair);
            list.Sort(new Comparison<CardFlair>(GameUtils.CardFlairSortComparisonDesc));
        }
    }

    private void RegisterHeroCardStack(NetCache.CardStack netStack, EntityDef entityDef)
    {
        this.RegisterHeroCardStack(entityDef, netStack.Def.Name, new CardFlair(netStack.Def.Premium), new DateTime(netStack.Date), netStack.Count, netStack.NumSeen);
    }

    private void RegisterHeroCardStack(EntityDef entityDef, string cardID, CardFlair cardFlair, DateTime insertDate, int count, int numSeen)
    {
        this.RegisterHeroCardFlair(cardID, cardFlair);
        this.RegisterCardStack(entityDef, cardID, cardFlair, insertDate, count, numSeen);
    }

    public void RegisterMassDisenchantListener(OnMassDisenchant listener)
    {
        if (!this.m_massDisenchantListeners.Contains(listener))
        {
            this.m_massDisenchantListeners.Add(listener);
        }
    }

    public void RegisterNewCardSeenListener(DelOnNewCardSeen listener)
    {
        if (!this.m_newCardSeenListeners.Contains(listener))
        {
            this.m_newCardSeenListeners.Add(listener);
        }
    }

    public void RegisterTaggedDeckChanged(OnTaggedDeckChanged listener)
    {
        this.m_taggedDeckChangedListeners.Add(listener);
    }

    public bool RemoveAchievesCompletedListener(DelOnAchievesCompleted listener)
    {
        return this.m_achievesCompletedListeners.Remove(listener);
    }

    public bool RemoveCardRewardInsertedListener(DelOnCardRewardInserted listener)
    {
        return this.m_cardRewardListeners.Remove(listener);
    }

    private void RemoveCollectionCard(string cardID, CardFlair cardFlair, int count)
    {
        CollectionCardStack collectionStack = this.GetCollectionStack(cardID);
        collectionStack.RemoveCards(cardFlair, count);
        int num = collectionStack.GetArtStack(cardFlair).Count;
        foreach (CollectionDeck deck in this.GetDecks().Values)
        {
            int cardCount = deck.GetCardCount(cardID, cardFlair);
            if (cardCount > num)
            {
                int num3 = cardCount - num;
                for (int i = 0; i < num3; i++)
                {
                    deck.RemoveCard(cardID, cardFlair.Premium, true);
                }
                if (!CollectionDeckTray.Get().HandleDeletedCardDeckUpdate(cardID, cardFlair))
                {
                    deck.SendChanges();
                }
            }
        }
        NetCache.CardDefinition cardDef = new NetCache.CardDefinition {
            Name = cardID,
            Premium = cardFlair.Premium
        };
        this.NotifyNetCacheOfRemovedCards(cardDef, count);
    }

    public bool RemoveCollectionChangedListener(DelOnCollectionChanged listener)
    {
        return this.m_collectionChangedListeners.Remove(listener);
    }

    public bool RemoveCollectionLoadedListener(DelOnCollectionLoaded listener)
    {
        return this.m_collectionLoadedListeners.Remove(listener);
    }

    public void RemoveCollectionNetHandlers()
    {
        Network network = Network.Get();
        network.RemoveNetHandler(PegasusUtil.DBAction.PacketID.ID, new Network.NetHandler(this.OnDBAction));
        network.RemoveNetHandler(DeckCreated.PacketID.ID, new Network.NetHandler(this.OnDeckCreated));
        network.RemoveNetHandler(DeckDeleted.PacketID.ID, new Network.NetHandler(this.OnDeckDeleted));
        network.RemoveNetHandler(DeckRenamed.PacketID.ID, new Network.NetHandler(this.OnDeckRenamed));
        network.RemoveNetHandler(BoughtSoldCard.PacketID.ID, new Network.NetHandler(this.OnCardSale));
        network.RemoveNetHandler(PegasusUtil.MassDisenchantResponse.PacketID.ID, new Network.NetHandler(this.OnMassDisenchantResponse));
        network.RemoveNetHandler(PegasusUtil.SetFavoriteHeroResponse.PacketID.ID, new Network.NetHandler(this.OnSetFavoriteHeroResponse));
        network.RemoveNetHandler(SetCardBackResponse.PacketID.ID, new Network.NetHandler(this.OnDefaultCardBackSet));
    }

    private void RemoveDeck(long id)
    {
        this.m_decks.Remove(id);
        this.m_baseDecks.Remove(id);
        NetCache.NetCacheDecks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>();
        for (int i = 0; i < netObject.Decks.Count; i++)
        {
            NetCache.DeckHeader header = netObject.Decks[i];
            if (header.ID == id)
            {
                netObject.Decks.RemoveAt(i);
                break;
            }
        }
    }

    public bool RemoveDeckContentsListener(DelOnDeckContents listener)
    {
        return this.m_deckContentsListeners.Remove(listener);
    }

    public bool RemoveDeckCreatedListener(DelOnDeckCreated listener)
    {
        return this.m_deckCreatedListeners.Remove(listener);
    }

    public bool RemoveDeckDeletedListener(DelOnDeckDeleted listener)
    {
        return this.m_deckDeletedListeners.Remove(listener);
    }

    public bool RemoveDefaultCardbackChangedListener(DefaultCardbackChangedCallback callback)
    {
        return this.RemoveDefaultCardbackChangedListener(callback, null);
    }

    public bool RemoveDefaultCardbackChangedListener(DefaultCardbackChangedCallback callback, object userData)
    {
        DefaultCardbackChangedListener item = new DefaultCardbackChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_defaultCardbackChangedListeners.Remove(item);
    }

    public bool RemoveFavoriteHeroChangedListener(FavoriteHeroChangedCallback callback)
    {
        return this.RemoveFavoriteHeroChangedListener(callback, null);
    }

    public bool RemoveFavoriteHeroChangedListener(FavoriteHeroChangedCallback callback, object userData)
    {
        FavoriteHeroChangedListener item = new FavoriteHeroChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_favoriteHeroChangedListeners.Remove(item);
    }

    public void RemoveMassDisenchantListener(OnMassDisenchant listener)
    {
        this.m_massDisenchantListeners.Remove(listener);
    }

    public bool RemoveNewCardSeenListener(DelOnNewCardSeen listener)
    {
        return this.m_newCardSeenListeners.Remove(listener);
    }

    public void RemoveTaggedDeckChanged(OnTaggedDeckChanged listener)
    {
        this.m_taggedDeckChangedListeners.Remove(listener);
    }

    public void RequestDeckContents(long id)
    {
        CollectionDeck deck = this.GetDeck(id);
        if ((deck != null) && deck.NetworkContentsLoaded())
        {
            this.FireDeckContentsEvent(id);
        }
        else
        {
            float num2;
            float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            if ((this.m_pendingRequestDeckContents != null) && this.m_pendingRequestDeckContents.TryGetValue(id, out num2))
            {
                if ((num2 - realtimeSinceStartup) < 10f)
                {
                    return;
                }
                this.m_pendingRequestDeckContents.Remove(id);
            }
            if (this.m_pendingRequestDeckContents == null)
            {
                this.m_pendingRequestDeckContents = new Map<long, float>();
            }
            this.m_pendingRequestDeckContents[id] = realtimeSinceStartup;
            Network.Get().GetDeckContents(id);
        }
    }

    public void SendCreateDeck(DeckType deckType, string name, string heroCardID)
    {
        int heroDatabaseAssetID = GameUtils.TranslateCardIdToDbId(heroCardID);
        if (heroDatabaseAssetID == 0)
        {
            Debug.LogWarning(string.Format("CollectionManager.SendCreateDeck(): Unknown hero cardID {0}", heroCardID));
        }
        else
        {
            CardFlair bestHeroFlairOwned = this.GetBestHeroFlairOwned(heroCardID);
            if (bestHeroFlairOwned == null)
            {
                Debug.LogWarning(string.Format("CollectionManager.SendCreateDeck(): bestOwnedFlair is null for cardID {0}", heroCardID));
            }
            else
            {
                Network.Get().CreateDeck(deckType, name, heroDatabaseAssetID, bestHeroFlairOwned.Premium);
            }
        }
    }

    public void SendDeleteDeck(long id)
    {
        Network.Get().DeleteDeck(id);
    }

    public void SetHasVisitedCollection(bool enable)
    {
        this.m_hasVisitedCollection = enable;
    }

    public void SetTaggedDeck(DeckTag tag, CollectionDeck deck, object callbackData = null)
    {
        CollectionDeck taggedDeck = this.GetTaggedDeck(tag);
        if (deck != taggedDeck)
        {
            this.m_taggedDecks[tag] = deck;
            foreach (OnTaggedDeckChanged changed in this.m_taggedDeckChangedListeners.ToArray())
            {
                changed(tag, deck, taggedDeck, callbackData);
            }
        }
    }

    public CollectionDeck SetTaggedDeck(DeckTag tag, long deckId, object callbackData = null)
    {
        CollectionDeck deck = null;
        this.m_decks.TryGetValue(deckId, out deck);
        this.SetTaggedDeck(tag, deck, callbackData);
        return deck;
    }

    public CollectionDeck StartEditingDeck(DeckTag tag, long deckId, object callbackData = null)
    {
        this.m_editMode = true;
        if (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL)
        {
            Enum[] args = new Enum[] { PresenceStatus.DECKEDITOR };
            PresenceMgr.Get().SetStatus(args);
        }
        return this.SetTaggedDeck(tag, deckId, callbackData);
    }

    private void UpdateCardCounts(NetCache.NetCacheCollection netCacheCards, NetCache.CardDefinition cardDef, int count, int newCount)
    {
        netCacheCards.TotalCardsOwned += count;
        if (cardDef.Premium == TAG_PREMIUM.NORMAL)
        {
            EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDef.Name);
            if (entityDef.IsBasicCardUnlock())
            {
                Map<TAG_CLASS, int> map;
                TAG_CLASS tag_class;
                int num = !entityDef.IsElite() ? 2 : 1;
                if ((newCount < 0) || (newCount > num))
                {
                    Debug.LogError(string.Concat(new object[] { "CollectionManager.UpdateCardCounts: created an illegal stack size of ", newCount, " for card ", entityDef }));
                    count = 0;
                }
                int num2 = map[tag_class];
                (map = netCacheCards.BasicCardsUnlockedPerClass)[tag_class = entityDef.GetClass()] = num2 + count;
            }
        }
    }

    private void UpdateDeckHeroArt(string heroCardID)
    {
        <UpdateDeckHeroArt>c__AnonStorey2D0 storeyd = new <UpdateDeckHeroArt>c__AnonStorey2D0 {
            heroCardID = heroCardID
        };
        CardFlair bestHeroFlairOwned = this.GetBestHeroFlairOwned(storeyd.heroCardID);
        if (bestHeroFlairOwned == null)
        {
            Debug.LogError(string.Format("CollectionManager.UpdateDeckHeroArt({0}): bestArtOwned is null", storeyd.heroCardID));
        }
        else
        {
            TAG_CLASS heroClass = DefLoader.Get().GetEntityDef(storeyd.heroCardID).GetClass();
            PreconDeck preconDeck = this.GetPreconDeck(heroClass);
            if (preconDeck != null)
            {
                this.m_preconDecks[heroClass] = new PreconDeck(preconDeck.ID, bestHeroFlairOwned);
            }
            foreach (CollectionDeck deck2 in this.m_baseDecks.Values.ToList<CollectionDeck>().FindAll(new Predicate<CollectionDeck>(storeyd.<>m__A3)))
            {
                deck2.HeroCardFlair = bestHeroFlairOwned;
            }
            foreach (CollectionDeck deck3 in this.m_decks.Values.ToList<CollectionDeck>().FindAll(new Predicate<CollectionDeck>(storeyd.<>m__A4)))
            {
                deck3.HeroCardFlair = bestHeroFlairOwned;
            }
        }
    }

    private void UpdateFavoriteHero(TAG_CLASS heroClass, string heroCardId, TAG_PREMIUM premium)
    {
        foreach (NetCache.DeckHeader header in NetCache.Get().GetNetObject<NetCache.NetCacheDecks>().Decks)
        {
            if (!header.HeroOverridden)
            {
                EntityDef entityDef = DefLoader.Get().GetEntityDef(header.Hero);
                if (heroClass == entityDef.GetClass())
                {
                    header.Hero = heroCardId;
                    header.HeroPremium = premium;
                    CollectionDeck deck = this.GetDeck(header.ID);
                    if (deck != null)
                    {
                        deck.HeroCardID = heroCardId;
                        deck.HeroCardFlair.Premium = premium;
                    }
                    CollectionDeck baseDeck = this.GetBaseDeck(header.ID);
                    if (baseDeck != null)
                    {
                        baseDeck.HeroCardID = heroCardId;
                        baseDeck.HeroCardFlair.Premium = premium;
                    }
                }
            }
        }
        if (this.m_favoriteHeroChangedListeners.Count > 0)
        {
            NetCache.CardDefinition favoriteHero = new NetCache.CardDefinition {
                Name = heroCardId,
                Premium = premium
            };
            foreach (FavoriteHeroChangedListener listener in this.m_favoriteHeroChangedListeners.ToArray())
            {
                listener.Fire(heroClass, favoriteHero);
            }
        }
    }

    private void UpdateShowAdvancedCMOption()
    {
        if (!Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, false) && (NetCache.Get().GetNetObject<NetCache.NetCacheCollection>().TotalCardsOwned >= 0x74))
        {
            Options.Get().SetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, true);
        }
    }

    private void WillReset()
    {
        this.m_heroFlair.Clear();
        this.m_decks.Clear();
        this.m_baseDecks.Clear();
        this.m_preconDecks.Clear();
        this.m_defaultCardbackChangedListeners.Clear();
        this.m_favoriteHeroChangedListeners.Clear();
        if (<>f__am$cache1C == null)
        {
            <>f__am$cache1C = kvp => kvp.Key;
        }
        if (<>f__am$cache1D == null)
        {
            <>f__am$cache1D = kvp => new CollectionCardStack(kvp.Key);
        }
        this.m_collectibleStacks = Enumerable.ToDictionary<KeyValuePair<string, CollectionCardStack>, string, CollectionCardStack>(this.m_collectibleStacks, <>f__am$cache1C, <>f__am$cache1D);
        this.m_cardStacksRegistered = false;
    }

    [CompilerGenerated]
    private sealed class <GetBestHeroesIOwn>c__AnonStorey2CD
    {
        internal NetCache.CardDefinition hero;

        internal bool <>m__9D(NetCache.CardDefinition e)
        {
            return (this.hero.Name == e.Name);
        }
    }

    [CompilerGenerated]
    private sealed class <GetHeroesIOwn>c__AnonStorey2CC
    {
        internal TAG_CLASS heroClass;

        internal bool <>m__9C(CollectionCardStack obj)
        {
            EntityDef entityDef = DefLoader.Get().GetEntityDef(obj.CardID);
            return (entityDef.IsHero() && (this.heroClass == entityDef.GetClass()));
        }
    }

    [CompilerGenerated]
    private sealed class <MarkAllInstancesAsSeen>c__AnonStorey2CE
    {
        internal CollectionCardStack.ArtStack artStack;

        internal bool <>m__9E(NetCache.CardStack obj)
        {
            if (!obj.Def.Name.Equals(this.artStack.CardID))
            {
                return false;
            }
            return this.artStack.Flair.Equals(new CardFlair(obj.Def.Premium));
        }
    }

    [CompilerGenerated]
    private sealed class <NotifyNetCacheOfNewCards>c__AnonStorey2D2
    {
        internal NetCache.CardDefinition cardDef;

        internal bool <>m__A6(NetCache.CardStack obj)
        {
            return (obj.Def.Name.Equals(this.cardDef.Name) && (obj.Def.Premium == this.cardDef.Premium));
        }
    }

    [CompilerGenerated]
    private sealed class <NotifyNetCacheOfRemovedCards>c__AnonStorey2D1
    {
        internal NetCache.CardDefinition cardDef;

        internal bool <>m__A5(NetCache.CardStack obj)
        {
            return (obj.Def.Name.Equals(this.cardDef.Name) && (obj.Def.Premium == this.cardDef.Premium));
        }
    }

    [CompilerGenerated]
    private sealed class <NumCardsOwnedInSet>c__AnonStorey2CF
    {
        internal TAG_CARD_SET cardSet;

        internal bool <>m__A1(CollectionCardStack obj)
        {
            EntityDef entityDef = DefLoader.Get().GetEntityDef(obj.CardID);
            return (this.cardSet == entityDef.GetCardSet());
        }
    }

    [CompilerGenerated]
    private sealed class <OnDBAction>c__AnonStorey2CA
    {
        internal long deckID;

        internal bool <>m__9A(NetCache.DeckHeader deckHeader)
        {
            return (deckHeader.ID == this.deckID);
        }
    }

    [CompilerGenerated]
    private sealed class <OnDeckRenamed>c__AnonStorey2CB
    {
        internal long id;

        internal bool <>m__9B(NetCache.DeckHeader deckHeader)
        {
            return (deckHeader.ID == this.id);
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateDeckHeroArt>c__AnonStorey2D0
    {
        internal string heroCardID;

        internal bool <>m__A3(CollectionDeck obj)
        {
            return obj.HeroCardID.Equals(this.heroCardID);
        }

        internal bool <>m__A4(CollectionDeck obj)
        {
            return obj.HeroCardID.Equals(this.heroCardID);
        }
    }

    public class DeckSort : IComparer<CollectionDeck>
    {
        public int Compare(CollectionDeck a, CollectionDeck b)
        {
            return a.ID.CompareTo(b.ID);
        }
    }

    public enum DeckTag
    {
        Editing,
        Arena
    }

    public delegate void DefaultCardbackChangedCallback(int newDefaultCardBackID, object userData);

    private class DefaultCardbackChangedListener : EventListener<CollectionManager.DefaultCardbackChangedCallback>
    {
        public void Fire(int newDefaultCardBackID)
        {
            base.m_callback(newDefaultCardBackID, base.m_userData);
        }
    }

    public delegate void DelOnAchievesCompleted(List<Achievement> achievements);

    public delegate void DelOnAllDeckContents();

    public delegate void DelOnCardRewardInserted(string cardID, CardFlair flair);

    public delegate void DelOnCollectionChanged();

    public delegate void DelOnCollectionLoaded();

    public delegate void DelOnDeckContents(long id);

    public delegate void DelOnDeckCreated(long id);

    public delegate void DelOnDeckDeleted(long id);

    public delegate void DelOnNewCardSeen(string cardID, CardFlair flair);

    public delegate void FavoriteHeroChangedCallback(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero, object userData);

    private class FavoriteHeroChangedListener : EventListener<CollectionManager.FavoriteHeroChangedCallback>
    {
        public void Fire(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero)
        {
            base.m_callback(heroClass, favoriteHero, base.m_userData);
        }
    }

    public delegate void OnMassDisenchant(int amount);

    public delegate void OnTaggedDeckChanged(CollectionManager.DeckTag tag, CollectionDeck newDeck, CollectionDeck oldDeck, object callbackData);

    public class PreconDeck
    {
        private CardFlair m_heroFlair;
        private long m_id;

        public PreconDeck(long id, CardFlair heroFlair)
        {
            this.m_id = id;
            this.m_heroFlair = heroFlair;
        }

        public CardFlair HeroFlair
        {
            get
            {
                return this.m_heroFlair;
            }
        }

        public long ID
        {
            get
            {
                return this.m_id;
            }
        }
    }
}

