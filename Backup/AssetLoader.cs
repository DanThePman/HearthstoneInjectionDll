using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
    public static readonly PlatformDependentValue<bool> DOWNLOADABLE_LANGUAGE_PACKS;
    private static HashSet<string> fileListInExtraBundles_;
    public const string fileListInExtraBundlesTxtName_ = "manifest-filelist-extra.csv";
    private AssetBundle[,] m_downloadedFamilyBundles;
    private AssetBundle[,] m_downloadedLocalizedFamilyBundles;
    private AssetBundle[,] m_familyBundles;
    private AssetBundle[,] m_localizedFamilyBundles;
    private bool m_ready;
    private AssetBundle[] m_sharedBundle = new AssetBundle[4];
    private List<GameObject> m_waitingOnObjects = new List<GameObject>();
    private string m_workingDir;
    private static string[] s_cardSetDirectories;
    private static Map<AssetFamily, Map<string, List<FileInfo>>> s_fileInfos;
    private static AssetLoader s_instance;
    private readonly Vector3 SPAWN_POS_CAMERA_OFFSET = new Vector3(0f, 0f, -5000f);

    static AssetLoader()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            iOS = true,
            Android = true,
            PC = false,
            Mac = false
        };
        DOWNLOADABLE_LANGUAGE_PACKS = value2;
        fileListInExtraBundles_ = new HashSet<string>();
    }

    private bool AssetFromDownloadablePack(Asset asset)
    {
        if (((DOWNLOADABLE_LANGUAGE_PACKS == null) || (Localization.GetLocale() == Locale.enUS)) || (Localization.GetLocale() == Locale.enGB))
        {
            return false;
        }
        string filePath = FileUtils.StripLocaleFromPath(string.Format("Final/{0}", asset.GetPath()));
        bool flag = DownloadManifest.Get().ContainsFile(filePath);
        if (flag)
        {
            Log.Downloader.Print(string.Format("File {0} is downloadable according to manifest.", filePath), new object[0]);
        }
        return flag;
    }

    private static int AvailableExtraAssetBundlesCount()
    {
        return (!UseFileListInExtraBundles() ? 9 : 1);
    }

    private void Awake()
    {
        s_instance = this;
        this.m_workingDir = Directory.GetCurrentDirectory().Replace(@"\", "/");
        AssetCache.Initialize();
        int length = Enum.GetNames(typeof(AssetFamily)).Length;
        int numberOfBundles = 0;
        int numberOfLocaleBundles = 0;
        foreach (AssetFamilyBundleInfo info in AssetBundleInfo.FamilyInfo.Values)
        {
            if (info.NumberOfBundles > numberOfBundles)
            {
                numberOfBundles = info.NumberOfBundles;
            }
            if (info.NumberOfLocaleBundles > numberOfLocaleBundles)
            {
                numberOfLocaleBundles = info.NumberOfLocaleBundles;
            }
        }
        this.m_familyBundles = new AssetBundle[length, numberOfBundles + AvailableExtraAssetBundlesCount()];
        this.m_localizedFamilyBundles = new AssetBundle[length, numberOfLocaleBundles + AvailableExtraAssetBundlesCount()];
        numberOfLocaleBundles = 0;
        foreach (AssetFamilyBundleInfo info2 in AssetBundleInfo.FamilyInfo.Values)
        {
            if (info2.NumberOfDownloadableLocaleBundles > numberOfLocaleBundles)
            {
                numberOfLocaleBundles = info2.NumberOfDownloadableLocaleBundles;
            }
        }
        this.m_downloadedLocalizedFamilyBundles = new AssetBundle[length, numberOfLocaleBundles];
    }

    private void CacheAsset(Asset asset, UnityEngine.Object assetObject)
    {
        long timestamp = TimeUtils.BinaryStamp();
        AssetCache.CachedAsset item = new AssetCache.CachedAsset();
        item.SetAsset(asset);
        item.SetAssetObject(assetObject);
        item.SetCreatedTimestamp(timestamp);
        item.SetLastRequestTimestamp(timestamp);
        item.SetPersistent(asset.IsPersistent());
        AssetCache.Add(asset, item);
    }

    [DebuggerHidden]
    private IEnumerator CreateCachedAsset<RequestType, AssetType>(RequestType request, Asset asset, Action<AssetCache.CachedAsset> successCallback, UnityEngine.Object fallback) where RequestType: AssetCache.CacheRequest
    {
        return new <CreateCachedAsset>c__Iterator230<RequestType, AssetType> { asset = asset, request = request, fallback = fallback, successCallback = successCallback, <$>asset = asset, <$>request = request, <$>fallback = fallback, <$>successCallback = successCallback, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator CreateCachedAsset_FromBundle<RequestType>(RequestType request, Asset asset, UnityEngine.Object fallback) where RequestType: AssetCache.CacheRequest
    {
        return new <CreateCachedAsset_FromBundle>c__Iterator231<RequestType> { asset = asset, fallback = fallback, request = request, <$>asset = asset, <$>fallback = fallback, <$>request = request, <>f__this = this };
    }

    private WWW CreateLocalFile(string absPath)
    {
        return new WWW(string.Format("file://{0}", absPath));
    }

    private string CreateLocalFilePath(string relPath)
    {
        return string.Format("{0}/{1}", this.m_workingDir, relPath);
    }

    [DebuggerHidden]
    private IEnumerator DownloadFile(string path, FileCallback callback, object callbackData)
    {
        return new <DownloadFile>c__Iterator22E { path = path, callback = callback, callbackData = callbackData, <$>path = path, <$>callback = callback, <$>callbackData = callbackData, <>f__this = this };
    }

    private static bool FileIsInFileListInExtraBundles(string fileName)
    {
        if (!UseFileListInExtraBundles())
        {
            return false;
        }
        return fileListInExtraBundles_.Contains(fileName);
    }

    public static AssetLoader Get()
    {
        return s_instance;
    }

    private AssetBundle GetBundleForAsset(Asset asset, out string finalAssetName)
    {
        foreach (Locale locale in Localization.GetLoadOrder(asset.GetFamily()))
        {
            finalAssetName = string.Format("Final/{0}", asset.GetPath(locale));
            AssetBundle bundleForAsset = this.GetBundleForAsset(finalAssetName, asset);
            if (bundleForAsset != null)
            {
                asset.SetLocale(locale);
                return bundleForAsset;
            }
        }
        finalAssetName = string.Format("Final/{0}", asset.GetPath());
        return this.GetBundleForAsset(finalAssetName, asset);
    }

    private AssetBundle GetBundleForAsset(string assetName, Asset asset)
    {
        bool downloaded = this.AssetFromDownloadablePack(asset);
        AssetBundle bundle = this.GetBundleForAsset(assetName, asset.GetFamily(), downloaded);
        if (bundle != null)
        {
            return bundle;
        }
        return null;
    }

    private AssetBundle GetBundleForAsset(string assetName, AssetFamily family, bool downloaded)
    {
        AssetBundle bundle;
        if (!downloaded)
        {
            bundle = this.GetBundleForFamily(family, assetName, null);
            if ((bundle != null) && bundle.Contains(assetName))
            {
                return bundle;
            }
        }
        Locale[] loadOrder = Localization.GetLoadOrder(family);
        for (int i = 0; i < loadOrder.Length; i++)
        {
            bundle = !downloaded ? this.GetBundleForFamily(family, assetName, new Locale?(loadOrder[i])) : this.GetDownloadedBundleForFamily(family, assetName, loadOrder[i]);
            if ((bundle != null) && bundle.Contains(assetName))
            {
                return bundle;
            }
        }
        return null;
    }

    private AssetBundle GetBundleForFamily(AssetFamily family, string assetName, Locale? locale = new Locale?())
    {
        return this.GetBundleForFamily(family, -1, locale, assetName);
    }

    private AssetBundle GetBundleForFamily(AssetFamily family, int bundleNum, Locale? locale = new Locale?(), string assetName = null)
    {
        AssetFamilyBundleInfo info;
        string str;
        int num = (int) family;
        try
        {
            info = AssetBundleInfo.FamilyInfo[family];
        }
        catch (IndexOutOfRangeException)
        {
            object[] args = new object[] { family, assetName };
            UnityEngine.Debug.LogErrorFormat("GetBundleForFamily: Failed to find bundle family: \"{0}\" for asset: \"{1}\".", args);
            return null;
        }
        int y = locale.HasValue ? info.NumberOfLocaleBundles : info.NumberOfBundles;
        if (!string.IsNullOrEmpty(assetName))
        {
            if (FileIsInFileListInExtraBundles(assetName))
            {
                bundleNum = y;
            }
            else
            {
                bundleNum = (y > 1) ? GeneralUtils.UnsignedMod(assetName.GetHashCode(), y) : 0;
            }
        }
        if (locale.HasValue)
        {
            string fileName = string.Format("{0}{1}{2}.unity3d", info.BundleName, EnumUtils.GetString<Locale>(locale.Value), bundleNum);
            if (UpdateManager.Get().ContainsFile(fileName))
            {
                object[] objArray2 = new object[] { fileName };
                Log.UpdateManager.Print("AssetLoader.GetBundleForFamily - Use UpdateManager.GetAssetBundle {0}", objArray2);
                this.m_familyBundles[num, bundleNum] = UpdateManager.Get().GetAssetBundle(fileName);
            }
            if (this.m_localizedFamilyBundles[num, bundleNum] == null)
            {
                str = this.CreateLocalFilePath(string.Format("Data/{0}{1}", AssetBundleInfo.BundlePathPlatformModifier(), fileName));
                this.m_localizedFamilyBundles[num, bundleNum] = !System.IO.File.Exists(str) ? null : AssetBundle.CreateFromFile(str);
            }
            return this.m_localizedFamilyBundles[num, bundleNum];
        }
        if (this.m_familyBundles[num, bundleNum] == null)
        {
            string str3 = string.Format("{0}{1}.unity3d", info.BundleName, bundleNum);
            if (UpdateManager.Get().ContainsFile(str3))
            {
                object[] objArray3 = new object[] { str3 };
                Log.UpdateManager.Print("AssetLoader.GetBundleForFamily - Use UpdateManager.GetAssetBundle {0}", objArray3);
                this.m_familyBundles[num, bundleNum] = UpdateManager.Get().GetAssetBundle(str3);
            }
            if (this.m_familyBundles[num, bundleNum] == null)
            {
                str = this.CreateLocalFilePath(string.Format("Data/{0}{1}", AssetBundleInfo.BundlePathPlatformModifier(), str3));
                this.m_familyBundles[num, bundleNum] = !System.IO.File.Exists(str) ? null : AssetBundle.CreateFromFile(str);
            }
        }
        return this.m_familyBundles[num, bundleNum];
    }

    private AssetBundle GetDownloadedBundleForFamily(AssetFamily family, string assetName, Locale locale)
    {
        int hashCode = assetName.GetHashCode();
        int num2 = (int) family;
        int numberOfDownloadableLocaleBundles = AssetBundleInfo.FamilyInfo[family].NumberOfDownloadableLocaleBundles;
        int num4 = (numberOfDownloadableLocaleBundles > 1) ? GeneralUtils.UnsignedMod(hashCode, numberOfDownloadableLocaleBundles) : 0;
        if (this.m_downloadedLocalizedFamilyBundles[num2, num4] == null)
        {
            string fileName = DownloadManifest.Get().DownloadableBundleFileName(string.Format("{0}{1}_dlc_{2}.unity3d", AssetBundleInfo.FamilyInfo[family].BundleName, EnumUtils.GetString<Locale>(locale), num4));
            if (fileName == null)
            {
                return null;
            }
            object[] args = new object[] { fileName };
            Log.Asset.Print("Attempting to load localized, downloaded AssetBundle {0}", args);
            this.m_downloadedLocalizedFamilyBundles[num2, num4] = Downloader.Get().GetDownloadedBundle(fileName);
        }
        return this.m_downloadedLocalizedFamilyBundles[num2, num4];
    }

    [DebuggerHidden]
    private IEnumerator Init()
    {
        return new <Init>c__Iterator22D { <>f__this = this };
    }

    private static void InitFileListInExtraBundles()
    {
        fileListInExtraBundles_.Clear();
        string path = string.Format("{0}/{1}", FileUtils.PersistentDataPath, "manifest-filelist-extra.csv");
        if (!System.IO.File.Exists(path))
        {
            path = FileUtils.GetAssetPath("manifest-filelist-extra.csv");
        }
        object[] args = new object[] { path };
        Log.UpdateManager.Print("InitFileListInExtraBundles - {0}", args);
        if (System.IO.File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string str2;
                while ((str2 = reader.ReadLine()) != null)
                {
                    fileListInExtraBundles_.Add(str2);
                }
            }
            Log.UpdateManager.Print("InitFileListInExtraBundles - Success", new object[0]);
        }
    }

    public bool IsReady()
    {
        return this.m_ready;
    }

    public bool IsWaitingOnObject(GameObject go)
    {
        return this.m_waitingOnObjects.Contains(go);
    }

    public GameObject LoadActor(string name, bool usePrefabPosition = false, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadCardTexture() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.Actor, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadActor(string cardName, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(cardName, AssetFamily.Actor, false, callback, callbackData, persistent, null);
    }

    public bool LoadActor(string cardName, bool usePrefabPosition, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(cardName, AssetFamily.Actor, usePrefabPosition, callback, callbackData, persistent, null);
    }

    public Map<string, EntityDef> LoadBatchCardXmls(List<string> cardIds, out int errors)
    {
        errors = 0;
        Map<string, EntityDef> map = new Map<string, EntityDef>();
        UnityEngine.Object obj2 = null;
        string name = Localization.GetLocale().ToString();
        Asset asset = new Asset(name, AssetFamily.CardXML, true);
        AssetBundle bundleForAsset = this.GetBundleForAsset(name, asset);
        if (bundleForAsset == null)
        {
            Error.AddDevFatal("AssetLoader.LoadCardXml: Could not load CardXml bundle", new object[0]);
            return null;
        }
        obj2 = bundleForAsset.LoadAsset(name);
        if (obj2 == null)
        {
            Error.AddDevFatal("AssetLoader.LoadCardXml: Could not load CardXml for locale " + Localization.GetLocale(), new object[0]);
            return null;
        }
        TextAsset asset2 = (TextAsset) obj2;
        using (StringReader reader = new StringReader(asset2.text))
        {
            using (XmlReader reader2 = XmlReader.Create(reader))
            {
                while (true)
                {
                    EntityDef def = new EntityDef();
                    if (!def.LoadDataFromCardXml(reader2))
                    {
                        return map;
                    }
                    string cardId = def.GetCardId();
                    if (map.ContainsKey(cardId))
                    {
                        UnityEngine.Debug.LogError(string.Format("AssetLoader.LoadBatchedCardXmls: Loaded duplicate card id {0}", cardId));
                    }
                    else
                    {
                        map.Add(cardId, def);
                    }
                }
            }
        }
    }

    public GameObject LoadBoard(string name, bool usePrefabPosition = true, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadBoard() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.Board, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadBoard(string boardName, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(boardName, AssetFamily.Board, true, callback, callbackData, persistent, null);
    }

    private void LoadCachedObject(Asset asset, ObjectCallback callback, object callbackData)
    {
        <LoadCachedObject>c__AnonStorey35E storeye = new <LoadCachedObject>c__AnonStorey35E {
            asset = asset
        };
        if (storeye.asset.GetName() == null)
        {
            if (callback != null)
            {
                callback(null, null, callbackData);
            }
        }
        else
        {
            long timestamp = TimeUtils.BinaryStamp();
            AssetCache.CachedAsset asset2 = AssetCache.Find(storeye.asset);
            if (asset2 != null)
            {
                asset2.SetLastRequestTimestamp(timestamp);
                if (callback != null)
                {
                    UnityEngine.Object assetObject = asset2.GetAssetObject();
                    callback(storeye.asset.GetName(), assetObject, callbackData);
                }
            }
            else
            {
                AssetCache.ObjectCacheRequest request = AssetCache.GetRequest<AssetCache.ObjectCacheRequest>(storeye.asset);
                if (request != null)
                {
                    request.SetLastRequestTimestamp(timestamp);
                    if (request.DidFail())
                    {
                        if (callback != null)
                        {
                            callback(storeye.asset.GetName(), null, callbackData);
                        }
                    }
                    else
                    {
                        request.AddRequester(callback, callbackData);
                    }
                }
                else
                {
                    storeye.request = new AssetCache.ObjectCacheRequest();
                    storeye.request.SetPersistent(storeye.asset.IsPersistent());
                    storeye.request.SetCreatedTimestamp(timestamp);
                    storeye.request.SetLastRequestTimestamp(timestamp);
                    storeye.request.AddRequester(callback, callbackData);
                    AssetCache.AddRequest(storeye.asset, storeye.request);
                    Action<AssetCache.CachedAsset> successCallback = new Action<AssetCache.CachedAsset>(storeye.<>m__1F3);
                    base.StartCoroutine(this.CreateCachedAsset<AssetCache.ObjectCacheRequest, UnityEngine.Object>(storeye.request, storeye.asset, successCallback, null));
                }
            }
        }
    }

    private void LoadCachedPrefab(Asset asset, bool usePrefabPosition, GameObjectCallback callback, object callbackData, UnityEngine.Object fallback)
    {
        <LoadCachedPrefab>c__AnonStorey35F storeyf = new <LoadCachedPrefab>c__AnonStorey35F {
            asset = asset,
            usePrefabPosition = usePrefabPosition,
            <>f__this = this
        };
        if (storeyf.asset.GetName() == null)
        {
            if (callback != null)
            {
                callback(null, null, callbackData);
            }
        }
        else
        {
            long timestamp = TimeUtils.BinaryStamp();
            AssetCache.CachedAsset asset2 = AssetCache.Find(storeyf.asset);
            if (asset2 != null)
            {
                asset2.SetLastRequestTimestamp(timestamp);
                UnityEngine.Object assetObject = asset2.GetAssetObject();
                base.StartCoroutine(this.WaitThenCallGameObjectCallback(storeyf.asset, assetObject, storeyf.usePrefabPosition, callback, callbackData));
            }
            else
            {
                AssetCache.PrefabCacheRequest request = AssetCache.GetRequest<AssetCache.PrefabCacheRequest>(storeyf.asset);
                if (request != null)
                {
                    request.SetLastRequestTimestamp(timestamp);
                    if (request.DidFail())
                    {
                        if (callback != null)
                        {
                            callback(storeyf.asset.GetName(), null, callbackData);
                        }
                    }
                    else
                    {
                        request.AddRequester(callback, callbackData);
                    }
                }
                else
                {
                    storeyf.request = new AssetCache.PrefabCacheRequest();
                    storeyf.request.SetPersistent(storeyf.asset.IsPersistent());
                    storeyf.request.SetCreatedTimestamp(timestamp);
                    storeyf.request.SetLastRequestTimestamp(timestamp);
                    storeyf.request.AddRequester(callback, callbackData);
                    AssetCache.AddRequest(storeyf.asset, storeyf.request);
                    Action<AssetCache.CachedAsset> successCallback = new Action<AssetCache.CachedAsset>(storeyf.<>m__1F4);
                    base.StartCoroutine(this.CreateCachedAsset<AssetCache.PrefabCacheRequest, GameObject>(storeyf.request, storeyf.asset, successCallback, fallback));
                }
            }
        }
    }

    public GameObject LoadCardBack(string name, bool usePrefabPosition = true, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadCardBack() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.CardBack, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadCardBack(string name, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(name, AssetFamily.CardBack, true, callback, callbackData, persistent, null);
    }

    public GameObject LoadCardPrefab(string name, bool usePrefabPosition = true, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadCardTexture() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.CardPrefab, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadCardPrefab(string cardName, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(cardName, AssetFamily.CardPrefab, true, callback, callbackData, persistent, null);
    }

    public Texture LoadCardTexture(string name, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadCardTexture() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.CardTexture, persistent);
        UnityEngine.Object obj2 = this.LoadObjectImmediately(asset);
        if ((obj2 == null) || !(obj2 is Texture))
        {
            object[] messageArgs = new object[] { name };
            Error.AddDevFatal("AssetLoader.LoadCardTexture() - Expected a Texture and loaded null or something else for asset {0}.", messageArgs);
            return null;
        }
        return (Texture) obj2;
    }

    public bool LoadFile(string path, FileCallback callback, object callbackData)
    {
        if (string.IsNullOrEmpty(path))
        {
            UnityEngine.Debug.LogWarning("AssetLoader.LoadFile() - path was null or empty");
            return false;
        }
        base.StartCoroutine(this.DownloadFile(path, callback, callbackData));
        return true;
    }

    public GameObject LoadFontDef(string name, bool usePrefabPosition = true, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadFontDef() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.FontDef, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadFontDef(string name, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(name, AssetFamily.FontDef, true, callback, callbackData, persistent, null);
    }

    public GameObject LoadGameObject(string name, bool usePrefabPosition = true, bool persistent = false)
    {
        Asset asset = new Asset(name, AssetFamily.GameObject, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadGameObject(string name, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(name, AssetFamily.GameObject, true, callback, callbackData, persistent, null);
    }

    private GameObject LoadGameObjectImmediately(Asset asset, bool usePrefabPosition)
    {
        UnityEngine.Object obj2 = this.LoadObjectImmediately(asset);
        if ((obj2 == null) || !(obj2 is GameObject))
        {
            UnityEngine.Debug.LogError(string.Format("AssetLoader.LoadGameObjectImmediately() - Expected a prefab and loaded null or something else for asset {0} from family .", asset.GetName(), asset.GetFamily()));
            return null;
        }
        GameObject original = (GameObject) obj2;
        if (usePrefabPosition)
        {
            return UnityEngine.Object.Instantiate<GameObject>(original);
        }
        return (GameObject) UnityEngine.Object.Instantiate(original, this.NewGameObjectSpawnPosition(original), original.transform.rotation);
    }

    public MovieTexture LoadMovie(string name, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadMovie() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.Movie, persistent);
        UnityEngine.Object obj2 = this.LoadObjectImmediately(asset);
        if ((obj2 == null) || !(obj2 is MovieTexture))
        {
            object[] messageArgs = new object[] { name };
            Error.AddDevFatal("AssetLoader.LoadMovie() - Expected a MovieTexture and loaded null or something else for asset {0}.", messageArgs);
            return null;
        }
        return (MovieTexture) obj2;
    }

    public bool LoadMovie(string name, ObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadObject(name, AssetFamily.Movie, callback, callbackData, persistent);
    }

    private bool LoadObject(string assetName, AssetFamily family, ObjectCallback callback, object callbackData, bool persistent = false)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            Log.Asset.Print("AssetLoader.LoadObject() - name was null or empty", new object[0]);
            return false;
        }
        Asset asset = new Asset(assetName, family, persistent);
        this.LoadCachedObject(asset, callback, callbackData);
        return true;
    }

    private UnityEngine.Object LoadObjectImmediately(Asset asset)
    {
        UnityEngine.Object assetObject = null;
        string str;
        AssetCache.CachedAsset asset2 = AssetCache.Find(asset);
        if (asset2 != null)
        {
            asset2.SetLastRequestTimestamp(TimeUtils.BinaryStamp());
            return asset2.GetAssetObject();
        }
        AssetBundle bundleForAsset = this.GetBundleForAsset(asset, out str);
        if (bundleForAsset != null)
        {
            assetObject = bundleForAsset.LoadAsset(str);
            if (assetObject != null)
            {
                this.CacheAsset(asset, assetObject);
            }
        }
        return assetObject;
    }

    private bool LoadPrefab(string assetName, AssetFamily family, bool usePrefabPosition, GameObjectCallback callback, object callbackData, bool persistent = false, UnityEngine.Object fallback = null)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            UnityEngine.Debug.LogWarning("AssetLoader.LoadPrefab() - name was null or empty");
            return false;
        }
        Asset asset = new Asset(assetName, family, persistent);
        this.LoadCachedPrefab(asset, usePrefabPosition, callback, callbackData, fallback);
        return true;
    }

    public Material LoadPremiumMaterial(string name, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadPremiumMaterial() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.CardPremium, persistent);
        UnityEngine.Object obj2 = this.LoadObjectImmediately(asset);
        if ((obj2 == null) || !(obj2 is Material))
        {
            object[] messageArgs = new object[] { name };
            Error.AddDevFatal("AssetLoader.LoadPremiumMaterial() - Expected a Material and loaded null or something else for asset {0}.", messageArgs);
            return null;
        }
        return (Material) obj2;
    }

    public GameObject LoadSound(string name, bool usePrefabPosition = true, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadSound() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.Sound, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadSound(string soundName, GameObjectCallback callback, object callbackData = null, bool persistent = false, GameObject fallback = null)
    {
        return this.LoadPrefab(soundName, AssetFamily.Sound, true, callback, callbackData, persistent, fallback);
    }

    public GameObject LoadSpell(string name, bool usePrefabPosition = true, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadSpell() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.Spell, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadSpell(string name, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(name, AssetFamily.Spell, true, callback, callbackData, persistent, null);
    }

    public Texture LoadTexture(string name, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadTexture() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.Texture, persistent);
        UnityEngine.Object obj2 = this.LoadObjectImmediately(asset);
        if ((obj2 == null) || !(obj2 is Texture))
        {
            object[] messageArgs = new object[] { name };
            Error.AddDevFatal("AssetLoader.LoadTexture() - Expected a Texture and loaded null or something else for asset {0}.", messageArgs);
            return null;
        }
        return (Texture) obj2;
    }

    public bool LoadTexture(string textureName, ObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadObject(textureName, AssetFamily.Texture, callback, callbackData, persistent);
    }

    public GameObject LoadUIScreen(string name, bool usePrefabPosition = true, bool persistent = false)
    {
        if (name == null)
        {
            Error.AddDevFatal("AssetLoader.LoadUIScreen() - An asset request was made but no file name was given.", new object[0]);
            return null;
        }
        Asset asset = new Asset(name, AssetFamily.Screen, persistent);
        return this.LoadGameObjectImmediately(asset, usePrefabPosition);
    }

    public bool LoadUIScreen(string screenName, GameObjectCallback callback, object callbackData = null, bool persistent = false)
    {
        return this.LoadPrefab(screenName, AssetFamily.Screen, true, callback, callbackData, false, null);
    }

    private static void LogMissingAsset(AssetFamily family, string assetname)
    {
        Log.MissingAssets.Print(LogLevel.Error, string.Format("[{0}] {1}", family, assetname), new object[0]);
    }

    private Vector3 NewGameObjectSpawnPosition(UnityEngine.Object prefab)
    {
        if (Camera.main == null)
        {
            return Vector3.zero;
        }
        return (Camera.main.transform.position + this.SPAWN_POS_CAMERA_OFFSET);
    }

    private void OnApplicationQuit()
    {
        AssetCache.ForceClearAllCaches();
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    private void PreloadBundles()
    {
        if (AssetBundleInfo.UseSharedDependencyBundle)
        {
            for (int i = 0; i < 4; i++)
            {
                string path = this.CreateLocalFilePath(string.Format("Data/{0}{1}{2}.unity3d", AssetBundleInfo.BundlePathPlatformModifier(), "shared", i));
                this.m_sharedBundle[i] = AssetBundle.CreateFromFile(path);
            }
        }
        foreach (KeyValuePair<AssetFamily, AssetFamilyBundleInfo> pair in AssetBundleInfo.FamilyInfo)
        {
            AssetFamily key = pair.Key;
            AssetFamilyBundleInfo info = pair.Value;
            for (int j = 0; j < (info.NumberOfBundles + AvailableExtraAssetBundlesCount()); j++)
            {
                Locale? locale = null;
                this.GetBundleForFamily(key, j, locale, null);
            }
            for (int k = 0; k < info.NumberOfLocaleBundles; k++)
            {
                Locale[] loadOrder = Localization.GetLoadOrder(false);
                for (int m = 0; m < loadOrder.Length; m++)
                {
                    this.GetBundleForFamily(key, k, new Locale?(loadOrder[m]), null);
                }
            }
        }
        if (DOWNLOADABLE_LANGUAGE_PACKS != null)
        {
            DownloadManifest.Get();
        }
        InitFileListInExtraBundles();
    }

    public void ReloadUpdatableBundles()
    {
        foreach (KeyValuePair<AssetFamily, AssetFamilyBundleInfo> pair in AssetBundleInfo.FamilyInfo)
        {
            AssetFamily key = pair.Key;
            AssetFamilyBundleInfo info = pair.Value;
            if (info.Updatable)
            {
                for (int i = 0; i < info.NumberOfBundles; i++)
                {
                    Locale? locale = null;
                    this.GetBundleForFamily(key, i, locale, null);
                }
                for (int j = 0; j < info.NumberOfLocaleBundles; j++)
                {
                    Locale[] loadOrder = Localization.GetLoadOrder(false);
                    for (int k = 0; k < loadOrder.Length; k++)
                    {
                        this.GetBundleForFamily(key, j, new Locale?(loadOrder[k]), null);
                    }
                }
            }
        }
    }

    public void SetReady(bool ready)
    {
        this.m_ready = ready;
    }

    private void Start()
    {
        base.StartCoroutine(this.Init());
    }

    public void UnloadUpdatableBundles()
    {
        foreach (KeyValuePair<AssetFamily, AssetFamilyBundleInfo> pair in AssetBundleInfo.FamilyInfo)
        {
            AssetFamily key = pair.Key;
            AssetFamilyBundleInfo info = pair.Value;
            if (info.Updatable)
            {
                for (int i = 0; i < info.NumberOfBundles; i++)
                {
                    Locale? locale = null;
                    AssetBundle bundle = this.GetBundleForFamily(key, i, locale, null);
                    if (bundle != null)
                    {
                        bundle.Unload(true);
                    }
                }
                for (int j = 0; j < info.NumberOfLocaleBundles; j++)
                {
                    Locale[] loadOrder = Localization.GetLoadOrder(false);
                    for (int k = 0; k < loadOrder.Length; k++)
                    {
                        AssetBundle bundle2 = this.GetBundleForFamily(key, j, new Locale?(loadOrder[k]), null);
                        if (bundle2 != null)
                        {
                            bundle2.Unload(true);
                        }
                    }
                }
            }
        }
    }

    private static bool UseFileListInExtraBundles()
    {
        return (fileListInExtraBundles_.Count != 0);
    }

    [DebuggerHidden]
    private static IEnumerator WaitForDownload(WWW file)
    {
        return new <WaitForDownload>c__Iterator22F { file = file, <$>file = file };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenCallGameObjectCallback(Asset asset, UnityEngine.Object prefab, bool usePrefabPosition, GameObjectCallback callback, object callbackData)
    {
        return new <WaitThenCallGameObjectCallback>c__Iterator232 { asset = asset, prefab = prefab, callback = callback, callbackData = callbackData, usePrefabPosition = usePrefabPosition, <$>asset = asset, <$>prefab = prefab, <$>callback = callback, <$>callbackData = callbackData, <$>usePrefabPosition = usePrefabPosition, <>f__this = this };
    }

    private void WillReset()
    {
        AssetBundle[,] localizedFamilyBundles = this.m_localizedFamilyBundles;
        int length = localizedFamilyBundles.GetLength(0);
        int num4 = localizedFamilyBundles.GetLength(1);
        for (int i = 0; i < length; i++)
        {
            for (int k = 0; k < num4; k++)
            {
                AssetBundle bundle = localizedFamilyBundles[i, k];
                if (bundle != null)
                {
                    bundle.Unload(true);
                }
            }
        }
        this.m_localizedFamilyBundles = new AssetBundle[this.m_localizedFamilyBundles.GetLength(0), this.m_localizedFamilyBundles.GetLength(1)];
        AssetBundle[,] downloadedLocalizedFamilyBundles = this.m_downloadedLocalizedFamilyBundles;
        int num6 = downloadedLocalizedFamilyBundles.GetLength(0);
        int num8 = downloadedLocalizedFamilyBundles.GetLength(1);
        for (int j = 0; j < num6; j++)
        {
            for (int m = 0; m < num8; m++)
            {
                AssetBundle bundle2 = downloadedLocalizedFamilyBundles[j, m];
                if (bundle2 != null)
                {
                    bundle2.Unload(true);
                }
            }
        }
        this.m_downloadedLocalizedFamilyBundles = new AssetBundle[this.m_downloadedLocalizedFamilyBundles.GetLength(0), this.m_downloadedLocalizedFamilyBundles.GetLength(1)];
        this.PreloadBundles();
    }

    [CompilerGenerated]
    private sealed class <CreateCachedAsset_FromBundle>c__Iterator231<RequestType> : IDisposable, IEnumerator, IEnumerator<object> where RequestType: AssetCache.CacheRequest
    {
        internal object $current;
        internal int $PC;
        internal Asset <$>asset;
        internal UnityEngine.Object <$>fallback;
        internal RequestType <$>request;
        internal AssetLoader <>f__this;
        internal AssetBundle <bundle>__1;
        internal string <finalAssetName>__0;
        internal UnityEngine.Object <result>__3;
        internal string <userErrorMessage>__2;
        internal Asset asset;
        internal UnityEngine.Object fallback;
        internal RequestType request;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            this.$PC = -1;
            if (this.$PC == 0)
            {
                this.<bundle>__1 = this.<>f__this.GetBundleForAsset(this.asset, out this.<finalAssetName>__0);
                if (this.<bundle>__1 == null)
                {
                    if (!this.<>f__this.AssetFromDownloadablePack(this.asset))
                    {
                        if (this.fallback != null)
                        {
                            AssetLoader.LogMissingAsset(this.asset.GetFamily(), this.<finalAssetName>__0);
                            UnityEngine.Debug.LogError(string.Concat(new object[] { "Asset ", this.<finalAssetName>__0, " in ", this.asset.GetFamily(), "failed to load. Using fallback." }));
                            AssetCache.RemoveRequest(this.asset);
                            this.<>f__this.CacheAsset(this.asset, this.fallback);
                            this.request.OnLoadSucceeded();
                        }
                        else
                        {
                            this.request.OnLoadFailed(this.asset.GetName());
                            UnityEngine.Debug.LogError("Fatal Error: Asset not found in family or backup bundles: " + this.<finalAssetName>__0);
                            this.<userErrorMessage>__2 = string.Format("Failed to load bundle for {0}. Should be in bundle {1}.", this.<finalAssetName>__0, AssetBundleInfo.FamilyInfo[this.asset.GetFamily()].BundleName);
                            Error.AddDevFatal(this.<userErrorMessage>__2, new object[0]);
                        }
                    }
                    else
                    {
                        this.request.OnLoadFailed(this.asset.GetName());
                        UnityEngine.Debug.LogError("Downloadable asset failed to load : " + this.<finalAssetName>__0);
                    }
                }
                else
                {
                    this.<result>__3 = this.<bundle>__1.LoadAsset(this.<finalAssetName>__0);
                    if (this.<result>__3 == null)
                    {
                        AssetLoader.LogMissingAsset(this.asset.GetFamily(), this.<finalAssetName>__0);
                    }
                    AssetCache.RemoveRequest(this.asset);
                    this.<>f__this.CacheAsset(this.asset, this.<result>__3);
                    this.request.OnLoadSucceeded();
                }
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

    [CompilerGenerated]
    private sealed class <CreateCachedAsset>c__Iterator230<RequestType, AssetType> : IDisposable, IEnumerator, IEnumerator<object> where RequestType: AssetCache.CacheRequest
    {
        internal object $current;
        internal int $PC;
        internal Asset <$>asset;
        internal UnityEngine.Object <$>fallback;
        internal RequestType <$>request;
        internal Action<AssetCache.CachedAsset> <$>successCallback;
        internal AssetLoader <>f__this;
        internal AssetCache.CachedAsset <cachedAsset>__0;
        internal Asset asset;
        internal UnityEngine.Object fallback;
        internal RequestType request;
        internal Action<AssetCache.CachedAsset> successCallback;

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
                    AssetCache.StartLoading(this.asset.GetName());
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.CreateCachedAsset_FromBundle<RequestType>(this.request, this.asset, this.fallback));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<cachedAsset>__0 = AssetCache.Find(this.asset);
                    if (this.request.DidSucceed() && (this.<cachedAsset>__0 != null))
                    {
                        this.successCallback(this.<cachedAsset>__0);
                        AssetCache.StopLoading(this.asset.GetName());
                        this.$PC = -1;
                        break;
                    }
                    AssetCache.StopLoading(this.asset.GetName());
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

    [CompilerGenerated]
    private sealed class <DownloadFile>c__Iterator22E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AssetLoader.FileCallback <$>callback;
        internal object <$>callbackData;
        internal string <$>path;
        internal AssetLoader <>f__this;
        internal WWW <file>__1;
        internal string <filePath>__0;
        internal string <message>__2;
        internal AssetLoader.FileCallback callback;
        internal object callbackData;
        internal string path;

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
                    if (this.path != null)
                    {
                        this.<filePath>__0 = this.<>f__this.CreateLocalFilePath(this.path);
                        this.<file>__1 = this.<>f__this.CreateLocalFile(this.<filePath>__0);
                        this.$current = this.<>f__this.StartCoroutine(AssetLoader.WaitForDownload(this.<file>__1));
                        this.$PC = 1;
                        return true;
                    }
                    if (this.callback != null)
                    {
                        this.callback(null, null, this.callbackData);
                    }
                    break;

                case 1:
                    if (this.<file>__1.error == null)
                    {
                        if (this.callback != null)
                        {
                            this.callback(this.path, this.<file>__1, this.callbackData);
                        }
                        this.$PC = -1;
                        break;
                    }
                    this.<message>__2 = string.Format("AssetLoader.DownloadFile() - FAILED to load asset '{0}' path '{1}', reason '{2}'", this.path, this.<file>__1.url, this.<file>__1.error);
                    UnityEngine.Debug.LogError(this.<message>__2);
                    if (this.callback != null)
                    {
                        this.callback(this.path, null, this.callbackData);
                    }
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

    [CompilerGenerated]
    private sealed class <Init>c__Iterator22D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AssetLoader <>f__this;

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
                    this.<>f__this.PreloadBundles();
                    ApplicationMgr.Get().WillReset += new System.Action(this.<>f__this.WillReset);
                    this.<>f__this.SetReady(true);
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    this.$PC = -1;
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

    [CompilerGenerated]
    private sealed class <LoadCachedObject>c__AnonStorey35E
    {
        internal Asset asset;
        internal AssetCache.ObjectCacheRequest request;

        internal void <>m__1F3(AssetCache.CachedAsset cachedAsset)
        {
            UnityEngine.Object assetObject = cachedAsset.GetAssetObject();
            this.request.OnLoadComplete(this.asset.GetName(), assetObject);
        }
    }

    [CompilerGenerated]
    private sealed class <LoadCachedPrefab>c__AnonStorey35F
    {
        internal AssetLoader <>f__this;
        internal Asset asset;
        internal AssetCache.PrefabCacheRequest request;
        internal bool usePrefabPosition;

        internal void <>m__1F4(AssetCache.CachedAsset cachedAsset)
        {
            UnityEngine.Object assetObject = cachedAsset.GetAssetObject();
            foreach (AssetCache.GameObjectRequester requester in this.request.GetRequesters())
            {
                this.<>f__this.StartCoroutine(this.<>f__this.WaitThenCallGameObjectCallback(this.asset, assetObject, this.usePrefabPosition, requester.m_callback, requester.m_callbackData));
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WaitForDownload>c__Iterator22F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal WWW <$>file;
        internal WWW file;

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
                case 1:
                    if (!this.file.isDone)
                    {
                        this.$current = 0;
                        this.$PC = 1;
                        return true;
                    }
                    this.$PC = -1;
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

    [CompilerGenerated]
    private sealed class <WaitThenCallGameObjectCallback>c__Iterator232 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Asset <$>asset;
        internal AssetLoader.GameObjectCallback <$>callback;
        internal object <$>callbackData;
        internal UnityEngine.Object <$>prefab;
        internal bool <$>usePrefabPosition;
        internal AssetLoader <>f__this;
        internal GameObject <instance>__4;
        internal string <internalErrorMessage>__2;
        internal string <name>__0;
        internal GameObject <original>__3;
        internal string <userErrorMessage>__1;
        internal Asset asset;
        internal AssetLoader.GameObjectCallback callback;
        internal object callbackData;
        internal UnityEngine.Object prefab;
        internal bool usePrefabPosition;

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
                {
                    this.<name>__0 = this.asset.GetName();
                    if (this.prefab is GameObject)
                    {
                        AssetCache.StartLoading(this.<name>__0);
                        this.<original>__3 = (GameObject) this.prefab;
                        this.<instance>__4 = null;
                        if (this.usePrefabPosition)
                        {
                            this.<instance>__4 = (GameObject) UnityEngine.Object.Instantiate(this.prefab);
                        }
                        else
                        {
                            this.<instance>__4 = (GameObject) UnityEngine.Object.Instantiate(this.prefab, this.<>f__this.NewGameObjectSpawnPosition(this.prefab), this.<original>__3.transform.rotation);
                        }
                        this.<>f__this.m_waitingOnObjects.Add(this.<instance>__4);
                        this.$current = new WaitForEndOfFrame();
                        this.$PC = 1;
                        return true;
                    }
                    object[] args = new object[] { this.<name>__0 };
                    this.<userErrorMessage>__1 = GameStrings.Format("GLOBAL_ERROR_ASSET_INCORRECT_DATA", args);
                    Error.AddFatal(this.<userErrorMessage>__1);
                    this.<internalErrorMessage>__2 = string.Format("AssetLoader.WaitThenCallGameObjectCallback() - {0} (prefab={1})", this.<userErrorMessage>__1, this.prefab);
                    UnityEngine.Debug.LogError(this.<internalErrorMessage>__2);
                    if (this.callback != null)
                    {
                        this.callback(this.<name>__0, null, this.callbackData);
                    }
                    break;
                }
                case 1:
                    this.<>f__this.m_waitingOnObjects.Remove(this.<instance>__4);
                    AssetCache.StopLoading(this.<name>__0);
                    if (!AssetCache.HasItem(this.asset))
                    {
                        UnityEngine.Object.DestroyImmediate(this.<instance>__4);
                    }
                    if (GeneralUtils.IsCallbackValid(this.callback))
                    {
                        this.callback(this.<name>__0, this.<instance>__4, this.callbackData);
                    }
                    this.$PC = -1;
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

    public delegate void FileCallback(string path, WWW file, object callbackData);

    public delegate void GameObjectCallback(string name, GameObject go, object callbackData);

    public delegate void ObjectCallback(string name, UnityEngine.Object obj, object callbackData);
}

