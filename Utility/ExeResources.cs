// File: Utility/ExeResources.cs
// Date: 2024/12/31
// Description: A class to read resources from an executable file.
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
    public class ResourceReader : IDisposable
    {
        private IntPtr hModule;

        public ResourceReader(string path)
        {
            hModule = NativeMethods.LoadLibraryEx(path, IntPtr.Zero, NativeMethods.LOAD_LIBRARY_AS_DATAFILE);
        }

        public byte[] ReadResource(string name, string type)
        {
            IntPtr res = NativeMethods.FindResource(hModule, name, type);
            if (res == IntPtr.Zero)
                return null;
            IntPtr data = NativeMethods.LoadResource(hModule, res);
            if (data == IntPtr.Zero)
                return null;
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
}
