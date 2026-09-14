using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Testcontainers.PostgreSql;

namespace CraigsClone.Tests.E2E;

/// <summary>
/// The real app on a real port, backed by its own throwaway Postgres, plus a headless browser.
/// Used by HTTP integration tests (CreateClient) and browser tests (Browser + BaseUrl).
/// </summary>
public class WebAppFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16").Build();
    private WebApplicationFactory<Program> _factory = null!;
    private IPlaywright _pw = null!;

    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = "";
    public IServiceProvider Services => _factory.Services;

    public HttpClient CreateClient() => new() { BaseAddress = new Uri(BaseUrl + "/") };

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.UseSetting("ConnectionStrings:Default", _pg.GetConnectionString()));
        _factory.UseKestrel();      // real port so a browser can reach it (.NET 10 API)
        _factory.StartServer();

        // Ask Kestrel which address it bound to (port 0 = random free port).
        var addresses = _factory.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()?.Addresses
            ?? throw new InvalidOperationException("Kestrel reported no addresses.");
        BaseUrl = addresses.First(a => a.StartsWith("http://")).TrimEnd('/');

        _pw = await Playwright.CreateAsync();
        Browser = await _pw.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null) await Browser.DisposeAsync();
        _pw?.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        await _pg.DisposeAsync();
    }
}

[CollectionDefinition("app")]
public class AppCollection : ICollectionFixture<WebAppFixture> { }
