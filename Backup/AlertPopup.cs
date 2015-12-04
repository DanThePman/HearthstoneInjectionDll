using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AlertPopup : DialogBase
{
    private const float BUTTON_FRAME_WIDTH = 80f;
    public GameObject m_alertIcon;
    public Vector3 m_alertIconOffset;
    public UberText m_alertText;
    public NineSliceElement m_body;
    public MultiSliceElement m_buttonContainer;
    public List<GameObject> m_buttonIconsSet1 = new List<GameObject>();
    public List<GameObject> m_buttonIconsSet2 = new List<GameObject>();
    public UIBButton m_cancelButton;
    public GameObject m_clickCatcher;
    public UIBButton m_confirmButton;
    public Header m_header;
    public Vector3 m_loadPosition;
    public UIBButton m_okayButton;
    public float m_padding;
    private PopupInfo m_popupInfo;
    public Vector3 m_showPosition;
    private PopupInfo m_updateInfo;

    protected override void Awake()
    {
        base.Awake();
        this.m_okayButton.AddEventListener(UIEventType.RELEASE, e => this.ButtonPress(Response.OK));
        this.m_confirmButton.AddEventListener(UIEventType.RELEASE, e => this.ButtonPress(Response.CONFIRM));
        this.m_cancelButton.AddEventListener(UIEventType.RELEASE, e => this.ButtonPress(Response.CANCEL));
    }

    private void ButtonPress(Response response)
    {
        if (this.m_popupInfo.m_responseCallback != null)
        {
            this.m_popupInfo.m_responseCallback(response, this.m_popupInfo.m_responseUserData);
        }
        this.Hide();
    }

    public PopupInfo GetInfo()
    {
        return this.m_popupInfo;
    }

    public override void GoBack()
    {
        this.ButtonPress(Response.CANCEL);
    }

    public override bool HandleKeyboardInput()
    {
        if (((Input.GetKeyUp(KeyCode.Escape) && (this.m_popupInfo != null)) && (this.m_popupInfo.m_keyboardEscIsCancel && this.m_cancelButton.enabled)) && this.m_cancelButton.gameObject.activeSelf)
        {
            this.GoBack();
            return true;
        }
        return false;
    }

    private void InitInfo()
    {
        if (this.m_popupInfo == null)
        {
            this.m_popupInfo = new PopupInfo();
        }
        if (this.m_popupInfo.m_headerText == null)
        {
            this.m_popupInfo.m_headerText = GameStrings.Get("GLOBAL_DEFAULT_ALERT_HEADER");
        }
    }

    private void OnDestroy()
    {
        if (UniversalInputManager.Get() != null)
        {
            UniversalInputManager.Get().SetSystemDialogActive(false);
        }
    }

    protected override void OnHideAnimFinished()
    {
        UniversalInputManager.Get().SetSystemDialogActive(false);
        base.OnHideAnimFinished();
    }

    protected override void OnShowAnimFinished()
    {
        base.OnShowAnimFinished();
        if (this.m_updateInfo != null)
        {
            this.UpdateInfoAfterAnim();
        }
    }

    public void SetInfo(PopupInfo info)
    {
        this.m_popupInfo = info;
    }

    public override void Show()
    {
        base.Show();
        this.InitInfo();
        this.UpdateAll(this.m_popupInfo);
        Transform transform = base.transform;
        transform.localPosition += this.m_popupInfo.m_offset;
        if (this.m_popupInfo.m_layerToUse.HasValue)
        {
            SceneUtils.SetLayer(this, this.m_popupInfo.m_layerToUse.Value);
        }
        if (this.m_popupInfo.m_disableBlocker)
        {
            this.m_clickCatcher.SetActive(false);
        }
        base.DoShowAnimation();
        bool active = ((this.m_popupInfo == null) || !this.m_popupInfo.m_layerToUse.HasValue) || (((GameLayer) this.m_popupInfo.m_layerToUse.Value) == GameLayer.UI);
        UniversalInputManager.Get().SetSystemDialogActive(active);
    }

    private void Start()
    {
        this.m_alertText.Text = GameStrings.Get("GLOBAL_OKAY");
    }

    private void UpdateAll(PopupInfo popupInfo)
    {
        this.m_alertIcon.SetActive(popupInfo.m_showAlertIcon);
        bool flag = popupInfo.m_iconSet == PopupInfo.IconSet.Default;
        bool flag2 = popupInfo.m_iconSet == PopupInfo.IconSet.Alternate;
        for (int i = 0; i < this.m_buttonIconsSet1.Count; i++)
        {
            this.m_buttonIconsSet1[i].SetActive(flag);
        }
        for (int j = 0; j < this.m_buttonIconsSet2.Count; j++)
        {
            this.m_buttonIconsSet2[j].SetActive(flag2);
        }
        this.UpdateHeaderText(popupInfo.m_headerText);
        this.UpdateTexts(popupInfo);
        this.UpdateLayout();
    }

    private void UpdateButtons(ResponseDisplay displayType)
    {
        this.m_confirmButton.gameObject.SetActive(false);
        this.m_cancelButton.gameObject.SetActive(false);
        this.m_okayButton.gameObject.SetActive(false);
        switch (displayType)
        {
            case ResponseDisplay.OK:
                this.m_okayButton.gameObject.SetActive(true);
                break;

            case ResponseDisplay.CONFIRM:
                this.m_confirmButton.gameObject.SetActive(true);
                break;

            case ResponseDisplay.CANCEL:
                this.m_cancelButton.gameObject.SetActive(true);
                break;

            case ResponseDisplay.CONFIRM_CANCEL:
                this.m_confirmButton.gameObject.SetActive(true);
                this.m_cancelButton.gameObject.SetActive(true);
                break;
        }
        this.m_buttonContainer.UpdateSlices();
        this.m_header.m_container.transform.position = this.m_body.m_top.transform.position;
    }

    private void UpdateHeaderText(string text)
    {
        bool flag = string.IsNullOrEmpty(text);
        this.m_header.m_container.gameObject.SetActive(!flag);
        if (!flag)
        {
            this.m_header.m_text.ResizeToFit = false;
            this.m_header.m_text.Text = text;
            this.m_header.m_text.UpdateNow();
            MeshRenderer component = this.m_body.m_middle.GetComponent<MeshRenderer>();
            float x = this.m_header.m_text.GetTextBounds().size.x;
            float num2 = this.m_header.m_text.transform.worldToLocalMatrix.MultiplyVector(this.m_header.m_text.GetTextBounds().size).x;
            float num3 = 0.8f * this.m_header.m_text.transform.worldToLocalMatrix.MultiplyVector(component.GetComponent<Renderer>().bounds.size).x;
            if (num2 > num3)
            {
                this.m_header.m_text.Width = num3;
                this.m_header.m_text.ResizeToFit = true;
                this.m_header.m_text.UpdateNow();
                x = this.m_header.m_text.GetTextBounds().size.x;
            }
            else
            {
                this.m_header.m_text.Width = num2;
            }
            WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex(x, 0) };
            TransformUtil.SetLocalScaleToWorldDimension(this.m_header.m_middle, dimensions);
            this.m_header.m_container.UpdateSlices();
        }
    }

    public void UpdateInfo(PopupInfo info)
    {
        this.m_updateInfo = info;
        this.UpdateButtons(this.m_updateInfo.m_responseDisplay);
        if (base.m_showAnimState != DialogBase.ShowAnimState.IN_PROGRESS)
        {
            this.UpdateInfoAfterAnim();
        }
    }

    private void UpdateInfoAfterAnim()
    {
        this.m_popupInfo = this.m_updateInfo;
        this.m_updateInfo = null;
        this.UpdateAll(this.m_popupInfo);
    }

    private void UpdateLayout()
    {
        bool activeSelf = this.m_alertIcon.activeSelf;
        Bounds textBounds = this.m_alertText.GetTextBounds();
        float x = textBounds.size.x;
        float a = (textBounds.size.y + this.m_padding) + this.m_popupInfo.m_padding;
        float num3 = 0f;
        float b = 0f;
        if (activeSelf)
        {
            OrientedBounds bounds2 = TransformUtil.ComputeOrientedWorldBounds(this.m_alertIcon, true);
            num3 = bounds2.Extents[0].magnitude * 2f;
            b = bounds2.Extents[2].magnitude * 2f;
        }
        x = Mathf.Max(TransformUtil.GetBoundsOfChildren(this.m_confirmButton).size.x * 2f, x);
        this.m_body.SetSize(x + num3, Mathf.Max(a, b));
        Vector3 offset = new Vector3(0f, 0.01f, 0f);
        TransformUtil.SetPoint(this.m_alertIcon, Anchor.TOP_LEFT_XZ, this.m_body.m_middle, Anchor.TOP_LEFT_XZ, offset);
        Transform transform = this.m_alertIcon.transform;
        transform.localPosition += this.m_alertIconOffset;
        if (this.m_popupInfo.m_alertTextAlignment == UberText.AlignmentOptions.Center)
        {
            TransformUtil.SetPoint(this.m_alertText, Anchor.TOP_XZ, this.m_body.m_middle, Anchor.TOP_XZ, offset);
        }
        else
        {
            TransformUtil.SetPoint(this.m_alertText, Anchor.TOP_LEFT_XZ, this.m_body.m_middle, Anchor.TOP_LEFT_XZ, offset);
        }
        Vector3 position = this.m_alertText.transform.position;
        position.x += num3;
        this.m_alertText.transform.position = position;
        if (this.m_popupInfo.m_alertTextAlignment == UberText.AlignmentOptions.Center)
        {
            this.m_alertText.Width -= num3;
        }
        this.UpdateButtons(this.m_popupInfo.m_responseDisplay);
        this.m_buttonContainer.transform.position = this.m_body.m_bottom.transform.position;
    }

    private void UpdateTexts(PopupInfo popupInfo)
    {
        this.m_alertText.RichText = this.m_popupInfo.m_richTextEnabled;
        this.m_alertText.Alignment = this.m_popupInfo.m_alertTextAlignment;
        if (popupInfo.m_headerText == null)
        {
            popupInfo.m_headerText = GameStrings.Get("GLOBAL_DEFAULT_ALERT_HEADER");
        }
        this.m_alertText.Text = popupInfo.m_text;
        this.m_okayButton.SetText((popupInfo.m_okText != null) ? popupInfo.m_okText : GameStrings.Get("GLOBAL_OKAY"));
        this.m_confirmButton.SetText((popupInfo.m_confirmText != null) ? popupInfo.m_confirmText : GameStrings.Get("GLOBAL_CONFIRM"));
        this.m_cancelButton.SetText((popupInfo.m_cancelText != null) ? popupInfo.m_cancelText : GameStrings.Get("GLOBAL_CANCEL"));
    }

    public string BodyText
    {
        get
        {
            return this.m_alertText.Text;
        }
        set
        {
            this.m_alertText.Text = value;
            if (this.m_popupInfo != null)
            {
                this.UpdateLayout();
            }
        }
    }

    [Serializable]
    public class Header
    {
        public MultiSliceElement m_container;
        public GameObject m_middle;
        public UberText m_text;
    }

    public class PopupInfo
    {
        public UberText.AlignmentOptions m_alertTextAlignment;
        public string m_cancelText;
        public string m_confirmText;
        public bool m_disableBlocker;
        public string m_headerText;
        public IconSet m_iconSet;
        public string m_id;
        public bool m_keyboardEscIsCancel = true;
        public GameLayer? m_layerToUse;
        public Vector3 m_offset = Vector3.zero;
        public string m_okText;
        public float m_padding;
        public AlertPopup.ResponseCallback m_responseCallback;
        public AlertPopup.ResponseDisplay m_responseDisplay = AlertPopup.ResponseDisplay.OK;
        public object m_responseUserData;
        public bool m_richTextEnabled = true;
        public Vector3? m_scaleOverride;
        public bool m_showAlertIcon = true;
        public string m_text;

        public enum IconSet
        {
            Default,
            Alternate,
            None
        }
    }

    public enum Response
    {
        OK,
        CONFIRM,
        CANCEL
    }

    public delegate void ResponseCallback(AlertPopup.Response response, object userData);

    public enum ResponseDisplay
    {
        NONE,
        OK,
        CONFIRM,
        CANCEL,
        CONFIRM_CANCEL
    }
}

