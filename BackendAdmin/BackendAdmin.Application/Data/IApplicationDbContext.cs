namespace BackendAdmin.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Menu> Menus { get; }
    DbSet<AppAdmin> Applications { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
}
