using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlowFromPurchase;

/// <summary>
/// Commande pour creer automatiquement un flux de tresorerie (CashFlow)
/// quand un achat est valide.
/// Cette commande est appelee par le service Achats (logique interne).
///
/// Criteres d'acceptation:
/// - Type = EXPENSE
/// - Status = APPROVED (achat valide = decaissement confirme)
/// - Le compte est debite automatiquement
/// - RelatedType = "PURCHASE", RelatedId = purchaseId
/// - IsSystemGenerated = true
/// - Label = "Achat #[reference]"
/// - ThirdPartyType = SUPPLIER
/// </summary>
public record CreateCashFlowFromPurchaseCommand(
    string ApplicationId,
    string BoutiqueId,
    Guid PurchaseId,
    string PurchaseReference,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    DateTime PurchaseDate,
    string? SupplierName,
    string? SupplierId,
    string CategoryId
) : ICommand<CreateCashFlowFromPurchaseResult>;

public record CreateCashFlowFromPurchaseResult(
    CashFlowDTO CashFlow,
    decimal NewAccountBalance
);
