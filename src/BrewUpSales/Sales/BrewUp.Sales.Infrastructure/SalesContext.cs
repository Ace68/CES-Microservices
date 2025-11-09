using BrewUp.Sales.Entities.Dtos;
using BrewUp.Sales.Infrastructure.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.Infrastructure;

public class SalesContext(DbContextOptions<SalesContext> options) : DbContext(options)
{
    public DbSet<SalesOrder> SalesOrder { get; set; }
    public DbSet<SalesOrderRow> SalesOrderRow { get; set; }
    
    public DbSet<Product> Product { get; set; }
    public DbSet<Availability> Availability { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Logging configuration
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter((_, level) => level == LogLevel.Information)
                .AddConsole();
        });

        optionsBuilder.UseLoggerFactory(loggerFactory);
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
        
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new SalesOrderMapping());
        modelBuilder.ApplyConfiguration(new SalesOrderRowMapping());
        
        modelBuilder.ApplyConfiguration(new ProductMapping());
        modelBuilder.ApplyConfiguration(new AvailabilityMapping());
        
        modelBuilder.Entity<SalesOrder>()
            .HasMany(s => s.SalesOrderRows)
            .WithOne(r => r.SalesOrder)
            .HasForeignKey(r => r.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        modelBuilder.Entity<Product>()
            .HasMany(s => s.Availabilities)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}