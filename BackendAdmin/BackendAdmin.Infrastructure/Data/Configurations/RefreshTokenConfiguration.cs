

namespace BackendAdmin.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {

        builder.HasKey(a => a.Id);
        builder
            .Property(a => a.Id)
            .ValueGeneratedOnAdd();


    }
}