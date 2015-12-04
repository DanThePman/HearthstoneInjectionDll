using System;
using UnityEngine;

public class NineSliceElement : MultiSliceElement
{
    public GameObject m_bottom;
    public GameObject m_bottomLeft;
    public GameObject m_bottomRight;
    public MultiSliceElement.Direction m_HeightDirection = MultiSliceElement.Direction.Z;
    public GameObject m_left;
    public GameObject m_middle;
    public GameObject m_right;
    public GameObject m_top;
    public GameObject m_topLeft;
    public GameObject m_topRight;
    public MultiSliceElement.Direction m_WidthDirection;

    private OrientedBounds GetSliceBounds(GameObject slice)
    {
        if (slice != null)
        {
            return TransformUtil.ComputeOrientedWorldBounds(slice, base.m_ignore, true);
        }
        OrientedBounds bounds = new OrientedBounds();
        bounds.Extents = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
        bounds.Origin = Vector3.zero;
        bounds.CenterOffset = Vector3.zero;
        return bounds;
    }

    public Vector2 GetWorldDimensions()
    {
        OrientedBounds bounds = TransformUtil.ComputeOrientedWorldBounds(this.m_middle, base.m_ignore, true);
        return new Vector2(bounds.Extents[(int) this.m_WidthDirection].magnitude * 2f, bounds.Extents[(int) this.m_HeightDirection].magnitude * 2f);
    }

    public void SetEntireHeight(float height)
    {
        int heightDirection = (int) this.m_HeightDirection;
        OrientedBounds sliceBounds = this.GetSliceBounds(this.m_topLeft);
        OrientedBounds bounds2 = this.GetSliceBounds(this.m_top);
        OrientedBounds bounds3 = this.GetSliceBounds(this.m_topRight);
        OrientedBounds bounds4 = this.GetSliceBounds(this.m_bottomLeft);
        OrientedBounds bounds5 = this.GetSliceBounds(this.m_bottom);
        OrientedBounds bounds6 = this.GetSliceBounds(this.m_bottomRight);
        float[] values = new float[] { sliceBounds.Extents[heightDirection].magnitude, bounds2.Extents[heightDirection].magnitude, bounds3.Extents[heightDirection].magnitude };
        float num2 = Mathf.Max(values) * 2f;
        float[] singleArray2 = new float[] { bounds4.Extents[heightDirection].magnitude, bounds5.Extents[heightDirection].magnitude, bounds6.Extents[heightDirection].magnitude };
        float num3 = Mathf.Max(singleArray2) * 2f;
        this.SetHeight((height - num2) - num3);
    }

    public void SetEntireSize(Vector2 size)
    {
        this.SetEntireSize(size.x, size.y);
    }

    public void SetEntireSize(float width, float height)
    {
        int widthDirection = (int) this.m_WidthDirection;
        int heightDirection = (int) this.m_HeightDirection;
        OrientedBounds sliceBounds = this.GetSliceBounds(this.m_topLeft);
        OrientedBounds bounds2 = this.GetSliceBounds(this.m_top);
        OrientedBounds bounds3 = this.GetSliceBounds(this.m_topRight);
        OrientedBounds bounds4 = this.GetSliceBounds(this.m_left);
        OrientedBounds bounds5 = this.GetSliceBounds(this.m_right);
        OrientedBounds bounds6 = this.GetSliceBounds(this.m_bottomLeft);
        OrientedBounds bounds7 = this.GetSliceBounds(this.m_bottom);
        OrientedBounds bounds8 = this.GetSliceBounds(this.m_bottomRight);
        float[] values = new float[] { sliceBounds.Extents[widthDirection].magnitude, bounds4.Extents[widthDirection].magnitude, bounds6.Extents[widthDirection].magnitude };
        float num3 = Mathf.Max(values) * 2f;
        float[] singleArray2 = new float[] { bounds3.Extents[widthDirection].magnitude, bounds5.Extents[widthDirection].magnitude, bounds8.Extents[widthDirection].magnitude };
        float num4 = Mathf.Max(singleArray2) * 2f;
        float[] singleArray3 = new float[] { sliceBounds.Extents[heightDirection].magnitude, bounds2.Extents[heightDirection].magnitude, bounds3.Extents[heightDirection].magnitude };
        float num5 = Mathf.Max(singleArray3) * 2f;
        float[] singleArray4 = new float[] { bounds6.Extents[heightDirection].magnitude, bounds7.Extents[heightDirection].magnitude, bounds8.Extents[heightDirection].magnitude };
        float num6 = Mathf.Max(singleArray4) * 2f;
        this.SetSize((width - num3) - num4, (height - num5) - num6);
    }

    public void SetEntireWidth(float width)
    {
        int widthDirection = (int) this.m_WidthDirection;
        OrientedBounds sliceBounds = this.GetSliceBounds(this.m_topLeft);
        OrientedBounds bounds2 = this.GetSliceBounds(this.m_left);
        OrientedBounds bounds3 = this.GetSliceBounds(this.m_bottomLeft);
        OrientedBounds bounds4 = this.GetSliceBounds(this.m_topRight);
        OrientedBounds bounds5 = this.GetSliceBounds(this.m_right);
        OrientedBounds bounds6 = this.GetSliceBounds(this.m_bottomRight);
        float[] values = new float[] { sliceBounds.Extents[widthDirection].magnitude, bounds2.Extents[widthDirection].magnitude, bounds3.Extents[widthDirection].magnitude };
        float num2 = Mathf.Max(values) * 2f;
        float[] singleArray2 = new float[] { bounds4.Extents[widthDirection].magnitude, bounds5.Extents[widthDirection].magnitude, bounds6.Extents[widthDirection].magnitude };
        float num3 = Mathf.Max(singleArray2) * 2f;
        this.SetWidth((width - num2) - num3);
    }

