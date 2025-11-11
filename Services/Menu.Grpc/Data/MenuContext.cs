using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Menu.Grpc.Models;

namespace Menu.Grpc.Data;

public class MenuContext : DbContext
{

    public MenuContext(DbContextOptions<MenuContext> options)
        : base(options)
    {
    }
    public DbSet<MenuNav> Menus { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuNav>()
            .HasData(
                new MenuNav
                {
                    Id = new Guid("1cb25cbe-1f2f-4fc8-bf53-25255dbd9b68"),
                    Name = "Produit",
                    ApiRoute = "/product",
                    UrlFront = "/produit",
                    Icon = "fa-solid fa-bag-shopping",
                    IsActif = true,
                    AppAdminId = new Guid("b9bd41f4-cb78-4d71-b2a2-08d385944f15")

                },
                new MenuNav
                {
                    Id = new Guid("5d5b22a1-dc69-4b66-811a-9b4598b71f00"),
                    Name = "Tableau de bord",
                    ApiRoute = "/sale",
                    UrlFront = "/dashboard",
                    Icon = "fa-solid fa-chart-pie",
                    IsActif = true,
                    AppAdminId = new Guid("2c252630-2e4f-4d09-b2d6-057e4f04eff1")

                }

            );

        //modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        //base.OnModelCreating(modelBuilder);
    }

}
