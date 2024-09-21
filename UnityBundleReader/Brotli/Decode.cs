/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace UnityBundleReader.Brotli;

/// <summary>API for Brotli decompression.</summary>
sealed class Decode
{
    const int DefaultCodeLength = 8;

    const int CodeLengthRepeatCode = 16;

    const int NumLiteralCodes = 256;

    const int NumInsertAndCopyCodes = 704;

    const int NumBlockLengthCodes = 26;

    const int LiteralContextBits = 6;

    const int DistanceContextBits = 2;

    const int HuffmanTableBits = 8;

    const int HuffmanTableMask = 0xFF;

    const int CodeLengthCodes = 18;

    static readonly int[] CodeLengthCodeOrder = [1, 2, 3, 4, 0, 5, 17, 6, 16, 7, 8, 9, 10, 11, 12, 13, 14, 15];

    const int NumDistanceShortCodes = 16;

    static readonly int[] DistanceShortCodeIndexOffset = [3, 2, 1, 0, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2];

    static readonly int[] DistanceShortCodeValueOffset = [0, 0, 0, 0, -1, 1, -2, 2, -3, 3, -1, 1, -2, 2, -3, 3];

    /// <summary>Static Huffman code for the code length code lengths.</summary>
    static readonly int[] FixedTable =
    [
        0x020000, 0x020004, 0x020003, 0x030002, 0x020000, 0x020004, 0x020003, 0x040001, 0x020000, 0x020004, 0x020003, 0x030002, 0x020000, 0x020004, 0x020003, 0x040005
    ];

    /// <summary>Decodes a number in the range [0..255], by reading 1 - 11 bits.</summary>
    static int DecodeVarLenUnsignedByte(BitReader br)
    {
        if (BitReader.ReadBits(br, 1) != 0)
        {
            int n = BitReader.ReadBits(br, 3);
            if (n == 0)
            {
                return 1;
            }
            return BitReader.ReadBits(br, n) + (1<<n);
        }
        return 0;
    }

    static void DecodeMetaBlockLength(BitReader br, State state)
    {
        state.InputEnd = BitReader.ReadBits(br, 1) == 1;
        state.MetaBlockLength = 0;
        state.IsUncompressed = false;
        state.IsMetadata = false;
        if (state.InputEnd && BitReader.ReadBits(br, 1) != 0)
        {
            return;
        }
        int sizeNibbles = BitReader.ReadBits(br, 2) + 4;
        if (sizeNibbles == 7)
        {
            state.IsMetadata = true;
            if (BitReader.ReadBits(br, 1) != 0)
            {
                throw new BrotliRuntimeException("Corrupted reserved bit");
            }
            int sizeBytes = BitReader.ReadBits(br, 2);
            if (sizeBytes == 0)
            {
                return;
            }
            for (int i = 0; i < sizeBytes; i++)
            {
                int bits = BitReader.ReadBits(br, 8);
                if (bits == 0 && i + 1 == sizeBytes && sizeBytes > 1)
                {
                    throw new BrotliRuntimeException("Exuberant nibble");
                }
                state.MetaBlockLength |= bits<<i * 8;
            }
        }
        else
        {
            for (int i = 0; i < sizeNibbles; i++)
            {
                int bits = BitReader.ReadBits(br, 4);
                if (bits == 0 && i + 1 == sizeNibbles && sizeNibbles > 4)
                {
                    throw new BrotliRuntimeException("Exuberant nibble");
                }
                state.MetaBlockLength |= bits<<i * 4;
            }
        }
        state.MetaBlockLength++;
        if (!state.InputEnd)
        {
            state.IsUncompressed = BitReader.ReadBits(br, 1) == 1;
        }
    }

    /// <summary>Decodes the next Huffman code from bit-stream.</summary>
    static int ReadSymbol(int[] table, int offset, BitReader br)
    {
        int val = (int)(long)((ulong)br.Accumulator>> br.BITOffset);
        offset += val & HuffmanTableMask;
        int bits = table[offset]>> 16;
        int sym = table[offset] & 0xFFFF;
        if (bits <= HuffmanTableBits)
        {
            br.BITOffset += bits;
            return sym;
        }
        offset += sym;
        int mask = (1<<bits) - 1;
        offset += (int)((uint)(val & mask)>> HuffmanTableBits);
        br.BITOffset += (table[offset]>> 16) + HuffmanTableBits;
        return table[offset] & 0xFFFF;
    }

    static int ReadBlockLength(int[] table, int offset, BitReader br)
    {
        BitReader.FillBitWindow(br);
        int code = ReadSymbol(table, offset, br);
        int n = Prefix.BlockLengthNBits[code];
        return Prefix.BlockLengthOffset[code] + BitReader.ReadBits(br, n);
    }

