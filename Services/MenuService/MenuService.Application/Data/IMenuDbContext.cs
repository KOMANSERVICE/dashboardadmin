namespace MenuService.Application.Data;

public interface IMenuDbContext
{
    DbSet<Menu> Menus { get; }
}
