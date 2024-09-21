using UnityBundleReader.Extensions;
using UnityBundleReader.Math;

namespace UnityBundleReader.Classes;

public class HumanPoseMask
{
    public uint Word0;
    public uint Word1;
    public uint Word2;

    public HumanPoseMask(ObjectReader reader)
    {
        int[]? version = reader.Version;

        Word0 = reader.ReadUInt32();
        Word1 = reader.ReadUInt32();
        if (version[0] > 5 || version[0] == 5 && version[1] >= 2) //5.2 and up
        {
            Word2 = reader.ReadUInt32();
        }
    }
}

public class SkeletonMaskElement
{
    public uint MPathHash;
    public float MWeight;

    public SkeletonMaskElement(ObjectReader reader)
    {
        MPathHash = reader.ReadUInt32();
        MWeight = reader.ReadSingle();
    }
}

public class SkeletonMask
{
    public readonly SkeletonMaskElement[] MData;

    public SkeletonMask(ObjectReader reader)
    {
        int numElements = reader.ReadInt32();
        MData = new SkeletonMaskElement[numElements];
        for (int i = 0; i < numElements; i++)
        {
            MData[i] = new SkeletonMaskElement(reader);
        }
    }
}

public class LayerConstant
{
    public uint MStateMachineIndex;
    public uint MStateMachineMotionSetIndex;
    public HumanPoseMask MBodyMask;
    public SkeletonMask MSkeletonMask;
    public uint MBinding;
    public int MLayerBlendingMode;
    public float MDefaultWeight;
    public bool MIKPass;
    public bool MSyncedLayerAffectsTiming;

    public LayerConstant(ObjectReader reader)
    {
        int[]? version = reader.Version;

        MStateMachineIndex = reader.ReadUInt32();
        MStateMachineMotionSetIndex = reader.ReadUInt32();
        MBodyMask = new HumanPoseMask(reader);
        MSkeletonMask = new SkeletonMask(reader);
        MBinding = reader.ReadUInt32();
        MLayerBlendingMode = reader.ReadInt32();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 2) //4.2 and up
        {
            MDefaultWeight = reader.ReadSingle();
        }
        MIKPass = reader.ReadBoolean();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 2) //4.2 and up
        {
            MSyncedLayerAffectsTiming = reader.ReadBoolean();
        }
        reader.AlignStream();
    }
}

public class ConditionConstant
{
    public uint MConditionMode;
    public uint MEventID;
    public float MEventThreshold;
    public float MExitTime;

    public ConditionConstant(ObjectReader reader)
    {
        MConditionMode = reader.ReadUInt32();
        MEventID = reader.ReadUInt32();
        MEventThreshold = reader.ReadSingle();
        MExitTime = reader.ReadSingle();
    }
}

public class TransitionConstant
{
    public readonly ConditionConstant[] MConditionConstantArray;
    public uint MDestinationState;
    public uint MFullPathID;
    public uint MID;
    public uint MUserID;
    public float MTransitionDuration;
    public float MTransitionOffset;
    public float MExitTime;
    public bool MHasExitTime;
    public bool MHasFixedDuration;
    public int MInterruptionSource;
    public bool MOrderedInterruption;
    public bool MAtomic;
    public bool MCanTransitionToSelf;

    public TransitionConstant(ObjectReader reader)
    {
        int[]? version = reader.Version;

        int numConditions = reader.ReadInt32();
        MConditionConstantArray = new ConditionConstant[numConditions];
        for (int i = 0; i < numConditions; i++)
        {
            MConditionConstantArray[i] = new ConditionConstant(reader);
        }

        MDestinationState = reader.ReadUInt32();
        if (version[0] >= 5) //5.0 and up
        {
            MFullPathID = reader.ReadUInt32();
        }

        MID = reader.ReadUInt32();
        MUserID = reader.ReadUInt32();
        MTransitionDuration = reader.ReadSingle();
        MTransitionOffset = reader.ReadSingle();
        if (version[0] >= 5) //5.0 and up
        {
            MExitTime = reader.ReadSingle();
            MHasExitTime = reader.ReadBoolean();
            MHasFixedDuration = reader.ReadBoolean();
            reader.AlignStream();
            MInterruptionSource = reader.ReadInt32();
            MOrderedInterruption = reader.ReadBoolean();
        }
        else
        {
            MAtomic = reader.ReadBoolean();
        }

        if (version[0] > 4 || version[0] == 4 && version[1] >= 5) //4.5 and up
        {
            MCanTransitionToSelf = reader.ReadBoolean();
        }

        reader.AlignStream();
    }
}

