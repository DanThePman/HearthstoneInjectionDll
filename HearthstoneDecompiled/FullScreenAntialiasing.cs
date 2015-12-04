using System;
using UnityEngine;

public class FullScreenAntialiasing : MonoBehaviour
{
    private Material m_FXAA_Material;
    public Shader m_FXAA_Shader;

    private void Awake()
    {
        base.gameObject.GetComponent<Camera>().enabled = true;
        if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
        {
            base.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (this.m_FXAA_Material != null)
        {
            UnityEngine.Object.Destroy(this.m_FXAA_Material);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, this.FXAA_Material);
    }

    private void Start()
    {
        if ((this.m_FXAA_Shader == null) || (this.FXAA_Material == null))
        {
            base.enabled = false;
        }
        if (!this.m_FXAA_Shader.isSupported)
        {
            base.enabled = false;
        }
    }

    private void Update()
    {
    }

    protected Material FXAA_Material
    {
        get
        {
            if (this.m_FXAA_Material == null)
            {
                this.m_FXAA_Material = new Material(this.m_FXAA_Shader);
                SceneUtils.SetHideFlags(this.m_FXAA_Material, HideFlags.DontSave);
            }
            return this.m_FXAA_Material;
        }
    }
}

