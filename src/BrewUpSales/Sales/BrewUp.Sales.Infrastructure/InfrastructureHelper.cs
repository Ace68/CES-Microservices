using BrewUp.Sales.Entities.Dtos;
using BrewUp.Sales.Infrastructure.Repository;
using BrewUp.Shared.ReadModel;
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
        
        services.AddScoped<IBrewUpPersister<SalesOrder>, SalesOrderPersister>();
        services.AddScoped<IBrewUpPersister<Product>, ProductPersister>();
        services.AddScoped<IBrewUpPersister<SalesForProduct>, SalesForProductPersister>();
        
        return services;
    }
}