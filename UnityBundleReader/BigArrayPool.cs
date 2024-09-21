﻿using System.Buffers;

namespace AssetStudio
{
    public static class BigArrayPool<T>
    {
        private static readonly ArrayPool<T> SShared = ArrayPool<T>.Create(64 * 1024 * 1024, 3);
        public static ArrayPool<T> Shared => SShared;
    }
}
