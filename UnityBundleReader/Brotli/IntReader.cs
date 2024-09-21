/* Copyright 2017 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace UnityBundleReader.Brotli;

/// <summary>Byte-to-int conversion magic.</summary>
sealed class IntReader
{
    byte[] _byteBuffer = [];
    int[] _intBuffer = [];

    internal static void Init(IntReader ir, byte[] byteBuffer, int[] intBuffer)
    {
        ir._byteBuffer = byteBuffer;
        ir._intBuffer = intBuffer;
    }

    /// <summary>Translates bytes to ints.</summary>
    /// <remarks>
    ///     Translates bytes to ints.
    ///     NB: intLen == 4 * byteSize!
    ///     NB: intLen should be less or equal to intBuffer length.
    /// </remarks>
    internal static void Convert(IntReader ir, int intLen)
    {
        for (int i = 0; i < intLen; ++i)
        {
            ir._intBuffer[i] = ir._byteBuffer[i * 4] & 0xFF
                               | (ir._byteBuffer[i * 4 + 1] & 0xFF)<<8
                               | (ir._byteBuffer[i * 4 + 2] & 0xFF)<<16
                               | (ir._byteBuffer[i * 4 + 3] & 0xFF)<<24;
        }
    }
}
