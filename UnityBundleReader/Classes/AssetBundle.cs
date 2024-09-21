using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

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
    public AssetBundle(ObjectReader reader) : base(reader)
    {
        int mPreloadTableSize = reader.ReadInt32();
        PPtr<Object>[] preloadTable = new PPtr<Object>[mPreloadTableSize];
        for (int i = 0; i < mPreloadTableSize; i++)
        {
            preloadTable[i] = new PPtr<Object>(reader);
        }

        int mContainerSize = reader.ReadInt32();
        KeyValuePair<string, AssetInfo>[] container = new KeyValuePair<string, AssetInfo>[mContainerSize];
        for (int i = 0; i < mContainerSize; i++)
        {
            container[i] = new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader));
        }
    }
}
