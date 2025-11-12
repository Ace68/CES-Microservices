using Muflone.Messages.Events;

namespace BrewUp.Shared.ReadModel;

public interface IBrewUpPersister<T> where T : DtoBase
{
   
    Task<T> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task DeleteAsync(T entity, CancellationToken cancellationToken);
    Task PublishAggregateEventsAsync(T entity, CancellationToken cancellationToken);
    IEnumerable<DomainEvent> GetUncommittedEvents();
}