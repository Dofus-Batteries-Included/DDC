﻿namespace UnityBundleReader.Classes;

public abstract class Component : EditorExtension
{
    public PPtr<GameObject> MGameObject;

    protected Component(ObjectReader reader) : base(reader)
    {
        MGameObject = new PPtr<GameObject>(reader);
    }
}
