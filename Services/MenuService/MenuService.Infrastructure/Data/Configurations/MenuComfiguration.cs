using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuService.Infrastructure.Data.Configurations;

public class MenuComfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.HasKey(mc => mc.Id);
        builder.Property(mc => mc.Id)
            .HasConversion(
                id => id,
                id => id)
            .ValueGeneratedOnAdd();
    }
}
