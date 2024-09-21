namespace UnityBundleReader
{
    public class BuildType
    {
        private string _buildType;

        public BuildType(string type)
        {
            _buildType = type;
        }

        public bool IsAlpha => _buildType == "a";
        public bool IsPatch => _buildType == "p";
    }
}
