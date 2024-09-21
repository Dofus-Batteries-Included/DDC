using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public class StreamingInfo
    {
        public readonly long Offset; //ulong
        public readonly uint Size;
        public readonly string Path;

        public StreamingInfo(ObjectReader reader)
        {
            int[]? version = reader.Version;

            if (version[0] >= 2020) //2020.1 and up
            {
                Offset = reader.ReadInt64();
            }
            else
            {
                Offset = reader.ReadUInt32();
            }
            Size = reader.ReadUInt32();
            Path = reader.ReadAlignedString();
        }
    }

    public class GLTextureSettings
    {
        public int MFilterMode;
        public int MAniso;
        public float MMipBias;
        public int MWrapMode;

        public GLTextureSettings(ObjectReader reader)
        {
            int[]? version = reader.Version;

            MFilterMode = reader.ReadInt32();
            MAniso = reader.ReadInt32();
            MMipBias = reader.ReadSingle();
            if (version[0] >= 2017)//2017.x and up
            {
                MWrapMode = reader.ReadInt32(); //m_WrapU
                int mWrapV = reader.ReadInt32();
                int mWrapW = reader.ReadInt32();
            }
            else
            {
                MWrapMode = reader.ReadInt32();
            }
        }
    }

    public sealed class Texture2D : Texture
    {
        public int MWidth;
        public int MHeight;
        public TextureFormat MTextureFormat;
        public bool MMipMap;
        public int MMipCount;
        public GLTextureSettings MTextureSettings;
        public ResourceReader ImageData;
        public readonly StreamingInfo MStreamData;

        public Texture2D(ObjectReader reader) : base(reader)
        {
            MWidth = reader.ReadInt32();
            MHeight = reader.ReadInt32();
            int mCompleteImageSize = reader.ReadInt32();
            if (Version[0] >= 2020) //2020.1 and up
            {
                int mMipsStripped = reader.ReadInt32();
            }
            MTextureFormat = (TextureFormat)reader.ReadInt32();
            if (Version[0] < 5 || (Version[0] == 5 && Version[1] < 2)) //5.2 down
            {
                MMipMap = reader.ReadBoolean();
            }
            else
            {
                MMipCount = reader.ReadInt32();
            }
            if (Version[0] > 2 || (Version[0] == 2 && Version[1] >= 6)) //2.6.0 and up
            {
                bool mIsReadable = reader.ReadBoolean();
            }
            if (Version[0] >= 2020) //2020.1 and up
            {
                bool mIsPreProcessed = reader.ReadBoolean();
            }
            if (Version[0] > 2019 || (Version[0] == 2019 && Version[1] >= 3)) //2019.3 and up
            {
                bool mIgnoreMasterTextureLimit = reader.ReadBoolean();
            }
            if (Version[0] >= 3) //3.0.0 - 5.4
            {
                if (Version[0] < 5 || (Version[0] == 5 && Version[1] <= 4))
                {
                    bool mReadAllowed = reader.ReadBoolean();
                }
            }
            if (Version[0] > 2018 || (Version[0] == 2018 && Version[1] >= 2)) //2018.2 and up
            {
                bool mStreamingMipmaps = reader.ReadBoolean();
            }
            reader.AlignStream();
            if (Version[0] > 2018 || (Version[0] == 2018 && Version[1] >= 2)) //2018.2 and up
            {
                int mStreamingMipmapsPriority = reader.ReadInt32();
            }
            int mImageCount = reader.ReadInt32();
            int mTextureDimension = reader.ReadInt32();
            MTextureSettings = new GLTextureSettings(reader);
            if (Version[0] >= 3) //3.0 and up
            {
                int mLightmapFormat = reader.ReadInt32();
            }
            if (Version[0] > 3 || (Version[0] == 3 && Version[1] >= 5)) //3.5.0 and up
            {
                int mColorSpace = reader.ReadInt32();
            }
            if (Version[0] > 2020 || (Version[0] == 2020 && Version[1] >= 2)) //2020.2 and up
            {
                byte[]? mPlatformBlob = reader.ReadUInt8Array();
                reader.AlignStream();
            }
            int imageDataSize = reader.ReadInt32();
            if (imageDataSize == 0 && ((Version[0] == 5 && Version[1] >= 3) || Version[0] > 5))//5.3.0 and up
            {
                MStreamData = new StreamingInfo(reader);
            }

            ResourceReader resourceReader;
            if (!string.IsNullOrEmpty(MStreamData?.Path))
            {
                resourceReader = new ResourceReader(MStreamData.Path, AssetsFile, MStreamData.Offset, MStreamData.Size);
            }
            else
            {
                resourceReader = new ResourceReader(reader, reader.BaseStream.Position, imageDataSize);
            }
            ImageData = resourceReader;
        }
    }

    public enum TextureFormat
    {
        Alpha8 = 1,
        Argb4444,
        RGB24,
        Rgba32,
        Argb32,
        ArgbFloat,
        RGB565,
        Bgr24,
        R16,
        Dxt1,
        Dxt3,
        Dxt5,
        Rgba4444,
        Bgra32,
        RHalf,
        RgHalf,
        RgbaHalf,
        RFloat,
        RgFloat,
        RgbaFloat,
        Yuy2,
        RGB9E5Float,
        RGBFloat,
        BC6H,
        BC7,
        BC4,
        BC5,
        Dxt1Crunched,
        Dxt5Crunched,
        PVRTCRGB2,
        PVRTCRgba2,
        PVRTCRGB4,
        PVRTCRgba4,
        ETCRGB4,
        AtcRGB4,
        AtcRgba8,
        EACR = 41,
        EACRSigned,
        EACRg,
        EACRgSigned,
        ETC2RGB,
        ETC2Rgba1,
        ETC2Rgba8,
        AstcRGB4X4,
        AstcRGB5X5,
        AstcRGB6X6,
        AstcRGB8X8,
        AstcRGB10X10,
        AstcRGB12X12,
        AstcRgba4X4,
        AstcRgba5X5,
        AstcRgba6X6,
        AstcRgba8X8,
        AstcRgba10X10,
        AstcRgba12X12,
        ETCRGB43Ds,
        ETCRgba83Ds,
        Rg16,
        R8,
        ETCRGB4Crunched,
        ETC2Rgba8Crunched,
        AstcHDR4X4,
        AstcHDR5X5,
        AstcHDR6X6,
        AstcHDR8X8,
        AstcHDR10X10,
        AstcHDR12X12,
        Rg32,
        RGB48,
        Rgba64
    }
}