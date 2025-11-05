using System.Reflection;
using System.Text;
using BrewUp.Infrastructure.Helpers;
using BrewUp.Sales.Entities.Dtos;
using BrewUp.Shared.Domain;
using BrewUp.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muflone;
using Muflone.Core;
using Muflone.Messages.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BrewUp.Sales.Infrastructure.Repository;

public class SalesOrderRepository(SalesContext salesContext,
    IEventBus eventBus,
    ILoggerFactory loggerFactory) : IBrewUpRepository<SalesOrder>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SalesOrderRepository>();
    private IEnumerable<DomainEvent> Published { get; set; } = [];
    private static readonly JsonSerializerSettings SerializerSettings;
    
    static SalesOrderRepository()
    {
        SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            ContractResolver = new PrivateContractResolver()
        };
    }
    
    public async Task<SalesOrder> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var queryable = salesContext.Set<SalesOrder>()
                .Include(c => c.SalesOrderRows)
                .Where(a => a.Id.Equals(id));
            var result = await queryable.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
            return result ?? ConstructAggregate<SalesOrder>();
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
            throw;
        }
    }

    public async Task AddAsync(SalesOrder entity, CancellationToken cancellationToken)
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

    public async Task UpdateAsync(SalesOrder entity, CancellationToken cancellationToken)
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

    public async Task DeleteAsync(SalesOrder entity, CancellationToken cancellationToken)
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

    public async Task PublishAggregateEventsAsync(SalesOrder entity, CancellationToken cancellationToken)
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
    
    private async Task AddEntityAsync(SalesOrder entity, CancellationToken cancellationToken)
    {
        var dbSet = salesContext.Set<SalesOrder>();
        await dbSet.AddAsync(entity, cancellationToken);
        await salesContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task UpdateEntityAsync(SalesOrder entity, CancellationToken cancellationToken)
    {
        var dbSet = salesContext.Set<SalesOrder>();
        dbSet.Update(entity);
        await salesContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task DeleteEntityAsync(SalesOrder entity, CancellationToken cancellationToken)
    {
        salesContext.Set<SalesOrder>().Remove(entity);
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