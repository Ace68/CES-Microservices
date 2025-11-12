using System.Reflection;
using BrewUp.Shared.Exceptions;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muflone;
using Muflone.Messages.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BrewUp.Warehouse.Infrastructure.Repository;

public class ProductPersister(WarehouseContext warehouseContext,
    IEventBus eventBus,
    ILoggerFactory loggerFactory) : IBrewUpPersister<Product>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ProductPersister>();
    private IEnumerable<DomainEvent> Published { get; set; } = [];
    private static readonly JsonSerializerSettings SerializerSettings;
    
    static ProductPersister()
    {
        SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            ContractResolver = new PrivateContractResolver()
        };
    }
    
    public async Task<Product> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var queryable = warehouseContext.Set<Product>()
                .Include(c => c.Availabilities)
                .Where(a => a.Id.Equals(id));
            var result = await queryable.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
            return result ?? ConstructAggregate<Product>();
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
            throw;
        }
    }

    public async Task AddAsync(Product entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (warehouseContext.Database.ProviderName?.Contains("InMemory") != true)
            {
                await using var transaction = await warehouseContext.Database.BeginTransactionAsync(cancellationToken);
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

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (warehouseContext.Database.ProviderName?.Contains("InMemory") != true)
            {
                await using var transaction = await warehouseContext.Database.BeginTransactionAsync(cancellationToken);
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

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            if (warehouseContext.Database.ProviderName?.Contains("InMemory") != true)
            {
                await using var transaction = await warehouseContext.Database.BeginTransactionAsync(cancellationToken);
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

    public async Task PublishAggregateEventsAsync(Product entity, CancellationToken cancellationToken)
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
    
    private async Task AddEntityAsync(Product entity, CancellationToken cancellationToken)
    {
        var dbSet = warehouseContext.Set<Product>();
        await dbSet.AddAsync(entity, cancellationToken);
        await warehouseContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task UpdateEntityAsync(Product entity, CancellationToken cancellationToken)
    {
        var dbSet = warehouseContext.Set<Product>();
        dbSet.Update(entity);
        await warehouseContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task DeleteEntityAsync(Product entity, CancellationToken cancellationToken)
    {
        warehouseContext.Set<Product>().Remove(entity);
        await warehouseContext.SaveChangesAsync(cancellationToken);
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

internal class PrivateContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(p => base.CreateProperty(p, memberSerialization))
            .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(f => base.CreateProperty(f, memberSerialization)))
            .ToList();
        props.ForEach(p => { p.Writable = true; p.Readable = true; });
        return props;
    }
}