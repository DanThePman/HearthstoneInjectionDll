using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PageTurn : MonoBehaviour
{
    private readonly string BACK_PAGE_NAME = "PageTurnBack";
    private readonly string FRONT_PAGE_NAME = "PageTurnFront";
    private GameObject m_BackPageGameObject;
    private GameObject m_FrontPageGameObject;
    private Vector3 m_initialPosition;
    public Shader m_MaskShader;
    private GameObject m_MeshGameObject;
    private Camera m_OffscreenPageTurnCamera;
    private GameObject m_OffscreenPageTurnCameraGO;
    private Camera m_OffscreenPageTurnMaskCamera;
    private Bounds m_RenderBounds;
    private float m_RenderOffset = 500f;
    private RenderTexture m_TempMaskBuffer;
    private RenderTexture m_TempRenderBuffer;
    private GameObject m_TheBoxOuterFrame;
    public float m_TurnLeftSpeed = 1.65f;
    public float m_TurnRightSpeed = 1.65f;
    private readonly string PAGE_CENTER_NAME = "PageCenter";
    private readonly string PAGE_TURN_LEFT_ANIM = "PageTurnLeft";
    private readonly string PAGE_TURN_MAT_ANIM = "PageTurnMaterialAnimation";
    private readonly string PAGE_TURN_RIGHT_ANIM = "PageTurnRight";
    private readonly string WAIT_THEN_COMPLETE_PAGE_TURN_COROUTINE = "WaitThenCompletePageTurn";

    private void Awake()
    {
        this.m_initialPosition = base.transform.position;
        Transform transform = base.transform.Find(this.FRONT_PAGE_NAME);
        if (transform != null)
        {
            this.m_FrontPageGameObject = transform.gameObject;
        }
        if (this.m_FrontPageGameObject == null)
        {
            UnityEngine.Debug.LogError("Failed to find " + this.FRONT_PAGE_NAME + " Object.");
        }
        transform = base.transform.Find(this.BACK_PAGE_NAME);
        if (transform != null)
        {
            this.m_BackPageGameObject = transform.gameObject;
        }
        if (this.m_BackPageGameObject == null)
        {
            UnityEngine.Debug.LogError("Failed to find " + this.BACK_PAGE_NAME + " Object.");
        }
        this.Show(false);
        this.m_TheBoxOuterFrame = Box.Get().m_OuterFrame;
        this.CreateCamera();
        this.CreateRenderTexture();
        this.SetupMaterial();
    }

    private void CreateCamera()
    {
        if (this.m_OffscreenPageTurnCameraGO == null)
        {
            if (this.m_OffscreenPageTurnCamera != null)
            {
                UnityEngine.Object.DestroyImmediate(this.m_OffscreenPageTurnCamera);
            }
            this.m_OffscreenPageTurnCameraGO = new GameObject();
            this.m_OffscreenPageTurnCamera = this.m_OffscreenPageTurnCameraGO.AddComponent<Camera>();
            this.m_OffscreenPageTurnCameraGO.name = base.name + "_OffScreenPageTurnCamera";
            this.SetupCamera(this.m_OffscreenPageTurnCamera);
        }
        if (this.m_OffscreenPageTurnMaskCamera == null)
        {
            GameObject obj2 = new GameObject();
            this.m_OffscreenPageTurnMaskCamera = obj2.AddComponent<Camera>();
            obj2.name = base.name + "_OffScreenPageTurnMaskCamera";
            this.SetupCamera(this.m_OffscreenPageTurnMaskCamera);
            this.m_OffscreenPageTurnMaskCamera.SetReplacementShader(this.m_MaskShader, "BasePage");
        }
    }

    private void CreateRenderTexture()
    {
        int width = Screen.currentResolution.width;
        if (width < Screen.currentResolution.height)
        {
            width = Screen.currentResolution.height;
        }
        int num2 = 0x200;
        if (width > 640)
        {
            num2 = 0x400;
        }
        if (width > 0x500)
        {
            num2 = 0x800;
        }
        if (width > 0x9c4)
        {
            num2 = 0x1000;
        }
        GraphicsQuality renderQualityLevel = GraphicsManager.Get().RenderQualityLevel;
        switch (renderQualityLevel)
        {
            case GraphicsQuality.Medium:
                num2 = 0x400;
                break;

            case GraphicsQuality.Low:
                num2 = 0x200;
                break;
        }
        if (this.m_TempRenderBuffer == null)
        {
            if (renderQualityLevel == GraphicsQuality.High)
            {
                this.m_TempRenderBuffer = new RenderTexture(num2, num2, 0x10, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            }
            else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB1555))
            {
                this.m_TempRenderBuffer = new RenderTexture(num2, num2, 0x10, RenderTextureFormat.ARGB1555, RenderTextureReadWrite.Default);
            }
            else if ((renderQualityLevel == GraphicsQuality.Low) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444))
            {
                this.m_TempRenderBuffer = new RenderTexture(num2, num2, 0x10, RenderTextureFormat.ARGB4444, RenderTextureReadWrite.Default);
            }
            else
            {
                this.m_TempRenderBuffer = new RenderTexture(num2, num2, 0x10, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            }
            this.m_TempRenderBuffer.Create();
        }
        if (this.m_TempMaskBuffer == null)
        {
            if (renderQualityLevel == GraphicsQuality.High)
            {
                this.m_TempMaskBuffer = new RenderTexture(num2, num2, 0x10, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            }
            else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB1555))
            {
                this.m_TempMaskBuffer = new RenderTexture(num2, num2, 0x10, RenderTextureFormat.ARGB1555, RenderTextureReadWrite.Default);
            }
            else if ((renderQualityLevel == GraphicsQuality.Low) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444))
            {
                this.m_TempMaskBuffer = new RenderTexture(num2, num2, 0x10, RenderTextureFormat.ARGB4444, RenderTextureReadWrite.Default);
            }
            else
            {
                this.m_TempMaskBuffer = new RenderTexture(num2, num2, 0x10, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            }
            this.m_TempMaskBuffer.Create();
        }
        if (this.m_OffscreenPageTurnCamera != null)
        {
            this.m_OffscreenPageTurnCamera.targetTexture = this.m_TempRenderBuffer;
        }
        if (this.m_OffscreenPageTurnMaskCamera != null)
        {
            this.m_OffscreenPageTurnMaskCamera.targetTexture = this.m_TempMaskBuffer;
        }
    }

    private Transform FindPageCenter(GameObject page)
    {
        Transform transform = page.transform.Find(this.PAGE_CENTER_NAME);
        if (transform == null)
        {
            UnityEngine.Debug.LogError("Failed to find " + this.PAGE_CENTER_NAME + " Object.");
        }
        return transform;
    }

    [DebuggerHidden]
    private IEnumerator FinishTurnLeftPage(PageTurningData pageTurningData)
    {
        return new <FinishTurnLeftPage>c__Iterator52 { <>f__this = this };
    }

    public float GetLeftTurnAnimTime()
    {
        return (base.GetComponent<Animation>()[this.PAGE_TURN_LEFT_ANIM].length / this.m_TurnLeftSpeed);
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

    protected void OnDisable()
    {
        if (this.m_TempRenderBuffer != null)
        {
            UnityEngine.Object.Destroy(this.m_TempRenderBuffer);
        }
        if (this.m_TempMaskBuffer != null)
        {
            UnityEngine.Object.Destroy(this.m_TempMaskBuffer);
        }
        if (this.m_OffscreenPageTurnCameraGO != null)
        {
            UnityEngine.Object.Destroy(this.m_OffscreenPageTurnCameraGO);
        }
        if (this.m_OffscreenPageTurnCamera != null)
        {
            UnityEngine.Object.Destroy(this.m_OffscreenPageTurnCamera);
        }
        if (this.m_OffscreenPageTurnMaskCamera != null)
        {
            UnityEngine.Object.Destroy(this.m_OffscreenPageTurnMaskCamera);
        }
    }

    protected void OnEnable()
    {
        if (this.m_OffscreenPageTurnCameraGO != null)
        {
            this.CreateCamera();
        }
        if ((this.m_TempRenderBuffer != null) || (this.m_TempMaskBuffer != null))
        {
            this.CreateRenderTexture();
            this.SetupMaterial();
        }
    }

    private void Render(GameObject page)
    {
        this.Show(true);
        this.m_FrontPageGameObject.SetActive(true);
        this.m_BackPageGameObject.SetActive(true);
        this.m_OffscreenPageTurnCameraGO.transform.position = base.transform.position;
        bool enabled = this.m_FrontPageGameObject.GetComponent<Renderer>().enabled;
        bool flag2 = this.m_BackPageGameObject.GetComponent<Renderer>().enabled;
        this.m_FrontPageGameObject.GetComponent<Renderer>().enabled = false;
        this.m_BackPageGameObject.GetComponent<Renderer>().enabled = false;
        bool activeSelf = this.m_TheBoxOuterFrame.activeSelf;
        this.m_TheBoxOuterFrame.SetActive(false);
        this.m_OffscreenPageTurnCamera.Render();
        this.m_OffscreenPageTurnMaskCamera.transform.position = base.transform.position;
        this.m_OffscreenPageTurnMaskCamera.RenderWithShader(this.m_MaskShader, "BasePage");
        this.m_FrontPageGameObject.GetComponent<Renderer>().enabled = enabled;
        this.m_BackPageGameObject.GetComponent<Renderer>().enabled = flag2;
        this.m_TheBoxOuterFrame.SetActive(activeSelf);
    }

    private void SetupCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = GetWorldScale(this.m_FrontPageGameObject.transform).x / 2f;
        camera.transform.parent = base.transform;
        camera.nearClipPlane = -20f;
        camera.farClipPlane = 20f;
        camera.depth = (Camera.main != null) ? (Camera.main.depth + 100f) : 0f;
        camera.backgroundColor = Color.black;
        camera.clearFlags = CameraClearFlags.Color;
        camera.cullingMask = GameLayer.Default.LayerBit() | GameLayer.CardRaycast.LayerBit();
        camera.enabled = false;
        camera.renderingPath = RenderingPath.Forward;
        camera.transform.Rotate((float) 90f, 0f, (float) 0f);
        SceneUtils.SetHideFlags(camera, HideFlags.HideAndDontSave);
    }

    private void SetupMaterial()
    {
        Material material = this.m_FrontPageGameObject.GetComponent<Renderer>().material;
        material.mainTexture = this.m_TempRenderBuffer;
        material.SetTexture("_MaskTex", this.m_TempMaskBuffer);
        material.renderQueue = 0xbb9;
        Material material2 = this.m_BackPageGameObject.GetComponent<Renderer>().material;
        material2.SetTexture("_MaskTex", this.m_TempMaskBuffer);
        material2.renderQueue = 0xbba;
    }

    private void Show(bool show)
    {
        base.transform.position = !show ? ((Vector3) (Vector3.right * this.m_RenderOffset)) : this.m_initialPosition;
    }

    public void TurnLeft(GameObject flippingPage, GameObject otherPage)
    {
        this.TurnLeft(flippingPage, otherPage, null);
    }

    public void TurnLeft(GameObject flippingPage, GameObject otherPage, DelOnPageTurnComplete callback)
    {
        this.TurnLeft(flippingPage, otherPage, callback, null);
    }

    public void TurnLeft(GameObject flippingPage, GameObject otherPage, DelOnPageTurnComplete callback, object callbackData)
    {
        TurnPageData data = new TurnPageData {
            flippingPage = flippingPage,
            otherPage = otherPage,
            callback = callback,
            callbackData = callbackData
        };
        base.StopCoroutine("TurnLeftPage");
        base.StartCoroutine("TurnLeftPage", data);
    }

    [DebuggerHidden]
    private IEnumerator TurnLeftPage(TurnPageData pageData)
    {
        return new <TurnLeftPage>c__Iterator51 { pageData = pageData, <$>pageData = pageData, <>f__this = this };
    }

    public void TurnRight(GameObject flippingPage, GameObject otherPage)
    {
        this.TurnRight(flippingPage, otherPage, null);
    }

    public void TurnRight(GameObject flippingPage, GameObject otherPage, DelOnPageTurnComplete callback)
    {
        this.TurnRight(flippingPage, otherPage, callback, null);
    }

    public void TurnRight(GameObject flippingPage, GameObject otherPage, DelOnPageTurnComplete callback, object callbackData)
    {
        this.Render(flippingPage);
        if (GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.Low)
        {
            UnityEngine.Time.captureFramerate = 0x12;
        }
        else if (GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.Medium)
        {
            UnityEngine.Time.captureFramerate = 0x18;
        }
        else
        {
            UnityEngine.Time.captureFramerate = 30;
        }
        base.GetComponent<Animation>().Stop(this.PAGE_TURN_RIGHT_ANIM);
        base.GetComponent<Animation>()[this.PAGE_TURN_RIGHT_ANIM].time = 0f;
        base.GetComponent<Animation>()[this.PAGE_TURN_RIGHT_ANIM].speed = this.m_TurnRightSpeed;
        base.GetComponent<Animation>().Play(this.PAGE_TURN_RIGHT_ANIM);
        this.m_FrontPageGameObject.GetComponent<Renderer>().material.SetFloat("_Alpha", 1f);
        this.m_BackPageGameObject.GetComponent<Renderer>().material.SetFloat("_Alpha", 1f);
        float num = base.GetComponent<Animation>()[this.PAGE_TURN_RIGHT_ANIM].length / this.m_TurnRightSpeed;
        PageTurningData data = new PageTurningData {
            m_secondsToWait = num,
            m_callback = callback,
            m_callbackData = callbackData
        };
        base.StopCoroutine(this.WAIT_THEN_COMPLETE_PAGE_TURN_COROUTINE);
        base.StartCoroutine(this.WAIT_THEN_COMPLETE_PAGE_TURN_COROUTINE, data);
    }

    [DebuggerHidden]
    private IEnumerator WaitThenCompletePageTurn(PageTurningData pageTurningData)
    {
        return new <WaitThenCompletePageTurn>c__Iterator53 { pageTurningData = pageTurningData, <$>pageTurningData = pageTurningData, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <FinishTurnLeftPage>c__Iterator52 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PageTurn <>f__this;

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
                case 1:
                    if (this.<>f__this.GetComponent<Animation>().isPlaying)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    UnityEngine.Time.captureFramerate = 0;
                    this.<>f__this.Show(false);
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
    private sealed class <TurnLeftPage>c__Iterator51 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PageTurn.TurnPageData <$>pageData;
        internal PageTurn <>f__this;
        internal PageTurn.DelOnPageTurnComplete <callback>__2;
        internal object <callbackData>__3;
        internal GameObject <flippingPage>__0;
        internal GameObject <otherPage>__1;
        internal Vector3 <otherPagePos>__5;
        internal Vector3 <pagePos>__4;
        internal PageTurn.PageTurningData <pageTurningData>__7;
        internal float <showNewPage>__6;
        internal PageTurn.TurnPageData pageData;

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
                    this.$current = null;
                    this.$PC = 1;
                    goto Label_03DD;

                case 1:
                    this.$current = null;
                    this.$PC = 2;
                    goto Label_03DD;

                case 2:
                    this.$current = null;
                    this.$PC = 3;
                    goto Label_03DD;

                case 3:
                    this.<flippingPage>__0 = this.pageData.flippingPage;
                    this.<otherPage>__1 = this.pageData.otherPage;
                    this.<callback>__2 = this.pageData.callback;
                    this.<callbackData>__3 = this.pageData.callbackData;
                    this.<pagePos>__4 = this.<flippingPage>__0.transform.position;
                    this.<otherPagePos>__5 = this.<otherPage>__1.transform.position;
                    this.<flippingPage>__0.transform.position = this.<otherPagePos>__5;
                    this.<otherPage>__1.transform.position = this.<pagePos>__4;
                    this.<>f__this.Render(this.<flippingPage>__0);
                    this.<flippingPage>__0.transform.position = this.<pagePos>__4;
                    this.<otherPage>__1.transform.position = this.<otherPagePos>__5;
                    if (GraphicsManager.Get().RenderQualityLevel != GraphicsQuality.Low)
                    {
                        if (GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.Medium)
                        {
                            UnityEngine.Time.captureFramerate = 0x18;
                        }
                        else
                        {
                            UnityEngine.Time.captureFramerate = 30;
                        }
                        break;
                    }
                    UnityEngine.Time.captureFramerate = 0x12;
                    break;

                case 4:
                    goto Label_02FF;

                default:
                    goto Label_03DB;
            }
            this.<>f__this.m_FrontPageGameObject.GetComponent<Renderer>().material.SetFloat("_Alpha", 1f);
            this.<>f__this.m_BackPageGameObject.GetComponent<Renderer>().material.SetFloat("_Alpha", 1f);
            this.<>f__this.GetComponent<Animation>().Stop(this.<>f__this.PAGE_TURN_LEFT_ANIM);
            this.<>f__this.GetComponent<Animation>()[this.<>f__this.PAGE_TURN_LEFT_ANIM].time = 0.22f;
            this.<>f__this.GetComponent<Animation>()[this.<>f__this.PAGE_TURN_LEFT_ANIM].speed = this.<>f__this.m_TurnLeftSpeed;
            this.<>f__this.GetComponent<Animation>().Play(this.<>f__this.PAGE_TURN_LEFT_ANIM);
            this.<>f__this.GetComponent<Animation>().Stop(this.<>f__this.PAGE_TURN_MAT_ANIM);
            this.<>f__this.GetComponent<Animation>()[this.<>f__this.PAGE_TURN_MAT_ANIM].time = 0.22f;
            this.<>f__this.GetComponent<Animation>()[this.<>f__this.PAGE_TURN_MAT_ANIM].speed = this.<>f__this.m_TurnLeftSpeed;
            this.<>f__this.GetComponent<Animation>().Blend(this.<>f__this.PAGE_TURN_MAT_ANIM, 1f, 0f);
            this.<showNewPage>__6 = 0.35f;
        Label_02FF:
            while (this.<>f__this.GetComponent<Animation>()[this.<>f__this.PAGE_TURN_LEFT_ANIM].time < (this.<>f__this.GetComponent<Animation>()[this.<>f__this.PAGE_TURN_LEFT_ANIM].length - this.<showNewPage>__6))
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_03DD;
            }
            PageTurn.PageTurningData data = new PageTurn.PageTurningData {
                m_callback = this.<callback>__2,
                m_callbackData = this.<callbackData>__3,
                m_animation = this.<>f__this.GetComponent<Animation>()[this.<>f__this.PAGE_TURN_LEFT_ANIM]
            };
            this.<pageTurningData>__7 = data;
            this.<>f__this.StartCoroutine(this.<>f__this.FinishTurnLeftPage(this.<pageTurningData>__7));
            if (this.<callbackData>__3 != null)
            {
                this.<pageTurningData>__7.m_callback(this.<callbackData>__3);
                this.$PC = -1;
            }
        Label_03DB:
            return false;
        Label_03DD:
            return true;
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
    private sealed class <WaitThenCompletePageTurn>c__Iterator53 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PageTurn.PageTurningData <$>pageTurningData;
        internal PageTurn <>f__this;
        internal PageTurn.PageTurningData pageTurningData;

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
                    this.$current = new WaitForSeconds(this.pageTurningData.m_secondsToWait);
                    this.$PC = 1;
                    return true;

                case 1:
                    UnityEngine.Time.captureFramerate = 0;
                    this.<>f__this.Show(false);
                    if (this.pageTurningData.m_callback != null)
                    {
                        this.pageTurningData.m_callback(this.pageTurningData.m_callbackData);
                        this.$PC = -1;
                        break;
                    }
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

    public delegate void DelOnPageTurnComplete(object callbackData);

    private class PageTurningData
    {
        public AnimationState m_animation;
        public PageTurn.DelOnPageTurnComplete m_callback;
        public object m_callbackData;
        public float m_secondsToWait;
    }

    private class TurnPageData
    {
        public PageTurn.DelOnPageTurnComplete callback;
        public object callbackData;
        public GameObject flippingPage;
        public GameObject otherPage;
    }
}

