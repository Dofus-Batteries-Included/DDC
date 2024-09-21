namespace UnityBundleReader.Classes;

public sealed class MeshFilter : Component
{
    public PPtr<Mesh> MMesh;

    public MeshFilter(ObjectReader reader) : base(reader)
    {
        MMesh = new PPtr<Mesh>(reader);
    }
}
