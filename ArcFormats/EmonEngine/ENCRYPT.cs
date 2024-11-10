using System;
using Utility;

namespace ArcFormats.EmonEngine
{
    internal class ENCRYPT
    {
        public static int ShiftValue(uint result, uint key)
        {
            uint originalValue = 0;
            int shift = 0;

            for (int i = 0; i < 32; i++)
            {
                shift += (int)key;
                int originalPosition = shift % 32;
                uint bit = (result >> originalPosition) & 1;
                originalValue |= (bit << i);
            }

            return (int)originalValue;
        }

        private static void InitTable(byte[] buffer, uint key)
        {
            int length = buffer.Length;
            byte[] table = new byte[length];

            int[] xSequence = new int[length];
            int currentX = 0;

            for (int i = 0; i < length; i++)
            {
                currentX = (int)((currentX + key) % length);
                xSequence[i] = currentX;
            }

            int[] invXSequence = new int[length];
            for (int i = 0; i < xSequence.Length; i++)
            {
                invXSequence[xSequence[i]] = i;
            }

            for (int i = 0; i < length; i++)
            {
                table[invXSequence[i]] = buffer[i];
            }

            Array.Copy(table, 0, buffer, 0, length);  // Fixed the CopyTo call
        }

        public static byte[] Encrypt(byte[] buffer, int offset, int length, byte[] routine)
        {
            byte[] data = new byte[length];
            Array.Copy(buffer, offset, data, 0, length);

            for (int i = 0; i < 8; i++)
            {
                uint key = BitConverter.ToUInt32(routine, 8 + i * 4);

                switch (routine[i])
                {
                    case 1:
                        for (int j = 0; j < data.Length; j += 4)
                        {
                            uint v = BitConverter.ToUInt32(data, j);
                            BitConverter.GetBytes(v ^ key).CopyTo(data, j);
                        }
                        break;

                    case 2:
                        uint prev = 0;
                        for (int j = 0; j < data.Length; j += 4)
                        {
                            uint v = BitConverter.ToUInt32(data, j);
                            uint newVal = v ^ key ^ prev;
                            BitConverter.GetBytes(newVal).CopyTo(data, j);
                            prev = newVal;
                        }
                        break;

                    case 4:
                        for (int j = 0; j < data.Length; j += 4)
                        {
                            uint v = BitConverter.ToUInt32(data, j);
                            int result = ShiftValue(v, key);
                            BitConverter.GetBytes(result).CopyTo(data, j);
                        }
                        break;

                    case 8:
                        InitTable(data, key);
                        break;
                }
            }

            return data;
        }

        public static byte[] ApplyXorMask(byte[] data)
        {
            byte[] xorMask = Utils.HexStringToByteArray("ca96e2f800000000");
            byte[] transformedData = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                transformedData[i] = (byte)(data[i] ^ xorMask[i % xorMask.Length]);
            }

            return transformedData;
        }

        public static byte[] ApplyHeaderXorMask(byte[] data)
        {
            byte[] xorMask = Utils.HexStringToByteArray("ca0000f8009600000000e200");
            byte[] transformedData = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                transformedData[i] = (byte)(data[i] ^ xorMask[i % xorMask.Length]);
            }

            return transformedData;
        }

        public static byte[] ApplyImageHeaderXorMask(byte[] data)
        {
            byte[] xorMask = Utils.HexStringToByteArray("ca96e2d0000000a89f96e27800000000cb82e2f800140000ca86c8f800000000");
            byte[] transformedData = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                transformedData[i] = (byte)(data[i] ^ xorMask[i % xorMask.Length]);
            }

            return transformedData;
        }
    }
}
