using BrewUp.Sales.Entities.Dtos;
using BrewUp.Sales.ReadModel.EventHandlers;
using BrewUp.Sales.ReadModel.Queries;
using BrewUp.Sales.ReadModel.Services;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Sales.ReadModel;

public static class SalesReadModelHelper
{
    public static IServiceCollection AddSalesReadModel(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        services.AddScoped<IQueries<SalesOrder>, SalesOrderQuery>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();

        services.AddDomainEventHandler<SalesOrderCreatedEventHandler>();
        services.AddDomainEventHandler<SalesOrderCreatedForIntegrationEventHandler>();

        return services;
    }
}