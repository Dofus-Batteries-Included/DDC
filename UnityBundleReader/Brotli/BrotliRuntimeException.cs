/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace UnityBundleReader.Brotli;

/// <summary>Unchecked exception used internally.</summary>
[Serializable]
class BrotliRuntimeException : Exception
{
    internal BrotliRuntimeException(string message) : base(message)
    {
    }

    internal BrotliRuntimeException(string message, Exception cause) : base(message, cause)
    {
    }
}
