using VisualArt.MediaApi.Controllers;
using VisualArt.MediaApi.Services;

namespace VisualArt.MediaApi.Configuration;

public static class ConfigureMediaMiddleware
{
    public static void AddMediaServices(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.Configure<StorageConfig>(configurationManager.GetSection("StorageConfig"));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();        

        services.AddScoped<IStorage, LocalFileStorage>();
        services.AddScoped<MediaApiController>();
        services.AddSingleton<LocalFileMonitor>();
        services.AddHostedService<LocalFileMonitor>();
    }
}
