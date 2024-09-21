using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
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
        public PPtr<Material>[] MMaterials;
        public StaticBatchInfo MStaticBatchInfo;
        public uint[] MSubsetIndices;

        protected Renderer(ObjectReader reader) : base(reader)
        {
            if (Version[0] < 5) //5.0 down
            {
                var mEnabled = reader.ReadBoolean();
                var mCastShadows = reader.ReadBoolean();
                var mReceiveShadows = reader.ReadBoolean();
                var mLightmapIndex = reader.ReadByte();
            }
            else //5.0 and up
            {
                if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 4)) //5.4 and up
                {
                    var mEnabled = reader.ReadBoolean();
                    var mCastShadows = reader.ReadByte();
                    var mReceiveShadows = reader.ReadByte();
                    if (Version[0] > 2017 || (Version[0] == 2017 && Version[1] >= 2)) //2017.2 and up
                    {
                        var mDynamicOccludee = reader.ReadByte();
                    }
                    if (Version[0] >= 2021) //2021.1 and up
                    {
                        var mStaticShadowCaster = reader.ReadByte();
                    }
                    var mMotionVectors = reader.ReadByte();
                    var mLightProbeUsage = reader.ReadByte();
                    var mReflectionProbeUsage = reader.ReadByte();
                    if (Version[0] > 2019 || (Version[0] == 2019 && Version[1] >= 3)) //2019.3 and up
                    {
                        var mRayTracingMode = reader.ReadByte();
                    }
                    if (Version[0] >= 2020) //2020.1 and up
                    {
                        var mRayTraceProcedural = reader.ReadByte();
                    }
                    reader.AlignStream();
                }
                else
                {
                    var mEnabled = reader.ReadBoolean();
                    reader.AlignStream();
                    var mCastShadows = reader.ReadByte();
                    var mReceiveShadows = reader.ReadBoolean();
                    reader.AlignStream();
                }

                if (Version[0] >= 2018) //2018 and up
                {
                    var mRenderingLayerMask = reader.ReadUInt32();
                }

                if (Version[0] > 2018 || (Version[0] == 2018 && Version[1] >= 3)) //2018.3 and up
                {
                    var mRendererPriority = reader.ReadInt32();
                }

                var mLightmapIndex = reader.ReadUInt16();
                var mLightmapIndexDynamic = reader.ReadUInt16();
            }

            if (Version[0] >= 3) //3.0 and up
            {
                var mLightmapTilingOffset = reader.ReadVector4();
            }

            if (Version[0] >= 5) //5.0 and up
            {
                var mLightmapTilingOffsetDynamic = reader.ReadVector4();
            }

            var mMaterialsSize = reader.ReadInt32();
            MMaterials = new PPtr<Material>[mMaterialsSize];
            for (int i = 0; i < mMaterialsSize; i++)
            {
                MMaterials[i] = new PPtr<Material>(reader);
            }

            if (Version[0] < 3) //3.0 down
            {
                var mLightmapTilingOffset = reader.ReadVector4();
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

                var mStaticBatchRoot = new PPtr<Transform>(reader);
            }

            if (Version[0] > 5 || (Version[0] == 5 && Version[1] >= 4)) //5.4 and up
            {
                var mProbeAnchor = new PPtr<Transform>(reader);
                var mLightProbeVolumeOverride = new PPtr<GameObject>(reader);
            }
            else if (Version[0] > 3 || (Version[0] == 3 && Version[1] >= 5)) //3.5 - 5.3
            {
                var mUseLightProbes = reader.ReadBoolean();
                reader.AlignStream();

                if (Version[0] >= 5)//5.0 and up
                {
                    var mReflectionProbeUsage = reader.ReadInt32();
                }

                var mLightProbeAnchor = new PPtr<Transform>(reader); //5.0 and up m_ProbeAnchor
            }

            if (Version[0] > 4 || (Version[0] == 4 && Version[1] >= 3)) //4.3 and up
            {
                if (Version[0] == 4 && Version[1] == 3) //4.3
                {
                    var mSortingLayer = reader.ReadInt16();
                }
                else
                {
                    var mSortingLayerID = reader.ReadUInt32();
                }

                //SInt16 m_SortingLayer 5.6 and up
                var mSortingOrder = reader.ReadInt16();
                reader.AlignStream();
            }
        }
    }
}
