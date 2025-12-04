namespace MagasinService.Infrastructure.Data.Configurations;

public class StockSlipItemConfiguration : IEntityTypeConfiguration<StockSlipItem>
{
    public void Configure(EntityTypeBuilder<StockSlipItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                stockSlipItemId => stockSlipItemId.Value,
                dbId => StockSlipItemId.Of(dbId)
            )
            .ValueGeneratedOnAdd();

        builder.Property(e => e.StockSlipId)
            .HasConversion(
                stockSlipId => stockSlipId.Value,
                dbId => StockSlipId.Of(dbId)
            )
            .IsRequired();

        builder.Property(e => e.ProductId)
            .IsRequired();

        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.Note)
            .HasMaxLength(250);

        builder.HasOne(e => e.StockSlip)
            .WithMany(e => e.StockSlipItems)
            .HasForeignKey(e => e.StockSlipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.StockSlipId, e.ProductId });
    }
}