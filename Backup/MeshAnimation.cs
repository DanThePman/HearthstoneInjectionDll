using System;
using UnityEngine;

public class MeshAnimation : MonoBehaviour
{
    public float FrameDuration;
    public bool Loop;
    private float m_FrameTime;
    private int m_Index;
    private MeshFilter m_Mesh;
    private bool m_Playing;
    public Mesh[] Meshes;

    public void Play()
    {
        base.enabled = true;
        this.m_Playing = true;
    }

    public void Reset()
    {
        this.m_Mesh.mesh = this.Meshes[0];
        this.m_FrameTime = 0f;
        this.m_Index = 0;
    }

    private void Start()
    {
        this.m_Mesh = base.GetComponent<MeshFilter>();
    }

    public void Stop()
    {
        this.m_Playing = false;
        base.enabled = false;
    }

    private void Update()
    {
        if (this.m_Playing)
        {
            this.m_FrameTime += UnityEngine.Time.deltaTime;
            if (this.m_FrameTime >= this.FrameDuration)
            {
                this.m_Index = (this.m_Index + 1) % this.Meshes.Length;
                this.m_FrameTime -= this.FrameDuration;
                if (!this.Loop && (this.m_Index == 0))
                {
                    this.m_Playing = false;
                    base.enabled = false;
                }
                else
                {
                    this.m_Mesh.mesh = this.Meshes[this.m_Index];
                }
            }
        }
    }
}

