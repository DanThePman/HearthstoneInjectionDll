using System;
using UnityEngine;

[ExecuteInEditMode]
public class NewNineSliceElement : MonoBehaviour
{
    public GameObject m_anchorBone;
    public GameObject m_bottom;
    public GameObject m_bottomLeft;
    public GameObject m_bottomRight;
    public GameObject m_left;
    public GameObject m_middle;
    public Vector2 m_middleScale;
    public Mode m_mode;
    public PinnedPoint m_pinnedPoint = PinnedPoint.TOP;
    public Vector3 m_pinnedPointOffset;
    public PlaneAxis m_planeAxis = PlaneAxis.XZ;
    public GameObject m_right;
    public Vector2 m_size;
    public GameObject m_top;
    public GameObject m_topLeft;
    public GameObject m_topRight;

    private void SetPieceHeight(GameObject piece, float height)
    {
        Vector3 localScale = piece.transform.localScale;
        localScale.z = (height * piece.transform.localScale.z) / piece.GetComponent<Renderer>().bounds.size.y;
        piece.transform.localScale = localScale;
    }

    private void SetPieceWidth(GameObject piece, float width)
    {
        Vector3 localScale = piece.transform.localScale;
        localScale.x = (width * piece.transform.localScale.x) / piece.GetComponent<Renderer>().bounds.size.x;
        piece.transform.localScale = localScale;
    }

