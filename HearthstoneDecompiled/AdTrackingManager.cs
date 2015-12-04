using System;
using UnityEngine;

public class AdTrackingManager : MonoBehaviour
{
    private static AdTrackingManager s_Instance;

    private void Awake()
    {
        s_Instance = this;
    }

    public static AdTrackingManager Get()
    {
        return s_Instance;
    }

    public void GetDeepLink()
    {
    }

    private void OnDestroy()
    {
        s_Instance = null;
    }

    public void TrackAccountCreated()
    {
    }

    public void TrackAdventureProgress(string description)
    {
    }

    public void TrackCreditsLaunch()
    {
    }

    public void TrackFirstLogin()
    {
    }

    public void TrackLogin()
    {
    }

    public void TrackSale(string price, string currencyCode, string productId, string transactionId)
    {
    }

    public void TrackTutorialProgress(string description)
    {
    }
}

