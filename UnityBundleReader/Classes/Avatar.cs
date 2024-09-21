using UnityBundleReader.Extensions;
using UnityBundleReader.Math;

namespace UnityBundleReader.Classes
{
    public class Node
    {
        public int MParentId;
        public int MAxesId;

        public Node(ObjectReader reader)
        {
            MParentId = reader.ReadInt32();
            MAxesId = reader.ReadInt32();
        }
    }

    public class Limit
    {
        public object MMin;
        public object MMax;

        public Limit(ObjectReader reader)
        {
            var version = reader.Version;
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
            {
                MMin = reader.ReadVector3();
                MMax = reader.ReadVector3();
            }
            else
            {
                MMin = reader.ReadVector4();
                MMax = reader.ReadVector4();
            }
        }
    }

    public class Axes
    {
        public Vector4 MPreQ;
        public Vector4 MPostQ;
        public object MSgn;
        public Limit MLimit;
        public float MLength;
        public uint MType;

        public Axes(ObjectReader reader)
        {
            var version = reader.Version;
            MPreQ = reader.ReadVector4();
            MPostQ = reader.ReadVector4();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4 and up
            {
                MSgn = reader.ReadVector3();
            }
            else
            {
                MSgn = reader.ReadVector4();
            }
            MLimit = new Limit(reader);
            MLength = reader.ReadSingle();
            MType = reader.ReadUInt32();
        }
    }

    public class Skeleton
    {
        public Node[] MNode;
        public uint[] MID;
        public Axes[] MAxesArray;


        public Skeleton(ObjectReader reader)
        {
            int numNodes = reader.ReadInt32();
            MNode = new Node[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                MNode[i] = new Node(reader);
            }

            MID = reader.ReadUInt32Array();

            int numAxes = reader.ReadInt32();
            MAxesArray = new Axes[numAxes];
            for (int i = 0; i < numAxes; i++)
            {
                MAxesArray[i] = new Axes(reader);
            }
        }
    }

    public class SkeletonPose
    {
        public Xform[] MX;

        public SkeletonPose(ObjectReader reader)
        {
            int numXforms = reader.ReadInt32();
            MX = new Xform[numXforms];
            for (int i = 0; i < numXforms; i++)
            {
                MX[i] = new Xform(reader);
            }
        }
    }

    public class Hand
    {
        public int[] MHandBoneIndex;

        public Hand(ObjectReader reader)
        {
            MHandBoneIndex = reader.ReadInt32Array();
        }
    }

    public class Handle
    {
        public Xform MX;
        public uint MParentHumanIndex;
        public uint MID;

        public Handle(ObjectReader reader)
        {
            MX = new Xform(reader);
            MParentHumanIndex = reader.ReadUInt32();
            MID = reader.ReadUInt32();
        }
    }

    public class Collider
    {
        public Xform MX;
        public uint MType;
        public uint MXMotionType;
        public uint MYMotionType;
        public uint MZMotionType;
        public float MMinLimitX;
        public float MMaxLimitX;
        public float MMaxLimitY;
        public float MMaxLimitZ;

        public Collider(ObjectReader reader)
        {
            MX = new Xform(reader);
            MType = reader.ReadUInt32();
            MXMotionType = reader.ReadUInt32();
            MYMotionType = reader.ReadUInt32();
            MZMotionType = reader.ReadUInt32();
            MMinLimitX = reader.ReadSingle();
            MMaxLimitX = reader.ReadSingle();
            MMaxLimitY = reader.ReadSingle();
            MMaxLimitZ = reader.ReadSingle();
        }
    }

    public class Human
    {
        public Xform MRootX;
        public Skeleton MSkeleton;
        public SkeletonPose MSkeletonPose;
        public Hand MLeftHand;
        public Hand MRightHand;
        public Handle[] MHandles;
        public Collider[] MColliderArray;
        public int[] MHumanBoneIndex;
        public float[] MHumanBoneMass;
        public int[] MColliderIndex;
        public float MScale;
        public float MArmTwist;
        public float MForeArmTwist;
        public float MUpperLegTwist;
        public float MLegTwist;
        public float MArmStretch;
        public float MLegStretch;
        public float MFeetSpacing;
        public bool MHasLeftHand;
        public bool MHasRightHand;
        public bool MHasTDoF;

