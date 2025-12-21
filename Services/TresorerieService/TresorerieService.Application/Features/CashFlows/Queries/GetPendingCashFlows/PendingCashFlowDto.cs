namespace TresorerieService.Application.Features.CashFlows.Queries.GetPendingCashFlows;

/// <summary>
/// DTO pour un flux en attente de validation
/// </summary>
public record PendingCashFlowDto(
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
    string? ThirdPartyName,
    DateTime? SubmittedAt,
    string? SubmittedBy
);
