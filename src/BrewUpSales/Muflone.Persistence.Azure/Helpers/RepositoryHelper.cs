using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.Persistence.Azure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Muflone.Persistence.Azure.Helpers;

public static class RepositoryHelper
{
    private const string EventClrTypeHeader = "EventClrTypeName";
    private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("RepositoryHelper");
    
    public static EventData ToEventData(Guid eventId, object @event, IDictionary<string, object> headers)
    {
        var data = JsonConvert.SerializeObject(@event);
        var eventHeaders = new Dictionary<string, object>(headers) { { EventClrTypeHeader, @event.GetType().AssemblyQualifiedName! } };
        var metadata = JsonConvert.SerializeObject(eventHeaders);
        var typeName = @event.GetType().Name;
		
        return new EventData(eventId, typeName, true, data, metadata);
    }

    public static DeserializedEvent ToDeserializedEvent(EventStore @event)
    {
        var metadata = DeserializeMetadata(new ResolvedEvent(@event.Metadata, @event.Data));
        
        DateTime commitDate = DateTime.MinValue;
        if (!metadata.TryGetValue("CommitDate", out var commitDateObj))
            return new DeserializedEvent(@event.AggregateId, @event.AggregateName, @event.AggregateType,
                @event.EventType,
                commitDate, @event.Version, @event.CommitPosition);
        
        var commitDateStr = commitDateObj?.ToString();
        commitDate = DateTime.Parse(commitDateStr!);
        
        return new DeserializedEvent(@event.AggregateId, @event.AggregateName, @event.AggregateType, @event.EventType,
            commitDate, @event.Version, @event.CommitPosition);
    }
    
    public static Dictionary<string, object> DeserializeMetadata(ResolvedEvent resolvedEvent)
    {
        try
        {
            var metadataJson = Encoding.UTF8.GetString(resolvedEvent.Metadata.ToArray());
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(metadataJson)!;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deserializing metadata");
            throw;
        }
    }
    
    public static object DeserializeEvent(ResolvedEvent resolvedEvent)
    {
        try
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(resolvedEvent.Metadata.ToArray())).Property(EventClrTypeHeader)!.Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Data.ToArray()), Type.GetType(((string)eventClrTypeName)!)!)!;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deserializing event");
            throw;
        }
    }
    
    public static DomainEvent DeserializeCloudEvent(ResolvedCloudEvent cloudEvent)
    {
        try
        {
            var metadataBytes = Enumerable.Range(0, cloudEvent.CloudEventMetadata.Length / 2)
                .Select(x => Convert.ToByte(cloudEvent.CloudEventMetadata.Substring(x * 2, 2), 16))
                .ToArray();
            
            string json = Encoding.UTF8.GetString(metadataBytes);
            var eventHeaders = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)!;
            
            var eventClrTypeName = eventHeaders[EventClrTypeHeader].ToString();
            var eventType = Type.GetType(eventClrTypeName!, throwOnError: true);
            
            var dataBytes = Enumerable.Range(0, cloudEvent.CloudEventData.Length / 2)
                .Select(x => Convert.ToByte(cloudEvent.CloudEventData.Substring(x * 2, 2), 16))
                .ToArray();
            var dataJson = Encoding.UTF8.GetString(dataBytes);

            return (DomainEvent) JsonConvert.DeserializeObject(dataJson, eventType!)!;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deserializing CloudEvent");
            throw;
        }
    }
    
    public static string GetTableNameFromEvent(JsonElement data)
    {
        try
        {
            var schema = data.GetProperty("eventsource").GetProperty("schema").GetString();
            var table = data.GetProperty("eventsource").GetProperty("tbl").GetString();
            return $"[{schema}].[{table}]";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retreiving table name from event");
            throw;
        }
    }
}