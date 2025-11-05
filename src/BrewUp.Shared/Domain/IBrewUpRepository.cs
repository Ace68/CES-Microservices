using Muflone.Messages.Events;

namespace BrewUp.Shared.Domain;

public interface IBrewUpRepository<T> where T : BrewUpAggregateRoot
{
   
    Task<T> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task DeleteAsync(T entity, CancellationToken cancellationToken);
    Task PublishAggregateEventsAsync(T entity, CancellationToken cancellationToken);
    IEnumerable<DomainEvent> GetUncommittedEvents();
}