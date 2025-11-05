using BrewUp.Warehouse.Domain;
using BrewUp.Warehouse.Facade.Acl;
using BrewUp.Warehouse.Facade.Validators;
using BrewUp.Warehouse.Infrastructure;
using BrewUp.Warehouse.ReadModel;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Warehouse.Facade;

public static class WarehouseFacadeHelper
{
    public static IServiceCollection AddWarehouseFacade(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
        
        services.AddScoped<IWarehouseFacade, WarehouseFacade>();

        services.AddWarehouseDomain();
        services.AddWarehouseInfrastructure(configurationManager);
        services.AddWarehouseReadModel(configurationManager);

        services.AddIntegrationEventHandler<SalesOrderReadyForProcessingEventHandler>();

        return services;
    }
}