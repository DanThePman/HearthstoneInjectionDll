using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HighlightRender : MonoBehaviour
{
    private readonly string BLEND_SHADER_NAME = "Custom/Selection/HighlightMaskBlend";
    private const float BLUR_BLEND1 = 1.25f;
    private const float BLUR_BLEND2 = 1.25f;
    private const float BLUR_BLEND3 = 1f;
    private const float BLUR_BLEND4 = 1.5f;
    private readonly string HIGHLIGHT_SHADER_NAME = "Custom/Selection/Highlight";
    private Material m_BlendMaterial;
    private Shader m_BlendShader;
    private Camera m_Camera;
    private float m_CameraOrthoSize;
    private RenderTexture m_CameraTexture;
    private Shader m_HighlightShader;
    private bool m_Initialized;
    private Material m_Material;
    private Material m_MultiSampleBlendMaterial;
    private Shader m_MultiSampleBlendShader;
    private Material m_MultiSampleMaterial;
    private Shader m_MultiSampleShader;
    private Map<Transform, Vector3> m_ObjectsOrginalPosition;
    private Vector3 m_OrgPosition;
    private Quaternion m_OrgRotation;
    private Vector3 m_OrgScale;
    private GameObject m_RenderPlane;
    private float m_RenderScale = 1f;
    private int m_RenderSizeX = 200;
    private int m_RenderSizeY = 200;
    public Transform m_RootTransform;
    public float m_SilouetteClipSize = 1f;
    public float m_SilouetteRenderSize = 1f;
    private Shader m_UnlitBlackShader;
    private Shader m_UnlitColorShader;
    private Shader m_UnlitDarkGreyShader;
    private Shader m_UnlitGreyShader;
    private Shader m_UnlitLightGreyShader;
    private Shader m_UnlitWhiteShader;
    private Map<Renderer, bool> m_VisibilityStates;
    private const int MAX_HIGHLIGHT_EXCLUDE_PARENT_SEARCH = 0x19;
    private readonly string MULTISAMPLE_BLEND_SHADER_NAME = "Custom/Selection/HighlightMultiSampleBlend";
    private readonly string MULTISAMPLE_SHADER_NAME = "Custom/Selection/HighlightMultiSample";
    private const float ORTHO_SIZE1 = 0.2f;
    private const float ORTHO_SIZE2 = 0.25f;
    private const float ORTHO_SIZE3 = 0.01f;
    private const float ORTHO_SIZE4 = -0.05f;
    private const float RENDER_SIZE1 = 0.3f;
    private const float RENDER_SIZE2 = 0.3f;
    private const float RENDER_SIZE3 = 0.5f;
    private const float RENDER_SIZE4 = 0.92f;
    private static float s_offset = -2000f;
    private const int SILHOUETTE_RENDER_DEPTH = 0x18;
    private const int SILHOUETTE_RENDER_SIZE = 200;
    private readonly string UNLIT_BLACK_SHADER_NAME = "Custom/Unlit/Color/BlackOverlay";
    private readonly string UNLIT_COLOR_SHADER_NAME = "Custom/UnlitColor";
    private readonly string UNLIT_DARKGREY_SHADER_NAME = "Custom/Unlit/Color/DarkGrey";
    private readonly string UNLIT_GREY_SHADER_NAME = "Custom/Unlit/Color/Grey";
    private readonly string UNLIT_LIGHTGREY_SHADER_NAME = "Custom/Unlit/Color/LightGrey";
    private readonly string UNLIT_WHITE_SHADER_NAME = "Custom/Unlit/Color/White";

    private void CreateCamera(Transform renderPlane)
    {
        if (this.m_Camera == null)
        {
            GameObject obj2 = new GameObject();
            this.m_Camera = obj2.AddComponent<Camera>();
            obj2.name = renderPlane.name + "_SilhouetteCamera";
            SceneUtils.SetHideFlags(obj2, HideFlags.HideAndDontSave);
            this.m_Camera.orthographic = true;
            this.m_Camera.orthographicSize = this.m_CameraOrthoSize;
            this.m_Camera.transform.position = renderPlane.position;
            this.m_Camera.transform.rotation = renderPlane.rotation;
            this.m_Camera.transform.Rotate((float) 90f, 180f, (float) 0f);
            this.m_Camera.transform.parent = renderPlane;
            this.m_Camera.nearClipPlane = -this.m_RenderScale + 1f;
            this.m_Camera.farClipPlane = this.m_RenderScale + 1f;
            this.m_Camera.depth = Camera.main.depth - 5f;
            this.m_Camera.backgroundColor = Color.black;
            this.m_Camera.clearFlags = CameraClearFlags.Color;
            this.m_Camera.depthTextureMode = DepthTextureMode.None;
            this.m_Camera.renderingPath = RenderingPath.VertexLit;
            this.m_Camera.SetReplacementShader(this.m_UnlitColorShader, string.Empty);
            this.m_Camera.targetTexture = null;
            this.m_Camera.enabled = false;
        }
    }

    public void CreateSilhouetteTexture()
    {
        this.CreateSilhouetteTexture(false);
    }

    public void CreateSilhouetteTexture(bool force)
    {
        this.Initialize();
        if (this.VisibilityStatesChanged() || force)
        {
            this.SetupRenderObjects();
            if ((this.m_RenderPlane != null) && ((this.m_RenderSizeX >= 1) && (this.m_RenderSizeY >= 1)))
            {
                bool enabled = base.GetComponent<Renderer>().enabled;
                base.GetComponent<Renderer>().enabled = false;
                RenderTexture source = RenderTexture.GetTemporary((int) (this.m_RenderSizeX * 0.3f), (int) (this.m_RenderSizeY * 0.3f), 0x18);
                RenderTexture blend = RenderTexture.GetTemporary((int) (this.m_RenderSizeX * 0.3f), (int) (this.m_RenderSizeY * 0.3f), 0x18);
                RenderTexture dest = RenderTexture.GetTemporary((int) (this.m_RenderSizeX * 0.3f), (int) (this.m_RenderSizeY * 0.3f), 0x18);
                RenderTexture texture4 = RenderTexture.GetTemporary((int) (this.m_RenderSizeX * 0.5f), (int) (this.m_RenderSizeY * 0.5f), 0x18);
                RenderTexture texture5 = RenderTexture.GetTemporary((int) (this.m_RenderSizeX * 0.5f), (int) (this.m_RenderSizeY * 0.5f), 0x18);
                RenderTexture texture6 = RenderTexture.GetTemporary(this.m_RenderSizeX, this.m_RenderSizeY, 0x18);
                RenderTexture texture7 = RenderTexture.GetTemporary(this.m_RenderSizeX, this.m_RenderSizeY, 0x18);
                RenderTexture texture8 = RenderTexture.GetTemporary(this.m_RenderSizeX, this.m_RenderSizeY, 0x18);
                RenderTexture texture = RenderTexture.GetTemporary((int) (this.m_RenderSizeX * 0.92f), (int) (this.m_RenderSizeY * 0.92f), 0x18);
                this.m_CameraTexture.DiscardContents();
                this.m_Camera.clearFlags = CameraClearFlags.Color;
                this.m_Camera.depth = Camera.main.depth - 1f;
                this.m_Camera.clearFlags = CameraClearFlags.Color;
                this.m_Camera.orthographicSize = this.m_CameraOrthoSize + (0.1f * this.m_SilouetteClipSize);
                this.m_Camera.targetTexture = texture;
                this.m_Camera.RenderWithShader(this.m_UnlitWhiteShader, "Highlight");
                this.m_Camera.depth = Camera.main.depth - 5f;
                this.m_Camera.orthographicSize = this.m_CameraOrthoSize - (0.2f * this.m_SilouetteRenderSize);
                this.m_Camera.targetTexture = source;
                this.m_Camera.RenderWithShader(this.m_UnlitDarkGreyShader, "Highlight");
                this.m_Camera.depth = Camera.main.depth - 4f;
                this.m_Camera.orthographicSize = this.m_CameraOrthoSize - (0.25f * this.m_SilouetteRenderSize);
                this.m_Camera.targetTexture = blend;
                this.m_Camera.RenderWithShader(this.m_UnlitGreyShader, "Highlight");
                this.SampleBlend(source, blend, dest, 1.25f);
                this.m_Camera.depth = Camera.main.depth - 3f;
                this.m_Camera.orthographicSize = this.m_CameraOrthoSize - (0.01f * this.m_SilouetteRenderSize);
                this.m_Camera.targetTexture = texture4;
                this.m_Camera.RenderWithShader(this.m_UnlitLightGreyShader, "Highlight");
                this.SampleBlend(dest, texture4, texture5, 1.25f);
                this.m_Camera.depth = Camera.main.depth - 2f;
                this.m_Camera.orthographicSize = this.m_CameraOrthoSize - (-0.05f * this.m_SilouetteRenderSize);
                this.m_Camera.targetTexture = texture6;
                this.m_Camera.RenderWithShader(this.m_UnlitWhiteShader, "Highlight");
                this.SampleBlend(texture5, texture6, texture7, 1f);
                this.Sample(texture7, texture8, 1.5f);
                this.BlendMaterial.SetTexture("_BlendTex", texture);
                float y = 0.8f;
                Vector2[] offsets = new Vector2[] { new Vector2(-y, -y), new Vector2(-y, y), new Vector2(y, y), new Vector2(y, -y) };
                Graphics.BlitMultiTap(texture8, this.m_CameraTexture, this.BlendMaterial, offsets);
                RenderTexture.ReleaseTemporary(source);
                RenderTexture.ReleaseTemporary(blend);
                RenderTexture.ReleaseTemporary(dest);
                RenderTexture.ReleaseTemporary(texture4);
                RenderTexture.ReleaseTemporary(texture5);
                RenderTexture.ReleaseTemporary(texture6);
                RenderTexture.ReleaseTemporary(texture7);
                RenderTexture.ReleaseTemporary(texture8);
                RenderTexture.ReleaseTemporary(texture);
                this.m_Camera.orthographicSize = this.m_CameraOrthoSize;
                base.GetComponent<Renderer>().enabled = enabled;
                this.RestoreRenderObjects();
            }
        }
    }

    [ContextMenu("Export Silhouette Texture")]
    public void ExportSilhouetteTexture()
    {
        RenderTexture.active = this.m_CameraTexture;
        Texture2D textured = new Texture2D(this.m_RenderSizeX, this.m_RenderSizeY, TextureFormat.RGB24, false);
        textured.ReadPixels(new Rect(0f, 0f, (float) this.m_RenderSizeX, (float) this.m_RenderSizeY), 0, 0, false);
        textured.Apply();
        string path = Application.dataPath + "/SilhouetteTexture.png";
        System.IO.File.WriteAllBytes(path, textured.EncodeToPNG());
        RenderTexture.active = null;
        Debug.Log(string.Format("Silhouette Texture Created: {0}", path));
    }

    private List<GameObject> GetExcludedObjects()
    {
        List<GameObject> list = new List<GameObject>();
        HighlightExclude[] componentsInChildren = this.m_RootTransform.GetComponentsInChildren<HighlightExclude>();
        if (componentsInChildren == null)
        {
            return null;
        }
        foreach (HighlightExclude exclude in componentsInChildren)
        {
            Transform[] transformArray = exclude.GetComponentsInChildren<Transform>();
            if (transformArray != null)
            {
                foreach (Transform transform in transformArray)
                {
                    list.Add(transform.gameObject);
                }
            }
        }
        list.Add(base.gameObject);
        list.Add(base.transform.parent.gameObject);
        return list;
    }

    public static Vector3 GetWorldScale(Transform transform)
    {
        Vector3 localScale = transform.localScale;
        for (Transform transform2 = transform.parent; transform2 != null; transform2 = transform2.parent)
        {
            localScale = Vector3.Scale(localScale, transform2.localScale);
        }
        return localScale;
    }

    protected void Initialize()
    {
        if (!this.m_Initialized)
        {
            this.m_Initialized = true;
            if (this.m_HighlightShader == null)
            {
                this.m_HighlightShader = ShaderUtils.FindShader(this.HIGHLIGHT_SHADER_NAME);
            }
            if (this.m_HighlightShader == null)
            {
                Debug.LogError("Failed to load Highlight Shader: " + this.HIGHLIGHT_SHADER_NAME);
                base.enabled = false;
            }
            base.GetComponent<Renderer>().material.shader = this.m_HighlightShader;
            if (this.m_MultiSampleShader == null)
            {
                this.m_MultiSampleShader = ShaderUtils.FindShader(this.MULTISAMPLE_SHADER_NAME);
            }
            if (this.m_MultiSampleShader == null)
            {
                Debug.LogError("Failed to load Highlight Shader: " + this.MULTISAMPLE_SHADER_NAME);
                base.enabled = false;
            }
            if (this.m_MultiSampleBlendShader == null)
            {
                this.m_MultiSampleBlendShader = ShaderUtils.FindShader(this.MULTISAMPLE_BLEND_SHADER_NAME);
            }
            if (this.m_MultiSampleBlendShader == null)
            {
                Debug.LogError("Failed to load Highlight Shader: " + this.MULTISAMPLE_BLEND_SHADER_NAME);
                base.enabled = false;
            }
            if (this.m_BlendShader == null)
            {
                this.m_BlendShader = ShaderUtils.FindShader(this.BLEND_SHADER_NAME);
            }
            if (this.m_BlendShader == null)
            {
                Debug.LogError("Failed to load Highlight Shader: " + this.BLEND_SHADER_NAME);
                base.enabled = false;
            }
            if (this.m_RootTransform == null)
            {
                Transform parent = base.transform.parent.parent;
                if (parent.GetComponent<ActorStateMgr>() != null)
                {
                    this.m_RootTransform = parent.parent;
                }
                else
                {
                    this.m_RootTransform = parent;
                }
                if (this.m_RootTransform == null)
                {
                    Debug.LogError("m_RootTransform is null. Highlighting disabled!");
                    base.enabled = false;
                }
            }
            this.m_VisibilityStates = new Map<Renderer, bool>();
            HighlightSilhouetteInclude[] componentsInChildren = this.m_RootTransform.GetComponentsInChildren<HighlightSilhouetteInclude>();
            if (componentsInChildren != null)
            {
                foreach (HighlightSilhouetteInclude include in componentsInChildren)
                {
                    Renderer component = include.gameObject.GetComponent<Renderer>();
                    if (component != null)
                    {
                        this.m_VisibilityStates.Add(component, false);
                    }
                }
            }
            this.m_UnlitColorShader = ShaderUtils.FindShader(this.UNLIT_COLOR_SHADER_NAME);
            if (this.m_UnlitColorShader == null)
            {
                Debug.LogError("Failed to load Highlight Rendering Shader: " + this.UNLIT_COLOR_SHADER_NAME);
            }
            this.m_UnlitGreyShader = ShaderUtils.FindShader(this.UNLIT_GREY_SHADER_NAME);
            if (this.m_UnlitGreyShader == null)
            {
                Debug.LogError("Failed to load Highlight Rendering Shader: " + this.UNLIT_GREY_SHADER_NAME);
            }
            this.m_UnlitLightGreyShader = ShaderUtils.FindShader(this.UNLIT_LIGHTGREY_SHADER_NAME);
            if (this.m_UnlitLightGreyShader == null)
            {
                Debug.LogError("Failed to load Highlight Rendering Shader: " + this.UNLIT_LIGHTGREY_SHADER_NAME);
            }
            this.m_UnlitDarkGreyShader = ShaderUtils.FindShader(this.UNLIT_DARKGREY_SHADER_NAME);
            if (this.m_UnlitDarkGreyShader == null)
            {
                Debug.LogError("Failed to load Highlight Rendering Shader: " + this.UNLIT_DARKGREY_SHADER_NAME);
            }
            this.m_UnlitBlackShader = ShaderUtils.FindShader(this.UNLIT_BLACK_SHADER_NAME);
            if (this.m_UnlitBlackShader == null)
            {
                Debug.LogError("Failed to load Highlight Rendering Shader: " + this.UNLIT_BLACK_SHADER_NAME);
            }
            this.m_UnlitWhiteShader = ShaderUtils.FindShader(this.UNLIT_WHITE_SHADER_NAME);
            if (this.m_UnlitWhiteShader == null)
            {
                Debug.LogError("Failed to load Highlight Rendering Shader: " + this.UNLIT_WHITE_SHADER_NAME);
            }
        }
    }

    private bool isHighlighExclude(Transform objXform)
    {
        if (objXform == null)
        {
            return true;
        }
        HighlightExclude component = objXform.GetComponent<HighlightExclude>();
        if ((component != null) && component.enabled)
        {
            return true;
        }
        Transform parent = objXform.transform.parent;
        if (parent != null)
        {
            for (int i = 0; (parent != this.m_RootTransform) || (parent != null); i++)
            {
                if ((parent == null) || (i > 0x19))
                {
                    break;
                }
                HighlightExclude exclude2 = parent.GetComponent<HighlightExclude>();
                if ((exclude2 != null) && exclude2.ExcludeChildren)
                {
                    return true;
                }
                parent = parent.parent;
            }
        }
        return false;
    }

    public bool isTextureCreated()
    {
        return ((this.m_CameraTexture != null) && this.m_CameraTexture.IsCreated());
    }

    protected void OnApplicationFocus(bool state)
    {
    }

    protected void OnDisable()
    {
        if (this.m_Material != null)
        {
            UnityEngine.Object.Destroy(this.m_Material);
        }
        if (this.m_MultiSampleMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_MultiSampleMaterial);
        }
        if (this.m_BlendMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_BlendMaterial);
        }
        if (this.m_MultiSampleBlendMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_MultiSampleBlendMaterial);
        }
        if (this.m_VisibilityStates != null)
        {
            this.m_VisibilityStates.Clear();
        }
        if (this.m_CameraTexture != null)
        {
            if (RenderTexture.active == this.m_CameraTexture)
            {
                RenderTexture.active = null;
            }
            this.m_CameraTexture.Release();
            this.m_CameraTexture = null;
        }
        this.m_Initialized = false;
    }

    private void RestoreRenderObjects()
    {
        this.m_RootTransform.position = this.m_OrgPosition;
        this.m_RootTransform.rotation = this.m_OrgRotation;
        this.m_RootTransform.localScale = this.m_OrgScale;
        this.m_RenderPlane = null;
    }

    private void Sample(RenderTexture source, RenderTexture dest, float off)
    {
        if ((source != null) && (dest != null))
        {
            dest.DiscardContents();
            Vector2[] offsets = new Vector2[] { new Vector2(-off, -off), new Vector2(-off, off), new Vector2(off, off), new Vector2(off, -off) };
            Graphics.BlitMultiTap(source, dest, this.MultiSampleMaterial, offsets);
        }
    }

    private void SampleBlend(RenderTexture source, RenderTexture blend, RenderTexture dest, float off)
    {
        if (((source != null) && (dest != null)) && (blend != null))
        {
            this.MultiSampleBlendMaterial.SetTexture("_BlendTex", blend);
            dest.DiscardContents();
            Vector2[] offsets = new Vector2[] { new Vector2(-off, -off), new Vector2(-off, off), new Vector2(off, off), new Vector2(off, -off) };
            Graphics.BlitMultiTap(source, dest, this.MultiSampleBlendMaterial, offsets);
        }
    }

    private void SetupRenderObjects()
    {
        if (this.m_RootTransform == null)
        {
            this.m_RenderPlane = null;
        }
        else
        {
            s_offset -= 10f;
            if (s_offset < -9000f)
            {
                s_offset = -2000f;
            }
            this.m_OrgPosition = this.m_RootTransform.position;
            this.m_OrgRotation = this.m_RootTransform.rotation;
            this.m_OrgScale = this.m_RootTransform.localScale;
            Vector3 vector = (Vector3) (Vector3.left * s_offset);
            this.m_RootTransform.position = vector;
            this.SetWorldScale(this.m_RootTransform, Vector3.one);
            this.m_RootTransform.rotation = Quaternion.identity;
            Bounds bounds = base.GetComponent<Renderer>().bounds;
            float x = bounds.size.x;
            float z = bounds.size.z;
            if (z < bounds.size.y)
            {
                z = bounds.size.y;
            }
            if (x > z)
            {
                this.m_RenderSizeX = 200;
                this.m_RenderSizeY = (int) (200f * (z / x));
            }
            else
            {
                this.m_RenderSizeX = (int) (200f * (x / z));
                this.m_RenderSizeY = 200;
            }
            this.m_CameraOrthoSize = z * 0.5f;
            if (this.m_CameraTexture == null)
            {
                if ((this.m_RenderSizeX < 1) || (this.m_RenderSizeY < 1))
                {
                    this.m_RenderSizeX = 200;
                    this.m_RenderSizeY = 200;
                }
                this.m_CameraTexture = new RenderTexture(this.m_RenderSizeX, this.m_RenderSizeY, 0x18);
                this.m_CameraTexture.format = RenderTextureFormat.ARGB32;
            }
            HighlightState componentInChildren = this.m_RootTransform.GetComponentInChildren<HighlightState>();
            if (componentInChildren == null)
            {
                Debug.LogError("Can not find Highlight(HighlightState component) object for selection highlighting.");
                this.m_RenderPlane = null;
            }
            else
            {
                componentInChildren.transform.localPosition = Vector3.zero;
                HighlightRender render = this.m_RootTransform.GetComponentInChildren<HighlightRender>();
                if (render == null)
                {
                    Debug.LogError("Can not find render plane object(HighlightRender component) for selection highlighting.");
                    this.m_RenderPlane = null;
                }
                else
                {
                    this.m_RenderPlane = render.gameObject;
                    this.m_RenderScale = GetWorldScale(this.m_RenderPlane.transform).x;
                    this.CreateCamera(render.transform);
                }
            }
        }
    }

    public void SetWorldScale(Transform xform, Vector3 scale)
    {
        GameObject obj2 = new GameObject();
        Transform transform = obj2.transform;
        transform.parent = null;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        Transform parent = xform.parent;
        xform.parent = transform;
        xform.localScale = scale;
        xform.parent = parent;
        UnityEngine.Object.Destroy(obj2);
    }

    protected void Update()
    {
        if (((this.m_CameraTexture != null) && this.m_Initialized) && !this.m_CameraTexture.IsCreated())
        {
            this.CreateSilhouetteTexture();
        }
    }

    private bool VisibilityStatesChanged()
    {
        bool flag = false;
        HighlightSilhouetteInclude[] componentsInChildren = this.m_RootTransform.GetComponentsInChildren<HighlightSilhouetteInclude>();
        List<Renderer> list = new List<Renderer>();
        foreach (HighlightSilhouetteInclude include in componentsInChildren)
        {
            Renderer component = include.gameObject.GetComponent<Renderer>();
            if (component != null)
            {
                list.Add(component);
            }
        }
        Map<Renderer, bool> map = new Map<Renderer, bool>();
        foreach (Renderer renderer2 in list)
        {
            bool flag2 = renderer2.enabled && renderer2.gameObject.activeInHierarchy;
            if (!this.m_VisibilityStates.ContainsKey(renderer2))
            {
                map.Add(renderer2, flag2);
                if (flag2)
                {
                    flag = true;
                }
            }
            else
            {
                if (this.m_VisibilityStates[renderer2] != flag2)
                {
                    flag = true;
                }
                map.Add(renderer2, flag2);
            }
        }
        return flag;
    }

    protected Material BlendMaterial
    {
        get
        {
            if (this.m_BlendMaterial == null)
            {
                this.m_BlendMaterial = new Material(this.m_BlendShader);
                SceneUtils.SetHideFlags(this.m_BlendMaterial, HideFlags.DontSave);
            }
            return this.m_BlendMaterial;
        }
    }

    protected Material HighlightMaterial
    {
        get
        {
            if (this.m_Material == null)
            {
                this.m_Material = new Material(this.m_HighlightShader);
                SceneUtils.SetHideFlags(this.m_Material, HideFlags.DontSave);
            }
            return this.m_Material;
        }
    }

    protected Material MultiSampleBlendMaterial
    {
        get
        {
            if (this.m_MultiSampleBlendMaterial == null)
            {
                this.m_MultiSampleBlendMaterial = new Material(this.m_MultiSampleBlendShader);
                SceneUtils.SetHideFlags(this.m_MultiSampleBlendMaterial, HideFlags.DontSave);
            }
            return this.m_MultiSampleBlendMaterial;
        }
    }

    protected Material MultiSampleMaterial
    {
        get
        {
            if (this.m_MultiSampleMaterial == null)
            {
                this.m_MultiSampleMaterial = new Material(this.m_MultiSampleShader);
                SceneUtils.SetHideFlags(this.m_MultiSampleMaterial, HideFlags.DontSave);
            }
            return this.m_MultiSampleMaterial;
        }
    }

    public RenderTexture SilhouetteTexture
    {
        get
        {
            return this.m_CameraTexture;
        }
    }
}

