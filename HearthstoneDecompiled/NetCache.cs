using BobNetProto;
using PegasusShared;
using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class NetCache
{
    [CompilerGenerated]
    private static Predicate<ProfileNotice> <>f__am$cacheF;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map2B;
    private List<NetCacheBatchRequest> m_cacheRequests = new List<NetCacheBatchRequest>();
    private Map<System.Type, int> m_changeRequests = new Map<System.Type, int>();
    private static bool m_fatalErrorCodeSet;
    private List<DelGoldBalanceListener> m_goldBalanceListeners = new List<DelGoldBalanceListener>();
    private List<System.Type> m_inTransitRequests = new List<System.Type>();
    private long m_lastForceCheckedSeason;
    private Map<System.Type, object> m_netCache = new Map<System.Type, object>();
    private List<DelNewNoticesListener> m_newNoticesListeners = new List<DelNewNoticesListener>();
    private NetCacheHeroLevels m_prevHeroLevels;
    private NetCacheMedalInfo m_previousMedalInfo;
    private static readonly Map<GetAccountInfo.Request, System.Type> m_requestTypeMap;
    private static readonly Map<System.Type, GetAccountInfo.Request> m_typeMap;
    private Map<System.Type, List<System.Action>> m_updatedListeners = new Map<System.Type, List<System.Action>>();
    private static NetCache s_instance;

    static NetCache()
    {
        Map<System.Type, GetAccountInfo.Request> map = new Map<System.Type, GetAccountInfo.Request>();
        map.Add(typeof(NetCacheLastLogin), GetAccountInfo.Request.LAST_LOGIN);
        map.Add(typeof(NetCacheDecks), GetAccountInfo.Request.DECK_LIST);
        map.Add(typeof(NetCacheCollection), GetAccountInfo.Request.COLLECTION);
        map.Add(typeof(NetCacheMedalInfo), GetAccountInfo.Request.MEDAL_INFO);
        map.Add(typeof(NetCacheMedalHistory), GetAccountInfo.Request.MEDAL_HISTORY);
        map.Add(typeof(NetCacheBoosters), GetAccountInfo.Request.BOOSTERS);
        map.Add(typeof(NetCacheCardBacks), GetAccountInfo.Request.CARD_BACKS);
        map.Add(typeof(NetCachePlayerRecords), GetAccountInfo.Request.PLAYER_RECORD);
        map.Add(typeof(NetCacheGamesPlayed), GetAccountInfo.Request.GAMES_PLAYED);
        map.Add(typeof(NetCacheDeckLimit), GetAccountInfo.Request.DECK_LIMIT);
        map.Add(typeof(NetCacheProfileProgress), GetAccountInfo.Request.CAMPAIGN_INFO);
        map.Add(typeof(NetCacheProfileNotices), GetAccountInfo.Request.NOTICES);
        map.Add(typeof(NetCacheClientOptions), GetAccountInfo.Request.CLIENT_OPTIONS);
        map.Add(typeof(NetCacheCardValues), GetAccountInfo.Request.CARD_VALUES);
        map.Add(typeof(NetCacheDisconnectedGame), GetAccountInfo.Request.DISCONNECTED);
        map.Add(typeof(NetCacheArcaneDustBalance), GetAccountInfo.Request.ARCANE_DUST_BALANCE);
        map.Add(typeof(NetCacheFeatures), GetAccountInfo.Request.FEATURES);
        map.Add(typeof(NetCacheRewardProgress), GetAccountInfo.Request.REWARD_PROGRESS);
        map.Add(typeof(NetCacheGoldBalance), GetAccountInfo.Request.GOLD_BALANCE);
        map.Add(typeof(NetCacheHeroLevels), GetAccountInfo.Request.HERO_XP);
        map.Add(typeof(NetCachePlayQueue), GetAccountInfo.Request.PVP_QUEUE);
        map.Add(typeof(NetCacheNotSoMassiveLogin), GetAccountInfo.Request.NOT_SO_MASSIVE_LOGIN);
        map.Add(typeof(NetCacheBoosterTallies), GetAccountInfo.Request.BOOSTER_TALLY);
        map.Add(typeof(NetCacheTavernBrawlInfo), GetAccountInfo.Request.TAVERN_BRAWL_INFO);
        map.Add(typeof(NetCacheTavernBrawlRecord), GetAccountInfo.Request.TAVERN_BRAWL_RECORD);
        map.Add(typeof(NetCacheFavoriteHeroes), GetAccountInfo.Request.FAVORITE_HEROES);
        map.Add(typeof(NetCacheAccountLicenses), GetAccountInfo.Request.ACCOUNT_LICENSES);
        m_typeMap = map;
        m_requestTypeMap = GetInvertTypeMap();
        m_fatalErrorCodeSet = false;
        s_instance = new NetCache();
    }

    private void AddCollectionManagerToRequest(ref NetCacheBatchRequest request)
    {
        request.AddRequests(new List<Request> { new Request(typeof(NetCacheDecks), 0), new Request(typeof(NetCacheCollection), 0), new Request(typeof(NetCacheDeckLimit), 0), new Request(typeof(NetCacheCardValues), 0), new Request(typeof(NetCacheArcaneDustBalance), 0), new Request(typeof(NetCacheClientOptions), 0) });
    }

    private void AddRandomDeckMakerToRequest(ref NetCacheBatchRequest request)
    {
        request.AddRequests(new List<Request> { new Request(typeof(NetCacheCollection), 0) });
    }

    public void CheckSeasonForRoll()
    {
        if (this.GetNetObject<NetCacheProfileNotices>() != null)
        {
            NetCacheRewardProgress netObject = this.GetNetObject<NetCacheRewardProgress>();
            if (netObject != null)
            {
                DateTime utcNow = DateTime.UtcNow;
                DateTime time2 = DateTime.FromFileTimeUtc(netObject.SeasonEndDate);
                if ((time2 < utcNow) && (this.m_lastForceCheckedSeason != netObject.Season))
                {
                    this.m_lastForceCheckedSeason = netObject.Season;
                    object[] args = new object[] { this.m_lastForceCheckedSeason, time2, utcNow };
                    Log.Rachelle.Print("NetCache.CheckSeasonForRoll oldSeason = {0} season end = {1} utc now = {2}", args);
                    this.ReloadNetObject<NetCacheProfileNotices>();
                }
            }
        }
    }

    public void Clear()
    {
        this.m_netCache.Clear();
        this.m_prevHeroLevels = null;
        this.m_changeRequests.Clear();
        this.m_cacheRequests.Clear();
        this.m_inTransitRequests.Clear();
    }

    public bool ClientOptionExists(ServerOption type)
    {
        NetCacheClientOptions netObject = this.GetNetObject<NetCacheClientOptions>();
        if (netObject == null)
        {
            return false;
        }
        if (!netObject.ClientOptions.ContainsKey(type))
        {
            return false;
        }
        ClientOptionBase base2 = netObject.ClientOptions[type];
        return (base2 != null);
    }

    public static void DefaultErrorHandler(ErrorInfo info)
    {
        if (info.Error == ErrorCode.TIMEOUT)
        {
            if (BreakingNews.SHOWS_BREAKING_NEWS)
            {
                string error = "GLOBAL_ERROR_NETWORK_UTIL_TIMEOUT";
                Network.Get().ShowBreakingNewsOrError(error, 0f);
            }
            else
            {
                ShowError(info, "GLOBAL_ERROR_NETWORK_UTIL_TIMEOUT", new object[0]);
            }
        }
        else
        {
            ShowError(info, "GLOBAL_ERROR_NETWORK_GENERIC", new object[0]);
        }
    }

    public void DeleteClientOption(ServerOption type)
    {
        ConnectAPI.DeleteClientOption((int) type);
        this.SetClientOption(type, null);
    }

    private List<ProfileNotice> FindNewNotices(List<ProfileNotice> receivedNotices)
    {
        List<ProfileNotice> list = new List<ProfileNotice>();
        NetCacheProfileNotices netObject = this.GetNetObject<NetCacheProfileNotices>();
        if (netObject == null)
        {
            list.AddRange(receivedNotices);
            return list;
        }
        <FindNewNotices>c__AnonStorey31C storeyc = new <FindNewNotices>c__AnonStorey31C();
        using (List<ProfileNotice>.Enumerator enumerator = receivedNotices.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                storeyc.receivedNotice = enumerator.Current;
                if (netObject.Notices.Find(new Predicate<ProfileNotice>(storeyc.<>m__13B)) == null)
                {
                    list.Add(storeyc.receivedNotice);
                }
            }
        }
        return list;
    }

    public static NetCache Get()
    {
        if (s_instance == null)
        {
            Debug.LogError("no NetCache object");
        }
        return s_instance;
    }

    public bool GetBoolOption(ServerOption type)
    {
        ClientOptionBool ret = null;
        if (!this.GetOption<ClientOptionBool>(type, out ret))
        {
            return false;
        }
        return ret.OptionValue;
    }

    public bool GetBoolOption(ServerOption type, out bool ret)
    {
        ret = false;
        ClientOptionBool @bool = null;
        if (!this.GetOption<ClientOptionBool>(type, out @bool))
        {
            return false;
        }
        ret = @bool.OptionValue;
        return true;
    }

    public float GetFloatOption(ServerOption type)
    {
        ClientOptionFloat ret = null;
        if (!this.GetOption<ClientOptionFloat>(type, out ret))
        {
            return 0f;
        }
        return ret.OptionValue;
    }

    public bool GetFloatOption(ServerOption type, out float ret)
    {
        ret = 0f;
        ClientOptionFloat num = null;
        if (!this.GetOption<ClientOptionFloat>(type, out num))
        {
            return false;
        }
        ret = num.OptionValue;
        return true;
    }

    public static string GetInternalErrorMessage(ErrorInfo info, bool includeStackTrace = true)
    {
        Map<System.Type, object> netCache = Get().m_netCache;
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("NetCache Error: {0}", info.Error);
        builder.AppendFormat("\nFrom: {0}", info.RequestingFunction.Method.Name);
        builder.AppendFormat("\nRequested Data ({0}):", info.RequestedTypes.Count);
        foreach (KeyValuePair<System.Type, Request> pair in info.RequestedTypes)
        {
            object obj2 = null;
            netCache.TryGetValue(pair.Key, out obj2);
            if (obj2 == null)
            {
                builder.AppendFormat("\n[{0}] MISSING", pair.Key);
            }
            else
            {
                builder.AppendFormat("\n[{0}]", pair.Key);
            }
        }
        if (includeStackTrace)
        {
            builder.AppendFormat("\nStack Trace:\n{0}", info.RequestStackTrace);
        }
        return builder.ToString();
    }

    public int GetIntOption(ServerOption type)
    {
        ClientOptionInt ret = null;
        if (!this.GetOption<ClientOptionInt>(type, out ret))
        {
            return 0;
        }
        return ret.OptionValue;
    }

    public bool GetIntOption(ServerOption type, out int ret)
    {
        ret = 0;
        ClientOptionInt num = null;
        if (!this.GetOption<ClientOptionInt>(type, out num))
        {
            return false;
        }
        ret = num.OptionValue;
        return true;
    }

    private static Map<GetAccountInfo.Request, System.Type> GetInvertTypeMap()
    {
        Map<GetAccountInfo.Request, System.Type> map = new Map<GetAccountInfo.Request, System.Type>();
        foreach (KeyValuePair<System.Type, GetAccountInfo.Request> pair in m_typeMap)
        {
            map[pair.Value] = pair.Key;
        }
        return map;
    }

    public long GetLongOption(ServerOption type)
    {
        ClientOptionLong ret = null;
        if (!this.GetOption<ClientOptionLong>(type, out ret))
        {
            return 0L;
        }
        return ret.OptionValue;
    }

    public bool GetLongOption(ServerOption type, out long ret)
    {
        ret = 0L;
        ClientOptionLong @long = null;
        if (!this.GetOption<ClientOptionLong>(type, out @long))
        {
            return false;
        }
        ret = @long.OptionValue;
        return true;
    }

    public T GetNetObject<T>()
    {
        System.Type type = typeof(T);
        object testData = this.GetTestData(type);
        if (testData != null)
        {
            return (T) testData;
        }
        if (this.m_netCache.TryGetValue(typeof(T), out testData) && (testData is T))
        {
            return (T) testData;
        }
        return default(T);
    }

    private bool GetOption<T>(ServerOption type, out T ret) where T: ClientOptionBase
    {
        ret = null;
        NetCacheClientOptions netObject = Get().GetNetObject<NetCacheClientOptions>();
        if (!this.ClientOptionExists(type))
        {
            return false;
        }
        T local = netObject.ClientOptions[type] as T;
        if (local == null)
        {
            return false;
        }
        ret = local;
        return true;
    }

    private object GetTestData(System.Type type)
    {
        if ((type == typeof(NetCacheBoosters)) && GameUtils.IsFakePackOpeningEnabled())
        {
            NetCacheBoosters boosters = new NetCacheBoosters();
            int fakePackCount = GameUtils.GetFakePackCount();
            BoosterStack item = new BoosterStack {
                Id = 1,
                Count = fakePackCount
            };
            boosters.BoosterStacks.Add(item);
            return boosters;
        }
        return null;
    }

    public ulong GetULongOption(ServerOption type)
    {
        ClientOptionULong ret = null;
        if (!this.GetOption<ClientOptionULong>(type, out ret))
        {
            return 0L;
        }
        return ret.OptionValue;
    }

    public bool GetULongOption(ServerOption type, out ulong ret)
    {
        ret = 0L;
        ClientOptionULong @long = null;
        if (!this.GetOption<ClientOptionULong>(type, out @long))
        {
            return false;
        }
        ret = @long.OptionValue;
        return true;
    }

    public void Heartbeat()
    {
        NetCacheBatchRequest[] requestArray = this.m_cacheRequests.ToArray();
        DateTime now = DateTime.Now;
        foreach (NetCacheBatchRequest request in requestArray)
        {
            if (!request.m_canTimeout)
            {
                continue;
            }
            TimeSpan span = (TimeSpan) (now - request.m_timeAdded);
            if ((span >= Network.GetMaxDeferredWait()) && !Network.Get().HaveUnhandledPackets())
            {
                request.m_canTimeout = false;
                if (!m_fatalErrorCodeSet)
                {
                    ErrorInfo info = new ErrorInfo {
                        Error = ErrorCode.TIMEOUT,
                        RequestingFunction = request.m_requestFunc,
                        RequestedTypes = new Map<System.Type, Request>(request.m_requests),
                        RequestStackTrace = request.m_requestStackTrace
                    };
                    string str = "CT";
                    int num2 = 0;
                    foreach (KeyValuePair<System.Type, Request> pair in request.m_requests)
                    {
                        RequestResult result = pair.Value.m_result;
                        if ((result != RequestResult.GENERIC_COMPLETE) && (result != RequestResult.DATA_COMPLETE))
                        {
                            char[] separator = new char[] { '+' };
                            string[] strArray = pair.Value.m_type.ToString().Split(separator);
                            if (strArray.GetLength(0) != 0)
                            {
                                string str2 = strArray[strArray.GetLength(0) - 1];
                                object[] objArray1 = new object[] { str, ";", str2, "=", (int) pair.Value.m_result };
                                str = string.Concat(objArray1);
                                num2++;
                            }
                        }
                        if (num2 >= 3)
                        {
                            break;
                        }
                    }
                    FatalErrorMgr.Get().SetErrorCode("HS", str, null, null);
                    m_fatalErrorCodeSet = true;
                    request.m_errorCallback(info);
                }
            }
        }
    }

    public void InitNetCache()
    {
        ConnectAPI.RegisterThrottledPacketListener(new ConnectAPI.ThrottledPacketListener(this.OnPacketThrottled));
        this.RegisterNetCacheHandlers();
    }

    public void NetCacheChanged<T>()
    {
        System.Type key = typeof(T);
        int num = 0;
        this.m_changeRequests.TryGetValue(key, out num);
        num++;
        this.m_changeRequests[key] = num;
        if (num <= 1)
        {
            while (this.m_changeRequests[key] > 0)
            {
                this.NetCacheChangedImpl<T>();
                this.m_changeRequests[key] -= 1;
            }
        }
    }

    private void NetCacheChangedImpl<T>()
    {
        foreach (NetCacheBatchRequest request in this.m_cacheRequests.ToArray())
        {
            foreach (KeyValuePair<System.Type, Request> pair in request.m_requests)
            {
                if (pair.Key == typeof(T))
                {
                    this.NetCacheCheckRequest(request);
                    break;
                }
            }
        }
    }

    private void NetCacheCheckRequest(NetCacheBatchRequest request)
    {
        foreach (KeyValuePair<System.Type, Request> pair in request.m_requests)
        {
            if (!this.m_netCache.ContainsKey(pair.Key) || (this.m_netCache[pair.Key] == null))
            {
                return;
            }
        }
        request.m_canTimeout = false;
        request.m_callback();
    }

    private void NetCacheMakeBatchRequest(NetCacheBatchRequest batchRequest)
    {
        List<GetAccountInfo.Request> rl = new List<GetAccountInfo.Request>();
        foreach (KeyValuePair<System.Type, Request> pair in batchRequest.m_requests)
        {
            Request request = pair.Value;
            if (request == null)
            {
                Debug.LogError(string.Format("NetUseBatchRequest Null request for {0}...SKIP", request.m_type.Name));
            }
            else
            {
                if (request.m_reload)
                {
                    this.m_netCache[request.m_type] = null;
                }
                if ((!this.m_netCache.ContainsKey(request.m_type) || (this.m_netCache[request.m_type] == null)) && !this.m_inTransitRequests.Contains(request.m_type))
                {
                    request.m_result = RequestResult.PENDING;
                    this.m_inTransitRequests.Add(request.m_type);
                    rl.Add(m_typeMap[request.m_type]);
                }
            }
        }
        if (rl.Count > 0)
        {
            ConnectAPI.RequestNetCacheObjectList(rl);
        }
        this.m_cacheRequests.Add(batchRequest);
        this.NetCacheCheckRequest(batchRequest);
    }

    private void NetCacheReload_Internal(NetCacheBatchRequest request, System.Type type)
    {
        this.m_netCache[type] = null;
        this.NetCacheUse_Internal(request, type);
    }

    private void NetCacheUse_Internal(NetCacheBatchRequest request, System.Type type)
    {
        if ((request != null) && request.m_requests.ContainsKey(type))
        {
            Log.Bob.Print(string.Format("NetCache ...SKIP {0}", type.Name), new object[0]);
        }
        else if (this.m_netCache.ContainsKey(type) && (this.m_netCache[type] != null))
        {
            Log.Bob.Print(string.Format("NetCache ...USE {0}", type.Name), new object[0]);
        }
        else
        {
            Log.Bob.Print(string.Format("NetCache <<<GET {0}", type.Name), new object[0]);
            this.RequestNetCacheObject(type);
        }
    }

    private void OnAccountLicensesInfoResponse()
    {
        AccountLicensesInfoResponse accountLicensesInfoResponse = ConnectAPI.GetAccountLicensesInfoResponse();
        NetCacheAccountLicenses netCacheObject = new NetCacheAccountLicenses();
        foreach (AccountLicenseInfo info in accountLicensesInfoResponse.List)
        {
            netCacheObject.AccountLicenses[info.License] = info;
        }
        this.OnNetCacheObjReceived<NetCacheAccountLicenses>(netCacheObject);
    }

    private void OnAllHeroXP()
    {
        NetCacheHeroLevels allHeroXP = ConnectAPI.GetAllHeroXP();
        if (this.m_prevHeroLevels != null)
        {
            <OnAllHeroXP>c__AnonStorey31B storeyb = new <OnAllHeroXP>c__AnonStorey31B();
            using (List<HeroLevel>.Enumerator enumerator = allHeroXP.Levels.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    storeyb.newHeroLevel = enumerator.Current;
                    HeroLevel level = this.m_prevHeroLevels.Levels.Find(new Predicate<HeroLevel>(storeyb.<>m__139));
                    if (level != null)
                    {
                        if ((((storeyb.newHeroLevel != null) && (storeyb.newHeroLevel.CurrentLevel != null)) && (storeyb.newHeroLevel.CurrentLevel.Level != level.CurrentLevel.Level)) && (((storeyb.newHeroLevel.CurrentLevel.Level == 20) || (storeyb.newHeroLevel.CurrentLevel.Level == 30)) || (((storeyb.newHeroLevel.CurrentLevel.Level == 40) || (storeyb.newHeroLevel.CurrentLevel.Level == 50)) || (storeyb.newHeroLevel.CurrentLevel.Level == 60))))
                        {
                            if (storeyb.newHeroLevel.Class == TAG_CLASS.DRUID)
                            {
                                BnetPresenceMgr.Get().SetGameField(5, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                            else if (storeyb.newHeroLevel.Class == TAG_CLASS.HUNTER)
                            {
                                BnetPresenceMgr.Get().SetGameField(6, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                            else if (storeyb.newHeroLevel.Class == TAG_CLASS.MAGE)
                            {
                                BnetPresenceMgr.Get().SetGameField(7, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                            else if (storeyb.newHeroLevel.Class == TAG_CLASS.PALADIN)
                            {
                                BnetPresenceMgr.Get().SetGameField(8, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                            else if (storeyb.newHeroLevel.Class == TAG_CLASS.PRIEST)
                            {
                                BnetPresenceMgr.Get().SetGameField(9, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                            else if (storeyb.newHeroLevel.Class == TAG_CLASS.ROGUE)
                            {
                                BnetPresenceMgr.Get().SetGameField(10, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                            else if (storeyb.newHeroLevel.Class == TAG_CLASS.SHAMAN)
                            {
                                BnetPresenceMgr.Get().SetGameField(11, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                            else if (storeyb.newHeroLevel.Class == TAG_CLASS.WARLOCK)
                            {
                                BnetPresenceMgr.Get().SetGameField(12, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                            else if (storeyb.newHeroLevel.Class == TAG_CLASS.WARRIOR)
                            {
                                BnetPresenceMgr.Get().SetGameField(13, storeyb.newHeroLevel.CurrentLevel.Level);
                            }
                        }
                        storeyb.newHeroLevel.PrevLevel = level.CurrentLevel;
                    }
                }
            }
        }
        this.m_prevHeroLevels = allHeroXP;
        this.OnNetCacheObjReceived<NetCacheHeroLevels>(allHeroXP);
    }

    private void OnArcaneDustBalance()
    {
        NetCacheArcaneDustBalance netCacheObject = new NetCacheArcaneDustBalance {
            Balance = ConnectAPI.GetArcaneDustBalance()
        };
        this.OnNetCacheObjReceived<NetCacheArcaneDustBalance>(netCacheObject);
    }

    public void OnArcaneDustBalanceChanged(long balanceChange)
    {
        NetCacheArcaneDustBalance netObject = this.GetNetObject<NetCacheArcaneDustBalance>();
        netObject.Balance += balanceChange;
        this.NetCacheChanged<NetCacheArcaneDustBalance>();
    }

    private void OnBoosters()
    {
        this.OnNetCacheObjReceived<NetCacheBoosters>(ConnectAPI.GetBoosters());
    }

    private void OnBoosterTallies()
    {
        this.OnNetCacheObjReceived<NetCacheBoosterTallies>(ConnectAPI.GetBoosterTallies());
    }

    private void OnCardBacks()
    {
        this.OnNetCacheObjReceived<NetCacheCardBacks>(ConnectAPI.GetCardBacks());
    }

    private void OnCardValues()
    {
        NetCacheCardValues netCacheObject = new NetCacheCardValues();
        CardValues cardValues = ConnectAPI.GetCardValues();
        if ((cardValues != null) || Network.ShouldBeConnectedToAurora())
        {
            netCacheObject.CardNerfIndex = cardValues.CardNerfIndex;
            foreach (PegasusUtil.CardValue value2 in cardValues.Cards)
            {
                string str = GameUtils.TranslateDbIdToCardId(value2.Card.Asset);
                if (str == null)
                {
                    Debug.LogError(string.Format("NetCache.OnCardValues(): Cannot find card '{0}' in card manifest.  Confirm your card manifest matches your game server's database.", value2.Card.Asset));
                }
                CardDefinition key = new CardDefinition {
                    Name = str,
                    Premium = (TAG_PREMIUM) value2.Card.Premium
                };
                CardValue value3 = new CardValue {
                    Buy = value2.Buy,
                    Sell = value2.Sell,
                    Nerfed = value2.Nerfed
                };
                netCacheObject.Values.Add(key, value3);
            }
        }
        this.OnNetCacheObjReceived<NetCacheCardValues>(netCacheObject);
    }

    private void OnClientOptions()
    {
        NetCacheClientOptions netObject = this.GetNetObject<NetCacheClientOptions>();
        bool flag = netObject == null;
        if (flag)
        {
            netObject = new NetCacheClientOptions();
        }
        ConnectAPI.ReadClientOptions(netObject.ClientOptions);
        this.OnNetCacheObjReceived<NetCacheClientOptions>(netObject);
        if (flag)
        {
            OptionsMigration.UpgradeServerOptions();
        }
    }

    private void OnCollection()
    {
        this.OnNetCacheObjReceived<NetCacheCollection>(ConnectAPI.GetCollectionCardStacks());
    }

    private void OnDBAction()
    {
        Network.DBAction action = ConnectAPI.DBAction();
        if (action.Result != Network.DBAction.ResultType.SUCCESS)
        {
            Debug.LogError(string.Format("Unhandled dbAction {0} with error {1}", action.Action, action.Result));
        }
    }

    private void OnDeckLimit()
    {
        NetCacheDeckLimit netCacheObject = new NetCacheDeckLimit {
            DeckLimit = ConnectAPI.GetDeckLimit()
        };
        this.OnNetCacheObjReceived<NetCacheDeckLimit>(netCacheObject);
    }

    private void OnDecks()
    {
        this.OnNetCacheObjReceived<NetCacheDecks>(ConnectAPI.GetDeckHeaders());
    }

    private void OnFavoriteHeroesResponse()
    {
        FavoriteHeroesResponse favoriteHeroesResponse = ConnectAPI.GetFavoriteHeroesResponse();
        NetCacheFavoriteHeroes netCacheObject = new NetCacheFavoriteHeroes();
        foreach (FavoriteHero hero in favoriteHeroesResponse.FavoriteHeroes)
        {
            TAG_CLASS tag_class;
            if (!EnumUtils.TryCast<TAG_CLASS>(hero.ClassId, out tag_class))
            {
                Debug.LogWarning(string.Format("NetCache.OnFavoriteHeroesResponse() unrecognized hero class {0}", hero.ClassId));
            }
            else
            {
                TAG_PREMIUM tag_premium;
                if (!EnumUtils.TryCast<TAG_PREMIUM>(hero.Hero.Premium, out tag_premium))
                {
                    Debug.LogWarning(string.Format("NetCache.OnFavoriteHeroesResponse() unrecognized hero premium {0} for hero class {1}", hero.Hero.Premium, tag_class));
                }
                else
                {
                    CardDefinition definition = new CardDefinition {
                        Name = GameUtils.TranslateDbIdToCardId(hero.Hero.Asset),
                        Premium = tag_premium
                    };
                    netCacheObject.FavoriteHeroes[tag_class] = definition;
                }
            }
        }
        this.OnNetCacheObjReceived<NetCacheFavoriteHeroes>(netCacheObject);
    }

    private void OnFeaturesChanged()
    {
        NetCacheFeatures netCacheObject = ConnectAPI.GetFeatures();
        this.OnNetCacheObjReceived<NetCacheFeatures>(netCacheObject);
    }

    private void OnGamesInfo()
    {
        NetCacheGamesPlayed gamesInfo = ConnectAPI.GetGamesInfo();
        if (gamesInfo == null)
        {
            Debug.LogWarning("error getting games info");
        }
        else
        {
            this.OnNetCacheObjReceived<NetCacheGamesPlayed>(gamesInfo);
        }
    }

    private void OnGenericResponse()
    {
        Network.GenericResponse genericResponse = ConnectAPI.GetGenericResponse();
        if (genericResponse == null)
        {
            Debug.LogError(string.Format("NetCache - GenericResponse parse error", new object[0]));
        }
        else if (genericResponse.RequestId == 0xc9L)
        {
            System.Type type;
            if (!m_requestTypeMap.TryGetValue((GetAccountInfo.Request) genericResponse.RequestSubId, out type))
            {
                Debug.LogError(string.Format("NetCache - Ignoring unexpected request ID {0}", genericResponse.RequestId));
            }
            else
            {
                foreach (NetCacheBatchRequest request in this.m_cacheRequests.ToArray())
                {
                    if (request.m_requests.ContainsKey(type))
                    {
                        Network.GenericResponse.Result resultCode = genericResponse.ResultCode;
                        if (resultCode != Network.GenericResponse.Result.REQUEST_IN_PROCESS)
                        {
                            if (resultCode == Network.GenericResponse.Result.REQUEST_COMPLETE)
                            {
                                goto Label_00E0;
                            }
                            goto Label_0111;
                        }
                        if (request.m_requests[type].m_result == RequestResult.PENDING)
                        {
                            request.m_requests[type].m_result = RequestResult.IN_PROCESS;
                        }
                    }
                    continue;
                Label_00E0:
                    request.m_requests[type].m_result = RequestResult.GENERIC_COMPLETE;
                    Debug.LogWarning(string.Format("GenericResponse Success for request ID {0}", genericResponse.RequestId));
                    continue;
                Label_0111:
                    Debug.LogError(string.Format("Unhandled failure {0} for request ID {1}", genericResponse.ResultCode, genericResponse.RequestId));
                    request.m_requests[type].m_result = RequestResult.ERROR;
                    ErrorInfo info = new ErrorInfo {
                        Error = ErrorCode.SERVER,
                        ServerError = (uint) genericResponse.ResultCode,
                        RequestingFunction = request.m_requestFunc,
                        RequestedTypes = new Map<System.Type, Request>(request.m_requests),
                        RequestStackTrace = request.m_requestStackTrace
                    };
                    FatalErrorMgr.Get().SetErrorCode("HS", "CG" + genericResponse.ResultCode.ToString(), genericResponse.RequestId.ToString(), genericResponse.RequestSubId.ToString());
                    request.m_errorCallback(info);
                }
            }
        }
    }

    private void OnGoldBalance()
    {
        NetCacheGoldBalance goldBalance = ConnectAPI.GetGoldBalance();
        this.OnNetCacheObjReceived<NetCacheGoldBalance>(goldBalance);
        foreach (DelGoldBalanceListener listener in this.m_goldBalanceListeners.ToArray())
        {
            listener(goldBalance);
        }
    }

    private void OnHearthstoneUnavailable(bool gamePacket)
    {
        Network.UnavailableReason hearthstoneUnavailable = Network.GetHearthstoneUnavailable(gamePacket);
        Debug.Log("Hearthstone Unavailable!  Reason: " + hearthstoneUnavailable.mainReason);
        string mainReason = hearthstoneUnavailable.mainReason;
        if (mainReason != null)
        {
            int num;
            if (<>f__switch$map2B == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                dictionary.Add("VERSION", 0);
                dictionary.Add("OFFLINE", 1);
                <>f__switch$map2B = dictionary;
            }
            if (<>f__switch$map2B.TryGetValue(mainReason, out num))
            {
                if (num == 0)
                {
                    ErrorParams parms = new ErrorParams {
                        m_message = GameStrings.Format("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UPGRADE", new object[0])
                    };
                    if (Error.HAS_APP_STORE != null)
                    {
                        parms.m_redirectToStore = true;
                    }
                    Error.AddFatal(parms);
                    return;
                }
                if (num == 1)
                {
                    Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_OFFLINE");
                    return;
                }
            }
        }
        Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN");
    }

    private void OnHearthstoneUnavailableGame()
    {
        this.OnHearthstoneUnavailable(true);
    }

    private void OnHearthstoneUnavailableUtil()
    {
        this.OnHearthstoneUnavailable(false);
    }

    private void OnLastGameInfo()
    {
        NetCacheDisconnectedGame disconnectedGameInfo = ConnectAPI.GetDisconnectedGameInfo();
        this.OnNetCacheObjReceived<NetCacheDisconnectedGame>(disconnectedGameInfo);
    }

    private void OnMedalHistory()
    {
        this.OnNetCacheObjReceived<NetCacheMedalHistory>(ConnectAPI.GetMedalHistory());
    }

    private void OnMedalInfo()
    {
        NetCacheMedalInfo medalInfo = ConnectAPI.GetMedalInfo();
        if (this.m_previousMedalInfo != null)
        {
            medalInfo.PreviousMedalInfo = this.m_previousMedalInfo.Clone();
        }
        this.m_previousMedalInfo = medalInfo;
        this.OnNetCacheObjReceived<NetCacheMedalInfo>(medalInfo);
    }

    private void OnNetCacheObjReceived<T>(T netCacheObject)
    {
        List<System.Action> list;
        System.Type type = typeof(T);
        Log.Bob.Print(string.Format("OnNetCacheObjReceived SAVE --> {0}", type.Name), new object[0]);
        this.UpdateRequestNeedState(type, RequestResult.DATA_COMPLETE);
        this.m_netCache[type] = netCacheObject;
        this.m_inTransitRequests.Remove(type);
        this.NetCacheChanged<T>();
        if (this.m_updatedListeners.TryGetValue(type, out list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                System.Action action = list[i];
                action();
            }
        }
    }

    private void OnNotSoMassiveLoginReply()
    {
        NotSoMassiveLoginReply notSoMassiveLoginReply = ConnectAPI.GetNotSoMassiveLoginReply();
        if (notSoMassiveLoginReply != null)
        {
            SpecialEventManager.Get().InitEventTiming(notSoMassiveLoginReply.SpecialEventTiming);
        }
        NetCacheNotSoMassiveLogin netCacheObject = new NetCacheNotSoMassiveLogin {
            Packet = notSoMassiveLoginReply
        };
        if (notSoMassiveLoginReply.HasTavernBrawls)
        {
            this.OnNetCacheObjReceived<NetCacheTavernBrawlInfo>(new NetCacheTavernBrawlInfo(notSoMassiveLoginReply.TavernBrawls));
            if (notSoMassiveLoginReply.TavernBrawls.HasCurrentTavernBrawl && (notSoMassiveLoginReply.TavernBrawls.CurrentTavernBrawl.MyRecord != null))
            {
                this.OnNetCacheObjReceived<NetCacheTavernBrawlRecord>(new NetCacheTavernBrawlRecord(notSoMassiveLoginReply.TavernBrawls.CurrentTavernBrawl.MyRecord));
            }
        }
        this.OnNetCacheObjReceived<NetCacheNotSoMassiveLogin>(netCacheObject);
    }

    private void OnPacketThrottled(int packetID, long retryMillis)
    {
        if (packetID == 0xc9)
        {
            DateTime time = DateTime.Now.AddMilliseconds((double) retryMillis);
            foreach (NetCacheBatchRequest request in this.m_cacheRequests)
            {
                request.m_timeAdded = time;
            }
        }
    }

    private void OnPlayerRecords()
    {
        this.OnNetCacheObjReceived<NetCachePlayerRecords>(ConnectAPI.GetPlayerRecords());
    }

    private void OnPlayQueue()
    {
        this.OnNetCacheObjReceived<NetCachePlayQueue>(ConnectAPI.GetPlayQueue());
        object[] args = new object[] { Get().GetNetObject<NetCachePlayQueue>().GameType };
        Log.Bob.Print("play queue {0}", args);
    }

    private void OnProfileNotices()
    {
        List<ProfileNotice> profileNotices = ConnectAPI.GetProfileNotices();
        if (<>f__am$cacheF == null)
        {
            <>f__am$cacheF = obj => obj.Type == ProfileNotice.NoticeType.GAINED_MEDAL;
        }
        if (profileNotices.Find(<>f__am$cacheF) != null)
        {
            this.m_previousMedalInfo = null;
            NetCacheMedalInfo netObject = this.GetNetObject<NetCacheMedalInfo>();
            if (netObject != null)
            {
                netObject.PreviousMedalInfo = null;
            }
        }
        List<ProfileNotice> newNotices = this.FindNewNotices(profileNotices);
        NetCacheProfileNotices netCacheObject = new NetCacheProfileNotices();
        netCacheObject.Notices.AddRange(profileNotices);
        this.OnNetCacheObjReceived<NetCacheProfileNotices>(netCacheObject);
        DelNewNoticesListener[] listenerArray = this.m_newNoticesListeners.ToArray();
        object[] args = new object[] { newNotices.Count, listenerArray.Length };
        Log.Rachelle.Print("NetCache.OnProfileNotices() sending {0} new notices to {1} listeners", args);
        foreach (DelNewNoticesListener listener in listenerArray)
        {
            listener(newNotices);
        }
    }

    private void OnProfileProgress()
    {
        this.OnNetCacheObjReceived<NetCacheProfileProgress>(ConnectAPI.GetProfileProgress());
    }

    private void OnRewardProgress()
    {
        this.OnNetCacheObjReceived<NetCacheRewardProgress>(ConnectAPI.GetRewardProgress());
    }

    private void OnTavernBrawlInfoResponse()
    {
        TavernBrawlInfo tavernBrawlInfoPacket = ConnectAPI.GetTavernBrawlInfoPacket();
        this.OnNetCacheObjReceived<NetCacheTavernBrawlInfo>(new NetCacheTavernBrawlInfo(tavernBrawlInfoPacket));
        if (tavernBrawlInfoPacket.HasCurrentTavernBrawl && (tavernBrawlInfoPacket.CurrentTavernBrawl.MyRecord != null))
        {
            this.OnNetCacheObjReceived<NetCacheTavernBrawlRecord>(new NetCacheTavernBrawlRecord(tavernBrawlInfoPacket.CurrentTavernBrawl.MyRecord));
        }
    }

    private void OnTavernBrawlRecordResponse()
    {
        TavernBrawlPlayerRecord tavernBrawlRecordPacket = ConnectAPI.GetTavernBrawlRecordPacket();
        this.OnNetCacheObjReceived<NetCacheTavernBrawlRecord>(new NetCacheTavernBrawlRecord(tavernBrawlRecordPacket));
    }

    public void RefreshNetObject<T>()
    {
        this.RequestNetCacheObject(typeof(T));
    }

    public void RegisterBoosterTallyUpdate(NetCacheCallback callback)
    {
        this.RegisterBoosterTallyUpdate(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterBoosterTallyUpdate(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterFriendChallenge---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterBoosterTallyUpdate));
        batchRequest.AddRequest(new Request(typeof(NetCacheBoosterTallies), true));
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterCollectionManager(NetCacheCallback callback)
    {
        this.RegisterCollectionManager(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterCollectionManager(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterCollectionManager---", new object[0]);
        NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterCollectionManager));
        this.AddCollectionManagerToRequest(ref request);
        this.NetCacheMakeBatchRequest(request);
    }

    public void RegisterFeatures(NetCacheCallback callback)
    {
        this.RegisterFeatures(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterFeatures(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterFeatures---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterFeatures));
        batchRequest.AddRequest(new Request(typeof(NetCacheFeatures), false));
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterFriendChallenge(NetCacheCallback callback)
    {
        this.RegisterFriendChallenge(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterFriendChallenge(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterFriendChallenge---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterFriendChallenge));
        batchRequest.AddRequest(new Request(typeof(NetCacheProfileProgress), false));
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterGoldBalanceListener(DelGoldBalanceListener listener)
    {
        if (!this.m_goldBalanceListeners.Contains(listener))
        {
            this.m_goldBalanceListeners.Add(listener);
        }
    }

    private void RegisterNetCacheHandlers()
    {
        Network network = Network.Get();
        network.RegisterNetHandler(PegasusUtil.DBAction.PacketID.ID, new Network.NetHandler(this.OnDBAction), null);
        network.RegisterNetHandler(PegasusUtil.GenericResponse.PacketID.ID, new Network.NetHandler(this.OnGenericResponse), null);
        network.RegisterNetHandler(BoosterList.PacketID.ID, new Network.NetHandler(this.OnBoosters), null);
        network.RegisterNetHandler(BoosterTallyList.PacketID.ID, new Network.NetHandler(this.OnBoosterTallies), null);
        network.RegisterNetHandler(Collection.PacketID.ID, new Network.NetHandler(this.OnCollection), null);
        network.RegisterNetHandler(DeckList.PacketID.ID, new Network.NetHandler(this.OnDecks), null);
        network.RegisterNetHandler(PegasusUtil.MedalHistory.PacketID.ID, new Network.NetHandler(this.OnMedalHistory), null);
        network.RegisterNetHandler(MedalInfo.PacketID.ID, new Network.NetHandler(this.OnMedalInfo), null);
        network.RegisterNetHandler(ProfileDeckLimit.PacketID.ID, new Network.NetHandler(this.OnDeckLimit), null);
        network.RegisterNetHandler(ProfileProgress.PacketID.ID, new Network.NetHandler(this.OnProfileProgress), null);
        network.RegisterNetHandler(GamesInfo.PacketID.ID, new Network.NetHandler(this.OnGamesInfo), null);
        network.RegisterNetHandler(PegasusUtil.ProfileNotices.PacketID.ID, new Network.NetHandler(this.OnProfileNotices), null);
        network.RegisterNetHandler(ClientOptions.PacketID.ID, new Network.NetHandler(this.OnClientOptions), null);
        network.RegisterNetHandler(Disconnected.PacketID.ID, new Network.NetHandler(this.OnLastGameInfo), null);
        network.RegisterNetHandler(CardValues.PacketID.ID, new Network.NetHandler(this.OnCardValues), null);
        network.RegisterNetHandler(ArcaneDustBalance.PacketID.ID, new Network.NetHandler(this.OnArcaneDustBalance), null);
        network.RegisterNetHandler(GoldBalance.PacketID.ID, new Network.NetHandler(this.OnGoldBalance), null);
        network.RegisterNetHandler(GuardianVars.PacketID.ID, new Network.NetHandler(this.OnFeaturesChanged), null);
        network.RegisterNetHandler(PlayerRecords.PacketID.ID, new Network.NetHandler(this.OnPlayerRecords), null);
        network.RegisterNetHandler(RewardProgress.PacketID.ID, new Network.NetHandler(this.OnRewardProgress), null);
        network.RegisterNetHandler(HeroXP.PacketID.ID, new Network.NetHandler(this.OnAllHeroXP), null);
        network.RegisterNetHandler(PlayQueue.PacketID.ID, new Network.NetHandler(this.OnPlayQueue), null);
        network.RegisterNetHandler(CardBacks.PacketID.ID, new Network.NetHandler(this.OnCardBacks), null);
        network.RegisterNetHandler(NotSoMassiveLoginReply.PacketID.ID, new Network.NetHandler(this.OnNotSoMassiveLoginReply), null);
        network.RegisterNetHandler(TavernBrawlInfo.PacketID.ID, new Network.NetHandler(this.OnTavernBrawlInfoResponse), null);
        network.RegisterNetHandler(TavernBrawlPlayerRecordResponse.PacketID.ID, new Network.NetHandler(this.OnTavernBrawlRecordResponse), null);
        network.RegisterNetHandler(FavoriteHeroesResponse.PacketID.ID, new Network.NetHandler(this.OnFavoriteHeroesResponse), null);
        network.RegisterNetHandler(AccountLicensesInfoResponse.PacketID.ID, new Network.NetHandler(this.OnAccountLicensesInfoResponse), null);
        network.RegisterNetHandler(Deadend.PacketID.ID, new Network.NetHandler(this.OnHearthstoneUnavailableGame), null);
        network.RegisterNetHandler(DeadendUtil.PacketID.ID, new Network.NetHandler(this.OnHearthstoneUnavailableUtil), null);
    }

    public void RegisterNewNoticesListener(DelNewNoticesListener listener)
    {
        if (!this.m_newNoticesListeners.Contains(listener))
        {
            this.m_newNoticesListeners.Add(listener);
        }
    }

    public void RegisterReconnectMgr(NetCacheCallback callback)
    {
        this.RegisterReconnectMgr(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterReconnectMgr(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterReconnectMgr---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterReconnectMgr));
        batchRequest.AddRequest(new Request(typeof(NetCacheDisconnectedGame), false));
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterScreenBox(NetCacheCallback callback)
    {
        this.RegisterScreenBox(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenBox(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenBox---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenBox));
        batchRequest.AddRequests(new List<Request> { new Request(typeof(NetCacheBoosters), 0), new Request(typeof(NetCacheClientOptions), 0), new Request(typeof(NetCacheProfileProgress), 0), new Request(typeof(NetCacheFeatures), 0), new Request(typeof(NetCacheMedalInfo), 0), new Request(typeof(NetCacheProfileNotices), 0) });
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterScreenCollectionManager(NetCacheCallback callback)
    {
        this.RegisterScreenCollectionManager(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenCollectionManager(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenCollectionManager---", new object[0]);
        NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenCollectionManager));
        this.AddCollectionManagerToRequest(ref request);
        this.AddRandomDeckMakerToRequest(ref request);
        request.AddRequests(new List<Request> { new Request(typeof(NetCacheFeatures), 0), new Request(typeof(NetCacheHeroLevels), 0) });
        this.NetCacheMakeBatchRequest(request);
    }

    public void RegisterScreenEndOfGame(NetCacheCallback callback)
    {
        this.RegisterScreenEndOfGame(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenEndOfGame(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenEndOfGame---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenEndOfGame));
        batchRequest.AddRequests(new List<Request> { new Request(typeof(NetCacheMedalHistory), 0), new Request(typeof(NetCacheRewardProgress), 0), new Request(typeof(NetCacheMedalInfo), 1), new Request(typeof(NetCacheGamesPlayed), 1), new Request(typeof(NetCacheProfileNotices), 1), new Request(typeof(NetCachePlayerRecords), 1), new Request(typeof(NetCacheHeroLevels), 1) });
        if ((GameMgr.Get() != null) && (GameMgr.Get().GetGameType() == GameType.GT_TAVERNBRAWL))
        {
            batchRequest.AddRequest(new Request(typeof(NetCacheTavernBrawlRecord), true));
        }
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterScreenForge(NetCacheCallback callback)
    {
        this.RegisterScreenForge(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenForge(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenForge---", new object[0]);
        NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenForge));
        this.AddCollectionManagerToRequest(ref request);
        request.AddRequests(new List<Request> { new Request(typeof(NetCacheFeatures), 0), new Request(typeof(NetCacheHeroLevels), 0) });
        this.NetCacheMakeBatchRequest(request);
    }

    public void RegisterScreenFriendly(NetCacheCallback callback)
    {
        this.RegisterScreenFriendly(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenFriendly(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenFriendly---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenFriendly));
        batchRequest.AddRequests(new List<Request> { new Request(typeof(NetCacheDecks), 0), new Request(typeof(NetCacheHeroLevels), 0) });
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterScreenLogin(NetCacheCallback callback)
    {
        this.RegisterScreenLogin(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenLogin(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenLogin---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenLogin));
        batchRequest.AddRequests(new List<Request> { new Request(typeof(NetCacheNotSoMassiveLogin), 0), new Request(typeof(NetCacheProfileNotices), 0), new Request(typeof(NetCacheProfileProgress), 0), new Request(typeof(NetCacheRewardProgress), 0), new Request(typeof(NetCachePlayerRecords), 0), new Request(typeof(NetCacheGoldBalance), 0), new Request(typeof(NetCacheHeroLevels), 0), new Request(typeof(NetCacheCardBacks), 0), new Request(typeof(NetCacheFavoriteHeroes), 0), new Request(typeof(NetCacheAccountLicenses), 0), new Request(typeof(NetCacheBoosterTallies), 1), new Request(typeof(NetCacheClientOptions), 1) });
        this.NetCacheMakeBatchRequest(batchRequest);
        TavernBrawlManager.Get().IsRefreshingTavernBrawlInfo = true;
    }

    public void RegisterScreenPackOpening(NetCacheCallback callback)
    {
        this.RegisterScreenPackOpening(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenPackOpening(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenPackOpening---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenPackOpening));
        batchRequest.AddRequest(new Request(typeof(NetCacheBoosters), false));
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterScreenPractice(NetCacheCallback callback)
    {
        this.RegisterScreenPractice(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenPractice(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenPractice---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenPractice));
        batchRequest.AddRequests(new List<Request> { new Request(typeof(NetCacheDecks), 0), new Request(typeof(NetCacheFeatures), 0), new Request(typeof(NetCacheHeroLevels), 0), new Request(typeof(NetCacheRewardProgress), 0) });
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterScreenQuestLog(NetCacheCallback callback)
    {
        this.RegisterScreenQuestLog(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenQuestLog(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenQuestLog---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenQuestLog));
        batchRequest.AddRequests(new List<Request> { new Request(typeof(NetCacheMedalInfo), 0), new Request(typeof(NetCacheHeroLevels), 0), new Request(typeof(NetCachePlayerRecords), 0), new Request(typeof(NetCacheProfileProgress), 1) });
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterScreenTourneys(NetCacheCallback callback)
    {
        this.RegisterScreenTourneys(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterScreenTourneys(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterScreenTourneys---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterScreenTourneys));
        batchRequest.AddRequests(new List<Request> { new Request(typeof(NetCacheMedalHistory), 0), new Request(typeof(NetCacheMedalInfo), 1), new Request(typeof(NetCachePlayerRecords), 0), new Request(typeof(NetCacheDecks), 0), new Request(typeof(NetCacheFeatures), 0), new Request(typeof(NetCacheHeroLevels), 0), new Request(typeof(NetCachePlayQueue), 1) });
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterTutorialEndGameScreen(NetCacheCallback callback)
    {
        this.RegisterTutorialEndGameScreen(callback, new ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void RegisterTutorialEndGameScreen(NetCacheCallback callback, ErrorCallback errorCallback)
    {
        Log.Bob.Print("---RegisterTutorialEndGameScreen---", new object[0]);
        NetCacheBatchRequest batchRequest = new NetCacheBatchRequest(callback, errorCallback, new RequestFunc(this.RegisterTutorialEndGameScreen));
        batchRequest.AddRequests(new List<Request> { new Request(typeof(NetCacheProfileProgress), 0), new Request(typeof(NetCacheProfileNotices), 1) });
        this.NetCacheMakeBatchRequest(batchRequest);
    }

    public void RegisterUpdatedListener(System.Type type, System.Action listener)
    {
        List<System.Action> list;
        if (!this.m_updatedListeners.TryGetValue(type, out list))
        {
            list = new List<System.Action>();
            this.m_updatedListeners[type] = list;
        }
        list.Add(listener);
    }

    public void ReloadNetObject<T>()
    {
        this.NetCacheReload_Internal(null, typeof(T));
    }

    public void RemoveGoldBalanceListener(DelGoldBalanceListener listener)
    {
        this.m_goldBalanceListeners.Remove(listener);
    }

    public void RemoveNewNoticesListener(DelNewNoticesListener listener)
    {
        this.m_newNoticesListeners.Remove(listener);
    }

    public bool RemoveNotice(long ID)
    {
        <RemoveNotice>c__AnonStorey31A storeya = new <RemoveNotice>c__AnonStorey31A {
            ID = ID
        };
        NetCacheProfileNotices notices = this.m_netCache[typeof(NetCacheProfileNotices)] as NetCacheProfileNotices;
        if (notices == null)
        {
            Debug.LogWarning(string.Format("NetCache.RemoveNotice({0}) - profileNotices is null", storeya.ID));
            return false;
        }
        if (notices.Notices == null)
        {
            Debug.LogWarning(string.Format("NetCache.RemoveNotice({0}) - profileNotices.Notices is null", storeya.ID));
            return false;
        }
        ProfileNotice item = notices.Notices.Find(new Predicate<ProfileNotice>(storeya.<>m__138));
        if (item == null)
        {
            return false;
        }
        notices.Notices.Remove(item);
        return true;
    }

    public void RemoveUpdatedListener(System.Type type, System.Action listener)
    {
        List<System.Action> list;
        if (this.m_updatedListeners.TryGetValue(type, out list))
        {
            list.Remove(listener);
        }
    }

    private void RequestNetCacheObject(System.Type type)
    {
        if (!this.m_inTransitRequests.Contains(type))
        {
            this.m_inTransitRequests.Add(type);
            ConnectAPI.RequestNetCacheObject(m_typeMap[type]);
        }
    }

    public void SetBoolOption(ServerOption type, bool val)
    {
        ConnectAPI.SetClientOptionBool((int) type, val);
        this.SetClientOption(type, new ClientOptionBool(val));
    }

    private void SetClientOption(ServerOption type, ClientOptionBase newVal)
    {
        NetCacheClientOptions options = (NetCacheClientOptions) this.m_netCache[typeof(NetCacheClientOptions)];
        options.ClientOptions[type] = newVal;
        this.NetCacheChanged<NetCacheClientOptions>();
    }

    public void SetFloatOption(ServerOption type, float val)
    {
        ConnectAPI.SetClientOptionFloat((int) type, val);
        this.SetClientOption(type, new ClientOptionFloat(val));
    }

    public void SetIntOption(ServerOption type, int val)
    {
        ConnectAPI.SetClientOptionInt((int) type, val);
        this.SetClientOption(type, new ClientOptionInt(val));
    }

    public void SetLongOption(ServerOption type, long val)
    {
        ConnectAPI.SetClientOptionLong((int) type, val);
        this.SetClientOption(type, new ClientOptionLong(val));
    }

    public void SetULongOption(ServerOption type, ulong val)
    {
        ConnectAPI.SetClientOptionULong((int) type, val);
        this.SetClientOption(type, new ClientOptionULong(val));
    }

    public static void ShowError(ErrorInfo info, string localizationKey, params object[] localizationArgs)
    {
        Error.AddFatalLoc(localizationKey, localizationArgs);
        Debug.LogError(GetInternalErrorMessage(info, true));
    }

    public void UnloadNetObject<T>()
    {
        System.Type type = typeof(T);
        this.m_netCache[type] = null;
    }

    public void UnregisterNetCacheHandler(NetCacheCallback handler)
    {
        foreach (NetCacheBatchRequest request in this.m_cacheRequests)
        {
            if (request.m_callback == handler)
            {
                this.m_cacheRequests.Remove(request);
                break;
            }
        }
    }

    private void UpdateRequestNeedState(System.Type type, RequestResult result)
    {
        foreach (NetCacheBatchRequest request in this.m_cacheRequests)
        {
            if (request.m_requests.ContainsKey(type))
            {
                request.m_requests[type].m_result = result;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <FindNewNotices>c__AnonStorey31C
    {
        internal NetCache.ProfileNotice receivedNotice;

        internal bool <>m__13B(NetCache.ProfileNotice obj)
        {
            return (obj.NoticeID == this.receivedNotice.NoticeID);
        }
    }

    [CompilerGenerated]
    private sealed class <OnAllHeroXP>c__AnonStorey31B
    {
        internal NetCache.HeroLevel newHeroLevel;

        internal bool <>m__139(NetCache.HeroLevel obj)
        {
            return (obj.Class == this.newHeroLevel.Class);
        }
    }

    [CompilerGenerated]
    private sealed class <RemoveNotice>c__AnonStorey31A
    {
        internal long ID;

        internal bool <>m__138(NetCache.ProfileNotice obj)
        {
            return (obj.NoticeID == this.ID);
        }
    }

    public class BoosterCard
    {
        public BoosterCard()
        {
            this.Def = new NetCache.CardDefinition();
        }

        public long Date { get; set; }

        public NetCache.CardDefinition Def { get; set; }
    }

    public class BoosterStack
    {
        public int Count { get; set; }

        public int Id { get; set; }
    }

    public class BoosterTally
    {
        public int Count { get; set; }

        public int Id { get; set; }

        public bool IsOpen { get; set; }

        public Network.BoosterSource Source { get; set; }

        public bool WasBought { get; set; }
    }

    public class CardDefinition
    {
        public override bool Equals(object obj)
        {
            NetCache.CardDefinition definition = obj as NetCache.CardDefinition;
            return (((definition != null) && (this.Premium == definition.Premium)) && this.Name.Equals(definition.Name));
        }

        public override int GetHashCode()
        {
            return (this.Name.GetHashCode() + this.Premium);
        }

        public override string ToString()
        {
            return string.Format("[CardDefinition: Name={0}, Premium={1}]", this.Name, this.Premium);
        }

        public string Name { get; set; }

        public TAG_PREMIUM Premium { get; set; }
    }

    public class CardStack
    {
        public CardStack()
        {
            this.Def = new NetCache.CardDefinition();
        }

        public int Count { get; set; }

        public long Date { get; set; }

        public NetCache.CardDefinition Def { get; set; }

        public int NumSeen { get; set; }
    }

    public class CardValue
    {
        public int Buy { get; set; }

        public bool Nerfed { get; set; }

        public int Sell { get; set; }
    }

    public class ClientOptionBase
    {
    }

    public class ClientOptionBool : NetCache.ClientOptionBase
    {
        public ClientOptionBool(bool val)
        {
            this.OptionValue = val;
        }

        public bool OptionValue { get; set; }
    }

    public class ClientOptionFloat : NetCache.ClientOptionBase
    {
        public ClientOptionFloat(float val)
        {
            this.OptionValue = val;
        }

        public float OptionValue { get; set; }
    }

    public class ClientOptionInt : NetCache.ClientOptionBase
    {
        public ClientOptionInt(int val)
        {
            this.OptionValue = val;
        }

        public int OptionValue { get; set; }
    }

    public class ClientOptionLong : NetCache.ClientOptionBase
    {
        public ClientOptionLong(long val)
        {
            this.OptionValue = val;
        }

        public long OptionValue { get; set; }
    }

    public class ClientOptionULong : NetCache.ClientOptionBase
    {
        public ClientOptionULong(ulong val)
        {
            this.OptionValue = val;
        }

        public ulong OptionValue { get; set; }
    }

    public class DeckHeader
    {
        public override string ToString()
        {
            object[] args = new object[] { this.ID, this.Name, this.Hero, this.HeroPremium, this.HeroPower, this.IsTourneyValid, this.Type, this.CardBack, this.CardBackOverridden, this.HeroOverridden };
            return string.Format("[DeckHeader: ID={0}, Name={1}, Hero={2}, HeroPremium={3}, HeroPower={4}, IsTourneyValid={5}, DeckType={6}, CardBack={7}, CardBackOverridden={8} HeroOverridden={9}]", args);
        }

        public int CardBack { get; set; }

        public bool CardBackOverridden { get; set; }

        public string Hero { get; set; }

        public bool HeroOverridden { get; set; }

        public string HeroPower { get; set; }

        public TAG_PREMIUM HeroPremium { get; set; }

        public long ID { get; set; }

        public bool IsTourneyValid { get; set; }

        public string Name { get; set; }

        public int SeasonId { get; set; }

        public DeckType Type { get; set; }
    }

    public delegate void DelGoldBalanceListener(NetCache.NetCacheGoldBalance balance);

    public delegate void DelNewNoticesListener(List<NetCache.ProfileNotice> newNotices);

    public delegate void ErrorCallback(NetCache.ErrorInfo info);

    public enum ErrorCode
    {
        NONE,
        TIMEOUT,
        SERVER
    }

    public class ErrorInfo
    {
        public NetCache.ErrorCode Error { get; set; }

        public Map<System.Type, NetCache.Request> RequestedTypes { get; set; }

        public NetCache.RequestFunc RequestingFunction { get; set; }

        public string RequestStackTrace { get; set; }

        public uint ServerError { get; set; }
    }

    public class HeroLevel
    {
        public HeroLevel()
        {
            this.Class = TAG_CLASS.INVALID;
            this.PrevLevel = null;
            this.CurrentLevel = new LevelInfo();
            this.NextReward = null;
        }

        public override string ToString()
        {
            object[] args = new object[] { this.Class, this.PrevLevel, this.CurrentLevel, this.NextReward };
            return string.Format("[HeroLevel: Class={0}, PrevLevel={1}, CurrentLevel={2}, NextReward={3}]", args);
        }

        public TAG_CLASS Class { get; set; }

        public LevelInfo CurrentLevel { get; set; }

        public NextLevelReward NextReward { get; set; }

        public LevelInfo PrevLevel { get; set; }

        public class LevelInfo
        {
            public LevelInfo()
            {
                this.Level = 0;
                this.MaxLevel = 60;
                this.XP = 0L;
                this.MaxXP = 0L;
            }

            public bool IsMaxLevel()
            {
                return (this.Level == this.MaxLevel);
            }

            public override string ToString()
            {
                return string.Format("[LevelInfo: Level={0}, XP={1}, MaxXP={2}]", this.Level, this.XP, this.MaxXP);
            }

            public int Level { get; set; }

            public int MaxLevel { get; set; }

            public long MaxXP { get; set; }

            public long XP { get; set; }
        }

        public class NextLevelReward
        {
            public NextLevelReward()
            {
                this.Level = 0;
                this.Reward = null;
            }

            public override string ToString()
            {
                return string.Format("[NextLevelReward: Level={0}, Reward={1}]", this.Level, this.Reward);
            }

            public int Level { get; set; }

            public RewardData Reward { get; set; }
        }
    }

    public class MedalHistory
    {
        public long Date { get; set; }

        public int LegendRank { get; set; }

        public int Season { get; set; }

        public int StarLevel { get; set; }

        public int Stars { get; set; }
    }

    public class NetCacheAccountLicenses
    {
        public NetCacheAccountLicenses()
        {
            this.AccountLicenses = new Map<long, AccountLicenseInfo>();
        }

        public Map<long, AccountLicenseInfo> AccountLicenses { get; set; }
    }

    public class NetCacheArcaneDustBalance
    {
        public long Balance { get; set; }
    }

    private class NetCacheBatchRequest
    {
        public NetCache.NetCacheCallback m_callback;
        public bool m_canTimeout = true;
        public NetCache.ErrorCallback m_errorCallback;
        public NetCache.RequestFunc m_requestFunc;
        public Map<System.Type, NetCache.Request> m_requests = new Map<System.Type, NetCache.Request>();
        public string m_requestStackTrace;
        public DateTime m_timeAdded = DateTime.Now;

        public NetCacheBatchRequest(NetCache.NetCacheCallback reply, NetCache.ErrorCallback errorCallback, NetCache.RequestFunc requestFunc)
        {
            this.m_callback = reply;
            this.m_errorCallback = errorCallback;
            this.m_requestFunc = requestFunc;
            this.m_requestStackTrace = Environment.StackTrace;
        }

        public void AddRequest(NetCache.Request r)
        {
            if (!this.m_requests.ContainsKey(r.m_type))
            {
                this.m_requests.Add(r.m_type, r);
            }
        }

        public void AddRequests(List<NetCache.Request> requests)
        {
            foreach (NetCache.Request request in requests)
            {
                this.AddRequest(request);
            }
        }
    }

    public class NetCacheBoosters
    {
        public NetCacheBoosters()
        {
            this.BoosterStacks = new List<NetCache.BoosterStack>();
        }

        public NetCache.BoosterStack GetBoosterStack(int id)
        {
            <GetBoosterStack>c__AnonStorey31D storeyd = new <GetBoosterStack>c__AnonStorey31D {
                id = id
            };
            return this.BoosterStacks.Find(new Predicate<NetCache.BoosterStack>(storeyd.<>m__13C));
        }

        public int GetTotalNumBoosters()
        {
            int num = 0;
            foreach (NetCache.BoosterStack stack in this.BoosterStacks)
            {
                num += stack.Count;
            }
            return num;
        }

        public List<NetCache.BoosterStack> BoosterStacks { get; set; }

        [CompilerGenerated]
        private sealed class <GetBoosterStack>c__AnonStorey31D
        {
            internal int id;

            internal bool <>m__13C(NetCache.BoosterStack obj)
            {
                return (obj.Id == this.id);
            }
        }
    }

    public class NetCacheBoosterTallies
    {
        public NetCacheBoosterTallies()
        {
            this.BoosterTallies = new List<NetCache.BoosterTally>();
        }

        public List<NetCache.BoosterTally> BoosterTallies { get; set; }
    }

    public delegate void NetCacheCallback();

    public class NetCacheCardBacks
    {
        public NetCacheCardBacks()
        {
            this.CardBacks = new HashSet<int>();
            this.CardBacks.Add(0);
        }

        public HashSet<int> CardBacks { get; set; }

        public int DefaultCardBack { get; set; }
    }

    public class NetCacheCardValues
    {
        public NetCacheCardValues()
        {
            this.CardNerfIndex = 0;
            this.Values = new Map<NetCache.CardDefinition, NetCache.CardValue>();
        }

        public int CardNerfIndex { get; set; }

        public Map<NetCache.CardDefinition, NetCache.CardValue> Values { get; set; }
    }

    public class NetCacheClientOptions
    {
        public NetCacheClientOptions()
        {
            this.ClientOptions = new Map<ServerOption, NetCache.ClientOptionBase>();
        }

        public Map<ServerOption, NetCache.ClientOptionBase> ClientOptions { get; set; }
    }

    public class NetCacheCollection
    {
        public Map<TAG_CLASS, int> BasicCardsUnlockedPerClass = new Map<TAG_CLASS, int>();
        public int TotalCardsOwned;

        public NetCacheCollection()
        {
            this.Stacks = new List<NetCache.CardStack>();
            IEnumerator enumerator = Enum.GetValues(typeof(TAG_CLASS)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    TAG_CLASS current = (TAG_CLASS) ((int) enumerator.Current);
                    this.BasicCardsUnlockedPerClass[current] = 0;
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

        public List<NetCache.CardStack> Stacks { get; set; }
    }

    public class NetCacheDeckLimit
    {
        public int DeckLimit { get; set; }
    }

    public class NetCacheDecks
    {
        public NetCacheDecks()
        {
            this.Decks = new List<NetCache.DeckHeader>();
        }

        public List<NetCache.DeckHeader> Decks { get; set; }
    }

    public class NetCacheDisconnectedGame
    {
        public BattleNet.GameServerInfo ServerInfo { get; set; }
    }

    public class NetCacheFavoriteHeroes
    {
        public NetCacheFavoriteHeroes()
        {
            this.FavoriteHeroes = new Map<TAG_CLASS, NetCache.CardDefinition>();
        }

        public Map<TAG_CLASS, NetCache.CardDefinition> FavoriteHeroes { get; set; }
    }

    public class NetCacheFeatures
    {
        public NetCacheFeatures()
        {
            this.Games = new CacheGames();
            this.Collection = new CacheCollection();
            this.Store = new CacheStore();
            this.Heroes = new CacheHeroes();
        }

        public CacheCollection Collection { get; set; }

        public CacheGames Games { get; set; }

        public CacheHeroes Heroes { get; set; }

        public CacheStore Store { get; set; }

        public class CacheCollection
        {
            public bool Crafting { get; set; }

            public bool Manager { get; set; }
        }

        public class CacheGames
        {
            public bool Casual { get; set; }

            public bool Forge { get; set; }

            public bool Friendly { get; set; }

            public bool Practice { get; set; }

            public int ShowUserUI { get; set; }

            public bool TavernBrawl { get; set; }

            public bool Tournament { get; set; }
        }

        public class CacheHeroes
        {
            public bool Hunter { get; set; }

            public bool Mage { get; set; }

            public bool Paladin { get; set; }

            public bool Priest { get; set; }

            public bool Rogue { get; set; }

            public bool Shaman { get; set; }

            public bool Warlock { get; set; }

            public bool Warrior { get; set; }
        }

        public class CacheStore
        {
            public bool BattlePay { get; set; }

            public bool BuyWithGold { get; set; }

            public bool Store { get; set; }
        }
    }

    public class NetCacheGamesPlayed
    {
        public int FreeRewardProgress { get; set; }

        public int GamesLost { get; set; }

        public int GamesStarted { get; set; }

        public int GamesWon { get; set; }
    }

    public class NetCacheGoldBalance
    {
        public long GetTotal()
        {
            return (this.CappedBalance + this.BonusBalance);
        }

        public long BonusBalance { get; set; }

        public long Cap { get; set; }

        public long CappedBalance { get; set; }

        public long CapWarning { get; set; }
    }

    public class NetCacheHeroLevels
    {
        public NetCacheHeroLevels()
        {
            this.Levels = new List<NetCache.HeroLevel>();
        }

        public override string ToString()
        {
            string str = "[START NetCacheHeroLevels]\n";
            foreach (NetCache.HeroLevel level in this.Levels)
            {
                str = str + string.Format("{0}\n", level);
            }
            return (str + "[END NetCacheHeroLevels]");
        }

        public List<NetCache.HeroLevel> Levels { get; set; }
    }

    public class NetCacheLastLogin
    {
        public long LastLogin { get; set; }
    }

    public class NetCacheMedalHistory
    {
        public NetCacheMedalHistory()
        {
            this.Medals = new List<NetCache.MedalHistory>();
        }

        public List<NetCache.MedalHistory> Medals { get; set; }
    }

    public class NetCacheMedalInfo
    {
        public NetCache.NetCacheMedalInfo Clone()
        {
            NetCache.NetCacheMedalInfo info = new NetCache.NetCacheMedalInfo();
            info.CopyFrom(this);
            return info;
        }

        public void CopyFrom(NetCache.NetCacheMedalInfo other)
        {
            this.SeasonWins = other.SeasonWins;
            this.Stars = other.Stars;
            this.Streak = other.Streak;
            this.StarLevel = other.StarLevel;
            this.BestStarLevel = other.BestStarLevel;
            this.CanLoseStars = other.CanLoseStars;
            this.CanLoseLevel = other.CanLoseLevel;
            this.GainLevelStars = other.GainLevelStars;
            this.StartStars = other.StartStars;
            this.LegendIndex = other.LegendIndex;
        }

        public override string ToString()
        {
            object[] args = new object[] { this.SeasonWins, this.Stars, this.Streak, this.StarLevel, this.CanLoseLevel, this.GainLevelStars, this.StartStars, this.LegendIndex, this.CanLoseStars };
            return string.Format("[NetCacheMedalInfo] SeasonWins={0} Stars={1} Streak={2} StarLevel={3} CanLoseStars={8} CanLoseLevel={4} GainLevelStars={5} StartStars={6} LegendIndex={7}", args);
        }

        public int BestStarLevel { get; set; }

        public bool CanLoseLevel { get; set; }

        public bool CanLoseStars { get; set; }

        public int GainLevelStars { get; set; }

        public int LegendIndex { get; set; }

        public NetCache.NetCacheMedalInfo PreviousMedalInfo { get; set; }

        public int SeasonWins { get; set; }

        public int StarLevel { get; set; }

        public int Stars { get; set; }

        public int StartStars { get; set; }

        public int Streak { get; set; }
    }

    public class NetCacheNotSoMassiveLogin
    {
        public NotSoMassiveLoginReply Packet { get; set; }
    }

    public class NetCachePlayerRecords
    {
        public NetCachePlayerRecords()
        {
            this.Records = new List<NetCache.PlayerRecord>();
        }

        public List<NetCache.PlayerRecord> Records { get; set; }
    }

    public class NetCachePlayQueue
    {
        public BnetGameType GameType { get; set; }
    }

    public class NetCacheProfileNotices
    {
        public NetCacheProfileNotices()
        {
            this.Notices = new List<NetCache.ProfileNotice>();
        }

        public List<NetCache.ProfileNotice> Notices { get; set; }
    }

    public class NetCacheProfileProgress
    {
        public AdventureOption[] AdventureOptions { get; set; }

        public int BestForgeWins { get; set; }

        public TutorialProgress CampaignProgress { get; set; }

        public int DisplayBanner { get; set; }

        public long LastForgeDate { get; set; }
    }

    public class NetCacheRewardProgress
    {
        public int GoldPerReward { get; set; }

        public int MaxGoldPerDay { get; set; }

        public int MaxHeroLevel { get; set; }

        public long NextQuestCancelDate { get; set; }

        public int PackRewardId { get; set; }

        public int Season { get; set; }

        public long SeasonEndDate { get; set; }

        public float SpecialEventTimingMod { get; set; }

        public int WinsPerGold { get; set; }

        public int XPSoloLimit { get; set; }
    }

    public class NetCacheSubscribeResponse
    {
        public ulong FeaturesSupported;
        public ulong KeepAliveDelay;
        public ulong RequestMaxWaitSecs;
        public ulong Route;
    }

    public class NetCacheTavernBrawlInfo
    {
        public NetCacheTavernBrawlInfo(TavernBrawlInfo info)
        {
            this.Info = info;
        }

        public TavernBrawlInfo Info { get; set; }
    }

    public class NetCacheTavernBrawlRecord
    {
        public NetCacheTavernBrawlRecord(TavernBrawlPlayerRecord record)
        {
            this.Record = record;
        }

        public TavernBrawlPlayerRecord Record { get; set; }
    }

    public class PlayerRecord
    {
        public int Data { get; set; }

        public int Losses { get; set; }

        public GameType RecordType { get; set; }

        public int Ties { get; set; }

        public int Wins { get; set; }
    }

    public abstract class ProfileNotice
    {
        private NoticeType m_type;

        protected ProfileNotice(NoticeType init)
        {
            this.m_type = init;
            this.NoticeID = 0L;
            this.Origin = NoticeOrigin.UNKNOWN;
            this.OriginData = 0L;
            this.Date = 0L;
        }

        public long Date { get; set; }

        public long NoticeID { get; set; }

        public NoticeOrigin Origin { get; set; }

        public long OriginData { get; set; }

        public NoticeType Type
        {
            get
            {
                return this.m_type;
            }
        }

        public enum NoticeOrigin
        {
            ACCOUNT_LICENSE_FLAGS = 0x15,
            ACHIEVEMENT = 7,
            ACK = 6,
            ADVENTURE_FLAGS = 0x13,
            ADVENTURE_PROGRESS = 0x12,
            BETA_REIMBURSE = 2,
            BLIZZCON = 13,
            DISCONNECTED_GAME = 15,
            EVENT = 14,
            FORGE = 3,
            IGR = 0x11,
            LEVEL_UP = 8,
            OUT_OF_BAND_LICENSE = 0x10,
            PRECON_DECK = 5,
            PURCHASE_CANCELED = 12,
            PURCHASE_COMPLETE = 10,
            PURCHASE_FAILED = 11,
            SEASON = 1,
            TAVERN_BRAWL_REWARD = 20,
            TOURNEY = 4,
            UNKNOWN = -1
        }

        public enum NoticeType
        {
            ACCOUNT_LICENSE = 0x10,
            ADVENTURE_PROGRESS = 14,
            BONUS_STARS = 12,
            DISCONNECTED_GAME = 4,
            GAINED_MEDAL = 1,
            HERO_LEVEL_UP = 15,
            PRECON_DECK = 5,
            PURCHASE = 10,
            REWARD_BOOSTER = 2,
            REWARD_CARD = 3,
            REWARD_CARD_BACK = 11,
            REWARD_DUST = 6,
            REWARD_FORGE = 8,
            REWARD_GOLD = 9,
            REWARD_MOUNT = 7
        }
    }

    public class ProfileNoticeAcccountLicense : NetCache.ProfileNotice
    {
        public ProfileNoticeAcccountLicense() : base(NetCache.ProfileNotice.NoticeType.ACCOUNT_LICENSE)
        {
        }

        public long CasID { get; set; }

        public long License { get; set; }
    }

    public class ProfileNoticeAdventureProgress : NetCache.ProfileNotice
    {
        public ProfileNoticeAdventureProgress() : base(NetCache.ProfileNotice.NoticeType.ADVENTURE_PROGRESS)
        {
        }

        public ulong? Flags { get; set; }

        public int? Progress { get; set; }

        public int Wing { get; set; }
    }

    public class ProfileNoticeBonusStars : NetCache.ProfileNotice
    {
        public ProfileNoticeBonusStars() : base(NetCache.ProfileNotice.NoticeType.BONUS_STARS)
        {
        }

        public int StarLevel { get; set; }

        public int Stars { get; set; }
    }

    public class ProfileNoticeDisconnectedGame : NetCache.ProfileNotice
    {
        public ProfileNoticeDisconnectedGame() : base(NetCache.ProfileNotice.NoticeType.DISCONNECTED_GAME)
        {
        }

        public PegasusShared.ProfileNoticeDisconnectedGameResult.GameResult GameResult { get; set; }

        public PegasusShared.GameType GameType { get; set; }

        public int MissionId { get; set; }

        public ProfileNoticeDisconnectedGameResult.PlayerResult OpponentResult { get; set; }

        public int PlayerIndex { get; set; }

        public ProfileNoticeDisconnectedGameResult.PlayerResult YourResult { get; set; }
    }

    public class ProfileNoticeLevelUp : NetCache.ProfileNotice
    {
        public ProfileNoticeLevelUp() : base(NetCache.ProfileNotice.NoticeType.HERO_LEVEL_UP)
        {
        }

        public int HeroClass { get; set; }

        public int NewLevel { get; set; }
    }

    public class ProfileNoticeMedal : NetCache.ProfileNotice
    {
        public ProfileNoticeMedal() : base(NetCache.ProfileNotice.NoticeType.GAINED_MEDAL)
        {
        }

        public int BestStarLevel { get; set; }

        public Network.RewardChest Chest { get; set; }

        public int LegendRank { get; set; }

        public int StarLevel { get; set; }
    }

    public class ProfileNoticePreconDeck : NetCache.ProfileNotice
    {
        public ProfileNoticePreconDeck() : base(NetCache.ProfileNotice.NoticeType.PRECON_DECK)
        {
        }

        public long DeckID { get; set; }

        public int HeroAsset { get; set; }
    }

    public class ProfileNoticePurchase : NetCache.ProfileNotice
    {
        public ProfileNoticePurchase() : base(NetCache.ProfileNotice.NoticeType.PURCHASE)
        {
        }

        public override string ToString()
        {
            object[] args = new object[] { base.NoticeID, base.Type, base.Origin, base.OriginData, base.Date, this.ProductID, this.Data, this.CurrencyType };
            return string.Format("[ProfileNoticePurchase: NoticeID={0}, Type={1}, Origin={2}, OriginData={3}, Date={4} ProductID='{5}', Data={6} Currency={7}]", args);
        }

        public Currency CurrencyType { get; set; }

        public long Data { get; set; }

        public string ProductID { get; set; }
    }

    public class ProfileNoticeRewardBooster : NetCache.ProfileNotice
    {
        public ProfileNoticeRewardBooster() : base(NetCache.ProfileNotice.NoticeType.REWARD_BOOSTER)
        {
            this.Id = 0;
            this.Count = 0;
        }

        public int Count { get; set; }

        public int Id { get; set; }
    }

    public class ProfileNoticeRewardCard : NetCache.ProfileNotice
    {
        public ProfileNoticeRewardCard() : base(NetCache.ProfileNotice.NoticeType.REWARD_CARD)
        {
        }

        public string CardID { get; set; }

        public TAG_PREMIUM Premium { get; set; }

        public int Quantity { get; set; }
    }

    public class ProfileNoticeRewardCardBack : NetCache.ProfileNotice
    {
        public ProfileNoticeRewardCardBack() : base(NetCache.ProfileNotice.NoticeType.REWARD_CARD_BACK)
        {
        }

        public override string ToString()
        {
            object[] args = new object[] { base.NoticeID, base.Type, base.Origin, base.OriginData, base.Date, this.CardBackID };
            return string.Format("[ProfileNoticePurchase: NoticeID={0}, Type={1}, Origin={2}, OriginData={3}, Date={4} CardBackID={5}]", args);
        }

        public int CardBackID { get; set; }
    }

    public class ProfileNoticeRewardDust : NetCache.ProfileNotice
    {
        public ProfileNoticeRewardDust() : base(NetCache.ProfileNotice.NoticeType.REWARD_DUST)
        {
        }

        public int Amount { get; set; }
    }

    public class ProfileNoticeRewardForge : NetCache.ProfileNotice
    {
        public ProfileNoticeRewardForge() : base(NetCache.ProfileNotice.NoticeType.REWARD_FORGE)
        {
        }

        public int Quantity { get; set; }
    }

    public class ProfileNoticeRewardGold : NetCache.ProfileNotice
    {
        public ProfileNoticeRewardGold() : base(NetCache.ProfileNotice.NoticeType.REWARD_GOLD)
        {
        }

        public int Amount { get; set; }
    }

    public class ProfileNoticeRewardMount : NetCache.ProfileNotice
    {
        public ProfileNoticeRewardMount() : base(NetCache.ProfileNotice.NoticeType.REWARD_MOUNT)
        {
        }

        public int MountID { get; set; }
    }

    public class Request
    {
        public bool m_reload;
        public NetCache.RequestResult m_result;
        public System.Type m_type;
        public const bool RELOAD = true;

        public Request(System.Type rt, bool rl = false)
        {
            this.m_type = rt;
            this.m_reload = rl;
            this.m_result = NetCache.RequestResult.UNKNOWN;
        }
    }

    public delegate void RequestFunc(NetCache.NetCacheCallback callback, NetCache.ErrorCallback errorCallback);

    public enum RequestResult
    {
        UNKNOWN,
        PENDING,
        IN_PROCESS,
        GENERIC_COMPLETE,
        DATA_COMPLETE,
        ERROR
    }
}

