using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

[ExecuteInEditMode, CustomEditClass]
public class UberText : MonoBehaviour
{
    [CompilerGenerated]
    private static Func<char, bool> <>f__am$cache6E;
    [CompilerGenerated]
    private static Func<char, bool> <>f__am$cache6F;
    [CompilerGenerated]
    private static Func<char, char> <>f__am$cache70;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map43;
    private const float BOLD_MAX_SIZE = 10f;
    private readonly string BOLD_OUTLINE_TEXT_SHADER_NAME = "Hidden/TextBoldOutline_Unlit";
    private readonly string BOLD_SHADER_NAME = "Hidden/Text_Bold";
    private const int CACHE_FILE_MAX_SIZE = 0xc350;
    private const int CACHE_FILE_VERSION_TEMP = 2;
    private const float CHARACTER_SIZE_SCALE = 0.01f;
    private const string FONT_NAME_BELWE = "Belwe";
    private const string FONT_NAME_BELWE_OUTLINE = "Belwe_Outline";
    private const string FONT_NAME_BLIZZARD_GLOBAL = "BlizzardGlobal";
    private const string FONT_NAME_FRANKLIN_GOTHIC = "FranklinGothic";
    private readonly string INLINE_IMAGE_SHADER_NAME = "Hero/Unlit_Transparent";
    [SerializeField]
    private AlignmentOptions m_Alignment = AlignmentOptions.Center;
    [SerializeField]
    private float m_AmbientLightBlend;
    [SerializeField]
    private AnchorOptions m_Anchor = AnchorOptions.Middle;
    [SerializeField]
    private bool m_AntiAlias;
    [SerializeField]
    private float m_AntiAliasAmount = 0.5f;
    [SerializeField]
    private float m_AntiAliasEdge = 0.5f;
    private Shader m_AntialiasingTextShader;
    private Material m_BoldMaterial;
    private Shader m_BoldOutlineShader;
    private Shader m_BoldShader;
    [SerializeField]
    private float m_BoldSize;
    [SerializeField]
    private bool m_Cache = true;
    private int m_CacheHash;
    private Camera m_Camera;
    private GameObject m_CameraGO;
    [SerializeField]
    private float m_CharacterSize = 5f;
    private bool m_Ellipsized;
    [SerializeField]
    private Font m_Font;
    [SerializeField]
    private int m_FontSize = 0x23;
    private float m_FontSizeModifier = 1f;
    private Texture m_FontTexture;
    [SerializeField]
    private bool m_ForceWrapLargeWords;
    [SerializeField]
    private bool m_GameStringLookup;
    [SerializeField]
    private Color m_GradientLowerColor = Color.white;
    [SerializeField]
    private Color m_GradientUpperColor = Color.white;
    [SerializeField]
    private float m_Height = 1f;
    private bool m_Hidden;
    private Material m_InlineImageMaterial;
    private Shader m_InlineImageShader;
    private bool m_isFontDefLoaded;
    private int m_LineCount;
    private float m_LineSpaceModifier = 1f;
    [SerializeField]
    private float m_LineSpacing = 1f;
    private Font m_LocalizedFont;
    [SerializeField]
    private LocalizationSettings m_LocalizedSettings;
    [SerializeField]
    private float m_MinCharacterSize = 1f;
    [SerializeField]
    private int m_MinFontSize = 10;
    private float m_Offset;
    private int m_OrgRenderQueue = -9999;
    [SerializeField]
    private bool m_Outline;
    [SerializeField]
    private Color m_OutlineColor = Color.black;
    [SerializeField]
    private float m_OutlineSize = 1f;
    private Material m_OutlineTextMaterial;
    private Shader m_OutlineTextShader;
    private GameObject m_PlaneGameObject;
    private Material m_PlaneMaterial;
    private Mesh m_PlaneMesh;
    private Shader m_PlaneShader;
    private float m_PreviousPlaneHeight;
    private float m_PreviousPlaneWidth;
    private int m_PreviousResolution = 0x100;
    private Vector2 m_PreviousTexelSize;
    private string m_PreviousText = string.Empty;
    [SerializeField]
    private GameObject m_RenderOnObject;
    [SerializeField]
    private int m_RenderQueue;
    [SerializeField]
    private bool m_RenderToTexture;
    [SerializeField]
    private bool m_ResizeToFit;
    [SerializeField]
    private int m_Resolution = 0x100;
    [SerializeField]
    private bool m_RichText = true;
    [SerializeField]
    private bool m_Shadow;
    [SerializeField]
    private float m_ShadowBlur = 1.5f;
    [SerializeField]
    private Color m_ShadowColor = new Color(0.1f, 0.1f, 0.1f, 0.333f);
    private Material m_ShadowMaterial;
    [SerializeField]
    private float m_ShadowOffset = 1f;
    private GameObject m_ShadowPlaneGameObject;
    private Shader m_ShadowTextShader;
    private float m_SingleLineAdjustment;
    [SerializeField]
    private string m_Text = "Uber Text";
    private Material m_TextAntialiasingMaterial;
    [SerializeField]
    private Color m_TextColor = Color.white;
    private Material m_TextMaterial;
    private Map<TextRenderMaterial, int> m_TextMaterialIndices = new Map<TextRenderMaterial, int>();
    private TextMesh m_TextMesh;
    private GameObject m_TextMeshGameObject;
    private bool m_TextSet;
    private Shader m_TextShader;
    private RenderTexture m_TextTexture;
    [SerializeField]
    private bool m_Underwear;
    [SerializeField]
    private bool m_UnderwearFlip;
    [SerializeField]
    private float m_UnderwearHeight = 0.2f;
    [SerializeField]
    private float m_UnderwearWidth = 0.2f;
    private bool m_updated;
    [SerializeField]
    private bool m_UseEditorText;
    [SerializeField]
    private float m_Width = 1f;
    private string[] m_Words;
    [SerializeField]
    private bool m_WordWrap;
    private float m_WorldHeight;
    private float m_WorldWidth;
    private const int MAX_REDUCE_TEXT_COUNT = 40;
    private readonly string OUTLINE_NO_VERT_COLOR_TEXT_SHADER_NAME = "Hidden/TextOutline_Unlit_NoVertColor";
    private readonly string OUTLINE_TEXT_SHADER_NAME = "Hidden/TextOutline_Unlit";
    private readonly Vector3[] PLANE_NORMALS = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
    private readonly string PLANE_SHADER_NAME = "Hidden/TextPlane";
    private readonly int[] PLANE_TRIANGLES = new int[] { 3, 1, 2, 2, 1, 0 };
    private readonly Vector2[] PLANE_UVS = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
    private static int RENDER_LAYER = 0x1c;
    private static int RENDER_LAYER_BIT = GameLayer.InvisibleRender.LayerBit();
    private static Map<int, CachedTextValues> s_CachedText = new Map<int, CachedTextValues>();
    private static Texture2D s_InlineImageTexture;
    private static bool s_InlineImageTextureLoaded = false;
    private static float s_offset = -3000f;
    private static Map<Font, Vector2> s_TexelUpdateData = new Map<Font, Vector2>();
    private static Map<Font, int> s_TexelUpdateFrame = new Map<Font, int>();
    private static RenderTextureFormat s_TextureFormat = RenderTextureFormat.DefaultHDR;
    private readonly string SHADOW_SHADER_NAME = "Hidden/TextShadow";
    private readonly string TEXT_ANTIALAISING_SHADER_NAME = "Hidden/TextAntialiasing";
    private readonly string TEXT_SHADER_NAME = "Hero/Text_Unlit";
    private Material TextMeshBaseMaterial;

    public LocalizationSettings.LocaleAdjustment AddLocaleAdjustment(Locale locale)
    {
        return this.m_LocalizedSettings.AddLocale(locale);
    }

    private void AntiAliasRender()
    {
        if ((this.m_PlaneGameObject != null) || (this.m_RenderOnObject != null))
        {
            if (this.m_AntiAlias)
            {
                Texture texture;
                if (this.m_RenderOnObject != null)
                {
                    texture = this.m_RenderOnObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");
                    this.m_RenderOnObject.GetComponent<Renderer>().sharedMaterial = this.TextAntialiasingMaterial;
                    this.m_RenderOnObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", texture);
                }
                else
                {
                    texture = this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");
                    this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial = this.TextAntialiasingMaterial;
                    this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", texture);
                }
                Vector2 vector = this.TexelSize(texture);
                this.m_TextAntialiasingMaterial.SetFloat("_OffsetX", vector.x * this.m_AntiAliasAmount);
                this.m_TextAntialiasingMaterial.SetFloat("_OffsetY", vector.y * this.m_AntiAliasAmount);
                this.m_TextAntialiasingMaterial.SetFloat("_Edge", this.m_AntiAliasEdge);
            }
            else if (this.m_RenderOnObject != null)
            {
                this.m_RenderOnObject.GetComponent<Renderer>().sharedMaterial = this.PlaneMaterial;
            }
            else
            {
                this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial = this.PlaneMaterial;
            }
        }
    }

    private void Awake()
    {
        if ((!this.m_GameStringLookup && !this.m_TextSet) && !this.m_UseEditorText)
        {
            this.m_Text = string.Empty;
        }
        this.FindSupportedTextureFormat();
        if (s_InlineImageTexture == null)
        {
            s_InlineImageTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            s_InlineImageTexture.SetPixel(0, 0, Color.clear);
            s_InlineImageTexture.SetPixel(1, 0, Color.clear);
            s_InlineImageTexture.SetPixel(0, 1, Color.clear);
            s_InlineImageTexture.SetPixel(1, 1, Color.clear);
            s_InlineImageTexture.Apply();
        }
        this.DestroyChildren();
    }

    private void Bold()
    {
        if (this.m_BoldSize > 10f)
        {
            this.m_BoldSize = 10f;
        }
        if (this.m_Outline)
        {
            if (this.m_BoldOutlineShader == null)
            {
                this.m_BoldOutlineShader = ShaderUtils.FindShader(this.BOLD_OUTLINE_TEXT_SHADER_NAME);
                if (this.m_BoldOutlineShader == null)
                {
                    Debug.LogError("UberText Failed to load Shader: " + this.BOLD_OUTLINE_TEXT_SHADER_NAME);
                }
            }
            this.m_BoldMaterial.shader = this.m_BoldOutlineShader;
            Vector2 vector = this.TexelSize(this.m_BoldMaterial.mainTexture);
            this.m_BoldMaterial.SetColor("_OutlineColor", this.m_OutlineColor);
            this.m_BoldMaterial.SetFloat("_BoldOffsetX", this.m_BoldSize * vector.x);
            this.m_BoldMaterial.SetFloat("_BoldOffsetY", this.m_BoldSize * vector.y);
            this.m_BoldMaterial.SetFloat("_OutlineOffsetX", vector.x * (this.m_OutlineSize + (this.m_BoldSize * 0.75f)));
            this.m_BoldMaterial.SetFloat("_OutlineOffsetY", vector.y * (this.m_OutlineSize + (this.m_BoldSize * 0.75f)));
        }
        else
        {
            this.m_BoldMaterial.shader = this.m_BoldShader;
            Vector2 vector2 = this.TexelSize(this.m_BoldMaterial.mainTexture);
            this.m_BoldMaterial.SetFloat("_BoldOffsetX", this.m_BoldSize * vector2.x);
            this.m_BoldMaterial.SetFloat("_BoldOffsetY", this.m_BoldSize * vector2.y);
            this.m_BoldMaterial.SetColor("_Color", this.m_TextColor);
        }
    }

    private string[] BreakStringIntoWords(string text)
    {
        if ((text == null) || (text == string.Empty))
        {
            return null;
        }
        List<string> list = new List<string>();
        bool flag = false;
        TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(text);
        List<int> list2 = new List<int>(StringInfo.ParseCombiningCharacters(text).Length);
        while (textElementEnumerator.MoveNext())
        {
            string textElement = textElementEnumerator.GetTextElement();
            list2.Add(char.ConvertToUtf32(textElement, 0));
        }
        StringBuilder builder = new StringBuilder(char.ConvertFromUtf32(list2[0]));
        if ((char.ConvertFromUtf32(list2[0])[0] == '<') && this.m_RichText)
        {
            flag = true;
        }
        for (int i = 1; i < list2.Count; i++)
        {
            if ((char.ConvertFromUtf32(list2[i])[0] == '<') && this.m_RichText)
            {
                flag = true;
                builder.Append(char.ConvertFromUtf32(list2[i]));
            }
            else if (char.ConvertFromUtf32(list2[i])[0] == '>')
            {
                flag = false;
                builder.Append(char.ConvertFromUtf32(list2[i]));
            }
            else if (flag)
            {
                builder.Append(char.ConvertFromUtf32(list2[i]));
            }
            else
            {
                int lastChar = list2[i - 1];
                int wideChar = list2[i];
                int nextChar = 0;
                if (i < (list2.Count - 1))
                {
                    nextChar = list2[i + 1];
                }
                if (this.CanWrapBetween(lastChar, wideChar, nextChar))
                {
                    list.Add(builder.ToString());
                    builder.Length = 0;
                }
                builder.Append(char.ConvertFromUtf32(list2[i]));
            }
        }
        list.Add(builder.ToString());
        return list.ToArray();
    }

