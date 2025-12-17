using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Infrastructure.Data;

public class TresorerieDbContext : DbContext//, IMenuDbContext
{
    public TresorerieDbContext(DbContextOptions<TresorerieDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CashFlow> CashFlows => Set<CashFlow>();
    public DbSet<CashFlowHistory> Budgets => Set<CashFlowHistory>();
    public DbSet<RecurringCashFlow> RecurringCashFlows => Set<RecurringCashFlow>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
