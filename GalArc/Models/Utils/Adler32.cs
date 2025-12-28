namespace GalArc.Models.Utils;

internal sealed class Adler32
{
    private uint _a = 1;
    private uint _b;

    public uint Checksum
    {
        get
        {
            return (_b << 16) | _a;
        }
    }

    private const int Modulus = 65521;

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
        Adler32 adler32 = new();
        adler32.Update(data, offset, length);
        return adler32.Checksum;
    }

    public static uint Compute(byte[] data)
    {
        return Compute(data, 0, data.Length);
    }
}
