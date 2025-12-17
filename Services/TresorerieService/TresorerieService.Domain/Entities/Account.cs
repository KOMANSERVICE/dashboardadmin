using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TresorerieService.Domain.Abstractions;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Domain.Entities;

[Table("TA00001")]

public class Account : Entity<Guid>
{
    [Column("fld1")]
    public string ApplicationId { get; set; }

    [Column("fld2")]
    public string BoutiqueId { get; set; }

    // Informations du compte
    [Column("fld3")]
    public string Name { get; set; }

    [Column("fld14")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("fld4")]
    public AccountType Type { get; set; }

    [Column("fld5")]
    [StringLength(150)]
    public string? AccountNumber { get; set; }

    [Column("fld6")]
    [StringLength(100)]
    public string? BankName { get; set; }

    // Soldes
    [Column("fld7", TypeName = "decimal(15,2)")]
    public decimal InitialBalance { get; set; } = 0;

    [Column("fld8", TypeName = "decimal(15,2)")]
    public decimal CurrentBalance { get; set; } = 0;

    [Column("fld9")]
    [StringLength(3)]
    public string Currency { get; set; } = "XOF"; // Code ISO 4217 du Franc CFA BCEAO

    // Configuration
    [Column("fld10")]
    public bool IsActive { get; set; } = true;

    [Column("fld11")]
    public bool IsDefault { get; set; } = false;

    // Limites
    [Column("fld12", TypeName = "decimal(15,2)")]
    public decimal? OverdraftLimit { get; set; }

    [Column("fld13", TypeName = "decimal(15,2)")]
    public decimal? AlertThreshold { get; set; }

    // Navigation properties
    public virtual ICollection<CashFlow> CashFlows { get; set; }
    public virtual ICollection<CashFlow> DestinationCashFlows { get; set; }
}
