using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class MonoBehaviour : Behaviour
    {
        public PPtr<MonoScript> MScript;
        public string MName;

        public MonoBehaviour(ObjectReader reader) : base(reader)
        {
            MScript = new PPtr<MonoScript>(reader);
            MName = reader.ReadAlignedString();
        }
    }
}