public class LeafInfoConstant
{
    public uint[] MIDArray;
    public uint MIndexOffset;

    public LeafInfoConstant(ObjectReader reader)
    {
        MIDArray = reader.ReadUInt32Array();
        MIndexOffset = reader.ReadUInt32();
    }
}

public class MotionNeighborList
{
    public uint[] MNeighborArray;

    public MotionNeighborList(ObjectReader reader)
    {
        MNeighborArray = reader.ReadUInt32Array();
    }
}

public class Blend2dDataConstant
{
    public Vector2[] MChildPositionArray;
    public float[] MChildMagnitudeArray;
    public Vector2[] MChildPairVectorArray;
    public float[] MChildPairAvgMagInvArray;
    public readonly MotionNeighborList[] MChildNeighborListArray;

    public Blend2dDataConstant(ObjectReader reader)
    {
        MChildPositionArray = reader.ReadVector2Array();
        MChildMagnitudeArray = reader.ReadSingleArray();
        MChildPairVectorArray = reader.ReadVector2Array();
        MChildPairAvgMagInvArray = reader.ReadSingleArray();

        int numNeighbours = reader.ReadInt32();
        MChildNeighborListArray = new MotionNeighborList[numNeighbours];
        for (int i = 0; i < numNeighbours; i++)
        {
            MChildNeighborListArray[i] = new MotionNeighborList(reader);
        }
    }
}

public class Blend1dDataConstant // wrong labeled
{
    public float[] MChildThresholdArray;

    public Blend1dDataConstant(ObjectReader reader)
    {
        MChildThresholdArray = reader.ReadSingleArray();
    }
}

public class BlendDirectDataConstant
{
    public uint[] MChildBlendEventIDArray;
    public bool MNormalizedBlendValues;

    public BlendDirectDataConstant(ObjectReader reader)
    {
        MChildBlendEventIDArray = reader.ReadUInt32Array();
        MNormalizedBlendValues = reader.ReadBoolean();
        reader.AlignStream();
    }
}

public class BlendTreeNodeConstant
{
    public uint MBlendType;
    public uint MBlendEventID;
    public uint MBlendEventYid;
    public uint[] MChildIndices;
    public float[] MChildThresholdArray;
    public Blend1dDataConstant MBlend1dData;
    public Blend2dDataConstant MBlend2dData;
    public BlendDirectDataConstant MBlendDirectData;
    public uint MClipID;
    public uint MClipIndex;
    public float MDuration;
    public float MCycleOffset;
    public bool MMirror;

