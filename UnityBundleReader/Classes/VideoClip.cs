using System.IO;

namespace AssetStudio
{
    public class StreamedResource
    {
        public string MSource;
        public long MOffset; //ulong
        public long MSize; //ulong

        public StreamedResource(BinaryReader reader)
        {
            MSource = reader.ReadAlignedString();
            MOffset = reader.ReadInt64();
            MSize = reader.ReadInt64();
        }
    }

    public sealed class VideoClip : NamedObject
    {
        public ResourceReader MVideoData;
        public string MOriginalPath;
        public StreamedResource MExternalResources;

        public VideoClip(ObjectReader reader) : base(reader)
        {
            MOriginalPath = reader.ReadAlignedString();
            var mProxyWidth = reader.ReadUInt32();
            var mProxyHeight = reader.ReadUInt32();
            var width = reader.ReadUInt32();
            var height = reader.ReadUInt32();
            if (Version[0] > 2017 || (Version[0] == 2017 && Version[1] >= 2)) //2017.2 and up
            {
                var mPixelAspecRatioNum = reader.ReadUInt32();
                var mPixelAspecRatioDen = reader.ReadUInt32();
            }
            var mFrameRate = reader.ReadDouble();
            var mFrameCount = reader.ReadUInt64();
            var mFormat = reader.ReadInt32();
            var mAudioChannelCount = reader.ReadUInt16Array();
            reader.AlignStream();
            var mAudioSampleRate = reader.ReadUInt32Array();
            var mAudioLanguage = reader.ReadStringArray();
            if (Version[0] >= 2020) //2020.1 and up
            {
                var mVideoShadersSize = reader.ReadInt32();
                var mVideoShaders = new PPtr<Shader>[mVideoShadersSize];
                for (int i = 0; i < mVideoShadersSize; i++)
                {
                    mVideoShaders[i] = new PPtr<Shader>(reader);
                }
            }
            MExternalResources = new StreamedResource(reader);
            var mHasSplitAlpha = reader.ReadBoolean();
            if (Version[0] >= 2020) //2020.1 and up
            {
                var mSRGB = reader.ReadBoolean();
            }

            ResourceReader resourceReader;
            if (!string.IsNullOrEmpty(MExternalResources.MSource))
            {
                resourceReader = new ResourceReader(MExternalResources.MSource, AssetsFile, MExternalResources.MOffset, MExternalResources.MSize);
            }
            else
            {
                resourceReader = new ResourceReader(reader, reader.BaseStream.Position, MExternalResources.MSize);
            }
            MVideoData = resourceReader;
        }
    }
}
