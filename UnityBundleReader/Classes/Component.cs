using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class Component : EditorExtension
    {
        public PPtr<GameObject> MGameObject;

        protected Component(ObjectReader reader) : base(reader)
        {
            MGameObject = new PPtr<GameObject>(reader);
        }
    }
}
