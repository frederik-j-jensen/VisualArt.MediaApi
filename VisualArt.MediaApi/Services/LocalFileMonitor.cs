using Microsoft.Extensions.Options;
using VisualArt.MediaApi.Configuration;

namespace VisualArt.MediaApi.Services;

public class LocalFileMonitor : BackgroundService
{
    private readonly ILogger _logger;
    private readonly StorageConfig _config;
    private readonly FileSystemWatcher _monitor;

    public LocalFileMonitor(ILogger<LocalFileMonitor> logger, IOptions<StorageConfig> config)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));

        var storageLocation = _config.Location;

        if (!Directory.Exists(storageLocation))
            Directory.CreateDirectory(storageLocation);

        _monitor = new(storageLocation)
        {
            NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size
        };

        _monitor.Changed += OnChanged;
        _monitor.Created += (object sender, FileSystemEventArgs e) => Console.WriteLine($"Created: {e.FullPath}");
        _monitor.Deleted += (object sender, FileSystemEventArgs e) => Console.WriteLine($"Deleted: {e.FullPath}");
        _monitor.Renamed += (object sender, RenamedEventArgs e) => Console.WriteLine($"Renamed:  {e.OldFullPath} >> {e.FullPath}");
        _monitor.Error += OnError;

        _monitor.Filter = "*.*";

    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _monitor.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        Console.WriteLine($"Changed: {e.FullPath}");
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        if (ex != null)
        {
            _logger.LogError(ex, ex.StackTrace);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _monitor.Dispose();
    }
}

