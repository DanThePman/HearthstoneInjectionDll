using PegasusShared;
using System;
using System.Runtime.CompilerServices;

public class CollectionDeckSlot
{
    private string m_cardId;
    private int m_count;
    private int m_index;
    private bool m_owned = true;
    private TAG_PREMIUM m_premium;
    public DelOnSlotEmptied OnSlotEmptied;

    public void CopyFrom(CollectionDeckSlot otherSlot)
    {
        this.Index = otherSlot.Index;
        this.Count = otherSlot.Count;
        this.CardID = otherSlot.CardID;
        this.Premium = otherSlot.Premium;
        this.Owned = otherSlot.Owned;
    }

    public int GetUID(DeckType deckType)
    {
        return ((deckType != DeckType.NORMAL_DECK) ? this.UID : this.ClientUID);
    }

    public override string ToString()
    {
        object[] args = new object[] { this.Index, this.Premium, this.Count, this.CardID };
        return string.Format("[CollectionDeckSlot: Index={0}, Premium={1}, Count={2}, CardID={3}]", args);
    }

    public string CardID
    {
        get
        {
            return this.m_cardId;
        }
        set
        {
            this.m_cardId = value;
        }
    }

    public int ClientUID
    {
        get
        {
            return GameUtils.ClientCardUID(this.CardID, this.Premium, this.Owned);
        }
    }

    public int Count
    {
        get
        {
            return this.m_count;
        }
        set
        {
            this.m_count = value;
            if ((this.m_count <= 0) && (this.OnSlotEmptied != null))
            {
                this.OnSlotEmptied(this);
            }
        }
    }

    public int Index
    {
        get
        {
            return this.m_index;
        }
        set
        {
            this.m_index = value;
        }
    }

    public bool Owned
    {
        get
        {
            return this.m_owned;
        }
        set
        {
            this.m_owned = value;
        }
    }

    public TAG_PREMIUM Premium
    {
        get
        {
            return this.m_premium;
        }
        set
        {
            this.m_premium = value;
        }
    }

    public int UID
    {
        get
        {
            return GameUtils.CardUID(this.CardID, this.Premium);
        }
    }

    public delegate void DelOnSlotEmptied(CollectionDeckSlot slot);
}

