using System.Text;
using UnityBundleReader.Extensions;

namespace UnityBundleReader;

public class WebFile
{
    public readonly StreamFile[] FileList;

    class WebData
    {
        public required int DataOffset;
        public required int DataLength;
        public required string Path;
    }

    public WebFile(EndianBinaryReader reader)
    {
        reader.Endian = EndianType.LittleEndian;
        string signature = reader.ReadStringToNull();
        int headLength = reader.ReadInt32();
        List<WebData> dataList = new();
        while (reader.BaseStream.Position < headLength)
        {
            int dataOffset = reader.ReadInt32();
            int dataLength = reader.ReadInt32();
            int pathLength = reader.ReadInt32();
            string path = Encoding.UTF8.GetString(reader.ReadBytes(pathLength));
            dataList.Add(new WebData { DataOffset = dataOffset, DataLength = dataLength, Path = path });
        }
        FileList = new StreamFile[dataList.Count];
        for (int i = 0; i < dataList.Count; i++)
        {
            WebData data = dataList[i];
            StreamFile file = new();
            file.path = data.Path;
            file.fileName = Path.GetFileName(data.Path);
            reader.BaseStream.Position = data.DataOffset;
            file.stream = new MemoryStream(reader.ReadBytes(data.DataLength));
            FileList[i] = file;
        }
    }
}
