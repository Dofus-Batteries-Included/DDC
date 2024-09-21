using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class MonoScript : NamedObject
    {
        public string MClassName;
        public string MNamespace;
        public string MAssemblyName;

        public MonoScript(ObjectReader reader) : base(reader)
        {
            if (Version[0] > 3 || (Version[0] == 3 && Version[1] >= 4)) //3.4 and up
            {
                var mExecutionOrder = reader.ReadInt32();
            }
            if (Version[0] < 5) //5.0 down
            {
                var mPropertiesHash = reader.ReadUInt32();
            }
            else
            {
                var mPropertiesHash = reader.ReadBytes(16);
            }
            if (Version[0] < 3) //3.0 down
            {
                var mPathName = reader.ReadAlignedString();
            }
            MClassName = reader.ReadAlignedString();
            if (Version[0] >= 3) //3.0 and up
            {
                MNamespace = reader.ReadAlignedString();
            }
            MAssemblyName = reader.ReadAlignedString();
            if (Version[0] < 2018 || (Version[0] == 2018 && Version[1] < 2)) //2018.2 down
            {
                var mIsEditorScript = reader.ReadBoolean();
            }
        }
    }
}
