namespace BackendAdmin.Infrastructure.Data.Configurations;


public class AppAdminConfiguration : IEntityTypeConfiguration<AppAdmin>
{
    public void Configure(EntityTypeBuilder<AppAdmin> builder)
    {

        builder.HasKey(a => a.Id);
        builder
            .Property(a => a.Id)
            .HasConversion(
                applicatidonId => applicatidonId.Value,
                dbId => AppAdminId.Of(dbId)
            )
            .ValueGeneratedOnAdd();


    }
}

