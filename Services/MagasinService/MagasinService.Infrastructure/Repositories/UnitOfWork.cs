using Microsoft.EntityFrameworkCore.Storage;

namespace MagasinService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MagasinServiceDbContext _dbContext;

    public UnitOfWork(MagasinServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesDataAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _dbContext.Database.BeginTransactionAsync();
    }
}