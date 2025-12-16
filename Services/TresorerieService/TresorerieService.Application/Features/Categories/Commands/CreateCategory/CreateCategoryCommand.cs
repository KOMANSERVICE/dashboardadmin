using TresorerieService.Application.Features.Categories.DTOs;

namespace TresorerieService.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string ApplicationId,
    string Name,
    CategoryType Type,
    string? Icon
) : ICommand<CreateCategoryResult>;

public record CreateCategoryResult(CategoryDTO Category);
