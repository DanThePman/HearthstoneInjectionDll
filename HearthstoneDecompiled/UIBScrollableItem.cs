using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class UIBScrollableItem : MonoBehaviour
{
    public ActiveState m_active;
    private ActiveStateCallback m_activeStateCallback;
    public Vector3 m_offset = Vector3.zero;
    public Vector3 m_size = Vector3.one;

    private Vector3[] GetBoundsPoints()
    {
        Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
        Vector3[] vectorArray = new Vector3[] { localToWorldMatrix * new Vector3(this.m_size.x * 0.5f, 0f, 0f), localToWorldMatrix * new Vector3(0f, this.m_size.y * 0.5f, 0f), localToWorldMatrix * new Vector3(0f, 0f, this.m_size.z * 0.5f) };
        Vector3 vector = base.transform.position + (localToWorldMatrix * this.m_offset);
        return new Vector3[] { (((vector + vectorArray[0]) + vectorArray[1]) + vectorArray[2]), (((vector + vectorArray[0]) + vectorArray[1]) - vectorArray[2]), (((vector + vectorArray[0]) - vectorArray[1]) + vectorArray[2]), (((vector + vectorArray[0]) - vectorArray[1]) - vectorArray[2]), (((vector - vectorArray[0]) + vectorArray[1]) + vectorArray[2]), (((vector - vectorArray[0]) + vectorArray[1]) - vectorArray[2]), (((vector - vectorArray[0]) - vectorArray[1]) + vectorArray[2]), (((vector - vectorArray[0]) - vectorArray[1]) - vectorArray[2]) };
    }

    public OrientedBounds GetOrientedBounds()
    {
        Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
        OrientedBounds bounds = new OrientedBounds {
            Origin = base.transform.position + (localToWorldMatrix * this.m_offset)
        };
        bounds.Extents = new Vector3[] { localToWorldMatrix * new Vector3(this.m_size.x * 0.5f, 0f, 0f), localToWorldMatrix * new Vector3(0f, this.m_size.y * 0.5f, 0f), localToWorldMatrix * new Vector3(0f, 0f, this.m_size.z * 0.5f) };
        return bounds;
    }

    public void GetWorldBounds(out Vector3 min, out Vector3 max)
    {
        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3[] boundsPoints = this.GetBoundsPoints();
        for (int i = 0; i < 8; i++)
        {
            min.x = Mathf.Min(boundsPoints[i].x, min.x);
            min.y = Mathf.Min(boundsPoints[i].y, min.y);
            min.z = Mathf.Min(boundsPoints[i].z, min.z);
            max.x = Mathf.Max(boundsPoints[i].x, max.x);
            max.y = Mathf.Max(boundsPoints[i].y, max.y);
            max.z = Mathf.Max(boundsPoints[i].z, max.z);
        }
    }

    public bool IsActive()
    {
        if (this.m_activeStateCallback != null)
        {
            return this.m_activeStateCallback();
        }
        return ((this.m_active == ActiveState.Active) || ((this.m_active == ActiveState.UseHierarchy) && base.gameObject.activeInHierarchy));
    }

    public void SetCustomActiveState(ActiveStateCallback callback)
    {
        this.m_activeStateCallback = callback;
    }

    public enum ActiveState
    {
        Active,
        Inactive,
        UseHierarchy
    }

    public delegate bool ActiveStateCallback();
}

