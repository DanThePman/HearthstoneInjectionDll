using System;

internal static class HashPrimeNumbers
{
    private static readonly int[] primeTbl = new int[] { 
        11, 0x13, 0x25, 0x49, 0x6d, 0xa3, 0xfb, 0x16f, 0x22d, 0x337, 0x4d5, 0x745, 0xad9, 0x1051, 0x1867, 0x249b, 
        0x36e9, 0x5261, 0x7b8b, 0xb947, 0x115e7, 0x1a0e1, 0x27149, 0x3a9e5, 0x57ee3, 0x83e39, 0xc5d67, 0x128c09, 0x1bd1ff, 0x29bb13, 0x3e988b, 0x5de4c1, 
        0x8cd721, 0xd342ab
     };

    public static int CalcPrime(int x)
    {
        for (int i = (x & -2) - 1; i < 0x7fffffff; i += 2)
        {
            if (TestPrime(i))
            {
                return i;
            }
        }
        return x;
    }

    public static bool TestPrime(int x)
    {
        if ((x & 1) == 0)
        {
            return (x == 2);
        }
        int num = (int) Math.Sqrt((double) x);
        for (int i = 3; i < num; i += 2)
        {
            if ((x % i) == 0)
            {
                return false;
            }
        }
        return true;
    }

    public static int ToPrime(int x)
    {
        for (int i = 0; i < primeTbl.Length; i++)
        {
            if (x <= primeTbl[i])
            {
                return primeTbl[i];
            }
        }
        return CalcPrime(x);
    }
}

