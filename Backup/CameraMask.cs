using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class CameraMask : MonoBehaviour
{
    [CustomEditField(Sections="Render Camera", Parent="m_UseCameraFromLayer")]
    public GameLayer m_CameraFromLayer;
    [CustomEditField(Sections="Mask Settings")]
    public GameObject m_ClipObjects;
    [CustomEditField(Sections="Render Camera")]
    public List<GameLayer> m_CullingMasks = new List<GameLayer> { 0, 0x13 };
    [CustomEditField(Sections="Mask Settings")]
    public float m_Height = 1f;
    private Camera m_MaskCamera;
    private GameObject m_MaskCameraGameObject;
    [CustomEditField(Sections="Mask Settings")]
    public bool m_RealtimeUpdate;
    private Camera m_RenderCamera;
    [CustomEditField(Sections="Mask Settings")]
    public CAMERA_MASK_UP_VECTOR m_UpVector;
    [CustomEditField(Sections="Render Camera")]
    public bool m_UseCameraFromLayer;
    [CustomEditField(Sections="Mask Settings")]
    public float m_Width = 1f;

    private bool Init()
    {
        if (this.m_MaskCamera != null)
        {
            return false;
        }
        if (this.m_MaskCameraGameObject != null)
        {
            UnityEngine.Object.Destroy(this.m_MaskCameraGameObject);
        }
        this.m_RenderCamera = !this.m_UseCameraFromLayer ? Camera.main : CameraUtils.FindFirstByLayer(this.m_CameraFromLayer);
        if (this.m_RenderCamera == null)
        {
            return false;
        }
        this.m_MaskCameraGameObject = new GameObject("MaskCamera");
        SceneUtils.SetLayer(this.m_MaskCameraGameObject, GameLayer.CameraMask);
        this.m_MaskCameraGameObject.transform.parent = this.m_RenderCamera.gameObject.transform;
        this.m_MaskCameraGameObject.transform.localPosition = Vector3.zero;
        this.m_MaskCameraGameObject.transform.localRotation = Quaternion.identity;
        this.m_MaskCameraGameObject.transform.localScale = Vector3.one;
        int num = GameLayer.CameraMask.LayerBit();
        foreach (GameLayer layer in this.m_CullingMasks)
        {
            num |= layer.LayerBit();
        }
        this.m_MaskCamera = this.m_MaskCameraGameObject.AddComponent<Camera>();
        this.m_MaskCamera.CopyFrom(this.m_RenderCamera);
        this.m_MaskCamera.clearFlags = CameraClearFlags.Nothing;
        this.m_MaskCamera.cullingMask = num;
        this.m_MaskCamera.depth = this.m_RenderCamera.depth + 1f;
        if (this.m_ClipObjects == null)
        {
            this.m_ClipObjects = base.gameObject;
        }
        foreach (Transform transform in this.m_ClipObjects.GetComponentsInChildren<Transform>())
        {
            GameObject gameObject = transform.gameObject;
            if (gameObject != null)
            {
                SceneUtils.SetLayer(gameObject, GameLayer.CameraMask);
            }
        }
        this.UpdateCameraClipping();
        UniversalInputManager.Get().AddCameraMaskCamera(this.m_MaskCamera);
        return true;
    }

    private void OnDisable()
    {
        if ((this.m_MaskCamera != null) && (UniversalInputManager.Get() != null))
        {
            UniversalInputManager.Get().RemoveCameraMaskCamera(this.m_MaskCamera);
        }
        if (this.m_MaskCameraGameObject != null)
        {
            UnityEngine.Object.Destroy(this.m_MaskCameraGameObject);
        }
        this.m_MaskCamera = null;
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 matrixx = new Matrix4x4();
        if (this.m_UpVector == CAMERA_MASK_UP_VECTOR.Z)
        {
            matrixx.SetTRS(base.transform.position, Quaternion.identity, base.transform.lossyScale);
        }
        else
        {
            matrixx.SetTRS(base.transform.position, Quaternion.Euler(90f, 0f, 0f), base.transform.lossyScale);
        }
        Gizmos.matrix = matrixx;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(this.m_Width, this.m_Height, 0f));
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void OnEnable()
    {
        this.Init();
    }

    private void Update()
    {
        if (this.m_RealtimeUpdate)
        {
            this.UpdateCameraClipping();
        }
    }

    private void UpdateCameraClipping()
    {
        if ((this.m_RenderCamera != null) || this.Init())
        {
            Vector3 zero = Vector3.zero;
            Vector3 position = Vector3.zero;
            if (this.m_UpVector == CAMERA_MASK_UP_VECTOR.Y)
            {
                zero = new Vector3(base.transform.position.x - ((this.m_Width * 0.5f) * base.transform.lossyScale.x), base.transform.position.y, base.transform.position.z - ((this.m_Height * 0.5f) * base.transform.lossyScale.z));
                position = new Vector3(base.transform.position.x + ((this.m_Width * 0.5f) * base.transform.lossyScale.x), base.transform.position.y, base.transform.position.z + ((this.m_Height * 0.5f) * base.transform.lossyScale.z));
            }
            else
            {
                zero = new Vector3(base.transform.position.x - ((this.m_Width * 0.5f) * base.transform.lossyScale.x), base.transform.position.y - ((this.m_Height * 0.5f) * base.transform.lossyScale.y), base.transform.position.z);
                position = new Vector3(base.transform.position.x + ((this.m_Width * 0.5f) * base.transform.lossyScale.x), base.transform.position.y + ((this.m_Height * 0.5f) * base.transform.lossyScale.y), base.transform.position.z);
            }
            Vector3 vector3 = this.m_RenderCamera.WorldToViewportPoint(zero);
            Vector3 vector4 = this.m_RenderCamera.WorldToViewportPoint(position);
            if ((vector3.x < 0f) && (vector4.x < 0f))
            {
                if (this.m_MaskCamera.enabled)
                {
                    this.m_MaskCamera.enabled = false;
                }
            }
            else if ((vector3.x > 1f) && (vector4.x > 1f))
            {
                if (this.m_MaskCamera.enabled)
                {
                    this.m_MaskCamera.enabled = false;
                }
            }
            else if ((vector3.y < 0f) && (vector4.y < 0f))
            {
                if (this.m_MaskCamera.enabled)
                {
                    this.m_MaskCamera.enabled = false;
                }
            }
            else if ((vector3.y > 1f) && (vector4.y > 1f))
            {
                if (this.m_MaskCamera.enabled)
                {
                    this.m_MaskCamera.enabled = false;
                }
            }
            else
            {
                if (!this.m_MaskCamera.enabled)
                {
                    this.m_MaskCamera.enabled = true;
                }
                Rect rect = new Rect(vector3.x, vector3.y, vector4.x - vector3.x, vector4.y - vector3.y);
                if (rect.x < 0f)
                {
                    rect.width += rect.x;
                    rect.x = 0f;
                }
                if (rect.y < 0f)
                {
                    rect.height += rect.y;
                    rect.y = 0f;
                }
                if (rect.x > 1f)
                {
                    rect.width -= rect.x;
                    rect.x = 1f;
                }
                if (rect.y > 1f)
                {
                    rect.height -= rect.y;
                    rect.y = 1f;
                }
                rect.width = Mathf.Min(1f - rect.x, rect.width);
                rect.height = Mathf.Min(1f - rect.y, rect.height);
                this.m_MaskCamera.rect = new Rect(0f, 0f, 1f, 1f);
                this.m_MaskCamera.ResetProjectionMatrix();
                Matrix4x4 projectionMatrix = this.m_MaskCamera.projectionMatrix;
                this.m_MaskCamera.rect = rect;
                this.m_MaskCamera.projectionMatrix = (Matrix4x4.TRS(new Vector3((-rect.x * 2f) / rect.width, (-rect.y * 2f) / rect.height, 0f), Quaternion.identity, Vector3.one) * Matrix4x4.TRS(new Vector3((1f / rect.width) - 1f, (1f / rect.height) - 1f, 0f), Quaternion.identity, new Vector3(1f / rect.width, 1f / rect.height, 1f))) * projectionMatrix;
            }
        }
    }

    [ContextMenu("UpdateMask")]
    public void UpdateMask()
    {
        this.UpdateCameraClipping();
    }

    public enum CAMERA_MASK_UP_VECTOR
    {
        Y,
        Z
    }
}

