using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using BrewUp.Shared.Configuration;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.Persistence.Azure.Helpers;
using Muflone.Persistence.Azure.Models;

namespace Muflone.Persistence.Azure.Dispatcher;

public sealed class EventHubListener(
    EventHubParameters eventHubParameters,
    IEventBus eventBus,
    ILogger<EventHubListener> logger)
    : IAsyncDisposable
{
    private readonly EventProcessorClient _eventProcessorClient = new(
        new BlobContainerClient(
            eventHubParameters.BlobStorageConnectionString,
            eventHubParameters.BlobStorageContainerName),
        EventHubConsumerClient.DefaultConsumerGroupName,
        eventHubParameters.EventHubConnectionString,
        eventHubParameters.EventHubName);
    
    private CancellationTokenSource? _cts;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // Register handlers for processing events and errors
        _eventProcessorClient.ProcessEventAsync += ProcessEventHandler;
        _eventProcessorClient.ProcessErrorAsync += ProcessErrorHandler;
        
        logger.LogInformation("Starting Event Hub processor");
        
        await _eventProcessorClient.StartProcessingAsync(cancellationToken);
    }
    
    private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        try
        {
            // Deserialize the event data
            using var doc = JsonDocument.Parse(eventArgs.Data.Body.ToArray());
            var root = doc.RootElement;
            var dataJson = root.GetProperty("data");
 
            using var innerDoc = JsonDocument.Parse(dataJson.GetString()!);
            var data = innerDoc.RootElement;
            var cols = data.GetProperty("eventsource").GetProperty("cols").EnumerateArray();
            var current = JsonSerializer.Deserialize<Dictionary<string, string>>(data.GetProperty("eventrow").GetProperty("current").GetString()!);
 
            var @event = RepositoryHelper.DeserializeEvent(GetEventElements(cols, current!));
            await eventBus.PublishAsync((DomainEvent) @event, _cts!.Token).ConfigureAwait(false);
 
            // Persist progress so we don't reprocess this event on restart
            await eventArgs.UpdateCheckpointAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }
    
    private static ResolvedEvent GetEventElements(JsonElement.ArrayEnumerator cols, Dictionary<string, string> current)
    {
        string aggregateId = string.Empty;
        byte[] metadata = [];
        byte[] data = [];
        
        foreach (var name in cols.Select(col => col.GetProperty("name").GetString()))
        {
            switch (name)
            {
                case "AggregateId":
                    aggregateId = current[name];
                    break;
                    
                case "Metadata":
                    metadata = Encoding.UTF8.GetBytes(current[name]);
                    break;
                case "Data":
                    data = Encoding.UTF8.GetBytes(current[name]);
                    break;
            }
        }
 
        return new ResolvedEvent(aggregateId, metadata, data);
    }

    private static Task ProcessErrorHandler(ProcessErrorEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Exception.Message);
        Console.ResetColor();
        return Task.CompletedTask;
    }

    #region Dispose
    public async ValueTask DisposeAsync()
    {
        await _cts?.CancelAsync()!;
        await _eventProcessorClient.StopProcessingAsync();
        GC.SuppressFinalize(this);
    }
    
    ~EventHubListener()
    {
    }
    #endregion
}