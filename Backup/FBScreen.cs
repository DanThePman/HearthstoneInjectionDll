using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class FBScreen
{
    private static bool resizable;

    public static Layout.OptionCenterHorizontal CenterHorizontal()
    {
        return new Layout.OptionCenterHorizontal();
    }

    public static Layout.OptionCenterVertical CenterVertical()
    {
        return new Layout.OptionCenterVertical();
    }

    public static Layout.OptionLeft Left(float amount)
    {
        return new Layout.OptionLeft { Amount = amount };
    }

    public static void SetAspectRatio(int width, int height, params Layout[] layoutParams)
    {
        int num = (Screen.height / height) * width;
        Screen.SetResolution(num, Screen.height, Screen.fullScreen);
    }

    private static void SetLayout(IEnumerable<Layout> parameters)
    {
    }

    public static void SetResolution(int width, int height, bool fullscreen, int preferredRefreshRate = 0, params Layout[] layoutParams)
    {
        Screen.SetResolution(width, height, fullscreen, preferredRefreshRate);
    }

    public static void SetUnityPlayerEmbedCSS(string key, string value)
    {
    }

    public static Layout.OptionTop Top(float amount)
    {
        return new Layout.OptionTop { Amount = amount };
    }

    public static bool FullScreen
    {
        get
        {
            return Screen.fullScreen;
        }
        set
        {
            Screen.fullScreen = value;
        }
    }

    public static int Height
    {
        get
        {
            return Screen.height;
        }
    }

    public static bool Resizable
    {
        get
        {
            return resizable;
        }
    }

    public static int Width
    {
        get
        {
            return Screen.width;
        }
    }

    public class Layout
    {
        public class OptionCenterHorizontal : FBScreen.Layout
        {
        }

        public class OptionCenterVertical : FBScreen.Layout
        {
        }

        public class OptionLeft : FBScreen.Layout
        {
            public float Amount;
        }

        public class OptionTop : FBScreen.Layout
        {
            public float Amount;
        }
    }
}

