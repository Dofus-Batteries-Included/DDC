using System.IO.Compression;
using System.Text;
using UnityBundleReader.Classes;
using static UnityBundleReader.ImportHelper;
using Object = UnityBundleReader.Classes.Object;

namespace UnityBundleReader;

public class AssetsManager
{
    public string? SpecifyUnityVersion;
    public readonly List<SerializedFile> AssetsFileList = new();

    internal readonly Dictionary<string, int> AssetsFileIndexCache = new(StringComparer.OrdinalIgnoreCase);
    internal readonly Dictionary<string, BinaryReader> ResourceFileReaders = new(StringComparer.OrdinalIgnoreCase);

    readonly List<string> _importFiles = [];
    readonly HashSet<string> _importFilesHash = new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _noExistFiles = new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _assetsFileListHash = new(StringComparer.OrdinalIgnoreCase);

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

        Progress.Reset();
        //use a for loop because list size can change
        for (int i = 0; i < _importFiles.Count; i++)
        {
            LoadFile(_importFiles[i]);
            Progress.Report(i + 1, _importFiles.Count);
        }

        _importFiles.Clear();
        _importFilesHash.Clear();
        _noExistFiles.Clear();
        _assetsFileListHash.Clear();

        ReadAssets();
        ProcessAssets();
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
            Logger.Info($"Loading {reader.FullPath}");
            try
            {
                SerializedFile assetsFile = new(reader, this);
                CheckStrippedVersion(assetsFile);
                AssetsFileList.Add(assetsFile);
                _assetsFileListHash.Add(assetsFile.FileName);

                foreach (FileIdentifier? sharedFile in assetsFile.MExternals)
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
            catch (Exception e)
            {
                Logger.Error($"Error while reading assets file {reader.FullPath}", e);
                reader.Dispose();
            }
        }
        else
        {
            Logger.Info($"Skipping {reader.FullPath}");
            reader.Dispose();
        }
    }

    void LoadAssetsFromMemory(FileReader reader, string originalPath, string? unityVersion = null)
    {
        if (!_assetsFileListHash.Contains(reader.FileName))
        {
            try
            {
                SerializedFile assetsFile = new(reader, this);
                assetsFile.OriginalPath = originalPath;
                if (!string.IsNullOrEmpty(unityVersion) && assetsFile.Header.MVersion < SerializedFileFormatVersion.Unknown7)
                {
                    assetsFile.SetVersion(unityVersion);
                }
                CheckStrippedVersion(assetsFile);
                AssetsFileList.Add(assetsFile);
                _assetsFileListHash.Add(assetsFile.FileName);
            }
            catch (Exception e)
            {
                Logger.Error($"Error while reading assets file {reader.FullPath} from {Path.GetFileName(originalPath)}", e);
                ResourceFileReaders.Add(reader.FileName, reader);
            }
        }
        else
        {
            Logger.Info($"Skipping {originalPath} ({reader.FileName})");
        }
    }

    void LoadBundleFile(FileReader reader, string? originalPath = null)
    {
        Logger.Info("Loading " + reader.FullPath);
        try
        {
            BundleFile bundleFile = new(reader);
            foreach (StreamFile? file in bundleFile.FileList)
            {
                string dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath) ?? ".", file.fileName);
                FileReader subReader = new(dummyPath, file.stream);
                if (subReader.FileType == FileType.AssetsFile)
                {
                    LoadAssetsFromMemory(subReader, originalPath ?? reader.FullPath, bundleFile.MHeader.UnityRevision);
                }
                else
                {
                    ResourceFileReaders[file.fileName] = subReader; //TODO
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
            Logger.Error(str, e);
        }
        finally
        {
            reader.Dispose();
        }
    }

    void LoadWebFile(FileReader reader)
    {
        Logger.Info("Loading " + reader.FullPath);
        try
        {
            WebFile webFile = new(reader);
            foreach (StreamFile? file in webFile.FileList)
            {
                string dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath) ?? ".", file.fileName);
                FileReader subReader = new(dummyPath, file.stream);
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
                        ResourceFileReaders[file.fileName] = subReader; //TODO
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error while reading web file {reader.FullPath}", e);
        }
        finally
        {
            reader.Dispose();
        }
    }

    void LoadZipFile(FileReader reader)
    {
        Logger.Info("Loading " + reader.FileName);
        try
        {
            using (ZipArchive archive = new(reader.BaseStream, ZipArchiveMode.Read))
            {
                List<string> splitFiles = new();
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
                            using (Stream entryStream = entry.Open())
                            {
                                entryStream.CopyTo(splitStream);
                            }
                        }
                        splitStream.Seek(0, SeekOrigin.Begin);
                        FileReader entryReader = new(basePath, splitStream);
                        LoadFile(entryReader);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error while reading zip split file {basePath}", e);
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
                    catch (Exception e)
                    {
                        Logger.Error($"Error while reading zip entry {entry.FullName}", e);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error while reading zip file {reader.FileName}", e);
        }
        finally
        {
            reader.Dispose();
        }
    }

    public void CheckStrippedVersion(SerializedFile assetsFile)
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

    public void Clear()
    {
        foreach (SerializedFile? assetsFile in AssetsFileList)
        {
            assetsFile.Objects.Clear();
            assetsFile.Reader.Close();
        }
        AssetsFileList.Clear();

        foreach (KeyValuePair<string, BinaryReader> resourceFileReader in ResourceFileReaders)
        {
            resourceFileReader.Value.Close();
        }
        ResourceFileReaders.Clear();

        AssetsFileIndexCache.Clear();
    }

    void ReadAssets()
    {
        Logger.Info("Read assets...");

        int progressCount = AssetsFileList.Sum(x => x.MObjects.Count);
        int i = 0;
        Progress.Reset();
        foreach (SerializedFile assetsFile in AssetsFileList)
        {
            Logger.Info($"Reading assets from {assetsFile.FileName}...");

            foreach (ObjectInfo? objectInfo in assetsFile.MObjects)
            {
                ObjectReader objectReader = new(assetsFile.Reader, assetsFile, objectInfo);
                Logger.Info($"Reading object of type {objectReader.Type}...");
                try
                {
                    switch (objectReader.Type)
                    {
                        case ClassIDType.MonoBehaviour:
                            Object obj = new MonoBehaviour(objectReader);
                            assetsFile.AddObject(obj);
                            break;
                        default:
                            Logger.Debug("Object skipped because it is not a MonoBehaviour.");
                            break;
                    }
                }
                catch (Exception e)
                {
                    StringBuilder sb = new();
                    sb.AppendLine("Unable to load object")
                        .AppendLine($"Assets {assetsFile.FileName}")
                        .AppendLine($"Path {assetsFile.OriginalPath}")
                        .AppendLine($"Type {objectReader.Type}")
                        .AppendLine($"PathID {objectInfo.MPathID}")
                        .Append(e);
                    Logger.Error(sb.ToString());
                }

                Progress.Report(++i, progressCount);
            }
        }
    }

    void ProcessAssets()
    {
        Logger.Info("Process Assets...");

        foreach (SerializedFile? assetsFile in AssetsFileList)
        {
            foreach (Object? obj in assetsFile.Objects)
            {
                switch (obj)
                {
                    case GameObject mGameObject:
                    {
                        foreach (PPtr<Component>? pptr in mGameObject.MComponents)
                        {
                            if (pptr.TryGet(out Component? mComponent))
                            {
                                switch (mComponent)
                                {
                                    case Transform mTransform:
                                        mGameObject.MTransform = mTransform;
                                        break;
                                    case MeshRenderer mMeshRenderer:
                                        mGameObject.MMeshRenderer = mMeshRenderer;
                                        break;
                                    case MeshFilter mMeshFilter:
                                        mGameObject.MMeshFilter = mMeshFilter;
                                        break;
                                    case SkinnedMeshRenderer mSkinnedMeshRenderer:
                                        mGameObject.MSkinnedMeshRenderer = mSkinnedMeshRenderer;
                                        break;
                                    case Animator mAnimator:
                                        mGameObject.MAnimator = mAnimator;
                                        break;
                                    case Animation mAnimation:
                                        mGameObject.MAnimation = mAnimation;
                                        break;
                                }
                            }
                        }
                        break;
                    }
                    case SpriteAtlas mSpriteAtlas:
                    {
                        foreach (PPtr<Sprite>? mPackedSprite in mSpriteAtlas.MPackedSprites)
                        {
                            if (mPackedSprite.TryGet(out Sprite? mSprite))
                            {
                                if (mSprite.MSpriteAtlas.IsNull)
                                {
                                    mSprite.MSpriteAtlas.Set(mSpriteAtlas);
                                }
                                else
                                {
                                    mSprite.MSpriteAtlas.TryGet(out SpriteAtlas? mSpriteAtlaOld);
                                    if (mSpriteAtlaOld.MIsVariant)
                                    {
                                        mSprite.MSpriteAtlas.Set(mSpriteAtlas);
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}
