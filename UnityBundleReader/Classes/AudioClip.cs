using UnityBundleReader.Extensions;

namespace UnityBundleReader.Classes
{
    public sealed class AudioClip : NamedObject
    {
        public int MFormat;
        public FMODSoundType MType;
        public bool M3D;
        public bool MUseHardware;

        //version 5
        public int MLoadType;
        public int MChannels;
        public int MFrequency;
        public int MBitsPerSample;
        public float MLength;
        public bool MIsTrackerFormat;
        public int MSubsoundIndex;
        public bool MPreloadAudioData;
        public bool MLoadInBackground;
        public bool MLegacy3D;
        public AudioCompressionFormat MCompressionFormat;

        public string MSource;
        public long MOffset; //ulong
        public long MSize; //ulong
        public ResourceReader MAudioData;

        public AudioClip(ObjectReader reader) : base(reader)
        {
            if (Version[0] < 5)
            {
                MFormat = reader.ReadInt32();
                MType = (FMODSoundType)reader.ReadInt32();
                M3D = reader.ReadBoolean();
                MUseHardware = reader.ReadBoolean();
                reader.AlignStream();

                if (Version[0] >= 4 || (Version[0] == 3 && Version[1] >= 2)) //3.2.0 to 5
                {
                    int mStream = reader.ReadInt32();
                    MSize = reader.ReadInt32();
                    var tsize = MSize % 4 != 0 ? MSize + 4 - MSize % 4 : MSize;
                    if (reader.ByteSize + reader.ByteStart - reader.Position != tsize)
                    {
                        MOffset = reader.ReadUInt32();
                        MSource = AssetsFile.FullName + ".resS";
                    }
                }
                else
                {
                    MSize = reader.ReadInt32();
                }
            }
            else
            {
                MLoadType = reader.ReadInt32();
                MChannels = reader.ReadInt32();
                MFrequency = reader.ReadInt32();
                MBitsPerSample = reader.ReadInt32();
                MLength = reader.ReadSingle();
                MIsTrackerFormat = reader.ReadBoolean();
                reader.AlignStream();
                MSubsoundIndex = reader.ReadInt32();
                MPreloadAudioData = reader.ReadBoolean();
                MLoadInBackground = reader.ReadBoolean();
                MLegacy3D = reader.ReadBoolean();
                reader.AlignStream();

                //StreamedResource m_Resource
                MSource = reader.ReadAlignedString();
                MOffset = reader.ReadInt64();
                MSize = reader.ReadInt64();
                MCompressionFormat = (AudioCompressionFormat)reader.ReadInt32();
            }

            ResourceReader resourceReader;
            if (!string.IsNullOrEmpty(MSource))
            {
                resourceReader = new ResourceReader(MSource, AssetsFile, MOffset, MSize);
            }
            else
            {
                resourceReader = new ResourceReader(reader, reader.BaseStream.Position, MSize);
            }
            MAudioData = resourceReader;
        }
    }

    public enum FMODSoundType
    {
        Unknown = 0,
        Acc = 1,
        Aiff = 2,
        Asf = 3,
        At3 = 4,
        Cdda = 5,
        Dls = 6,
        Flac = 7,
        Fsb = 8,
        Gcadpcm = 9,
        It = 10,
        Midi = 11,
        Mod = 12,
        Mpeg = 13,
        Oggvorbis = 14,
        Playlist = 15,
        Raw = 16,
        S3M = 17,
        Sf2 = 18,
        User = 19,
        Wav = 20,
        Xm = 21,
        Xma = 22,
        Vag = 23,
        Audioqueue = 24,
        Xwma = 25,
        Bcwav = 26,
        At9 = 27,
        Vorbis = 28,
        MediaFoundation = 29
    }

    public enum AudioCompressionFormat
    {
        PCM = 0,
        Vorbis = 1,
        Adpcm = 2,
        Mp3 = 3,
        Psmvag = 4,
        Hevag = 5,
        Xma = 6,
        Aac = 7,
        Gcadpcm = 8,
        Atrac9 = 9
    }
}
