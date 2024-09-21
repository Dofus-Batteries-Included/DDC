using UnityBundleReader.Extensions;
using UnityBundleReader.Math;

namespace UnityBundleReader.Classes
{
    public class Keyframe<T>
    {
        public float Time;
        public T Value;
        public T InSlope;
        public T OutSlope;
        public int WeightedMode;
        public T InWeight;
        public T OutWeight;


        public Keyframe(ObjectReader reader, Func<T> readerFunc)
        {
            Time = reader.ReadSingle();
            Value = readerFunc();
            InSlope = readerFunc();
            OutSlope = readerFunc();
            if (reader.Version[0] >= 2018) //2018 and up
            {
                WeightedMode = reader.ReadInt32();
                InWeight = readerFunc();
                OutWeight = readerFunc();
            }
        }
    }

    public class AnimationCurve<T>
    {
        public readonly Keyframe<T>[] MCurve;
        public int MPreInfinity;
        public int MPostInfinity;
        public int MRotationOrder;

        public AnimationCurve(ObjectReader reader, Func<T> readerFunc)
        {
            int[]? version = reader.Version;
            int numCurves = reader.ReadInt32();
            MCurve = new Keyframe<T>[numCurves];
            for (int i = 0; i < numCurves; i++)
            {
                MCurve[i] = new Keyframe<T>(reader, readerFunc);
            }

            MPreInfinity = reader.ReadInt32();
            MPostInfinity = reader.ReadInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3))//5.3 and up
            {
                MRotationOrder = reader.ReadInt32();
            }
        }
    }

    public class QuaternionCurve
    {
        public AnimationCurve<Quaternion> Curve;
        public string Path;

        public QuaternionCurve(ObjectReader reader)
        {
            Curve = new AnimationCurve<Quaternion>(reader, reader.ReadQuaternion);
            Path = reader.ReadAlignedString();
        }
    }

    public class PackedFloatVector
    {
        public readonly uint MNumItems;
        public readonly float MRange;
        public readonly float MStart;
        public readonly byte[] MData;
        public readonly byte MBitSize;

        public PackedFloatVector(ObjectReader reader)
        {
            MNumItems = reader.ReadUInt32();
            MRange = reader.ReadSingle();
            MStart = reader.ReadSingle();

            int numData = reader.ReadInt32();
            MData = reader.ReadBytes(numData);
            reader.AlignStream();

            MBitSize = reader.ReadByte();
            reader.AlignStream();
        }

        public float[] UnpackFloats(int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
        {
            int bitPos = MBitSize * start;
            int indexPos = bitPos / 8;
            bitPos %= 8;

            float scale = 1.0f / MRange;
            if (numChunks == -1)
                numChunks = (int)MNumItems / itemCountInChunk;
            int end = chunkStride * numChunks / 4;
            List<float>? data = new List<float>();
            for (int index = 0; index != end; index += chunkStride / 4)
            {
                for (int i = 0; i < itemCountInChunk; ++i)
                {
                    uint x = 0;

                    int bits = 0;
                    while (bits < MBitSize)
                    {
                        x |= (uint)((MData[indexPos] >> bitPos) << bits);
                        int num = System.Math.Min(MBitSize - bits, 8 - bitPos);
                        bitPos += num;
                        bits += num;
                        if (bitPos == 8)
                        {
                            indexPos++;
                            bitPos = 0;
                        }
                    }
                    x &= (uint)(1 << MBitSize) - 1u;
                    data.Add(x / (scale * ((1 << MBitSize) - 1)) + MStart);
                }
            }

            return data.ToArray();
        }
    }

    public class PackedIntVector
    {
        public uint MNumItems;
        public readonly byte[] MData;
        public byte MBitSize;

        public PackedIntVector(ObjectReader reader)
        {
            MNumItems = reader.ReadUInt32();

            int numData = reader.ReadInt32();
            MData = reader.ReadBytes(numData);
            reader.AlignStream();

            MBitSize = reader.ReadByte();
            reader.AlignStream();
        }

        public int[] UnpackInts()
        {
            int[]? data = new int[MNumItems];
            int indexPos = 0;
            int bitPos = 0;
            for (int i = 0; i < MNumItems; i++)
            {
                int bits = 0;
                data[i] = 0;
                while (bits < MBitSize)
                {
                    data[i] |= (MData[indexPos] >> bitPos) << bits;
                    int num = System.Math.Min(MBitSize - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                data[i] &= (1 << MBitSize) - 1;
            }
            return data;
        }
    }

    public class PackedQuatVector
    {
        public readonly uint MNumItems;
        public readonly byte[] MData;

        public PackedQuatVector(ObjectReader reader)
        {
            MNumItems = reader.ReadUInt32();

            int numData = reader.ReadInt32();
            MData = reader.ReadBytes(numData);

            reader.AlignStream();
        }

        public Quaternion[] UnpackQuats()
        {
            Quaternion[]? data = new Quaternion[MNumItems];
            int indexPos = 0;
            int bitPos = 0;

            for (int i = 0; i < MNumItems; i++)
            {
                uint flags = 0;

                int bits = 0;
                while (bits < 3)
                {
                    flags |= (uint)((MData[indexPos] >> bitPos) << bits);
                    int num = System.Math.Min(3 - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                flags &= 7;


                Quaternion q = new Quaternion();
                float sum = 0;
                for (int j = 0; j < 4; j++)
                {
                    if ((flags & 3) != j)
                    {
                        int bitSize = ((flags & 3) + 1) % 4 == j ? 9 : 10;
                        uint x = 0;

                        bits = 0;
                        while (bits < bitSize)
                        {
                            x |= (uint)((MData[indexPos] >> bitPos) << bits);
                            int num = System.Math.Min(bitSize - bits, 8 - bitPos);
                            bitPos += num;
                            bits += num;
                            if (bitPos == 8)
                            {
                                indexPos++;
                                bitPos = 0;
                            }
                        }
                        x &= (uint)((1 << bitSize) - 1);
                        q[j] = x / (0.5f * ((1 << bitSize) - 1)) - 1;
                        sum += q[j] * q[j];
                    }
                }

                int lastComponent = (int)(flags & 3);
                q[lastComponent] = (float)System.Math.Sqrt(1 - sum);
                if ((flags & 4) != 0u)
                    q[lastComponent] = -q[lastComponent];
                data[i] = q;
            }

            return data;
        }
    }

    public class CompressedAnimationCurve
    {
        public string MPath;
        public PackedIntVector MTimes;
        public PackedQuatVector MValues;
        public PackedFloatVector MSlopes;
        public int MPreInfinity;
        public int MPostInfinity;

        public CompressedAnimationCurve(ObjectReader reader)
        {
            MPath = reader.ReadAlignedString();
            MTimes = new PackedIntVector(reader);
            MValues = new PackedQuatVector(reader);
            MSlopes = new PackedFloatVector(reader);
            MPreInfinity = reader.ReadInt32();
            MPostInfinity = reader.ReadInt32();
        }
    }

    public class Vector3Curve
    {
        public AnimationCurve<Vector3> Curve;
        public string Path;

        public Vector3Curve(ObjectReader reader)
        {
            Curve = new AnimationCurve<Vector3>(reader, reader.ReadVector3);
            Path = reader.ReadAlignedString();
        }
    }

    public class FloatCurve
    {
        public AnimationCurve<float> Curve;
        public string Attribute;
        public string Path;
        public ClassIDType ClassID;
        public PPtr<MonoScript> Script;


        public FloatCurve(ObjectReader reader)
        {
            Curve = new AnimationCurve<float>(reader, reader.ReadSingle);
            Attribute = reader.ReadAlignedString();
            Path = reader.ReadAlignedString();
            ClassID = (ClassIDType)reader.ReadInt32();
            Script = new PPtr<MonoScript>(reader);
        }
    }

    public class PPtrKeyframe
    {
        public float Time;
        public PPtr<Object> Value;


        public PPtrKeyframe(ObjectReader reader)
        {
            Time = reader.ReadSingle();
            Value = new PPtr<Object>(reader);
        }
    }

    public class PPtrCurve
    {
        public readonly PPtrKeyframe[] Curve;
        public string Attribute;
        public string Path;
        public int ClassID;
        public PPtr<MonoScript> Script;


        public PPtrCurve(ObjectReader reader)
        {
            int numCurves = reader.ReadInt32();
            Curve = new PPtrKeyframe[numCurves];
            for (int i = 0; i < numCurves; i++)
            {
                Curve[i] = new PPtrKeyframe(reader);
            }

            Attribute = reader.ReadAlignedString();
            Path = reader.ReadAlignedString();
            ClassID = reader.ReadInt32();
            Script = new PPtr<MonoScript>(reader);
        }
    }

    public class AABB
    {
        public Vector3 MCenter;
        public Vector3 MExtent;

        public AABB(ObjectReader reader)
        {
            MCenter = reader.ReadVector3();
            MExtent = reader.ReadVector3();
        }
    }

    public class Xform
    {
        public Vector3 T;
        public Quaternion Q;
        public Vector3 S;

        public Xform(ObjectReader reader)
        {
            int[]? version = reader.Version;
            T = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
            Q = reader.ReadQuaternion();
            S = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
        }
    }

    public class HandPose
    {
        public Xform MGrabX;
        public float[] MDoFArray;
        public float MOverride;
        public float MCloseOpen;
        public float MInOut;
        public float MGrab;

        public HandPose(ObjectReader reader)
        {
            MGrabX = new Xform(reader);
            MDoFArray = reader.ReadSingleArray();
            MOverride = reader.ReadSingle();
            MCloseOpen = reader.ReadSingle();
            MInOut = reader.ReadSingle();
            MGrab = reader.ReadSingle();
        }
    }

    public class HumanGoal
    {
        public Xform MX;
        public float MWeightT;
        public float MWeightR;
        public Vector3 MHintT;
        public float MHintWeightT;

        public HumanGoal(ObjectReader reader)
        {
            int[]? version = reader.Version;
            MX = new Xform(reader);
            MWeightT = reader.ReadSingle();
            MWeightR = reader.ReadSingle();
            if (version[0] >= 5)//5.0 and up
            {
                MHintT = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
                MHintWeightT = reader.ReadSingle();
            }
        }
    }

    public class HumanPose
    {
        public Xform MRootX;
        public Vector3 MLookAtPosition;
        public Vector4 MLookAtWeight;
        public readonly HumanGoal[] MGoalArray;
        public HandPose MLeftHandPose;
        public HandPose MRightHandPose;
        public float[] MDoFArray;
        public readonly Vector3[] MTDoFArray;

        public HumanPose(ObjectReader reader)
        {
            int[]? version = reader.Version;
            MRootX = new Xform(reader);
            MLookAtPosition = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
            MLookAtWeight = reader.ReadVector4();

            int numGoals = reader.ReadInt32();
            MGoalArray = new HumanGoal[numGoals];
            for (int i = 0; i < numGoals; i++)
            {
                MGoalArray[i] = new HumanGoal(reader);
            }

            MLeftHandPose = new HandPose(reader);
            MRightHandPose = new HandPose(reader);

            MDoFArray = reader.ReadSingleArray();

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2))//5.2 and up
            {
                int numTDof = reader.ReadInt32();
                MTDoFArray = new Vector3[numTDof];
                for (int i = 0; i < numTDof; i++)
                {
                    MTDoFArray[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
                }
            }
        }
    }

    public class StreamedClip
    {
        public readonly uint[] Data;
        public uint CurveCount;

        public StreamedClip(ObjectReader reader)
        {
            Data = reader.ReadUInt32Array();
            CurveCount = reader.ReadUInt32();
        }

        public class StreamedCurveKey
        {
            public readonly int Index;
            public readonly float[] Coeff;

            public readonly float Value;
            public readonly float OutSlope;
            public float InSlope;

            public StreamedCurveKey(BinaryReader reader)
            {
                Index = reader.ReadInt32();
                Coeff = reader.ReadSingleArray(4);

                OutSlope = Coeff[2];
                Value = Coeff[3];
            }

            public float CalculateNextInSlope(float dx, StreamedCurveKey rhs)
            {
                //Stepped
                if (Coeff[0] == 0f && Coeff[1] == 0f && Coeff[2] == 0f)
                {
                    return float.PositiveInfinity;
                }

                dx = System.Math.Max(dx, 0.0001f);
                float dy = rhs.Value - Value;
                float length = 1.0f / (dx * dx);
                float d1 = OutSlope * dx;
                float d2 = dy + dy + dy - d1 - d1 - Coeff[1] / length;
                return d2 / dx;
            }
        }

        public class StreamedFrame
        {
            public readonly float Time;
            public readonly StreamedCurveKey[] KeyList;

            public StreamedFrame(BinaryReader reader)
            {
                Time = reader.ReadSingle();

                int numKeys = reader.ReadInt32();
                KeyList = new StreamedCurveKey[numKeys];
                for (int i = 0; i < numKeys; i++)
                {
                    KeyList[i] = new StreamedCurveKey(reader);
                }
            }
        }

        public List<StreamedFrame> ReadData()
        {
            List<StreamedFrame>? frameList = new List<StreamedFrame>();
            byte[]? buffer = new byte[Data.Length * 4];
            Buffer.BlockCopy(Data, 0, buffer, 0, buffer.Length);
            using (BinaryReader? reader = new BinaryReader(new MemoryStream(buffer)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    frameList.Add(new StreamedFrame(reader));
                }
            }

            for (int frameIndex = 2; frameIndex < frameList.Count - 1; frameIndex++)
            {
                StreamedFrame? frame = frameList[frameIndex];
                foreach (StreamedCurveKey? curveKey in frame.KeyList)
                {
                    for (int i = frameIndex - 1; i >= 0; i--)
                    {
                        StreamedFrame? preFrame = frameList[i];
                        StreamedCurveKey? preCurveKey = preFrame.KeyList.FirstOrDefault(x => x.Index == curveKey.Index);
                        if (preCurveKey != null)
                        {
                            curveKey.InSlope = preCurveKey.CalculateNextInSlope(frame.Time - preFrame.Time, curveKey);
                            break;
                        }
                    }
                }
            }
            return frameList;
        }
    }

    public class DenseClip
    {
        public int MFrameCount;
        public uint MCurveCount;
        public float MSampleRate;
        public float MBeginTime;
        public float[] MSampleArray;

        public DenseClip(ObjectReader reader)
        {
            MFrameCount = reader.ReadInt32();
            MCurveCount = reader.ReadUInt32();
            MSampleRate = reader.ReadSingle();
            MBeginTime = reader.ReadSingle();
            MSampleArray = reader.ReadSingleArray();
        }
    }

    public class ConstantClip
    {
        public float[] Data;

        public ConstantClip(ObjectReader reader)
        {
            Data = reader.ReadSingleArray();
        }
    }

    public class ValueConstant
    {
        public readonly uint MID;
        public readonly uint MTypeID;
        public uint MType;
        public uint MIndex;

        public ValueConstant(ObjectReader reader)
        {
            int[]? version = reader.Version;
            MID = reader.ReadUInt32();
            if (version[0] < 5 || (version[0] == 5 && version[1] < 5))//5.5 down
            {
                MTypeID = reader.ReadUInt32();
            }
            MType = reader.ReadUInt32();
            MIndex = reader.ReadUInt32();
        }
    }

    public class ValueArrayConstant
    {
        public readonly ValueConstant[] MValueArray;

        public ValueArrayConstant(ObjectReader reader)
        {
            int numVals = reader.ReadInt32();
            MValueArray = new ValueConstant[numVals];
            for (int i = 0; i < numVals; i++)
            {
                MValueArray[i] = new ValueConstant(reader);
            }
        }
    }

    public class Clip
    {
        public StreamedClip MStreamedClip;
        public DenseClip MDenseClip;
        public ConstantClip MConstantClip;
        public readonly ValueArrayConstant MBinding;

        public Clip(ObjectReader reader)
        {
            int[]? version = reader.Version;
            MStreamedClip = new StreamedClip(reader);
            MDenseClip = new DenseClip(reader);
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                MConstantClip = new ConstantClip(reader);
            }
            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 3)) //2018.3 down
            {
                MBinding = new ValueArrayConstant(reader);
            }
        }

        public AnimationClipBindingConstant ConvertValueArrayToGenericBinding()
        {
            AnimationClipBindingConstant? bindings = new AnimationClipBindingConstant();
            List<GenericBinding>? genericBindings = new List<GenericBinding>();
            ValueArrayConstant? values = MBinding;
            for (int i = 0; i < values.MValueArray.Length;)
            {
                uint curveID = values.MValueArray[i].MID;
                uint curveTypeID = values.MValueArray[i].MTypeID;
                GenericBinding? binding = new GenericBinding();
                genericBindings.Add(binding);
                if (curveTypeID == 4174552735) //CRC(PositionX))
                {
                    binding.Path = curveID;
                    binding.Attribute = 1; //kBindTransformPosition
                    binding.TypeID = ClassIDType.Transform;
                    i += 3;
                }
                else if (curveTypeID == 2211994246) //CRC(QuaternionX))
                {
                    binding.Path = curveID;
                    binding.Attribute = 2; //kBindTransformRotation
                    binding.TypeID = ClassIDType.Transform;
                    i += 4;
                }
                else if (curveTypeID == 1512518241) //CRC(ScaleX))
                {
                    binding.Path = curveID;
                    binding.Attribute = 3; //kBindTransformScale
                    binding.TypeID = ClassIDType.Transform;
                    i += 3;
                }
                else
                {
                    binding.TypeID = ClassIDType.Animator;
                    binding.Path = 0;
                    binding.Attribute = curveID;
                    i++;
                }
            }
            bindings.GenericBindings = genericBindings.ToArray();
            return bindings;
        }
    }

    public class ValueDelta
    {
        public float MStart;
        public float MStop;

        public ValueDelta(ObjectReader reader)
        {
            MStart = reader.ReadSingle();
            MStop = reader.ReadSingle();
        }
    }

    public class ClipMuscleConstant
    {
        public HumanPose MDeltaPose;
        public Xform MStartX;
        public Xform MStopX;
        public Xform MLeftFootStartX;
        public Xform MRightFootStartX;
        public Xform MMotionStartX;
        public Xform MMotionStopX;
        public Vector3 MAverageSpeed;
        public Clip MClip;
        public float MStartTime;
        public float MStopTime;
        public float MOrientationOffsetY;
        public float MLevel;
        public float MCycleOffset;
        public float MAverageAngularSpeed;
        public int[] MIndexArray;
        public readonly ValueDelta[] MValueArrayDelta;
        public float[] MValueArrayReferencePose;
        public bool MMirror;
        public bool MLoopTime;
        public bool MLoopBlend;
        public bool MLoopBlendOrientation;
        public bool MLoopBlendPositionY;
        public bool MLoopBlendPositionXZ;
        public bool MStartAtOrigin;
        public bool MKeepOriginalOrientation;
        public bool MKeepOriginalPositionY;
        public bool MKeepOriginalPositionXZ;
        public bool MHeightFromFeet;

        public ClipMuscleConstant(ObjectReader reader)
        {
            int[]? version = reader.Version;
            MDeltaPose = new HumanPose(reader);
            MStartX = new Xform(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 5))//5.5 and up
            {
                MStopX = new Xform(reader);
            }
            MLeftFootStartX = new Xform(reader);
            MRightFootStartX = new Xform(reader);
            if (version[0] < 5)//5.0 down
            {
                MMotionStartX = new Xform(reader);
                MMotionStopX = new Xform(reader);
            }
            MAverageSpeed = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
            MClip = new Clip(reader);
            MStartTime = reader.ReadSingle();
            MStopTime = reader.ReadSingle();
            MOrientationOffsetY = reader.ReadSingle();
            MLevel = reader.ReadSingle();
            MCycleOffset = reader.ReadSingle();
            MAverageAngularSpeed = reader.ReadSingle();

            MIndexArray = reader.ReadInt32Array();
            if (version[0] < 4 || (version[0] == 4 && version[1] < 3)) //4.3 down
            {
                int[]? mAdditionalCurveIndexArray = reader.ReadInt32Array();
            }
            int numDeltas = reader.ReadInt32();
            MValueArrayDelta = new ValueDelta[numDeltas];
            for (int i = 0; i < numDeltas; i++)
            {
                MValueArrayDelta[i] = new ValueDelta(reader);
            }
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3))//5.3 and up
            {
                MValueArrayReferencePose = reader.ReadSingleArray();
            }

            MMirror = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                MLoopTime = reader.ReadBoolean();
            }
            MLoopBlend = reader.ReadBoolean();
            MLoopBlendOrientation = reader.ReadBoolean();
            MLoopBlendPositionY = reader.ReadBoolean();
            MLoopBlendPositionXZ = reader.ReadBoolean();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 5))//5.5 and up
            {
                MStartAtOrigin = reader.ReadBoolean();
            }
            MKeepOriginalOrientation = reader.ReadBoolean();
            MKeepOriginalPositionY = reader.ReadBoolean();
            MKeepOriginalPositionXZ = reader.ReadBoolean();
            MHeightFromFeet = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

    public class GenericBinding
    {
        public uint Path;
        public uint Attribute;
        public PPtr<Object> Script;
        public ClassIDType TypeID;
        public byte CustomType;
        public byte IsPPtrCurve;
        public byte IsIntCurve;

        public GenericBinding() { }

        public GenericBinding(ObjectReader reader)
        {
            int[]? version = reader.Version;
            Path = reader.ReadUInt32();
            Attribute = reader.ReadUInt32();
            Script = new PPtr<Object>(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                TypeID = (ClassIDType)reader.ReadInt32();
            }
            else
            {
                TypeID = (ClassIDType)reader.ReadUInt16();
            }
            CustomType = reader.ReadByte();
            IsPPtrCurve = reader.ReadByte();
            if (version[0] > 2022 || (version[0] == 2022 && version[1] >= 1)) //2022.1 and up
            {
                IsIntCurve = reader.ReadByte();
            }
            reader.AlignStream();
        }
    }

    public class AnimationClipBindingConstant
    {
        public GenericBinding[] GenericBindings;
        public readonly PPtr<Object>[] PptrCurveMapping;

        public AnimationClipBindingConstant() { }

        public AnimationClipBindingConstant(ObjectReader reader)
        {
            int numBindings = reader.ReadInt32();
            GenericBindings = new GenericBinding[numBindings];
            for (int i = 0; i < numBindings; i++)
            {
                GenericBindings[i] = new GenericBinding(reader);
            }

            int numMappings = reader.ReadInt32();
            PptrCurveMapping = new PPtr<Object>[numMappings];
            for (int i = 0; i < numMappings; i++)
            {
                PptrCurveMapping[i] = new PPtr<Object>(reader);
            }
        }

        public GenericBinding FindBinding(int index)
        {
            int curves = 0;
            foreach (GenericBinding? b in GenericBindings)
            {
                if (b.TypeID == ClassIDType.Transform)
                {
                    switch (b.Attribute)
                    {
                        case 1: //kBindTransformPosition
                        case 3: //kBindTransformScale
                        case 4: //kBindTransformEuler
                            curves += 3;
                            break;
                        case 2: //kBindTransformRotation
                            curves += 4;
                            break;
                        default:
                            curves += 1;
                            break;
                    }
                }
                else
                {
                    curves += 1;
                }
                if (curves > index)
                {
                    return b;
                }
            }

            return null;
        }
    }

    public class AnimationEvent
    {
        public float Time;
        public string FunctionName;
        public string Data;
        public PPtr<Object> ObjectReferenceParameter;
        public float FloatParameter;
        public int INTParameter;
        public int MessageOptions;

        public AnimationEvent(ObjectReader reader)
        {
            int[]? version = reader.Version;

            Time = reader.ReadSingle();
            FunctionName = reader.ReadAlignedString();
            Data = reader.ReadAlignedString();
            ObjectReferenceParameter = new PPtr<Object>(reader);
            FloatParameter = reader.ReadSingle();
            if (version[0] >= 3) //3 and up
            {
                INTParameter = reader.ReadInt32();
            }
            MessageOptions = reader.ReadInt32();
        }
    }

    public enum AnimationType
    {
        Legacy = 1,
        Generic = 2,
        Humanoid = 3
    };

    public sealed class AnimationClip : NamedObject
    {
        public readonly AnimationType MAnimationType;
        public bool MLegacy;
        public bool MCompressed;
        public bool MUseHighQualityCurve;
        public readonly QuaternionCurve[] MRotationCurves;
        public readonly CompressedAnimationCurve[] MCompressedRotationCurves;
        public readonly Vector3Curve[] MEulerCurves;
        public readonly Vector3Curve[] MPositionCurves;
        public readonly Vector3Curve[] MScaleCurves;
        public readonly FloatCurve[] MFloatCurves;
        public readonly PPtrCurve[] MPPtrCurves;
        public float MSampleRate;
        public int MWrapMode;
        public AABB MBounds;
        public uint MMuscleClipSize;
        public ClipMuscleConstant MMuscleClip;
        public AnimationClipBindingConstant MClipBindingConstant;
        public readonly AnimationEvent[] MEvents;


        public AnimationClip(ObjectReader reader) : base(reader)
        {
            if (Version[0] >= 5)//5.0 and up
            {
                MLegacy = reader.ReadBoolean();
            }
            else if (Version[0] >= 4)//4.0 and up
            {
                MAnimationType = (AnimationType)reader.ReadInt32();
                if (MAnimationType == AnimationType.Legacy)
                    MLegacy = true;
            }
            else
            {
                MLegacy = true;
            }
            MCompressed = reader.ReadBoolean();
            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3))//4.3 and up
            {
                MUseHighQualityCurve = reader.ReadBoolean();
            }
            reader.AlignStream();
            int numRCurves = reader.ReadInt32();
            MRotationCurves = new QuaternionCurve[numRCurves];
            for (int i = 0; i < numRCurves; i++)
            {
                MRotationCurves[i] = new QuaternionCurve(reader);
            }

            int numCRCurves = reader.ReadInt32();
            MCompressedRotationCurves = new CompressedAnimationCurve[numCRCurves];
            for (int i = 0; i < numCRCurves; i++)
            {
                MCompressedRotationCurves[i] = new CompressedAnimationCurve(reader);
            }

            if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 3))//5.3 and up
            {
                int numEulerCurves = reader.ReadInt32();
                MEulerCurves = new Vector3Curve[numEulerCurves];
                for (int i = 0; i < numEulerCurves; i++)
                {
                    MEulerCurves[i] = new Vector3Curve(reader);
                }
            }

            int numPCurves = reader.ReadInt32();
            MPositionCurves = new Vector3Curve[numPCurves];
            for (int i = 0; i < numPCurves; i++)
            {
                MPositionCurves[i] = new Vector3Curve(reader);
            }

            int numSCurves = reader.ReadInt32();
            MScaleCurves = new Vector3Curve[numSCurves];
            for (int i = 0; i < numSCurves; i++)
            {
                MScaleCurves[i] = new Vector3Curve(reader);
            }

            int numFCurves = reader.ReadInt32();
            MFloatCurves = new FloatCurve[numFCurves];
            for (int i = 0; i < numFCurves; i++)
            {
                MFloatCurves[i] = new FloatCurve(reader);
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3)) //4.3 and up
            {
                int numPtrCurves = reader.ReadInt32();
                MPPtrCurves = new PPtrCurve[numPtrCurves];
                for (int i = 0; i < numPtrCurves; i++)
                {
                    MPPtrCurves[i] = new PPtrCurve(reader);
                }
            }

            MSampleRate = reader.ReadSingle();
            MWrapMode = reader.ReadInt32();
            if (Version[0] > 3 || (Version[0] == 3 && Version[1] >= 4)) //3.4 and up
            {
                MBounds = new AABB(reader);
            }
            if (Version[0] >= 4)//4.0 and up
            {
                MMuscleClipSize = reader.ReadUInt32();
                MMuscleClip = new ClipMuscleConstant(reader);
            }
            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3)) //4.3 and up
            {
                MClipBindingConstant = new AnimationClipBindingConstant(reader);
            }
            if (Version[0] > 2018 || (Version[0] == 2018 && Version[1] >= 3)) //2018.3 and up
            {
                bool mHasGenericRootTransform = reader.ReadBoolean();
                bool mHasMotionFloatCurves = reader.ReadBoolean();
                reader.AlignStream();
            }
            int numEvents = reader.ReadInt32();
            MEvents = new AnimationEvent[numEvents];
            for (int i = 0; i < numEvents; i++)
            {
                MEvents[i] = new AnimationEvent(reader);
            }
            if (Version[0] >= 2017) //2017 and up
            {
                reader.AlignStream();
            }
        }
    }
}
