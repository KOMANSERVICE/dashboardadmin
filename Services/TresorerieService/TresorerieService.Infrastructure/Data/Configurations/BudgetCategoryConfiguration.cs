using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Infrastructure.Data.Configurations;

internal class BudgetCategoryConfiguration : IEntityTypeConfiguration<BudgetCategory>
{
    public void Configure(EntityTypeBuilder<BudgetCategory> builder)
    {
        // Clé composite (BudgetId, CategoryId)
        builder.HasKey(bc => new { bc.BudgetId, bc.CategoryId });

        // Configuration de la relation avec Budget
        builder.HasOne(bc => bc.Budget)
            .WithMany(b => b.BudgetCategories)
            .HasForeignKey(bc => bc.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuration de la relation avec Category
        builder.HasOne(bc => bc.Category)
            .WithMany(c => c.BudgetCategories)
            .HasForeignKey(bc => bc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
