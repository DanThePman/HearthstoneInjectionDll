using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class StoreChallengePrompt : UIBPopup
{
    private static readonly string FMT_URL_CVV_INFO = "https://nydus.battle.net/WTCG/{0}/client/support/cvv?targetRegion={1}";
    public UIBButton m_cancelButton;
    private List<CancelListener> m_cancelListeners = new List<CancelListener>();
    private ulong m_challengeID;
    private ChallengeType m_challengeType;
    public UIBButton m_infoButton;
    public GameObject m_infoButtonFrame;
    private string m_input = string.Empty;
    public UberText m_inputText;
    public UberText m_messageText;
    public UIBButton m_submitButton;
    private List<SubmitListener> m_submitListeners = new List<SubmitListener>();
    private static readonly StoreURL s_cvvURL = new StoreURL(FMT_URL_CVV_INFO, StoreURL.Param.LOCALE, StoreURL.Param.REGION);

    private void Awake()
    {
        this.m_inputText.RichText = false;
        this.m_submitButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnSubmitPressed));
        this.m_cancelButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelPressed));
        this.m_infoButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnInfoPressed));
    }

    private void Cancel()
    {
        ulong challengeID = this.m_challengeID;
        this.Hide(true);
        foreach (CancelListener listener in this.m_cancelListeners.ToArray())
        {
            listener(challengeID);
        }
    }

    private void ClearInput()
    {
        UniversalInputManager.Get().SetInputText(string.Empty);
    }

    protected override void Hide(bool animate)
    {
        if (this.IsShown())
        {
            base.m_shown = false;
            this.HideInput();
            base.DoHideAnimation(!animate, new UIBPopup.OnAnimationComplete(this.OnHidden));
        }
    }

    public ulong HideChallenge()
    {
        ulong challengeID = this.m_challengeID;
        this.Hide(false);
        return challengeID;
    }

    private void HideInput()
    {
        UniversalInputManager.Get().CancelTextInput(base.gameObject, false);
        this.m_inputText.gameObject.SetActive(true);
    }

    private void OnCancelPressed(UIEvent e)
    {
        this.Cancel();
    }

    protected override void OnHidden()
    {
        this.m_challengeID = 0L;
    }

    private void OnInfoPressed(UIEvent e)
    {
        Application.OpenURL(s_cvvURL.GetURL());
    }

    private void OnInputCanceled(bool userRequested, GameObject requester)
    {
        this.m_input = string.Empty;
        this.UpdateInputText();
        this.Cancel();
    }

    private void OnInputComplete(string input)
    {
        this.m_input = input;
        this.UpdateInputText();
        this.Submit();
    }

    private void OnInputUpdated(string input)
    {
        this.m_input = input;
        this.UpdateInputText();
    }

    private void OnShown()
    {
        if (this.IsShown())
        {
            this.ShowInput();
        }
    }

    private void OnSubmitPressed(UIEvent e)
    {
        this.Submit();
    }

    public void RegisterCancelListener(CancelListener listener)
    {
        if (!this.m_cancelListeners.Contains(listener))
        {
            this.m_cancelListeners.Add(listener);
        }
    }

    public void RegisterSubmitListener(SubmitListener listener)
    {
        if (!this.m_submitListeners.Contains(listener))
        {
            this.m_submitListeners.Add(listener);
        }
    }

    public void RemoveConfirmListener(CancelListener listener)
    {
        this.m_cancelListeners.Remove(listener);
    }

    public void RemoveSubmitListener(SubmitListener listener)
    {
        this.m_submitListeners.Remove(listener);
    }

    public bool Show(ChallengeType type, ulong challengeID, bool isRetry)
    {
        if (this.IsShown())
        {
            return false;
        }
        base.m_shown = true;
        this.m_challengeType = type;
        this.m_challengeID = challengeID;
        string str = string.Empty;
        bool flag = false;
        ChallengeType challengeType = this.m_challengeType;
        if (challengeType != ChallengeType.PASSWORD)
        {
            if (challengeType != ChallengeType.CVV)
            {
                Debug.LogError(string.Format("StoreChallengePrompt.Show(): unrecognized challenge type {0}", this.m_challengeType));
                flag = false;
            }
            else
            {
                object[] args = new object[] { this.m_infoButton.GetText() };
                str = GameStrings.Format("GLUE_STORE_CHALLENGE_CVV_MESSAGE", args);
                flag = true;
            }
        }
        else
        {
            str = !isRetry ? GameStrings.Get("GLUE_STORE_CHALLENGE_PWD_MESSAGE") : GameStrings.Get("GLUE_STORE_CHALLENGE_PWD_RETRY_MESSAGE");
            flag = false;
        }
        this.m_messageText.Text = str;
        this.m_infoButtonFrame.SetActive(flag);
        this.m_input = string.Empty;
        this.UpdateInputText();
        base.DoShowAnimation(new UIBPopup.OnAnimationComplete(this.OnShown));
        return true;
    }

    private void ShowInput()
    {
        this.m_inputText.gameObject.SetActive(false);
        Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
        Bounds bounds = this.m_inputText.GetBounds();
        Rect rect = CameraUtils.CreateGUIViewportRect(camera, bounds.min, bounds.max);
        UniversalInputManager.TextInputParams parms = new UniversalInputManager.TextInputParams {
            m_owner = base.gameObject,
            m_password = true,
            m_rect = rect,
            m_updatedCallback = new UniversalInputManager.TextInputUpdatedCallback(this.OnInputUpdated),
            m_completedCallback = new UniversalInputManager.TextInputCompletedCallback(this.OnInputComplete),
            m_canceledCallback = new UniversalInputManager.TextInputCanceledCallback(this.OnInputCanceled),
            m_font = this.m_inputText.TrueTypeFont,
            m_alignment = 4,
            m_maxCharacters = (this.m_challengeType != ChallengeType.CVV) ? 0 : 4
        };
        UniversalInputManager.Get().UseTextInput(parms, false);
    }

    private void Submit()
    {
        ulong challengeID = this.m_challengeID;
        this.Hide(true);
        foreach (SubmitListener listener in this.m_submitListeners.ToArray())
        {
            listener(challengeID, this.m_input);
        }
    }

    private void UpdateInputText()
    {
        StringBuilder builder = new StringBuilder(this.m_input.Length);
        for (int i = 0; i < this.m_input.Length; i++)
        {
            builder.Append('*');
        }
        this.m_inputText.Text = builder.ToString();
    }

    public delegate void CancelListener(ulong challengeID);

    public delegate void SubmitListener(ulong challengeID, string password);
}

