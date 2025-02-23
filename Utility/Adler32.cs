// File: Utility/Adler32.cs
// Date: 2024/08/27
// Description: Adler32 checksum algorithm.
//
// Copyright (c) 2018 arcusmaximus
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

namespace Utility
{
    public class Adler32
    {
        private uint _a = 1;
        private uint _b = 0;

        public uint Checksum
        {
            get
            {
                return (_b << 16) | _a;
            }
        }

        private const int Modulus = 65521;

        /// <summary>
        /// Compute adler32 checksum of given byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Update(byte[] data, int offset, int length)
        {
            for (int counter = 0; counter < length; ++counter)
            {
                _a = (_a + (data[offset + counter])) % Modulus;
                _b = (_b + _a) % Modulus;
            }
        }

        public static uint Compute(byte[] data, int offset, int length)
        {
            Adler32 adler32 = new Adler32();
            adler32.Update(data, offset, length);
            return adler32.Checksum;
        }

        public static uint Compute(byte[] data)
        {
            return Compute(data, 0, data.Length);
        }
    }
}