    static int TranslateShortCodes(int code, int[] ringBuffer, int index)
    {
        if (code < NumDistanceShortCodes)
        {
            index += DistanceShortCodeIndexOffset[code];
            index &= 3;
            return ringBuffer[index] + DistanceShortCodeValueOffset[code];
        }
        return code - NumDistanceShortCodes + 1;
    }

    static void MoveToFront(int[] v, int index)
    {
        int value = v[index];
        for (; index > 0; index--)
        {
            v[index] = v[index - 1];
        }
        v[0] = value;
    }

    static void InverseMoveToFrontTransform(byte[] v, int vLen)
    {
        int[] mtf = new int[256];
        for (int i = 0; i < 256; i++)
        {
            mtf[i] = i;
        }
        for (int i = 0; i < vLen; i++)
        {
            int index = v[i] & 0xFF;
            v[i] = unchecked((byte)mtf[index]);
            if (index != 0)
            {
                MoveToFront(mtf, index);
            }
        }
    }

    static void ReadHuffmanCodeLengths(int[] codeLengthCodeLengths, int numSymbols, int[] codeLengths, BitReader br)
    {
        int symbol = 0;
        int prevCodeLen = DefaultCodeLength;
        int repeat = 0;
        int repeatCodeLen = 0;
        int space = 32768;
        int[] table = new int[32];
        Huffman.BuildHuffmanTable(table, 0, 5, codeLengthCodeLengths, CodeLengthCodes);
        while (symbol < numSymbols && space > 0)
        {
            BitReader.ReadMoreInput(br);
            BitReader.FillBitWindow(br);
            int p = (int)(long)((ulong)br.Accumulator>> br.BITOffset) & 31;
            br.BITOffset += table[p]>> 16;
            int codeLen = table[p] & 0xFFFF;
            if (codeLen < CodeLengthRepeatCode)
            {
                repeat = 0;
                codeLengths[symbol++] = codeLen;
                if (codeLen != 0)
                {
                    prevCodeLen = codeLen;
                    space -= 32768>> codeLen;
                }
            }
            else
            {
                int extraBits = codeLen - 14;
                int newLen = 0;
                if (codeLen == CodeLengthRepeatCode)
                {
                    newLen = prevCodeLen;
                }
                if (repeatCodeLen != newLen)
                {
                    repeat = 0;
                    repeatCodeLen = newLen;
                }
                int oldRepeat = repeat;
                if (repeat > 0)
                {
                    repeat -= 2;
                    repeat <<= extraBits;
                }
                repeat += BitReader.ReadBits(br, extraBits) + 3;
                int repeatDelta = repeat - oldRepeat;
                if (symbol + repeatDelta > numSymbols)
                {
                    throw new BrotliRuntimeException("symbol + repeatDelta > numSymbols");
                }
                // COV_NF_LINE
                for (int i = 0; i < repeatDelta; i++)
                {
                    codeLengths[symbol++] = repeatCodeLen;
                }
                if (repeatCodeLen != 0)
                {
                    space -= repeatDelta<<15 - repeatCodeLen;
                }
            }
        }
        if (space != 0)
        {
            throw new BrotliRuntimeException("Unused space");
        }
        // COV_NF_LINE
        // TODO: Pass max_symbol to Huffman table builder instead?
        Utils.FillWithZeroes(codeLengths, symbol, numSymbols - symbol);
    }

