using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Xml;
using UnityEngine;

public class ErrorReporter : MonoBehaviour
{
    private const int hearthstoneProjectID_ = 70;
    private static ErrorReporter instance_;
    private List<string> m_previousExceptions = new List<string>();
    private int sendCount_;
    private static readonly HashSet<string> sentReports_ = new HashSet<string>();
    private IPAddress unknownAddress_ = new IPAddress(new byte[4]);

    private static bool alreadySent(string hash)
    {
        return sentReports_.Contains(hash);
    }

    private void Awake()
    {
        instance_ = this;
        this.register();
    }

    private static string buildMarkup(string title, string stackTrace, string hashBlock)
    {
        string str = createEscapedSGML(stackTrace);
        object[] objArray1 = new object[] { 
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<ReportedIssue xmlns=\"http://schemas.datacontract.org/2004/07/Inspector.Models\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">\n\t<Summary>", title, "</Summary>\n\t<Assertion>", str, "</Assertion>\n\t<HashBlock>", hashBlock, "</HashBlock>\n\t<BuildNumber>", 0x2acc, "</BuildNumber>\n\t<Module>Hearthstone Client</Module>\n\t<EnteredBy>0</EnteredBy>\n\t<IssueType>Exception</IssueType>\n\t<ProjectId>", 70, "</ProjectId>\n\t<Metadata><NameValuePairs>\n\t\t<NameValuePair><Name>Build</Name><Value>", 0x2acc, "</Value></NameValuePair>\n\t\t<NameValuePair><Name>OS.Platform</Name><Value>", Application.platform, "</Value></NameValuePair>\n\t\t<NameValuePair><Name>Unity.Version</Name><Value>", Application.unityVersion, 
            "</Value></NameValuePair>\n\t\t<NameValuePair><Name>Unity.Genuine</Name><Value>", Application.genuine, "</Value></NameValuePair>\n\t\t<NameValuePair><Name>Locale</Name><Value>", Localization.GetLocaleName(), "</Value></NameValuePair>\n\t</NameValuePairs></Metadata>\n</ReportedIssue>\n"
         };
        return string.Concat(objArray1);
    }

    private void callback(string message, string stackTrace, LogType logType)
    {
        switch (logType)
        {
            case LogType.Error:
                if (Vars.Key("Application.SendErrors").GetBool(false))
                {
                    this.send(message, stackTrace);
                }
                break;

            case LogType.Assert:
                if (Vars.Key("Application.SendAsserts").GetBool(false))
                {
                    this.send(message, stackTrace);
                }
                break;

            case LogType.Exception:
                if (Vars.Key("Application.SendExceptions").GetBool(true))
                {
                    this.send(message, stackTrace);
                }
                this.reportUnhandledException(message, stackTrace);
                break;
        }
    }

    private static string createEscapedSGML(string blob)
    {
        XmlElement element = new XmlDocument().CreateElement("root");
        element.InnerText = blob;
        return element.InnerXml;
    }

    private static string createHash(string blob)
    {
        return Blizzard.Crypto.SHA1.Calc(blob);
    }

    public static ErrorReporter Get()
    {
        return instance_;
    }

    private void OnApplicationQuit()
    {
        this.unregister();
    }

    private void OnDisable()
    {
        this.unregister();
    }

    private void OnEnable()
    {
        this.register();
    }

    public void register()
    {
        Application.logMessageReceived += new Application.LogCallback(this.callback);
    }

    private void reportUnhandledException(string message, string stackTrace)
    {
        string item = createHash(message + stackTrace);
        if (!this.m_previousExceptions.Contains(item))
        {
            this.m_previousExceptions.Add(item);
            object[] messageArgs = new object[] { message, stackTrace };
            Error.AddDevFatal("Uncaught Exception!\n{0}\nAt:\n{1}", messageArgs);
        }
    }

    public void send(string message, string stackTrace)
    {
        string hash = createHash(message + stackTrace);
        if (!alreadySent(hash))
        {
            sentReports_.Add(hash);
            try
            {
                string s = buildMarkup(message, stackTrace, hash);
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                WWWForm form = new WWWForm();
                form.AddBinaryData("file", bytes, "ReportedIssue.xml", "application/octet-stream");
                WWW www = new WWW(this.submitURL, form);
                this.sendCount_++;
                base.StartCoroutine(this.wait(www));
            }
            catch (SecurityException exception)
            {
                this.unregister();
                Blizzard.Log.Error("Unable to send error report (security): " + exception.Message);
            }
            catch (Exception exception2)
            {
                this.unregister();
                Blizzard.Log.Error("Unable to send error report (unknown): " + exception2.Message);
            }
        }
    }

    public void unregister()
    {
        Application.logMessageReceived -= new Application.LogCallback(this.callback);
    }

    [DebuggerHidden]
    private IEnumerator wait(WWW www)
    {
        return new <wait>c__Iterator259 { www = www, <$>www = www, <>f__this = this };
    }

    public bool busy
    {
        get
        {
            return (this.sendCount_ > 0);
        }
    }

    private IPAddress ipAddress
    {
        get
        {
            try
            {
                foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return address;
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (ArgumentException)
            {
            }
            return this.unknownAddress_;
        }
    }

    private static string localTime
    {
        get
        {
            return DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en-US"));
        }
    }

    private string submitURL
    {
        get
        {
            return ("http://iir.blizzard.com:3724/submit/" + 70);
        }
    }

    [CompilerGenerated]
    private sealed class <wait>c__Iterator259 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal WWW <$>www;
        internal ErrorReporter <>f__this;
        internal WWW www;

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
                    this.$current = this.www;
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.sendCount_--;
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
}

