using System.IO.Compression;
using Microsoft.Extensions.Logging;
using UnityBundleReader.Classes;
using static UnityBundleReader.ImportHelper;

namespace UnityBundleReader;

public class AssetsManager
{
    public string? SpecifyUnityVersion;
    public readonly List<SerializedFile> AssetsFileList = [];

    internal readonly Dictionary<string, int> AssetsFileIndexCache = new(StringComparer.OrdinalIgnoreCase);
    internal readonly Dictionary<string, BinaryReader> ResourceFileReaders = new(StringComparer.OrdinalIgnoreCase);

    readonly List<string> _importFiles = [];
    readonly HashSet<string> _importFilesHash = new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _noExistFiles = new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _assetsFileListHash = new(StringComparer.OrdinalIgnoreCase);
    readonly ILogger<AssetsManager> _logger;

    public AssetsManager(ILogger<AssetsManager> logger)
    {
        _logger = logger;
    }

    public void LoadFiles(params string[] files)
    {
        string? path = Path.GetDirectoryName(Path.GetFullPath(files[0]));
        MergeSplitAssets(path);
        string[] toReadFile = ProcessingSplitFiles(files.ToList());
        Load(toReadFile);
    }

    public void LoadFolder(string path)
    {
        MergeSplitAssets(path, true);
        List<string> files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
        string[] toReadFile = ProcessingSplitFiles(files);
        Load(toReadFile);
    }

    void Load(string[] files)
    {
        foreach (string? file in files)
        {
            _importFiles.Add(file);
            _importFilesHash.Add(Path.GetFileName(file));
        }

        //use a for loop because list size can change
        for (int i = 0; i < _importFiles.Count; i++)
        {
            LoadFile(_importFiles[i]);
        }

        _importFiles.Clear();
        _importFilesHash.Clear();
        _noExistFiles.Clear();
        _assetsFileListHash.Clear();

        ReadAssets();
    }

    void LoadFile(string fullName)
    {
        FileReader reader = new(fullName);
        LoadFile(reader);
    }

    void LoadFile(FileReader reader)
    {
        switch (reader.FileType)
        {
            case FileType.AssetsFile:
                LoadAssetsFile(reader);
                break;
            case FileType.BundleFile:
                LoadBundleFile(reader);
                break;
            case FileType.WebFile:
                LoadWebFile(reader);
                break;
            case FileType.GZipFile:
                LoadFile(DecompressGZip(reader));
                break;
            case FileType.BrotliFile:
                LoadFile(DecompressBrotli(reader));
                break;
            case FileType.ZipFile:
                LoadZipFile(reader);
                break;
        }
    }

    void LoadAssetsFile(FileReader reader)
    {
        if (!_assetsFileListHash.Contains(reader.FileName))
        {
            _logger.LogInformation("Loading {Path}...", reader.FullPath);
            try
            {
                SerializedFile assetsFile = new(reader, this);
                CheckStrippedVersion(assetsFile);
                AssetsFileList.Add(assetsFile);
                _assetsFileListHash.Add(assetsFile.FileName);

                foreach (FileIdentifier? sharedFile in assetsFile.Externals)
                {
                    string sharedFileName = sharedFile.FileName;

                    if (!_importFilesHash.Contains(sharedFileName))
                    {
                        string sharedFilePath = Path.Combine(Path.GetDirectoryName(reader.FullPath) ?? ".", sharedFileName);
                        if (!_noExistFiles.Contains(sharedFilePath))
                        {
                            if (!File.Exists(sharedFilePath))
                            {
                                string[] findFiles = Directory.GetFiles(Path.GetDirectoryName(reader.FullPath) ?? ".", sharedFileName, SearchOption.AllDirectories);
                                if (findFiles.Length > 0)
                                {
                                    sharedFilePath = findFiles[0];
                                }
                            }
                            if (File.Exists(sharedFilePath))
                            {
                                _importFiles.Add(sharedFilePath);
                                _importFilesHash.Add(sharedFileName);
                            }
                            else
                            {
                                _noExistFiles.Add(sharedFilePath);
                            }
                        }
                    }
                }
            }
            catch (Exception exn)
            {
                _logger.LogError(exn, "Error while reading assets file {Path}.", reader.FullPath);
                reader.Dispose();
            }
        }
        else
        {
            _logger.LogInformation("Skipped {Path}.", reader.FullPath);
            reader.Dispose();
        }
    }

