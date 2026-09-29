using Account.Domain;

namespace Account.Api;

public record CreateAccountRequest(string Email, string DisplayName);

public record AccountResponse(
    Guid Id,
    string Email,
    string DisplayName,
    AccountStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static AccountResponse FromEntity(Domain.Account account) => new(
        account.Id,
        account.Email,
        account.DisplayName,
        account.Status,
        account.CreatedAt,
        account.UpdatedAt);
}
