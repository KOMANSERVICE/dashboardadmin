using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.DTOs;

/// <summary>
/// DTO pour un mouvement de tresorerie (entree/sortie) sur un compte
/// </summary>
public record CashFlowMovementDto(
    Guid Id,
    string? Reference,
    CashFlowType Type,
    string Label,
    string? Description,
    decimal Amount,
    string Currency,
    DateTime Date,
    CashFlowStatus Status,
    string? ThirdPartyName
);
