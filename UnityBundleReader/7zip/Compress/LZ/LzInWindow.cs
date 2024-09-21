// LzInWindow.cs

namespace UnityBundleReader._7zip.Compress.LZ;

public class InWindow
{
    public byte[] BufferBase; // pointer to buffer with data
    Stream _stream;
    uint _posLimit; // offset (from _buffer) of first byte when new block reading must be done
    bool _streamEndWasReached; // if (true) then _streamPos shows real end of stream

    uint _pointerToLastSafePosition;

    public uint BufferOffset;

    public uint BlockSize; // Size of Allocated memory block
    public uint Pos; // offset (from _buffer) of curent byte
    uint _keepSizeBefore; // how many BYTEs must be kept in buffer before _pos
    uint _keepSizeAfter; // how many BYTEs must be kept buffer after _pos
    public uint StreamPos; // offset (from _buffer) of first not read byte from Stream

    public void MoveBlock()
    {
        uint offset = BufferOffset + Pos - _keepSizeBefore;
        // we need one additional byte, since MovePos moves on 1 byte.
        if (offset > 0)
        {
            offset--;
        }

        uint numBytes = BufferOffset + StreamPos - offset;

        // check negative offset ????
        for (uint i = 0; i < numBytes; i++)
        {
            BufferBase[i] = BufferBase[offset + i];
        }
        BufferOffset -= offset;
    }

    public virtual void ReadBlock()
    {
        if (_streamEndWasReached)
        {
            return;
        }
        while (true)
        {
            int size = (int)(0 - BufferOffset + BlockSize - StreamPos);
            if (size == 0)
            {
                return;
            }
            int numReadBytes = _stream.Read(BufferBase, (int)(BufferOffset + StreamPos), size);
            if (numReadBytes == 0)
            {
                _posLimit = StreamPos;
                uint pointerToPostion = BufferOffset + _posLimit;
                if (pointerToPostion > _pointerToLastSafePosition)
                {
                    _posLimit = _pointerToLastSafePosition - BufferOffset;
                }

                _streamEndWasReached = true;
                return;
            }
            StreamPos += (uint)numReadBytes;
            if (StreamPos >= Pos + _keepSizeAfter)
            {
                _posLimit = StreamPos - _keepSizeAfter;
            }
        }
    }

    void Free() => BufferBase = null;

    public void Create(uint keepSizeBefore, uint keepSizeAfter, uint keepSizeReserv)
    {
        _keepSizeBefore = keepSizeBefore;
        _keepSizeAfter = keepSizeAfter;
        uint blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;
        if (BufferBase == null || BlockSize != blockSize)
        {
            Free();
            BlockSize = blockSize;
            BufferBase = new byte[BlockSize];
        }
        _pointerToLastSafePosition = BlockSize - keepSizeAfter;
    }

    public void SetStream(Stream stream) => _stream = stream;
    public void ReleaseStream() => _stream = null;

    public void Init()
    {
        BufferOffset = 0;
        Pos = 0;
        StreamPos = 0;
        _streamEndWasReached = false;
        ReadBlock();
    }

    public void MovePos()
    {
        Pos++;
        if (Pos > _posLimit)
        {
            uint pointerToPostion = BufferOffset + Pos;
            if (pointerToPostion > _pointerToLastSafePosition)
            {
                MoveBlock();
            }
            ReadBlock();
        }
    }

    public byte GetIndexByte(int index) => BufferBase[BufferOffset + Pos + index];

    // index + limit have not to exceed _keepSizeAfter;
    public uint GetMatchLen(int index, uint distance, uint limit)
    {
        if (_streamEndWasReached)
        {
            if (Pos + index + limit > StreamPos)
            {
                limit = StreamPos - (uint)(Pos + index);
            }
        }
        distance++;
        // Byte *pby = _buffer + (size_t)_pos + index;
        uint pby = BufferOffset + Pos + (uint)index;

        uint i;
        for (i = 0; i < limit && BufferBase[pby + i] == BufferBase[pby + i - distance]; i++)
        {
            ;
        }
        return i;
    }

    public uint GetNumAvailableBytes() => StreamPos - Pos;

    public void ReduceOffsets(int subValue)
    {
        BufferOffset += (uint)subValue;
        _posLimit -= (uint)subValue;
        Pos -= (uint)subValue;
        StreamPos -= (uint)subValue;
    }
}
