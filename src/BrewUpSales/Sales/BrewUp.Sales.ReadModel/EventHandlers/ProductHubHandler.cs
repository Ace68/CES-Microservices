using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Events;
using BrewUp.Shared.Configuration;
using Muflone;
using Muflone.Messages.Events;
using Muflone.Persistence.Azure.Helpers;

namespace BrewUp.Sales.ReadModel.EventHandlers;

public sealed class ProductHubHandler(
    EventHubParameters eventHubParameters,
    IEventBus eventBus) : IAsyncDisposable
{
    private readonly EventProcessorClient _eventProcessorClient = new(
        new BlobContainerClient(
            eventHubParameters.BlobStorageConnectionString,
            eventHubParameters.BlobStorageContainerName),
        "sales",
        eventHubParameters.EventHubConnectionString,
        "producthub");
    
    private CancellationTokenSource? _cts;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        _eventProcessorClient.ProcessEventAsync += ProcessEventHandler;
        _eventProcessorClient.ProcessErrorAsync += ProcessErrorHandler;
        
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
            
            var operation = root.GetProperty("operation").GetString();
            var cols = data.GetProperty("eventsource").GetProperty("cols").EnumerateArray();
            var current = JsonSerializer.Deserialize<Dictionary<string, string>>(data.GetProperty("eventrow").GetProperty("current").GetString()!);
            var old = JsonSerializer.Deserialize<Dictionary<string, string>>(data.GetProperty("eventrow").GetProperty("old").GetString()!);
            
            var tableName = RepositoryHelper.GetTableNameFromEvent(data);
            if (tableName != "[dbo].[Product]")
            {
                await eventArgs.UpdateCheckpointAsync();
                return;
            }
            
            DeserializeEventMetadata(eventArgs, root, data);
            
            switch (operation)
            {
                case "INS":
                    var productCreated = ProcessInsert(cols, current!);
                    await eventBus.PublishAsync(productCreated, _cts!.Token).ConfigureAwait(false);
                    break;
                case "UPD":
                    ProcessUpdate(cols, current!, old!);
                    break;
                case "DEL":
                    ProcessDelete(cols, old!);
                    break;
            }
            
            await eventArgs.UpdateCheckpointAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }        
    }

    private static void DeserializeEventMetadata(ProcessEventArgs eventArgs, JsonElement root, JsonElement data)
    {
        Console.WriteLine("Event Args");
        Console.WriteLine($"  Sequence:Offset => {eventArgs.Data.SequenceNumber}:{eventArgs.Data.Offset}");
        Console.WriteLine();
        
        Console.WriteLine("Event Data");
        Console.WriteLine($"  Spec version:       {root.GetProperty("specversion").GetString()}");
        Console.WriteLine($"  Operation:          {root.GetProperty("type").GetString()}");
        Console.WriteLine($"  Time:               {root.GetProperty("time").GetString()}");
        Console.WriteLine($"  Event ID:           {root.GetProperty("id").GetString()}");
        Console.WriteLine($"  Logical ID:         {root.GetProperty("logicalid").GetString()}");
        Console.WriteLine($"  Operation:          {root.GetProperty("operation").GetString()}");
        Console.WriteLine($"  Data content type:  {root.GetProperty("datacontenttype").GetString()}");
        Console.WriteLine();
        
        Console.WriteLine("Data");
        Console.WriteLine($"  Database:           {data.GetProperty("eventsource").GetProperty("db").GetString()}");
        Console.WriteLine($"  Schema:             {data.GetProperty("eventsource").GetProperty("schema").GetString()}");
        Console.WriteLine($"  Table:              {data.GetProperty("eventsource").GetProperty("tbl").GetString()}");
        Console.WriteLine();
    }
    
    private static IntegrationEvent ProcessInsert(JsonElement.ArrayEnumerator cols, Dictionary<string, string> current)
    {
        Console.WriteLine("Operation: Insert");
        Console.ForegroundColor = ConsoleColor.Green;
        
        var productId = string.Empty;
        var productName = string.Empty;
        var productDescription = string.Empty;
        var productType = string.Empty;
        
        foreach (var col in cols)
        {
            var name = col.GetProperty("name").GetString();
            if (name == "Id")
                productId = current[name];
            if (name == "ProductName")
                productName = current[name];
            if (name == "ProductDescription")
                productDescription = current[name];
            if (name == "ProductType")
                productType = current[name];
        }
 
        ProductCreated productCreated = new(
            new ProductId(productId),
            new ProductName(productName),
            new ProductDescription(productDescription),
            new ProductType(productType));
        
        return productCreated;
    }
    
    private static void ProcessUpdate(JsonElement.ArrayEnumerator cols, Dictionary<string, string> current, Dictionary<string, string> old)
    {
        Console.WriteLine("Operation: Update");
 
        foreach (var col in cols)
        {
            var name = col.GetProperty("name").GetString();
 
            if (old.Count > 0 && current[name] != old[name])
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\t{name}: {current[name]} (old: {old[name]})");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"\t{name}: {current[name]}");
            }
        }
    }
    
    private static void ProcessDelete(JsonElement.ArrayEnumerator cols, Dictionary<string, string> old)
    {
        Console.WriteLine("Operation: Delete");
        Console.ForegroundColor = ConsoleColor.Red;
 
        foreach (var col in cols)
        {
            var name = col.GetProperty("name").GetString();
            Console.WriteLine($"\t{name}: {old[name]}");
        }
 
        Console.ResetColor();
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
    
    ~ProductHubHandler()
    {
    }
    #endregion
}