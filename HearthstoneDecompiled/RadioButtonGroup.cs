using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class RadioButtonGroup : MonoBehaviour
{
    public GameObject m_buttonContainer;
    private DelButtonDoubleClicked m_buttonDoubleClickedCB;
    private DelButtonSelected m_buttonSelectedCB;
    public GameObject m_firstRadioButtonBone;
    public FramedRadioButton m_framedRadioButtonPrefab;
    private List<FramedRadioButton> m_framedRadioButtons = new List<FramedRadioButton>();
    private Vector3 m_spacingFudgeFactor = Vector3.zero;

    private FramedRadioButton CreateNewFramedRadioButton()
    {
        FramedRadioButton button = UnityEngine.Object.Instantiate<FramedRadioButton>(this.m_framedRadioButtonPrefab);
        button.transform.parent = this.m_buttonContainer.transform;
        button.transform.localPosition = Vector3.zero;
        button.transform.localScale = Vector3.one;
        button.transform.localRotation = Quaternion.identity;
        button.m_radioButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnRadioButtonReleased));
        button.m_radioButton.AddEventListener(UIEventType.DOUBLECLICK, new UIEvent.Handler(this.OnRadioButtonDoubleClicked));
        return button;
    }

    public void Hide()
    {
        this.m_buttonContainer.SetActive(false);
    }

    private void OnRadioButtonDoubleClicked(UIEvent e)
    {
        if (this.m_buttonDoubleClickedCB != null)
        {
            RadioButton element = e.GetElement() as RadioButton;
            if (element == null)
            {
                Debug.LogWarning(string.Format("RadioButtonGroup.OnRadioButtonDoubleClicked(): UIEvent {0} source is not a RadioButton!", e));
            }
            else
            {
                FramedRadioButton framedRadioButton = null;
                foreach (FramedRadioButton button3 in this.m_framedRadioButtons)
                {
                    if (element == button3.m_radioButton)
                    {
                        framedRadioButton = button3;
                        break;
                    }
                }
                if (framedRadioButton == null)
                {
                    Debug.LogWarning(string.Format("RadioButtonGroup.OnRadioButtonDoubleClicked(): could not find framed radio button for radio button ID {0}", element.GetButtonID()));
                }
                else
                {
                    this.m_buttonDoubleClickedCB(framedRadioButton);
                }
            }
        }
    }

    private void OnRadioButtonReleased(UIEvent e)
    {
        RadioButton element = e.GetElement() as RadioButton;
        if (element == null)
        {
            Debug.LogWarning(string.Format("RadioButtonGroup.OnRadioButtonReleased(): UIEvent {0} source is not a RadioButton!", e));
        }
        else
        {
            bool flag = element.IsSelected();
            foreach (FramedRadioButton button2 in this.m_framedRadioButtons)
            {
                RadioButton radioButton = button2.m_radioButton;
                bool selected = element == radioButton;
                radioButton.SetSelected(selected);
            }
            if (this.m_buttonSelectedCB != null)
            {
                this.m_buttonSelectedCB(element.GetButtonID(), element.GetUserData());
                if (UniversalInputManager.Get().IsTouchMode() && flag)
                {
                    this.OnRadioButtonDoubleClicked(e);
                }
            }
        }
    }

    public void SetSpacingFudgeFactor(Vector3 amount)
    {
        this.m_spacingFudgeFactor = amount;
    }

    public void ShowButtons(List<ButtonData> buttonData, DelButtonSelected buttonSelectedCallback, DelButtonDoubleClicked buttonDoubleClickedCallback)
    {
        this.m_buttonContainer.SetActive(true);
        int count = buttonData.Count;
        while (this.m_framedRadioButtons.Count > count)
        {
            FramedRadioButton button = this.m_framedRadioButtons[0];
            this.m_framedRadioButtons.RemoveAt(0);
            UnityEngine.Object.DestroyImmediate(button);
        }
        bool flag = 1 == count;
        Vector3 position = this.m_buttonContainer.transform.position;
        GameObject relative = new GameObject();
        RadioButton radioButton = null;
        for (int i = 0; i < count; i++)
        {
            FramedRadioButton button3;
            FramedRadioButton.FrameType sINGLE;
            if (this.m_framedRadioButtons.Count > i)
            {
                button3 = this.m_framedRadioButtons[i];
            }
            else
            {
                button3 = this.CreateNewFramedRadioButton();
                this.m_framedRadioButtons.Add(button3);
            }
            if (flag)
            {
                sINGLE = FramedRadioButton.FrameType.SINGLE;
            }
            else if (i == 0)
            {
                sINGLE = FramedRadioButton.FrameType.MULTI_LEFT_END;
            }
            else if ((count - 1) == i)
            {
                sINGLE = FramedRadioButton.FrameType.MULTI_RIGHT_END;
            }
            else
            {
                sINGLE = FramedRadioButton.FrameType.MULTI_MIDDLE;
            }
            ButtonData data = buttonData[i];
            button3.Show();
            button3.Init(sINGLE, data.m_text, data.m_id, data.m_userData);
            if (data.m_selected)
            {
                if (radioButton != null)
                {
                    Debug.LogWarning("RadioButtonGroup.WaitThenShowButtons(): more than one button was set as selected. Selecting the FIRST provided option.");
                    button3.m_radioButton.SetSelected(false);
                }
                else
                {
                    radioButton = button3.m_radioButton;
                    radioButton.SetSelected(true);
                }
            }
            else
            {
                button3.m_radioButton.SetSelected(false);
            }
            if (i == 0)
            {
                TransformUtil.SetPoint(button3.gameObject, Anchor.LEFT, this.m_firstRadioButtonBone, Anchor.LEFT);
            }
            else
            {
                TransformUtil.SetPoint(button3.gameObject, new Vector3(0f, 1f, 0.5f), relative, new Vector3(1f, 1f, 0.5f), this.m_spacingFudgeFactor);
            }
            relative = button3.m_frameFill;
        }
        position.x -= TransformUtil.GetBoundsOfChildren(this.m_buttonContainer).size.x / 2f;
        this.m_buttonContainer.transform.position = position;
        this.m_buttonSelectedCB = buttonSelectedCallback;
        this.m_buttonDoubleClickedCB = buttonDoubleClickedCallback;
        if ((radioButton != null) && (this.m_buttonSelectedCB != null))
        {
            this.m_buttonSelectedCB(radioButton.GetButtonID(), radioButton.GetUserData());
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ButtonData
    {
        public int m_id;
        public string m_text;
        public bool m_selected;
        public object m_userData;
        public ButtonData(int id, string text, object userData, bool selected)
        {
            this.m_id = id;
            this.m_text = text;
            this.m_userData = userData;
            this.m_selected = selected;
        }
    }

    public delegate void DelButtonDoubleClicked(FramedRadioButton framedRadioButton);

    public delegate void DelButtonSelected(int buttonID, object userData);
}

