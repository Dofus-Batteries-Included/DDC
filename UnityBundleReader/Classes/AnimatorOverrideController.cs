namespace UnityBundleReader.Classes
{
    public class AnimationClipOverride
    {
        public PPtr<AnimationClip> MOriginalClip;
        public PPtr<AnimationClip> MOverrideClip;

        public AnimationClipOverride(ObjectReader reader)
        {
            MOriginalClip = new PPtr<AnimationClip>(reader);
            MOverrideClip = new PPtr<AnimationClip>(reader);
        }
    }

    public sealed class AnimatorOverrideController : RuntimeAnimatorController
    {
        public PPtr<RuntimeAnimatorController> MController;
        public readonly AnimationClipOverride[] MClips;

        public AnimatorOverrideController(ObjectReader reader) : base(reader)
        {
            MController = new PPtr<RuntimeAnimatorController>(reader);

            int numOverrides = reader.ReadInt32();
            MClips = new AnimationClipOverride[numOverrides];
            for (int i = 0; i < numOverrides; i++)
            {
                MClips[i] = new AnimationClipOverride(reader);
            }
        }
    }
}
