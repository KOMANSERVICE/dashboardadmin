using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Infrastructure.Data.Configurations;

public class CashFlowConfigurations : IEntityTypeConfiguration<CashFlow>
{
    public void Configure(EntityTypeBuilder<CashFlow> builder)
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
            .WithMany(l => l.CashFlows)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DestinationAccount)
            .WithMany(l => l.DestinationCashFlows)
            .HasForeignKey(e => e.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // RecurringCashFlowId est une string (pas un FK), donc on ignore la navigation
        builder.Ignore(e => e.RecurringCashFlow);

        // CategoryId est une string (pas une FK), donc on ignore la navigation
        // Cela evite la creation d'une colonne shadow CategoryId1
        builder.Ignore(e => e.Category);

        // La relation avec CashFlowHistory utilise CashFlowId comme string, pas comme FK
        // On ignore la navigation pour eviter la creation d'une colonne shadow
        builder.Ignore(e => e.History);
    }
}
