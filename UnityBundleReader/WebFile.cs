using System.Text;
using UnityBundleReader.Extensions;

namespace UnityBundleReader
{
    public class WebFile
    {
        public StreamFile[] FileList;

        private class WebData
        {
            public int DataOffset;
            public int DataLength;
            public string Path;
        }

        public WebFile(EndianBinaryReader reader)
        {
            reader.Endian = EndianType.LittleEndian;
            var signature = reader.ReadStringToNull();
            var headLength = reader.ReadInt32();
            var dataList = new List<WebData>();
            while (reader.BaseStream.Position < headLength)
            {
                var data = new WebData();
                data.DataOffset = reader.ReadInt32();
                data.DataLength = reader.ReadInt32();
                var pathLength = reader.ReadInt32();
                data.Path = Encoding.UTF8.GetString(reader.ReadBytes(pathLength));
                dataList.Add(data);
            }
            FileList = new StreamFile[dataList.Count];
            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                var file = new StreamFile();
                file.path = data.Path;
                file.fileName = Path.GetFileName(data.Path);
                reader.BaseStream.Position = data.DataOffset;
                file.stream = new MemoryStream(reader.ReadBytes(data.DataLength));
                FileList[i] = file;
            }
        }
    }
}
