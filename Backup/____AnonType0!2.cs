using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
internal sealed class <>__AnonType0<<i>__T, <c>__T>
{
    private readonly <c>__T <c>;
    private readonly <i>__T <i>;

    [DebuggerHidden]
    public <>__AnonType0(<i>__T i, <c>__T c)
    {
        this.<i> = i;
        this.<c> = c;
    }

    [DebuggerHidden]
    public override bool Equals(object obj)
    {
        <>__AnonType0<<i>__T, <c>__T> type = obj as <>__AnonType0<<i>__T, <c>__T>;
        return ((type != null) && (EqualityComparer<<i>__T>.Default.Equals(this.<i>, type.<i>) && EqualityComparer<<c>__T>.Default.Equals(this.<c>, type.<c>)));
    }

    [DebuggerHidden]
    public override int GetHashCode()
    {
        int num = (((-2128831035 ^ EqualityComparer<<i>__T>.Default.GetHashCode(this.<i>)) * 0x1000193) ^ EqualityComparer<<c>__T>.Default.GetHashCode(this.<c>)) * 0x1000193;
        num += num << 13;
        num ^= num >> 7;
        num += num << 3;
        num ^= num >> 0x11;
        return (num + (num << 5));
    }

    [DebuggerHidden]
    public override string ToString()
    {
        string[] textArray1 = new string[] { "{", " i = ", (this.<i> == null) ? string.Empty : this.<i>.ToString(), ", c = ", (this.<c> == null) ? string.Empty : this.<c>.ToString(), " }" };
        return string.Concat(textArray1);
    }

    public <c>__T c
    {
        get
        {
            return this.<c>;
        }
    }

    public <i>__T i
    {
        get
        {
            return this.<i>;
        }
    }
}

