
namespace GalArc.Models.Utils;

internal static class MMX
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
        return r | (((x & mask) + (y & mask)) & mask);
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
        return r | (((x & mask) - (y & mask)) & mask);
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
