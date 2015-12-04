using System;
using UnityEngine;

public class ConnectionIndicator : MonoBehaviour
{
    private const float LATENCY_TOLERANCE = 3f;
    private bool m_active;
    public GameObject m_indicator;
    private static ConnectionIndicator s_instance;

    private void Awake()
    {
        s_instance = this;
        this.m_active = false;
        this.m_indicator.SetActive(false);
    }

    public static ConnectionIndicator Get()
    {
        return s_instance;
    }

    public bool IsVisible()
    {
        return this.m_active;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void SetIndicator(bool val)
    {
        if (val != this.m_active)
        {
            this.m_active = val;
            this.m_indicator.SetActive(val);
            BnetBar.Get().UpdateLayout();
        }
    }

    private void Update()
    {
        this.SetIndicator(ConnectAPI.TimeSinceLastPong() > 3f);
    }
}

