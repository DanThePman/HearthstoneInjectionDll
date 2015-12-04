using System;
using UnityEngine;

public static class RenderUtils
{
    public static float GetMainTextureOffsetX(Component c)
    {
        return GetMainTextureOffsetX(c.GetComponent<Renderer>());
    }

    public static float GetMainTextureOffsetX(GameObject go)
    {
        return GetMainTextureOffsetX(go.GetComponent<Renderer>());
    }

    public static float GetMainTextureOffsetX(Renderer r)
    {
        return r.material.mainTextureOffset.x;
    }

    public static float GetMainTextureOffsetY(Component c)
    {
        return GetMainTextureOffsetY(c.GetComponent<Renderer>());
    }

    public static float GetMainTextureOffsetY(GameObject go)
    {
        return GetMainTextureOffsetY(go.GetComponent<Renderer>());
    }

    public static float GetMainTextureOffsetY(Renderer r)
    {
        return r.material.mainTextureOffset.y;
    }

    public static float GetMainTextureScaleX(Component c)
    {
        return GetMainTextureScaleX(c.GetComponent<Renderer>());
    }

    public static float GetMainTextureScaleX(GameObject go)
    {
        return GetMainTextureScaleX(go.GetComponent<Renderer>());
    }

    public static float GetMainTextureScaleX(Renderer r)
    {
        return r.material.mainTextureScale.x;
    }

    public static float GetMainTextureScaleY(Component c)
    {
        return GetMainTextureScaleY(c.GetComponent<Renderer>());
    }

    public static float GetMainTextureScaleY(GameObject go)
    {
        return GetMainTextureScaleY(go.GetComponent<Renderer>());
    }

    public static float GetMainTextureScaleY(Renderer r)
    {
        return r.material.mainTextureScale.y;
    }

    public static Material GetMaterial(Component c, int materialIndex)
    {
        return GetMaterial(c.GetComponent<Renderer>(), materialIndex);
    }

    public static Material GetMaterial(GameObject go, int materialIndex)
    {
        return GetMaterial(go.GetComponent<Renderer>(), materialIndex);
    }

    public static Material GetMaterial(Renderer renderer, int materialIndex)
    {
        if (materialIndex < 0)
        {
            return null;
        }
        Material[] materials = renderer.materials;
        if (materialIndex >= materials.Length)
        {
            return null;
        }
        Material material = materials[materialIndex];
        material.shaderKeywords = renderer.sharedMaterials[materialIndex].shaderKeywords;
        return material;
    }

    public static Material GetSharedMaterial(Component c, int materialIndex)
    {
        return GetSharedMaterial(c.GetComponent<Renderer>(), materialIndex);
    }

    public static Material GetSharedMaterial(GameObject go, int materialIndex)
    {
        return GetSharedMaterial(go.GetComponent<Renderer>(), materialIndex);
    }

    public static Material GetSharedMaterial(Renderer renderer, int materialIndex)
    {
        if (materialIndex < 0)
        {
            return null;
        }
        Material[] sharedMaterials = renderer.sharedMaterials;
        if (materialIndex >= sharedMaterials.Length)
        {
            return null;
        }
        return sharedMaterials[materialIndex];
    }

    public static void SetAlpha(Component c, float alpha)
    {
        SetAlpha(c.gameObject, alpha, false);
    }

    public static void SetAlpha(GameObject go, float alpha)
    {
        SetAlpha(go, alpha, false);
    }

    public static void SetAlpha(Component c, float alpha, bool includeInactive)
    {
        SetAlpha(c.gameObject, alpha, includeInactive);
    }

