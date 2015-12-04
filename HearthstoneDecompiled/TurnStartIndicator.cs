using System;
using UnityEngine;

public class TurnStartIndicator : MonoBehaviour
{
    private const float AFTER_PUNCH_SCALE_VAL = 9.8f;
    private const float END_SCALE_VAL = 10f;
    public GameObject m_explosionFX;
    public GameObject m_godRays;
    public UberText m_labelBottom;
    public UberText m_labelMiddle;
    public UberText m_labelTop;
    public UberText m_reminderText;
    private const float START_SCALE_VAL = 1f;

    private void DeactivateTurnStartInstance()
    {
        base.gameObject.SetActive(false);
    }

    public void Hide()
    {
    }

    private void HideTurnStartInstance()
    {
        iTween.FadeTo(base.gameObject, 0f, 0.25f);
        object[] args = new object[] { "scale", new Vector3(1f, 1f, 1f), "time", 0.25f, "oncomplete", "DeactivateTurnStartInstance", "oncompletetarget", base.gameObject };
        iTween.ScaleTo(base.gameObject, iTween.Hash(args));
    }

    public bool IsShown()
    {
        return base.gameObject.activeSelf;
    }

    private void PunchTurnStartInstance()
    {
        iTween.ScaleTo(base.gameObject, new Vector3(9.8f, 9.8f, 9.8f), 0.15f);
    }

    public void SetReminderText(string newText)
    {
        this.m_reminderText.Text = newText;
    }

    public void Show()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            base.gameObject.transform.position = new Vector3(-7.8f, 8.2f, -4.2f);
        }
        else
        {
            base.gameObject.transform.position = new Vector3(-7.8f, 8.2f, -5f);
        }
        base.gameObject.SetActive(true);
        this.m_labelTop.Text = GameStrings.Get("GAMEPLAY_YOUR_TURN");
        this.m_labelMiddle.Text = GameStrings.Get("GAMEPLAY_YOUR_TURN");
        this.m_labelBottom.Text = GameStrings.Get("GAMEPLAY_YOUR_TURN");
        iTween.FadeTo(base.gameObject, 1f, 0.25f);
        base.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        object[] args = new object[] { "scale", new Vector3(10f, 10f, 10f), "time", 0.25f, "oncomplete", "PunchTurnStartInstance", "oncompletetarget", base.gameObject };
        iTween.ScaleTo(base.gameObject, iTween.Hash(args));
        object[] objArray2 = new object[] { "position", base.gameObject.transform.position + new Vector3(0.02f, 0.02f, 0.02f), "time", 1.5f, "oncomplete", "HideTurnStartInstance", "oncompletetarget", base.gameObject };
        iTween.MoveTo(base.gameObject, iTween.Hash(objArray2));
        this.m_explosionFX.GetComponent<ParticleSystem>().Play();
    }

    private void Start()
    {
        iTween.FadeTo(base.gameObject, 0f, 0f);
        base.gameObject.transform.position = new Vector3(-7.8f, 8.2f, -5f);
        base.gameObject.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        base.gameObject.SetActive(false);
        this.SetReminderText(string.Empty);
    }
}

