using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
internal sealed class <>__AnonType1<<Handler>__T, <Index>__T>
{
    private readonly <Handler>__T <Handler>;
    private readonly <Index>__T <Index>;

    [DebuggerHidden]
    public <>__AnonType1(<Handler>__T Handler, <Index>__T Index)
    {
        this.<Handler> = Handler;
        this.<Index> = Index;
    }

    [DebuggerHidden]
    public override bool Equals(object obj)
    {
        <>__AnonType1<<Handler>__T, <Index>__T> type = obj as <>__AnonType1<<Handler>__T, <Index>__T>;
        return ((type != null) && (EqualityComparer<<Handler>__T>.Default.Equals(this.<Handler>, type.<Handler>) && EqualityComparer<<Index>__T>.Default.Equals(this.<Index>, type.<Index>)));
    }

    [DebuggerHidden]
    public override int GetHashCode()
    {
        int num = (((-2128831035 ^ EqualityComparer<<Handler>__T>.Default.GetHashCode(this.<Handler>)) * 0x1000193) ^ EqualityComparer<<Index>__T>.Default.GetHashCode(this.<Index>)) * 0x1000193;
        num += num << 13;
        num ^= num >> 7;
        num += num << 3;
        num ^= num >> 0x11;
        return (num + (num << 5));
    }

    [DebuggerHidden]
    public override string ToString()
    {
        string[] textArray1 = new string[] { "{", " Handler = ", (this.<Handler> == null) ? string.Empty : this.<Handler>.ToString(), ", Index = ", (this.<Index> == null) ? string.Empty : this.<Index>.ToString(), " }" };
        return string.Concat(textArray1);
    }

    public <Handler>__T Handler
    {
        get
        {
            return this.<Handler>;
        }
    }

    public <Index>__T Index
    {
        get
        {
            return this.<Index>;
        }
    }
}

