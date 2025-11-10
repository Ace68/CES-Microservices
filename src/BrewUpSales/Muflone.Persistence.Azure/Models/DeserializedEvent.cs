namespace Muflone.Persistence.Azure.Models;

public record DeserializedEvent(string AggregateId, string AggregateName, string AggregateType,
    string EventType, DateTime CommitDate, int Version, long CommitPosition);