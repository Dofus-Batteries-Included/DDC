using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public sealed class MonoBehaviour : Behaviour
{
    public PPtr<MonoScript> MScript;
    public readonly string MName;

    public MonoBehaviour(ObjectReader reader) : base(reader)
    {
        MScript = new PPtr<MonoScript>(reader);
        MName = reader.ReadAlignedString();
    }
}
