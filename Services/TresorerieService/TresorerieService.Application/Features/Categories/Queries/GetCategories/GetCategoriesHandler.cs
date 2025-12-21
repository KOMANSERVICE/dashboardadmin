using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Categories.DTOs;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesHandler(IGenericRepository<Category> categoryRepository)
    : IQueryHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    public async Task<GetCategoriesResponse> Handle(
        GetCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        // Recuperer les categories selon les filtres
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.ApplicationId == query.ApplicationId
                 && c.BoutiqueId == query.BoutiqueId
                 && (query.IncludeInactive || c.IsActive)
                 && (!query.Type.HasValue || c.Type == query.Type.Value),
            cancellationToken);

        // Mapper vers les DTOs et trier par nom
        var categoryDtos = categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDTO(
                Id: c.Id,
                ApplicationId: c.ApplicationId,
                BoutiqueId: c.BoutiqueId,
                Name: c.Name,
                Type: c.Type,
                Icon: c.Icon,
                IsActive: c.IsActive,
                CreatedAt: c.CreatedAt,
                UpdatedAt: c.UpdatedAt
            ))
            .ToList();

        return new GetCategoriesResponse(
            Categories: categoryDtos,
            TotalCount: categoryDtos.Count
        );
    }
}
