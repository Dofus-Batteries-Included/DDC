using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class EditorExtension : Object
    {
        protected EditorExtension(ObjectReader reader) : base(reader)
        {
            if (Platform == BuildTarget.NoTarget)
            {
                var mPrefabParentObject = new PPtr<EditorExtension>(reader);
                var mPrefabInternal = new PPtr<Object>(reader); //PPtr<Prefab>
            }
        }
    }
}
