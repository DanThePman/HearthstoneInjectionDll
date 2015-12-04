using System;
using UnityEngine;

public class DisableMesh_ColorAlpha : MonoBehaviour
{
    private Material m_material;

    private void Start()
    {
        this.m_material = base.GetComponent<Renderer>().material;
        if (this.m_material == null)
        {
            base.enabled = false;
        }
        if (!this.m_material.HasProperty("_Color"))
        {
            base.enabled = false;
        }
    }

    private void Update()
    {
        if (this.m_material.color.a == 0f)
        {
            base.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            base.GetComponent<Renderer>().enabled = true;
        }
    }
}

