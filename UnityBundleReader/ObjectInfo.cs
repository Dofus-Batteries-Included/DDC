using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class ObjectInfo
    {
        public long ByteStart;
        public uint ByteSize;
        public int TypeID;
        public int ClassID;
        public ushort IsDestroyed;
        public byte Stripped;

        public long MPathID;
        public SerializedType SerializedType;
    }
}
