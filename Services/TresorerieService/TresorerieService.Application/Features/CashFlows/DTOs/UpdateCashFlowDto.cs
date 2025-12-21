namespace TresorerieService.Application.Features.CashFlows.DTOs;

/// <summary>
/// DTO pour la modification d'un flux de tresorerie en brouillon.
/// Les champs non modifiables sont: Id, Type, CreatedBy, CreatedAt.
/// </summary>
public record UpdateCashFlowDto(
    string? CategoryId,
    string? Label,
    string? Description,
    decimal? Amount,
    decimal? TaxAmount,
    decimal? TaxRate,
    string? Currency,
    Guid? AccountId,
    Guid? DestinationAccountId,
    string? PaymentMethod,
    DateTime? Date,
    ThirdPartyType? ThirdPartyType,
    string? ThirdPartyName,
    string? ThirdPartyId,
    string? AttachmentUrl
);
