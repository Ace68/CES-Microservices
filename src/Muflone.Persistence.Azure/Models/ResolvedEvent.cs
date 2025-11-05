namespace Muflone.Persistence.Azure.Models;

public record ResolvedEvent(string AggregateId, byte[] Metadata, byte[] Data);