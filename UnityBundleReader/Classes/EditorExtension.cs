namespace UnityBundleReader.Classes;

public abstract class EditorExtension : Object
{
    protected EditorExtension(ObjectReader reader) : base(reader)
    {
        if (Platform == BuildTarget.NoTarget)
        {
            PPtr<EditorExtension>? mPrefabParentObject = new(reader);
            PPtr<Object>? mPrefabInternal = new(reader); //PPtr<Prefab>
        }
    }
}
