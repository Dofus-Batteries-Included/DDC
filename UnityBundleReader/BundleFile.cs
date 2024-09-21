using K4os.Compression.LZ4;
using UnityBundleReader.Extensions;

namespace UnityBundleReader;

[Flags]
public enum ArchiveFlags
{
    CompressionTypeMask = 0x3f,
    BlocksAndDirectoryInfoCombined = 0x40,
    BlocksInfoAtTheEnd = 0x80,
    OldWebPluginCompatibility = 0x100,
    BlockInfoNeedPaddingAtStart = 0x200
}

[Flags]
public enum StorageBlockFlags
{
    CompressionTypeMask = 0x3f,
    Streamed = 0x40
}

public enum CompressionType
{
    None,
    Lzma,
    Lz4,
    Lz4Hc,
    Lzham
}

public class BundleFile
{
    public class Header
    {
        public string Signature;
        public uint Version;
        public string UnityVersion;
        public string UnityRevision;
        public long Size;
        public uint CompressedBlocksInfoSize;
        public uint UncompressedBlocksInfoSize;
        public ArchiveFlags Flags;
    }

    public class StorageBlock
    {
        public uint CompressedSize;
        public uint UncompressedSize;
        public StorageBlockFlags Flags;
    }

    public class Node
    {
        public long Offset;
        public long Size;
        public uint Flags;
        public string Path;
    }

    public readonly Header MHeader;
    StorageBlock[] _mBlocksInfo;
    Node[] _mDirectoryInfo;

    public StreamFile[] FileList;

    public BundleFile(FileReader reader)
    {
        MHeader = new Header();
        MHeader.Signature = reader.ReadStringToNull();
        MHeader.Version = reader.ReadUInt32();
        MHeader.UnityVersion = reader.ReadStringToNull();
        MHeader.UnityRevision = reader.ReadStringToNull();
        switch (MHeader.Signature)
        {
            case "UnityArchive":
                break; //TODO
            case "UnityWeb":
            case "UnityRaw":
                if (MHeader.Version == 6)
                {
                    goto case "UnityFS";
                }
                ReadHeaderAndBlocksInfo(reader);
                using (Stream? blocksStream = CreateBlocksStream(reader.FullPath))
                {
                    ReadBlocksAndDirectory(reader, blocksStream);
                    ReadFiles(blocksStream, reader.FullPath);
                }
                break;
            case "UnityFS":
                ReadHeader(reader);
                ReadBlocksInfoAndDirectory(reader);
                using (Stream? blocksStream = CreateBlocksStream(reader.FullPath))
                {
                    ReadBlocks(reader, blocksStream);
                    ReadFiles(blocksStream, reader.FullPath);
                }
                break;
        }
    }

    void ReadHeaderAndBlocksInfo(EndianBinaryReader reader)
    {
        if (MHeader.Version >= 4)
        {
            byte[]? hash = reader.ReadBytes(16);
            uint crc = reader.ReadUInt32();
        }
        uint minimumStreamedBytes = reader.ReadUInt32();
        MHeader.Size = reader.ReadUInt32();
        uint numberOfLevelsToDownloadBeforeStreaming = reader.ReadUInt32();
        int levelCount = reader.ReadInt32();
        _mBlocksInfo = new StorageBlock[1];
        for (int i = 0; i < levelCount; i++)
        {
            StorageBlock? storageBlock = new()
            {
                CompressedSize = reader.ReadUInt32(),
                UncompressedSize = reader.ReadUInt32()
            };
            if (i == levelCount - 1)
            {
                _mBlocksInfo[0] = storageBlock;
            }
        }
        if (MHeader.Version >= 2)
        {
            uint completeFileSize = reader.ReadUInt32();
        }
        if (MHeader.Version >= 3)
        {
            uint fileInfoHeaderSize = reader.ReadUInt32();
        }
        reader.Position = MHeader.Size;
    }

