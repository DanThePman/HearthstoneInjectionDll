using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureChooserButton : AdventureGenericButton
{
    [CustomEditField(Sections="Sub Button Settings"), SerializeField]
    public iTween.EaseType m_ActivateEaseType = iTween.EaseType.easeOutBounce;
    private AdventureDbId m_AdventureId;
    [SerializeField]
    private float m_ButtonBottomPadding;
    [SerializeField, CustomEditField(Sections="Button State Table")]
    public StateEventTable m_ButtonStateTable;
    [SerializeField, CustomEditField(Sections="Sub Button Settings")]
    public iTween.EaseType m_DeactivateEaseType = iTween.EaseType.easeOutSine;
    private List<Expanded> m_ExpandedEventList = new List<Expanded>();
    private AdventureChooserSubButton m_LastSelectedSubButton;
    private Vector3 m_MainButtonExtents = Vector3.zero;
    private List<ModeSelection> m_ModeSelectionEventList = new List<ModeSelection>();
    private bool m_SelectSubButtonOnToggle;
    [SerializeField, CustomEditField(Sections="Sub Button Settings")]
    public float m_SubButtonAnimationTime = 0.25f;
    [CustomEditField(Sections="Sub Button Settings"), SerializeField]
    public GameObject m_SubButtonContainer;
    [SerializeField]
    private float m_SubButtonContainerBtmPadding = 0.1f;
    [SerializeField]
    private float m_SubButtonHeight = 3.75f;
    private List<AdventureChooserSubButton> m_SubButtons = new List<AdventureChooserSubButton>();
    [CustomEditField(Sections="Sub Button Settings"), SerializeField]
    public float m_SubButtonShowPosZ;
    [SerializeField, CustomEditField(Sections="Sub Button Settings")]
    public float m_SubButtonVisibilityPadding = 5f;
    private bool m_Toggled;
    private List<Toggled> m_ToggleEventList = new List<Toggled>();
    private List<VisualUpdated> m_VisualUpdatedEventList = new List<VisualUpdated>();
    private const string s_EventButtonContract = "Contract";
    private const string s_EventButtonExpand = "Expand";

    public void AddExpandedListener(Expanded dlg)
    {
        this.m_ExpandedEventList.Add(dlg);
    }

    public void AddModeSelectionListener(ModeSelection dlg)
    {
        this.m_ModeSelectionEventList.Add(dlg);
    }

    public void AddToggleListener(Toggled dlg)
    {
        this.m_ToggleEventList.Add(dlg);
    }

    public void AddVisualUpdatedListener(VisualUpdated dlg)
    {
        this.m_VisualUpdatedEventList.Add(dlg);
    }

    protected override void Awake()
    {
        base.Awake();
        this.m_SubButtonContainer.SetActive(this.m_Toggled);
        this.m_SubButtonContainer.transform.localPosition = this.GetHiddenPosition();
        this.AddEventListener(UIEventType.RELEASE, e => this.ToggleButton(!this.m_Toggled));
        if (base.m_PortraitRenderer != null)
        {
            this.m_MainButtonExtents = base.m_PortraitRenderer.bounds.extents;
        }
    }

    public bool ContainsSubButton(AdventureChooserSubButton btn)
    {
        <ContainsSubButton>c__AnonStorey298 storey = new <ContainsSubButton>c__AnonStorey298 {
            btn = btn
        };
        return this.m_SubButtons.Exists(new Predicate<AdventureChooserSubButton>(storey.<>m__2));
    }

    public AdventureChooserSubButton CreateSubButton(AdventureModeDbId id, AdventureSubDef subDef, string subButtonPrefab, bool useAsLastSelected)
    {
        <CreateSubButton>c__AnonStorey297 storey = new <CreateSubButton>c__AnonStorey297 {
            <>f__this = this
        };
        if (this.m_SubButtonContainer == null)
        {
            Debug.LogError("m_SubButtonContainer cannot be null. Unable to create subbutton.", this);
            return null;
        }
        storey.newsubbutton = GameUtils.LoadGameObjectWithComponent<AdventureChooserSubButton>(subButtonPrefab);
        if (storey.newsubbutton == null)
        {
            return null;
        }
        GameUtils.SetParent(storey.newsubbutton, this.m_SubButtonContainer, false);
        if (useAsLastSelected || (this.m_LastSelectedSubButton == null))
        {
            this.m_LastSelectedSubButton = storey.newsubbutton;
        }
        storey.newsubbutton.gameObject.name = string.Format("AdventureChooserSubButton({0})", id);
        storey.newsubbutton.SetAdventure(this.m_AdventureId, id);
        storey.newsubbutton.SetButtonText(subDef.GetShortName());
        storey.newsubbutton.SetPortraitTexture(subDef.m_Texture);
        storey.newsubbutton.SetPortraitTiling(subDef.m_TextureTiling);
        storey.newsubbutton.SetPortraitOffset(subDef.m_TextureOffset);
        storey.newsubbutton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storey.<>m__1));
        this.m_SubButtons.Add(storey.newsubbutton);
        this.UpdateButtonPositions();
        this.m_SubButtonContainer.transform.localPosition = this.GetHiddenPosition();
        return storey.newsubbutton;
    }

    public void DisableSubButtonHighlights()
    {
        foreach (AdventureChooserSubButton button in this.m_SubButtons)
        {
            button.SetHighlight(false);
        }
    }

    private void FireExpandedEvent(bool expand)
    {
        foreach (Expanded expanded in this.m_ExpandedEventList.ToArray())
        {
            expanded(this, expand);
        }
    }

    private void FireModeSelectedEvent(AdventureChooserSubButton btn)
    {
        foreach (ModeSelection selection in this.m_ModeSelectionEventList.ToArray())
        {
            selection(btn);
        }
    }

    private void FireToggleEvent()
    {
        foreach (Toggled toggled in this.m_ToggleEventList.ToArray())
        {
            toggled(this.m_Toggled);
        }
    }

    private void FireVisualUpdatedEvent()
    {
        foreach (VisualUpdated updated in this.m_VisualUpdatedEventList.ToArray())
        {
            updated();
        }
    }

    public AdventureDbId GetAdventure()
    {
        return this.m_AdventureId;
    }

    public float GetFullButtonHeight()
    {
        if ((base.m_PortraitRenderer == null) || (this.m_SubButtonContainer == null))
        {
            return TransformUtil.GetBoundsOfChildren(base.gameObject).size.z;
        }
        float num = (this.m_SubButtonContainer.transform.localPosition.z + (this.m_SubButtonHeight * this.m_SubButtons.Count)) + this.m_SubButtonContainerBtmPadding;
        float num2 = base.m_PortraitRenderer.transform.localPosition.z - this.m_MainButtonExtents.z;
        float num3 = base.m_PortraitRenderer.transform.localPosition.z + this.m_MainButtonExtents.z;
        return ((Math.Max(num3, num) - num2) - this.m_ButtonBottomPadding);
    }

    private Vector3 GetHiddenPosition()
    {
        Vector3 localPosition = this.m_SubButtonContainer.transform.localPosition;
        return new Vector3(localPosition.x, localPosition.y, (this.m_SubButtonShowPosZ - (this.m_SubButtonHeight * this.m_SubButtons.Count)) - this.m_SubButtonContainerBtmPadding);
    }

    private Vector3 GetShowPosition()
    {
        Vector3 localPosition = this.m_SubButtonContainer.transform.localPosition;
        return new Vector3(localPosition.x, localPosition.y, this.m_SubButtonShowPosZ);
    }

    public AdventureChooserSubButton GetSubButtonFromMode(AdventureModeDbId mode)
    {
        foreach (AdventureChooserSubButton button in this.m_SubButtons)
        {
            if (button.GetMode() == mode)
            {
                return button;
            }
        }
        return null;
    }

    public AdventureChooserSubButton[] GetSubButtons()
    {
        return this.m_SubButtons.ToArray();
    }

    private void OnButtonAnimating(Vector3 curr, float zposshowlimit)
    {
        this.m_SubButtonContainer.transform.localPosition = curr;
        this.UpdateSubButtonsVisibility(curr, zposshowlimit);
        this.FireVisualUpdatedEvent();
    }

    private void OnExpandAnimationComplete()
    {
        if (this.m_SubButtonContainer.activeSelf != this.m_Toggled)
        {
            this.m_SubButtonContainer.SetActive(this.m_Toggled);
        }
        this.FireExpandedEvent(this.m_Toggled);
    }

    private void OnSubButtonClicked(AdventureChooserSubButton btn)
    {
        this.m_LastSelectedSubButton = btn;
        this.FireModeSelectedEvent(btn);
        foreach (AdventureChooserSubButton button in this.m_SubButtons)
        {
            button.SetHighlight(button == btn);
        }
    }

    public void SetAdventure(AdventureDbId id)
    {
        this.m_AdventureId = id;
    }

    public void SetSelectSubButtonOnToggle(bool flag)
    {
        this.m_SelectSubButtonOnToggle = flag;
    }

    public void ToggleButton(bool toggle)
    {
        if (toggle != this.m_Toggled)
        {
            this.m_Toggled = toggle;
            this.m_ButtonStateTable.CancelQueuedStates();
            this.m_ButtonStateTable.TriggerState(!this.m_Toggled ? "Contract" : "Expand", true, null);
            if (this.m_Toggled)
            {
                this.m_SubButtonContainer.SetActive(true);
            }
            Vector3 hiddenPosition = this.GetHiddenPosition();
            Vector3 showPosition = this.GetShowPosition();
            Vector3 curr = !this.m_Toggled ? showPosition : hiddenPosition;
            Vector3 vector4 = !this.m_Toggled ? hiddenPosition : showPosition;
            this.m_SubButtonContainer.transform.localPosition = curr;
            this.UpdateSubButtonsVisibility(curr, this.m_SubButtonShowPosZ);
            object[] args = new object[] { 
                "islocal", true, "from", curr, "to", vector4, "time", this.m_SubButtonAnimationTime, "easeType", !this.m_Toggled ? this.m_DeactivateEaseType : this.m_ActivateEaseType, "oncomplete", "OnExpandAnimationComplete", "oncompletetarget", base.gameObject, "onupdate", newVal => this.OnButtonAnimating((Vector3) newVal, this.m_SubButtonShowPosZ), 
                "onupdatetarget", base.gameObject
             };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ValueTo(base.gameObject, hashtable);
            this.FireToggleEvent();
            if ((this.m_Toggled && this.m_SelectSubButtonOnToggle) && (this.m_LastSelectedSubButton != null))
            {
                this.OnSubButtonClicked(this.m_LastSelectedSubButton);
            }
        }
    }

    public void UpdateButtonPositions()
    {
        float subButtonHeight = this.m_SubButtonHeight;
        for (int i = 1; i < this.m_SubButtons.Count; i++)
        {
            AdventureChooserSubButton component = this.m_SubButtons[i];
            TransformUtil.SetLocalPosZ(component, subButtonHeight * i);
        }
    }

    private void UpdateSubButtonsVisibility(Vector3 curr, float zposshowlimit)
    {
        float subButtonHeight = this.m_SubButtonHeight;
        for (int i = 0; i < this.m_SubButtons.Count; i++)
        {
            float num3 = (subButtonHeight * (i + 1)) + curr.z;
            bool flag = (zposshowlimit - num3) <= this.m_SubButtonVisibilityPadding;
            GameObject gameObject = this.m_SubButtons[i].gameObject;
            if (gameObject.activeSelf != flag)
            {
                gameObject.SetActive(flag);
            }
        }
    }

    [CustomEditField(Sections="Button Settings")]
    public float ButtonBottomPadding
    {
        get
        {
            return this.m_ButtonBottomPadding;
        }
        set
        {
            this.m_ButtonBottomPadding = value;
            this.UpdateButtonPositions();
        }
    }

    [CustomEditField(Sections="Sub Button Settings")]
    public float SubButtonContainerBtmPadding
    {
        get
        {
            return this.m_SubButtonContainerBtmPadding;
        }
        set
        {
            this.m_SubButtonContainerBtmPadding = value;
            this.UpdateButtonPositions();
        }
    }

    [CustomEditField(Sections="Sub Button Settings")]
    public float SubButtonHeight
    {
        get
        {
            return this.m_SubButtonHeight;
        }
        set
        {
            this.m_SubButtonHeight = value;
            this.UpdateButtonPositions();
        }
    }

    [CustomEditField(Sections="Button Settings")]
    public bool Toggle
    {
        get
        {
            return this.m_Toggled;
        }
        set
        {
            this.ToggleButton(value);
        }
    }

    [CompilerGenerated]
    private sealed class <ContainsSubButton>c__AnonStorey298
    {
        internal AdventureChooserSubButton btn;

        internal bool <>m__2(AdventureChooserSubButton x)
        {
            return (x == this.btn);
        }
    }

    [CompilerGenerated]
    private sealed class <CreateSubButton>c__AnonStorey297
    {
        internal AdventureChooserButton <>f__this;
        internal AdventureChooserSubButton newsubbutton;

        internal void <>m__1(UIEvent e)
        {
            this.<>f__this.OnSubButtonClicked(this.newsubbutton);
        }
    }

    public delegate void Expanded(AdventureChooserButton button, bool expand);

    public delegate void ModeSelection(AdventureChooserSubButton btn);

    public delegate void Toggled(bool toggle);

    public delegate void VisualUpdated();
}

