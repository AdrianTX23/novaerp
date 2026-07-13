using MediatR;

namespace NovaERP.Application.Features.Catalog.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId) : IRequest;
