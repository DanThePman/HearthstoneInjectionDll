using System;
using System.Collections;
using UnityEngine;

public class UIStatus : MonoBehaviour
{
    public Color m_ErrorColor;
    public float m_FadeDelaySec = 2f;
    public iTween.EaseType m_FadeEaseType = iTween.EaseType.linear;
    public float m_FadeSec = 0.5f;
    public Color m_InfoColor;
    private bool m_isScreenshot;
    public UberText m_Text;
    private static UIStatus s_instance;

    public void AddError(string message)
    {
        this.m_Text.TextColor = this.m_ErrorColor;
        this.ShowMessage(message);
    }

    public void AddInfo(string message)
    {
        this.AddInfo(message, false);
    }

    public void AddInfo(string message, bool isScreenshot)
    {
        this.AddInfo(message, isScreenshot, -1f);
    }

    public void AddInfo(string message, float delay)
    {
        this.AddInfo(message, false, delay);
    }

    public void AddInfo(string message, bool isScreenshot, float delay)
    {
        this.m_isScreenshot = isScreenshot;
        this.m_Text.TextColor = this.m_InfoColor;
        this.ShowMessage(message, delay);
    }

    private void Awake()
    {
        s_instance = this;
        this.m_Text.gameObject.SetActive(false);
        if (OverlayUI.Get() == null)
        {
            throw new UnityException("Trying to create UIStatus before OverlayUI!");
        }
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
    }

    public static UIStatus Get()
    {
        if (s_instance == null)
        {
            s_instance = AssetLoader.Get().LoadUIScreen("UIStatus", true, false).GetComponent<UIStatus>();
        }
        return s_instance;
    }

    public void HideIfScreenshotMessage()
    {
        if (this.m_isScreenshot)
        {
            iTween.Stop(this.m_Text.gameObject);
            this.OnFadeComplete();
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnFadeComplete()
    {
        this.m_isScreenshot = false;
        this.m_Text.gameObject.SetActive(false);
    }

    private void ShowMessage(string message)
    {
        this.ShowMessage(message, -1f);
    }

    private void ShowMessage(string message, float delay)
    {
        this.m_Text.Text = string.Empty;
        if (message.Contains("\n"))
        {
            this.m_Text.ResizeToFit = false;
            this.m_Text.WordWrap = true;
            this.m_Text.ForceWrapLargeWords = true;
        }
        else
        {
            this.m_Text.ResizeToFit = true;
            this.m_Text.WordWrap = false;
            this.m_Text.ForceWrapLargeWords = false;
        }
        this.m_Text.Text = message;
        this.m_Text.gameObject.SetActive(true);
        this.m_Text.TextAlpha = 1f;
        iTween.Stop(this.m_Text.gameObject, true);
        if (delay < 0f)
        {
            delay = this.m_FadeDelaySec;
        }
        object[] args = new object[] { "amount", 0f, "delay", delay, "time", this.m_FadeSec, "easeType", this.m_FadeEaseType, "oncomplete", "OnFadeComplete", "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(this.m_Text.gameObject, hashtable);
    }
}

