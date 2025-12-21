using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlowFromSale;

/// <summary>
/// Commande pour creer automatiquement un flux de tresorerie (CashFlow)
/// quand une vente est validee.
/// Cette commande est appelee par le service Ventes (logique interne).
/// </summary>
public record CreateCashFlowFromSaleCommand(
    string ApplicationId,
    string BoutiqueId,
    Guid SaleId,
    string SaleReference,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    DateTime SaleDate,
    string? CustomerName,
    string? CustomerId,
    string CategoryId
) : ICommand<CreateCashFlowFromSaleResult>;

public record CreateCashFlowFromSaleResult(
    CashFlowDTO CashFlow,
    decimal NewAccountBalance
);
