using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct ArenaRecord
{
    public int wins;
    public int losses;
    public bool isFinished;
    public static ArenaRecord Invalid;
    public static bool TryParse(string s, out ArenaRecord result)
    {
        result = Invalid;
        if (string.IsNullOrEmpty(s))
        {
            return false;
        }
        char[] separator = new char[] { ',' };
        string[] strArray = s.Split(separator);
        if (strArray.Length != 3)
        {
            return false;
        }
        int num = 0;
        int num2 = 0;
        if (!int.TryParse(strArray[0], out num) || !int.TryParse(strArray[1], out num2))
        {
            return false;
        }
        int num3 = 0;
        if (!int.TryParse(strArray[2], out num3))
        {
            return false;
        }
        result.wins = num;
        result.losses = num2;
        result.isFinished = num3 == 1;
        return true;
    }
}

