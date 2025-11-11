namespace MenuService.Infrastructure.Data;

public class MenuDbContext : DbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options)
        : base(options)
    {
    }
    public DbSet<Menu> Menus => Set<Menu>();
}
