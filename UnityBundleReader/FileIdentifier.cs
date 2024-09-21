using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class FileIdentifier
    {
        public Guid Guid;
        public int Type; //enum { kNonAssetType = 0, kDeprecatedCachedAssetType = 1, kSerializedAssetType = 2, kMetaAssetType = 3 };
        public string PathName;

        //custom
        public string FileName;
    }
}
