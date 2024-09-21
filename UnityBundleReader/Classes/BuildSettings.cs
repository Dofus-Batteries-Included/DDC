using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class BuildSettings : Object
    {
        public string MVersion;

        public BuildSettings(ObjectReader reader) : base(reader)
        {
            var levels = reader.ReadStringArray();

            var hasRenderTexture = reader.ReadBoolean();
            var hasProVersion = reader.ReadBoolean();
            var hasPublishingRights = reader.ReadBoolean();
            var hasShadows = reader.ReadBoolean();

            MVersion = reader.ReadAlignedString();
        }
    }
}
