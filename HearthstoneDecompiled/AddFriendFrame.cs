using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AddFriendFrame : MonoBehaviour
{
    public AddFriendFrameBones m_Bones;
    public UberText m_HeaderText;
    private PegUIElement m_inputBlocker;
    public Font m_InputFont;
    private string m_inputText = string.Empty;
    public TextField m_InputTextField;
    public UberText m_InstructionText;
    private Font m_localizedInputFont;
    private BnetPlayer m_player;
    private string m_playerDisplayName;
    private bool m_usePlayer;

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
        if (this.m_inputBlocker != null)
        {
            UnityEngine.Object.Destroy(this.m_inputBlocker.gameObject);
        }
        UnityEngine.Object.Destroy(base.gameObject);
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
        GameObject obj2 = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "AddFriendInputBlocker");
        obj2.transform.parent = base.transform.parent;
        this.m_inputBlocker = obj2.AddComponent<PegUIElement>();
        this.m_inputBlocker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnInputBlockerReleased));
        TransformUtil.SetPosZ(this.m_inputBlocker, base.transform.position.z + 1f);
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
        this.m_HeaderText.Text = GameStrings.Get("GLOBAL_ADDFRIEND_HEADER");
        this.m_InstructionText.Text = GameStrings.Get("GLOBAL_ADDFRIEND_INSTRUCTION");
        this.InitInputBlocker();
    }

    private void Layout()
    {
        base.transform.parent = BaseUI.Get().transform;
        base.transform.position = BaseUI.Get().GetAddFriendBone().position;
        if ((UniversalInputManager.Get().IsTouchMode() && W8Touch.s_isWindows8OrGreater) || W8Touch.Get().IsVirtualKeyboardVisible())
        {
            Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + 100f, base.transform.position.z);
            base.transform.position = vector;
        }
    }

    private void OnClosed()
    {
        if (this.Closed != null)
        {
            this.Closed();
        }
    }

    private void OnInputBlockerReleased(UIEvent e)
    {
        this.OnClosed();
    }

    private void OnInputCanceled()
    {
        this.OnClosed();
    }

    private void OnInputChanged(string text)
    {
        this.m_inputText = text;
        this.UpdateInstructions();
        this.m_usePlayer = string.Compare(this.m_playerDisplayName, text.Trim(), true) == 0;
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
        string email = !this.m_usePlayer ? input.Trim() : this.m_player.GetBattleTag().ToString();
        if (email.Contains("@"))
        {
            BnetFriendMgr.Get().SendInviteByEmail(email);
        }
        else if (email.Contains("#"))
        {
            BnetFriendMgr.Get().SendInviteByBattleTag(email);
        }
        else
        {
            string message = GameStrings.Get("GLOBAL_ADDFRIEND_ERROR_MALFORMED");
            UIStatus.Get().AddError(message);
        }
        this.OnClosed();
    }

    public void SetPlayer(BnetPlayer player)
    {
        this.m_player = player;
        if (player == null)
        {
            this.m_usePlayer = false;
            this.m_playerDisplayName = null;
        }
        else
        {
            this.m_usePlayer = true;
            this.m_playerDisplayName = FriendUtils.GetUniqueName(this.m_player);
        }
        this.m_inputText = this.m_playerDisplayName;
        this.m_InputTextField.Text = this.m_inputText;
        this.UpdateInstructions();
    }

    private void Start()
    {
        this.m_InputTextField.SetInputFont(this.m_localizedInputFont);
        this.m_InputTextField.Activate();
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

