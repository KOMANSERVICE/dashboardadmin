using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TresorerieService.Domain.Abstractions;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Domain.Entities;

[Table("TC00003")]
public class Category : Entity<Guid>
{
    [Column("fld1")]
    public string ApplicationId { get; set; }

    [Column("fld2")]
    public string Name { get; set; }

    [Column("fld3")]
    public CategoryType Type { get; set; }

    [Column("fld4")]
    [StringLength(50)]
    public string? Icon { get; set; }

    [Column("fld5")]
    public bool IsActive { get; set; } = true;

    [Column("fld6")]
    public string BoutiqueId { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<CashFlow> CashFlows { get; set; } = new List<CashFlow>();
    public virtual ICollection<BudgetCategory> BudgetCategories { get; set; } = new List<BudgetCategory>();
}
