/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace UnityBundleReader.Brotli;

/// <summary>Contains a collection of huffman trees with the same alphabet size.</summary>
sealed class HuffmanTreeGroup
{
    /// <summary>The maximal alphabet size in this group.</summary>
    int _alphabetSize;

    /// <summary>Storage for Huffman lookup tables.</summary>
    internal int[] Codes = [];

    /// <summary>
    ///     Offsets of distinct lookup tables in
    ///     <see cref="Codes" />
    ///     storage.
    /// </summary>
    internal int[] Trees = [];

    /// <summary>Initializes the Huffman tree group.</summary>
    /// <param name="group">POJO to be initialised</param>
    /// <param name="alphabetSize">the maximal alphabet size in this group</param>
    /// <param name="n">number of Huffman codes</param>
    internal static void Init(HuffmanTreeGroup group, int alphabetSize, int n)
    {
        group._alphabetSize = alphabetSize;
        group.Codes = new int[n * Huffman.HuffmanMaxTableSize];
        group.Trees = new int[n];
    }

    /// <summary>Decodes Huffman trees from input stream and constructs lookup tables.</summary>
    /// <param name="group">target POJO</param>
    /// <param name="br">data source</param>
    internal static void Decode(HuffmanTreeGroup group, BitReader br)
    {
        int next = 0;
        int n = group.Trees.Length;
        for (int i = 0; i < n; i++)
        {
            group.Trees[i] = next;
            Brotli.Decode.ReadHuffmanCode(group._alphabetSize, group.Codes, next, br);
            next += Huffman.HuffmanMaxTableSize;
        }
    }
}
