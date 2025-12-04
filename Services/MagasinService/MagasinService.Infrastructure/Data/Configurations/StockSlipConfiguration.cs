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

        builder.Property(e => e.SlipType)
            .IsRequired();

        builder.Property(e => e.SourceLocationId)
            .HasConversion(
                stockLocationId => stockLocationId.Value,
                dbId => StockLocationId.Of(dbId)
            )
            .IsRequired();

        builder.Property(e => e.DestinationLocationId)
            .HasConversion(
                stockLocationId => stockLocationId != null ? stockLocationId.Value : (Guid?)null,
                dbId => dbId.HasValue ? StockLocationId.Of(dbId.Value) : null
            );

        builder.HasOne(e => e.SourceLocation)
            .WithMany(e => e.SourceSlips)
            .HasForeignKey(e => e.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DestinationLocation)
            .WithMany(e => e.DestinationSlips)
            .HasForeignKey(e => e.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.StockSlipItems)
            .WithOne(e => e.StockSlip)
            .HasForeignKey(e => e.StockSlipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Reference).IsUnique();
        builder.HasIndex(e => e.Date);
        builder.HasIndex(e => e.BoutiqueId);
    }
}