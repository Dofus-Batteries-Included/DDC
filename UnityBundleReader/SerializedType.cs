namespace UnityBundleReader
{
    public class SerializedType
    {
        public int ClassID;
        public bool MIsStrippedType;
        public short MScriptTypeIndex = -1;
        public TypeTree MType;
        public byte[] MScriptID; //Hash128
        public byte[] MOldTypeHash; //Hash128
        public int[] MTypeDependencies;
        public string MKlassName;
        public string MNameSpace;
        public string MAsmName;
    }
}
