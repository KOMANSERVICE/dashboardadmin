namespace BackendAdmin.Infrastructure.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.ApiKeyHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.ApplicationId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ApplicationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Scopes)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.RevokedReason)
            .HasMaxLength(500);

        builder.HasIndex(a => a.ApiKeyHash).IsUnique();
        builder.HasIndex(a => a.ApplicationId);
    }
}
