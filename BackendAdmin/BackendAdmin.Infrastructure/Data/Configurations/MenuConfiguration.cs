using BackendAdmin.Domain.ValueObjects;

namespace BackendAdmin.Infrastructure.Data.Configurations;


public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                menuId => menuId.Value,
                dbId => MenuId.Of(dbId)
            )
            .ValueGeneratedOnAdd();

        builder.Property(e => e.AppAdminId)
            .HasConversion(
                appAdminId => appAdminId.Value,
                dbId => AppAdminId.Of(dbId)
            )
            .IsRequired();

        builder.HasOne(e => e.AppAdmin)
            .WithMany(b => b.Menus)
            .HasForeignKey(e => e.AppAdminId)
            .OnDelete(DeleteBehavior.Restrict);


    }
}

