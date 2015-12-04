using System;
using UnityEngine;

public class TiledBackground : MonoBehaviour
{
    public float Depth;

    private void Awake()
    {
        if (base.GetComponent<Renderer>().material == null)
        {
            Debug.LogError("TiledBackground requires the mesh renderer to have a material");
            UnityEngine.Object.Destroy(this);
        }
    }

    public void SetBounds(Bounds bounds)
    {
        base.transform.localScale = Vector3.one;
        Vector3 min = base.GetComponent<Renderer>().bounds.min;
        Vector3 max = base.GetComponent<Renderer>().bounds.max;
        if (base.transform.parent != null)
        {
            min = base.transform.parent.InverseTransformPoint(min);
            max = base.transform.parent.InverseTransformPoint(max);
        }
        Vector3 vector3 = VectorUtils.Abs(max - min);
        Vector3 vector4 = new Vector3((vector3.x <= 0f) ? 0f : (bounds.size.x / vector3.x), (vector3.y <= 0f) ? 0f : (bounds.size.y / vector3.y), (vector3.z <= 0f) ? 0f : (bounds.size.z / vector3.z));
        base.transform.localScale = vector4;
        base.transform.localPosition = bounds.center + new Vector3(0f, 0f, -this.Depth);
        base.GetComponent<Renderer>().material.mainTextureScale = vector4;
    }

    public Vector2 Offset
    {
        get
        {
            return new Vector2(base.GetComponent<Renderer>().material.mainTextureOffset.x / base.GetComponent<Renderer>().material.mainTextureScale.x, base.GetComponent<Renderer>().material.mainTextureOffset.y / base.GetComponent<Renderer>().material.mainTextureScale.y);
        }
        set
        {
            base.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(base.GetComponent<Renderer>().material.mainTextureScale.x * value.x, base.GetComponent<Renderer>().material.mainTextureScale.y * value.y);
        }
    }
}

