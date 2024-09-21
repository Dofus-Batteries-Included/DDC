namespace UnityBundleReader.Classes
{
    public abstract class EditorExtension : Object
    {
        protected EditorExtension(ObjectReader reader) : base(reader)
        {
            if (Platform == BuildTarget.NoTarget)
            {
                PPtr<EditorExtension>? mPrefabParentObject = new PPtr<EditorExtension>(reader);
                PPtr<Object>? mPrefabInternal = new PPtr<Object>(reader); //PPtr<Prefab>
            }
        }
    }
}
