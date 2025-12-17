using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.CashFlows.Queries.GetUnreconciledCashFlows;

/// <summary>
/// DTO pour un flux non réconcilié
/// </summary>
public record UnreconciledCashFlowDto(
    Guid Id,
    string? Reference,
    CashFlowType Type,
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
    ThirdPartyType? ThirdPartyType,
    string? ThirdPartyName,
    DateTime? ValidatedAt,
    string? ValidatedBy
);
