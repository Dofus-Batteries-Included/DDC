using UnityBundleReader.Extensions;

namespace UnityBundleReader
{
    public class ResourceReader
    {
        private bool _needSearch;
        private string _path;
        private SerializedFile _assetsFile;
        private long _offset;
        private long _size;
        private BinaryReader _reader;

        public int Size { get => (int)_size; }

        public ResourceReader(string path, SerializedFile assetsFile, long offset, long size)
        {
            _needSearch = true;
            _path = path;
            _assetsFile = assetsFile;
            _offset = offset;
            _size = size;
        }

        public ResourceReader(BinaryReader reader, long offset, long size)
        {
            _reader = reader;
            _offset = offset;
            _size = size;
        }

        private BinaryReader GetReader()
        {
            if (_needSearch)
            {
                string? resourceFileName = Path.GetFileName(_path);
                if (_assetsFile.AssetsManager.ResourceFileReaders.TryGetValue(resourceFileName, out _reader))
                {
                    _needSearch = false;
                    return _reader;
                }
                string? assetsFileDirectory = Path.GetDirectoryName(_assetsFile.FullName);
                string? resourceFilePath = Path.Combine(assetsFileDirectory, resourceFileName);
                if (!File.Exists(resourceFilePath))
                {
                    string[]? findFiles = Directory.GetFiles(assetsFileDirectory, resourceFileName, SearchOption.AllDirectories);
                    if (findFiles.Length > 0)
                    {
                        resourceFilePath = findFiles[0];
                    }
                }
                if (File.Exists(resourceFilePath))
                {
                    _needSearch = false;
                    _reader = new BinaryReader(File.OpenRead(resourceFilePath));
                    _assetsFile.AssetsManager.ResourceFileReaders.Add(resourceFileName, _reader);
                    return _reader;
                }
                throw new FileNotFoundException($"Can't find the resource file {resourceFileName}");
            }
            else
            {
                return _reader;
            }
        }

        public byte[] GetData()
        {
            BinaryReader? binaryReader = GetReader();
            binaryReader.BaseStream.Position = _offset;
            return binaryReader.ReadBytes((int)_size);
        }

        public void GetData(byte[] buff)
        {
            BinaryReader? binaryReader = GetReader();
            binaryReader.BaseStream.Position = _offset;
            binaryReader.Read(buff, 0, (int)_size);
        }

        public void WriteData(string path)
        {
            BinaryReader? binaryReader = GetReader();
            binaryReader.BaseStream.Position = _offset;
            using (FileStream? writer = File.OpenWrite(path))
            {
                binaryReader.BaseStream.CopyTo(writer, _size);
            }
        }
    }
}
