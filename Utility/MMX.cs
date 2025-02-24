// File: Utility/MMX.cs
// Date: 2024/12/30
// Description: MMX instruction set partial implementation.
//
// Copyright (C) 2014-2015 by morkt
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

namespace Utility
{
    public static class MMX
    {
        public static ulong PAddB(ulong x, ulong y)
        {
            ulong r = 0;
            for (ulong mask = 0xFF; mask != 0; mask <<= 8)
            {
                r |= ((x & mask) + (y & mask)) & mask;
            }
            return r;
        }

        public static uint PAddB(uint x, uint y)
        {
            uint r13 = (x & 0xFF00FF00u) + (y & 0xFF00FF00u);
            uint r02 = (x & 0x00FF00FFu) + (y & 0x00FF00FFu);
            return (r13 & 0xFF00FF00u) | (r02 & 0x00FF00FFu);
        }

        public static ulong PAddW(ulong x, ulong y)
        {
            ulong mask = 0xffff;
            ulong r = ((x & mask) + (y & mask)) & mask;
            mask <<= 16;
            r |= ((x & mask) + (y & mask)) & mask;
            mask <<= 16;
            r |= ((x & mask) + (y & mask)) & mask;
            mask <<= 16;
            r |= ((x & mask) + (y & mask)) & mask;
            return r;
        }

        public static ulong PAddD(ulong x, ulong y)
        {
            ulong mask = 0xffffffff;
            ulong r = ((x & mask) + (y & mask)) & mask;
            mask <<= 32;
            return r | ((x & mask) + (y & mask)) & mask;
        }

        public static ulong PSubB(ulong x, ulong y)
        {
            ulong r = 0;
            for (ulong mask = 0xFF; mask != 0; mask <<= 8)
            {
                r |= ((x & mask) - (y & mask)) & mask;
            }
            return r;
        }

        public static ulong PSubW(ulong x, ulong y)
        {
            ulong mask = 0xffff;
            ulong r = ((x & mask) - (y & mask)) & mask;
            mask <<= 16;
            r |= ((x & mask) - (y & mask)) & mask;
            mask <<= 16;
            r |= ((x & mask) - (y & mask)) & mask;
            mask <<= 16;
            r |= ((x & mask) - (y & mask)) & mask;
            return r;
        }

        public static ulong PSubD(ulong x, ulong y)
        {
            ulong mask = 0xffffffff;
            ulong r = ((x & mask) - (y & mask)) & mask;
            mask <<= 32;
            return r | ((x & mask) - (y & mask)) & mask;
        }

        public static ulong PSllD(ulong x, int count)
        {
            count &= 0x1F;
            ulong mask = 0xFFFFFFFFu << count;
            mask |= mask << 32;
            return (x << count) & mask;
        }

        public static ulong PSrlD(ulong x, int count)
        {
            count &= 0x1F;
            ulong mask = 0xFFFFFFFFu >> count;
            mask |= mask << 32;
            return (x >> count) & mask;
        }

        public static ulong PUNPCKLDQ(ulong x, ulong y)
        {
            return (x & 0xFFFFFFFFu) | (y & 0xFFFFFFFFu) << 32;
        }

        public static ulong PUNPCKLDQ(ulong x)
        {
            return (x & 0xFFFFFFFFu) | (x & 0xFFFFFFFFu) << 32;
        }
    }
}
