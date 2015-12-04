using System;
using UnityEngine;

public class GameplayErrorManager : MonoBehaviour
{
    private float m_displaySecsLeft;
    private GUIStyle m_errorDisplayStyle;
    public GameplayErrorCloud m_errorMessagePrefab;
    private string m_message;
    private static GameplayErrorManager s_instance;
    private static GameplayErrorCloud s_messageInstance;

    private void Awake()
    {
        s_instance = this;
        s_messageInstance = UnityEngine.Object.Instantiate<GameplayErrorCloud>(this.m_errorMessagePrefab);
    }

    public void DisplayMessage(string message)
    {
        this.m_message = message;
        this.m_displaySecsLeft = message.Length * 0.1f;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            s_messageInstance.transform.localPosition = new Vector3(-7.9f, 9f, -4.43f);
            s_messageInstance.gameObject.GetComponentInChildren<UberText>().gameObject.transform.localPosition = new Vector3(2.49f, 0f, -2.13f);
        }
        else
        {
            s_messageInstance.transform.localPosition = new Vector3(-7.9f, 9.98f, -5.17f);
        }
        s_messageInstance.ShowMessage(this.m_message, this.m_displaySecsLeft);
        SoundManager.Get().LoadAndPlay("UI_no_can_do");
    }

    public static GameplayErrorManager Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void Start()
    {
        this.m_message = string.Empty;
        this.m_errorDisplayStyle = new GUIStyle();
        this.m_errorDisplayStyle.fontSize = 0x18;
        this.m_errorDisplayStyle.fontStyle = FontStyle.Bold;
        this.m_errorDisplayStyle.alignment = TextAnchor.UpperCenter;
    }
}

