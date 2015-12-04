using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class StoreSendToBAM : UIBPopup
{
    private const string FMT_URL_EULA_AND_TOS = "https://nydus.battle.net/WTCG/{0}/client/legal/terms-of-sale?targetRegion={1}";
    private const string FMT_URL_GENERIC_PAYMENT_FAIL = "https://nydus.battle.net/WTCG/{0}/client/add-payment?targetRegion={1}&flowId=4";
    private const string FMT_URL_NO_PAYMENT_METHOD = "https://nydus.battle.net/WTCG/{0}/client/add-payment?targetRegion={1}&flowId=1";
    private const string FMT_URL_PAYMENT_EXPIRED = "https://nydus.battle.net/WTCG/{0}/client/add-payment?targetRegion={1}&flowId=5";
    private static readonly PlatformDependentValue<string> FMT_URL_PAYMENT_INFO;
    private const string FMT_URL_PURCHASE_UNIQUENESS_VIOLATED = "https://nydus.battle.net/WTCG/{0}/client/support/already-owned?targetRegion={1}";
    private static readonly string FMT_URL_RESET_PASSWORD = "https://nydus.battle.net/WTCG/{0}/client/password-reset?targetRegion={1}";
    private static readonly PlatformDependentValue<string> GLUE_STORE_PAYMENT_INFO_DETAILS;
    private static readonly PlatformDependentValue<string> GLUE_STORE_PAYMENT_INFO_URL_DETAILS;
    public MultiSliceElement m_allSections;
    public UIBButton m_cancelButton;
    private List<DelCancelListener> m_cancelListeners = new List<DelCancelListener>();
    private string m_errorCode = string.Empty;
    public UberText m_headlineText;
    public UberText m_messageText;
    public GameObject m_midSection;
    public StoreMiniSummary m_miniSummary;
    private MoneyOrGTAPPTransaction m_moneyOrGTAPPTransaction;
    public UIBButton m_okayButton;
    private List<DelOKListener> m_okayListeners = new List<DelOKListener>();
    private Vector3 m_originalShowScale = Vector3.zero;
    private BAMReason m_sendToBAMReason;
    public GameObject m_sendToBAMRoot;
    public Transform m_sendToBAMRootWithSummaryBone;
    private static Map<BAMReason, SendToBAMText> s_bamTextMap;
    private static readonly string SEND_TO_BAM_THEN_HIDE_COROUTINE = "SendToBAMThenHide";
    private static readonly Vector3 SHOW_MINI_SUMMARY_SCALE_PHONE;

    static StoreSendToBAM()
    {
        PlatformDependentValue<string> value2 = new PlatformDependentValue<string>(PlatformCategory.OS) {
            PC = "https://nydus.battle.net/WTCG/{0}/client/support/purchase?targetRegion={1}",
            iOS = "https://nydus.battle.net/WTCG/{0}/client/support/purchase?targetRegion={1}&targetDevice=ipad",
            Android = "https://nydus.battle.net/WTCG/{0}/client/support/purchase?targetRegion={1}&targetDevice=android"
        };
        FMT_URL_PAYMENT_INFO = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.OS) {
            PC = "GLUE_STORE_PAYMENT_INFO_DETAILS",
            iOS = "GLUE_MOBILE_STORE_PAYMENT_INFO_DETAILS_APPLE",
            Android = "GLUE_MOBILE_STORE_PAYMENT_INFO_DETAILS_ANDROID"
        };
        GLUE_STORE_PAYMENT_INFO_DETAILS = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.OS) {
            PC = "GLUE_STORE_PAYMENT_INFO_URL_DETAILS",
            iOS = "GLUE_MOBILE_STORE_PAYMENT_INFO_URL_DETAILS",
            Android = "GLUE_MOBILE_STORE_PAYMENT_INFO_URL_DETAILS"
        };
        GLUE_STORE_PAYMENT_INFO_URL_DETAILS = value2;
        SHOW_MINI_SUMMARY_SCALE_PHONE = new Vector3(80f, 80f, 80f);
    }

    private void Awake()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            base.m_scaleMode = CanvasScaleMode.WIDTH;
        }
        Map<BAMReason, SendToBAMText> map = new Map<BAMReason, SendToBAMText>();
        map.Add(BAMReason.PAYMENT_INFO, new SendToBAMText("GLUE_STORE_PAYMENT_INFO_HEADLINE", (string) GLUE_STORE_PAYMENT_INFO_DETAILS, (string) GLUE_STORE_PAYMENT_INFO_URL_DETAILS, (string) FMT_URL_PAYMENT_INFO, StoreURL.Param.LOCALE, StoreURL.Param.REGION));
        map.Add(BAMReason.NEED_PASSWORD_RESET, new SendToBAMText("GLUE_STORE_FORGOT_PWD_HEADLINE", "GLUE_STORE_FORGOT_PWD_DETAILS", "GLUE_STORE_FORGOT_PWD_URL_DETAILS", FMT_URL_RESET_PASSWORD, StoreURL.Param.LOCALE, StoreURL.Param.REGION));
        map.Add(BAMReason.NO_VALID_PAYMENT_METHOD, new SendToBAMText("GLUE_STORE_NO_PAYMENT_HEADLINE", "GLUE_STORE_NO_PAYMENT_DETAILS", "GLUE_STORE_NO_PAYMENT_URL_DETAILS", "https://nydus.battle.net/WTCG/{0}/client/add-payment?targetRegion={1}&flowId=1", StoreURL.Param.LOCALE, StoreURL.Param.REGION));
        map.Add(BAMReason.CREDIT_CARD_EXPIRED, new SendToBAMText("GLUE_STORE_GENERIC_BP_FAIL_HEADLINE", "GLUE_STORE_CC_EXPIRY_DETAILS", "GLUE_STORE_GENERIC_BP_FAIL_URL_DETAILS", "https://nydus.battle.net/WTCG/{0}/client/add-payment?targetRegion={1}&flowId=5", StoreURL.Param.LOCALE, StoreURL.Param.REGION));
        map.Add(BAMReason.GENERIC_PAYMENT_FAIL, new SendToBAMText("GLUE_STORE_GENERIC_BP_FAIL_HEADLINE", "GLUE_STORE_GENERIC_BP_FAIL_DETAILS", "GLUE_STORE_GENERIC_BP_FAIL_URL_DETAILS", "https://nydus.battle.net/WTCG/{0}/client/add-payment?targetRegion={1}&flowId=4", StoreURL.Param.LOCALE, StoreURL.Param.REGION));
        map.Add(BAMReason.EULA_AND_TOS, new SendToBAMText("GLUE_STORE_EULA_AND_TOS_HEADLINE", "GLUE_STORE_EULA_AND_TOS_DETAILS", "GLUE_STORE_EULA_AND_TOS_URL_DETAILS", "https://nydus.battle.net/WTCG/{0}/client/legal/terms-of-sale?targetRegion={1}", StoreURL.Param.LOCALE, StoreURL.Param.REGION));
        map.Add(BAMReason.PRODUCT_UNIQUENESS_VIOLATED, new SendToBAMText("GLUE_STORE_PURCHASE_LOCK_HEADER", "GLUE_STORE_FAIL_PRODUCT_UNIQUENESS_VIOLATED", "GLUE_STORE_FAIL_PRODUCT_UNIQUENESS_VIOLATED_URL", "https://nydus.battle.net/WTCG/{0}/client/support/already-owned?targetRegion={1}", StoreURL.Param.LOCALE, StoreURL.Param.REGION));
        s_bamTextMap = map;
        this.m_okayButton.SetText(GameStrings.Get("GLOBAL_MORE"));
        this.m_cancelButton.SetText(GameStrings.Get("GLOBAL_CANCEL"));
        this.m_okayButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnOkayPressed));
        this.m_cancelButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelPressed));
    }

    private void LayoutMessageText()
    {
        this.m_messageText.UpdateNow();
        TransformUtil.SetLocalScaleZ(this.m_midSection, 1f);
        float num = TransformUtil.ComputeOrientedWorldBounds(this.m_midSection, true).Extents[2].magnitude * 2f;
        Bounds textWorldSpaceBounds = this.m_messageText.GetTextWorldSpaceBounds();
        TransformUtil.SetLocalScaleZ(this.m_midSection, textWorldSpaceBounds.size.z / num);
        this.m_allSections.UpdateSlices();
    }

    private bool OnCancel()
    {
        base.StopCoroutine(SEND_TO_BAM_THEN_HIDE_COROUTINE);
        this.Hide(true);
        foreach (DelCancelListener listener in this.m_cancelListeners.ToArray())
        {
            listener(this.m_moneyOrGTAPPTransaction);
        }
        return true;
    }

    private void OnCancelPressed(UIEvent e)
    {
        Navigation.GoBack();
    }

    protected override void OnHidden()
    {
        this.m_okayButton.SetEnabled(true);
    }

    private void OnOkayPressed(UIEvent e)
    {
        base.StopCoroutine(SEND_TO_BAM_THEN_HIDE_COROUTINE);
        base.StartCoroutine(SEND_TO_BAM_THEN_HIDE_COROUTINE);
    }

    public void RegisterCancelListener(DelCancelListener listener)
    {
        if (!this.m_cancelListeners.Contains(listener))
        {
            this.m_cancelListeners.Add(listener);
        }
    }

    public void RegisterOkayListener(DelOKListener listener)
    {
        if (!this.m_okayListeners.Contains(listener))
        {
            this.m_okayListeners.Add(listener);
        }
    }

    public void RemoveCancelListener(DelCancelListener listener)
    {
        this.m_cancelListeners.Remove(listener);
    }

    public void RemoveOkayListener(DelOKListener listener)
    {
        this.m_okayListeners.Remove(listener);
    }

    [DebuggerHidden]
    private IEnumerator SendToBAMThenHide()
    {
        return new <SendToBAMThenHide>c__Iterator1E3 { <>f__this = this };
    }

    public void Show(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, BAMReason reason, string errorCode, bool fromPreviousPurchase)
    {
        this.m_moneyOrGTAPPTransaction = moneyOrGTAPPTransaction;
        this.m_sendToBAMReason = reason;
        this.m_errorCode = errorCode;
        this.UpdateText();
        if ((moneyOrGTAPPTransaction != null) && (fromPreviousPurchase || moneyOrGTAPPTransaction.ShouldShowMiniSummary()))
        {
            this.m_sendToBAMRoot.transform.position = this.m_sendToBAMRootWithSummaryBone.position;
            this.m_miniSummary.SetDetails(this.m_moneyOrGTAPPTransaction.ProductID, 1);
            this.m_miniSummary.gameObject.SetActive(true);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.m_originalShowScale = base.m_showScale;
                base.m_showScale = SHOW_MINI_SUMMARY_SCALE_PHONE;
            }
        }
        else
        {
            this.m_sendToBAMRoot.transform.localPosition = Vector3.zero;
            this.m_miniSummary.gameObject.SetActive(false);
            if ((UniversalInputManager.UsePhoneUI != null) && (this.m_originalShowScale != Vector3.zero))
            {
                base.m_showScale = this.m_originalShowScale;
                this.m_originalShowScale = Vector3.zero;
            }
        }
        if (!base.m_shown)
        {
            Navigation.Push(new Navigation.NavigateBackHandler(this.OnCancel));
            base.m_shown = true;
            this.m_headlineText.UpdateNow();
            this.LayoutMessageText();
            base.DoShowAnimation(null);
        }
    }

    private void UpdateText()
    {
        SendToBAMText text = s_bamTextMap[this.m_sendToBAMReason];
        if (text == null)
        {
            UnityEngine.Debug.LogError(string.Format("StoreSendToBAM.UpdateText(): don't know how to update text for BAM reason {0}", this.m_sendToBAMReason));
            this.m_headlineText.Text = string.Empty;
            this.m_messageText.Text = string.Empty;
        }
        else
        {
            string details = text.GetDetails();
            if (!string.IsNullOrEmpty(this.m_errorCode))
            {
                object[] args = new object[] { this.m_errorCode };
                details = details + " " + GameStrings.Format("GLUE_STORE_FAIL_DETAILS_ERROR_CODE", args);
            }
            details = details + "\n\n" + text.GetGoToURLDetails(this.m_okayButton.m_ButtonText.Text);
            this.m_headlineText.Text = text.GetHeadline();
            this.m_messageText.Text = details;
        }
    }

    [CompilerGenerated]
    private sealed class <SendToBAMThenHide>c__Iterator1E3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal StoreSendToBAM.DelOKListener[] <$s_1061>__3;
        internal int <$s_1062>__4;
        internal StoreSendToBAM <>f__this;
        internal StoreSendToBAM.DelOKListener <listener>__5;
        internal StoreSendToBAM.DelOKListener[] <listeners>__2;
        internal StoreSendToBAM.SendToBAMText <sendToBAMText>__1;
        internal string <url>__0;

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
                    this.<>f__this.m_okayButton.SetEnabled(false);
                    this.<url>__0 = string.Empty;
                    this.<sendToBAMText>__1 = StoreSendToBAM.s_bamTextMap[this.<>f__this.m_sendToBAMReason];
                    if (this.<sendToBAMText>__1 != null)
                    {
                        this.<url>__0 = this.<sendToBAMText>__1.GetURL();
                        break;
                    }
                    UnityEngine.Debug.LogError(string.Format("StoreSendToBAM.SendToBAMThenHide(): can't get URL for BAM reason {0}", this.<>f__this.m_sendToBAMReason));
                    break;

                case 1:
                    Navigation.Pop();
                    this.<>f__this.Hide(true);
                    this.<listeners>__2 = this.<>f__this.m_okayListeners.ToArray();
                    this.<$s_1061>__3 = this.<listeners>__2;
                    this.<$s_1062>__4 = 0;
                    while (this.<$s_1062>__4 < this.<$s_1061>__3.Length)
                    {
                        this.<listener>__5 = this.<$s_1061>__3[this.<$s_1062>__4];
                        this.<listener>__5(this.<>f__this.m_moneyOrGTAPPTransaction, this.<>f__this.m_sendToBAMReason);
                        this.<$s_1062>__4++;
                    }
                    this.$PC = -1;
                    goto Label_016A;

                default:
                    goto Label_016A;
            }
            if (!string.IsNullOrEmpty(this.<url>__0))
            {
                Application.OpenURL(this.<url>__0);
            }
            this.$current = new WaitForSeconds(2f);
            this.$PC = 1;
            return true;
        Label_016A:
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

    public enum BAMReason
    {
        PAYMENT_INFO,
        NEED_PASSWORD_RESET,
        NO_VALID_PAYMENT_METHOD,
        CREDIT_CARD_EXPIRED,
        GENERIC_PAYMENT_FAIL,
        EULA_AND_TOS,
        PRODUCT_UNIQUENESS_VIOLATED
    }

    public delegate void DelCancelListener(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction);

    public delegate void DelOKListener(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, StoreSendToBAM.BAMReason reason);

    private class SendToBAMText
    {
        private string m_detailsKey;
        private string m_goToURLKey;
        private string m_headlineKey;
        private StoreURL m_url;

        public SendToBAMText(string headlineKey, string detailsKey, string goToURLKey, string urlFmt, StoreURL.Param urlParam1, StoreURL.Param urlParam2)
        {
            this.m_headlineKey = headlineKey;
            this.m_detailsKey = detailsKey;
            this.m_goToURLKey = goToURLKey;
            this.m_url = new StoreURL(urlFmt, urlParam1, urlParam2);
        }

        public string GetDetails()
        {
            return GameStrings.Get(this.m_detailsKey);
        }

        public string GetGoToURLDetails(string buttonName)
        {
            object[] args = new object[] { buttonName };
            return GameStrings.Format(this.m_goToURLKey, args);
        }

        public string GetHeadline()
        {
            return GameStrings.Get(this.m_headlineKey);
        }

        public string GetURL()
        {
            return this.m_url.GetURL();
        }
    }
}

