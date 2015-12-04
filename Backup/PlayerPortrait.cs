using System;
using UnityEngine;

public class PlayerPortrait : MonoBehaviour
{
    private string m_currentTextureName;
    private string m_loadingTextureName;
    private BnetProgramId m_programId;

    public BnetProgramId GetProgramId()
    {
        return this.m_programId;
    }

    public bool IsIconLoading()
    {
        return (this.m_loadingTextureName != null);
    }

    public bool IsIconReady()
    {
        return ((this.m_loadingTextureName == null) && (this.m_currentTextureName != null));
    }

    private void OnTextureLoaded(string name, UnityEngine.Object obj, object callbackData)
    {
        if (name == this.m_loadingTextureName)
        {
            Texture texture = obj as Texture;
            if (texture == null)
            {
                object[] messageArgs = new object[] { name, this.m_programId };
                Error.AddDevFatal("PlayerPortrait.OnTextureLoaded() - Failed to load {0}. ProgramId={1}", messageArgs);
                this.m_currentTextureName = null;
                this.m_loadingTextureName = null;
            }
            else
            {
                this.m_currentTextureName = this.m_loadingTextureName;
                this.m_loadingTextureName = null;
                base.GetComponent<Renderer>().material.mainTexture = texture;
            }
        }
    }

    public bool SetProgramId(BnetProgramId programId)
    {
        if (this.m_programId == programId)
        {
            return false;
        }
        this.m_programId = programId;
        this.UpdateIcon();
        return true;
    }

    private void UpdateIcon()
    {
        if (this.m_programId == null)
        {
            this.m_currentTextureName = null;
            this.m_loadingTextureName = null;
            base.GetComponent<Renderer>().material.mainTexture = null;
        }
        else
        {
            string textureName = BnetProgramId.GetTextureName(this.m_programId);
            if ((this.m_currentTextureName != textureName) && (this.m_loadingTextureName != textureName))
            {
                this.m_loadingTextureName = textureName;
                AssetLoader.Get().LoadTexture(this.m_loadingTextureName, new AssetLoader.ObjectCallback(this.OnTextureLoaded), null, false);
            }
        }
    }
}