    // TODO: Use specialized versions for smaller tables.
    internal static void ReadHuffmanCode(int alphabetSize, int[] table, int offset, BitReader br)
    {
        bool ok = true;
        int simpleCodeOrSkip;
        BitReader.ReadMoreInput(br);
        // TODO: Avoid allocation.
        int[] codeLengths = new int[alphabetSize];
        simpleCodeOrSkip = BitReader.ReadBits(br, 2);
        if (simpleCodeOrSkip == 1)
        {
            // Read symbols, codes & code lengths directly.
            int maxBitsCounter = alphabetSize - 1;
            int maxBits = 0;
            int[] symbols = new int[4];
            int numSymbols = BitReader.ReadBits(br, 2) + 1;
            while (maxBitsCounter != 0)
            {
                maxBitsCounter >>= 1;
                maxBits++;
            }
            // TODO: uncomment when codeLengths is reused.
            // Utils.fillWithZeroes(codeLengths, 0, alphabetSize);
            for (int i = 0; i < numSymbols; i++)
            {
                symbols[i] = BitReader.ReadBits(br, maxBits) % alphabetSize;
                codeLengths[symbols[i]] = 2;
            }
            codeLengths[symbols[0]] = 1;
            switch (numSymbols)
            {
                case 1:
                {
                    break;
                }

                case 2:
                {
                    ok = symbols[0] != symbols[1];
                    codeLengths[symbols[1]] = 1;
                    break;
                }

                case 3:
                {
                    ok = symbols[0] != symbols[1] && symbols[0] != symbols[2] && symbols[1] != symbols[2];
                    break;
                }

                default:
                {
                    ok = symbols[0] != symbols[1]
                         && symbols[0] != symbols[2]
                         && symbols[0] != symbols[3]
                         && symbols[1] != symbols[2]
                         && symbols[1] != symbols[3]
                         && symbols[2] != symbols[3];
                    if (BitReader.ReadBits(br, 1) == 1)
                    {
                        codeLengths[symbols[2]] = 3;
                        codeLengths[symbols[3]] = 3;
                    }
                    else
                    {
                        codeLengths[symbols[0]] = 2;
                    }
                    break;
                }
            }
        }
        else
        {
            // Decode Huffman-coded code lengths.
            int[] codeLengthCodeLengths = new int[CodeLengthCodes];
            int space = 32;
            int numCodes = 0;
            for (int i = simpleCodeOrSkip; i < CodeLengthCodes && space > 0; i++)
            {
                int codeLenIdx = CodeLengthCodeOrder[i];
                BitReader.FillBitWindow(br);
                int p = (int)(br.Accumulator>>> br.BITOffset) & 15;
                // TODO: Demultiplex FIXED_TABLE.
                br.BITOffset += FixedTable[p]>> 16;
                int v = FixedTable[p] & 0xFFFF;
                codeLengthCodeLengths[codeLenIdx] = v;
                if (v != 0)
                {
                    space -= 32>> v;
                    numCodes++;
                }
            }
            ok = numCodes == 1 || space == 0;
            ReadHuffmanCodeLengths(codeLengthCodeLengths, alphabetSize, codeLengths, br);
        }
        if (!ok)
        {
            throw new BrotliRuntimeException("Can't readHuffmanCode");
        }
        // COV_NF_LINE
        Huffman.BuildHuffmanTable(table, offset, HuffmanTableBits, codeLengths, alphabetSize);
    }

    static int DecodeContextMap(int contextMapSize, byte[] contextMap, BitReader br)
    {
        BitReader.ReadMoreInput(br);
        int numTrees = DecodeVarLenUnsignedByte(br) + 1;
        if (numTrees == 1)
        {
            Utils.FillWithZeroes(contextMap, 0, contextMapSize);
            return numTrees;
        }
        bool useRleForZeros = BitReader.ReadBits(br, 1) == 1;
        int maxRunLengthPrefix = 0;
        if (useRleForZeros)
        {
            maxRunLengthPrefix = BitReader.ReadBits(br, 4) + 1;
        }
        int[] table = new int[Huffman.HuffmanMaxTableSize];
        ReadHuffmanCode(numTrees + maxRunLengthPrefix, table, 0, br);
        for (int i = 0; i < contextMapSize;)
        {
            BitReader.ReadMoreInput(br);
            BitReader.FillBitWindow(br);
            int code = ReadSymbol(table, 0, br);
            if (code == 0)
            {
                contextMap[i] = 0;
                i++;
            }
            else if (code <= maxRunLengthPrefix)
            {
                int reps = (1<<code) + BitReader.ReadBits(br, code);
                while (reps != 0)
                {
                    if (i >= contextMapSize)
                    {
                        throw new BrotliRuntimeException("Corrupted context map");
                    }
                    // COV_NF_LINE
                    contextMap[i] = 0;
                    i++;
                    reps--;
                }
            }
            else
            {
                contextMap[i] = unchecked((byte)(code - maxRunLengthPrefix));
                i++;
            }
        }
        if (BitReader.ReadBits(br, 1) == 1)
        {
            InverseMoveToFrontTransform(contextMap, contextMapSize);
        }
        return numTrees;
    }

    static void DecodeBlockTypeAndLength(State state, int treeType)
    {
        BitReader br = state.BR;
        int[] ringBuffers = state.BlockTypeRb;
        int offset = treeType * 2;
        BitReader.FillBitWindow(br);
        int blockType = ReadSymbol(state.BlockTypeTrees, treeType * Huffman.HuffmanMaxTableSize, br);
        state.BlockLength[treeType] = ReadBlockLength(state.BlockLenTrees, treeType * Huffman.HuffmanMaxTableSize, br);
        if (blockType == 1)
        {
            blockType = ringBuffers[offset + 1] + 1;
        }
        else if (blockType == 0)
        {
            blockType = ringBuffers[offset];
        }
        else
        {
            blockType -= 2;
        }
        if (blockType >= state.NumBlockTypes[treeType])
        {
            blockType -= state.NumBlockTypes[treeType];
        }
        ringBuffers[offset] = ringBuffers[offset + 1];
        ringBuffers[offset + 1] = blockType;
    }

