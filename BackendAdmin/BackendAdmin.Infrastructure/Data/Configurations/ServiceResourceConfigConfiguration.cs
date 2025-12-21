namespace BackendAdmin.Infrastructure.Data.Configurations;

public class ServiceResourceConfigConfiguration : IEntityTypeConfiguration<ServiceResourceConfig>
{
    public void Configure(EntityTypeBuilder<ServiceResourceConfig> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ServiceName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.UlimitsJson)
            .HasMaxLength(4000);

        builder.HasIndex(e => e.ServiceName).IsUnique();
    }
}
