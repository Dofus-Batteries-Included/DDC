using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public abstract class Behaviour : Component
{
    public byte Enabled;

    protected Behaviour(ObjectReader reader) : base(reader)
    {
        Enabled = reader.ReadByte();
        reader.AlignStream();
    }
}
