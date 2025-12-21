using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Infrastructure.Data.Configurations;

public class RecurringCashFlowConfiguration : IEntityTypeConfiguration<RecurringCashFlow>
{
    public void Configure(EntityTypeBuilder<RecurringCashFlow> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                Id => Id,
                Id => Id
            )
            .ValueGeneratedOnAdd()
            .HasColumnName("Id");

        // Relations
        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // CategoryId est une string (pas une FK), donc on ignore la navigation
        builder.Ignore(e => e.Category);

        // La relation avec CashFlow est geree via le champ RecurringCashFlowId (string)
        // qui stocke l'ID sous forme de string pour etre flexible
        builder.Ignore(e => e.GeneratedCashFlows);
    }
}
