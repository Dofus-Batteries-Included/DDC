using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes;

public sealed class Animator : Behaviour
{
    public PPtr<Avatar> MAvatar;
    public PPtr<RuntimeAnimatorController> MController;
    public bool MHasTransformHierarchy = true;

    public Animator(ObjectReader reader) : base(reader)
    {
        MAvatar = new PPtr<Avatar>(reader);
        MController = new PPtr<RuntimeAnimatorController>(reader);
        int mCullingMode = reader.ReadInt32();

        if (Version[0] > 4 || Version[0] == 4 && Version[1] >= 5) //4.5 and up
        {
            int mUpdateMode = reader.ReadInt32();
        }

        bool mApplyRootMotion = reader.ReadBoolean();
        if (Version[0] == 4 && Version[1] >= 5) //4.5 and up - 5.0 down
        {
            reader.AlignStream();
        }

        if (Version[0] >= 5) //5.0 and up
        {
            bool mLinearVelocityBlending = reader.ReadBoolean();
            if (Version[0] > 2021 || Version[0] == 2021 && Version[1] >= 2) //2021.2 and up
            {
                bool mStabilizeFeet = reader.ReadBoolean();
            }
            reader.AlignStream();
        }

        if (Version[0] < 4 || Version[0] == 4 && Version[1] < 5) //4.5 down
        {
            bool mAnimatePhysics = reader.ReadBoolean();
        }

        if (Version[0] > 4 || Version[0] == 4 && Version[1] >= 3) //4.3 and up
        {
            MHasTransformHierarchy = reader.ReadBoolean();
        }

        if (Version[0] > 4 || Version[0] == 4 && Version[1] >= 5) //4.5 and up
        {
            bool mAllowConstantClipSamplingOptimization = reader.ReadBoolean();
        }
        if (Version[0] >= 5 && Version[0] < 2018) //5.0 and up - 2018 down
        {
            reader.AlignStream();
        }

        if (Version[0] >= 2018) //2018 and up
        {
            bool mKeepAnimatorControllerStateOnDisable = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
