using System;
using UnityEngine;

public class CameraFade : MonoBehaviour
{
    private Camera m_Camera;
    private float m_CameraDepth = 14f;
    public Color m_Color = Color.black;
    public float m_Fade = 1f;
    private Material m_Material;
    private GameObject m_PlaneGameObject;
    public bool m_RenderOverAll;
    private Texture2D m_TempTexture;

    private void Awake()
    {
        this.m_TempTexture = new Texture2D(1, 1);
        this.m_TempTexture.SetPixel(0, 0, Color.white);
        this.m_TempTexture.Apply();
        this.m_Camera = base.GetComponent<Camera>();
        if (this.m_Camera == null)
        {
            Debug.LogError("CameraFade faild to find camera component!");
            base.enabled = false;
        }
        this.m_CameraDepth = this.m_Camera.depth;
        this.SetupCamera();
    }

    private void CreateRenderPlane()
    {
        base.gameObject.AddComponent<MeshFilter>();
        base.gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { new Vector3(-10f, -10f, 0f), new Vector3(10f, -10f, 0f), new Vector3(-10f, 10f, 0f), new Vector3(10f, 10f, 0f) };
        mesh.colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
        mesh.uv = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
        mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.triangles = new int[] { 3, 1, 2, 2, 1, 0 };
        base.GetComponent<Renderer>().GetComponent<MeshFilter>().mesh = mesh;
        this.m_Material = new Material(ShaderUtils.FindShader("Hidden/CameraFade"));
        base.GetComponent<Renderer>().sharedMaterial = this.m_Material;
    }

    private void SetupCamera()
    {
        this.m_Camera.farClipPlane = 1f;
        this.m_Camera.nearClipPlane = -1f;
        this.m_Camera.clearFlags = CameraClearFlags.Nothing;
        this.m_Camera.orthographicSize = 0.5f;
    }

    private void Update()
    {
        if (this.m_Fade <= 0f)
        {
            if ((base.GetComponent<Renderer>() != null) && base.GetComponent<Renderer>().enabled)
            {
                base.GetComponent<Renderer>().enabled = false;
            }
            if (this.m_Camera.enabled)
            {
                this.m_Camera.enabled = false;
            }
        }
        else
        {
            if (base.GetComponent<Renderer>() == null)
            {
                this.CreateRenderPlane();
            }
            if (!base.GetComponent<Renderer>().enabled)
            {
                base.GetComponent<Renderer>().enabled = true;
            }
            if (!this.m_Camera.enabled)
            {
                this.m_Camera.enabled = true;
            }
            if (this.m_RenderOverAll)
            {
                if (this.m_Camera.depth < 100f)
                {
                    this.m_Camera.depth = 100f;
                }
            }
            else if (this.m_Camera.depth != this.m_CameraDepth)
            {
                this.m_Camera.depth = this.m_CameraDepth;
            }
            Color color = new Color(this.m_Color.r, this.m_Color.g, this.m_Color.b, this.m_Fade);
            this.m_Material.color = color;
        }
    }
}

