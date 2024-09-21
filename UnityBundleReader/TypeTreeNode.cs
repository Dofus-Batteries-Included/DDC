namespace UnityBundleReader
{
    public class TypeTreeNode
    {
        public string MType;
        public string MName;
        public int MByteSize;
        public int MIndex;
        public int MTypeFlags; //m_IsArray
        public int MVersion;
        public int MMetaFlag;
        public int MLevel;
        public uint MTypeStrOffset;
        public uint MNameStrOffset;
        public ulong MRefTypeHash;

        public TypeTreeNode() { }

        public TypeTreeNode(string type, string name, int level, bool align)
        {
            MType = type;
            MName = name;
            MLevel = level;
            MMetaFlag = align ? 0x4000 : 0;
        }
    }
}