    private Vector2 CalcTextureSize()
    {
        Vector2 vector = new Vector2((float) this.m_Resolution, (float) this.m_Resolution);
        if (this.m_Width > this.m_Height)
        {
            vector.x = this.m_Resolution;
            vector.y = this.m_Resolution * (this.m_Height / this.m_Width);
        }
        else
        {
            vector.x = this.m_Resolution * (this.m_Width / this.m_Height);
            vector.y = this.m_Resolution;
        }
        if ((GraphicsManager.Get() != null) && (GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.Low))
        {
            vector.x *= 0.75f;
            vector.y *= 0.75f;
        }
        return vector;
    }

    private bool CanWrapBetween(int lastChar, int wideChar, int nextChar)
    {
        int num;
        if ((Localization.GetLocale() != Locale.frFR) && (Localization.GetLocale() != Locale.deDE))
        {
            goto Label_0095;
        }
        if (char.IsWhiteSpace(char.ConvertFromUtf32(wideChar), 0))
        {
            num = nextChar;
            switch (num)
            {
                case 0x3a:
                case 0x3b:
                case 0x3f:
                    break;

                default:
                    if (((num != 0x21) && (num != 0x2e)) && ((num != 0xab) && (num != 0xbb)))
                    {
                        goto Label_0077;
                    }
                    break;
            }
            return false;
        }
    Label_0077:
        if (char.IsWhiteSpace(char.ConvertFromUtf32(wideChar), 0) && (lastChar == 0xab))
        {
            return false;
        }
    Label_0095:
        if (lastChar == 0x2d)
        {
            if ((wideChar >= 0x30) && (wideChar <= 0x39))
            {
                return false;
            }
            return true;
        }
        if (lastChar == 0x3b)
        {
            return true;
        }
        if (wideChar == 0x7c)
        {
            return true;
        }
        if (!char.IsWhiteSpace(char.ConvertFromUtf32(lastChar), 0))
        {
            if (char.IsWhiteSpace(char.ConvertFromUtf32(wideChar), 0))
            {
                return true;
            }
            num = lastChar;
            switch (num)
            {
                case 0x3008:
                case 0x300a:
                case 0x300c:
                case 0x300e:
                case 0x3010:
                case 0xffe1:
                case 0xffe5:
                case 0xffe6:
                case 0xfe59:
                case 0xfe5b:
                case 0xfe5d:
                    break;

                default:
                    if (((((num != 0x5b) && (num != 0x5c)) && ((num != 0x24) && (num != 40))) && (((num != 0x7b) && (num != 0x2018)) && ((num != 0x201c) && (num != 0x2035)))) && ((((num != 0x3014) && (num != 0x301d)) && ((num != 0xff04) && (num != 0xff08))) && ((num != 0xff3b) && (num != 0xff5b))))
                    {
                        num = wideChar;
                        switch (num)
                        {
                            case 0xfe50:
                            case 0xfe51:
                            case 0xfe52:
                            case 0xfe54:
                            case 0xfe55:
                            case 0xfe56:
                            case 0xfe57:
                            case 0xfe5a:
                            case 0xfe5c:
                            case 0xfe5e:
                            case 0x3009:
                            case 0x300b:
                            case 0x300d:
                            case 0x300f:
                            case 0x3011:
                            case 0x29:
                            case 0x2c:
                            case 0x2e:
                            case 0x3a:
                            case 0x3b:
                            case 0x3f:
                            case 0x2022:
                            case 0x2026:
                            case 0x2027:
                            case 0xff09:
                            case 0xff0c:
                            case 0xff0e:
                            case 0xff1a:
                            case 0xff1b:
                            case 0xff1f:
                                goto Label_043D;
                        }
                        if ((((((num != 0x2013) && (num != 0x2014)) && ((num != 0x2032) && (num != 0x2033))) && (((num != 0x3001) && (num != 0x3002)) && ((num != 0xff9e) && (num != 0xff9f)))) && ((((num != 0x21) && (num != 0x25)) && ((num != 0x5d) && (num != 0x7d))) && (((num != 0xb0) && (num != 0xb7)) && ((num != 0x2019) && (num != 0x201d))))) && (((((num != 0x2103) && (num != 0x3015)) && ((num != 0x301e) && (num != 0x30fc))) && (((num != 0xfe30) && (num != 0xff01)) && ((num != 0xff05) && (num != 0xff3d)))) && (((num != 0xff5d) && (num != 0xff70)) && (num != 0xffe0))))
                        {
                            switch (lastChar)
                            {
                                case 0x3002:
                                case 0xff0c:
                                    return true;
                            }
                            if ((Localization.GetLocale() == Locale.koKR) && (this.m_Alignment == AlignmentOptions.Center))
                            {
                                return false;
                            }
                            if ((((wideChar < 0x1100) || (wideChar > 0x11ff)) && ((wideChar < 0x3000) || (wideChar > 0xd7af))) && ((((wideChar < 0xf900) || (wideChar > 0xfaff)) && ((wideChar < 0xff00) || (wideChar > 0xff9f))) && ((wideChar < 0xffa0) || (wideChar > 0xffdc))))
                            {
                                return false;
                            }
                            return true;
                        }
                        goto Label_043D;
                    }
                    break;
            }
        }
        return false;
    Label_043D:
        return false;
    }

