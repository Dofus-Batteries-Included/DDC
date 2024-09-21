﻿using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public sealed class BuildSettings : Object
{
    public string MVersion;

    public BuildSettings(ObjectReader reader) : base(reader)
    {
        string[]? levels = reader.ReadStringArray();

        bool hasRenderTexture = reader.ReadBoolean();
        bool hasProVersion = reader.ReadBoolean();
        bool hasPublishingRights = reader.ReadBoolean();
        bool hasShadows = reader.ReadBoolean();

        MVersion = reader.ReadAlignedString();
    }
}
