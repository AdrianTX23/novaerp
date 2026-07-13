using FluentAssertions;
using NovaERP.Application.Features.Partners.CreatePartner;
using NovaERP.Domain.Partners;
using NovaERP.UnitTests.TestSupport;

namespace NovaERP.UnitTests.Features.Partners;

public sealed class CreatePartnerCommandHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Creates_a_partner_that_is_both_customer_and_supplier()
    {
        using var db = TestDbContextFactory.Create(_tenantId);
        var sut = new CreatePartnerCommandHandler(db, new FakeTenantProvider(_tenantId));

        var result = await sut.Handle(
            new CreatePartnerCommand(
                "Acme S.A.", PartnerType.Customer | PartnerType.Supplier, "20123456789", "hi@acme.com", null, null),
            CancellationToken.None);

        result.Type.Should().HaveFlag(PartnerType.Customer);
        result.Type.Should().HaveFlag(PartnerType.Supplier);
    }
}
