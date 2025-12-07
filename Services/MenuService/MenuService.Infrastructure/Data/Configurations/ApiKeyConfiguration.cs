using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuService.Infrastructure.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasKey(ak => ak.Id);

        builder.Property(ak => ak.Id)
            .HasConversion(id => id, id => id)
            .ValueGeneratedOnAdd();

        builder.Property(ak => ak.ApiKeyHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(ak => ak.ApplicationId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ak => ak.ApplicationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ak => ak.Scopes)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ak => ak.RevokedReason)
            .HasMaxLength(500);

        builder.HasIndex(ak => ak.ApiKeyHash)
            .IsUnique();

        builder.HasIndex(ak => ak.ApplicationId);
    }
}
