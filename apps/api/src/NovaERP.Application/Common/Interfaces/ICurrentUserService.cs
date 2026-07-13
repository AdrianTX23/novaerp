namespace NovaERP.Application.Common.Interfaces;

/// <summary>
/// Identidad del usuario autenticado del request actual. Usado por el
/// SaveChangesInterceptor para rellenar CreatedBy/LastModifiedBy.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
}
