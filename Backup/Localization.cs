using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

public class Localization
{
    public const Locale DEFAULT_LOCALE = Locale.enUS;
    public static readonly string DEFAULT_LOCALE_NAME = Locale.enUS.ToString();
    public static readonly Map<Locale, Locale[]> LOAD_ORDERS;
    public static readonly PlatformDependentValue<bool> LOCALE_FROM_OPTIONS;
    private CultureInfo m_cultureInfo;
    private List<string> m_foreignLocaleNames;
    private List<Locale> m_foreignLocales;
    private Locale m_locale;
    private static Localization s_instance;

    static Localization()
    {
        Map<Locale, Locale[]> map = new Map<Locale, Locale[]>();
        map.Add(Locale.enUS, new Locale[1]);
        Locale[] localeArray1 = new Locale[2];
        localeArray1[0] = Locale.enGB;
        map.Add(Locale.enGB, localeArray1);
        Locale[] localeArray2 = new Locale[] { Locale.frFR };
        map.Add(Locale.frFR, localeArray2);
        Locale[] localeArray3 = new Locale[] { Locale.deDE };
        map.Add(Locale.deDE, localeArray3);
        Locale[] localeArray4 = new Locale[] { Locale.koKR };
        map.Add(Locale.koKR, localeArray4);
        Locale[] localeArray5 = new Locale[] { Locale.esES };
        map.Add(Locale.esES, localeArray5);
        Locale[] localeArray6 = new Locale[] { Locale.esMX };
        map.Add(Locale.esMX, localeArray6);
        Locale[] localeArray7 = new Locale[] { Locale.ruRU };
        map.Add(Locale.ruRU, localeArray7);
        Locale[] localeArray8 = new Locale[] { Locale.zhTW };
        map.Add(Locale.zhTW, localeArray8);
        Locale[] localeArray9 = new Locale[] { Locale.zhCN };
        map.Add(Locale.zhCN, localeArray9);
        Locale[] localeArray10 = new Locale[] { Locale.itIT };
        map.Add(Locale.itIT, localeArray10);
        Locale[] localeArray11 = new Locale[] { Locale.ptBR };
        map.Add(Locale.ptBR, localeArray11);
        Locale[] localeArray12 = new Locale[] { Locale.plPL };
        map.Add(Locale.plPL, localeArray12);
        Locale[] localeArray13 = new Locale[] { Locale.jaJP };
        map.Add(Locale.jaJP, localeArray13);
        LOAD_ORDERS = map;
        s_instance = new Localization();
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            iOS = true,
            Android = true,
            PC = false,
            Mac = false
        };
        LOCALE_FROM_OPTIONS = value2;
    }

    public static string ConvertLocaleToDotNet(Locale locale)
    {
        return ConvertLocaleToDotNet(locale.ToString());
    }

    public static string ConvertLocaleToDotNet(string localeName)
    {
        string str = localeName.Substring(0, 2);
        string str2 = localeName.Substring(2, 2).ToUpper();
        return string.Format("{0}-{1}", str, str2);
    }

    public static bool DoesLocaleUseDecimalPoint(Locale locale)
    {
        switch (locale)
        {
            case Locale.enUS:
            case Locale.enGB:
            case Locale.koKR:
            case Locale.esMX:
            case Locale.zhTW:
            case Locale.zhCN:
            case Locale.jaJP:
                return true;

            case Locale.frFR:
            case Locale.deDE:
            case Locale.esES:
            case Locale.ruRU:
            case Locale.itIT:
            case Locale.ptBR:
            case Locale.plPL:
                return false;
        }
        return true;
    }

    public static CultureInfo GetCultureInfo()
    {
        return s_instance.m_cultureInfo;
    }

    public static List<string> GetForeignLocaleNames()
    {
        List<string> foreignLocaleNames = s_instance.m_foreignLocaleNames;
        if (foreignLocaleNames == null)
        {
            foreignLocaleNames = new List<string>();
            foreach (string str in Enum.GetNames(typeof(Locale)))
            {
                if ((str != Locale.UNKNOWN.ToString()) && (str != DEFAULT_LOCALE_NAME))
                {
                    foreignLocaleNames.Add(str);
                }
            }
            s_instance.m_foreignLocaleNames = foreignLocaleNames;
        }
        return foreignLocaleNames;
    }

    public static List<Locale> GetForeignLocales()
    {
        List<Locale> foreignLocales = s_instance.m_foreignLocales;
        if (foreignLocales == null)
        {
            foreignLocales = new List<Locale>();
            IEnumerator enumerator = Enum.GetValues(typeof(Locale)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Locale current = (Locale) ((int) enumerator.Current);
                    switch (current)
                    {
                        case Locale.UNKNOWN:
                        case Locale.enUS:
                        {
                            continue;
                        }
                    }
                    foreignLocales.Add(current);
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
            s_instance.m_foreignLocales = foreignLocales;
        }
        return foreignLocales;
    }

    public static Locale[] GetLoadOrder(AssetFamily family)
    {
        return GetLoadOrder((family == AssetFamily.CardTexture) || (family == AssetFamily.CardPremium));
    }

    public static Locale[] GetLoadOrder(bool isCardTexture = false)
    {
        return GetLoadOrder(s_instance.m_locale, isCardTexture);
    }

    public static Locale[] GetLoadOrder(Locale locale, bool isCardTexture = false)
    {
        Locale[] array = LOAD_ORDERS[locale];
        if ((Network.IsRunning() && (BattleNet.GetAccountCountry() == "CHN")) && isCardTexture)
        {
            Array.Resize<Locale>(ref array, array.Length + 1);
            Array.Copy(array, 0, array, 1, array.Length - 1);
            array[0] = Locale.zhCN;
        }
        return array;
    }

    public static Locale GetLocale()
    {
        return s_instance.m_locale;
    }

    public static string GetLocaleName()
    {
        return s_instance.m_locale.ToString();
    }

    public static void Initialize()
    {
        Locale locale;
        Locale? nullable = null;
        if ((LOCALE_FROM_OPTIONS != null) && EnumUtils.TryGetEnum<Locale>(Options.Get().GetString(Option.LOCALE), out locale))
        {
            nullable = new Locale?(locale);
        }
        if (!nullable.HasValue)
        {
            Locale locale2;
            string launchOption = null;
            if (ApplicationMgr.IsPublic())
            {
                launchOption = BattleNet.GetLaunchOption("LOCALE");
            }
            if (string.IsNullOrEmpty(launchOption))
            {
                launchOption = Vars.Key("Localization.Locale").GetStr(DEFAULT_LOCALE_NAME);
            }
            if (ApplicationMgr.IsInternal())
            {
                string str = Vars.Key("Localization.OverrideBnetLocale").GetStr(string.Empty);
                if (!string.IsNullOrEmpty(str))
                {
                    launchOption = str;
                }
            }
            if (EnumUtils.TryGetEnum<Locale>(launchOption, out locale2))
            {
                nullable = new Locale?(locale2);
            }
            else
            {
                nullable = 0;
            }
        }
        SetLocale(nullable.Value);
    }

    public static bool IsForeignLocale(Locale locale)
    {
        return (locale != Locale.enUS);
    }

    public static bool IsForeignLocaleName(string localeName)
    {
        Locale locale;
        try
        {
            locale = EnumUtils.Parse<Locale>(localeName);
        }
        catch (Exception)
        {
            return false;
        }
        return IsForeignLocale(locale);
    }

    public static bool IsIMELocale()
    {
        return (((GetLocale() == Locale.zhCN) || (GetLocale() == Locale.zhTW)) || (GetLocale() == Locale.koKR));
    }

    public static bool IsValidLocaleName(string localeName)
    {
        return Enum.IsDefined(typeof(Locale), localeName);
    }

    public static bool IsValidLocaleName(string localeName, params Locale[] locales)
    {
        if ((locales != null) && (locales.Length != 0))
        {
            for (int i = 0; i < locales.Length; i++)
            {
                string str = locales[i].ToString();
                if (localeName == str)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static void SetLocale(Locale locale)
    {
        s_instance.SetPegLocale(locale);
    }

    public static bool SetLocaleName(string localeName)
    {
        if (!IsValidLocaleName(localeName))
        {
            return false;
        }
        s_instance.SetPegLocaleName(localeName);
        return true;
    }

    private void SetPegLocale(Locale locale)
    {
        string localeName = locale.ToString();
        this.SetPegLocaleName(localeName);
    }

    private void SetPegLocaleName(string localeName)
    {
        this.m_locale = EnumUtils.Parse<Locale>(localeName);
        string name = ConvertLocaleToDotNet(this.m_locale);
        this.m_cultureInfo = CultureInfo.CreateSpecificCulture(name);
    }
}

