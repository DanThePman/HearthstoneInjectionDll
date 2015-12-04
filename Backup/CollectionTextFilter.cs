using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class CollectionTextFilter
{
    [CompilerGenerated]
    private static Func<string, bool> <>f__am$cache9;
    [CompilerGenerated]
    private static Func<string, string> <>f__am$cacheA;
    [CompilerGenerated]
    private static Func<string, string> <>f__am$cacheB;
    [CompilerGenerated]
    private static Func<string, string> <>f__am$cacheC;
    private static readonly GAME_TAG[] FILTERABLE_KEYWORDS;
    private List<CollectionFilter<GAME_TAG>> m_deducedFilters;
    private List<string> m_multiwordedStringTokens = null;
    private Map<string, CollectionFilter<GAME_TAG>> m_namedFilters = new Map<string, CollectionFilter<GAME_TAG>>();
    private List<string> m_stringTokens = new List<string>();
    private string m_unreadLowerCaseString;
    private string m_unreadString;
    private static Map<TAG_CARD_SET, string[]> s_cachedShorthandCardSetNames;
    private static Map<char, string> s_europeanConversionTable;

    static CollectionTextFilter()
    {
        Map<char, string> map = new Map<char, string>();
        map.Add('œ', "oe");
        map.Add('\x00e6', "ae");
        map.Add('’', "'");
        map.Add('\x00ab', "\"");
        map.Add('\x00bb', "\"");
        map.Add('\x00e4', "ae");
        map.Add('\x00fc', "ue");
        map.Add('\x00f6', "oe");
        map.Add('\x00df', "ss");
        s_europeanConversionTable = map;
        FILTERABLE_KEYWORDS = new GAME_TAG[] { GAME_TAG.TAUNT, GAME_TAG.STEALTH, GAME_TAG.DIVINE_SHIELD, GAME_TAG.ENRAGED, GAME_TAG.CHARGE, GAME_TAG.SPELLPOWER, GAME_TAG.WINDFURY, GAME_TAG.BATTLECRY, GAME_TAG.DEATHRATTLE };
        s_cachedShorthandCardSetNames = null;
    }

    public CollectionTextFilter()
    {
        this.m_namedFilters = new Map<string, CollectionFilter<GAME_TAG>>();
        this.m_unreadString = GameStrings.Get("GLUE_COLLECTION_CARD_NEW");
        this.m_unreadLowerCaseString = this.m_unreadString.ToLower();
        this.AddNamedFiltersForTags(FILTERABLE_KEYWORDS, new FilterHasNameFunction<GAME_TAG>(GameStrings.HasKeywordName), new FilterGetNameFunction<GAME_TAG>(GameStrings.GetKeywordName));
        this.AddNamedFiltersForEnum<TAG_RARITY>(GAME_TAG.RARITY, new FilterHasNameFunction<TAG_RARITY>(GameStrings.HasRarityText), new FilterGetNameFunction<TAG_RARITY>(GameStrings.GetRarityText));
        this.AddNamedFiltersForEnum<TAG_RACE>(GAME_TAG.CARDRACE, new FilterHasNameFunction<TAG_RACE>(GameStrings.HasRaceName), new FilterGetNameFunction<TAG_RACE>(GameStrings.GetRaceName));
        this.AddNamedFiltersForEnum<TAG_CARDTYPE>(GAME_TAG.CARDTYPE, new FilterHasNameFunction<TAG_CARDTYPE>(GameStrings.HasCardTypeName), new FilterGetNameFunction<TAG_CARDTYPE>(GameStrings.GetCardTypeName));
    }

    private void AddNamedFilter(string name, CollectionFilter<GAME_TAG> filter)
    {
        string input = name.ToLower();
        string key = RemoveDiacritics(ConvertEuropeanCharacters(input));
        if (!this.m_namedFilters.ContainsKey(key))
        {
            this.m_namedFilters.Add(key, filter);
        }
        else
        {
            Debug.LogWarning(string.Format("CollectionFilter.AddNamedFilter() trying to add duplicate tag '{0}'. Did a tag get added on the server that the client doesn't know about?", input));
        }
    }

    private void AddNamedFiltersForEnum<T>(GAME_TAG key, FilterHasNameFunction<T> hasNameFunction, FilterGetNameFunction<T> getNameFunction)
    {
        IEnumerator enumerator = Enum.GetValues(typeof(T)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                T current = (T) enumerator.Current;
                if (hasNameFunction(current))
                {
                    string name = getNameFunction(current);
                    CollectionFilter<GAME_TAG> filter = new CollectionFilter<GAME_TAG>();
                    filter.SetKey(key);
                    filter.SetValue(current);
                    filter.SetFunc(CollectionFilterFunc.EQUAL);
                    this.AddNamedFilter(name, filter);
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

    private void AddNamedFiltersForTags(GAME_TAG[] flaggableKeys, FilterHasNameFunction<GAME_TAG> hasNameFunction, FilterGetNameFunction<GAME_TAG> getNameFunction)
    {
        foreach (GAME_TAG game_tag in flaggableKeys)
        {
            if (hasNameFunction(game_tag))
            {
                string name = getNameFunction(game_tag);
                CollectionFilter<GAME_TAG> filter = new CollectionFilter<GAME_TAG>();
                filter.SetKey(game_tag);
                filter.SetValue(1);
                filter.SetFunc(CollectionFilterFunc.EQUAL);
                this.AddNamedFilter(name, filter);
            }
        }
    }

    private bool CheckContains(string str, string filter, string filterEuropean, string filterDiacritics)
    {
        return ((str != null) && ((str.Contains(filter) || str.Contains(filterEuropean)) || str.Contains(filterDiacritics)));
    }

    private bool CheckMultiwordValue(IEnumerable<string> searchTerms, IEnumerable<string> multiwordValue)
    {
        IEnumerator<string> enumerator = multiwordValue.GetEnumerator();
        int num = 0;
        IEnumerator<string> enumerator2 = searchTerms.GetEnumerator();
        try
        {
            while (enumerator2.MoveNext())
            {
                string current = enumerator2.Current;
                num++;
                if (!enumerator.MoveNext() || !this.CheckStringEqualsVariousForms(current, enumerator.Current))
                {
                    return false;
                }
            }
        }
        finally
        {
            if (enumerator2 == null)
            {
            }
            enumerator2.Dispose();
        }
        if (enumerator.MoveNext())
        {
            return false;
        }
        return true;
    }

    private List<string> CheckMultiwordValue(List<string> tokens, string multiwordValue)
    {
        if (multiwordValue != null)
        {
            char[] separator = new char[] { ' ' };
            string[] strArray = multiwordValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length < 2)
            {
                return tokens;
            }
            for (int i = 0; i < tokens.Count; i++)
            {
                bool flag = true;
                for (int j = 0; j < strArray.Length; j++)
                {
                    if (((i + j) >= tokens.Count) || !this.CheckStringEqualsVariousForms(strArray[j], tokens[i + j]))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    List<string> list = new List<string>((tokens.Count - strArray.Length) + 1);
                    for (int k = 0; k < i; k++)
                    {
                        list.Add(tokens[k]);
                    }
                    list.Add(multiwordValue);
                    for (int m = i + strArray.Length; m < tokens.Count; m++)
                    {
                        list.Add(tokens[m]);
                    }
                    return list;
                }
            }
        }
        return tokens;
    }

    private bool CheckNamedFilterContains(string filterString, out string foundKey)
    {
        foundKey = null;
        if (this.m_namedFilters.ContainsKey(filterString))
        {
            foundKey = filterString;
            return true;
        }
        string key = ConvertEuropeanCharacters(filterString);
        if (this.m_namedFilters.ContainsKey(key))
        {
            foundKey = key;
            return true;
        }
        string str2 = RemoveDiacritics(filterString);
        if (this.m_namedFilters.ContainsKey(str2))
        {
            foundKey = str2;
            return true;
        }
        str2 = RemoveDiacritics(key);
        if (this.m_namedFilters.ContainsKey(str2))
        {
            foundKey = str2;
            return true;
        }
        return false;
    }

    private bool CheckStringEqualsVariousForms(string filterString, string otherString)
    {
        if (filterString.Equals(otherString, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        string str = ConvertEuropeanCharacters(otherString);
        if (filterString.Equals(str, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        string str2 = RemoveDiacritics(otherString);
        if (filterString.Equals(str2, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        str2 = RemoveDiacritics(str);
        if (filterString.Equals(str2, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        str = ConvertEuropeanCharacters(filterString);
        if (otherString.Equals(str, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        str2 = RemoveDiacritics(filterString);
        if (otherString.Equals(str2, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        str2 = RemoveDiacritics(str);
        return otherString.Equals(str2, StringComparison.InvariantCultureIgnoreCase);
    }

    public void ClearFilter()
    {
        this.m_stringTokens.Clear();
        this.m_multiwordedStringTokens = null;
    }

    public static string ConvertEuropeanCharacters(string input)
    {
        int length = input.Length;
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            string str;
            if (s_europeanConversionTable.TryGetValue(input[i], out str))
            {
                builder.Append(str);
            }
            else
            {
                builder.Append(input[i]);
            }
        }
        return builder.ToString();
    }

    private bool DoesMatchAllSearchTerms(List<string> searchTerms, EntityDef entityDef, CollectionCardStack.ArtStack artStack)
    {
        string stringTag = entityDef.GetStringTag(GAME_TAG.CARDNAME);
        string str2 = entityDef.GetStringTag(GAME_TAG.CARDTEXT_INHAND);
        string str3 = entityDef.GetStringTag(GAME_TAG.ARTISTNAME);
        string[] array = (str3 != null) ? str3.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
        string input = (stringTag != null) ? stringTag.ToLower() : null;
        string str5 = (str2 != null) ? str2.ToLower() : null;
        string str = (stringTag != null) ? ConvertEuropeanCharacters(input) : null;
        string str7 = (str2 != null) ? ConvertEuropeanCharacters(str5) : null;
        string str8 = (stringTag != null) ? RemoveDiacritics(input) : null;
        string str9 = (str2 != null) ? RemoveDiacritics(str5) : null;
        if (<>f__am$cacheA == null)
        {
            <>f__am$cacheA = n => n.ToLower();
        }
        array.ForEachReassign<string>(<>f__am$cacheA);
        if (<>f__am$cacheB == null)
        {
            <>f__am$cacheB = n => ConvertEuropeanCharacters(n);
        }
        IEnumerable<string> enumerable = Enumerable.Select<string, string>(array, <>f__am$cacheB);
        if (<>f__am$cacheC == null)
        {
            <>f__am$cacheC = n => RemoveDiacritics(n).Replace("'", string.Empty).Replace("\"", string.Empty);
        }
        IEnumerable<string> enumerable2 = Enumerable.Select<string, string>(array, <>f__am$cacheC);
        for (int i = 0; i < searchTerms.Count; i++)
        {
            string str10;
            <DoesMatchAllSearchTerms>c__AnonStorey2C8 storeyc = new <DoesMatchAllSearchTerms>c__AnonStorey2C8 {
                filterString = searchTerms[i]
            };
            if (this.CheckNamedFilterContains(storeyc.filterString, out str10))
            {
                CollectionFilter<GAME_TAG> filter = this.m_namedFilters[str10];
                if (filter.DoesValueMatch(entityDef.GetTag(filter.GetKey())))
                {
                    continue;
                }
                if ((((GAME_TAG) filter.GetKey()) == GAME_TAG.RARITY) && this.CheckStringEqualsVariousForms(str10, GameStrings.GetRarityText(TAG_RARITY.COMMON)))
                {
                    string filterString = GameStrings.GetRarityText(TAG_RARITY.FREE).ToLower();
                    if (this.CheckNamedFilterContains(filterString, out str10))
                    {
                        filter = this.m_namedFilters[str10];
                        GAME_TAG key = filter.GetKey();
                        int tag = entityDef.GetTag(key);
                        if (filter.DoesValueMatch(tag))
                        {
                            continue;
                        }
                    }
                }
            }
            string filterEuropean = ConvertEuropeanCharacters(storeyc.filterString);
            string filterDiacritics = RemoveDiacritics(storeyc.filterString);
            if ((!this.CheckContains(input, storeyc.filterString, filterEuropean, filterDiacritics) && !this.CheckContains(str5, storeyc.filterString, filterEuropean, filterDiacritics)) && (((!this.CheckContains(str, storeyc.filterString, filterEuropean, filterDiacritics) && !this.CheckContains(str7, storeyc.filterString, filterEuropean, filterDiacritics)) && !this.CheckContains(str8, storeyc.filterString, filterEuropean, filterDiacritics)) && !this.CheckContains(str9, storeyc.filterString, filterEuropean, filterDiacritics)))
            {
                Func<string, bool> func = new Func<string, bool>(storeyc.<>m__96);
                if ((!Enumerable.Any<string>(array, func) && !Enumerable.Any<string>(enumerable, func)) && !Enumerable.Any<string>(enumerable2, func))
                {
                    TAG_CARD_SET cardSet = entityDef.GetCardSet();
                    if (cardSet != TAG_CARD_SET.INVALID)
                    {
                        string[] strArray2;
                        string cardSetName = GameStrings.GetCardSetName(cardSet);
                        if (cardSetName.Equals(storeyc.filterString, StringComparison.InvariantCultureIgnoreCase) || (ShorthandCardSetNames.TryGetValue(cardSet, out strArray2) && Enumerable.Any<string>(strArray2, new Func<string, bool>(storeyc.<>m__97))))
                        {
                            continue;
                        }
                        char[] separator = new char[] { ' ' };
                        string[] multiwordValue = cardSetName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        if ((multiwordValue.Length > 1) && this.CheckMultiwordValue(searchTerms.GetRange(i, searchTerms.Count - i), multiwordValue))
                        {
                            i += multiwordValue.Length - 1;
                            continue;
                        }
                    }
                    if ((artStack != null) && (artStack.Count != artStack.NumSeen))
                    {
                        int num3 = !entityDef.IsElite() ? 2 : 1;
                        if ((artStack.NumSeen < num3) && (this.CheckContains(this.m_unreadString, storeyc.filterString, filterEuropean, filterDiacritics) || this.CheckContains(this.m_unreadLowerCaseString, storeyc.filterString, filterEuropean, filterDiacritics)))
                        {
                            continue;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    public bool DoesValueMatch(EntityDef entityDef, CollectionCardStack.ArtStack artStack)
    {
        if (this.m_stringTokens.Count == 0)
        {
            return false;
        }
        return (this.DoesMatchAllSearchTerms(this.m_stringTokens, entityDef, artStack) || ((this.m_multiwordedStringTokens != null) && this.DoesMatchAllSearchTerms(this.m_multiwordedStringTokens, entityDef, artStack)));
    }

    public bool IsEmpty()
    {
        return (this.m_stringTokens.Count == 0);
    }

    public static string RemoveDiacritics(string input)
    {
        string str = input.Normalize(NormalizationForm.FormD);
        int length = str.Length;
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(str[i]) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(str[i]);
            }
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public void SetTextFilterValue(string val)
    {
        this.ClearFilter();
        if (val != null)
        {
            char[] separator = new char[] { ' ' };
            foreach (string str in val.Split(separator))
            {
                if (str.Length != 0)
                {
                    string item = str.ToLower();
                    this.m_stringTokens.Add(item);
                }
            }
            if (this.m_stringTokens.Count > 0)
            {
                this.m_multiwordedStringTokens = this.m_stringTokens;
                if (<>f__am$cache9 == null)
                {
                    <>f__am$cache9 = k => k.IndexOf(' ') >= 0;
                }
                IEnumerator<string> enumerator = Enumerable.Where<string>(this.m_namedFilters.Keys, <>f__am$cache9).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        this.m_multiwordedStringTokens = this.CheckMultiwordValue(this.m_multiwordedStringTokens, current);
                    }
                }
                finally
                {
                    if (enumerator == null)
                    {
                    }
                    enumerator.Dispose();
                }
                if (this.m_multiwordedStringTokens == this.m_stringTokens)
                {
                    this.m_multiwordedStringTokens = null;
                }
            }
        }
    }

    public bool ShouldRunFilter()
    {
        return ((this.m_stringTokens != null) && (this.m_stringTokens.Count > 0));
    }

    private static Map<TAG_CARD_SET, string[]> ShorthandCardSetNames
    {
        get
        {
            if (s_cachedShorthandCardSetNames == null)
            {
                object[][] objArrayArray1 = new object[5][];
                objArrayArray1[0] = new object[] { TAG_CARD_SET.FP1, "GLOBAL_CARD_SET_NAXX_SEARCHABLE_SHORTHAND_NAMES" };
                objArrayArray1[1] = new object[] { TAG_CARD_SET.PE1, "GLOBAL_CARD_SET_GVG_SEARCHABLE_SHORTHAND_NAMES" };
                objArrayArray1[2] = new object[] { TAG_CARD_SET.BRM, "GLOBAL_CARD_SET_BRM_SEARCHABLE_SHORTHAND_NAMES" };
                objArrayArray1[3] = new object[] { TAG_CARD_SET.TGT, "GLOBAL_CARD_SET_TGT_SEARCHABLE_SHORTHAND_NAMES" };
                objArrayArray1[4] = new object[] { TAG_CARD_SET.LOE, "GLOBAL_CARD_SET_LOE_SEARCHABLE_SHORTHAND_NAMES" };
                object[][] objArray = objArrayArray1;
                foreach (object[] objArray2 in objArray)
                {
                    TAG_CARD_SET key = (TAG_CARD_SET) ((int) objArray2[0]);
                    string str = (string) objArray2[1];
                    char[] separator = new char[] { ' ' };
                    string[] collection = GameStrings.Get(str).Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    if (collection.Length > 0)
                    {
                        if (s_cachedShorthandCardSetNames == null)
                        {
                            s_cachedShorthandCardSetNames = new Map<TAG_CARD_SET, string[]>();
                        }
                        List<string> list = new List<string>(collection);
                        foreach (string str2 in collection)
                        {
                            string str3 = ConvertEuropeanCharacters(str2);
                            if (!str2.Equals(str3))
                            {
                                list.Add(str3);
                            }
                            string str4 = RemoveDiacritics(str2);
                            if (!str2.Equals(str4))
                            {
                                list.Add(str4);
                            }
                        }
                        string[] strArray3 = list.ToArray();
                        s_cachedShorthandCardSetNames.Add(key, strArray3);
                    }
                }
            }
            return s_cachedShorthandCardSetNames;
        }
    }

    [CompilerGenerated]
    private sealed class <DoesMatchAllSearchTerms>c__AnonStorey2C8
    {
        internal string filterString;

        internal bool <>m__96(string n)
        {
            return n.Equals(this.filterString);
        }

        internal bool <>m__97(string n)
        {
            return n.Equals(this.filterString, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    private delegate string FilterGetNameFunction<T>(T param);

    private delegate bool FilterHasNameFunction<T>(T param);
}

