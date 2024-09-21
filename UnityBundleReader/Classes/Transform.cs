using UnityBundleReader.Extensions;
using UnityBundleReader.Math;

namespace UnityBundleReader.Classes
{
    public class Transform : Component
    {
        public Quaternion MLocalRotation;
        public Vector3 MLocalPosition;
        public Vector3 MLocalScale;
        public PPtr<Transform>[] MChildren;
        public PPtr<Transform> MFather;

        public Transform(ObjectReader reader) : base(reader)
        {
            MLocalRotation = reader.ReadQuaternion();
            MLocalPosition = reader.ReadVector3();
            MLocalScale = reader.ReadVector3();

            int mChildrenCount = reader.ReadInt32();
            MChildren = new PPtr<Transform>[mChildrenCount];
            for (int i = 0; i < mChildrenCount; i++)
            {
                MChildren[i] = new PPtr<Transform>(reader);
            }
            MFather = new PPtr<Transform>(reader);
        }
    }
}
