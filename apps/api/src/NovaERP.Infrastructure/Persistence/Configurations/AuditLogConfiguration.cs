using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Audit;

namespace NovaERP.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.UserEmail).HasMaxLength(256);
        builder.Property(a => a.Changes).HasColumnType("jsonb");

        builder.HasIndex(a => new { a.TenantId, a.CreatedAt });
        builder.HasIndex(a => new { a.TenantId, a.EntityName, a.EntityId });
    }
}
