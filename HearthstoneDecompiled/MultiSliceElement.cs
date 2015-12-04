using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass, ExecuteInEditMode]
public class MultiSliceElement : MonoBehaviour
{
    public Direction m_direction;
    public List<GameObject> m_ignore = new List<GameObject>();
    public Vector3 m_localPinnedPointOffset = Vector3.zero;
    public Vector3 m_localSliceSpacing = Vector3.zero;
    public bool m_reverse;
    [CustomEditField(ListTable=true)]
    public List<Slice> m_slices = new List<Slice>();
    public bool m_useUberText;
    public XAxisAlign m_XAlign;
    public YAxisAlign m_YAlign = YAxisAlign.BOTTOM;
    public ZAxisAlign m_ZAlign = ZAxisAlign.BACK;

    public void AddSlice(GameObject obj)
    {
        this.AddSlice(obj, Vector3.zero, Vector3.zero, false);
    }

    public void AddSlice(GameObject obj, Vector3 minLocalPadding, Vector3 maxLocalPadding, bool reverse = false)
    {
        Slice item = new Slice {
            m_slice = obj,
            m_minLocalPadding = minLocalPadding,
            m_maxLocalPadding = maxLocalPadding,
            m_reverse = reverse
        };
        this.m_slices.Add(item);
    }

    public void ClearSlices()
    {
        this.m_slices.Clear();
    }

    private Vector3 GetAlignmentVector(Vector3 interpolate)
    {
        return new Vector3(interpolate.x * (((float) this.m_XAlign) * 0.5f), interpolate.y * (((float) this.m_YAlign) * 0.5f), interpolate.z * (((float) this.m_ZAlign) * 0.5f));
    }

    public void UpdateSlices()
    {
        if (this.m_slices.Count != 0)
        {
            float num = !this.m_reverse ? 1f : -1f;
            int direction = (int) this.m_direction;
            Slice[] sliceArray = this.m_slices.FindAll(s => TransformUtil.CanComputeOrientedWorldBounds(s.m_slice, this.m_useUberText, this.m_ignore, true)).ToArray();
            if (sliceArray.Length != 0)
            {
                Vector3 zero = Vector3.zero;
                Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                Slice slice = sliceArray[0];
                GameObject go = slice.m_slice;
                go.transform.localPosition = Vector3.zero;
                OrientedBounds bounds = TransformUtil.ComputeOrientedWorldBounds(go, this.m_useUberText, slice.m_minLocalPadding, slice.m_maxLocalPadding, this.m_ignore, true);
                float num3 = num * (!slice.m_reverse ? 1f : -1f);
                Vector3 vector4 = (Vector3) (((bounds.Extents[0] + bounds.Extents[1]) + bounds.Extents[2]) * num3);
                Transform transform = go.transform;
                transform.position += bounds.CenterOffset + vector4;
                zero = ((Vector3) (bounds.Extents[direction] * num3)) + vector4;
                TransformUtil.GetBoundsMinMax((Vector3) (worldToLocalMatrix * (go.transform.position - bounds.CenterOffset)), (Vector3) (worldToLocalMatrix * bounds.Extents[0]), (Vector3) (worldToLocalMatrix * bounds.Extents[1]), (Vector3) (worldToLocalMatrix * bounds.Extents[2]), ref min, ref max);
                Vector3 vector5 = (Vector3) (this.m_localSliceSpacing * num);
                for (int i = 1; i < sliceArray.Length; i++)
                {
                    Slice slice2 = sliceArray[i];
                    GameObject obj3 = slice2.m_slice;
                    float num5 = num * (!slice2.m_reverse ? 1f : -1f);
                    obj3.transform.localPosition = Vector3.zero;
                    OrientedBounds bounds2 = TransformUtil.ComputeOrientedWorldBounds(obj3, this.m_useUberText, slice2.m_minLocalPadding, slice2.m_maxLocalPadding, this.m_ignore, true);
                    Vector3 vector6 = (Vector3) (obj3.transform.localToWorldMatrix * vector5);
                    Vector3 vector7 = (Vector3) (bounds2.Extents[direction] * num5);
                    Transform transform2 = obj3.transform;
                    transform2.position += ((bounds2.CenterOffset + zero) + vector7) + vector6;
                    zero += ((Vector3) (vector7 * 2f)) + vector6;
                    TransformUtil.GetBoundsMinMax((Vector3) (worldToLocalMatrix * (obj3.transform.position - bounds2.CenterOffset)), (Vector3) (worldToLocalMatrix * bounds2.Extents[0]), (Vector3) (worldToLocalMatrix * bounds2.Extents[1]), (Vector3) (worldToLocalMatrix * bounds2.Extents[2]), ref min, ref max);
                }
                Vector3 vector8 = new Vector3(min.x, max.y, min.z);
                Vector3 vector9 = new Vector3(max.x, min.y, max.z);
                Vector3 vector10 = (Vector3) (base.transform.localToWorldMatrix * (vector8 + this.GetAlignmentVector(vector9 - vector8)));
                Vector3 vector11 = (Vector3) ((base.transform.localToWorldMatrix * this.m_localPinnedPointOffset) * num);
                Vector3 vector12 = base.transform.position - vector10;
                Vector3 vector13 = vector11 + vector12;
                foreach (Slice slice3 in sliceArray)
                {
                    Transform transform3 = slice3.m_slice.transform;
                    transform3.position += vector13;
                }
            }
        }
    }

    public enum Direction
    {
        X,
        Y,
        Z
    }

    [Serializable]
    public class Slice
    {
        public Vector3 m_maxLocalPadding;
        public Vector3 m_minLocalPadding;
        public bool m_reverse;
        public GameObject m_slice;
    }

    public enum XAxisAlign
    {
        LEFT,
        MIDDLE,
        RIGHT
    }

    public enum YAxisAlign
    {
        TOP,
        MIDDLE,
        BOTTOM
    }

    public enum ZAxisAlign
    {
        FRONT,
        MIDDLE,
        BACK
    }
}

