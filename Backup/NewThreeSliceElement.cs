using System;
using UnityEngine;

[ExecuteInEditMode]
public class NewThreeSliceElement : MonoBehaviour
{
    private Vector3 m_bottomAnchor;
    public Direction m_direction;
    private Transform m_identity;
    private Vector3 m_leftAnchor;
    public Vector3 m_leftOffset;
    public GameObject m_leftOrTop;
    public GameObject m_middle;
    public Vector3 m_middleOffset;
    public Vector3 m_middleScale = Vector3.one;
    public PinnedPoint m_pinnedPoint;
    public Vector3 m_pinnedPointOffset;
    public PlaneAxis m_planeAxis = PlaneAxis.XZ;
    private Vector3 m_rightAnchor;
    public Vector3 m_rightOffset;
    public GameObject m_rightOrBottom;
    private Vector3 m_topAnchor;

    private void DisplayOnXAxis()
    {
        switch (this.m_pinnedPoint)
        {
            case PinnedPoint.LEFT:
                this.m_leftOrTop.transform.localPosition = this.m_pinnedPointOffset;
                TransformUtil.SetPoint(this.m_middle, this.m_leftAnchor, this.m_leftOrTop, this.m_rightAnchor, this.m_identity.transform.TransformPoint(this.m_middleOffset));
                TransformUtil.SetPoint(this.m_rightOrBottom, this.m_leftAnchor, this.m_middle, this.m_rightAnchor, this.m_identity.transform.TransformPoint(this.m_rightOffset));
                break;

            case PinnedPoint.MIDDLE:
                this.m_middle.transform.localPosition = this.m_pinnedPointOffset;
                TransformUtil.SetPoint(this.m_leftOrTop, this.m_rightAnchor, this.m_middle, this.m_leftAnchor, this.m_identity.transform.TransformPoint(this.m_leftOffset));
                TransformUtil.SetPoint(this.m_rightOrBottom, this.m_leftAnchor, this.m_middle, this.m_rightAnchor, this.m_identity.transform.TransformPoint(this.m_rightOffset));
                break;

            case PinnedPoint.RIGHT:
                this.m_rightOrBottom.transform.localPosition = this.m_pinnedPointOffset;
                TransformUtil.SetPoint(this.m_middle, this.m_rightAnchor, this.m_rightOrBottom, this.m_leftAnchor, this.m_identity.transform.TransformPoint(this.m_middleOffset));
                TransformUtil.SetPoint(this.m_leftOrTop, this.m_rightAnchor, this.m_middle, this.m_leftAnchor, this.m_identity.transform.TransformPoint(this.m_leftOffset));
                break;
        }
    }

    private void DisplayOnYAxis()
    {
    }

    private void DisplayOnZAxis()
    {
        switch (this.m_pinnedPoint)
        {
            case PinnedPoint.MIDDLE:
                this.m_middle.transform.localPosition = this.m_pinnedPointOffset;
                TransformUtil.SetPoint(this.m_leftOrTop, this.m_bottomAnchor, this.m_middle, this.m_topAnchor, this.m_identity.transform.TransformPoint(this.m_leftOffset));
                TransformUtil.SetPoint(this.m_rightOrBottom, this.m_topAnchor, this.m_middle, this.m_bottomAnchor, this.m_identity.transform.TransformPoint(this.m_rightOffset));
                break;

            case PinnedPoint.TOP:
                this.m_leftOrTop.transform.localPosition = this.m_pinnedPointOffset;
                TransformUtil.SetPoint(this.m_middle, this.m_topAnchor, this.m_leftOrTop, this.m_bottomAnchor, this.m_identity.transform.TransformPoint(this.m_middleOffset));
                TransformUtil.SetPoint(this.m_rightOrBottom, this.m_topAnchor, this.m_middle, this.m_bottomAnchor, this.m_identity.transform.TransformPoint(this.m_rightOffset));
                break;

            case PinnedPoint.BOTTOM:
                this.m_rightOrBottom.transform.localPosition = this.m_pinnedPointOffset;
                TransformUtil.SetPoint(this.m_middle, this.m_bottomAnchor, this.m_rightOrBottom, this.m_topAnchor, this.m_identity.transform.TransformPoint(this.m_middleOffset));
                TransformUtil.SetPoint(this.m_leftOrTop, this.m_bottomAnchor, this.m_middle, this.m_topAnchor, this.m_identity.transform.TransformPoint(this.m_leftOffset));
                break;
        }
    }

    private void OnDestroy()
    {
        if ((this.m_identity != null) && (this.m_identity.gameObject != null))
        {
            UnityEngine.Object.DestroyImmediate(this.m_identity.gameObject);
        }
    }

    public virtual void SetSize(Vector3 size)
    {
        this.m_middle.transform.localScale = size;
        if (this.m_identity == null)
        {
            this.m_identity = new GameObject().transform;
        }
        this.m_identity.position = Vector3.zero;
        if (this.m_planeAxis == PlaneAxis.XZ)
        {
            this.m_leftAnchor = new Vector3(0f, 0f, 0.5f);
            this.m_rightAnchor = new Vector3(1f, 0f, 0.5f);
            this.m_topAnchor = new Vector3(0.5f, 0f, 1f);
            this.m_bottomAnchor = new Vector3(0.5f, 0f, 0f);
        }
        else
        {
            this.m_leftAnchor = new Vector3(0f, 0.5f, 0f);
            this.m_rightAnchor = new Vector3(1f, 0.5f, 0f);
            this.m_topAnchor = new Vector3(0.5f, 0f, 0f);
            this.m_bottomAnchor = new Vector3(0.5f, 1f, 0f);
        }
        switch (this.m_direction)
        {
            case Direction.X:
                this.DisplayOnXAxis();
                break;

            case Direction.Z:
                this.DisplayOnZAxis();
                break;
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

    public enum PlaneAxis
    {
        XY,
        XZ
    }
}

