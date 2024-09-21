namespace UnityBundleReader.Classes;

public abstract class Component : EditorExtension
{
    public PPtr<GameObject> GameObject;

    protected Component(ObjectReader reader) : base(reader)
    {
        GameObject = new PPtr<GameObject>(reader);
    }
}
