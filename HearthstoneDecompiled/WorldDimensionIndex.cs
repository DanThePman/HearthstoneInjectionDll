using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct WorldDimensionIndex
{
    public float Dimension;
    public int Index;
    public WorldDimensionIndex(float dimension, int index)
    {
        this.Dimension = dimension;
        this.Index = index;
    }
}

