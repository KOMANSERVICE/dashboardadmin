using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Infrastructure.Data.Configurations;

internal class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasConversion(
                id => id,
                id => id)
            .ValueGeneratedOnAdd();

        // Ignorer la propriété calculée RemainingAmount
        builder.Ignore(b => b.RemainingAmount);

        // Configuration de la relation avec BudgetCategory
        builder.HasMany(b => b.BudgetCategories)
            .WithOne(bc => bc.Budget)
            .HasForeignKey(bc => bc.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignorer la navigation Expenses (CashFlow utilise BudgetId comme string, pas FK)
        builder.Ignore(b => b.Expenses);
    }
}
