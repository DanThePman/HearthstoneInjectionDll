using System;

public class ErrorParams
{
    public Error.AcknowledgeCallback m_ackCallback;
    public object m_ackUserData;
    public bool m_allowClick = true;
    public float m_delayBeforeNextReset;
    public string m_header;
    public string m_message;
    public bool m_redirectToStore;
    public ErrorType m_type;
}

