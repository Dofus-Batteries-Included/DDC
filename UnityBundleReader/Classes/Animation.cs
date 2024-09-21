namespace UnityBundleReader.Classes
{
    public sealed class Animation : Behaviour
    {
        public PPtr<AnimationClip>[] MAnimations;

        public Animation(ObjectReader reader) : base(reader)
        {
            PPtr<AnimationClip>? mAnimation = new PPtr<AnimationClip>(reader);
            int numAnimations = reader.ReadInt32();
            MAnimations = new PPtr<AnimationClip>[numAnimations];
            for (int i = 0; i < numAnimations; i++)
            {
                MAnimations[i] = new PPtr<AnimationClip>(reader);
            }
        }
    }
}
