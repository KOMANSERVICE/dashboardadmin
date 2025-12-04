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
            );

        builder.Property(e => e.StockMovementId)
            .HasConversion(
                stockMovementId => stockMovementId.Value,
                dbId => StockMovementId.Of(dbId)
            );

        builder.Property(e => e.ProductId)
            .IsRequired();

        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        // Relations
        builder.HasOne(e => e.StockSlip)
            .WithMany(s => s.StockSlipItems)
            .HasForeignKey(e => e.StockSlipId);

        builder.HasOne(e => e.StockMovement)
            .WithOne()
            .HasForeignKey<StockSlipItem>(e => e.StockMovementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}