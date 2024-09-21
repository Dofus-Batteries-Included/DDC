using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public sealed class TextAsset : NamedObject
{
    public byte[] MScript;

    public TextAsset(ObjectReader reader) : base(reader)
    {
        MScript = reader.ReadUInt8Array();
    }
}
