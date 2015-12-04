using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureStore : Store
{
    private bool m_animating;
    private Network.Bundle m_bundle;
    [CustomEditField(Sections="UI")]
    public UIBButton m_BuyDungeonButton;
    [CustomEditField(Sections="UI")]
    public UberText m_DetailsText1;
    [CustomEditField(Sections="UI")]
    public UberText m_DetailsText2;
    private Network.Bundle m_fullDungeonBundle;
    [CustomEditField(Sections="UI")]
    public UberText m_Headline;
    private static readonly int NUM_BUNDLE_ITEMS_REQUIRED = 1;

    protected override void BuyWithGold(UIEvent e)
    {
        if ((this.m_bundle != null) && !this.m_animating)
        {
            base.FireBuyWithGoldEventGTAPP(this.m_bundle.ProductID, 1);
        }
    }

    protected override void BuyWithMoney(UIEvent e)
    {
        if ((this.m_bundle != null) && !this.m_animating)
        {
            base.FireBuyWithMoneyEvent(this.m_bundle.ProductID, 1);
        }
    }

    public override void Close()
    {
        Navigation.GoBack();
    }

    public override void Hide()
    {
        base.m_shown = false;
        StoreManager.Get().RemoveAuthorizationExitListener(new StoreManager.AuthorizationExitCallback(this.OnAuthExit));
        base.EnableFullScreenEffects(false);
        base.DoHideAnimation(null);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            BnetBar.Get().SetCurrencyType(null);
        }
    }

    private void OnAuthExit(object userData)
    {
        base.ActivateCover(false);
        SceneUtils.SetLayer(base.gameObject, GameLayer.Default);
        base.EnableFullScreenEffects(false);
        StoreManager.Get().RemoveAuthorizationExitListener(new StoreManager.AuthorizationExitCallback(this.OnAuthExit));
        base.FireExitEvent(true);
    }

    private void OnBuyDungeonPressed(UIEvent e)
    {
        this.Close();
        Options.Get().SetInt(Option.LAST_SELECTED_STORE_ADVENTURE_ID, (int) AdventureConfig.Get().GetSelectedAdventure());
        StoreManager.Get().StartGeneralTransaction(GeneralStoreMode.ADVENTURE);
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
        this.Hide();
        base.FireExitEvent(false);
        return true;
    }

    public void SetAdventureProduct(ProductType product, int productData)
    {
        List<Network.BundleItem> items = new List<Network.BundleItem>();
        Network.BundleItem item = new Network.BundleItem {
            Product = product,
            ProductData = productData,
            Quantity = 1
        };
        items.Add(item);
        string productName = StoreManager.Get().GetProductName(items);
        this.m_Headline.Text = productName;
        string str2 = string.Empty;
        switch (product)
        {
            case ProductType.PRODUCT_TYPE_NAXX:
                str2 = "NAXX";
                break;

            case ProductType.PRODUCT_TYPE_BRM:
                str2 = "BRM";
                break;

            case ProductType.PRODUCT_TYPE_LOE:
                str2 = "LOE";
                break;
        }
        string key = string.Format("GLUE_STORE_PRODUCT_DETAILS_{0}_PART_1", str2);
        string str4 = string.Format("GLUE_STORE_PRODUCT_DETAILS_{0}_PART_2", str2);
        object[] args = new object[] { productName };
        this.m_DetailsText1.Text = GameStrings.Format(key, args);
        this.m_DetailsText2.Text = GameStrings.Format(str4, new object[0]);
        bool productExists = false;
        List<Network.Bundle> list2 = StoreManager.Get().GetAvailableBundlesForProduct(product, out productExists, productData, NUM_BUNDLE_ITEMS_REQUIRED);
        if (list2.Count == 1)
        {
            this.m_bundle = list2[0];
        }
        else
        {
            Debug.LogWarning(string.Format("AdventureStore.SetAdventureProduct(): expected to find 1 available bundle for product {0} productData {1}, found {2}", product, productData, list2.Count));
            this.m_bundle = null;
        }
        StoreManager.Get().GetAvailableAdventureBundle(product, out this.m_fullDungeonBundle, out productExists);
    }

    private void SetUpBuyButtons()
    {
        this.SetUpBuyWithGoldButton();
        this.SetUpBuyWithMoneyButton();
        this.SetUpBuyDungeonButton();
    }

    private void SetUpBuyDungeonButton()
    {
        string text = string.Empty;
        if (this.m_fullDungeonBundle != null)
        {
            string str2 = StoreManager.Get().FormatCostBundle(this.m_fullDungeonBundle);
            object[] args = new object[] { this.m_fullDungeonBundle.Items.Count, str2 };
            text = GameStrings.Format("GLUE_STORE_DUNGEON_BUTTON_TEXT", args);
        }
        else
        {
            text = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
        }
        this.m_BuyDungeonButton.SetText(text);
    }

    private void SetUpBuyWithGoldButton()
    {
        string text = string.Empty;
        if (this.m_bundle != null)
        {
            text = this.m_bundle.GoldCost.ToString();
            NetCache.NetCacheGoldBalance netObject = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>();
            this.UpdateGoldButtonState(netObject);
        }
        else
        {
            Debug.LogWarning("AdventureStore.SetUpBuyWithGoldButton(): m_bundle is null");
            text = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
            base.SetMoneyButtonState(Store.BuyButtonState.DISABLED);
        }
        base.m_buyWithGoldButton.SetText(text);
    }

    private void SetUpBuyWithMoneyButton()
    {
        string text = string.Empty;
        if (this.m_bundle != null)
        {
            text = StoreManager.Get().FormatCostBundle(this.m_bundle);
            this.UpdateMoneyButtonState();
        }
        else
        {
            Debug.LogWarning("AdventureStore.SetUpBuyWithMoneyButton(): m_bundle is null");
            text = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
            base.SetMoneyButtonState(Store.BuyButtonState.DISABLED);
        }
        base.m_buyWithMoneyButton.SetText(text);
    }

    protected override void ShowImpl(Store.DelOnStoreShown onStoreShownCB, bool isTotallyFake)
    {
        <ShowImpl>c__AnonStorey32C storeyc = new <ShowImpl>c__AnonStorey32C {
            onStoreShownCB = onStoreShownCB,
            <>f__this = this
        };
        if (!base.m_shown)
        {
            base.m_shown = true;
            Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
            StoreManager.Get().RegisterAuthorizationExitListener(new StoreManager.AuthorizationExitCallback(this.OnAuthExit));
            base.EnableFullScreenEffects(true);
            this.SetUpBuyButtons();
            this.m_animating = true;
            base.DoShowAnimation(new UIBPopup.OnAnimationComplete(storeyc.<>m__162));
            if (UniversalInputManager.UsePhoneUI != null)
            {
                BnetBar.Get().SetCurrencyType(1);
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        this.m_BuyDungeonButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBuyDungeonPressed));
        base.m_offClicker.AddEventListener(UIEventType.RELEASE, e => this.Close());
    }

    private void UpdateGoldButtonState(NetCache.NetCacheGoldBalance balance)
    {
        Store.BuyButtonState eNABLED = Store.BuyButtonState.ENABLED;
        if (this.m_bundle == null)
        {
            eNABLED = Store.BuyButtonState.DISABLED;
        }
        else if (this.m_bundle.GoldCost == 0)
        {
            eNABLED = Store.BuyButtonState.DISABLED;
        }
        else if (!StoreManager.Get().IsOpen())
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
        else if (balance.GetTotal() < this.m_bundle.GoldCost)
        {
            eNABLED = Store.BuyButtonState.DISABLED_NOT_ENOUGH_GOLD;
        }
        base.SetGoldButtonState(eNABLED);
    }

    private void UpdateMoneyButtonState()
    {
        Store.BuyButtonState eNABLED = Store.BuyButtonState.ENABLED;
        if (this.m_bundle == null)
        {
            eNABLED = Store.BuyButtonState.DISABLED;
        }
        else if (!StoreManager.Get().IsOpen())
        {
            eNABLED = Store.BuyButtonState.DISABLED;
        }
        else if (!StoreManager.Get().IsBattlePayFeatureEnabled())
        {
            eNABLED = Store.BuyButtonState.DISABLED_FEATURE;
        }
        base.SetMoneyButtonState(eNABLED);
    }

    [CompilerGenerated]
    private sealed class <ShowImpl>c__AnonStorey32C
    {
        internal AdventureStore <>f__this;
        internal Store.DelOnStoreShown onStoreShownCB;

        internal void <>m__162()
        {
            this.<>f__this.m_animating = false;
            if (this.onStoreShownCB != null)
            {
                this.onStoreShownCB();
            }
        }
    }
}

