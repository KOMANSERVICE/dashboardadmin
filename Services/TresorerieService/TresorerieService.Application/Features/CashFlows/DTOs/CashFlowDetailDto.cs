namespace TresorerieService.Application.Features.CashFlows.DTOs;

/// <summary>
/// DTO complet pour le detail d'un flux de tresorerie
/// Inclut toutes les informations du flux avec les noms des relations
/// </summary>
public record CashFlowDetailDto(
    // Identifiants
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string? Reference,

    // Type et statut
    CashFlowType Type,
    CashFlowStatus Status,

    // Categorie avec nom
    string CategoryId,
    string CategoryName,

    // Informations principales
    string Label,
    string? Description,

    // Montants
    decimal Amount,
    decimal TaxAmount,
    decimal TaxRate,
    string Currency,

    // Compte source avec nom
    Guid AccountId,
    string AccountName,

    // Compte destination (pour TRANSFER) avec nom
    Guid? DestinationAccountId,
    string? DestinationAccountName,

    // Paiement
    string PaymentMethod,
    DateTime Date,

    // Tiers
    ThirdPartyType? ThirdPartyType,
    string? ThirdPartyName,
    string? ThirdPartyId,

    // Pieces jointes
    string? AttachmentUrl,

    // Lien systeme externe
    string? RelatedType,
    string? RelatedId,

    // Flags systeme
    bool IsRecurring,
    string? RecurringCashFlowId,
    bool IsSystemGenerated,
    bool AutoApproved,

    // Reconciliation bancaire
    bool IsReconciled,
    DateTime? ReconciledAt,
    string? ReconciledBy,
    string? BankStatementReference,

    // Audit - Creation
    DateTime CreatedAt,
    string CreatedBy,

    // Audit - Soumission
    DateTime? SubmittedAt,
    string? SubmittedBy,

    // Audit - Validation/Rejet
    DateTime? ValidatedAt,
    string? ValidatedBy,
    string? RejectionReason,

    // Budget (non implemente - toujours null pour l'instant)
    string? BudgetId,
    string? BudgetName,
    decimal? BudgetImpact
);
