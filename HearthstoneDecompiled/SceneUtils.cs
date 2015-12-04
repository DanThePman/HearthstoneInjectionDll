using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SceneUtils
{
    public static Transform CreateBone(Component template)
    {
        return CreateBone(template.gameObject);
    }

    public static Transform CreateBone(GameObject template)
    {
        GameObject destination = new GameObject(string.Format("{0}Bone", template.name)) {
            transform = { parent = template.transform.parent }
        };
        TransformUtil.CopyLocal(destination, template);
        return destination.transform;
    }

    public static void EnableColliders(Component c, bool enable)
    {
        EnableColliders(c.gameObject, enable);
    }

    public static void EnableColliders(GameObject go, bool enable)
    {
        Collider[] componentsInChildren = go.GetComponentsInChildren<Collider>();
        if (componentsInChildren != null)
        {
            foreach (Collider collider in componentsInChildren)
            {
                collider.enabled = enable;
            }
        }
    }

    public static void EnableRenderers(Component c, bool enable)
    {
        EnableRenderers(c.gameObject, enable);
    }

    public static void EnableRenderers(GameObject go, bool enable)
    {
        EnableRenderers(go, enable, false);
    }

    public static void EnableRenderers(Component c, bool enable, bool includeInactive)
    {
        EnableRenderers(c.gameObject, enable, false);
    }

    public static void EnableRenderers(GameObject go, bool enable, bool includeInactive)
    {
        Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(includeInactive);
        if (componentsInChildren != null)
        {
            foreach (Renderer renderer in componentsInChildren)
            {
                renderer.enabled = enable;
            }
        }
    }

    public static void EnableRenderersAndColliders(Component c, bool enable)
    {
        EnableRenderersAndColliders(c.gameObject, enable);
    }

    public static void EnableRenderersAndColliders(GameObject go, bool enable)
    {
        Collider component = go.GetComponent<Collider>();
        if (component != null)
        {
            component.enabled = enable;
        }
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = enable;
        }
        IEnumerator enumerator = go.transform.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                EnableRenderersAndColliders(current.gameObject, enable);
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

    public static GameObject FindChild(GameObject parentObject, string name)
    {
        if (parentObject.name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return parentObject;
        }
        IEnumerator enumerator = parentObject.transform.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                GameObject obj2 = FindChild(current.gameObject, name);
                if (obj2 != null)
                {
                    return obj2;
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
        return null;
    }

    public static GameObject FindChildBySubstring(GameObject parentObject, string substr)
    {
        if (parentObject.name.IndexOf(substr, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return parentObject;
        }
        IEnumerator enumerator = parentObject.transform.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                GameObject obj2 = FindChildBySubstring(current.gameObject, substr);
                if (obj2 != null)
                {
                    return obj2;
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
        return null;
    }

    public static GameObject FindChildByTag(GameObject go, string tag)
    {
        Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
        if (componentsInChildren != null)
        {
            foreach (Transform transform in componentsInChildren)
            {
                if (transform.CompareTag(tag))
                {
                    return transform.gameObject;
                }
            }
        }
        return null;
    }

    public static T FindComponentInParents<T>(Component child) where T: Component
    {
        if (child != null)
        {
            for (Transform transform = child.transform.parent; transform != null; transform = transform.parent)
            {
                T component = transform.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
        }
        return null;
    }

    public static T FindComponentInParents<T>(GameObject child) where T: Component
    {
        if (child == null)
        {
            return null;
        }
        return FindComponentInParents<T>(child.transform);
    }

    public static T FindComponentInThisOrParents<T>(Component start) where T: Component
    {
        if (start != null)
        {
            for (Transform transform = start.transform; transform != null; transform = transform.parent)
            {
                T component = transform.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
        }
        return null;
    }

    public static T FindComponentInThisOrParents<T>(GameObject start) where T: Component
    {
        if (start == null)
        {
            return null;
        }
        return FindComponentInThisOrParents<T>(start.transform);
    }

    public static Transform FindFirstChild(Transform parent)
    {
        IEnumerator enumerator = parent.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                return (Transform) enumerator.Current;
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
        return null;
    }

    public static GameObject FindTopParent(Component c)
    {
        Transform parent = c.transform;
        while (parent.parent != null)
        {
            parent = parent.parent;
        }
        return parent.gameObject;
    }

    public static GameObject FindTopParent(GameObject go)
    {
        return FindTopParent(go.transform);
    }

    public static T GetComponentInChildrenOnly<T>(Component c) where T: Component
    {
        if (c == null)
        {
            return null;
        }
        return GetComponentInChildrenOnly<T>(c.gameObject);
    }

    public static T GetComponentInChildrenOnly<T>(GameObject go) where T: Component
    {
        if (go != null)
        {
            IEnumerator enumerator = go.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    T componentInChildren = ((Transform) enumerator.Current).GetComponentInChildren<T>();
                    if (componentInChildren != null)
                    {
                        return componentInChildren;
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
        return null;
    }

    public static T[] GetComponentsInChildrenOnly<T>(Component c) where T: Component
    {
        if (c == null)
        {
            return new T[0];
        }
        return GetComponentsInChildrenOnly<T>(c.gameObject);
    }

    public static T[] GetComponentsInChildrenOnly<T>(GameObject go) where T: Component
    {
        return GetComponentsInChildrenOnly<T>(go, false);
    }

    public static T[] GetComponentsInChildrenOnly<T>(Component c, bool includeInactive) where T: Component
    {
        if (c == null)
        {
            return new T[0];
        }
        return GetComponentsInChildrenOnly<T>(c.gameObject, includeInactive);
    }

    public static T[] GetComponentsInChildrenOnly<T>(GameObject go, bool includeInactive) where T: Component
    {
        if (go == null)
        {
            return new T[0];
        }
        List<T> list = new List<T>();
        IEnumerator enumerator = go.transform.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                T[] componentsInChildren = ((Transform) enumerator.Current).GetComponentsInChildren<T>(includeInactive);
                list.AddRange(componentsInChildren);
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
        return list.ToArray();
    }

    public static bool IsAncestorOf(Component ancestor, Component descendant)
    {
        for (Transform transform = descendant.transform; transform != null; transform = transform.parent)
        {
            if (transform == ancestor.transform)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsAncestorOf(GameObject ancestor, GameObject descendant)
    {
        return IsAncestorOf(ancestor.transform, descendant.transform);
    }

    public static bool IsDescendantOf(Component descendant, Component ancestor)
    {
        if (descendant == ancestor)
        {
            return true;
        }
        IEnumerator enumerator = ancestor.transform.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                if (IsDescendantOf(descendant, current))
                {
                    return true;
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
        return false;
    }

    public static bool IsDescendantOf(Component descendant, GameObject ancestor)
    {
        return IsDescendantOf((Component) descendant.transform, (Component) ancestor.transform);
    }

    public static bool IsDescendantOf(GameObject descendant, Component ancestor)
    {
        return IsDescendantOf((Component) descendant.transform, (Component) ancestor.transform);
    }

    public static bool IsDescendantOf(GameObject descendant, GameObject ancestor)
    {
        return IsDescendantOf((Component) descendant.transform, (Component) ancestor.transform);
    }

    public static string LayerMaskToString(LayerMask mask)
    {
        if (mask == 0)
        {
            return "[NO LAYERS]";
        }
        StringBuilder builder = new StringBuilder("[");
        IEnumerator enumerator = Enum.GetValues(typeof(GameLayer)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                GameLayer current = (GameLayer) ((int) enumerator.Current);
                if ((mask & current.LayerBit()) != 0)
                {
                    builder.Append(current);
                    builder.Append(", ");
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
        if (builder.Length == 1)
        {
            return "[NO LAYERS]";
        }
        builder.Remove(builder.Length - 2, 2);
        builder.Append("]");
        return builder.ToString();
    }

    public static void ReplaceLayer(GameObject parentObject, GameLayer newLayer, GameLayer oldLayer)
    {
        if (parentObject.layer == oldLayer)
        {
            parentObject.layer = (int) newLayer;
        }
        IEnumerator enumerator = parentObject.transform.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                if (current.gameObject.layer == oldLayer)
                {
                    SetLayer(current.gameObject, newLayer);
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

    public static void ResizeBoxCollider(Component c, Bounds bounds)
    {
        ResizeBoxCollider(c.GetComponent<Collider>(), bounds.min, bounds.max);
    }

    public static void ResizeBoxCollider(GameObject go, Bounds bounds)
    {
        ResizeBoxCollider(go.GetComponent<Collider>(), bounds.min, bounds.max);
    }

    public static void ResizeBoxCollider(Component c, Component worldCorner1, Component worldCorner2)
    {
        ResizeBoxCollider(c, worldCorner1.transform.position, worldCorner2.transform.position);
    }

    public static void ResizeBoxCollider(Component c, Component worldCorner1, GameObject worldCorner2)
    {
        ResizeBoxCollider(c, worldCorner1.transform.position, worldCorner2.transform.position);
    }

    public static void ResizeBoxCollider(Component c, GameObject worldCorner1, Component worldCorner2)
    {
        ResizeBoxCollider(c, worldCorner1.transform.position, worldCorner2.transform.position);
    }

    public static void ResizeBoxCollider(Component c, GameObject worldCorner1, GameObject worldCorner2)
    {
        ResizeBoxCollider(c, worldCorner1.transform.position, worldCorner2.transform.position);
    }

    public static void ResizeBoxCollider(Component c, Vector3 worldCorner1, Vector3 worldCorner2)
    {
        Vector3 lhs = c.transform.InverseTransformPoint(worldCorner1);
        Vector3 rhs = c.transform.InverseTransformPoint(worldCorner2);
        Vector3 vector3 = Vector3.Min(lhs, rhs);
        Vector3 vector4 = Vector3.Max(lhs, rhs);
        BoxCollider component = c.GetComponent<BoxCollider>();
        component.center = (Vector3) (0.5f * (vector3 + vector4));
        component.size = vector4 - vector3;
    }

    public static void ResizeBoxCollider(GameObject go, Component worldCorner1, Component worldCorner2)
    {
        ResizeBoxCollider(go.GetComponent<Collider>(), worldCorner1.transform.position, worldCorner2.transform.position);
    }

    public static void ResizeBoxCollider(GameObject go, Component worldCorner1, GameObject worldCorner2)
    {
        ResizeBoxCollider(go.GetComponent<Collider>(), worldCorner1.transform.position, worldCorner2.transform.position);
    }

    public static void ResizeBoxCollider(GameObject go, GameObject worldCorner1, Component worldCorner2)
    {
        ResizeBoxCollider(go.GetComponent<Collider>(), worldCorner1.transform.position, worldCorner2.transform.position);
    }

    public static void ResizeBoxCollider(GameObject go, GameObject worldCorner1, GameObject worldCorner2)
    {
        ResizeBoxCollider(go.GetComponent<Collider>(), worldCorner1.transform.position, worldCorner2.transform.position);
    }

    public static void ResizeBoxCollider(GameObject go, Vector3 worldCorner1, Vector3 worldCorner2)
    {
        ResizeBoxCollider(go.GetComponent<Collider>(), worldCorner1, worldCorner2);
    }

    public static void SetHideFlags(UnityEngine.Object obj, HideFlags flags)
    {
    }

    public static void SetLayer(Component c, GameLayer layer)
    {
        SetLayer(c.gameObject, (int) layer);
    }

    public static void SetLayer(Component c, int layer)
    {
        SetLayer(c.gameObject, layer);
    }

    public static void SetLayer(GameObject go, GameLayer layer)
    {
        SetLayer(go, (int) layer);
    }

    public static void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        IEnumerator enumerator = go.transform.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                SetLayer(current.gameObject, layer);
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

    public static void SetRenderQueue(GameObject go, int renderQueue)
    {
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
        {
            if (renderer.material != null)
            {
                renderer.material.renderQueue = renderQueue;
            }
        }
    }
}

