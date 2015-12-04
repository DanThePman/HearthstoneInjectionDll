using System;

public class ServiceDescriptor
{
    private uint hash;
    private uint id;
    private const uint INVALID_SERVICE_ID = 0xff;
    protected MethodDescriptor[] Methods;
    private string name;

    public ServiceDescriptor(string n)
    {
        this.name = n;
        this.id = 0xff;
        this.hash = Compute32.Hash(this.name);
        Console.WriteLine(string.Concat(new object[] { "service: ", n, ", hash: ", this.hash }));
    }

    public int GetMethodCount()
    {
        if (this.Methods == null)
        {
            return 0;
        }
        return this.Methods.Length;
    }

    public MethodDescriptor GetMethodDescriptor(uint method_id)
    {
        if (this.Methods == null)
        {
            return null;
        }
        if (method_id >= this.Methods.Length)
        {
            return null;
        }
        return this.Methods[method_id];
    }

    public MethodDescriptor GetMethodDescriptorByName(string name)
    {
        if (this.Methods != null)
        {
            foreach (MethodDescriptor descriptor in this.Methods)
            {
                if ((descriptor != null) && (descriptor.Name == name))
                {
                    return descriptor;
                }
            }
        }
        return null;
    }

    public string GetMethodName(uint method_id)
    {
        if (((this.Methods != null) && (method_id > 0)) && (method_id <= this.Methods.Length))
        {
            return this.Methods[method_id].Name;
        }
        return string.Empty;
    }

    public MethodDescriptor.ParseMethod GetParser(uint method_id)
    {
        if (this.Methods == null)
        {
            BattleNet.Log.LogWarning("ServiceDescriptor unable to get parser, no methods have been set.");
            return null;
        }
        if (method_id <= 0)
        {
            object[] args = new object[] { method_id, this.Methods.Length };
            BattleNet.Log.LogWarning("ServiceDescriptor unable to get parser, invalid index={0}/{1}", args);
            return null;
        }
        if (method_id >= this.Methods.Length)
        {
            object[] objArray2 = new object[] { method_id, this.Methods.Length };
            BattleNet.Log.LogWarning("ServiceDescriptor unable to get parser, invalid index={0}/{1}", objArray2);
            return null;
        }
        if (this.Methods[method_id].Parser == null)
        {
            object[] objArray3 = new object[] { method_id, this.Methods.Length };
            BattleNet.Log.LogWarning("ServiceDescriptor unable to get parser, invalid index={0}/{1}", objArray3);
            return null;
        }
        return this.Methods[method_id].Parser;
    }

    public bool HasMethodListener(uint method_id)
    {
        return ((((this.Methods != null) && (method_id > 0)) && (method_id <= this.Methods.Length)) && this.Methods[method_id].HasListener());
    }

    public void NotifyMethodListener(RPCContext context)
    {
        if (((this.Methods != null) && (context.Header.MethodId > 0)) && (context.Header.MethodId <= this.Methods.Length))
        {
            this.Methods[context.Header.MethodId].NotifyListener(context);
        }
    }

    public void RegisterMethodListener(uint method_id, RPCContextDelegate callback)
    {
        if (((this.Methods != null) && (method_id > 0)) && (method_id <= this.Methods.Length))
        {
            this.Methods[method_id].RegisterListener(callback);
        }
    }

    public uint Hash
    {
        get
        {
            return this.hash;
        }
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
}

