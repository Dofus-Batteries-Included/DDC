using UnityBundleReader.Extensions;
using UnityBundleReader.Math;

namespace UnityBundleReader.Classes
{
    public class UnityTexEnv
    {
        public PPtr<Texture> MTexture;
        public Vector2 MScale;
        public Vector2 MOffset;

        public UnityTexEnv(ObjectReader reader)
        {
            MTexture = new PPtr<Texture>(reader);
            MScale = reader.ReadVector2();
            MOffset = reader.ReadVector2();
        }
    }

    public class UnityPropertySheet
    {
        public KeyValuePair<string, UnityTexEnv>[] MTexEnvs;
        public KeyValuePair<string, int>[] MInts;
        public KeyValuePair<string, float>[] MFloats;
        public KeyValuePair<string, Color>[] MColors;

        public UnityPropertySheet(ObjectReader reader)
        {
            var version = reader.Version;

            int mTexEnvsSize = reader.ReadInt32();
            MTexEnvs = new KeyValuePair<string, UnityTexEnv>[mTexEnvsSize];
            for (int i = 0; i < mTexEnvsSize; i++)
            {
                MTexEnvs[i] = new KeyValuePair<string, UnityTexEnv>(reader.ReadAlignedString(), new UnityTexEnv(reader));
            }

            if (version[0] >= 2021) //2021.1 and up
            {
                int mIntsSize = reader.ReadInt32();
                MInts = new KeyValuePair<string, int>[mIntsSize];
                for (int i = 0; i < mIntsSize; i++)
                {
                    MInts[i] = new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32());
                }
            }

            int mFloatsSize = reader.ReadInt32();
            MFloats = new KeyValuePair<string, float>[mFloatsSize];
            for (int i = 0; i < mFloatsSize; i++)
            {
                MFloats[i] = new KeyValuePair<string, float>(reader.ReadAlignedString(), reader.ReadSingle());
            }

            int mColorsSize = reader.ReadInt32();
            MColors = new KeyValuePair<string, Color>[mColorsSize];
            for (int i = 0; i < mColorsSize; i++)
            {
                MColors[i] = new KeyValuePair<string, Color>(reader.ReadAlignedString(), reader.ReadColor4());
            }
        }
    }

    public sealed class Material : NamedObject
    {
        public PPtr<Shader> MShader;
        public UnityPropertySheet MSavedProperties;

        public Material(ObjectReader reader) : base(reader)
        {
            MShader = new PPtr<Shader>(reader);

            if (Version[0] == 4 && Version[1] >= 1) //4.x
            {
                var mShaderKeywords = reader.ReadStringArray();
            }

            if (Version[0] > 2021 || (Version[0] == 2021 && Version[1] >= 3)) //2021.3 and up
            {
                var mValidKeywords = reader.ReadStringArray();
                var mInvalidKeywords = reader.ReadStringArray();
            }
            else if (Version[0] >= 5) //5.0 ~ 2021.2
            {
                var mShaderKeywords = reader.ReadAlignedString();
            }

            if (Version[0] >= 5) //5.0 and up
            {
                var mLightmapFlags = reader.ReadUInt32();
            }

            if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 6)) //5.6 and up
            {
                var mEnableInstancingVariants = reader.ReadBoolean();
                //var m_DoubleSidedGI = a_Stream.ReadBoolean(); //2017 and up
                reader.AlignStream();
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3)) //4.3 and up
            {
                var mCustomRenderQueue = reader.ReadInt32();
            }

            if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }

            if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadStringArray();
            }

            MSavedProperties = new UnityPropertySheet(reader);

            //vector m_BuildTextureStacks 2020 and up
        }
    }
}
