using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Downloader : MonoBehaviour
{
    private List<string> m_bundlesToDownload;
    private Map<string, AssetBundle> m_downloadableBundles;
    private float m_downloadStartTime;
    private bool m_hasShownInternalFailureAlert;
    private bool m_isDownloading;
    private static string m_remoteUri;
    private static string m_remoteUriCN;
    private static int m_remoteUriCNIndex = 1;
    private static Downloader s_instance;

    public bool AllLocalizedAudioBundlesDownloaded()
    {
        return (this.m_isDownloading || (this.BundlesMissing().Count == 0));
    }

    private void Awake()
    {
        s_instance = this;
        this.m_bundlesToDownload = new List<string>();
        this.m_downloadableBundles = new Map<string, AssetBundle>();
        if (ApplicationMgr.GetMobileEnvironment() == MobileEnv.PRODUCTION)
        {
            m_remoteUri = "http://dist.blizzard.com/hs-pod/dlc/";
            m_remoteUriCN = "http://client{0}.pdl.battlenet.com.cn/hs-pod/dlc/";
        }
        else
        {
            m_remoteUri = "http://streaming-t5.corp.blizzard.net/hearthstone/dlc/";
        }
        m_remoteUri = m_remoteUri + "win/";
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
    }

    private List<string> BundlesMissing()
    {
        List<string> list = this.BundlesToDownloadForLocale();
        List<string> list2 = new List<string>();
        foreach (string str in list)
        {
            if (!this.m_downloadableBundles.ContainsKey(str))
            {
                list2.Add(str);
            }
        }
        return list2;
    }

    private List<string> BundlesToDownloadForLocale()
    {
        List<string> list = new List<string>();
        foreach (Locale locale in Localization.GetLoadOrder(false))
        {
            switch (locale)
            {
                case Locale.enUS:
                case Locale.enGB:
                    break;

                default:
                    foreach (AssetFamilyBundleInfo info in AssetBundleInfo.FamilyInfo.Values)
                    {
                        for (int i = 0; i < info.NumberOfDownloadableLocaleBundles; i++)
                        {
                            string item = string.Format("{0}{1}_dlc_{2}.unity3d", info.BundleName, locale, i);
                            list.Add(item);
                        }
                    }
                    break;
            }
        }
        return list;
    }

    public static string BundleURL(string fileName)
    {
        string remoteUri = m_remoteUri;
        if ((MobileDeviceLocale.GetCurrentRegionId() == Network.BnetRegion.REGION_CN) && (m_remoteUriCNIndex != GetTryCount()))
        {
            remoteUri = string.Format(m_remoteUriCN, "0" + m_remoteUriCNIndex.ToString());
        }
        return string.Format("{0}{1}/{2}", remoteUri, DownloadManifest.Get().HashForBundle(fileName), fileName);
    }

    public void DeleteLocalizedBundles()
    {
        foreach (AssetBundle bundle in this.m_downloadableBundles.Values)
        {
            bundle.Unload(true);
        }
        Caching.CleanCache();
        this.m_downloadableBundles.Clear();
        UnityEngine.Debug.Log("Cleared cache of downloadable bundles.");
    }

    [DebuggerHidden]
    private IEnumerator DownloadAndCache(string fileName)
    {
        return new <DownloadAndCache>c__Iterator1A9 { fileName = fileName, <$>fileName = fileName, <>f__this = this };
    }

    public void DownloadLocalizedBundles()
    {
        this.DownloadLocalizedBundles(this.BundlesToDownloadForLocale());
    }

    private void DownloadLocalizedBundles(List<string> bundlesToDownload)
    {
        if (((bundlesToDownload != null) && (bundlesToDownload.Count > 0)) && !this.m_isDownloading)
        {
            this.m_isDownloading = true;
            this.m_downloadStartTime = UnityEngine.Time.realtimeSinceStartup;
            Log.Downloader.Print("Starting to load or download localized bundles at " + this.m_downloadStartTime, new object[0]);
            this.m_bundlesToDownload = bundlesToDownload;
            this.DownloadNextFile();
        }
    }

    private void DownloadNextFile()
    {
        if (this.m_bundlesToDownload.Count > 0)
        {
            string bundleName = this.m_bundlesToDownload[0];
            string fileName = DownloadManifest.Get().DownloadableBundleFileName(bundleName);
            if (fileName == null)
            {
                Error.AddDevFatal(string.Format("Downloader.DownloadNextFile() - Attempting to download bundle not listed in manifest.  No hash found for bundle {0}", bundleName), new object[0]);
                this.m_bundlesToDownload.RemoveAt(0);
                this.DownloadNextFile();
            }
            else
            {
                base.StartCoroutine(this.DownloadAndCache(fileName));
            }
        }
        else
        {
            Log.Downloader.Print("Finished downloading or loading all bundles - duration: " + (UnityEngine.Time.realtimeSinceStartup - this.m_downloadStartTime), new object[0]);
            AssetCache.ClearAllCachesFailedRequests();
            this.m_isDownloading = false;
        }
    }

    public static Downloader Get()
    {
        return s_instance;
    }

    public AssetBundle GetDownloadedBundle(string fileName)
    {
        AssetBundle bundle = null;
        this.m_downloadableBundles.TryGetValue(fileName, out bundle);
        if (bundle == null)
        {
            UnityEngine.Debug.Log(string.Format("Attempted to load bundle {0} but not available yet.", fileName));
        }
        return bundle;
    }

    public static int GetTryCount()
    {
        return ((MobileDeviceLocale.GetCurrentRegionId() != Network.BnetRegion.REGION_CN) ? 1 : 4);
    }

    public static string NextBundleURL(string fileName)
    {
        if (MobileDeviceLocale.GetCurrentRegionId() == Network.BnetRegion.REGION_CN)
        {
            m_remoteUriCNIndex = (m_remoteUriCNIndex != GetTryCount()) ? (m_remoteUriCNIndex + 1) : 1;
        }
        return BundleURL(fileName);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void Start()
    {
        if (AssetLoader.DOWNLOADABLE_LANGUAGE_PACKS != null)
        {
            this.DownloadLocalizedBundles();
        }
        else
        {
            Log.Downloader.Print("Not downloading language bundles (DOWNLOADABLE_LANGUAGE_PACKS=false)", new object[0]);
        }
    }

    private void WillReset()
    {
        base.StopAllCoroutines();
        this.m_isDownloading = false;
        this.m_hasShownInternalFailureAlert = false;
        foreach (AssetBundle bundle in this.m_downloadableBundles.Values)
        {
            if (bundle != null)
            {
                bundle.Unload(true);
            }
        }
        this.m_downloadableBundles.Clear();
        if (AssetLoader.DOWNLOADABLE_LANGUAGE_PACKS != null)
        {
            this.DownloadLocalizedBundles();
        }
    }

    [CompilerGenerated]
    private sealed class <DownloadAndCache>c__Iterator1A9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>fileName;
        internal Downloader <>f__this;
        internal int <assetVersion>__2;
        internal WWW <bundleRequest>__5;
        internal AlertPopup.PopupInfo <info>__6;
        internal float <startTime>__0;
        internal int <tryCount>__3;
        internal string <version>__1;
        internal string <webResource>__4;
        internal string fileName;

        [DebuggerHidden]
        public void Dispose()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 2:
                    try
                    {
                    }
                    finally
                    {
                        if (this.<bundleRequest>__5 != null)
                        {
                            this.<bundleRequest>__5.Dispose();
                        }
                    }
                    break;
            }
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            bool flag = false;
            switch (num)
            {
                case 0:
                case 1:
                {
                    while (!Caching.ready)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_038A;
                    }
                    this.<startTime>__0 = UnityEngine.Time.realtimeSinceStartup;
                    this.<version>__1 = "4.1".Replace(".", string.Empty);
                    this.<assetVersion>__2 = 2;
                    try
                    {
                        this.<assetVersion>__2 = Convert.ToInt32(this.<version>__1);
                    }
                    catch (Exception)
                    {
                        UnityEngine.Debug.LogWarning("Could not convert cosmeticVersion to an int!");
                    }
                    object[] args = new object[] { this.<assetVersion>__2 };
                    Log.Downloader.Print("Downloader asset version: {0}", args);
                    this.<tryCount>__3 = 1;
                    while (this.<tryCount>__3 <= Downloader.GetTryCount())
                    {
                        this.<webResource>__4 = Downloader.BundleURL(this.fileName);
                        UnityEngine.Debug.Log(string.Format("Loading or downloading file {0} from {1}", this.fileName, this.<webResource>__4));
                        this.<bundleRequest>__5 = WWW.LoadFromCacheOrDownload(this.<webResource>__4, this.<assetVersion>__2);
                        num = 0xfffffffd;
                    Label_0110:
                        try
                        {
                            switch (num)
                            {
                                case 2:
                                    if (this.<bundleRequest>__5.error == null)
                                    {
                                        break;
                                    }
                                    UnityEngine.Debug.LogError(string.Format("DownloadAndCache - Error when downloading url {0} - error: {1}", this.<webResource>__4, this.<bundleRequest>__5.error));
                                    if ((ApplicationMgr.IsInternal() && (DialogManager.Get() != null)) && (!this.<>f__this.m_hasShownInternalFailureAlert && (Localization.GetLocale() != Locale.jaJP)))
                                    {
                                        this.<info>__6 = new AlertPopup.PopupInfo();
                                        this.<info>__6.m_headerText = "Language Pack Download Failed";
                                        this.<info>__6.m_text = string.Format("Failed to download bundle {0} with error {1}.  Some sounds may not play.  Try again by relaunching the game.", this.<webResource>__4, this.<bundleRequest>__5.error);
                                        this.<info>__6.m_showAlertIcon = true;
                                        this.<info>__6.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
                                        DialogManager.Get().ShowPopup(this.<info>__6);
                                        this.<>f__this.m_hasShownInternalFailureAlert = true;
                                    }
                                    this.<webResource>__4 = Downloader.NextBundleURL(this.fileName);
                                    goto Label_0316;

                                default:
                                    Log.Downloader.Print(string.Format("Started downloading and caching url {0} - duration: {1}", this.<webResource>__4, UnityEngine.Time.realtimeSinceStartup - this.<startTime>__0), new object[0]);
                                    this.$current = this.<bundleRequest>__5;
                                    this.$PC = 2;
                                    flag = true;
                                    goto Label_038A;
                            }
                            Log.Downloader.Print(string.Format("Finished downloading and caching url {0} - duration: {1}", this.<webResource>__4, UnityEngine.Time.realtimeSinceStartup - this.<startTime>__0), new object[0]);
                            this.<startTime>__0 = UnityEngine.Time.realtimeSinceStartup;
                            this.<>f__this.m_downloadableBundles.Add(this.fileName, this.<bundleRequest>__5.assetBundle);
                            Log.Downloader.Print(string.Format("Finished loading asset bundle {0} - duration: {1}", this.<webResource>__4, UnityEngine.Time.realtimeSinceStartup - this.<startTime>__0), new object[0]);
                            this.<startTime>__0 = UnityEngine.Time.realtimeSinceStartup;
                            break;
                        }
                        finally
                        {
                            if (!flag)
                            {
                            }
                            if (this.<bundleRequest>__5 != null)
                            {
                                this.<bundleRequest>__5.Dispose();
                            }
                        }
                    Label_0316:
                        this.<tryCount>__3++;
                    }
                    break;
                }
                case 2:
                    goto Label_0110;

                default:
                    goto Label_0388;
            }
            Log.Downloader.Print(string.Format("Finished closing WWW for bundle {0} - duration: {1}", this.fileName, UnityEngine.Time.realtimeSinceStartup - this.<startTime>__0), new object[0]);
            this.<>f__this.m_bundlesToDownload.RemoveAt(0);
            this.<>f__this.DownloadNextFile();
            this.$PC = -1;
        Label_0388:
            return false;
        Label_038A:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

