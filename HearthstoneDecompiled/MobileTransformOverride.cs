using System;
using UnityEngine;

public class MobileTransformOverride : MonoBehaviour
{
    public bool m_activate;
    public bool m_deactivate;
    public bool m_deactivateIfNotMobile;
    public Vector3 m_localPosition;
    public Vector3 m_localScale;
    public bool m_overrideLocalPosition;
    public bool m_overrideLocalScale;

    public void Awake()
    {
        this.UpdateObject();
    }

    private void UpdateObject()
    {
        if (this.m_deactivateIfNotMobile)
        {
            base.gameObject.SetActive(true);
        }
    }
}

