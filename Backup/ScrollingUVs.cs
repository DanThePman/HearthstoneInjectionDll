using System;
using UnityEngine;

public class ScrollingUVs : MonoBehaviour
{
    private Material m_material;
    private Vector2 m_offset = Vector2.zero;
    public int materialIndex;
    public Vector2 uvAnimationRate = new Vector2(1f, 1f);

    private void LateUpdate()
    {
        if (base.GetComponent<Renderer>().enabled)
        {
            if (this.m_material == null)
            {
                this.m_material = base.GetComponent<Renderer>().materials[this.materialIndex];
            }
            this.m_offset += (Vector2) (this.uvAnimationRate * UnityEngine.Time.deltaTime);
            this.m_material.SetTextureOffset("_MainTex", this.m_offset);
        }
    }

    private void Start()
    {
        this.m_material = base.GetComponent<Renderer>().materials[this.materialIndex];
    }
}

