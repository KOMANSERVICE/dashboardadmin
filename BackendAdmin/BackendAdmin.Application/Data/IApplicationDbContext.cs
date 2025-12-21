namespace BackendAdmin.Application.Data;

public interface IApplicationDbContext
{
    DbSet<AppAdmin> Applications { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ApiKey> ApiKeys { get; }
    DbSet<ServiceResourceConfig> ServiceResourceConfigs { get; }
}
