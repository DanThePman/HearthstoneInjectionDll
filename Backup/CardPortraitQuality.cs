using System;

public class CardPortraitQuality
{
    public const int HIGH = 3;
    public bool LoadPremium;
    public const int LOW = 1;
    public const int MEDIUM = 2;
    public const int NOT_LOADED = 0;
    public int TextureQuality;

    public CardPortraitQuality(int quality, bool loadPremium)
    {
        this.TextureQuality = quality;
        this.LoadPremium = loadPremium;
    }

    public CardPortraitQuality(int quality, TAG_PREMIUM premiumType)
    {
        this.TextureQuality = quality;
        this.LoadPremium = premiumType == TAG_PREMIUM.GOLDEN;
    }

    public static CardPortraitQuality GetDefault()
    {
        return new CardPortraitQuality(3, true);
    }

    public static CardPortraitQuality GetFromDef(CardDef def)
    {
        if (def == null)
        {
            return GetDefault();
        }
        return def.GetPortraitQuality();
    }

    public static CardPortraitQuality GetUnloaded()
    {
        return new CardPortraitQuality(0, false);
    }

    public static bool operator >=(CardPortraitQuality left, CardPortraitQuality right)
    {
        if (left == null)
        {
            return false;
        }
        return ((right == null) || ((left.TextureQuality >= right.TextureQuality) && (left.LoadPremium || !right.LoadPremium)));
    }

    public static bool operator <=(CardPortraitQuality left, CardPortraitQuality right)
    {
        if (left == null)
        {
            return true;
        }
        if (right == null)
        {
            return false;
        }
        return ((left.TextureQuality <= right.TextureQuality) && (!left.LoadPremium || right.LoadPremium));
    }

    public override string ToString()
    {
        object[] objArray1 = new object[] { "(", this.TextureQuality, ", ", this.LoadPremium, ")" };
        return string.Concat(objArray1);
    }
}