    private void CleanUp()
    {
        this.m_Offset = 0f;
        this.DestroyRenderPlane();
        this.DestroyCamera();
        this.DestroyTexture();
        this.DestroyShadow();
        this.DestroyTextMesh();
        this.m_updated = false;
        if (this.m_BoldMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_BoldMaterial);
        }
        if (this.m_TextMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_TextMaterial);
        }
        if (this.m_OutlineTextMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_OutlineTextMaterial);
        }
        if (this.m_TextAntialiasingMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_TextAntialiasingMaterial);
        }
        if (this.m_ShadowMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_ShadowMaterial);
        }
        if (this.m_PlaneMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_PlaneMaterial);
        }
        if (this.m_InlineImageMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_InlineImageMaterial);
        }
    }

    private static void CreateCacheFolder()
    {
        string cacheFolderPath = GetCacheFolderPath();
        if (!Directory.Exists(cacheFolderPath))
        {
            try
            {
                Directory.CreateDirectory(cacheFolderPath);
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UberText.CreateCacheFolder() - Failed to create {0}. Reason={1}", cacheFolderPath, exception.Message));
            }
        }
    }

    private void CreateCamera()
    {
        if (this.m_Camera == null)
        {
            this.m_CameraGO = new GameObject();
            this.m_Camera = this.m_CameraGO.AddComponent<Camera>();
            this.m_CameraGO.name = "UberText_RenderCamera_" + base.name;
            SceneUtils.SetHideFlags(this.m_CameraGO, HideFlags.HideAndDontSave);
            this.m_Camera.orthographic = true;
            this.m_CameraGO.transform.parent = this.m_TextMeshGameObject.transform;
            this.m_CameraGO.transform.rotation = Quaternion.identity;
            this.m_CameraGO.transform.position = this.m_TextMeshGameObject.transform.position;
            this.m_Camera.nearClipPlane = -0.1f;
            this.m_Camera.farClipPlane = 0.1f;
            if (Camera.main != null)
            {
                this.m_Camera.depth = Camera.main.depth - 50f;
            }
            Color textColor = this.m_TextColor;
            if (this.m_Outline)
            {
                textColor = this.m_OutlineColor;
            }
            this.m_Camera.backgroundColor = new Color(textColor.r, textColor.g, textColor.b, 0f);
            this.m_Camera.clearFlags = CameraClearFlags.Color;
            this.m_Camera.depthTextureMode = DepthTextureMode.None;
            this.m_Camera.renderingPath = RenderingPath.Forward;
            this.m_Camera.cullingMask = RENDER_LAYER_BIT;
            this.m_Camera.enabled = false;
        }
    }

    private void CreateEditorRoot()
    {
    }

    private void CreateRenderPlane()
    {
        if (((this.m_PlaneMesh == null) || (this.m_Width != this.m_PreviousPlaneWidth)) || ((this.m_Height != this.m_PreviousPlaneHeight) || (this.m_PreviousResolution != this.m_Resolution)))
        {
            if (this.m_PlaneGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(this.m_PlaneGameObject);
            }
            this.m_PlaneGameObject = new GameObject();
            this.m_PlaneGameObject.name = "UberText_RenderPlane_" + base.name;
            this.m_PlaneGameObject.AddComponent<MeshFilter>();
            this.m_PlaneGameObject.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            SceneUtils.SetHideFlags(this.m_PlaneGameObject, HideFlags.DontSave);
            this.m_PlaneGameObject.transform.parent = base.transform;
            this.m_PlaneGameObject.transform.position = base.transform.position;
            this.m_PlaneGameObject.transform.rotation = base.transform.rotation;
            this.m_PlaneGameObject.transform.Rotate((float) -90f, 0f, (float) 0f);
            this.m_PlaneGameObject.transform.localScale = Vector3.one;
            float x = this.m_Width * 0.5f;
            float z = this.m_Height * 0.5f;
            mesh.vertices = new Vector3[] { new Vector3(-x, 0f, -z), new Vector3(x, 0f, -z), new Vector3(-x, 0f, z), new Vector3(x, 0f, z) };
            mesh.colors = new Color[] { this.m_GradientLowerColor, this.m_GradientLowerColor, this.m_GradientUpperColor, this.m_GradientUpperColor };
            mesh.uv = this.PLANE_UVS;
            mesh.normals = this.PLANE_NORMALS;
            mesh.triangles = this.PLANE_TRIANGLES;
            Mesh mesh2 = mesh;
            this.m_PlaneGameObject.GetComponent<MeshFilter>().mesh = mesh2;
            this.m_PlaneMesh = mesh2;
            this.m_PlaneMesh.RecalculateBounds();
            this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial = this.PlaneMaterial;
            this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial.mainTexture = this.m_TextTexture;
            this.m_PreviousPlaneWidth = this.m_Width;
            this.m_PreviousPlaneHeight = this.m_Height;
        }
    }

    private void CreateTextMesh()
    {
        if (this.m_TextMeshGameObject == null)
        {
            this.m_TextMeshGameObject = new GameObject();
            this.m_TextMeshGameObject.name = "UberText_RenderObject_" + base.name;
            SceneUtils.SetHideFlags(this.m_TextMeshGameObject, HideFlags.HideAndDontSave);
        }
        else if (this.m_TextMeshGameObject.GetComponent<TextMesh>() != null)
        {
            this.SetText(string.Empty);
        }
        if (this.m_RenderToTexture)
        {
            Vector3 vector = new Vector3(-3000f, 3000f, this.Offset);
            this.m_TextMeshGameObject.transform.parent = null;
            this.m_TextMeshGameObject.transform.position = vector;
            this.m_TextMeshGameObject.transform.rotation = Quaternion.identity;
        }
        else
        {
            this.m_TextMeshGameObject.transform.parent = base.transform;
            this.m_TextMeshGameObject.transform.localPosition = Vector3.zero;
            this.m_TextMeshGameObject.transform.localRotation = Quaternion.identity;
            this.m_TextMeshGameObject.transform.localScale = Vector3.one;
        }
        if (this.m_TextMesh == null)
        {
            this.m_TextMaterialIndices.Clear();
            if (this.m_TextMeshGameObject == null)
            {
                return;
            }
            if (this.m_TextMeshGameObject.GetComponent<MeshRenderer>() == null)
            {
                this.m_TextMeshGameObject.AddComponent<MeshRenderer>();
            }
            TextMesh component = this.m_TextMeshGameObject.GetComponent<TextMesh>();
            if (component != null)
            {
                this.m_TextMesh = component;
            }
            else
            {
                this.m_TextMesh = this.m_TextMeshGameObject.AddComponent<TextMesh>();
            }
            if (this.m_TextMesh == null)
            {
                Debug.LogError("UberText: Faild to create TextMesh");
                return;
            }
            this.SetRichText(this.m_RichText);
            Texture fontTexture = this.GetFontTexture();
            this.m_TextMesh.GetComponent<Renderer>().sharedMaterial = this.TextMaterial;
            this.m_TextMesh.GetComponent<Renderer>().sharedMaterial.mainTexture = fontTexture;
            this.m_TextMesh.GetComponent<Renderer>().sharedMaterial.color = this.m_TextColor;
            if (this.m_RichText)
            {
                Material[] materialArray = new Material[2];
                materialArray[0] = this.m_TextMesh.GetComponent<Renderer>().sharedMaterial;
                this.m_TextMaterialIndices.Add(TextRenderMaterial.Text, 0);
                materialArray[1] = this.BoldMaterial;
                materialArray[1].mainTexture = fontTexture;
                this.m_TextMaterialIndices.Add(TextRenderMaterial.Bold, 1);
                this.m_TextMesh.GetComponent<Renderer>().sharedMaterials = materialArray;
            }
            else
            {
                Material[] materialArray2 = new Material[] { this.m_TextMesh.GetComponent<Renderer>().sharedMaterial };
                this.m_TextMaterialIndices.Add(TextRenderMaterial.Text, 0);
                this.m_TextMesh.GetComponent<Renderer>().sharedMaterials = materialArray2;
            }
        }
        if (!this.m_Outline && (this.m_TextMesh.GetComponent<Renderer>().sharedMaterial == this.m_OutlineTextMaterial))
        {
            Texture mainTexture = this.m_TextMesh.GetComponent<Renderer>().sharedMaterial.mainTexture;
            this.m_TextMesh.GetComponent<Renderer>().sharedMaterial = this.TextMaterial;
            this.m_TextMesh.GetComponent<Renderer>().sharedMaterial.mainTexture = mainTexture;
        }
        this.SetFont(this.m_Font);
        this.SetFontSize(this.m_FontSize);
        this.SetLineSpacing(this.m_LineSpacing);
        this.SetActualCharacterSize(this.m_CharacterSize * 0.01f);
        if (this.m_Text == null)
        {
            this.SetText(string.Empty);
        }
        else
        {
            this.SetText(this.m_Text);
        }
    }

    private void CreateTexture()
    {
        Vector2 vector = this.CalcTextureSize();
        if (this.m_TextTexture != null)
        {
            if (this.m_Camera.targetTexture == null)
            {
                this.m_Camera.targetTexture = this.m_TextTexture;
            }
            if ((this.m_TextTexture.width == ((int) vector.x)) && (this.m_TextTexture.height == ((int) vector.y)))
            {
                return;
            }
        }
        this.DestroyTexture();
        this.m_TextTexture = new RenderTexture((int) vector.x, (int) vector.y, 0, s_TextureFormat);
        SceneUtils.SetHideFlags(this.m_TextTexture, HideFlags.HideAndDontSave);
        if (this.m_Camera != null)
        {
            this.m_Camera.targetTexture = this.m_TextTexture;
        }
        if ((this.m_PlaneGameObject != null) && (this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial != null))
        {
            this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial.mainTexture = this.m_TextTexture;
        }
        this.m_PreviousResolution = this.m_Resolution;
    }

    private static void DeleteOldCacheFiles()
    {
        IEnumerator enumerator = Enum.GetValues(typeof(Locale)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Locale current = (Locale) ((int) enumerator.Current);
                string path = string.Format("{0}/text_{1}.cache", FileUtils.PersistentDataPath, current);
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        continue;
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(string.Format("UberText.DeleteOldCacheFiles() - Failed to delete {0}. Reason={1}", path, exception.Message));
                        continue;
                    }
                }
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
    }

    private void DestroyCamera()
    {
        if (this.m_CameraGO != null)
        {
            this.m_Camera.targetTexture = null;
            UnityEngine.Object.Destroy(this.m_CameraGO);
        }
    }

    private void DestroyChildren()
    {
        GameObject obj2 = new GameObject("UberTextDestroyDummy");
        foreach (Transform transform in base.GetComponentsInChildren<Transform>())
        {
            if ((base.transform != transform) && (transform != null))
            {
                GameObject gameObject = transform.gameObject;
                if ((gameObject != null) && gameObject.name.StartsWith("UberText_"))
                {
                    gameObject.transform.parent = obj2.transform;
                }
            }
        }
        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(obj2);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(obj2);
        }
    }

    private void DestroyRenderPlane()
    {
        if (this.m_PlaneGameObject != null)
        {
            MeshFilter component = this.m_PlaneGameObject.GetComponent<MeshFilter>();
            if (component != null)
            {
                UnityEngine.Object.DestroyImmediate(component.sharedMesh);
                UnityEngine.Object.DestroyImmediate(component);
            }
            UnityEngine.Object.DestroyImmediate(this.m_PlaneGameObject);
        }
    }

    private void DestroyShadow()
    {
        if (this.m_ShadowPlaneGameObject != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_ShadowPlaneGameObject);
        }
    }

    private void DestroyTextMesh()
    {
        if (this.m_TextMeshGameObject != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_TextMeshGameObject);
        }
    }

    private void DestroyTexture()
    {
        if (this.m_TextTexture != null)
        {
            UnityEngine.Object.Destroy(this.m_TextTexture);
        }
    }

    public void EditorAwake()
    {
        this.DestroyChildren();
        this.UpdateText();
    }

    public static UberText[] EnableAllTextInObject(GameObject obj, bool enable)
    {
        UberText[] componentsInChildren = obj.GetComponentsInChildren<UberText>();
        EnableAllTextObjects(componentsInChildren, enable);
        return componentsInChildren;
    }

    public static void EnableAllTextObjects(UberText[] objs, bool enable)
    {
        foreach (UberText text in objs)
        {
            text.gameObject.SetActive(enable);
        }
    }

    private void FindSupportedTextureFormat()
    {
        if (s_TextureFormat == RenderTextureFormat.DefaultHDR)
        {
            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444))
            {
                s_TextureFormat = RenderTextureFormat.ARGB4444;
            }
            else
            {
                s_TextureFormat = RenderTextureFormat.Default;
            }
        }
    }

    public float GetActualCharacterSize()
    {
        return (this.m_TextMesh.characterSize / 0.01f);
    }

    public LocalizationSettings GetAllLocalizationSettings()
    {
        return this.m_LocalizedSettings;
    }

    public Bounds GetBounds()
    {
        Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
        Vector3 vector = (Vector3) (localToWorldMatrix.MultiplyVector(Vector3.up) * (this.m_Height * 0.5f));
        Vector3 vector2 = (Vector3) (localToWorldMatrix.MultiplyVector(Vector3.right) * (this.m_Width * 0.5f));
        return new Bounds { min = (base.transform.position - vector2) + vector, max = (base.transform.position + vector2) - vector };
    }

    private static string GetCacheFilePath()
    {
        return string.Format("{0}/text_{1}.cache", GetCacheFolderPath(), Localization.GetLocale());
    }

    private static string GetCacheFolderPath()
    {
        return string.Format("{0}/UberText", FileUtils.CachePath);
    }

    private Texture GetFontTexture()
    {
        if (this.m_LocalizedFont == null)
        {
            return this.m_Font.material.mainTexture;
        }
        return this.m_LocalizedFont.material.mainTexture;
    }

    public int GetLineCount()
    {
        return this.m_LineCount;
    }

    public LocalizationSettings.LocaleAdjustment GetLocaleAdjustment(Locale locale)
    {
        return this.m_LocalizedSettings.GetLocale(locale);
    }

    public Vector3 GetLocalizationPositionOffset()
    {
        Vector3 zero = Vector3.zero;
        if (this.m_LocalizedSettings != null)
        {
            LocalizationSettings.LocaleAdjustment locale = this.m_LocalizedSettings.GetLocale(Localization.GetLocale());
            if (locale != null)
            {
                zero = locale.m_PositionOffset;
            }
        }
        return zero;
    }

    public Font GetLocalizedFont()
    {
        if (this.m_LocalizedFont != null)
        {
            return this.m_LocalizedFont;
        }
        return this.m_Font;
    }

    private Vector3 GetLossyWorldScale(Transform xform)
    {
        Quaternion rotation = xform.rotation;
        xform.rotation = Quaternion.identity;
        Vector3 lossyScale = base.transform.lossyScale;
        xform.rotation = rotation;
        return lossyScale;
    }

    public Bounds GetTextBounds()
    {
        if (!this.m_updated)
        {
            this.UpdateNow();
        }
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        if (this.m_TextMesh != null)
        {
            Quaternion rotation = base.transform.rotation;
            base.transform.rotation = Quaternion.identity;
            bounds = this.m_TextMesh.GetComponent<Renderer>().bounds;
            base.transform.rotation = rotation;
        }
        return bounds;
    }

    public Bounds GetTextWorldSpaceBounds()
    {
        if (!this.m_updated)
        {
            this.UpdateNow();
        }
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        if (this.m_TextMesh != null)
        {
            bounds = this.m_TextMesh.GetComponent<Renderer>().bounds;
        }
        return bounds;
    }

    public static Vector3 GetWorldScale(Transform xform)
    {
        Vector3 localScale = xform.localScale;
        if (xform.parent != null)
        {
            for (Transform transform = xform.parent; transform != null; transform = transform.parent)
            {
                localScale.Scale(transform.localScale);
            }
        }
        return localScale;
    }

    private Vector2 GetWorldWidthAndHight()
    {
        Quaternion rotation = base.transform.rotation;
        base.transform.rotation = Quaternion.identity;
        Vector3 lossyScale = base.transform.lossyScale;
        float width = this.m_Width;
        if (lossyScale.x > 0f)
        {
            width = this.m_Width * lossyScale.x;
        }
        float height = this.m_Height;
        if (lossyScale.y > 0f)
        {
            height = this.m_Height * lossyScale.y;
        }
        base.transform.rotation = rotation;
        return new Vector2(width, height);
    }

    public void Hide()
    {
        this.m_Hidden = true;
        this.UpdateText();
    }

    private string InlineImage(string tag)
    {
        string str;
        if (tag == string.Empty)
        {
            return string.Empty;
        }
        if (!s_InlineImageTextureLoaded)
        {
            AssetLoader.Get().LoadTexture("mana_in_line", new AssetLoader.ObjectCallback(this.SetManaTexture), null, false);
        }
        int index = this.TextEffectsMaterial(TextRenderMaterial.InlineImages, this.InlineImageMaterial);
        float num2 = 0f;
        float num3 = 0f;
        string key = tag;
        if (key != null)
        {
            int num4;
            if (<>f__switch$map43 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(13);
                dictionary.Add("<m>", 0);
                dictionary.Add("<me>", 1);
                dictionary.Add("<m0>", 2);
                dictionary.Add("<m1>", 3);
                dictionary.Add("<m2>", 4);
                dictionary.Add("<m3>", 5);
                dictionary.Add("<m4>", 6);
                dictionary.Add("<m5>", 7);
                dictionary.Add("<m6>", 8);
                dictionary.Add("<m7>", 9);
                dictionary.Add("<m8>", 10);
                dictionary.Add("<m9>", 11);
                dictionary.Add("<m10>", 12);
                <>f__switch$map43 = dictionary;
            }
            if (<>f__switch$map43.TryGetValue(key, out num4))
            {
                switch (num4)
                {
                    case 0:
                        num2 = 0f;
                        num3 = 0f;
                        goto Label_025B;

                    case 1:
                        num2 = 0.75f;
                        num3 = 0.25f;
                        goto Label_025B;

                    case 2:
                        num2 = 0f;
                        num3 = 0.75f;
                        goto Label_025B;

                    case 3:
                        num2 = 0.25f;
                        num3 = 0.75f;
                        goto Label_025B;

                    case 4:
                        num2 = 0.5f;
                        num3 = 0.75f;
                        goto Label_025B;

                    case 5:
                        num2 = 0.75f;
                        num3 = 0.75f;
                        goto Label_025B;

                    case 6:
                        num2 = 0f;
                        num3 = 0.5f;
                        goto Label_025B;

                    case 7:
                        num2 = 0.25f;
                        num3 = 0.5f;
                        goto Label_025B;

                    case 8:
                        num2 = 0.5f;
                        num3 = 0.5f;
                        goto Label_025B;

                    case 9:
                        num2 = 0.75f;
                        num3 = 0.5f;
                        goto Label_025B;

                    case 10:
                        num2 = 0f;
                        num3 = 0.25f;
                        goto Label_025B;

                    case 11:
                        num2 = 0.25f;
                        num3 = 0.25f;
                        goto Label_025B;

                    case 12:
                        num2 = 0.5f;
                        num3 = 0.25f;
                        goto Label_025B;
                }
            }
        }
        return tag;
    Label_025B:
        str = string.Empty;
        this.m_TextMesh.GetComponent<Renderer>().sharedMaterials[index].mainTexture = s_InlineImageTexture;
        object[] args = new object[] { index, this.m_FontSize, num2, num3 };
        return string.Format("<quad material={0} size={1} x={2} y={3} width=0.25 height=0.25 />", args);
    }

    public bool IsDone()
    {
        return this.m_updated;
    }

    public bool IsEllipsized()
    {
        return this.m_Ellipsized;
    }

    public bool isHidden()
    {
        return this.m_Hidden;
    }

    public bool IsMultiLine()
    {
        return (this.m_LineCount > 1);
    }

    private static int LineCount(string s)
    {
        int num = 1;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '\n')
            {
                num++;
            }
        }
        return num;
    }

    public static void LoadCachedData()
    {
        s_CachedText.Clear();
        DeleteOldCacheFiles();
        CreateCacheFolder();
        string cacheFilePath = GetCacheFilePath();
        if (System.IO.File.Exists(cacheFilePath))
        {
            int num = 2;
            num = 0x2acc;
            using (BinaryReader reader = new BinaryReader(System.IO.File.Open(cacheFilePath, FileMode.Open)))
            {
                if (((reader.BaseStream.Length == 0) || (reader.ReadInt32() != num)) || (reader.PeekChar() == -1))
                {
                    return;
                }
                try
                {
                    while (reader.PeekChar() != -1)
                    {
                        int key = reader.ReadInt32();
                        CachedTextValues values = new CachedTextValues {
                            m_Text = reader.ReadString(),
                            m_CharSize = reader.ReadSingle(),
                            m_OriginalTextHash = reader.ReadInt32()
                        };
                        s_CachedText.Add(key, values);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(string.Format("UberText LoadCachedData() failed: {0}", exception.Message));
                    s_CachedText.Clear();
                }
            }
            if (s_CachedText.Count > 0xc350)
            {
                s_CachedText.Clear();
            }
            Log.Kyle.Print("UberText Cache Loaded: " + s_CachedText.Count.ToString(), new object[0]);
        }
    }

    private string LocalizationFixes(string text)
    {
        return text;
    }

    private void OnDestroy()
    {
        this.CleanUp();
    }

    private void OnDisable()
    {
        if (this.m_RenderOnObject != null)
        {
            this.m_RenderOnObject.GetComponent<Renderer>().enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = base.transform.localToWorldMatrix;
        Gizmos.color = new Color(0.3f, 0.3f, 0.35f, 0.2f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(this.m_Width + (this.m_Width * 0.02f), this.m_Height + (this.m_Height * 0.02f), 0f));
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(this.m_Width, this.m_Height, 0f));
        if (this.m_Underwear)
        {
            float x = (this.m_Width * this.m_UnderwearWidth) * 0.5f;
            float y = this.m_Height * this.m_UnderwearHeight;
            if (this.m_UnderwearFlip)
            {
                Vector3 center = new Vector3(-((this.m_Width * 0.5f) - (x * 0.5f)), (this.m_Height * 0.5f) - (y * 0.5f), 0f);
                Gizmos.DrawWireCube(center, new Vector3(x, y, 0f));
                Vector3 vector2 = new Vector3((this.m_Width * 0.5f) - (x * 0.5f), (this.m_Height * 0.5f) - (y * 0.5f), 0f);
                Gizmos.DrawWireCube(vector2, new Vector3(x, y, 0f));
            }
            else
            {
                Vector3 vector3 = new Vector3(-((this.m_Width * 0.5f) - (x * 0.5f)), -((this.m_Height * 0.5f) - (y * 0.5f)), 0f);
                Gizmos.DrawWireCube(vector3, new Vector3(x, y, 0f));
                Vector3 vector4 = new Vector3((this.m_Width * 0.5f) - (x * 0.5f), -((this.m_Height * 0.5f) - (y * 0.5f)), 0f);
                Gizmos.DrawWireCube(vector4, new Vector3(x, y, 0f));
            }
        }
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = base.transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(this.m_Width + (this.m_Width * 0.04f), this.m_Height + (this.m_Height * 0.04f), 0f));
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void OnEnable()
    {
        this.m_updated = false;
        this.SetFont(this.m_Font);
        this.UpdateNow();
    }

    private void OutlineRender()
    {
        if (this.m_TextMesh == null)
        {
            Debug.LogError(string.Format("UberText OutlineRender ({0}, {1}): m_TextMesh == null", base.gameObject.name, this.m_Text));
        }
        else
        {
            Material sharedMaterial = this.m_TextMesh.GetComponent<Renderer>().sharedMaterial;
            if (sharedMaterial == null)
            {
                Debug.LogError(string.Format("UberText OutlineRender ({0}, {1}): m_TextMesh.renderer.sharedMaterial == null", base.gameObject.name, this.m_Text));
            }
            else
            {
                Texture mainTexture = sharedMaterial.mainTexture;
                if (mainTexture == null)
                {
                    Debug.LogError(string.Format("UberText OutlineRender ({0}, {1}): textMat.mainTexture == null", base.gameObject.name, this.m_Text));
                }
                else if (this.OutlineTextMaterial == null)
                {
                    Debug.LogError(string.Format("UberText OutlineRender ({0}, {1}): OutlineTextMaterial == null", base.gameObject.name, this.m_Text));
                }
                else
                {
                    this.m_TextMesh.GetComponent<Renderer>().sharedMaterial = this.OutlineTextMaterial;
                    if (this.m_OutlineTextMaterial == null)
                    {
                        Debug.LogError(string.Format("UberText OutlineRender ({0}, {1}): m_OutlineTextMaterial == null", base.gameObject.name, this.m_Text));
                    }
                    else
                    {
                        this.m_OutlineTextMaterial.mainTexture = mainTexture;
                        Vector2 vector = this.TexelSize(this.m_OutlineTextMaterial.mainTexture);
                        float outlineSize = this.m_OutlineSize;
                        this.m_OutlineTextMaterial.SetFloat("_OutlineOffsetX", vector.x * outlineSize);
                        this.m_OutlineTextMaterial.SetFloat("_OutlineOffsetY", vector.y * outlineSize);
                        this.m_OutlineTextMaterial.SetColor("_Color", this.m_TextColor);
                        this.m_OutlineTextMaterial.SetColor("_OutlineColor", this.m_OutlineColor);
                        this.m_OutlineTextMaterial.SetFloat("_TexelSizeX", vector.x);
                        this.m_OutlineTextMaterial.SetFloat("_TexelSizeY", vector.y);
                    }
                }
            }
        }
    }

    private string ProcessText(string text)
    {
        if (!this.m_RichText)
        {
            return text;
        }
        StringBuilder builder = new StringBuilder();
        builder.Append("<material=1></material>");
        builder.Append(text);
        for (int i = 0; i < text.Length; i++)
        {
            if ((text[i] == '<') && (i <= (text.Length - 2)))
            {
                if (text[i + 1] == 'b')
                {
                    if (((i + 3) >= text.Length) || (text[i + 3] != '<'))
                    {
                        this.Bold();
                        if (this.m_TextMesh.GetComponent<Renderer>().sharedMaterials.Length < 1)
                        {
                            Debug.LogWarning("UberText: Tried to set Bold material, but material missing!");
                        }
                        else
                        {
                            builder.Replace("<b>", "<material=1>");
                            builder.Replace("</b>", "</material>");
                            i++;
                        }
                    }
                }
                else if (((text[i + 1] == 'm') && (i <= (text.Length - 3))) && (text[i + 2] != 'a'))
                {
                    int index = text.Substring(i).IndexOf('>');
                    if (index < 1)
                    {
                        i++;
                    }
                    else
                    {
                        string oldValue = text.Substring(i, index + 1);
                        builder.Replace(oldValue, this.InlineImage(oldValue));
                    }
                }
            }
        }
        string str2 = builder.ToString();
        if (str2 == null)
        {
            Debug.LogWarning("UberText: ProcessText returned a null string!");
            str2 = string.Empty;
        }
        return str2;
    }

    private void ReduceText(string text, int step, int newSize)
    {
        if (this.m_FontSize != 1)
        {
            this.SetFontSize(newSize);
            float height = this.m_Height;
            float width = this.m_Width;
            if (!this.m_RenderToTexture)
            {
                height = this.m_WorldHeight;
                width = this.m_WorldWidth;
            }
            if (!this.IsMultiLine())
            {
                this.SetLineSpacing(0f);
            }
            float y = this.m_TextMesh.GetComponent<Renderer>().bounds.size.y;
            float x = this.m_TextMesh.GetComponent<Renderer>().bounds.size.x;
            int num5 = 0;
            while ((y > height) || (x > width))
            {
                num5++;
                if (num5 > 40)
                {
                    break;
                }
                newSize -= step;
                if (newSize < this.m_MinFontSize)
                {
                    newSize = this.m_MinFontSize;
                    break;
                }
                this.SetFontSize(newSize);
                if (this.m_WordWrap)
                {
                    this.SetText(this.WordWrapString(text, width));
                }
                y = this.m_TextMesh.GetComponent<Renderer>().bounds.size.y;
                x = this.m_TextMesh.GetComponent<Renderer>().bounds.size.x;
            }
            if (!this.IsMultiLine())
            {
                this.SetLineSpacing(this.m_LineSpacing);
            }
            this.m_FontSize = newSize;
        }
    }

    private void ReduceText_CharSize(string text)
    {
        float height = this.m_Height;
        float width = this.m_Width;
        float characterSize = this.m_TextMesh.characterSize;
        if (!this.IsMultiLine())
        {
            this.SetLineSpacing(0f);
        }
        else
        {
            this.SetLineSpacing(this.m_LineSpacing);
        }
        float x = this.m_TextMesh.GetComponent<Renderer>().bounds.size.x;
        float y = this.m_TextMesh.GetComponent<Renderer>().bounds.size.y;
        int num6 = 0;
        while ((y > height) || (x > width))
        {
            num6++;
            if (num6 > 40)
            {
                break;
            }
            characterSize *= 0.95f;
            if (characterSize <= (this.m_MinCharacterSize * 0.01f))
            {
                characterSize = this.m_MinCharacterSize * 0.01f;
                this.SetActualCharacterSize(characterSize);
                if (this.m_WordWrap)
                {
                    this.SetText(this.WordWrapString(text, width, true));
                }
                break;
            }
            this.SetActualCharacterSize(characterSize);
            if (this.m_WordWrap)
            {
                this.SetText(this.WordWrapString(text, width, false));
            }
            if (LineCount(this.m_TextMesh.text) > 1)
            {
                this.SetLineSpacing(this.m_LineSpacing);
            }
            else
            {
                this.SetLineSpacing(0f);
            }
            x = this.m_TextMesh.GetComponent<Renderer>().bounds.size.x;
            y = this.m_TextMesh.GetComponent<Renderer>().bounds.size.y;
        }
        this.SetLineSpacing(this.m_LineSpacing);
    }

    public void RemoveLocaleAdjustment(Locale locale)
    {
        this.m_LocalizedSettings.RemoveLocale(locale);
    }

    private string RemoveTagsFromWord(string word)
    {
        if (!this.m_RichText)
        {
            return word;
        }
        if (!word.Contains("<"))
        {
            return word;
        }
        StringBuilder builder = new StringBuilder();
        bool flag = false;
        for (int i = 0; i < word.Length; i++)
        {
            if (word[i] == '<')
            {
                if (word[i + 1] == 'q')
                {
                    if (!word.Substring(i).Contains(">"))
                    {
                        return builder.ToString();
                    }
                    int num2 = i + 1;
                    while (word[num2] != '>')
                    {
                        num2++;
                    }
                    builder.Append("W");
                    i = num2;
                }
                else
                {
                    flag = true;
                }
            }
            else if (word[i] == '>')
            {
                flag = false;
            }
            else if (!flag)
            {
                builder.Append(word[i]);
            }
        }
        return builder.ToString();
    }

    private void RenderText()
    {
        if (!this.m_updated || ((this.m_TextTexture != null) && !this.m_TextTexture.IsCreated()))
        {
            if (this.m_Font == null)
            {
                Debug.LogWarning(string.Format("UberText error: Font is null for {0}", base.gameObject.name));
            }
            else if ((this.m_Text == null) || (this.m_Text == string.Empty))
            {
                if (this.m_TextMesh != null)
                {
                    this.m_TextMesh.GetComponent<Renderer>().enabled = false;
                }
                if (this.m_PlaneGameObject != null)
                {
                    this.m_PlaneGameObject.GetComponent<Renderer>().enabled = false;
                }
                if (this.m_RenderOnObject != null)
                {
                    this.m_RenderOnObject.GetComponent<Renderer>().enabled = false;
                }
                if (this.m_ShadowPlaneGameObject != null)
                {
                    this.m_ShadowPlaneGameObject.GetComponent<Renderer>().enabled = false;
                }
            }
            else
            {
                if (this.m_TextMesh != null)
                {
                    this.m_TextMesh.GetComponent<Renderer>().enabled = true;
                }
                if (this.m_PlaneGameObject != null)
                {
                    this.m_PlaneGameObject.GetComponent<Renderer>().enabled = true;
                }
                if (this.m_RenderOnObject != null)
                {
                    this.m_RenderOnObject.GetComponent<Renderer>().enabled = true;
                }
                if (this.m_ShadowPlaneGameObject != null)
                {
                    this.m_ShadowPlaneGameObject.GetComponent<Renderer>().enabled = true;
                }
                if (this.m_Hidden)
                {
                    if (this.m_TextMesh != null)
                    {
                        this.m_TextMesh.GetComponent<Renderer>().enabled = false;
                    }
                    if (this.m_PlaneGameObject != null)
                    {
                        this.m_PlaneGameObject.GetComponent<Renderer>().enabled = false;
                    }
                    if (this.m_RenderOnObject != null)
                    {
                        this.m_RenderOnObject.GetComponent<Renderer>().enabled = false;
                    }
                    if (this.m_ShadowPlaneGameObject != null)
                    {
                        this.m_ShadowPlaneGameObject.GetComponent<Renderer>().enabled = false;
                    }
                }
                else
                {
                    if (this.m_TextMesh != null)
                    {
                        this.m_TextMesh.GetComponent<Renderer>().enabled = true;
                    }
                    if (this.m_PlaneGameObject != null)
                    {
                        this.m_PlaneGameObject.GetComponent<Renderer>().enabled = true;
                    }
                    if (this.m_RenderOnObject != null)
                    {
                        this.m_RenderOnObject.GetComponent<Renderer>().enabled = true;
                    }
                    if (this.m_ShadowPlaneGameObject != null)
                    {
                        this.m_ShadowPlaneGameObject.GetComponent<Renderer>().enabled = true;
                    }
                    Vector2 worldWidthAndHight = this.GetWorldWidthAndHight();
                    this.m_WorldWidth = worldWidthAndHight.x;
                    this.m_WorldHeight = worldWidthAndHight.y;
                    this.CreateTextMesh();
                    if (this.m_TextMesh != null)
                    {
                        this.UpdateTextMesh();
                        if (this.m_Outline)
                        {
                            this.OutlineRender();
                        }
                        if (this.m_RenderToTexture)
                        {
                            this.CreateCamera();
                            this.CreateTexture();
                            if (this.m_RenderOnObject != null)
                            {
                                this.SetupRenderOnObject();
                            }
                            else
                            {
                                this.CreateRenderPlane();
                            }
                            this.SetupForRender();
                            if (this.m_RenderOnObject == null)
                            {
                                this.ShadowRender();
                            }
                            this.UpdateTexelSize();
                            if (this.m_TextTexture == null)
                            {
                                Debug.LogWarning("UberText Render to Texture m_TextTexture is null!");
                                this.m_updated = false;
                                return;
                            }
                            if (this.m_Camera.targetTexture != this.m_TextTexture)
                            {
                                this.m_Camera.targetTexture = this.m_TextTexture;
                            }
                            this.m_Camera.Render();
                            if (!this.m_TextTexture.IsCreated())
                            {
                                Debug.LogWarning("UberText Render to Texture m_TextTexture.IsCreated() == false after render!");
                                this.m_updated = false;
                                return;
                            }
                            this.AntiAliasRender();
                        }
                        this.UpdateLayers();
                        this.UpdateRenderQueue();
                        this.UpdateColor();
                        if (this.m_RenderOnObject != null)
                        {
                            this.m_RenderOnObject.GetComponent<Renderer>().enabled = true;
                        }
                        this.m_PreviousText = this.m_Text;
                        this.m_updated = true;
                    }
                }
            }
        }
    }

    private void ResizeTextToFit(string text)
    {
        if ((text != null) && (text != string.Empty))
        {
            Transform parent = this.m_TextMeshGameObject.transform.parent;
            Quaternion rotation = this.m_TextMeshGameObject.transform.rotation;
            Vector3 localScale = this.m_TextMeshGameObject.transform.localScale;
            this.m_TextMeshGameObject.transform.parent = null;
            this.m_TextMeshGameObject.transform.localScale = Vector3.one;
            this.m_TextMeshGameObject.transform.rotation = Quaternion.identity;
            float width = this.m_Width;
            string str = this.RemoveTagsFromWord(text);
            if (str == null)
            {
                str = string.Empty;
            }
            this.SetText(str);
            if (this.m_WordWrap)
            {
                this.SetText(this.WordWrapString(text, width));
            }
            this.ReduceText_CharSize(text);
            this.m_TextMeshGameObject.transform.parent = parent;
            this.m_TextMeshGameObject.transform.localScale = localScale;
            this.m_TextMeshGameObject.transform.rotation = rotation;
            if (!this.m_WordWrap)
            {
                this.SetText(text);
            }
        }
    }

    private void SetActualCharacterSize(float characterSize)
    {
        if (this.m_TextMesh.characterSize != characterSize)
        {
            this.m_TextMesh.characterSize = characterSize;
        }
    }

    private void SetFont(Font font)
    {
        if (font != null)
        {
            if (!this.m_isFontDefLoaded)
            {
                FontTable table = FontTable.Get();
                if (table != null)
                {
                    FontDef fontDef = table.GetFontDef(font);
                    if (fontDef != null)
                    {
                        this.m_LocalizedFont = fontDef.m_Font;
                        this.m_LineSpaceModifier = fontDef.m_LineSpaceModifier;
                        this.m_FontSizeModifier = fontDef.m_FontSizeModifier;
                        this.m_SingleLineAdjustment = fontDef.m_SingleLineAdjustment;
                        this.m_isFontDefLoaded = true;
                    }
                    else
                    {
                        Debug.LogError("Error loading fontDef for:" + base.name);
                    }
                }
            }
            if ((this.m_TextMesh != null) && (this.m_TextMesh.font != font))
            {
                if (this.m_LocalizedFont == null)
                {
                    this.m_TextMesh.font = this.m_Font;
                }
                else
                {
                    this.m_TextMesh.font = this.m_LocalizedFont;
                }
                this.m_FontTexture = this.m_TextMesh.font.material.mainTexture;
                this.UpdateFontTextures();
            }
        }
    }

    private void SetFontSize(int fontSize)
    {
        if (this.m_LocalizedSettings != null)
        {
            LocalizationSettings.LocaleAdjustment locale = this.m_LocalizedSettings.GetLocale(Localization.GetLocale());
            if (locale != null)
            {
                fontSize = (int) (locale.m_FontSizeModifier * fontSize);
            }
        }
        fontSize = (int) (this.m_FontSizeModifier * fontSize);
        if (this.m_TextMesh.fontSize != fontSize)
        {
            this.m_TextMesh.fontSize = fontSize;
        }
    }

    public void SetFontWithoutLocalization(FontDef fontDef)
    {
        Font font = fontDef.m_Font;
        if (font != null)
        {
            this.m_Font = font;
            this.m_LocalizedFont = this.m_Font;
            this.m_LineSpaceModifier = fontDef.m_LineSpaceModifier;
            this.m_FontSizeModifier = fontDef.m_FontSizeModifier;
            this.m_SingleLineAdjustment = fontDef.m_SingleLineAdjustment;
            this.m_isFontDefLoaded = true;
            if (this.m_TextMesh != null)
            {
                this.m_TextMesh.font = font;
                this.m_FontTexture = this.m_TextMesh.font.material.mainTexture;
                this.UpdateFontTextures();
            }
            this.UpdateText();
        }
    }

    public void SetGameStringText(string gameStringTag)
    {
        this.Text = GameStrings.Get(gameStringTag);
    }

    private void SetLineSpacing(float lineSpacing)
    {
        int num = LineCount(this.m_TextMesh.text);
        if (this.m_LocalizedSettings != null)
        {
            LocalizationSettings.LocaleAdjustment locale = this.m_LocalizedSettings.GetLocale(Localization.GetLocale());
            if (locale != null)
            {
                if (num == 1)
                {
                    lineSpacing += locale.m_SingleLineAdjustment;
                }
                else
                {
                    lineSpacing *= locale.m_LineSpaceModifier;
                }
            }
        }
        if (num == 1)
        {
            lineSpacing += this.m_SingleLineAdjustment;
        }
        else
        {
            lineSpacing *= this.m_LineSpaceModifier;
        }
        if (this.m_TextMesh.lineSpacing != lineSpacing)
        {
            this.m_TextMesh.lineSpacing = lineSpacing;
        }
    }

    private void SetManaTexture(string name, UnityEngine.Object obj, object callbackData)
    {
        s_InlineImageTexture = obj as Texture2D;
        s_InlineImageTextureLoaded = true;
        this.m_TextMesh.GetComponent<Renderer>().sharedMaterials[this.m_TextMaterialIndices[TextRenderMaterial.InlineImages]].mainTexture = s_InlineImageTexture;
    }

    private void SetRichText(bool richText)
    {
        if (this.m_TextMesh.richText != richText)
        {
            this.m_TextMesh.richText = richText;
        }
    }

    private void SetText(string text)
    {
        if (this.m_TextMesh.text != text)
        {
            this.m_TextMesh.text = text;
        }
    }

    private void SetupForRender()
    {
        if (this.m_RenderToTexture)
        {
            Vector3 vector = new Vector3(-3000f, 3000f, this.Offset);
            this.m_TextMeshGameObject.transform.parent = null;
            this.m_TextMeshGameObject.transform.position = vector;
            this.m_TextMeshGameObject.transform.rotation = Quaternion.identity;
            this.m_TextMeshGameObject.transform.localScale = Vector3.one;
            this.m_TextMeshGameObject.layer = RENDER_LAYER;
            float x = -3000f;
            if (this.Alignment == AlignmentOptions.Left)
            {
                x += this.m_Width * 0.5f;
            }
            if (this.Alignment == AlignmentOptions.Right)
            {
                x -= this.m_Width * 0.5f;
            }
            float num2 = 0f;
            if (this.m_Anchor == AnchorOptions.Upper)
            {
                num2 += this.m_Height * 0.5f;
            }
            if (this.m_Anchor == AnchorOptions.Lower)
            {
                num2 -= this.m_Height * 0.5f;
            }
            Vector3 vector2 = new Vector3(x, 3000f - num2, this.Offset);
            this.m_CameraGO.transform.parent = this.m_TextMeshGameObject.transform;
            this.m_CameraGO.transform.position = vector2;
            Color textColor = this.m_TextColor;
            if (this.m_Outline)
            {
                textColor = this.m_OutlineColor;
            }
            this.m_Camera.backgroundColor = new Color(textColor.r, textColor.g, textColor.b, 0f);
            this.m_Camera.orthographicSize = this.m_Height * 0.5f;
            if ((this.RenderOnObject == null) && (this.m_PlaneGameObject != null))
            {
                this.m_PlaneGameObject.GetComponent<Renderer>().sharedMaterial.mainTexture = this.m_TextTexture;
            }
        }
        else
        {
            this.m_TextMeshGameObject.transform.parent = base.transform;
            this.m_TextMeshGameObject.transform.localPosition = Vector3.zero;
            this.m_CameraGO.transform.position = base.transform.position;
        }
    }

    private void SetupRenderOnObject()
    {
        if (this.m_RenderOnObject != null)
        {
            this.m_RenderOnObject.GetComponent<Renderer>().sharedMaterial = this.PlaneMaterial;
            this.m_RenderOnObject.GetComponent<Renderer>().sharedMaterial.mainTexture = this.m_TextTexture;
        }
    }

    private void ShadowRender()
    {
        if (!this.m_Shadow)
        {
            this.DestroyShadow();
        }
        else if (this.m_PlaneGameObject != null)
        {
            if (this.m_ShadowPlaneGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(this.m_ShadowPlaneGameObject);
            }
            this.m_ShadowPlaneGameObject = new GameObject();
            this.m_ShadowPlaneGameObject.name = "UberText_ShadowPlane_" + base.name;
            this.m_ShadowPlaneGameObject.AddComponent<MeshFilter>();
            this.m_ShadowPlaneGameObject.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            SceneUtils.SetHideFlags(this.m_ShadowPlaneGameObject, HideFlags.DontSave);
            this.m_ShadowPlaneGameObject.transform.parent = this.m_PlaneGameObject.transform;
            this.m_ShadowPlaneGameObject.transform.localRotation = Quaternion.identity;
            this.m_ShadowPlaneGameObject.transform.localScale = Vector3.one;
            float x = -this.m_ShadowOffset * 0.01f;
            this.m_ShadowPlaneGameObject.transform.localPosition = new Vector3(x, 0f, x);
            float num2 = this.m_Width * 0.5f;
            float z = this.m_Height * 0.5f;
            mesh.vertices = new Vector3[] { new Vector3(-num2, 0f, -z), new Vector3(num2, 0f, -z), new Vector3(-num2, 0f, z), new Vector3(num2, 0f, z) };
            mesh.uv = this.PLANE_UVS;
            mesh.normals = this.PLANE_NORMALS;
            mesh.triangles = this.PLANE_TRIANGLES;
            Mesh mesh3 = mesh;
            this.m_ShadowPlaneGameObject.GetComponent<MeshFilter>().mesh = mesh3;
            mesh3.RecalculateBounds();
            this.m_ShadowPlaneGameObject.GetComponent<Renderer>().sharedMaterial = this.ShadowMaterial;
            this.m_ShadowMaterial.mainTexture = this.m_TextTexture;
            Vector2 vector = this.TexelSize(this.m_TextTexture);
            this.m_ShadowMaterial.SetColor("_Color", this.m_ShadowColor);
            this.m_ShadowMaterial.SetFloat("_OffsetX", vector.x * this.m_ShadowBlur);
            this.m_ShadowMaterial.SetFloat("_OffsetY", vector.y * this.m_ShadowBlur);
        }
    }

    public void Show()
    {
        this.m_Hidden = false;
        this.UpdateText();
    }

    private void Start()
    {
        this.m_updated = false;
    }

    public static void StoreCachedData()
    {
        CreateCacheFolder();
        string cacheFilePath = GetCacheFilePath();
        using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(cacheFilePath, FileMode.Create)))
        {
            int num = 2;
            num = 0x2acc;
            writer.Write(num);
            foreach (KeyValuePair<int, CachedTextValues> pair in s_CachedText)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value.m_Text);
                writer.Write(pair.Value.m_CharSize);
                writer.Write(pair.Value.m_OriginalTextHash);
            }
        }
        Log.Kyle.Print("UberText Cache Stored: " + cacheFilePath, new object[0]);
    }

    public Vector2 TexelSize(Texture texture)
    {
        int frameCount = UnityEngine.Time.frameCount;
        Font key = this.m_Font;
        if (this.m_LocalizedFont != null)
        {
            key = this.m_LocalizedFont;
        }
        if (s_TexelUpdateFrame.ContainsKey(key) && (s_TexelUpdateFrame[key] == frameCount))
        {
            return s_TexelUpdateData[key];
        }
        Vector2 vector = new Vector2 {
            x = 1f / ((float) texture.width),
            y = 1f / ((float) texture.height)
        };
        s_TexelUpdateFrame[key] = frameCount;
        s_TexelUpdateData[key] = vector;
        return vector;
    }

    private int TextEffectsMaterial(TextRenderMaterial materialKey, Material material)
    {
        if (!this.m_TextMaterialIndices.ContainsKey(materialKey))
        {
            Material[] array = new Material[this.m_TextMesh.GetComponent<Renderer>().sharedMaterials.Length + 1];
            int index = array.Length - 1;
            this.m_TextMesh.GetComponent<Renderer>().sharedMaterials.CopyTo(array, 0);
            array[index] = material;
            this.m_TextMesh.GetComponent<Renderer>().sharedMaterials = array;
            this.m_TextMaterialIndices.Add(materialKey, index);
            return index;
        }
        return this.m_TextMaterialIndices[materialKey];
    }

    private void Update()
    {
        if ((this.m_RenderToTexture && (this.m_TextTexture != null)) && !this.m_TextTexture.IsCreated())
        {
            Log.Kyle.Print("UberText Texture lost 1. UpdateText Called", new object[0]);
            this.m_updated = false;
            this.RenderText();
        }
        else
        {
            this.RenderText();
            this.UpdateTexelSize();
        }
    }

    private void UpdateColor()
    {
        if (this.m_Outline)
        {
            if (this.m_OutlineTextMaterial != null)
            {
                this.m_OutlineTextMaterial.SetColor("_Color", this.m_TextColor);
                this.m_OutlineTextMaterial.SetColor("_OutlineColor", this.m_OutlineColor);
                this.m_OutlineTextMaterial.SetFloat("_LightingBlend", this.m_AmbientLightBlend);
            }
            if (this.m_BoldMaterial != null)
            {
                this.m_BoldMaterial.SetColor("_Color", this.m_TextColor);
                this.m_BoldMaterial.SetColor("_OutlineColor", this.m_OutlineColor);
                this.m_BoldMaterial.SetFloat("_LightingBlend", this.m_AmbientLightBlend);
            }
        }
        else
        {
            if (this.m_TextMaterial != null)
            {
                this.m_TextMaterial.SetColor("_Color", this.m_TextColor);
                this.m_TextMaterial.SetFloat("_LightingBlend", this.m_AmbientLightBlend);
            }
            if (this.m_BoldMaterial != null)
            {
                this.m_BoldMaterial.SetColor("_Color", this.m_TextColor);
                this.m_BoldMaterial.SetFloat("_LightingBlend", this.m_AmbientLightBlend);
            }
        }
        if (this.m_Shadow && (this.m_ShadowMaterial != null))
        {
            this.m_ShadowMaterial.SetColor("_Color", this.m_ShadowColor);
        }
        if (this.m_PlaneMesh != null)
        {
            this.m_PlaneMesh.colors = new Color[] { this.m_GradientLowerColor, this.m_GradientLowerColor, this.m_GradientUpperColor, this.m_GradientUpperColor };
        }
    }

    private void UpdateEditorText()
    {
    }

    private void UpdateFontTextures()
    {
        if (this.m_TextMaterial != null)
        {
            this.m_TextMaterial.mainTexture = this.m_FontTexture;
        }
        if (this.m_OutlineTextMaterial != null)
        {
            this.m_OutlineTextMaterial.mainTexture = this.m_FontTexture;
        }
        if (this.m_BoldMaterial != null)
        {
            this.m_BoldMaterial.mainTexture = this.m_FontTexture;
        }
    }

    private void UpdateLayers()
    {
        if (this.m_RenderToTexture)
        {
            this.m_TextMeshGameObject.layer = 0;
            if (this.m_PlaneGameObject != null)
            {
                this.m_PlaneGameObject.layer = base.gameObject.layer;
            }
        }
        else if (this.m_TextMeshGameObject != null)
        {
            this.m_TextMeshGameObject.layer = base.gameObject.layer;
        }
        if (this.m_Shadow && (this.m_ShadowPlaneGameObject != null))
        {
            this.m_ShadowPlaneGameObject.layer = base.gameObject.layer;
        }
    }

    public void UpdateNow()
    {
        if (base.gameObject.activeInHierarchy)
        {
            this.m_updated = false;
            this.RenderText();
        }
    }

    private void UpdateRenderQueue()
    {
        GameObject renderOnObject;
        if (this.m_RenderToTexture)
        {
            if (this.m_RenderOnObject != null)
            {
                renderOnObject = this.m_RenderOnObject;
            }
            else
            {
                renderOnObject = this.m_PlaneGameObject;
            }
        }
        else
        {
            renderOnObject = this.m_TextMeshGameObject;
        }
        if (renderOnObject != null)
        {
            if (this.m_OrgRenderQueue == -9999)
            {
                this.m_OrgRenderQueue = renderOnObject.GetComponent<Renderer>().sharedMaterial.renderQueue;
            }
            foreach (Material material in renderOnObject.GetComponent<Renderer>().sharedMaterials)
            {
                material.renderQueue = this.m_OrgRenderQueue + this.m_RenderQueue;
            }
            if (this.m_Shadow && (this.m_ShadowPlaneGameObject != null))
            {
                this.m_ShadowPlaneGameObject.GetComponent<Renderer>().sharedMaterial.renderQueue = renderOnObject.GetComponent<Renderer>().sharedMaterial.renderQueue - 1;
            }
        }
    }

    private void UpdateTexelSize()
    {
        if (this.m_FontTexture == null)
        {
            this.m_FontTexture = this.GetFontTexture();
        }
        if (this.m_FontTexture == null)
        {
            Debug.LogError(string.Format("UberText.UpdateTexelSize() - m_FontTexture == null!  text={0}", this.m_Text));
        }
        else
        {
            Vector2 vector = this.TexelSize(this.m_FontTexture);
            if (vector != this.m_PreviousTexelSize)
            {
                if (this.m_BoldMaterial != null)
                {
                    this.m_BoldMaterial.SetFloat("_BoldOffsetX", this.m_BoldSize * vector.x);
                    this.m_BoldMaterial.SetFloat("_BoldOffsetY", this.m_BoldSize * vector.y);
                }
                if (this.m_Outline && !this.m_RenderToTexture)
                {
                    if (this.m_OutlineTextMaterial != null)
                    {
                        this.m_OutlineTextMaterial.SetFloat("_OutlineOffsetX", vector.x * this.m_OutlineSize);
                        this.m_OutlineTextMaterial.SetFloat("_OutlineOffsetY", vector.y * this.m_OutlineSize);
                        this.m_OutlineTextMaterial.SetFloat("_TexelSizeX", vector.x);
                        this.m_OutlineTextMaterial.SetFloat("_TexelSizeY", vector.y);
                    }
                    if (this.m_BoldMaterial != null)
                    {
                        this.m_BoldMaterial.SetFloat("_BoldOffsetX", this.m_BoldSize * vector.x);
                        this.m_BoldMaterial.SetFloat("_BoldOffsetY", this.m_BoldSize * vector.y);
                        this.m_BoldMaterial.SetFloat("_OutlineOffsetX", vector.x * this.m_OutlineSize);
                        this.m_BoldMaterial.SetFloat("_OutlineOffsetY", vector.y * this.m_OutlineSize);
                    }
                }
                if ((this.m_Shadow && this.m_RenderToTexture) && (this.m_ShadowMaterial != null))
                {
                    this.m_ShadowMaterial.SetFloat("_OffsetX", vector.x * this.m_ShadowBlur);
                    this.m_ShadowMaterial.SetFloat("_OffsetY", vector.y * this.m_ShadowBlur);
                }
                if ((this.m_AntiAlias && this.m_RenderToTexture) && (this.m_TextAntialiasingMaterial != null))
                {
                    this.m_TextAntialiasingMaterial.SetFloat("_OffsetX", vector.x * this.m_AntiAliasAmount);
                    this.m_TextAntialiasingMaterial.SetFloat("_OffsetY", vector.y * this.m_AntiAliasAmount);
                }
                this.m_PreviousTexelSize = vector;
            }
        }
    }

    public void UpdateText()
    {
        if (base.gameObject.activeInHierarchy)
        {
            this.m_updated = false;
        }
    }

    private void UpdateTextMesh()
    {
        string text = string.Empty;
        bool flag = false;
        bool flag2 = false;
        if (!flag)
        {
            this.m_CacheHash = new CachedTextKeyData { m_Text = this.m_Text, m_CharSize = this.m_CharacterSize, m_Font = this.m_Font, m_FontSize = this.m_FontSize, m_Height = this.m_Height, m_Width = this.m_Width, m_LineSpacing = this.m_LineSpacing }.GetHashCode();
            if ((this.m_Cache && (this.m_WordWrap || this.m_ResizeToFit)) && s_CachedText.ContainsKey(this.m_CacheHash))
            {
                CachedTextValues values = s_CachedText[this.m_CacheHash];
                if (values.m_OriginalTextHash == this.m_Text.GetHashCode())
                {
                    text = values.m_Text;
                    this.SetText(text);
                    this.SetActualCharacterSize(values.m_CharSize);
                    flag2 = true;
                }
            }
        }
        Quaternion rotation = base.transform.rotation;
        base.transform.rotation = Quaternion.identity;
        if (!flag2)
        {
            string str2 = this.m_Text;
            text = string.Empty;
            if (this.m_GameStringLookup)
            {
                str2 = GameStrings.Get(str2.Trim());
            }
            if (Localization.GetLocale() != Locale.enUS)
            {
                str2 = this.LocalizationFixes(str2);
            }
            text = this.ProcessText(str2);
            this.m_Words = this.BreakStringIntoWords(text);
            this.m_LineCount = LineCount(text);
            this.m_Ellipsized = false;
            if (this.m_WordWrap && !this.m_ResizeToFit)
            {
                this.SetText(this.WordWrapString(text, this.m_WorldWidth));
            }
            else
            {
                this.SetText(text);
            }
            this.SetActualCharacterSize(this.m_CharacterSize * 0.01f);
        }
        this.m_TextMesh.GetComponent<Renderer>().enabled = true;
        this.SetFont(this.m_Font);
        this.SetFontSize(this.m_FontSize);
        this.SetLineSpacing(this.m_LineSpacing);
        switch (this.m_Alignment)
        {
            case AlignmentOptions.Left:
                this.m_TextMesh.alignment = TextAlignment.Left;
                switch (this.m_Anchor)
                {
                    case AnchorOptions.Upper:
                        this.m_TextMesh.transform.localPosition = new Vector3(-this.m_Width * 0.5f, this.m_Height * 0.5f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.UpperLeft;
                        goto Label_04E0;

                    case AnchorOptions.Middle:
                        this.m_TextMesh.transform.localPosition = new Vector3(-this.m_Width * 0.5f, 0f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.MiddleLeft;
                        goto Label_04E0;

                    case AnchorOptions.Lower:
                        this.m_TextMesh.transform.localPosition = new Vector3(-this.m_Width * 0.5f, -this.m_Height * 0.5f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.LowerLeft;
                        goto Label_04E0;
                }
                break;

            case AlignmentOptions.Center:
                this.m_TextMesh.alignment = TextAlignment.Center;
                switch (this.m_Anchor)
                {
                    case AnchorOptions.Upper:
                        this.m_TextMesh.transform.localPosition = new Vector3(0f, this.m_Height * 0.5f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.UpperCenter;
                        goto Label_04E0;

                    case AnchorOptions.Middle:
                        this.m_TextMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.MiddleCenter;
                        goto Label_04E0;

                    case AnchorOptions.Lower:
                        this.m_TextMesh.transform.localPosition = new Vector3(0f, -this.m_Height * 0.5f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.LowerCenter;
                        goto Label_04E0;
                }
                break;

            case AlignmentOptions.Right:
                this.m_TextMesh.alignment = TextAlignment.Right;
                switch (this.m_Anchor)
                {
                    case AnchorOptions.Upper:
                        this.m_TextMesh.transform.localPosition = new Vector3(this.m_Width * 0.5f, this.m_Height * 0.5f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.UpperRight;
                        goto Label_04E0;

                    case AnchorOptions.Middle:
                        this.m_TextMesh.transform.localPosition = new Vector3(this.m_Width * 0.5f, 0f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.MiddleRight;
                        goto Label_04E0;

                    case AnchorOptions.Lower:
                        this.m_TextMesh.transform.localPosition = new Vector3(this.m_Width * 0.5f, -this.m_Height * 0.5f, 0f);
                        this.m_TextMesh.anchor = TextAnchor.LowerRight;
                        goto Label_04E0;
                }
                break;
        }
    Label_04E0:
        if (this.m_ResizeToFit && !flag2)
        {
            this.ResizeTextToFit(text);
        }
        base.transform.rotation = rotation;
        if (((!flag && this.m_Cache) && !flag2) && (this.m_WordWrap || this.m_ResizeToFit))
        {
            double result = 0.0;
            if (!double.TryParse(this.m_Text, out result) && (this.m_Text.Length > 3))
            {
                s_CachedText[this.m_CacheHash] = new CachedTextValues();
                s_CachedText[this.m_CacheHash].m_Text = this.m_TextMesh.text;
                s_CachedText[this.m_CacheHash].m_CharSize = this.m_TextMesh.characterSize;
                s_CachedText[this.m_CacheHash].m_OriginalTextHash = this.m_Text.GetHashCode();
            }
        }
        if (this.m_LocalizedSettings != null)
        {
            LocalizationSettings.LocaleAdjustment locale = this.m_LocalizedSettings.GetLocale(Localization.GetLocale());
            if (locale != null)
            {
                Transform transform = this.m_TextMesh.transform;
                transform.localPosition += locale.m_PositionOffset;
            }
        }
    }

    private string WordWrapString(string text, float width)
    {
        return this.WordWrapString(text, width, false);
    }

    private string WordWrapString(string text, float width, bool ellipsis)
    {
        if ((text == null) || (text == string.Empty))
        {
            return string.Empty;
        }
        float num = width;
        float underwearHeight = 0f;
        float underwearWidth = 0f;
        if (this.m_Underwear)
        {
            underwearHeight = this.m_UnderwearHeight;
            underwearWidth = this.m_UnderwearWidth;
            if (this.m_LocalizedSettings != null)
            {
                LocalizationSettings.LocaleAdjustment locale = this.m_LocalizedSettings.GetLocale(Localization.GetLocale());
                if (locale != null)
                {
                    underwearWidth = locale.m_UnderwearWidth;
                    underwearHeight = locale.m_UnderwearHeight;
                }
            }
            if (this.m_UnderwearFlip)
            {
                underwearHeight = this.m_Height * underwearHeight;
            }
            else
            {
                underwearHeight = this.m_Height * (1f - underwearHeight);
            }
            underwearWidth = width * (1f - underwearWidth);
        }
        Quaternion rotation = this.m_TextMeshGameObject.transform.rotation;
        this.m_TextMeshGameObject.transform.rotation = Quaternion.identity;
        StringBuilder builder = new StringBuilder();
        StringBuilder builder2 = new StringBuilder();
        string[] words = this.m_Words;
        if (words == null)
        {
            return text;
        }
        if ((!this.m_ResizeToFit || ellipsis) && this.m_ForceWrapLargeWords)
        {
            List<string> list = new List<string>();
            foreach (string str in words)
            {
                this.SetText(str);
                float x = this.m_TextMesh.GetComponent<Renderer>().bounds.size.x;
                if (x < width)
                {
                    list.Add(str);
                }
                else
                {
                    int num6 = Mathf.CeilToInt(x / width);
                    int start = 0;
                    int end = 1;
                    for (int i = 0; i < num6; i++)
                    {
                        this.SetText(str.Slice(start, end));
                        while ((this.m_TextMesh.GetComponent<Renderer>().bounds.size.x < width) && (end < str.Length))
                        {
                            end++;
                            this.SetText(str.Slice(start, end));
                        }
                        list.Add(str.Slice(start, end - 1));
                        start = end - 1;
                    }
                    list.Add(str.Slice(start, str.Length));
                }
            }
            words = list.ToArray();
        }
        int num10 = 0;
        if (text.Contains("\n"))
        {
            foreach (char ch in text)
            {
                if (ch == '\n')
                {
                    num10++;
                }
            }
        }
        bool flag = false;
        if (this.m_Underwear && !this.m_UnderwearFlip)
        {
            flag = true;
        }
        if (this.m_Underwear && this.m_UnderwearFlip)
        {
            StringBuilder builder3 = new StringBuilder();
            StringBuilder builder4 = new StringBuilder();
            foreach (string str3 in words)
            {
                string str4 = str3;
                if (str3.Contains("<"))
                {
                    str4 = this.RemoveTagsFromWord(str3);
                }
                builder4.Append(str4);
                string str5 = builder4.ToString();
                if (str5 == null)
                {
                    Debug.LogWarning("UberText: actualLine is null in WordWrapString!");
                    str5 = string.Empty;
                }
                this.SetText(str5);
                if (this.m_TextMesh.GetComponent<Renderer>().bounds.size.x < width)
                {
                    builder3.Append(str3);
                }
                else
                {
                    flag = true;
                    break;
                }
            }
            if (builder3.ToString().Contains("\n"))
            {
                flag = true;
            }
        }
        foreach (string str6 in words)
        {
            string str7 = str6;
            if (str6.Contains("<"))
            {
                str7 = this.RemoveTagsFromWord(str6);
            }
            builder2.Append(str7);
            string str8 = builder2.ToString();
            if (str8 == null)
            {
                Debug.LogWarning("UberText: actualLine is null in WordWrapString!");
                str8 = string.Empty;
            }
            this.SetText(str8);
            float num15 = this.m_TextMesh.GetComponent<Renderer>().bounds.size.x;
            if (this.m_Underwear && flag)
            {
                this.SetText(builder.ToString());
                float y = this.m_TextMesh.GetComponent<Renderer>().bounds.size.y;
                if (this.m_UnderwearFlip)
                {
                    if ((y - ((this.m_Height - y) * 0.2f)) < underwearHeight)
                    {
                        width = underwearWidth;
                    }
                    else
                    {
                        width = num;
                    }
                }
                else if (y > underwearHeight)
                {
                    width = underwearWidth;
                }
            }
            if (num15 < width)
            {
                builder.Append(str6);
            }
            else
            {
                if (ellipsis)
                {
                    this.SetText(builder.ToString() + '\n');
                    if (this.m_TextMesh.GetComponent<Renderer>().bounds.size.y > this.m_Height)
                    {
                        this.m_Ellipsized = true;
                        builder.Append(" ...");
                        break;
                    }
                }
                num10++;
                builder.Append('\n');
                char[] trimChars = new char[] { ' ' };
                builder.Append(str6.TrimStart(trimChars));
                builder2 = new StringBuilder();
                for (int j = 0; j < this.m_LineCount; j++)
                {
                    builder2.Append("\n");
                }
                builder2.Append(str7);
            }
        }
        this.m_TextMeshGameObject.transform.rotation = rotation;
        string s = builder.ToString();
        if (s == null)
        {
            Debug.LogWarning("UberText: Word Wrap returned a null string!");
            s = string.Empty;
        }
        this.m_LineCount = LineCount(s);
        return s;
    }

    [CustomEditField(Sections="Alignment", Label="Enable")]
    public AlignmentOptions Alignment
    {
        get
        {
            return this.m_Alignment;
        }
        set
        {
            if (value != this.m_Alignment)
            {
                this.m_Alignment = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Render")]
    public float AmbientLightBlend
    {
        get
        {
            return this.m_AmbientLightBlend;
        }
        set
        {
            if (value != this.m_AmbientLightBlend)
            {
                this.m_AmbientLightBlend = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="Alignment")]
    public AnchorOptions Anchor
    {
        get
        {
            return this.m_Anchor;
        }
        set
        {
            if (value != this.m_Anchor)
            {
                this.m_Anchor = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="RenderToTexture")]
    public bool AntiAlias
    {
        get
        {
            return this.m_AntiAlias;
        }
        set
        {
            if (value != this.m_AntiAlias)
            {
                this.m_AntiAlias = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="AntiAlias")]
    public float AntiAliasAmount
    {
        get
        {
            return this.m_AntiAliasAmount;
        }
        set
        {
            if (value != this.m_AntiAliasAmount)
            {
                this.m_AntiAliasAmount = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="AntiAlias")]
    public float AntiAliasEdge
    {
        get
        {
            return this.m_AntiAliasEdge;
        }
        set
        {
            if (value != this.m_AntiAliasEdge)
            {
                this.m_AntiAliasEdge = value;
                this.UpdateText();
            }
        }
    }

    protected Material BoldMaterial
    {
        get
        {
            if (this.m_BoldMaterial == null)
            {
                if (this.m_BoldShader == null)
                {
                    this.m_BoldShader = ShaderUtils.FindShader(this.BOLD_SHADER_NAME);
                    if (this.m_BoldShader == null)
                    {
                        Debug.LogError("UberText Failed to load Shader: " + this.BOLD_SHADER_NAME);
                    }
                }
                this.m_BoldMaterial = new Material(this.m_BoldShader);
                SceneUtils.SetHideFlags(this.m_BoldMaterial, HideFlags.DontSave);
            }
            return this.m_BoldMaterial;
        }
    }

    [CustomEditField(Sections="Style")]
    public float BoldSize
    {
        get
        {
            return this.m_BoldSize;
        }
        set
        {
            if (value != this.m_BoldSize)
            {
                this.m_BoldSize = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Text")]
    public bool Cache
    {
        get
        {
            return this.m_Cache;
        }
        set
        {
            this.m_Cache = value;
        }
    }

    [CustomEditField(Sections="Style")]
    public float CharacterSize
    {
        get
        {
            return this.m_CharacterSize;
        }
        set
        {
            if (value != this.m_CharacterSize)
            {
                this.m_CharacterSize = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Style")]
    public int FontSize
    {
        get
        {
            return this.m_FontSize;
        }
        set
        {
            if (value != this.m_FontSize)
            {
                this.m_FontSize = value;
                if (this.m_FontSize < 1)
                {
                    this.m_FontSize = 1;
                }
                if (this.m_FontSize > 120)
                {
                    this.m_FontSize = 120;
                }
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Paragraph")]
    public bool ForceWrapLargeWords
    {
        get
        {
            return this.m_ForceWrapLargeWords;
        }
        set
        {
            if (value != this.m_ForceWrapLargeWords)
            {
                this.m_ForceWrapLargeWords = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Text")]
    public bool GameStringLookup
    {
        get
        {
            return this.m_GameStringLookup;
        }
        set
        {
            if (value != this.m_GameStringLookup)
            {
                this.m_GameStringLookup = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Hide=true)]
    public float GradientLowerAlpha
    {
        get
        {
            return this.m_GradientLowerColor.a;
        }
        set
        {
            if (value != this.m_GradientLowerColor.a)
            {
                this.m_GradientLowerColor.a = value;
                this.UpdateColor();
            }
        }
    }

    [CustomEditField(Parent="RenderToTexture")]
    public Color GradientLowerColor
    {
        get
        {
            return this.m_GradientLowerColor;
        }
        set
        {
            if (value != this.m_GradientLowerColor)
            {
                this.m_GradientLowerColor = value;
                this.UpdateColor();
            }
        }
    }

    [CustomEditField(Hide=true)]
    public float GradientUpperAlpha
    {
        get
        {
            return this.m_GradientUpperColor.a;
        }
        set
        {
            if (value != this.m_GradientUpperColor.a)
            {
                this.m_GradientUpperColor.a = value;
                this.UpdateColor();
            }
        }
    }

    [CustomEditField(Parent="RenderToTexture")]
    public Color GradientUpperColor
    {
        get
        {
            return this.m_GradientUpperColor;
        }
        set
        {
            if (value != this.m_GradientUpperColor)
            {
                this.m_GradientUpperColor = value;
                this.UpdateColor();
            }
        }
    }

    [CustomEditField(Sections="Size")]
    public float Height
    {
        get
        {
            return this.m_Height;
        }
        set
        {
            if (value != this.m_Height)
            {
                this.m_Height = value;
                if (this.m_Height < 0.01f)
                {
                    this.m_Height = 0.01f;
                }
                this.UpdateText();
            }
        }
    }

    protected Material InlineImageMaterial
    {
        get
        {
            if (this.m_InlineImageMaterial == null)
            {
                if (this.m_InlineImageShader == null)
                {
                    this.m_InlineImageShader = ShaderUtils.FindShader(this.INLINE_IMAGE_SHADER_NAME);
                    if (this.m_InlineImageShader == null)
                    {
                        Debug.LogError("UberText Failed to load Shader: " + this.INLINE_IMAGE_SHADER_NAME);
                    }
                }
                this.m_InlineImageMaterial = new Material(this.m_InlineImageShader);
            }
            return this.m_InlineImageMaterial;
        }
    }

    [CustomEditField(Sections="Size")]
    public float LineSpacing
    {
        get
        {
            return this.m_LineSpacing;
        }
        set
        {
            if (value != this.m_LineSpacing)
            {
                this.m_LineSpacing = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Localization")]
    public LocalizationSettings LocalizeSettings
    {
        get
        {
            return this.m_LocalizedSettings;
        }
        set
        {
            this.m_LocalizedSettings = value;
            this.UpdateText();
        }
    }

    [CustomEditField(Sections="Style")]
    public float MinCharacterSize
    {
        get
        {
            return this.m_MinCharacterSize;
        }
        set
        {
            if (value != this.m_MinCharacterSize)
            {
                this.m_MinCharacterSize = value;
                if (this.m_MinCharacterSize < 0.001f)
                {
                    this.m_MinCharacterSize = 0.001f;
                }
                if (this.m_MinCharacterSize > this.m_CharacterSize)
                {
                    this.m_MinCharacterSize = this.m_CharacterSize;
                }
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Style")]
    public int MinFontSize
    {
        get
        {
            return this.m_MinFontSize;
        }
        set
        {
            if (value != this.m_MinFontSize)
            {
                this.m_MinFontSize = value;
                if (this.m_MinFontSize < 1)
                {
                    this.m_MinFontSize = 1;
                }
                if (this.m_MinFontSize > this.m_FontSize)
                {
                    this.m_MinFontSize = this.m_FontSize;
                }
                this.UpdateText();
            }
        }
    }

    protected float Offset
    {
        get
        {
            if (this.m_Offset == 0f)
            {
                s_offset -= 100f;
                this.m_Offset = s_offset;
            }
            return this.m_Offset;
        }
    }

    [CustomEditField(Sections="Outline", Label="Enable")]
    public bool Outline
    {
        get
        {
            return this.m_Outline;
        }
        set
        {
            if (value != this.m_Outline)
            {
                this.m_Outline = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Hide=true)]
    public float OutlineAlpha
    {
        get
        {
            return this.m_OutlineColor.a;
        }
        set
        {
            if (value != this.m_OutlineColor.a)
            {
                this.m_OutlineColor.a = value;
                this.UpdateColor();
            }
        }
    }

    [CustomEditField(Parent="Outline", Label="Color")]
    public Color OutlineColor
    {
        get
        {
            return this.m_OutlineColor;
        }
        set
        {
            if (value != this.m_OutlineColor)
            {
                this.m_OutlineColor = value;
                this.UpdateColor();
            }
        }
    }

    [CustomEditField(Parent="Outline", Label="Size")]
    public float OutlineSize
    {
        get
        {
            return this.m_OutlineSize;
        }
        set
        {
            if (value != this.m_OutlineSize)
            {
                this.m_OutlineSize = value;
                this.UpdateText();
            }
        }
    }

    protected Material OutlineTextMaterial
    {
        get
        {
            string name = this.OUTLINE_TEXT_SHADER_NAME;
            if (!this.m_RichText)
            {
                name = this.OUTLINE_NO_VERT_COLOR_TEXT_SHADER_NAME;
            }
            if ((this.m_OutlineTextMaterial != null) && (name != this.m_OutlineTextMaterial.shader.name))
            {
                this.m_OutlineTextShader = null;
                UnityEngine.Object.DestroyImmediate(this.m_OutlineTextMaterial);
                this.m_OutlineTextMaterial = null;
            }
            if (this.m_OutlineTextMaterial == null)
            {
                if (this.m_OutlineTextShader == null)
                {
                    this.m_OutlineTextShader = ShaderUtils.FindShader(name);
                    if (this.m_OutlineTextShader == null)
                    {
                        Debug.LogError("UberText Failed to load Shader: " + name);
                    }
                }
                this.m_OutlineTextMaterial = new Material(this.m_OutlineTextShader);
                SceneUtils.SetHideFlags(this.m_OutlineTextMaterial, HideFlags.DontSave);
            }
            return this.m_OutlineTextMaterial;
        }
    }

    protected Material PlaneMaterial
    {
        get
        {
            if (this.m_PlaneMaterial == null)
            {
                if (this.m_PlaneShader == null)
                {
                    this.m_PlaneShader = ShaderUtils.FindShader(this.PLANE_SHADER_NAME);
                    if (this.m_PlaneShader == null)
                    {
                        Debug.LogError("UberText Failed to load Shader: " + this.PLANE_SHADER_NAME);
                    }
                }
                this.m_PlaneMaterial = new Material(this.m_PlaneShader);
                SceneUtils.SetHideFlags(this.m_PlaneMaterial, HideFlags.DontSave);
            }
            return this.m_PlaneMaterial;
        }
    }

    [CustomEditField(Sections="Render/Bake")]
    public GameObject RenderOnObject
    {
        get
        {
            return this.m_RenderOnObject;
        }
        set
        {
            if (value != this.m_RenderOnObject)
            {
                this.m_RenderOnObject = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Render")]
    public int RenderQueue
    {
        get
        {
            return this.m_RenderQueue;
        }
        set
        {
            if (value != this.m_RenderQueue)
            {
                this.m_RenderQueue = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Render/Bake")]
    public bool RenderToTexture
    {
        get
        {
            return this.m_RenderToTexture;
        }
        set
        {
            if (value != this.m_RenderToTexture)
            {
                this.m_RenderToTexture = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Paragraph")]
    public bool ResizeToFit
    {
        get
        {
            return this.m_ResizeToFit;
        }
        set
        {
            if (value != this.m_ResizeToFit)
            {
                this.m_ResizeToFit = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Style")]
    public bool RichText
    {
        get
        {
            return this.m_RichText;
        }
        set
        {
            if (value != this.m_RichText)
            {
                this.m_RichText = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Shadow", Label="Enable")]
    public bool Shadow
    {
        get
        {
            return this.m_Shadow;
        }
        set
        {
            if (value != this.m_Shadow)
            {
                this.m_Shadow = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="Shadow")]
    public float ShadowAlpha
    {
        get
        {
            return this.m_ShadowColor.a;
        }
        set
        {
            if (value != this.m_ShadowColor.a)
            {
                this.m_ShadowColor.a = value;
                this.UpdateColor();
            }
        }
    }

    [CustomEditField(Parent="Shadow")]
    public float ShadowBlur
    {
        get
        {
            return this.m_ShadowBlur;
        }
        set
        {
            if (value != this.m_ShadowBlur)
            {
                this.m_ShadowBlur = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="Shadow")]
    public Color ShadowColor
    {
        get
        {
            return this.m_ShadowColor;
        }
        set
        {
            if (value != this.m_ShadowColor)
            {
                this.m_ShadowColor = value;
                this.UpdateColor();
            }
        }
    }

    protected Material ShadowMaterial
    {
        get
        {
            if (this.m_ShadowMaterial == null)
            {
                if (this.m_ShadowTextShader == null)
                {
                    this.m_ShadowTextShader = ShaderUtils.FindShader(this.SHADOW_SHADER_NAME);
                    if (this.m_ShadowTextShader == null)
                    {
                        Debug.LogError("UberText Failed to load Shader: " + this.SHADOW_SHADER_NAME);
                    }
                }
                this.m_ShadowMaterial = new Material(this.m_ShadowTextShader);
                SceneUtils.SetHideFlags(this.m_ShadowMaterial, HideFlags.DontSave);
            }
            return this.m_ShadowMaterial;
        }
    }

    [CustomEditField(Parent="Shadow")]
    public float ShadowOffset
    {
        get
        {
            return this.m_ShadowOffset;
        }
        set
        {
            if (value != this.m_ShadowOffset)
            {
                this.m_ShadowOffset = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Text", T=EditType.TEXT_AREA)]
    public string Text
    {
        get
        {
            return this.m_Text;
        }
        set
        {
            this.m_TextSet = true;
            this.m_TextSet = true;
            if (value != this.m_Text)
            {
                if (value == null)
                {
                }
                this.m_Text = string.Empty;
                if (<>f__am$cache6E == null)
                {
                    <>f__am$cache6E = c => char.IsSurrogate(c);
                }
                if (Enumerable.Any<char>(this.m_Text, <>f__am$cache6E))
                {
                    if (<>f__am$cache6F == null)
                    {
                        <>f__am$cache6F = c => !char.IsLowSurrogate(c);
                    }
                    if (<>f__am$cache70 == null)
                    {
                        <>f__am$cache70 = c => !char.IsHighSurrogate(c) ? c : ((char) 0xfffd);
                    }
                    IEnumerable<char> source = Enumerable.Select<char, char>(Enumerable.Where<char>(this.m_Text, <>f__am$cache6F), <>f__am$cache70);
                    this.m_Text = new string(source.ToArray<char>());
                }
                if (this.m_Text != this.m_PreviousText)
                {
                    this.UpdateNow();
                }
            }
        }
    }

    [CustomEditField(Hide=true)]
    public float TextAlpha
    {
        get
        {
            return this.m_TextColor.a;
        }
        set
        {
            if (value != this.m_TextColor.a)
            {
                this.m_TextColor.a = value;
                this.UpdateColor();
            }
        }
    }

    protected Material TextAntialiasingMaterial
    {
        get
        {
            if (this.m_TextAntialiasingMaterial == null)
            {
                if (this.m_AntialiasingTextShader == null)
                {
                    this.m_AntialiasingTextShader = ShaderUtils.FindShader(this.TEXT_ANTIALAISING_SHADER_NAME);
                    if (this.m_AntialiasingTextShader == null)
                    {
                        Debug.LogError("UberText Failed to load Shader: " + this.TEXT_ANTIALAISING_SHADER_NAME);
                    }
                }
                this.m_TextAntialiasingMaterial = new Material(this.m_AntialiasingTextShader);
                SceneUtils.SetHideFlags(this.m_TextAntialiasingMaterial, HideFlags.DontSave);
            }
            return this.m_TextAntialiasingMaterial;
        }
    }

    [CustomEditField(Sections="Style")]
    public Color TextColor
    {
        get
        {
            return this.m_TextColor;
        }
        set
        {
            if (value != this.m_TextColor)
            {
                this.m_TextColor = value;
                this.UpdateColor();
            }
        }
    }

    protected Material TextMaterial
    {
        get
        {
            if (this.m_TextMaterial == null)
            {
                if (this.m_TextShader == null)
                {
                    this.m_TextShader = ShaderUtils.FindShader(this.TEXT_SHADER_NAME);
                    if (this.m_TextShader == null)
                    {
                        Debug.LogError("UberText Failed to load Shader: " + this.TEXT_SHADER_NAME);
                    }
                }
                this.m_TextMaterial = new Material(this.m_TextShader);
                SceneUtils.SetHideFlags(this.m_TextMaterial, HideFlags.DontSave);
            }
            return this.m_TextMaterial;
        }
    }

    [CustomEditField(Parent="RenderToTexture")]
    public int TextureResolution
    {
        get
        {
            return this.m_Resolution;
        }
        set
        {
            if (value != this.m_Resolution)
            {
                this.m_Resolution = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Style")]
    public Font TrueTypeFont
    {
        get
        {
            return this.m_Font;
        }
        set
        {
            if (value != this.m_Font)
            {
                this.m_Font = value;
                this.SetFont(this.m_Font);
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Underwear", Label="Enable")]
    public bool Underwear
    {
        get
        {
            return this.m_Underwear;
        }
        set
        {
            if (value != this.m_Underwear)
            {
                this.m_Underwear = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="Underwear", Label="Flip")]
    public bool UnderwearFlip
    {
        get
        {
            return this.m_UnderwearFlip;
        }
        set
        {
            if (value != this.m_UnderwearFlip)
            {
                this.m_UnderwearFlip = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="Underwear", Label="Height")]
    public float UnderwearHeight
    {
        get
        {
            return this.m_UnderwearHeight;
        }
        set
        {
            if (value != this.m_UnderwearHeight)
            {
                this.m_UnderwearHeight = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Parent="Underwear", Label="Width")]
    public float UnderwearWidth
    {
        get
        {
            return this.m_UnderwearWidth;
        }
        set
        {
            if (value != this.m_UnderwearWidth)
            {
                this.m_UnderwearWidth = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Text")]
    public bool UseEditorText
    {
        get
        {
            return this.m_UseEditorText;
        }
        set
        {
            if (value != this.m_UseEditorText)
            {
                this.m_UseEditorText = value;
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Size")]
    public float Width
    {
        get
        {
            return this.m_Width;
        }
        set
        {
            if (value != this.m_Width)
            {
                this.m_Width = value;
                if (this.m_Width < 0.01f)
                {
                    this.m_Width = 0.01f;
                }
                this.UpdateText();
            }
        }
    }

    [CustomEditField(Sections="Paragraph")]
    public bool WordWrap
    {
        get
        {
            return this.m_WordWrap;
        }
        set
        {
            if (value != this.m_WordWrap)
            {
                this.m_WordWrap = value;
                this.UpdateText();
            }
        }
    }

    public enum AlignmentOptions
    {
        Left,
        Center,
        Right
    }

    public enum AnchorOptions
    {
        Upper,
        Middle,
        Lower
    }

    private class CachedTextKeyData
    {
        public float m_CharSize;
        public Font m_Font;
        public int m_FontSize;
        public float m_Height;
        public float m_LineSpacing;
        public string m_Text;
        public float m_Width;

        public override int GetHashCode()
        {
            int num = 0;
            num = (this.m_Text.Length + this.m_FontSize) + this.m_Text.GetHashCode();
            num += this.m_FontSize.GetHashCode();
            num -= this.m_CharSize.GetHashCode();
            num += this.m_Width.GetHashCode();
            num -= this.m_Height.GetHashCode();
            num += this.m_Font.GetHashCode();
            return (num - this.m_LineSpacing.GetHashCode());
        }
    }

    [Serializable]
    private class CachedTextValues
    {
        public float m_CharSize;
        public int m_OriginalTextHash;
        public string m_Text;
    }

    private enum Fonts
    {
        BlizzardGlobal,
        Belwe,
        BelweOutline,
        FranklinGothic
    }

    [Serializable]
    public class LocalizationSettings
    {
        public List<LocaleAdjustment> m_LocaleAdjustments = new List<LocaleAdjustment>();

        public LocaleAdjustment AddLocale(Locale locale)
        {
            LocaleAdjustment item = this.GetLocale(locale);
            if (item == null)
            {
                item = new LocaleAdjustment(locale);
                this.m_LocaleAdjustments.Add(item);
            }
            return item;
        }

        public LocaleAdjustment GetLocale(Locale locale)
        {
            foreach (LocaleAdjustment adjustment in this.m_LocaleAdjustments)
            {
                if (adjustment.m_Locale == locale)
                {
                    return adjustment;
                }
            }
            return null;
        }

        public bool HasLocale(Locale locale)
        {
            return (this.GetLocale(locale) != null);
        }

        public void RemoveLocale(Locale locale)
        {
            for (int i = 0; i < this.m_LocaleAdjustments.Count; i++)
            {
                if (this.m_LocaleAdjustments[i].m_Locale == locale)
                {
                    this.m_LocaleAdjustments.RemoveAt(i);
                    return;
                }
            }
        }

        [Serializable]
        public class LocaleAdjustment
        {
            public float m_FontSizeModifier;
            public float m_LineSpaceModifier;
            public Locale m_Locale;
            public Vector3 m_PositionOffset;
            public float m_SingleLineAdjustment;
            public float m_UnderwearHeight;
            public float m_UnderwearWidth;

            public LocaleAdjustment()
            {
                this.m_LineSpaceModifier = 1f;
                this.m_FontSizeModifier = 1f;
                this.m_PositionOffset = Vector3.zero;
                this.m_Locale = Locale.enUS;
            }

            public LocaleAdjustment(Locale locale)
            {
                this.m_LineSpaceModifier = 1f;
                this.m_FontSizeModifier = 1f;
                this.m_PositionOffset = Vector3.zero;
                this.m_Locale = locale;
            }
        }
    }

    private enum TextRenderMaterial
    {
        Text,
        Bold,
        Outline,
        InlineImages
    }
}

