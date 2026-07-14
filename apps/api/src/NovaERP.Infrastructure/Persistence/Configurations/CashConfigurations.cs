using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Cash;

namespace NovaERP.Infrastructure.Persistence.Configurations;

public sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.ToTable("cash_movements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Amount).HasPrecision(18, 2);
        builder.Property(m => m.Concept).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);

        builder.HasIndex(m => new { m.TenantId, m.Date });
    }
}
