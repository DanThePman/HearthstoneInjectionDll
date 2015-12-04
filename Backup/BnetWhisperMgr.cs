using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class BnetWhisperMgr
{
    [CompilerGenerated]
    private static System.Action <>f__am$cache5;
    [CompilerGenerated]
    private static Comparison<BnetWhisper> <>f__am$cache6;
    private int m_firstPendingWhisperIndex = -1;
    private List<WhisperListener> m_whisperListeners = new List<WhisperListener>();
    private Map<BnetGameAccountId, List<BnetWhisper>> m_whisperMap = new Map<BnetGameAccountId, List<BnetWhisper>>();
    private List<BnetWhisper> m_whispers = new List<BnetWhisper>();
    private const int MAX_WHISPERS_PER_PLAYER = 100;
    private static BnetWhisperMgr s_instance;

    public bool AddWhisperListener(WhisperCallback callback)
    {
        return this.AddWhisperListener(callback, null);
    }

    public bool AddWhisperListener(WhisperCallback callback, object userData)
    {
        WhisperListener item = new WhisperListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_whisperListeners.Contains(item))
        {
            return false;
        }
        this.m_whisperListeners.Add(item);
        return true;
    }

    private bool CanProcessPendingWhispers()
    {
        if (this.m_firstPendingWhisperIndex >= 0)
        {
            for (int i = this.m_firstPendingWhisperIndex; i < this.m_whispers.Count; i++)
            {
                BnetWhisper whisper = this.m_whispers[i];
                if (!whisper.IsDisplayable())
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void FireWhisperEvent(BnetWhisper whisper)
    {
        foreach (WhisperListener listener in this.m_whisperListeners.ToArray())
        {
            listener.Fire(whisper);
        }
    }

    public static BnetWhisperMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new BnetWhisperMgr();
            if (<>f__am$cache5 == null)
            {
                <>f__am$cache5 = delegate {
                    s_instance.m_whispers.Clear();
                    s_instance.m_whisperMap.Clear();
                    s_instance.m_firstPendingWhisperIndex = -1;
                    BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(Get().OnPlayersChanged));
                };
            }
            ApplicationMgr.Get().WillReset += <>f__am$cache5;
        }
        return s_instance;
    }

    public List<BnetWhisper> GetWhispersWithPlayer(BnetPlayer player)
    {
        if (player == null)
        {
            return null;
        }
        List<BnetWhisper> list = new List<BnetWhisper>();
        foreach (BnetGameAccountId id in player.GetGameAccounts().Keys)
        {
            List<BnetWhisper> list2;
            if (this.m_whisperMap.TryGetValue(id, out list2))
            {
                list.AddRange(list2);
            }
        }
        if (list.Count == 0)
        {
            return null;
        }
        if (<>f__am$cache6 == null)
        {
            <>f__am$cache6 = delegate (BnetWhisper a, BnetWhisper b) {
                ulong timestampMicrosec = a.GetTimestampMicrosec();
                ulong num2 = b.GetTimestampMicrosec();
                if (timestampMicrosec < num2)
                {
                    return -1;
                }
                if (timestampMicrosec > num2)
                {
                    return 1;
                }
                return 0;
            };
        }
        list.Sort(<>f__am$cache6);
        return list;
    }

    public bool HavePendingWhispers()
    {
        return (this.m_firstPendingWhisperIndex >= 0);
    }

    public void Initialize()
    {
        Network.Get().SetWhisperHandler(new Network.WhisperHandler(this.OnWhispers));
        Network.Get().AddBnetErrorListener(BnetFeature.Whisper, new Network.BnetErrorCallback(this.OnBnetError));
    }

    private bool OnBnetError(BnetErrorInfo info, object userData)
    {
        object[] args = new object[] { info.GetFeatureEvent(), info.GetError() };
        Log.Mike.Print("BnetWhisperMgr.OnBnetError() - event={0} error={1}", args);
        return true;
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        if (this.CanProcessPendingWhispers())
        {
            BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
            this.ProcessPendingWhispers();
        }
    }

    private void OnWhispers(BnetWhisper[] whispers)
    {
        for (int i = 0; i < whispers.Length; i++)
        {
            BnetWhisper item = whispers[i];
            this.m_whispers.Add(item);
            if (!this.HavePendingWhispers())
            {
                if (item.IsDisplayable())
                {
                    this.ProcessWhisper(this.m_whispers.Count - 1);
                }
                else
                {
                    this.m_firstPendingWhisperIndex = i;
                    BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
                }
            }
        }
    }

    private void ProcessPendingWhispers()
    {
        if (this.m_firstPendingWhisperIndex >= 0)
        {
            for (int i = this.m_firstPendingWhisperIndex; i < this.m_whispers.Count; i++)
            {
                this.ProcessWhisper(i);
            }
            this.m_firstPendingWhisperIndex = -1;
        }
    }

    private void ProcessWhisper(int index)
    {
        BnetWhisper item = this.m_whispers[index];
        BnetGameAccountId theirGameAccountId = item.GetTheirGameAccountId();
        if (!BnetUtils.CanReceiveWhisperFrom(theirGameAccountId))
        {
            this.m_whispers.RemoveAt(index);
        }
        else
        {
            List<BnetWhisper> list;
            if (!this.m_whisperMap.TryGetValue(theirGameAccountId, out list))
            {
                list = new List<BnetWhisper>();
                this.m_whisperMap.Add(theirGameAccountId, list);
            }
            else if (list.Count == 100)
            {
                this.RemoveOldestWhisper(list);
            }
            list.Add(item);
            this.FireWhisperEvent(item);
        }
    }

    private void RemoveOldestWhisper(List<BnetWhisper> whispers)
    {
        BnetWhisper item = whispers[0];
        whispers.RemoveAt(0);
        this.m_whispers.Remove(item);
    }

    public bool RemoveWhisperListener(WhisperCallback callback)
    {
        return this.RemoveWhisperListener(callback, null);
    }

    public bool RemoveWhisperListener(WhisperCallback callback, object userData)
    {
        WhisperListener item = new WhisperListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_whisperListeners.Remove(item);
    }

    public bool SendWhisper(BnetPlayer player, string message)
    {
        if (player == null)
        {
            return false;
        }
        BnetGameAccount bestGameAccount = player.GetBestGameAccount();
        if (bestGameAccount == null)
        {
            return false;
        }
        Network.SendWhisper(bestGameAccount.GetId(), message);
        return true;
    }

    public void Shutdown()
    {
        Network.Get().RemoveBnetErrorListener(BnetFeature.Whisper, new Network.BnetErrorCallback(this.OnBnetError));
        Network.Get().SetWhisperHandler(null);
    }

    public delegate void WhisperCallback(BnetWhisper whisper, object userData);

    private class WhisperListener : EventListener<BnetWhisperMgr.WhisperCallback>
    {
        public void Fire(BnetWhisper whisper)
        {
            base.m_callback(whisper, base.m_userData);
        }
    }
}

