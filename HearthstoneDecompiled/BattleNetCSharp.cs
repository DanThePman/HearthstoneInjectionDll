using bnet;
using bnet.protocol;
using bnet.protocol.authentication;
using bnet.protocol.connection;
using bnet.protocol.notification;
using PegasusShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using WTCG.BI;

public class BattleNetCSharp : IBattleNet
{
    private AccountAPI m_accountAPI;
    private List<BattleNetAPI> m_apiList = new List<BattleNetAPI>();
    private string m_auroraEnvironment;
    private bool m_auroraVersionAsInt = true;
    private int m_auroraVersionInt;
    private string m_auroraVersionName = 0x2acc.ToString();
    private string m_auroraVersionSource = "undefined";
    private string m_auroraVersionString = "undefined";
    private AuthenticationAPI m_authenticationAPI;
    private List<BattleNet.BnetEvent> m_bnetEvents = new List<BattleNet.BnetEvent>();
    private ChallengeAPI m_challengeAPI;
    private ChannelAPI m_channelAPI;
    private uint m_connectedRegion = uint.MaxValue;
    private ServiceDescriptor m_connectionService = new ConnectionService();
    private ConnectionState m_connectionState;
    private List<BnetErrorInfo> m_errorEvents = new List<BnetErrorInfo>();
    private List<ServiceDescriptor> m_exportedServices = new List<ServiceDescriptor>();
    private FriendsAPI m_friendAPI;
    private GamesAPI m_gamesAPI;
    private List<ServiceDescriptor> m_importedServices = new List<ServiceDescriptor>();
    private bool m_initialized;
    private readonly long m_keepAliveIntervalMilliseconds = 0x4e20L;
    private LocalStorageAPI m_localStorageAPI;
    private BattleNetLogSource m_logSource = new BattleNetLogSource("Main");
    private NotificationAPI m_notificationAPI;
    private Map<string, NotificationHandler> m_notificationHandlers = new Map<string, NotificationHandler>();
    private ServiceDescriptor m_notificationListenerService = new NotificationListenerService();
    private ServiceDescriptor m_notificationService = new RPCServices.NotificationService();
    private PartyAPI m_partyAPI;
    private PresenceAPI m_presenceAPI;
    private ProfanityAPI m_profanityAPI;
    private ResourcesAPI m_resourcesAPI;
    private RPCConnection m_rpcConnection;
    private long m_serverTimeDeltaUTCSeconds;
    private long m_serverTimeUTCAtConnectMicroseconds;
    private bool m_shutdownNeedsImplentationWarningAlreadyGiven;
    private Map<ConnectionState, AuroraStateHandler> m_stateHandlers = new Map<ConnectionState, AuroraStateHandler>();
    private readonly DateTime m_unixEpoch = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private string m_userEmailAddress;
    private WhisperAPI m_whisperAPI;
    private string s_platformName = "iOS";
    private const string s_programName = "WTCG";

    public BattleNetCSharp()
    {
        this.m_friendAPI = new FriendsAPI(this);
        this.m_presenceAPI = new PresenceAPI(this);
        this.m_channelAPI = new ChannelAPI(this);
        this.m_gamesAPI = new GamesAPI(this);
        this.m_partyAPI = new PartyAPI(this);
        this.m_challengeAPI = new ChallengeAPI(this);
        this.m_whisperAPI = new WhisperAPI(this);
        this.m_notificationAPI = new NotificationAPI(this);
        this.m_accountAPI = new AccountAPI(this);
        this.m_authenticationAPI = new AuthenticationAPI(this);
        this.m_localStorageAPI = new LocalStorageAPI(this);
        this.m_resourcesAPI = new ResourcesAPI(this);
        this.m_profanityAPI = new ProfanityAPI(this);
        this.m_notificationHandlers.Add("GQ_ENTRY", new NotificationHandler(this.m_gamesAPI.QueueEntryHandler));
        this.m_notificationHandlers.Add("GQ_UPDATE", new NotificationHandler(this.m_gamesAPI.QueueUpdateHandler));
        this.m_notificationHandlers.Add("GQ_EXIT", new NotificationHandler(this.m_gamesAPI.QueueExitHandler));
        this.m_notificationHandlers.Add("MM_START", new NotificationHandler(this.m_gamesAPI.MatchMakerStartHandler));
        this.m_notificationHandlers.Add("MM_END", new NotificationHandler(this.m_gamesAPI.MatchMakerEndHandler));
        this.m_notificationHandlers.Add("G_RESULT", new NotificationHandler(this.m_gamesAPI.GameEntryHandler));
        this.m_notificationHandlers.Add("WHISPER", new NotificationHandler(this.m_whisperAPI.OnWhisper));
        this.m_notificationHandlers.Add("WTCG.UtilNotificationMessage", new NotificationHandler(this.<BattleNetCSharp>m__132));
        this.m_stateHandlers.Add(ConnectionState.Connect, new AuroraStateHandler(this.AuroraStateHandler_Connect));
        this.m_stateHandlers.Add(ConnectionState.InitRPC, new AuroraStateHandler(this.AuroraStateHandler_InitRPC));
        this.m_stateHandlers.Add(ConnectionState.WaitForInitRPC, new AuroraStateHandler(this.AuroraStateHandler_WaitForInitRPC));
        this.m_stateHandlers.Add(ConnectionState.Logon, new AuroraStateHandler(this.AuroraStateHandler_Logon));
        this.m_stateHandlers.Add(ConnectionState.WaitForLogon, new AuroraStateHandler(this.AuroraStateHandler_WaitForLogon));
        this.m_stateHandlers.Add(ConnectionState.WaitForGameAccountSelect, new AuroraStateHandler(this.AuroraStateHandler_WaitForGameAccountSelect));
        this.m_stateHandlers.Add(ConnectionState.WaitForAPIToInitialize, new AuroraStateHandler(this.AuroraStateHandler_WaitForAPIToInitialize));
        this.m_stateHandlers.Add(ConnectionState.Ready, new AuroraStateHandler(this.AuroraStateHandler_Ready));
        this.m_stateHandlers.Add(ConnectionState.Disconnected, new AuroraStateHandler(this.AuroraStateHandler_Unhandled));
        this.m_stateHandlers.Add(ConnectionState.Error, new AuroraStateHandler(this.AuroraStateHandler_Unhandled));
        this.m_apiList.Add(this.m_friendAPI);
        this.m_apiList.Add(this.m_presenceAPI);
        this.m_apiList.Add(this.m_channelAPI);
        this.m_apiList.Add(this.m_gamesAPI);
        this.m_apiList.Add(this.m_partyAPI);
        this.m_apiList.Add(this.m_challengeAPI);
        this.m_apiList.Add(this.m_whisperAPI);
        this.m_apiList.Add(this.m_notificationAPI);
        this.m_apiList.Add(this.m_accountAPI);
        this.m_apiList.Add(this.m_authenticationAPI);
        this.m_apiList.Add(this.m_localStorageAPI);
        this.m_apiList.Add(this.m_resourcesAPI);
        this.m_apiList.Add(this.m_profanityAPI);
    }

