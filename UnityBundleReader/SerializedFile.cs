using System.Text.RegularExpressions;
using UnityBundleReader.Extensions;
using Object = UnityBundleReader.Classes.Object;

namespace UnityBundleReader
{
    public class SerializedFile
    {
        public AssetsManager AssetsManager;
        public FileReader Reader;
        public string FullName;
        public string OriginalPath;
        public string FileName;
        public int[] Version = { 0, 0, 0, 0 };
        public BuildType BuildType;
        public List<Object> Objects;
        public Dictionary<long, Object> ObjectsDic;

        public SerializedFileHeader Header;
        private byte _mFileEndianess;
        public string UnityVersion = "2.5.0f5";
        public BuildTarget MTargetPlatform = BuildTarget.UnknownPlatform;
        private bool _mEnableTypeTree = true;
        public List<SerializedType> MTypes;
        public int BigIDEnabled = 0;
        public List<ObjectInfo> MObjects;
        private List<LocalSerializedObjectIdentifier> _mScriptTypes;
        public List<FileIdentifier> MExternals;
        public List<SerializedType> MRefTypes;
        public string UserInformation;

        public SerializedFile(FileReader reader, AssetsManager assetsManager)
        {
            AssetsManager = assetsManager;
            Reader = reader;
            FullName = reader.FullPath;
            FileName = reader.FileName;

            // ReadHeader
            Header = new SerializedFileHeader();
            Header.MMetadataSize = reader.ReadUInt32();
            Header.MFileSize = reader.ReadUInt32();
            Header.MVersion = (SerializedFileFormatVersion)reader.ReadUInt32();
            Header.MDataOffset = reader.ReadUInt32();

            if (Header.MVersion >= SerializedFileFormatVersion.Unknown9)
            {
                Header.MEndianess = reader.ReadByte();
                Header.MReserved = reader.ReadBytes(3);
                _mFileEndianess = Header.MEndianess;
            }
            else
            {
                reader.Position = Header.MFileSize - Header.MMetadataSize;
                _mFileEndianess = reader.ReadByte();
            }

            if (Header.MVersion >= SerializedFileFormatVersion.LargeFilesSupport)
            {
                Header.MMetadataSize = reader.ReadUInt32();
                Header.MFileSize = reader.ReadInt64();
                Header.MDataOffset = reader.ReadInt64();
                reader.ReadInt64(); // unknown
            }

            // ReadMetadata
            if (_mFileEndianess == 0)
            {
                reader.Endian = EndianType.LittleEndian;
            }
            if (Header.MVersion >= SerializedFileFormatVersion.Unknown7)
            {
                UnityVersion = reader.ReadStringToNull();
                SetVersion(UnityVersion);
            }
            if (Header.MVersion >= SerializedFileFormatVersion.Unknown8)
            {
                MTargetPlatform = (BuildTarget)reader.ReadInt32();
                if (!Enum.IsDefined(typeof(BuildTarget), MTargetPlatform))
                {
                    MTargetPlatform = BuildTarget.UnknownPlatform;
                }
            }
            if (Header.MVersion >= SerializedFileFormatVersion.HasTypeTreeHashes)
            {
                _mEnableTypeTree = reader.ReadBoolean();
            }

            // Read Types
            int typeCount = reader.ReadInt32();
            MTypes = new List<SerializedType>(typeCount);
            for (int i = 0; i < typeCount; i++)
            {
                MTypes.Add(ReadSerializedType(false));
            }

            if (Header.MVersion >= SerializedFileFormatVersion.Unknown7 && Header.MVersion < SerializedFileFormatVersion.Unknown14)
            {
                BigIDEnabled = reader.ReadInt32();
            }

            // Read Objects
            int objectCount = reader.ReadInt32();
            MObjects = new List<ObjectInfo>(objectCount);
            Objects = new List<Object>(objectCount);
            ObjectsDic = new Dictionary<long, Object>(objectCount);
            for (int i = 0; i < objectCount; i++)
            {
                ObjectInfo? objectInfo = new ObjectInfo();
                if (BigIDEnabled != 0)
                {
                    objectInfo.MPathID = reader.ReadInt64();
                }
                else if (Header.MVersion < SerializedFileFormatVersion.Unknown14)
                {
                    objectInfo.MPathID = reader.ReadInt32();
                }
                else
                {
                    reader.AlignStream();
                    objectInfo.MPathID = reader.ReadInt64();
                }

                if (Header.MVersion >= SerializedFileFormatVersion.LargeFilesSupport)
                    objectInfo.ByteStart = reader.ReadInt64();
                else
                    objectInfo.ByteStart = reader.ReadUInt32();

                objectInfo.ByteStart += Header.MDataOffset;
                objectInfo.ByteSize = reader.ReadUInt32();
                objectInfo.TypeID = reader.ReadInt32();
                if (Header.MVersion < SerializedFileFormatVersion.RefactoredClassId)
                {
                    objectInfo.ClassID = reader.ReadUInt16();
                    objectInfo.SerializedType = MTypes.Find(x => x.ClassID == objectInfo.TypeID);
                }
                else
                {
                    SerializedType? type = MTypes[objectInfo.TypeID];
                    objectInfo.SerializedType = type;
                    objectInfo.ClassID = type.ClassID;
                }
                if (Header.MVersion < SerializedFileFormatVersion.HasScriptTypeIndex)
                {
                    objectInfo.IsDestroyed = reader.ReadUInt16();
                }
                if (Header.MVersion >= SerializedFileFormatVersion.HasScriptTypeIndex && Header.MVersion < SerializedFileFormatVersion.RefactorTypeData)
                {
                    short mScriptTypeIndex = reader.ReadInt16();
                    if (objectInfo.SerializedType != null)
                        objectInfo.SerializedType.MScriptTypeIndex = mScriptTypeIndex;
                }
                if (Header.MVersion == SerializedFileFormatVersion.SupportsStrippedObject || Header.MVersion == SerializedFileFormatVersion.RefactoredClassId)
                {
                    objectInfo.Stripped = reader.ReadByte();
                }
                MObjects.Add(objectInfo);
            }

            if (Header.MVersion >= SerializedFileFormatVersion.HasScriptTypeIndex)
            {
                int scriptCount = reader.ReadInt32();
                _mScriptTypes = new List<LocalSerializedObjectIdentifier>(scriptCount);
                for (int i = 0; i < scriptCount; i++)
                {
                    LocalSerializedObjectIdentifier? mScriptType = new LocalSerializedObjectIdentifier();
                    mScriptType.LocalSerializedFileIndex = reader.ReadInt32();
                    if (Header.MVersion < SerializedFileFormatVersion.Unknown14)
                    {
                        mScriptType.LocalIdentifierInFile = reader.ReadInt32();
                    }
                    else
                    {
                        reader.AlignStream();
                        mScriptType.LocalIdentifierInFile = reader.ReadInt64();
                    }
                    _mScriptTypes.Add(mScriptType);
                }
            }

            int externalsCount = reader.ReadInt32();
            MExternals = new List<FileIdentifier>(externalsCount);
            for (int i = 0; i < externalsCount; i++)
            {
                FileIdentifier? mExternal = new FileIdentifier();
                if (Header.MVersion >= SerializedFileFormatVersion.Unknown6)
                {
                    string? tempEmpty = reader.ReadStringToNull();
                }
                if (Header.MVersion >= SerializedFileFormatVersion.Unknown5)
                {
                    mExternal.Guid = new Guid(reader.ReadBytes(16));
                    mExternal.Type = reader.ReadInt32();
                }
                mExternal.PathName = reader.ReadStringToNull();
                mExternal.FileName = Path.GetFileName(mExternal.PathName);
                MExternals.Add(mExternal);
            }

            if (Header.MVersion >= SerializedFileFormatVersion.SupportsRefObject)
            {
                int refTypesCount = reader.ReadInt32();
                MRefTypes = new List<SerializedType>(refTypesCount);
                for (int i = 0; i < refTypesCount; i++)
                {
                    MRefTypes.Add(ReadSerializedType(true));
                }
            }

            if (Header.MVersion >= SerializedFileFormatVersion.Unknown5)
            {
                UserInformation = reader.ReadStringToNull();
            }

            //reader.AlignStream(16);
        }

