using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.Exceptions;
using Muflone.Messages.Events;

namespace BrewUp.Sales.SharedKernel.Messages.Events;

public sealed class SalesOrderExceptionRaised(SalesOrderId aggregateId,
    BrewUpAggregateException aggregateException, Guid correlationId) : DomainEvent(aggregateId, correlationId)
{
    public BrewUpAggregateException AggregateException { get; private set; } = aggregateException;
}