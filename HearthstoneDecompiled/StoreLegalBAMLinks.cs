using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StoreLegalBAMLinks : UIBPopup
{
    private static readonly StoreURL CHANGE_PAYMENT_URL = new StoreURL(FMT_URL_CHANGE_PAYMENT, StoreURL.Param.LOCALE, StoreURL.Param.REGION);
    private static readonly string FMT_URL_CHANGE_PAYMENT = "https://nydus.battle.net/WTCG/{0}/client/choose-payment-method?targetRegion={1}";
    private static readonly string FMT_URL_TERMS_OF_SALE = "https://nydus.battle.net/WTCG/{0}/client/legal/terms-of-sale?targetRegion={1}";
    private List<CancelListener> m_cancelListeners = new List<CancelListener>();
    public PegUIElement m_offClickCatcher;
    public UIBButton m_paymentMethodButton;
    public GameObject m_root;
    private List<SendToBAMListener> m_sendToBAMListeners = new List<SendToBAMListener>();
    public UIBButton m_termsOfSaleButton;
    private static readonly string SEND_TO_BAM_THEN_HIDE_COROUTINE = "SendToBAMThenHide";
    private static readonly StoreURL TERMS_OF_SALE_URL = new StoreURL(FMT_URL_TERMS_OF_SALE, StoreURL.Param.LOCALE, StoreURL.Param.REGION);

    private void Awake()
    {
        this.m_termsOfSaleButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnTermsOfSalePressed));
        this.m_paymentMethodButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnPaymentMethodPressed));
        this.m_offClickCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClickCatcherPressed));
    }

    private void EnableButtons(bool enabled)
    {
        this.m_termsOfSaleButton.SetEnabled(enabled);
        this.m_paymentMethodButton.SetEnabled(enabled);
    }

    private void OnClickCatcherPressed(UIEvent e)
    {
        this.Hide(true);
        foreach (CancelListener listener in this.m_cancelListeners.ToArray())
        {
            listener();
        }
    }

    private void OnPaymentMethodPressed(UIEvent e)
    {
        base.StopCoroutine(SEND_TO_BAM_THEN_HIDE_COROUTINE);
        base.StartCoroutine(SEND_TO_BAM_THEN_HIDE_COROUTINE, BAMReason.CHANGE_PAYMENT_METHOD);
    }

    private void OnTermsOfSalePressed(UIEvent e)
    {
        base.StopCoroutine(SEND_TO_BAM_THEN_HIDE_COROUTINE);
        base.StartCoroutine(SEND_TO_BAM_THEN_HIDE_COROUTINE, BAMReason.READ_TERMS_OF_SALE);
    }

    public void RegisterCancelListener(CancelListener listener)
    {
        if (!this.m_cancelListeners.Contains(listener))
        {
            this.m_cancelListeners.Add(listener);
        }
    }

    public void RegisterSendToBAMListener(SendToBAMListener listener)
    {
        if (!this.m_sendToBAMListeners.Contains(listener))
        {
            this.m_sendToBAMListeners.Add(listener);
        }
    }

    public void RemoveCancelListener(CancelListener listener)
    {
        this.m_cancelListeners.Remove(listener);
    }

    public void RemoveSendToBAMListener(SendToBAMListener listener)
    {
        this.m_sendToBAMListeners.Remove(listener);
    }

    [DebuggerHidden]
    private IEnumerator SendToBAMThenHide(BAMReason reason)
    {
        return new <SendToBAMThenHide>c__Iterator1E2 { reason = reason, <$>reason = reason, <>f__this = this };
    }

    public override void Show()
    {
        base.Show();
        if (!base.m_shown)
        {
            this.EnableButtons(true);
        }
    }

    [CompilerGenerated]
    private sealed class <SendToBAMThenHide>c__Iterator1E2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal StoreLegalBAMLinks.BAMReason <$>reason;
        internal StoreLegalBAMLinks.SendToBAMListener[] <$s_1015>__3;
        internal int <$s_1016>__4;
        internal StoreLegalBAMLinks <>f__this;
        internal StoreLegalBAMLinks.SendToBAMListener <listener>__5;
        internal StoreLegalBAMLinks.SendToBAMListener[] <listeners>__2;
        internal StoreURL <storeURL>__0;
        internal string <url>__1;
        internal StoreLegalBAMLinks.BAMReason reason;

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
                    this.<>f__this.EnableButtons(false);
                    this.<storeURL>__0 = null;
                    switch (this.reason)
                    {
                        case StoreLegalBAMLinks.BAMReason.CHANGE_PAYMENT_METHOD:
                            this.<storeURL>__0 = StoreLegalBAMLinks.CHANGE_PAYMENT_URL;
                            goto Label_006D;

                        case StoreLegalBAMLinks.BAMReason.READ_TERMS_OF_SALE:
                            this.<storeURL>__0 = StoreLegalBAMLinks.TERMS_OF_SALE_URL;
                            goto Label_006D;
                    }
                    break;

                case 1:
                    this.<>f__this.Hide(true);
                    this.<listeners>__2 = this.<>f__this.m_sendToBAMListeners.ToArray();
                    this.<$s_1015>__3 = this.<listeners>__2;
                    this.<$s_1016>__4 = 0;
                    while (this.<$s_1016>__4 < this.<$s_1015>__3.Length)
                    {
                        this.<listener>__5 = this.<$s_1015>__3[this.<$s_1016>__4];
                        this.<listener>__5(this.reason);
                        this.<$s_1016>__4++;
                    }
                    this.$PC = -1;
                    goto Label_0170;

                default:
                    goto Label_0170;
            }
        Label_006D:
            this.<url>__1 = string.Empty;
            if (this.<storeURL>__0 == null)
            {
                UnityEngine.Debug.LogError(string.Format("StoreLegalBAMLinks.SendToBAMThenHide(): could not get URL for reason {0}", this.reason));
            }
            else
            {
                this.<url>__1 = this.<storeURL>__0.GetURL();
            }
            if (!string.IsNullOrEmpty(this.<url>__1))
            {
                Application.OpenURL(this.<url>__1);
            }
            this.$current = new WaitForSeconds(2f);
            this.$PC = 1;
            return true;
        Label_0170:
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
        CHANGE_PAYMENT_METHOD,
        READ_TERMS_OF_SALE
    }

    public delegate void CancelListener();

    public delegate void SendToBAMListener(StoreLegalBAMLinks.BAMReason urlType);
}

