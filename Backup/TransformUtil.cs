using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransformUtil
{
    private const float ASPECT_RANGE = 0.2777778f;
    private const float MAX_PHONE_ASPECT_RATIO = 1.777778f;
    private const float MIN_PHONE_ASPECT_RATIO = 1.5f;

    public static void AttachAndPreserveLocalTransform(Transform child, Transform parent)
    {
        TransformProps destination = new TransformProps();
        CopyLocal(destination, child);
        child.parent = parent;
        CopyLocal(child, destination);
    }

    public static bool CanComputeOrientedWorldBounds(GameObject go, bool includeAllChildren = true)
    {
        return CanComputeOrientedWorldBounds(go, true, includeAllChildren);
    }

    public static bool CanComputeOrientedWorldBounds(GameObject go, bool includeUberText, bool includeAllChildren = true)
    {
        return CanComputeOrientedWorldBounds(go, includeUberText, null, includeAllChildren);
    }

    public static bool CanComputeOrientedWorldBounds(GameObject go, List<GameObject> ignoreMeshes, bool includeAllChildren = true)
    {
        return CanComputeOrientedWorldBounds(go, true, ignoreMeshes, includeAllChildren);
    }

    public static bool CanComputeOrientedWorldBounds(GameObject go, bool includeUberText, List<GameObject> ignoreMeshes, bool includeAllChildren = true)
    {
        if ((go != null) && go.activeSelf)
        {
            List<MeshFilter> list = GetComponentsWithIgnore<MeshFilter>(go, ignoreMeshes, includeAllChildren);
            if ((list != null) && (list.Count > 0))
            {
                return true;
            }
            if (includeUberText)
            {
                List<UberText> list2 = GetComponentsWithIgnore<UberText>(go, ignoreMeshes, includeAllChildren);
                return ((list2 != null) && (list2.Count > 0));
            }
        }
        return false;
    }

    public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, bool includeAllChildren = true)
    {
        return ComputeOrientedWorldBounds(go, true, includeAllChildren);
    }

    public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, bool includeUberText, bool includeAllChildren = true)
    {
        return ComputeOrientedWorldBounds(go, includeUberText, Vector3.zero, Vector3.zero, null, includeAllChildren);
    }

    public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, List<GameObject> ignoreMeshes, bool includeAllChildren = true)
    {
        return ComputeOrientedWorldBounds(go, true, ignoreMeshes, includeAllChildren);
    }

    public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, bool includeUberText, List<GameObject> ignoreMeshes, bool includeAllChildren = true)
    {
        return ComputeOrientedWorldBounds(go, includeUberText, Vector3.zero, Vector3.zero, ignoreMeshes, includeAllChildren);
    }

    public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, Vector3 minLocalPadding, Vector3 maxLocalPadding, bool includeAllChildren = true)
    {
        return ComputeOrientedWorldBounds(go, true, minLocalPadding, maxLocalPadding, includeAllChildren);
    }

    public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, bool includeUberText, Vector3 minLocalPadding, Vector3 maxLocalPadding, bool includeAllChildren = true)
    {
        return ComputeOrientedWorldBounds(go, includeUberText, minLocalPadding, maxLocalPadding, null, includeAllChildren);
    }

    public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, Vector3 minLocalPadding, Vector3 maxLocalPadding, List<GameObject> ignoreMeshes, bool includeAllChildren = true)
    {
        return ComputeOrientedWorldBounds(go, true, minLocalPadding, maxLocalPadding, ignoreMeshes, includeAllChildren);
    }

    public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, bool includeUberText, Vector3 minLocalPadding, Vector3 maxLocalPadding, List<GameObject> ignoreMeshes, bool includeAllChildren = true)
    {
        if ((go == null) || !go.activeSelf)
        {
            return null;
        }
        List<MeshFilter> list = GetComponentsWithIgnore<MeshFilter>(go, ignoreMeshes, includeAllChildren);
        List<UberText> list2 = null;
        if (includeUberText)
        {
            list2 = GetComponentsWithIgnore<UberText>(go, ignoreMeshes, includeAllChildren);
        }
        if (((list == null) || (list.Count == 0)) && ((list2 == null) || (list2.Count == 0)))
        {
            return null;
        }
        Matrix4x4 worldToLocalMatrix = go.transform.worldToLocalMatrix;
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        if (list != null)
        {
            foreach (MeshFilter filter in list)
            {
                if (filter.gameObject.activeSelf)
                {
                    Matrix4x4 matrixx2 = filter.transform.localToWorldMatrix;
                    Bounds bounds = filter.sharedMesh.bounds;
                    Matrix4x4 matrixx3 = worldToLocalMatrix * matrixx2;
                    Vector3[] vectorArray = new Vector3[] { matrixx3 * new Vector3(bounds.extents.x, 0f, 0f), matrixx3 * new Vector3(0f, bounds.extents.y, 0f), matrixx3 * new Vector3(0f, 0f, bounds.extents.z) };
                    Vector3 vector3 = (Vector3) (matrixx2 * filter.sharedMesh.bounds.center);
                    Vector3 origin = (Vector3) (worldToLocalMatrix * (filter.transform.position + vector3));
                    GetBoundsMinMax(origin, vectorArray[0], vectorArray[1], vectorArray[2], ref min, ref max);
                }
            }
        }
        if (list2 != null)
        {
            foreach (UberText text in list2)
            {
                if (text.gameObject.activeSelf)
                {
                    Matrix4x4 matrixx4 = text.transform.localToWorldMatrix;
                    Matrix4x4 matrixx5 = worldToLocalMatrix * matrixx4;
                    Vector3[] vectorArray2 = new Vector3[] { matrixx5 * new Vector3(text.Width * 0.5f, 0f, 0f), matrixx5 * new Vector3(0f, text.Height * 0.5f), matrixx5 * new Vector3(0f, 0f, 0.01f) };
                    GetBoundsMinMax((Vector3) (worldToLocalMatrix * text.transform.position), vectorArray2[0], vectorArray2[1], vectorArray2[2], ref min, ref max);
                }
            }
        }
        if (minLocalPadding.sqrMagnitude > 0f)
        {
            min -= minLocalPadding;
        }
        if (maxLocalPadding.sqrMagnitude > 0f)
        {
            max += maxLocalPadding;
        }
        Matrix4x4 localToWorldMatrix = go.transform.localToWorldMatrix;
        Matrix4x4 matrixx7 = localToWorldMatrix;
        matrixx7.SetColumn(3, Vector4.zero);
        Vector3 vector5 = (Vector3) (((localToWorldMatrix * max) + (localToWorldMatrix * min)) * 0.5f);
        Vector3 vector6 = (Vector3) ((max - min) * 0.5f);
        OrientedBounds bounds3 = new OrientedBounds();
        bounds3.Extents = new Vector3[] { matrixx7 * new Vector3(vector6.x, 0f, 0f), matrixx7 * new Vector3(0f, vector6.y, 0f), matrixx7 * new Vector3(0f, 0f, vector6.z) };
        bounds3.Origin = vector5;
        bounds3.CenterOffset = go.transform.position - vector5;
        return bounds3;
    }

    public static Bounds ComputeSetPointBounds(Component c)
    {
        return ComputeSetPointBounds(c.gameObject, false);
    }

    public static Bounds ComputeSetPointBounds(GameObject go)
    {
        return ComputeSetPointBounds(go, false);
    }

    public static Bounds ComputeSetPointBounds(Component c, bool includeInactive)
    {
        return ComputeSetPointBounds(c.gameObject, includeInactive);
    }

    public static Bounds ComputeSetPointBounds(GameObject go, bool includeInactive)
    {
        Bounds bounds;
        UberText component = go.GetComponent<UberText>();
        if (component != null)
        {
            return component.GetTextWorldSpaceBounds();
        }
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        Collider collider = go.GetComponent<Collider>();
        if (collider == null)
        {
            return GetBoundsOfChildren(go, includeInactive);
        }
        if (collider.enabled)
        {
            bounds = collider.bounds;
        }
        else
        {
            collider.enabled = true;
            bounds = collider.bounds;
            collider.enabled = false;
        }
        MobileHitBox box = go.GetComponent<MobileHitBox>();
        if ((box != null) && box.HasExecuted())
        {
            bounds.size = new Vector3(bounds.size.x / box.m_scaleX, bounds.size.y / box.m_scaleY, bounds.size.z / box.m_scaleY);
        }
        return bounds;
    }

    public static Vector2 ComputeUnitAnchor(Bounds bounds, Vector2 worldPoint)
    {
        return new Vector2 { x = (worldPoint.x - bounds.min.x) / bounds.size.x, y = (worldPoint.y - bounds.min.y) / bounds.size.y };
    }

    public static Vector3 ComputeWorldPoint(Bounds bounds, Vector3 selfUnitAnchor)
    {
        return new Vector3 { x = Mathf.Lerp(bounds.min.x, bounds.max.x, selfUnitAnchor.x), y = Mathf.Lerp(bounds.min.y, bounds.max.y, selfUnitAnchor.y), z = Mathf.Lerp(bounds.min.z, bounds.max.z, selfUnitAnchor.z) };
    }

    public static Vector3 ComputeWorldScale(Component c)
    {
        return ComputeWorldScale(c.gameObject);
    }

    public static Vector3 ComputeWorldScale(GameObject go)
    {
        Vector3 localScale = go.transform.localScale;
        if (go.transform.parent != null)
        {
            for (Transform transform = go.transform.parent; transform != null; transform = transform.parent)
            {
                localScale.Scale(transform.localScale);
            }
        }
        return localScale;
    }

    public static void CopyLocal(TransformProps destination, Component source)
    {
        CopyLocal(destination, source.gameObject);
    }

    public static void CopyLocal(TransformProps destination, GameObject source)
    {
        destination.scale = source.transform.localScale;
        destination.rotation = source.transform.localRotation;
        destination.position = source.transform.localPosition;
    }

    public static void CopyLocal(Component destination, TransformProps source)
    {
        CopyLocal(destination.gameObject, source);
    }

    public static void CopyLocal(Component destination, Component source)
    {
        CopyLocal(destination.gameObject, source.gameObject);
    }

    public static void CopyLocal(Component destination, GameObject source)
    {
        CopyLocal(destination.gameObject, source);
    }

    public static void CopyLocal(GameObject destination, TransformProps source)
    {
        destination.transform.localScale = source.scale;
        destination.transform.localRotation = source.rotation;
        destination.transform.localPosition = source.position;
    }

    public static void CopyLocal(GameObject destination, Component source)
    {
        CopyLocal(destination, source.gameObject);
    }

    public static void CopyLocal(GameObject destination, GameObject source)
    {
        destination.transform.localScale = source.transform.localScale;
        destination.transform.localRotation = source.transform.localRotation;
        destination.transform.localPosition = source.transform.localPosition;
    }

    public static void CopyWorld(TransformProps destination, Component source)
    {
        CopyWorld(destination, source.gameObject);
    }

    public static void CopyWorld(TransformProps destination, GameObject source)
    {
        destination.scale = ComputeWorldScale(source);
        destination.rotation = source.transform.rotation;
        destination.position = source.transform.position;
    }

    public static void CopyWorld(Component destination, TransformProps source)
    {
        CopyWorld(destination.gameObject, source);
    }

    public static void CopyWorld(Component destination, Component source)
    {
        CopyWorld(destination.gameObject, source);
    }

    public static void CopyWorld(Component destination, GameObject source)
    {
        CopyWorld(destination.gameObject, source);
    }

    public static void CopyWorld(GameObject destination, TransformProps source)
    {
        SetWorldScale(destination, source.scale);
        destination.transform.rotation = source.rotation;
        destination.transform.position = source.position;
    }

    public static void CopyWorld(GameObject destination, Component source)
    {
        CopyWorld(destination, source.gameObject);
    }

    public static void CopyWorld(GameObject destination, GameObject source)
    {
        CopyWorldScale(destination, source);
        destination.transform.rotation = source.transform.rotation;
        destination.transform.position = source.transform.position;
    }

    public static void CopyWorldScale(Component destination, Component source)
    {
        CopyWorldScale(destination.gameObject, source.gameObject);
    }

    public static void CopyWorldScale(Component destination, GameObject source)
    {
        CopyWorldScale(destination.gameObject, source);
    }

    public static void CopyWorldScale(GameObject destination, Component source)
    {
        CopyWorldScale(destination, source.gameObject);
    }

    public static void CopyWorldScale(GameObject destination, GameObject source)
    {
        Vector3 scale = ComputeWorldScale(source);
        SetWorldScale(destination, scale);
    }

    public static Vector3 Divide(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
    }

    public static Vector3 GetAspectRatioDependentPosition(Vector3 aspect3to2, Vector3 aspect16to9)
    {
        float num = PhoneAspectRatioScale();
        float num2 = 1f - num;
        return (Vector3) ((aspect16to9 * num) + (aspect3to2 * num2));
    }

    public static float GetAspectRatioDependentValue(float aspect3to2, float aspect16to9)
    {
        float num = PhoneAspectRatioScale();
        float num2 = 1f - num;
        return ((aspect16to9 * num) + (aspect3to2 * num2));
    }

    public static Vector3[] GetBoundCorners(Vector3 origin, Vector3 xExtent, Vector3 yExtent, Vector3 zExtent)
    {
        Vector3 vector = origin + xExtent;
        Vector3 vector2 = origin - xExtent;
        Vector3 vector3 = yExtent + zExtent;
        Vector3 vector4 = yExtent - zExtent;
        Vector3 vector5 = -yExtent + zExtent;
        Vector3 vector6 = -yExtent - zExtent;
        return new Vector3[] { (vector + vector3), (vector + vector4), (vector + vector5), (vector + vector6), (vector2 - vector3), (vector2 - vector4), (vector2 - vector5), (vector2 - vector6) };
    }

    public static void GetBoundsMinMax(Vector3 origin, Vector3 xExtent, Vector3 yExtent, Vector3 zExtent, ref Vector3 min, ref Vector3 max)
    {
        Vector3[] vectorArray = GetBoundCorners(origin, xExtent, yExtent, zExtent);
        for (int i = 0; i < vectorArray.Length; i++)
        {
            min.x = Mathf.Min(vectorArray[i].x, min.x);
            min.y = Mathf.Min(vectorArray[i].y, min.y);
            min.z = Mathf.Min(vectorArray[i].z, min.z);
            max.x = Mathf.Max(vectorArray[i].x, max.x);
            max.y = Mathf.Max(vectorArray[i].y, max.y);
            max.z = Mathf.Max(vectorArray[i].z, max.z);
        }
    }

    public static Bounds GetBoundsOfChildren(Component c)
    {
        return GetBoundsOfChildren(c.gameObject, false);
    }

    public static Bounds GetBoundsOfChildren(GameObject go)
    {
        return GetBoundsOfChildren(go, false);
    }

    public static Bounds GetBoundsOfChildren(Component c, bool includeInactive)
    {
        return GetBoundsOfChildren(c.gameObject, includeInactive);
    }

    public static Bounds GetBoundsOfChildren(GameObject go, bool includeInactive)
    {
        Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(includeInactive);
        if (componentsInChildren.Length == 0)
        {
            return new Bounds(go.transform.position, Vector3.zero);
        }
        Bounds bounds = componentsInChildren[0].bounds;
        for (int i = 1; i < componentsInChildren.Length; i++)
        {
            Renderer renderer = componentsInChildren[i];
            Bounds bounds2 = renderer.bounds;
            Vector3 max = Vector3.Max(bounds2.max, bounds.max);
            Vector3 min = Vector3.Min(bounds2.min, bounds.min);
            bounds.SetMinMax(min, max);
        }
        return bounds;
    }

    public static List<T> GetComponentsWithIgnore<T>(GameObject obj, List<GameObject> ignoreObjects, bool includeAllChildren = true) where T: Component
    {
        List<T> results = new List<T>();
        if (includeAllChildren)
        {
            obj.GetComponentsInChildren<T>(results);
        }
        T component = obj.GetComponent<T>();
        if (component != null)
        {
            results.Add(component);
        }
        if ((ignoreObjects != null) && (ignoreObjects.Count > 0))
        {
            T[] localArray = results.ToArray();
            results.Clear();
            foreach (T local2 in localArray)
            {
                bool flag = true;
                foreach (GameObject obj2 in ignoreObjects)
                {
                    if (((obj2 == null) || (local2.transform == obj2.transform)) || local2.transform.IsChildOf(obj2.transform))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    results.Add(local2);
                }
            }
        }
        return results;
    }

    public static Vector3 GetUnitAnchor(Anchor anchor)
    {
        Vector3 vector = new Vector3();
        switch (anchor)
        {
            case Anchor.TOP_LEFT:
                vector.x = 0f;
                vector.y = 1f;
                vector.z = 0f;
                return vector;

            case Anchor.TOP:
                vector.x = 0.5f;
                vector.y = 1f;
                vector.z = 0f;
                return vector;

            case Anchor.TOP_RIGHT:
                vector.x = 1f;
                vector.y = 1f;
                vector.z = 0f;
                return vector;

            case Anchor.LEFT:
                vector.x = 0f;
                vector.y = 0.5f;
                vector.z = 0f;
                return vector;

            case Anchor.CENTER:
                vector.x = 0.5f;
                vector.y = 0.5f;
                vector.z = 0f;
                return vector;

            case Anchor.RIGHT:
                vector.x = 1f;
                vector.y = 0.5f;
                vector.z = 0f;
                return vector;

            case Anchor.BOTTOM_LEFT:
                vector.x = 0f;
                vector.y = 0f;
                vector.z = 0f;
                return vector;

            case Anchor.BOTTOM:
                vector.x = 0.5f;
                vector.y = 0f;
                vector.z = 0f;
                return vector;

            case Anchor.BOTTOM_RIGHT:
                vector.x = 1f;
                vector.y = 0f;
                vector.z = 0f;
                return vector;

            case Anchor.FRONT:
                vector.x = 0.5f;
                vector.y = 0f;
                vector.z = 1f;
                return vector;

            case Anchor.BACK:
                vector.x = 0.5f;
                vector.y = 0f;
                vector.z = 0f;
                return vector;

            case Anchor.TOP_LEFT_XZ:
                vector.x = 0f;
                vector.z = 1f;
                vector.y = 0f;
                return vector;

            case Anchor.TOP_XZ:
                vector.x = 0.5f;
                vector.z = 1f;
                vector.y = 0f;
                return vector;

            case Anchor.TOP_RIGHT_XZ:
                vector.x = 1f;
                vector.z = 1f;
                vector.y = 0f;
                return vector;

            case Anchor.LEFT_XZ:
                vector.x = 0f;
                vector.z = 0.5f;
                vector.y = 0f;
                return vector;

            case Anchor.CENTER_XZ:
                vector.x = 0.5f;
                vector.z = 0.5f;
                vector.y = 0f;
                return vector;

            case Anchor.RIGHT_XZ:
                vector.x = 1f;
                vector.z = 0.5f;
                vector.y = 0f;
                return vector;

            case Anchor.BOTTOM_LEFT_XZ:
                vector.x = 0f;
                vector.z = 0f;
                vector.y = 0f;
                return vector;

            case Anchor.BOTTOM_XZ:
                vector.x = 0.5f;
                vector.z = 0f;
                vector.y = 0f;
                return vector;

            case Anchor.BOTTOM_RIGHT_XZ:
                vector.x = 1f;
                vector.z = 0f;
                vector.y = 0f;
                return vector;

            case Anchor.FRONT_XZ:
                vector.x = 0.5f;
                vector.z = 0f;
                vector.y = 1f;
                return vector;

            case Anchor.BACK_XZ:
                vector.x = 0.5f;
                vector.z = 0f;
                vector.y = 0f;
                return vector;
        }
        return vector;
    }

    public static void Identity(Component c)
    {
        c.transform.localScale = Vector3.one;
        c.transform.localRotation = Quaternion.identity;
        c.transform.localPosition = Vector3.zero;
    }

    public static void Identity(GameObject go)
    {
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localPosition = Vector3.zero;
    }

    public static Vector3 Multiply(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public static void OrientTo(Component source, Component target)
    {
        OrientTo(source.transform, target.transform);
    }

    public static void OrientTo(Component source, GameObject target)
    {
        OrientTo(source.transform, target.transform);
    }

    public static void OrientTo(GameObject source, Component target)
    {
        OrientTo(source.transform, target.transform);
    }

    public static void OrientTo(GameObject source, GameObject target)
    {
        OrientTo(source.transform, target.transform);
    }

    public static void OrientTo(Transform source, Transform target)
    {
        OrientTo(source, source.transform.position, target.transform.position);
    }

    public static void OrientTo(Transform source, Vector3 sourcePosition, Vector3 targetPosition)
    {
        Vector3 forward = targetPosition - sourcePosition;
        if (forward.sqrMagnitude > Mathf.Epsilon)
        {
            source.rotation = Quaternion.LookRotation(forward);
        }
    }

    public static float PhoneAspectRatioScale()
    {
        float num = ((float) Screen.width) / ((float) Screen.height);
        return ((Mathf.Clamp(num, 1.5f, 1.777778f) - 1.5f) / 0.2777778f);
    }

    public static Vector3 RandomVector3(Vector3 min, Vector3 max)
    {
        return new Vector3 { x = UnityEngine.Random.Range(min.x, max.x), y = UnityEngine.Random.Range(min.y, max.y), z = UnityEngine.Random.Range(min.z, max.z) };
    }

    public static void SetEulerAngleX(Component c, float x)
    {
        Transform transform = c.transform;
        transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    public static void SetEulerAngleX(GameObject go, float x)
    {
        Transform transform = go.transform;
        transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    public static void SetEulerAngleY(Component c, float y)
    {
        Transform transform = c.transform;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
    }

    public static void SetEulerAngleY(GameObject go, float y)
    {
        Transform transform = go.transform;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
    }

    public static void SetEulerAngleZ(Component c, float z)
    {
        Transform transform = c.transform;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
    }

    public static void SetEulerAngleZ(GameObject go, float z)
    {
        Transform transform = go.transform;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
    }

    public static void SetLocalEulerAngleX(Component c, float x)
    {
        Transform transform = c.transform;
        transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    public static void SetLocalEulerAngleX(GameObject go, float x)
    {
        Transform transform = go.transform;
        transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    public static void SetLocalEulerAngleY(Component c, float y)
    {
        Transform transform = c.transform;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, transform.localEulerAngles.z);
    }

    public static void SetLocalEulerAngleY(GameObject go, float y)
    {
        Transform transform = go.transform;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, transform.localEulerAngles.z);
    }

    public static void SetLocalEulerAngleZ(Component c, float z)
    {
        Transform transform = c.transform;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
    }

    public static void SetLocalEulerAngleZ(GameObject go, float z)
    {
        Transform transform = go.transform;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
    }

    public static void SetLocalPosX(Component component, float x)
    {
        Transform transform = component.transform;
        transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
    }

    public static void SetLocalPosX(GameObject go, float x)
    {
        Transform transform = go.transform;
        transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
    }

    public static void SetLocalPosY(Component component, float y)
    {
        Transform transform = component.transform;
        transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
    }

    public static void SetLocalPosY(GameObject go, float y)
    {
        Transform transform = go.transform;
        transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
    }

    public static void SetLocalPosZ(Component component, float z)
    {
        Transform transform = component.transform;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
    }

    public static void SetLocalPosZ(GameObject go, float z)
    {
        Transform transform = go.transform;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
    }

    public static void SetLocalScaleToWorldDimension(GameObject obj, params WorldDimensionIndex[] dimensions)
    {
        SetLocalScaleToWorldDimension(obj, null, dimensions);
    }

    public static unsafe void SetLocalScaleToWorldDimension(GameObject obj, List<GameObject> ignoreMeshes, params WorldDimensionIndex[] dimensions)
    {
        Vector3 localScale = obj.transform.localScale;
        OrientedBounds bounds = ComputeOrientedWorldBounds(obj, ignoreMeshes, true);
        for (int i = 0; i < dimensions.Length; i++)
        {
            ref Vector3 vectorRef;
            int num3;
            float num2 = bounds.Extents[dimensions[i].Index].magnitude * 2f;
            float num4 = vectorRef[num3];
            (vectorRef = (Vector3) &localScale)[num3 = dimensions[i].Index] = num4 * ((num2 > Mathf.Epsilon) ? (dimensions[i].Dimension / num2) : 0.001f);
            if (Mathf.Abs(localScale[dimensions[i].Index]) < 0.001f)
            {
                localScale[dimensions[i].Index] = 0.001f;
            }
        }
        obj.transform.localScale = localScale;
    }

    public static void SetLocalScaleX(Component component, float x)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
    }

    public static void SetLocalScaleX(GameObject go, float x)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
    }

    public static void SetLocalScaleXY(Component component, Vector2 v)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(v.x, v.y, transform.localScale.z);
    }

    public static void SetLocalScaleXY(GameObject go, Vector2 v)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(v.x, v.y, transform.localScale.z);
    }

    public static void SetLocalScaleXY(Component component, float x, float y)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(x, y, transform.localScale.z);
    }

    public static void SetLocalScaleXY(GameObject go, float x, float y)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(x, y, transform.localScale.z);
    }

    public static void SetLocalScaleXZ(Component component, Vector2 v)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(v.x, transform.localScale.y, v.y);
    }

    public static void SetLocalScaleXZ(GameObject go, Vector2 v)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(v.x, transform.localScale.y, v.y);
    }

    public static void SetLocalScaleXZ(Component component, float x, float z)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(x, transform.localScale.y, z);
    }

    public static void SetLocalScaleXZ(GameObject go, float x, float z)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(x, transform.localScale.y, z);
    }

    public static void SetLocalScaleY(Component component, float y)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
    }

    public static void SetLocalScaleY(GameObject go, float y)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
    }

    public static void SetLocalScaleYZ(Component component, Vector2 v)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(transform.localScale.x, v.x, v.y);
    }

    public static void SetLocalScaleYZ(GameObject go, Vector2 v)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(transform.localScale.x, v.x, v.y);
    }

    public static void SetLocalScaleYZ(Component component, float y, float z)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(transform.localScale.x, y, z);
    }

    public static void SetLocalScaleYZ(GameObject go, float y, float z)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(transform.localScale.x, y, z);
    }

    public static void SetLocalScaleZ(Component component, float z)
    {
        Transform transform = component.transform;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
    }

    public static void SetLocalScaleZ(GameObject go, float z)
    {
        Transform transform = go.transform;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
    }

    public static void SetPoint(Component src, Anchor srcAnchor, Component dst, Anchor dstAnchor)
    {
        SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), Vector3.zero, false);
    }

    public static void SetPoint(Component src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor)
    {
        SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), Vector3.zero, false);
    }

    public static void SetPoint(Component self, Vector3 selfUnitAnchor, Component relative, Vector3 relativeUnitAnchor)
    {
        SetPoint(self.gameObject, selfUnitAnchor, relative.gameObject, relativeUnitAnchor, Vector3.zero, false);
    }

    public static void SetPoint(Component self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor)
    {
        SetPoint(self.gameObject, selfUnitAnchor, relative, relativeUnitAnchor, Vector3.zero, false);
    }

    public static void SetPoint(GameObject src, Anchor srcAnchor, Component dst, Anchor dstAnchor)
    {
        SetPoint(src, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), Vector3.zero, false);
    }

    public static void SetPoint(GameObject src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor)
    {
        SetPoint(src, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), Vector3.zero, false);
    }

    public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, Component relative, Vector3 relativeUnitAnchor)
    {
        SetPoint(self, selfUnitAnchor, relative.gameObject, relativeUnitAnchor, Vector3.zero, false);
    }

    public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor)
    {
        SetPoint(self, selfUnitAnchor, relative, relativeUnitAnchor, Vector3.zero, false);
    }

    public static void SetPoint(Component src, Anchor srcAnchor, Component dst, Anchor dstAnchor, bool includeInactive)
    {
        SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), Vector3.zero, includeInactive);
    }

    public static void SetPoint(Component src, Anchor srcAnchor, Component dst, Anchor dstAnchor, Vector3 offset)
    {
        SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), offset, false);
    }

    public static void SetPoint(Component src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, bool includeInactive)
    {
        SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), Vector3.zero, includeInactive);
    }

    public static void SetPoint(Component src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, Vector3 offset)
    {
        SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), offset, false);
    }

    public static void SetPoint(Component self, Vector3 selfUnitAnchor, Component relative, Vector3 relativeUnitAnchor, Vector3 offset)
    {
        SetPoint(self.gameObject, selfUnitAnchor, relative.gameObject, relativeUnitAnchor, offset, false);
    }

    public static void SetPoint(Component self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor, Vector3 offset)
    {
        SetPoint(self.gameObject, selfUnitAnchor, relative, relativeUnitAnchor, offset, false);
    }

    public static void SetPoint(GameObject src, Anchor srcAnchor, Component dst, Anchor dstAnchor, bool includeInactive)
    {
        SetPoint(src, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), Vector3.zero, includeInactive);
    }

    public static void SetPoint(GameObject src, Anchor srcAnchor, Component dst, Anchor dstAnchor, Vector3 offset)
    {
        SetPoint(src, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), offset, false);
    }

    public static void SetPoint(GameObject src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, bool includeInactive)
    {
        SetPoint(src, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), Vector3.zero, includeInactive);
    }

    public static void SetPoint(GameObject src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, Vector3 offset)
    {
        SetPoint(src, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), offset, false);
    }

    public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, Component relative, Vector3 relativeUnitAnchor, Vector3 offset)
    {
        SetPoint(self, selfUnitAnchor, relative.gameObject, relativeUnitAnchor, offset, false);
    }

    public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor, Vector3 offset)
    {
        SetPoint(self, selfUnitAnchor, relative, relativeUnitAnchor, offset, false);
    }

    public static void SetPoint(Component src, Anchor srcAnchor, Component dst, Anchor dstAnchor, Vector3 offset, bool includeInactive)
    {
        SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), offset, includeInactive);
    }

    public static void SetPoint(Component src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, Vector3 offset, bool includeInactive)
    {
        SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), offset, includeInactive);
    }

    public static void SetPoint(Component self, Vector3 selfUnitAnchor, Component relative, Vector3 relativeUnitAnchor, Vector3 offset, bool includeInactive)
    {
        SetPoint(self.gameObject, selfUnitAnchor, relative.gameObject, relativeUnitAnchor, offset, includeInactive);
    }

    public static void SetPoint(Component self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor, Vector3 offset, bool includeInactive)
    {
        SetPoint(self.gameObject, selfUnitAnchor, relative, relativeUnitAnchor, offset, includeInactive);
    }

    public static void SetPoint(GameObject src, Anchor srcAnchor, Component dst, Anchor dstAnchor, Vector3 offset, bool includeInactive)
    {
        SetPoint(src, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), offset, includeInactive);
    }

    public static void SetPoint(GameObject src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, Vector3 offset, bool includeInactive)
    {
        SetPoint(src, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), offset, includeInactive);
    }

    public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, Component relative, Vector3 relativeUnitAnchor, Vector3 offset, bool includeInactive)
    {
        SetPoint(self, selfUnitAnchor, relative.gameObject, relativeUnitAnchor, offset, includeInactive);
    }

    public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor, Vector3 offset, bool includeInactive)
    {
        Bounds bounds = ComputeSetPointBounds(self, includeInactive);
        Bounds bounds2 = ComputeSetPointBounds(relative, includeInactive);
        Vector3 vector = ComputeWorldPoint(bounds, selfUnitAnchor);
        Vector3 vector2 = ComputeWorldPoint(bounds2, relativeUnitAnchor);
        Vector3 translation = new Vector3((vector2.x - vector.x) + offset.x, (vector2.y - vector.y) + offset.y, (vector2.z - vector.z) + offset.z);
        self.transform.Translate(translation, Space.World);
    }

    public static void SetPosX(Component component, float x)
    {
        Transform transform = component.transform;
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    public static void SetPosX(GameObject go, float x)
    {
        Transform transform = go.transform;
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    public static void SetPosY(Component component, float y)
    {
        Transform transform = component.transform;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    public static void SetPosY(GameObject go, float y)
    {
        Transform transform = go.transform;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    public static void SetPosZ(Component component, float z)
    {
        Transform transform = component.transform;
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

    public static void SetPosZ(GameObject go, float z)
    {
        Transform transform = go.transform;
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

    public static void SetWorldScale(Component destination, Vector3 scale)
    {
        SetWorldScale(destination.gameObject, scale);
    }

    public static void SetWorldScale(GameObject destination, Vector3 scale)
    {
        if (destination.transform.parent != null)
        {
            for (Transform transform = destination.transform.parent; transform != null; transform = transform.parent)
            {
                scale.Scale(Vector3Reciprocal(transform.localScale));
            }
        }
        destination.transform.localScale = scale;
    }

    public static Vector3 Vector3Reciprocal(Vector3 source)
    {
        Vector3 vector = source;
        if (vector.x != 0f)
        {
            vector.x = 1f / vector.x;
        }
        if (vector.y != 0f)
        {
            vector.y = 1f / vector.y;
        }
        if (vector.z != 0f)
        {
            vector.z = 1f / vector.z;
        }
        return vector;
    }
}

