using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public abstract class Behaviour : Component
    {
        public byte MEnabled;

        protected Behaviour(ObjectReader reader) : base(reader)
        {
            MEnabled = reader.ReadByte();
            reader.AlignStream();
        }
    }
}
