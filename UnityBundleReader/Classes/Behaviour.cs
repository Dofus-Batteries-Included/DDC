using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
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