        public void SetVersion(string stringVersion)
        {
            if (stringVersion != StrippedVersion)
            {
                UnityVersion = stringVersion;
                string[]? buildSplit = Regex.Replace(stringVersion, @"\d", "").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                BuildType = new BuildType(buildSplit[0]);
                string[]? versionSplit = Regex.Replace(stringVersion, @"\D", ".").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                Version = versionSplit.Select(int.Parse).ToArray();
            }
        }

        private SerializedType ReadSerializedType(bool isRefType)
        {
            SerializedType? type = new SerializedType();

            type.ClassID = Reader.ReadInt32();

            if (Header.MVersion >= SerializedFileFormatVersion.RefactoredClassId)
            {
                type.MIsStrippedType = Reader.ReadBoolean();
            }

            if (Header.MVersion >= SerializedFileFormatVersion.RefactorTypeData)
            {
                type.MScriptTypeIndex = Reader.ReadInt16();
            }

            if (Header.MVersion >= SerializedFileFormatVersion.HasTypeTreeHashes)
            {
                if (isRefType && type.MScriptTypeIndex >= 0)
                {
                    type.MScriptID = Reader.ReadBytes(16);
                }
                else if ((Header.MVersion < SerializedFileFormatVersion.RefactoredClassId && type.ClassID < 0) || (Header.MVersion >= SerializedFileFormatVersion.RefactoredClassId && type.ClassID == 114))
                {
                    type.MScriptID = Reader.ReadBytes(16);
                }
                type.MOldTypeHash = Reader.ReadBytes(16);
            }

            if (_mEnableTypeTree)
            {
                type.MType = new TypeTree();
                type.MType.m_Nodes = new List<TypeTreeNode>();
                if (Header.MVersion >= SerializedFileFormatVersion.Unknown12 || Header.MVersion == SerializedFileFormatVersion.Unknown10)
                {
                    TypeTreeBlobRead(type.MType);
                }
                else
                {
                    ReadTypeTree(type.MType);
                }
                if (Header.MVersion >= SerializedFileFormatVersion.StoresTypeDependencies)
                {
                    if (isRefType)
                    {
                        type.MKlassName = Reader.ReadStringToNull();
                        type.MNameSpace = Reader.ReadStringToNull();
                        type.MAsmName = Reader.ReadStringToNull();
                    }
                    else
                    {
                        type.MTypeDependencies = Reader.ReadInt32Array();
                    }
                }
            }

            return type;
        }

