namespace MagasinService.Infrastructure.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                stockMovementId => stockMovementId.Value,
                dbId => StockMovementId.Of(dbId)
            )
            .ValueGeneratedOnAdd();

        builder.Property(e => e.SourceLocationId)
            .HasConversion(
                stockLocationId => stockLocationId.Value,
                dbId => StockLocationId.Of(dbId)
            );

        builder.Property(e => e.DestinationLocationId)
            .HasConversion(
                stockLocationId => stockLocationId.Value,
                dbId => StockLocationId.Of(dbId)
            );

        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.Reference)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.MovementType)
            .IsRequired();

        builder.Property(e => e.ProductId)
            .IsRequired();

        // Relations
        builder.HasOne(e => e.SourceLocation)
            .WithMany(l => l.SourceMovements)
            .HasForeignKey(e => e.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DestinationLocation)
            .WithMany(l => l.DestinationMovements)
            .HasForeignKey(e => e.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}