    public static void SetAlpha(GameObject go, float alpha, bool includeInactive)
    {
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>(includeInactive))
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
                else if (material.HasProperty("_TintColor"))
                {
                    Color color2 = material.GetColor("_TintColor");
                    color2.a = alpha;
                    material.SetColor("_TintColor", color2);
                }
            }
            if (renderer.GetComponent<Light>() != null)
            {
                Color color3 = renderer.GetComponent<Light>().color;
                color3.a = alpha;
                renderer.GetComponent<Light>().color = color3;
            }
        }
        foreach (UberText text in go.GetComponentsInChildren<UberText>(includeInactive))
        {
            Color textColor = text.TextColor;
            text.TextColor = new Color(textColor.r, textColor.g, textColor.b, alpha);
        }
    }

    public static void SetMainTextureOffsetX(Component c, float x)
    {
        SetMainTextureOffsetY(c.GetComponent<Renderer>(), x);
    }

    public static void SetMainTextureOffsetX(GameObject go, float x)
    {
        SetMainTextureOffsetY(go.GetComponent<Renderer>(), x);
    }

    public static void SetMainTextureOffsetX(Renderer r, float x)
    {
        Vector2 mainTextureOffset = r.material.mainTextureOffset;
        mainTextureOffset.x = x;
        r.material.mainTextureOffset = mainTextureOffset;
    }

    public static void SetMainTextureOffsetY(Component c, float y)
    {
        SetMainTextureOffsetY(c.GetComponent<Renderer>(), y);
    }

    public static void SetMainTextureOffsetY(GameObject go, float y)
    {
        SetMainTextureOffsetY(go.GetComponent<Renderer>(), y);
    }

    public static void SetMainTextureOffsetY(Renderer r, float y)
    {
        Vector2 mainTextureOffset = r.material.mainTextureOffset;
        mainTextureOffset.y = y;
        r.material.mainTextureOffset = mainTextureOffset;
    }

    public static void SetMainTextureScaleX(Component c, float x)
    {
        SetMainTextureScaleX(c.GetComponent<Renderer>(), x);
    }

    public static void SetMainTextureScaleX(GameObject go, float x)
    {
        SetMainTextureScaleX(go.GetComponent<Renderer>(), x);
    }

    public static void SetMainTextureScaleX(Renderer r, float x)
    {
        Vector2 mainTextureScale = r.material.mainTextureScale;
        mainTextureScale.x = x;
        r.material.mainTextureScale = mainTextureScale;
    }

    public static void SetMainTextureScaleY(Component c, float y)
    {
        SetMainTextureScaleY(c.GetComponent<Renderer>(), y);
    }

    public static void SetMainTextureScaleY(GameObject go, float y)
    {
        SetMainTextureScaleY(go.GetComponent<Renderer>(), y);
    }

    public static void SetMainTextureScaleY(Renderer r, float y)
    {
        Vector2 mainTextureScale = r.material.mainTextureScale;
        mainTextureScale.y = y;
        r.material.mainTextureScale = mainTextureScale;
    }

    public static void SetMaterial(Component c, int materialIndex, Material material)
    {
        SetMaterial(c.GetComponent<Renderer>(), materialIndex, material);
    }

    public static void SetMaterial(GameObject go, int materialIndex, Material material)
    {
        SetMaterial(go.GetComponent<Renderer>(), materialIndex, material);
    }

    public static void SetMaterial(Renderer renderer, int materialIndex, Material material)
    {
        if (materialIndex >= 0)
        {
            Material[] materials = renderer.materials;
            if (materialIndex < materials.Length)
            {
                materials[materialIndex] = material;
                renderer.materials = materials;
                renderer.materials[materialIndex].shaderKeywords = material.shaderKeywords;
            }
        }
    }

    public static void SetSharedMaterial(Component c, int materialIndex, Material material)
    {
        SetSharedMaterial(c.GetComponent<Renderer>(), materialIndex, material);
    }

    public static void SetSharedMaterial(GameObject go, int materialIndex, Material material)
    {
        SetSharedMaterial(go.GetComponent<Renderer>(), materialIndex, material);
    }

    public static void SetSharedMaterial(Renderer renderer, int materialIndex, Material material)
    {
        if ((material != null) && (materialIndex >= 0))
        {
            Material[] sharedMaterials = renderer.sharedMaterials;
            if (materialIndex < sharedMaterials.Length)
            {
                sharedMaterials[materialIndex] = material;
                sharedMaterials[materialIndex].shaderKeywords = material.shaderKeywords;
                renderer.sharedMaterials = sharedMaterials;
            }
        }
    }
}

