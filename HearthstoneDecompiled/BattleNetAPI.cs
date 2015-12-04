using System;
using System.Runtime.CompilerServices;

public class BattleNetAPI
{
    protected BattleNetCSharp m_battleNet;
    private LogDelegate[] m_logDelegates;
    public BattleNetLogSource m_logSource;
    protected RPCConnection m_rpcConnection;

    protected BattleNetAPI(BattleNetCSharp battlenet, string logSourceName)
    {
        this.m_battleNet = battlenet;
        this.m_logSource = new BattleNetLogSource(logSourceName);
        this.m_logDelegates = new LogDelegate[] { new LogDelegate(this.m_logSource.LogDebug), new LogDelegate(this.m_logSource.LogError) };
    }

    public bool CheckRPCCallback(string name, RPCContext context)
    {
        BattleNetErrors status = (BattleNetErrors) context.Header.Status;
        int index = (status != BattleNetErrors.ERROR_OK) ? 1 : 0;
        object[] args = new object[] { status, !string.IsNullOrEmpty(name) ? name : "<null>", (context.Request == null) ? "<null>" : context.Request.ToString() };
        this.m_logDelegates[index]("Callback invoked, status = {0}, name = {1}, request = {2}", args);
        return (status == BattleNetErrors.ERROR_OK);
    }

    public virtual void Initialize()
    {
    }

    public virtual void InitRPCListeners(RPCConnection rpcConnection)
    {
        this.m_rpcConnection = rpcConnection;
    }

    public virtual void OnConnected(BattleNetErrors error)
    {
    }

    public virtual void OnDisconnected()
    {
    }

    public virtual void OnGameAccountSelected()
    {
    }

    public virtual void OnLogon()
    {
    }

    public virtual void Process()
    {
    }

    public BattleNetLogSource ApiLog
    {
        get
        {
            return this.m_logSource;
        }
    }

    private delegate void LogDelegate(string format, params object[] args);
}

