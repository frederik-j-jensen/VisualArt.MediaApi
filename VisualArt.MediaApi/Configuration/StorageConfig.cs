namespace VisualArt.MediaApi.Configuration;

public class StorageConfig
{
    public string Location { get; set; } = Path.Combine(Path.GetTempPath(), "VisualArt.MediaApi");
    public long MaxFileSize { get; set; } = 500 * 1024 * 1024;
    public HashSet<string> AllowedExtensions { get; set; } = new();
}