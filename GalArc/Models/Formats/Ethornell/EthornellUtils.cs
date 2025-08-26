using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace GalArc.Models.Formats.Ethornell;

internal abstract class DecryptionBase
{
    protected uint key;

    protected byte UpdateKey()
    {
        ushort v0 = (ushort)((0x15A4E35 * key) >> 16);
        key = (uint)((v0 << 16) + (ushort)(0x4E35 * key) + 1);
        return (byte)(v0 & 0x7FFF);
    }

    public virtual byte[] Decode()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// DSC FORMAT 1.00. Used in Ethornell scripts "BurikoCompiledScriptVer1.00".
/// </summary>
/// This part of code is based on the code from Crass.
internal class DscDecoder : DecryptionBase
{
    private readonly uint dec_count;
    private readonly uint unpacked_size;
    private readonly BitStream input;
    private readonly byte[] output;

    public DscDecoder(byte[] data)
    {
        key = BitConverter.ToUInt32(data, 16);
        unpacked_size = BitConverter.ToUInt32(data, 20);
        dec_count = BitConverter.ToUInt32(data, 24);
        input = new BitStream(new MemoryStream(data), BitStreamEndianness.Msb, BitStreamMode.Read);
        output = new byte[unpacked_size];
        input.BaseStream.Position = 0x20;
    }

    private static void CreateTree(DscNode[] hnodes, DscCode[] hcode, int node_count)
    {
        int[,] nodes_index = new int[2, 512];
        nodes_index[0, 0] = 0;
        int next_node_index = 1;
        int depth_nodes = 1;
        int depth = 0;
        int child_index = 0;
        for (int n = 0; n < node_count;)
        {
            int huffman_nodes_index = child_index;
            child_index ^= 1;

            int depth_existed_nodes = 0;
            while (n < hcode.Length && hcode[n].Depth == depth)
            {
                hnodes[nodes_index[huffman_nodes_index, depth_existed_nodes]] = new DscNode { IsParent = false, Code = hcode[n++].Code };
                depth_existed_nodes++;
            }
            int depth_nodes_to_create = depth_nodes - depth_existed_nodes;
            for (int i = 0; i < depth_nodes_to_create; i++)
            {
                DscNode node = new() { IsParent = true };
                nodes_index[child_index, i * 2] = node.LeftChildIndex = next_node_index++;
                nodes_index[child_index, (i * 2) + 1] = node.RightChildIndex = next_node_index++;
                hnodes[nodes_index[huffman_nodes_index, depth_existed_nodes + i]] = node;
            }
            depth++;
            depth_nodes = depth_nodes_to_create * 2;
        }
    }

    private int Decompress(DscNode[] hnodes, uint dec_count)
    {
        int dst = 0;

        for (uint i = 0; i < dec_count; i++)
        {
            int node_index = 0;
            do
            {
                switch (input.ReadBit())
                {
                    case 0:
                        node_index = hnodes[node_index].LeftChildIndex;
                        break;
                    case 1:
                        node_index = hnodes[node_index].RightChildIndex;
                        break;
                    case -1:
                        throw new EndOfStreamException();
                }
            }
            while (hnodes[node_index].IsParent);

            int code = hnodes[node_index].Code;
            if (code >= 256)
            {
                int offset = input.ReadBits(12);
                if (-1 == offset)
                {
                    break;
                }

                int count = (code & 0xff) + 2;
                offset += 2;
                Binary.CopyOverlapped(output, dst - offset, dst, count);
                dst += count;
            }
            else
            {
                output[dst++] = (byte)code;
            }
        }
        return dst;
    }

    public override byte[] Decode()
    {
        DscCode[] hcodes = new DscCode[512];
        DscNode[] hnodes = new DscNode[1023];

        int leaf_node_count = 0;
        for (ushort i = 0; i < 512; i++)
        {
            int src = input.BaseStream.ReadByte();
            byte depth = (byte)(src - UpdateKey());
            if (depth != 0)
            {
                hcodes[leaf_node_count].Depth = depth;
                hcodes[leaf_node_count].Code = i;
                leaf_node_count++;
            }
        }

        Array.Sort(hcodes, 0, leaf_node_count, new DscCodeComparer());
        CreateTree(hnodes, hcodes, leaf_node_count);
        Decompress(hnodes, dec_count);
        return output;
    }

    private struct DscNode
    {
        public bool IsParent;
        public int Code;
        public int LeftChildIndex;
        public int RightChildIndex;
    }

    private struct DscCode
    {
        public ushort Code;
        public ushort Depth;
    }

    private class DscCodeComparer : IComparer<DscCode>
    {
        public int Compare(DscCode x, DscCode y)
        {
            int compare = x.Depth.CompareTo(y.Depth);
            return compare != 0 ? compare : x.Code.CompareTo(y.Code);
        }
    }
}

/// <summary>
/// SDC FORMAT 1.00. Used in BGI.gdb.
/// </summary>
internal class SdcDecoder : DecryptionBase
{
    private readonly int size;
    private readonly uint unpacked_size;
    private readonly ushort check_sum;
    private readonly ushort check_xor;
    private readonly byte[] input;

    public SdcDecoder(byte[] data)
    {
        key = BitConverter.ToUInt32(data, 16);
        size = BitConverter.ToInt32(data, 20);
        unpacked_size = BitConverter.ToUInt32(data, 24);
        check_sum = BitConverter.ToUInt16(data, 28);
        check_xor = BitConverter.ToUInt16(data, 30);
        Buffer.BlockCopy(data, 32, input = new byte[size], 0, size);
    }

    public override byte[] Decode()
    {
        byte[] dec = new byte[size];
        uint checksum = 0, checkxor = 0;
        for (int i = 0; i < size; i++)
        {
            dec[i] = (byte)(input[i] - UpdateKey());
            checksum += input[i];
            checkxor ^= input[i];
        }
        if ((checksum & 0xFFFF) != check_sum || (checkxor & 0xFFFF) != check_xor)
        {
            throw new InvalidDataException("Checksum error");
        }

        byte[] output = new byte[unpacked_size];
        int act_unpacked_size = 0;
        for (int i = 0; i < size;)
        {
            if ((dec[i] & 0x80) != 0)
            {
                int code = dec[i] & 0x7f;
                int count = (code >> 3) + 2;
                int offset = ((code & 7) << 8) | dec[i + 1] + 2;
                Binary.CopyOverlapped(output, act_unpacked_size - offset, act_unpacked_size, count);
                act_unpacked_size += count;
                i += 2;
            }
            else
            {
                int count = dec[i++] + 1;
                Buffer.BlockCopy(dec, i, output, act_unpacked_size, count);
                act_unpacked_size += count;
                i += count;
            }
        }
        return output;
    }
}
