namespace TresorerieService.Application.Features.Accounts.DTOs;

public record UpdateAccountDTO(
    string Name,
    decimal? AlertThreshold,
    decimal? OverdraftLimit,
    bool IsActive,
    decimal? InitialBalance = null
);
