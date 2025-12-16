using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Categories.DTOs;

public record CategoryDTO(
    Guid Id,
    string ApplicationId,
    string Name,
    CategoryType Type,
    string? Icon,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
