using System;
using UnityEngine;

[ExecuteInEditMode]
public class ThreeSliceElement : MonoBehaviour
{
    public Direction m_direction;
    private Bounds m_initialMiddleBounds;
    private Vector3 m_initialScale = Vector3.zero;
    public GameObject m_left;
    public Vector3 m_leftOffset;
    public GameObject m_middle;
    public Vector3 m_middleOffset;
    public float m_middleScale = 1f;
    public PinnedPoint m_pinnedPoint;
    public Vector3 m_pinnedPointOffset;
    public GameObject m_right;
    public Vector3 m_rightOffset;
    public float m_width;

    private void Awake()
    {
        if (this.m_middle != null)
        {
            this.SetInitialValues();
        }
    }

    public Vector3 GetMiddleSize()
    {
        return this.m_middle.GetComponent<Renderer>().bounds.size;
    }

    public Vector3 GetSize()
    {
        return this.GetSize(true);
    }

    public Vector3 GetSize(bool zIsHeight)
    {
        Vector3 size = this.m_left.GetComponent<Renderer>().bounds.size;
        Vector3 vector2 = this.m_middle.GetComponent<Renderer>().bounds.size;
        Vector3 vector3 = this.m_right.GetComponent<Renderer>().bounds.size;
        float x = (size.x + vector3.x) + vector2.x;
        float y = Mathf.Max(Mathf.Max(size.z, vector2.z), vector3.z);
        float z = Mathf.Max(Mathf.Max(size.y, vector2.y), vector3.y);
        if (zIsHeight)
        {
            return new Vector3(x, y, z);
        }
        return new Vector3(x, z, y);
    }

    public void SetInitialValues()
    {
        this.m_initialMiddleBounds = this.m_middle.GetComponent<Renderer>().bounds;
        this.m_initialScale = this.m_middle.transform.lossyScale;
        this.m_width = (this.m_middle.GetComponent<Renderer>().bounds.size.x + this.m_left.GetComponent<Renderer>().bounds.size.x) + this.m_right.GetComponent<Renderer>().bounds.size.x;
    }

    public void SetMiddleWidth(float globalWidth)
    {
        this.m_width = (globalWidth + this.m_left.GetComponent<Renderer>().bounds.size.x) + this.m_right.GetComponent<Renderer>().bounds.size.x;
        this.UpdateDisplay();
    }

    public void SetWidth(float globalWidth)
    {
        this.m_width = globalWidth;
        this.UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (base.enabled)
        {
            if (this.m_initialMiddleBounds.size == Vector3.zero)
            {
                this.m_initialMiddleBounds = this.m_middle.GetComponent<Renderer>().bounds;
            }
            float num = this.m_width - (this.m_left.GetComponent<Renderer>().bounds.size.x + this.m_right.GetComponent<Renderer>().bounds.size.x);
            switch (this.m_direction)
            {
                case Direction.X:
                {
                    Vector3 scale = TransformUtil.ComputeWorldScale(this.m_middle.transform);
                    scale.x = (this.m_initialScale.x * num) / this.m_initialMiddleBounds.size.x;
                    TransformUtil.SetWorldScale(this.m_middle.transform, scale);
                    break;
                }
            }
            switch (this.m_pinnedPoint)
            {
                case PinnedPoint.LEFT:
                    this.m_left.transform.localPosition = this.m_pinnedPointOffset;
                    TransformUtil.SetPoint(this.m_middle, Anchor.LEFT, this.m_left, Anchor.RIGHT, this.m_middleOffset);
                    TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT, this.m_rightOffset);
                    break;

                case PinnedPoint.MIDDLE:
                    this.m_middle.transform.localPosition = this.m_pinnedPointOffset;
                    TransformUtil.SetPoint(this.m_left, Anchor.RIGHT, this.m_middle, Anchor.LEFT, this.m_leftOffset);
                    TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT, this.m_rightOffset);
                    break;

                case PinnedPoint.RIGHT:
                    this.m_right.transform.localPosition = this.m_pinnedPointOffset;
                    TransformUtil.SetPoint(this.m_middle, Anchor.RIGHT, this.m_right, Anchor.LEFT, this.m_middleOffset);
                    TransformUtil.SetPoint(this.m_left, Anchor.RIGHT, this.m_middle, Anchor.LEFT, this.m_leftOffset);
                    break;
            }
        }
    }

    public enum Direction
    {
        X,
        Y,
        Z
    }

    public enum PinnedPoint
    {
        LEFT,
        MIDDLE,
        RIGHT,
        TOP,
        BOTTOM
    }
}

