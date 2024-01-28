using VisualArt.MediaApi.Services;

namespace VisualArt.MediaApi.Controllers;

public class MediaApiController
{
    private readonly ILogger _logger;
    private readonly IStorage _storage;

    public MediaApiController(ILogger<MediaApiController> logger, IStorage storage)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public IEnumerable<Models.FileInfoResult> ListFiles(uint? start, uint? count)
    {
        return _storage.ListFiles().Skip((int)(start ?? 0)).Take((int)(count ?? int.MaxValue));
    }

    public async IAsyncEnumerable<Models.FileInfoResult> UploadFiles(IFormFileCollection files)
    {
        foreach (var file in files)
        {
            {
                var result = _storage.ValidateFileForUpload(file);
                if (!result.IsValid)
                {
                    _logger.LogWarning($"File [{file.FileName}] is not valid for upload: {result.Message}");
                    continue;
                }
            }
            using (var stream = file.OpenReadStream())
            {
                var result = await _storage.SaveFileAsync(file.FileName, stream);

                if (result.Exists)
                {
                    yield return result;
                }
            }
        }
    }
}
