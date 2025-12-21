
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using TresorerieService.Domain.Abstractions;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Domain.Entities;

[Table("TC00001")]
public class CashFlow : Entity<Guid>
{
    [Column("fld1")]
    public string ApplicationId { get; set; }

    [Column("fld2")]
    public string BoutiqueId { get; set; }

    [Column("fld3")]
    public string? Reference { get; set; }

    [Column("fld4")]
    public CashFlowType Type { get; set; }

    [Column("fld5")]
    public string CategoryId { get; set; }

    [Column("fld6")]
    public string Label { get; set; }

    [Column("fld7")]
    public string? Description { get; set; }

    // Montants
    [Column("fld8", TypeName = "decimal(15,2)")]
    public decimal Amount { get; set; }

    [Column("fld9", TypeName = "decimal(15,2)")]
    public decimal TaxAmount { get; set; } = 0;

    [Column("fld10", TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; } = 0;

    [Column("fld11")]
    public string Currency { get; set; } = "XOF"; // Code ISO 4217 du Franc CFA BCEAO

    // Comptes
    [Column("fld12")]
    public Guid AccountId { get; set; }

    [Column("fld13")]
    public Guid? DestinationAccountId { get; set; }

    // Paiement
    [Column("fld14")]
    public string PaymentMethod { get; set; }

    [Column("fld15")]
    public DateTime Date { get; set; }

    // Tiers
    [Column("fld16")]
    public ThirdPartyType? ThirdPartyType { get; set; }

    [Column("fld17")]
    public string? ThirdPartyName { get; set; }

    [Column("fld18")]
    public string? ThirdPartyId { get; set; }

    // Workflow
    [Column("fld19")]
    public CashFlowStatus Status { get; set; } = CashFlowStatus.DRAFT;

    [Column("fld20")]
    public DateTime? SubmittedAt { get; set; }

    [Column("fld21")]
    public string? SubmittedBy { get; set; }

    [Column("fld22")]
    public DateTime? ValidatedAt { get; set; }

    [Column("fld23")]
    public string? ValidatedBy { get; set; }

    [Column("fld24")]
    public string? RejectionReason { get; set; }

    // Réconciliation bancaire
    [Column("fld25")]
    public bool IsReconciled { get; set; } = false;

    [Column("fld26")]
    public DateTime? ReconciledAt { get; set; }

    [Column("fld27")]
    public string? ReconciledBy { get; set; }

    [Column("fld28")]
    public string? BankStatementReference { get; set; }

    // Récurrence
    [Column("fld29")]
    public bool IsRecurring { get; set; } = false;

    [Column("fld30")]
    public string? RecurringCashFlowId { get; set; }

    // Budget (seulement pour EXPENSE)
    //[Column("budget_id")]
    //public string? BudgetId { get; set; }


    [Column("fld31")]
    public string? AttachmentUrl { get; set; }

    // Lien système externe
    [Column("fld32")]
    public string? RelatedType { get; set; }

    [Column("fld33")]
    public string? RelatedId { get; set; }

    // Système
    [Column("fld34")]
    public bool IsSystemGenerated { get; set; } = false;

    [Column("fld35")]
    public bool AutoApproved { get; set; } = false;

    public virtual Category Category { get; set; }

    public virtual Account Account { get; set; }

    public virtual Account? DestinationAccount { get; set; }

    public virtual RecurringCashFlow? RecurringCashFlow { get; set; }

    // public virtual Budget? Budget { get; set; }

    public virtual ICollection<CashFlowHistory> History { get; set; }
}