using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RecruitAFriendFrame : MonoBehaviour
{
    public RecruitAFriendFrameBones m_Bones;
    public UberText m_DescriptionText;
    public UberText m_DisclaimerText;
    private PegUIElement m_inputBlocker;
    public Font m_InputFont;
    private string m_inputText = string.Empty;
    public TextField m_InputTextField;
    public UberText m_InstructionText;
    private Font m_localizedInputFont;
    public UberText m_TitleText;

    public event System.Action Closed;

    private void Awake()
    {
        this.InitItems();
        this.Layout();
        this.InitInput();
        this.InitInputTextField();
    }

    public void Close()
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void FireClosedEvent()
    {
        if (this.Closed != null)
        {
            this.Closed();
        }
    }

    private void InitInput()
    {
        FontDef fontDef = FontTable.Get().GetFontDef(this.m_InputFont);
        if (fontDef == null)
        {
            this.m_localizedInputFont = this.m_InputFont;
        }
        else
        {
            this.m_localizedInputFont = fontDef.m_Font;
        }
    }

    private void InitInputBlocker()
    {
        GameObject obj2 = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "RecruitAFriendInputBlocker", this);
        obj2.transform.position = this.m_Bones.m_InputBlocker.position;
        this.m_inputBlocker = obj2.AddComponent<PegUIElement>();
        this.m_inputBlocker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnInputBlockerReleased));
    }

    private void InitInputTextField()
    {
        this.m_InputTextField.Preprocess += new Action<Event>(this.OnInputPreprocess);
        this.m_InputTextField.Changed += new Action<string>(this.OnInputChanged);
        this.m_InputTextField.Submitted += new Action<string>(this.OnInputSubmitted);
        this.m_InputTextField.Canceled += new System.Action(this.OnInputCanceled);
        this.m_InstructionText.gameObject.SetActive(true);
    }

    private void InitItems()
    {
        this.InitInputBlocker();
    }

    private void Layout()
    {
        base.transform.parent = BaseUI.Get().transform;
        base.transform.position = BaseUI.Get().GetRecruitAFriendBone().position;
    }

    private void OnInputBlockerReleased(UIEvent e)
    {
        this.FireClosedEvent();
    }

    private void OnInputCanceled()
    {
        this.FireClosedEvent();
    }

    private void OnInputChanged(string text)
    {
        this.m_inputText = text;
        this.UpdateInstructions();
    }

    private void OnInputPreprocess(Event e)
    {
        if (Input.imeIsSelected)
        {
            this.UpdateInstructions();
        }
    }

    private void OnInputSubmitted(string input)
    {
        if (!RecruitListMgr.Get().CanAddMoreRecruits())
        {
            this.ShowAlertPopup("GLOBAL_FRIENDLIST_TOO_MANY_RECRUITS_ALERT_MESSAGE");
        }
        else if (!RecruitListMgr.IsValidRecruitInput(input))
        {
            this.ShowAlertPopup("GLOBAL_FRIENDLIST_INVALID_EMAIL");
        }
        else
        {
            RecruitListMgr.Get().SendRecruitAFriendInvite(input);
            this.FireClosedEvent();
        }
    }

    private void ShowAlertPopup(string msgGameStringKey)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_text = GameStrings.Format(msgGameStringKey, new object[0]),
            m_showAlertIcon = true,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK,
            m_responseCallback = null
        };
        DialogManager.Get().ShowPopup(info);
    }

    private void Start()
    {
        this.m_InputTextField.SetInputFont(this.m_localizedInputFont);
        this.m_InputTextField.Text = this.m_inputText;
        this.UpdateInstructions();
    }

    private void UpdateInstructions()
    {
        this.m_InstructionText.gameObject.SetActive(string.IsNullOrEmpty(this.m_inputText) && string.IsNullOrEmpty(Input.compositionString));
    }

    public void UpdateLayout()
    {
        this.Layout();
    }
}

