using Menu.Grpc.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Grpc.Data.Configurations;

public class MenuComfiguration : IEntityTypeConfiguration<MenuNav>
{
    public void Configure(EntityTypeBuilder<MenuNav> builder)
    {
        builder.HasKey(mc => mc.Id);
        builder.Property(mc => mc.Id)
            .HasConversion(
                id => id,
                id => id)
            .ValueGeneratedOnAdd();
    }
}
