using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class SkinnedMeshRenderer : Renderer
    {
        public PPtr<Mesh> MMesh;
        public PPtr<Transform>[] MBones;
        public float[] MBlendShapeWeights;

        public SkinnedMeshRenderer(ObjectReader reader) : base(reader)
        {
            int mQuality = reader.ReadInt32();
            bool mUpdateWhenOffscreen = reader.ReadBoolean();
            bool mSkinNormals = reader.ReadBoolean(); //3.1.0 and below
            reader.AlignStream();

            if (Version[0] == 2 && Version[1] < 6) //2.6 down
            {
                PPtr<Animation>? mDisableAnimationWhenOffscreen = new PPtr<Animation>(reader);
            }

            MMesh = new PPtr<Mesh>(reader);

            MBones = new PPtr<Transform>[reader.ReadInt32()];
            for (int b = 0; b < MBones.Length; b++)
            {
                MBones[b] = new PPtr<Transform>(reader);
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3)) //4.3 and up
            {
                MBlendShapeWeights = reader.ReadSingleArray();
            }
        }
    }
}
