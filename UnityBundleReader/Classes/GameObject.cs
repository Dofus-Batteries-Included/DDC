using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public sealed class GameObject : EditorExtension
{
    public string Name;

    public GameObject(ObjectReader reader) : base(reader)
    {
        int size = reader.ReadInt32();
        PPtr<Component>[] components = new PPtr<Component>[size];
        for (int i = 0; i < size; i++)
        {
            if (Versions[0] == 5 && Versions[1] < 5 || Versions[0] < 5) //5.5 down
            {
                int first = reader.ReadInt32();
            }
            components[i] = new PPtr<Component>(reader);
        }

        int layer = reader.ReadInt32();
        Name = reader.ReadAlignedString();
    }
}
