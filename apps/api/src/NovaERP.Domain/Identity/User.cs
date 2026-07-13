using NovaERP.Domain.Common;

namespace NovaERP.Domain.Identity;

public sealed class User : TenantAuditableEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private readonly List<UserRole> _roles = [];
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    private User() { }

    public User(Guid tenantId, string email, string passwordHash, string fullName)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
    }

    public void AssignRole(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId))
        {
            return;
        }

        _roles.Add(new UserRole(Id, roleId));
    }

    public void UpdatePasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;
}
