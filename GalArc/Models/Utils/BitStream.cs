using System;
using System.Diagnostics;
using System.IO;

namespace GalArc.Models.Utils;

internal enum BitStreamEndianness
{
    Msb,
    Lsb
}

internal enum BitStreamMode
{
    Read,
    Write
}

internal sealed class BitStream : IDisposable
{
    /// <summary>
    /// Base stream that the BitStream operates on.
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// Endianness used for reading and writing data for the <see cref="BitStream" />.
    /// </summary>
    /// <remarks>Endianness determines the byte order in which data is processed. This field is used
    /// internally to specify whether the bit stream operates in big-endian or little-endian mode.</remarks>
    /// <example>
    /// Read mode example:
    /// 0b10100001
    /// In big-endian (MSB), the most significant bit 10100001 is processed first.
    ///                                               ^
    /// In little-endian (LSB), the least significant bit 10100001 is processed first.
    ///                                                          ^
    /// Write mode example:
    /// If we keep writing bit 1 into the stream:
    /// In big-endian (MSB), we get: 0b10000000, 0b11000000, 0b11100000, 0b11110000, 0b11111000, 0b11111100, 0b11111110, 0b11111111
    /// In little-endian (LSB), we get: 0b00000001, 0b00000011, 0b00000111, 0b00001111, 0b00011111, 0b00111111, 0b01111111, 0b11111111
    /// </example>
    private readonly BitStreamEndianness endianness;

    /// <summary>
    /// Mode of operation for the <see cref="BitStream" />.
    /// </summary>
    /// <remarks>Specifies whether the <see cref="BitStream" /> operates in a read or write
    /// mode. It is intended for internal use to control the behavior of the stream.</remarks>
    private readonly BitStreamMode mode;

    /// <summary>
    /// Current byte being processed.
    /// </summary>
    private byte currentByte;

    /// <summary>
    /// Bit position in the current byte.
    /// </summary>
    private int bitPosition;

    /// <summary>
    /// Original byte position in the stream when the BitStream was created.
    /// </summary>
    /// Used for resetting the stream.
    private readonly int originalPos;

    public BitStream(Stream stream, BitStreamEndianness order = BitStreamEndianness.Msb, BitStreamMode mode = BitStreamMode.Read)
    {
        BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
        this.endianness = order;
        this.mode = mode;
        this.currentByte = 0;
        this.bitPosition = mode is BitStreamMode.Read ? 8 : 0;
        this.originalPos = (int)stream.Position;
        switch (mode)
        {
            case BitStreamMode.Write when !stream.CanWrite:
                throw new InvalidOperationException("Stream must be writable in write mode.");
            case BitStreamMode.Read when !stream.CanRead:
                throw new InvalidOperationException("Stream must be readable in read mode.");
        }
    }

    public int ReadBits(int bitCount)
    {
        if (mode != BitStreamMode.Read)
            throw new InvalidOperationException("Stream is not in Read mode.");
        if (bitCount < 1 || bitCount > 32)
            throw new ArgumentOutOfRangeException(nameof(bitCount), "Bit count must be between 1 and 32.");

        int result = 0;

        for (int i = 0; i < bitCount; i++)
        {
            if (bitPosition == 8)
            {
                int readByte = BaseStream.ReadByte();
                if (readByte == -1)
                    throw new EndOfStreamException();

                currentByte = (byte)readByte;
                bitPosition = 0;
            }

            int shift = endianness is BitStreamEndianness.Msb
                ? 7 - bitPosition
                : bitPosition;
            int bit = (currentByte >> shift) & 1;

            result = (result << 1) | bit;
            bitPosition++;
        }

        return result;
    }

    public int ReadBit()
    {
        return ReadBits(1);
    }

    public void WriteBit(int bit)
    {
        if (mode != BitStreamMode.Write)
            throw new InvalidOperationException("Stream is not in Write mode.");
        if (bit != 0 && bit != 1)
            throw new ArgumentOutOfRangeException(nameof(bit), "Bit must be 0 or 1.");

        int shift = endianness == BitStreamEndianness.Msb
            ? 7 - bitPosition
            : bitPosition;
        currentByte = (byte)((currentByte & ~(1 << shift)) | (bit << shift));
        bitPosition++;

        if (bitPosition == 8)
        {
            BaseStream.WriteByte(currentByte);
            currentByte = 0;
            bitPosition = 0;
        }
    }

    public void WriteBit(bool bit)
    {
        WriteBit(bit ? 1 : 0);
    }

    public void WriteBits(byte[] bits, int bitCount)
    {
        if (mode != BitStreamMode.Write)
            throw new InvalidOperationException("Stream is not in Write mode.");
        if (bits == null || bits.Length == 0)
            return;
        if (bitCount < 1 || bitCount > bits.Length * 8)
            throw new ArgumentOutOfRangeException(nameof(bitCount), "Bit count must be between 1 and the number of bits in the byte array.");

        int bitCountWritten = 0;
        int currentByteIndex = 0;
        int currentBitIndex = 0;

        while (bitCountWritten < bitCount)
        {
            int shift = endianness == BitStreamEndianness.Msb
                ? 7 - currentBitIndex
                : currentBitIndex;
            int bit = (bits[currentByteIndex] >> shift) & 1;
            WriteBit(bit);
            bitCountWritten++;
            currentBitIndex++;

            if (currentBitIndex == 8)
            {
                currentBitIndex = 0;
                currentByteIndex++;
            }
        }
    }

    public void WriteBits(byte[] bits)
    {
        WriteBits(bits, bits.Length * 8);
    }

    public void WriteBits(byte bits)
    {
        WriteBits([bits], 8);
    }

    public void WriteBits(bool[] bits)
    {
        Debug.WriteLine("You are using bool[] as input. The BitStream will just traverse the array and write 1 for true and 0 for false regardless of the endianness.");
        if (bits == null || bits.Length == 0)
            return;
        foreach (bool bit in bits)
        {
            WriteBit(bit ? 1 : 0);
        }
    }

    public void Reset()
    {
        if (mode != BitStreamMode.Read)
            throw new InvalidOperationException("Stream not in Read mode cannot be reset.");

        BaseStream.Position = originalPos;
        currentByte = 0;
        bitPosition = 8;
    }

    #region IDisposable Members
    private bool is_disposed;

    private void Dispose(bool disposing)
    {
        if (!is_disposed)
        {
            if (disposing)
            {
            }

            if (mode is BitStreamMode.Write && bitPosition > 0)
            {
                BaseStream.WriteByte(currentByte);
            }
            currentByte = 0;
            is_disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
