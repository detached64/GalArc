using System;

namespace GalArc.Models.Utils;

internal static class Binary
{
    public static uint RotL(uint v, int count)
    {
        count &= 0x1F;
        return v << count | v >> (32 - count);
    }

    public static uint RotR(uint v, int count)
    {
        count &= 0x1F;
        return v >> count | v << (32 - count);
    }

    public static byte RotByteL(byte v, int count)
    {
        count &= 7;
        return (byte)(v << count | v >> (8 - count));
    }

    public static byte RotByteR(byte v, int count)
    {
        count &= 7;
        return (byte)(v >> count | v << (8 - count));
    }

    public static void CopyOverlapped(byte[] data, int src, int dst, int count)
    {
        if (dst > src)
        {
            while (count > 0)
            {
                int preceding = Math.Min(dst - src, count);
                Buffer.BlockCopy(data, src, data, dst, preceding);
                dst += preceding;
                count -= preceding;
            }
        }
        else
        {
            Buffer.BlockCopy(data, src, data, dst, count);
        }
    }
}
