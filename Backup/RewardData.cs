using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RewardData
{
    private bool m_isDummyReward;
    protected List<long> m_noticeIDs = new List<long>();
    private NetCache.ProfileNotice.NoticeOrigin m_origin = NetCache.ProfileNotice.NoticeOrigin.UNKNOWN;
    private long m_originData;
    private Reward.Type m_type;

    protected RewardData(Reward.Type type)
    {
        this.m_type = type;
    }

    public void AcknowledgeNotices()
    {
        long[] numArray = this.m_noticeIDs.ToArray();
        this.m_noticeIDs.Clear();
        foreach (long num in numArray)
        {
            Network.AckNotice(num);
        }
    }

    public void AddNoticeID(long noticeID)
    {
        if (!this.m_noticeIDs.Contains(noticeID))
        {
            this.m_noticeIDs.Add(noticeID);
        }
    }

    protected abstract string GetGameObjectName();
    public List<long> GetNoticeIDs()
    {
        return this.m_noticeIDs;
    }

    public void LoadRewardObject(Reward.DelOnRewardLoaded callback)
    {
        this.LoadRewardObject(callback, null);
    }

    public void LoadRewardObject(Reward.DelOnRewardLoaded callback, object callbackData)
    {
        string gameObjectName = this.GetGameObjectName();
        if (string.IsNullOrEmpty(gameObjectName))
        {
            Debug.LogError(string.Format("Reward.LoadRewardObject(): Do not know how to load reward object for {0}.", this));
        }
        else
        {
            Reward.LoadRewardCallbackData data = new Reward.LoadRewardCallbackData {
                m_callback = callback,
                m_callbackData = callbackData
            };
            AssetLoader.Get().LoadGameObject(gameObjectName, new AssetLoader.GameObjectCallback(this.OnRewardObjectLoaded), data, false);
        }
    }

    public void MarkAsDummyReward()
    {
        this.m_isDummyReward = true;
    }

    private void OnRewardObjectLoaded(string name, GameObject go, object callbackData)
    {
        Reward component = go.GetComponent<Reward>();
        if (component == null)
        {
            Debug.LogWarning(string.Format("Reward.OnRewardObjectLoaded() - game object loaded from {0} has no reward component", name));
        }
        else
        {
            component.SetData(this, true);
        }
        Reward.LoadRewardCallbackData loadRewardCallbackData = callbackData as Reward.LoadRewardCallbackData;
        component.NotifyLoadedWhenReady(loadRewardCallbackData);
    }

    public void SetOrigin(NetCache.ProfileNotice.NoticeOrigin origin, long originData)
    {
        this.m_origin = origin;
        this.m_originData = originData;
    }

    public bool IsDummyReward
    {
        get
        {
            return this.m_isDummyReward;
        }
    }

    public NetCache.ProfileNotice.NoticeOrigin Origin
    {
        get
        {
            return this.m_origin;
        }
    }

    public long OriginData
    {
        get
        {
            return this.m_originData;
        }
    }

    public Reward.Type RewardType
    {
        get
        {
            return this.m_type;
        }
    }
}

