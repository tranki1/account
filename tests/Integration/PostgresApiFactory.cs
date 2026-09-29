using Account.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Account.Tests.Integration;

/// <summary>
/// Spins up a real PostgreSQL container via Testcontainers and boots the
/// application against it. Shared across a test class via IClassFixture.
/// Requires a running Docker/compatible engine.
/// </summary>
public sealed class PostgresApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("accounts")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:AccountsDb", _container.GetConnectionString());
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Apply EF Core migrations against the fresh container.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }
}
