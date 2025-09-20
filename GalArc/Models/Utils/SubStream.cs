using System;
using System.IO;

namespace GalArc.Models.Utils;

internal class SubStream : Stream
{
    private readonly Stream _baseStream;
    private readonly long _start;
    private readonly long _end;
    private readonly bool _leaveOpen;
    private bool _isDisposed;

    public SubStream(Stream baseStream, long offset, long size, bool leaveOpen = true)
    {
        ArgumentNullException.ThrowIfNull(baseStream);
        if (!baseStream.CanRead)
            throw new ArgumentException("Base stream must support reading.", nameof(baseStream));
        if (!baseStream.CanSeek)
            throw new ArgumentException("Base stream must support seeking.", nameof(baseStream));
        if (offset < 0 || offset > baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative and within the length of the base stream.");
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be non-negative.");
        _baseStream = baseStream;
        _leaveOpen = leaveOpen;
        _start = offset;
        _end = Math.Min(offset + size, baseStream.Length);
        _baseStream.Position = _start;
    }

    public SubStream(Stream _baseStream, long offset, bool leaveOpen = true) :
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
        set
        {
            if (value < 0 || value > Length)
                throw new ArgumentOutOfRangeException(nameof(value), "Position must be non-negative and within the length of the SubStream.");
            _baseStream.Position = _start + Math.Min(value, Length);
        }
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
        long available = _end - _baseStream.Position;
        if (available <= 0)
        {
            return 0;
        }
        int bytesToRead = (int)Math.Min(count, available);
        return _baseStream.Read(buffer, offset, bytesToRead);
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

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing && !_leaveOpen)
            {
                _baseStream?.Dispose();
            }
            _isDisposed = true;
        }
        base.Dispose(disposing);
    }
}
