using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class VS : MonoBehaviour
{
    public GameObject m_shadow;

    public void ActivateAnimation(bool active = true)
    {
        base.gameObject.GetComponentInChildren<Animation>().enabled = active;
    }

    public void ActivateShadow(bool active = true)
    {
        this.m_shadow.SetActive(active);
    }

    private void OnDestroy()
    {
        this.SetDefaults();
    }

    private void SetDefaults()
    {
        this.ActivateShadow(false);
    }

    private void Start()
    {
        this.SetDefaults();
    }
}

