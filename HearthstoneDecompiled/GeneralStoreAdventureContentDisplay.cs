using System;
using UnityEngine;

public class GeneralStoreAdventureContentDisplay : MonoBehaviour
{
    public MeshRenderer m_keyArt;
    public MeshRenderer m_logo;
    public GameObject m_preorderFrame;
    public PegUIElement m_rewardChest;
    public GameObject m_rewardsFrame;

    public void SetPreOrder(bool preorder)
    {
        if (this.m_rewardChest != null)
        {
            this.m_rewardChest.gameObject.SetActive(!preorder);
        }
        if (this.m_rewardsFrame != null)
        {
            this.m_rewardsFrame.SetActive(!preorder);
        }
        if (this.m_preorderFrame != null)
        {
            this.m_preorderFrame.SetActive(preorder);
        }
    }

    public void UpdateAdventureType(StoreAdventureDef advDef)
    {
        if (advDef != null)
        {
            AssetLoader.Get().LoadTexture(FileUtils.GameAssetPathToName(advDef.m_logoTextureName), delegate (string name, UnityEngine.Object obj, object data) {
                Texture texture = obj as Texture;
                if (texture == null)
                {
                    Debug.LogError(string.Format("Failed to load texture {0}!", name));
                }
                else
                {
                    this.m_logo.material.mainTexture = texture;
                }
            }, null, false);
            this.m_keyArt.material = advDef.m_keyArt;
        }
    }
}

