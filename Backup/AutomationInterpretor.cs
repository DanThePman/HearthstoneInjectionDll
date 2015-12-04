using System;
using UnityEngine;

public class AutomationInterpretor
{
    private const string CUSTOM_ARG_PREFIX = "+";
    private static bool m_DebugLog;
    public KeyCode m_ExportMouseKey = KeyCode.F9;
    public bool m_initLoadComplete;
    public bool m_isClosed;
    public bool m_isClosing;
    private static bool m_isDebugBuild;
    public bool m_wasPaused;
    public const float s_connectionTimeoutLength = 60f;
    private static AutomationInterpretor s_instance;
    public const string s_newPacketTag = "<!--NEWPACKET-->";

    public static AutomationInterpretor Get()
    {
        if (s_instance == null)
        {
            s_instance = new AutomationInterpretor();
        }
        return s_instance;
    }

    public void Start()
    {
    }

    public void Update()
    {
    }
}

