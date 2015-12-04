using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using WTCG.BI;

public class BIReport : MonoBehaviour
{
    private const string BIURL = "http://iir.blizzard.com:3724/submit/WTCG";
    private const string DOP_PROTO_MESSAGE_TYPE = "WTCG.BI.Session.DataOnlyPatching";
    private static BIReport s_instance;
    private static string s_sessionId;
    private const string TELEMETRY_PROTO_MESSAGE_TYPE = "WTCG.BI.ClientTelemetry";

    private void Awake()
    {
        s_instance = this;
        this.GenerateSessionID();
    }

    public static double ConvertDateTimeToUnixEpoch(DateTime time)
    {
        DateTime time2 = new DateTime(0x7b2, 1, 1);
        TimeSpan span = (TimeSpan) (time - time2.ToLocalTime());
        return span.TotalSeconds;
    }

    private void GenerateSessionID()
    {
        if (s_sessionId != null)
        {
            Log.BIReport.Print("WARNING: Replacing session ID [" + s_sessionId + "]", new object[0]);
        }
        string message = SystemInfo.deviceUniqueIdentifier + DateTime.Now.ToFileTimeUtc().ToString();
        Log.BIReport.Print("rawSessionId = " + message, new object[0]);
        s_sessionId = Blizzard.Crypto.SHA1.Calc(message);
        Log.BIReport.Print("s_sessionId = " + s_sessionId, new object[0]);
    }

    public static BIReport Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    [DebuggerHidden]
    public IEnumerator Report(byte[] data)
    {
        return new <Report>c__Iterator233 { data = data, <$>data = data, <>f__this = this };
    }

    [DebuggerHidden]
    public IEnumerator Report(byte[] data, bool isDataOnlyPatching)
    {
        return new <Report>c__Iterator234 { isDataOnlyPatching = isDataOnlyPatching, data = data, <$>isDataOnlyPatching = isDataOnlyPatching, <$>data = data };
    }

