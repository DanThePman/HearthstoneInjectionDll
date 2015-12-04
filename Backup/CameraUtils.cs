using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CameraUtils
{
    public static Plane CreateBottomPlane(Camera camera)
    {
        Vector3 inPoint = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.nearClipPlane));
        Vector3 vector2 = camera.ViewportToWorldPoint(new Vector3(1f, 0f, camera.nearClipPlane));
        Vector3 inNormal = Vector3.Cross(camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.farClipPlane)) - inPoint, vector2 - inPoint);
        inNormal.Normalize();
        return new Plane(inNormal, inPoint);
    }

    public static Rect CreateGUIScreenRect(Camera camera, Component topLeft, Component bottomRight)
    {
        return CreateGUIScreenRect(camera, topLeft.transform.position, bottomRight.transform.position);
    }

    public static Rect CreateGUIScreenRect(Camera camera, Component topLeft, GameObject bottomRight)
    {
        return CreateGUIScreenRect(camera, topLeft.transform.position, bottomRight.transform.position);
    }

    public static Rect CreateGUIScreenRect(Camera camera, GameObject topLeft, Component bottomRight)
    {
        return CreateGUIScreenRect(camera, topLeft.transform.position, bottomRight.transform.position);
    }

    public static Rect CreateGUIScreenRect(Camera camera, GameObject topLeft, GameObject bottomRight)
    {
        return CreateGUIScreenRect(camera, topLeft.transform.position, bottomRight.transform.position);
    }

    public static Rect CreateGUIScreenRect(Camera camera, Vector3 worldTopLeft, Vector3 worldBottomRight)
    {
        Vector3 vector = camera.WorldToScreenPoint(worldTopLeft);
        Vector3 vector2 = camera.WorldToScreenPoint(worldBottomRight);
        return new Rect(vector.x, vector2.y, vector2.x - vector.x, vector.y - vector2.y);
    }

    public static Rect CreateGUIViewportRect(Camera camera, Component topLeft, Component bottomRight)
    {
        return CreateGUIViewportRect(camera, topLeft.transform.position, bottomRight.transform.position);
    }

    public static Rect CreateGUIViewportRect(Camera camera, Component topLeft, GameObject bottomRight)
    {
        return CreateGUIViewportRect(camera, topLeft.transform.position, bottomRight.transform.position);
    }

    public static Rect CreateGUIViewportRect(Camera camera, GameObject topLeft, Component bottomRight)
    {
        return CreateGUIViewportRect(camera, topLeft.transform.position, bottomRight.transform.position);
    }

    public static Rect CreateGUIViewportRect(Camera camera, GameObject topLeft, GameObject bottomRight)
    {
        return CreateGUIViewportRect(camera, topLeft.transform.position, bottomRight.transform.position);
    }

    public static Rect CreateGUIViewportRect(Camera camera, Vector3 worldTopLeft, Vector3 worldBottomRight)
    {
        Vector3 vector = camera.WorldToViewportPoint(worldTopLeft);
        Vector3 vector2 = camera.WorldToViewportPoint(worldBottomRight);
        return new Rect(vector.x, 1f - vector.y, vector2.x - vector.x, vector.y - vector2.y);
    }

    public static GameObject CreateInputBlocker(Camera camera)
    {
        return CreateInputBlocker(camera, string.Empty, null, null, 0f);
    }

    public static GameObject CreateInputBlocker(Camera camera, string name)
    {
        return CreateInputBlocker(camera, name, null, null, 0f);
    }

    public static GameObject CreateInputBlocker(Camera camera, string name, Component parent)
    {
        return CreateInputBlocker(camera, name, parent, parent, 0f);
    }

    public static GameObject CreateInputBlocker(Camera camera, string name, Component parent, float worldOffset)
    {
        return CreateInputBlocker(camera, name, parent, parent, worldOffset);
    }

    public static GameObject CreateInputBlocker(Camera camera, string name, Component parent, Component relative)
    {
        return CreateInputBlocker(camera, name, parent, relative, 0f);
    }

    public static GameObject CreateInputBlocker(Camera camera, string name, Component parent, Component relative, float worldOffset)
    {
        Vector3 one;
        GameObject obj2 = new GameObject(name) {
            layer = camera.gameObject.layer
        };
        obj2.transform.parent = (parent != null) ? parent.transform : null;
        obj2.transform.localScale = Vector3.one;
        obj2.transform.rotation = Quaternion.Inverse(camera.transform.rotation);
        if (relative == null)
        {
            obj2.transform.position = GetPosInFrontOfCamera(camera, (float) (camera.nearClipPlane + worldOffset));
        }
        else
        {
            obj2.transform.position = GetPosInFrontOfCamera(camera, relative.transform.position, worldOffset);
        }
        Bounds farClipBounds = GetFarClipBounds(camera);
        if (parent == null)
        {
            one = Vector3.one;
        }
        else
        {
            one = TransformUtil.ComputeWorldScale(parent);
        }
        Vector3 vector2 = new Vector3 {
            x = farClipBounds.size.x / one.x
        };
        if (farClipBounds.size.z > 0f)
        {
            vector2.y = farClipBounds.size.z / one.z;
        }
        else
        {
            vector2.y = farClipBounds.size.y / one.y;
        }
        obj2.AddComponent<BoxCollider>().size = vector2;
        return obj2;
    }

    public static LayerMask CreateLayerMask(List<Camera> cameras)
    {
        LayerMask mask = 0;
        foreach (Camera camera in cameras)
        {
            mask |= camera.cullingMask;
        }
        return mask;
    }

    public static Plane CreateTopPlane(Camera camera)
    {
        Vector3 inPoint = camera.ViewportToWorldPoint(new Vector3(0f, 1f, camera.nearClipPlane));
        Vector3 vector2 = camera.ViewportToWorldPoint(new Vector3(1f, 1f, camera.nearClipPlane));
        Vector3 inNormal = Vector3.Cross(camera.ViewportToWorldPoint(new Vector3(0f, 1f, camera.farClipPlane)) - inPoint, vector2 - inPoint);
        inNormal.Normalize();
        return new Plane(inNormal, inPoint);
    }

    public static void FindAllByLayer(GameLayer layer, List<Camera> cameras)
    {
        FindAllByLayerMask(layer.LayerBit(), cameras);
    }

    public static void FindAllByLayer(int layer, List<Camera> cameras)
    {
        FindAllByLayerMask(((int) 1) << layer, cameras);
    }

    public static void FindAllByLayerMask(LayerMask mask, List<Camera> cameras)
    {
        foreach (Camera camera in Camera.allCameras)
        {
            if ((camera.cullingMask & mask) != 0)
            {
                cameras.Add(camera);
            }
        }
    }

    public static Camera FindFirstByLayer(GameLayer layer)
    {
        return FindFirstByLayerMask(layer.LayerBit());
    }

    public static Camera FindFirstByLayer(int layer)
    {
        return FindFirstByLayerMask(((int) 1) << layer);
    }

    public static Camera FindFirstByLayerMask(LayerMask mask)
    {
        foreach (Camera camera in Camera.allCameras)
        {
            if ((camera.cullingMask & mask) != 0)
            {
                return camera;
            }
        }
        return null;
    }

    public static Camera FindFullScreenEffectsCamera(bool activeOnly)
    {
        foreach (Camera camera in Camera.allCameras)
        {
            FullScreenEffects component = camera.GetComponent<FullScreenEffects>();
            if ((component != null) && (!activeOnly || component.isActive()))
            {
                return camera;
            }
        }
        return null;
    }

    public static Bounds GetFarClipBounds(Camera camera)
    {
        Vector3 center = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.farClipPlane));
        Vector3 vector2 = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.farClipPlane));
        Vector3 vector3 = camera.ViewportToWorldPoint(new Vector3(1f, 1f, camera.farClipPlane));
        return new Bounds(center, new Vector3(vector3.x - vector2.x, vector3.y - vector2.y, vector3.z - vector2.z));
    }

    public static Bounds GetNearClipBounds(Camera camera)
    {
        Vector3 center = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
        Vector3 vector2 = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.nearClipPlane));
        Vector3 vector3 = camera.ViewportToWorldPoint(new Vector3(1f, 1f, camera.nearClipPlane));
        return new Bounds(center, new Vector3(vector3.x - vector2.x, vector3.y - vector2.y, vector3.z - vector2.z));
    }

    public static Vector3 GetNearestPosInFrontOfCamera(Camera camera, float worldOffset = 0f)
    {
        return GetPosInFrontOfCamera(camera, (float) (camera.nearClipPlane + worldOffset));
    }

    public static Vector3 GetPosInFrontOfCamera(Camera camera, float worldDistance)
    {
        Vector3 position = camera.transform.position + new Vector3(0f, 0f, worldDistance);
        float magnitude = camera.transform.InverseTransformPoint(position).magnitude;
        Vector3 vector3 = new Vector3(0f, 0f, magnitude);
        return camera.transform.TransformPoint(vector3);
    }

    public static Vector3 GetPosInFrontOfCamera(Camera camera, Vector3 worldPoint)
    {
        return GetPosInFrontOfCamera(camera, worldPoint, 0f);
    }

    public static Vector3 GetPosInFrontOfCamera(Camera camera, Vector3 worldPoint, float worldOffset)
    {
        Vector3 position = camera.transform.position;
        Vector3 forward = camera.transform.forward;
        Plane plane = new Plane(-forward, worldPoint);
        float num2 = plane.GetDistanceToPoint(position) + worldOffset;
        Vector3 vector3 = (Vector3) (num2 * forward);
        return (position + vector3);
    }

    public static bool Raycast(Camera camera, Vector3 screenPoint, out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();
        if (!camera.pixelRect.Contains(screenPoint))
        {
            return false;
        }
        return Physics.Raycast(camera.ScreenPointToRay(screenPoint), out hitInfo, camera.farClipPlane, camera.cullingMask);
    }

    public static bool Raycast(Camera camera, Vector3 screenPoint, LayerMask layerMask, out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();
        if (!camera.pixelRect.Contains(screenPoint))
        {
            return false;
        }
        return Physics.Raycast(camera.ScreenPointToRay(screenPoint), out hitInfo, camera.farClipPlane, (int) layerMask);
    }

    public static float ScreenToWorldDist(Camera camera, float screenDist)
    {
        return ScreenToWorldDist(camera, screenDist, camera.nearClipPlane);
    }

    public static float ScreenToWorldDist(Camera camera, float screenDist, float worldDist)
    {
        Vector3 vector = camera.ScreenToWorldPoint(new Vector3(0f, 0f, worldDist));
        return (camera.ScreenToWorldPoint(new Vector3(screenDist, 0f, worldDist)).x - vector.x);
    }

    public static float ScreenToWorldDist(Camera camera, float screenDist, Vector3 worldPoint)
    {
        float worldDist = Vector3.Distance(camera.transform.position, worldPoint);
        return ScreenToWorldDist(camera, screenDist, worldDist);
    }
}

