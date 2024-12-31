// File: Utility/NativeMethods.cs
// Date: 2024/12/31
// Description: Native methods.
//
// Copyright (C) 2024 detached64
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Runtime.InteropServices;

namespace Utility
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
        [DllImport("kernel32.dll")]
        public static extern IntPtr FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LockResource(IntPtr hResData);
        [DllImport("kernel32.dll")]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        public const uint LOAD_LIBRARY_AS_DATAFILE = 0x02;
        public const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20;
        public const string RT_RCDATA = "#10";
    }
}
