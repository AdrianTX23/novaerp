namespace NovaERP.Domain.Partners;

/// <summary>
/// Flags: un mismo Partner puede ser cliente y proveedor a la vez (frecuente
/// entre empresas que se compran mutuamente insumos y servicios).
/// </summary>
[Flags]
public enum PartnerType
{
    Customer = 1,
    Supplier = 2,
}
