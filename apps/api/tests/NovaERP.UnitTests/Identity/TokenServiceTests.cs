using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NovaERP.Domain.Identity;
using NovaERP.Infrastructure.Identity;

namespace NovaERP.UnitTests.Identity;

public sealed class TokenServiceTests
{
    private readonly TokenService _sut = new(Options.Create(new JwtOptions
    {
        Issuer = "NovaERP",
        Audience = "NovaERP.Clients",
        SigningKey = "clave-de-prueba-suficientemente-larga-para-hmac-sha256!!",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7,
    }));

    [Fact]
    public void CreateAccessToken_embeds_sub_email_tenant_and_permissions()
    {
        var user = new User(Guid.NewGuid(), "adrian@acme.com", "hash", "Adrián Pico");
        var permissions = new[] { Permissions.InventoryRead, Permissions.SalesManage };

        var token = _sut.CreateAccessToken(user, permissions);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);

        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "adrian@acme.com");
        jwt.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == user.TenantId.ToString());
        jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value)
            .Should().BeEquivalentTo(permissions);
    }

    [Fact]
    public void CreateRefreshToken_hash_matches_HashRefreshToken()
    {
        var refresh = _sut.CreateRefreshToken();

        _sut.HashRefreshToken(refresh.Value).Should().Be(refresh.Hash);
    }
}
