using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TresorerieService.Domain.Abstractions;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Domain.Entities;

/// <summary>
/// Entité représentant une méthode de paiement.
/// Table: TP00001 (Tresorerie PaymentMethod)
/// </summary>
[Table("TP00001")]
public class PaymentMethod : Entity<Guid>
{
    /// <summary>
    /// Identifiant de l'application
    /// </summary>
    [Column("fld1")]
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant de la boutique
    /// </summary>
    [Column("fld2")]
    public string BoutiqueId { get; set; } = string.Empty;

    /// <summary>
    /// Nom de la méthode de paiement
    /// </summary>
    [Column("fld3")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type de méthode de paiement (CASH, CARD, TRANSFER, CHECK, MOBILE)
    /// </summary>
    [Column("fld4")]
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Indique si cette méthode est la méthode par défaut
    /// </summary>
    [Column("fld5")]
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Indique si cette méthode est active
    /// </summary>
    [Column("fld6")]
    public bool IsActive { get; set; } = true;
}
