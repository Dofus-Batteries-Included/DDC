namespace UnityBundleReader.Classes
{
    public sealed class PPtr<T> where T : Object
    {
        public int MFileID;
        public long MPathID;

        private SerializedFile _assetsFile;
        private int _index = -2; //-2 - Prepare, -1 - Missing

        public PPtr(ObjectReader reader)
        {
            MFileID = reader.ReadInt32();
            MPathID = reader.MVersion < SerializedFileFormatVersion.Unknown14 ? reader.ReadInt32() : reader.ReadInt64();
            _assetsFile = reader.AssetsFile;
        }

        private bool TryGetAssetsFile(out SerializedFile result)
        {
            result = null;
            if (MFileID == 0)
            {
                result = _assetsFile;
                return true;
            }

            if (MFileID > 0 && MFileID - 1 < _assetsFile.MExternals.Count)
            {
                var assetsManager = _assetsFile.AssetsManager;
                var assetsFileList = assetsManager.AssetsFileList;
                var assetsFileIndexCache = assetsManager.AssetsFileIndexCache;

                if (_index == -2)
                {
                    var mExternal = _assetsFile.MExternals[MFileID - 1];
                    var name = mExternal.FileName;
                    if (!assetsFileIndexCache.TryGetValue(name, out _index))
                    {
                        _index = assetsFileList.FindIndex(x => x.FileName.Equals(name, StringComparison.OrdinalIgnoreCase));
                        assetsFileIndexCache.Add(name, _index);
                    }
                }

                if (_index >= 0)
                {
                    result = assetsFileList[_index];
                    return true;
                }
            }

            return false;
        }

        public bool TryGet(out T result)
        {
            if (TryGetAssetsFile(out var sourceFile))
            {
                if (sourceFile.ObjectsDic.TryGetValue(MPathID, out var obj))
                {
                    if (obj is T variable)
                    {
                        result = variable;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public bool TryGet<T2>(out T2 result) where T2 : Object
        {
            if (TryGetAssetsFile(out var sourceFile))
            {
                if (sourceFile.ObjectsDic.TryGetValue(MPathID, out var obj))
                {
                    if (obj is T2 variable)
                    {
                        result = variable;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public void Set(T mObject)
        {
            var name = mObject.AssetsFile.FileName;
            if (string.Equals(_assetsFile.FileName, name, StringComparison.OrdinalIgnoreCase))
            {
                MFileID = 0;
            }
            else
            {
                MFileID = _assetsFile.MExternals.FindIndex(x => string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase));
                if (MFileID == -1)
                {
                    _assetsFile.MExternals.Add(new FileIdentifier
                    {
                        FileName = mObject.AssetsFile.FileName
                    });
                    MFileID = _assetsFile.MExternals.Count;
                }
                else
                {
                    MFileID += 1;
                }
            }

            var assetsManager = _assetsFile.AssetsManager;
            var assetsFileList = assetsManager.AssetsFileList;
            var assetsFileIndexCache = assetsManager.AssetsFileIndexCache;

            if (!assetsFileIndexCache.TryGetValue(name, out _index))
            {
                _index = assetsFileList.FindIndex(x => x.FileName.Equals(name, StringComparison.OrdinalIgnoreCase));
                assetsFileIndexCache.Add(name, _index);
            }

            MPathID = mObject.MPathID;
        }

        public bool IsNull => MPathID == 0 || MFileID < 0;
    }
}
