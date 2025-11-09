using BrewUp.Sales.Entities.Dtos;
using BrewUp.Sales.Infrastructure.Repository;
using BrewUp.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Sales.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddSalesInfrastructure(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        services.AddDbContext<SalesContext>(options =>
            options.UseSqlServer(configurationManager["BrewUp:SqlServer:ConnectionString"]!));
        
        services.AddScoped<IBrewUpRepository<SalesOrder>, SalesOrderRepository>();
        services.AddScoped<IBrewUpRepository<Product>, ProductRepository>();
        
        return services;
    }
}