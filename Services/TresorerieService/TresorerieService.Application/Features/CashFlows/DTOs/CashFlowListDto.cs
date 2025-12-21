namespace TresorerieService.Application.Features.CashFlows.DTOs;

/// <summary>
/// DTO pour l'affichage d'un flux dans la liste (version allegee)
/// </summary>
public record CashFlowListDto(
    Guid Id,
    string? Reference,
    CashFlowType Type,
    CashFlowStatus Status,
    string CategoryId,
    string CategoryName,
    string Label,
    decimal Amount,
    string Currency,
    Guid AccountId,
    string AccountName,
    Guid? DestinationAccountId,
    string? DestinationAccountName,
    string PaymentMethod,
    DateTime Date,
    string? ThirdPartyName,
    DateTime CreatedAt,
    string CreatedBy
);
