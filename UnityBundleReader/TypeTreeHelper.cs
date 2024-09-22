using System.Collections.Specialized;
using System.Text;
using UnityBundleReader.Extensions;

namespace UnityBundleReader;

public static class TypeTreeHelper
{
    public static string ReadTypeString(TypeTree mType, ObjectReader reader)
    {
        reader.Reset();
        StringBuilder sb = new();
        List<TypeTreeNode> mNodes = mType.Nodes;
        for (int i = 0; i < mNodes.Count; i++)
        {
            ReadStringValue(sb, mNodes, reader, ref i);
        }
        long readed = reader.Position - reader.ByteStart;
        if (readed != reader.ByteSize)
        {
            Logger.Info($"Error while read type, read {readed} bytes but expected {reader.ByteSize} bytes");
        }
        return sb.ToString();
    }

    static void ReadStringValue(StringBuilder sb, List<TypeTreeNode> mNodes, BinaryReader reader, ref int i)
    {
        TypeTreeNode mNode = mNodes[i];
        int level = mNode.Level;
        string? varTypeStr = mNode.Type;
        string? varNameStr = mNode.Name;
        object? value = null;
        bool append = true;
        bool align = (mNode.MetaFlag & 0x4000) != 0;
        switch (varTypeStr)
        {
            case "SInt8":
                value = reader.ReadSByte();
                break;
            case "UInt8":
                value = reader.ReadByte();
                break;
            case "char":
                value = BitConverter.ToChar(reader.ReadBytes(2), 0);
                break;
            case "short":
            case "SInt16":
                value = reader.ReadInt16();
                break;
            case "UInt16":
            case "unsigned short":
                value = reader.ReadUInt16();
                break;
            case "int":
            case "SInt32":
                value = reader.ReadInt32();
                break;
            case "UInt32":
            case "unsigned int":
            case "Type*":
                value = reader.ReadUInt32();
                break;
            case "long long":
            case "SInt64":
                value = reader.ReadInt64();
                break;
            case "UInt64":
            case "unsigned long long":
            case "FileSize":
                value = reader.ReadUInt64();
                break;
            case "float":
                value = reader.ReadSingle();
                break;
            case "double":
                value = reader.ReadDouble();
                break;
            case "bool":
                value = reader.ReadBoolean();
                break;
            case "string":
                append = false;
                string str = reader.ReadAlignedString();
                sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", new string('\t', level), varTypeStr, varNameStr, str);
                List<TypeTreeNode> toSkip = GetNodes(mNodes, i);
                i += toSkip.Count - 1;
                break;
            case "map":
            {
                if ((mNodes[i + 1].MetaFlag & 0x4000) != 0)
                {
                    align = true;
                }
                append = false;
                sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 1), "Array", "Array");
                int size = reader.ReadInt32();
                sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level + 1), "int", "size", size);
                List<TypeTreeNode> map = GetNodes(mNodes, i);
                i += map.Count - 1;
                List<TypeTreeNode> first = GetNodes(map, 4);
                int next = 4 + first.Count;
                List<TypeTreeNode> second = GetNodes(map, next);
                for (int j = 0; j < size; j++)
                {
                    sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), j);
                    sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 2), "pair", "data");
                    int tmp1 = 0;
                    int tmp2 = 0;
                    ReadStringValue(sb, first, reader, ref tmp1);
                    ReadStringValue(sb, second, reader, ref tmp2);
                }
                break;
            }
            case "TypelessData":
            {
                append = false;
                int size = reader.ReadInt32();
                reader.ReadBytes(size);
                i += 2;
                sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level), "int", "size", size);
                break;
            }
            default:
            {
                if (i < mNodes.Count - 1 && mNodes[i + 1].Type == "Array") //Array
                {
                    if ((mNodes[i + 1].MetaFlag & 0x4000) != 0)
                    {
                        align = true;
                    }
                    append = false;
                    sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                    sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 1), "Array", "Array");
                    int size = reader.ReadInt32();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level + 1), "int", "size", size);
                    List<TypeTreeNode> vector = GetNodes(mNodes, i);
                    i += vector.Count - 1;
                    for (int j = 0; j < size; j++)
                    {
                        sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), j);
                        int tmp = 3;
                        ReadStringValue(sb, vector, reader, ref tmp);
                    }
                    break;
                }
                //Class
                append = false;
                sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                List<TypeTreeNode> @class = GetNodes(mNodes, i);
                i += @class.Count - 1;
                for (int j = 1; j < @class.Count; j++)
                {
                    ReadStringValue(sb, @class, reader, ref j);
                }
                break;
            }
        }
        if (append)
        {
            sb.Append($"{new string('\t', level)}{varTypeStr} {varNameStr} = {value}\r\n");
        }
        if (align)
        {
            reader.AlignStream();
        }
    }

    public static OrderedDictionary ReadType(TypeTree types, ObjectReader reader, IReadOnlyCollection<string>? propertiesToKeep = null)
    {
        reader.Reset();
        OrderedDictionary obj = new();
        List<TypeTreeNode> nodes = types.Nodes;
        for (int i = 1; i < nodes.Count; i++)
        {
            TypeTreeNode node = nodes[i];
            string? varNameStr = node.Name;
            bool keepValue = varNameStr != null && (propertiesToKeep == null || propertiesToKeep.Contains(varNameStr));

            if (keepValue)
            {
                object value = ReadValue(nodes, reader, ref i);
                obj[varNameStr!] = value;
            }
            else
            {
                SkipValue(nodes, reader, ref i);
            }

        }

        long read = reader.Position - reader.ByteStart;
        if (read != reader.ByteSize)
        {
            Logger.Info($"Error while read type, read {read} bytes but expected {reader.ByteSize} bytes");
        }
        return obj;
    }

    static object ReadValue(List<TypeTreeNode> mNodes, BinaryReader reader, ref int i)
    {
        TypeTreeNode mNode = mNodes[i];
        string? varTypeStr = mNode.Type;
        object value;
        bool align = (mNode.MetaFlag & 0x4000) != 0;
        switch (varTypeStr)
        {
            case "SInt8":
                value = reader.ReadSByte();
                break;
            case "UInt8":
                value = reader.ReadByte();
                break;
            case "char":
                value = BitConverter.ToChar(reader.ReadBytes(2), 0);
                break;
            case "short":
            case "SInt16":
                value = reader.ReadInt16();
                break;
            case "UInt16":
            case "unsigned short":
                value = reader.ReadUInt16();
                break;
            case "int":
            case "SInt32":
                value = reader.ReadInt32();
                break;
            case "UInt32":
            case "unsigned int":
            case "Type*":
                value = reader.ReadUInt32();
                break;
            case "long long":
            case "SInt64":
                value = reader.ReadInt64();
                break;
            case "UInt64":
            case "unsigned long long":
            case "FileSize":
                value = reader.ReadUInt64();
                break;
            case "float":
                value = reader.ReadSingle();
                break;
            case "double":
                value = reader.ReadDouble();
                break;
            case "bool":
                value = reader.ReadBoolean();
                break;
            case "string":
                value = reader.ReadAlignedString();
                List<TypeTreeNode> toSkip = GetNodes(mNodes, i);
                i += toSkip.Count - 1;
                break;
            case "map":
            {
                if ((mNodes[i + 1].MetaFlag & 0x4000) != 0)
                {
                    align = true;
                }
                List<TypeTreeNode> map = GetNodes(mNodes, i);
                i += map.Count - 1;
                List<TypeTreeNode> first = GetNodes(map, 4);
                int next = 4 + first.Count;
                List<TypeTreeNode> second = GetNodes(map, next);
                int size = reader.ReadInt32();
                List<KeyValuePair<object, object>> dic = new(size);
                for (int j = 0; j < size; j++)
                {
                    int tmp1 = 0;
                    int tmp2 = 0;
                    dic.Add(new KeyValuePair<object, object>(ReadValue(first, reader, ref tmp1), ReadValue(second, reader, ref tmp2)));
                }
                value = dic;
                break;
            }
            case "TypelessData":
            {
                int size = reader.ReadInt32();
                value = reader.ReadBytes(size);
                i += 2;
                break;
            }
            default:
            {
                if (i < mNodes.Count - 1 && mNodes[i + 1].Type == "Array") //Array
                {
                    if ((mNodes[i + 1].MetaFlag & 0x4000) != 0)
                    {
                        align = true;
                    }
                    List<TypeTreeNode> vector = GetNodes(mNodes, i);
                    i += vector.Count - 1;
                    int size = reader.ReadInt32();
                    List<object> list = new(size);
                    for (int j = 0; j < size; j++)
                    {
                        int tmp = 3;
                        list.Add(ReadValue(vector, reader, ref tmp));
                    }
                    value = list;
                    break;
                }
                //Class
                List<TypeTreeNode> @class = GetNodes(mNodes, i);
                i += @class.Count - 1;
                OrderedDictionary obj = new();
                for (int j = 1; j < @class.Count; j++)
                {
                    TypeTreeNode classmember = @class[j];
                    string? name = classmember.Name;
                    object classMemberValue = ReadValue(@class, reader, ref j);

                    if (name != null)
                    {
                        obj[name] = classMemberValue;
                    }
                    else
                    {
                        Logger.Warning($"Class member {classmember} is unnamed.");
                    }
                }
                value = obj;
                break;
            }
        }
        if (align)
        {
            reader.AlignStream();
        }
        return value;
    }

    static void SkipValue(List<TypeTreeNode> mNodes, BinaryReader reader, ref int i)
    {
        TypeTreeNode mNode = mNodes[i];
        string? varTypeStr = mNode.Type;
        bool align = (mNode.MetaFlag & 0x4000) != 0;
        switch (varTypeStr)
        {
            case "SInt8":
                reader.BaseStream.Position += 1;
                break;
            case "UInt8":
                reader.BaseStream.Position += 1;
                break;
            case "char":
                reader.BaseStream.Position += 2;
                break;
            case "short":
            case "SInt16":
                reader.BaseStream.Position += 2;
                break;
            case "UInt16":
            case "unsigned short":
                reader.BaseStream.Position += 2;
                break;
            case "int":
            case "SInt32":
                reader.BaseStream.Position += 4;
                break;
            case "UInt32":
            case "unsigned int":
            case "Type*":
                reader.BaseStream.Position += 4;
                break;
            case "long long":
            case "SInt64":
                reader.BaseStream.Position += 8;
                break;
            case "UInt64":
            case "unsigned long long":
            case "FileSize":
                reader.BaseStream.Position += 8;
                break;
            case "float":
                reader.BaseStream.Position += 4;
                break;
            case "double":
                reader.BaseStream.Position += 8;
                break;
            case "bool":
                reader.BaseStream.Position += 1;
                break;
            case "string":
                reader.ReadAlignedString();
                List<TypeTreeNode> toSkip = GetNodes(mNodes, i);
                i += toSkip.Count - 1;
                break;
            case "map":
            {
                if ((mNodes[i + 1].MetaFlag & 0x4000) != 0)
                {
                    align = true;
                }
                List<TypeTreeNode> map = GetNodes(mNodes, i);
                i += map.Count - 1;
                List<TypeTreeNode> first = GetNodes(map, 4);
                int next = 4 + first.Count;
                List<TypeTreeNode> second = GetNodes(map, next);
                int size = reader.ReadInt32();
                for (int j = 0; j < size; j++)
                {
                    int tmp1 = 0;
                    int tmp2 = 0;
                    SkipValue(first, reader, ref tmp1);
                    SkipValue(second, reader, ref tmp2);
                }
                break;
            }
            case "TypelessData":
            {
                int size = reader.ReadInt32();
                reader.BaseStream.Position += size;
                i += 2;
                break;
            }
            default:
            {
                if (i < mNodes.Count - 1 && mNodes[i + 1].Type == "Array") //Array
                {
                    if ((mNodes[i + 1].MetaFlag & 0x4000) != 0)
                    {
                        align = true;
                    }
                    List<TypeTreeNode> vector = GetNodes(mNodes, i);
                    i += vector.Count - 1;
                    int size = reader.ReadInt32();
                    for (int j = 0; j < size; j++)
                    {
                        int tmp = 3;
                        SkipValue(vector, reader, ref tmp);
                    }
                    break;
                }
                //Class
                List<TypeTreeNode> @class = GetNodes(mNodes, i);
                i += @class.Count - 1;
                for (int j = 1; j < @class.Count; j++)
                {
                    SkipValue(@class, reader, ref j);
                }
                break;
            }
        }
        if (align)
        {
            reader.AlignStream();
        }
    }

    static List<TypeTreeNode> GetNodes(List<TypeTreeNode> mNodes, int index)
    {
        List<TypeTreeNode> nodes =
        [
            mNodes[index]
        ];
        int level = mNodes[index].Level;
        for (int i = index + 1; i < mNodes.Count; i++)
        {
            TypeTreeNode member = mNodes[i];
            int level2 = member.Level;
            if (level2 <= level)
            {
                return nodes;
            }
            nodes.Add(member);
        }
        return nodes;
    }
}