    public BlendTreeNodeConstant(ObjectReader reader)
    {
        int[]? version = reader.Version;

        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
        {
            MBlendType = reader.ReadUInt32();
        }
        MBlendEventID = reader.ReadUInt32();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
        {
            MBlendEventYid = reader.ReadUInt32();
        }
        MChildIndices = reader.ReadUInt32Array();
        if (version[0] < 4 || version[0] == 4 && version[1] < 1) //4.1 down
        {
            MChildThresholdArray = reader.ReadSingleArray();
        }

        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
        {
            MBlend1dData = new Blend1dDataConstant(reader);
            MBlend2dData = new Blend2dDataConstant(reader);
        }

        if (version[0] >= 5) //5.0 and up
        {
            MBlendDirectData = new BlendDirectDataConstant(reader);
        }

        MClipID = reader.ReadUInt32();
        if (version[0] == 4 && version[1] >= 5) //4.5 - 5.0
        {
            MClipIndex = reader.ReadUInt32();
        }

        MDuration = reader.ReadSingle();

        if (version[0] > 4 || version[0] == 4 && version[1] > 1 || version[0] == 4 && version[1] == 1 && version[2] >= 3) //4.1.3 and up
        {
            MCycleOffset = reader.ReadSingle();
            MMirror = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}

public class BlendTreeConstant
{
    public readonly BlendTreeNodeConstant[] MNodeArray;
    public ValueArrayConstant MBlendEventArrayConstant;

    public BlendTreeConstant(ObjectReader reader)
    {
        int[]? version = reader.Version;

        int numNodes = reader.ReadInt32();
        MNodeArray = new BlendTreeNodeConstant[numNodes];
        for (int i = 0; i < numNodes; i++)
        {
            MNodeArray[i] = new BlendTreeNodeConstant(reader);
        }

        if (version[0] < 4 || version[0] == 4 && version[1] < 5) //4.5 down
        {
            MBlendEventArrayConstant = new ValueArrayConstant(reader);
        }
    }
}

public class StateConstant
{
    public readonly TransitionConstant[] MTransitionConstantArray;
    public int[] MBlendTreeConstantIndexArray;
    public readonly LeafInfoConstant[] MLeafInfoArray;
    public readonly BlendTreeConstant[] MBlendTreeConstantArray;
    public uint MNameID;
    public uint MPathID;
    public uint MFullPathID;
    public uint MTagID;
    public uint MSpeedParamID;
    public uint MMirrorParamID;
    public uint MCycleOffsetParamID;
    public float MSpeed;
    public float MCycleOffset;
    public bool MIKOnFeet;
    public bool MWriteDefaultValues;
    public bool MLoop;
    public bool MMirror;

    public StateConstant(ObjectReader reader)
    {
        int[]? version = reader.Version;

        int numTransistions = reader.ReadInt32();
        MTransitionConstantArray = new TransitionConstant[numTransistions];
        for (int i = 0; i < numTransistions; i++)
        {
            MTransitionConstantArray[i] = new TransitionConstant(reader);
        }

        MBlendTreeConstantIndexArray = reader.ReadInt32Array();

        if (version[0] < 5 || version[0] == 5 && version[1] < 2) //5.2 down
        {
            int numInfos = reader.ReadInt32();
            MLeafInfoArray = new LeafInfoConstant[numInfos];
            for (int i = 0; i < numInfos; i++)
            {
                MLeafInfoArray[i] = new LeafInfoConstant(reader);
            }
        }

        int numBlends = reader.ReadInt32();
        MBlendTreeConstantArray = new BlendTreeConstant[numBlends];
        for (int i = 0; i < numBlends; i++)
        {
            MBlendTreeConstantArray[i] = new BlendTreeConstant(reader);
        }

        MNameID = reader.ReadUInt32();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 3) //4.3 and up
        {
            MPathID = reader.ReadUInt32();
        }
        if (version[0] >= 5) //5.0 and up
        {
            MFullPathID = reader.ReadUInt32();
        }

        MTagID = reader.ReadUInt32();
        if (version[0] > 5 || version[0] == 5 && version[1] >= 1) //5.1 and up
        {
            MSpeedParamID = reader.ReadUInt32();
            MMirrorParamID = reader.ReadUInt32();
            MCycleOffsetParamID = reader.ReadUInt32();
        }

        if (version[0] > 2017 || version[0] == 2017 && version[1] >= 2) //2017.2 and up
        {
            uint mTimeParamID = reader.ReadUInt32();
        }

        MSpeed = reader.ReadSingle();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
        {
            MCycleOffset = reader.ReadSingle();
        }
        MIKOnFeet = reader.ReadBoolean();
        if (version[0] >= 5) //5.0 and up
        {
            MWriteDefaultValues = reader.ReadBoolean();
        }

        MLoop = reader.ReadBoolean();
        if (version[0] > 4 || version[0] == 4 && version[1] >= 1) //4.1 and up
        {
            MMirror = reader.ReadBoolean();
        }

        reader.AlignStream();
    }
}

public class SelectorTransitionConstant
{
    public uint MDestination;
    public readonly ConditionConstant[] MConditionConstantArray;

    public SelectorTransitionConstant(ObjectReader reader)
    {
        MDestination = reader.ReadUInt32();

        int numConditions = reader.ReadInt32();
        MConditionConstantArray = new ConditionConstant[numConditions];
        for (int i = 0; i < numConditions; i++)
        {
            MConditionConstantArray[i] = new ConditionConstant(reader);
        }
    }
}

public class SelectorStateConstant
{
    public readonly SelectorTransitionConstant[] MTransitionConstantArray;
    public uint MFullPathID;
    public bool MIsEntry;

    public SelectorStateConstant(ObjectReader reader)
    {
        int numTransitions = reader.ReadInt32();
        MTransitionConstantArray = new SelectorTransitionConstant[numTransitions];
        for (int i = 0; i < numTransitions; i++)
        {
            MTransitionConstantArray[i] = new SelectorTransitionConstant(reader);
        }

        MFullPathID = reader.ReadUInt32();
        MIsEntry = reader.ReadBoolean();
        reader.AlignStream();
    }
}

public class StateMachineConstant
{
    public readonly StateConstant[] MStateConstantArray;
    public readonly TransitionConstant[] MAnyStateTransitionConstantArray;
    public readonly SelectorStateConstant[] MSelectorStateConstantArray;
    public uint MDefaultState;
    public uint MMotionSetCount;

