using UnityBundleReader.Extensions;
using UnityBundleReader.Math;

namespace UnityBundleReader.Classes;

public class SpriteAtlasData
{
    public PPtr<Texture2D> Texture;
    public PPtr<Texture2D> AlphaTexture;
    public Rectf TextureRect;
    public Vector2 TextureRectOffset;
    public Vector2 AtlasRectOffset;
    public Vector4 UVTransform;
    public float DownscaleMultiplier;
    public SpriteSettings SettingsRaw;
    public readonly SecondarySpriteTexture[] SecondaryTextures;

    public SpriteAtlasData(ObjectReader reader)
    {
        int[] version = reader.Version;
        Texture = new PPtr<Texture2D>(reader);
        AlphaTexture = new PPtr<Texture2D>(reader);
        TextureRect = new Rectf(reader);
        TextureRectOffset = reader.ReadVector2();
        if (version[0] > 2017 || version[0] == 2017 && version[1] >= 2) //2017.2 and up
        {
            AtlasRectOffset = reader.ReadVector2();
        }
        UVTransform = reader.ReadVector4();
        DownscaleMultiplier = reader.ReadSingle();
        SettingsRaw = new SpriteSettings(reader);
        if (version[0] > 2020 || version[0] == 2020 && version[1] >= 2) //2020.2 and up
        {
            int secondaryTexturesSize = reader.ReadInt32();
            SecondaryTextures = new SecondarySpriteTexture[secondaryTexturesSize];
            for (int i = 0; i < secondaryTexturesSize; i++)
            {
                SecondaryTextures[i] = new SecondarySpriteTexture(reader);
            }
            reader.AlignStream();
        }
    }
}

public sealed class SpriteAtlas : NamedObject
{
    public readonly PPtr<Sprite>[] MPackedSprites;
    public readonly Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData> MRenderDataMap;
    public readonly bool MIsVariant;

    public SpriteAtlas(ObjectReader reader) : base(reader)
    {
        int mPackedSpritesSize = reader.ReadInt32();
        MPackedSprites = new PPtr<Sprite>[mPackedSpritesSize];
        for (int i = 0; i < mPackedSpritesSize; i++)
        {
            MPackedSprites[i] = new PPtr<Sprite>(reader);
        }

        string[] mPackedSpriteNamesToIndex = reader.ReadStringArray();

        int mRenderDataMapSize = reader.ReadInt32();
        MRenderDataMap = new Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData>(mRenderDataMapSize);
        for (int i = 0; i < mRenderDataMapSize; i++)
        {
            Guid first = new(reader.ReadBytes(16));
            long second = reader.ReadInt64();
            SpriteAtlasData value = new(reader);
            MRenderDataMap.Add(new KeyValuePair<Guid, long>(first, second), value);
        }
        string mTag = reader.ReadAlignedString();
        MIsVariant = reader.ReadBoolean();
        reader.AlignStream();
    }
}
