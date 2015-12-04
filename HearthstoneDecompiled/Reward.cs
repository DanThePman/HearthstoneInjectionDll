using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class Reward : MonoBehaviour
{
    public PegUIElement m_clickCatcher;
    private List<OnClickedListener> m_clickListeners = new List<OnClickedListener>();
    private RewardData m_data;
    private List<OnHideListener> m_hideListeners = new List<OnHideListener>();
    private bool m_ready = true;
    protected RewardBanner m_rewardBanner;
    public GameObject m_rewardBannerBone;
    public RewardBanner m_rewardBannerPrefab;
    public GameObject m_root;
    private bool m_shown;
    private Type m_type;

    protected Reward()
    {
        this.InitData();
    }

    protected virtual void Awake()
    {
        if (this.m_rewardBannerPrefab != null)
        {
            if (UniversalInputManager.UsePhoneUI == null)
            {
                this.m_rewardBanner = UnityEngine.Object.Instantiate<RewardBanner>(this.m_rewardBannerPrefab);
                this.m_rewardBanner.gameObject.SetActive(false);
                this.m_rewardBanner.transform.parent = this.m_rewardBannerBone.transform;
                this.m_rewardBanner.transform.localPosition = Vector3.zero;
            }
            else
            {
                this.m_rewardBanner = (RewardBanner) GameUtils.Instantiate(this.m_rewardBannerPrefab, this.m_rewardBannerBone, false);
            }
        }
        this.EnableClickCatcher(false);
        SoundManager.Get().Load("game_end_reward");
    }

    public void EnableClickCatcher(bool enabled)
    {
        if (this.m_clickCatcher != null)
        {
            this.m_clickCatcher.gameObject.SetActive(enabled);
        }
    }

    public void Hide(bool animate = false)
    {
        if (!animate)
        {
            this.OnHideAnimateComplete();
        }
        else
        {
            iTween.FadeTo(base.gameObject, 0f, RewardUtils.REWARD_HIDE_TIME);
            object[] args = new object[] { "scale", RewardUtils.REWARD_HIDDEN_SCALE, "time", RewardUtils.REWARD_HIDE_TIME, "oncomplete", "OnHideAnimateComplete", "oncompletetarget", base.gameObject };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ScaleTo(base.gameObject, hashtable);
        }
    }

    protected virtual void HideReward()
    {
        this.OnHide();
    }

    protected abstract void InitData();
    public void NotifyLoadedWhenReady(LoadRewardCallbackData loadRewardCallbackData)
    {
        base.StartCoroutine(this.WaitThenNotifyLoaded(loadRewardCallbackData));
    }

    private void OnClickReleased(UIEvent e)
    {
        foreach (OnClickedListener listener in this.m_clickListeners.ToArray())
        {
            listener.Fire(this);
        }
    }

    protected virtual void OnDataSet(bool updateVisuals)
    {
    }

    private void OnHide()
    {
        foreach (OnHideListener listener in this.m_hideListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private void OnHideAnimateComplete()
    {
        this.HideReward();
        this.m_shown = false;
    }

    public bool RegisterClickListener(OnClickedCallback callback)
    {
        return this.RegisterClickListener(callback, null);
    }

    public bool RegisterClickListener(OnClickedCallback callback, object userData)
    {
        OnClickedListener item = new OnClickedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_clickListeners.Contains(item))
        {
            return false;
        }
        this.m_clickListeners.Add(item);
        return true;
    }

    public bool RegisterHideListener(OnHideCallback callback)
    {
        return this.RegisterHideListener(callback, null);
    }

    public bool RegisterHideListener(OnHideCallback callback, object userData)
    {
        OnHideListener item = new OnHideListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_hideListeners.Contains(item))
        {
            return false;
        }
        this.m_hideListeners.Add(item);
        return true;
    }

    public bool RemoveClickListener(OnClickedCallback callback)
    {
        return this.RemoveClickListener(callback, null);
    }

    public bool RemoveClickListener(OnClickedCallback callback, object userData)
    {
        OnClickedListener item = new OnClickedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_clickListeners.Remove(item);
    }

    public void RemoveHideListener(OnHideCallback callback, object userData)
    {
        OnHideListener item = new OnHideListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        this.m_hideListeners.Remove(item);
    }

    public void SetData(RewardData data, bool updateVisuals)
    {
        this.m_data = data;
        this.OnDataSet(updateVisuals);
    }

    protected void SetReady(bool ready)
    {
        this.m_ready = ready;
    }

    protected void SetRewardText(string headline, string details, string source)
    {
        if ((UniversalInputManager.UsePhoneUI != null) && (this.RewardType != Type.GOLD))
        {
            details = string.Empty;
        }
        if (this.m_rewardBanner != null)
        {
            this.m_rewardBanner.SetText(headline, details, source);
        }
    }

    public void Show(bool updateCacheValues)
    {
        this.Data.AcknowledgeNotices();
        if (this.m_rewardBanner != null)
        {
            this.m_rewardBanner.gameObject.SetActive(true);
        }
        this.ShowReward(updateCacheValues);
        this.m_shown = true;
    }

    protected virtual void ShowReward(bool updateCacheValues)
    {
    }

    private void Start()
    {
        if (this.m_clickCatcher != null)
        {
            this.m_clickCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClickReleased));
        }
        this.Hide(false);
    }

    [DebuggerHidden]
    private IEnumerator WaitThenNotifyLoaded(LoadRewardCallbackData loadRewardCallbackData)
    {
        return new <WaitThenNotifyLoaded>c__Iterator1B7 { loadRewardCallbackData = loadRewardCallbackData, <$>loadRewardCallbackData = loadRewardCallbackData, <>f__this = this };
    }

    public RewardData Data
    {
        get
        {
            return this.m_data;
        }
    }

    public bool IsShown
    {
        get
        {
            return this.m_shown;
        }
    }

    public Type RewardType
    {
        get
        {
            return this.Data.RewardType;
        }
    }

    [CompilerGenerated]
    private sealed class <WaitThenNotifyLoaded>c__Iterator1B7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Reward.LoadRewardCallbackData <$>loadRewardCallbackData;
        internal Reward <>f__this;
        internal Reward.LoadRewardCallbackData loadRewardCallbackData;

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
                    if (this.loadRewardCallbackData.m_callback != null)
                    {
                        break;
                    }
                    goto Label_0086;

                case 1:
                    break;

                default:
                    goto Label_0086;
            }
            while (!this.<>f__this.m_ready)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.loadRewardCallbackData.m_callback(this.<>f__this, this.loadRewardCallbackData.m_callbackData);
            this.$PC = -1;
        Label_0086:
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

    public delegate void DelOnRewardLoaded(Reward reward, object callbackData);

    public class LoadRewardCallbackData
    {
        public Reward.DelOnRewardLoaded m_callback;
        public object m_callbackData;
    }

    public delegate void OnClickedCallback(Reward reward, object userData);

    private class OnClickedListener : EventListener<Reward.OnClickedCallback>
    {
        public void Fire(Reward reward)
        {
            base.m_callback(reward, base.m_userData);
        }
    }

    public delegate void OnHideCallback(object userData);

    private class OnHideListener : EventListener<Reward.OnHideCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public enum Type
    {
        ARCANE_DUST,
        BOOSTER_PACK,
        CARD,
        CARD_BACK,
        CRAFTABLE_CARD,
        FORGE_TICKET,
        GOLD,
        MOUNT,
        CLASS_CHALLENGE
    }
}

