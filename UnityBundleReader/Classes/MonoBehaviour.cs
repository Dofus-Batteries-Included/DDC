using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public sealed class MonoBehaviour : Behaviour
{
    public PPtr<MonoScript> Script;
    public readonly string? Name;

    public MonoBehaviour(ObjectReader reader) : base(reader)
    {
        Script = new PPtr<MonoScript>(reader);
        string name = reader.ReadAlignedString();
        Name = string.IsNullOrWhiteSpace(name) ? null : name;
    }
}
