using bnet.protocol.config;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class RPCMeterConfigParser
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map29;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map2A;

    public static RPCMeterConfig ParseConfig(string str)
    {
        string str2;
        RPCMeterConfig config = new RPCMeterConfig();
        Tokenizer tokenizer = new Tokenizer(str);
    Label_000D:
        str2 = tokenizer.NextString();
        if (str2 == null)
        {
            return config;
        }
        string key = str2;
        if (key != null)
        {
            int num;
            if (<>f__switch$map2A == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(5);
                dictionary.Add("method", 0);
                dictionary.Add("income_per_second:", 1);
                dictionary.Add("initial_balance:", 2);
                dictionary.Add("cap_balance:", 3);
                dictionary.Add("startup_period:", 4);
                <>f__switch$map2A = dictionary;
            }
            if (<>f__switch$map2A.TryGetValue(key, out num))
            {
                switch (num)
                {
                    case 0:
                        config.AddMethod(ParseMethod(tokenizer));
                        goto Label_000D;

                    case 1:
                        config.IncomePerSecond = tokenizer.NextUInt32();
                        goto Label_000D;

                    case 2:
                        config.InitialBalance = tokenizer.NextUInt32();
                        goto Label_000D;

                    case 3:
                        config.CapBalance = tokenizer.NextUInt32();
                        goto Label_000D;

                    case 4:
                        config.StartupPeriod = tokenizer.NextFloat();
                        goto Label_000D;
                }
            }
        }
        tokenizer.SkipUnknownToken();
        goto Label_000D;
    }

    public static RPCMethodConfig ParseMethod(Tokenizer tokenizer)
    {
        string str;
        RPCMethodConfig config = new RPCMethodConfig();
        tokenizer.NextOpenBracket();
    Label_000C:
        str = tokenizer.NextString();
        switch (str)
        {
            case null:
                throw new Exception("Parsing ended with unfinished RPCMethodConfig");

            case "}":
                return config;
        }
        string key = str;
        if (key != null)
        {
            int num;
            if (<>f__switch$map29 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(11);
                dictionary.Add("service_name:", 0);
                dictionary.Add("method_name:", 1);
                dictionary.Add("fixed_call_cost:", 2);
                dictionary.Add("fixed_packet_size:", 3);
                dictionary.Add("variable_multiplier:", 4);
                dictionary.Add("multiplier:", 5);
                dictionary.Add("rate_limit_count:", 6);
                dictionary.Add("rate_limit_seconds:", 7);
                dictionary.Add("max_packet_size:", 8);
                dictionary.Add("max_encoded_size:", 9);
                dictionary.Add("timeout:", 10);
                <>f__switch$map29 = dictionary;
            }
            if (<>f__switch$map29.TryGetValue(key, out num))
            {
                switch (num)
                {
                    case 0:
                        config.ServiceName = tokenizer.NextQuotedString();
                        goto Label_000C;

                    case 1:
                        config.MethodName = tokenizer.NextQuotedString();
                        goto Label_000C;

                    case 2:
                        config.FixedCallCost = tokenizer.NextUInt32();
                        goto Label_000C;

                    case 3:
                        config.FixedPacketSize = tokenizer.NextUInt32();
                        goto Label_000C;

                    case 4:
                        config.VariableMultiplier = tokenizer.NextUInt32();
                        goto Label_000C;

                    case 5:
                        config.Multiplier = tokenizer.NextFloat();
                        goto Label_000C;

                    case 6:
                        config.RateLimitCount = tokenizer.NextUInt32();
                        goto Label_000C;

                    case 7:
                        config.RateLimitSeconds = tokenizer.NextUInt32();
                        goto Label_000C;

                    case 8:
                        config.MaxPacketSize = tokenizer.NextUInt32();
                        goto Label_000C;

                    case 9:
                        config.MaxEncodedSize = tokenizer.NextUInt32();
                        goto Label_000C;

                    case 10:
                        config.Timeout = tokenizer.NextFloat();
                        goto Label_000C;
                }
            }
        }
        tokenizer.SkipUnknownToken();
        goto Label_000C;
    }
}

