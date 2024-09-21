using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public abstract class Texture : NamedObject
    {
        protected Texture(ObjectReader reader) : base(reader)
        {
            if (Version[0] > 2017 || (Version[0] == 2017 && Version[1] >= 3)) //2017.3 and up
            {
                int mForcedFallbackFormat = reader.ReadInt32();
                bool mDownscaleFallback = reader.ReadBoolean();
                if (Version[0] > 2020 || (Version[0] == 2020 && Version[1] >= 2)) //2020.2 and up
                {
                    bool mIsAlphaChannelOptional = reader.ReadBoolean();
                }
                reader.AlignStream();
            }
        }
    }
}