        private void ReadTypeTree(TypeTree mType, int level = 0)
        {
            TypeTreeNode? typeTreeNode = new TypeTreeNode();
            mType.m_Nodes.Add(typeTreeNode);
            typeTreeNode.MLevel = level;
            typeTreeNode.MType = Reader.ReadStringToNull();
            typeTreeNode.MName = Reader.ReadStringToNull();
            typeTreeNode.MByteSize = Reader.ReadInt32();
            if (Header.MVersion == SerializedFileFormatVersion.Unknown2)
            {
                int variableCount = Reader.ReadInt32();
            }
            if (Header.MVersion != SerializedFileFormatVersion.Unknown3)
            {
                typeTreeNode.MIndex = Reader.ReadInt32();
            }
            typeTreeNode.MTypeFlags = Reader.ReadInt32();
            typeTreeNode.MVersion = Reader.ReadInt32();
            if (Header.MVersion != SerializedFileFormatVersion.Unknown3)
            {
                typeTreeNode.MMetaFlag = Reader.ReadInt32();
            }

            int childrenCount = Reader.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
            {
                ReadTypeTree(mType, level + 1);
            }
        }

        private void TypeTreeBlobRead(TypeTree mType)
        {
            int numberOfNodes = Reader.ReadInt32();
            int stringBufferSize = Reader.ReadInt32();
            for (int i = 0; i < numberOfNodes; i++)
            {
                TypeTreeNode? typeTreeNode = new TypeTreeNode();
                mType.m_Nodes.Add(typeTreeNode);
                typeTreeNode.MVersion = Reader.ReadUInt16();
                typeTreeNode.MLevel = Reader.ReadByte();
                typeTreeNode.MTypeFlags = Reader.ReadByte();
                typeTreeNode.MTypeStrOffset = Reader.ReadUInt32();
                typeTreeNode.MNameStrOffset = Reader.ReadUInt32();
                typeTreeNode.MByteSize = Reader.ReadInt32();
                typeTreeNode.MIndex = Reader.ReadInt32();
                typeTreeNode.MMetaFlag = Reader.ReadInt32();
                if (Header.MVersion >= SerializedFileFormatVersion.TypeTreeNodeWithTypeFlags)
                {
                    typeTreeNode.MRefTypeHash = Reader.ReadUInt64();
                }
            }
            mType.m_StringBuffer = Reader.ReadBytes(stringBufferSize);

            using (BinaryReader? stringBufferReader = new BinaryReader(new MemoryStream(mType.m_StringBuffer)))
            {
                for (int i = 0; i < numberOfNodes; i++)
                {
                    TypeTreeNode? mNode = mType.m_Nodes[i];
                    mNode.MType = ReadString(stringBufferReader, mNode.MTypeStrOffset);
                    mNode.MName = ReadString(stringBufferReader, mNode.MNameStrOffset);
                }
            }

            string ReadString(BinaryReader stringBufferReader, uint value)
            {
                bool isOffset = (value & 0x80000000) == 0;
                if (isOffset)
                {
                    stringBufferReader.BaseStream.Position = value;
                    return stringBufferReader.ReadStringToNull();
                }
                uint offset = value & 0x7FFFFFFF;
                if (CommonString.StringBuffer.TryGetValue(offset, out string? str))
                {
                    return str;
                }
                return offset.ToString();
            }
        }

        public void AddObject(Object obj)
        {
            Objects.Add(obj);
            ObjectsDic.Add(obj.MPathID, obj);
        }

        public bool IsVersionStripped => UnityVersion == StrippedVersion;

        private const string StrippedVersion = "0.0.0";
    }
}
