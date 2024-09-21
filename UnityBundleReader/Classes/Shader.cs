using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public class Hash128
    {
        public byte[] Bytes;

        public Hash128(BinaryReader reader)
        {
            Bytes = reader.ReadBytes(16);
        }
    }

    public class StructParameter
    {
        public MatrixParameter[] MMatrixParams;
        public VectorParameter[] MVectorParams;

        public StructParameter(BinaryReader reader)
        {
            var mNameIndex = reader.ReadInt32();
            var mIndex = reader.ReadInt32();
            var mArraySize = reader.ReadInt32();
            var mStructSize = reader.ReadInt32();

            int numVectorParams = reader.ReadInt32();
            MVectorParams = new VectorParameter[numVectorParams];
            for (int i = 0; i < numVectorParams; i++)
            {
                MVectorParams[i] = new VectorParameter(reader);
            }

            int numMatrixParams = reader.ReadInt32();
            MMatrixParams = new MatrixParameter[numMatrixParams];
            for (int i = 0; i < numMatrixParams; i++)
            {
                MMatrixParams[i] = new MatrixParameter(reader);
            }
        }
    }

    public class SamplerParameter
    {
        public uint Sampler;
        public int BindPoint;

        public SamplerParameter(BinaryReader reader)
        {
            Sampler = reader.ReadUInt32();
            BindPoint = reader.ReadInt32();
        }
    }
    public enum TextureDimension
    {
        Unknown = -1,
        None = 0,
        Any = 1,
        Tex2D = 2,
        Tex3D = 3,
        Cube = 4,
        Tex2DArray = 5,
        CubeArray = 6
    };

    public class SerializedTextureProperty
    {
        public string MDefaultName;
        public TextureDimension MTexDim;

        public SerializedTextureProperty(BinaryReader reader)
        {
            MDefaultName = reader.ReadAlignedString();
            MTexDim = (TextureDimension)reader.ReadInt32();
        }
    }

    public enum SerializedPropertyType
    {
        Color = 0,
        Vector = 1,
        Float = 2,
        Range = 3,
        Texture = 4,
        Int = 5
    };

    public class SerializedProperty
    {
        public string MName;
        public string MDescription;
        public string[] MAttributes;
        public SerializedPropertyType MType;
        public uint MFlags;
        public float[] MDefValue;
        public SerializedTextureProperty MDefTexture;

        public SerializedProperty(BinaryReader reader)
        {
            MName = reader.ReadAlignedString();
            MDescription = reader.ReadAlignedString();
            MAttributes = reader.ReadStringArray();
            MType = (SerializedPropertyType)reader.ReadInt32();
            MFlags = reader.ReadUInt32();
            MDefValue = reader.ReadSingleArray(4);
            MDefTexture = new SerializedTextureProperty(reader);
        }
    }

    public class SerializedProperties
    {
        public SerializedProperty[] MProps;

        public SerializedProperties(BinaryReader reader)
        {
            int numProps = reader.ReadInt32();
            MProps = new SerializedProperty[numProps];
            for (int i = 0; i < numProps; i++)
            {
                MProps[i] = new SerializedProperty(reader);
            }
        }
    }

    public class SerializedShaderFloatValue
    {
        public float Val;
        public string Name;

        public SerializedShaderFloatValue(BinaryReader reader)
        {
            Val = reader.ReadSingle();
            Name = reader.ReadAlignedString();
        }
    }

    public class SerializedShaderRTBlendState
    {
        public SerializedShaderFloatValue SrcBlend;
        public SerializedShaderFloatValue DestBlend;
        public SerializedShaderFloatValue SrcBlendAlpha;
        public SerializedShaderFloatValue DestBlendAlpha;
        public SerializedShaderFloatValue BlendOp;
        public SerializedShaderFloatValue BlendOpAlpha;
        public SerializedShaderFloatValue ColMask;

        public SerializedShaderRTBlendState(BinaryReader reader)
        {
            SrcBlend = new SerializedShaderFloatValue(reader);
            DestBlend = new SerializedShaderFloatValue(reader);
            SrcBlendAlpha = new SerializedShaderFloatValue(reader);
            DestBlendAlpha = new SerializedShaderFloatValue(reader);
            BlendOp = new SerializedShaderFloatValue(reader);
            BlendOpAlpha = new SerializedShaderFloatValue(reader);
            ColMask = new SerializedShaderFloatValue(reader);
        }
    }

    public class SerializedStencilOp
    {
        public SerializedShaderFloatValue Pass;
        public SerializedShaderFloatValue Fail;
        public SerializedShaderFloatValue ZFail;
        public SerializedShaderFloatValue Comp;

        public SerializedStencilOp(BinaryReader reader)
        {
            Pass = new SerializedShaderFloatValue(reader);
            Fail = new SerializedShaderFloatValue(reader);
            ZFail = new SerializedShaderFloatValue(reader);
            Comp = new SerializedShaderFloatValue(reader);
        }
    }

    public class SerializedShaderVectorValue
    {
        public SerializedShaderFloatValue X;
        public SerializedShaderFloatValue Y;
        public SerializedShaderFloatValue Z;
        public SerializedShaderFloatValue W;
        public string Name;

        public SerializedShaderVectorValue(BinaryReader reader)
        {
            X = new SerializedShaderFloatValue(reader);
            Y = new SerializedShaderFloatValue(reader);
            Z = new SerializedShaderFloatValue(reader);
            W = new SerializedShaderFloatValue(reader);
            Name = reader.ReadAlignedString();
        }
    }

    public enum FogMode
    {
        Unknown = -1,
        Disabled = 0,
        Linear = 1,
        Exp = 2,
        Exp2 = 3
    };

    public class SerializedShaderState
    {
        public string MName;
        public SerializedShaderRTBlendState[] RTBlend;
        public bool RTSeparateBlend;
        public SerializedShaderFloatValue ZClip;
        public SerializedShaderFloatValue ZTest;
        public SerializedShaderFloatValue ZWrite;
        public SerializedShaderFloatValue Culling;
        public SerializedShaderFloatValue Conservative;
        public SerializedShaderFloatValue OffsetFactor;
        public SerializedShaderFloatValue OffsetUnits;
        public SerializedShaderFloatValue AlphaToMask;
        public SerializedStencilOp StencilOp;
        public SerializedStencilOp StencilOpFront;
        public SerializedStencilOp StencilOpBack;
        public SerializedShaderFloatValue StencilReadMask;
        public SerializedShaderFloatValue StencilWriteMask;
        public SerializedShaderFloatValue StencilRef;
        public SerializedShaderFloatValue FogStart;
        public SerializedShaderFloatValue FogEnd;
        public SerializedShaderFloatValue FogDensity;
        public SerializedShaderVectorValue FogColor;
        public FogMode FogMode;
        public int GPUProgramID;
        public SerializedTagMap MTags;
        public int MLOD;
        public bool Lighting;

        public SerializedShaderState(ObjectReader reader)
        {
            var version = reader.Version;

            MName = reader.ReadAlignedString();
            RTBlend = new SerializedShaderRTBlendState[8];
            for (int i = 0; i < 8; i++)
            {
                RTBlend[i] = new SerializedShaderRTBlendState(reader);
            }
            RTSeparateBlend = reader.ReadBoolean();
            reader.AlignStream();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                ZClip = new SerializedShaderFloatValue(reader);
            }
            ZTest = new SerializedShaderFloatValue(reader);
            ZWrite = new SerializedShaderFloatValue(reader);
            Culling = new SerializedShaderFloatValue(reader);
            if (version[0] >= 2020) //2020.1 and up
            {
                Conservative = new SerializedShaderFloatValue(reader);
            }
            OffsetFactor = new SerializedShaderFloatValue(reader);
            OffsetUnits = new SerializedShaderFloatValue(reader);
            AlphaToMask = new SerializedShaderFloatValue(reader);
            StencilOp = new SerializedStencilOp(reader);
            StencilOpFront = new SerializedStencilOp(reader);
            StencilOpBack = new SerializedStencilOp(reader);
            StencilReadMask = new SerializedShaderFloatValue(reader);
            StencilWriteMask = new SerializedShaderFloatValue(reader);
            StencilRef = new SerializedShaderFloatValue(reader);
            FogStart = new SerializedShaderFloatValue(reader);
            FogEnd = new SerializedShaderFloatValue(reader);
            FogDensity = new SerializedShaderFloatValue(reader);
            FogColor = new SerializedShaderVectorValue(reader);
            FogMode = (FogMode)reader.ReadInt32();
            GPUProgramID = reader.ReadInt32();
            MTags = new SerializedTagMap(reader);
            MLOD = reader.ReadInt32();
            Lighting = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

    public class ShaderBindChannel
    {
        public sbyte Source;
        public sbyte Target;

        public ShaderBindChannel(BinaryReader reader)
        {
            Source = reader.ReadSByte();
            Target = reader.ReadSByte();
        }
    }

    public class ParserBindChannels
    {
        public ShaderBindChannel[] MChannels;
        public uint MSourceMap;

        public ParserBindChannels(BinaryReader reader)
        {
            int numChannels = reader.ReadInt32();
            MChannels = new ShaderBindChannel[numChannels];
            for (int i = 0; i < numChannels; i++)
            {
                MChannels[i] = new ShaderBindChannel(reader);
            }
            reader.AlignStream();

            MSourceMap = reader.ReadUInt32();
        }
    }

    public class VectorParameter
    {
        public int MNameIndex;
        public int MIndex;
        public int MArraySize;
        public sbyte MType;
        public sbyte MDim;

        public VectorParameter(BinaryReader reader)
        {
            MNameIndex = reader.ReadInt32();
            MIndex = reader.ReadInt32();
            MArraySize = reader.ReadInt32();
            MType = reader.ReadSByte();
            MDim = reader.ReadSByte();
            reader.AlignStream();
        }
    }

    public class MatrixParameter
    {
        public int MNameIndex;
        public int MIndex;
        public int MArraySize;
        public sbyte MType;
        public sbyte MRowCount;

        public MatrixParameter(BinaryReader reader)
        {
            MNameIndex = reader.ReadInt32();
            MIndex = reader.ReadInt32();
            MArraySize = reader.ReadInt32();
            MType = reader.ReadSByte();
            MRowCount = reader.ReadSByte();
            reader.AlignStream();
        }
    }

    public class TextureParameter
    {
        public int MNameIndex;
        public int MIndex;
        public int MSamplerIndex;
        public sbyte MDim;

        public TextureParameter(ObjectReader reader)
        {
            var version = reader.Version;

            MNameIndex = reader.ReadInt32();
            MIndex = reader.ReadInt32();
            MSamplerIndex = reader.ReadInt32();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                var mMultiSampled = reader.ReadBoolean();
            }
            MDim = reader.ReadSByte();
            reader.AlignStream();
        }
    }

    public class BufferBinding
    {
        public int MNameIndex;
        public int MIndex;
        public int MArraySize;

        public BufferBinding(ObjectReader reader)
        {
            var version = reader.Version;

            MNameIndex = reader.ReadInt32();
            MIndex = reader.ReadInt32();
            if (version[0] >= 2020) //2020.1 and up
            {
                MArraySize = reader.ReadInt32();
            }
        }
    }

    public class ConstantBuffer
    {
        public int MNameIndex;
        public MatrixParameter[] MMatrixParams;
        public VectorParameter[] MVectorParams;
        public StructParameter[] MStructParams;
        public int MSize;
        public bool MIsPartialCb;

        public ConstantBuffer(ObjectReader reader)
        {
            var version = reader.Version;

            MNameIndex = reader.ReadInt32();

            int numMatrixParams = reader.ReadInt32();
            MMatrixParams = new MatrixParameter[numMatrixParams];
            for (int i = 0; i < numMatrixParams; i++)
            {
                MMatrixParams[i] = new MatrixParameter(reader);
            }

            int numVectorParams = reader.ReadInt32();
            MVectorParams = new VectorParameter[numVectorParams];
            for (int i = 0; i < numVectorParams; i++)
            {
                MVectorParams[i] = new VectorParameter(reader);
            }
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                int numStructParams = reader.ReadInt32();
                MStructParams = new StructParameter[numStructParams];
                for (int i = 0; i < numStructParams; i++)
                {
                    MStructParams[i] = new StructParameter(reader);
                }
            }
            MSize = reader.ReadInt32();

            if ((version[0] == 2020 && version[1] > 3) ||
               (version[0] == 2020 && version[1] == 3 && version[2] >= 2) || //2020.3.2f1 and up
               (version[0] > 2021) ||
               (version[0] == 2021 && version[1] > 1) ||
               (version[0] == 2021 && version[1] == 1 && version[2] >= 4)) //2021.1.4f1 and up
            {
                MIsPartialCb = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }

    public class UavParameter
    {
        public int MNameIndex;
        public int MIndex;
        public int MOriginalIndex;

        public UavParameter(BinaryReader reader)
        {
            MNameIndex = reader.ReadInt32();
            MIndex = reader.ReadInt32();
            MOriginalIndex = reader.ReadInt32();
        }
    }

    public enum ShaderGpuProgramType
    {
        Unknown = 0,
        GLLegacy = 1,
        Gles31AEP = 2,
        Gles31 = 3,
        Gles3 = 4,
        Gles = 5,
        GLCore32 = 6,
        GLCore41 = 7,
        GLCore43 = 8,
        Dx9VertexSm20 = 9,
        Dx9VertexSm30 = 10,
        Dx9PixelSm20 = 11,
        Dx9PixelSm30 = 12,
        Dx10Level9Vertex = 13,
        Dx10Level9Pixel = 14,
        Dx11VertexSm40 = 15,
        Dx11VertexSm50 = 16,
        Dx11PixelSm40 = 17,
        Dx11PixelSm50 = 18,
        Dx11GeometrySm40 = 19,
        Dx11GeometrySm50 = 20,
        Dx11HullSm50 = 21,
        Dx11DomainSm50 = 22,
        MetalVS = 23,
        MetalFs = 24,
        Spirv = 25,
        ConsoleVS = 26,
        ConsoleFs = 27,
        ConsoleHs = 28,
        ConsoleDS = 29,
        ConsoleGs = 30,
        RayTracing = 31,
        PS5Nggc = 32
    };

    public class SerializedProgramParameters
    {
        public VectorParameter[] MVectorParams;
        public MatrixParameter[] MMatrixParams;
        public TextureParameter[] MTextureParams;
        public BufferBinding[] MBufferParams;
        public ConstantBuffer[] MConstantBuffers;
        public BufferBinding[] MConstantBufferBindings;
        public UavParameter[] MUavParams;
        public SamplerParameter[] MSamplers;

        public SerializedProgramParameters(ObjectReader reader)
        {
            int numVectorParams = reader.ReadInt32();
            MVectorParams = new VectorParameter[numVectorParams];
            for (int i = 0; i < numVectorParams; i++)
            {
                MVectorParams[i] = new VectorParameter(reader);
            }

            int numMatrixParams = reader.ReadInt32();
            MMatrixParams = new MatrixParameter[numMatrixParams];
            for (int i = 0; i < numMatrixParams; i++)
            {
                MMatrixParams[i] = new MatrixParameter(reader);
            }

            int numTextureParams = reader.ReadInt32();
            MTextureParams = new TextureParameter[numTextureParams];
            for (int i = 0; i < numTextureParams; i++)
            {
                MTextureParams[i] = new TextureParameter(reader);
            }

            int numBufferParams = reader.ReadInt32();
            MBufferParams = new BufferBinding[numBufferParams];
            for (int i = 0; i < numBufferParams; i++)
            {
                MBufferParams[i] = new BufferBinding(reader);
            }

            int numConstantBuffers = reader.ReadInt32();
            MConstantBuffers = new ConstantBuffer[numConstantBuffers];
            for (int i = 0; i < numConstantBuffers; i++)
            {
                MConstantBuffers[i] = new ConstantBuffer(reader);
            }

            int numConstantBufferBindings = reader.ReadInt32();
            MConstantBufferBindings = new BufferBinding[numConstantBufferBindings];
            for (int i = 0; i < numConstantBufferBindings; i++)
            {
                MConstantBufferBindings[i] = new BufferBinding(reader);
            }

            int numUavParams = reader.ReadInt32();
            MUavParams = new UavParameter[numUavParams];
            for (int i = 0; i < numUavParams; i++)
            {
                MUavParams[i] = new UavParameter(reader);
            }

            int numSamplers = reader.ReadInt32();
            MSamplers = new SamplerParameter[numSamplers];
            for (int i = 0; i < numSamplers; i++)
            {
                MSamplers[i] = new SamplerParameter(reader);
            }
        }
    }

    public class SerializedSubProgram
    {
        public uint MBlobIndex;
        public ParserBindChannels MChannels;
        public ushort[] MKeywordIndices;
        public sbyte MShaderHardwareTier;
        public ShaderGpuProgramType MGpuProgramType;
        public SerializedProgramParameters MParameters;
        public VectorParameter[] MVectorParams;
        public MatrixParameter[] MMatrixParams;
        public TextureParameter[] MTextureParams;
        public BufferBinding[] MBufferParams;
        public ConstantBuffer[] MConstantBuffers;
        public BufferBinding[] MConstantBufferBindings;
        public UavParameter[] MUavParams;
        public SamplerParameter[] MSamplers;

        public SerializedSubProgram(ObjectReader reader)
        {
            var version = reader.Version;

            MBlobIndex = reader.ReadUInt32();
            MChannels = new ParserBindChannels(reader);

            if ((version[0] >= 2019 && version[0] < 2021) || (version[0] == 2021 && version[1] < 2)) //2019 ~2021.1
            {
                var mGlobalKeywordIndices = reader.ReadUInt16Array();
                reader.AlignStream();
                var mLocalKeywordIndices = reader.ReadUInt16Array();
                reader.AlignStream();
            }
            else
            {
                MKeywordIndices = reader.ReadUInt16Array();
                if (version[0] >= 2017) //2017 and up
                {
                    reader.AlignStream();
                }
            }

            MShaderHardwareTier = reader.ReadSByte();
            MGpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();

            if ((version[0] == 2020 && version[1] > 3) ||
               (version[0] == 2020 && version[1] == 3 && version[2] >= 2) || //2020.3.2f1 and up
               (version[0] > 2021) ||
               (version[0] == 2021 && version[1] > 1) ||
               (version[0] == 2021 && version[1] == 1 && version[2] >= 1)) //2021.1.1f1 and up
            {
                MParameters = new SerializedProgramParameters(reader);
            }
            else
            {
                int numVectorParams = reader.ReadInt32();
                MVectorParams = new VectorParameter[numVectorParams];
                for (int i = 0; i < numVectorParams; i++)
                {
                    MVectorParams[i] = new VectorParameter(reader);
                }

                int numMatrixParams = reader.ReadInt32();
                MMatrixParams = new MatrixParameter[numMatrixParams];
                for (int i = 0; i < numMatrixParams; i++)
                {
                    MMatrixParams[i] = new MatrixParameter(reader);
                }

                int numTextureParams = reader.ReadInt32();
                MTextureParams = new TextureParameter[numTextureParams];
                for (int i = 0; i < numTextureParams; i++)
                {
                    MTextureParams[i] = new TextureParameter(reader);
                }

                int numBufferParams = reader.ReadInt32();
                MBufferParams = new BufferBinding[numBufferParams];
                for (int i = 0; i < numBufferParams; i++)
                {
                    MBufferParams[i] = new BufferBinding(reader);
                }

                int numConstantBuffers = reader.ReadInt32();
                MConstantBuffers = new ConstantBuffer[numConstantBuffers];
                for (int i = 0; i < numConstantBuffers; i++)
                {
                    MConstantBuffers[i] = new ConstantBuffer(reader);
                }

                int numConstantBufferBindings = reader.ReadInt32();
                MConstantBufferBindings = new BufferBinding[numConstantBufferBindings];
                for (int i = 0; i < numConstantBufferBindings; i++)
                {
                    MConstantBufferBindings[i] = new BufferBinding(reader);
                }

                int numUavParams = reader.ReadInt32();
                MUavParams = new UavParameter[numUavParams];
                for (int i = 0; i < numUavParams; i++)
                {
                    MUavParams[i] = new UavParameter(reader);
                }

                if (version[0] >= 2017) //2017 and up
                {
                    int numSamplers = reader.ReadInt32();
                    MSamplers = new SamplerParameter[numSamplers];
                    for (int i = 0; i < numSamplers; i++)
                    {
                        MSamplers[i] = new SamplerParameter(reader);
                    }
                }
            }

            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                if (version[0] >= 2021) //2021.1 and up
                {
                    var mShaderRequirements = reader.ReadInt64();
                }
                else
                {
                    var mShaderRequirements = reader.ReadInt32();
                }
            }
        }
    }

    public class SerializedProgram
    {
        public SerializedSubProgram[] MSubPrograms;
        public SerializedProgramParameters MCommonParameters;
        public ushort[] MSerializedKeywordStateMask;

        public SerializedProgram(ObjectReader reader)
        {
            var version = reader.Version;

            int numSubPrograms = reader.ReadInt32();
            MSubPrograms = new SerializedSubProgram[numSubPrograms];
            for (int i = 0; i < numSubPrograms; i++)
            {
                MSubPrograms[i] = new SerializedSubProgram(reader);
            }

            if ((version[0] == 2020 && version[1] > 3) ||
               (version[0] == 2020 && version[1] == 3 && version[2] >= 2) || //2020.3.2f1 and up
               (version[0] > 2021) ||
               (version[0] == 2021 && version[1] > 1) ||
               (version[0] == 2021 && version[1] == 1 && version[2] >= 1)) //2021.1.1f1 and up
            {
                MCommonParameters = new SerializedProgramParameters(reader);
            }

            if (version[0] > 2022 || (version[0] == 2022 && version[1] >= 1)) //2022.1 and up
            {
                MSerializedKeywordStateMask = reader.ReadUInt16Array();
                reader.AlignStream();
            }
        }
    }

    public enum PassType
    {
        Normal = 0,
        Use = 1,
        Grab = 2
    };

    public class SerializedPass
    {
        public Hash128[] MEditorDataHash;
        public byte[] MPlatforms;
        public ushort[] MLocalKeywordMask;
        public ushort[] MGlobalKeywordMask;
        public KeyValuePair<string, int>[] MNameIndices;
        public PassType MType;
        public SerializedShaderState MState;
        public uint MProgramMask;
        public SerializedProgram ProgVertex;
        public SerializedProgram ProgFragment;
        public SerializedProgram ProgGeometry;
        public SerializedProgram ProgHull;
        public SerializedProgram ProgDomain;
        public SerializedProgram ProgRayTracing;
        public bool MHasInstancingVariant;
        public string MUseName;
        public string MName;
        public string MTextureName;
        public SerializedTagMap MTags;
        public ushort[] MSerializedKeywordStateMask;

        public SerializedPass(ObjectReader reader)
        {
            var version = reader.Version;

            if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) //2020.2 and up
            {
                int numEditorDataHash = reader.ReadInt32();
                MEditorDataHash = new Hash128[numEditorDataHash];
                for (int i = 0; i < numEditorDataHash; i++)
                {
                    MEditorDataHash[i] = new Hash128(reader);
                }
                reader.AlignStream();
                MPlatforms = reader.ReadUInt8Array();
                reader.AlignStream();
                if (version[0] < 2021 || (version[0] == 2021 && version[1] < 2)) //2021.1 and down
                {
                    MLocalKeywordMask = reader.ReadUInt16Array();
                    reader.AlignStream();
                    MGlobalKeywordMask = reader.ReadUInt16Array();
                    reader.AlignStream();
                }
            }

            int numIndices = reader.ReadInt32();
            MNameIndices = new KeyValuePair<string, int>[numIndices];
            for (int i = 0; i < numIndices; i++)
            {
                MNameIndices[i] = new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32());
            }

            MType = (PassType)reader.ReadInt32();
            MState = new SerializedShaderState(reader);
            MProgramMask = reader.ReadUInt32();
            ProgVertex = new SerializedProgram(reader);
            ProgFragment = new SerializedProgram(reader);
            ProgGeometry = new SerializedProgram(reader);
            ProgHull = new SerializedProgram(reader);
            ProgDomain = new SerializedProgram(reader);
            if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) //2019.3 and up
            {
                ProgRayTracing = new SerializedProgram(reader);
            }
            MHasInstancingVariant = reader.ReadBoolean();
            if (version[0] >= 2018) //2018 and up
            {
                var mHasProceduralInstancingVariant = reader.ReadBoolean();
            }
            reader.AlignStream();
            MUseName = reader.ReadAlignedString();
            MName = reader.ReadAlignedString();
            MTextureName = reader.ReadAlignedString();
            MTags = new SerializedTagMap(reader);
            if (version[0] == 2021 && version[1] >= 2) //2021.2 ~2021.x
            {
                MSerializedKeywordStateMask = reader.ReadUInt16Array();
                reader.AlignStream();
            }
        }
    }

    public class SerializedTagMap
    {
        public KeyValuePair<string, string>[] Tags;

        public SerializedTagMap(BinaryReader reader)
        {
            int numTags = reader.ReadInt32();
            Tags = new KeyValuePair<string, string>[numTags];
            for (int i = 0; i < numTags; i++)
            {
                Tags[i] = new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString());
            }
        }
    }

    public class SerializedSubShader
    {
        public SerializedPass[] MPasses;
        public SerializedTagMap MTags;
        public int MLOD;

        public SerializedSubShader(ObjectReader reader)
        {
            int numPasses = reader.ReadInt32();
            MPasses = new SerializedPass[numPasses];
            for (int i = 0; i < numPasses; i++)
            {
                MPasses[i] = new SerializedPass(reader);
            }

            MTags = new SerializedTagMap(reader);
            MLOD = reader.ReadInt32();
        }
    }

    public class SerializedShaderDependency
    {
        public string From;
        public string To;

        public SerializedShaderDependency(BinaryReader reader)
        {
            From = reader.ReadAlignedString();
            To = reader.ReadAlignedString();
        }
    }

    public class SerializedCustomEditorForRenderPipeline
    {
        public string CustomEditorName;
        public string RenderPipelineType;

        public SerializedCustomEditorForRenderPipeline(BinaryReader reader)
        {
            CustomEditorName = reader.ReadAlignedString();
            RenderPipelineType = reader.ReadAlignedString();
        }
    }

    public class SerializedShader
    {
        public SerializedProperties MPropInfo;
        public SerializedSubShader[] MSubShaders;
        public string[] MKeywordNames;
        public byte[] MKeywordFlags;
        public string MName;
        public string MCustomEditorName;
        public string MFallbackName;
        public SerializedShaderDependency[] MDependencies;
        public SerializedCustomEditorForRenderPipeline[] MCustomEditorForRenderPipelines;
        public bool MDisableNoSubshadersMessage;

        public SerializedShader(ObjectReader reader)
        {
            var version = reader.Version;

            MPropInfo = new SerializedProperties(reader);

            int numSubShaders = reader.ReadInt32();
            MSubShaders = new SerializedSubShader[numSubShaders];
            for (int i = 0; i < numSubShaders; i++)
            {
                MSubShaders[i] = new SerializedSubShader(reader);
            }

            if (version[0] > 2021 || (version[0] == 2021 && version[1] >= 2)) //2021.2 and up
            {
                MKeywordNames = reader.ReadStringArray();
                MKeywordFlags = reader.ReadUInt8Array();
                reader.AlignStream();
            }

            MName = reader.ReadAlignedString();
            MCustomEditorName = reader.ReadAlignedString();
            MFallbackName = reader.ReadAlignedString();

            int numDependencies = reader.ReadInt32();
            MDependencies = new SerializedShaderDependency[numDependencies];
            for (int i = 0; i < numDependencies; i++)
            {
                MDependencies[i] = new SerializedShaderDependency(reader);
            }

            if (version[0] >= 2021) //2021.1 and up
            {
                int mCustomEditorForRenderPipelinesSize = reader.ReadInt32();
                MCustomEditorForRenderPipelines = new SerializedCustomEditorForRenderPipeline[mCustomEditorForRenderPipelinesSize];
                for (int i = 0; i < mCustomEditorForRenderPipelinesSize; i++)
                {
                    MCustomEditorForRenderPipelines[i] = new SerializedCustomEditorForRenderPipeline(reader);
                }
            }

            MDisableNoSubshadersMessage = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

    public enum ShaderCompilerPlatform
    {
        None = -1,
        GL = 0,
        D3D9 = 1,
        Xbox360 = 2,
        PS3 = 3,
        D3D11 = 4,
        Gles20 = 5,
        NaCl = 6,
        Flash = 7,
        D3D119X = 8,
        Gles3Plus = 9,
        Psp2 = 10,
        PS4 = 11,
        XboxOne = 12,
        Psm = 13,
        Metal = 14,
        OpenGLCore = 15,
        N3Ds = 16,
        WiiU = 17,
        Vulkan = 18,
        Switch = 19,
        XboxOneD3D12 = 20,
        GameCoreXboxOne = 21,
        GameCoreScarlett = 22,
        PS5 = 23,
        PS5Nggc = 24
    };

    public class Shader : NamedObject
    {
        public byte[] MScript;
        //5.3 - 5.4
        public uint DecompressedSize;
        public byte[] MSubProgramBlob;
        //5.5 and up
        public SerializedShader MParsedForm;
        public ShaderCompilerPlatform[] Platforms;
        public uint[][] Offsets;
        public uint[][] CompressedLengths;
        public uint[][] DecompressedLengths;
        public byte[] CompressedBlob;

        public Shader(ObjectReader reader) : base(reader)
        {
            if (Version[0] == 5 && Version[1] >= 5 || Version[0] > 5) //5.5 and up
            {
                MParsedForm = new SerializedShader(reader);
                Platforms = reader.ReadUInt32Array().Select(x => (ShaderCompilerPlatform)x).ToArray();
                if (Version[0] > 2019 || (Version[0] == 2019 && Version[1] >= 3)) //2019.3 and up
                {
                    Offsets = reader.ReadUInt32ArrayArray();
                    CompressedLengths = reader.ReadUInt32ArrayArray();
                    DecompressedLengths = reader.ReadUInt32ArrayArray();
                }
                else
                {
                    Offsets = reader.ReadUInt32Array().Select(x => new[] { x }).ToArray();
                    CompressedLengths = reader.ReadUInt32Array().Select(x => new[] { x }).ToArray();
                    DecompressedLengths = reader.ReadUInt32Array().Select(x => new[] { x }).ToArray();
                }
                CompressedBlob = reader.ReadUInt8Array();
                reader.AlignStream();

                var mDependenciesCount = reader.ReadInt32();
                for (int i = 0; i < mDependenciesCount; i++)
                {
                    new PPtr<Shader>(reader);
                }

                if (Version[0] >= 2018)
                {
                    var mNonModifiableTexturesCount = reader.ReadInt32();
                    for (int i = 0; i < mNonModifiableTexturesCount; i++)
                    {
                        var first = reader.ReadAlignedString();
                        new PPtr<Texture>(reader);
                    }
                }

                var mShaderIsBaked = reader.ReadBoolean();
                reader.AlignStream();
            }
            else
            {
                MScript = reader.ReadUInt8Array();
                reader.AlignStream();
                var mPathName = reader.ReadAlignedString();
                if (Version[0] == 5 && Version[1] >= 3) //5.3 - 5.4
                {
                    DecompressedSize = reader.ReadUInt32();
                    MSubProgramBlob = reader.ReadUInt8Array();
                }
            }
        }
    }
}
