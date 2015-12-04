using System;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenEffectGlow : ScreenEffect
{
    private int m_PreviousLayer;
    private bool m_PreviousRenderGlowOnly;
    public bool m_RenderGlowOnly;

    private void Awake()
    {
        this.m_PreviousLayer = base.gameObject.layer;
    }

    private void SetLayer()
    {
        if (this.m_PreviousRenderGlowOnly != this.m_RenderGlowOnly)
        {
            this.m_PreviousRenderGlowOnly = this.m_RenderGlowOnly;
            if (this.m_RenderGlowOnly)
            {
                this.m_PreviousLayer = base.gameObject.layer;
                SceneUtils.SetLayer(base.gameObject, GameLayer.ScreenEffects);
            }
            else
            {
                SceneUtils.SetLayer(base.gameObject, this.m_PreviousLayer);
            }
        }
    }

    private void Start()
    {
        this.SetLayer();
    }

    private void Update()
    {
    }
}

