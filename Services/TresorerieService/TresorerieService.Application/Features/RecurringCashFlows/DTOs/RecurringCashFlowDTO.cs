using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.RecurringCashFlows.DTOs;

public record RecurringCashFlowDTO(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    CashFlowType Type,
    string CategoryId,
    string CategoryName,
    string Label,
    string? Description,
    decimal Amount,
    Guid AccountId,
    string AccountName,
    string PaymentMethod,
    string? ThirdPartyName,
    RecurringFrequency Frequency,
    int Interval,
    int? DayOfMonth,
    int? DayOfWeek,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime NextOccurrence,
    bool AutoValidate,
    bool IsActive,
    DateTime? LastGeneratedAt,
    DateTime CreatedAt,
    string CreatedBy
);
