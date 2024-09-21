using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class PlayerSettings : Object
    {
        public string CompanyName;
        public string ProductName;

        public PlayerSettings(ObjectReader reader) : base(reader)
        {
            if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 4)) //5.4.0 nad up
            {
                byte[]? productGuid = reader.ReadBytes(16);
            }

            bool androidProfiler = reader.ReadBoolean();
            //bool AndroidFilterTouchesWhenObscured 2017.2 and up
            //bool AndroidEnableSustainedPerformanceMode 2018 and up
            reader.AlignStream();
            int defaultScreenOrientation = reader.ReadInt32();
            int targetDevice = reader.ReadInt32();
            if (Version[0] < 5 || (Version[0] == 5 && Version[1] < 3)) //5.3 down
            {
                if (Version[0] < 5) //5.0 down
                {
                    int targetPlatform = reader.ReadInt32(); //4.0 and up targetGlesGraphics
                    if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 6)) //4.6 and up
                    {
                        int targetIOSGraphics = reader.ReadInt32();
                    }
                }
                int targetResolution = reader.ReadInt32();
            }
            else
            {
                bool useOnDemandResources = reader.ReadBoolean();
                reader.AlignStream();
            }
            if (Version[0] > 3 || (Version[0] == 3 && Version[1] >= 5)) //3.5 and up
            {
                int accelerometerFrequency = reader.ReadInt32();
            }
            CompanyName = reader.ReadAlignedString();
            ProductName = reader.ReadAlignedString();
        }
    }
}
