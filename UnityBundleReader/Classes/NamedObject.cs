using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public class NamedObject : EditorExtension
{
    public string Name;

    protected NamedObject(ObjectReader reader) : base(reader)
    {
        Name = reader.ReadAlignedString();
    }
}
