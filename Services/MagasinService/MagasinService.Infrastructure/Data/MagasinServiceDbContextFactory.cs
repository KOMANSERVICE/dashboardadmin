using Microsoft.EntityFrameworkCore.Design;

namespace MagasinService.Infrastructure.Data;

public class MagasinServiceDbContextFactory : IDesignTimeDbContextFactory<MagasinServiceDbContext>
{
    public MagasinServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MagasinServiceDbContext>();

        optionsBuilder.UseNpgsql("");

        return new MagasinServiceDbContext(optionsBuilder.Options);
    }
}

