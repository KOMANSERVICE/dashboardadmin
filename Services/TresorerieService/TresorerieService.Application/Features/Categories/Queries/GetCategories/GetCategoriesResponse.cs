using TresorerieService.Application.Features.Categories.DTOs;

namespace TresorerieService.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Reponse pour la liste des categories de tresorerie
/// </summary>
public record GetCategoriesResponse(
    IReadOnlyList<CategoryDTO> Categories,
    int TotalCount
);
