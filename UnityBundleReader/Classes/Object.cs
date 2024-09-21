using System.Collections.Specialized;

namespace UnityBundleReader.Classes
{
    public class Object
    {
        public SerializedFile AssetsFile;
        public ObjectReader Reader;
        public long MPathID;
        public int[] Version;
        protected BuildType BuildType;
        public BuildTarget Platform;
        public ClassIDType Type;
        public SerializedType SerializedType;
        public uint ByteSize;

        public Object(ObjectReader reader)
        {
            Reader = reader;
            reader.Reset();
            AssetsFile = reader.AssetsFile;
            Type = reader.Type;
            MPathID = reader.MPathID;
            Version = reader.Version;
            BuildType = reader.BuildType;
            Platform = reader.Platform;
            SerializedType = reader.SerializedType;
            ByteSize = reader.ByteSize;

            if (Platform == BuildTarget.NoTarget)
            {
                var mObjectHideFlags = reader.ReadUInt32();
            }
        }

        public string Dump()
        {
            if (SerializedType?.MType != null)
            {
                return TypeTreeHelper.ReadTypeString(SerializedType.MType, Reader);
            }
            return null;
        }

        public string Dump(TypeTree mType)
        {
            if (mType != null)
            {
                return TypeTreeHelper.ReadTypeString(mType, Reader);
            }
            return null;
        }

        public OrderedDictionary ToType()
        {
            if (SerializedType?.MType != null)
            {
                return TypeTreeHelper.ReadType(SerializedType.MType, Reader);
            }
            return null;
        }

        public OrderedDictionary ToType(TypeTree mType)
        {
            if (mType != null)
            {
                return TypeTreeHelper.ReadType(mType, Reader);
            }
            return null;
        }

        public byte[] GetRawData()
        {
            Reader.Reset();
            return Reader.ReadBytes((int)ByteSize);
        }
    }
}
