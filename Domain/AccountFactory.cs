namespace Account.Domain;

/// <summary>
/// Pure domain logic for creating and validating accounts.
/// Contains no I/O so it can be unit tested without a database or network.
/// </summary>
public static class AccountFactory
{
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var trimmed = email.Trim();
        var atIndex = trimmed.IndexOf('@');
        // Must contain a single '@' that isn't first/last, and a dot after it.
        return atIndex > 0
            && atIndex < trimmed.Length - 1
            && trimmed.IndexOf('@', atIndex + 1) < 0
            && trimmed.IndexOf('.', atIndex) > 0;
    }

    /// <summary>
    /// Creates a new active account with normalized values and timestamps.
    /// Throws <see cref="ArgumentException"/> for invalid input.
    /// </summary>
    public static Account Create(string email, string displayName, DateTimeOffset now)
    {
        if (!IsValidEmail(email))
        {
            throw new ArgumentException("A valid email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        return new Account
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            Status = AccountStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
