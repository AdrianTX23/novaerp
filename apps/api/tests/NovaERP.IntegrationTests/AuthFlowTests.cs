using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NovaERP.API.Contracts;
using NovaERP.IntegrationTests.TestSupport;

namespace NovaERP.IntegrationTests;

public sealed class AuthFlowTests(NovaErpWebApplicationFactory factory)
    : IClassFixture<NovaErpWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_creates_a_tenant_and_returns_an_access_token()
    {
        var email = $"owner-{Guid.NewGuid():N}@acme.com";

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            CompanyName: $"Acme {Guid.NewGuid():N}", FullName: "Owner Real", Email: email, Password: "Test1234"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.User.Email.Should().Be(email);
        body.User.Roles.Should().Contain("Owner");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_with_an_email_already_in_use_returns_409()
    {
        var email = $"dup-{Guid.NewGuid():N}@acme.com";
        var request = new RegisterRequest(
            CompanyName: $"Acme {Guid.NewGuid():N}", FullName: "Owner", Email: email, Password: "Test1234");

        (await _client.PostAsJsonAsync("/api/auth/register", request)).StatusCode
            .Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync("/api/auth/register",
            request with { CompanyName = $"Acme {Guid.NewGuid():N}" });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Protected_endpoint_without_a_token_returns_401()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
