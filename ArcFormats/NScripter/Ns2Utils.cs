using System;
using System.Linq;
using Utility;

namespace ArcFormats.NScripter
{
    internal class Ns2Decryptor
    {
        private const int BlockSize = 32;
        private const int InitPos = 16;

        private byte[] key1;
        private byte[] key2;
        private byte[] m_buffer = new byte[64];
        private readonly MD5 md5 = new MD5();
        private readonly byte[] data;

        public Ns2Decryptor(byte[] data, byte[] key)
        {
            if (key.Length < 96)
            {
                throw new ArgumentException("Key length must be at least 96 bytes.");
            }
            this.key1 = key.Take(48).ToArray();
            this.key2 = key.Skip(48).Take(48).ToArray();
            this.data = data;
        }

        public void Decrypt()
        {
            for (int i = 0; i < data.Length; i += BlockSize)
            {
                byte[] state = new byte[16];
                byte[] temp1 = new byte[16];
                byte[] temp2 = new byte[16];

                Buffer.BlockCopy(data, InitPos + i, m_buffer, 0, 16);
                Buffer.BlockCopy(key1, 0, m_buffer, 16, 48);
                md5.Initialize();
                md5.Update(m_buffer, 0, 64);
                Buffer.BlockCopy(md5.State, 0, state, 0, 16);
                for (int j = 0; j < 16; j++)
                {
                    m_buffer[j] = (byte)(data[i + j] ^ state[j]);
                    temp1[j] = m_buffer[j];
                }
                Buffer.BlockCopy(key2, 0, m_buffer, 16, 48);
                md5.Initialize();
                md5.Update(m_buffer, 0, 64);
                Buffer.BlockCopy(md5.State, 0, state, 0, 16);
                for (int j = 0; j < 16; j++)
                {
                    m_buffer[j] = (byte)(data[i + j + 16] ^ state[j]);
                    temp2[j] = m_buffer[j];
                }
                Buffer.BlockCopy(temp2, 0, data, i, 16);
                Buffer.BlockCopy(key1, 0, m_buffer, 16, 48);
                md5.Initialize();
                md5.Update(m_buffer, 0, 64);
                Buffer.BlockCopy(md5.State, 0, state, 0, 16);
                for (int j = 0; j < 16; j++)
                {
                    data[i + j + 16] = (byte)(temp1[j] ^ state[j]);
                }
            }
        }
    }
}
