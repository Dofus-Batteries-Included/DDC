using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AssetStudio
{
    public sealed class TextAsset : NamedObject
    {
        public byte[] MScript;

        public TextAsset(ObjectReader reader) : base(reader)
        {
            MScript = reader.ReadUInt8Array();
        }
    }
}
