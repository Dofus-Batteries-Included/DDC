using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public sealed class MonoBehaviour : Behaviour
{
    public PPtr<MonoScript> Script;
    public readonly string Name;

    public MonoBehaviour(ObjectReader reader) : base(reader)
    {
        Script = new PPtr<MonoScript>(reader);
        Name = reader.ReadAlignedString();
    }
}
