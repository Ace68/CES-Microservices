using System.Text;
using Muflone.Persistence.Azure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Muflone.Persistence.Azure.Helpers;

public static class RepositoryHelper
{
    private const string EventClrTypeHeader = "EventClrTypeName";
    
    public static EventData ToEventData(Guid eventId, object @event, IDictionary<string, object> headers)
    {
        var data = JsonConvert.SerializeObject(@event);
        var eventHeaders = new Dictionary<string, object>(headers) { { EventClrTypeHeader, @event.GetType().AssemblyQualifiedName! } };
        var metadata = JsonConvert.SerializeObject(eventHeaders);
        var typeName = @event.GetType().Name;
		
        return new EventData(eventId, typeName, true, data, metadata);
    }
    
    public static object DeserializeEvent(ResolvedEvent resolvedEvent)
    {
        try
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(resolvedEvent.Metadata.ToArray())).Property(EventClrTypeHeader)!.Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Data.ToArray()), Type.GetType(((string)eventClrTypeName)!)!)!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}