using bnet.protocol;
using bnet.protocol.attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class ProtocolHelper
{
    public static bnet.protocol.attribute.Attribute CreateAttribute(string name, bool val)
    {
        bnet.protocol.attribute.Attribute attribute = new bnet.protocol.attribute.Attribute();
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        variant.SetBoolValue(val);
        attribute.SetName(name);
        attribute.SetValue(variant);
        return attribute;
    }

    public static bnet.protocol.attribute.Attribute CreateAttribute(string name, long val)
    {
        bnet.protocol.attribute.Attribute attribute = new bnet.protocol.attribute.Attribute();
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        variant.SetIntValue(val);
        attribute.SetName(name);
        attribute.SetValue(variant);
        return attribute;
    }

    public static bnet.protocol.attribute.Attribute CreateAttribute(string name, string val)
    {
        bnet.protocol.attribute.Attribute attribute = new bnet.protocol.attribute.Attribute();
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        variant.SetStringValue(val);
        attribute.SetName(name);
        attribute.SetValue(variant);
        return attribute;
    }

    public static bnet.protocol.attribute.Attribute CreateAttribute(string name, byte[] val)
    {
        bnet.protocol.attribute.Attribute attribute = new bnet.protocol.attribute.Attribute();
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        variant.SetBlobValue(val);
        attribute.SetName(name);
        attribute.SetValue(variant);
        return attribute;
    }

    public static bnet.protocol.attribute.Attribute CreateAttribute(string name, ulong val)
    {
        bnet.protocol.attribute.Attribute attribute = new bnet.protocol.attribute.Attribute();
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        variant.SetUintValue(val);
        attribute.SetName(name);
        attribute.SetValue(variant);
        return attribute;
    }

    public static EntityId CreateEntityId(ulong high, ulong low)
    {
        EntityId id = new EntityId();
        id.SetHigh(high);
        id.SetLow(low);
        return id;
    }

    public static bnet.protocol.attribute.Attribute FindAttribute(List<bnet.protocol.attribute.Attribute> list, string attributeName, Func<bnet.protocol.attribute.Attribute, bool> condition = null)
    {
        <FindAttribute>c__AnonStorey317 storey = new <FindAttribute>c__AnonStorey317 {
            attributeName = attributeName,
            condition = condition
        };
        if (list == null)
        {
            return null;
        }
        if (storey.condition == null)
        {
            return Enumerable.FirstOrDefault<bnet.protocol.attribute.Attribute>(list, new Func<bnet.protocol.attribute.Attribute, bool>(storey.<>m__133));
        }
        return Enumerable.FirstOrDefault<bnet.protocol.attribute.Attribute>(list, new Func<bnet.protocol.attribute.Attribute, bool>(storey.<>m__134));
    }

    public static ulong GetUintAttribute(List<bnet.protocol.attribute.Attribute> list, string attributeName, ulong defaultValue, Func<bnet.protocol.attribute.Attribute, bool> condition = null)
    {
        bnet.protocol.attribute.Attribute attribute;
        <GetUintAttribute>c__AnonStorey318 storey = new <GetUintAttribute>c__AnonStorey318 {
            attributeName = attributeName,
            condition = condition
        };
        if (list == null)
        {
            return defaultValue;
        }
        if (storey.condition == null)
        {
            attribute = Enumerable.FirstOrDefault<bnet.protocol.attribute.Attribute>(list, new Func<bnet.protocol.attribute.Attribute, bool>(storey.<>m__135));
        }
        else
        {
            attribute = Enumerable.FirstOrDefault<bnet.protocol.attribute.Attribute>(list, new Func<bnet.protocol.attribute.Attribute, bool>(storey.<>m__136));
        }
        return ((attribute != null) ? attribute.Value.UintValue : defaultValue);
    }

    public static bool IsNone(this bnet.protocol.attribute.Variant val)
    {
        return ((((!val.HasBoolValue && !val.HasIntValue) && (!val.HasFloatValue && !val.HasStringValue)) && ((!val.HasBlobValue && !val.HasMessageValue) && (!val.HasFourccValue && !val.HasUintValue))) && !val.HasEntityidValue);
    }

    [CompilerGenerated]
    private sealed class <FindAttribute>c__AnonStorey317
    {
        internal string attributeName;
        internal Func<bnet.protocol.attribute.Attribute, bool> condition;

        internal bool <>m__133(bnet.protocol.attribute.Attribute a)
        {
            return (a.Name == this.attributeName);
        }

        internal bool <>m__134(bnet.protocol.attribute.Attribute a)
        {
            return (!(a.Name == this.attributeName) ? false : this.condition(a));
        }
    }

    [CompilerGenerated]
    private sealed class <GetUintAttribute>c__AnonStorey318
    {
        internal string attributeName;
        internal Func<bnet.protocol.attribute.Attribute, bool> condition;

        internal bool <>m__135(bnet.protocol.attribute.Attribute a)
        {
            return ((a.Name == this.attributeName) && a.Value.HasUintValue);
        }

        internal bool <>m__136(bnet.protocol.attribute.Attribute a)
        {
            return ((!(a.Name == this.attributeName) || !a.Value.HasUintValue) ? false : this.condition(a));
        }
    }
}

