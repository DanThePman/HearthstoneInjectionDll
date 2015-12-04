using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BreakingNews : MonoBehaviour
{
    [CompilerGenerated]
    private static BreakingNewsRecievedDelegate <>f__am$cache6;
    private string m_error;
    private Status m_status;
    private string m_text = string.Empty;
    private float m_timeFetched;
    private static BreakingNews s_instance;
    public static bool SHOWS_BREAKING_NEWS;
    private const float TIMEOUT = 15f;

    public void Awake()
    {
        SHOWS_BREAKING_NEWS = (bool) Network.TUTORIALS_WITHOUT_ACCOUNT;
        s_instance = this;
    }

    public void Fetch()
    {
        if (SHOWS_BREAKING_NEWS)
        {
            this.m_error = null;
            this.m_status = Status.Fetching;
            this.m_text = string.Empty;
            this.m_timeFetched = UnityEngine.Time.realtimeSinceStartup;
            if (<>f__am$cache6 == null)
            {
                <>f__am$cache6 = delegate (string response, bool error) {
                    if (error)
                    {
                        s_instance.OnBreakingNewsError(response);
                    }
                    else
                    {
                        s_instance.OnBreakingNewsResponse(response);
                    }
                };
            }
            FetchBreakingNews(NydusLink.GetBreakingNewsLink(), <>f__am$cache6);
        }
    }

    public static void FetchBreakingNews(string url, BreakingNewsRecievedDelegate callback)
    {
        WWW request = new WWW(url);
        s_instance.StartCoroutine(s_instance.FetchBreakingNewsProgress(request, callback));
    }

    [DebuggerHidden]
    public IEnumerator FetchBreakingNewsProgress(WWW request, BreakingNewsRecievedDelegate callback)
    {
        return new <FetchBreakingNewsProgress>c__Iterator238 { request = request, callback = callback, <$>request = request, <$>callback = callback };
    }

    public static BreakingNews Get()
    {
        return s_instance;
    }

    public string GetError()
    {
        return this.m_error;
    }

    public Status GetStatus()
    {
        if (!SHOWS_BREAKING_NEWS)
        {
            return Status.Available;
        }
        if ((this.m_status == Status.Fetching) && ((UnityEngine.Time.realtimeSinceStartup - this.m_timeFetched) > 15f))
        {
            this.m_status = Status.TimedOut;
        }
        return this.m_status;
    }

    public string GetText()
    {
        if (SHOWS_BREAKING_NEWS)
        {
            if ((this.m_status != Status.Fetching) && (this.m_status != Status.TimedOut))
            {
                return this.m_text;
            }
            UnityEngine.Debug.LogError(string.Format("Fetched breaking news when it was unavailable, status={0}", this.m_status));
        }
        return string.Empty;
    }

    public void OnBreakingNewsError(string error)
    {
        this.m_error = error;
        object[] args = new object[] { error };
        Log.JMac.Print("Breaking News error received: {0}", args);
    }

    public void OnBreakingNewsResponse(string response)
    {
        object[] args = new object[] { response };
        Log.JMac.Print("Breaking News response received: {0}", args);
        this.m_text = response;
        if ((this.m_text.Length <= 2) || this.m_text.ToLowerInvariant().Contains("<html>"))
        {
            this.m_text = string.Empty;
        }
        this.m_status = Status.Available;
    }

    public void OnDestroy()
    {
        s_instance = null;
    }

    [CompilerGenerated]
    private sealed class <FetchBreakingNewsProgress>c__Iterator238 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal BreakingNews.BreakingNewsRecievedDelegate <$>callback;
        internal WWW <$>request;
        internal BreakingNews.BreakingNewsRecievedDelegate callback;
        internal WWW request;

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
                    break;

                case 1:
                    break;
                    this.$PC = -1;
                    goto Label_00BB;

                default:
                    goto Label_00BB;
            }
            if ((this.request.error != null) && (this.request.error != string.Empty))
            {
                this.callback(this.request.error, true);
            }
            else if (this.request.isDone)
            {
                this.callback(this.request.text, false);
            }
            else
            {
                this.$current = new WaitForSeconds(0.1f);
                this.$PC = 1;
                return true;
            }
        Label_00BB:
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

    public delegate void BreakingNewsRecievedDelegate(string response, bool error);

    public enum Status
    {
        Fetching,
        Available,
        TimedOut
    }
}

