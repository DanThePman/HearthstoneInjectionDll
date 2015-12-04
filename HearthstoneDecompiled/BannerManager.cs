using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class BannerManager
{
    private bool m_bannerWasAcknowledged;
    private List<int> m_seenBanners = new List<int>();
    private static BannerManager s_instance;

    private BannerManager()
    {
    }

    private bool AcknowledgeBanner(int banner)
    {
        this.m_seenBanners.Add(banner);
        if ((banner != this.GetDisplayBannerId()) || this.m_bannerWasAcknowledged)
        {
            return false;
        }
        this.m_bannerWasAcknowledged = true;
        NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
        if (netObject != null)
        {
            netObject.DisplayBanner = banner;
            NetCache.Get().NetCacheChanged<NetCache.NetCacheProfileProgress>();
        }
        Network.AcknowledgeBanner(banner);
        return true;
    }

    public static BannerManager Get()
    {
        if (s_instance == null)
        {
            s_instance = new BannerManager();
        }
        return s_instance;
    }

    private int GetDisplayBannerId()
    {
        int @int = Vars.Key("Events.BannerIdOverride").GetInt(0);
        if (@int != 0)
        {
            return @int;
        }
        NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
        return ((netObject != null) ? netObject.DisplayBanner : 0);
    }

    public bool ShowABanner(DelOnCloseBanner callback = null)
    {
        int displayBannerId = this.GetDisplayBannerId();
        if (this.m_seenBanners.Contains(displayBannerId))
        {
            return false;
        }
        if (displayBannerId == 0)
        {
            return false;
        }
        DbfRecord record = GameDbf.Banner.GetRecord(displayBannerId);
        string assetPath = (record != null) ? record.GetAssetName("PREFAB") : null;
        if ((record == null) || (assetPath == null))
        {
            Debug.LogWarning(string.Format("No banner defined for bannerID={0}", displayBannerId));
            return false;
        }
        BannerPopup popup = GameUtils.LoadGameObjectWithComponent<BannerPopup>(assetPath);
        if (popup == null)
        {
            return false;
        }
        this.AcknowledgeBanner(displayBannerId);
        popup.Show(record.GetLocString("TEXT"), callback);
        return true;
    }

    public bool ShowCustomBanner(string bannerAsset, string bannerText, DelOnCloseBanner callback = null)
    {
        BannerPopup popup = GameUtils.LoadGameObjectWithComponent<BannerPopup>(bannerAsset);
        if (popup == null)
        {
            return false;
        }
        popup.Show(bannerText, callback);
        return true;
    }

    public delegate void DelOnCloseBanner();
}

