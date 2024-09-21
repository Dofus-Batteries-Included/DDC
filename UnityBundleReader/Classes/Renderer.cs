using UnityBundleReader.Extensions;
using UnityBundleReader.Math;

namespace UnityBundleReader.Classes
{
    public class StaticBatchInfo
    {
        public ushort FirstSubMesh;
        public ushort SubMeshCount;

        public StaticBatchInfo(ObjectReader reader)
        {
            FirstSubMesh = reader.ReadUInt16();
            SubMeshCount = reader.ReadUInt16();
        }
    }

    public abstract class Renderer : Component
    {
        public readonly PPtr<Material>[] MMaterials;
        public StaticBatchInfo MStaticBatchInfo;
        public uint[] MSubsetIndices;

        protected Renderer(ObjectReader reader) : base(reader)
        {
            if (Version[0] < 5) //5.0 down
            {
                bool mEnabled = reader.ReadBoolean();
                bool mCastShadows = reader.ReadBoolean();
                bool mReceiveShadows = reader.ReadBoolean();
                byte mLightmapIndex = reader.ReadByte();
            }
            else //5.0 and up
            {
                if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 4)) //5.4 and up
                {
                    bool mEnabled = reader.ReadBoolean();
                    byte mCastShadows = reader.ReadByte();
                    byte mReceiveShadows = reader.ReadByte();
                    if (Version[0] > 2017 || (Version[0] == 2017 && Version[1] >= 2)) //2017.2 and up
                    {
                        byte mDynamicOccludee = reader.ReadByte();
                    }
                    if (Version[0] >= 2021) //2021.1 and up
                    {
                        byte mStaticShadowCaster = reader.ReadByte();
                    }
                    byte mMotionVectors = reader.ReadByte();
                    byte mLightProbeUsage = reader.ReadByte();
                    byte mReflectionProbeUsage = reader.ReadByte();
                    if (Version[0] > 2019 || (Version[0] == 2019 && Version[1] >= 3)) //2019.3 and up
                    {
                        byte mRayTracingMode = reader.ReadByte();
                    }
                    if (Version[0] >= 2020) //2020.1 and up
                    {
                        byte mRayTraceProcedural = reader.ReadByte();
                    }
                    reader.AlignStream();
                }
                else
                {
                    bool mEnabled = reader.ReadBoolean();
                    reader.AlignStream();
                    byte mCastShadows = reader.ReadByte();
                    bool mReceiveShadows = reader.ReadBoolean();
                    reader.AlignStream();
                }

                if (Version[0] >= 2018) //2018 and up
                {
                    uint mRenderingLayerMask = reader.ReadUInt32();
                }

                if (Version[0] > 2018 || (Version[0] == 2018 && Version[1] >= 3)) //2018.3 and up
                {
                    int mRendererPriority = reader.ReadInt32();
                }

                ushort mLightmapIndex = reader.ReadUInt16();
                ushort mLightmapIndexDynamic = reader.ReadUInt16();
            }

            if (Version[0] >= 3) //3.0 and up
            {
                Vector4 mLightmapTilingOffset = reader.ReadVector4();
            }

            if (Version[0] >= 5) //5.0 and up
            {
                Vector4 mLightmapTilingOffsetDynamic = reader.ReadVector4();
            }

            int mMaterialsSize = reader.ReadInt32();
            MMaterials = new PPtr<Material>[mMaterialsSize];
            for (int i = 0; i < mMaterialsSize; i++)
            {
                MMaterials[i] = new PPtr<Material>(reader);
            }

            if (Version[0] < 3) //3.0 down
            {
                Vector4 mLightmapTilingOffset = reader.ReadVector4();
            }
            else //3.0 and up
            {
                if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 5)) //5.5 and up
                {
                    MStaticBatchInfo = new StaticBatchInfo(reader);
                }
                else
                {
                    MSubsetIndices = reader.ReadUInt32Array();
                }

                PPtr<Transform>? mStaticBatchRoot = new PPtr<Transform>(reader);
            }

            if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 4)) //5.4 and up
            {
                PPtr<Transform>? mProbeAnchor = new PPtr<Transform>(reader);
                PPtr<GameObject>? mLightProbeVolumeOverride = new PPtr<GameObject>(reader);
            }
            else if (Version[0] > 3 || (Version[0] == 3 && Version[1] >= 5)) //3.5 - 5.3
            {
                bool mUseLightProbes = reader.ReadBoolean();
                reader.AlignStream();

                if (Version[0] >= 5)//5.0 and up
                {
                    int mReflectionProbeUsage = reader.ReadInt32();
                }

                PPtr<Transform>? mLightProbeAnchor = new PPtr<Transform>(reader); //5.0 and up m_ProbeAnchor
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3)) //4.3 and up
            {
                if (Version[0] == 4 && Version[1] == 3) //4.3
                {
                    short mSortingLayer = reader.ReadInt16();
                }
                else
                {
                    uint mSortingLayerID = reader.ReadUInt32();
                }

                //SInt16 m_SortingLayer 5.6 and up
                short mSortingOrder = reader.ReadInt16();
                reader.AlignStream();
            }
        }
    }
}
