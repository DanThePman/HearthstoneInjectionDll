using bnet.protocol;
using bnet.protocol.authentication;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Text;
using WTCG.BI;

public class AuthenticationAPI : BattleNetAPI
{
    private EntityId m_accountEntity;
    private ServiceDescriptor m_authClientService;
    private bool m_authenticationFailure;
    private ServiceDescriptor m_authServerService;
    private EntityId m_gameAccount;
    private List<EntityId> m_gameAccounts;
    private BattleNet.DllQueueInfo m_queueInfo;

    public AuthenticationAPI(BattleNetCSharp battlenet) : base(battlenet, "Authentication")
    {
        this.m_authServerService = new RPCServices.AuthServerService();
        this.m_authClientService = new RPCServices.AuthClientService();
    }

    public bool AuthenticationFailure()
    {
        return this.m_authenticationFailure;
    }

    public void GetQueueInfo(ref BattleNet.DllQueueInfo queueInfo)
    {
        queueInfo.position = this.m_queueInfo.position;
        queueInfo.end = this.m_queueInfo.end;
        queueInfo.stdev = this.m_queueInfo.stdev;
        queueInfo.changed = this.m_queueInfo.changed;
        this.m_queueInfo.changed = false;
    }

    private void HandleGameAccountSelected(RPCContext context)
    {
        GameAccountSelectedRequest request = GameAccountSelectedRequest.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleGameAccountSelected : " + request.ToString());
    }

    private void HandleLoadModuleRequest(RPCContext context)
    {
        base.ApiLog.LogWarning("RPC Called: LoadModule");
        this.m_authenticationFailure = true;
    }

    private void HandleLogonCompleteRequest(RPCContext context)
    {
        LogonResult result = LogonResult.ParseFrom(context.Payload);
        BattleNetErrors errorCode = (BattleNetErrors) result.ErrorCode;
        if (errorCode != BattleNetErrors.ERROR_OK)
        {
            base.m_battleNet.EnqueueErrorInfo(BnetFeature.Auth, BnetFeatureEvent.Auth_OnFinish, errorCode, context.Context);
        }
        else
        {
            this.m_accountEntity = result.Account;
            base.m_battleNet.Presence.PresenceSubscribe(this.m_accountEntity);
            this.m_gameAccounts = new List<EntityId>();
            foreach (EntityId id in result.GameAccountList)
            {
                this.m_gameAccounts.Add(id);
                base.m_battleNet.Presence.PresenceSubscribe(id);
            }
            if (this.m_gameAccounts.Count > 0)
            {
                this.m_gameAccount = result.GameAccountList[0];
            }
            base.m_battleNet.IssueSelectGameAccountRequest();
            base.m_battleNet.SetConnectedRegion(result.ConnectedRegion);
            object[] args = new object[] { result };
            base.ApiLog.LogDebug("LogonComplete {0}", args);
            object[] objArray2 = new object[] { result.ConnectedRegion };
            base.ApiLog.LogDebug("Region (connected): {0}", objArray2);
            BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_INFO, BIReport.TelemetryEvent.EVENT_LOGIN_SUCCESS);
        }
    }

    private void HandleLogonQueueEnd(RPCContext context)
    {
        base.ApiLog.LogDebug("HandleLogonQueueEnd : ");
        this.SaveQueuePosition(0, 0L, 0L, true);
    }

    private void HandleLogonQueueUpdate(RPCContext context)
    {
        LogonQueueUpdateRequest request = LogonQueueUpdateRequest.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleLogonQueueUpdate : " + request.ToString());
        long end = (long) ((request.EstimatedTime - ((ulong) base.m_battleNet.ServerTimeUTCAtConnectMicroseconds)) / ((ulong) 0xf4240L));
        this.SaveQueuePosition((int) request.Position, end, (long) request.EtaDeviationInSec, false);
    }

    private void HandleLogonUpdateRequest(RPCContext context)
    {
        base.ApiLog.LogDebug("RPC Called: LogonUpdate");
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_authClientService.Id, 5, new RPCContextDelegate(this.HandleLogonCompleteRequest));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_authClientService.Id, 10, new RPCContextDelegate(this.HandleLogonUpdateRequest));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_authClientService.Id, 1, new RPCContextDelegate(this.HandleLoadModuleRequest));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_authClientService.Id, 12, new RPCContextDelegate(this.HandleLogonQueueUpdate));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_authClientService.Id, 13, new RPCContextDelegate(this.HandleLogonQueueEnd));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_authClientService.Id, 14, new RPCContextDelegate(this.HandleGameAccountSelected));
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
    }

    public void SaveQueuePosition(int position, long end, long stdev, bool ended)
    {
        this.m_queueInfo.changed = ((ended || (position != this.m_queueInfo.position)) || (end != this.m_queueInfo.end)) || (stdev != this.m_queueInfo.stdev);
        this.m_queueInfo.position = position;
        this.m_queueInfo.end = end;
        this.m_queueInfo.stdev = stdev;
    }

    public void VerifyWebCredentials(string token)
    {
        if (base.m_rpcConnection != null)
        {
            VerifyWebCredentialsRequest message = new VerifyWebCredentialsRequest();
            byte[] bytes = Encoding.UTF8.GetBytes(token);
            message.SetWebCredentials(bytes);
            base.m_rpcConnection.BeginAuth();
            base.m_rpcConnection.QueueRequest(this.AuthClientService.Id, 7, message, null, 0);
        }
    }

    public EntityId AccountId
    {
        get
        {
            return this.m_accountEntity;
        }
    }

    public ServiceDescriptor AuthClientService
    {
        get
        {
            return this.m_authClientService;
        }
    }

    public ServiceDescriptor AuthServerService
    {
        get
        {
            return this.m_authServerService;
        }
    }

    public EntityId GameAccountId
    {
        get
        {
            return this.m_gameAccount;
        }
    }
}

