namespace UnityBundleReader;

public class BuildType
{
    readonly string _buildType;

    public BuildType(string type)
    {
        _buildType = type;
    }

    public bool IsAlpha => _buildType == "a";
    public bool IsPatch => _buildType == "p";
}
