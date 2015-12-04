using PegasusShared;
using PegasusUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class TavernBrawlManager
{
    [CompilerGenerated]
    private static Func<NetCache.HeroLevel, bool> <>f__am$cacheC;
    private const float DEFAULT_REFRESH_SPEC_SLUSH_SECONDS_MAX = 120f;
    private const float DEFAULT_REFRESH_SPEC_SLUSH_SECONDS_MIN = 0f;
    private TavernBrawlMission m_currentMission;
    private bool m_hasGottenClientOptionsAtLeastOnce;
    private DateTime? m_nextSeasonStartDateLocal;
    private bool m_scenarioAssetPendingLoad;
    private DateTime? m_scheduledRefreshTimeLocal;
    private List<CallbackEnsureServerDataReady> m_serverDataReadyCallbacks;
    public const int MINIMUM_CLASS_LEVEL = 20;
    private const int PRUNE_CACHED_ASSETS_MAX_AGE_DAYS = 0x7c;
    private static Map<AssetRequestKey, LoadCachedAssetCallback> s_assetRequests = new Map<AssetRequestKey, LoadCachedAssetCallback>();
    private static TavernBrawlManager s_instance;
    private bool s_isFirstTimeSeeingCurrentSeason;
    private bool s_isFirstTimeSeeingThisFeature;

    public event System.Action OnTavernBrawlUpdated;

    private static void CachedReceivedAsset(AssetType assetType, int assetId, byte[] assetBytes, int assetBytesLength)
    {
        byte[] assetHash = System.Security.Cryptography.SHA1.Create().ComputeHash(assetBytes, 0, assetBytesLength);
        string path = GetCachedAssetFilePath(assetType, assetId, assetHash);
        try
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                stream.Write(assetBytes, 0, assetBytesLength);
            }
        }
        catch (Exception exception)
        {
            object[] messageArgs = new object[] { path, exception.ToString() };
            Error.AddDevFatal("Error saving cached asset {0}:\n{1}", messageArgs);
        }
    }

    public void Cheat_ResetSeenStuff(int newValue)
    {
        if (!ApplicationMgr.IsPublic())
        {
            this.RegisterOptionsListeners(false);
            Options.Get().SetInt(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD, newValue);
            Options.Get().SetInt(Option.LATEST_SEEN_TAVERNBRAWL_SEASON, newValue);
            Options.Get().SetInt(Option.TIMES_SEEN_TAVERNBRAWL_CRAZY_RULES_QUOTE, 0);
            this.CheckLatestSeenSeason(false);
            this.RegisterOptionsListeners(true);
        }
    }

    public void Cheat_ResetToServerData()
    {
        if (!ApplicationMgr.IsPublic())
        {
            this.NetCache_OnTavernBrawlInfo();
            if (this.m_currentMission != null)
            {
                LoadCachedAsset(true, AssetType.ASSET_TYPE_SCENARIO, this.m_currentMission.missionId, 0, null, new LoadCachedAssetCallback(this.OnTavernBrawlScenarioLoaded));
            }
        }
    }

    public void Cheat_SetScenario(int scenarioId)
    {
        if (!ApplicationMgr.IsPublic())
        {
            if (this.m_currentMission == null)
            {
                this.m_currentMission = new TavernBrawlMission();
            }
            this.m_currentMission.missionId = scenarioId;
            this.m_scenarioAssetPendingLoad = true;
            if (this.OnTavernBrawlUpdated != null)
            {
                this.OnTavernBrawlUpdated();
            }
            LoadCachedAsset(true, AssetType.ASSET_TYPE_SCENARIO, scenarioId, 0, null, new LoadCachedAssetCallback(this.OnTavernBrawlScenarioLoaded));
        }
    }

    private void CheckLatestSeenSeason(bool canSetOption)
    {
        if (this.IsTavernBrawlInfoReady)
        {
            bool flag = !this.m_hasGottenClientOptionsAtLeastOnce;
            this.m_hasGottenClientOptionsAtLeastOnce = true;
            bool isFirstTimeSeeingThisFeature = this.IsFirstTimeSeeingThisFeature;
            bool isFirstTimeSeeingCurrentSeason = this.IsFirstTimeSeeingCurrentSeason;
            this.s_isFirstTimeSeeingThisFeature = false;
            this.s_isFirstTimeSeeingCurrentSeason = false;
            TavernBrawlInfo serverInfo = this.ServerInfo;
            if (serverInfo.HasCurrentTavernBrawl)
            {
                NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
                bool flag4 = ((netObject != null) && netObject.Games.TavernBrawl) && this.HasUnlockedTavernBrawl;
                TavernBrawlSpec currentTavernBrawl = serverInfo.CurrentTavernBrawl;
                int @int = Options.Get().GetInt(Option.LATEST_SEEN_TAVERNBRAWL_SEASON);
                if ((@int == 0) && flag4)
                {
                    this.s_isFirstTimeSeeingThisFeature = true;
                    NotificationManager.Get().ForceRemoveSoundFromPlayedList("VO_INNKEEPER_TAVERNBRAWL_PUSH_32");
                    NotificationManager.Get().ForceRemoveSoundFromPlayedList("VO_INNKEEPER_TAVERNBRAWL_WELCOME1_27");
                }
                if ((@int < currentTavernBrawl.SeasonId) && flag4)
                {
                    this.s_isFirstTimeSeeingCurrentSeason = true;
                    NotificationManager.Get().ForceRemoveSoundFromPlayedList("VO_INNKEEPER_TAVERNBRAWL_DESC2_30");
                    Hub.s_hasAlreadyShownTBAnimation = false;
                    if (canSetOption)
                    {
                        Options.Get().SetInt(Option.LATEST_SEEN_TAVERNBRAWL_SEASON, currentTavernBrawl.SeasonId);
                    }
                }
            }
            if (((flag || (isFirstTimeSeeingThisFeature != this.IsFirstTimeSeeingThisFeature)) || (isFirstTimeSeeingCurrentSeason != this.IsFirstTimeSeeingCurrentSeason)) && (this.OnTavernBrawlUpdated != null))
            {
                this.OnTavernBrawlUpdated();
            }
        }
    }

    public CollectionDeck CurrentDeck()
    {
        foreach (CollectionDeck deck in CollectionManager.Get().GetDecks().Values)
        {
            if (deck.Type == DeckType.TAVERN_BRAWL_DECK)
            {
                return deck;
            }
        }
        return null;
    }

    public TavernBrawlMission CurrentMission()
    {
        return this.m_currentMission;
    }

    public void EnsureScenarioDataReady(CallbackEnsureServerDataReady callback = null)
    {
        <EnsureScenarioDataReady>c__AnonStorey34A storeya = new <EnsureScenarioDataReady>c__AnonStorey34A {
            callback = callback,
            <>f__this = this
        };
        if (this.IsScenarioDataReady)
        {
            if (storeya.callback != null)
            {
                storeya.callback();
            }
        }
        else
        {
            if (storeya.callback != null)
            {
                if (this.m_serverDataReadyCallbacks == null)
                {
                    this.m_serverDataReadyCallbacks = new List<CallbackEnsureServerDataReady>();
                }
                this.m_serverDataReadyCallbacks.Add(storeya.callback);
            }
            TavernBrawlSpec currentTavernBrawl = this.ServerInfo.CurrentTavernBrawl;
            if (Enumerable.Any<KeyValuePair<AssetRequestKey, LoadCachedAssetCallback>>(s_assetRequests, new Func<KeyValuePair<AssetRequestKey, LoadCachedAssetCallback>, bool>(storeya.<>m__1AB)))
            {
                LoadCachedAsset(false, AssetType.ASSET_TYPE_SCENARIO, currentTavernBrawl.ScenarioId, currentTavernBrawl.ScenarioRecordByteSize, currentTavernBrawl.ScenarioRecordHash, new LoadCachedAssetCallback(this.OnTavernBrawlScenarioLoaded));
            }
            else if (ApplicationMgr.IsInternal())
            {
                ApplicationMgr.Get().ScheduleCallback(Mathf.Max(0f, UnityEngine.Random.Range((float) -3f, (float) 3f)), false, new ApplicationMgr.ScheduledCallback(storeya.<>m__1AC), null);
            }
            else
            {
                LoadCachedAsset(true, AssetType.ASSET_TYPE_SCENARIO, currentTavernBrawl.ScenarioId, currentTavernBrawl.ScenarioRecordByteSize, currentTavernBrawl.ScenarioRecordHash, new LoadCachedAssetCallback(this.OnTavernBrawlScenarioLoaded));
            }
        }
    }

    private void FireTavernBrawlInfoReceived()
    {
        if (TavernBrawlDisplay.Get() != null)
        {
            if (this.m_currentMission == null)
            {
                TavernBrawlDisplay.Get().RefreshDataBasedUI(0f);
            }
            else
            {
                this.EnsureScenarioDataReady(null);
            }
        }
        if (Box.Get() != null)
        {
            Box.Get().UpdateUI(false);
        }
        if (this.OnTavernBrawlUpdated != null)
        {
            this.OnTavernBrawlUpdated();
        }
    }

    public int GamesWon()
    {
        return ((this.MyRecord != null) ? this.MyRecord.GamesWon : 0);
    }

    public static TavernBrawlManager Get()
    {
        if (s_instance == null)
        {
            Debug.LogError("Trying to retrieve the Tavern Brawl Manager without calling TavernBrawlManager.Init()!");
        }
        return s_instance;
    }

    private static string GetCachedAssetFileExtension(AssetType assetType)
    {
        if (assetType != AssetType.ASSET_TYPE_SCENARIO)
        {
            return assetType.ToString().Replace("ASSET_TYPE_", string.Empty).ToLower();
        }
        return "scen";
    }

    private static string GetCachedAssetFilePath(AssetType assetType, int assetId, byte[] assetHash)
    {
        string cachedAssetFolder = GetCachedAssetFolder(assetType);
        string cachedAssetFileExtension = GetCachedAssetFileExtension(assetType);
        object[] args = new object[] { cachedAssetFolder, assetId, assetHash.ToHexString(), cachedAssetFileExtension };
        return string.Format("{0}/{1}_{2}.{3}", args);
    }

    private static string GetCachedAssetFolder(AssetType assetType)
    {
        string str;
        if (assetType == AssetType.ASSET_TYPE_SCENARIO)
        {
            str = "Scenario";
        }
        else
        {
            str = "Other";
        }
        string cachePath = FileUtils.CachePath;
        return string.Format("{0}/{1}", cachePath, str);
    }

    public bool HasCreatedDeck()
    {
        return (this.CurrentDeck() != null);
    }

    public bool HasValidDeck()
    {
        if ((this.m_currentMission == null) || !this.m_currentMission.canCreateDeck)
        {
            return false;
        }
        CollectionDeck deck = this.CurrentDeck();
        if (deck == null)
        {
            return false;
        }
        if (!deck.NetworkContentsLoaded())
        {
            CollectionManager.Get().RequestDeckContents(deck.ID);
            return false;
        }
        if (CollectionDeckValidator.GetDeckViolations(deck).Count > 0)
        {
            return false;
        }
        return true;
    }

    public static void Init()
    {
        if (s_instance == null)
        {
            s_instance = new TavernBrawlManager();
        }
        s_instance.InitImpl();
    }

    private void InitImpl()
    {
        string path = string.Format("{0}/Cached", FileUtils.PersistentDataPath);
        if (Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception)
            {
            }
        }
        NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheTavernBrawlInfo), new System.Action(this.NetCache_OnTavernBrawlInfo));
        NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheHeroLevels), new System.Action(this.NetCache_OnClientOptions));
        Network.Get().RegisterNetHandler(GetAssetResponse.PacketID.ID, new Network.NetHandler(this.Network_OnGetAssetResponse), null);
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        this.RegisterOptionsListeners(true);
    }

    public static bool IsInTavernBrawlFriendlyChallenge()
    {
        return (((SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL) || (SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY)) && FriendChallengeMgr.Get().IsChallengeTavernBrawl());
    }

    private static bool LoadCachedAsset(bool canRequestFromServer, AssetType assetType, int assetId, uint expectedSize, byte[] expectedSha1Hash, LoadCachedAssetCallback cb)
    {
        bool flag = false;
        bool flag2 = false;
        byte[] buffer = null;
        if (expectedSha1Hash == null)
        {
            flag2 = expectedSize == 0;
        }
        else
        {
            string path = GetCachedAssetFilePath(assetType, assetId, expectedSha1Hash);
            if ((expectedSize != 0) && !System.IO.File.Exists(path))
            {
                flag2 = true;
                try
                {
                    Directory.CreateDirectory(GetCachedAssetFolder(assetType));
                }
                catch (Exception exception)
                {
                    object[] messageArgs = new object[] { path, exception.ToString() };
                    Error.AddDevFatal("Error creating cached asset folder {0}:\n{1}", messageArgs);
                    return flag;
                }
            }
            else
            {
                try
                {
                    FileInfo info = new FileInfo(path);
                    if (info.Length != expectedSize)
                    {
                        flag2 = true;
                    }
                    else if (expectedSize != 0)
                    {
                        buffer = System.IO.File.ReadAllBytes(path);
                        flag2 = !GeneralUtils.AreArraysEqual<byte>(System.Security.Cryptography.SHA1.Create().ComputeHash(buffer, 0, buffer.Length), expectedSha1Hash);
                        if (!flag2)
                        {
                            flag = true;
                        }
                    }
                }
                catch (Exception exception2)
                {
                    object[] objArray2 = new object[] { path, exception2.ToString() };
                    Error.AddDevFatal("Error reading cached asset folder {0}:\n{1}", objArray2);
                    flag2 = true;
                }
            }
        }
        if (flag2)
        {
            if (canRequestFromServer)
            {
                object[] objArray3 = new object[] { assetType, assetId, (expectedSha1Hash != null) ? expectedSha1Hash.ToHexString() : "<null>" };
                Log.Henry.ConsolePrint("LoadCachedAsset: locally available=false, requesting from server {0} id={1} hash={2}", objArray3);
                if (cb != null)
                {
                    s_assetRequests[new AssetRequestKey(assetType, assetId)] = cb;
                }
                ConnectAPI.SendAssetRequest(assetType, assetId);
                return flag;
            }
            object[] objArray4 = new object[] { assetType, assetId, (expectedSha1Hash != null) ? expectedSha1Hash.ToHexString() : "<null>" };
            Log.Henry.ConsolePrint("LoadCachedAsset: locally available=false, not requesting from server yet - {0} id={1} hash={2}", objArray4);
            return flag;
        }
        object[] args = new object[] { assetType, assetId, (expectedSha1Hash != null) ? expectedSha1Hash.ToHexString() : "<null>" };
        Log.Henry.ConsolePrint("LoadCachedAsset: locally available=true {0} id={1} hash={2}", args);
        if (cb != null)
        {
            if (buffer == null)
            {
                buffer = new byte[0];
            }
            cb(DatabaseResult.DB_E_SUCCESS, buffer, buffer.Length);
        }
        return flag;
    }

    private void NetCache_OnClientOptions()
    {
        this.RegisterOptionsListeners(false);
        this.CheckLatestSeenSeason(true);
        this.RegisterOptionsListeners(true);
    }

    private void NetCache_OnTavernBrawlInfo()
    {
        this.IsRefreshingTavernBrawlInfo = false;
        TavernBrawlInfo serverInfo = this.ServerInfo;
        if (serverInfo != null)
        {
            if (serverInfo.HasCurrentTavernBrawl)
            {
                this.m_currentMission = new TavernBrawlMission();
                TavernBrawlSpec currentTavernBrawl = serverInfo.CurrentTavernBrawl;
                this.m_currentMission.seasonId = currentTavernBrawl.SeasonId;
                this.m_currentMission.missionId = currentTavernBrawl.ScenarioId;
                this.m_currentMission.rewardType = currentTavernBrawl.RewardType;
                this.m_currentMission.rewardTrigger = currentTavernBrawl.RewardTrigger;
                this.m_currentMission.RewardData1 = currentTavernBrawl.RewardData1;
                this.m_currentMission.RewardData2 = currentTavernBrawl.RewardData2;
                this.CheckLatestSeenSeason(true);
                if (currentTavernBrawl.HasEndSecondsFromNow)
                {
                    this.m_currentMission.endDateLocal = new DateTime?(DateTime.Now + new TimeSpan(0, 0, (int) currentTavernBrawl.EndSecondsFromNow));
                }
                else
                {
                    this.m_currentMission.endDateLocal = null;
                }
                this.m_scenarioAssetPendingLoad = true;
            }
            else
            {
                this.m_currentMission = null;
            }
            if (serverInfo.HasNextStartSecondsFromNow)
            {
                this.m_nextSeasonStartDateLocal = new DateTime?(DateTime.Now + new TimeSpan(0, 0, (int) serverInfo.NextStartSecondsFromNow));
            }
            else
            {
                this.m_nextSeasonStartDateLocal = null;
            }
            ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.ScheduledEndOfCurrentTBCallback), null);
            int currentTavernBrawlSeasonEnd = this.CurrentTavernBrawlSeasonEnd;
            if (this.IsTavernBrawlActive && (currentTavernBrawlSeasonEnd > 0))
            {
                object[] args = new object[] { currentTavernBrawlSeasonEnd };
                Log.EventTiming.Print("Scheduling end of current TB {0} secs from now.", args);
                ApplicationMgr.Get().ScheduleCallback((float) currentTavernBrawlSeasonEnd, true, new ApplicationMgr.ScheduledCallback(this.ScheduledEndOfCurrentTBCallback), null);
            }
            ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.ScheduledRefreshTBSpecCallback), null);
            int nextTavernBrawlSeasonStart = this.NextTavernBrawlSeasonStart;
            if (nextTavernBrawlSeasonStart >= 0)
            {
                this.m_scheduledRefreshTimeLocal = new DateTime?(DateTime.Now + new TimeSpan(0, 0, 0, nextTavernBrawlSeasonStart, 0));
                object[] objArray2 = new object[] { nextTavernBrawlSeasonStart };
                Log.EventTiming.Print("Scheduling TB refresh for {0} secs from now.", objArray2);
                ApplicationMgr.Get().ScheduleCallback((float) nextTavernBrawlSeasonStart, true, new ApplicationMgr.ScheduledCallback(this.ScheduledRefreshTBSpecCallback), null);
            }
            this.FireTavernBrawlInfoReceived();
        }
    }

    private void Network_OnGetAssetResponse()
    {
        GetAssetResponse assetResponse = ConnectAPI.GetAssetResponse();
        if (assetResponse != null)
        {
            for (int i = 0; i < assetResponse.Responses.Count; i++)
            {
                LoadCachedAssetCallback callback;
                AssetResponse response2 = assetResponse.Responses[i];
                if (response2.ErrorCode != DatabaseResult.DB_E_SUCCESS)
                {
                    object[] args = new object[] { (int) response2.ErrorCode, response2.ErrorCode.ToString(), (int) response2.RequestedKey.Type, response2.RequestedKey.Type.ToString(), response2.RequestedKey.AssetId };
                    Debug.Log(string.Format("Network_OnGetAssetResponse: error={0}:{1} type={2}:{3} id={4}", args));
                }
                AssetRequestKey key = new AssetRequestKey();
                byte[] assetBytes = null;
                int assetBytesLength = 0;
                if (response2.HasScenarioAsset)
                {
                    key.Type = AssetType.ASSET_TYPE_SCENARIO;
                    key.AssetId = response2.RequestedKey.AssetId;
                    assetBytes = ProtobufUtil.ToByteArray(response2.ScenarioAsset);
                    assetBytesLength = assetBytes.Length;
                }
                if (assetBytes != null)
                {
                    CachedReceivedAsset(key.Type, key.AssetId, assetBytes, assetBytesLength);
                }
                if (s_assetRequests.TryGetValue(key, out callback))
                {
                    if (assetBytes == null)
                    {
                        assetBytes = new byte[0];
                        assetBytesLength = 0;
                    }
                    s_assetRequests.Remove(key);
                    callback(response2.ErrorCode, assetBytes, assetBytesLength);
                }
            }
            ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(TavernBrawlManager.PruneCachedAssetFiles), null);
            ApplicationMgr.Get().ScheduleCallback(5f, true, new ApplicationMgr.ScheduledCallback(TavernBrawlManager.PruneCachedAssetFiles), null);
        }
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        if (!GameMgr.Get().IsNextTavernBrawl() || GameMgr.Get().IsNextSpectator())
        {
            return false;
        }
        FindGameState state = eventData.m_state;
        switch (state)
        {
            case FindGameState.BNET_ERROR:
            case FindGameState.SERVER_GAME_CANCELED:
                break;

            default:
                if ((state != FindGameState.CLIENT_CANCELED) && (state != FindGameState.CLIENT_ERROR))
                {
                    goto Label_008B;
                }
                break;
        }
        Enum[] status = PresenceMgr.Get().GetStatus();
        if (((status != null) && (status.Length > 0)) && (((PresenceStatus) status[0]) == PresenceStatus.TAVERN_BRAWL_QUEUE))
        {
            PresenceMgr.Get().SetPrevStatus();
        }
    Label_008B:
        return false;
    }

    private void OnOptionChangedCallback(Option option, object prevValue, bool existed, object userData)
    {
        this.RegisterOptionsListeners(false);
        this.CheckLatestSeenSeason(false);
        this.RegisterOptionsListeners(true);
    }

    private void OnTavernBrawlScenarioLoaded(DatabaseResult code, byte[] assetBytes, int assetBytesLength)
    {
        if (((assetBytes != null) && (assetBytes.Length != 0)) && ((assetBytesLength != 0) && (this.m_currentMission != null)))
        {
            ScenarioDbRecord protoScenario = ProtobufUtil.ParseFrom<ScenarioDbRecord>(assetBytes, 0, assetBytesLength);
            if (this.m_currentMission.missionId == protoScenario.Id)
            {
                this.m_scenarioAssetPendingLoad = false;
                DbfRecord record = DbfUtils.ConvertFromProtobuf(protoScenario);
                if (record == null)
                {
                    object[] args = new object[] { (protoScenario != null) ? protoScenario.ToString() : "(null)" };
                    Log.MissingAssets.Print("DbfUtils.ConvertFromProtobuf(protoScenario) returned null:\n{0}", args);
                }
                else
                {
                    GameDbf.Scenario.RelaceRecord(record);
                }
                this.m_currentMission.canSelectHeroForDeck = false;
                this.m_currentMission.canCreateDeck = false;
                this.m_currentMission.canEditDeck = false;
                foreach (GameSetupRule rule in protoScenario.Rules)
                {
                    if (rule.RuleType == RuleType.RULE_CHOOSE_HERO)
                    {
                        this.m_currentMission.canSelectHeroForDeck = true;
                        this.m_currentMission.canCreateDeck = false;
                        this.m_currentMission.canEditDeck = false;
                    }
                    else if (rule.RuleType == RuleType.RULE_CHOOSE_DECK)
                    {
                        this.m_currentMission.canSelectHeroForDeck = true;
                        this.m_currentMission.canCreateDeck = true;
                        this.m_currentMission.canEditDeck = true;
                    }
                }
                if (TavernBrawlDisplay.Get() != null)
                {
                    TavernBrawlDisplay.Get().RefreshDataBasedUI(0f);
                }
                if (this.m_serverDataReadyCallbacks != null)
                {
                    CallbackEnsureServerDataReady[] readyArray = this.m_serverDataReadyCallbacks.ToArray();
                    this.m_serverDataReadyCallbacks.Clear();
                    for (int i = 0; i < readyArray.Length; i++)
                    {
                        CallbackEnsureServerDataReady ready = readyArray[i];
                        ready();
                    }
                }
            }
        }
    }

    private static void PruneCachedAssetFiles(object userData)
    {
        string cachePath = FileUtils.CachePath;
        string message = null;
        string name = null;
        try
        {
            DirectoryInfo info = new DirectoryInfo(cachePath);
            if (info.Exists)
            {
                foreach (DirectoryInfo info2 in info.GetDirectories())
                {
                    message = info2.FullName;
                    foreach (FileInfo info3 in info.GetFiles())
                    {
                        name = info3.Name;
                        TimeSpan span = (TimeSpan) (info3.LastWriteTime - DateTime.Now);
                        if (span.TotalDays > 124.0)
                        {
                            info3.Delete();
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            object[] messageArgs = new object[] { name, exception.ToString() };
            Error.AddDevWarning("Error pruning dir={0} file={1}:\n{2}", message, messageArgs);
        }
    }

    public void RefreshServerData()
    {
        NetCache.Get().ReloadNetObject<NetCache.NetCacheTavernBrawlInfo>();
    }

    private void RegisterOptionsListeners(bool register)
    {
        if (register)
        {
            NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheClientOptions), new System.Action(this.NetCache_OnClientOptions));
            Options.Get().RegisterChangedListener(Option.LATEST_SEEN_TAVERNBRAWL_SEASON, new Options.ChangedCallback(this.OnOptionChangedCallback));
            Options.Get().RegisterChangedListener(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD, new Options.ChangedCallback(this.OnOptionChangedCallback));
        }
        else
        {
            NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheClientOptions), new System.Action(this.NetCache_OnClientOptions));
            Options.Get().UnregisterChangedListener(Option.LATEST_SEEN_TAVERNBRAWL_SEASON, new Options.ChangedCallback(this.OnOptionChangedCallback));
            Options.Get().UnregisterChangedListener(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD, new Options.ChangedCallback(this.OnOptionChangedCallback));
        }
    }

    public int RewardProgress()
    {
        return ((this.MyRecord != null) ? this.MyRecord.RewardProgress : 0);
    }

    private void ScheduledEndOfCurrentTBCallback(object userData)
    {
        Log.EventTiming.Print("ScheduledEndOfCurrentTBCallback: ending current TB now.", new object[0]);
        this.m_currentMission = null;
        if (GameMgr.Get().IsFindingGame())
        {
            GameMgr.Get().CancelFindGame();
        }
        this.FireTavernBrawlInfoReceived();
    }

    private void ScheduledRefreshTBSpecCallback(object userData)
    {
        Log.EventTiming.Print("ScheduledRefreshTBSpecCallback: refreshing now.", new object[0]);
        this.RefreshServerData();
    }

    public bool SelectHeroBeforeMission()
    {
        return (((this.m_currentMission != null) && this.m_currentMission.canSelectHeroForDeck) && !this.m_currentMission.canCreateDeck);
    }

    public bool ShouldNewFriendlyChallengeBeTavernBrawl()
    {
        return ((SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL) && this.IsTavernBrawlActive);
    }

    public void StartGame(long deckId = 0)
    {
        if (this.m_currentMission == null)
        {
            Error.AddDevFatal("TB: m_currentMission is null", new object[0]);
        }
        else
        {
            Enum[] args = new Enum[] { PresenceStatus.TAVERN_BRAWL_QUEUE };
            PresenceMgr.Get().SetStatus(args);
            GameMgr.Get().FindGame(GameType.GT_TAVERNBRAWL, this.m_currentMission.missionId, deckId, 0L);
        }
    }

    public int WinStreak()
    {
        return ((this.MyRecord != null) ? this.MyRecord.WinStreak : 0);
    }

    public int CurrentTavernBrawlSeasonEnd
    {
        get
        {
            if ((this.m_currentMission == null) || !this.m_currentMission.endDateLocal.HasValue)
            {
                return -1;
            }
            TimeSpan span = this.m_currentMission.endDateLocal.Value - DateTime.Now;
            return (int) span.TotalSeconds;
        }
    }

    public bool HasSeenTavernBrawlScreen
    {
        get
        {
            return (Options.Get().GetInt(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD) > 0);
        }
    }

    public bool HasUnlockedTavernBrawl
    {
        get
        {
            NetCache.NetCacheHeroLevels netObject = NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>();
            if (netObject == null)
            {
                return false;
            }
            if (<>f__am$cacheC == null)
            {
                <>f__am$cacheC = l => l.CurrentLevel.Level >= 20;
            }
            return Enumerable.Any<NetCache.HeroLevel>(netObject.Levels, <>f__am$cacheC);
        }
    }

    public bool IsCheated
    {
        get
        {
            if (ApplicationMgr.IsPublic())
            {
                return false;
            }
            if (this.m_currentMission == null)
            {
                return ((this.ServerInfo != null) && (this.ServerInfo.CurrentTavernBrawl != null));
            }
            return (((this.ServerInfo == null) || (this.ServerInfo.CurrentTavernBrawl == null)) || (this.m_currentMission.missionId != this.ServerInfo.CurrentTavernBrawl.ScenarioId));
        }
    }

    public bool IsFirstTimeSeeingCurrentSeason
    {
        get
        {
            return (this.s_isFirstTimeSeeingCurrentSeason && this.IsTavernBrawlActive);
        }
    }

    public bool IsFirstTimeSeeingThisFeature
    {
        get
        {
            return (this.s_isFirstTimeSeeingThisFeature && this.IsTavernBrawlActive);
        }
    }

    public bool IsRefreshingTavernBrawlInfo { get; set; }

    public bool IsScenarioDataReady
    {
        get
        {
            return (((this.m_currentMission == null) || (this.ServerInfo == null)) || !this.m_scenarioAssetPendingLoad);
        }
    }

    public bool IsTavernBrawlActive
    {
        get
        {
            return ((this.CurrentMission() != null) && (this.CurrentTavernBrawlSeasonEnd > 0));
        }
    }

    public bool IsTavernBrawlInfoReady
    {
        get
        {
            if (NetCache.Get().GetNetObject<NetCache.NetCacheClientOptions>() == null)
            {
                return false;
            }
            if (this.ServerInfo == null)
            {
                return false;
            }
            return true;
        }
    }

    private TavernBrawlPlayerRecord MyRecord
    {
        get
        {
            NetCache.NetCacheTavernBrawlRecord netObject = NetCache.Get().GetNetObject<NetCache.NetCacheTavernBrawlRecord>();
            return ((netObject != null) ? netObject.Record : null);
        }
    }

    public int NextTavernBrawlSeasonStart
    {
        get
        {
            if (!this.m_nextSeasonStartDateLocal.HasValue)
            {
                return -1;
            }
            TimeSpan span = this.m_nextSeasonStartDateLocal.Value - DateTime.Now;
            return (int) span.TotalSeconds;
        }
    }

    public float ScheduledSecondsToRefresh
    {
        get
        {
            if (!this.m_scheduledRefreshTimeLocal.HasValue)
            {
                return -1f;
            }
            TimeSpan span = this.m_scheduledRefreshTimeLocal.Value - DateTime.Now;
            return (float) span.TotalSeconds;
        }
    }

    private TavernBrawlInfo ServerInfo
    {
        get
        {
            NetCache.NetCacheTavernBrawlInfo netObject = NetCache.Get().GetNetObject<NetCache.NetCacheTavernBrawlInfo>();
            return ((netObject != null) ? netObject.Info : null);
        }
    }

    [CompilerGenerated]
    private sealed class <EnsureScenarioDataReady>c__AnonStorey34A
    {
        internal TavernBrawlManager <>f__this;
        internal TavernBrawlManager.CallbackEnsureServerDataReady callback;

        internal bool <>m__1AB(KeyValuePair<TavernBrawlManager.AssetRequestKey, TavernBrawlManager.LoadCachedAssetCallback> kv)
        {
            return ((kv.Key.AssetId == this.<>f__this.m_currentMission.missionId) && (kv.Key.Type == AssetType.ASSET_TYPE_SCENARIO));
        }

        internal void <>m__1AC(object userData)
        {
            TavernBrawlManager manager = TavernBrawlManager.Get();
            if (manager.IsScenarioDataReady)
            {
                if (this.callback != null)
                {
                    if (manager.m_serverDataReadyCallbacks != null)
                    {
                        manager.m_serverDataReadyCallbacks.Remove(this.callback);
                    }
                    this.callback();
                }
            }
            else
            {
                TavernBrawlSpec currentTavernBrawl = manager.ServerInfo.CurrentTavernBrawl;
                TavernBrawlManager.LoadCachedAsset(true, AssetType.ASSET_TYPE_SCENARIO, currentTavernBrawl.ScenarioId, currentTavernBrawl.ScenarioRecordByteSize, currentTavernBrawl.ScenarioRecordHash, new TavernBrawlManager.LoadCachedAssetCallback(manager.OnTavernBrawlScenarioLoaded));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AssetRequestKey
    {
        public AssetType Type;
        public int AssetId;
        public AssetRequestKey(AssetType type, int assetId)
        {
            this.Type = type;
            this.AssetId = assetId;
        }

        public override int GetHashCode()
        {
            return (this.Type.GetHashCode() ^ this.AssetId.GetHashCode());
        }
    }

    public delegate void CallbackEnsureServerDataReady();

    public delegate void LoadCachedAssetCallback(DatabaseResult code, byte[] assetBytes, int assetBytesLength);
}

