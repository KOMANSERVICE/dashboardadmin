using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TresorerieService.Domain.Abstractions;

namespace TresorerieService.Domain.Entities;

/// <summary>
/// Historique des alertes budgetaires generees lors du depassement de seuil
/// </summary>
[Table("TB00003")]
public class BudgetAlert : Entity<Guid>
{
    [Required]
    [Column("fld1")]
    public Guid BudgetId { get; set; }

    [Required]
    [Column("fld2")]
    public Guid CashFlowId { get; set; }

    /// <summary>
    /// Type d'alerte: THRESHOLD_REACHED (seuil atteint) ou EXCEEDED (budget depasse)
    /// </summary>
    [Required]
    [Column("fld3")]
    [StringLength(50)]
    public string AlertType { get; set; }

    /// <summary>
    /// Montant depense au moment de l'alerte
    /// </summary>
    [Required]
    [Column("fld4", TypeName = "decimal(15,2)")]
    public decimal SpentAmountAtAlert { get; set; }

    /// <summary>
    /// Montant alloue au moment de l'alerte
    /// </summary>
    [Required]
    [Column("fld5", TypeName = "decimal(15,2)")]
    public decimal AllocatedAmountAtAlert { get; set; }

    /// <summary>
    /// Taux de consommation au moment de l'alerte (pourcentage)
    /// </summary>
    [Column("fld6", TypeName = "decimal(5,2)")]
    public decimal ConsumptionRate { get; set; }

    /// <summary>
    /// Seuil d'alerte configure au moment de l'alerte
    /// </summary>
    [Column("fld7")]
    public int ThresholdAtAlert { get; set; }

    /// <summary>
    /// Message d'alerte genere
    /// </summary>
    [Column("fld8")]
    [StringLength(500)]
    public string? Message { get; set; }

    /// <summary>
    /// Indique si l'alerte a ete lue/acquittee
    /// </summary>
    [Column("fld9")]
    public bool IsAcknowledged { get; set; } = false;

    /// <summary>
    /// Date d'acquittement de l'alerte
    /// </summary>
    [Column("fld10")]
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Utilisateur ayant acquitte l'alerte
    /// </summary>
    [Column("fld11")]
    [StringLength(255)]
    public string? AcknowledgedBy { get; set; }

    // Navigation properties
    public virtual Budget Budget { get; set; } = null!;
    public virtual CashFlow CashFlow { get; set; } = null!;
}
