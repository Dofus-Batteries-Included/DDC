using UnityBundleReader._7zip.Compress.LZMA;

namespace UnityBundleReader;

public static class SevenZipHelper
{
    public static MemoryStream StreamDecompress(MemoryStream inStream)
    {
        Decoder decoder = new();

        inStream.Seek(0, SeekOrigin.Begin);
        MemoryStream newOutStream = new();

        byte[] properties = new byte[5];
        if (inStream.Read(properties, 0, 5) != 5)
        {
            throw new Exception("input .lzma is too short");
        }
        long outSize = 0;
        for (int i = 0; i < 8; i++)
        {
            int v = inStream.ReadByte();
            if (v < 0)
            {
                throw new Exception("Can't Read 1");
            }
            outSize |= (long)(byte)v<<8 * i;
        }
        decoder.SetDecoderProperties(properties);

        long compressedSize = inStream.Length - inStream.Position;
        decoder.Code(inStream, newOutStream, compressedSize, outSize, null);

        newOutStream.Position = 0;
        return newOutStream;
    }

    public static void StreamDecompress(Stream compressedStream, Stream decompressedStream, long compressedSize, long decompressedSize)
    {
        long basePosition = compressedStream.Position;
        Decoder decoder = new();
        byte[] properties = new byte[5];
        if (compressedStream.Read(properties, 0, 5) != 5)
        {
            throw new Exception("input .lzma is too short");
        }
        decoder.SetDecoderProperties(properties);
        decoder.Code(compressedStream, decompressedStream, compressedSize - 5, decompressedSize, null);
        compressedStream.Position = basePosition + compressedSize;
    }
}
