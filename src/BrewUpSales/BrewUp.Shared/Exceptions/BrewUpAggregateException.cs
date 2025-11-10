namespace BrewUp.Shared.Exceptions;

public record BrewUpAggregateException(string Aggregateid, string AggregateType, string Message);