using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ArenaStore : Store
{
    public UIBButton m_backButton;
    private Network.Bundle m_bundle;
    private NoGTAPPTransactionData m_goldTransactionData;
    public GameObject m_storeClosed;
    private static readonly int NUM_BUNDLE_ITEMS_REQUIRED = 1;
    private static ArenaStore s_instance;

    protected override void Awake()
    {
        s_instance = this;
        base.m_destroyOnSceneLoad = false;
        base.Awake();
        this.m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
    }

    protected override void BuyWithGold(UIEvent e)
    {
        if (this.m_goldTransactionData != null)
        {
            base.FireBuyWithGoldEventNoGTAPP(this.m_goldTransactionData);
        }
    }

    protected override void BuyWithMoney(UIEvent e)
    {
        if (this.m_bundle != null)
        {
            base.FireBuyWithMoneyEvent(this.m_bundle.ProductID, 1);
        }
    }

    private void ExitForgeStore(bool authorizationBackButtonPressed)
    {
        base.ActivateCover(false);
        SceneUtils.SetLayer(base.gameObject, GameLayer.Default);
        base.EnableFullScreenEffects(false);
        StoreManager.Get().RemoveAuthorizationExitListener(new StoreManager.AuthorizationExitCallback(this.OnAuthExit));
        base.FireExitEvent(authorizationBackButtonPressed);
    }

    public static ArenaStore Get()
    {
        return s_instance;
    }

    public override void Hide()
    {
        StoreManager.Get().RemoveAuthorizationExitListener(new StoreManager.AuthorizationExitCallback(this.OnAuthExit));
        base.EnableFullScreenEffects(false);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            BnetBar.Get().SetCurrencyType(null);
        }
        base.Hide();
    }

    private void OnAuthExit(object userData)
    {
        Navigation.Pop();
        this.ExitForgeStore(true);
    }

    private void OnBackPressed(UIEvent e)
    {
        Navigation.GoBack();
    }

    protected override void OnDestroy()
    {
        s_instance = null;
    }

    public override void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
    {
        this.UpdateGoldButtonState(balance);
    }

    public override void OnMoneySpent()
    {
        this.UpdateMoneyButtonState();
    }

    private bool OnNavigateBack()
    {
        this.ExitForgeStore(false);
        return true;
    }

    private void SetUpBuyButtons()
    {
        this.SetUpBuyWithGoldButton();
        this.SetUpBuyWithMoneyButton();
    }

    private void SetUpBuyWithGoldButton()
    {
        string text = string.Empty;
        NoGTAPPTransactionData noGTAPPTransactionData = new NoGTAPPTransactionData {
            Product = ProductType.PRODUCT_TYPE_DRAFT,
            ProductData = 0,
            Quantity = 1
        };
        long goldCostNoGTAPP = StoreManager.Get().GetGoldCostNoGTAPP(noGTAPPTransactionData);
        if (goldCostNoGTAPP > 0L)
        {
            this.m_goldTransactionData = noGTAPPTransactionData;
            text = goldCostNoGTAPP.ToString();
            NetCache.NetCacheGoldBalance netObject = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>();
            this.UpdateGoldButtonState(netObject);
        }
        else
        {
            Debug.LogWarning("ForgeStore.SetUpBuyWithGoldButton(): no gold price for purchase Arena without GTAPP");
            text = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
            base.SetGoldButtonState(Store.BuyButtonState.DISABLED);
        }
        base.m_buyWithGoldButton.SetText(text);
    }

    private void SetUpBuyWithMoneyButton()
    {
        bool productExists = false;
        List<Network.Bundle> list = StoreManager.Get().GetAvailableBundlesForProduct(ProductType.PRODUCT_TYPE_DRAFT, out productExists, 0, NUM_BUNDLE_ITEMS_REQUIRED);
        string text = string.Empty;
        if (list.Count == 1)
        {
            this.m_bundle = list[0];
            text = StoreManager.Get().FormatCostBundle(this.m_bundle);
            this.UpdateMoneyButtonState();
        }
        else
        {
            Debug.LogWarning(string.Format("ForgeStore.SetUpBuyWithMoneyButton(): expecting 1 bundle for purchasing Forge, found {0}", list.Count));
            text = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
            base.SetMoneyButtonState(Store.BuyButtonState.DISABLED);
        }
        base.m_buyWithMoneyButton.SetText(text);
    }

    protected override void ShowImpl(Store.DelOnStoreShown onStoreShownCB, bool isTotallyFake)
    {
        <ShowImpl>c__AnonStorey32D storeyd = new <ShowImpl>c__AnonStorey32D {
            isTotallyFake = isTotallyFake,
            onStoreShownCB = onStoreShownCB,
            <>f__this = this
        };
        base.m_shown = true;
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
        StoreManager.Get().RegisterAuthorizationExitListener(new StoreManager.AuthorizationExitCallback(this.OnAuthExit));
        base.EnableFullScreenEffects(true);
        this.SetUpBuyButtons();
        base.DoShowAnimation(new UIBPopup.OnAnimationComplete(storeyd.<>m__163));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            BnetBar.Get().SetCurrencyType(1);
        }
    }

    protected override void Start()
    {
        base.Start();
        this.m_backButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBackPressed));
    }

    private void UpdateGoldButtonState(NetCache.NetCacheGoldBalance balance)
    {
        Store.BuyButtonState eNABLED = Store.BuyButtonState.ENABLED;
        if (this.m_goldTransactionData == null)
        {
            eNABLED = Store.BuyButtonState.DISABLED;
        }
        else
        {
            long goldCostNoGTAPP = StoreManager.Get().GetGoldCostNoGTAPP(this.m_goldTransactionData);
            if (!StoreManager.Get().IsOpen())
            {
                eNABLED = Store.BuyButtonState.DISABLED;
            }
            else if (!StoreManager.Get().IsBuyWithGoldFeatureEnabled())
            {
                eNABLED = Store.BuyButtonState.DISABLED_FEATURE;
            }
            else if (balance == null)
            {
                eNABLED = Store.BuyButtonState.DISABLED;
            }
            else if (balance.GetTotal() < goldCostNoGTAPP)
            {
                eNABLED = Store.BuyButtonState.DISABLED_NOT_ENOUGH_GOLD;
            }
        }
        base.SetGoldButtonState(eNABLED);
    }

    private void UpdateMoneyButtonState()
    {
        Store.BuyButtonState eNABLED = Store.BuyButtonState.ENABLED;
        if ((this.m_bundle == null) || !StoreManager.Get().IsOpen())
        {
            eNABLED = Store.BuyButtonState.DISABLED;
            this.m_storeClosed.SetActive(true);
        }
        else if (!StoreManager.Get().IsBattlePayFeatureEnabled())
        {
            eNABLED = Store.BuyButtonState.DISABLED_FEATURE;
        }
        else
        {
            this.m_storeClosed.SetActive(false);
        }
        base.SetMoneyButtonState(eNABLED);
    }

    [CompilerGenerated]
    private sealed class <ShowImpl>c__AnonStorey32D
    {
        internal ArenaStore <>f__this;
        internal bool isTotallyFake;
        internal Store.DelOnStoreShown onStoreShownCB;

        internal void <>m__163()
        {
            if (this.isTotallyFake)
            {
                this.<>f__this.m_buyWithGoldButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.OnBuyWithGoldPressed));
                this.<>f__this.m_buyWithMoneyButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.OnBuyWithMoneyPressed));
                this.<>f__this.m_infoButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.OnInfoPressed));
            }
            if (this.onStoreShownCB != null)
            {
                this.onStoreShownCB();
            }
        }
    }
}

