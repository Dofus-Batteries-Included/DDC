using UnityBundleReader.Extensions;
using UnityBundleReader.Math;

namespace UnityBundleReader.Classes
{
    public class SecondarySpriteTexture
    {
        public PPtr<Texture2D> Texture;
        public string Name;

        public SecondarySpriteTexture(ObjectReader reader)
        {
            Texture = new PPtr<Texture2D>(reader);
            Name = reader.ReadStringToNull();
        }
    }

    public enum SpritePackingRotation
    {
        None = 0,
        FlipHorizontal = 1,
        FlipVertical = 2,
        Rotate180 = 3,
        Rotate90 = 4
    };

    public enum SpritePackingMode
    {
        Tight = 0,
        Rectangle
    };

    public enum SpriteMeshType
    {
        FullRect,
        Tight
    };

    public class SpriteSettings
    {
        public readonly uint SettingsRaw;

        public uint Packed;
        public SpritePackingMode PackingMode;
        public SpritePackingRotation PackingRotation;
        public SpriteMeshType MeshType;

        public SpriteSettings(BinaryReader reader)
        {
            SettingsRaw = reader.ReadUInt32();

            Packed = SettingsRaw & 1; //1
            PackingMode = (SpritePackingMode)((SettingsRaw >> 1) & 1); //1
            PackingRotation = (SpritePackingRotation)((SettingsRaw >> 2) & 0xf); //4
            MeshType = (SpriteMeshType)((SettingsRaw >> 6) & 1); //1
            //reserved
        }
    }

    public class SpriteVertex
    {
        public Vector3 Pos;
        public Vector2 UV;

        public SpriteVertex(ObjectReader reader)
        {
            int[]? version = reader.Version;

            Pos = reader.ReadVector3();
            if (version[0] < 4 || (version[0] == 4 && version[1] <= 3)) //4.3 and down
            {
                UV = reader.ReadVector2();
            }
        }
    }

    public class SpriteRenderData
    {
        public PPtr<Texture2D> Texture;
        public PPtr<Texture2D> AlphaTexture;
        public readonly SecondarySpriteTexture[] SecondaryTextures;
        public readonly SubMesh[] MSubMeshes;
        public byte[] MIndexBuffer;
        public VertexData MVertexData;
        public readonly SpriteVertex[] Vertices;
        public ushort[] Indices;
        public Matrix4X4[] MBindpose;
        public BoneWeights4[] MSourceSkin;
        public Rectf TextureRect;
        public Vector2 TextureRectOffset;
        public Vector2 AtlasRectOffset;
        public SpriteSettings SettingsRaw;
        public Vector4 UVTransform;
        public float DownscaleMultiplier;

        public SpriteRenderData(ObjectReader reader)
        {
            int[]? version = reader.Version;

            Texture = new PPtr<Texture2D>(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2)) //5.2 and up
            {
                AlphaTexture = new PPtr<Texture2D>(reader);
            }

            if (version[0] >= 2019) //2019 and up
            {
                int secondaryTexturesSize = reader.ReadInt32();
                SecondaryTextures = new SecondarySpriteTexture[secondaryTexturesSize];
                for (int i = 0; i < secondaryTexturesSize; i++)
                {
                    SecondaryTextures[i] = new SecondarySpriteTexture(reader);
                }
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                int mSubMeshesSize = reader.ReadInt32();
                MSubMeshes = new SubMesh[mSubMeshesSize];
                for (int i = 0; i < mSubMeshesSize; i++)
                {
                    MSubMeshes[i] = new SubMesh(reader);
                }

                MIndexBuffer = reader.ReadUInt8Array();
                reader.AlignStream();

                MVertexData = new VertexData(reader);
            }
            else
            {
                int verticesSize = reader.ReadInt32();
                Vertices = new SpriteVertex[verticesSize];
                for (int i = 0; i < verticesSize; i++)
                {
                    Vertices[i] = new SpriteVertex(reader);
                }

                Indices = reader.ReadUInt16Array();
                reader.AlignStream();
            }

            if (version[0] >= 2018) //2018 and up
            {
                MBindpose = reader.ReadMatrixArray();

                if (version[0] == 2018 && version[1] < 2) //2018.2 down
                {
                    int mSourceSkinSize = reader.ReadInt32();
                    for (int i = 0; i < mSourceSkinSize; i++)
                    {
                        MSourceSkin[i] = new BoneWeights4(reader);
                    }
                }
            }

            TextureRect = new Rectf(reader);
            TextureRectOffset = reader.ReadVector2();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                AtlasRectOffset = reader.ReadVector2();
            }

            SettingsRaw = new SpriteSettings(reader);
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                UVTransform = reader.ReadVector4();
            }

            if (version[0] >= 2017) //2017 and up
            {
                DownscaleMultiplier = reader.ReadSingle();
            }
        }
    }

    public class Rectf
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Rectf(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Width = reader.ReadSingle();
            Height = reader.ReadSingle();
        }
    }

    public sealed class Sprite : NamedObject
    {
        public Rectf MRect;
        public Vector2 MOffset;
        public Vector4 MBorder;
        public float MPixelsToUnits;
        public Vector2 MPivot = new Vector2(0.5f, 0.5f);
        public uint MExtrude;
        public bool MIsPolygon;
        public KeyValuePair<Guid, long> MRenderDataKey;
        public string[] MAtlasTags;
        public readonly PPtr<SpriteAtlas> MSpriteAtlas;
        public SpriteRenderData MRd;
        public readonly Vector2[][] MPhysicsShape;

        public Sprite(ObjectReader reader) : base(reader)
        {
            MRect = new Rectf(reader);
            MOffset = reader.ReadVector2();
            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 5)) //4.5 and up
            {
                MBorder = reader.ReadVector4();
            }

            MPixelsToUnits = reader.ReadSingle();
            if (Version[0] > 5
                || (Version[0] == 5 && Version[1] > 4)
                || (Version[0] == 5 && Version[1] == 4 && Version[2] >= 2)
                || (Version[0] == 5 && Version[1] == 4 && Version[2] == 1 && BuildType.IsPatch && Version[3] >= 3)) //5.4.1p3 and up
            {
                MPivot = reader.ReadVector2();
            }

            MExtrude = reader.ReadUInt32();
            if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 3)) //5.3 and up
            {
                MIsPolygon = reader.ReadBoolean();
                reader.AlignStream();
            }

            if (Version[0] >= 2017) //2017 and up
            {
                Guid first = new Guid(reader.ReadBytes(16));
                long second = reader.ReadInt64();
                MRenderDataKey = new KeyValuePair<Guid, long>(first, second);

                MAtlasTags = reader.ReadStringArray();

                MSpriteAtlas = new PPtr<SpriteAtlas>(reader);
            }

            MRd = new SpriteRenderData(reader);

            if (Version[0] >= 2017) //2017 and up
            {
                int mPhysicsShapeSize = reader.ReadInt32();
                MPhysicsShape = new Vector2[mPhysicsShapeSize][];
                for (int i = 0; i < mPhysicsShapeSize; i++)
                {
                    MPhysicsShape[i] = reader.ReadVector2Array();
                }
            }

            //vector m_Bones 2018 and up
        }
    }
}
