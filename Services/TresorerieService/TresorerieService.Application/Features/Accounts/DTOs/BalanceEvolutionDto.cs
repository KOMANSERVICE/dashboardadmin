namespace TresorerieService.Application.Features.Accounts.DTOs;

/// <summary>
/// DTO pour un point d'evolution du solde sur une periode
/// </summary>
public record BalanceEvolutionDto(
    DateTime Date,
    decimal Balance,
    decimal TotalIncome,
    decimal TotalExpense
);
