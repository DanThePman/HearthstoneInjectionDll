using System;
using UnityEngine;

public class OptionsMenuPhone : MonoBehaviour
{
    public UIBButton m_doneButton;
    public GameObject m_mainContentsPanel;
    public OptionsMenu m_optionsMenu;

    private void Start()
    {
        this.m_doneButton.AddEventListener(UIEventType.RELEASE, e => this.m_optionsMenu.Hide(true));
    }
}

