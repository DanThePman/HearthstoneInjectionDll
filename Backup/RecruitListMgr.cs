using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class RecruitListMgr
{
    private List<Network.RecruitInfo> m_lastRecruits = new List<Network.RecruitInfo>();
    private List<RecruitAcceptedListener> m_recruitAcceptedListeners = new List<RecruitAcceptedListener>();
    private List<Network.RecruitInfo> m_recruits = new List<Network.RecruitInfo>();
    private List<RecruitsChangedListener> m_recruitsChangedListeners = new List<RecruitsChangedListener>();
    private ulong s_id;
    private static RecruitListMgr s_instance;

    private RecruitListMgr()
    {
    }

    public bool AddRecruitAcceptedListener(RecruitAcceptedCallback callback)
    {
        RecruitAcceptedListener item = new RecruitAcceptedListener();
        item.SetCallback(callback);
        if (this.m_recruitAcceptedListeners.Contains(item))
        {
            return false;
        }
        this.m_recruitAcceptedListeners.Add(item);
        return true;
    }

    public bool AddRecruitsChangedListener(RecruitsChangedCallback callback)
    {
        RecruitsChangedListener item = new RecruitsChangedListener();
        item.SetCallback(callback);
        if (this.m_recruitsChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_recruitsChangedListeners.Add(item);
        return true;
    }

    public bool CanAddMoreRecruits()
    {
        int num = 0;
        foreach (Network.RecruitInfo info in this.m_recruits)
        {
            if (info.RecruitID.IsEmpty())
            {
                num++;
            }
        }
        return (num < 5);
    }

    private void FireRecruitAcceptedEvent()
    {
        foreach (Network.RecruitInfo info in this.m_lastRecruits)
        {
            Network.RecruitInfo recruitInfoFromId = this.GetRecruitInfoFromId(info.ID);
            if (((recruitInfoFromId != null) && (info.Status != 4)) && (recruitInfoFromId.Status == 4))
            {
                Log.Cameron.Print("comparing recruits", new object[0]);
                Log.Cameron.Print("old recruit " + info, new object[0]);
                Log.Cameron.Print("new recruit " + recruitInfoFromId, new object[0]);
                RecruitAcceptedListener[] listenerArray = this.m_recruitAcceptedListeners.ToArray();
                for (int i = 0; i < listenerArray.Length; i++)
                {
                    Log.Cameron.Print("recruit accepted " + recruitInfoFromId.Nickname, new object[0]);
                    listenerArray[i].Fire(recruitInfoFromId);
                }
            }
        }
    }

    private void FireRecruitsChangedEvent()
    {
        RecruitsChangedListener[] listenerArray = this.m_recruitsChangedListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire();
        }
    }

    public static RecruitListMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new RecruitListMgr();
        }
        return s_instance;
    }

    public Network.RecruitInfo GetRecruitInfoFromAccountId(BnetAccountId gameAccountID)
    {
        foreach (Network.RecruitInfo info in this.m_recruits)
        {
            if (info.RecruitID == gameAccountID)
            {
                return info;
            }
        }
        return null;
    }

    public Network.RecruitInfo GetRecruitInfoFromId(ulong uniqueID)
    {
        foreach (Network.RecruitInfo info in this.m_recruits)
        {
            if (info.ID == uniqueID)
            {
                return info;
            }
        }
        return null;
    }

    public List<Network.RecruitInfo> GetRecruitList()
    {
        return this.m_recruits;
    }

    public void Init()
    {
        if (s_instance == null)
        {
            s_instance = new RecruitListMgr();
        }
    }

    public static bool IsValidRecruitInput(string email)
    {
        return (FriendUtils.IsValidEmail(email) || true);
    }

    private void OnRecruitListResponse()
    {
        Log.Cameron.Print("recruit list response!", new object[0]);
        this.FireRecruitsChangedEvent();
        this.FireRecruitAcceptedEvent();
        this.m_lastRecruits = new List<Network.RecruitInfo>(this.m_recruits);
    }

    private void OnReset()
    {
        this.m_recruits.Clear();
    }

    public void RecruitFriendCancel(ulong uniqueID)
    {
        Network.RecruitInfo recruitInfoFromId = this.GetRecruitInfoFromId(uniqueID);
        if (recruitInfoFromId != null)
        {
            this.m_recruits.Remove(recruitInfoFromId);
            this.FireRecruitsChangedEvent();
        }
    }

    public void RefreshRecruitList()
    {
        this.OnRecruitListResponse();
    }

    public bool RemoveRecruitsChangedListener(RecruitsChangedCallback callback)
    {
        RecruitsChangedListener item = new RecruitsChangedListener();
        item.SetCallback(callback);
        return this.m_recruitsChangedListeners.Remove(item);
    }

    private int secondsSinceEpoch()
    {
        TimeSpan span = (TimeSpan) (DateTime.UtcNow - new DateTime(0x7b2, 1, 1));
        return (int) span.TotalSeconds;
    }

    public void SendRecruitAFriendInvite(string email)
    {
        Network.RecruitInfo info;
        Network.RecruitInfo info3;
        char[] separator = new char[] { ':' };
        string[] strArray = email.Split(separator);
        ulong result = 0L;
        if (ulong.TryParse(strArray[0], out result))
        {
            BnetAccountId gameAccountID = new BnetAccountId();
            gameAccountID.SetHi(0x100000000000000L);
            gameAccountID.SetLo(result);
            info3 = new Network.RecruitInfo {
                RecruitID = gameAccountID
            };
            info = info3;
            Network.RecruitInfo recruitInfoFromAccountId = this.GetRecruitInfoFromAccountId(gameAccountID);
            if (recruitInfoFromAccountId != null)
            {
                this.m_recruits.Remove(recruitInfoFromAccountId);
            }
        }
        else
        {
            ulong num2;
            info3 = new Network.RecruitInfo();
            this.s_id = (num2 = this.s_id) + ((ulong) 1L);
            info3.ID = num2;
            info = info3;
        }
        info.Nickname = strArray[0];
        if (strArray.Length > 1)
        {
            info.Status = int.Parse(strArray[1]);
        }
        if (strArray.Length > 2)
        {
            info.Level = int.Parse(strArray[2]);
        }
        info.CreationTimeMicrosec = (ulong) ((this.secondsSinceEpoch() * 0x3e8L) * 0x3e8L);
        this.m_recruits.Add(info);
        this.OnRecruitListResponse();
    }

    public delegate void RecruitAcceptedCallback(Network.RecruitInfo recruit);

    private class RecruitAcceptedListener : EventListener<RecruitListMgr.RecruitAcceptedCallback>
    {
        public void Fire(Network.RecruitInfo info)
        {
            base.m_callback(info);
        }
    }

    public delegate void RecruitsChangedCallback();

    private class RecruitsChangedListener : EventListener<RecruitListMgr.RecruitsChangedCallback>
    {
        public void Fire()
        {
            base.m_callback();
        }
    }

    public enum RecruitStatus
    {
        NOTHING,
        PENDING,
        INELIGIBLE,
        FAILED,
        ACCEPTED
    }
}

