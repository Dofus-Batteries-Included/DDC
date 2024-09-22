namespace UnityBundleReader;

public class FileIdentifier
{
    public Guid Guid;
    public int Type; //enum { kNonAssetType = 0, kDeprecatedCachedAssetType = 1, kSerializedAssetType = 2, kMetaAssetType = 3 };
    public string? PathName;

    //custom
    public required string FileName;
}
