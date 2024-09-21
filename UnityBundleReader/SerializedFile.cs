using System.Text.RegularExpressions;
using UnityBundleReader.Extensions;
using Object = UnityBundleReader.Classes.Object;

namespace UnityBundleReader;

public class SerializedFile
{
    public readonly AssetsManager AssetsManager;
    public readonly FileReader Reader;
    public readonly string FullName;
    public string? OriginalPath;
    public readonly string FileName;
    public int[] Version = [0, 0, 0, 0];
    public BuildType? BuildType;
    public readonly List<Object> Objects;
    public readonly Dictionary<long, Object> ObjectsDic;

    public readonly SerializedFileHeader Header;
    public string UnityVersion = "2.5.0f5";
    public readonly BuildTarget MTargetPlatform = BuildTarget.UnknownPlatform;
    readonly bool _mEnableTypeTree = true;
    public readonly List<SerializedType> MTypes;
    public readonly int BigIDEnabled;
    public readonly List<ObjectInfo> ObjectInfos;
    public readonly List<FileIdentifier> Externals;
    public readonly List<SerializedType> RefTypes = [];
    public string? UserInformation;

    public SerializedFile(FileReader reader, AssetsManager assetsManager)
    {
        byte fileEndianess;
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
            Header.Reserved = reader.ReadBytes(3);
            fileEndianess = Header.MEndianess;
        }
        else
        {
            reader.Position = Header.MFileSize - Header.MMetadataSize;
            fileEndianess = reader.ReadByte();
        }

        if (Header.MVersion >= SerializedFileFormatVersion.LargeFilesSupport)
        {
            Header.MMetadataSize = reader.ReadUInt32();
            Header.MFileSize = reader.ReadInt64();
            Header.MDataOffset = reader.ReadInt64();
            reader.ReadInt64(); // unknown
        }

