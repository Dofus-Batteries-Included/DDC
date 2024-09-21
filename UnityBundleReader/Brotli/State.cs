/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace UnityBundleReader.Brotli;

sealed class State
{
    internal int RunningState = Brotli.RunningState.Uninitialized;

    internal int NextRunningState;

    internal readonly BitReader BR = new();

    internal byte[]? RingBuffer;

    internal readonly int[] BlockTypeTrees = new int[3 * Huffman.HuffmanMaxTableSize];

    internal readonly int[] BlockLenTrees = new int[3 * Huffman.HuffmanMaxTableSize];

    internal int MetaBlockLength;

    internal bool InputEnd;

    internal bool IsUncompressed;

    internal bool IsMetadata;

    internal readonly HuffmanTreeGroup HGroup0 = new();

    internal readonly HuffmanTreeGroup HGroup1 = new();

    internal readonly HuffmanTreeGroup HGroup2 = new();

    internal readonly int[] BlockLength = new int[3];

    internal readonly int[] NumBlockTypes = new int[3];

    internal readonly int[] BlockTypeRb = new int[6];

    internal readonly int[] DistRb = [16, 15, 11, 4];

    internal int Pos = 0;

    internal int MaxDistance = 0;

    internal int DistRbIdx = 0;

    internal bool TrivialLiteralContext = false;

    internal int LiteralTreeIndex = 0;

    internal int LiteralTree;

    internal int J;

    internal int InsertLength;

    internal byte[]? ContextModes;

    internal byte[]? ContextMap;

    internal int ContextMapSlice;

    internal int DistContextMapSlice;

    internal int ContextLookupOffset1;

    internal int ContextLookupOffset2;

    internal int TreeCommandOffset;

    internal int DistanceCode;

    internal byte[]? DistContextMap;

    internal int NumDirectDistanceCodes;

    internal int DistancePostfixMask;

    internal int DistancePostfixBits;

    internal int Distance;

    internal int CopyLength;

    internal int CopyDst;

    internal int MaxBackwardDistance;

    internal int MaxRingBufferSize;

    internal int RingBufferSize = 0;

    internal long ExpectedTotalSize = 0;

    internal byte[] CustomDictionary = [];

    internal int BytesToIgnore = 0;

    internal int OutputOffset;

    internal int OutputLength;

    internal int OutputUsed;

    internal int BytesWritten;

    internal int BytesToWrite;

    internal byte[]? Output;

    // Current meta-block header information.
    // TODO: Update to current spec.
    static int DecodeWindowBits(BitReader br)
    {
        if (BitReader.ReadBits(br, 1) == 0)
        {
            return 16;
        }
        int n = BitReader.ReadBits(br, 3);
        if (n != 0)
        {
            return 17 + n;
        }
        n = BitReader.ReadBits(br, 3);
        if (n != 0)
        {
            return 8 + n;
        }
        return 17;
    }

    /// <summary>Associate input with decoder state.</summary>
    /// <param name="state">uninitialized state without associated input</param>
    /// <param name="input">compressed data source</param>
    internal static void SetInput(State state, Stream input)
    {
        if (state.RunningState != Brotli.RunningState.Uninitialized)
        {
            throw new InvalidOperationException("State MUST be uninitialized");
        }
        BitReader.Init(state.BR, input);
        int windowBits = DecodeWindowBits(state.BR);
        if (windowBits == 9)
        {
            /* Reserved case for future expansion. */
            throw new BrotliRuntimeException("Invalid 'windowBits' code");
        }
        state.MaxRingBufferSize = 1<<windowBits;
        state.MaxBackwardDistance = state.MaxRingBufferSize - 16;
        state.RunningState = Brotli.RunningState.BlockStart;
    }

    /// <exception cref="System.IO.IOException" />
    internal static void Close(State state)
    {
        if (state.RunningState == Brotli.RunningState.Uninitialized)
        {
            throw new InvalidOperationException("State MUST be initialized");
        }
        if (state.RunningState == Brotli.RunningState.Closed)
        {
            return;
        }
        state.RunningState = Brotli.RunningState.Closed;
        BitReader.Close(state.BR);
    }
}
