using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class ObjectReader : EndianBinaryReader
    {
        public SerializedFile AssetsFile;
        public long MPathID;
        public long ByteStart;
        public uint ByteSize;
        public ClassIDType Type;
        public SerializedType SerializedType;
        public BuildTarget Platform;
        public SerializedFileFormatVersion MVersion;

        public int[] Version => AssetsFile.Version;
        public BuildType BuildType => AssetsFile.BuildType;

        public ObjectReader(EndianBinaryReader reader, SerializedFile assetsFile, ObjectInfo objectInfo) : base(reader.BaseStream, reader.Endian)
        {
            AssetsFile = assetsFile;
            MPathID = objectInfo.MPathID;
            ByteStart = objectInfo.ByteStart;
            ByteSize = objectInfo.ByteSize;
            if (Enum.IsDefined(typeof(ClassIDType), objectInfo.ClassID))
            {
                Type = (ClassIDType)objectInfo.ClassID;
            }
            else
            {
                Type = ClassIDType.UnknownType;
            }
            SerializedType = objectInfo.SerializedType;
            Platform = assetsFile.MTargetPlatform;
            MVersion = assetsFile.Header.MVersion;
        }

        public void Reset()
        {
            Position = ByteStart;
        }
    }
}
