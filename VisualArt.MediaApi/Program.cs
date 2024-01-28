using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using VisualArt.MediaApi.Configuration;
using VisualArt.MediaApi.Controllers;

namespace VisualArt.MediaApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = builder.Configuration.GetValue<long>("MultipartBodyLengthLimit");
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddMediaServices(builder.Configuration);
        var app = builder.Build();

        app.ConfigureMediaServicesExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.MapPost("/api/media",
            ([FromServices] MediaApiController mediaController, IFormFileCollection files) =>
                mediaController.UploadFiles(files))
                .DisableAntiforgery();

        app.MapGet("/api/media",
            ([FromServices] MediaApiController mediaController, uint? start, uint? count) =>
                mediaController.ListFiles(start, count));

        app.Run();
    }
}