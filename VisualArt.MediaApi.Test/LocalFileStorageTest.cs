using VisualArt.MediaApi.Services;
using VisualArt.MediaApi.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace VisualArt.MediaApi.Test;

public class LocalFileStorageTest
{

    private LocalFileStorage _storage;

    public LocalFileStorageTest()
    {

        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<LocalFileStorage>();

        var config = new StorageConfig()
        {
            Location = "testStorage",
            MaxFileSize = 1024,
            AllowedExtensions = new HashSet<string>() { ".txt", ".jpg", ".png" }
        };
        var options = Options.Create(config);

        Directory.Delete(config.Location, true);

        _storage = new LocalFileStorage(logger, options);
    }

    [Fact]
    public void TestListFiles()
    {
        var result = _storage.ListFiles();

        Assert.True(result.Count() == 0);
    }

    [Fact]
    public async void TestSaveFileAsync()
    {
        var fileName = "test.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        var fileInfoResult = await _storage.SaveFileAsync(fileName, stream);

        Assert.True(fileInfoResult != null);
        Assert.True(fileInfoResult.Name == fileName);

        var listFilesResult = _storage.ListFiles();

        Assert.True(listFilesResult.Count() == 1);
    }

    [Fact]
    public async void TestSaveSameFileTwice()
    {
        var fileName = "test.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        var fileInfo1 = await _storage.SaveFileAsync(fileName, stream);
        var fileInfo2 = await _storage.SaveFileAsync(fileName, stream);

        var listFilesResult = _storage.ListFiles();

        Assert.True(fileInfo2 != null);

        Assert.True(fileInfo2.Exists);

        Assert.True(listFilesResult.Count() == 1);
    }

    [Fact]
    public void TestValidateFileForUploadWithValidFile()
    {
        IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("test")), 0, 0, "Data", "test.txt");

        var result = _storage.ValidateFileForUpload(file);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void TestValidateFileForUploadWithInvalidExtension()
    {
        IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("test")), 0, 0, "Data", "test.exe");

        var result = _storage.ValidateFileForUpload(file);

        Assert.False(result.IsValid);
    }
}