    static void DecodeLiteralBlockSwitch(State state)
    {
        DecodeBlockTypeAndLength(state, 0);
        int literalBlockType = state.BlockTypeRb[1];
        state.ContextMapSlice = literalBlockType<<LiteralContextBits;
        state.LiteralTreeIndex = state.ContextMap == null ? 0 : state.ContextMap[state.ContextMapSlice] & 0xFF;
        state.LiteralTree = state.HGroup0.Trees[state.LiteralTreeIndex];
        int contextMode = state.ContextModes == null ? 0 : state.ContextModes[literalBlockType];
        state.ContextLookupOffset1 = Context.LookupOffsets[contextMode];
        state.ContextLookupOffset2 = Context.LookupOffsets[contextMode + 1];
    }

    static void DecodeCommandBlockSwitch(State state)
    {
        DecodeBlockTypeAndLength(state, 1);
        state.TreeCommandOffset = state.HGroup1.Trees[state.BlockTypeRb[3]];
    }

    static void DecodeDistanceBlockSwitch(State state)
    {
        DecodeBlockTypeAndLength(state, 2);
        state.DistContextMapSlice = state.BlockTypeRb[5]<<DistanceContextBits;
    }

    static void MaybeReallocateRingBuffer(State state)
    {
        int newSize = state.MaxRingBufferSize;
        if (newSize > state.ExpectedTotalSize)
        {
            /* TODO: Handle 2GB+ cases more gracefully. */
            int minimalNewSize = (int)state.ExpectedTotalSize + state.CustomDictionary.Length;
            while (newSize>> 1 > minimalNewSize)
            {
                newSize >>= 1;
            }
            if (!state.InputEnd && newSize < 16384 && state.MaxRingBufferSize >= 16384)
            {
                newSize = 16384;
            }
        }
        if (newSize <= state.RingBufferSize)
        {
            return;
        }
        int ringBufferSizeWithSlack = newSize + Dictionary.MaxTransformedWordLength;
        byte[] newBuffer = new byte[ringBufferSizeWithSlack];
        if (state.RingBuffer != null)
        {
            Array.Copy(state.RingBuffer, 0, newBuffer, 0, state.RingBufferSize);
        }
        else if (state.CustomDictionary.Length != 0)
        {
            /* Prepend custom dictionary, if any. */
            int length = state.CustomDictionary.Length;
            int offset = 0;
            if (length > state.MaxBackwardDistance)
            {
                offset = length - state.MaxBackwardDistance;
                length = state.MaxBackwardDistance;
            }
            Array.Copy(state.CustomDictionary, offset, newBuffer, 0, length);
            state.Pos = length;
            state.BytesToIgnore = length;
        }
        state.RingBuffer = newBuffer;
        state.RingBufferSize = newSize;
    }

    /// <summary>Reads next metablock header.</summary>
    /// <param name="state">decoding state</param>
    static void ReadMetablockInfo(State state)
    {
        BitReader br = state.BR;
        if (state.InputEnd)
        {
            state.NextRunningState = RunningState.Finished;
            state.BytesToWrite = state.Pos;
            state.BytesWritten = 0;
            state.RunningState = RunningState.Write;
            return;
        }
        // TODO: Reset? Do we need this?
        state.HGroup0.Codes = [];
        state.HGroup0.Trees = [];
        state.HGroup1.Codes = [];
        state.HGroup1.Trees = [];
        state.HGroup2.Codes = [];
        state.HGroup2.Trees = [];
        BitReader.ReadMoreInput(br);
        DecodeMetaBlockLength(br, state);
        if (state.MetaBlockLength == 0 && !state.IsMetadata)
        {
            return;
        }
        if (state.IsUncompressed || state.IsMetadata)
        {
            BitReader.JumpToByteBoundary(br);
            state.RunningState = state.IsMetadata ? RunningState.ReadMetadata : RunningState.CopyUncompressed;
        }
        else
        {
            state.RunningState = RunningState.CompressedBlockStart;
        }
        if (state.IsMetadata)
        {
            return;
        }
        state.ExpectedTotalSize += state.MetaBlockLength;
        if (state.RingBufferSize < state.MaxRingBufferSize)
        {
            MaybeReallocateRingBuffer(state);
        }
    }

