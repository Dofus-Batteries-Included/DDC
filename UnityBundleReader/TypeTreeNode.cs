namespace UnityBundleReader;

public class TypeTreeNode
{
    public string? Type;
    public string? Name;
    public int ByteSize;
    public int Index;
    public int TypeFlags; //m_IsArray
    public int Version;
    public int MetaFlag;
    public int Level;
    public uint TypeStrOffset;
    public uint NameStrOffset;
    public ulong RefTypeHash;

    public TypeTreeNode() { }

    public TypeTreeNode(string type, string name, int level, bool align)
    {
        Type = type;
        Name = name;
        Level = level;
        MetaFlag = align ? 0x4000 : 0;
    }
}
