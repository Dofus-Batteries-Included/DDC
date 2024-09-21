using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public class ResourceManager : Object
{
    public readonly KeyValuePair<string, PPtr<Object>>[] MContainer;

    public ResourceManager(ObjectReader reader) : base(reader)
    {
        int mContainerSize = reader.ReadInt32();
        MContainer = new KeyValuePair<string, PPtr<Object>>[mContainerSize];
        for (int i = 0; i < mContainerSize; i++)
        {
            MContainer[i] = new KeyValuePair<string, PPtr<Object>>(reader.ReadAlignedString(), new PPtr<Object>(reader));
        }
    }
}
