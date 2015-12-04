using System;
using UnityEngine;

public class LocalizedTexture : MonoBehaviour
{
    public string m_textureName;

    private void Awake()
    {
        AssetLoader.Get().LoadTexture(this.m_textureName, new AssetLoader.ObjectCallback(this.OnTextureLoaded), null, false);
    }

    private void OnTextureLoaded(string name, UnityEngine.Object obj, object callbackData)
    {
        Texture texture = obj as Texture;
        if (texture == null)
        {
            Debug.LogError("Failed to load LocalizedTexture m_textureName!");
        }
        else
        {
            base.GetComponent<Renderer>().material.mainTexture = texture;
        }
    }
}

