using System.IO.Compression;
using UnityBundleReader.Brotli;

namespace UnityBundleReader;

public static class ImportHelper
{
    public static void MergeSplitAssets(string? path, bool allDirectories = false)
    {
        string[] splitFiles = Directory.GetFiles(path ?? ".", "*.split0", allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (string? splitFile in splitFiles)
        {
            string destFile = Path.GetFileNameWithoutExtension(splitFile);
            string? destPath = Path.GetDirectoryName(splitFile);
            string destFull = Path.Combine(destPath ?? ".", destFile);
            if (!File.Exists(destFull))
            {
                string[] splitParts = Directory.GetFiles(destPath ?? ".", destFile + ".split*");
                using (FileStream destStream = File.Create(destFull))
                {
                    for (int i = 0; i < splitParts.Length; i++)
                    {
                        string splitPart = destFull + ".split" + i;
                        using (FileStream sourceStream = File.OpenRead(splitPart))
                        {
                            sourceStream.CopyTo(destStream);
                        }
                    }
                }
            }
        }
    }

    public static string[] ProcessingSplitFiles(List<string> selectFile)
    {
        List<string> splitFiles = selectFile.Where(x => x.Contains(".split"))
            .Select(x => Path.Combine(Path.GetDirectoryName(x) ?? ".", Path.GetFileNameWithoutExtension(x)))
            .Distinct()
            .ToList();
        selectFile.RemoveAll(x => x.Contains(".split"));
        foreach (string? file in splitFiles)
        {
            if (File.Exists(file))
            {
                selectFile.Add(file);
            }
        }
        return selectFile.Distinct().ToArray();
    }

    public static FileReader DecompressGZip(FileReader reader)
    {
        using (reader)
        {
            MemoryStream stream = new();
            using (GZipStream gs = new(reader.BaseStream, CompressionMode.Decompress))
            {
                gs.CopyTo(stream);
            }
            stream.Position = 0;
            return new FileReader(reader.FullPath, stream);
        }
    }

    public static FileReader DecompressBrotli(FileReader reader)
    {
        using (reader)
        {
            MemoryStream stream = new();
            using (BrotliInputStream brotliStream = new(reader.BaseStream))
            {
                brotliStream.CopyTo(stream);
            }
            stream.Position = 0;
            return new FileReader(reader.FullPath, stream);
        }
    }
}
