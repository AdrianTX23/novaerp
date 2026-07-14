using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Crm;
using NovaERP.Domain.Partners;

namespace NovaERP.Infrastructure.Persistence.Configurations;

public sealed class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("opportunities");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.Stage).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.EstimatedValue).HasPrecision(18, 2);

        builder.Ignore(o => o.IsClosed);

        builder.HasIndex(o => new { o.TenantId, o.Stage });
        builder.HasIndex(o => new { o.TenantId, o.CustomerId });

        builder.HasOne<Partner>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
