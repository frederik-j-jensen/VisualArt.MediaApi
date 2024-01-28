using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using VisualArt.MediaApi.Configuration;
using VisualArt.MediaApi.Models;

namespace VisualArt.MediaApi.Services;

public class LocalFileStorage : IStorage
{
    private readonly ILogger _logger;
    private readonly StorageConfig _config;
    private string StorageLocation => _config.Location;

    public LocalFileStorage(ILogger<LocalFileStorage> logger, IOptions<StorageConfig> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = options.Value ?? throw new ArgumentNullException(nameof(options));

        CreateStorageDirectory();
    }

    public IEnumerable<Models.FileInfoResult> ListFiles()
    {
        var di = new DirectoryInfo(StorageLocation);
        if (!di.Exists)
        {
            _logger.LogWarning($"Directory {StorageLocation} does not exist.");
            return Enumerable.Empty<Models.FileInfoResult>();
        }
        return di.GetFiles().Select(FileInfoResult.Create);
    }

    public async Task<Models.FileInfoResult> SaveFileAsync(string fileName, Stream stream)
    {
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        _ = stream ?? throw new ArgumentNullException(nameof(stream));

        Path.GetInvalidFileNameChars().ToList().ForEach(c => fileName = fileName.Replace(c, '_'));

        var fullFileName = Path.Combine(StorageLocation, fileName);

        var hash = CalculateHash(stream);

        if (FileExistsInStore(fullFileName, hash))
        {
            _logger.LogInformation($"File {fileName} already exists in storage.");
        }
        else
        {
            try
            {
                var tempFile = $"{fullFileName}.{Guid.NewGuid()}.tmp";
                using (var fileStream = File.Create(tempFile))
                {
                    await stream.CopyToAsync(fileStream);
                }
                File.Move(tempFile, fullFileName, true);

                _logger.LogInformation($"Saved file {fullFileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving file {fullFileName}");
                throw;
            }
        }
        return FileInfoResult.Create(fullFileName);
    }

    private void CreateStorageDirectory()
    {
        if (!Directory.Exists(StorageLocation))
        {
            Directory.CreateDirectory(StorageLocation);
            _logger.LogInformation($"Local file storage created at {StorageLocation}.");
        }
    }

    public bool FileExistsInStore(string fullFileName, string hash)
    {
        return File.Exists(fullFileName) && hash == CalculateHash(fullFileName);
    }

    public static string CalculateHash(Stream stream)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hashBytes = sha1.ComputeHash(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public static string CalculateHash(string fullFileName)
    {
        using (var stream = File.OpenRead(fullFileName))
        {
            return CalculateHash(stream);
        }
    }

    public ValidationResult ValidateFileForUpload(IFormFile file)
    {
        _ = file ?? throw new ArgumentNullException(nameof(file));

        if (file.Length > _config.MaxFileSize)
            return ValidationResult.Invalid($"File size {file.Length} is too large for file {file.FileName}");

        var extension = Path.GetExtension(file.FileName);
        if (!_config.AllowedExtensions.Contains(extension.ToLower()))
            return ValidationResult.Invalid($"File extension {extension} is not allowed for file {file.FileName}");

        return ValidationResult.Valid;
    }
}