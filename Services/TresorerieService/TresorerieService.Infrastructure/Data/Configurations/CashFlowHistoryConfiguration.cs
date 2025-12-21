using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Infrastructure.Data.Configurations;

public class CashFlowHistoryConfiguration : IEntityTypeConfiguration<CashFlowHistory>
{
    public void Configure(EntityTypeBuilder<CashFlowHistory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                Id => Id,
                Id => Id
            )
            .ValueGeneratedOnAdd();

        // CashFlowId est une string (pas une FK), donc on ignore la navigation
        // Cela evite la creation d'une colonne shadow CashFlowId1
        builder.Ignore(e => e.CashFlow);
    }
}
