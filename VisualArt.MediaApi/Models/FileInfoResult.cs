namespace VisualArt.MediaApi.Models;

public record FileInfoResult(bool Exists, string Name, long Size, DateTime Created, DateTime Modified)
{
    static public FileInfoResult Create(string fileName)
    {
        return Create(new FileInfo(fileName));
    }
    static public FileInfoResult Create(FileInfo fi)
    {
        if(fi.Exists)
            return new FileInfoResult(fi.Exists, fi.Name, fi.Length, File.GetCreationTimeUtc(fi.FullName), File.GetLastWriteTimeUtc(fi.FullName));
        else
            return new FileInfoResult(fi.Exists, fi.Name, 0, DateTime.MinValue, DateTime.MinValue);
    }
}
