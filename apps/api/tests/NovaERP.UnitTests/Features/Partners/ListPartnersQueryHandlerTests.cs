using FluentAssertions;
using NovaERP.Application.Features.Partners.ListPartners;
using NovaERP.Domain.Partners;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Partners;

public sealed class ListPartnersQueryHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Filtering_by_customer_also_returns_partners_that_are_both()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        db.Partners.Add(new Partner(_tenantId, "Solo Cliente", PartnerType.Customer));
        db.Partners.Add(new Partner(_tenantId, "Solo Proveedor", PartnerType.Supplier));
        db.Partners.Add(new Partner(_tenantId, "Ambos", PartnerType.Customer | PartnerType.Supplier));
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new ListPartnersQueryHandler(db);
        var result = await sut.Handle(new ListPartnersQuery(PartnerType.Customer), CancellationToken.None);

        result.Select(p => p.Name).Should().BeEquivalentTo(["Solo Cliente", "Ambos"]);
    }
}