    [CompilerGenerated]
    private void <BattleNetCSharp>m__132(bnet.protocol.notification.Notification n)
    {
        this.m_notificationAPI.OnNotification("WTCG.UtilNotificationMessage", n);
    }

    public void AcceptFriendlyChallenge(ref BattleNet.DllEntityId partyId)
    {
        this.m_partyAPI.AcceptFriendlyChallenge(partyId.ToProtocol());
    }

    public void AcceptPartyInvite(ulong invitationId)
    {
        this.m_partyAPI.AcceptPartyInvite(invitationId);
    }

    public void AnswerChallenge(ulong challengeID, string answer)
    {
        this.m_challengeAPI.AnswerChallenge(challengeID, answer);
    }

    public void ApplicationWasPaused()
    {
        this.m_logSource.LogWarning("Application was paused.");
        ConnectAPI.ApplicationPaused();
        if (this.m_rpcConnection != null)
        {
            this.m_rpcConnection.Update();
        }
    }

    public void ApplicationWasUnpaused()
    {
        this.m_logSource.LogWarning("Application was unpaused.");
    }

    public void AppQuit()
    {
        ConnectAPI.CloseAll();
        this.Reset();
    }

    public void AssignPartyRole(BattleNet.DllEntityId partyId, BattleNet.DllEntityId memberId, uint roleId)
    {
        this.m_partyAPI.AssignPartyRole(partyId.ToProtocol(), memberId.ToProtocol(), roleId);
    }

    public void AuroraStateHandler_Connect()
    {
    }

    public void AuroraStateHandler_InitRPC()
    {
        this.m_importedServices.Clear();
        this.m_exportedServices.Clear();
        ConnectRequest message = new ConnectRequest();
        this.m_importedServices.Add(this.m_authenticationAPI.AuthServerService);
        this.m_importedServices.Add(this.m_gamesAPI.GameUtilityService);
        this.m_importedServices.Add(this.m_gamesAPI.GameMasterService);
        this.m_importedServices.Add(this.m_notificationService);
        this.m_importedServices.Add(this.m_presenceAPI.PresenceService);
        this.m_importedServices.Add(this.m_channelAPI.ChannelService);
        this.m_importedServices.Add(this.m_channelAPI.ChannelOwnerService);
        this.m_importedServices.Add(this.m_channelAPI.ChannelInvitationService);
        this.m_importedServices.Add(this.m_friendAPI.FriendsService);
        this.m_importedServices.Add(this.m_challengeAPI.ChallengeService);
        this.m_importedServices.Add(this.m_accountAPI.AccountService);
        this.m_importedServices.Add(this.m_resourcesAPI.ResourcesService);
        this.m_exportedServices.Add(this.m_authenticationAPI.AuthClientService);
        this.m_exportedServices.Add(this.m_gamesAPI.GameMasterSubscriberService);
        this.m_exportedServices.Add(this.m_gamesAPI.GameFactorySubscribeService);
        this.m_exportedServices.Add(this.m_notificationListenerService);
        this.m_exportedServices.Add(this.m_channelAPI.ChannelSubscriberService);
        this.m_exportedServices.Add(this.m_channelAPI.ChannelInvitationNotifyService);
        this.m_exportedServices.Add(this.m_friendAPI.FriendsNotifyService);
        this.m_exportedServices.Add(this.m_challengeAPI.ChallengeNotifyService);
        this.m_exportedServices.Add(this.m_accountAPI.AccountNotifyService);
        message.SetBindRequest(this.CreateBindRequest(this.m_importedServices, this.m_exportedServices));
        this.m_rpcConnection.QueueRequest(this.m_connectionService.Id, 1, message, new RPCContextDelegate(this.OnConnectCallback), 0);
        this.SwitchToState(ConnectionState.WaitForInitRPC);
    }