    public void Report_DataOnlyPatching(DataOnlyPatching.Status status, Locale locale, int currentBuild, int newBuild)
    {
        DataOnlyPatching.Locale unknownLocale = DataOnlyPatching.Locale.UnknownLocale;
        switch (locale)
        {
            case Locale.enUS:
                unknownLocale = DataOnlyPatching.Locale.enUS;
                break;

            case Locale.enGB:
                unknownLocale = DataOnlyPatching.Locale.enGB;
                break;

            case Locale.frFR:
                unknownLocale = DataOnlyPatching.Locale.frFR;
                break;

            case Locale.deDE:
                unknownLocale = DataOnlyPatching.Locale.deDE;
                break;

            case Locale.koKR:
                unknownLocale = DataOnlyPatching.Locale.koKR;
                break;

            case Locale.esES:
                unknownLocale = DataOnlyPatching.Locale.esES;
                break;

            case Locale.esMX:
                unknownLocale = DataOnlyPatching.Locale.esMX;
                break;

            case Locale.ruRU:
                unknownLocale = DataOnlyPatching.Locale.ruRU;
                break;

            case Locale.zhTW:
                unknownLocale = DataOnlyPatching.Locale.zhTW;
                break;

            case Locale.zhCN:
                unknownLocale = DataOnlyPatching.Locale.zhCN;
                break;

            case Locale.itIT:
                unknownLocale = DataOnlyPatching.Locale.itIT;
                break;

            case Locale.ptBR:
                unknownLocale = DataOnlyPatching.Locale.ptBR;
                break;

            case Locale.plPL:
                unknownLocale = DataOnlyPatching.Locale.plPL;
                break;

            case Locale.jaJP:
                unknownLocale = DataOnlyPatching.Locale.Locale15;
                break;
        }
        DataOnlyPatching.Platform unknownPlatform = DataOnlyPatching.Platform.UnknownPlatform;
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
                unknownPlatform = DataOnlyPatching.Platform.Mac;
                break;

            case RuntimePlatform.OSXPlayer:
                unknownPlatform = DataOnlyPatching.Platform.Mac;
                break;

            case RuntimePlatform.WindowsPlayer:
                unknownPlatform = DataOnlyPatching.Platform.Windows;
                break;

            case RuntimePlatform.WindowsEditor:
                unknownPlatform = DataOnlyPatching.Platform.Windows;
                break;

            case RuntimePlatform.IPhonePlayer:
                unknownPlatform = (UniversalInputManager.UsePhoneUI == null) ? DataOnlyPatching.Platform.iPad : DataOnlyPatching.Platform.iPhone;
                break;

            case RuntimePlatform.Android:
                unknownPlatform = (UniversalInputManager.UsePhoneUI == null) ? DataOnlyPatching.Platform.Android_Tablet : DataOnlyPatching.Platform.Android_Phone;
                break;
        }
        DataOnlyPatching protobuf = new DataOnlyPatching {
            Status_ = status,
            Locale_ = unknownLocale,
            Platform_ = unknownPlatform,
            BnetRegion_ = (DataOnlyPatching.BnetRegion) BattleNet.GetCurrentRegion(),
            GameAccountId_ = BattleNet.GetMyGameAccountId().lo,
            CurrentBuild_ = currentBuild,
            NewBuild_ = newBuild,
            SessionId_ = s_sessionId,
            DeviceUniqueIdentifier_ = SystemInfo.deviceUniqueIdentifier
        };
        Log.BIReport.Print("Report " + protobuf.ToString(), new object[0]);
        base.StartCoroutine(this.Report(ProtobufUtil.ToByteArray(protobuf), true));
    }

    public void Report_Telemetry(Telemetry.Level level, TelemetryEvent telemetryEvent)
    {
        this.Report_Telemetry(level, telemetryEvent, 0, null);
    }

    public void Report_Telemetry(Telemetry.Level level, TelemetryEvent telemetryEvent, Network.BnetRegion overrideBnetRegion)
    {
        this.Report_Telemetry(level, telemetryEvent, 0, null, overrideBnetRegion);
    }

    public void Report_Telemetry(Telemetry.Level level, TelemetryEvent telemetryEvent, int errorCode, string message)
    {
        this.Report_Telemetry(level, telemetryEvent, errorCode, message, Network.BnetRegion.REGION_UNINITIALIZED);
    }

    public void Report_Telemetry(Telemetry.Level level, TelemetryEvent telemetryEvent, int errorCode, string message, Network.BnetRegion overrideBnetRegion)
    {
        if (s_sessionId == null)
        {
            Log.BIReport.Print("ERROR: Sending report while s_sessionId == NULL", new object[0]);
        }
        Telemetry.Locale locale = Telemetry.Locale.LOCALE_UNKNOWN;
        switch (Localization.GetLocale())
        {
            case Locale.enUS:
                locale = Telemetry.Locale.LOCALE_ENUS;
                break;

            case Locale.enGB:
                locale = Telemetry.Locale.LOCALE_ENGB;
                break;

            case Locale.frFR:
                locale = Telemetry.Locale.LOCALE_FRFR;
                break;

            case Locale.deDE:
                locale = Telemetry.Locale.LOCALE_DEDE;
                break;

            case Locale.koKR:
                locale = Telemetry.Locale.LOCALE_KOKR;
                break;

            case Locale.esES:
                locale = Telemetry.Locale.LOCALE_ESES;
                break;

            case Locale.esMX:
                locale = Telemetry.Locale.LOCALE_ESMX;
                break;

            case Locale.ruRU:
                locale = Telemetry.Locale.LOCALE_RURU;
                break;

            case Locale.zhTW:
                locale = Telemetry.Locale.LOCALE_ZHTW;
                break;

            case Locale.zhCN:
                locale = Telemetry.Locale.LOCALE_ZHCN;
                break;

            case Locale.itIT:
                locale = Telemetry.Locale.LOCALE_ITIT;
                break;

            case Locale.ptBR:
                locale = Telemetry.Locale.LOCALE_PTBR;
                break;

            case Locale.plPL:
                locale = Telemetry.Locale.LOCALE_PLPL;
                break;

            case Locale.jaJP:
                locale = Telemetry.Locale.LOCALE_15;
                break;
        }
        Telemetry.Platform platform = Telemetry.Platform.PLATFORM_UNKNOWN;
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
                platform = Telemetry.Platform.PLATFORM_MAC;
                break;

            case RuntimePlatform.OSXPlayer:
                platform = Telemetry.Platform.PLATFORM_MAC;
                break;

            case RuntimePlatform.WindowsPlayer:
                platform = Telemetry.Platform.PLATFORM_PC;
                break;

            case RuntimePlatform.WindowsEditor:
                platform = Telemetry.Platform.PLATFORM_PC;
                break;

            case RuntimePlatform.IPhonePlayer:
                platform = Telemetry.Platform.PLATFORM_IOS;
                break;

            case RuntimePlatform.Android:
                platform = Telemetry.Platform.PLATFORM_ANDROID;
                break;
        }
        Telemetry.ScreenUI nui = Telemetry.ScreenUI.SCREENUI_UNKNOWN;
        if ((Application.platform == RuntimePlatform.IPhonePlayer) || (Application.platform == RuntimePlatform.Android))
        {
            nui = (UniversalInputManager.UsePhoneUI == null) ? Telemetry.ScreenUI.SCREENUI_TABLET : Telemetry.ScreenUI.SCREENUI_PHONE;
        }
        else
        {
            nui = Telemetry.ScreenUI.SCREENUI_DESKTOP;
        }
        Telemetry data = new Telemetry {
            Time_ = (long) ConvertDateTimeToUnixEpoch(DateTime.Now),
            Level_ = level,
            Version_ = string.Format("{0}.{1}", "4.1", 0x2acc),
            Locale_ = locale,
            Platform_ = platform,
            Os_ = SystemInfo.operatingSystem,
            ScreenUI_ = nui,
            Store_ = Telemetry.Store.STORE_BLIZZARD,
            SessionId_ = s_sessionId,
            DeviceUniqueIdentifier_ = SystemInfo.deviceUniqueIdentifier,
            Event_ = (ulong) ((long) telemetryEvent)
        };
        if (BattleNet.IsInitialized() && (BattleNet.GetCurrentRegion() != Network.BnetRegion.REGION_UNINITIALIZED))
        {
            data.BnetRegion_ = (Telemetry.BnetRegion) BattleNet.GetCurrentRegion();
            data.GameAccountId_ = BattleNet.GetMyGameAccountId().lo;
        }
        if (overrideBnetRegion != Network.BnetRegion.REGION_UNINITIALIZED)
        {
            data.BnetRegion_ = (Telemetry.BnetRegion) overrideBnetRegion;
        }
        data.ErrorCode_ = errorCode;
        if (message != null)
        {
            data.Message_ = message;
        }
        Log.BIReport.Print("Report: " + this.TelemetryDataToString(data), new object[0]);
        base.StartCoroutine(this.Report(ProtobufUtil.ToByteArray(data)));
    }

    private string TelemetryDataToString(Telemetry data)
    {
        object[] objArray1 = new object[] { 
            "Event_ = ", (TelemetryEvent) ((int) data.Event_), " Time = ", data.Time_, " Level = ", data.Level_, " Version = ", data.Version_, " Locale = ", data.Locale_, " Platform = ", data.Platform_, " OS = ", data.Os_, " ScreenUI = ", data.ScreenUI_, 
            " Store = ", data.Store_, " SessionId = ", data.SessionId_, " DeviceUniqueIdentifier = ", data.DeviceUniqueIdentifier_, " BnetRegion_ = ", data.BnetRegion_, " GameAccountId_ = ", data.GameAccountId_, " ErrorCode_ = ", data.ErrorCode_, " Message = ", data.Message_
         };
        return string.Concat(objArray1);
    }

    [CompilerGenerated]
    private sealed class <Report>c__Iterator233 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal byte[] <$>data;
        internal BIReport <>f__this;
        internal byte[] data;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.Report(this.data, false));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <Report>c__Iterator234 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal byte[] <$>data;
        internal bool <$>isDataOnlyPatching;
        internal Dictionary<string, string> <headers>__0;
        internal string <protoMessageType>__1;
        internal WWW <www>__2;
        internal byte[] data;
        internal bool isDataOnlyPatching;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<headers>__0 = new Dictionary<string, string>();
                    this.<headers>__0["ir-exchange"] = "biapi";
                    this.<protoMessageType>__1 = !this.isDataOnlyPatching ? "WTCG.BI.ClientTelemetry" : "WTCG.BI.Session.DataOnlyPatching";
                    this.<headers>__0["irrh-x-proto-message-type"] = this.<protoMessageType>__1;
                    this.<www>__2 = new WWW("http://iir.blizzard.com:3724/submit/WTCG", this.data, this.<headers>__0);
                    this.$current = this.<www>__2;
                    this.$PC = 1;
                    return true;

                case 1:
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    public enum TelemetryEvent
    {
        EVENT_FATAL_BNET_ERROR = 600,
        EVENT_IGNORABLE_BNET_ERROR = 500,
        EVENT_LOGIN_ERROR_RESOLVE_HOST = 110,
        EVENT_LOGIN_REQUEST = 100,
        EVENT_LOGIN_SUCCESS = 200,
        EVENT_ON_RESET = 700,
        EVENT_ON_RESET_WITH_LOGIN = 710,
        EVENT_THIRD_PARTY_PURCHASE_DEFERRED = 840,
        EVENT_THIRD_PARTY_PURCHASE_FAILED = 830,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_RECEIVED = 860,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_REQUEST = 890,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_REQUEST_FOUND = 0x37c,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_REQUEST_NOT_FOUND = 0x37b,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_SIZE = 850,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_SUBMITTED = 870,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_SUBMITTED_DANGLING = 880,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_SUBMITTED_DANGLING_FAILED = 0x371,
        EVENT_THIRD_PARTY_PURCHASE_RECEIPT_SUBMITTED_FAILED = 0x367,
        EVENT_THIRD_PARTY_PURCHASE_REQUEST = 800,
        EVENT_THIRD_PARTY_PURCHASE_SUCCESS = 810,
        EVENT_THIRD_PARTY_PURCHASE_SUCCESS_MALFORMED = 820,
        EVENT_WEB_LOGIN_ERROR = 410,
        EVENT_WEB_LOGIN_TOKEN_PROVIDED = 300
    }
}

