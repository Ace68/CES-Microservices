using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Logging;

namespace Muflone.Persistence.Azure.Dispatcher;

public class EventHubListener(
    EventHubParameters eventHubParameters,
    ILogger<EventHubListener> logger)
    : IAsyncDisposable
{
    private readonly EventHubConsumerClient _consumer = new(
        EventHubConsumerClient.DefaultConsumerGroupName,
        eventHubParameters.ConnectionString,
        eventHubParameters.EventHubName);

    private CancellationTokenSource? _cts;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await foreach (PartitionEvent partitionEvent in _consumer.ReadEventsAsync(_cts.Token))
        {
            var eventBody = partitionEvent.Data.EventBody.ToString();
            logger.LogInformation("Received event: {EventBody}", eventBody);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _cts?.CancelAsync()!;
        await _consumer.CloseAsync();
        GC.SuppressFinalize(this);
    }
}