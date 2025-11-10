using Muflone.Persistence.Azure.Models;

namespace Muflone.Persistence.Azure;

public interface IEventStoreService
{
    Task<IEnumerable<DeserializedEvent>> GetAllAggregateEventsAsync(string aggregateId, CancellationToken cancellationToken = default);
}