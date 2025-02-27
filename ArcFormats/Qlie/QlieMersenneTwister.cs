// Mersenne Twister random number generator modified for QLIE decryption.
//
// Copyright (C) 2015 by morkt
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

namespace ArcFormats.Qlie
{
    internal class QlieMersenneTwister
    {
        private const uint DefaultSeed = 5489;

        private const int StateLength = 64;
        private const int StateM = 39;
        private const uint MatrixA = 0x9908B0DF;
        private const uint SignMask = 0x80000000;
        private const uint LowerMask = 0x7FFFFFFF;
        private const uint TemperingMaskB = 0x9C4F88E3;
        private const uint TemperingMaskC = 0xE7F70000;

        private uint[] mt = new uint[StateLength];
        private int mti = StateLength;

        public QlieMersenneTwister(uint seed)
        {
            SRand(seed);
        }

        public void SRand(uint seed)
        {
            mt[0] = seed;
            for (mti = 1; mti < mt.Length; ++mti)
            {
                mt[mti] = (0x6611BC19u * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + (uint)mti);
            }
        }

        public void XorState(byte[] hash)
        {
            int length = Math.Min(hash.Length / 4, StateLength);
            if (length == 0)
            {
                return;
            }

            unsafe
            {
                fixed (byte* hash_fixed = hash)
                {
                    uint* hash32 = (uint*)hash_fixed;
                    for (int i = 0; i < length; ++i)
                    {
                        mt[i] ^= hash32[i];
                    }
                }
            }
        }

        private uint[] mag01 = { 0, MatrixA };

        public uint Rand()
        {
            uint y;

            if (mti >= StateLength)
            {
                int kk;
                for (kk = 0; kk < StateLength - StateM; kk++)
                {
                    y = (mt[kk] & SignMask) | (mt[kk + 1] & LowerMask) >> 1;
                    mt[kk] = mt[kk + StateM] ^ y ^ mag01[mt[kk + 1] & 1];
                }
                for (; kk < StateLength - 1; kk++)
                {
                    y = (mt[kk] & SignMask) | (mt[kk + 1] & LowerMask) >> 1;
                    mt[kk] = mt[kk + StateM - StateLength] ^ y ^ mag01[mt[kk + 1] & 1];
                }
                y = (mt[StateLength - 1] & SignMask) | (mt[0] & LowerMask) >> 1;
                mt[StateLength - 1] = mt[StateM - 1] ^ y ^ mag01[mt[kk - 1] & 1];

                mti = 0;
            }

            y = mt[mti++];
            y ^= y >> 11;
            y ^= (y << 7) & TemperingMaskB;
            y ^= (y << 15) & TemperingMaskC;
            y ^= y >> 18;

            return y;
        }

        public ulong Rand64()
        {
            // unlike C/C++, in C# order of the function calls in sub-expressions is well-defined
            // (left-to-right), but it still feels safer to split expressions with side effects.
            ulong v = Rand();
            return v | (ulong)Rand() << 32;
        }
    }
}
