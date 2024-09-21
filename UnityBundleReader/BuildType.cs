using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
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
