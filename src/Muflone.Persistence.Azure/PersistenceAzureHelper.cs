using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Persistence.Azure.Dispatcher;
using Muflone.Persistence.Azure.Persistence;

namespace Muflone.Persistence.Azure;

public static class PersistenceAzureHelper
{
	public static IServiceCollection AddEventstoreAzurePersistence(this IServiceCollection service,
		IConfigurationManager configurationManager)
	{
		service.AddDbContext<EventStoreContext>(options =>
			options.UseSqlServer(configurationManager["Muflone:SqlStore:ConnectionString"]!));
		service.AddScoped<IRepository, EventStoreRepository>();
		
		var eventhubParameters = configurationManager.GetSection("Muflone:EventHub").Get<EventHubParameters>();
		service.AddSingleton<EventHubListener>(sp => 
			new EventHubListener(
				eventhubParameters!,
				sp.GetRequiredService<ILogger<EventHubListener>>()));
		service.AddHostedService<EventHubListenerHostedService>();

		return service;
	}
}