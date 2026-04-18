using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TailspinToys.Api;
using TailspinToys.Api.Models;

/*
 * TestPublishersRoutes.cs
 * Integration tests for the /api/publishers endpoint.
 */
namespace TailspinToys.Api.Tests;

/// <summary>
/// Integration tests covering publisher endpoints.
/// </summary>
public class TestPublishersRoutes : IDisposable
{
    private readonly string _dbPath;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private static readonly Dictionary<string, object>[] TestPublishers =
    [
        new() { ["name"] = "DevGames Inc" },
        new() { ["name"] = "Scrum Masters" }
    ];

    private const string PublishersApiPath = "/api/publishers";

    /// <summary>
/// Initializes a test web application factory and seeds test data for publisher tests.
/// </summary>
public TestPublishersRoutes()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"TestDb_{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={_dbPath}";

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = connectionString
                    });
                });
            });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TailspinToysContext>();
        SeedTestData(db);
    }

    private void SeedTestData(TailspinToysContext db)
    {
        var publishers = TestPublishers.Select(p => new Publisher { Name = (string)p["name"] }).ToList();
        db.Publishers.AddRange(publishers);
        db.SaveChanges();
    }

    [Fact]
    /// <summary>
/// Verifies that GET /api/publishers returns all publishers with id and name fields.
/// </summary>
public async Task GetPublishers_ReturnsAllPublishers()
    {
        var response = await _client.GetAsync(PublishersApiPath);
        var data = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(data);
        Assert.Equal(TestPublishers.Length, data.Count);

        for (var i = 0; i < data.Count; i++)
        {
            var publisherData = data[i];
            Assert.True(publisherData.ContainsKey("id"));
            Assert.Equal(TestPublishers[i]["name"].ToString(), publisherData["name"]?.ToString());
        }
    }

    /// <summary>
/// Performs test cleanup and removes temporary test database files.
/// </summary>
public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch (IOException) { }
    }
}
