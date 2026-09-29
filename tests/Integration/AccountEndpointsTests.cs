using System.Net;
using System.Net.Http.Json;
using Account.Api;
using FluentAssertions;
using Xunit;

namespace Account.Tests.Integration;

/// <summary>
/// Integration tests against the real API + a PostgreSQL Testcontainer.
/// These are the tests allowed to touch a database (never unit tests).
/// </summary>
[Collection("postgres")]
public class AccountEndpointsTests(PostgresApiFactory factory) : IClassFixture<PostgresApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateAccount_ThenGetById_ReturnsCreatedAccount()
    {
        var request = new CreateAccountRequest($"user-{Guid.NewGuid():N}@example.com", "Jane Doe");

        var createResponse = await _client.PostAsJsonAsync("/v1/accounts", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<AccountResponse>();
        created.Should().NotBeNull();
        created!.Email.Should().Be(request.Email.ToLowerInvariant());
        created.DisplayName.Should().Be("Jane Doe");

        var getResponse = await _client.GetAsync($"/v1/accounts/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content.ReadFromJsonAsync<AccountResponse>();
        fetched!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task CreateAccount_WithInvalidEmail_ReturnsValidationProblem()
    {
        var request = new CreateAccountRequest("not-an-email", "Jane");

        var response = await _client.PostAsJsonAsync("/v1/accounts", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAccount_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/v1/accounts/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListAccounts_ReturnsOk()
    {
        var response = await _client.GetAsync("/v1/accounts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
