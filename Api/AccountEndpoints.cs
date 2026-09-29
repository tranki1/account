using Account.Data;
using Microsoft.EntityFrameworkCore;
using AccountEntity = Account.Domain.Account;

namespace Account.Api;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/accounts").WithTags("Accounts");

        group.MapGet("/", async (AccountsDbContext db, CancellationToken ct) =>
        {
            var accounts = await db.Accounts
                .AsNoTracking()
                .OrderBy(a => a.CreatedAt)
                .Select(a => AccountResponse.FromEntity(a))
                .ToListAsync(ct);
            return Results.Ok(accounts);
        })
        .WithName("ListAccounts");

        group.MapGet("/{id:guid}", async (Guid id, AccountsDbContext db, CancellationToken ct) =>
        {
            var account = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
            return account is null
                ? Results.NotFound()
                : Results.Ok(AccountResponse.FromEntity(account));
        })
        .WithName("GetAccount");

        group.MapPost("/", async (CreateAccountRequest request, AccountsDbContext db, CancellationToken ct) =>
        {
            AccountEntity account;
            try
            {
                account = Domain.AccountFactory.Create(request.Email, request.DisplayName, DateTimeOffset.UtcNow);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["request"] = [ex.Message]
                });
            }

            db.Accounts.Add(account);
            await db.SaveChangesAsync(ct);

            return Results.CreatedAtRoute(
                "GetAccount",
                new { id = account.Id },
                AccountResponse.FromEntity(account));
        })
        .WithName("CreateAccount");

        return app;
    }
}
