using BrewUp.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Persistence.Azure.Dispatcher;
using Muflone.Persistence.Azure.Persistence;

namespace Muflone.Persistence.Azure;

public static class PersistenceAzureHelper
{
	public static IServiceCollection AddEventStoreAzurePersistence(this IServiceCollection services,
		IConfigurationManager configurationManager)
	{
		services.AddDbContext<EventStoreContext>(options =>
			options.UseSqlServer(configurationManager["Muflone:SqlStore:ConnectionString"]!));
		services.AddScoped<IRepository, EventStoreRepository>();
		services.AddScoped<IEventStoreService, EventStoreService>();
		
		var eventhubParameters = configurationManager.GetSection("Muflone:EventHub").Get<EventHubParameters>();
		services.AddSingleton<EventHubListener>(sp => 
			new EventHubListener(
				eventhubParameters!,
				sp.GetRequiredService<IEventBus>(),
				sp.GetRequiredService<ILogger<EventHubListener>>()));
		services.AddHostedService<EventHubListenerHostedService>();
		
		// services.AddHostedService<EventDispatcherHostedService>(sp =>
		// 	new EventDispatcherHostedService(
		// 		new EventDispatcher(
		// 			sp.GetRequiredService<EventStoreContext>(),
		// 			sp.GetRequiredService<IEventBus>(),
		// 			sp.GetRequiredService<ILoggerFactory>())));

		return services;
	}
}