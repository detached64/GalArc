// File: Utility/ArcEncoding.cs
// Date: 2024/08/26
// Description: Encoding for ArcFormats.
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
using System.Text;

namespace Utility
{
    public static class ArcEncoding
    {
        public static Encoding GBK => Encoding.GetEncoding(936);

        public static Encoding Shift_JIS => Encoding.GetEncoding(932);
    }
}
