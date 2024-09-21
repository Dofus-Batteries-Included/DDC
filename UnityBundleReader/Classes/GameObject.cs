using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class GameObject : EditorExtension
    {
        public PPtr<Component>[] MComponents;
        public string MName;

        public Transform MTransform;
        public MeshRenderer MMeshRenderer;
        public MeshFilter MMeshFilter;
        public SkinnedMeshRenderer MSkinnedMeshRenderer;
        public Animator MAnimator;
        public Animation MAnimation;

        public GameObject(ObjectReader reader) : base(reader)
        {
            int mComponentSize = reader.ReadInt32();
            MComponents = new PPtr<Component>[mComponentSize];
            for (int i = 0; i < mComponentSize; i++)
            {
                if ((Version[0] == 5 && Version[1] < 5) || Version[0] < 5) //5.5 down
                {
                    int first = reader.ReadInt32();
                }
                MComponents[i] = new PPtr<Component>(reader);
            }

            int mLayer = reader.ReadInt32();
            MName = reader.ReadAlignedString();
        }
    }
}
