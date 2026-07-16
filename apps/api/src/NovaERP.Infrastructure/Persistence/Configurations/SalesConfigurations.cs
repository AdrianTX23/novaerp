using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Partners;
using NovaERP.Domain.Sales;

namespace NovaERP.Infrastructure.Persistence.Configurations;

public sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

        // Número de pedido único por empresa (ej. SO-00001).
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();

        // Filtra ventas por cliente y por fecha en el dashboard/reportes.
        builder.HasIndex(o => new { o.TenantId, o.CustomerId });
        builder.HasIndex(o => new { o.TenantId, o.OrderDate });

        // El patrón real del dashboard/reportes es Status=Confirmed + rango de
        // fechas: sin este índice, Postgres escanea por fecha y filtra Status
        // en memoria (o al revés).
        builder.HasIndex(o => new { o.TenantId, o.Status, o.OrderDate });

        builder.HasOne<Partner>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey(l => l.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Lines).AutoInclude();
    }
}

public sealed class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("sales_order_lines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductSku).HasMaxLength(50).IsRequired();
        builder.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Quantity).HasPrecision(18, 3);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 2);
        builder.Property(l => l.LineTotal).HasPrecision(18, 2);

        builder.HasIndex(l => l.SalesOrderId);

        // FK al producto para trazabilidad; Restrict porque los productos se
        // borran de forma lógica (soft-delete), así que la fila nunca desaparece
        // y la línea histórica conserva además su snapshot de nombre/SKU/precio.
        builder.HasOne<Domain.Catalog.Product>()
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