    public StateMachineConstant(ObjectReader reader)
    {
        int[]? version = reader.Version;

        int numStates = reader.ReadInt32();
        MStateConstantArray = new StateConstant[numStates];
        for (int i = 0; i < numStates; i++)
        {
            MStateConstantArray[i] = new StateConstant(reader);
        }

        int numAnyStates = reader.ReadInt32();
        MAnyStateTransitionConstantArray = new TransitionConstant[numAnyStates];
        for (int i = 0; i < numAnyStates; i++)
        {
            MAnyStateTransitionConstantArray[i] = new TransitionConstant(reader);
        }

        if (version[0] >= 5) //5.0 and up
        {
            int numSelectors = reader.ReadInt32();
            MSelectorStateConstantArray = new SelectorStateConstant[numSelectors];
            for (int i = 0; i < numSelectors; i++)
            {
                MSelectorStateConstantArray[i] = new SelectorStateConstant(reader);
            }
        }

        MDefaultState = reader.ReadUInt32();
        MMotionSetCount = reader.ReadUInt32();
    }
}

public class ValueArray
{
    public bool[] MBoolValues;
    public int[] MIntValues;
    public float[] MFloatValues;
    public Vector4[] MVectorValues;
    public readonly Vector3[] MPositionValues;
    public Vector4[] MQuaternionValues;
    public readonly Vector3[] MScaleValues;

    public ValueArray(ObjectReader reader)
    {
        int[]? version = reader.Version;

        if (version[0] < 5 || version[0] == 5 && version[1] < 5) //5.5 down
        {
            MBoolValues = reader.ReadBooleanArray();
            reader.AlignStream();
            MIntValues = reader.ReadInt32Array();
            MFloatValues = reader.ReadSingleArray();
        }

        if (version[0] < 4 || version[0] == 4 && version[1] < 3) //4.3 down
        {
            MVectorValues = reader.ReadVector4Array();
        }
        else
        {
            int numPosValues = reader.ReadInt32();
            MPositionValues = new Vector3[numPosValues];
            for (int i = 0; i < numPosValues; i++)
            {
                MPositionValues[i] = version[0] > 5 || version[0] == 5 && version[1] >= 4 ? reader.ReadVector3() : reader.ReadVector4(); //5.4 and up
            }

            MQuaternionValues = reader.ReadVector4Array();

            int numScaleValues = reader.ReadInt32();
            MScaleValues = new Vector3[numScaleValues];
            for (int i = 0; i < numScaleValues; i++)
            {
                MScaleValues[i] = version[0] > 5 || version[0] == 5 && version[1] >= 4 ? reader.ReadVector3() : reader.ReadVector4(); //5.4 and up
            }

            if (version[0] > 5 || version[0] == 5 && version[1] >= 5) //5.5 and up
            {
                MFloatValues = reader.ReadSingleArray();
                MIntValues = reader.ReadInt32Array();
                MBoolValues = reader.ReadBooleanArray();
                reader.AlignStream();
            }
        }
    }
}

public class ControllerConstant
{
    public readonly LayerConstant[] MLayerArray;
    public readonly StateMachineConstant[] MStateMachineArray;
    public ValueArrayConstant MValues;
    public ValueArray MDefaultValues;

    public ControllerConstant(ObjectReader reader)
    {
        int numLayers = reader.ReadInt32();
        MLayerArray = new LayerConstant[numLayers];
        for (int i = 0; i < numLayers; i++)
        {
            MLayerArray[i] = new LayerConstant(reader);
        }

        int numStates = reader.ReadInt32();
        MStateMachineArray = new StateMachineConstant[numStates];
        for (int i = 0; i < numStates; i++)
        {
            MStateMachineArray[i] = new StateMachineConstant(reader);
        }

        MValues = new ValueArrayConstant(reader);
        MDefaultValues = new ValueArray(reader);
    }
}

public sealed class AnimatorController : RuntimeAnimatorController
{
    public readonly PPtr<AnimationClip>[] MAnimationClips;

    public AnimatorController(ObjectReader reader) : base(reader)
    {
        uint mControllerSize = reader.ReadUInt32();
        ControllerConstant? mController = new(reader);

        int tosSize = reader.ReadInt32();
        KeyValuePair<uint, string>[]? mTos = new KeyValuePair<uint, string>[tosSize];
        for (int i = 0; i < tosSize; i++)
        {
            mTos[i] = new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadAlignedString());
        }

        int numClips = reader.ReadInt32();
        MAnimationClips = new PPtr<AnimationClip>[numClips];
        for (int i = 0; i < numClips; i++)
        {
            MAnimationClips[i] = new PPtr<AnimationClip>(reader);
        }
    }
}
