namespace Account.Domain;

/// <summary>
/// Represents an account in the Accounts service.
/// This is the go-forward PostgreSQL-backed model. The interim schema
/// remains in SQL Server during migration (see S1 spec).
/// </summary>
public class Account
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public required string DisplayName { get; set; }

    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public enum AccountStatus
{
    Active = 0,
    Suspended = 1,
    Closed = 2
}
