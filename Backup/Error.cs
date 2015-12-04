using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Error
{
    public static readonly PlatformDependentValue<bool> HAS_APP_STORE;

    static Error()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            PC = false,
            Mac = false,
            iOS = true,
            Android = true
        };
        HAS_APP_STORE = value2;
    }

    public static void AddDevFatal(string message, params object[] messageArgs)
    {
        string str = string.Format(message, messageArgs);
        if (!ApplicationMgr.IsInternal())
        {
            Debug.LogError(string.Format("Error.AddDevFatal() - message={0}", str));
        }
        else
        {
            ErrorParams parms = new ErrorParams {
                m_message = str
            };
            AddFatal(parms);
        }
    }

    public static void AddDevWarning(string header, string message, params object[] messageArgs)
    {
        string str = string.Format(message, messageArgs);
        if (!ApplicationMgr.IsInternal())
        {
            Debug.LogWarning(string.Format("Error.AddDevWarning() - header={0} message={1}", header, str));
        }
        else
        {
            ErrorParams parms = new ErrorParams {
                m_header = header,
                m_message = str
            };
            AddWarning(parms);
        }
    }

    public static void AddFatal(ErrorParams parms)
    {
        Debug.LogError(string.Format("Error.AddFatal() - message={0}", parms.m_message));
        if (UniversalInputManager.Get() != null)
        {
            UniversalInputManager.Get().CancelTextInput(null, true);
        }
        if (ShouldUseWarningDialogForFatalError())
        {
            if (string.IsNullOrEmpty(parms.m_header))
            {
                parms.m_header = "Fatal Error as Warning";
            }
            ShowWarningDialog(parms);
        }
        else
        {
            parms.m_type = ErrorType.FATAL;
            FatalErrorMessage message = new FatalErrorMessage();
            if (parms.m_header == null)
            {
            }
            message.m_id = string.Empty + parms.m_message;
            message.m_text = parms.m_message;
            message.m_ackCallback = parms.m_ackCallback;
            message.m_ackUserData = parms.m_ackUserData;
            message.m_allowClick = parms.m_allowClick;
            message.m_redirectToStore = parms.m_redirectToStore;
            message.m_delayBeforeNextReset = parms.m_delayBeforeNextReset;
            FatalErrorMgr.Get().Add(message);
        }
    }

    public static void AddFatal(string message)
    {
        ErrorParams parms = new ErrorParams {
            m_message = message
        };
        AddFatal(parms);
    }

    public static void AddFatalLoc(string messageKey, params object[] messageArgs)
    {
        ErrorParams parms = new ErrorParams {
            m_message = GameStrings.Format(messageKey, messageArgs)
        };
        AddFatal(parms);
    }

    public static void AddWarning(ErrorParams parms)
    {
        if (DialogManager.Get() == null)
        {
            AddFatal(parms);
        }
        else
        {
            Debug.LogWarning(string.Format("Error.AddWarning() - header={0} message={1}", parms.m_header, parms.m_message));
            if (UniversalInputManager.Get() != null)
            {
                UniversalInputManager.Get().CancelTextInput(null, true);
            }
            ShowWarningDialog(parms);
        }
    }

    public static void AddWarning(string header, string message, params object[] messageArgs)
    {
        ErrorParams parms = new ErrorParams {
            m_header = header,
            m_message = string.Format(message, messageArgs)
        };
        AddWarning(parms);
    }

    public static void AddWarningLoc(string headerKey, string messageKey, params object[] messageArgs)
    {
        ErrorParams parms = new ErrorParams {
            m_header = GameStrings.Get(headerKey),
            m_message = GameStrings.Format(messageKey, messageArgs)
        };
        AddWarning(parms);
    }

    private static void OnWarningPopupResponse(AlertPopup.Response response, object userData)
    {
        ErrorParams @params = (ErrorParams) userData;
        if (@params.m_ackCallback != null)
        {
            @params.m_ackCallback(@params.m_ackUserData);
        }
    }

    private static bool ShouldUseWarningDialogForFatalError()
    {
        if (ApplicationMgr.IsPublic())
        {
            return false;
        }
        if (DialogManager.Get() == null)
        {
            return false;
        }
        return !Options.Get().GetBool(Option.ERROR_SCREEN);
    }

    private static void ShowWarningDialog(ErrorParams parms)
    {
        parms.m_type = ErrorType.WARNING;
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_id = parms.m_header + parms.m_message,
            m_headerText = parms.m_header,
            m_text = parms.m_message,
            m_responseCallback = new AlertPopup.ResponseCallback(Error.OnWarningPopupResponse),
            m_responseUserData = parms,
            m_showAlertIcon = true
        };
        DialogManager.Get().ShowPopup(info);
    }

    public delegate void AcknowledgeCallback(object userData);
}

