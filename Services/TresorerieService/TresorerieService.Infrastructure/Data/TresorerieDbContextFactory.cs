using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TresorerieService.Infrastructure.Data;

internal class TresorerieDbContextFactory : IDesignTimeDbContextFactory<TresorerieDbContext>
{
    public TresorerieDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TresorerieDbContext>();

        // Chaine de connexion "factice" ou minimale pour EF Core
        optionsBuilder.UseNpgsql("");

        return new TresorerieDbContext(optionsBuilder.Options);
    }
}
