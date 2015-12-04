using System;
using System.Collections;
using System.Text;
using UnityEngine;

public class AndroidDeviceSettings
{
    public string applicationStorageFolder;
    public float aspectRatio;
    public int densityDpi = 300;
    public float diagonalInches;
    public float heightInches;
    public float heightPixels;
    public bool isExtraLarge = true;
    public bool isOnPhoneWhitelist;
    public bool isOnTabletWhitelist;
    private static AndroidDeviceSettings s_instance;
    public int screenLayout;
    public const int SCREENLAYOUT_SIZE_XLARGE = 4;
    public float widthInches;
    public float widthPixels;
    public float xdpi;
    public float ydpi;

    private AndroidDeviceSettings()
    {
    }

    public static AndroidDeviceSettings Get()
    {
        if (s_instance == null)
        {
            s_instance = new AndroidDeviceSettings();
        }
        return s_instance;
    }

    public static bool IsCurrentTextureFormatSupported()
    {
        Map<string, TextureFormat> map2 = new Map<string, TextureFormat>();
        map2.Add(string.Empty, TextureFormat.ARGB32);
        map2.Add("etc1", TextureFormat.ETC_RGB4);
        map2.Add("etc2", TextureFormat.ETC2_RGBA8);
        map2.Add("astc", TextureFormat.ASTC_RGBA_10x10);
        map2.Add("atc", TextureFormat.ATC_RGBA8);
        map2.Add("dxt", TextureFormat.DXT5);
        map2.Add("pvrtc", TextureFormat.PVRTC_RGBA4);
        Map<string, TextureFormat> map = map2;
        bool flag = SystemInfo.SupportsTextureFormat(map[string.Empty]);
        Debug.Log("Checking whether texture format of build () is supported? " + flag);
        StringBuilder builder = new StringBuilder();
        builder.Append("All supported texture formats: ");
        IEnumerator enumerator = Enum.GetValues(typeof(TextureFormat)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                TextureFormat current = (TextureFormat) ((int) enumerator.Current);
                try
                {
                    if (SystemInfo.SupportsTextureFormat(current))
                    {
                        builder.Append(current + ", ");
                    }
                    continue;
                }
                catch (ArgumentException)
                {
                    continue;
                }
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
        Log.Graphics.Print(builder.ToString(), new object[0]);
        return flag;
    }

    public bool IsMusicPlaying()
    {
        return false;
    }
}

