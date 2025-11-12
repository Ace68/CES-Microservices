using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.Entities.Entities;
using BrewUp.Warehouse.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Warehouse.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddWarehouseInfrastructure(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        services.AddDbContext<WarehouseContext>(options =>
            options.UseSqlServer(configurationManager["BrewUp:SqlServer:ConnectionString"]!));
        
        services.AddScoped<IBrewUpPersister<Product>, ProductPersister>();
        
        return services;
    }
}