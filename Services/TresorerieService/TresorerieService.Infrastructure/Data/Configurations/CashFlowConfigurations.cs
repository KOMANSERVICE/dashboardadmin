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
    }
}
