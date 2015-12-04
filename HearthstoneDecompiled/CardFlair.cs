using System;

public class CardFlair
{
    public const TAG_PREMIUM DEFAULT_PREMIUM_TYPE = TAG_PREMIUM.NORMAL;
    private TAG_PREMIUM m_premium;

    public CardFlair(TAG_PREMIUM premium)
    {
        this.Premium = premium;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is CardFlair))
        {
            return false;
        }
        CardFlair flair = obj as CardFlair;
        return (this.Premium == flair.Premium);
    }

    public override int GetHashCode()
    {
        int num = 0x17;
        return ((num * 13) + this.Premium.GetHashCode());
    }

    public static TAG_PREMIUM GetPremiumType(CardFlair flair)
    {
        if (flair == null)
        {
            return TAG_PREMIUM.NORMAL;
        }
        return flair.Premium;
    }

    public override string ToString()
    {
        return string.Format("[CardFlair: Premium={0}]", this.Premium);
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
}

