using bnet.protocol;
using bnet.protocol.account;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class AccountAPI : BattleNetAPI
{
    private string m_accountCountry;
    private ServiceDescriptor m_accountNotify;
    private ServiceDescriptor m_accountService;
    private List<AccountLicense> m_licenses;
    private uint m_preferredRegion;

    public AccountAPI(BattleNetCSharp battlenet) : base(battlenet, "Account")
    {
        this.m_accountService = new RPCServices.AccountService();
        this.m_accountNotify = new AccountNotify();
        this.m_preferredRegion = uint.MaxValue;
        this.m_licenses = new List<AccountLicense>();
    }

    public bool CheckLicense(uint licenseId)
    {
        if ((this.m_licenses != null) && (this.m_licenses.Count != 0))
        {
            foreach (AccountLicense license in this.m_licenses)
            {
                if (license.Id == licenseId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public string GetAccountCountry()
    {
        return this.m_accountCountry;
    }

    private void GetAccountLevelInfo(EntityId accountId)
    {
        GetAccountStateRequest message = new GetAccountStateRequest();
        message.SetEntityId(accountId);
        AccountFieldOptions val = new AccountFieldOptions();
        val.SetFieldAccountLevelInfo(true);
        message.SetOptions(val);
        base.m_rpcConnection.QueueRequest(this.m_accountService.Id, 30, message, new RPCContextDelegate(this.GetAccountStateCallback), 0);
    }

    private void GetAccountStateCallback(RPCContext context)
    {
        if ((context == null) || (context.Payload == null))
        {
            base.ApiLog.LogWarning("GetAccountLevelInfo invalid context!");
        }
        else
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                object[] args = new object[] { status.ToString() };
                base.ApiLog.LogError("GetAccountLevelInfo failed with error={0}", args);
            }
            else
            {
                GetAccountStateResponse response = GetAccountStateResponse.ParseFrom(context.Payload);
                if ((response == null) || !response.IsInitialized)
                {
                    base.ApiLog.LogWarning("GetAccountStateCallback unable to parse response!");
                }
                else if (!response.HasState || !response.State.HasAccountLevelInfo)
                {
                    base.ApiLog.LogWarning("GetAccountStateCallback response has no data!");
                }
                else
                {
                    GetAccountStateRequest request = (GetAccountStateRequest) context.Request;
                    if ((request != null) && (request.EntityId == base.m_battleNet.AccountId))
                    {
                        AccountLevelInfo accountLevelInfo = response.State.AccountLevelInfo;
                        this.m_preferredRegion = accountLevelInfo.PreferredRegion;
                        this.m_accountCountry = accountLevelInfo.Country;
                        object[] objArray2 = new object[] { this.m_preferredRegion };
                        base.ApiLog.LogDebug("Region (preferred): {0}", objArray2);
                        object[] objArray3 = new object[] { this.m_accountCountry };
                        base.ApiLog.LogDebug("Country (account): {0}", objArray3);
                        if (accountLevelInfo.LicensesList.Count > 0)
                        {
                            this.m_licenses.Clear();
                            object[] objArray4 = new object[] { accountLevelInfo.LicensesList.Count };
                            base.ApiLog.LogDebug("Found {0} licenses.", objArray4);
                            for (int i = 0; i < accountLevelInfo.LicensesList.Count; i++)
                            {
                                AccountLicense item = accountLevelInfo.LicensesList[i];
                                this.m_licenses.Add(item);
                                object[] objArray5 = new object[] { item.Id };
                                base.ApiLog.LogDebug("Adding license id={0}", objArray5);
                            }
                        }
                        else
                        {
                            base.ApiLog.LogWarning("No licenses found!");
                        }
                    }
                    base.ApiLog.LogDebug("GetAccountLevelInfo, status=" + status.ToString());
                }
            }
        }
    }

    public void GetPlayRestrictions(ref BattleNet.DllLockouts restrictions, bool reload)
    {
        if (!reload)
        {
            if (this.LastGameSessionInfo != null)
            {
                restrictions.loaded = true;
                restrictions.sessionStartTime = this.LastGameSessionInfo.SessionStartTime;
                return;
            }
            if (this.GameSessionRunningCount > 0)
            {
                return;
            }
        }
        this.LastGameSessionInfo = null;
        this.GameSessionRunningCount++;
        GetGameSessionInfoRequest message = new GetGameSessionInfoRequest();
        message.SetEntityId(base.m_battleNet.GameAccountId);
        GetGameSessionInfoRequestContext context = new GetGameSessionInfoRequestContext(this);
        base.m_rpcConnection.QueueRequest(this.m_accountService.Id, 0x22, message, new RPCContextDelegate(context.GetGameSessionInfoRequestContextCallback), 0);
    }

    public uint GetPreferredRegion()
    {
        return this.m_preferredRegion;
    }

    private void HandleAccountNotify_AccountStateUpdated(RPCContext context)
    {
        if ((context == null) || (context.Payload == null))
        {
            base.ApiLog.LogWarning("HandleAccountNotify_AccountStateUpdated invalid context!");
        }
        else
        {
            AccountStateNotification notification = AccountStateNotification.ParseFrom(context.Payload);
            if ((notification == null) || !notification.IsInitialized)
            {
                base.ApiLog.LogWarning("HandleAccountNotify_AccountStateUpdated unable to parse response!");
            }
            else if (!notification.HasState)
            {
                object[] args = new object[] { notification };
                base.ApiLog.LogDebug("HandleAccountNotify_AccountStateUpdated HasState=false, data={0}", args);
            }
            else
            {
                AccountState state = notification.State;
                if (!state.HasAccountLevelInfo)
                {
                    object[] objArray2 = new object[] { notification };
                    base.ApiLog.LogDebug("HandleAccountNotify_AccountStateUpdated HasAccountLevelInfo=false, data={0}", objArray2);
                }
                else if (!state.AccountLevelInfo.HasPreferredRegion)
                {
                    object[] objArray3 = new object[] { notification };
                    base.ApiLog.LogDebug("HandleAccountNotify_AccountStateUpdated HasPreferredRegion=false, data={0}", objArray3);
                }
                else
                {
                    object[] objArray4 = new object[] { notification };
                    base.ApiLog.LogDebug("HandleAccountNotify_AccountStateUpdated, data={0}", objArray4);
                }
            }
        }
    }

    private void HandleAccountNotify_GameAccountStateUpdated(RPCContext context)
    {
        GameAccountStateNotification notification = GameAccountStateNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleAccountNotify_GameAccountStateUpdated, data=" + notification);
    }

    private void HandleAccountNotify_GameAccountsUpdated(RPCContext context)
    {
        GameAccountNotification notification = GameAccountNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleAccountNotify_GameAccountsUpdated, data=" + notification);
    }

    private void HandleAccountNotify_GameSessionUpdated(RPCContext context)
    {
        GameAccountSessionNotification notification = GameAccountSessionNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleAccountNotify_GameSessionUpdated, data=" + notification);
    }

    public override void Initialize()
    {
        base.ApiLog.LogDebug("Account API initializing");
        base.Initialize();
        this.GetAccountLevelInfo(base.m_battleNet.AccountId);
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_accountNotify.Id, 1, new RPCContextDelegate(this.HandleAccountNotify_AccountStateUpdated));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_accountNotify.Id, 2, new RPCContextDelegate(this.HandleAccountNotify_GameAccountStateUpdated));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_accountNotify.Id, 3, new RPCContextDelegate(this.HandleAccountNotify_GameAccountsUpdated));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_accountNotify.Id, 4, new RPCContextDelegate(this.HandleAccountNotify_GameSessionUpdated));
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
    }

    private void SubscribeToAccountService()
    {
        SubscriptionUpdateRequest message = new SubscriptionUpdateRequest();
        SubscriberReference val = new SubscriberReference();
        val.SetEntityId(base.m_battleNet.AccountId);
        val.SetObjectId(0L);
        AccountFieldOptions options = new AccountFieldOptions();
        options.SetAllFields(true);
        val.SetAccountOptions(options);
        message.AddRef(val);
        val = new SubscriberReference();
        val.SetEntityId(base.m_battleNet.GameAccountId);
        val.SetObjectId(0L);
        new GameAccountFieldOptions().SetAllFields(true);
        message.AddRef(val);
        base.m_rpcConnection.QueueRequest(this.m_accountService.Id, 0x19, message, new RPCContextDelegate(this.SubscribeToAccountServiceCallback), 0);
    }

    private void SubscribeToAccountServiceCallback(RPCContext context)
    {
        BattleNetErrors status = (BattleNetErrors) context.Header.Status;
        if (status != BattleNetErrors.ERROR_OK)
        {
            base.ApiLog.LogError("SubscribeToAccountServiceCallback: " + status.ToString());
        }
        else
        {
            base.ApiLog.LogDebug("SubscribeToAccountServiceCallback, status=" + status.ToString());
        }
    }

    public ServiceDescriptor AccountNotifyService
    {
        get
        {
            return this.m_accountNotify;
        }
    }

    public ServiceDescriptor AccountService
    {
        get
        {
            return this.m_accountService;
        }
    }

    public int GameSessionRunningCount { get; set; }

    public bool HasLicenses
    {
        get
        {
            if (this.m_licenses == null)
            {
                return false;
            }
            return (this.m_licenses.Count > 0);
        }
    }

    public GameSessionInfo LastGameSessionInfo { get; set; }

    public class GameSessionInfo
    {
        public ulong SessionStartTime;
    }

    private class GetGameSessionInfoRequestContext
    {
        private AccountAPI m_parent;

        public GetGameSessionInfoRequestContext(AccountAPI parent)
        {
            this.m_parent = parent;
        }

        public void GetGameSessionInfoRequestContextCallback(RPCContext context)
        {
            this.m_parent.GameSessionRunningCount--;
            if ((context == null) || (context.Payload == null))
            {
                this.m_parent.ApiLog.LogWarning("GetPlayRestrictions invalid context!");
            }
            else
            {
                BattleNetErrors status = (BattleNetErrors) context.Header.Status;
                if (status != BattleNetErrors.ERROR_OK)
                {
                    object[] args = new object[] { status.ToString() };
                    this.m_parent.ApiLog.LogError("GetPlayRestrictions failed with error={0}", args);
                }
                else
                {
                    GetGameSessionInfoResponse response = GetGameSessionInfoResponse.ParseFrom(context.Payload);
                    if ((response == null) || !response.IsInitialized)
                    {
                        this.m_parent.ApiLog.LogWarning("GetPlayRestrictions unable to parse response!");
                    }
                    else if (!response.HasSessionInfo)
                    {
                        this.m_parent.ApiLog.LogWarning("GetPlayRestrictions response has no data!");
                    }
                    else
                    {
                        this.m_parent.LastGameSessionInfo = new AccountAPI.GameSessionInfo();
                        if (response.SessionInfo.HasStartTime)
                        {
                            this.m_parent.LastGameSessionInfo.SessionStartTime = response.SessionInfo.StartTime;
                        }
                        else
                        {
                            this.m_parent.ApiLog.LogWarning("GetPlayRestrictions response has no HasStartTime!");
                        }
                    }
                }
            }
        }
    }
}

