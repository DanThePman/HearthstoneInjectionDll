using bnet.protocol;
using bnet.protocol.resources;
using RPCServices;
using System;
using System.Runtime.CompilerServices;
using System.Text;

public class ResourcesAPI : BattleNetAPI
{
    private Map<uint, ResourcesAPIPendingState> m_pendingLookups;
    private ServiceDescriptor m_resourcesService;

    public ResourcesAPI(BattleNetCSharp battlenet) : base(battlenet, "ResourcesAPI")
    {
        this.m_resourcesService = new RPCServices.ResourcesService();
        this.m_pendingLookups = new Map<uint, ResourcesAPIPendingState>();
    }

    public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder builder = new StringBuilder(ba.Length * 2);
        foreach (byte num in ba)
        {
            builder.AppendFormat("{0:x2}", num);
        }
        return builder.ToString();
    }

    private void GetContentHandleCallback(RPCContext context)
    {
        ResourcesAPIPendingState state = null;
        if (!this.m_pendingLookups.TryGetValue(context.Header.Token, out state))
        {
            base.ApiLog.LogWarning("Received unmatched lookup response");
        }
        else
        {
            this.m_pendingLookups.Remove(context.Header.Token);
            bnet.protocol.ContentHandle contentHandle = bnet.protocol.ContentHandle.ParseFrom(context.Payload);
            if ((contentHandle == null) || !contentHandle.IsInitialized)
            {
                base.ApiLog.LogWarning("Received invalid response");
                state.Callback(null, state.UserContext);
            }
            else
            {
                BattleNetErrors status = (BattleNetErrors) context.Header.Status;
                if (status != BattleNetErrors.ERROR_OK)
                {
                    object[] args = new object[] { status };
                    base.ApiLog.LogWarning("Battle.net Resources API C#: Failed lookup. Error={0}", args);
                    state.Callback(null, state.UserContext);
                }
                else
                {
                    ContentHandle handle2 = ContentHandle.FromProtocol(contentHandle);
                    state.Callback(handle2, state.UserContext);
                }
            }
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        base.ApiLog.LogDebug("Initializing");
    }

    public void LookupResource(FourCC programId, FourCC streamId, FourCC locale, ResourceLookupCallback cb, object userContext)
    {
        ContentHandleRequest message = new ContentHandleRequest();
        message.SetProgramId(programId.GetValue());
        message.SetStreamId(streamId.GetValue());
        message.SetLocale(locale.GetValue());
        if ((message == null) || !message.IsInitialized)
        {
            base.ApiLog.LogWarning("Unable to create request for RPC call.");
        }
        else
        {
            RPCContext context = base.m_rpcConnection.QueueRequest(this.m_resourcesService.Id, 1, message, new RPCContextDelegate(this.GetContentHandleCallback), 0);
            ResourcesAPIPendingState state = new ResourcesAPIPendingState {
                Callback = cb,
                UserContext = userContext
            };
            this.m_pendingLookups.Add(context.Header.Token, state);
            object[] args = new object[] { programId, streamId, locale };
            base.ApiLog.LogDebug("Lookup request sent. PID={0} StreamID={1} Locale={2}", args);
        }
    }

    private void ResouceLookupTestCallback(ContentHandle contentHandle, object userContext)
    {
        if (contentHandle == null)
        {
            base.ApiLog.LogWarning("Lookup failed");
        }
        else
        {
            int num = (int) userContext;
            object[] args = new object[] { num, contentHandle.Region, contentHandle.Usage, contentHandle.Sha256Digest };
            base.ApiLog.LogDebug("Lookup done i={0} Region={1} Usage={2} SHA256={3}", args);
        }
    }

    public ServiceDescriptor ResourcesService
    {
        get
        {
            return this.m_resourcesService;
        }
    }

    public delegate void ResourceLookupCallback(ContentHandle contentHandle, object userContext);
}

