using System;
using UnityEngine;

public class ArenaPhoneControl : MonoBehaviour
{
    public BoxCollider m_ButtonCollider;
    public UberText m_ChooseText;
    public Vector3 m_CountAndViewDeckCollCenter;
    public Vector3 m_CountAndViewDeckCollSize;
    private ControlMode m_CurrentMode;
    public GameObject m_ViewDeckButton;
    public Vector3 m_ViewDeckCollCenter;
    public Vector3 m_ViewDeckCollSize;

    private void Awake()
    {
        this.m_CurrentMode = ControlMode.ChooseHero;
        this.m_ButtonCollider.enabled = false;
        this.m_ChooseText.Text = GameStrings.Get("GLUE_CHOOSE_YOUR_HERO");
    }

    private void RotateTo(float rotFrom, float rotTo)
    {
        object[] args = new object[] { "from", rotFrom, "to", rotTo, "time", 1f, "easetype", iTween.EaseType.easeOutBounce, "onupdate", val => base.transform.localRotation = Quaternion.Euler((float) val, 0f, 0f) };
        iTween.ValueTo(base.gameObject, iTween.Hash(args));
    }

    public void SetMode(ControlMode mode)
    {
        if (mode != this.m_CurrentMode)
        {
            switch (mode)
            {
                case ControlMode.ChooseHero:
                    this.m_ViewDeckButton.SetActive(false);
                    this.m_ButtonCollider.enabled = false;
                    this.m_ChooseText.Text = GameStrings.Get("GLUE_CHOOSE_YOUR_HERO");
                    if (this.m_CurrentMode == ControlMode.CardCountViewDeck)
                    {
                        this.RotateTo(180f, 0f);
                    }
                    break;

                case ControlMode.ChooseCard:
                    this.m_ViewDeckButton.SetActive(false);
                    this.m_ButtonCollider.enabled = false;
                    this.m_ChooseText.Text = GameStrings.Get("GLUE_DRAFT_INSTRUCTIONS");
                    if (this.m_CurrentMode == ControlMode.CardCountViewDeck)
                    {
                        this.RotateTo(180f, 0f);
                    }
                    break;

                case ControlMode.CardCountViewDeck:
                    this.m_ButtonCollider.center = this.m_CountAndViewDeckCollCenter;
                    this.m_ButtonCollider.size = this.m_CountAndViewDeckCollSize;
                    this.m_ButtonCollider.enabled = true;
                    this.RotateTo(0f, 180f);
                    break;

                case ControlMode.ViewDeck:
                    this.m_ButtonCollider.center = this.m_ViewDeckCollCenter;
                    this.m_ButtonCollider.size = this.m_ViewDeckCollSize;
                    this.m_ViewDeckButton.SetActive(true);
                    this.m_ButtonCollider.enabled = true;
                    if (this.m_CurrentMode == ControlMode.CardCountViewDeck)
                    {
                        this.RotateTo(180f, 0f);
                    }
                    break;

                case ControlMode.Rewards:
                    this.m_ViewDeckButton.SetActive(false);
                    this.m_ButtonCollider.enabled = false;
                    this.m_ChooseText.Text = string.Empty;
                    if (this.m_CurrentMode == ControlMode.CardCountViewDeck)
                    {
                        this.RotateTo(180f, 0f);
                    }
                    break;
            }
            this.m_CurrentMode = mode;
        }
    }

    [ContextMenu("CardCountViewDeck")]
    public void SetModeCardCountViewDeck()
    {
        this.SetMode(ControlMode.CardCountViewDeck);
    }

    [ContextMenu("ChooseCard")]
    public void SetModeChooseCard()
    {
        this.SetMode(ControlMode.ChooseCard);
    }

    [ContextMenu("ChooseHero")]
    public void SetModeChooseHero()
    {
        this.SetMode(ControlMode.ChooseHero);
    }

    [ContextMenu("ViewDeck")]
    public void SetModeViewDeck()
    {
        this.SetMode(ControlMode.ViewDeck);
    }

    public enum ControlMode
    {
        ChooseHero,
        ChooseCard,
        CardCountViewDeck,
        ViewDeck,
        Rewards
    }
}

