using System;

public class CollectionDeckViolationDeckSize : CollectionDeckViolation
{
    private int m_totalCardCount;
    private int m_totalOwnedCardCount;

    public int TotalCardCount
    {
        get
        {
            return this.m_totalCardCount;
        }
        set
        {
            this.m_totalCardCount = value;
        }
    }

    public int TotalOwnedCardCount
    {
        get
        {
            return this.m_totalOwnedCardCount;
        }
        set
        {
            this.m_totalOwnedCardCount = value;
        }
    }
}

