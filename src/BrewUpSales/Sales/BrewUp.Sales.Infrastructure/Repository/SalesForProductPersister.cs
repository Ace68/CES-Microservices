using BrewUp.Sales.Entities.Dtos;
using BrewUp.Shared.Exceptions;
using BrewUp.Shared.ReadModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muflone;
using Muflone.Messages.Events;

namespace BrewUp.Sales.Infrastructure.Repository;

public class SalesForProductPersister(SalesContext salesContext,
    IEventBus eventBus,
    ILoggerFactory loggerFactory) : IBrewUpPersister<SalesForProduct>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SalesForProductPersister>();
    private IEnumerable<DomainEvent> Published { get; set; } = [];
    
    public async Task<SalesForProduct> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var queryable = salesContext.Set<SalesForProduct>()
                .Where(a => a.Id.Equals(id));
            var result = await queryable.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
            return result ?? ConstructAggregate<SalesForProduct>();
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
            throw;
        }
    }

    public async Task AddAsync(SalesForProduct entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (salesContext.Database.ProviderName?.Contains("InMemory") != true)
            {
                await using var transaction = await salesContext.Database.BeginTransactionAsync(cancellationToken);
                await AddEntityAsync(entity, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                // Skip transaction for InMemory provider
                await AddEntityAsync(entity, cancellationToken);
            }
            
            await PublishAggregateEventsAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
            throw;
        }
    }

    public async Task UpdateAsync(SalesForProduct entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (salesContext.Database.ProviderName?.Contains("InMemory") != true)
            {
                await using var transaction = await salesContext.Database.BeginTransactionAsync(cancellationToken);
                await UpdateEntityAsync(entity, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                await UpdateEntityAsync(entity, cancellationToken);
            }
        
            await PublishAggregateEventsAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
            throw;
        }
    }

    public async Task DeleteAsync(SalesForProduct entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            if (salesContext.Database.ProviderName?.Contains("InMemory") != true)
            {
                await using var transaction = await salesContext.Database.BeginTransactionAsync(cancellationToken);
                await DeleteEntityAsync(entity, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                await DeleteEntityAsync(entity, cancellationToken);
            }
            
            await PublishAggregateEventsAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
            throw;
        }
    }

    public async Task PublishAggregateEventsAsync(SalesForProduct entity, CancellationToken cancellationToken)
    {
        IEnumerable<DomainEvent> uncommittedEvents = entity.GetUncommittedEvents().ToList();
        Published = uncommittedEvents;
        
        foreach (var @event in entity.GetUncommittedEvents())
        {
            await eventBus.PublishAsync(@event, cancellationToken);
        }
        entity.ClearUncommittedEvents();
    }

    public IEnumerable<DomainEvent> GetUncommittedEvents()
    {
        return Published;
    }
    
    private async Task AddEntityAsync(SalesForProduct entity, CancellationToken cancellationToken)
    {
        var dbSet = salesContext.Set<SalesForProduct>();
        await dbSet.AddAsync(entity, cancellationToken);
        await salesContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task UpdateEntityAsync(SalesForProduct entity, CancellationToken cancellationToken)
    {
        var dbSet = salesContext.Set<SalesForProduct>();
        dbSet.Update(entity);
        await salesContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task DeleteEntityAsync(SalesForProduct entity, CancellationToken cancellationToken)
    {
        salesContext.Set<SalesForProduct>().Remove(entity);
        await salesContext.SaveChangesAsync(cancellationToken);
    }
    
    private static TAggregate ConstructAggregate<TAggregate>()
    {
        return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true)!;
    }
    
    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;
        
        if (disposing)
        {
            // TODO: dispose managed state (managed objects).
        }
        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.
        _disposedValue = true;
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~SalesOrderRepository() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
    #endregion
}