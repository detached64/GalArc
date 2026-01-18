using GalArc.Models.Utils;
using System;

namespace GalArc.Models.Formats.Cmvs;

internal abstract class CmvsEncryption
{
    public abstract string Version { get; }
    public abstract string InitString { get; }
    public abstract byte[] Decrypt(byte[] data);
    public virtual byte[] Encrypt(byte[] data)
    {
        throw new NotImplementedException();
    }
    public static CmvsEncryption Create(int version)
    {
        return version switch
        {
            1 => new CmvsEncryptionV1(),
            _ => throw new NotSupportedException(),
        };
    }
}

internal class CmvsEncryptionV1 : CmvsEncryption    // CPZ1 | CPackCode1024
{
    public override string Version => "1";
    public override string InitString => "暗号はね、解析しちゃダメなの…ほんとだよ♪あれ、誰に言ってるの？";

    private byte[] Key;

    public CmvsEncryptionV1()
    {
        InitKey();
    }

    private void InitKey()
    {
        Key = ArcEncoding.Shift_JIS.GetBytes(InitString);
        for (int i = 0; i < Key.Length; i++)
        {
            Key[i] += 0xA;
        }
    }

    public override byte[] Decrypt(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)((data[i] ^ Key[i & 0x3F]) - 0x6C);
        }
        return data;
    }

    public override byte[] Encrypt(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)((data[i] + 0x6C) ^ Key[i & 0x3F]);
        }
        return data;
    }
}
