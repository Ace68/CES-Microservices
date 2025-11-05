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
	public static IServiceCollection AddEventstoreAzurePersistence(this IServiceCollection services,
		IConfigurationManager configurationManager)
	{
		services.AddDbContext<EventStoreContext>(options =>
			options.UseSqlServer(configurationManager["Muflone:SqlStore:ConnectionString"]!));
		services.AddScoped<IRepository, EventStoreRepository>();
		
		var eventhubParameters = configurationManager.GetSection("Muflone:EventHub").Get<EventHubParameters>();
		services.AddSingleton<EventHubListener>(sp => 
			new EventHubListener(
				eventhubParameters!,
				sp.GetRequiredService<IEventBus>(),
				sp.GetRequiredService<ILogger<EventHubListener>>()));
		services.AddHostedService<EventHubListenerHostedService>();

		return services;
	}
}