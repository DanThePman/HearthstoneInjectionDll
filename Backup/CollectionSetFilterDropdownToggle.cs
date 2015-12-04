using System;
using UnityEngine;

public class CollectionSetFilterDropdownToggle : PegUIElement
{
    public MeshRenderer m_buttonMesh;
    public MeshRenderer m_currentIconQuad;

    public void SetEnabledVisual(bool enabled)
    {
        if (this.m_buttonMesh != null)
        {
            this.m_buttonMesh.material.SetFloat("_Desaturate", !enabled ? 1f : 0f);
        }
    }

    public void SetToggleIconOffset(Vector2? materialOffset)
    {
        if (materialOffset.HasValue)
        {
            this.m_currentIconQuad.material.SetTextureOffset("_MainTex", materialOffset.Value);
        }
    }
}

