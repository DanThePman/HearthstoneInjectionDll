using System;
using System.Runtime.InteropServices;
using System.Text;

public static class MemUtils
{
    public static void FreePtr(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public static IntPtr PtrFromBytes(byte[] bytes)
    {
        return PtrFromBytes(bytes, 0);
    }

    public static IntPtr PtrFromBytes(byte[] bytes, int offset)
    {
        if (bytes == null)
        {
            return IntPtr.Zero;
        }
        int len = bytes.Length - offset;
        return PtrFromBytes(bytes, offset, len);
    }

    public static IntPtr PtrFromBytes(byte[] bytes, int offset, int len)
    {
        if (bytes == null)
        {
            return IntPtr.Zero;
        }
        if (len <= 0)
        {
            return IntPtr.Zero;
        }
        IntPtr destination = Marshal.AllocHGlobal(len);
        Marshal.Copy(bytes, offset, destination, len);
        return destination;
    }

    public static byte[] PtrToBytes(IntPtr ptr, int size)
    {
        if (ptr == IntPtr.Zero)
        {
            return null;
        }
        if (size == 0)
        {
            return null;
        }
        byte[] destination = new byte[size];
        Marshal.Copy(ptr, destination, 0, size);
        return destination;
    }

    public static string StringFromUtf8Ptr(IntPtr ptr)
    {
        int num;
        return StringFromUtf8Ptr(ptr, out num);
    }

    public static string StringFromUtf8Ptr(IntPtr ptr, out int len)
    {
        len = 0;
        if (ptr == IntPtr.Zero)
        {
            return null;
        }
        len = StringPtrByteLen(ptr);
        if (len == 0)
        {
            return null;
        }
        byte[] destination = new byte[len];
        Marshal.Copy(ptr, destination, 0, len);
        return Encoding.UTF8.GetString(destination);
    }

    public static int StringPtrByteLen(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
        {
            return 0;
        }
        int ofs = 0;
        while (Marshal.ReadByte(ptr, ofs) != 0)
        {
            ofs++;
        }
        return ofs;
    }

    public static T StructFromBytes<T>(byte[] bytes)
    {
        return StructFromBytes<T>(bytes, 0);
    }

    public static T StructFromBytes<T>(byte[] bytes, int offset)
    {
        System.Type t = typeof(T);
        int cb = Marshal.SizeOf(t);
        if (bytes == null)
        {
            return default(T);
        }
        if ((bytes.Length - offset) < cb)
        {
            return default(T);
        }
        IntPtr destination = Marshal.AllocHGlobal(cb);
        Marshal.Copy(bytes, offset, destination, cb);
        T local = (T) Marshal.PtrToStructure(destination, t);
        Marshal.FreeHGlobal(destination);
        return local;
    }

    public static byte[] StructToBytes<T>(T t)
    {
        int cb = Marshal.SizeOf(typeof(T));
        byte[] destination = new byte[cb];
        IntPtr ptr = Marshal.AllocHGlobal(cb);
        Marshal.StructureToPtr(t, ptr, true);
        Marshal.Copy(ptr, destination, 0, cb);
        Marshal.FreeHGlobal(ptr);
        return destination;
    }

    public static IntPtr Utf8PtrFromString(string managedString)
    {
        if (managedString == null)
        {
            return IntPtr.Zero;
        }
        int cb = 1 + Encoding.UTF8.GetByteCount(managedString);
        byte[] bytes = new byte[cb];
        Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, bytes, 0);
        IntPtr destination = Marshal.AllocHGlobal(cb);
        Marshal.Copy(bytes, 0, destination, cb);
        return destination;
    }
}

