namespace MagasinService.Infrastructure.Data.Configurations;

public class StockLocationConfiguration : IEntityTypeConfiguration<StockLocation>
{
    public void Configure(EntityTypeBuilder<StockLocation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                stockLocationId => stockLocationId.Value,
                dbId => StockLocationId.Of(dbId)
            )
            .ValueGeneratedOnAdd();

        builder.HasMany(e => e.SourceMovements)
            .WithOne(e => e.SourceLocation)
            .HasForeignKey(e => e.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.DestinationMovements)
            .WithOne(e => e.DestinationLocation)
            .HasForeignKey(e => e.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.SourceSlips)
            .WithOne(e => e.SourceLocation)
            .HasForeignKey(e => e.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.DestinationSlips)
            .WithOne(e => e.DestinationLocation!)
            .HasForeignKey(e => e.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
