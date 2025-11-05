using BrewUp.Shared.Configuration;
using BrewUp.Warehouse.ReadModel.EventHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone;

namespace BrewUp.Warehouse.ReadModel;

public static class WarehouseReadModelHelper
{
    public static IServiceCollection AddWarehouseReadModel(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        services.AddDomainEventHandler<AvailabilityUpdatedEventHandler>();
        
        // var eventhubParameters = configurationManager.GetSection("Muflone:EventHub").Get<EventHubParameters>();
        // services.AddSingleton<ProductHubHandler>(_ => 
        //     new ProductHubHandler(
        //         eventhubParameters!));
        // services.AddHostedService<EventHubListenerHostedService>();

        return services;
    }
}