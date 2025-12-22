using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TresorerieService.Domain.Abstractions;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Domain.Entities;

[Table("TB00001")]
public class Budget :Entity<Guid>
{

    // Identification
    [Required]
    [Column("application_id")]
    [StringLength(36)]
    public string ApplicationId { get; set; }

    [Required]
    [Column("boutique_id")]
    [StringLength(36)]
    public string BoutiqueId { get; set; }

    // Période et description
    [Required]
    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("end_date")]
    public DateTime EndDate { get; set; }

    // Montants
    [Required]
    [Column("allocated_amount", TypeName = "decimal(15,2)")]
    public decimal AllocatedAmount { get; set; }

    [Column("spent_amount", TypeName = "decimal(15,2)")]
    public decimal SpentAmount { get; set; } = 0;

    [NotMapped]
    public decimal RemainingAmount => AllocatedAmount - SpentAmount;

[Column("currency")]
    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    // Configuration
    [Column("type")]
    public BudgetType Type { get; set; } = BudgetType.CATEGORY;

    [Column("alert_threshold")]
    public int AlertThreshold { get; set; } = 80;

    [Column("is_exceeded")]
    public bool IsExceeded { get; set; } = false;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;


    

    // Navigation properties
    public virtual ICollection<CashFlow> Expenses { get; set; }
public virtual ICollection<BudgetCategory> BudgetCategories { get; set; }

    
  

}
