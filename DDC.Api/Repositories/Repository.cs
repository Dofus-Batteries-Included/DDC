namespace DDC.Api.Repositories;

static class Repository
{
    public static readonly string Path = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DDC", "Repository");
    public static readonly string RawDataPath = System.IO.Path.Join(Path, "Raw");
}
