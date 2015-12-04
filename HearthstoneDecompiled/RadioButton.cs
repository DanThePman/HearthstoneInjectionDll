using System;
using UnityEngine;

public class RadioButton : PegUIElement
{
    public GameObject m_hoverGlow;
    private int m_id;
    public GameObject m_selectedGlow;
    private object m_userData;

    protected override void Awake()
    {
        base.Awake();
        this.m_hoverGlow.SetActive(false);
        this.m_selectedGlow.SetActive(false);
        SoundManager.Get().Load("tiny_button_press_2");
        SoundManager.Get().Load("tiny_button_mouseover_2");
    }

    public int GetButtonID()
    {
        return this.m_id;
    }

    public object GetUserData()
    {
        return this.m_userData;
    }

    public bool IsSelected()
    {
        return this.m_selectedGlow.activeSelf;
    }

    protected override void OnDoubleClick()
    {
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_hoverGlow.SetActive(false);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        SoundManager.Get().LoadAndPlay("tiny_button_mouseover_2");
        this.m_hoverGlow.SetActive(true);
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        SoundManager.Get().LoadAndPlay("tiny_button_press_2");
    }

    public void SetButtonID(int id)
    {
        this.m_id = id;
    }

    public void SetSelected(bool selected)
    {
        this.m_selectedGlow.SetActive(selected);
    }

    public void SetUserData(object userData)
    {
        this.m_userData = userData;
    }
}

