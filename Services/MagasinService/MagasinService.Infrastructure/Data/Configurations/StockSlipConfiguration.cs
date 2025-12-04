namespace MagasinService.Infrastructure.Data.Configurations;

public class StockSlipConfiguration : IEntityTypeConfiguration<StockSlip>
{
    public void Configure(EntityTypeBuilder<StockSlip> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                stockSlipId => stockSlipId.Value,
                dbId => StockSlipId.Of(dbId)
            )
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Reference)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        builder.Property(e => e.BoutiqueId)
            .IsRequired();

        builder.Property(e => e.IsInbound)
            .IsRequired();

        // Relations
        builder.HasMany(e => e.StockSlipItems)
            .WithOne(i => i.StockSlip)
            .HasForeignKey(i => i.StockSlipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}