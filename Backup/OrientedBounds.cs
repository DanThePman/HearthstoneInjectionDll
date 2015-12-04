using System;
using UnityEngine;

public class OrientedBounds
{
    public Vector3 CenterOffset;
    public Vector3[] Extents;
    public Vector3 Origin;

    public Vector3 GetTrueCenterPosition()
    {
        return (this.Origin + this.CenterOffset);
    }
}

