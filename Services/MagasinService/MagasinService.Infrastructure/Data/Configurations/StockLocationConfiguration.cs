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

       
    }
}
