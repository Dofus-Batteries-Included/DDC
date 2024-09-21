using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public class NamedObject : EditorExtension
{
    public string MName;

    protected NamedObject(ObjectReader reader) : base(reader)
    {
        MName = reader.ReadAlignedString();
    }
}
