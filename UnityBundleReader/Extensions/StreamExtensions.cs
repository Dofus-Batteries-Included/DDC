namespace UnityBundleReader.Extensions;

public static class StreamExtensions
{
    const int BufferSize = 81920;

    public static void CopyTo(this Stream source, Stream destination, long size)
    {
        byte[] buffer = new byte[BufferSize];
        for (long left = size; left > 0; left -= BufferSize)
        {
            int toRead = BufferSize < left ? BufferSize : (int)left;
            int read = source.Read(buffer, 0, toRead);
            destination.Write(buffer, 0, read);
            if (read != toRead)
            {
                return;
            }
        }
    }
}
