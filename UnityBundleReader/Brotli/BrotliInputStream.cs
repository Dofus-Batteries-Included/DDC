/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace UnityBundleReader.Brotli;

/// <summary>
///     <p>
///         <see cref="System.IO.Stream" />
///         decorator that decompresses brotli data.
///     </p>
///     Not thread-safe.
/// </summary>
public class BrotliInputStream : Stream
{
    public const int DefaultInternalBufferSize = 16384;

    /// <summary>Internal buffer used for efficient byte-by-byte reading.</summary>
    readonly byte[] _buffer;

    /// <summary>Number of decoded but still unused bytes in internal buffer.</summary>
    int _remainingBufferBytes;

    /// <summary>Next unused byte offset.</summary>
    int _bufferOffset;

    /// <summary>Decoder state.</summary>
    readonly State _state = new();

    /// <summary>
    ///     Creates a
    ///     <see cref="System.IO.Stream" />
    ///     wrapper that decompresses brotli data.
    ///     <p>
    ///         For byte-by-byte reading (
    ///         <see cref="ReadByte()" />
    ///         ) internal buffer with
    ///         <see cref="DefaultInternalBufferSize" />
    ///         size is allocated and used.
    ///     </p>
    ///     Will block the thread until first kilobyte of data of source is available.
    /// </summary>
    /// <param name="source">underlying data source</param>
    /// <exception cref="System.IO.IOException">in case of corrupted data or source stream problems</exception>
    public BrotliInputStream(Stream source) : this(source, DefaultInternalBufferSize, null)
    {
    }

    /// <summary>
    ///     Creates a
    ///     <see cref="System.IO.Stream" />
    ///     wrapper that decompresses brotli data.
    ///     <p>
    ///         For byte-by-byte reading (
    ///         <see cref="ReadByte()" />
    ///         ) internal buffer of specified size is
    ///         allocated and used.
    ///     </p>
    ///     Will block the thread until first kilobyte of data of source is available.
    /// </summary>
    /// <param name="source">compressed data source</param>
    /// <param name="byteReadBufferSize">
    ///     size of internal buffer used in case of
    ///     byte-by-byte reading
    /// </param>
    /// <exception cref="System.IO.IOException">in case of corrupted data or source stream problems</exception>
    public BrotliInputStream(Stream source, int byteReadBufferSize) : this(source, byteReadBufferSize, null)
    {
    }

    /// <summary>
    ///     Creates a
    ///     <see cref="System.IO.Stream" />
    ///     wrapper that decompresses brotli data.
    ///     <p>
    ///         For byte-by-byte reading (
    ///         <see cref="ReadByte()" />
    ///         ) internal buffer of specified size is
    ///         allocated and used.
    ///     </p>
    ///     Will block the thread until first kilobyte of data of source is available.
    /// </summary>
    /// <param name="source">compressed data source</param>
    /// <param name="byteReadBufferSize">
    ///     size of internal buffer used in case of
    ///     byte-by-byte reading
    /// </param>
    /// <param name="customDictionary">
    ///     custom dictionary data;
    ///     <see langword="null" />
    ///     if not used
    /// </param>
    /// <exception cref="System.IO.IOException">in case of corrupted data or source stream problems</exception>
    public BrotliInputStream(Stream source, int byteReadBufferSize, byte[]? customDictionary)
    {
        if (byteReadBufferSize <= 0)
        {
            throw new ArgumentException("Bad buffer size:" + byteReadBufferSize);
        }
        if (source == null)
        {
            throw new ArgumentException("source is null");
        }
        _buffer = new byte[byteReadBufferSize];
        _remainingBufferBytes = 0;
        _bufferOffset = 0;
        try
        {
            State.SetInput(_state, source);
        }
        catch (BrotliRuntimeException ex)
        {
            throw new IOException("Brotli decoder initialization failed", ex);
        }
        if (customDictionary != null)
        {
            Decode.SetCustomDictionary(_state, customDictionary);
        }
    }

    /// <summary>
    ///     <inheritDoc />
    /// </summary>
    /// <exception cref="System.IO.IOException" />
    public override void Close() => State.Close(_state);

    /// <summary>
    ///     <inheritDoc />
    /// </summary>
    /// <exception cref="System.IO.IOException" />
    public override int ReadByte()
    {
        if (_bufferOffset >= _remainingBufferBytes)
        {
            _remainingBufferBytes = Read(_buffer, 0, _buffer.Length);
            _bufferOffset = 0;
            if (_remainingBufferBytes == -1)
            {
                return -1;
            }
        }
        return _buffer[_bufferOffset++] & 0xFF;
    }

    /// <summary>
    ///     <inheritDoc />
    /// </summary>
    /// <exception cref="System.IO.IOException" />
    public override int Read(byte[] destBuffer, int destOffset, int destLen)
    {
        if (destOffset < 0)
        {
            throw new ArgumentException("Bad offset: " + destOffset);
        }
        if (destLen < 0)
        {
            throw new ArgumentException("Bad length: " + destLen);
        }
        if (destOffset + destLen > destBuffer.Length)
        {
            throw new ArgumentException("Buffer overflow: " + (destOffset + destLen) + " > " + destBuffer.Length);
        }
        if (destLen == 0)
        {
            return 0;
        }
        int copyLen = System.Math.Max(_remainingBufferBytes - _bufferOffset, 0);
        if (copyLen != 0)
        {
            copyLen = System.Math.Min(copyLen, destLen);
            Array.Copy(_buffer, _bufferOffset, destBuffer, destOffset, copyLen);
            _bufferOffset += copyLen;
            destOffset += copyLen;
            destLen -= copyLen;
            if (destLen == 0)
            {
                return copyLen;
            }
        }
        try
        {
            _state.Output = destBuffer;
            _state.OutputOffset = destOffset;
            _state.OutputLength = destLen;
            _state.OutputUsed = 0;
            Decode.Decompress(_state);
            if (_state.OutputUsed == 0)
            {
                return 0;
            }
            return _state.OutputUsed + copyLen;
        }
        catch (BrotliRuntimeException ex)
        {
            throw new IOException("Brotli stream decoding failed", ex);
        }
    }

    // <{[INJECTED CODE]}>
    public override bool CanRead => true;

    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override bool CanWrite => false;

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override void Flush() { }
}
