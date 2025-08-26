using System;
using System.Runtime.InteropServices;

namespace GalArc.Models.Utils;

internal sealed class ResourceReader(string path) : IDisposable
{
    private readonly IntPtr hModule = NativeMethods.LoadLibraryEx(path, IntPtr.Zero, NativeMethods.LOAD_LIBRARY_AS_DATAFILE);

    public byte[] ReadResource(string name, string type)
    {
        IntPtr res = NativeMethods.FindResource(hModule, name, type);
        if (res == IntPtr.Zero)
        {
            return null;
        }

        IntPtr data = NativeMethods.LoadResource(hModule, res);
        if (data == IntPtr.Zero)
        {
            return null;
        }

        uint size = NativeMethods.SizeofResource(hModule, res);
        NativeMethods.LockResource(data);
        byte[] buffer = new byte[size];
        Marshal.Copy(data, buffer, 0, buffer.Length);
        return buffer;
    }

    public byte[] ReadResource(string name)
    {
        return ReadResource(name, NativeMethods.RT_RCDATA);
    }

    public void Dispose()
    {
        NativeMethods.FreeLibrary(hModule);
    }
}
