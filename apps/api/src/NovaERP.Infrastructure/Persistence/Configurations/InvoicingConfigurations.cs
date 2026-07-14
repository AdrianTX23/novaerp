using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Invoicing;

namespace NovaERP.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).HasMaxLength(30).IsRequired();
        builder.Property(i => i.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Total).HasPrecision(18, 2);
        builder.Property(i => i.AmountPaid).HasPrecision(18, 2);

        builder.Ignore(i => i.OutstandingBalance);

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique();
        // Un pedido de venta se factura una sola vez.
        builder.HasIndex(i => new { i.TenantId, i.SalesOrderId }).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.CustomerId });
        builder.HasIndex(i => new { i.TenantId, i.Status });

        builder.HasMany(i => i.Lines)
            .WithOne()
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Lines).AutoInclude();
        builder.Navigation(i => i.Payments).AutoInclude();
    }
}

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductSku).HasMaxLength(50).IsRequired();
        builder.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Quantity).HasPrecision(18, 3);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 2);
        builder.Property(l => l.LineTotal).HasPrecision(18, 2);

        builder.HasIndex(l => l.InvoiceId);
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Reference).HasMaxLength(200);

        builder.HasIndex(p => p.InvoiceId);
        // La Caja (Fase 10) consulta ingresos por fecha.
        builder.HasIndex(p => new { p.TenantId, p.PaidAt });
    }
}
