using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public class StreamedResource
{
    public readonly string MSource;
    public readonly long MOffset; //ulong
    public readonly long MSize; //ulong

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
    public readonly StreamedResource MExternalResources;

    public VideoClip(ObjectReader reader) : base(reader)
    {
        MOriginalPath = reader.ReadAlignedString();
        uint mProxyWidth = reader.ReadUInt32();
        uint mProxyHeight = reader.ReadUInt32();
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        if (Version[0] > 2017 || Version[0] == 2017 && Version[1] >= 2) //2017.2 and up
        {
            uint mPixelAspecRatioNum = reader.ReadUInt32();
            uint mPixelAspecRatioDen = reader.ReadUInt32();
        }
        double mFrameRate = reader.ReadDouble();
        ulong mFrameCount = reader.ReadUInt64();
        int mFormat = reader.ReadInt32();
        ushort[]? mAudioChannelCount = reader.ReadUInt16Array();
        reader.AlignStream();
        uint[]? mAudioSampleRate = reader.ReadUInt32Array();
        string[]? mAudioLanguage = reader.ReadStringArray();
        if (Version[0] >= 2020) //2020.1 and up
        {
            int mVideoShadersSize = reader.ReadInt32();
            PPtr<Shader>[]? mVideoShaders = new PPtr<Shader>[mVideoShadersSize];
            for (int i = 0; i < mVideoShadersSize; i++)
            {
                mVideoShaders[i] = new PPtr<Shader>(reader);
            }
        }
        MExternalResources = new StreamedResource(reader);
        bool mHasSplitAlpha = reader.ReadBoolean();
        if (Version[0] >= 2020) //2020.1 and up
        {
            bool mSRGB = reader.ReadBoolean();
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
