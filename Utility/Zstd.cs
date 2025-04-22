// File: Utility/Zstd.cs
// Date: 2024/08/31
// Description: Zstd compression method based on Zstandard.Net.
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

using ZstdNet;

namespace Utility.Compression
{
    public static class Zstd
    {
        public static byte[] Decompress(byte[] data)
        {
            using (var decompressor = new Decompressor())
            {
                return decompressor.Unwrap(data);
            }
        }

        public static byte[] Compress(byte[] data)
        {
            using (var compressor = new Compressor())
            {
                return compressor.Wrap(data);
            }
        }
    }
}
