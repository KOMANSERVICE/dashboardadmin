using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.RecurringCashFlows.DTOs;

/// <summary>
/// DTO pour la modification d'un flux de tresorerie recurrent.
/// Champs NON modifiables: Id, CreatedBy, CreatedAt
/// Si la frequence est modifiee, NextOccurrence sera recalculee automatiquement.
/// </summary>
public record UpdateRecurringCashFlowDto(
    CashFlowType? Type,
    string? CategoryId,
    string? Label,
    string? Description,
    decimal? Amount,
    Guid? AccountId,
    string? PaymentMethod,
    string? ThirdPartyName,
    RecurringFrequency? Frequency,
    int? Interval,
    int? DayOfMonth,
    int? DayOfWeek,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? AutoValidate,
    bool? IsActive
);
