using System;

namespace ArcFormats.EmonEngine
{
    internal class DECRYPT
    {
        internal static unsafe void Decrypt(byte[] buffer, int offset, int length, byte[] routine)
        {
            if (null == buffer)
                throw new ArgumentNullException("buffer", "Buffer cannot be null.");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Buffer offset should be non-negative.");
            if (buffer.Length - offset < length)
                throw new ArgumentException("Buffer offset and length are out of bounds.");
            fixed (byte* data8 = &buffer[offset])
            {
                uint* data32 = (uint*)data8;
                int length32 = length / 4;
                int key_index = routine.Length;
                for (int i = 7; i >= 0; --i)
                {
                    key_index -= 4;
                    uint key = BitConverter.ToUInt32(routine, key_index);
                    switch (routine[i])
                    {
                        case 1:
                            for (int j = 0; j < length32; ++j)
                                data32[j] ^= key;
                            break;
                        case 2:
                            for (int j = 0; j < length32; ++j)
                            {
                                uint v = data32[j];
                                data32[j] = v ^ key;
                                key = v;
                            }
                            break;
                        case 4:
                            for (int j = 0; j < length32; ++j)
                                data32[j] = ShiftValue(data32[j], key);
                            break;
                        case 8:
                            InitTable(buffer, offset, length, key);
                            break;
                    }
                }
            }
        }

        static uint ShiftValue(uint val, uint key)
        {
            int shift = 0;
            uint result = 0;
            for (int i = 0; i < 32; ++i)
            {
                shift += (int)key;
                result |= ((val >> i) & 1) << shift;
            }
            return result;
        }

        static void InitTable(byte[] buffer, int offset, int length, uint key)
        {
            var table = new byte[length];
            int x = 0;
            for (int i = 0; i < length; ++i)
            {
                x += (int)key;
                while (x >= length)
                    x -= length;
                table[x] = buffer[offset + i];
            }
            Buffer.BlockCopy(table, 0, buffer, offset, length);
        }
    }
}

