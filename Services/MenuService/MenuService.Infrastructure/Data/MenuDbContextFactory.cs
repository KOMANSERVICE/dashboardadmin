using Microsoft.EntityFrameworkCore.Design;

namespace MenuService.Infrastructure.Data;

public class MenuDbContextFactory : IDesignTimeDbContextFactory<MenuDbContext>
{
    public MenuDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MenuDbContext>();

        // Chaine de connexion "factice" ou minimale pour EF Core
        optionsBuilder.UseNpgsql("");

        return new MenuDbContext(optionsBuilder.Options);
    }
}