    void LoadAssetsFromMemory(FileReader reader, string originalPath, string? unityVersion = null)
    {
        if (!_assetsFileListHash.Contains(reader.FileName))
        {
            try
            {
                SerializedFile assetsFile = new(reader, this)
                {
                    OriginalPath = originalPath
                };

                if (!string.IsNullOrEmpty(unityVersion) && assetsFile.Header.MVersion < SerializedFileFormatVersion.Unknown7)
                {
                    assetsFile.SetVersion(unityVersion);
                }
                CheckStrippedVersion(assetsFile);
                AssetsFileList.Add(assetsFile);
                _assetsFileListHash.Add(assetsFile.FileName);
            }
            catch (Exception exn)
            {
                _logger.LogError(exn, "Error while reading assets file {Path} from {FileName}.", reader.FullPath, Path.GetFileName(originalPath));
                ResourceFileReaders.Add(reader.FileName, reader);
            }
        }
        else
        {
            _logger.LogInformation("Skipped {OriginalPath} ({FileName}).", originalPath, reader.FileName);
        }
    }

    void LoadBundleFile(FileReader reader, string? originalPath = null)
    {
        _logger.LogInformation("Loading {Path}...", reader.FullPath);
        try
        {
            BundleFile bundleFile = new(reader);
            foreach (StreamFile? file in bundleFile.FileList)
            {
                string dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath) ?? ".", file.FileName);
                FileReader subReader = new(dummyPath, file.Stream);
                if (subReader.FileType == FileType.AssetsFile)
                {
                    LoadAssetsFromMemory(subReader, originalPath ?? reader.FullPath, bundleFile.Header.UnityRevision);
                }
                else
                {
                    ResourceFileReaders[file.FileName] = subReader; //TODO
                }
            }
        }
        catch (Exception e)
        {
            string str = $"Error while reading bundle file {reader.FullPath}";
            if (originalPath != null)
            {
                str += $" from {Path.GetFileName(originalPath)}";
            }
            Logger.Error(e, str);
        }
        finally
        {
            reader.Dispose();
        }
    }

    void LoadWebFile(FileReader reader)
    {
        _logger.LogInformation("Loading {Path}...", reader.FullPath);
        try
        {
            WebFile webFile = new(reader);
            foreach (StreamFile? file in webFile.FileList)
            {
                string dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath) ?? ".", file.FileName);
                FileReader subReader = new(dummyPath, file.Stream);
                switch (subReader.FileType)
                {
                    case FileType.AssetsFile:
                        LoadAssetsFromMemory(subReader, reader.FullPath);
                        break;
                    case FileType.BundleFile:
                        LoadBundleFile(subReader, reader.FullPath);
                        break;
                    case FileType.WebFile:
                        LoadWebFile(subReader);
                        break;
                    case FileType.ResourceFile:
                        ResourceFileReaders[file.FileName] = subReader; //TODO
                        break;
                }
            }
        }
        catch (Exception exn)
        {
            _logger.LogError(exn, "Error while reading web file {Path}.", reader.FullPath);
        }
        finally
        {
            reader.Dispose();
        }
    }

    void LoadZipFile(FileReader reader)
    {
        _logger.LogInformation("Loading {Path}...", reader.FileName);
        try
        {
            using ZipArchive archive = new(reader.BaseStream, ZipArchiveMode.Read);

            List<string> splitFiles = [];
            // register all files before parsing the assets so that the external references can be found
            // and find split files
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.Name.Contains(".split"))
                {
                    string baseName = Path.GetFileNameWithoutExtension(entry.Name);
                    string basePath = Path.Combine(Path.GetDirectoryName(entry.FullName) ?? ".", baseName);
                    if (!splitFiles.Contains(basePath))
                    {
                        splitFiles.Add(basePath);
                        _importFilesHash.Add(baseName);
                    }
                }
                else
                {
                    _importFilesHash.Add(entry.Name);
                }
            }

            // merge split files and load the result
            foreach (string basePath in splitFiles)
            {
                try
                {
                    Stream splitStream = new MemoryStream();
                    int i = 0;
                    while (true)
                    {
                        string path = $"{basePath}.split{i++}";
                        ZipArchiveEntry? entry = archive.GetEntry(path);
                        if (entry == null)
                        {
                            break;
                        }

                        using Stream entryStream = entry.Open();
                        entryStream.CopyTo(splitStream);
                    }
                    splitStream.Seek(0, SeekOrigin.Begin);
                    FileReader entryReader = new(basePath, splitStream);
                    LoadFile(entryReader);
                }
                catch (Exception exn)
                {
                    _logger.LogError(exn, "Error while reading zip split file {Path}.", basePath);
                }
            }

            // load all entries
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                try
                {
                    string dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath) ?? ".", reader.FileName, entry.FullName);
                    // create a new stream
                    // - to store the deflated stream in
                    // - to keep the data for later extraction
                    Stream streamReader = new MemoryStream();
                    using (Stream entryStream = entry.Open())
                    {
                        entryStream.CopyTo(streamReader);
                    }
                    streamReader.Position = 0;

                    FileReader entryReader = new(dummyPath, streamReader);
                    LoadFile(entryReader);
                    if (entryReader.FileType == FileType.ResourceFile)
                    {
                        entryReader.Position = 0;
                        ResourceFileReaders.TryAdd(entry.Name, entryReader);
                    }
                }
                catch (Exception exn)
                {
                    _logger.LogError(exn, "Error while reading zip entry {Name}.", entry.FullName);
                }
            }
        }
        catch (Exception exn)
        {
            _logger.LogError(exn, "Error while reading zip file {Name}.", reader.FileName);
        }
        finally
        {
            reader.Dispose();
        }
    }

    void CheckStrippedVersion(SerializedFile assetsFile)
    {
        if (assetsFile.IsVersionStripped && string.IsNullOrEmpty(SpecifyUnityVersion))
        {
            throw new Exception("The Unity version has been stripped, please set the version in the options");
        }
        if (!string.IsNullOrEmpty(SpecifyUnityVersion))
        {
            assetsFile.SetVersion(SpecifyUnityVersion);
        }
    }

    void ReadAssets()
    {
        _logger.LogInformation("Reading assets...");

        int progressCount = AssetsFileList.Sum(x => x.ObjectInfos.Count);
        int i = 0;
        foreach (SerializedFile assetsFile in AssetsFileList)
        {
            _logger.LogInformation("Reading assets from {Name}...", assetsFile.FileName);

            foreach (ObjectInfo? objectInfo in assetsFile.ObjectInfos)
            {
                ObjectReader objectReader = new(assetsFile.Reader, assetsFile, objectInfo);
                try
                {
                    switch (objectReader.Type)
                    {
                        case ClassIDType.MonoBehaviour:
                            MonoBehaviour mb = new(objectReader);
                            assetsFile.AddObject(mb);
                            _logger.LogInformation("Found MonoBehaviour {Name}.", mb.Name ?? "__UNNAMED__");
                            break;
                        default:
                            _logger.LogDebug("Object of type {Type} skipped because it is not a MonoBehaviour.", objectReader.Type);
                            break;
                    }
                }
                catch (Exception exn)
                {
                    _logger.LogError(
                        exn,
                        "Unable to load object.\nAssets {Name}\nPath {Path}\nType {Type}\nPathID {PathId}",
                        assetsFile.FileName,
                        assetsFile.OriginalPath,
                        objectReader.Type,
                        objectInfo.PathId
                    );
                }
            }
        }
    }
}
