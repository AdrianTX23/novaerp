using NovaERP.Domain.Common;

namespace NovaERP.Domain.Partners;

/// <summary>
/// Cliente y/o proveedor. Se modelan como una sola entidad (patrón "Business
/// Partner" de SAP/Odoo) en vez de dos tablas casi idénticas, porque una misma
/// empresa suele ser ambas cosas y porque Ventas/Compras terminan
/// referenciando la misma forma de datos (nombre, documento, contacto).
/// </summary>
public sealed class Partner : TenantAuditableEntity
{
    public string Name { get; private set; } = null!;
    public PartnerType Type { get; private set; }
    public string? DocumentNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Partner() { }

    public Partner(
        Guid tenantId,
        string name,
        PartnerType type,
        string? documentNumber = null,
        string? email = null,
        string? phone = null,
        string? address = null)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        Type = type;
        DocumentNumber = documentNumber;
        Email = email;
        Phone = phone;
        Address = address;
    }

    public void Update(
        string name, PartnerType type, string? documentNumber, string? email, string? phone, string? address)
    {
        Name = name.Trim();
        Type = type;
        DocumentNumber = documentNumber;
        Email = email;
        Phone = phone;
        Address = address;
    }

    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;
}
