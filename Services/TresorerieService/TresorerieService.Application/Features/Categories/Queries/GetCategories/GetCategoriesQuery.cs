using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Query pour recuperer la liste des categories de tresorerie
/// </summary>
public record GetCategoriesQuery(
    string ApplicationId,
    CategoryType? Type = null,
    bool IncludeInactive = false
) : IQuery<GetCategoriesResponse>;
