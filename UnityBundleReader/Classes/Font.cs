using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class Font : NamedObject
    {
        public byte[] MFontData;

        public Font(ObjectReader reader) : base(reader)
        {
            if ((Version[0] == 5 && Version[1] >= 5) || Version[0] > 5)//5.5 and up
            {
                float mLineSpacing = reader.ReadSingle();
                PPtr<Material>? mDefaultMaterial = new PPtr<Material>(reader);
                float mFontSize = reader.ReadSingle();
                PPtr<Texture>? mTexture = new PPtr<Texture>(reader);
                int mAsciiStartOffset = reader.ReadInt32();
                float mTracking = reader.ReadSingle();
                int mCharacterSpacing = reader.ReadInt32();
                int mCharacterPadding = reader.ReadInt32();
                int mConvertCase = reader.ReadInt32();
                int mCharacterRectsSize = reader.ReadInt32();
                for (int i = 0; i < mCharacterRectsSize; i++)
                {
                    reader.Position += 44;//CharacterInfo data 41
                }
                int mKerningValuesSize = reader.ReadInt32();
                for (int i = 0; i < mKerningValuesSize; i++)
                {
                    reader.Position += 8;
                }
                float mPixelScale = reader.ReadSingle();
                int mFontDataSize = reader.ReadInt32();
                if (mFontDataSize > 0)
                {
                    MFontData = reader.ReadBytes(mFontDataSize);
                }
            }
            else
            {
                int mAsciiStartOffset = reader.ReadInt32();

                if (Version[0] <= 3)
                {
                    int mFontCountX = reader.ReadInt32();
                    int mFontCountY = reader.ReadInt32();
                }

                float mKerning = reader.ReadSingle();
                float mLineSpacing = reader.ReadSingle();

                if (Version[0] <= 3)
                {
                    int mPerCharacterKerningSize = reader.ReadInt32();
                    for (int i = 0; i < mPerCharacterKerningSize; i++)
                    {
                        int first = reader.ReadInt32();
                        float second = reader.ReadSingle();
                    }
                }
                else
                {
                    int mCharacterSpacing = reader.ReadInt32();
                    int mCharacterPadding = reader.ReadInt32();
                }

                int mConvertCase = reader.ReadInt32();
                PPtr<Material>? mDefaultMaterial = new PPtr<Material>(reader);

                int mCharacterRectsSize = reader.ReadInt32();
                for (int i = 0; i < mCharacterRectsSize; i++)
                {
                    int index = reader.ReadInt32();
                    //Rectf uv
                    float uvx = reader.ReadSingle();
                    float uvy = reader.ReadSingle();
                    float uvwidth = reader.ReadSingle();
                    float uvheight = reader.ReadSingle();
                    //Rectf vert
                    float vertx = reader.ReadSingle();
                    float verty = reader.ReadSingle();
                    float vertwidth = reader.ReadSingle();
                    float vertheight = reader.ReadSingle();
                    float width = reader.ReadSingle();

                    if (Version[0] >= 4)
                    {
                        bool flipped = reader.ReadBoolean();
                        reader.AlignStream();
                    }
                }

                PPtr<Texture>? mTexture = new PPtr<Texture>(reader);

                int mKerningValuesSize = reader.ReadInt32();
                for (int i = 0; i < mKerningValuesSize; i++)
                {
                    int pairfirst = reader.ReadInt16();
                    int pairsecond = reader.ReadInt16();
                    float second = reader.ReadSingle();
                }

                if (Version[0] <= 3)
                {
                    bool mGridFont = reader.ReadBoolean();
                    reader.AlignStream();
                }
                else { float mPixelScale = reader.ReadSingle(); }

                int mFontDataSize = reader.ReadInt32();
                if (mFontDataSize > 0)
                {
                    MFontData = reader.ReadBytes(mFontDataSize);
                }
            }
        }
    }
}
