// File: Utility/WindowsSorter.cs
// Date: 2024/10/28
// Description: Windows sort comparer.
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

using System.Collections;
using System.Runtime.InteropServices;

namespace Utility
{
    public class WindowsSorter : IComparer
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string param1, string param2);

        public int Compare(object name1, object name2)
        {
            if (name1 == null && name2 == null)
            {
                return 0;
            }
            if (name1 == null)
            {
                return -1;
            }
            if (name2 == null)
            {
                return 1;
            }
            return StrCmpLogicalW(name1.ToString(), name2.ToString());
        }
    }
}
