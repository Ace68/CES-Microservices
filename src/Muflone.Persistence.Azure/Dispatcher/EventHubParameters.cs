namespace Muflone.Persistence.Azure.Dispatcher;

public record EventHubParameters(string ConnectionString, string EventHubName);