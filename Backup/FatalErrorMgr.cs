using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class FatalErrorMgr
{
    private List<ErrorListener> m_errorListeners = new List<ErrorListener>();
    private string m_generatedErrorCode;
    private List<FatalErrorMessage> m_messages = new List<FatalErrorMessage>();
    private string m_text;
    private static FatalErrorMgr s_instance;

    public void Add(FatalErrorMessage message)
    {
        this.m_messages.Add(message);
        this.FireErrorListeners(message);
    }

    public bool AddErrorListener(ErrorCallback callback)
    {
        return this.AddErrorListener(callback, null);
    }

    public bool AddErrorListener(ErrorCallback callback, object userData)
    {
        ErrorListener item = new ErrorListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_errorListeners.Contains(item))
        {
            return false;
        }
        this.m_errorListeners.Add(item);
        return true;
    }

    public bool AddUnique(FatalErrorMessage message)
    {
        if (!string.IsNullOrEmpty(message.m_id))
        {
            foreach (FatalErrorMessage message2 in this.m_messages)
            {
                if (message2.m_id == message.m_id)
                {
                    return false;
                }
            }
        }
        this.Add(message);
        return true;
    }

    public void ClearAllErrors()
    {
        this.m_messages.Clear();
        this.m_generatedErrorCode = null;
    }

    protected void FireErrorListeners(FatalErrorMessage message)
    {
        ErrorListener[] listenerArray = this.m_errorListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire(message);
        }
    }

    public static FatalErrorMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new FatalErrorMgr();
        }
        return s_instance;
    }

    public string GetFormattedErrorCode()
    {
        return this.m_generatedErrorCode;
    }

    public List<FatalErrorMessage> GetMessages()
    {
        return this.m_messages;
    }

    public bool HasError()
    {
        return (this.m_messages.Count > 0);
    }

    public void NotifyExitPressed()
    {
        Log.Mike.Print("FatalErrorDialog.NotifyExitPressed() - BEGIN", new object[0]);
        this.SendAcknowledgements();
        Log.Mike.Print("FatalErrorDialog.NotifyExitPressed() - calling ApplicationMgr.Get().Exit()", new object[0]);
        ApplicationMgr.Get().Exit();
        Log.Mike.Print("FatalErrorDialog.NotifyExitPressed() - END", new object[0]);
    }

    public bool RemoveErrorListener(ErrorCallback callback)
    {
        return this.RemoveErrorListener(callback, null);
    }

    public bool RemoveErrorListener(ErrorCallback callback, object userData)
    {
        ErrorListener item = new ErrorListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_errorListeners.Remove(item);
    }

    private void SendAcknowledgements()
    {
        foreach (FatalErrorMessage message in this.m_messages.ToArray())
        {
            if (message.m_ackCallback != null)
            {
                message.m_ackCallback(message.m_ackUserData);
            }
        }
    }

    public void SetErrorCode(string prefixSource, string errorSubset1, string errorSubset2 = null, string errorSubset3 = null)
    {
        this.m_generatedErrorCode = prefixSource + ":" + errorSubset1;
        if (errorSubset2 != null)
        {
            this.m_generatedErrorCode = this.m_generatedErrorCode + ":" + errorSubset2;
        }
        if (errorSubset3 != null)
        {
            this.m_generatedErrorCode = this.m_generatedErrorCode + ":" + errorSubset3;
        }
        Log.Yim.Print("m_generatedErrorCode = " + this.m_generatedErrorCode, new object[0]);
    }

    public delegate void ErrorCallback(FatalErrorMessage message, object userData);

    protected class ErrorListener : EventListener<FatalErrorMgr.ErrorCallback>
    {
        public void Fire(FatalErrorMessage message)
        {
            if (GeneralUtils.IsCallbackValid(base.m_callback))
            {
                base.m_callback(message, base.m_userData);
            }
        }
    }
}

