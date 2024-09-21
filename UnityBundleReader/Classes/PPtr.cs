using System.Diagnostics.CodeAnalysis;

namespace UnityBundleReader.Classes;

public sealed class PPtr<T> where T: Object
{
    int _fileId;
    long _pathId;

    readonly SerializedFile _assetsFile;
    int _index = -2; //-2 - Prepare, -1 - Missing

    public PPtr(ObjectReader reader)
    {
        _fileId = reader.ReadInt32();
        _pathId = reader.MVersion < SerializedFileFormatVersion.Unknown14 ? reader.ReadInt32() : reader.ReadInt64();
        _assetsFile = reader.AssetsFile;
    }

    bool TryGetAssetsFile([NotNullWhen(true)] out SerializedFile? result)
    {
        result = null;
        if (_fileId == 0)
        {
            result = _assetsFile;
            return true;
        }

        if (_fileId > 0 && _fileId - 1 < _assetsFile.Externals.Count)
        {
            AssetsManager assetsManager = _assetsFile.AssetsManager;
            List<SerializedFile> assetsFileList = assetsManager.AssetsFileList;
            Dictionary<string, int> assetsFileIndexCache = assetsManager.AssetsFileIndexCache;

            if (_index == -2)
            {
                FileIdentifier mExternal = _assetsFile.Externals[_fileId - 1];
                string name = mExternal.FileName;
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

    public bool TryGet([NotNullWhen(true)] out T? result)
    {
        if (TryGetAssetsFile(out SerializedFile? sourceFile))
        {
            if (sourceFile.ObjectsDic.TryGetValue(_pathId, out Object? obj))
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

    public bool TryGet<TCast>([NotNullWhen(true)] out TCast? result) where TCast: Object
    {
        if (TryGetAssetsFile(out SerializedFile? sourceFile))
        {
            if (sourceFile.ObjectsDic.TryGetValue(_pathId, out Object? obj))
            {
                if (obj is TCast variable)
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
        string name = mObject.AssetsFile.FileName;
        if (string.Equals(_assetsFile.FileName, name, StringComparison.OrdinalIgnoreCase))
        {
            _fileId = 0;
        }
        else
        {
            _fileId = _assetsFile.Externals.FindIndex(x => string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase));
            if (_fileId == -1)
            {
                _assetsFile.Externals.Add(
                    new FileIdentifier
                    {
                        FileName = mObject.AssetsFile.FileName
                    }
                );
                _fileId = _assetsFile.Externals.Count;
            }
            else
            {
                _fileId += 1;
            }
        }

        AssetsManager assetsManager = _assetsFile.AssetsManager;
        List<SerializedFile> assetsFileList = assetsManager.AssetsFileList;
        Dictionary<string, int> assetsFileIndexCache = assetsManager.AssetsFileIndexCache;

        if (!assetsFileIndexCache.TryGetValue(name, out _index))
        {
            _index = assetsFileList.FindIndex(x => x.FileName.Equals(name, StringComparison.OrdinalIgnoreCase));
            assetsFileIndexCache.Add(name, _index);
        }

        _pathId = mObject.PathId;
    }

    public bool IsNull => _pathId == 0 || _fileId < 0;
}