    public virtual void SetSize(float width, float height)
    {
        Vector3 vector3;
        Vector3 vector4;
        Vector3 vector5;
        Vector3 vector6;
        Vector3 vector7;
        if (this.m_mode == Mode.UseSize)
        {
            this.m_size = new Vector2(width, height);
            Vector3 size = this.m_topLeft.GetComponent<Renderer>().bounds.size;
            Vector3 vector2 = this.m_bottomLeft.GetComponent<Renderer>().bounds.size;
            width = Mathf.Max((float) (width - (size.x + vector2.x)), (float) 1f);
            height = Mathf.Max((float) (height - (size.y + vector2.y)), (float) 1f);
            this.SetPieceWidth(this.m_middle, width);
            this.SetPieceHeight(this.m_middle, height);
            this.SetPieceWidth(this.m_top, width);
            this.SetPieceWidth(this.m_bottom, width);
            this.SetPieceHeight(this.m_left, height);
            this.SetPieceHeight(this.m_right, height);
        }
        else
        {
            TransformUtil.SetLocalScaleXZ(this.m_middle, new Vector2(width, height));
            TransformUtil.SetLocalScaleX(this.m_top, width);
            TransformUtil.SetLocalScaleX(this.m_bottom, width);
            TransformUtil.SetLocalScaleZ(this.m_left, height);
            TransformUtil.SetLocalScaleZ(this.m_right, height);
        }
        if (this.m_planeAxis == PlaneAxis.XZ)
        {
            vector3 = new Vector3(0f, 0f, 1f);
            vector4 = new Vector3(0f, 0f, 0f);
            vector5 = new Vector3(0f, 0f, 0.5f);
            vector6 = new Vector3(1f, 0f, 0.5f);
            vector7 = new Vector3(0.5f, 0f, 0.5f);
        }
        else
        {
            vector3 = new Vector3(0f, 1f, 0f);
            vector4 = new Vector3(0f, 0f, 0f);
            vector5 = new Vector3(0f, 0.5f, 0f);
            vector6 = new Vector3(1f, 0.5f, 0f);
            vector7 = new Vector3(0.5f, 0.5f, 0f);
        }
        switch (this.m_pinnedPoint)
        {
            case PinnedPoint.TOPLEFT:
                TransformUtil.SetPoint(this.m_topLeft, Anchor.TOP_LEFT, this.m_anchorBone, Anchor.TOP_LEFT);
                TransformUtil.SetPoint(this.m_top, Anchor.LEFT, this.m_topLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topRight, Anchor.LEFT, this.m_top, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_left, vector3, this.m_topLeft, vector4);
                TransformUtil.SetPoint(this.m_middle, Anchor.LEFT, this.m_left, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomLeft, vector3, this.m_left, vector4);
                TransformUtil.SetPoint(this.m_bottom, Anchor.LEFT, this.m_bottomLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.LEFT, this.m_bottom, Anchor.RIGHT);
                break;

            case PinnedPoint.TOP:
                TransformUtil.SetPoint(this.m_top, Anchor.TOP, this.m_anchorBone, Anchor.TOP);
                TransformUtil.SetPoint(this.m_topLeft, Anchor.RIGHT, this.m_top, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_topRight, Anchor.LEFT, this.m_top, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_left, vector3, this.m_topLeft, vector4);
                TransformUtil.SetPoint(this.m_middle, Anchor.LEFT, this.m_left, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomLeft, vector3, this.m_left, vector4);
                TransformUtil.SetPoint(this.m_bottom, Anchor.LEFT, this.m_bottomLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.LEFT, this.m_bottom, Anchor.RIGHT);
                break;

            case PinnedPoint.TOPRIGHT:
                TransformUtil.SetPoint(this.m_topRight, Anchor.TOP_RIGHT, this.m_anchorBone, Anchor.TOP_RIGHT);
                TransformUtil.SetPoint(this.m_top, Anchor.RIGHT, this.m_topRight, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_topLeft, Anchor.RIGHT, this.m_top, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_left, vector3, this.m_topLeft, vector4);
                TransformUtil.SetPoint(this.m_middle, Anchor.LEFT, this.m_left, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomLeft, vector3, this.m_left, vector4);
                TransformUtil.SetPoint(this.m_bottom, Anchor.LEFT, this.m_bottomLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.LEFT, this.m_bottom, Anchor.RIGHT);
                break;

            case PinnedPoint.LEFT:
                TransformUtil.SetPoint(this.m_left, vector5, this.m_anchorBone, vector5);
                TransformUtil.SetPoint(this.m_middle, Anchor.LEFT, this.m_left, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topLeft, vector4, this.m_left, vector3);
                TransformUtil.SetPoint(this.m_top, Anchor.LEFT, this.m_topLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topRight, Anchor.LEFT, this.m_top, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomLeft, vector3, this.m_left, vector4);
                TransformUtil.SetPoint(this.m_bottom, Anchor.LEFT, this.m_bottomLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.LEFT, this.m_bottom, Anchor.RIGHT);
                break;

            case PinnedPoint.MIDDLE:
                TransformUtil.SetPoint(this.m_middle, vector7, this.m_anchorBone, vector7);
                TransformUtil.SetPoint(this.m_left, Anchor.RIGHT, this.m_middle, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topLeft, vector4, this.m_left, vector3);
                TransformUtil.SetPoint(this.m_top, Anchor.LEFT, this.m_topLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topRight, Anchor.LEFT, this.m_top, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomLeft, vector3, this.m_left, vector4);
                TransformUtil.SetPoint(this.m_bottom, Anchor.LEFT, this.m_bottomLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.LEFT, this.m_bottom, Anchor.RIGHT);
                break;

            case PinnedPoint.RIGHT:
                TransformUtil.SetPoint(this.m_right, vector6, this.m_anchorBone, vector6);
                TransformUtil.SetPoint(this.m_middle, Anchor.RIGHT, this.m_right, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_left, Anchor.RIGHT, this.m_middle, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_topLeft, vector4, this.m_left, vector3);
                TransformUtil.SetPoint(this.m_top, Anchor.LEFT, this.m_topLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topRight, Anchor.LEFT, this.m_top, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomLeft, vector3, this.m_left, vector4);
                TransformUtil.SetPoint(this.m_bottom, Anchor.LEFT, this.m_bottomLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.LEFT, this.m_bottom, Anchor.RIGHT);
                break;

            case PinnedPoint.BOTTOMLEFT:
                TransformUtil.SetPoint(this.m_bottomLeft, Anchor.BOTTOM_LEFT, this.m_anchorBone, Anchor.BOTTOM_LEFT);
                TransformUtil.SetPoint(this.m_bottom, Anchor.LEFT, this.m_bottomLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.LEFT, this.m_bottom, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_left, vector4, this.m_bottomLeft, vector3);
                TransformUtil.SetPoint(this.m_middle, Anchor.LEFT, this.m_left, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topLeft, vector4, this.m_left, vector3);
                TransformUtil.SetPoint(this.m_top, Anchor.LEFT, this.m_topLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topRight, Anchor.LEFT, this.m_top, Anchor.RIGHT);
                break;

            case PinnedPoint.BOTTOM:
                TransformUtil.SetPoint(this.m_bottom, Anchor.BOTTOM, this.m_anchorBone, Anchor.BOTTOM);
                TransformUtil.SetPoint(this.m_bottomLeft, Anchor.RIGHT, this.m_bottom, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.LEFT, this.m_bottom, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_left, vector4, this.m_bottomLeft, vector3);
                TransformUtil.SetPoint(this.m_middle, Anchor.LEFT, this.m_left, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topLeft, vector4, this.m_left, vector3);
                TransformUtil.SetPoint(this.m_top, Anchor.LEFT, this.m_topLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topRight, Anchor.LEFT, this.m_top, Anchor.RIGHT);
                break;

            case PinnedPoint.BOTTOMRIGHT:
                TransformUtil.SetPoint(this.m_bottomRight, Anchor.BOTTOM_RIGHT, this.m_anchorBone, Anchor.BOTTOM_RIGHT);
                TransformUtil.SetPoint(this.m_bottom, Anchor.RIGHT, this.m_bottomRight, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_bottomLeft, Anchor.RIGHT, this.m_bottom, Anchor.LEFT);
                TransformUtil.SetPoint(this.m_left, vector4, this.m_bottomLeft, vector3);
                TransformUtil.SetPoint(this.m_middle, Anchor.LEFT, this.m_left, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_right, Anchor.LEFT, this.m_middle, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topLeft, vector4, this.m_left, vector3);
                TransformUtil.SetPoint(this.m_top, Anchor.LEFT, this.m_topLeft, Anchor.RIGHT);
                TransformUtil.SetPoint(this.m_topRight, Anchor.LEFT, this.m_top, Anchor.RIGHT);
                break;
        }
    }

    public enum Mode
    {
        UseMiddleScale,
        UseSize
    }

    public enum PinnedPoint
    {
        TOPLEFT,
        TOP,
        TOPRIGHT,
        LEFT,
        MIDDLE,
        RIGHT,
        BOTTOMLEFT,
        BOTTOM,
        BOTTOMRIGHT
    }

    public enum PlaneAxis
    {
        XY,
        XZ
    }
}

