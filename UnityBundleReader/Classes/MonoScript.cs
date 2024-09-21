using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public sealed class MonoScript : NamedObject
{
    public string ClassName;
    public string? Namespace;
    public string AssemblyName;

    public MonoScript(ObjectReader reader) : base(reader)
    {
        if (Versions[0] > 3 || Versions[0] == 3 && Versions[1] >= 4) //3.4 and up
        {
            int executionOrder = reader.ReadInt32();
        }
        if (Versions[0] < 5) //5.0 down
        {
            uint propertiesHash = reader.ReadUInt32();
        }
        else
        {
            byte[] propertiesHash = reader.ReadBytes(16);
        }
        if (Versions[0] < 3) //3.0 down
        {
            string pathName = reader.ReadAlignedString();
        }
        ClassName = reader.ReadAlignedString();
        if (Versions[0] >= 3) //3.0 and up
        {
            Namespace = reader.ReadAlignedString();
        }
        AssemblyName = reader.ReadAlignedString();
        if (Versions[0] < 2018 || Versions[0] == 2018 && Versions[1] < 2) //2018.2 down
        {
            bool isEditorScript = reader.ReadBoolean();
        }
    }
}
