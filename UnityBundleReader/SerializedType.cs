namespace UnityBundleReader;

public class SerializedType
{
    public int ClassID;
    public bool IsStrippedType;
    public short ScriptTypeIndex = -1;
    public TypeTree? Type;
    public byte[]? ScriptId; //Hash128
    public byte[]? OldTypeHash; //Hash128
    public int[]? TypeDependencies;
    public string? ClassName;
    public string? Namespace;
    public string? AsmName;
}