        // ReadMetadata
        if (fileEndianess == 0)
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
        ObjectInfos = new List<ObjectInfo>(objectCount);
        Objects = new List<Object>(objectCount);
        ObjectsDic = new Dictionary<long, Object>(objectCount);
        for (int i = 0; i < objectCount; i++)
        {
            ObjectInfo objectInfo = new();
            if (BigIDEnabled != 0)
            {
                objectInfo.PathId = reader.ReadInt64();
            }
            else if (Header.MVersion < SerializedFileFormatVersion.Unknown14)
            {
                objectInfo.PathId = reader.ReadInt32();
            }
            else
            {
                reader.AlignStream();
                objectInfo.PathId = reader.ReadInt64();
            }

            if (Header.MVersion >= SerializedFileFormatVersion.LargeFilesSupport)
            {
                objectInfo.ByteStart = reader.ReadInt64();
            }
            else
            {
                objectInfo.ByteStart = reader.ReadUInt32();
            }

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
                SerializedType type = MTypes[objectInfo.TypeID];
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
                {
                    objectInfo.SerializedType.ScriptTypeIndex = mScriptTypeIndex;
                }
            }
            if (Header.MVersion == SerializedFileFormatVersion.SupportsStrippedObject || Header.MVersion == SerializedFileFormatVersion.RefactoredClassId)
            {
                objectInfo.Stripped = reader.ReadByte();
            }
            ObjectInfos.Add(objectInfo);
        }

        if (Header.MVersion >= SerializedFileFormatVersion.HasScriptTypeIndex)
        {
            int scriptCount = reader.ReadInt32();
            List<LocalSerializedObjectIdentifier> scriptTypes = new(scriptCount);
            for (int i = 0; i < scriptCount; i++)
            {
                LocalSerializedObjectIdentifier mScriptType = new();
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
                scriptTypes.Add(mScriptType);
            }
        }

        int externalsCount = reader.ReadInt32();
        Externals = new List<FileIdentifier>(externalsCount);
        for (int i = 0; i < externalsCount; i++)
        {
            Guid guid = default;
            int type = default;

            if (Header.MVersion >= SerializedFileFormatVersion.Unknown6)
            {
                _ = reader.ReadStringToNull();
            }

            if (Header.MVersion >= SerializedFileFormatVersion.Unknown5)
            {
                guid = new Guid(reader.ReadBytes(16));
                type = reader.ReadInt32();
            }

            string pathName = reader.ReadStringToNull();
            string fileName = Path.GetFileName(pathName);

            FileIdentifier external = new() { Guid = guid, Type = type, PathName = pathName, FileName = fileName };
            Externals.Add(external);
        }

        if (Header.MVersion >= SerializedFileFormatVersion.SupportsRefObject)
        {
            int refTypesCount = reader.ReadInt32();
            RefTypes = new List<SerializedType>(refTypesCount);
            for (int i = 0; i < refTypesCount; i++)
            {
                RefTypes.Add(ReadSerializedType(true));
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
            string[] buildSplit = Regex.Replace(stringVersion, @"\d", "").Split(["."], StringSplitOptions.RemoveEmptyEntries);
            BuildType = new BuildType(buildSplit[0]);
            string[] versionSplit = Regex.Replace(stringVersion, @"\D", ".").Split(["."], StringSplitOptions.RemoveEmptyEntries);
            Version = versionSplit.Select(int.Parse).ToArray();
        }
    }

    SerializedType ReadSerializedType(bool isRefType)
    {
        int typeClassID = Reader.ReadInt32();


        bool typeIsStrippedType = false;
        if (Header.MVersion >= SerializedFileFormatVersion.RefactoredClassId)
        {
            typeIsStrippedType = Reader.ReadBoolean();
        }


        short typeScriptTypeIndex = 0;
        if (Header.MVersion >= SerializedFileFormatVersion.RefactorTypeData)
        {
            typeScriptTypeIndex = Reader.ReadInt16();
        }


        byte[]? typeScriptId = null;
        byte[]? typeOldTypeHash = null;
        if (Header.MVersion >= SerializedFileFormatVersion.HasTypeTreeHashes)
        {
            if (isRefType && typeScriptTypeIndex >= 0)
            {
                typeScriptId = Reader.ReadBytes(16);
            }
            else if (Header.MVersion < SerializedFileFormatVersion.RefactoredClassId && typeClassID < 0
                     || Header.MVersion >= SerializedFileFormatVersion.RefactoredClassId && typeClassID == 114)
            {
                typeScriptId = Reader.ReadBytes(16);
            }
            typeOldTypeHash = Reader.ReadBytes(16);
        }

        TypeTree? typeTree = null;
        string? typeClassName = null;
        string? typeNamespace = null;
        string? typeAsmName = null;
        int[]? typeDependencies = null;
        if (_mEnableTypeTree)
        {
            typeTree = new TypeTree
            {
                Nodes = []
            };

            if (Header.MVersion is >= SerializedFileFormatVersion.Unknown12 or SerializedFileFormatVersion.Unknown10)
            {
                TypeTreeBlobRead(typeTree);
            }
            else
            {
                ReadTypeTree(typeTree);
            }

            if (Header.MVersion >= SerializedFileFormatVersion.StoresTypeDependencies)
            {
                if (isRefType)
                {
                    typeClassName = Reader.ReadStringToNull();
                    typeNamespace = Reader.ReadStringToNull();
                    typeAsmName = Reader.ReadStringToNull();
                }
                else
                {
                    typeDependencies = Reader.ReadInt32Array();
                }
            }
        }

        return new SerializedType
        {
            ClassID = typeClassID,
            ClassName = typeClassName,
            Namespace = typeNamespace,
            AsmName = typeAsmName,
            IsStrippedType = typeIsStrippedType,
            ScriptTypeIndex = typeScriptTypeIndex,
            ScriptId = typeScriptId,
            OldTypeHash = typeOldTypeHash,
            Type = typeTree,
            TypeDependencies = typeDependencies
        };
    }

    void ReadTypeTree(TypeTree mType, int level = 0)
    {
        TypeTreeNode typeTreeNode = new();
        mType.Nodes.Add(typeTreeNode);
        typeTreeNode.Level = level;
        typeTreeNode.Type = Reader.ReadStringToNull();
        typeTreeNode.Name = Reader.ReadStringToNull();
        typeTreeNode.ByteSize = Reader.ReadInt32();
        if (Header.MVersion == SerializedFileFormatVersion.Unknown2)
        {
            int variableCount = Reader.ReadInt32();
        }
        if (Header.MVersion != SerializedFileFormatVersion.Unknown3)
        {
            typeTreeNode.Index = Reader.ReadInt32();
        }
        typeTreeNode.TypeFlags = Reader.ReadInt32();
        typeTreeNode.Version = Reader.ReadInt32();
        if (Header.MVersion != SerializedFileFormatVersion.Unknown3)
        {
            typeTreeNode.MetaFlag = Reader.ReadInt32();
        }

        int childrenCount = Reader.ReadInt32();
        for (int i = 0; i < childrenCount; i++)
        {
            ReadTypeTree(mType, level + 1);
        }
    }

    void TypeTreeBlobRead(TypeTree mType)
    {
        int numberOfNodes = Reader.ReadInt32();
        int stringBufferSize = Reader.ReadInt32();
        for (int i = 0; i < numberOfNodes; i++)
        {
            TypeTreeNode typeTreeNode = new();
            mType.Nodes.Add(typeTreeNode);
            typeTreeNode.Version = Reader.ReadUInt16();
            typeTreeNode.Level = Reader.ReadByte();
            typeTreeNode.TypeFlags = Reader.ReadByte();
            typeTreeNode.TypeStrOffset = Reader.ReadUInt32();
            typeTreeNode.NameStrOffset = Reader.ReadUInt32();
            typeTreeNode.ByteSize = Reader.ReadInt32();
            typeTreeNode.Index = Reader.ReadInt32();
            typeTreeNode.MetaFlag = Reader.ReadInt32();
            if (Header.MVersion >= SerializedFileFormatVersion.TypeTreeNodeWithTypeFlags)
            {
                typeTreeNode.RefTypeHash = Reader.ReadUInt64();
            }
        }
        mType.StringBuffer = Reader.ReadBytes(stringBufferSize);

        using (BinaryReader stringBufferReader = new(new MemoryStream(mType.StringBuffer)))
        {
            for (int i = 0; i < numberOfNodes; i++)
            {
                TypeTreeNode mNode = mType.Nodes[i];
                mNode.Type = ReadString(stringBufferReader, mNode.TypeStrOffset);
                mNode.Name = ReadString(stringBufferReader, mNode.NameStrOffset);
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
        ObjectsDic.Add(obj.PathId, obj);
    }

    public bool IsVersionStripped => UnityVersion == StrippedVersion;

    const string StrippedVersion = "0.0.0";
}
