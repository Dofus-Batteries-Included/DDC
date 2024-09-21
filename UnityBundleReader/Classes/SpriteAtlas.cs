using System;
using System.Collections.Generic;

namespace AssetStudio
{
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
        public SecondarySpriteTexture[] SecondaryTextures;

        public SpriteAtlasData(ObjectReader reader)
        {
            var version = reader.Version;
            Texture = new PPtr<Texture2D>(reader);
            AlphaTexture = new PPtr<Texture2D>(reader);
            TextureRect = new Rectf(reader);
            TextureRectOffset = reader.ReadVector2();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                AtlasRectOffset = reader.ReadVector2();
            }
            UVTransform = reader.ReadVector4();
            DownscaleMultiplier = reader.ReadSingle();
            SettingsRaw = new SpriteSettings(reader);
            if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) //2020.2 and up
            {
                var secondaryTexturesSize = reader.ReadInt32();
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
        public PPtr<Sprite>[] MPackedSprites;
        public Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData> MRenderDataMap;
        public bool MIsVariant;

        public SpriteAtlas(ObjectReader reader) : base(reader)
        {
            var mPackedSpritesSize = reader.ReadInt32();
            MPackedSprites = new PPtr<Sprite>[mPackedSpritesSize];
            for (int i = 0; i < mPackedSpritesSize; i++)
            {
                MPackedSprites[i] = new PPtr<Sprite>(reader);
            }

            var mPackedSpriteNamesToIndex = reader.ReadStringArray();

            var mRenderDataMapSize = reader.ReadInt32();
            MRenderDataMap = new Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData>(mRenderDataMapSize);
            for (int i = 0; i < mRenderDataMapSize; i++)
            {
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                var value = new SpriteAtlasData(reader);
                MRenderDataMap.Add(new KeyValuePair<Guid, long>(first, second), value);
            }
            var mTag = reader.ReadAlignedString();
            MIsVariant = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
