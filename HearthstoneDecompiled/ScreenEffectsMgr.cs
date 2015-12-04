using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenEffectsMgr : MonoBehaviour
{
    private static List<ScreenEffect> m_ActiveScreenEffects;
    private Camera m_EffectsObjectsCamera;
    private GameObject m_EffectsObjectsCameraGO;
    private Camera m_MainCamera;
    private ScreenEffectsRender m_ScreenEffectsRender;
    private static ScreenEffectsMgr s_Instance;

    private void Awake()
    {
        s_Instance = this;
        if (m_ActiveScreenEffects == null)
        {
            m_ActiveScreenEffects = new List<ScreenEffect>();
        }
        if (!SystemInfo.supportsRenderTextures && (SystemInfo.graphicsDeviceName != "Null Device"))
        {
            base.enabled = false;
        }
    }

    private void CreateBackPlane(Camera camera)
    {
        Vector3 vector = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.farClipPlane));
        Vector3 vector2 = camera.ViewportToWorldPoint(new Vector3(1f, 1f, camera.farClipPlane));
        Vector3 vector3 = new Vector3((vector2.x - vector.x) * 0.5f, (vector2.y - vector.y) * 0.5f, (vector2.z - vector.z) * 0.5f);
        float farClipPlane = camera.farClipPlane;
        camera.gameObject.AddComponent<MeshFilter>();
        camera.gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { new Vector3(-vector3.x, -vector3.z, farClipPlane), new Vector3(vector3.x, -vector3.z, farClipPlane), new Vector3(-vector3.x, vector3.z, farClipPlane), new Vector3(vector3.x, vector3.z, farClipPlane) };
        mesh.colors = new Color[] { Color.black, Color.black, Color.black, Color.black };
        mesh.uv = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
        mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.triangles = new int[] { 3, 1, 2, 2, 1, 0 };
        camera.gameObject.GetComponent<Renderer>().GetComponent<MeshFilter>().mesh = mesh;
        Material material = new Material(Shader.Find("Hidden/ScreenEffectsBackPlane"));
        camera.gameObject.GetComponent<Renderer>().sharedMaterial = material;
    }

    private void CreateCamera(out Camera camera, out GameObject cameraGO, string cameraName)
    {
        cameraGO = new GameObject(cameraName);
        SceneUtils.SetLayer(cameraGO, GameLayer.CameraMask);
        this.UpdateCameraTransform();
        camera = cameraGO.AddComponent<Camera>();
        camera.CopyFrom(this.m_MainCamera);
        camera.clearFlags = CameraClearFlags.Nothing;
        if (UniversalInputManager.Get() != null)
        {
            UniversalInputManager.Get().AddIgnoredCamera(camera);
        }
    }

    public static ScreenEffectsMgr Get()
    {
        return s_Instance;
    }

    public int GetActiveScreenEffectsCount()
    {
        if (m_ActiveScreenEffects == null)
        {
            return 0;
        }
        return m_ActiveScreenEffects.Count;
    }

    private void Init()
    {
        this.m_MainCamera = Camera.main;
        if (this.m_MainCamera != null)
        {
            this.m_ScreenEffectsRender = this.m_MainCamera.GetComponent<ScreenEffectsRender>();
            if (this.m_ScreenEffectsRender == null)
            {
                this.m_ScreenEffectsRender = this.m_MainCamera.gameObject.AddComponent<ScreenEffectsRender>();
                this.m_MainCamera.hdr = false;
            }
            else
            {
                this.m_ScreenEffectsRender.enabled = true;
            }
            this.CreateCamera(out this.m_EffectsObjectsCamera, out this.m_EffectsObjectsCameraGO, "ScreenEffectsObjectRenderCamera");
            this.m_EffectsObjectsCamera.depth = this.m_MainCamera.depth - 1f;
            this.m_EffectsObjectsCamera.clearFlags = CameraClearFlags.Color;
            this.m_EffectsObjectsCamera.backgroundColor = Color.clear;
            this.m_EffectsObjectsCameraGO.hideFlags = HideFlags.HideAndDontSave;
            SceneUtils.SetLayer(this.m_EffectsObjectsCameraGO, 0x17);
            this.m_EffectsObjectsCamera.enabled = false;
            this.m_ScreenEffectsRender.m_EffectsObjectsCamera = this.m_EffectsObjectsCamera;
            this.m_EffectsObjectsCamera.cullingMask = 0x800101;
        }
    }

    private void OnDestroy()
    {
        s_Instance = null;
        if (m_ActiveScreenEffects != null)
        {
            m_ActiveScreenEffects.Clear();
            m_ActiveScreenEffects = null;
        }
    }

    private void OnDisable()
    {
        if (this.m_EffectsObjectsCameraGO != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_EffectsObjectsCameraGO);
        }
        if (this.m_ScreenEffectsRender != null)
        {
            this.m_ScreenEffectsRender.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (Camera.main != null)
        {
            this.Init();
        }
    }

    public static void RegisterScreenEffect(ScreenEffect effect)
    {
        if (m_ActiveScreenEffects == null)
        {
            m_ActiveScreenEffects = new List<ScreenEffect>();
        }
        if (!m_ActiveScreenEffects.Contains(effect))
        {
            m_ActiveScreenEffects.Add(effect);
        }
    }

    public static void UnRegisterScreenEffect(ScreenEffect effect)
    {
        if (m_ActiveScreenEffects != null)
        {
            m_ActiveScreenEffects.Remove(effect);
        }
    }

    private void Update()
    {
        if (this.m_MainCamera == null)
        {
            if (Camera.main == null)
            {
                return;
            }
            this.Init();
        }
        if (this.m_ScreenEffectsRender != null)
        {
            if ((m_ActiveScreenEffects != null) && (m_ActiveScreenEffects.Count > 0))
            {
                if (!this.m_ScreenEffectsRender.enabled)
                {
                    this.m_ScreenEffectsRender.enabled = true;
                }
            }
            else if (this.m_ScreenEffectsRender.enabled)
            {
                this.m_ScreenEffectsRender.enabled = false;
            }
            this.UpdateCameraTransform();
        }
    }

    private void UpdateCameraTransform()
    {
        if ((this.m_EffectsObjectsCameraGO != null) && (this.m_MainCamera != null))
        {
            Transform transform = this.m_MainCamera.transform;
            this.m_EffectsObjectsCameraGO.transform.position = transform.position;
            this.m_EffectsObjectsCameraGO.transform.rotation = transform.rotation;
        }
    }

    public enum EFFECT_TYPE
    {
        Glow,
        Distortion
    }
}

