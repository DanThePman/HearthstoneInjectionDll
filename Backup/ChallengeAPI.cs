using bnet.protocol.challenge;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class ChallengeAPI : BattleNetAPI
{
    private ServiceDescriptor m_challengeNotifyService;
    private Map<uint, BattleNet.DllChallengeInfo> m_challengePendingList;
    private ServiceDescriptor m_challengeService;
    private List<BattleNet.DllChallengeInfo> m_challengeUpdateList;
    private ExternalChallenge m_nextExternalChallenge;
    private const uint PWD_FOURCC = 0x505744;
    private Map<uint, ulong> s_pendingAnswers;

    public ChallengeAPI(BattleNetCSharp battlenet) : base(battlenet, "Challenge")
    {
        this.m_challengeService = new RPCServices.ChallengeService();
        this.m_challengeNotifyService = new ChallengeNotify();
        this.m_challengeUpdateList = new List<BattleNet.DllChallengeInfo>();
        this.m_challengePendingList = new Map<uint, BattleNet.DllChallengeInfo>();
        this.s_pendingAnswers = new Map<uint, ulong>();
    }

    private void AbortChallenge(ulong id)
    {
        ChallengeCancelledRequest message = new ChallengeCancelledRequest();
        message.SetId((uint) id);
        base.m_rpcConnection.QueueRequest(this.ChallengeService.Id, 3, message, new RPCContextDelegate(this.AbortChallengeCallback), 0);
        uint key = 0;
        bool flag = false;
        foreach (KeyValuePair<uint, BattleNet.DllChallengeInfo> pair in this.m_challengePendingList)
        {
            if (pair.Value.challengeId == id)
            {
                key = pair.Key;
                flag = true;
                break;
            }
        }
        if (flag)
        {
            this.m_challengePendingList.Remove(key);
        }
    }

    private void AbortChallengeCallback(RPCContext context)
    {
    }

    public void AnswerChallenge(ulong challengeID, string answer)
    {
        ChallengeAnsweredRequest message = new ChallengeAnsweredRequest();
        message.SetAnswer("pass");
        byte[] val = new byte[] { 2, 3, 4, 5, 6, 7, 0, 0 };
        message.SetData(val);
        if (message.IsInitialized)
        {
            RPCContext context = base.m_rpcConnection.QueueRequest(this.ChallengeService.Id, 2, message, new RPCContextDelegate(this.ChallengeAnsweredCallback), 0);
            this.s_pendingAnswers.Add(context.Header.Token, challengeID);
        }
    }

    public void CancelChallenge(ulong challengeID)
    {
        this.AbortChallenge(challengeID);
    }

    private void ChallengeAnsweredCallback(RPCContext context)
    {
        ChallengeAnsweredResponse response = ChallengeAnsweredResponse.ParseFrom(context.Payload);
        if (response.IsInitialized)
        {
            ulong num = 0L;
            if (this.s_pendingAnswers.TryGetValue(context.Header.Token, out num))
            {
                if (response.HasDoRetry && response.DoRetry)
                {
                    BattleNet.DllChallengeInfo item = new BattleNet.DllChallengeInfo {
                        challengeId = num,
                        isRetry = true
                    };
                    this.m_challengeUpdateList.Add(item);
                }
                this.s_pendingAnswers.Remove(context.Header.Token);
            }
        }
    }

    private void ChallengedPickedCallback(RPCContext context)
    {
        BattleNet.DllChallengeInfo info;
        if (!this.m_challengePendingList.TryGetValue(context.Header.Token, out info))
        {
            base.ApiLog.LogWarning("Battle.net Challenge API C#: Received unexpected ChallengedPicked.");
        }
        else
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                this.m_challengePendingList.Remove(context.Header.Token);
                base.ApiLog.LogWarning("Battle.net Challenge API C#: Failed ChallengedPicked. " + status);
            }
            else
            {
                this.m_challengeUpdateList.Add(info);
                this.m_challengePendingList.Remove(context.Header.Token);
            }
        }
    }

    private void ChallengeResultCallback(RPCContext context)
    {
    }

    private void ChallengeUserCallback(RPCContext context)
    {
        ChallengeUserRequest request = ChallengeUserRequest.ParseFrom(context.Payload);
        if (request.IsInitialized)
        {
            ulong id = request.Id;
            bool flag = false;
            for (int i = 0; i < request.ChallengesCount; i++)
            {
                Challenge challenge = request.Challenges[i];
                if (challenge.Type == 0x505744)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                this.AbortChallenge(id);
            }
            else
            {
                ChallengePickedRequest message = new ChallengePickedRequest();
                message.SetChallenge(0x505744);
                message.SetId((uint) id);
                message.SetNewChallengeProtocol(true);
                RPCContext context2 = base.m_rpcConnection.QueueRequest(this.ChallengeService.Id, 1, message, new RPCContextDelegate(this.ChallengedPickedCallback), 0);
                BattleNet.DllChallengeInfo info = new BattleNet.DllChallengeInfo {
                    challengeId = id,
                    isRetry = false
                };
                this.m_challengePendingList.Add(context2.Header.Token, info);
            }
        }
    }

    public void ClearChallenges()
    {
        this.m_challengeUpdateList.Clear();
    }

    public void GetChallenges([Out] BattleNet.DllChallengeInfo[] challenges)
    {
        this.m_challengeUpdateList.CopyTo(challenges);
    }

    public ExternalChallenge GetNextExternalChallenge()
    {
        ExternalChallenge nextExternalChallenge = this.m_nextExternalChallenge;
        if (this.m_nextExternalChallenge != null)
        {
            this.m_nextExternalChallenge = this.m_nextExternalChallenge.Next;
        }
        return nextExternalChallenge;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
        rpcConnection.RegisterServiceMethodListener(this.m_challengeNotifyService.Id, 1, new RPCContextDelegate(this.ChallengeUserCallback));
        rpcConnection.RegisterServiceMethodListener(this.m_challengeNotifyService.Id, 2, new RPCContextDelegate(this.ChallengeResultCallback));
        rpcConnection.RegisterServiceMethodListener(this.m_challengeNotifyService.Id, 3, new RPCContextDelegate(this.OnExternalChallengeCallback));
        rpcConnection.RegisterServiceMethodListener(this.m_challengeNotifyService.Id, 4, new RPCContextDelegate(this.OnExternalChallengeResultCallback));
    }

    public int NumChallenges()
    {
        return this.m_challengeUpdateList.Count;
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
    }

    private void OnExternalChallengeCallback(RPCContext context)
    {
        ChallengeExternalRequest request = ChallengeExternalRequest.ParseFrom(context.Payload);
        if (!request.IsInitialized || !request.HasPayload)
        {
            object[] args = new object[] { request.IsInitialized, request.HasRequestToken, request.HasPayload, request.HasPayloadType };
            base.ApiLog.LogWarning("Bad ChallengeExternalRequest received IsInitialized={0} HasRequestToken={1} HasPayload={2} HasPayloadType={3}", args);
        }
        else if (request.PayloadType != "web_auth_url")
        {
            object[] objArray2 = new object[] { request.PayloadType };
            base.ApiLog.LogWarning("Received a PayloadType we don't know how to handle PayloadType={0}", objArray2);
        }
        else
        {
            ExternalChallenge challenge = new ExternalChallenge {
                PayLoadType = request.PayloadType,
                URL = Encoding.ASCII.GetString(request.Payload)
            };
            object[] objArray3 = new object[] { challenge.PayLoadType, challenge.URL };
            base.ApiLog.LogDebug("Received external challenge PayLoadType={0} URL={1}", objArray3);
            if (this.m_nextExternalChallenge == null)
            {
                this.m_nextExternalChallenge = challenge;
            }
            else
            {
                this.m_nextExternalChallenge.Next = challenge;
            }
        }
    }

    private void OnExternalChallengeResultCallback(RPCContext context)
    {
    }

    public ServiceDescriptor ChallengeNotifyService
    {
        get
        {
            return this.m_challengeNotifyService;
        }
    }

    public ServiceDescriptor ChallengeService
    {
        get
        {
            return this.m_challengeService;
        }
    }
}

