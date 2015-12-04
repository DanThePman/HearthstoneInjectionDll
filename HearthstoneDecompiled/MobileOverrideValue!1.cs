using System;
using UnityEngine;

[Serializable]
public class MobileOverrideValue<T>
{
    public ScreenCategory[] screens;
    public T[] values;

    public MobileOverrideValue()
    {
        this.screens = new ScreenCategory[] { ScreenCategory.PC };
        this.values = new T[] { default(T) };
    }

    public MobileOverrideValue(T defaultValue)
    {
        this.screens = new ScreenCategory[] { ScreenCategory.PC };
        this.values = new T[] { defaultValue };
    }

    public T[] GetValues()
    {
        return this.values;
    }

    public static implicit operator T(MobileOverrideValue<T> val)
    {
        if (val == null)
        {
            return default(T);
        }
        ScreenCategory[] screens = val.screens;
        T[] values = val.values;
        if (screens.Length < 1)
        {
            Debug.LogError("MobileOverrideValue should always have at least one value!");
            return default(T);
        }
        T local = values[0];
        ScreenCategory screen = PlatformSettings.Screen;
        for (int i = 1; i < screens.Length; i++)
        {
            if (screen == screens[i])
            {
                local = values[i];
            }
        }
        return local;
    }
}

