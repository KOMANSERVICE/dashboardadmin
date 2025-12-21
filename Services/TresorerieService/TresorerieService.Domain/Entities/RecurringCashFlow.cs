using System.ComponentModel.DataAnnotations.Schema;
using TresorerieService.Domain.Abstractions;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Domain.Entities;

/// <summary>
/// Entite representant un flux de tresorerie recurrent.
/// Table: TR00001 (Tresorerie Recurring)
/// </summary>
[Table("TR00001")]
public class RecurringCashFlow : Entity<Guid>
{
    [Column("fld1")]
    public string ApplicationId { get; set; } = string.Empty;

    [Column("fld2")]
    public string BoutiqueId { get; set; } = string.Empty;

    /// <summary>
    /// Type de flux: INCOME ou EXPENSE uniquement (pas TRANSFER)
    /// </summary>
    [Column("fld3")]
    public CashFlowType Type { get; set; }

    /// <summary>
    /// Identifiant de la categorie
    /// </summary>
    [Column("fld4")]
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// Libelle du flux recurrent
    /// </summary>
    [Column("fld5")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Description optionnelle
    /// </summary>
    [Column("fld6")]
    public string? Description { get; set; }

    /// <summary>
    /// Montant de chaque occurrence
    /// </summary>
    [Column("fld7", TypeName = "decimal(15,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Compte source
    /// </summary>
    [Column("fld8")]
    public Guid AccountId { get; set; }

    /// <summary>
    /// Mode de paiement
    /// </summary>
    [Column("fld9")]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Nom du tiers (fournisseur ou client)
    /// </summary>
    [Column("fld10")]
    public string? ThirdPartyName { get; set; }

    /// <summary>
    /// Frequence de recurrence: DAILY, WEEKLY, MONTHLY, QUARTERLY, YEARLY
    /// </summary>
    [Column("fld11")]
    public RecurringFrequency Frequency { get; set; }

    /// <summary>
    /// Intervalle de recurrence (ex: tous les 2 mois = interval=2 avec Frequency=MONTHLY)
    /// </summary>
    [Column("fld12")]
    public int Interval { get; set; } = 1;

    /// <summary>
    /// Jour du mois pour frequence MONTHLY (1-31)
    /// </summary>
    [Column("fld13")]
    public int? DayOfMonth { get; set; }

    /// <summary>
    /// Jour de la semaine pour frequence WEEKLY (1-7, 1=Lundi)
    /// </summary>
    [Column("fld14")]
    public int? DayOfWeek { get; set; }

    /// <summary>
    /// Date de debut de la recurrence
    /// </summary>
    [Column("fld15")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Date de fin optionnelle de la recurrence
    /// </summary>
    [Column("fld16")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Prochaine date d'occurrence (calculee automatiquement)
    /// </summary>
    [Column("fld17")]
    public DateTime NextOccurrence { get; set; }

    /// <summary>
    /// Si true, les flux generes sont automatiquement approuves
    /// </summary>
    [Column("fld18")]
    public bool AutoValidate { get; set; } = false;

    /// <summary>
    /// Indique si le flux recurrent est actif
    /// </summary>
    [Column("fld19")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date de derniere generation d'un flux
    /// </summary>
    [Column("fld20")]
    public DateTime? LastGeneratedAt { get; set; }

    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual Account? Account { get; set; }
    public virtual ICollection<CashFlow> GeneratedCashFlows { get; set; } = new List<CashFlow>();
}