    static void ReadMetablockHuffmanCodesAndContextMaps(State state)
    {
        BitReader br = state.BR;
        for (int i = 0; i < 3; i++)
        {
            state.NumBlockTypes[i] = DecodeVarLenUnsignedByte(br) + 1;
            state.BlockLength[i] = 1<<28;
            if (state.NumBlockTypes[i] > 1)
            {
                ReadHuffmanCode(state.NumBlockTypes[i] + 2, state.BlockTypeTrees, i * Huffman.HuffmanMaxTableSize, br);
                ReadHuffmanCode(NumBlockLengthCodes, state.BlockLenTrees, i * Huffman.HuffmanMaxTableSize, br);
                state.BlockLength[i] = ReadBlockLength(state.BlockLenTrees, i * Huffman.HuffmanMaxTableSize, br);
            }
        }
        BitReader.ReadMoreInput(br);
        state.DistancePostfixBits = BitReader.ReadBits(br, 2);
        state.NumDirectDistanceCodes = NumDistanceShortCodes + (BitReader.ReadBits(br, 4)<<state.DistancePostfixBits);
        state.DistancePostfixMask = (1<<state.DistancePostfixBits) - 1;
        int numDistanceCodes = state.NumDirectDistanceCodes + (48<<state.DistancePostfixBits);
        // TODO: Reuse?
        state.ContextModes = new byte[state.NumBlockTypes[0]];
        for (int i = 0; i < state.NumBlockTypes[0];)
        {
            /* Ensure that less than 256 bits read between readMoreInput. */
            int limit = System.Math.Min(i + 96, state.NumBlockTypes[0]);
            for (; i < limit; ++i)
            {
                state.ContextModes[i] = unchecked((byte)(BitReader.ReadBits(br, 2)<<1));
            }
            BitReader.ReadMoreInput(br);
        }
        // TODO: Reuse?
        state.ContextMap = new byte[state.NumBlockTypes[0]<<LiteralContextBits];
        int numLiteralTrees = DecodeContextMap(state.NumBlockTypes[0]<<LiteralContextBits, state.ContextMap, br);
        state.TrivialLiteralContext = true;
        for (int j = 0; j < state.NumBlockTypes[0]<<LiteralContextBits; j++)
        {
            if (state.ContextMap[j] != j>> LiteralContextBits)
            {
                state.TrivialLiteralContext = false;
                break;
            }
        }
        // TODO: Reuse?
        state.DistContextMap = new byte[state.NumBlockTypes[2]<<DistanceContextBits];
        int numDistTrees = DecodeContextMap(state.NumBlockTypes[2]<<DistanceContextBits, state.DistContextMap, br);
        HuffmanTreeGroup.Init(state.HGroup0, NumLiteralCodes, numLiteralTrees);
        HuffmanTreeGroup.Init(state.HGroup1, NumInsertAndCopyCodes, state.NumBlockTypes[1]);
        HuffmanTreeGroup.Init(state.HGroup2, numDistanceCodes, numDistTrees);
        HuffmanTreeGroup.Decode(state.HGroup0, br);
        HuffmanTreeGroup.Decode(state.HGroup1, br);
        HuffmanTreeGroup.Decode(state.HGroup2, br);
        state.ContextMapSlice = 0;
        state.DistContextMapSlice = 0;
        state.ContextLookupOffset1 = Context.LookupOffsets[state.ContextModes[0]];
        state.ContextLookupOffset2 = Context.LookupOffsets[state.ContextModes[0] + 1];
        state.LiteralTreeIndex = 0;
        state.LiteralTree = state.HGroup0.Trees[0];
        state.TreeCommandOffset = state.HGroup1.Trees[0];
        // TODO: == 0?
        state.BlockTypeRb[0] = state.BlockTypeRb[2] = state.BlockTypeRb[4] = 1;
        state.BlockTypeRb[1] = state.BlockTypeRb[3] = state.BlockTypeRb[5] = 0;
    }

    static void CopyUncompressedData(State state)
    {
        if (state.RingBuffer == null)
        {
            throw new InvalidOperationException("Ring buffer is null.");
        }

        BitReader br = state.BR;
        byte[] ringBuffer = state.RingBuffer;
        // Could happen if block ends at ring buffer end.
        if (state.MetaBlockLength <= 0)
        {
            BitReader.Reload(br);
            state.RunningState = RunningState.BlockStart;
            return;
        }
        int chunkLength = System.Math.Min(state.RingBufferSize - state.Pos, state.MetaBlockLength);
        BitReader.CopyBytes(br, ringBuffer, state.Pos, chunkLength);
        state.MetaBlockLength -= chunkLength;
        state.Pos += chunkLength;
        if (state.Pos == state.RingBufferSize)
        {
            state.NextRunningState = RunningState.CopyUncompressed;
            state.BytesToWrite = state.RingBufferSize;
            state.BytesWritten = 0;
            state.RunningState = RunningState.Write;
            return;
        }
        BitReader.Reload(br);
        state.RunningState = RunningState.BlockStart;
    }

