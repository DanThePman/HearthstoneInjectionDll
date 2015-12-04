using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct Link
{
    public int HashCode;
    public int Next;
}

