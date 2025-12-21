using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(mc => mc.Id);
        builder.Property(mc => mc.Id)
            .HasConversion(
                id => id,
                id => id)
            .ValueGeneratedOnAdd();
    }
}
