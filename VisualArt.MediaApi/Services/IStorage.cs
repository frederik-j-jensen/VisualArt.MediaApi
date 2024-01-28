using VisualArt.MediaApi.Models;

namespace VisualArt.MediaApi.Services;

public interface IStorage
{
    Task<Models.FileInfoResult> SaveFileAsync(string fileName, Stream stream);
    IEnumerable<Models.FileInfoResult> ListFiles();
    ValidationResult ValidateFileForUpload(IFormFile file);
}
