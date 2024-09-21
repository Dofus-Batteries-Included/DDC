using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class SerializedFileHeader
    {
        public uint MMetadataSize;
        public long MFileSize;
        public SerializedFileFormatVersion MVersion;
        public long MDataOffset;
        public byte MEndianess;
        public byte[] MReserved;
    }
}
