using Microsoft.EntityFrameworkCore.Design;

namespace BackendAdmin.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Chaine de connexion "factice" ou minimale pour EF Core
        optionsBuilder.UseNpgsql("");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