    Stream CreateBlocksStream(string path)
    {
        Stream blocksStream;
        long uncompressedSizeSum = _mBlocksInfo.Sum(x => x.UncompressedSize);
        if (uncompressedSizeSum >= int.MaxValue)
        {
            /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, uncompressedSizeSum);
            assetsDataStream = memoryMappedFile.CreateViewStream();*/
            blocksStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }
        else
        {
            blocksStream = new MemoryStream((int)uncompressedSizeSum);
        }
        return blocksStream;
    }

    void ReadBlocksAndDirectory(EndianBinaryReader reader, Stream blocksStream)
    {
        bool isCompressed = MHeader.Signature == "UnityWeb";
        foreach (StorageBlock? blockInfo in _mBlocksInfo)
        {
            byte[]? uncompressedBytes = reader.ReadBytes((int)blockInfo.CompressedSize);
            if (isCompressed)
            {
                using (MemoryStream? memoryStream = new(uncompressedBytes))
                {
                    using (MemoryStream? decompressStream = SevenZipHelper.StreamDecompress(memoryStream))
                    {
                        uncompressedBytes = decompressStream.ToArray();
                    }
                }
            }
            blocksStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
        }
        blocksStream.Position = 0;
        EndianBinaryReader? blocksReader = new(blocksStream);
        int nodesCount = blocksReader.ReadInt32();
        _mDirectoryInfo = new Node[nodesCount];
        for (int i = 0; i < nodesCount; i++)
        {
            _mDirectoryInfo[i] = new Node
            {
                Path = blocksReader.ReadStringToNull(),
                Offset = blocksReader.ReadUInt32(),
                Size = blocksReader.ReadUInt32()
            };
        }
    }

    public void ReadFiles(Stream blocksStream, string path)
    {
        FileList = new StreamFile[_mDirectoryInfo.Length];
        for (int i = 0; i < _mDirectoryInfo.Length; i++)
        {
            Node? node = _mDirectoryInfo[i];
            StreamFile? file = new();
            FileList[i] = file;
            file.path = node.Path;
            file.fileName = Path.GetFileName(node.Path);
            if (node.Size >= int.MaxValue)
            {
                /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, entryinfo_size);
                file.stream = memoryMappedFile.CreateViewStream();*/
                string? extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
                Directory.CreateDirectory(extractPath);
                file.stream = new FileStream(extractPath + file.fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            else
            {
                file.stream = new MemoryStream((int)node.Size);
            }
            blocksStream.Position = node.Offset;
            blocksStream.CopyTo(file.stream, node.Size);
            file.stream.Position = 0;
        }
    }

    void ReadHeader(EndianBinaryReader reader)
    {
        MHeader.Size = reader.ReadInt64();
        MHeader.CompressedBlocksInfoSize = reader.ReadUInt32();
        MHeader.UncompressedBlocksInfoSize = reader.ReadUInt32();
        MHeader.Flags = (ArchiveFlags)reader.ReadUInt32();
        if (MHeader.Signature != "UnityFS")
        {
            reader.ReadByte();
        }
    }

    void ReadBlocksInfoAndDirectory(EndianBinaryReader reader)
    {
        byte[] blocksInfoBytes;
        if (MHeader.Version >= 7)
        {
            reader.AlignStream(16);
        }
        if ((MHeader.Flags & ArchiveFlags.BlocksInfoAtTheEnd) != 0)
        {
            long position = reader.Position;
            reader.Position = reader.BaseStream.Length - MHeader.CompressedBlocksInfoSize;
            blocksInfoBytes = reader.ReadBytes((int)MHeader.CompressedBlocksInfoSize);
            reader.Position = position;
        }
        else //0x40 BlocksAndDirectoryInfoCombined
        {
            blocksInfoBytes = reader.ReadBytes((int)MHeader.CompressedBlocksInfoSize);
        }
        MemoryStream blocksInfoUncompresseddStream;
        uint uncompressedSize = MHeader.UncompressedBlocksInfoSize;
        CompressionType compressionType = (CompressionType)(MHeader.Flags & ArchiveFlags.CompressionTypeMask);
        switch (compressionType)
        {
            case CompressionType.None:
            {
                blocksInfoUncompresseddStream = new MemoryStream(blocksInfoBytes);
                break;
            }
            case CompressionType.Lzma:
            {
                blocksInfoUncompresseddStream = new MemoryStream((int)uncompressedSize);
                using (MemoryStream? blocksInfoCompressedStream = new(blocksInfoBytes))
                {
                    SevenZipHelper.StreamDecompress(
                        blocksInfoCompressedStream,
                        blocksInfoUncompresseddStream,
                        MHeader.CompressedBlocksInfoSize,
                        MHeader.UncompressedBlocksInfoSize
                    );
                }
                blocksInfoUncompresseddStream.Position = 0;
                break;
            }
            case CompressionType.Lz4:
            case CompressionType.Lz4Hc:
            {
                byte[]? uncompressedBytes = new byte[uncompressedSize];
                int numWrite = LZ4Codec.Decode(blocksInfoBytes, uncompressedBytes);
                if (numWrite != uncompressedSize)
                {
                    throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                }
                blocksInfoUncompresseddStream = new MemoryStream(uncompressedBytes);
                break;
            }
            default:
                throw new IOException($"Unsupported compression type {compressionType}");
        }
        using (EndianBinaryReader? blocksInfoReader = new(blocksInfoUncompresseddStream))
        {
            byte[]? uncompressedDataHash = blocksInfoReader.ReadBytes(16);
            int blocksInfoCount = blocksInfoReader.ReadInt32();
            _mBlocksInfo = new StorageBlock[blocksInfoCount];
            for (int i = 0; i < blocksInfoCount; i++)
            {
                _mBlocksInfo[i] = new StorageBlock
                {
                    UncompressedSize = blocksInfoReader.ReadUInt32(),
                    CompressedSize = blocksInfoReader.ReadUInt32(),
                    Flags = (StorageBlockFlags)blocksInfoReader.ReadUInt16()
                };
            }

            int nodesCount = blocksInfoReader.ReadInt32();
            _mDirectoryInfo = new Node[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                _mDirectoryInfo[i] = new Node
                {
                    Offset = blocksInfoReader.ReadInt64(),
                    Size = blocksInfoReader.ReadInt64(),
                    Flags = blocksInfoReader.ReadUInt32(),
                    Path = blocksInfoReader.ReadStringToNull()
                };
            }
        }
        if ((MHeader.Flags & ArchiveFlags.BlockInfoNeedPaddingAtStart) != 0)
        {
            reader.AlignStream(16);
        }
    }

    void ReadBlocks(EndianBinaryReader reader, Stream blocksStream)
    {
        foreach (StorageBlock? blockInfo in _mBlocksInfo)
        {
            CompressionType compressionType = (CompressionType)(blockInfo.Flags & StorageBlockFlags.CompressionTypeMask);
            switch (compressionType)
            {
                case CompressionType.None:
                {
                    reader.BaseStream.CopyTo(blocksStream, blockInfo.CompressedSize);
                    break;
                }
                case CompressionType.Lzma:
                {
                    SevenZipHelper.StreamDecompress(reader.BaseStream, blocksStream, blockInfo.CompressedSize, blockInfo.UncompressedSize);
                    break;
                }
                case CompressionType.Lz4:
                case CompressionType.Lz4Hc:
                {
                    int compressedSize = (int)blockInfo.CompressedSize;
                    byte[]? compressedBytes = BigArrayPool<byte>.Shared.Rent(compressedSize);
                    reader.Read(compressedBytes, 0, compressedSize);
                    int uncompressedSize = (int)blockInfo.UncompressedSize;
                    byte[]? uncompressedBytes = BigArrayPool<byte>.Shared.Rent(uncompressedSize);
                    int numWrite = LZ4Codec.Decode(compressedBytes, 0, compressedSize, uncompressedBytes, 0, uncompressedSize);
                    if (numWrite != uncompressedSize)
                    {
                        throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                    }
                    blocksStream.Write(uncompressedBytes, 0, uncompressedSize);
                    BigArrayPool<byte>.Shared.Return(compressedBytes);
                    BigArrayPool<byte>.Shared.Return(uncompressedBytes);
                    break;
                }
                default:
                    throw new IOException($"Unsupported compression type {compressionType}");
            }
        }
        blocksStream.Position = 0;
    }
}
