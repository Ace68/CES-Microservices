using BrewUp.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muflone.Persistence.Azure.Helpers;
using Muflone.Persistence.Azure.Models;

namespace Muflone.Persistence.Azure;

internal sealed class EventStoreService(EventStoreContext eventStoreContext,
    ILoggerFactory loggerFactory) : IEventStoreService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<EventStoreService>();

    public async Task<IEnumerable<DeserializedEvent>> GetAllAggregateEventsAsync(string aggregateId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var readResult = await eventStoreContext.Set<EventStore>()
                .Where(a => a.AggregateId.Equals(aggregateId))
                .ToListAsync(cancellationToken: cancellationToken);
            
            return readResult.Select(RepositoryHelper.ToDeserializedEvent).ToList();
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
            throw;
        }
    }
}