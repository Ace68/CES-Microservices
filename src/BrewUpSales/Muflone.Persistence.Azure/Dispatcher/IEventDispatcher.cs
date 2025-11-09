namespace Muflone.Persistence.Azure.Dispatcher;

public interface IEventDispatcher
{
    Task DispatchAllEventsAsync(long lastPosition, CancellationToken cancellationToken = new ());
}