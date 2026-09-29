using Account.Domain;
using FluentAssertions;
using Xunit;

namespace Account.Tests.Unit;

/// <summary>
/// Pure unit tests. No database, no network — fast and deterministic,
/// per the S1 testing rule.
/// </summary>
public class AccountFactoryTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("first.last@sub.example.co", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("no-at-sign", false)]
    [InlineData("@nolocal.com", false)]
    [InlineData("trailing@", false)]
    [InlineData("two@@ats.com", false)]
    [InlineData("noDotAfterAt@localhost", false)]
    public void IsValidEmail_ValidatesFormat(string email, bool expected)
    {
        AccountFactory.IsValidEmail(email).Should().Be(expected);
    }

    [Fact]
    public void Create_NormalizesEmailAndTrimsName()
    {
        var now = DateTimeOffset.UtcNow;

        var account = AccountFactory.Create("  User@Example.COM ", "  Jane Doe  ", now);

        account.Email.Should().Be("user@example.com");
        account.DisplayName.Should().Be("Jane Doe");
        account.Status.Should().Be(AccountStatus.Active);
        account.CreatedAt.Should().Be(now);
        account.UpdatedAt.Should().Be(now);
        account.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_Throws_ForInvalidEmail()
    {
        var act = () => AccountFactory.Create("bad-email", "Jane", DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void Create_Throws_ForEmptyDisplayName()
    {
        var act = () => AccountFactory.Create("user@example.com", "  ", DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentException>().WithParameterName("displayName");
    }
}
