namespace UnityBundleReader.Classes;

public abstract class EditorExtension : Object
{
    protected EditorExtension(ObjectReader reader) : base(reader)
    {
        if (Platform == BuildTarget.NoTarget)
        {
            PPtr<EditorExtension> prefabParents = new(reader);
            PPtr<Object> prefabs = new(reader); //PPtr<Prefab>
        }
    }
}
