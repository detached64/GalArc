using System;

namespace ArcFormats.Cmvs
{
    internal static class CmvsMD5
    {
        //private static readonly uint[] T = new uint[64];

        //static CmvsMD5()
        //{
        //    for (int i = 0; i < 64; i++)
        //    {
        //        T[i] = (uint)(Math.Pow(2, 32) * Math.Abs(Math.Sin(i + 1)));
        //    }
        //}

        public static void ComputeHash(uint[] data)
        {
            uint[] state = new uint[4]
            {
                0xC74A2B01,
                0xE7C8AB8F,
                0xD8BEDC4E,
                0x7302A4C5
            };

            uint[] buffer = new uint[16];
            Array.Copy(data, buffer, 4);
            buffer[4] = 0x80; // Padding

            // Append length in bits (16 bytes = 128 bits for simplicity)
            buffer[14] = 16 * 8; // Length in bits
            buffer[15] = 0;

            MD5Transform(state, buffer);

            // reverse order
            data[0] = state[3];
            data[1] = state[2];
            data[2] = state[1];
            data[3] = state[0];
        }

        private static void MD5Transform(uint[] state, uint[] block)
        {
            uint a = state[0];
            uint b = state[1];
            uint c = state[2];
            uint d = state[3];

            // Main MD5 operations (similar to the original C code)
            uint F1(uint x, uint y, uint z) => z ^ (x & (y ^ z));
            uint F2(uint x, uint y, uint z) => F1(z, x, y);
            uint F3(uint x, uint y, uint z) => x ^ y ^ z;
            uint F4(uint x, uint y, uint z) => y ^ (x | ~z);

            // Perform MD5 steps
            MD5Step(F1, ref a, b, c, d, block[0] + 0xd76aa478, 7);
            MD5Step(F1, ref d, a, b, c, block[1] + 0xe8c7b756, 12);
            MD5Step(F1, ref c, d, a, b, block[2] + 0x242070db, 17);
            MD5Step(F1, ref b, c, d, a, block[3] + 0xc1bdceee, 22);
            MD5Step(F1, ref a, b, c, d, block[4] + 0xf57c0faf, 7);
            MD5Step(F1, ref d, a, b, c, block[5] + 0x4787c62a, 12);
            MD5Step(F1, ref c, d, a, b, block[6] + 0xa8304613, 17);
            MD5Step(F1, ref b, c, d, a, block[7] + 0xfd469501, 22);
            MD5Step(F1, ref a, b, c, d, block[8] + 0x698098d8, 7);
            MD5Step(F1, ref d, a, b, c, block[9] + 0x8b44f7af, 12);
            MD5Step(F1, ref c, d, a, b, block[10] + 0xffff5bb1, 17);
            MD5Step(F1, ref b, c, d, a, block[11] + 0x895cd7be, 22);
            MD5Step(F1, ref a, b, c, d, block[12] + 0x6b901122, 7);
            MD5Step(F1, ref d, a, b, c, block[13] + 0xfd987193, 12);
            MD5Step(F1, ref c, d, a, b, block[14] + 0xa679438e, 17);
            MD5Step(F1, ref b, c, d, a, block[15] + 0x49b40821, 22);

            MD5Step(F2, ref a, b, c, d, block[1] + 0xf61e2562, 5);
            MD5Step(F2, ref d, a, b, c, block[6] + 0xc040b340, 9);
            MD5Step(F2, ref c, d, a, b, block[11] + 0x265e5a51, 14);
            MD5Step(F2, ref b, c, d, a, block[0] + 0xe9b6c7aa, 20);
            MD5Step(F2, ref a, b, c, d, block[5] + 0xd62f105d, 5);
            MD5Step(F2, ref d, a, b, c, block[10] + 0x02441453, 9);
            MD5Step(F2, ref c, d, a, b, block[15] + 0xd8a1e681, 14);
            MD5Step(F2, ref b, c, d, a, block[4] + 0xe7d3fbc8, 20);
            MD5Step(F2, ref a, b, c, d, block[9] + 0x21e1cde6, 5);
            MD5Step(F2, ref d, a, b, c, block[14] + 0xc33707d6, 9);
            MD5Step(F2, ref c, d, a, b, block[3] + 0xf4d50d87, 14);
            MD5Step(F2, ref b, c, d, a, block[8] + 0x455a14ed, 20);
            MD5Step(F2, ref a, b, c, d, block[13] + 0xa9e3e905, 5);
            MD5Step(F2, ref d, a, b, c, block[2] + 0xfcefa3f8, 9);
            MD5Step(F2, ref c, d, a, b, block[7] + 0x676f02d9, 14);
            MD5Step(F2, ref b, c, d, a, block[12] + 0x8d2a4c8a, 20);

            MD5Step(F3, ref a, b, c, d, block[5] + 0xfffa3942, 4);
            MD5Step(F3, ref d, a, b, c, block[8] + 0x8771f681, 11);
            MD5Step(F3, ref c, d, a, b, block[11] + 0x6d9d6122, 16);
            MD5Step(F3, ref b, c, d, a, block[14] + 0xfde5380c, 23);
            MD5Step(F3, ref a, b, c, d, block[1] + 0xa4beea44, 4);
            MD5Step(F3, ref d, a, b, c, block[4] + 0x4bdecfa9, 11);
            MD5Step(F3, ref c, d, a, b, block[7] + 0xf6bb4b60, 16);
            MD5Step(F3, ref b, c, d, a, block[10] + 0xbebfbc70, 23);
            MD5Step(F3, ref a, b, c, d, block[13] + 0x289b7ec6, 4);
            MD5Step(F3, ref d, a, b, c, block[0] + 0xeaa127fa, 11);
            MD5Step(F3, ref c, d, a, b, block[3] + 0xd4ef3085, 16);
            MD5Step(F3, ref b, c, d, a, block[6] + 0x04881d05, 23);
            MD5Step(F3, ref a, b, c, d, block[9] + 0xd9d4d039, 4);
            MD5Step(F3, ref d, a, b, c, block[12] + 0xe6db99e5, 11);
            MD5Step(F3, ref c, d, a, b, block[15] + 0x1fa27cf8, 16);
            MD5Step(F3, ref b, c, d, a, block[2] + 0xc4ac5665, 23);

            MD5Step(F4, ref a, b, c, d, block[0] + 0xf4292244, 6);
            MD5Step(F4, ref d, a, b, c, block[7] + 0x432aff97, 10);
            MD5Step(F4, ref c, d, a, b, block[14] + 0xab9423a7, 15);
            MD5Step(F4, ref b, c, d, a, block[5] + 0xfc93a039, 21);
            MD5Step(F4, ref a, b, c, d, block[12] + 0x655b59c3, 6);
            MD5Step(F4, ref d, a, b, c, block[3] + 0x8f0ccc92, 10);
            MD5Step(F4, ref c, d, a, b, block[10] + 0xffeff47d, 15);
            MD5Step(F4, ref b, c, d, a, block[1] + 0x85845dd1, 21);
            MD5Step(F4, ref a, b, c, d, block[8] + 0x6fa87e4f, 6);
            MD5Step(F4, ref d, a, b, c, block[15] + 0xfe2ce6e0, 10);
            MD5Step(F4, ref c, d, a, b, block[6] + 0xa3014314, 15);
            MD5Step(F4, ref b, c, d, a, block[13] + 0x4e0811a1, 21);
            MD5Step(F4, ref a, b, c, d, block[4] + 0xf7537e82, 6);
            MD5Step(F4, ref d, a, b, c, block[11] + 0xbd3af235, 10);
            MD5Step(F4, ref c, d, a, b, block[2] + 0x2ad7d2bb, 15);
            MD5Step(F4, ref b, c, d, a, block[9] + 0xeb86d391, 21);

            // Final state update
            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
        }

        private static void MD5Step(Func<uint, uint, uint, uint> f, ref uint w, uint x, uint y, uint z, uint inValue, int s)
        {
            w += f(x, y, z) + inValue;
            w = (w << s | w >> (32 - s)) + x;
        }
    }
}
