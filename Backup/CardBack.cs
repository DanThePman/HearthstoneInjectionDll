using System;
using UnityEngine;

public class CardBack : MonoBehaviour
{
    public Material m_CardBackMaterial;
    public Material m_CardBackMaterial1;
    public Mesh m_CardBackMesh;
    public Texture2D m_CardBackTexture;
    public GameObject m_DragEffect;
    public float m_EffectMaxVelocity = 40f;
    public float m_EffectMinVelocity = 2f;
    public Texture2D m_HiddenCardEchoTexture;
}