    public void AuroraStateHandler_Logon()
    {
        BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_INFO, BIReport.TelemetryEvent.EVENT_LOGIN_REQUEST, MobileDeviceLocale.GetCurrentRegionId());
        this.m_logSource.LogDebug("Sending Logon request");
        LogonRequest message = this.CreateLogonRequest();
        this.m_rpcConnection.QueueRequest(this.m_authenticationAPI.AuthServerService.Id, 1, message, null, 0);
        this.SwitchToState(ConnectionState.WaitForLogon);
    }

    public void AuroraStateHandler_Ready()
    {
    }

    public void AuroraStateHandler_Unhandled()
    {
        this.m_logSource.LogError("Unhandled Aurora State");
    }

    public void AuroraStateHandler_WaitForAPIToInitialize()
    {
        if (this.m_friendAPI.IsInitialized)
        {
            this.SwitchToState(ConnectionState.Ready);
        }
    }

    public void AuroraStateHandler_WaitForGameAccountSelect()
    {
    }

    public void AuroraStateHandler_WaitForInitRPC()
    {
    }

    public void AuroraStateHandler_WaitForLogon()
    {
        if (this.m_authenticationAPI.AuthenticationFailure())
        {
            this.EnqueueErrorInfo(BnetFeature.Bnet, BnetFeatureEvent.Bnet_OnDisconnected, BattleNetErrors.ERROR_NO_AUTH, 0);
        }
    }

    public int BattleNetStatus()
    {
        switch (this.m_connectionState)
        {
            case ConnectionState.Disconnected:
                return 0;

            case ConnectionState.Connect:
            case ConnectionState.InitRPC:
            case ConnectionState.WaitForInitRPC:
            case ConnectionState.Logon:
            case ConnectionState.WaitForLogon:
            case ConnectionState.WaitForGameAccountSelect:
            case ConnectionState.WaitForAPIToInitialize:
                return 1;

            case ConnectionState.Ready:
                return 4;

            case ConnectionState.Error:
                return 3;
        }
        this.m_logSource.LogError("Unknown Battle.Net Status");
        return 0;
    }

    public void CancelChallenge(ulong challengeID)
    {
        this.m_challengeAPI.CancelChallenge(challengeID);
    }

    public void CancelFindGame()
    {
        this.m_gamesAPI.CancelFindGame();
    }

    public bool CheckWebAuth(out string url)
    {
        url = null;
        if ((this.m_challengeAPI != null) && this.InState(ConnectionState.WaitForLogon))
        {
            ExternalChallenge nextExternalChallenge = this.m_challengeAPI.GetNextExternalChallenge();
            if (nextExternalChallenge != null)
            {
                Log.JMac.Print("Challenge url is " + nextExternalChallenge.URL, new object[0]);
                string token = Vars.Key("Aurora.VerifyWebCredentials").GetStr(null);
                if (token == null)
                {
                    string message = "No webToken defined!  Provide a token in config -> Aurora.VerifyWebCredentials";
                    Debug.LogError(message);
                    this.m_logSource.LogError(message);
                }
                else
                {
                    this.ProvideWebAuthToken(token);
                    object[] objArray1 = new object[] { token };
                    this.m_logSource.LogDebug("Calling ProvideWebAuthToken with={0}", objArray1);
                    Debug.Log("Providing WebAuth token (from config -> Aurora.VerifyWebCredentials).  If this repeats, then web token is invalid!");
                    return false;
                }
                url = nextExternalChallenge.URL;
                object[] args = new object[] { url };
                this.m_logSource.LogDebug("Delivering a challenge url={0}", args);
                return true;
            }
        }
        return false;
    }

    public void ClearBnetEvents()
    {
        this.m_bnetEvents.Clear();
    }

    public void ClearChallenges()
    {
        this.m_challengeAPI.ClearChallenges();
    }

    public void ClearErrors()
    {
        this.m_errorEvents.Clear();
    }

    public void ClearFriendsUpdates()
    {
        this.m_friendAPI.ClearFriendsUpdates();
    }

    public void ClearNotifications()
    {
        this.m_notificationAPI.ClearNotifications();
    }

    public void ClearPartyAttribute(BattleNet.DllEntityId partyId, string attributeKey)
    {
        this.m_partyAPI.ClearPartyAttribute(partyId.ToProtocol(), attributeKey);
    }

    public void ClearPartyListenerEvents()
    {
        this.m_partyAPI.ClearPartyListenerEvents();
    }

    public void ClearPartyUpdates()
    {
        this.m_partyAPI.ClearPartyUpdates();
    }

    public void ClearPresence()
    {
        this.m_presenceAPI.ClearPresence();
    }

    public void ClearWhispers()
    {
        this.m_whisperAPI.ClearWhispers();
    }

    public void CloseAurora()
    {
        if (this.m_rpcConnection != null)
        {
            this.m_rpcConnection.Disconnect();
        }
        this.SwitchToState(ConnectionState.Disconnected);
    }

    public void ConnectAurora(string address, int port, SslParameters sslParams)
    {
        object[] args = new object[] { address, port };
        this.m_logSource.LogInfo("Sending connection request to {0}:{1}", args);
        object[] objArray2 = new object[] { !this.m_auroraVersionAsInt ? this.m_auroraVersionString : this.m_auroraVersionInt.ToString() };
        this.m_logSource.LogDebug("Aurora version is {0}", objArray2);
        this.m_rpcConnection = new RPCConnection();
        this.m_connectionService.Id = 0;
        this.m_rpcConnection.serviceHelper.AddImportedService(this.m_connectionService.Id, this.m_connectionService);
        this.m_rpcConnection.serviceHelper.AddExportedService(this.m_connectionService.Id, this.m_connectionService);
        this.m_rpcConnection.RegisterServiceMethodListener(this.m_connectionService.Id, 4, new RPCContextDelegate(this.HandleForceDisconnectRequest));
        this.m_rpcConnection.RegisterServiceMethodListener(this.m_connectionService.Id, 3, new RPCContextDelegate(this.HandleEchoRequest));
        this.m_rpcConnection.SetOnConnectHandler(new RPCConnection.OnConnectHandler(this.OnConnectHandlerCallback));
        this.m_rpcConnection.SetOnDisconnectHandler(new RPCConnection.OnDisconectHandler(this.OnDisconectHandlerCallback));
        this.m_rpcConnection.Connect(address, port, sslParams);
        this.SwitchToState(ConnectionState.InitRPC);
    }

    private BindRequest CreateBindRequest(List<ServiceDescriptor> imports, List<ServiceDescriptor> exports)
    {
        BindRequest request = new BindRequest();
        foreach (ServiceDescriptor descriptor in imports)
        {
            request.AddImportedServiceHash(descriptor.Hash);
        }
        uint num = 0;
        foreach (ServiceDescriptor descriptor2 in exports)
        {
            descriptor2.Id = ++num;
            BoundService val = new BoundService();
            val.SetId(descriptor2.Id);
            val.SetHash(descriptor2.Hash);
            request.AddExportedService(val);
            this.m_rpcConnection.serviceHelper.AddExportedService(descriptor2.Id, descriptor2);
            object[] args = new object[] { descriptor2.Id, descriptor2.Name };
            this.m_logSource.LogDebug("Exporting service id={0} name={1}", args);
        }
        return request;
    }

    private LogonRequest CreateLogonRequest()
    {
        LogonRequest request = new LogonRequest();
        request.SetProgram("WTCG");
        request.SetLocale(Localization.GetLocaleName());
        request.SetPlatform(this.s_platformName);
        request.SetVersion(this.m_auroraVersionName);
        request.SetApplicationVersion(1);
        request.SetPublicComputer(false);
        request.SetAllowLogonQueueNotifications(true);
        string val = "Hearthstone/";
        string str2 = val;
        object[] objArray1 = new object[] { str2, "4.1.", 0x2acc, " (" };
        val = string.Concat(objArray1);
        if (PlatformSettings.OS == OSCategory.iOS)
        {
            val = val + "iOS;";
        }
        else if (PlatformSettings.OS == OSCategory.Android)
        {
            val = val + "Android;";
        }
        else if (PlatformSettings.OS == OSCategory.PC)
        {
            val = val + "PC;";
        }
        else if (PlatformSettings.OS == OSCategory.Mac)
        {
            val = val + "Mac;";
        }
        str2 = val;
        object[] objArray2 = new object[] { 
            str2, SystemInfo.deviceModel, ";", SystemInfo.deviceType, ";", SystemInfo.deviceUniqueIdentifier, ";", SystemInfo.graphicsDeviceID, ";", SystemInfo.graphicsDeviceName, ";", SystemInfo.graphicsDeviceVendor, ";", SystemInfo.graphicsDeviceVendorID, ";", SystemInfo.graphicsDeviceVersion, 
            ";", SystemInfo.graphicsMemorySize, ";", SystemInfo.graphicsShaderLevel, ";", SystemInfo.npotSupport, ";", SystemInfo.operatingSystem, ";", SystemInfo.processorCount, ";", SystemInfo.processorType, ";", SystemInfo.supportedRenderTargetCount, ";", SystemInfo.supports3DTextures, 
            ";", SystemInfo.supportsAccelerometer, ";", SystemInfo.supportsComputeShaders, ";", SystemInfo.supportsGyroscope, ";", SystemInfo.supportsImageEffects, ";", SystemInfo.supportsInstancing, ";", SystemInfo.supportsLocationService, ";", SystemInfo.supportsRenderTextures, ";", SystemInfo.supportsRenderToCubemap, 
            ";", SystemInfo.supportsShadows, ";", SystemInfo.supportsSparseTextures, ";", SystemInfo.supportsStencil, ";", SystemInfo.supportsVibration, ";", SystemInfo.systemMemorySize, ";", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf), ";", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444), ";", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth), 
            ";", SystemInfo.graphicsDeviceVersion.StartsWith("Metal"), ";", Screen.currentResolution.width, ";", Screen.currentResolution.height, ";", Screen.dpi, ";"
         };
        val = string.Concat(objArray2);
        if ((PlatformSettings.OS == OSCategory.iOS) || (PlatformSettings.OS == OSCategory.Android))
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                val = val + "Phone;";
            }
            else
            {
                val = val + "Tablet;";
            }
        }
        else
        {
            val = val + "Desktop;";
        }
        val = val + ((string) Application.genuine) + ") Battle.net/CSharp";
        Log.Yim.Print("userAgent = " + val, new object[0]);
        Log.Kyle.Print("userAgent = " + val, new object[0]);
        request.SetUserAgent(val);
        bool flag = false;
        request.SetEmail(this.m_userEmailAddress);
        object[] args = new object[] { flag };
        this.m_logSource.LogDebug("CreateLogonRequest SSL={0}", args);
        if (!string.IsNullOrEmpty(this.m_userEmailAddress))
        {
            object[] objArray4 = new object[] { this.m_userEmailAddress };
            this.m_logSource.LogDebug("Email = {0}", objArray4);
        }
        return request;
    }

    public void CreateParty(string szPartyType, int privacyLevel, byte[] creatorBlob)
    {
        this.m_partyAPI.CreateParty(szPartyType, privacyLevel, creatorBlob);
    }

    public void DeclineFriendlyChallenge(ref BattleNet.DllEntityId partyId)
    {
        this.m_partyAPI.DeclineFriendlyChallenge(partyId.ToProtocol(), "deny");
    }

    public void DeclinePartyInvite(ulong invitationId)
    {
        this.m_partyAPI.DeclinePartyInvite(invitationId);
    }

    public void DissolveParty(BattleNet.DllEntityId partyId)
    {
        this.m_partyAPI.DissolveParty(partyId.ToProtocol());
    }

    public void EnqueueErrorInfo(BnetFeature feature, BnetFeatureEvent featureEvent, BattleNetErrors error, int context = 0)
    {
        object[] args = new object[] { feature.ToString(), featureEvent.ToString(), new bnet.Error(error), context };
        Log.BattleNet.Print(LogLevel.Warning, string.Format("Enqueuing BattleNetError {0} {1} code={2} context={3}", args), new object[0]);
        Log.JMac.Print(LogLevel.Warning, string.Format("Enqueuing BattleNetError {0} context={1}", error, context), new object[0]);
        this.m_errorEvents.Add(new BnetErrorInfo(feature, featureEvent, error, context));
    }

    public string FilterProfanity(string unfiltered)
    {
        return this.m_profanityAPI.FilterProfanity(unfiltered);
    }

    public void FindGame(byte[] requestGuid, BnetGameType gameType, int missionId, long deckId, long aiDeckId, bool setScenarioIdAttr)
    {
        this.m_gamesAPI.FindGame(requestGuid, gameType, missionId, deckId, aiDeckId, setScenarioIdAttr);
    }

    public string GetAccountCountry()
    {
        return this.Account.GetAccountCountry();
    }

    public EntityId GetAccountEntity()
    {
        return this.m_authenticationAPI.AccountId;
    }

    public int GetAccountRegion()
    {
        if (this.Account.GetPreferredRegion() == uint.MaxValue)
        {
            return -1;
        }
        return (int) this.Account.GetPreferredRegion();
    }

    public void GetAllPartyAttributes(BattleNet.DllEntityId partyId, out string[] allKeys)
    {
        this.m_partyAPI.GetAllPartyAttributes(partyId.ToProtocol(), out allKeys);
    }

    public void GetBnetEvents([Out] BattleNet.BnetEvent[] bnetEvents)
    {
        this.m_bnetEvents.CopyTo(bnetEvents);
    }

    public int GetBnetEventsSize()
    {
        return this.m_bnetEvents.Count;
    }

    public void GetChallenges([Out] BattleNet.DllChallengeInfo[] challenges)
    {
        this.m_challengeAPI.GetChallenges(challenges);
    }

    public int GetCountPartyMembers(BattleNet.DllEntityId partyId)
    {
        return this.m_partyAPI.GetCountPartyMembers(partyId.ToProtocol());
    }

    public int GetCurrentRegion()
    {
        if (this.m_connectedRegion == uint.MaxValue)
        {
            return -1;
        }
        return (int) this.m_connectedRegion;
    }

    public long GetCurrentTimeSecondsSinceUnixEpoch()
    {
        TimeSpan span = (TimeSpan) (DateTime.UtcNow - this.m_unixEpoch);
        return (long) span.TotalSeconds;
    }

    public string GetEnvironment()
    {
        return this.m_auroraEnvironment;
    }

    public void GetErrors([Out] BnetErrorInfo[] errors)
    {
        this.m_errorEvents.Sort(new BnetErrorComparer());
        this.m_errorEvents.CopyTo(errors);
    }

    public int GetErrorsCount()
    {
        return this.m_errorEvents.Count;
    }

    public void GetFriendsInfo(ref BattleNet.DllFriendsInfo info)
    {
        this.m_friendAPI.GetFriendsInfo(ref info);
    }

    public void GetFriendsUpdates([Out] BattleNet.FriendsUpdate[] updates)
    {
        this.m_friendAPI.GetFriendsUpdates(updates);
    }

    public string GetLaunchOption(string key)
    {
        this.m_logSource.LogError("GetLaunchOption not implemented");
        return string.Empty;
    }

    public BattleNetLogSource GetLogSource()
    {
        return this.m_logSource;
    }

    public int GetMaxPartyMembers(BattleNet.DllEntityId partyId)
    {
        return this.m_partyAPI.GetMaxPartyMembers(partyId.ToProtocol());
    }

    public BattleNet.DllEntityId GetMyAccountId()
    {
        return new BattleNet.DllEntityId { hi = this.AccountId.High, lo = this.AccountId.Low };
    }

    public BattleNet.DllEntityId GetMyGameAccountId()
    {
        return new BattleNet.DllEntityId { hi = this.GameAccountId.High, lo = this.GameAccountId.Low };
    }

    public int GetNotificationCount()
    {
        return this.m_notificationAPI.GetNotificationCount();
    }

    public void GetNotifications([Out] BnetNotification[] notifications)
    {
        this.m_notificationAPI.GetNotifications(notifications);
    }

    public void GetPartyAttributeBlob(BattleNet.DllEntityId partyId, string attributeKey, out byte[] value)
    {
        this.m_partyAPI.GetPartyAttributeBlob(partyId.ToProtocol(), attributeKey, out value);
    }

    public bool GetPartyAttributeLong(BattleNet.DllEntityId partyId, string attributeKey, out long value)
    {
        return this.m_partyAPI.GetPartyAttributeLong(partyId.ToProtocol(), attributeKey, out value);
    }

    public void GetPartyAttributeString(BattleNet.DllEntityId partyId, string attributeKey, out string value)
    {
        this.m_partyAPI.GetPartyAttributeString(partyId.ToProtocol(), attributeKey, out value);
    }

    public void GetPartyInviteRequests(BattleNet.DllEntityId partyId, out InviteRequest[] requests)
    {
        this.m_partyAPI.GetPartyInviteRequests(partyId.ToProtocol(), out requests);
    }

    public void GetPartyListenerEvents(out BattleNet.PartyListenerEvent[] updates)
    {
        this.m_partyAPI.GetPartyListenerEvents(out updates);
    }

    public void GetPartyMembers(BattleNet.DllEntityId partyId, out BattleNet.DllPartyMember[] members)
    {
        this.m_partyAPI.GetPartyMembers(partyId.ToProtocol(), out members);
    }

    public int GetPartyPrivacy(BattleNet.DllEntityId partyId)
    {
        return this.m_partyAPI.GetPartyPrivacy(partyId.ToProtocol());
    }

    public void GetPartySentInvites(BattleNet.DllEntityId partyId, out PartyInvite[] invites)
    {
        this.m_partyAPI.GetPartySentInvites(partyId.ToProtocol(), out invites);
    }

    public void GetPartyUpdates([Out] BattleNet.PartyEvent[] updates)
    {
        this.m_partyAPI.GetPartyUpdates(updates);
    }

    public void GetPartyUpdatesInfo(ref BattleNet.DllPartyInfo info)
    {
        info.size = this.m_partyAPI.GetPartyUpdateCount();
    }

    public void GetPlayRestrictions(ref BattleNet.DllLockouts restrictions, bool reload)
    {
        this.Account.GetPlayRestrictions(ref restrictions, reload);
    }

    public void GetPresence([Out] BattleNet.PresenceUpdate[] updates)
    {
        this.m_presenceAPI.GetPresence(updates);
    }

    public BattleNet.QueueEvent GetQueueEvent()
    {
        return this.m_gamesAPI.GetQueueEvent();
    }

    public void GetQueueInfo(ref BattleNet.DllQueueInfo queueInfo)
    {
        this.m_authenticationAPI.GetQueueInfo(ref queueInfo);
    }

    public void GetReceivedPartyInvites(out PartyInvite[] invites)
    {
        this.m_partyAPI.GetReceivedPartyInvites(out invites);
    }

    public int GetShutdownMinutes()
    {
        if (!this.m_shutdownNeedsImplentationWarningAlreadyGiven)
        {
            this.m_logSource.LogWarning("GetShutdownMinutes : needs to be implemented with C# broadcast");
            this.m_shutdownNeedsImplentationWarningAlreadyGiven = true;
        }
        return 0;
    }

    private SslParameters GetSSLParams()
    {
        SslParameters parameters = new SslParameters {
            bundleInfo = new SslCertBundleInfo()
        };
        parameters.bundleInfo.isUsingCertBundle = true;
        parameters.bundleInfo.isCertBundleSigned = true;
        TextAsset asset = (TextAsset) UnityEngine.Resources.Load("SSLCert/ssl_cert_bundle");
        if (asset == null)
        {
            return null;
        }
        parameters.bundleInfo.certBundleBytes = asset.bytes;
        return parameters;
    }

    private static string GetStoredBNetIP()
    {
        return null;
    }

    public string GetStoredBNetIPAddress()
    {
        return GetStoredBNetIP();
    }

    private static string GetStoredUserName()
    {
        return null;
    }

    private static string GetStoredVersion()
    {
        return null;
    }

    public string GetVersion()
    {
        if (this.m_auroraVersionAsInt)
        {
            return this.m_auroraVersionInt.ToString();
        }
        return this.m_auroraVersionString;
    }

    public int GetVersionInt()
    {
        return this.m_auroraVersionInt;
    }

    public string GetVersionSource()
    {
        return this.m_auroraVersionSource;
    }

    public string GetVersionString()
    {
        return this.m_auroraVersionString;
    }

    public void GetWhisperInfo(ref BattleNet.DllWhisperInfo info)
    {
        this.m_whisperAPI.GetWhisperInfo(ref info);
    }

    public void GetWhispers([Out] BnetWhisper[] whispers)
    {
        this.m_whisperAPI.GetWhispers(whispers);
    }

    private void HandleEchoRequest(RPCContext context)
    {
        if (this.m_rpcConnection == null)
        {
            Debug.LogError("HandleEchoRequest with null RPC Connection");
        }
        else
        {
            EchoRequest request = EchoRequest.ParseFrom(context.Payload);
            EchoResponse response = new EchoResponse();
            if (request.HasTime)
            {
                response.SetTime(request.Time);
            }
            if (request.HasPayload)
            {
                response.SetPayload(request.Payload);
            }
            EchoResponse message = response;
            this.m_rpcConnection.QueueResponse(context, message);
            Console.WriteLine(string.Empty);
            Console.WriteLine("[*]send echo response");
        }
    }

    private void HandleForceDisconnectRequest(RPCContext context)
    {
        DisconnectNotification notification = DisconnectNotification.ParseFrom(context.Payload);
        this.m_logSource.LogDebug("RPC Called: ForceDisconnect : " + notification.ErrorCode);
        this.EnqueueErrorInfo(BnetFeature.Bnet, BnetFeatureEvent.Bnet_OnDisconnected, (BattleNetErrors) notification.ErrorCode, 0);
    }

    private void HandleNotificationReceived(RPCContext context)
    {
        NotificationHandler handler;
        bnet.protocol.notification.Notification notification = bnet.protocol.notification.Notification.ParseFrom(context.Payload);
        this.m_logSource.LogDebug("Notification: " + notification);
        if (this.m_notificationHandlers.TryGetValue(notification.Type, out handler))
        {
            handler(notification);
        }
        else
        {
            this.m_logSource.LogWarning("unhandled battle net notification: " + notification.Type);
        }
    }

    public void IgnoreInviteRequest(BattleNet.DllEntityId partyId, BattleNet.DllEntityId requestedTargetId)
    {
        this.m_partyAPI.IgnoreInviteRequest(partyId.ToProtocol(), requestedTargetId.ToProtocol());
    }

    public bool Init(bool internalMode)
    {
        if (this.m_initialized)
        {
            return true;
        }
        string storedUserName = null;
        string storedVersion = null;
        string hostName = null;
        int port = 0;
        try
        {
            storedUserName = GetStoredUserName();
            storedVersion = GetStoredVersion();
            hostName = GetStoredBNetIP();
        }
        catch (Exception exception)
        {
            this.m_logSource.LogError("Exception while loading settings: " + exception.Message);
        }
        bool isDev = ApplicationMgr.GetMobileEnvironment() == MobileEnv.DEVELOPMENT;
        if (!MobileDeviceLocale.UseClientConfigForEnv())
        {
            Network.BnetRegion currentRegionId = MobileDeviceLocale.GetCurrentRegionId();
            MobileDeviceLocale.ConnectionData connectionDataFromRegionId = MobileDeviceLocale.GetConnectionDataFromRegionId(currentRegionId, isDev);
            hostName = connectionDataFromRegionId.address;
            port = connectionDataFromRegionId.port;
            storedVersion = connectionDataFromRegionId.version;
            object[] args = new object[] { currentRegionId, connectionDataFromRegionId.address, connectionDataFromRegionId.port, storedVersion };
            Debug.Log(string.Format("Region-based Connection Data for region {0}: {1}:{2}:{3}", args));
            int num2 = -1;
            if (storedVersion == "product")
            {
                num2 = BattleNet.ProductVersion();
            }
            else
            {
                try
                {
                    num2 = Convert.ToInt32(storedVersion);
                }
                catch (Exception)
                {
                }
            }
            if (num2 >= 0)
            {
                this.m_auroraVersionAsInt = true;
                this.m_auroraVersionInt = num2;
                this.m_auroraVersionSource = "int";
            }
            else
            {
                this.m_auroraVersionAsInt = false;
                this.m_auroraVersionString = storedVersion;
                this.m_auroraVersionSource = "string";
            }
        }
        else
        {
            this.m_auroraVersionSource = Vars.Key("Aurora.Version.Source").GetStr("undefined");
            if (this.m_auroraVersionSource == "product")
            {
                this.m_auroraVersionAsInt = true;
                this.m_auroraVersionInt = BattleNet.ProductVersion();
            }
            else if (this.m_auroraVersionSource == "int")
            {
                this.m_auroraVersionAsInt = true;
                this.m_auroraVersionInt = Vars.Key("Aurora.Version.Int").GetInt(0);
                if (this.m_auroraVersionInt == 0)
                {
                    this.m_logSource.LogError("Aurora.Version.Int undefined");
                }
            }
            else if (this.m_auroraVersionSource == "string")
            {
                this.m_auroraVersionAsInt = false;
                this.m_auroraVersionString = Vars.Key("Aurora.Version.String").GetStr("undefined");
            }
            else if (this.m_auroraVersionSource == "PAX")
            {
                this.m_auroraVersionAsInt = false;
                this.m_auroraVersionString = "PAX";
                this.m_auroraVersionInt = Vars.Key("Aurora.Version.Int").GetInt(0);
                if (this.m_auroraVersionInt == 0)
                {
                    this.m_logSource.LogError("Aurora.Version.Int undefined");
                }
            }
            else
            {
                this.m_logSource.LogError("unknown version source: " + this.m_auroraVersionSource);
                this.m_auroraVersionAsInt = true;
                this.m_auroraVersionInt = 0;
            }
        }
        if (this.m_auroraVersionAsInt)
        {
            this.m_auroraVersionSource = "int";
        }
        if (storedUserName == null)
        {
            storedUserName = Vars.Key("Aurora.Username").GetStr("NOT_PROVIDED_PLEASE_PROVIDE_VIA_CONFIG");
        }
        if ((storedUserName != null) && (storedUserName.IndexOf("@") == -1))
        {
            storedUserName = storedUserName + "@blizzard.com";
        }
        string def = !isDev ? "us.actual.battle.net" : "bn11-01.battle.net";
        switch (hostName)
        {
            case null:
                hostName = Vars.Key("Aurora.Env").GetStr(def);
                break;

            case "default":
                hostName = def;
                break;
        }
        this.m_auroraEnvironment = hostName;
        if (port == 0)
        {
            port = Vars.Key("Aurora.Port").GetInt(0x45f);
        }
        string address = string.Empty;
        bool hostAddress = false;
        try
        {
            hostAddress = UriUtils.GetHostAddress(hostName, out address);
        }
        catch (Exception exception2)
        {
            this.m_logSource.LogError("Exception within GetHostAddress: " + exception2.Message);
        }
        if (hostAddress || !Network.ShouldBeConnectedToAurora())
        {
            DebugConsole.Get().Init();
            this.m_initialized = ConnectAPI.ConnectInit();
            this.m_userEmailAddress = storedUserName;
            object[] objArray2 = new object[] { storedUserName };
            this.m_logSource.LogDebug("Username is {0}", objArray2);
            if (Network.ShouldBeConnectedToAurora())
            {
                SslParameters sSLParams = this.GetSSLParams();
                if (sSLParams == null)
                {
                    if (ApplicationMgr.IsInternal())
                    {
                        Error.AddDevFatal("Failed to load SSL certificate.", new object[0]);
                    }
                    else
                    {
                        Error.AddFatalLoc("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN", new object[0]);
                    }
                }
                this.ConnectAurora(hostName, port, sSLParams);
            }
        }
        else if (ApplicationMgr.IsInternal())
        {
            Error.AddDevFatal("Environment " + hostName + " could not be resolved! Please check your environment and Internet connection!", new object[0]);
        }
        else
        {
            Error.AddFatalLoc("GLOBAL_ERROR_NETWORK_NO_CONNECTION", new object[0]);
        }
        return this.m_initialized;
    }

    private void InitRPCListeners()
    {
        this.m_rpcConnection.RegisterServiceMethodListener(this.m_notificationListenerService.Id, 1, new RPCContextDelegate(this.HandleNotificationReceived));
        foreach (BattleNetAPI tapi in this.m_apiList)
        {
            tapi.InitRPCListeners(this.m_rpcConnection);
        }
    }

    private bool InState(ConnectionState state)
    {
        return (this.m_connectionState == state);
    }

    public bool IsInitialized()
    {
        return this.m_initialized;
    }

    public void IssueSelectGameAccountRequest()
    {
        this.m_rpcConnection.QueueRequest(this.m_authenticationAPI.AuthServerService.Id, 4, this.GameAccountId, new RPCContextDelegate(this.OnSelectGameAccountCallback), 0);
        this.SwitchToState(ConnectionState.WaitForGameAccountSelect);
    }

    public void JoinParty(BattleNet.DllEntityId partyId, string szPartyType)
    {
        this.m_partyAPI.JoinParty(partyId.ToProtocol(), szPartyType);
    }

    public void KickPartyMember(BattleNet.DllEntityId partyId, BattleNet.DllEntityId memberId)
    {
        this.m_partyAPI.KickPartyMember(partyId.ToProtocol(), memberId.ToProtocol());
    }

    public void LeaveParty(BattleNet.DllEntityId partyId)
    {
        this.m_partyAPI.LeaveParty(partyId.ToProtocol());
    }

    public void ManageFriendInvite(int action, ulong inviteId)
    {
        this.m_friendAPI.ManageFriendInvite(action, inviteId);
    }

    public PegasusPacket NextUtilPacket()
    {
        return this.m_gamesAPI.NextUtilPacket();
    }

    public int NumChallenges()
    {
        return this.m_challengeAPI.NumChallenges();
    }

    private void OnConnectCallback(RPCContext context)
    {
        BattleNetErrors status = (BattleNetErrors) context.Header.Status;
        this.m_logSource.LogDebug("RPC Connected, error : " + status.ToString());
        if (status != BattleNetErrors.ERROR_OK)
        {
            this.SwitchToState(ConnectionState.Error);
            this.EnqueueErrorInfo(BnetFeature.Bnet, BnetFeatureEvent.Bnet_OnConnected, status, 0);
        }
        else
        {
            ConnectResponse response = ConnectResponse.ParseFrom(context.Payload);
            if (response.HasServerTime)
            {
                this.m_serverTimeUTCAtConnectMicroseconds = (long) response.ServerTime;
                this.m_serverTimeDeltaUTCSeconds = (this.m_serverTimeUTCAtConnectMicroseconds / 0xf4240L) - this.GetCurrentTimeSecondsSinceUnixEpoch();
            }
            if ((response.HasBindResult && response.HasBindResponse) && (response.BindResult == 0))
            {
                int num = 0;
                foreach (uint num2 in response.BindResponse.ImportedServiceIdList)
                {
                    ServiceDescriptor serviceDescriptor = this.m_importedServices[num++];
                    serviceDescriptor.Id = num2;
                    this.m_rpcConnection.serviceHelper.AddImportedService(num2, serviceDescriptor);
                    object[] objArray1 = new object[] { serviceDescriptor.Id, serviceDescriptor.Name };
                    this.m_logSource.LogDebug("Importing service id={0} name={1}", objArray1);
                }
                if (response.HasContentHandleArray)
                {
                    if (!Vars.Key("Aurora.DisableConnectionMetering").GetBool(false))
                    {
                        this.m_rpcConnection.SetConnectionMeteringContentHandles(response.ContentHandleArray, this.m_localStorageAPI);
                    }
                    else
                    {
                        this.m_logSource.LogWarning("Connection metering disabled by configuration.");
                    }
                }
                else
                {
                    this.m_logSource.LogDebug("Connection response had not connection metering request");
                }
                object[] args = new object[] { response.ServerId.Label };
                this.m_logSource.LogDebug("FRONT ServerId={0:x2}", args);
                this.InitRPCListeners();
                this.PrintBindServiceResponse(response.BindResponse);
                this.SwitchToState(ConnectionState.Logon);
            }
            else
            {
                object[] objArray3 = new object[] { response.BindResult };
                this.m_logSource.LogWarning("BindRequest failed with error={0}", objArray3);
                this.SwitchToState(ConnectionState.Error);
            }
        }
    }

    public void OnConnectHandlerCallback(BattleNetErrors error)
    {
        if (error != BattleNetErrors.ERROR_OK)
        {
            this.SwitchToState(ConnectionState.Error);
            this.EnqueueErrorInfo(BnetFeature.Bnet, BnetFeatureEvent.Bnet_OnConnected, error, 0);
        }
        foreach (BattleNetAPI tapi in this.m_apiList)
        {
            tapi.OnConnected(error);
        }
    }

    private void OnDisconectHandlerCallback(BattleNetErrors error)
    {
        object[] args = new object[] { (int) error, error.ToString() };
        this.m_logSource.LogInfo("Disconnected: code={0} {1}", args);
        if (error != BattleNetErrors.ERROR_OK)
        {
            this.SwitchToState(ConnectionState.Error);
            this.EnqueueErrorInfo(BnetFeature.Bnet, BnetFeatureEvent.Bnet_OnDisconnected, error, 0);
        }
        this.m_bnetEvents.Add(BattleNet.BnetEvent.Disconnected);
        foreach (BattleNetAPI tapi in this.m_apiList)
        {
            tapi.OnDisconnected();
        }
    }

    private void OnSelectGameAccountCallback(RPCContext context)
    {
        BattleNetErrors status = (BattleNetErrors) context.Header.Status;
        if (status == BattleNetErrors.ERROR_OK)
        {
            this.SwitchToState(ConnectionState.WaitForAPIToInitialize);
            foreach (BattleNetAPI tapi in this.m_apiList)
            {
                tapi.Initialize();
                tapi.OnGameAccountSelected();
            }
        }
        else
        {
            this.SwitchToState(ConnectionState.Error);
            this.EnqueueErrorInfo(BnetFeature.Auth, BnetFeatureEvent.Auth_GameAccountSelected, status, context.Context);
            object[] args = new object[] { status.ToString() };
            this.m_logSource.LogError("Failed to select a game account status = {0}", args);
        }
    }

    public int PresenceSize()
    {
        return this.m_presenceAPI.PresenceSize();
    }

    private void PrintBindServiceResponse(BindResponse response)
    {
        string message = "BindResponse: { ";
        int importedServiceIdCount = response.ImportedServiceIdCount;
        message = (message + "Num Imported Services: " + importedServiceIdCount) + " [";
        for (int i = 0; i < importedServiceIdCount; i++)
        {
            message = message + " Id:" + response.ImportedServiceId[i];
        }
        message = message + " ]" + " }";
        this.m_logSource.LogDebug(message);
    }

    public void ProcessAurora()
    {
        if (!this.InState(ConnectionState.Disconnected) && !this.InState(ConnectionState.Error))
        {
            AuroraStateHandler handler;
            if (this.m_rpcConnection != null)
            {
                this.m_rpcConnection.Update();
                if (this.m_rpcConnection.MillisecondsSinceLastPacketSent > this.m_keepAliveIntervalMilliseconds)
                {
                    this.m_rpcConnection.QueueRequest(this.m_connectionService.Id, 5, new NoData(), null, 0);
                }
            }
            if (this.m_stateHandlers.TryGetValue(this.m_connectionState, out handler))
            {
                handler();
            }
            else
            {
                this.m_logSource.LogError("Missing state handler");
            }
            for (int i = 0; i < this.m_apiList.Count; i++)
            {
                this.m_apiList[i].Process();
            }
        }
    }

    public void ProvideWebAuthToken(string token)
    {
        BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_INFO, BIReport.TelemetryEvent.EVENT_WEB_LOGIN_TOKEN_PROVIDED, MobileDeviceLocale.GetCurrentRegionId());
        object[] args = new object[] { token };
        this.m_logSource.LogDebug("ProvideWebAuthToken token={0}", args);
        if ((this.m_authenticationAPI != null) && this.InState(ConnectionState.WaitForLogon))
        {
            this.m_authenticationAPI.VerifyWebCredentials(token);
        }
    }

    public void QueryAurora()
    {
    }

    public void RemoveFriend(BnetAccountId account)
    {
        this.m_friendAPI.RemoveFriend(account);
    }

    public void RequestCloseAurora()
    {
        this.SwitchToState(ConnectionState.Disconnected);
    }

    public void RequestPartyInvite(BattleNet.DllEntityId partyId, BattleNet.DllEntityId whomToAskForApproval, BattleNet.DllEntityId whomToInvite, string szPartyType)
    {
        this.m_partyAPI.RequestPartyInvite(partyId.ToProtocol(), whomToAskForApproval.ToProtocol(), whomToInvite.ToProtocol(), szPartyType);
    }

    public void RequestPresenceFields(bool isGameAccountEntityId, [In] BattleNet.DllEntityId entityId, [In] BattleNet.DllPresenceFieldKey[] fieldList)
    {
        this.m_presenceAPI.RequestPresenceFields(isGameAccountEntityId, entityId, fieldList);
    }

    public void RescindFriendlyChallenge(ref BattleNet.DllEntityId partyId)
    {
        this.m_partyAPI.DeclineFriendlyChallenge(partyId.ToProtocol(), "kill");
    }

    public void Reset()
    {
        this.CloseAurora();
    }

    public void RevokePartyInvite(BattleNet.DllEntityId partyId, ulong invitationId)
    {
        this.m_partyAPI.RevokePartyInvite(partyId.ToProtocol(), invitationId);
    }

    public void SendFriendInvite(string sender, string target, bool byEmail)
    {
        this.m_friendAPI.SendFriendInvite(sender, target, byEmail);
    }

    public void SendFriendlyChallengeInvite(ref BattleNet.DllEntityId gameAccount, int scenarioId)
    {
        this.m_partyAPI.SendFriendlyChallengeInvite(gameAccount.ToProtocol(), scenarioId);
    }

    public void SendPartyChatMessage(BattleNet.DllEntityId partyId, string message)
    {
        this.m_partyAPI.SendPartyChatMessage(partyId.ToProtocol(), message);
    }

    public void SendPartyInvite(BattleNet.DllEntityId partyId, BattleNet.DllEntityId inviteeId, bool isReservation)
    {
        this.m_partyAPI.SendPartyInvite(partyId.ToProtocol(), inviteeId.ToProtocol(), isReservation);
    }

    public void SendUtilPacket(int packetId, int systemId, byte[] bytes, int size, int subID, int context, ulong route)
    {
        this.m_gamesAPI.SendUtilPacket(packetId, systemId, bytes, size, subID, context, route);
    }

    public void SendWhisper(BnetGameAccountId gameAccount, string message)
    {
        this.m_whisperAPI.SendWhisper(gameAccount, message);
    }

    public void SetConnectedRegion(uint region)
    {
        this.m_connectedRegion = region;
    }

    public void SetMyFriendlyChallengeDeck(ref BattleNet.DllEntityId partyId, long deckID)
    {
        this.m_partyAPI.SetPartyDeck(partyId.ToProtocol(), deckID);
    }

    public void SetPartyAttributeBlob(BattleNet.DllEntityId partyId, string attributeKey, [In] byte[] value)
    {
        this.m_partyAPI.SetPartyAttributeBlob(partyId.ToProtocol(), attributeKey, value);
    }

    public void SetPartyAttributeLong(BattleNet.DllEntityId partyId, string attributeKey, [In] long value)
    {
        this.m_partyAPI.SetPartyAttributeLong(partyId.ToProtocol(), attributeKey, value);
    }

    public void SetPartyAttributeString(BattleNet.DllEntityId partyId, string attributeKey, [In] string value)
    {
        this.m_partyAPI.SetPartyAttributeString(partyId.ToProtocol(), attributeKey, value);
    }

    public void SetPartyPrivacy(BattleNet.DllEntityId partyId, int privacyLevel)
    {
        this.m_partyAPI.SetPartyPrivacy(partyId.ToProtocol(), privacyLevel);
    }

    public void SetPresenceBlob(uint field, byte[] val)
    {
        this.m_presenceAPI.SetPresenceBlob(field, val);
    }

    public void SetPresenceBool(uint field, bool val)
    {
        this.m_presenceAPI.SetPresenceBool(field, val);
    }

    public void SetPresenceInt(uint field, long val)
    {
        this.m_presenceAPI.SetPresenceInt(field, val);
    }

    public void SetPresenceString(uint field, string val)
    {
        this.m_presenceAPI.SetPresenceString(field, val);
    }

    public void SetRichPresence([In] BattleNet.DllRichPresenceUpdate[] updates)
    {
        this.m_presenceAPI.PublishRichPresence(updates);
    }

    private bool SwitchToState(ConnectionState state)
    {
        if (state == this.m_connectionState)
        {
            return false;
        }
        bool flag = true;
        if ((state != ConnectionState.Disconnected) || (this.m_connectionState != ConnectionState.Ready))
        {
            flag = state > this.m_connectionState;
        }
        if (flag)
        {
            object[] args = new object[] { this.m_connectionState.ToString(), state.ToString() };
            this.m_logSource.LogDebug("Expected state change {0} -> {1}", args);
        }
        else
        {
            object[] objArray2 = new object[] { this.m_connectionState.ToString(), state.ToString() };
            this.m_logSource.LogWarning("Unexpected state changes {0} -> {1}", objArray2);
            this.m_logSource.LogDebugStackTrace("SwitchToState", 5, 0);
        }
        this.m_connectionState = state;
        return true;
    }

    public AccountAPI Account
    {
        get
        {
            return this.m_accountAPI;
        }
    }

    public EntityId AccountId
    {
        get
        {
            return this.m_authenticationAPI.AccountId;
        }
    }

    public ChallengeAPI Challenge
    {
        get
        {
            return this.m_challengeAPI;
        }
    }

    public ChannelAPI Channel
    {
        get
        {
            return this.m_channelAPI;
        }
    }

    public long CurrentUTCServerTimeSeconds
    {
        get
        {
            return (this.GetCurrentTimeSecondsSinceUnixEpoch() + this.m_serverTimeDeltaUTCSeconds);
        }
    }

    public FriendsAPI Friends
    {
        get
        {
            return this.m_friendAPI;
        }
    }

    public EntityId GameAccountId
    {
        get
        {
            return this.m_authenticationAPI.GameAccountId;
        }
    }

    public GamesAPI Games
    {
        get
        {
            return this.m_gamesAPI;
        }
    }

    public LocalStorageAPI LocalStorage
    {
        get
        {
            return this.m_localStorageAPI;
        }
    }

    public NotificationAPI Notification
    {
        get
        {
            return this.m_notificationAPI;
        }
    }

    public ServiceDescriptor NotificationService
    {
        get
        {
            return this.m_notificationService;
        }
    }

    public PartyAPI Party
    {
        get
        {
            return this.m_partyAPI;
        }
    }

    public PresenceAPI Presence
    {
        get
        {
            return this.m_presenceAPI;
        }
    }

    public ResourcesAPI Resources
    {
        get
        {
            return this.m_resourcesAPI;
        }
    }

    public long ServerTimeUTCAtConnectMicroseconds
    {
        get
        {
            return this.m_serverTimeUTCAtConnectMicroseconds;
        }
    }

    public WhisperAPI Whisper
    {
        get
        {
            return this.m_whisperAPI;
        }
    }

    public delegate void AuroraStateHandler();

    private class BnetErrorComparer : IComparer<BnetErrorInfo>
    {
        public int Compare(BnetErrorInfo x, BnetErrorInfo y)
        {
            if ((x != null) && (y != null))
            {
                if ((x.GetError() == BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED) && (y.GetError() != BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED))
                {
                    return 1;
                }
                if ((x.GetError() != BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED) && (y.GetError() == BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED))
                {
                    return -1;
                }
            }
            return 0;
        }
    }

    public enum ConnectionState
    {
        Disconnected,
        Connect,
        InitRPC,
        WaitForInitRPC,
        Logon,
        WaitForLogon,
        WaitForGameAccountSelect,
        WaitForAPIToInitialize,
        Ready,
        Error
    }

    private delegate void NotificationHandler(bnet.protocol.notification.Notification notification);

    private delegate void OnConnectHandler(BattleNetErrors error);

    private delegate void OnDisconectHandler(BattleNetErrors error);
}