    public void SetHeight(float height)
    {
        height = Mathf.Max(height, 0f);
        int heightDirection = (int) this.m_HeightDirection;
        WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex(height, heightDirection) };
        this.SetSliceSize(this.m_left, dimensions);
        WorldDimensionIndex[] indexArray2 = new WorldDimensionIndex[] { new WorldDimensionIndex(height, heightDirection) };
        this.SetSliceSize(this.m_right, indexArray2);
        WorldDimensionIndex[] indexArray3 = new WorldDimensionIndex[] { new WorldDimensionIndex(height, heightDirection) };
        this.SetSliceSize(this.m_middle, indexArray3);
        this.UpdateAllSlices();
    }

    public void SetMiddleScale(float scaleWidth, float scaleHeight)
    {
        Vector3 localScale = this.m_middle.transform.localScale;
        localScale[(int) this.m_WidthDirection] = scaleWidth;
        localScale[(int) this.m_HeightDirection] = scaleHeight;
        this.m_middle.transform.localScale = localScale;
        this.UpdateSegmentsToMatchMiddle();
        this.UpdateAllSlices();
    }

    public void SetSize(Vector2 size)
    {
        this.SetSize(size.x, size.y);
    }

    public void SetSize(float width, float height)
    {
        width = Mathf.Max(width, 0f);
        height = Mathf.Max(height, 0f);
        int widthDirection = (int) this.m_WidthDirection;
        int heightDirection = (int) this.m_HeightDirection;
        WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex(width, widthDirection) };
        this.SetSliceSize(this.m_top, dimensions);
        WorldDimensionIndex[] indexArray2 = new WorldDimensionIndex[] { new WorldDimensionIndex(width, widthDirection) };
        this.SetSliceSize(this.m_bottom, indexArray2);
        WorldDimensionIndex[] indexArray3 = new WorldDimensionIndex[] { new WorldDimensionIndex(height, heightDirection) };
        this.SetSliceSize(this.m_left, indexArray3);
        WorldDimensionIndex[] indexArray4 = new WorldDimensionIndex[] { new WorldDimensionIndex(height, heightDirection) };
        this.SetSliceSize(this.m_right, indexArray4);
        WorldDimensionIndex[] indexArray5 = new WorldDimensionIndex[] { new WorldDimensionIndex(width, widthDirection), new WorldDimensionIndex(height, heightDirection) };
        this.SetSliceSize(this.m_middle, indexArray5);
        this.UpdateAllSlices();
    }

    private void SetSliceSize(GameObject slice, params WorldDimensionIndex[] dimensions)
    {
        if (slice != null)
        {
            TransformUtil.SetLocalScaleToWorldDimension(slice, base.m_ignore, dimensions);
        }
    }

    public void SetWidth(float width)
    {
        width = Mathf.Max(width, 0f);
        int widthDirection = (int) this.m_WidthDirection;
        WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex(width, widthDirection) };
        this.SetSliceSize(this.m_top, dimensions);
        WorldDimensionIndex[] indexArray2 = new WorldDimensionIndex[] { new WorldDimensionIndex(width, widthDirection) };
        this.SetSliceSize(this.m_bottom, indexArray2);
        WorldDimensionIndex[] indexArray3 = new WorldDimensionIndex[] { new WorldDimensionIndex(width, widthDirection) };
        this.SetSliceSize(this.m_middle, indexArray3);
        this.UpdateAllSlices();
    }

    private void UpdateAllSlices()
    {
        for (int i = 0; i < base.m_slices.Count; i++)
        {
            if (base.m_slices[i].m_slice != null)
            {
                MultiSliceElement[] componentsInChildren = base.m_slices[i].m_slice.GetComponentsInChildren<MultiSliceElement>();
                for (int j = 0; j < componentsInChildren.Length; j++)
                {
                    componentsInChildren[j].UpdateSlices();
                }
            }
        }
        base.UpdateSlices();
    }

    private void UpdateSegmentsToMatchMiddle()
    {
        OrientedBounds bounds = TransformUtil.ComputeOrientedWorldBounds(this.m_middle, base.m_ignore, true);
        if (bounds != null)
        {
            float dimension = bounds.Extents[(int) this.m_WidthDirection].magnitude * 2f;
            float num2 = bounds.Extents[(int) this.m_HeightDirection].magnitude * 2f;
            int widthDirection = (int) this.m_WidthDirection;
            int heightDirection = (int) this.m_HeightDirection;
            WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex(dimension, widthDirection) };
            this.SetSliceSize(this.m_top, dimensions);
            WorldDimensionIndex[] indexArray2 = new WorldDimensionIndex[] { new WorldDimensionIndex(dimension, widthDirection) };
            this.SetSliceSize(this.m_bottom, indexArray2);
            WorldDimensionIndex[] indexArray3 = new WorldDimensionIndex[] { new WorldDimensionIndex(num2, heightDirection) };
            this.SetSliceSize(this.m_left, indexArray3);
            WorldDimensionIndex[] indexArray4 = new WorldDimensionIndex[] { new WorldDimensionIndex(num2, heightDirection) };
            this.SetSliceSize(this.m_right, indexArray4);
        }
    }
}

