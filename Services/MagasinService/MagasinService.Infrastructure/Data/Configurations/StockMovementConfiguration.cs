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

        builder.Property(e => e.SourceLocationId)
            .HasConversion(
                stockLocationId => stockLocationId.Value,
                dbId => StockLocationId.Of(dbId)
            )
            .IsRequired();

        builder.Property(e => e.DestinationLocationId)
            .HasConversion(
                stockLocationId => stockLocationId.Value,
                dbId => StockLocationId.Of(dbId)
            )
            .IsRequired();

        builder.HasOne(e => e.SourceLocation)
            .WithMany(e => e.SourceMovements)
            .HasForeignKey(e => e.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DestinationLocation)
            .WithMany(e => e.DestinationMovements)
            .HasForeignKey(e => e.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Reference);
        builder.HasIndex(e => e.Date);
        builder.HasIndex(e => e.ProductId);
    }
}