using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TresorerieService.Domain.Entities;

/// <summary>
/// Table de jointure entre Budget et Category.
/// Permet d'associer plusieurs catégories EXPENSE à un budget.
/// </summary>
[Table("TB00002")]
public class BudgetCategory
{
    [Column("budget_id")]
    public Guid BudgetId { get; set; }

    [Column("category_id")]
    public Guid CategoryId { get; set; }

    // Navigation properties
    public virtual Budget Budget { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
}
