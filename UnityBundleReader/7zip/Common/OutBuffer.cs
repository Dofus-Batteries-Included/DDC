// OutBuffer.cs

namespace UnityBundleReader._7zip.Common;

public class OutBuffer
{
    readonly byte[] _mBuffer;
    uint _mPos;
    readonly uint _mBufferSize;
    Stream _mStream;
    ulong _mProcessedSize;

    public OutBuffer(uint bufferSize)
    {
        _mBuffer = new byte[bufferSize];
        _mBufferSize = bufferSize;
    }

    public void SetStream(Stream stream) => _mStream = stream;
    public void FlushStream() => _mStream.Flush();
    public void CloseStream() => _mStream.Close();
    public void ReleaseStream() => _mStream = null;

    public void Init()
    {
        _mProcessedSize = 0;
        _mPos = 0;
    }

    public void WriteByte(byte b)
    {
        _mBuffer[_mPos++] = b;
        if (_mPos >= _mBufferSize)
        {
            FlushData();
        }
    }

    public void FlushData()
    {
        if (_mPos == 0)
        {
            return;
        }
        _mStream.Write(_mBuffer, 0, (int)_mPos);
        _mPos = 0;
    }

    public ulong GetProcessedSize() => _mProcessedSize + _mPos;
}
