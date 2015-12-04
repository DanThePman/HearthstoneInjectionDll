using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuickBlendShape : MonoBehaviour
{
    public BLEND_SHAPE_ANIMATION_TYPE m_AnimationType;
    private float m_animTime;
    public AnimationCurve m_BlendCurve;
    private Material[] m_BlendMaterials;
    private List<Mesh> m_BlendMeshes;
    public float m_BlendValue;
    public bool m_DisableOnMobile;
    public bool m_Loop = true;
    public Mesh[] m_Meshes;
    private MeshFilter m_MeshFilter;
    private Mesh m_OrgMesh;
    private bool m_Play;
    public bool m_PlayOnAwake;

    private void Awake()
    {
        this.m_MeshFilter = base.GetComponent<MeshFilter>();
        this.m_OrgMesh = this.m_MeshFilter.sharedMesh;
        if (this.m_DisableOnMobile && UniversalInputManager.Get().IsTouchMode())
        {
            MeshFilter component = base.GetComponent<MeshFilter>();
            if (((component.sharedMesh == null) && (this.m_Meshes.Length > 0)) && (this.m_Meshes[0] != null))
            {
                component.sharedMesh = this.m_Meshes[0];
            }
            base.enabled = false;
        }
        else
        {
            this.CreateBlendMeshes();
        }
    }

    private void BlendShapeAnimate()
    {
        if (this.m_BlendMaterials == null)
        {
            this.m_BlendMaterials = base.GetComponent<Renderer>().materials;
        }
        if (this.m_MeshFilter == null)
        {
            this.m_MeshFilter = base.GetComponent<MeshFilter>();
        }
        float time = this.m_BlendCurve.keys[this.m_BlendCurve.length - 1].time;
        this.m_animTime += UnityEngine.Time.deltaTime;
        float blendValue = this.m_BlendValue;
        if (blendValue >= 0f)
        {
            if (this.m_AnimationType == BLEND_SHAPE_ANIMATION_TYPE.Curve)
            {
                blendValue = this.m_BlendCurve.Evaluate(this.m_animTime);
            }
            int num3 = Mathf.FloorToInt(blendValue);
            if (num3 > (this.m_BlendMeshes.Count - 1))
            {
                num3 -= this.m_BlendMeshes.Count - 1;
            }
            this.m_MeshFilter.mesh = this.m_BlendMeshes[num3];
            foreach (Material material in this.m_BlendMaterials)
            {
                material.SetFloat("_Blend", blendValue - Mathf.FloorToInt(blendValue));
            }
            if (this.m_animTime > time)
            {
                if (this.m_Loop)
                {
                    this.m_animTime = 0f;
                }
                else
                {
                    this.m_Play = false;
                }
            }
        }
    }

    private void CreateBlendMeshes()
    {
        this.m_BlendMeshes = new List<Mesh>();
        for (int i = 0; i < this.m_Meshes.Length; i++)
        {
            if (base.GetComponent<MeshFilter>() == null)
            {
                MeshFilter filter = base.gameObject.AddComponent<MeshFilter>();
            }
            Mesh item = UnityEngine.Object.Instantiate<Mesh>(this.m_Meshes[i]);
            int length = this.m_Meshes[i].vertices.Length;
            int index = i + 1;
            if (index > (this.m_Meshes.Length - 1))
            {
                index = 0;
            }
            Mesh mesh2 = this.m_Meshes[index];
            Vector4[] vectorArray = new Vector4[length];
            for (int j = 0; j < length; j++)
            {
                if (j <= (mesh2.vertices.Length - 1))
                {
                    Vector3 vector = mesh2.vertices[j];
                    vectorArray[j] = new Vector4(vector.x, vector.y, vector.z, 1f);
                }
            }
            item.vertices = this.m_Meshes[i].vertices;
            item.tangents = vectorArray;
            this.m_BlendMeshes.Add(item);
        }
    }

    private void OnDisable()
    {
        if (this.m_DisableOnMobile && UniversalInputManager.Get().IsTouchMode())
        {
            this.m_MeshFilter.sharedMesh = this.m_OrgMesh;
        }
        else
        {
            this.m_animTime = 0f;
            if (this.m_BlendMaterials != null)
            {
                foreach (Material material in this.m_BlendMaterials)
                {
                    material.SetFloat("_Blend", 0f);
                }
            }
            if ((this.m_MeshFilter != null) && (this.m_OrgMesh != null))
            {
                this.m_MeshFilter.sharedMesh = this.m_OrgMesh;
            }
        }
    }

    private void OnEnable()
    {
        if ((!this.m_DisableOnMobile || !UniversalInputManager.Get().IsTouchMode()) && this.m_PlayOnAwake)
        {
            this.PlayAnimation();
        }
    }

    public void PlayAnimation()
    {
        if ((((!this.m_DisableOnMobile || !UniversalInputManager.Get().IsTouchMode()) && (this.m_MeshFilter != null)) && (this.m_Meshes != null)) && (this.m_BlendMeshes != null))
        {
            this.m_animTime = 0f;
            this.m_Play = true;
        }
    }

    public void StopAnimation()
    {
        if (!this.m_DisableOnMobile || !UniversalInputManager.Get().IsTouchMode())
        {
            this.m_Play = false;
        }
    }

    private void Update()
    {
        if ((!this.m_DisableOnMobile || !UniversalInputManager.Get().IsTouchMode()) && (this.m_Play || (this.m_AnimationType == BLEND_SHAPE_ANIMATION_TYPE.Float)))
        {
            this.BlendShapeAnimate();
        }
    }

    public enum BLEND_SHAPE_ANIMATION_TYPE
    {
        Curve,
        Float
    }
}

