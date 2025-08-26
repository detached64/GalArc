using System;
using System.Runtime.InteropServices;

namespace GalArc.Models.Utils;

internal static partial class NativeMethods
{
    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial IntPtr FreeLibrary(IntPtr hModule);
    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr FindResource(IntPtr hModule, string lpName, string lpType);
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial IntPtr LockResource(IntPtr hResData);
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

    public const uint LOAD_LIBRARY_AS_DATAFILE = 0x02;
    public const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20;
    public const string RT_RCDATA = "#10";
}
