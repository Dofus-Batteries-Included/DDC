using System.Collections;
using UnityBundleReader.Extensions;
using UnityBundleReader.Math;
using Half = UnityBundleReader.Math.Half;

namespace UnityBundleReader.Classes
{
    public class MinMaxAABB
    {
        public Vector3 MMin;
        public Vector3 MMax;

        public MinMaxAABB(BinaryReader reader)
        {
            MMin = reader.ReadVector3();
            MMax = reader.ReadVector3();
        }
    }

    public class CompressedMesh
    {
        public readonly PackedFloatVector MVertices;
        public readonly PackedFloatVector MUV;
        public readonly PackedFloatVector MBindPoses;
        public readonly PackedFloatVector MNormals;
        public readonly PackedFloatVector MTangents;
        public readonly PackedIntVector MWeights;
        public readonly PackedIntVector MNormalSigns;
        public readonly PackedIntVector MTangentSigns;
        public readonly PackedFloatVector MFloatColors;
        public readonly PackedIntVector MBoneIndices;
        public readonly PackedIntVector MTriangles;
        public readonly PackedIntVector MColors;
        public readonly uint MUVInfo;

        public CompressedMesh(ObjectReader reader)
        {
            int[]? version = reader.Version;

            MVertices = new PackedFloatVector(reader);
            MUV = new PackedFloatVector(reader);
            if (version[0] < 5)
            {
                MBindPoses = new PackedFloatVector(reader);
            }
            MNormals = new PackedFloatVector(reader);
            MTangents = new PackedFloatVector(reader);
            MWeights = new PackedIntVector(reader);
            MNormalSigns = new PackedIntVector(reader);
            MTangentSigns = new PackedIntVector(reader);
            if (version[0] >= 5)
            {
                MFloatColors = new PackedFloatVector(reader);
            }
            MBoneIndices = new PackedIntVector(reader);
            MTriangles = new PackedIntVector(reader);
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 5)) //3.5 and up
            {
                if (version[0] < 5)
                {
                    MColors = new PackedIntVector(reader);
                }
                else
                {
                    MUVInfo = reader.ReadUInt32();
                }
            }
        }
    }

    public class StreamInfo
    {
        public uint ChannelMask;
        public uint Offset;
        public uint Stride;
        public uint Align;
        public byte DividerOp;
        public ushort Frequency;

        public StreamInfo() { }

        public StreamInfo(ObjectReader reader)
        {
            int[]? version = reader.Version;

            ChannelMask = reader.ReadUInt32();
            Offset = reader.ReadUInt32();

            if (version[0] < 4) //4.0 down
            {
                Stride = reader.ReadUInt32();
                Align = reader.ReadUInt32();
            }
            else
            {
                Stride = reader.ReadByte();
                DividerOp = reader.ReadByte();
                Frequency = reader.ReadUInt16();
            }
        }
    }

    public class ChannelInfo
    {
        public byte Stream;
        public byte Offset;
        public byte Format;
        public byte Dimension;

        public ChannelInfo() { }

        public ChannelInfo(ObjectReader reader)
        {
            Stream = reader.ReadByte();
            Offset = reader.ReadByte();
            Format = reader.ReadByte();
            Dimension = (byte)(reader.ReadByte() & 0xF);
        }
    }

    public class VertexData
    {
        public uint MCurrentChannels;
        public readonly uint MVertexCount;
        public ChannelInfo[] MChannels;
        public StreamInfo[] MStreams;
        public byte[] MDataSize;

        public VertexData(ObjectReader reader)
        {
            int[]? version = reader.Version;

            if (version[0] < 2018)//2018 down
            {
                MCurrentChannels = reader.ReadUInt32();
            }

            MVertexCount = reader.ReadUInt32();

            if (version[0] >= 4) //4.0 and up
            {
                int mChannelsSize = reader.ReadInt32();
                MChannels = new ChannelInfo[mChannelsSize];
                for (int i = 0; i < mChannelsSize; i++)
                {
                    MChannels[i] = new ChannelInfo(reader);
                }
            }

            if (version[0] < 5) //5.0 down
            {
                if (version[0] < 4)
                {
                    MStreams = new StreamInfo[4];
                }
                else
                {
                    MStreams = new StreamInfo[reader.ReadInt32()];
                }

                for (int i = 0; i < MStreams.Length; i++)
                {
                    MStreams[i] = new StreamInfo(reader);
                }

                if (version[0] < 4) //4.0 down
                {
                    GetChannels(version);
                }
            }
            else //5.0 and up
            {
                GetStreams(version);
            }

            MDataSize = reader.ReadUInt8Array();
            reader.AlignStream();
        }

        private void GetStreams(int[] version)
        {
            int streamCount = MChannels.Max(x => x.Stream) + 1;
            MStreams = new StreamInfo[streamCount];
            uint offset = 0;
            for (int s = 0; s < streamCount; s++)
            {
                uint chnMask = 0;
                uint stride = 0;
                for (int chn = 0; chn < MChannels.Length; chn++)
                {
                    ChannelInfo? mChannel = MChannels[chn];
                    if (mChannel.Stream == s)
                    {
                        if (mChannel.Dimension > 0)
                        {
                            chnMask |= 1u << chn;
                            stride += mChannel.Dimension * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(mChannel.Format, version));
                        }
                    }
                }
                MStreams[s] = new StreamInfo
                {
                    ChannelMask = chnMask,
                    Offset = offset,
                    Stride = stride,
                    DividerOp = 0,
                    Frequency = 0
                };
                offset += MVertexCount * stride;
                //static size_t AlignStreamSize (size_t size) { return (size + (kVertexStreamAlign-1)) & ~(kVertexStreamAlign-1); }
                offset = (offset + (16u - 1u)) & ~(16u - 1u);
            }
        }

        private void GetChannels(int[] version)
        {
            MChannels = new ChannelInfo[6];
            for (int i = 0; i < 6; i++)
            {
                MChannels[i] = new ChannelInfo();
            }
            for (int s = 0; s < MStreams.Length; s++)
            {
                StreamInfo? mStream = MStreams[s];
                BitArray? channelMask = new BitArray(new[] { (int)mStream.ChannelMask });
                byte offset = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (channelMask.Get(i))
                    {
                        ChannelInfo? mChannel = MChannels[i];
                        mChannel.Stream = (byte)s;
                        mChannel.Offset = offset;
                        switch (i)
                        {
                            case 0: //kShaderChannelVertex
                            case 1: //kShaderChannelNormal
                                mChannel.Format = 0; //kChannelFormatFloat
                                mChannel.Dimension = 3;
                                break;
                            case 2: //kShaderChannelColor
                                mChannel.Format = 2; //kChannelFormatColor
                                mChannel.Dimension = 4;
                                break;
                            case 3: //kShaderChannelTexCoord0
                            case 4: //kShaderChannelTexCoord1
                                mChannel.Format = 0; //kChannelFormatFloat
                                mChannel.Dimension = 2;
                                break;
                            case 5: //kShaderChannelTangent
                                mChannel.Format = 0; //kChannelFormatFloat
                                mChannel.Dimension = 4;
                                break;
                        }
                        offset += (byte)(mChannel.Dimension * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(mChannel.Format, version)));
                    }
                }
            }
        }
    }

    public class BoneWeights4
    {
        public readonly float[] Weight;
        public readonly int[] BoneIndex;

        public BoneWeights4()
        {
            Weight = new float[4];
            BoneIndex = new int[4];
        }

        public BoneWeights4(ObjectReader reader)
        {
            Weight = reader.ReadSingleArray(4);
            BoneIndex = reader.ReadInt32Array(4);
        }
    }

    public class BlendShapeVertex
    {
        public Vector3 Vertex;
        public Vector3 Normal;
        public Vector3 Tangent;
        public uint Index;

        public BlendShapeVertex(ObjectReader reader)
        {
            Vertex = reader.ReadVector3();
            Normal = reader.ReadVector3();
            Tangent = reader.ReadVector3();
            Index = reader.ReadUInt32();
        }
    }

    public class MeshBlendShape
    {
        public uint FirstVertex;
        public uint VertexCount;
        public bool HasNormals;
        public bool HasTangents;

        public MeshBlendShape(ObjectReader reader)
        {
            int[]? version = reader.Version;

            if (version[0] == 4 && version[1] < 3) //4.3 down
            {
                string? name = reader.ReadAlignedString();
            }
            FirstVertex = reader.ReadUInt32();
            VertexCount = reader.ReadUInt32();
            if (version[0] == 4 && version[1] < 3) //4.3 down
            {
                Vector3 aabbMinDelta = reader.ReadVector3();
                Vector3 aabbMaxDelta = reader.ReadVector3();
            }
            HasNormals = reader.ReadBoolean();
            HasTangents = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                reader.AlignStream();
            }
        }
    }

    public class MeshBlendShapeChannel
    {
        public string Name;
        public uint NameHash;
        public int FrameIndex;
        public int FrameCount;

        public MeshBlendShapeChannel(ObjectReader reader)
        {
            Name = reader.ReadAlignedString();
            NameHash = reader.ReadUInt32();
            FrameIndex = reader.ReadInt32();
            FrameCount = reader.ReadInt32();
        }
    }

    public class BlendShapeData
    {
        public readonly BlendShapeVertex[] Vertices;
        public readonly MeshBlendShape[] Shapes;
        public readonly MeshBlendShapeChannel[] Channels;
        public float[] FullWeights;

        public BlendShapeData(ObjectReader reader)
        {
            int[]? version = reader.Version;

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                int numVerts = reader.ReadInt32();
                Vertices = new BlendShapeVertex[numVerts];
                for (int i = 0; i < numVerts; i++)
                {
                    Vertices[i] = new BlendShapeVertex(reader);
                }

                int numShapes = reader.ReadInt32();
                Shapes = new MeshBlendShape[numShapes];
                for (int i = 0; i < numShapes; i++)
                {
                    Shapes[i] = new MeshBlendShape(reader);
                }

                int numChannels = reader.ReadInt32();
                Channels = new MeshBlendShapeChannel[numChannels];
                for (int i = 0; i < numChannels; i++)
                {
                    Channels[i] = new MeshBlendShapeChannel(reader);
                }

                FullWeights = reader.ReadSingleArray();
            }
            else
            {
                int mShapesSize = reader.ReadInt32();
                MeshBlendShape[]? mShapes = new MeshBlendShape[mShapesSize];
                for (int i = 0; i < mShapesSize; i++)
                {
                    mShapes[i] = new MeshBlendShape(reader);
                }
                reader.AlignStream();
                int mShapeVerticesSize = reader.ReadInt32();
                BlendShapeVertex[]? mShapeVertices = new BlendShapeVertex[mShapeVerticesSize]; //MeshBlendShapeVertex
                for (int i = 0; i < mShapeVerticesSize; i++)
                {
                    mShapeVertices[i] = new BlendShapeVertex(reader);
                }
            }
        }
    }

    public enum GfxPrimitiveType
    {
        Triangles = 0,
        TriangleStrip = 1,
        Quads = 2,
        Lines = 3,
        LineStrip = 4,
        Points = 5
    };

    public class SubMesh
    {
        public readonly uint FirstByte;
        public uint IndexCount;
        public readonly GfxPrimitiveType Topology;
        public uint TriangleCount;
        public uint BaseVertex;
        public uint FirstVertex;
        public uint VertexCount;
        public AABB LocalAABB;

        public SubMesh(ObjectReader reader)
        {
            int[]? version = reader.Version;

            FirstByte = reader.ReadUInt32();
            IndexCount = reader.ReadUInt32();
            Topology = (GfxPrimitiveType)reader.ReadInt32();

            if (version[0] < 4) //4.0 down
            {
                TriangleCount = reader.ReadUInt32();
            }

            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                BaseVertex = reader.ReadUInt32();
            }

            if (version[0] >= 3) //3.0 and up
            {
                FirstVertex = reader.ReadUInt32();
                VertexCount = reader.ReadUInt32();
                LocalAABB = new AABB(reader);
            }
        }
    }

    public sealed class Mesh : NamedObject
    {
        private bool _mUse16BitIndices = true;
        public readonly SubMesh[] MSubMeshes;
        private uint[] _mIndexBuffer;
        public BlendShapeData MShapes;
        public Matrix4X4[] MBindPose;
        public uint[] MBoneNameHashes;
        public int MVertexCount;
        public float[] MVertices;
        public BoneWeights4[] MSkin;
        public float[] MNormals;
        public float[] MColors;
        public float[] MUV0;
        public float[] MUV1;
        public float[] MUV2;
        public float[] MUV3;
        public float[] MUV4;
        public float[] MUV5;
        public float[] MUV6;
        public float[] MUV7;
        public float[] MTangents;
        private VertexData _mVertexData;
        private CompressedMesh _mCompressedMesh;
        private StreamingInfo _mStreamData;

        public readonly List<uint> MIndices = new List<uint>();

        public Mesh(ObjectReader reader) : base(reader)
        {
            if (Version[0] < 3 || (Version[0] == 3 && Version[1] < 5)) //3.5 down
            {
                _mUse16BitIndices = reader.ReadInt32() > 0;
            }

            if (Version[0] == 2 && Version[1] <= 5) //2.5 and down
            {
                int mIndexBufferSize = reader.ReadInt32();

                if (_mUse16BitIndices)
                {
                    _mIndexBuffer = new uint[mIndexBufferSize / 2];
                    for (int i = 0; i < mIndexBufferSize / 2; i++)
                    {
                        _mIndexBuffer[i] = reader.ReadUInt16();
                    }
                    reader.AlignStream();
                }
                else
                {
                    _mIndexBuffer = reader.ReadUInt32Array(mIndexBufferSize / 4);
                }
            }

            int mSubMeshesSize = reader.ReadInt32();
            MSubMeshes = new SubMesh[mSubMeshesSize];
            for (int i = 0; i < mSubMeshesSize; i++)
            {
                MSubMeshes[i] = new SubMesh(reader);
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 1)) //4.1 and up
            {
                MShapes = new BlendShapeData(reader);
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3)) //4.3 and up
            {
                MBindPose = reader.ReadMatrixArray();
                MBoneNameHashes = reader.ReadUInt32Array();
                uint mRootBoneNameHash = reader.ReadUInt32();
            }

            if (Version[0] > 2 || (Version[0] == 2 && Version[1] >= 6)) //2.6.0 and up
            {
                if (Version[0] >= 2019) //2019 and up
                {
                    int mBonesAABBSize = reader.ReadInt32();
                    MinMaxAABB[]? mBonesAABB = new MinMaxAABB[mBonesAABBSize];
                    for (int i = 0; i < mBonesAABBSize; i++)
                    {
                        mBonesAABB[i] = new MinMaxAABB(reader);
                    }

                    uint[]? mVariableBoneCountWeights = reader.ReadUInt32Array();
                }

                byte mMeshCompression = reader.ReadByte();
                if (Version[0] >= 4)
                {
                    if (Version[0] < 5)
                    {
                        byte mStreamCompression = reader.ReadByte();
                    }
                    bool mIsReadable = reader.ReadBoolean();
                    bool mKeepVertices = reader.ReadBoolean();
                    bool mKeepIndices = reader.ReadBoolean();
                }
                reader.AlignStream();

                //Unity fixed it in 2017.3.1p1 and later versions
                if ((Version[0] > 2017 || (Version[0] == 2017 && Version[1] >= 4)) || //2017.4
                    ((Version[0] == 2017 && Version[1] == 3 && Version[2] == 1) && BuildType.IsPatch) || //fixed after 2017.3.1px
                    ((Version[0] == 2017 && Version[1] == 3) && mMeshCompression == 0))//2017.3.xfx with no compression
                {
                    int mIndexFormat = reader.ReadInt32();
                    _mUse16BitIndices = mIndexFormat == 0;
                }

                int mIndexBufferSize = reader.ReadInt32();
                if (_mUse16BitIndices)
                {
                    _mIndexBuffer = new uint[mIndexBufferSize / 2];
                    for (int i = 0; i < mIndexBufferSize / 2; i++)
                    {
                        _mIndexBuffer[i] = reader.ReadUInt16();
                    }
                    reader.AlignStream();
                }
                else
                {
                    _mIndexBuffer = reader.ReadUInt32Array(mIndexBufferSize / 4);
                }
            }

            if (Version[0] < 3 || (Version[0] == 3 && Version[1] < 5)) //3.4.2 and earlier
            {
                MVertexCount = reader.ReadInt32();
                MVertices = reader.ReadSingleArray(MVertexCount * 3); //Vector3

                MSkin = new BoneWeights4[reader.ReadInt32()];
                for (int s = 0; s < MSkin.Length; s++)
                {
                    MSkin[s] = new BoneWeights4(reader);
                }

                MBindPose = reader.ReadMatrixArray();

                MUV0 = reader.ReadSingleArray(reader.ReadInt32() * 2); //Vector2

                MUV1 = reader.ReadSingleArray(reader.ReadInt32() * 2); //Vector2

                if (Version[0] == 2 && Version[1] <= 5) //2.5 and down
                {
                    int mTangentSpaceSize = reader.ReadInt32();
                    MNormals = new float[mTangentSpaceSize * 3];
                    MTangents = new float[mTangentSpaceSize * 4];
                    for (int v = 0; v < mTangentSpaceSize; v++)
                    {
                        MNormals[v * 3] = reader.ReadSingle();
                        MNormals[v * 3 + 1] = reader.ReadSingle();
                        MNormals[v * 3 + 2] = reader.ReadSingle();
                        MTangents[v * 3] = reader.ReadSingle();
                        MTangents[v * 3 + 1] = reader.ReadSingle();
                        MTangents[v * 3 + 2] = reader.ReadSingle();
                        MTangents[v * 3 + 3] = reader.ReadSingle(); //handedness
                    }
                }
                else //2.6.0 and later
                {
                    MTangents = reader.ReadSingleArray(reader.ReadInt32() * 4); //Vector4

                    MNormals = reader.ReadSingleArray(reader.ReadInt32() * 3); //Vector3
                }
            }
            else
            {
                if (Version[0] < 2018 || (Version[0] == 2018 && Version[1] < 2)) //2018.2 down
                {
                    MSkin = new BoneWeights4[reader.ReadInt32()];
                    for (int s = 0; s < MSkin.Length; s++)
                    {
                        MSkin[s] = new BoneWeights4(reader);
                    }
                }

                if (Version[0] == 3 || (Version[0] == 4 && Version[1] <= 2)) //4.2 and down
                {
                    MBindPose = reader.ReadMatrixArray();
                }

                _mVertexData = new VertexData(reader);
            }

            if (Version[0] > 2 || (Version[0] == 2 && Version[1] >= 6)) //2.6.0 and later
            {
                _mCompressedMesh = new CompressedMesh(reader);
            }

            reader.Position += 24; //AABB m_LocalAABB

            if (Version[0] < 3 || (Version[0] == 3 && Version[1] <= 4)) //3.4.2 and earlier
            {
                int mColorsSize = reader.ReadInt32();
                MColors = new float[mColorsSize * 4];
                for (int v = 0; v < mColorsSize * 4; v++)
                {
                    MColors[v] = (float)reader.ReadByte() / 0xFF;
                }

                int mCollisionTrianglesSize = reader.ReadInt32();
                reader.Position += mCollisionTrianglesSize * 4; //UInt32 indices
                int mCollisionVertexCount = reader.ReadInt32();
            }

            int mMeshUsageFlags = reader.ReadInt32();

            if (Version[0] > 2022 || (Version[0] == 2022 && Version[1] >= 1)) //2022.1 and up
            {
                int mCookingOptions = reader.ReadInt32();
            }

            if (Version[0] >= 5) //5.0 and up
            {
                byte[]? mBakedConvexCollisionMesh = reader.ReadUInt8Array();
                reader.AlignStream();
                byte[]? mBakedTriangleCollisionMesh = reader.ReadUInt8Array();
                reader.AlignStream();
            }

            if (Version[0] > 2018 || (Version[0] == 2018 && Version[1] >= 2)) //2018.2 and up
            {
                float[]? mMeshMetrics = new float[2];
                mMeshMetrics[0] = reader.ReadSingle();
                mMeshMetrics[1] = reader.ReadSingle();
            }

            if (Version[0] > 2018 || (Version[0] == 2018 && Version[1] >= 3)) //2018.3 and up
            {
                reader.AlignStream();
                _mStreamData = new StreamingInfo(reader);
            }

            ProcessData();
        }

        private void ProcessData()
        {
            if (!string.IsNullOrEmpty(_mStreamData?.Path))
            {
                if (_mVertexData.MVertexCount > 0)
                {
                    ResourceReader? resourceReader = new ResourceReader(_mStreamData.Path, AssetsFile, _mStreamData.Offset, _mStreamData.Size);
                    _mVertexData.MDataSize = resourceReader.GetData();
                }
            }
            if (Version[0] > 3 || (Version[0] == 3 && Version[1] >= 5)) //3.5 and up
            {
                ReadVertexData();
            }

            if (Version[0] > 2 || (Version[0] == 2 && Version[1] >= 6)) //2.6.0 and later
            {
                DecompressCompressedMesh();
            }

            GetTriangles();
        }

        private void ReadVertexData()
        {
            MVertexCount = (int)_mVertexData.MVertexCount;

            for (int chn = 0; chn < _mVertexData.MChannels.Length; chn++)
            {
                ChannelInfo? mChannel = _mVertexData.MChannels[chn];
                if (mChannel.Dimension > 0)
                {
                    StreamInfo? mStream = _mVertexData.MStreams[mChannel.Stream];
                    BitArray? channelMask = new BitArray(new[] { (int)mStream.ChannelMask });
                    if (channelMask.Get(chn))
                    {
                        if (Version[0] < 2018 && chn == 2 && mChannel.Format == 2) //kShaderChannelColor && kChannelFormatColor
                        {
                            mChannel.Dimension = 4;
                        }

                        MeshHelper.VertexFormat vertexFormat = MeshHelper.ToVertexFormat(mChannel.Format, Version);
                        int componentByteSize = (int)MeshHelper.GetFormatSize(vertexFormat);
                        byte[]? componentBytes = new byte[MVertexCount * mChannel.Dimension * componentByteSize];
                        for (int v = 0; v < MVertexCount; v++)
                        {
                            int vertexOffset = (int)mStream.Offset + mChannel.Offset + (int)mStream.Stride * v;
                            for (int d = 0; d < mChannel.Dimension; d++)
                            {
                                int componentOffset = vertexOffset + componentByteSize * d;
                                Buffer.BlockCopy(_mVertexData.MDataSize, componentOffset, componentBytes, componentByteSize * (v * mChannel.Dimension + d), componentByteSize);
                            }
                        }

                        if (Reader.Endian == EndianType.BigEndian && componentByteSize > 1) //swap bytes
                        {
                            for (int i = 0; i < componentBytes.Length / componentByteSize; i++)
                            {
                                byte[]? buff = new byte[componentByteSize];
                                Buffer.BlockCopy(componentBytes, i * componentByteSize, buff, 0, componentByteSize);
                                buff = buff.Reverse().ToArray();
                                Buffer.BlockCopy(buff, 0, componentBytes, i * componentByteSize, componentByteSize);
                            }
                        }

                        int[] componentsIntArray = null;
                        float[] componentsFloatArray = null;
                        if (MeshHelper.IsIntFormat(vertexFormat))
                            componentsIntArray = MeshHelper.BytesToIntArray(componentBytes, vertexFormat);
                        else
                            componentsFloatArray = MeshHelper.BytesToFloatArray(componentBytes, vertexFormat);

                        if (Version[0] >= 2018)
                        {
                            switch (chn)
                            {
                                case 0: //kShaderChannelVertex
                                    MVertices = componentsFloatArray;
                                    break;
                                case 1: //kShaderChannelNormal
                                    MNormals = componentsFloatArray;
                                    break;
                                case 2: //kShaderChannelTangent
                                    MTangents = componentsFloatArray;
                                    break;
                                case 3: //kShaderChannelColor
                                    MColors = componentsFloatArray;
                                    break;
                                case 4: //kShaderChannelTexCoord0
                                    MUV0 = componentsFloatArray;
                                    break;
                                case 5: //kShaderChannelTexCoord1
                                    MUV1 = componentsFloatArray;
                                    break;
                                case 6: //kShaderChannelTexCoord2
                                    MUV2 = componentsFloatArray;
                                    break;
                                case 7: //kShaderChannelTexCoord3
                                    MUV3 = componentsFloatArray;
                                    break;
                                case 8: //kShaderChannelTexCoord4
                                    MUV4 = componentsFloatArray;
                                    break;
                                case 9: //kShaderChannelTexCoord5
                                    MUV5 = componentsFloatArray;
                                    break;
                                case 10: //kShaderChannelTexCoord6
                                    MUV6 = componentsFloatArray;
                                    break;
                                case 11: //kShaderChannelTexCoord7
                                    MUV7 = componentsFloatArray;
                                    break;
                                //2018.2 and up
                                case 12: //kShaderChannelBlendWeight
                                    if (MSkin == null)
                                    {
                                        InitMSkin();
                                    }
                                    for (int i = 0; i < MVertexCount; i++)
                                    {
                                        for (int j = 0; j < mChannel.Dimension; j++)
                                        {
                                            MSkin[i].Weight[j] = componentsFloatArray[i * mChannel.Dimension + j];
                                        }
                                    }
                                    break;
                                case 13: //kShaderChannelBlendIndices
                                    if (MSkin == null)
                                    {
                                        InitMSkin();
                                    }
                                    for (int i = 0; i < MVertexCount; i++)
                                    {
                                        for (int j = 0; j < mChannel.Dimension; j++)
                                        {
                                            MSkin[i].BoneIndex[j] = componentsIntArray[i * mChannel.Dimension + j];
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (chn)
                            {
                                case 0: //kShaderChannelVertex
                                    MVertices = componentsFloatArray;
                                    break;
                                case 1: //kShaderChannelNormal
                                    MNormals = componentsFloatArray;
                                    break;
                                case 2: //kShaderChannelColor
                                    MColors = componentsFloatArray;
                                    break;
                                case 3: //kShaderChannelTexCoord0
                                    MUV0 = componentsFloatArray;
                                    break;
                                case 4: //kShaderChannelTexCoord1
                                    MUV1 = componentsFloatArray;
                                    break;
                                case 5:
                                    if (Version[0] >= 5) //kShaderChannelTexCoord2
                                    {
                                        MUV2 = componentsFloatArray;
                                    }
                                    else //kShaderChannelTangent
                                    {
                                        MTangents = componentsFloatArray;
                                    }
                                    break;
                                case 6: //kShaderChannelTexCoord3
                                    MUV3 = componentsFloatArray;
                                    break;
                                case 7: //kShaderChannelTangent
                                    MTangents = componentsFloatArray;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void DecompressCompressedMesh()
        {
            //Vertex
            if (_mCompressedMesh.MVertices.MNumItems > 0)
            {
                MVertexCount = (int)_mCompressedMesh.MVertices.MNumItems / 3;
                MVertices = _mCompressedMesh.MVertices.UnpackFloats(3, 3 * 4);
            }
            //UV
            if (_mCompressedMesh.MUV.MNumItems > 0)
            {
                uint mUVInfo = _mCompressedMesh.MUVInfo;
                if (mUVInfo != 0)
                {
                    const int kInfoBitsPerUV = 4;
                    const int kUVDimensionMask = 3;
                    const int kUVChannelExists = 4;
                    const int kMaxTexCoordShaderChannels = 8;

                    int uvSrcOffset = 0;
                    for (int uv = 0; uv < kMaxTexCoordShaderChannels; uv++)
                    {
                        uint texCoordBits = mUVInfo >> (uv * kInfoBitsPerUV);
                        texCoordBits &= (1u << kInfoBitsPerUV) - 1u;
                        if ((texCoordBits & kUVChannelExists) != 0)
                        {
                            int uvDim = 1 + (int)(texCoordBits & kUVDimensionMask);
                            float[]? mUV = _mCompressedMesh.MUV.UnpackFloats(uvDim, uvDim * 4, uvSrcOffset, MVertexCount);
                            SetUV(uv, mUV);
                            uvSrcOffset += uvDim * MVertexCount;
                        }
                    }
                }
                else
                {
                    MUV0 = _mCompressedMesh.MUV.UnpackFloats(2, 2 * 4, 0, MVertexCount);
                    if (_mCompressedMesh.MUV.MNumItems >= MVertexCount * 4)
                    {
                        MUV1 = _mCompressedMesh.MUV.UnpackFloats(2, 2 * 4, MVertexCount * 2, MVertexCount);
                    }
                }
            }
            //BindPose
            if (Version[0] < 5)
            {
                if (_mCompressedMesh.MBindPoses.MNumItems > 0)
                {
                    MBindPose = new Matrix4X4[_mCompressedMesh.MBindPoses.MNumItems / 16];
                    float[]? mBindPosesUnpacked = _mCompressedMesh.MBindPoses.UnpackFloats(16, 4 * 16);
                    float[]? buffer = new float[16];
                    for (int i = 0; i < MBindPose.Length; i++)
                    {
                        Array.Copy(mBindPosesUnpacked, i * 16, buffer, 0, 16);
                        MBindPose[i] = new Matrix4X4(buffer);
                    }
                }
            }
            //Normal
            if (_mCompressedMesh.MNormals.MNumItems > 0)
            {
                float[]? normalData = _mCompressedMesh.MNormals.UnpackFloats(2, 4 * 2);
                int[]? signs = _mCompressedMesh.MNormalSigns.UnpackInts();
                MNormals = new float[_mCompressedMesh.MNormals.MNumItems / 2 * 3];
                for (int i = 0; i < _mCompressedMesh.MNormals.MNumItems / 2; ++i)
                {
                    float x = normalData[i * 2 + 0];
                    float y = normalData[i * 2 + 1];
                    float zsqr = 1 - x * x - y * y;
                    float z;
                    if (zsqr >= 0f)
                        z = (float)System.Math.Sqrt(zsqr);
                    else
                    {
                        z = 0;
                        Vector3 normal = new Vector3(x, y, z);
                        normal.Normalize();
                        x = normal.X;
                        y = normal.Y;
                        z = normal.Z;
                    }
                    if (signs[i] == 0)
                        z = -z;
                    MNormals[i * 3] = x;
                    MNormals[i * 3 + 1] = y;
                    MNormals[i * 3 + 2] = z;
                }
            }
            //Tangent
            if (_mCompressedMesh.MTangents.MNumItems > 0)
            {
                float[]? tangentData = _mCompressedMesh.MTangents.UnpackFloats(2, 4 * 2);
                int[]? signs = _mCompressedMesh.MTangentSigns.UnpackInts();
                MTangents = new float[_mCompressedMesh.MTangents.MNumItems / 2 * 4];
                for (int i = 0; i < _mCompressedMesh.MTangents.MNumItems / 2; ++i)
                {
                    float x = tangentData[i * 2 + 0];
                    float y = tangentData[i * 2 + 1];
                    float zsqr = 1 - x * x - y * y;
                    float z;
                    if (zsqr >= 0f)
                        z = (float)System.Math.Sqrt(zsqr);
                    else
                    {
                        z = 0;
                        Vector3 vector3F = new Vector3(x, y, z);
                        vector3F.Normalize();
                        x = vector3F.X;
                        y = vector3F.Y;
                        z = vector3F.Z;
                    }
                    if (signs[i * 2 + 0] == 0)
                        z = -z;
                    float w = signs[i * 2 + 1] > 0 ? 1.0f : -1.0f;
                    MTangents[i * 4] = x;
                    MTangents[i * 4 + 1] = y;
                    MTangents[i * 4 + 2] = z;
                    MTangents[i * 4 + 3] = w;
                }
            }
            //FloatColor
            if (Version[0] >= 5)
            {
                if (_mCompressedMesh.MFloatColors.MNumItems > 0)
                {
                    MColors = _mCompressedMesh.MFloatColors.UnpackFloats(1, 4);
                }
            }
            //Skin
            if (_mCompressedMesh.MWeights.MNumItems > 0)
            {
                int[]? weights = _mCompressedMesh.MWeights.UnpackInts();
                int[]? boneIndices = _mCompressedMesh.MBoneIndices.UnpackInts();

                InitMSkin();

                int bonePos = 0;
                int boneIndexPos = 0;
                int j = 0;
                int sum = 0;

                for (int i = 0; i < _mCompressedMesh.MWeights.MNumItems; i++)
                {
                    //read bone index and weight.
                    MSkin[bonePos].Weight[j] = weights[i] / 31.0f;
                    MSkin[bonePos].BoneIndex[j] = boneIndices[boneIndexPos++];
                    j++;
                    sum += weights[i];

                    //the weights add up to one. fill the rest for this vertex with zero, and continue with next one.
                    if (sum >= 31)
                    {
                        for (; j < 4; j++)
                        {
                            MSkin[bonePos].Weight[j] = 0;
                            MSkin[bonePos].BoneIndex[j] = 0;
                        }
                        bonePos++;
                        j = 0;
                        sum = 0;
                    }
                    //we read three weights, but they don't add up to one. calculate the fourth one, and read
                    //missing bone index. continue with next vertex.
                    else if (j == 3)
                    {
                        MSkin[bonePos].Weight[j] = (31 - sum) / 31.0f;
                        MSkin[bonePos].BoneIndex[j] = boneIndices[boneIndexPos++];
                        bonePos++;
                        j = 0;
                        sum = 0;
                    }
                }
            }
            //IndexBuffer
            if (_mCompressedMesh.MTriangles.MNumItems > 0)
            {
                _mIndexBuffer = Array.ConvertAll(_mCompressedMesh.MTriangles.UnpackInts(), x => (uint)x);
            }
            //Color
            if (_mCompressedMesh.MColors?.MNumItems > 0)
            {
                _mCompressedMesh.MColors.MNumItems *= 4;
                _mCompressedMesh.MColors.MBitSize /= 4;
                int[]? tempColors = _mCompressedMesh.MColors.UnpackInts();
                MColors = new float[_mCompressedMesh.MColors.MNumItems];
                for (int v = 0; v < _mCompressedMesh.MColors.MNumItems; v++)
                {
                    MColors[v] = tempColors[v] / 255f;
                }
            }
        }

        private void GetTriangles()
        {
            foreach (SubMesh? mSubMesh in MSubMeshes)
            {
                uint firstIndex = mSubMesh.FirstByte / 2;
                if (!_mUse16BitIndices)
                {
                    firstIndex /= 2;
                }
                uint indexCount = mSubMesh.IndexCount;
                GfxPrimitiveType topology = mSubMesh.Topology;
                if (topology == GfxPrimitiveType.Triangles)
                {
                    for (int i = 0; i < indexCount; i += 3)
                    {
                        MIndices.Add(_mIndexBuffer[firstIndex + i]);
                        MIndices.Add(_mIndexBuffer[firstIndex + i + 1]);
                        MIndices.Add(_mIndexBuffer[firstIndex + i + 2]);
                    }
                }
                else if (Version[0] < 4 || topology == GfxPrimitiveType.TriangleStrip)
                {
                    // de-stripify :
                    uint triIndex = 0;
                    for (int i = 0; i < indexCount - 2; i++)
                    {
                        uint a = _mIndexBuffer[firstIndex + i];
                        uint b = _mIndexBuffer[firstIndex + i + 1];
                        uint c = _mIndexBuffer[firstIndex + i + 2];

                        // skip degenerates
                        if (a == b || a == c || b == c)
                            continue;

                        // do the winding flip-flop of strips :
                        if ((i & 1) == 1)
                        {
                            MIndices.Add(b);
                            MIndices.Add(a);
                        }
                        else
                        {
                            MIndices.Add(a);
                            MIndices.Add(b);
                        }
                        MIndices.Add(c);
                        triIndex += 3;
                    }
                    //fix indexCount
                    mSubMesh.IndexCount = triIndex;
                }
                else if (topology == GfxPrimitiveType.Quads)
                {
                    for (int q = 0; q < indexCount; q += 4)
                    {
                        MIndices.Add(_mIndexBuffer[firstIndex + q]);
                        MIndices.Add(_mIndexBuffer[firstIndex + q + 1]);
                        MIndices.Add(_mIndexBuffer[firstIndex + q + 2]);
                        MIndices.Add(_mIndexBuffer[firstIndex + q]);
                        MIndices.Add(_mIndexBuffer[firstIndex + q + 2]);
                        MIndices.Add(_mIndexBuffer[firstIndex + q + 3]);
                    }
                    //fix indexCount
                    mSubMesh.IndexCount = indexCount / 2 * 3;
                }
                else
                {
                    throw new NotSupportedException("Failed getting triangles. Submesh topology is lines or points.");
                }
            }
        }

        private void InitMSkin()
        {
            MSkin = new BoneWeights4[MVertexCount];
            for (int i = 0; i < MVertexCount; i++)
            {
                MSkin[i] = new BoneWeights4();
            }
        }

        private void SetUV(int uv, float[] mUV)
        {
            switch (uv)
            {
                case 0:
                    MUV0 = mUV;
                    break;
                case 1:
                    MUV1 = mUV;
                    break;
                case 2:
                    MUV2 = mUV;
                    break;
                case 3:
                    MUV3 = mUV;
                    break;
                case 4:
                    MUV4 = mUV;
                    break;
                case 5:
                    MUV5 = mUV;
                    break;
                case 6:
                    MUV6 = mUV;
                    break;
                case 7:
                    MUV7 = mUV;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float[] GetUV(int uv)
        {
            switch (uv)
            {
                case 0:
                    return MUV0;
                case 1:
                    return MUV1;
                case 2:
                    return MUV2;
                case 3:
                    return MUV3;
                case 4:
                    return MUV4;
                case 5:
                    return MUV5;
                case 6:
                    return MUV6;
                case 7:
                    return MUV7;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public static class MeshHelper
    {
        public enum VertexChannelFormat
        {
            Float,
            Float16,
            Color,
            Byte,
            UInt32
        }

        public enum VertexFormat2017
        {
            Float,
            Float16,
            Color,
            UNorm8,
            SNorm8,
            UNorm16,
            SNorm16,
            UInt8,
            SInt8,
            UInt16,
            SInt16,
            UInt32,
            SInt32
        }

        public enum VertexFormat
        {
            Float,
            Float16,
            UNorm8,
            SNorm8,
            UNorm16,
            SNorm16,
            UInt8,
            SInt8,
            UInt16,
            SInt16,
            UInt32,
            SInt32
        }

        public static VertexFormat ToVertexFormat(int format, int[] version)
        {
            if (version[0] < 2017)
            {
                switch ((VertexChannelFormat)format)
                {
                    case VertexChannelFormat.Float:
                        return VertexFormat.Float;
                    case VertexChannelFormat.Float16:
                        return VertexFormat.Float16;
                    case VertexChannelFormat.Color: //in 4.x is size 4
                        return VertexFormat.UNorm8;
                    case VertexChannelFormat.Byte:
                        return VertexFormat.UInt8;
                    case VertexChannelFormat.UInt32: //in 5.x
                        return VertexFormat.UInt32;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }
            else if (version[0] < 2019)
            {
                switch ((VertexFormat2017)format)
                {
                    case VertexFormat2017.Float:
                        return VertexFormat.Float;
                    case VertexFormat2017.Float16:
                        return VertexFormat.Float16;
                    case VertexFormat2017.Color:
                    case VertexFormat2017.UNorm8:
                        return VertexFormat.UNorm8;
                    case VertexFormat2017.SNorm8:
                        return VertexFormat.SNorm8;
                    case VertexFormat2017.UNorm16:
                        return VertexFormat.UNorm16;
                    case VertexFormat2017.SNorm16:
                        return VertexFormat.SNorm16;
                    case VertexFormat2017.UInt8:
                        return VertexFormat.UInt8;
                    case VertexFormat2017.SInt8:
                        return VertexFormat.SInt8;
                    case VertexFormat2017.UInt16:
                        return VertexFormat.UInt16;
                    case VertexFormat2017.SInt16:
                        return VertexFormat.SInt16;
                    case VertexFormat2017.UInt32:
                        return VertexFormat.UInt32;
                    case VertexFormat2017.SInt32:
                        return VertexFormat.SInt32;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }
            else
            {
                return (VertexFormat)format;
            }
        }


        public static uint GetFormatSize(VertexFormat format)
        {
            switch (format)
            {
                case VertexFormat.Float:
                case VertexFormat.UInt32:
                case VertexFormat.SInt32:
                    return 4u;
                case VertexFormat.Float16:
                case VertexFormat.UNorm16:
                case VertexFormat.SNorm16:
                case VertexFormat.UInt16:
                case VertexFormat.SInt16:
                    return 2u;
                case VertexFormat.UNorm8:
                case VertexFormat.SNorm8:
                case VertexFormat.UInt8:
                case VertexFormat.SInt8:
                    return 1u;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static bool IsIntFormat(VertexFormat format)
        {
            return format >= VertexFormat.UInt8;
        }

        public static float[] BytesToFloatArray(byte[] inputBytes, VertexFormat format)
        {
            uint size = GetFormatSize(format);
            long len = inputBytes.Length / size;
            float[]? result = new float[len];
            for (int i = 0; i < len; i++)
            {
                switch (format)
                {
                    case VertexFormat.Float:
                        result[i] = BitConverter.ToSingle(inputBytes, i * 4);
                        break;
                    case VertexFormat.Float16:
                        result[i] = Half.ToHalf(inputBytes, i * 2);
                        break;
                    case VertexFormat.UNorm8:
                        result[i] = inputBytes[i] / 255f;
                        break;
                    case VertexFormat.SNorm8:
                        result[i] = System.Math.Max((sbyte)inputBytes[i] / 127f, -1f);
                        break;
                    case VertexFormat.UNorm16:
                        result[i] = BitConverter.ToUInt16(inputBytes, i * 2) / 65535f;
                        break;
                    case VertexFormat.SNorm16:
                        result[i] = System.Math.Max(BitConverter.ToInt16(inputBytes, i * 2) / 32767f, -1f);
                        break;
                }
            }
            return result;
        }

        public static int[] BytesToIntArray(byte[] inputBytes, VertexFormat format)
        {
            uint size = GetFormatSize(format);
            long len = inputBytes.Length / size;
            int[]? result = new int[len];
            for (int i = 0; i < len; i++)
            {
                switch (format)
                {
                    case VertexFormat.UInt8:
                    case VertexFormat.SInt8:
                        result[i] = inputBytes[i];
                        break;
                    case VertexFormat.UInt16:
                    case VertexFormat.SInt16:
                        result[i] = BitConverter.ToInt16(inputBytes, i * 2);
                        break;
                    case VertexFormat.UInt32:
                    case VertexFormat.SInt32:
                        result[i] = BitConverter.ToInt32(inputBytes, i * 4);
                        break;
                }
            }
            return result;
        }
    }
}
