using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace VisualArt.MediaApi.Test;

public class WebApplicationTest : IClassFixture<WebApplicationFactory<MediaApi.Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebApplicationTest(WebApplicationFactory<MediaApi.Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateInjectedClient()
    {
        var client = _factory.CreateDefaultClient();
        return client;
    }

    [Theory]
    [InlineData("/api/media")]
    public async Task GetEndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = CreateInjectedClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response!.Content!.Headers!.ContentType!.ToString());
    }

    [Theory]
    [InlineData("/api/something")]
    public async Task GetEndpointsReturnNotFoundWhenCalledWithNotMappedPath(string url)
    {
        // Arrange
        var client = CreateInjectedClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose() { }
}