    static bool WriteRingBuffer(State state)
    {
        if (state.RingBuffer == null)
        {
            throw new InvalidOperationException("Ring buffer is null.");
        }

        if (state.Output == null)
        {
            throw new InvalidOperationException("Output is null.");
        }

        /* Ignore custom dictionary bytes. */
        if (state.BytesToIgnore != 0)
        {
            state.BytesWritten += state.BytesToIgnore;
            state.BytesToIgnore = 0;
        }
        int toWrite = System.Math.Min(state.OutputLength - state.OutputUsed, state.BytesToWrite - state.BytesWritten);
        if (toWrite != 0)
        {
            Array.Copy(state.RingBuffer, state.BytesWritten, state.Output, state.OutputOffset + state.OutputUsed, toWrite);
            state.OutputUsed += toWrite;
            state.BytesWritten += toWrite;
        }
        return state.OutputUsed < state.OutputLength;
    }

    internal static void SetCustomDictionary(State state, byte[]? data) => state.CustomDictionary = data ?? [];

    /// <summary>Actual decompress implementation.</summary>
    internal static void Decompress(State state)
    {
        if (state.RunningState == RunningState.Uninitialized)
        {
            throw new InvalidOperationException("Can't decompress until initialized");
        }

        if (state.RunningState == RunningState.Closed)
        {
            throw new InvalidOperationException("Can't decompress after close");
        }

        if (state.RingBuffer == null)
        {
            throw new InvalidOperationException("Ring buffer is null.");
        }

        if (state.ContextMap == null)
        {
            throw new InvalidOperationException("Context map is null.");
        }

        if (state.DistContextMap == null)
        {
            throw new InvalidOperationException("Dist context map is null.");
        }

        BitReader br = state.BR;
        int ringBufferMask = state.RingBufferSize - 1;
        byte[] ringBuffer = state.RingBuffer;
        while (state.RunningState != RunningState.Finished)
        {
            switch (state.RunningState)
            {
                case RunningState.BlockStart:
                {
                    // TODO: extract cases to methods for the better readability.
                    if (state.MetaBlockLength < 0)
                    {
                        throw new BrotliRuntimeException("Invalid metablock length");
                    }
                    ReadMetablockInfo(state);
                    /* Ring-buffer would be reallocated here. */
                    ringBufferMask = state.RingBufferSize - 1;
                    ringBuffer = state.RingBuffer;
                    continue;
                }

                case RunningState.CompressedBlockStart:
                {
                    ReadMetablockHuffmanCodesAndContextMaps(state);
                    state.RunningState = RunningState.MainLoop;
                    goto case RunningState.MainLoop;
                }

                case RunningState.MainLoop:
                {
                    // Fall through
                    if (state.MetaBlockLength <= 0)
                    {
                        state.RunningState = RunningState.BlockStart;
                        continue;
                    }
                    BitReader.ReadMoreInput(br);
                    if (state.BlockLength[1] == 0)
                    {
                        DecodeCommandBlockSwitch(state);
                    }
                    state.BlockLength[1]--;
                    BitReader.FillBitWindow(br);
                    int cmdCode = ReadSymbol(state.HGroup1.Codes, state.TreeCommandOffset, br);
                    int rangeIdx = cmdCode>>> 6;
                    state.DistanceCode = 0;
                    if (rangeIdx >= 2)
                    {
                        rangeIdx -= 2;
                        state.DistanceCode = -1;
                    }
                    int insertCode = Prefix.InsertRangeLut[rangeIdx] + (cmdCode>>> 3 & 7);
                    int copyCode = Prefix.CopyRangeLut[rangeIdx] + (cmdCode & 7);
                    state.InsertLength = Prefix.InsertLengthOffset[insertCode] + BitReader.ReadBits(br, Prefix.InsertLengthNBits[insertCode]);
                    state.CopyLength = Prefix.CopyLengthOffset[copyCode] + BitReader.ReadBits(br, Prefix.CopyLengthNBits[copyCode]);
                    state.J = 0;
                    state.RunningState = RunningState.InsertLoop;
                    goto case RunningState.InsertLoop;
                }

                case RunningState.InsertLoop:
                {
                    // Fall through
                    if (state.TrivialLiteralContext)
                    {
                        while (state.J < state.InsertLength)
                        {
                            BitReader.ReadMoreInput(br);
                            if (state.BlockLength[0] == 0)
                            {
                                DecodeLiteralBlockSwitch(state);
                            }
                            state.BlockLength[0]--;
                            BitReader.FillBitWindow(br);
                            ringBuffer[state.Pos] = unchecked((byte)ReadSymbol(state.HGroup0.Codes, state.LiteralTree, br));
                            state.J++;
                            if (state.Pos++ == ringBufferMask)
                            {
                                state.NextRunningState = RunningState.InsertLoop;
                                state.BytesToWrite = state.RingBufferSize;
                                state.BytesWritten = 0;
                                state.RunningState = RunningState.Write;
                                break;
                            }
                        }
                    }
                    else
                    {
                        int prevByte1 = ringBuffer[state.Pos - 1 & ringBufferMask] & 0xFF;
                        int prevByte2 = ringBuffer[state.Pos - 2 & ringBufferMask] & 0xFF;
                        while (state.J < state.InsertLength)
                        {
                            BitReader.ReadMoreInput(br);
                            if (state.BlockLength[0] == 0)
                            {
                                DecodeLiteralBlockSwitch(state);
                            }
                            int literalTreeIndex = state.ContextMap[state.ContextMapSlice
                                                                    + (Context.Lookup[state.ContextLookupOffset1 + prevByte1]
                                                                       | Context.Lookup[state.ContextLookupOffset2 + prevByte2])]
                                                   & 0xFF;
                            state.BlockLength[0]--;
                            prevByte2 = prevByte1;
                            BitReader.FillBitWindow(br);
                            prevByte1 = ReadSymbol(state.HGroup0.Codes, state.HGroup0.Trees[literalTreeIndex], br);
                            ringBuffer[state.Pos] = unchecked((byte)prevByte1);
                            state.J++;
                            if (state.Pos++ == ringBufferMask)
                            {
                                state.NextRunningState = RunningState.InsertLoop;
                                state.BytesToWrite = state.RingBufferSize;
                                state.BytesWritten = 0;
                                state.RunningState = RunningState.Write;
                                break;
                            }
                        }
                    }
                    if (state.RunningState != RunningState.InsertLoop)
                    {
                        continue;
                    }
                    state.MetaBlockLength -= state.InsertLength;
                    if (state.MetaBlockLength <= 0)
                    {
                        state.RunningState = RunningState.MainLoop;
                        continue;
                    }
                    if (state.DistanceCode < 0)
                    {
                        BitReader.ReadMoreInput(br);
                        if (state.BlockLength[2] == 0)
                        {
                            DecodeDistanceBlockSwitch(state);
                        }
                        state.BlockLength[2]--;
                        BitReader.FillBitWindow(br);
                        state.DistanceCode = ReadSymbol(
                            state.HGroup2.Codes,
                            state.HGroup2.Trees[state.DistContextMap[state.DistContextMapSlice + (state.CopyLength > 4 ? 3 : state.CopyLength - 2)] & 0xFF],
                            br
                        );
                        if (state.DistanceCode >= state.NumDirectDistanceCodes)
                        {
                            state.DistanceCode -= state.NumDirectDistanceCodes;
                            int postfix = state.DistanceCode & state.DistancePostfixMask;
                            state.DistanceCode = (int)((uint)state.DistanceCode>> state.DistancePostfixBits);
                            int n = (int)((uint)state.DistanceCode>> 1) + 1;
                            int offset = (2 + (state.DistanceCode & 1)<<n) - 4;
                            state.DistanceCode = state.NumDirectDistanceCodes + postfix + (offset + BitReader.ReadBits(br, n)<<state.DistancePostfixBits);
                        }
                    }
                    // Convert the distance code to the actual distance by possibly looking up past distances
                    // from the ringBuffer.
                    state.Distance = TranslateShortCodes(state.DistanceCode, state.DistRb, state.DistRbIdx);
                    if (state.Distance < 0)
                    {
                        throw new BrotliRuntimeException("Negative distance");
                    }
                    // COV_NF_LINE
                    if (state.MaxDistance != state.MaxBackwardDistance && state.Pos < state.MaxBackwardDistance)
                    {
                        state.MaxDistance = state.Pos;
                    }
                    else
                    {
                        state.MaxDistance = state.MaxBackwardDistance;
                    }
                    state.CopyDst = state.Pos;
                    if (state.Distance > state.MaxDistance)
                    {
                        state.RunningState = RunningState.Transform;
                        continue;
                    }
                    if (state.DistanceCode > 0)
                    {
                        state.DistRb[state.DistRbIdx & 3] = state.Distance;
                        state.DistRbIdx++;
                    }
                    if (state.CopyLength > state.MetaBlockLength)
                    {
                        throw new BrotliRuntimeException("Invalid backward reference");
                    }
                    // COV_NF_LINE
                    state.J = 0;
                    state.RunningState = RunningState.CopyLoop;
                    goto case RunningState.CopyLoop;
                }

                case RunningState.CopyLoop:
                {
                    // fall through
                    int src = state.Pos - state.Distance & ringBufferMask;
                    int dst = state.Pos;
                    int copyLength = state.CopyLength - state.J;
                    if (src + copyLength < ringBufferMask && dst + copyLength < ringBufferMask)
                    {
                        for (int k = 0; k < copyLength; ++k)
                        {
                            ringBuffer[dst++] = ringBuffer[src++];
                        }
                        state.J += copyLength;
                        state.MetaBlockLength -= copyLength;
                        state.Pos += copyLength;
                    }
                    else
                    {
                        for (; state.J < state.CopyLength;)
                        {
                            ringBuffer[state.Pos] = ringBuffer[state.Pos - state.Distance & ringBufferMask];
                            state.MetaBlockLength--;
                            state.J++;
                            if (state.Pos++ == ringBufferMask)
                            {
                                state.NextRunningState = RunningState.CopyLoop;
                                state.BytesToWrite = state.RingBufferSize;
                                state.BytesWritten = 0;
                                state.RunningState = RunningState.Write;
                                break;
                            }
                        }
                    }
                    if (state.RunningState == RunningState.CopyLoop)
                    {
                        state.RunningState = RunningState.MainLoop;
                    }
                    continue;
                }

                case RunningState.Transform:
                {
                    if (state.CopyLength >= Dictionary.MinWordLength && state.CopyLength <= Dictionary.MaxWordLength)
                    {
                        int offset = Dictionary.OffsetsByLength[state.CopyLength];
                        int wordId = state.Distance - state.MaxDistance - 1;
                        int shift = Dictionary.SizeBitsByLength[state.CopyLength];
                        int mask = (1<<shift) - 1;
                        int wordIdx = wordId & mask;
                        int transformIdx = (int)((uint)wordId>> shift);
                        offset += wordIdx * state.CopyLength;
                        if (transformIdx < Transform.Transforms.Length)
                        {
                            int len = Transform.TransformDictionaryWord(
                                ringBuffer,
                                state.CopyDst,
                                Dictionary.GetData(),
                                offset,
                                state.CopyLength,
                                Transform.Transforms[transformIdx]
                            );
                            state.CopyDst += len;
                            state.Pos += len;
                            state.MetaBlockLength -= len;
                            if (state.CopyDst >= state.RingBufferSize)
                            {
                                state.NextRunningState = RunningState.CopyWrapBuffer;
                                state.BytesToWrite = state.RingBufferSize;
                                state.BytesWritten = 0;
                                state.RunningState = RunningState.Write;
                                continue;
                            }
                        }
                        else
                        {
                            throw new BrotliRuntimeException("Invalid backward reference");
                        }
                    }
                    else
                    {
                        // COV_NF_LINE
                        throw new BrotliRuntimeException("Invalid backward reference");
                    }
                    // COV_NF_LINE
                    state.RunningState = RunningState.MainLoop;
                    continue;
                }

                case RunningState.CopyWrapBuffer:
                {
                    Array.Copy(ringBuffer, state.RingBufferSize, ringBuffer, 0, state.CopyDst - state.RingBufferSize);
                    state.RunningState = RunningState.MainLoop;
                    continue;
                }

                case RunningState.ReadMetadata:
                {
                    while (state.MetaBlockLength > 0)
                    {
                        BitReader.ReadMoreInput(br);
                        // Optimize
                        BitReader.ReadBits(br, 8);
                        state.MetaBlockLength--;
                    }
                    state.RunningState = RunningState.BlockStart;
                    continue;
                }

                case RunningState.CopyUncompressed:
                {
                    CopyUncompressedData(state);
                    continue;
                }

                case RunningState.Write:
                {
                    if (!WriteRingBuffer(state))
                    {
                        // Output buffer is full.
                        return;
                    }
                    if (state.Pos >= state.MaxBackwardDistance)
                    {
                        state.MaxDistance = state.MaxBackwardDistance;
                    }
                    state.Pos &= ringBufferMask;
                    state.RunningState = state.NextRunningState;
                    continue;
                }

                default:
                {
                    throw new BrotliRuntimeException("Unexpected state " + state.RunningState);
                }
            }
        }
        if (state.RunningState == RunningState.Finished)
        {
            if (state.MetaBlockLength < 0)
            {
                throw new BrotliRuntimeException("Invalid metablock length");
            }
            BitReader.JumpToByteBoundary(br);
            BitReader.CheckHealth(state.BR, true);
        }
    }
}
