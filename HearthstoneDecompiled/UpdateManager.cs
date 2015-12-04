using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using WTCG.BI;

public class UpdateManager : MonoBehaviour
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map3E;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map3F;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map40;
    private Map<string, AssetBundle> m_assetBundles = new Map<string, AssetBundle>();
    private int m_assetsVersion;
    private InitCallback m_callback;
    private WWW m_currentDownload;
    private string m_error;
    private string[] m_remoteUri;
    private int m_remoteUriIndex;
    private DataOnlyPatching.Status m_reportStatus;
    private bool m_skipLoadingAssetBundle;
    private bool m_updateIsRequired;
    private UpdateProgress m_updateProgress;
    private static UpdateManager s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    private string CachedFileFolder()
    {
        return string.Format("Updates/{0}", this.m_assetsVersion);
    }

    private string CachedFilePath(string fileName)
    {
        return string.Format("{0}/{1}", this.CachedFileFolder(), fileName);
    }

    private bool CalculateAndCompareMD5(byte[] buf, string md5)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        string str = BitConverter.ToString(provider.ComputeHash(buf)).Replace("-", string.Empty);
        object[] args = new object[] { md5, str };
        Log.UpdateManager.Print("md5 expected {0} current {1}", args);
        return ((md5 == "0") || (str == md5));
    }

    private void CallCallback()
    {
        if (this.m_callback != null)
        {
            this.m_callback();
            this.m_callback = null;
        }
    }

    public bool ContainsFile(string fileName)
    {
        return this.m_assetBundles.ContainsKey(fileName);
    }

    private bool CurrentRegionIsCN()
    {
        return (BattleNet.GetCurrentRegion() == Network.BnetRegion.REGION_CN);
    }

    [DebuggerHidden]
    private IEnumerator DownloadToCache(UpdateItem item)
    {
        return new <DownloadToCache>c__Iterator265 { item = item, <$>item = item, <>f__this = this };
    }

    private string DownloadURI(string fileName)
    {
        string str = this.m_remoteUri[this.m_remoteUriIndex];
        str = str + "win/" + string.Format("{0}/", this.m_assetsVersion);
        return string.Format("{0}{1}", str, fileName);
    }

    public static UpdateManager Get()
    {
        return s_instance;
    }

    public AssetBundle GetAssetBundle(string fileName)
    {
        AssetBundle bundle = null;
        this.m_assetBundles.TryGetValue(fileName, out bundle);
        return bundle;
    }

    public string GetError()
    {
        return this.m_error;
    }

    public UpdateProgress GetProgressForCurrentFile()
    {
        this.m_updateProgress.downloadPercentage = 0f;
        if (this.m_currentDownload != null)
        {
            this.m_updateProgress.downloadPercentage = this.m_currentDownload.progress;
        }
        return this.m_updateProgress;
    }

    private int GetTryCount()
    {
        return this.m_remoteUri.Length;
    }

    [DebuggerHidden]
    public IEnumerator Initialize()
    {
        return new <Initialize>c__Iterator264 { <>f__this = this };
    }

    private void InitValues()
    {
        this.m_assetBundles.Clear();
        this.m_updateProgress = new UpdateProgress();
        this.m_currentDownload = null;
        this.m_error = null;
        this.m_updateIsRequired = false;
        this.m_skipLoadingAssetBundle = false;
        this.m_remoteUriIndex = Options.Get().GetInt(Option.PREFERRED_CDN_INDEX, 0) % this.m_remoteUri.Length;
        this.m_reportStatus = DataOnlyPatching.Status.SUCCEED_WITH_CACHE;
    }

    [DebuggerHidden]
    private IEnumerator LoadFromCache(UpdateItem item)
    {
        return new <LoadFromCache>c__Iterator266 { item = item, <$>item = item, <>f__this = this };
    }

    private void LoadManifest(byte[] buf, ref List<UpdateItem> list)
    {
        try
        {
            using (StreamReader reader = new StreamReader(new MemoryStream(buf)))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        char[] separator = new char[] { ';' };
                        string[] strArray = str.Split(separator);
                        if (strArray.Length != 3)
                        {
                            Log.UpdateManager.Print("Bad update manifest", new object[0]);
                            this.m_error = "Bad update manifest";
                            this.m_reportStatus = DataOnlyPatching.Status.FAILED_BAD_DATA;
                            return;
                        }
                        if (strArray[0].ToLower() == "flag")
                        {
                            string key = strArray[1].ToLower();
                            if (key != null)
                            {
                                int num;
                                if (<>f__switch$map40 == null)
                                {
                                    Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                                    dictionary.Add("required", 0);
                                    dictionary.Add("skip", 1);
                                    <>f__switch$map40 = dictionary;
                                }
                                if (<>f__switch$map40.TryGetValue(key, out num))
                                {
                                    if (num == 0)
                                    {
                                        this.m_updateIsRequired = strArray[2].ToLower() == "true";
                                        continue;
                                    }
                                    if (num == 1)
                                    {
                                        this.m_updateProgress.maxPatchingBarDisplayTime = Convert.ToInt32(strArray[2]);
                                        continue;
                                    }
                                }
                            }
                            object[] messageArgs = new object[] { strArray[1] };
                            Error.AddDevFatal("UpdateManager: {0} is unsupported", messageArgs);
                        }
                        else
                        {
                            UpdateItem item = new UpdateItem {
                                fileName = strArray[0],
                                size = Convert.ToInt32(strArray[1]),
                                md5 = strArray[2]
                            };
                            list.Add(item);
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            this.m_error = string.Format("LoadManifest Error: {0}", exception.Message);
            this.m_reportStatus = DataOnlyPatching.Status.FAILED_BAD_DATA;
        }
    }

    private void MoveToNextCDN()
    {
        this.m_remoteUriIndex = (this.m_remoteUriIndex + 1) % this.GetTryCount();
        Options.Get().SetInt(Option.PREFERRED_CDN_INDEX, this.m_remoteUriIndex);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void SetDownloadURI()
    {
        if (ApplicationMgr.IsPublic())
        {
            if (this.CurrentRegionIsCN())
            {
                this.m_remoteUri = new string[] { "http://client01.pdl.battlenet.com.cn/hs-pod/update/", "http://client02.pdl.battlenet.com.cn/hs-pod/update/", "http://client03.pdl.battlenet.com.cn/hs-pod/update/", "http://dist.blizzard.com/hs-pod/update/" };
            }
            else
            {
                this.m_remoteUri = new string[] { "http://dist.blizzard.com/hs-pod/update/", "http://llnw.blizzard.com/hs-pod/update/" };
            }
        }
        else
        {
            char[] separator = new char[] { ';' };
            this.m_remoteUri = Vars.Key("Application.CDN").GetStr("http://streaming-t5.corp.blizzard.net/hearthstone/update/").Split(separator);
        }
        foreach (string str in this.m_remoteUri)
        {
            object[] args = new object[] { str };
            Log.UpdateManager.Print("CDN: {0}", args);
        }
    }

    public void SetLastFailedDOPVersion(bool success)
    {
        object[] args = new object[] { !success ? this.m_assetsVersion : -1 };
        Log.UpdateManager.Print("Set LastFailedDOPVersion: {0}", args);
        Options.Get().SetInt(Option.LAST_FAILED_DOP_VERSION, !success ? this.m_assetsVersion : -1);
    }

    public void StartInitialize(int version, InitCallback callback)
    {
        this.m_callback = callback;
        this.m_assetsVersion = version;
        base.StartCoroutine(this.Initialize());
    }

    public bool StopWaitingForUpdate()
    {
        if (this.m_updateIsRequired)
        {
            UnityEngine.Debug.LogWarning("Cannot stop waiting for update, update is required!");
            return false;
        }
        this.m_skipLoadingAssetBundle = true;
        this.CallCallback();
        return true;
    }

    public bool UpdateIsRequired()
    {
        return this.m_updateIsRequired;
    }

    [CompilerGenerated]
    private sealed class <DownloadToCache>c__Iterator265 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal UpdateManager.UpdateItem <$>item;
        internal UpdateManager <>f__this;
        internal byte[] <buf>__2;
        internal Exception <ex>__4;
        internal WWW <file>__7;
        internal string <filePath>__1;
        internal FileStream <fileRead>__3;
        internal FileStream <fileWrite>__8;
        internal int <lastFailedDopVersion>__5;
        internal bool <needToDownload>__0;
        internal int <tryCount>__6;
        internal UpdateManager.UpdateItem item;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<needToDownload>__0 = true;
                    this.<filePath>__1 = this.<>f__this.CachedFilePath(this.item.fileName);
                    if (System.IO.File.Exists(this.<filePath>__1))
                    {
                        try
                        {
                            this.<buf>__2 = null;
                            this.<fileRead>__3 = new FileStream(this.<filePath>__1, FileMode.Open, FileAccess.Read);
                            object[] args = new object[] { this.<filePath>__1 };
                            Log.UpdateManager.Print("Check cached {0}", args);
                            this.<buf>__2 = new byte[this.<fileRead>__3.Length];
                            this.<fileRead>__3.Read(this.<buf>__2, 0, Convert.ToInt32(this.<fileRead>__3.Length));
                            this.<fileRead>__3.Close();
                            this.item.bytes = this.<buf>__2;
                            this.<needToDownload>__0 = !this.<>f__this.CalculateAndCompareMD5(this.<buf>__2, this.item.md5);
                        }
                        catch (Exception exception)
                        {
                            this.<ex>__4 = exception;
                            object[] objArray2 = new object[] { this.<ex>__4.Message };
                            Log.UpdateManager.Print("FileStream Error: {0}", objArray2);
                            this.<needToDownload>__0 = true;
                        }
                    }
                    if (this.<needToDownload>__0)
                    {
                        this.<>f__this.m_reportStatus = DataOnlyPatching.Status.SUCCEED;
                        this.<lastFailedDopVersion>__5 = Options.Get().GetInt(Option.LAST_FAILED_DOP_VERSION, -1);
                        object[] objArray3 = new object[] { this.<lastFailedDopVersion>__5 };
                        Log.UpdateManager.Print("LastFailedDOPVerion: {0}", objArray3);
                        if (this.<lastFailedDopVersion>__5 == this.<>f__this.m_assetsVersion)
                        {
                            this.<>f__this.StopWaitingForUpdate();
                        }
                        else
                        {
                            this.<>f__this.m_updateProgress.showProgressBar = true;
                        }
                        this.<tryCount>__6 = 0;
                        while (this.<tryCount>__6 < this.<>f__this.GetTryCount())
                        {
                            object[] objArray4 = new object[] { this.<>f__this.DownloadURI(this.item.fileName) };
                            Log.UpdateManager.Print("Download {0}", objArray4);
                            this.<file>__7 = new WWW(this.<>f__this.DownloadURI(this.item.fileName));
                            this.<>f__this.m_currentDownload = this.<file>__7;
                            this.$current = this.<file>__7;
                            this.$PC = 1;
                            return true;
                        Label_0288:
                            this.<>f__this.m_reportStatus = DataOnlyPatching.Status.SUCCEED;
                            object[] objArray5 = new object[] { this.item.fileName };
                            Log.UpdateManager.Print("Finished downloading {0}", objArray5);
                            Directory.CreateDirectory(this.<>f__this.CachedFileFolder());
                            this.<fileWrite>__8 = new FileStream(this.<filePath>__1, FileMode.Create, FileAccess.Write);
                            this.<fileWrite>__8.Write(this.<file>__7.bytes, 0, this.<file>__7.size);
                            this.<fileWrite>__8.Close();
                            this.item.bytes = this.<file>__7.bytes;
                            if (this.<>f__this.CalculateAndCompareMD5(this.<file>__7.bytes, this.item.md5))
                            {
                                break;
                            }
                            this.<>f__this.m_error = string.Format("MD5 mismatch {0} {1}", this.item.fileName, this.item.md5);
                            this.<>f__this.m_reportStatus = DataOnlyPatching.Status.FAILED_MD5_MISMATCH;
                        Label_0385:
                            if (!string.IsNullOrEmpty(this.<>f__this.m_error))
                            {
                                this.<>f__this.MoveToNextCDN();
                            }
                            this.<tryCount>__6++;
                        }
                    }
                    break;

                case 1:
                    this.<>f__this.m_error = this.<file>__7.error;
                    if (string.IsNullOrEmpty(this.<file>__7.error))
                    {
                        goto Label_0288;
                    }
                    this.<>f__this.m_reportStatus = DataOnlyPatching.Status.FAILED_DOWNLOADING;
                    goto Label_0385;

                default:
                    goto Label_03D0;
            }
            this.$PC = -1;
        Label_03D0:
            return false;
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

    [CompilerGenerated]
    private sealed class <Initialize>c__Iterator264 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<UpdateManager.UpdateItem>.Enumerator <$s_1578>__3;
        internal List<UpdateManager.UpdateItem>.Enumerator <$s_1579>__6;
        internal List<UpdateManager.UpdateItem>.Enumerator <$s_1580>__8;
        internal UpdateManager <>f__this;
        internal string <extension>__10;
        internal string <extension>__5;
        internal UpdateManager.UpdateItem <item>__4;
        internal UpdateManager.UpdateItem <item>__7;
        internal UpdateManager.UpdateItem <item>__9;
        internal List<UpdateManager.UpdateItem> <list>__2;
        internal WWW <manifestFile>__1;
        internal bool <success>__11;
        internal int <tryCount>__0;

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
                        this.<$s_1578>__3.Dispose();
                    }
                    break;

                case 3:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_1580>__8.Dispose();
                    }
                    break;
            }
        }

        public bool MoveNext()
        {
            string str;
            Dictionary<string, int> dictionary;
            int num2;
            uint num = (uint) this.$PC;
            this.$PC = -1;
            bool flag = false;
            switch (num)
            {
                case 0:
                    BIReport.Get().Report_DataOnlyPatching(DataOnlyPatching.Status.STARTED, Localization.GetLocale(), 0x2acc, this.<>f__this.m_assetsVersion);
                    this.<>f__this.SetDownloadURI();
                    this.<>f__this.InitValues();
                    this.<tryCount>__0 = 0;
                    goto Label_04DF;

                case 1:
                    this.<>f__this.m_error = this.<manifestFile>__1.error;
                    if (string.IsNullOrEmpty(this.<manifestFile>__1.error))
                    {
                        this.<>f__this.m_reportStatus = DataOnlyPatching.Status.SUCCEED_WITH_CACHE;
                        this.<list>__2 = new List<UpdateManager.UpdateItem>();
                        this.<>f__this.LoadManifest(this.<manifestFile>__1.bytes, ref this.<list>__2);
                        this.<>f__this.m_updateProgress.numFilesToDownload = this.<list>__2.Count;
                        if (!string.IsNullOrEmpty(this.<>f__this.m_error))
                        {
                            goto Label_04B1;
                        }
                        this.<$s_1578>__3 = this.<list>__2.GetEnumerator();
                        num = 0xfffffffd;
                        break;
                    }
                    this.<>f__this.m_reportStatus = DataOnlyPatching.Status.FAILED_DOWNLOADING;
                    goto Label_04B1;

                case 2:
                    break;

                case 3:
                    goto Label_03A3;

                default:
                    goto Label_05B4;
            }
            try
            {
                switch (num)
                {
                    case 2:
                        this.<>f__this.m_updateProgress.numFilesDownloaded++;
                        this.<>f__this.m_currentDownload = null;
                        goto Label_0278;
                }
                while (this.<$s_1578>__3.MoveNext())
                {
                    this.<item>__4 = this.<$s_1578>__3.Current;
                    this.<extension>__5 = System.IO.Path.GetExtension(this.<item>__4.fileName);
                    str = this.<extension>__5;
                    if (str != null)
                    {
                        if (UpdateManager.<>f__switch$map3E == null)
                        {
                            dictionary = new Dictionary<string, int>(1);
                            dictionary.Add(".unity3d", 0);
                            UpdateManager.<>f__switch$map3E = dictionary;
                        }
                        if (UpdateManager.<>f__switch$map3E.TryGetValue(str, out num2) && (num2 == 0))
                        {
                            this.$current = this.<>f__this.StartCoroutine(this.<>f__this.DownloadToCache(this.<item>__4));
                            this.$PC = 2;
                            flag = true;
                            goto Label_05B6;
                        }
                    }
                    object[] messageArgs = new object[] { this.<extension>__5 };
                    Error.AddDevFatal("UpdateManager: {0} is unsupported", messageArgs);
                Label_0278:
                    if (!string.IsNullOrEmpty(this.<>f__this.m_error))
                    {
                        goto Label_02BC;
                    }
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_1578>__3.Dispose();
            }
        Label_02BC:
            if (this.<>f__this.m_skipLoadingAssetBundle)
            {
                if (this.<>f__this.m_reportStatus == DataOnlyPatching.Status.SUCCEED)
                {
                    this.<>f__this.m_reportStatus = DataOnlyPatching.Status.SUCCEED_WITH_TIMEOVER;
                }
                this.<$s_1579>__6 = this.<list>__2.GetEnumerator();
                try
                {
                    while (this.<$s_1579>__6.MoveNext())
                    {
                        this.<item>__7 = this.<$s_1579>__6.Current;
                        this.<item>__7.bytes = null;
                    }
                }
                finally
                {
                    this.<$s_1579>__6.Dispose();
                }
                BIReport.Get().Report_DataOnlyPatching(this.<>f__this.m_reportStatus, Localization.GetLocale(), 0x2acc, this.<>f__this.m_assetsVersion);
                goto Label_05B4;
            }
            if (!string.IsNullOrEmpty(this.<>f__this.m_error))
            {
                goto Label_04F5;
            }
            AssetLoader.Get().UnloadUpdatableBundles();
            this.<$s_1580>__8 = this.<list>__2.GetEnumerator();
            num = 0xfffffffd;
        Label_03A3:
            try
            {
                switch (num)
                {
                    case 3:
                        goto Label_045E;
                }
                while (this.<$s_1580>__8.MoveNext())
                {
                    this.<item>__9 = this.<$s_1580>__8.Current;
                    this.<extension>__10 = System.IO.Path.GetExtension(this.<item>__9.fileName);
                    str = this.<extension>__10;
                    if (str != null)
                    {
                        if (UpdateManager.<>f__switch$map3F == null)
                        {
                            dictionary = new Dictionary<string, int>(1);
                            dictionary.Add(".unity3d", 0);
                            UpdateManager.<>f__switch$map3F = dictionary;
                        }
                        if (UpdateManager.<>f__switch$map3F.TryGetValue(str, out num2) && (num2 == 0))
                        {
                            this.$current = this.<>f__this.StartCoroutine(this.<>f__this.LoadFromCache(this.<item>__9));
                            this.$PC = 3;
                            flag = true;
                            goto Label_05B6;
                        }
                    }
                Label_045E:
                    if (!string.IsNullOrEmpty(this.<>f__this.m_error))
                    {
                        goto Label_04A2;
                    }
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_1580>__8.Dispose();
            }
        Label_04A2:
            AssetLoader.Get().ReloadUpdatableBundles();
            goto Label_04F5;
        Label_04B1:
            if (!string.IsNullOrEmpty(this.<>f__this.m_error))
            {
                this.<>f__this.MoveToNextCDN();
            }
            this.<tryCount>__0++;
        Label_04DF:
            if (this.<tryCount>__0 < this.<>f__this.GetTryCount())
            {
                object[] objArray1 = new object[] { this.<>f__this.DownloadURI("update.txt") };
                Log.UpdateManager.Print("Download {0}", objArray1);
                this.<manifestFile>__1 = new WWW(this.<>f__this.DownloadURI("update.txt"));
                this.$current = this.<manifestFile>__1;
                this.$PC = 1;
                goto Label_05B6;
            }
        Label_04F5:
            this.<success>__11 = string.IsNullOrEmpty(this.<>f__this.m_error);
            object[] args = new object[] { !this.<success>__11 ? this.<>f__this.m_assetsVersion : -1 };
            Log.UpdateManager.Print("Set LastFailedDOPVersion: {0}", args);
            Options.Get().SetInt(Option.LAST_FAILED_DOP_VERSION, !this.<success>__11 ? this.<>f__this.m_assetsVersion : -1);
            BIReport.Get().Report_DataOnlyPatching(this.<>f__this.m_reportStatus, Localization.GetLocale(), 0x2acc, this.<>f__this.m_assetsVersion);
            this.<>f__this.m_currentDownload = null;
            this.<>f__this.CallCallback();
            this.$PC = -1;
        Label_05B4:
            return false;
        Label_05B6:
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

    [CompilerGenerated]
    private sealed class <LoadFromCache>c__Iterator266 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal UpdateManager.UpdateItem <$>item;
        internal UpdateManager <>f__this;
        internal string <name>__3;
        internal AssetBundleCreateRequest <request>__0;
        internal TextAsset <xml>__2;
        internal string <xmlName>__1;
        internal UpdateManager.UpdateItem item;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    if (this.item.bytes != null)
                    {
                        this.<request>__0 = AssetBundle.CreateFromMemory(this.item.bytes);
                        this.$current = this.<request>__0;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.m_error = "No asset bundle in memory";
                    this.<>f__this.m_reportStatus = DataOnlyPatching.Status.FAILED_BAD_DATA;
                    break;

                case 1:
                    if (this.<request>__0 != null)
                    {
                        if (this.item.fileName.IndexOf(".xml.unity3d") != -1)
                        {
                            this.<xmlName>__1 = System.IO.Path.GetFileNameWithoutExtension(this.item.fileName);
                            this.<xml>__2 = (TextAsset) this.<request>__0.assetBundle.LoadAsset(this.<xmlName>__1);
                            object[] args = new object[] { this.<xmlName>__1 };
                            Log.UpdateManager.Print("Dbf.Reload - Use new {0}", args);
                            this.<name>__3 = System.IO.Path.GetFileNameWithoutExtension(this.<xmlName>__1);
                            GameDbf.Reload(this.<name>__3, this.<xml>__2.text);
                            this.<request>__0.assetBundle.Unload(true);
                        }
                        else
                        {
                            this.<>f__this.m_assetBundles.Add(this.item.fileName, this.<request>__0.assetBundle);
                        }
                        this.$PC = -1;
                        break;
                    }
                    this.<>f__this.m_error = "Bad Asset Bundle";
                    this.<>f__this.m_reportStatus = DataOnlyPatching.Status.FAILED_BAD_ASSETBUNDLE;
                    break;
            }
            return false;
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

    public delegate void InitCallback();

    private class UpdateItem
    {
        public byte[] bytes;
        public string fileName;
        public string md5;
        public int size;
    }

    public class UpdateProgress
    {
        public float downloadPercentage;
        public float maxPatchingBarDisplayTime = 20f;
        public int numFilesDownloaded;
        public int numFilesToDownload = 4;
        public bool showProgressBar;
    }
}

