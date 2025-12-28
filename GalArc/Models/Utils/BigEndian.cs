namespace GalArc.Models.Utils;

internal static class BigEndian
{
    public static uint Convert(uint u)
    {
        return u << 24 | (u & 0xff00) << 8 | (u & 0xff0000) >> 8 | u >> 24;
    }

    public static int Convert(int i)
    {
        return (int)Convert((uint)i);
    }

    public static ushort Convert(ushort u)
    {
        return (ushort)(u << 8 | u >> 8);
    }

    public static short Convert(short i)
    {
        return (short)Convert((ushort)i);
    }

    public static ulong Convert(ulong u)
    {
        return (ulong)Convert((uint)(u & 0xffffffff)) << 32 | (ulong)Convert((uint)(u >> 32));
    }

    public static long Convert(long i)
    {
        return (long)Convert((ulong)i);
    }
}
