using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShaderPreCompiler : MonoBehaviour
{
    private readonly string[] GOLDEN_UBER_KEYWORDS1 = new string[] { "FX3_ADDBLEND", "FX3_ALPHABLEND" };
    private readonly string[] GOLDEN_UBER_KEYWORDS2 = new string[] { "LAYER3", "FX3_FLOWMAP", "LAYER4" };
    public Shader m_GoldenUberShader;
    public Shader[] m_SceneChangeCompileShaders;
    public Shader[] m_StartupCompileShaders;
    private readonly Vector3[] MESH_NORMALS = new Vector3[] { Vector3.up, Vector3.up, Vector3.up };
    private readonly Vector4[] MESH_TANGENTS = new Vector4[] { new Vector4(1f, 0f, 0f, 0f), new Vector4(1f, 0f, 0f, 0f), new Vector4(1f, 0f, 0f, 0f) };
    private readonly int[] MESH_TRIANGLES;
    private readonly Vector2[] MESH_UVS = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f) };
    private readonly Vector3[] MESH_VERTS = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
    private bool PremiumShadersCompiled;
    protected static Map<string, Shader> s_shaderCache = new Map<string, Shader>();
    private bool SceneChangeShadersCompiled;

    public ShaderPreCompiler()
    {
        int[] numArray1 = new int[3];
        numArray1[0] = 2;
        numArray1[1] = 1;
        this.MESH_TRIANGLES = numArray1;
    }

    private void AddShader(string shaderName, Shader shader)
    {
        if (!s_shaderCache.ContainsKey(shaderName))
        {
            s_shaderCache.Add(shaderName, shader);
        }
    }

    private Material CreateMaterial(string name, Shader shader)
    {
        return new Material(shader) { name = name };
    }

    private GameObject CreateMesh(string name)
    {
        GameObject obj2 = new GameObject {
            name = name
        };
        obj2.transform.parent = base.gameObject.transform;
        obj2.transform.localPosition = Vector3.zero;
        obj2.transform.localRotation = Quaternion.identity;
        obj2.transform.localScale = Vector3.one;
        obj2.AddComponent<MeshFilter>();
        obj2.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh {
            vertices = this.MESH_VERTS,
            uv = this.MESH_UVS,
            normals = this.MESH_NORMALS,
            tangents = this.MESH_TANGENTS,
            triangles = this.MESH_TRIANGLES
        };
        obj2.GetComponent<MeshFilter>().mesh = mesh;
        return obj2;
    }

    public static Shader GetShader(string shaderName)
    {
        Shader shader;
        if (!s_shaderCache.TryGetValue(shaderName, out shader))
        {
            shader = Shader.Find(shaderName);
            if (shader != null)
            {
                s_shaderCache.Add(shaderName, shader);
            }
        }
        return shader;
    }

    private void Start()
    {
        if (GraphicsManager.Get().isVeryLowQualityDevice())
        {
            UnityEngine.Debug.Log("ShaderPreCompiler: Disabled, very low quality mode");
        }
        else
        {
            if (GraphicsManager.Get().RenderQualityLevel != GraphicsQuality.Low)
            {
                base.StartCoroutine(this.WarmupShaders(this.m_StartupCompileShaders));
            }
            SceneMgr.Get().RegisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.WarmupSceneChangeShader));
            this.AddShader(this.m_GoldenUberShader.name, this.m_GoldenUberShader);
            foreach (Shader shader in this.m_StartupCompileShaders)
            {
                if (shader != null)
                {
                    this.AddShader(shader.name, shader);
                }
            }
            foreach (Shader shader2 in this.m_SceneChangeCompileShaders)
            {
                if (shader2 != null)
                {
                    this.AddShader(shader2.name, shader2);
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WarmupGoldenUberShader()
    {
        return new <WarmupGoldenUberShader>c__Iterator28D { <>f__this = this };
    }

    private void WarmupSceneChangeShader(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        if ((((SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY) || (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)) || (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL)) && Network.ShouldBeConnectedToAurora())
        {
            base.StartCoroutine(this.WarmupGoldenUberShader());
            this.PremiumShadersCompiled = true;
        }
        if ((prevMode == SceneMgr.Mode.HUB) && !this.SceneChangeShadersCompiled)
        {
            this.SceneChangeShadersCompiled = true;
            if (GraphicsManager.Get().RenderQualityLevel != GraphicsQuality.Low)
            {
                base.StartCoroutine(this.WarmupShaders(this.m_SceneChangeCompileShaders));
            }
            if (this.SceneChangeShadersCompiled && this.PremiumShadersCompiled)
            {
                SceneMgr.Get().UnregisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.WarmupSceneChangeShader));
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WarmupShaders(Shader[] shaders)
    {
        return new <WarmupShaders>c__Iterator28E { shaders = shaders, <$>shaders = shaders };
    }

    [CompilerGenerated]
    private sealed class <WarmupGoldenUberShader>c__Iterator28D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string[] <$s_1847>__1;
        internal int <$s_1848>__2;
        internal string[] <$s_1849>__4;
        internal int <$s_1850>__5;
        internal ShaderPreCompiler <>f__this;
        internal float <end>__10;
        internal string <kw1>__3;
        internal string <kw2>__6;
        internal float <start>__9;
        internal ShaderVariantCollection.ShaderVariant <sv>__8;
        internal ShaderVariantCollection <svc>__7;
        internal float <totalTime>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<totalTime>__0 = 0f;
                    this.<$s_1847>__1 = this.<>f__this.GOLDEN_UBER_KEYWORDS1;
                    this.<$s_1848>__2 = 0;
                    while (this.<$s_1848>__2 < this.<$s_1847>__1.Length)
                    {
                        this.<kw1>__3 = this.<$s_1847>__1[this.<$s_1848>__2];
                        this.<$s_1849>__4 = this.<>f__this.GOLDEN_UBER_KEYWORDS2;
                        this.<$s_1850>__5 = 0;
                        while (this.<$s_1850>__5 < this.<$s_1849>__4.Length)
                        {
                            this.<kw2>__6 = this.<$s_1849>__4[this.<$s_1850>__5];
                            this.<svc>__7 = new ShaderVariantCollection();
                            this.<sv>__8 = new ShaderVariantCollection.ShaderVariant();
                            this.<sv>__8.shader = this.<>f__this.m_GoldenUberShader;
                            this.<sv>__8.keywords = new string[] { this.<kw1>__3, this.<kw2>__6 };
                            this.<svc>__7.Add(this.<sv>__8);
                            this.<start>__9 = UnityEngine.Time.realtimeSinceStartup;
                            this.<svc>__7.WarmUp();
                            this.<end>__10 = UnityEngine.Time.realtimeSinceStartup;
                            this.<totalTime>__0 += this.<end>__10 - this.<start>__9;
                            object[] args = new object[] { this.<>f__this.m_GoldenUberShader.name, this.<kw1>__3, this.<kw2>__6, this.<end>__10 - this.<start>__9 };
                            Log.Graphics.Print(string.Format("Golden Uber Shader Compile: {0} Keywords: {1}, {2} ({3}s)", args), new object[0]);
                            this.$current = null;
                            this.$PC = 1;
                            return true;
                        Label_0199:
                            this.<$s_1850>__5++;
                        }
                        this.<$s_1848>__2++;
                    }
                    Log.Graphics.Print("Profiling Shader Warmup: " + this.<totalTime>__0, new object[0]);
                    break;

                case 1:
                    goto Label_0199;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WarmupShaders>c__Iterator28E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Shader[] <$>shaders;
        internal Shader[] <$s_1851>__1;
        internal int <$s_1852>__2;
        internal float <end>__7;
        internal Shader <shader>__3;
        internal float <start>__6;
        internal ShaderVariantCollection.ShaderVariant <sv>__5;
        internal ShaderVariantCollection <svc>__4;
        internal float <totalTime>__0;
        internal Shader[] shaders;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<totalTime>__0 = 0f;
                    this.<$s_1851>__1 = this.shaders;
                    this.<$s_1852>__2 = 0;
                    goto Label_013D;

                case 1:
                    break;

                default:
                    goto Label_015C;
            }
        Label_012F:
            this.<$s_1852>__2++;
        Label_013D:
            if (this.<$s_1852>__2 < this.<$s_1851>__1.Length)
            {
                this.<shader>__3 = this.<$s_1851>__1[this.<$s_1852>__2];
                if (this.<shader>__3 != null)
                {
                    this.<svc>__4 = new ShaderVariantCollection();
                    this.<sv>__5 = new ShaderVariantCollection.ShaderVariant();
                    this.<sv>__5.shader = this.<shader>__3;
                    this.<svc>__4.Add(this.<sv>__5);
                    this.<start>__6 = UnityEngine.Time.realtimeSinceStartup;
                    this.<svc>__4.WarmUp();
                    this.<end>__7 = UnityEngine.Time.realtimeSinceStartup;
                    this.<totalTime>__0 += this.<end>__7 - this.<start>__6;
                    Log.Graphics.Print(string.Format("Shader Compile: {0} ({1}s)", this.<shader>__3.name, this.<end>__7 - this.<start>__6), new object[0]);
                    this.$current = null;
                    this.$PC = 1;
                    return true;
                }
                goto Label_012F;
            }
            goto Label_015C;
            this.$PC = -1;
        Label_015C:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

