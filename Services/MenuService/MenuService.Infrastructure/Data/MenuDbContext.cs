namespace MenuService.Infrastructure.Data;

public class MenuDbContext : DbContext, IMenuDbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options)
        : base(options)
    {
    }

    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
}
