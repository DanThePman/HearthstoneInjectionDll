using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class StoreSummary : UIBPopup
{
    private static readonly Color DISABLED_CONFIRM_BUTTON_TEXT_COLOR = new Color(0.1176f, 0.1176f, 0.1176f);
    private static readonly Color ENABLED_CONFIRM_BUTTON_TEXT_COLOR = new Color(0.239f, 0.184f, 0.098f);
    [CustomEditField(Sections="Objects")]
    public GameObject m_bottomSectionRoot;
    private Network.Bundle m_bundle;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_cancelButton;
    private List<CancelListener> m_cancelListeners = new List<CancelListener>();
    [CustomEditField(Sections="Text")]
    public UberText m_chargeDetailsText;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_confirmButton;
    [CustomEditField(Sections="Objects")]
    public GameObject m_confirmButtonCheckMark;
    private bool m_confirmButtonEnabled = true;
    private List<ConfirmListener> m_confirmListeners = new List<ConfirmListener>();
    [CustomEditField(Sections="Materials")]
    public Material m_disabledConfirmButtonMaterial;
    [CustomEditField(Sections="Materials")]
    public Material m_disabledConfirmCheckMarkMaterial;
    [CustomEditField(Sections="Materials")]
    public Material m_enabledConfirmButtonMaterial;
    [CustomEditField(Sections="Materials")]
    public Material m_enabledConfirmCheckMarkMaterial;
    [CustomEditField(Sections="Text")]
    public UberText m_headlineText;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_infoButton;
    private List<InfoListener> m_infoListeners = new List<InfoListener>();
    [CustomEditField(Sections="Text")]
    public UberText m_itemsHeadlineText;
    [CustomEditField(Sections="Text")]
    public UberText m_itemsText;
    [CustomEditField(Sections="Korean Specific Info")]
    public CheckBox m_koreanAgreementCheckBox;
    [CustomEditField(Sections="Korean Specific Info")]
    public UberText m_koreanAgreementTermsText;
    [CustomEditField(Sections="Korean Specific Info")]
    public Transform m_koreanBottomSectionBone;
    [CustomEditField(Sections="Korean Specific Info")]
    public GameObject m_koreanRequirementRoot;
    private List<PaymentAndTOSListener> m_paymentAndTOSListeners = new List<PaymentAndTOSListener>();
    [CustomEditField(Sections="Text")]
    public UberText m_priceHeadlineText;
    [CustomEditField(Sections="Text")]
    public UberText m_priceText;
    private int m_quantity;
    private bool m_staticTextResized;
    [CustomEditField(Sections="Text")]
    public UberText m_taxDisclaimerText;
    [CustomEditField(Sections="Terms of Sale")]
    public UberText m_termsOfSaleAgreementText;
    [CustomEditField(Sections="Terms of Sale")]
    public Transform m_termsOfSaleBottomSectionBone;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_termsOfSaleButton;
    [CustomEditField(Sections="Terms of Sale")]
    public GameObject m_termsOfSaleRoot;
    private bool m_textInitialized;

    private void Awake()
    {
        this.m_confirmButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnConfirmPressed));
        this.m_cancelButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelPressed));
        this.m_infoButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnInfoPressed));
        this.m_termsOfSaleButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnTermsOfSalePressed));
        this.m_koreanAgreementCheckBox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ToggleKoreanAgreement));
    }

    private void EnableConfirmButton(bool enabled)
    {
        this.m_confirmButtonEnabled = enabled;
        Material material = !this.m_confirmButtonEnabled ? this.m_disabledConfirmButtonMaterial : this.m_enabledConfirmButtonMaterial;
        foreach (MultiSliceElement element in this.m_confirmButton.m_RootObject.GetComponentsInChildren<MultiSliceElement>())
        {
            foreach (MultiSliceElement.Slice slice in element.m_slices)
            {
                MeshRenderer component = slice.m_slice.GetComponent<MeshRenderer>();
                if (component != null)
                {
                    component.material = material;
                }
            }
        }
        Material material2 = !this.m_confirmButtonEnabled ? this.m_disabledConfirmCheckMarkMaterial : this.m_enabledConfirmCheckMarkMaterial;
        this.m_confirmButtonCheckMark.GetComponent<MeshRenderer>().material = material2;
        Color color = !this.m_confirmButtonEnabled ? DISABLED_CONFIRM_BUTTON_TEXT_COLOR : ENABLED_CONFIRM_BUTTON_TEXT_COLOR;
        this.m_confirmButton.m_ButtonText.TextColor = color;
    }

    private string GetItemsText()
    {
        string productName = StoreManager.Get().GetProductName(this.m_bundle.Items);
        object[] args = new object[] { this.m_quantity, productName };
        return GameStrings.Format("GLUE_STORE_SUMMARY_ITEM_ORDERED", args);
    }

    private string GetPriceText()
    {
        double cost = this.m_bundle.Cost * this.m_quantity;
        return StoreManager.Get().FormatCostText(cost);
    }

    protected override void Hide(bool animate)
    {
        if (base.m_shown)
        {
            this.m_termsOfSaleButton.SetEnabled(false);
            this.m_infoButton.SetEnabled(false);
            base.m_shown = false;
            base.DoHideAnimation(!animate, null);
        }
    }

    private void OnCancelPressed(UIEvent e)
    {
        this.Hide(true);
        foreach (CancelListener listener in this.m_cancelListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private void OnConfirmPressed(UIEvent e)
    {
        if (this.m_confirmButtonEnabled)
        {
            this.Hide(true);
            foreach (ConfirmListener listener in this.m_confirmListeners.ToArray())
            {
                listener.Fire(this.m_bundle.ProductID, this.m_quantity);
            }
        }
    }

    private void OnInfoPressed(UIEvent e)
    {
        this.Hide(true);
        foreach (InfoListener listener in this.m_infoListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private void OnTermsOfSalePressed(UIEvent e)
    {
        this.Hide(true);
        foreach (PaymentAndTOSListener listener in this.m_paymentAndTOSListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private void PreRender()
    {
        this.m_itemsText.UpdateNow();
        this.m_priceText.UpdateNow();
        this.m_koreanAgreementTermsText.UpdateNow();
        if (!this.m_staticTextResized)
        {
            this.m_headlineText.UpdateNow();
            this.m_itemsHeadlineText.UpdateNow();
            this.m_priceHeadlineText.UpdateNow();
            this.m_taxDisclaimerText.UpdateNow();
            this.m_koreanAgreementCheckBox.m_uberText.UpdateNow();
            this.m_staticTextResized = true;
        }
    }

    public bool RegisterCancelListener(CancelCallback callback)
    {
        return this.RegisterCancelListener(callback, null);
    }

    public bool RegisterCancelListener(CancelCallback callback, object userData)
    {
        CancelListener item = new CancelListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_cancelListeners.Contains(item))
        {
            return false;
        }
        this.m_cancelListeners.Add(item);
        return true;
    }

    public bool RegisterConfirmListener(ConfirmCallback callback)
    {
        return this.RegisterConfirmListener(callback, null);
    }

    public bool RegisterConfirmListener(ConfirmCallback callback, object userData)
    {
        ConfirmListener item = new ConfirmListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_confirmListeners.Contains(item))
        {
            return false;
        }
        this.m_confirmListeners.Add(item);
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

    public bool RegisterPaymentAndTOSListener(PaymentAndTOSCallback callback)
    {
        return this.RegisterPaymentAndTOSListener(callback, null);
    }

    public bool RegisterPaymentAndTOSListener(PaymentAndTOSCallback callback, object userData)
    {
        PaymentAndTOSListener item = new PaymentAndTOSListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_paymentAndTOSListeners.Contains(item))
        {
            return false;
        }
        this.m_paymentAndTOSListeners.Add(item);
        return true;
    }

    public bool RemoveConfirmListener(CancelCallback callback)
    {
        return this.RemoveConfirmListener(callback, null);
    }

    public bool RemoveConfirmListener(ConfirmCallback callback)
    {
        return this.RemoveConfirmListener(callback, null);
    }

    public bool RemoveConfirmListener(CancelCallback callback, object userData)
    {
        CancelListener item = new CancelListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_cancelListeners.Remove(item);
    }

    public bool RemoveConfirmListener(ConfirmCallback callback, object userData)
    {
        ConfirmListener item = new ConfirmListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_confirmListeners.Remove(item);
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

    public bool RemovePaymentAndTOSListener(PaymentAndTOSCallback callback)
    {
        return this.RemovePaymentAndTOSListener(callback, null);
    }

    public bool RemovePaymentAndTOSListener(PaymentAndTOSCallback callback, object userData)
    {
        PaymentAndTOSListener item = new PaymentAndTOSListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_paymentAndTOSListeners.Remove(item);
    }

    private void SetDetails(string productID, int quantity, string paymentMethodName)
    {
        this.m_bundle = StoreManager.Get().GetBundle(productID);
        this.m_quantity = quantity;
        this.m_itemsText.Text = this.GetItemsText();
        this.m_priceText.Text = this.GetPriceText();
        this.m_taxDisclaimerText.Text = StoreManager.Get().GetTaxText();
        this.m_chargeDetailsText.Text = (paymentMethodName != null) ? GameStrings.Format("GLUE_STORE_SUMMARY_CHARGE_DETAILS", new object[] { paymentMethodName }) : string.Empty;
        string str = string.Empty;
        HashSet<ProductType> productsInBundle = StoreManager.Get().GetProductsInBundle(this.m_bundle);
        if (productsInBundle.Contains(ProductType.PRODUCT_TYPE_BOOSTER))
        {
            if (this.m_bundle.IsPreOrder())
            {
                str = GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_PACK_PREORDER");
            }
            else
            {
                str = GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_EXPERT_PACK");
            }
        }
        else if (productsInBundle.Contains(ProductType.PRODUCT_TYPE_DRAFT))
        {
            str = GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_FORGE_TICKET");
        }
        else if ((productsInBundle.Contains(ProductType.PRODUCT_TYPE_NAXX) || productsInBundle.Contains(ProductType.PRODUCT_TYPE_BRM)) || productsInBundle.Contains(ProductType.PRODUCT_TYPE_LOE))
        {
            if (this.m_bundle.Items.Count == 1)
            {
                str = GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_ADVENTURE_SINGLE");
            }
            else
            {
                str = GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_ADVENTURE_BUNDLE");
            }
        }
        else if (productsInBundle.Contains(ProductType.PRODUCT_TYPE_HERO))
        {
            str = GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_HERO");
        }
        this.m_koreanAgreementTermsText.Text = str;
    }

    public override void Show()
    {
        if (!base.m_shown)
        {
            this.m_infoButton.SetEnabled(true);
            this.m_termsOfSaleButton.SetEnabled(true);
            bool flag = StoreManager.Get().IsEuropeanCustomer();
            if (!this.m_textInitialized)
            {
                this.m_textInitialized = true;
                this.m_headlineText.Text = GameStrings.Get("GLUE_STORE_SUMMARY_HEADLINE");
                this.m_itemsHeadlineText.Text = GameStrings.Get("GLUE_STORE_SUMMARY_ITEMS_ORDERED_HEADLINE");
                this.m_priceHeadlineText.Text = GameStrings.Get("GLUE_STORE_SUMMARY_PRICE_HEADLINE");
                this.m_infoButton.SetText(GameStrings.Get("GLUE_STORE_INFO_BUTTON_TEXT"));
                string text = GameStrings.Get("GLUE_STORE_TERMS_OF_SALE_BUTTON_TEXT");
                this.m_termsOfSaleButton.SetText(text);
                string str2 = GameStrings.Get("GLUE_STORE_SUMMARY_PAY_NOW_TEXT");
                this.m_confirmButton.SetText(str2);
                this.m_cancelButton.SetText(GameStrings.Get("GLOBAL_CANCEL"));
                string key = !flag ? "GLUE_STORE_SUMMARY_TOS_AGREEMENT" : "GLUE_STORE_SUMMARY_TOS_AGREEMENT_EU";
                object[] args = new object[] { str2, text };
                this.m_termsOfSaleAgreementText.Text = GameStrings.Format(key, args);
            }
            if (StoreManager.Get().IsKoreanCustomer())
            {
                this.m_bottomSectionRoot.transform.localPosition = this.m_koreanBottomSectionBone.localPosition;
                this.m_koreanRequirementRoot.gameObject.SetActive(true);
                this.m_koreanAgreementCheckBox.SetChecked(false);
                this.m_infoButton.gameObject.SetActive(true);
                this.EnableConfirmButton(false);
            }
            else
            {
                this.m_koreanRequirementRoot.gameObject.SetActive(false);
                this.m_infoButton.gameObject.SetActive(false);
                this.EnableConfirmButton(true);
            }
            if (flag || StoreManager.Get().IsNorthAmericanCustomer())
            {
                this.m_bottomSectionRoot.transform.localPosition = this.m_termsOfSaleBottomSectionBone.localPosition;
                this.m_termsOfSaleRoot.gameObject.SetActive(true);
                this.m_termsOfSaleButton.gameObject.SetActive(true);
            }
            else
            {
                this.m_termsOfSaleRoot.gameObject.SetActive(false);
                this.m_termsOfSaleButton.gameObject.SetActive(false);
            }
            this.PreRender();
            base.m_shown = true;
            base.DoShowAnimation(null);
        }
    }

    public void Show(string productID, int quantity, string paymentMethodName)
    {
        this.SetDetails(productID, quantity, paymentMethodName);
        this.Show();
    }

    private void ToggleKoreanAgreement(UIEvent e)
    {
        bool enabled = this.m_koreanAgreementCheckBox.IsChecked();
        this.EnableConfirmButton(enabled);
    }

    public delegate void CancelCallback(object userData);

    private class CancelListener : EventListener<StoreSummary.CancelCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public delegate void ConfirmCallback(string productID, int quantity, object userData);

    private class ConfirmListener : EventListener<StoreSummary.ConfirmCallback>
    {
        public void Fire(string productID, int quantity)
        {
            base.m_callback(productID, quantity, base.m_userData);
        }
    }

    public delegate void InfoCallback(object userData);

    private class InfoListener : EventListener<StoreSummary.InfoCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public delegate void PaymentAndTOSCallback(object userData);

    private class PaymentAndTOSListener : EventListener<StoreSummary.PaymentAndTOSCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }
}

