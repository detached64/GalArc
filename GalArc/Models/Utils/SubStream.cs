using System;
using System.IO;

namespace GalArc.Models.Utils;

internal class SubStream : Stream
{
    private readonly Stream _baseStream;
    private readonly long _start;
    private readonly long _end;
    private readonly bool _leaveOpen;

    public SubStream(Stream baseStream, long offset, long size, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(baseStream);
        if (!baseStream.CanSeek)
            throw new ArgumentException("Base stream must support seeking.", nameof(baseStream));
        _baseStream = baseStream;
        _leaveOpen = leaveOpen;
        _start = offset;
        _end = Math.Min(offset + size, baseStream.Length);
    }

    public SubStream(Stream _baseStream, long offset, bool leaveOpen = false) :
        this(_baseStream, offset, _baseStream.Length - offset, leaveOpen)
    {
    }

    public override bool CanRead => _baseStream.CanRead;
    public override bool CanWrite => false;
    public override bool CanSeek => true;
    public override long Length => _end - _start;
    public override long Position
    {
        get => _baseStream.Position - _start;
        set => _baseStream.Position = Math.Max(_start + value, _start);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (SeekOrigin.Begin == origin)
            offset += _start;
        else if (SeekOrigin.Current == origin)
            offset += _baseStream.Position;
        else
            offset += _end;
        offset = Math.Max(offset, _start);
        _baseStream.Position = offset;
        return offset - _start;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = 0;
        long available = _end - _baseStream.Position;
        if (available > 0)
        {
            read = _baseStream.Read(buffer, offset, (int)Math.Min(count, available));
        }
        return read;
    }

    public override int ReadByte()
    {
        return _baseStream.Position < _end ? _baseStream.ReadByte() : -1;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("RangeStream does not support writing.");
    }

    public override void Flush()
    {
        _baseStream.Flush();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("RangeStream does not support setting length.");
    }
}
