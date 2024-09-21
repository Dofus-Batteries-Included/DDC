using System.Collections.Generic;

namespace AssetStudio
{
    public class ResourceManager : Object
    {
        public KeyValuePair<string, PPtr<Object>>[] MContainer;

        public ResourceManager(ObjectReader reader) : base(reader)
        {
            var mContainerSize = reader.ReadInt32();
            MContainer = new KeyValuePair<string, PPtr<Object>>[mContainerSize];
            for (int i = 0; i < mContainerSize; i++)
            {
                MContainer[i] = new KeyValuePair<string, PPtr<Object>>(reader.ReadAlignedString(), new PPtr<Object>(reader));
            }
        }
    }
}
