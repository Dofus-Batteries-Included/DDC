using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class MovieTexture : Texture
    {
        public byte[] MMovieData;
        public PPtr<AudioClip> MAudioClip;

        public MovieTexture(ObjectReader reader) : base(reader)
        {
            var mLoop = reader.ReadBoolean();
            reader.AlignStream();
            MAudioClip = new PPtr<AudioClip>(reader);
            MMovieData = reader.ReadUInt8Array();
        }
    }
}
