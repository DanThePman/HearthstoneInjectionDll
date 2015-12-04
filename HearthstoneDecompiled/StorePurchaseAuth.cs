using PegasusShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class StorePurchaseAuth : UIBPopup
{
    [CompilerGenerated]
    private static FixedRewardsMgr.DelPositionNonToastReward <>f__am$cacheD;
    [CompilerGenerated]
    private static FixedRewardsMgr.DelPositionNonToastReward <>f__am$cacheE;
    private List<AckPurchaseResultListener> m_ackPurchaseResultListeners = new List<AckPurchaseResultListener>();
    private List<ExitListener> m_exitListeners = new List<ExitListener>();
    [CustomEditField(Sections="Text")]
    public UberText m_failDetailsText;
    [CustomEditField(Sections="Text")]
    public UberText m_failHeadlineText;
    private bool m_isBackButton;
    [CustomEditField(Sections="Base UI")]
    public StoreMiniSummary m_miniSummary;
    private MoneyOrGTAPPTransaction m_moneyOrGTAPPTransaction;
    [CustomEditField(Sections="Base UI")]
    public UIBButton m_okButton;
    [CustomEditField(Sections="Base UI")]
    public MultiSliceElement m_root;
    private bool m_showingSuccess;
    [CustomEditField(Sections="Swirly Animation")]
    public Spell m_spell;
    [CustomEditField(Sections="Text")]
    public UberText m_successHeadlineText;
    [CustomEditField(Sections="Text")]
    public UberText m_waitingForAuthText;
    private const string s_BackButtonText = "GLOBAL_BACK";
    private const string s_OkButtonText = "GLOBAL_OKAY";

    private void Awake()
    {
        this.m_miniSummary.gameObject.SetActive(false);
        this.m_okButton.gameObject.SetActive(false);
        this.m_okButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnOkayButtonPressed));
    }

    public bool CompletePurchaseFailure(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails)
    {
        if (!base.gameObject.activeInHierarchy)
        {
            return false;
        }
        bool showMiniSummary = false;
        if (moneyOrGTAPPTransaction != null)
        {
            showMiniSummary = moneyOrGTAPPTransaction.ShouldShowMiniSummary();
        }
        this.ShowPurchaseFailure(moneyOrGTAPPTransaction, failDetails, showMiniSummary);
        return true;
    }

    public bool CompletePurchaseSuccess(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction)
    {
        if (!base.gameObject.activeInHierarchy)
        {
            return false;
        }
        bool showMiniSummary = false;
        if (moneyOrGTAPPTransaction != null)
        {
            showMiniSummary = moneyOrGTAPPTransaction.ShouldShowMiniSummary();
        }
        this.ShowPurchaseSuccess(moneyOrGTAPPTransaction, showMiniSummary);
        return true;
    }

    public override void Hide()
    {
        if (base.m_shown)
        {
            base.m_shown = false;
            base.DoHideAnimation(delegate {
                this.m_okButton.gameObject.SetActive(false);
                this.m_miniSummary.gameObject.SetActive(false);
                this.m_spell.ActivateState(SpellStateType.NONE);
            });
        }
    }

    private void OnOkayButtonPressed(UIEvent e)
    {
        if (this.m_isBackButton)
        {
            foreach (ExitListener listener in this.m_exitListeners.ToArray())
            {
                listener();
            }
        }
        else
        {
            this.Hide();
            foreach (AckPurchaseResultListener listener2 in this.m_ackPurchaseResultListeners.ToArray())
            {
                listener2(this.m_showingSuccess, this.m_moneyOrGTAPPTransaction);
            }
        }
        Scene scene = SceneMgr.Get().GetScene();
        if ((scene is Login) || (scene is Hub))
        {
            HashSet<RewardVisualTiming> rewardVisualTimings = new HashSet<RewardVisualTiming> {
                RewardVisualTiming.IMMEDIATE
            };
            if (<>f__am$cacheD == null)
            {
                <>f__am$cacheD = reward => reward.transform.localPosition = Login.REWARD_LOCAL_POS;
            }
            FixedRewardsMgr.Get().ShowFixedRewards(rewardVisualTimings, null, <>f__am$cacheD, (Vector3) Login.REWARD_PUNCH_SCALE, (Vector3) Login.REWARD_SCALE);
        }
        else if (scene is AdventureScene)
        {
            if (<>f__am$cacheE == null)
            {
                <>f__am$cacheE = reward => reward.transform.localPosition = AdventureScene.REWARD_LOCAL_POS;
            }
            FixedRewardsMgr.Get().ShowFixedRewards(new HashSet<RewardVisualTiming> { 1 }, null, <>f__am$cacheE, (Vector3) AdventureScene.REWARD_PUNCH_SCALE, (Vector3) AdventureScene.REWARD_SCALE);
        }
    }

    public void RegisterAckPurchaseResultListener(AckPurchaseResultListener listener)
    {
        if (!this.m_ackPurchaseResultListeners.Contains(listener))
        {
            this.m_ackPurchaseResultListeners.Add(listener);
        }
    }

    public void RegisterExitListener(ExitListener listener)
    {
        if (!this.m_exitListeners.Contains(listener))
        {
            this.m_exitListeners.Add(listener);
        }
    }

    public void RemoveAckPurchaseResultListener(AckPurchaseResultListener listener)
    {
        this.m_ackPurchaseResultListeners.Remove(listener);
    }

    public void RemoveExitListener(ExitListener listener)
    {
        this.m_exitListeners.Remove(listener);
    }

    public void Show(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, bool enableBackButton, bool isZeroCostLicense)
    {
        if (!base.m_shown)
        {
            if (isZeroCostLicense)
            {
                this.m_waitingForAuthText.Text = GameStrings.Get("GLUE_STORE_AUTH_ZERO_COST_WAITING");
                this.m_successHeadlineText.Text = GameStrings.Get("GLUE_STORE_AUTH_ZERO_COST_SUCCESS_HEADLINE");
                this.m_failHeadlineText.Text = GameStrings.Get("GLUE_STORE_AUTH_ZERO_COST_FAIL_HEADLINE");
            }
            else
            {
                this.m_waitingForAuthText.Text = GameStrings.Get("GLUE_STORE_AUTH_WAITING");
                this.m_successHeadlineText.Text = GameStrings.Get("GLUE_STORE_AUTH_SUCCESS_HEADLINE");
                this.m_failHeadlineText.Text = GameStrings.Get("GLUE_STORE_AUTH_FAIL_HEADLINE");
            }
            base.m_shown = true;
            this.m_showingSuccess = false;
            this.m_moneyOrGTAPPTransaction = moneyOrGTAPPTransaction;
            this.m_isBackButton = enableBackButton;
            this.m_okButton.gameObject.SetActive(enableBackButton);
            this.m_okButton.SetText(!enableBackButton ? "GLOBAL_OKAY" : "GLOBAL_BACK");
            this.m_waitingForAuthText.gameObject.SetActive(true);
            this.m_successHeadlineText.gameObject.SetActive(false);
            this.m_failHeadlineText.gameObject.SetActive(false);
            this.m_failDetailsText.gameObject.SetActive(false);
            this.m_spell.ActivateState(SpellStateType.BIRTH);
            if ((this.m_moneyOrGTAPPTransaction != null) && this.m_moneyOrGTAPPTransaction.ShouldShowMiniSummary())
            {
                this.ShowMiniSummary();
            }
            else
            {
                this.m_root.UpdateSlices();
            }
            base.DoShowAnimation(null);
        }
    }

    private void ShowMiniSummary()
    {
        if (this.m_moneyOrGTAPPTransaction != null)
        {
            this.m_miniSummary.SetDetails(this.m_moneyOrGTAPPTransaction.ProductID, 1);
            this.m_miniSummary.gameObject.SetActive(true);
            this.m_root.UpdateSlices();
        }
    }

    public void ShowPreviousPurchaseFailure(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, bool enableBackButton)
    {
        this.Show(moneyOrGTAPPTransaction, enableBackButton, false);
        this.ShowPurchaseFailure(moneyOrGTAPPTransaction, failDetails, true);
    }

    public void ShowPreviousPurchaseSuccess(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, bool enableBackButton)
    {
        this.Show(moneyOrGTAPPTransaction, enableBackButton, false);
        this.ShowPurchaseSuccess(moneyOrGTAPPTransaction, true);
    }

    private void ShowPurchaseFailure(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, bool showMiniSummary)
    {
        this.m_showingSuccess = false;
        this.m_moneyOrGTAPPTransaction = moneyOrGTAPPTransaction;
        this.m_isBackButton = false;
        this.m_okButton.gameObject.SetActive(true);
        this.m_okButton.SetText("GLOBAL_OKAY");
        if (showMiniSummary)
        {
            this.ShowMiniSummary();
        }
        this.m_failDetailsText.Text = failDetails;
        this.m_waitingForAuthText.gameObject.SetActive(false);
        this.m_successHeadlineText.gameObject.SetActive(false);
        this.m_failHeadlineText.gameObject.SetActive(true);
        this.m_failDetailsText.gameObject.SetActive(true);
        this.m_spell.ActivateState(SpellStateType.DEATH);
    }

    public void ShowPurchaseLocked(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, bool enableBackButton, bool isZeroCostLicense, PurchaseLockedDialogCallback purchaseLockedCallback)
    {
        object[] objArray1;
        <ShowPurchaseLocked>c__AnonStorey349 storey = new <ShowPurchaseLocked>c__AnonStorey349 {
            purchaseLockedCallback = purchaseLockedCallback
        };
        this.Show(moneyOrGTAPPTransaction, enableBackButton, isZeroCostLicense);
        string str = string.Empty;
        if (moneyOrGTAPPTransaction.Provider.HasValue)
        {
            switch (moneyOrGTAPPTransaction.Provider.Value)
            {
                case BattlePayProvider.BP_PROVIDER_APPLE:
                    str = GameStrings.Get("GLOBAL_STORE_MOBILE_NAME_APPLE");
                    goto Label_009B;

                case BattlePayProvider.BP_PROVIDER_GOOGLE_PLAY:
                    str = GameStrings.Get("GLOBAL_STORE_MOBILE_NAME_GOOGLE");
                    goto Label_009B;

                case BattlePayProvider.BP_PROVIDER_AMAZON:
                    str = GameStrings.Get("GLOBAL_STORE_MOBILE_NAME_AMAZON");
                    goto Label_009B;
            }
            str = GameStrings.Get("GLOBAL_STORE_MOBILE_NAME_DEFAULT");
        }
    Label_009B:
        objArray1 = new object[] { str };
        string str2 = GameStrings.Format("GLUE_STORE_PURCHASE_LOCK_DESCRIPTION", objArray1);
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_STORE_PURCHASE_LOCK_HEADER"),
            m_confirmText = GameStrings.Get("GLOBAL_CANCEL"),
            m_cancelText = GameStrings.Get("GLOBAL_HELP"),
            m_text = str2,
            m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
            m_iconSet = AlertPopup.PopupInfo.IconSet.Alternate,
            m_responseCallback = new AlertPopup.ResponseCallback(storey.<>m__1A2)
        };
        DialogManager.Get().ShowPopup(info);
    }

    public void ShowPurchaseMethodFailure(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, bool enableBackButton)
    {
        this.Show(moneyOrGTAPPTransaction, enableBackButton, false);
        this.ShowPurchaseFailure(moneyOrGTAPPTransaction, failDetails, false);
    }

    private void ShowPurchaseSuccess(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, bool showMiniSummary)
    {
        this.m_showingSuccess = true;
        this.m_moneyOrGTAPPTransaction = moneyOrGTAPPTransaction;
        this.m_isBackButton = false;
        this.m_okButton.gameObject.SetActive(true);
        this.m_okButton.SetText("GLOBAL_OKAY");
        if (showMiniSummary)
        {
            this.ShowMiniSummary();
        }
        this.m_waitingForAuthText.gameObject.SetActive(false);
        this.m_successHeadlineText.gameObject.SetActive(true);
        this.m_failHeadlineText.gameObject.SetActive(false);
        this.m_failDetailsText.gameObject.SetActive(false);
        this.m_spell.ActivateState(SpellStateType.ACTION);
    }

    [CompilerGenerated]
    private sealed class <ShowPurchaseLocked>c__AnonStorey349
    {
        internal StorePurchaseAuth.PurchaseLockedDialogCallback purchaseLockedCallback;

        internal void <>m__1A2(AlertPopup.Response response, object data)
        {
            if (this.purchaseLockedCallback != null)
            {
                this.purchaseLockedCallback(response == AlertPopup.Response.CANCEL);
            }
        }
    }

    public delegate void AckPurchaseResultListener(bool success, MoneyOrGTAPPTransaction moneyOrGTAPPTransaction);

    public delegate void ExitListener();

    public delegate void PurchaseLockedDialogCallback(bool showHelp);
}

