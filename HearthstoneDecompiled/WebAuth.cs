using System;
using UnityEngine;

public class WebAuth
{
    private string m_callbackGameObject;
    private static bool m_isNewCreatedAccount;
    private bool m_show;
    private string m_url;
    private Rect m_window;

    public WebAuth(string url, float x, float y, float width, float height, string gameObjectName)
    {
        Debug.Log("WebAuth");
        this.m_url = url;
        this.m_window = new Rect(x, y, width, height);
        this.m_callbackGameObject = gameObjectName;
        m_isNewCreatedAccount = false;
    }

    public static void ClearBrowserCache()
    {
        ClearURLCache();
    }

    public static void ClearLoginData()
    {
        DeleteStoredToken();
        DeleteCookies();
        ClearBrowserCache();
    }

    private static void ClearURLCache()
    {
    }

    private static void ClearWebViewCookies()
    {
    }

    public void Close()
    {
        Debug.Log("Close");
        SplashScreen screen = SplashScreen.Get();
        if (screen != null)
        {
            screen.HideWebLoginCanvas();
        }
        CloseWebView();
        FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
    }

    private static void CloseWebView()
    {
    }

    public static void DeleteCookies()
    {
        ClearWebViewCookies();
    }

    private static void DeleteStoredLoginToken()
    {
    }

    public static void DeleteStoredToken()
    {
        DeleteStoredLoginToken();
    }

    public Error GetError()
    {
        return Error.InternalError;
    }

    public static bool GetIsNewCreatedAccount()
    {
        return m_isNewCreatedAccount;
    }

    public string GetLoginCode()
    {
        Debug.Log("GetLoginCode");
        return GetWebViewLoginCode();
    }

    public Status GetStatus()
    {
        int webViewStatus = GetWebViewStatus();
        if ((webViewStatus >= 0) && (webViewStatus < 6))
        {
            return (Status) webViewStatus;
        }
        return Status.Error;
    }

    private static string GetStoredLoginToken()
    {
        return string.Empty;
    }

    public static string GetStoredToken()
    {
        return GetStoredLoginToken();
    }

    private static string GetWebViewLoginCode()
    {
        return string.Empty;
    }

    private static int GetWebViewStatus()
    {
        return 0;
    }

    private static void GoBack()
    {
    }

    public static void GoBackWebPage()
    {
        GoBack();
    }

    public void Hide()
    {
        if (this.m_show)
        {
            this.m_show = false;
            ShowWebView(false);
        }
    }

    public bool IsShown()
    {
        return this.m_show;
    }

    public void Load()
    {
        Debug.Log("Load");
        LoadWebView(this.m_url, this.m_window.x, this.m_window.y, this.m_window.width, this.m_window.height, SystemInfo.deviceUniqueIdentifier, this.m_callbackGameObject);
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
    }

    private static void LoadWebView(string str, float x, float y, float width, float height, string deviceUniqueIdentifier, string gameObjectName)
    {
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        this.Close();
    }

    private static void SetBreakingNews(string localized_title, string body, string gameObjectName)
    {
    }

    public void SetCountryCodeCookie(string countryCode, string domain)
    {
        SetWebViewCountryCodeCookie(countryCode, domain);
    }

    public static void SetIsNewCreatedAccount(bool isNewCreatedAccount)
    {
        m_isNewCreatedAccount = isNewCreatedAccount;
    }

    private static void SetRegionSelectVisualState(bool isVisible)
    {
    }

    private static bool SetStoredLoginToken(string str)
    {
        return true;
    }

    public static bool SetStoredToken(string str)
    {
        return SetStoredLoginToken(str);
    }

    private static void SetWebViewCountryCodeCookie(string countryCode, string domain)
    {
    }

    public void Show()
    {
        if (!this.m_show)
        {
            this.m_show = true;
            ShowWebView(true);
        }
    }

    private static void ShowWebView(bool show)
    {
    }

    public static void UpdateBreakingNews(string title, string body, string gameObjectName)
    {
        SetBreakingNews(title, body, gameObjectName);
    }

    public static void UpdateRegionSelectVisualState(bool isVisible)
    {
        SetRegionSelectVisualState(isVisible);
    }

    public enum Error
    {
        InternalError
    }

    public enum Status
    {
        Closed,
        Loading,
        ReadyToDisplay,
        Processing,
        Success,
        Error,
        MAX
    }
}

