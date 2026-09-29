using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Account.Data;

/// <summary>
/// Used by the EF Core tools (dotnet ef) at design time to create the
/// DbContext without starting the full application. The connection string
/// here is only for generating migrations; runtime config comes from
/// Secrets Manager / SSM.
/// </summary>
public class AccountsDbContextFactory : IDesignTimeDbContextFactory<AccountsDbContext>
{
    public AccountsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ACCOUNTS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=accounts;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AccountsDbContext(options);
    }
}
