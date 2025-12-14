using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Infrastructure.Data.Configurations;

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(mc => mc.Id);
        builder.Property(mc => mc.Id)
            .HasConversion(
                id => id,
                id => id)
            .ValueGeneratedOnAdd();
    }
}