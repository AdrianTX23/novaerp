using NovaERP.Domain.Common;

namespace NovaERP.Domain.Identity;

/// <summary>Join User ↔ Role. Se accede siempre vía la navegación de User (tenant-scoped).</summary>
public sealed class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RoleId = roleId;
    }
}
