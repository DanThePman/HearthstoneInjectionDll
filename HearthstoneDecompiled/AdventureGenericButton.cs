using System;
using UnityEngine;

[CustomEditClass]
public class AdventureGenericButton : PegUIElement
{
    [CustomEditField(Sections="Border Settings")]
    public MaterialProperties m_BorderMaterialProperties = new MaterialProperties();
    [CustomEditField(Sections="Border Settings")]
    public MeshRenderer m_BorderRenderer;
    [CustomEditField(Sections="Text Settings")]
    public UberText m_ButtonTextObject;
    public Color m_DisabledTextColor = new Color();
    [CustomEditField(Sections="Portrait Settings")]
    public MaterialProperties m_MaterialProperties = new MaterialProperties();
    [CustomEditField(Sections="Text Settings")]
    public Color m_NormalTextColor = new Color();
    private bool m_PortraitLoaded = true;
    [CustomEditField(Sections="Portrait Settings")]
    public MeshRenderer m_PortraitRenderer;
    private const int s_DefaultPortraitMaterialIndex = 1;
    private const string s_DefaultPortraitMaterialTextureName = "_MainTex";

    private void ApplyPortraitTexture(string name, UnityEngine.Object obj, object userdata)
    {
        this.m_PortraitLoaded = true;
        MaterialProperties properties = userdata as MaterialProperties;
        Texture texture = obj as Texture;
        if (texture == null)
        {
            Debug.LogError(string.Format("Unable to load portrait texture {0}.", name), obj);
        }
        else
        {
            this.m_PortraitRenderer.materials[properties.m_MaterialIndex].SetTexture(properties.m_MaterialPropertyName, texture);
        }
    }

    private bool CheckValidMaterialProperties(MaterialProperties matprop)
    {
        if (this.m_PortraitRenderer == null)
        {
            Debug.LogError("No portrait mesh renderer set.");
            return false;
        }
        if (matprop.m_MaterialIndex >= this.m_PortraitRenderer.materials.Length)
        {
            Debug.LogError(string.Format("Unable to find material index {0}", matprop.m_MaterialIndex));
            return false;
        }
        return true;
    }

    public bool IsPortraitLoaded()
    {
        return this.m_PortraitLoaded;
    }

    public void SetButtonText(string str)
    {
        if (this.m_ButtonTextObject != null)
        {
            this.m_ButtonTextObject.Text = str;
        }
    }

    public void SetDesaturate(bool desaturate)
    {
        if (this.CheckValidMaterialProperties(this.m_MaterialProperties) && this.CheckValidMaterialProperties(this.m_BorderMaterialProperties))
        {
            this.m_PortraitRenderer.materials[this.m_MaterialProperties.m_MaterialIndex].SetFloat("_Desaturate", !desaturate ? 0f : 1f);
            this.m_BorderRenderer.materials[this.m_BorderMaterialProperties.m_MaterialIndex].SetFloat("_Desaturate", !desaturate ? 0f : 1f);
            this.m_ButtonTextObject.TextColor = !desaturate ? this.m_NormalTextColor : this.m_DisabledTextColor;
        }
    }

    public void SetPortraitOffset(Vector2 offset)
    {
        this.SetPortraitOffset(offset, this.m_MaterialProperties.m_MaterialIndex, this.m_MaterialProperties.m_MaterialPropertyName);
    }

    public void SetPortraitOffset(Vector2 offset, int index, string mattexprop)
    {
        MaterialProperties matprop = new MaterialProperties {
            m_MaterialIndex = index,
            m_MaterialPropertyName = mattexprop
        };
        if (this.CheckValidMaterialProperties(matprop))
        {
            this.m_PortraitRenderer.materials[matprop.m_MaterialIndex].SetTextureOffset(matprop.m_MaterialPropertyName, offset);
        }
    }

    public void SetPortraitTexture(string texturename)
    {
        this.SetPortraitTexture(texturename, this.m_MaterialProperties.m_MaterialIndex, this.m_MaterialProperties.m_MaterialPropertyName);
    }

    public void SetPortraitTexture(string texturename, int index, string mattexprop)
    {
        if ((texturename != null) && (texturename.Length != 0))
        {
            texturename = FileUtils.GameAssetPathToName(texturename);
            MaterialProperties matprop = new MaterialProperties {
                m_MaterialIndex = index,
                m_MaterialPropertyName = mattexprop
            };
            if (this.CheckValidMaterialProperties(matprop))
            {
                this.m_PortraitLoaded = false;
                AssetLoader.Get().LoadTexture(texturename, new AssetLoader.ObjectCallback(this.ApplyPortraitTexture), matprop, false);
            }
        }
    }

    public void SetPortraitTiling(Vector2 tiling)
    {
        this.SetPortraitTiling(tiling, this.m_MaterialProperties.m_MaterialIndex, this.m_MaterialProperties.m_MaterialPropertyName);
    }

    public void SetPortraitTiling(Vector2 tiling, int index, string mattexprop)
    {
        MaterialProperties matprop = new MaterialProperties {
            m_MaterialIndex = index,
            m_MaterialPropertyName = mattexprop
        };
        if (this.CheckValidMaterialProperties(matprop))
        {
            this.m_PortraitRenderer.materials[matprop.m_MaterialIndex].SetTextureScale(matprop.m_MaterialPropertyName, tiling);
        }
    }

    [Serializable]
    public class MaterialProperties
    {
        public int m_MaterialIndex = 1;
        public string m_MaterialPropertyName = "_MainTex";
    }
}

