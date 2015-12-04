using System;
using System.Runtime.CompilerServices;

public class MethodDescriptor
{
    private bool hasReturnData;
    private uint id;
    private RPCContextDelegate listener;
    private ParseMethod m_parseMethod;
    private string name;

    public MethodDescriptor(string n, uint i, ParseMethod parseMethod)
    {
        this.name = n;
        this.id = i;
        this.m_parseMethod = parseMethod;
        if (this.m_parseMethod == null)
        {
            BattleNet.Log.LogError("MethodDescriptor called with a null method type!");
        }
    }

    public bool HasListener()
    {
        return (this.listener != null);
    }

    public void NotifyListener(RPCContext context)
    {
        if (this.listener != null)
        {
            this.listener(context);
        }
    }

    public void RegisterListener(RPCContextDelegate d)
    {
        this.listener = d;
    }

    public uint Id
    {
        get
        {
            return this.id;
        }
        set
        {
            this.id = value;
        }
    }

    public string Name
    {
        get
        {
            return this.name;
        }
    }

    public ParseMethod Parser
    {
        get
        {
            return this.m_parseMethod;
        }
    }

    public delegate IProtoBuf ParseMethod(byte[] bs);
}

