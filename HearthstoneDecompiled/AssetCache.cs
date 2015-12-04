using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AssetCache
{
    private Map<string, CachedAsset> m_assetMap = new Map<string, CachedAsset>();
    private Map<string, CacheRequest> m_assetRequestMap = new Map<string, CacheRequest>();
    private static Map<string, int> s_assetLoading = new Map<string, int>();
    private static long s_cacheClearTime;
    private static readonly Map<AssetFamily, AssetCache> s_cacheTable = new Map<AssetFamily, AssetCache>();

    public static void Add(Asset asset, CachedAsset item)
    {
        s_cacheTable[asset.GetFamily()].AddItem(asset.GetName(), item);
    }

    private void AddItem(string name, CachedAsset item)
    {
        if (this.m_assetMap.ContainsKey(name))
        {
            Debug.LogWarning(string.Format("AssetCache: Loaded asset {0} twice.  This probably happened because it was loaded asynchronously and synchronously.", name));
        }
        else
        {
            this.m_assetMap.Add(name, item);
        }
    }

    public static void AddRequest(Asset asset, CacheRequest request)
    {
        s_cacheTable[asset.GetFamily()].AddRequest(asset.GetName(), request);
    }

    public void AddRequest(string key, CacheRequest request)
    {
        this.m_assetRequestMap.Add(key, request);
    }

    private void Clear(bool clearPersistent = false, bool clearLoading = true)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, CachedAsset> pair in this.m_assetMap)
        {
            string key = pair.Key;
            if (!pair.Value.IsPersistent() || clearPersistent)
            {
                if (IsLoading(key) && !clearLoading)
                {
                    Log.Reset.Print(LogLevel.Warning, "Not clearing asset " + key + " because it's still loading", new object[0]);
                }
                else
                {
                    list.Add(key);
                }
            }
        }
        foreach (string str2 in list)
        {
            this.ClearItem(str2);
        }
        List<string> list2 = new List<string>();
        foreach (KeyValuePair<string, CacheRequest> pair2 in this.m_assetRequestMap)
        {
            string name = pair2.Key;
            if ((!pair2.Value.IsPersistent() || clearPersistent) && (!IsLoading(name) || clearLoading))
            {
                list2.Add(name);
            }
        }
        foreach (string str4 in list2)
        {
            this.ClearItem(str4);
        }
    }

    public static void ClearActor(string name)
    {
        s_cacheTable[AssetFamily.Actor].ClearItem(name);
    }

    public static void ClearActors(IEnumerable<string> names)
    {
        if (names != null)
        {
            s_cacheTable[AssetFamily.Actor].ClearItems(names);
        }
    }

    public static void ClearAllCaches(bool clearPersistent = false, bool clearLoading = true)
    {
        foreach (KeyValuePair<AssetFamily, AssetCache> pair in s_cacheTable)
        {
            pair.Value.Clear(clearPersistent, clearLoading);
        }
        s_cacheClearTime = TimeUtils.BinaryStamp();
    }

    public static void ClearAllCachesBetween(long startTimestamp, long endTimestamp)
    {
        foreach (KeyValuePair<AssetFamily, AssetCache> pair in s_cacheTable)
        {
            pair.Value.ClearItemsBetween(startTimestamp, endTimestamp);
        }
    }

    public static void ClearAllCachesFailedRequests()
    {
        foreach (KeyValuePair<AssetFamily, AssetCache> pair in s_cacheTable)
        {
            pair.Value.ClearAllFailedRequests();
        }
    }

    public static void ClearAllCachesSince(long sinceTimestamp)
    {
        long endTimestamp = TimeUtils.BinaryStamp();
        ClearAllCachesBetween(sinceTimestamp, endTimestamp);
    }

    private void ClearAllFailedRequests()
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, CacheRequest> pair in this.m_assetRequestMap)
        {
            if (pair.Value.DidFail())
            {
                list.Add(pair.Key);
            }
        }
        foreach (string str in list)
        {
            this.m_assetRequestMap.Remove(str);
        }
    }

    public static void ClearCardPrefab(string name)
    {
        s_cacheTable[AssetFamily.CardPrefab].ClearItem(name);
    }

    public static void ClearCardPrefabs(IEnumerable<string> names)
    {
        if (names != null)
        {
            s_cacheTable[AssetFamily.CardPrefab].ClearItems(names);
        }
    }

    public static void ClearGameObject(string name)
    {
        s_cacheTable[AssetFamily.GameObject].ClearItem(name);
    }

    public static bool ClearItem(Asset asset)
    {
        return s_cacheTable[asset.GetFamily()].ClearItem(asset.GetName());
    }

    private bool ClearItem(string key)
    {
        CachedAsset asset;
        CacheRequest request;
        bool flag = false;
        if (this.m_assetMap.TryGetValue(key, out asset))
        {
            asset.UnloadAssetObject();
            this.m_assetMap.Remove(key);
            flag = true;
        }
        if (this.m_assetRequestMap.TryGetValue(key, out request))
        {
            request.SetSuccess(false);
            this.m_assetRequestMap.Remove(key);
            flag = true;
        }
        if (!flag)
        {
            Log.Asset.Print(string.Format("AssetCache.ClearItem() - there is no asset and no request for key {0} in {1}", key, this), new object[0]);
        }
        return flag;
    }

    private void ClearItems(IEnumerable<string> itemsToRemove)
    {
        IEnumerator<string> enumerator = itemsToRemove.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                string current = enumerator.Current;
                this.ClearItem(current);
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
    }

    private void ClearItemsBetween(long startTimestamp, long endTimestamp)
    {
        if (endTimestamp >= startTimestamp)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (KeyValuePair<string, CachedAsset> pair in this.m_assetMap)
            {
                CachedAsset asset = pair.Value;
                if (!asset.IsPersistent())
                {
                    long lastRequestTimestamp = asset.GetLastRequestTimestamp();
                    if ((startTimestamp <= lastRequestTimestamp) && (lastRequestTimestamp <= endTimestamp))
                    {
                        set.Add(pair.Key);
                    }
                }
            }
            foreach (KeyValuePair<string, CacheRequest> pair2 in this.m_assetRequestMap)
            {
                CacheRequest request = pair2.Value;
                if (!request.IsPersistent())
                {
                    long num2 = request.GetLastRequestTimestamp();
                    if ((startTimestamp <= num2) && (num2 <= endTimestamp))
                    {
                        set.Add(pair2.Key);
                    }
                }
            }
            foreach (string str in set)
            {
                this.ClearItem(str);
            }
        }
    }

    public static void ClearSound(string name)
    {
        s_cacheTable[AssetFamily.Sound].ClearItem(name);
    }

    public static void ClearTexture(string name)
    {
        s_cacheTable[AssetFamily.Texture].ClearItem(name);
    }

    public static void ClearTextures(IEnumerable<string> names)
    {
        if (names != null)
        {
            s_cacheTable[AssetFamily.Texture].ClearItems(names);
        }
    }

    public static CachedAsset Find(Asset asset)
    {
        return s_cacheTable[asset.GetFamily()].GetItem(asset.GetName());
    }

    private void ForceClear()
    {
        foreach (KeyValuePair<string, CachedAsset> pair in this.m_assetMap)
        {
            pair.Value.UnloadAssetObject();
        }
        foreach (KeyValuePair<string, CacheRequest> pair2 in this.m_assetRequestMap)
        {
            pair2.Value.SetSuccess(false);
        }
        this.m_assetMap.Clear();
        this.m_assetRequestMap.Clear();
    }

    public static void ForceClearAllCaches()
    {
        foreach (KeyValuePair<AssetFamily, AssetCache> pair in s_cacheTable)
        {
            pair.Value.ForceClear();
        }
        s_cacheClearTime = TimeUtils.BinaryStamp();
    }

    public static long GetCacheClearTime()
    {
        return s_cacheClearTime;
    }

    private AssetFamily? GetFamily()
    {
        foreach (KeyValuePair<AssetFamily, AssetCache> pair in s_cacheTable)
        {
            if (this == pair.Value)
            {
                return new AssetFamily?(pair.Key);
            }
        }
        return null;
    }

    private CachedAsset GetItem(string key)
    {
        CachedAsset asset;
        return (!this.m_assetMap.TryGetValue(key, out asset) ? null : asset);
    }

    public static T GetRequest<T>(Asset asset) where T: CacheRequest
    {
        return s_cacheTable[asset.GetFamily()].GetRequest<T>(asset.GetName());
    }

    public T GetRequest<T>(string key) where T: CacheRequest
    {
        CacheRequest request;
        if (this.m_assetRequestMap.TryGetValue(key, out request))
        {
            return (request as T);
        }
        return null;
    }

    public static bool HasItem(Asset asset)
    {
        return s_cacheTable[asset.GetFamily()].HasItem(asset.GetName());
    }

    private bool HasItem(string key)
    {
        return this.m_assetMap.ContainsKey(key);
    }

    public static bool HasRequest(Asset asset)
    {
        return s_cacheTable[asset.GetFamily()].HasRequest(asset.GetName());
    }

    public bool HasRequest(string key)
    {
        return this.m_assetRequestMap.ContainsKey(key);
    }

    public static void Initialize()
    {
        IEnumerator enumerator = Enum.GetValues(typeof(AssetFamily)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                AssetFamily current = (AssetFamily) ((int) enumerator.Current);
                AssetCache cache = new AssetCache();
                s_cacheTable.Add(current, cache);
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
    }

    public static bool IsLoading(string name)
    {
        int num;
        return (s_assetLoading.TryGetValue(name, out num) && (num > 0));
    }

    public static bool RemoveRequest(Asset asset)
    {
        return s_cacheTable[asset.GetFamily()].RemoveRequest(asset.GetName());
    }

    public bool RemoveRequest(string key)
    {
        return this.m_assetRequestMap.Remove(key);
    }

    public static void StartLoading(string name)
    {
        Map<string, int> map;
        string str;
        if (!s_assetLoading.ContainsKey(name))
        {
            s_assetLoading.Add(name, 0);
        }
        int num = map[str];
        (map = s_assetLoading)[str = name] = num + 1;
    }

    public static void StopLoading(string name)
    {
        int num;
        if (s_assetLoading.TryGetValue(name, out num))
        {
            if (num < 1)
            {
                s_assetLoading.Remove(name);
            }
            else
            {
                Map<string, int> map;
                string str;
                int num2 = map[str];
                (map = s_assetLoading)[str = name] = num2 - 1;
            }
        }
    }

    public class CachedAsset
    {
        private Asset m_asset;
        private UnityEngine.Object m_assetObject;
        private long m_createdTimestamp;
        private long m_lastRequestTimestamp;
        private bool m_persistent;

        public Asset GetAsset()
        {
            return this.m_asset;
        }

        public UnityEngine.Object GetAssetObject()
        {
            return this.m_assetObject;
        }

        public long GetCreatedTimestamp()
        {
            return this.m_createdTimestamp;
        }

        public long GetLastRequestTimestamp()
        {
            return this.m_lastRequestTimestamp;
        }

        public bool IsPersistent()
        {
            return this.m_persistent;
        }

        public void SetAsset(Asset asset)
        {
            this.m_asset = asset;
        }

        public void SetAssetObject(UnityEngine.Object asset)
        {
            this.m_assetObject = asset;
        }

        public void SetCreatedTimestamp(long timestamp)
        {
            this.m_createdTimestamp = timestamp;
        }

        public void SetLastRequestTimestamp(long timestamp)
        {
            this.m_lastRequestTimestamp = timestamp;
        }

        public void SetPersistent(bool persistent)
        {
            this.m_persistent = persistent;
        }

        public void UnloadAssetObject()
        {
            object[] args = new object[] { this.m_asset.GetName(), this.m_asset.GetFamily(), this.IsPersistent() };
            Log.Asset.Print("CachedAsset.UnloadAssetObject() - unloading name={0} family={1} persistent={2}", args);
            this.m_assetObject = null;
        }
    }

    public abstract class CacheRequest
    {
        private bool m_complete;
        private long m_createdTimestamp;
        private long m_lastRequestTimestamp;
        private bool m_persistent;
        private bool m_success;

        protected CacheRequest()
        {
        }

        public bool DidFail()
        {
            return (this.m_complete && !this.m_success);
        }

        public bool DidSucceed()
        {
            return (this.m_complete && this.m_success);
        }

        public long GetCreatedTimestamp()
        {
            return this.m_createdTimestamp;
        }

        public long GetLastRequestTimestamp()
        {
            return this.m_lastRequestTimestamp;
        }

        public abstract int GetRequestCount();
        public bool IsComplete()
        {
            return this.m_complete;
        }

        public bool IsPersistent()
        {
            return this.m_persistent;
        }

        public bool IsSuccess()
        {
            return this.m_success;
        }

        public virtual void OnLoadFailed(string name)
        {
            this.m_complete = true;
            this.m_success = false;
        }

        public void OnLoadSucceeded()
        {
            this.m_complete = true;
            this.m_success = true;
        }

        public void SetComplete(bool complete)
        {
            this.m_complete = complete;
        }

        public void SetCreatedTimestamp(long timestamp)
        {
            this.m_createdTimestamp = timestamp;
        }

        public void SetLastRequestTimestamp(long timestamp)
        {
            this.m_lastRequestTimestamp = timestamp;
        }

        public void SetPersistent(bool persistent)
        {
            this.m_persistent = persistent;
        }

        public void SetSuccess(bool success)
        {
            this.m_success = success;
        }
    }

    public class GameObjectRequester
    {
        public AssetLoader.GameObjectCallback m_callback;
        public object m_callbackData;
    }

    public class ObjectCacheRequest : AssetCache.CacheRequest
    {
        private readonly List<AssetCache.ObjectRequester> m_requesters = new List<AssetCache.ObjectRequester>();

        public void AddRequester(AssetLoader.ObjectCallback callback, object callbackData)
        {
            AssetCache.ObjectRequester item = new AssetCache.ObjectRequester {
                m_callback = callback,
                m_callbackData = callbackData
            };
            this.m_requesters.Add(item);
        }

        public override int GetRequestCount()
        {
            return this.m_requesters.Count;
        }

        public List<AssetCache.ObjectRequester> GetRequesters()
        {
            return this.m_requesters;
        }

        public void OnLoadComplete(string name, UnityEngine.Object asset)
        {
            foreach (AssetCache.ObjectRequester requester in this.m_requesters)
            {
                AssetLoader.ObjectCallback callback = requester.m_callback;
                if (GeneralUtils.IsCallbackValid(callback))
                {
                    callback(name, asset, requester.m_callbackData);
                }
            }
        }

        public override void OnLoadFailed(string name)
        {
            base.OnLoadFailed(name);
            this.OnLoadComplete(name, null);
        }
    }

    public class ObjectRequester
    {
        public AssetLoader.ObjectCallback m_callback;
        public object m_callbackData;
    }

    public class PrefabCacheRequest : AssetCache.CacheRequest
    {
        private readonly List<AssetCache.GameObjectRequester> m_requesters = new List<AssetCache.GameObjectRequester>();

        public void AddRequester(AssetLoader.GameObjectCallback callback, object callbackData)
        {
            AssetCache.GameObjectRequester item = new AssetCache.GameObjectRequester {
                m_callback = callback,
                m_callbackData = callbackData
            };
            this.m_requesters.Add(item);
        }

        public override int GetRequestCount()
        {
            return this.m_requesters.Count;
        }

        public List<AssetCache.GameObjectRequester> GetRequesters()
        {
            return this.m_requesters;
        }

        public override void OnLoadFailed(string name)
        {
            base.OnLoadFailed(name);
            foreach (AssetCache.GameObjectRequester requester in this.m_requesters)
            {
                AssetLoader.GameObjectCallback callback = requester.m_callback;
                object callbackData = requester.m_callbackData;
                if (GeneralUtils.IsCallbackValid(callback))
                {
                    callback(name, null, callbackData);
                }
            }
        }
    }
}

