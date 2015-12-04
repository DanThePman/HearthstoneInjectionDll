using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CollectionFilterSet
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map1;
    private List<CollectionArtFilterSet> m_artFilterSets = new List<CollectionArtFilterSet>();
    private CardFlair m_flairRestriction;
    private bool m_onlyCraftable;

    public CollectionFilterSet(bool mustPassAllFilters)
    {
        IEnumerator enumerator = Enum.GetValues(typeof(TAG_PREMIUM)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                TAG_PREMIUM current = (TAG_PREMIUM) ((int) enumerator.Current);
                if (!ApplicationMgr.IsInternal() || !this.ShouldSkipPremiumType(current))
                {
                    CollectionArtFilterSet item = new CollectionArtFilterSet(new CardFlair(current), mustPassAllFilters);
                    this.m_artFilterSets.Add(item);
                }
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

    public void AddAccountFilter(ACCOUNT_TAG key, object val, CollectionFilterFunc func)
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            set.AddAccountFilter(key, val, func);
        }
    }

    public void AddGameFilter(GAME_TAG key, List<object> values, CollectionFilterFunc func)
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            set.AddGameFilter(key, values, func);
        }
    }

    public void AddGameFilter(GAME_TAG key, object val, CollectionFilterFunc func)
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            set.AddGameFilter(key, val, func);
        }
    }

    public void ClearFlairRestriction()
    {
        this.m_flairRestriction = null;
    }

    public List<FilteredArtStack> GenerateList()
    {
        List<FilteredArtStack> list;
        if (this.m_flairRestriction != null)
        {
            CollectionArtFilterSet set = this.m_artFilterSets.Find(obj => obj.Flair.Equals(this.m_flairRestriction));
            if (set == null)
            {
                Debug.LogError("Generate List: artFilterSet is null");
            }
            list = set.GenerateList();
        }
        else
        {
            list = CollectionArtFilterSet.GenerateUnion(this.m_artFilterSets);
        }
        if (this.m_onlyCraftable)
        {
            <GenerateList>c__AnonStorey2C9 storeyc = new <GenerateList>c__AnonStorey2C9 {
                cardValues = NetCache.Get().GetNetObject<NetCache.NetCacheCardValues>()
            };
            return list.FindAll(new Predicate<FilteredArtStack>(storeyc.<>m__99));
        }
        return list;
    }

    public void IncludeCardsNotOwned(List<CardFlair> flairTypes)
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            if (flairTypes.Contains(set.Flair))
            {
                set.RemoveAccountFilter(ACCOUNT_TAG.OWNED_COUNT, 1, CollectionFilterFunc.GREATER_EQUAL);
            }
            else
            {
                set.AddAccountFilter(ACCOUNT_TAG.OWNED_COUNT, 1, CollectionFilterFunc.GREATER_EQUAL);
            }
        }
    }

    public bool IsTextFilterEmpty()
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            if (!set.IsTextFilterEmpty())
            {
                return false;
            }
        }
        return true;
    }

    public void RemoveAccountFilter(ACCOUNT_TAG key, object val, CollectionFilterFunc func)
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            set.RemoveAccountFilter(key, val, func);
        }
    }

    public void RemoveAllGameFilters()
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            set.RemoveAllGameFilters();
        }
    }

    public void RemoveAllGameFiltersByTag(GAME_TAG key)
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            set.RemoveAllGameFiltersByTag(key);
        }
    }

    public void RemoveTextFilter()
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            set.RemoveTextFilter();
        }
    }

    public void SetFlairRestriction(CardFlair flair)
    {
        this.m_flairRestriction = flair;
    }

    public void SetIncludeOnlyCraftable(bool onlyCraftable)
    {
        this.m_onlyCraftable = onlyCraftable;
    }

    public void SetTextFilterValue(string text)
    {
        foreach (CollectionArtFilterSet set in this.m_artFilterSets)
        {
            set.SetTextFilterValue(text);
        }
    }

    private bool ShouldSkipPremiumType(TAG_PREMIUM premiumType)
    {
        string key = Options.Get().GetString(Option.COLLECTION_PREMIUM_TYPE);
        if (key != null)
        {
            int num;
            if (<>f__switch$map1 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                dictionary.Add("gold", 0);
                dictionary.Add("normal", 1);
                dictionary.Add("standard", 1);
                <>f__switch$map1 = dictionary;
            }
            if (<>f__switch$map1.TryGetValue(key, out num))
            {
                if (num == 0)
                {
                    if (premiumType != TAG_PREMIUM.GOLDEN)
                    {
                        return true;
                    }
                }
                else if ((num == 1) && (premiumType != TAG_PREMIUM.NORMAL))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [CompilerGenerated]
    private sealed class <GenerateList>c__AnonStorey2C9
    {
        internal NetCache.NetCacheCardValues cardValues;

        internal bool <>m__99(FilteredArtStack obj)
        {
            NetCache.CardDefinition key = new NetCache.CardDefinition {
                Name = obj.CardID,
                Premium = obj.Flair.Premium
            };
            return this.cardValues.Values.ContainsKey(key);
        }
    }

    private class CollectionArtFilterSet
    {
        private List<CollectionFilter<ACCOUNT_TAG>> m_accountFilters = new List<CollectionFilter<ACCOUNT_TAG>>();
        private CardFlair m_flairRestriction;
        private List<CollectionFilter<GAME_TAG>> m_gameFilters = new List<CollectionFilter<GAME_TAG>>();
        private bool m_mustPassAllFilters;
        private CollectionTextFilter m_textFilter;

        public CollectionArtFilterSet(CardFlair flairRestriction, bool mustPassAllFilters)
        {
            this.m_flairRestriction = flairRestriction;
            this.m_mustPassAllFilters = mustPassAllFilters;
        }

        public void AddAccountFilter(ACCOUNT_TAG key, object val, CollectionFilterFunc func)
        {
            if (!this.HasAccountFilter(key, val, func))
            {
                CollectionFilter<ACCOUNT_TAG> item = new CollectionFilter<ACCOUNT_TAG>();
                item.SetKey(key);
                item.SetValue(val);
                item.SetFunc(func);
                this.m_accountFilters.Add(item);
            }
        }

        public void AddGameFilter(GAME_TAG key, List<object> values, CollectionFilterFunc func)
        {
            if (!this.HasGameFilter(key, values, func))
            {
                CollectionFilter<GAME_TAG> item = new CollectionFilter<GAME_TAG>();
                item.SetKey(key);
                item.SetValues(values);
                item.SetFunc(func);
                this.m_gameFilters.Add(item);
            }
        }

        public void AddGameFilter(GAME_TAG key, object val, CollectionFilterFunc func)
        {
            if (!this.HasGameFilter(key, val, func))
            {
                CollectionFilter<GAME_TAG> item = new CollectionFilter<GAME_TAG>();
                item.SetKey(key);
                item.SetValue(val);
                item.SetFunc(func);
                this.m_gameFilters.Add(item);
            }
        }

        private bool FiltersExist()
        {
            return ((this.m_gameFilters.Count > 0) || ((this.m_accountFilters.Count > 0) || ((this.m_textFilter != null) && this.m_textFilter.ShouldRunFilter())));
        }

        public List<FilteredArtStack> GenerateList()
        {
            return new List<FilteredArtStack>(this.GenerateMap().Values);
        }

        private Map<NetCache.CardDefinition, FilteredArtStack> GenerateMap()
        {
            Map<NetCache.CardDefinition, FilteredArtStack> map = new Map<NetCache.CardDefinition, FilteredArtStack>();
            foreach (CollectionCardStack stack in CollectionManager.Get().GetCollectibleStacks().Values)
            {
                CollectionCardStack.ArtStack artStack = stack.GetArtStack(this.m_flairRestriction);
                FilteredArtStack stack3 = !this.m_mustPassAllFilters ? this.RunFilters_CanPassAny(artStack) : this.RunFilters_MustPassAll(artStack);
                if (stack3 != null)
                {
                    map.Add(stack3.GetCardDefinition(), stack3);
                }
            }
            return map;
        }

        public static List<FilteredArtStack> GenerateUnion(List<CollectionFilterSet.CollectionArtFilterSet> artFilterSets)
        {
            if ((artFilterSets == null) || (artFilterSets.Count == 0))
            {
                return new List<FilteredArtStack>();
            }
            Map<NetCache.CardDefinition, FilteredArtStack> map = new Map<NetCache.CardDefinition, FilteredArtStack>();
            foreach (CollectionFilterSet.CollectionArtFilterSet set in artFilterSets)
            {
                foreach (KeyValuePair<NetCache.CardDefinition, FilteredArtStack> pair in set.GenerateMap())
                {
                    NetCache.CardDefinition key = pair.Key;
                    FilteredArtStack stack = pair.Value;
                    if (!map.ContainsKey(key))
                    {
                        map.Add(key, stack);
                    }
                    else
                    {
                        FilteredArtStack stack2 = map[key];
                        if (!stack2.MergeWithArtStack(stack))
                        {
                            Debug.LogWarning(string.Format("CollectionArtFilterSet.GenerateUnion(): could not merge filtered art stack {0} with filtered art stack {1}", stack2, stack));
                        }
                    }
                }
            }
            return new List<FilteredArtStack>(map.Values);
        }

        public List<CollectionFilter<ACCOUNT_TAG>> GetAccountFilters()
        {
            return this.m_accountFilters;
        }

        public List<CollectionFilter<GAME_TAG>> GetGameFilters()
        {
            return this.m_gameFilters;
        }

        public bool HasAccountFilter(ACCOUNT_TAG key, List<object> values, CollectionFilterFunc func)
        {
            foreach (CollectionFilter<ACCOUNT_TAG> filter in this.m_accountFilters)
            {
                if (((ACCOUNT_TAG) filter.GetKey()) != key)
                {
                    continue;
                }
                bool flag = values.Count > 0;
                foreach (object obj2 in values)
                {
                    if (!filter.HasValue(obj2))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag && (filter.GetFunc() == func))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasAccountFilter(ACCOUNT_TAG key, object val, CollectionFilterFunc func)
        {
            List<object> values = new List<object> {
                val
            };
            return this.HasAccountFilter(key, values, func);
        }

        public bool HasGameFilter(GAME_TAG key, List<object> values, CollectionFilterFunc func)
        {
            foreach (CollectionFilter<GAME_TAG> filter in this.m_gameFilters)
            {
                if (((GAME_TAG) filter.GetKey()) != key)
                {
                    continue;
                }
                bool flag = values.Count > 0;
                foreach (object obj2 in values)
                {
                    if (!filter.HasValue(obj2))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag && (filter.GetFunc() == func))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasGameFilter(GAME_TAG key, object val, CollectionFilterFunc func)
        {
            List<object> values = new List<object> {
                val
            };
            return this.HasGameFilter(key, values, func);
        }

        public bool IsMustPassAllFilters()
        {
            return this.m_mustPassAllFilters;
        }

        public bool IsTextFilterEmpty()
        {
            return ((this.m_textFilter == null) || this.m_textFilter.IsEmpty());
        }

        public void RemoveAccountFilter(ACCOUNT_TAG key, object val, CollectionFilterFunc func)
        {
            for (int i = 0; i < this.m_accountFilters.Count; i++)
            {
                CollectionFilter<ACCOUNT_TAG> filter = this.m_accountFilters[i];
                if (((((ACCOUNT_TAG) filter.GetKey()) == key) && filter.HasValue(val)) && (filter.GetFunc() == func))
                {
                    this.m_accountFilters.RemoveAt(i);
                    return;
                }
            }
        }

        public void RemoveAllAccountFilters()
        {
            this.m_accountFilters.Clear();
        }

        public void RemoveAllGameFilters()
        {
            this.m_gameFilters.Clear();
        }

        public void RemoveAllGameFiltersByTag(GAME_TAG key)
        {
            for (int i = this.m_gameFilters.Count - 1; i >= 0; i--)
            {
                CollectionFilter<GAME_TAG> filter = this.m_gameFilters[i];
                if (((GAME_TAG) filter.GetKey()) == key)
                {
                    this.m_gameFilters.RemoveAt(i);
                }
            }
        }

        public void RemoveGameFilter(GAME_TAG key, object val, CollectionFilterFunc func)
        {
            for (int i = 0; i < this.m_gameFilters.Count; i++)
            {
                CollectionFilter<GAME_TAG> filter = this.m_gameFilters[i];
                if (((((GAME_TAG) filter.GetKey()) == key) && filter.HasValue(val)) && (filter.GetFunc() == func))
                {
                    this.m_gameFilters.RemoveAt(i);
                    return;
                }
            }
        }

        public void RemoveTextFilter()
        {
            if (this.m_textFilter == null)
            {
                this.m_textFilter.ClearFilter();
            }
        }

        private bool RunAccountFilter(CollectionCardStack.ArtStack artStack, CollectionFilter<ACCOUNT_TAG> filter)
        {
            ACCOUNT_TAG key = filter.GetKey();
            if (key != ACCOUNT_TAG.OWNED_COUNT)
            {
                return ((key == ACCOUNT_TAG.NEWEST_INSERT_DATE) && filter.DoesValueMatch(artStack.NewestInsertDate.Ticks));
            }
            return filter.DoesValueMatch(artStack.Count);
        }

        private FilteredArtStack RunFilters_CanPassAny(CollectionCardStack.ArtStack artStack)
        {
            if (!this.FiltersExist())
            {
                return new FilteredArtStack(artStack);
            }
            EntityDef entityDef = DefLoader.Get().GetEntityDef(artStack.CardID);
            if (entityDef == null)
            {
                Log.Rachelle.Print(string.Format("WARNING CollectionFilter.RunFilters_CanPassAny() - entityDef for {0} is null", artStack.CardID), new object[0]);
                return null;
            }
            foreach (CollectionFilter<GAME_TAG> filter in this.m_gameFilters)
            {
                if (this.RunGameFilter(entityDef, filter))
                {
                    return new FilteredArtStack(artStack);
                }
            }
            foreach (CollectionFilter<ACCOUNT_TAG> filter2 in this.m_accountFilters)
            {
                if (this.RunAccountFilter(artStack, filter2))
                {
                    return new FilteredArtStack(artStack);
                }
            }
            if (((this.m_textFilter != null) && this.m_textFilter.ShouldRunFilter()) && this.RunTextFilter(entityDef, artStack, this.m_textFilter))
            {
                return new FilteredArtStack(artStack);
            }
            return null;
        }

        private FilteredArtStack RunFilters_MustPassAll(CollectionCardStack.ArtStack artStack)
        {
            if (this.FiltersExist())
            {
                EntityDef entityDef = DefLoader.Get().GetEntityDef(artStack.CardID);
                if (entityDef == null)
                {
                    Log.Rachelle.Print(string.Format("WARNING CollectionFilter.RunFilters_MustPassAll() - entityDef for {0} is null", artStack.CardID), new object[0]);
                    return null;
                }
                foreach (CollectionFilter<GAME_TAG> filter in this.m_gameFilters)
                {
                    if (!this.RunGameFilter(entityDef, filter))
                    {
                        return null;
                    }
                }
                foreach (CollectionFilter<ACCOUNT_TAG> filter2 in this.m_accountFilters)
                {
                    if (!this.RunAccountFilter(artStack, filter2))
                    {
                        return null;
                    }
                }
                if (((this.m_textFilter != null) && this.m_textFilter.ShouldRunFilter()) && !this.RunTextFilter(entityDef, artStack, this.m_textFilter))
                {
                    return null;
                }
            }
            return new FilteredArtStack(artStack);
        }

        private bool RunGameFilter(EntityDef entityDef, CollectionFilter<GAME_TAG> filter)
        {
            GAME_TAG key = filter.GetKey();
            return filter.DoesValueMatch(entityDef.GetTag(key));
        }

        private bool RunTextFilter(EntityDef entityDef, CollectionCardStack.ArtStack artStack, CollectionTextFilter filter)
        {
            return filter.DoesValueMatch(entityDef, artStack);
        }

        public void SetTextFilterValue(string text)
        {
            if (this.m_textFilter == null)
            {
                this.m_textFilter = new CollectionTextFilter();
            }
            this.m_textFilter.SetTextFilterValue(text);
        }

        public CardFlair Flair
        {
            get
            {
                return this.m_flairRestriction;
            }
        }
    }
}

