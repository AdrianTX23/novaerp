using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using NovaERP.API.Contracts;
using NovaERP.Application.Features.Authentication.Common;
using NovaERP.Application.Features.Roles.Common;
using NovaERP.Application.Features.Users.Common;
using NovaERP.Domain.Identity;
using NovaERP.IntegrationTests.TestSupport;

namespace NovaERP.IntegrationTests;

/// <summary>
/// Ejercita el ciclo completo de gestión de usuarios/roles contra la app real
/// (HTTP + middleware + EF Core sobre Npgsql real, no InMemory). Es la
/// regresión verdadera del DbUpdateConcurrencyException de la Fase 4: ese bug
/// solo se manifestaba contra Postgres real, no contra el proveedor InMemory
/// que usan los unit tests de NovaERP.UnitTests.
/// </summary>
public sealed class UserAndRoleManagementTests(NovaErpWebApplicationFactory factory)
    : IClassFixture<NovaErpWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(HttpClient Client, Guid MemberRoleId, Guid AdminRoleId)> RegisterOwnerAsync()
    {
        var email = $"owner-{Guid.NewGuid():N}@acme.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            CompanyName: $"Acme {Guid.NewGuid():N}", FullName: "Owner", Email: email, Password: "Test1234"));
        var auth = (await register.Content.ReadFromJsonAsync<AuthResponse>())!;

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var roles = (await (await client.GetAsync("/api/roles")).Content.ReadFromJsonAsync<List<RoleDetail>>())!;
        var memberId = roles.Single(r => r.Name == SystemRoles.Member).Id;
        var adminId = roles.Single(r => r.Name == SystemRoles.Admin).Id;

        return (client, memberId, adminId);
    }

    [Fact]
    public async Task Reassigning_a_users_roles_keeps_unchanged_adds_new_and_removes_dropped()
    {
        var (client, memberId, adminId) = await RegisterOwnerAsync();

        var createResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(
            Email: $"teammate-{Guid.NewGuid():N}@acme.com", FullName: "Compañero", Password: "Test1234",
            RoleIds: [memberId]));
        var created = await createResponse.Content.ReadFromJsonAsync<UserSummary>();

        // Mantiene Member, agrega Admin, en la misma operación: exactamente el
        // patrón que disparaba el UPDATE fantasma contra Postgres real.
        var response = await client.PutAsJsonAsync(
            $"/api/users/{created!.Id}/roles", new UpdateUserRolesRequest([memberId, adminId]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<UserSummary>();
        updated!.Roles.Select(r => r.Id).Should().BeEquivalentTo([memberId, adminId]);
    }

    [Fact]
    public async Task Updating_a_roles_permissions_keeps_unchanged_adds_new_and_revokes_dropped()
    {
        var (client, _, _) = await RegisterOwnerAsync();

        var createResponse = await client.PostAsJsonAsync("/api/roles", new CreateRoleRequest(
            "Vendedor", "Solo ventas", [Permissions.SalesRead, Permissions.SalesManage]));
        var role = await createResponse.Content.ReadFromJsonAsync<RoleDetail>();

        var response = await client.PutAsJsonAsync($"/api/roles/{role!.Id}", new UpdateRoleRequest(
            "Vendedor Senior", "Ventas + reportes", [Permissions.SalesRead, Permissions.ReportsRead]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<RoleDetail>();
        updated!.PermissionCodes.Should().BeEquivalentTo([Permissions.SalesRead, Permissions.ReportsRead]);
    }

    [Fact]
    public async Task Cannot_deactivate_your_own_account()
    {
        var email = $"owner-{Guid.NewGuid():N}@acme.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            CompanyName: $"Acme {Guid.NewGuid():N}", FullName: "Owner", Email: email, Password: "Test1234"));
        var auth = (await register.Content.ReadFromJsonAsync<AuthResponse>())!;

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.PostAsync($"/api/users/{auth.User.Id}/deactivate", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
