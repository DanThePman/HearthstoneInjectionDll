using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class Store : UIBPopup
{
    protected bool m_buyButtonsEnabled = true;
    [CustomEditField(Sections="Store/UI")]
    public UIBButton m_buyWithGoldButton;
    private List<BuyWithGoldGTAPPListener> m_buyWithGoldGTAPPListeners = new List<BuyWithGoldGTAPPListener>();
    private List<BuyWithGoldNoGTAPPListener> m_buyWithGoldNoGTAPPListeners = new List<BuyWithGoldNoGTAPPListener>();
    [CustomEditField(Sections="Store/UI")]
    public TooltipZone m_buyWithGoldTooltip;
    [CustomEditField(Sections="Store/UI")]
    public PegUIElement m_buyWithGoldTooltipTrigger;
    [CustomEditField(Sections="Store/UI")]
    public UIBButton m_buyWithMoneyButton;
    private List<BuyWithMoneyListener> m_buyWithMoneyListeners = new List<BuyWithMoneyListener>();
    [CustomEditField(Sections="Store/UI")]
    public TooltipZone m_buyWithMoneyTooltip;
    [CustomEditField(Sections="Store/UI")]
    public PegUIElement m_buyWithMoneyTooltipTrigger;
    [CustomEditField(Sections="Store/UI")]
    public GameObject m_cover;
    [CustomEditField(Sections="Store/Materials")]
    public Material m_disabledGoldButtonMaterial;
    [CustomEditField(Sections="Store/Materials")]
    public Material m_disabledMoneyButtonMaterial;
    [CustomEditField(Sections="Store/Materials")]
    public Material m_enabledGoldButtonMaterial;
    [CustomEditField(Sections="Store/Materials")]
    public Material m_enabledMoneyButtonMaterial;
    private List<ExitListener> m_exitListeners = new List<ExitListener>();
    [CustomEditField(Sections="Store/Materials")]
    public List<MeshRenderer> m_goldButtonMeshes = new List<MeshRenderer>();
    private BuyButtonState m_goldButtonState;
    [CustomEditField(Sections="Store/UI")]
    public UIBButton m_infoButton;
    private List<InfoListener> m_infoListeners = new List<InfoListener>();
    [CustomEditField(Sections="Store/Materials")]
    public List<MeshRenderer> m_moneyButtonMeshes = new List<MeshRenderer>();
    private BuyButtonState m_moneyButtonState;
    [CustomEditField(Sections="Store/UI")]
    public PegUIElement m_offClicker;
    private List<ReadyListener> m_readyListeners = new List<ReadyListener>();
    [CustomEditField(Sections="Store/UI")]
    public GameObject m_root;
    protected StoreType m_storeType;

    public void ActivateCover(bool coverActive)
    {
        this.m_cover.SetActive(coverActive);
        this.EnableBuyButtons(!coverActive);
    }

    protected bool AllowBuyingWithGold()
    {
        return (this.m_buyButtonsEnabled && (BuyButtonState.ENABLED == this.m_goldButtonState));
    }

    protected bool AllowBuyingWithMoney()
    {
        return (this.m_buyButtonsEnabled && (BuyButtonState.ENABLED == this.m_moneyButtonState));
    }

    protected virtual void Awake()
    {
        this.m_infoButton.SetText(GameStrings.Get("GLUE_STORE_INFO_BUTTON_TEXT"));
    }

    protected virtual void BuyWithGold(UIEvent e)
    {
    }

    protected virtual void BuyWithMoney(UIEvent e)
    {
    }

    public virtual void Close()
    {
    }

    private void EnableBuyButtons(bool buyButtonsEnabled)
    {
        this.m_buyButtonsEnabled = buyButtonsEnabled;
        this.UpdateBuyButtonsState();
    }

    public void EnableClickCatcher(bool enabled)
    {
        this.m_offClicker.gameObject.SetActive(enabled);
    }

    protected void EnableFullScreenEffects(bool enable)
    {
        if (FullScreenFXMgr.Get() != null)
        {
            if (enable)
            {
                FullScreenFXMgr.Get().StartStandardBlurVignette(1f);
            }
            else
            {
                FullScreenFXMgr.Get().EndStandardBlurVignette(1f, null);
            }
        }
    }

    protected void FireBuyWithGoldEventGTAPP(string productID, int quantity)
    {
        foreach (BuyWithGoldGTAPPListener listener in this.m_buyWithGoldGTAPPListeners.ToArray())
        {
            listener.Fire(productID, quantity);
        }
    }

    protected void FireBuyWithGoldEventNoGTAPP(NoGTAPPTransactionData noGTAPPTransactionData)
    {
        foreach (BuyWithGoldNoGTAPPListener listener in this.m_buyWithGoldNoGTAPPListeners.ToArray())
        {
            listener.Fire(noGTAPPTransactionData);
        }
    }

    protected void FireBuyWithMoneyEvent(string productID, int quantity)
    {
        foreach (BuyWithMoneyListener listener in this.m_buyWithMoneyListeners.ToArray())
        {
            listener.Fire(productID, quantity);
        }
    }

    public void FireExitEvent(bool authorizationBackButtonPressed)
    {
        foreach (ExitListener listener in this.m_exitListeners.ToArray())
        {
            listener.Fire(authorizationBackButtonPressed);
        }
    }

    private string GetBuyButtonTooltipMessage(BuyButtonState state)
    {
        switch (state)
        {
            case BuyButtonState.DISABLED_NOT_ENOUGH_GOLD:
                return GameStrings.Get("GLUE_STORE_FAIL_NOT_ENOUGH_GOLD");

            case BuyButtonState.DISABLED_FEATURE:
                return GameStrings.Get("GLUE_STORE_DISABLED");

            case BuyButtonState.DISABLED_OWNED:
                return this.GetOwnedTooltipString();
        }
        return GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
    }

    protected virtual string GetOwnedTooltipString()
    {
        return GameStrings.Get("GLUE_STORE_DUNGEON_BUTTON_TEXT_PURCHASED");
    }

    public StoreType GetStoreType()
    {
        return this.m_storeType;
    }

    public bool IsCovered()
    {
        return this.m_cover.activeSelf;
    }

    public virtual bool IsReady()
    {
        return true;
    }

    [DebuggerHidden]
    private IEnumerator NotifyListenersWhenReady()
    {
        return new <NotifyListenersWhenReady>c__IteratorA { <>f__this = this };
    }

    protected void OnBuyWithGoldPressed(UIEvent e)
    {
        if (this.AllowBuyingWithGold())
        {
            this.BuyWithGold(e);
        }
    }

    protected void OnBuyWithMoneyPressed(UIEvent e)
    {
        if (this.AllowBuyingWithMoney())
        {
            this.BuyWithMoney(e);
        }
    }

    protected virtual void OnDestroy()
    {
        this.m_enabledGoldButtonMaterial = null;
        this.m_disabledGoldButtonMaterial = null;
        this.m_enabledMoneyButtonMaterial = null;
        this.m_disabledMoneyButtonMaterial = null;
        if (FullScreenFXMgr.Get() != null)
        {
            this.EnableFullScreenEffects(false);
        }
    }

    public virtual void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
    {
    }

    public virtual void OnGoldSpent()
    {
    }

    protected void OnHideBuyWithGoldTooltip(UIEvent e)
    {
        if (this.m_goldButtonState != BuyButtonState.ENABLED)
        {
            this.m_buyWithGoldTooltip.HideTooltip();
        }
    }

    protected void OnHideBuyWithMoneyTooltip(UIEvent e)
    {
        if (this.m_moneyButtonState != BuyButtonState.ENABLED)
        {
            this.m_buyWithMoneyTooltip.HideTooltip();
        }
    }

    protected void OnInfoPressed(UIEvent e)
    {
        foreach (InfoListener listener in this.m_infoListeners.ToArray())
        {
            listener.Fire();
        }
    }

    public virtual void OnMoneySpent()
    {
    }

    protected void OnShowBuyWithGoldTooltip(UIEvent e)
    {
        if (this.m_goldButtonState != BuyButtonState.ENABLED)
        {
            this.ShowBuyTooltip(this.m_buyWithGoldTooltip, "GLUE_STORE_GOLD_BUTTON_TOOLTIP_HEADLINE", this.m_goldButtonState);
        }
    }

    protected void OnShowBuyWithMoneyTooltip(UIEvent e)
    {
        if (this.m_moneyButtonState != BuyButtonState.ENABLED)
        {
            this.ShowBuyTooltip(this.m_buyWithMoneyTooltip, "GLUE_STORE_MONEY_BUTTON_TOOLTIP_HEADLINE", this.m_moneyButtonState);
        }
    }

    public bool RegisterBuyWithGoldGTAPPListener(BuyWithGoldGTAPPCallback callback)
    {
        return this.RegisterBuyWithGoldGTAPPListener(callback, null);
    }

    public bool RegisterBuyWithGoldGTAPPListener(BuyWithGoldGTAPPCallback callback, object userData)
    {
        BuyWithGoldGTAPPListener item = new BuyWithGoldGTAPPListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_buyWithGoldGTAPPListeners.Contains(item))
        {
            return false;
        }
        this.m_buyWithGoldGTAPPListeners.Add(item);
        return true;
    }

    public bool RegisterBuyWithGoldNoGTAPPListener(BuyWithGoldNoGTAPPCallback callback)
    {
        return this.RegisterBuyWithGoldNoGTAPPListener(callback, null);
    }

    public bool RegisterBuyWithGoldNoGTAPPListener(BuyWithGoldNoGTAPPCallback callback, object userData)
    {
        BuyWithGoldNoGTAPPListener item = new BuyWithGoldNoGTAPPListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_buyWithGoldNoGTAPPListeners.Contains(item))
        {
            return false;
        }
        this.m_buyWithGoldNoGTAPPListeners.Add(item);
        return true;
    }

    public bool RegisterBuyWithMoneyListener(BuyWithMoneyCallback callback)
    {
        return this.RegisterBuyWithMoneyListener(callback, null);
    }

    public bool RegisterBuyWithMoneyListener(BuyWithMoneyCallback callback, object userData)
    {
        BuyWithMoneyListener item = new BuyWithMoneyListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_buyWithMoneyListeners.Contains(item))
        {
            return false;
        }
        this.m_buyWithMoneyListeners.Add(item);
        return true;
    }

    public bool RegisterExitListener(ExitCallback callback)
    {
        return this.RegisterExitListener(callback, null);
    }

    public bool RegisterExitListener(ExitCallback callback, object userData)
    {
        ExitListener item = new ExitListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_exitListeners.Contains(item))
        {
            return false;
        }
        this.m_exitListeners.Add(item);
        return true;
    }

    public bool RegisterInfoListener(InfoCallback callback)
    {
        return this.RegisterInfoListener(callback, null);
    }

    public bool RegisterInfoListener(InfoCallback callback, object userData)
    {
        InfoListener item = new InfoListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_infoListeners.Contains(item))
        {
            return false;
        }
        this.m_infoListeners.Add(item);
        return true;
    }

    public bool RegisterReadyListener(ReadyCallback callback)
    {
        return this.RegisterReadyListener(callback, null);
    }

    public bool RegisterReadyListener(ReadyCallback callback, object userData)
    {
        ReadyListener item = new ReadyListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_readyListeners.Contains(item))
        {
            return false;
        }
        this.m_readyListeners.Add(item);
        return true;
    }

    public bool RemoveBuyWithGoldGTAPPListener(BuyWithGoldGTAPPCallback callback)
    {
        return this.RemoveBuyWithGoldGTAPPListener(callback, null);
    }

    public bool RemoveBuyWithGoldGTAPPListener(BuyWithGoldGTAPPCallback callback, object userData)
    {
        BuyWithGoldGTAPPListener item = new BuyWithGoldGTAPPListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_buyWithGoldGTAPPListeners.Remove(item);
    }

    public bool RemoveBuyWithGoldNoGTAPPListener(BuyWithGoldNoGTAPPCallback callback)
    {
        return this.RemoveBuyWithGoldNoGTAPPListener(callback, null);
    }

    public bool RemoveBuyWithGoldNoGTAPPListener(BuyWithGoldNoGTAPPCallback callback, object userData)
    {
        BuyWithGoldNoGTAPPListener item = new BuyWithGoldNoGTAPPListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_buyWithGoldNoGTAPPListeners.Remove(item);
    }

    public bool RemoveBuyWithMoneyListener(BuyWithMoneyCallback callback)
    {
        return this.RemoveBuyWithMoneyListener(callback, null);
    }

    public bool RemoveBuyWithMoneyListener(BuyWithMoneyCallback callback, object userData)
    {
        BuyWithMoneyListener item = new BuyWithMoneyListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_buyWithMoneyListeners.Remove(item);
    }

    public bool RemoveExitListener(ExitCallback callback)
    {
        return this.RemoveExitListener(callback, null);
    }

    public bool RemoveExitListener(ExitCallback callback, object userData)
    {
        ExitListener item = new ExitListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_exitListeners.Remove(item);
    }

    public bool RemoveInfoListener(InfoCallback callback)
    {
        return this.RemoveInfoListener(callback, null);
    }

    public bool RemoveInfoListener(InfoCallback callback, object userData)
    {
        InfoListener item = new InfoListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_infoListeners.Remove(item);
    }

    public bool RemoveReadyListener(ReadyCallback callback)
    {
        return this.RemoveReadyListener(callback, null);
    }

    public bool RemoveReadyListener(ReadyCallback callback, object userData)
    {
        ReadyListener item = new ReadyListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_readyListeners.Remove(item);
    }

    protected void SetGoldButtonState(BuyButtonState state)
    {
        this.m_goldButtonState = state;
        this.UpdateBuyButtonsState();
    }

    protected void SetMoneyButtonState(BuyButtonState state)
    {
        this.m_moneyButtonState = state;
        this.UpdateBuyButtonsState();
    }

    public void SetStoreType(StoreType storeType)
    {
        this.m_storeType = storeType;
    }

    public void Show(DelOnStoreShown onStoreShownCB, bool isTotallyFake)
    {
        this.ShowImpl(onStoreShownCB, isTotallyFake);
    }

    private void ShowBuyTooltip(TooltipZone tooltipZone, string tooltipText, BuyButtonState buttonState)
    {
        tooltipZone.ShowLayerTooltip(GameStrings.Get(tooltipText), this.GetBuyButtonTooltipMessage(buttonState));
    }

    protected virtual void ShowImpl(DelOnStoreShown onStoreShownCB, bool isTotallyFake)
    {
    }

    protected virtual void Start()
    {
        this.m_buyWithGoldButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBuyWithGoldPressed));
        this.m_buyWithMoneyButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBuyWithMoneyPressed));
        this.m_buyWithGoldTooltipTrigger.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnShowBuyWithGoldTooltip));
        this.m_buyWithMoneyTooltipTrigger.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnShowBuyWithMoneyTooltip));
        this.m_buyWithGoldTooltipTrigger.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnHideBuyWithGoldTooltip));
        this.m_buyWithMoneyTooltipTrigger.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnHideBuyWithMoneyTooltip));
        this.m_infoButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnInfoPressed));
        base.StartCoroutine(this.NotifyListenersWhenReady());
    }

    private void UpdateBuyButtonMaterial(List<MeshRenderer> renderers, Material material)
    {
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
    }

    private void UpdateBuyButtonsState()
    {
        bool flag = this.AllowBuyingWithMoney();
        bool flag2 = this.AllowBuyingWithGold();
        this.UpdateBuyButtonMaterial(this.m_moneyButtonMeshes, !flag ? this.m_disabledMoneyButtonMaterial : this.m_enabledMoneyButtonMaterial);
        this.UpdateBuyButtonMaterial(this.m_goldButtonMeshes, !flag2 ? this.m_disabledGoldButtonMaterial : this.m_enabledGoldButtonMaterial);
        this.m_buyWithGoldButton.GetComponent<Collider>().enabled = flag2;
        this.m_buyWithGoldTooltipTrigger.GetComponent<Collider>().enabled = !flag2;
        this.m_buyWithMoneyButton.GetComponent<Collider>().enabled = flag;
        this.m_buyWithMoneyTooltipTrigger.GetComponent<Collider>().enabled = !flag;
    }

    [CompilerGenerated]
    private sealed class <NotifyListenersWhenReady>c__IteratorA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Store.ReadyListener[] <$s_62>__1;
        internal int <$s_63>__2;
        internal Store <>f__this;
        internal Store.ReadyListener <listener>__3;
        internal Store.ReadyListener[] <listeners>__0;

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
                case 1:
                    if (!this.<>f__this.IsReady())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<listeners>__0 = this.<>f__this.m_readyListeners.ToArray();
                    this.<$s_62>__1 = this.<listeners>__0;
                    this.<$s_63>__2 = 0;
                    while (this.<$s_63>__2 < this.<$s_62>__1.Length)
                    {
                        this.<listener>__3 = this.<$s_62>__1[this.<$s_63>__2];
                        this.<listener>__3.Fire();
                        this.<$s_63>__2++;
                    }
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

    protected enum BuyButtonState
    {
        ENABLED,
        DISABLED_NOT_ENOUGH_GOLD,
        DISABLED_FEATURE,
        DISABLED,
        DISABLED_OWNED
    }

    public delegate void BuyWithGoldGTAPPCallback(string productID, int quantity, object userData);

    private class BuyWithGoldGTAPPListener : EventListener<Store.BuyWithGoldGTAPPCallback>
    {
        public void Fire(string productID, int quantity)
        {
            base.m_callback(productID, quantity, base.m_userData);
        }
    }

    public delegate void BuyWithGoldNoGTAPPCallback(NoGTAPPTransactionData noGTAPPTransactionData, object userData);

    private class BuyWithGoldNoGTAPPListener : EventListener<Store.BuyWithGoldNoGTAPPCallback>
    {
        public void Fire(NoGTAPPTransactionData noGTAPPTransactionData)
        {
            base.m_callback(noGTAPPTransactionData, base.m_userData);
        }
    }

    public delegate void BuyWithMoneyCallback(string productID, int quantity, object userData);

    private class BuyWithMoneyListener : EventListener<Store.BuyWithMoneyCallback>
    {
        public void Fire(string productID, int quantity)
        {
            base.m_callback(productID, quantity, base.m_userData);
        }
    }

    public delegate void DelOnStoreShown();

    public delegate void ExitCallback(bool authorizationBackButtonPressed, object userData);

    private class ExitListener : EventListener<Store.ExitCallback>
    {
        public void Fire(bool authorizationBackButtonPressed)
        {
            base.m_callback(authorizationBackButtonPressed, base.m_userData);
        }
    }

    public delegate void InfoCallback(object userData);

    private class InfoListener : EventListener<Store.InfoCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public delegate void ReadyCallback(object userData);

    private class ReadyListener : EventListener<Store.ReadyCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }
}

