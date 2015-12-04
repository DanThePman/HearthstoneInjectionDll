using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class ChangedCardMgr
{
    private Map<int, List<TrackedCard>> m_cards = new Map<int, List<TrackedCard>>();
    private int m_currentCardNerfIndex;
    private bool m_isInitialized;
    public const int MaxViewCount = 3;
    private static readonly ChangedCardMgr s_instance = new ChangedCardMgr();

    private void AddCard(TrackedCard card)
    {
        TrackedCard card2 = this.FindCard(card.Index, card.DbId);
        if (card2 != null)
        {
            card2.Count = card.Count;
        }
        else
        {
            List<TrackedCard> list;
            if (!this.m_cards.TryGetValue(card.Index, out list))
            {
                this.m_cards[card.Index] = new List<TrackedCard>();
            }
            this.m_cards[card.Index].Add(card);
        }
    }

    public bool AllowCard(int index, int dbId)
    {
        if (!this.m_isInitialized)
        {
            Debug.LogWarning("ChangedCardMgr.AllowCard called before Initialize!");
            return true;
        }
        TrackedCard card = this.FindCard(index, dbId);
        if (card == null)
        {
            card = new TrackedCard {
                Index = index,
                DbId = dbId,
                Count = 0
            };
            this.AddCard(card);
        }
        if (card.Count < 3)
        {
            card.Count++;
            this.Save();
            return true;
        }
        return false;
    }

    private TrackedCard FindCard(int index, int dbId)
    {
        List<TrackedCard> list;
        if (this.m_cards.TryGetValue(index, out list))
        {
            foreach (TrackedCard card in list)
            {
                if (card.DbId == dbId)
                {
                    return card;
                }
            }
        }
        return null;
    }

    public static ChangedCardMgr Get()
    {
        return s_instance;
    }

    public void Initialize()
    {
        if (!this.m_isInitialized)
        {
            this.Load();
        }
    }

    private void Load()
    {
        this.m_isInitialized = true;
        string str = Options.Get().GetString(Option.CHANGED_CARDS_DATA);
        if (!string.IsNullOrEmpty(str))
        {
            char[] separator = new char[] { '-' };
            foreach (string str2 in str.Split(separator))
            {
                if (!string.IsNullOrEmpty(str2))
                {
                    char[] chArray2 = new char[] { ',' };
                    string[] strArray3 = str2.Split(chArray2);
                    if (strArray3.Length == 3)
                    {
                        int num3;
                        int num4;
                        int num5;
                        for (int i = 0; i < 3; i++)
                        {
                            if (string.IsNullOrEmpty(strArray3[i]))
                            {
                            }
                        }
                        if ((GeneralUtils.TryParseInt(strArray3[0], out num3) && GeneralUtils.TryParseInt(strArray3[1], out num4)) && GeneralUtils.TryParseInt(strArray3[2], out num5))
                        {
                            TrackedCard card = new TrackedCard {
                                Index = num3,
                                DbId = num4,
                                Count = num5
                            };
                            this.AddCard(card);
                            if (card.Index > this.m_currentCardNerfIndex)
                            {
                                this.m_currentCardNerfIndex = card.Index;
                            }
                        }
                    }
                }
            }
            this.Save();
        }
    }

    private void Save()
    {
        foreach (List<TrackedCard> list in this.m_cards.Values)
        {
            foreach (TrackedCard card in list)
            {
                if (card.Index > this.m_currentCardNerfIndex)
                {
                    this.m_currentCardNerfIndex = card.Index;
                }
            }
        }
        StringBuilder builder = new StringBuilder();
        foreach (List<TrackedCard> list2 in this.m_cards.Values)
        {
            foreach (TrackedCard card2 in list2)
            {
                if (card2.Index == this.m_currentCardNerfIndex)
                {
                    builder.Append(card2.ToString());
                }
            }
        }
        Options.Get().SetString(Option.CHANGED_CARDS_DATA, builder.ToString());
    }

    private class TrackedCard
    {
        public override string ToString()
        {
            return string.Format("{0},{1},{2}-", this.Index, this.DbId, this.Count);
        }

        public int Count { get; set; }

        public int DbId { get; set; }

        public int Index { get; set; }
    }
}

