// File: Utility/HuffmanCompression.cs
// Date: 2024/11/28
// Description: Standard Huffman compression algorithm.
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

using System.Runtime.InteropServices;

namespace Utility.Compression
{
    public static class Huffman
    {
        [DllImport("UtilityNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int huffman_uncompress(byte[] dec, ulong dec_len, byte[] enc, ulong enc_len);

        [DllImport("UtilityNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int huffman_compress(byte[] enc, ulong enc_len, byte[] dec, ulong dec_len);

        public static byte[] Decompress(byte[] enc, int dec_length)
        {
            byte[] dec = new byte[dec_length];
            huffman_uncompress(dec, (ulong)dec_length, enc, (ulong)enc.Length);
            return dec;
        }

        public static byte[] Compress(byte[] dec, int enc_length)
        {
            byte[] enc = new byte[enc_length];
            huffman_compress(enc, (ulong)enc_length, dec, (ulong)dec.Length);
            return enc;
        }

        public static byte[] Compress(byte[] dec)
        {
            return Compress(dec, dec.Length);
        }
    }
}
