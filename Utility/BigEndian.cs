// File: Utility/BigEndian.cs
// Date: 2024/08/28
// Description: 基于morkt的相关代码，对BigEndian的读取进行封装：uint、int、ushort、short、ulong、long
//
// Copyright (C) 2014 by morkt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//

using System;
using System.Linq;

namespace Utility
{
    public class BigEndian
    {
        public static uint Read(uint u)
        {
            return u << 24 | (u & 0xff00) << 8 | (u & 0xff0000) >> 8 | u >> 24;
        }
        public static int Read(int i)
        {
            return (int)Read((uint)i);
        }
        public static ushort Read(ushort u)
        {
            return (ushort)(u << 8 | u >> 8);
        }
        public static short Read(short i)
        {
            return (short)Read((ushort)i);
        }
        public static ulong Read(ulong u)
        {
            return (ulong)Read((uint)(u & 0xffffffff)) << 32
                 | (ulong)Read((uint)(u >> 32));
        }
        public static long Read(long i)
        {
            return (long)Read((ulong)i);
        }

    }
}
