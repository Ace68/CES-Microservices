using Muflone.Persistence.Azure.Models;
using System.Text;
using System.Text.Json;
using Muflone.Core;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Muflone.Persistence.Azure.Persistence;

public sealed class EventStoreRepository : IRepository
{
	private const string EventClrTypeHeader = "EventClrTypeName";
	private const string AggregateClrTypeHeader = "AggregateClrTypeName";
	private const string CommitIdHeader = "CommitId";
	private const string CommitDateHeader = "CommitDate";
	private static readonly JsonSerializerOptions SerializerOptions;

	private readonly Func<Type, string, string> _aggregateIdToStreamName;

	// private readonly BlobServiceClient _blobServiceClient;
	private readonly EventStoreContext _eventStoreContext;

	static EventStoreRepository()
	{
		SerializerOptions = new JsonSerializerOptions
		{
			WriteIndented = true
		};
	}

	//This rename is needed to be consistent with naming convention of EventStore javascript
	public EventStoreRepository(EventStoreContext eventStoreContext)
		: this(eventStoreContext, (type, aggregateId) => $"{type.Name.ToLower()}{aggregateId.Replace("-","")}")
	{
	}

	public EventStoreRepository(EventStoreContext eventStoreContext, Func<Type, string, string> aggregateIdToStreamName)
	{
		_eventStoreContext = eventStoreContext;
		_aggregateIdToStreamName = aggregateIdToStreamName;
	}
	
	public Task<TAggregate?> GetByIdAsync<TAggregate>(IDomainId id, CancellationToken cancellationToken = new ()) where TAggregate : class, IAggregate
	{
		throw new NotImplementedException();
	}

	public Task<TAggregate?> GetByIdAsync<TAggregate>(IDomainId id, long version,
		CancellationToken cancellationToken = new ()) where TAggregate : class, IAggregate
	{
		throw new NotImplementedException();
	}
	
	public async Task SaveAsync(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders,
		CancellationToken cancellationToken = new ())
	{
		cancellationToken.ThrowIfCancellationRequested();
		
		var commitHeaders = new Dictionary<string, object>
		{
			{ CommitIdHeader, commitId },
			{ CommitDateHeader, DateTime.UtcNow},
			{ AggregateClrTypeHeader, aggregate.GetType().AssemblyQualifiedName! }
		};
		updateHeaders(commitHeaders);

		//Create a unique name for the container
		// var containerName = _aggregateIdToStreamName(aggregate.GetType(), aggregate.Id.Value);
		var aggregateName = aggregate.GetType().Name.ToLower();
		var newEvents = aggregate.GetUncommittedEvents().Cast<object>().ToList();
		var originalVersion = aggregate.Version - newEvents.Count;
		var expectedVersion = originalVersion == 0 ? ExpectedVersion.NoStream : originalVersion - 1;
		var eventsToSave = newEvents.Select(e => ToEventData(Guid.NewGuid(), e, commitHeaders)).ToList();

		try
		{
			await using var transaction = await _eventStoreContext.Database.BeginTransactionAsync(cancellationToken);
			var dbSet = _eventStoreContext.Set<EventStore>();

			foreach (var entity in eventsToSave.Select(eventData => EventStore.Create(Guid.NewGuid().ToString(), aggregate.Id.Value,
				         aggregateName, 
				         aggregate.GetType().Assembly.FullName!,
				         eventData.Type,
				         Encoding.UTF8.GetString(eventData.Data),
				         Encoding.UTF8.GetString(eventData.Metadata),
				         ++originalVersion)))
			{
				await dbSet.AddAsync(entity, cancellationToken);
			}
			await _eventStoreContext.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}

		aggregate.ClearUncommittedEvents();
	}

	public Task SaveAsync(IAggregate aggregate, Guid commitId, CancellationToken cancellationToken = new()) =>
		SaveAsync(aggregate, commitId, _ => { }, cancellationToken);

    public Task SaveAsync(IAggregate aggregate, Guid commitId) => SaveAsync(aggregate, commitId, _ => { });

	#region Helpers
	private static EventData ToEventData(Guid eventId, object @event, IDictionary<string, object> headers)
	{
		var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, SerializerOptions));
		var eventHeaders = new Dictionary<string, object>(headers) { { EventClrTypeHeader, @event.GetType().AssemblyQualifiedName! } };
		var metadata = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventHeaders, SerializerOptions));
		var typeName = @event.GetType().Name;
		
		return new EventData(eventId, typeName, true, data, metadata);
	}
	#endregion

	#region Dispose
	private bool _disposedValue; // To detect redundant calls

	private void Dispose(bool disposing)
	{
		if (_disposedValue) return;
		
		if (disposing)
		{
			// TODO: dispose managed state (managed objects).
		}
		// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
		// TODO: set large fields to null.
		_disposedValue = true;
	}

	// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
	// ~EventStoreRepository() {
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