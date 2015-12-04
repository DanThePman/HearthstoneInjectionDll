using System;
using UnityEngine;

public class Disable_LowQuality : MonoBehaviour
{
    private void Awake()
    {
        GraphicsManager.Get().RegisterLowQualityDisableObject(base.gameObject);
        if (GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.Low)
        {
            base.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        GraphicsManager manager = GraphicsManager.Get();
        if (manager != null)
        {
            manager.DeregisterLowQualityDisableObject(base.gameObject);
        }
    }
}

