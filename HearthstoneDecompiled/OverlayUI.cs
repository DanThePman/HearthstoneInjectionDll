using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class OverlayUI : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<GameObject> <>f__am$cache6;
    private HashSet<GameObject> m_destroyOnSceneLoad = new HashSet<GameObject>();
    public CanvasAnchors m_heightScale;
    public Camera m_PerspectiveUICamera;
    public Camera m_UICamera;
    public CanvasAnchors m_widthScale;
    private static OverlayUI s_instance;

    public void AddGameObject(GameObject go, CanvasAnchor anchor = 0, bool destroyOnSceneLoad = false, CanvasScaleMode scaleMode = 1)
    {
        CanvasAnchors anchors = (scaleMode != CanvasScaleMode.HEIGHT) ? this.m_widthScale : this.m_heightScale;
        TransformUtil.AttachAndPreserveLocalTransform(go.transform, anchors.GetAnchor(anchor));
        if (destroyOnSceneLoad)
        {
            this.DestroyOnSceneLoad(go);
        }
    }

    private void Awake()
    {
        s_instance = this;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneChange));
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
    }

    public void DestroyOnSceneLoad(GameObject go)
    {
        if (!this.m_destroyOnSceneLoad.Contains(go))
        {
            this.m_destroyOnSceneLoad.Add(go);
        }
    }

    public void DontDestroyOnSceneLoad(GameObject go)
    {
        if (this.m_destroyOnSceneLoad.Contains(go))
        {
            this.m_destroyOnSceneLoad.Remove(go);
        }
    }

    public static OverlayUI Get()
    {
        return s_instance;
    }

    public Vector3 GetRelativePosition(Vector3 worldPosition, Camera camera, Transform bone, float depth = 0f)
    {
        Vector3 position = camera.WorldToScreenPoint(worldPosition);
        Vector3 vector2 = this.m_UICamera.ScreenToWorldPoint(position);
        vector2.y = depth;
        return bone.InverseTransformPoint(vector2);
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    private void OnSceneChange(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (<>f__am$cache6 == null)
        {
            <>f__am$cache6 = delegate (GameObject go) {
                if (go != null)
                {
                    Debug.Log("destroying go " + go.name);
                    UnityEngine.Object.Destroy(go);
                    return true;
                }
                return false;
            };
        }
        this.m_destroyOnSceneLoad.RemoveWhere(<>f__am$cache6);
    }

    private void Start()
    {
        Log.Cameron.Print("loading overlay ui", new object[0]);
        CanvasScaler componentInChildren = base.gameObject.GetComponentInChildren<CanvasScaler>();
        Log.Cameron.Print("canvas scaler component " + componentInChildren, new object[0]);
        if (componentInChildren != null)
        {
            Log.Cameron.Print("canvas scaler values " + componentInChildren.referenceResolution, new object[0]);
            Log.Cameron.Print("object scale values " + componentInChildren.gameObject.transform.localScale, new object[0]);
        }
    }

    private void WillReset()
    {
        this.m_widthScale.WillReset();
        this.m_heightScale.WillReset();
    }
}

