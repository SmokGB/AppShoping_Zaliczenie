using AppShoping.DataAccess.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace AppShoping.DataAccess.Data;

public class ShopAppDbContext : DbContext
{

    public ShopAppDbContext(DbContextOptions<ShopAppDbContext> options) : base(options)
    {
    }

    public DbSet<Food> Foods => Set<Food>();
    public DbSet<PurchaseStatistics> Purchase => Set<PurchaseStatistics>();
}

