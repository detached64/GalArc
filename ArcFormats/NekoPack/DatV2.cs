using GalArc.Logs;
using System.IO;
using Utility;

namespace ArcFormats.NekoPack
{
    internal class DatV2 : DAT
    {
        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 8;
            uint temp = br.ReadUInt32();
            fs.Position += 4;
            uint parity = br.ReadUInt32();
            uint index_length = br.ReadUInt32();
            if (ParityCheck(temp, index_length) != parity)
            {
                Logger.Info("Parity check failed.");
            }
        }

        public uint ParityCheck(uint a1, uint a2)
        {
            uint v1 = (a2 ^ ((a2 ^ ((a2 ^ ((a2 ^ a1) + 0x5D588B65)) - 0x359D3E2A)) - 0x70E44324)) + 0x6C078965;
            uint v2 = ((a2 ^ ((a2 ^ a1) + 0x5D588B65)) - 0x359D3E2A) >> 27;
            return Binary.RotL(v1, (int)v2);
        }
    }
}
