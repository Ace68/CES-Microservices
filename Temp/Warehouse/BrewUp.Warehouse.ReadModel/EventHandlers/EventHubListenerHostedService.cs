using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BrewUp.Warehouse.ReadModel.EventHandlers;

public class EventHubListenerHostedService(
    ProductHubHandler listener,
    ILogger<EventHubListenerHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EventHub listener hosted service starting");

        try
        {
            await listener.StartAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("EventHub listener stopped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in EventHub listener");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("EventHub listener hosted service stopping");
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        listener.DisposeAsync().AsTask().Wait();
        base.Dispose();
    }
}