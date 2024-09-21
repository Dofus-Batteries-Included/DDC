using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class Animator : Behaviour
    {
        public PPtr<Avatar> MAvatar;
        public PPtr<RuntimeAnimatorController> MController;
        public bool MHasTransformHierarchy = true;

        public Animator(ObjectReader reader) : base(reader)
        {
            MAvatar = new PPtr<Avatar>(reader);
            MController = new PPtr<RuntimeAnimatorController>(reader);
            var mCullingMode = reader.ReadInt32();

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 5)) //4.5 and up
            {
                var mUpdateMode = reader.ReadInt32();
            }

            var mApplyRootMotion = reader.ReadBoolean();
            if (Version[0] == 4 && Version[1] >= 5) //4.5 and up - 5.0 down
            {
                reader.AlignStream();
            }

            if (Version[0] >= 5) //5.0 and up
            {
                var mLinearVelocityBlending = reader.ReadBoolean();
                if (Version[0] > 2021 || (Version[0] == 2021 && Version[1] >= 2)) //2021.2 and up
                {
                    var mStabilizeFeet = reader.ReadBoolean();
                }
                reader.AlignStream();
            }

            if (Version[0] < 4 || (Version[0] == 4 && Version[1] < 5)) //4.5 down
            {
                var mAnimatePhysics = reader.ReadBoolean();
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3)) //4.3 and up
            {
                MHasTransformHierarchy = reader.ReadBoolean();
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 5)) //4.5 and up
            {
                var mAllowConstantClipSamplingOptimization = reader.ReadBoolean();
            }
            if (Version[0] >= 5 && Version[0] < 2018) //5.0 and up - 2018 down
            {
                reader.AlignStream();
            }

            if (Version[0] >= 2018) //2018 and up
            {
                var mKeepAnimatorControllerStateOnDisable = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }
}