        public Human(ObjectReader reader)
        {
            var version = reader.Version;
            MRootX = new Xform(reader);
            MSkeleton = new Skeleton(reader);
            MSkeletonPose = new SkeletonPose(reader);
            MLeftHand = new Hand(reader);
            MRightHand = new Hand(reader);

            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
            {
                int numHandles = reader.ReadInt32();
                MHandles = new Handle[numHandles];
                for (int i = 0; i < numHandles; i++)
                {
                    MHandles[i] = new Handle(reader);
                }

                int numColliders = reader.ReadInt32();
                MColliderArray = new Collider[numColliders];
                for (int i = 0; i < numColliders; i++)
                {
                    MColliderArray[i] = new Collider(reader);
                }
            }

            MHumanBoneIndex = reader.ReadInt32Array();

            MHumanBoneMass = reader.ReadSingleArray();

            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
            {
                MColliderIndex = reader.ReadInt32Array();
            }

            MScale = reader.ReadSingle();
            MArmTwist = reader.ReadSingle();
            MForeArmTwist = reader.ReadSingle();
            MUpperLegTwist = reader.ReadSingle();
            MLegTwist = reader.ReadSingle();
            MArmStretch = reader.ReadSingle();
            MLegStretch = reader.ReadSingle();
            MFeetSpacing = reader.ReadSingle();
            MHasLeftHand = reader.ReadBoolean();
            MHasRightHand = reader.ReadBoolean();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2)) //5.2 and up
            {
                MHasTDoF = reader.ReadBoolean();
            }
            reader.AlignStream();
        }
    }

    public class AvatarConstant
    {
        public Skeleton MAvatarSkeleton;
        public SkeletonPose MAvatarSkeletonPose;
        public SkeletonPose MDefaultPose;
        public uint[] MSkeletonNameIDArray;
        public Human MHuman;
        public int[] MHumanSkeletonIndexArray;
        public int[] MHumanSkeletonReverseIndexArray;
        public int MRootMotionBoneIndex;
        public Xform MRootMotionBoneX;
        public Skeleton MRootMotionSkeleton;
        public SkeletonPose MRootMotionSkeletonPose;
        public int[] MRootMotionSkeletonIndexArray;

        public AvatarConstant(ObjectReader reader)
        {
            var version = reader.Version;
            MAvatarSkeleton = new Skeleton(reader);
            MAvatarSkeletonPose = new SkeletonPose(reader);

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                MDefaultPose = new SkeletonPose(reader);

                MSkeletonNameIDArray = reader.ReadUInt32Array();
            }

            MHuman = new Human(reader);

            MHumanSkeletonIndexArray = reader.ReadInt32Array();

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                MHumanSkeletonReverseIndexArray = reader.ReadInt32Array();
            }

            MRootMotionBoneIndex = reader.ReadInt32();
            MRootMotionBoneX = new Xform(reader);

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                MRootMotionSkeleton = new Skeleton(reader);
                MRootMotionSkeletonPose = new SkeletonPose(reader);

                MRootMotionSkeletonIndexArray = reader.ReadInt32Array();
            }
        }
    }

    public sealed class Avatar : NamedObject
    {
        public uint MAvatarSize;
        public AvatarConstant MAvatar;
        public KeyValuePair<uint, string>[] MTos;

        public Avatar(ObjectReader reader) : base(reader)
        {
            MAvatarSize = reader.ReadUInt32();
            MAvatar = new AvatarConstant(reader);

            int numTos = reader.ReadInt32();
            MTos = new KeyValuePair<uint, string>[numTos];
            for (int i = 0; i < numTos; i++)
            {
                MTos[i] = new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            //HumanDescription m_HumanDescription 2019 and up
        }

        public string FindBonePath(uint hash)
        {
            return MTos.FirstOrDefault(pair => pair.Key == hash).Value;
        }
    }
}
