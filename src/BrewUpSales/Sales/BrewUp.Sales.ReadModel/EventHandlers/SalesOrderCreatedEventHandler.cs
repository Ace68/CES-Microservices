using BrewUp.Sales.ReadModel.Services;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.EventHandlers;

public sealed class SalesOrderCreatedEventHandler(
    ISalesOrderService salesOrderService,
    ILoggerFactory loggerFactory) 
    : DomainEventHandlerBaseAsync<SalesOrderCreated>(loggerFactory)
{
    public override async Task HandleAsync(SalesOrderCreated @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await salesOrderService.CreateSalesOrderReadModelAsync((SalesOrderId) @event.AggregateId,
            @event.SalesOrderNumber, @event.SalesOrderDate, @event.CustomerId, @event.CustomerName,
            @event.SalesOrderDeliveryDate, @event.Rows, cancellationToken);
    }
}