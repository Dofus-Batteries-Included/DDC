using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class AssetInfo
    {
        public int PreloadIndex;
        public int PreloadSize;
        public PPtr<Object> Asset;

        public AssetInfo(ObjectReader reader)
        {
            PreloadIndex = reader.ReadInt32();
            PreloadSize = reader.ReadInt32();
            Asset = new PPtr<Object>(reader);
        }
    }

    public sealed class AssetBundle : NamedObject
    {
        public PPtr<Object>[] MPreloadTable;
        public KeyValuePair<string, AssetInfo>[] MContainer;

        public AssetBundle(ObjectReader reader) : base(reader)
        {
            var mPreloadTableSize = reader.ReadInt32();
            MPreloadTable = new PPtr<Object>[mPreloadTableSize];
            for (int i = 0; i < mPreloadTableSize; i++)
            {
                MPreloadTable[i] = new PPtr<Object>(reader);
            }

            var mContainerSize = reader.ReadInt32();
            MContainer = new KeyValuePair<string, AssetInfo>[mContainerSize];
            for (int i = 0; i < mContainerSize; i++)
            {
                MContainer[i] = new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader));
            }
        }
    }
}
