// File: Utility/Zstd.cs
// Date: 2024/08/31
// Description: 对Zstd的压缩和解压缩进行了封装，依赖Zstandard.Net
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using Zstandard.Net;

namespace Utility.Compression
{
    public class Zstd
    {
        public static byte[] Decompress(byte[] data)
        {
            Stream input = new MemoryStream(data);
            ZstandardStream stream = new ZstandardStream(input, CompressionMode.Decompress);
            MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static byte[] Compress(byte[] data)
        {
            var memoryStream = new MemoryStream();
            var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Compress);
            compressionStream.Write(data, 0, data.Length);
            compressionStream.Close();
            return memoryStream.ToArray();
        }
    }
}
