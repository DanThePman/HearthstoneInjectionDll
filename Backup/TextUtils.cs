using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public static class TextUtils
{
    private const int DEFAULT_STRING_BUILDER_CAPACITY_FUDGE = 10;

    public static string ComposeLineItemString(List<string> lines)
    {
        if (lines.Count == 0)
        {
            return string.Empty;
        }
        StringBuilder builder = new StringBuilder();
        foreach (string str in lines)
        {
            builder.AppendLine(str);
        }
        builder.Remove(builder.Length - 1, 1);
        return builder.ToString();
    }

    public static int CountCharInString(string s, char c)
    {
        int num = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == c)
            {
                num++;
            }
        }
        return num;
    }

    public static string DecodeWhitespaces(string text)
    {
        text = text.Replace(@"\n", "\n");
        text = text.Replace(@"\t", "\t");
        return text;
    }

    public static string EncodeWhitespaces(string text)
    {
        text = text.Replace("\n", @"\n");
        text = text.Replace("\t", @"\t");
        return text;
    }

    public static string FromHexString(string str)
    {
        if ((str.Length % 2) == 1)
        {
            throw new Exception("Hex string must have an even number of digits");
        }
        byte[] bytes = new byte[str.Length >> 1];
        for (int i = 0; i < (str.Length >> 1); i++)
        {
            bytes[i] = (byte) ((GetHexValue(str[i << 1]) << 4) + GetHexValue(str[(i << 1) + 1]));
        }
        return Encoding.UTF8.GetString(bytes);
    }

    private static int GetHexValue(char hex)
    {
        int num = hex;
        return (num - ((num >= 0x3a) ? 0x37 : 0x30));
    }

    public static bool HasBonusDamage(string powersText)
    {
        if (powersText != null)
        {
            for (int i = 0; i < powersText.Length; i++)
            {
                char ch = powersText[i];
                if (ch == '$')
                {
                    int num2 = ++i;
                    while (num2 < powersText.Length)
                    {
                        char ch2 = powersText[num2];
                        if ((ch2 < '0') || (ch2 > '9'))
                        {
                            break;
                        }
                        num2++;
                    }
                    if (num2 != i)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static string Slice<T>(this string str)
    {
        return str.Slice(0, str.Length);
    }

    public static string Slice(this string str, int start)
    {
        return str.Slice(start, str.Length);
    }

    public static string Slice(this string str, int start, int end)
    {
        int length = str.Length;
        if (start < 0)
        {
            start = length + start;
        }
        if (end < 0)
        {
            end = length + end;
        }
        int num2 = end - start;
        if (num2 <= 0)
        {
            return string.Empty;
        }
        int num3 = length - start;
        if (num2 > num3)
        {
            num2 = num3;
        }
        return str.Substring(start, num2);
    }

    public static string ToHexString(this byte[] bytes)
    {
        char[] chArray = new char[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            int num = bytes[i] >> 4;
            chArray[i * 2] = (char) ((0x37 + num) + (((num - 10) >> 0x1f) & -7));
            num = bytes[i] & 15;
            chArray[(i * 2) + 1] = (char) ((0x37 + num) + (((num - 10) >> 0x1f) & -7));
        }
        return new string(chArray);
    }

    public static string ToHexString(string str)
    {
        return Encoding.UTF8.GetBytes(str).ToHexString();
    }

    public static string TransformCardText(Entity entity, GAME_TAG textTag)
    {
        int damageBonus = entity.GetDamageBonus();
        int damageBonusDouble = entity.GetDamageBonusDouble();
        int healingDouble = entity.GetHealingDouble();
        return TransformCardText(damageBonus, damageBonusDouble, healingDouble, entity.GetStringTag(textTag));
    }

    public static string TransformCardText(EntityDef entityDef, GAME_TAG textTag)
    {
        return TransformCardText(0, 0, 0, entityDef.GetStringTag(textTag));
    }

    public static string TransformCardText(int damageBonus, int damageBonusDouble, int healingDouble, string powersText)
    {
        return GameStrings.ParseLanguageRules(TransformCardTextImpl(damageBonus, damageBonusDouble, healingDouble, powersText));
    }

    private static string TransformCardTextImpl(int damageBonus, int damageBonusDouble, int healingDouble, string powersText)
    {
        if (powersText == null)
        {
            return string.Empty;
        }
        if (powersText == string.Empty)
        {
            return string.Empty;
        }
        StringBuilder builder = new StringBuilder();
        bool flag = (damageBonus > 0) || (damageBonusDouble > 0);
        bool flag2 = healingDouble > 0;
        for (int i = 0; i < powersText.Length; i++)
        {
            char ch = powersText[i];
            if ((ch != '$') && (ch != '#'))
            {
                builder.Append(ch);
                continue;
            }
            int num2 = ++i;
            while (num2 < powersText.Length)
            {
                char ch2 = powersText[num2];
                if ((ch2 < '0') || (ch2 > '9'))
                {
                    break;
                }
                num2++;
            }
            if (num2 != i)
            {
                int num3 = Convert.ToInt32(powersText.Substring(i, num2 - i));
                switch (ch)
                {
                    case '$':
                        num3 += damageBonus;
                        for (int j = 0; j < damageBonusDouble; j++)
                        {
                            num3 *= 2;
                        }
                        break;

                    case '#':
                        for (int k = 0; k < healingDouble; k++)
                        {
                            num3 *= 2;
                        }
                        break;
                }
                if ((flag && (ch == '$')) || (flag2 && (ch == '#')))
                {
                    builder.Append('*');
                    builder.Append(num3);
                    builder.Append('*');
                }
                else
                {
                    builder.Append(num3);
                }
                i = num2 - 1;
            }
        }
        return builder.ToString();
    }
}

