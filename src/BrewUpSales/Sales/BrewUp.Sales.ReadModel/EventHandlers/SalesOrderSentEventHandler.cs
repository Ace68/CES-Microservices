using BrewUp.Sales.ReadModel.Services;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.EventHandlers;

public sealed class SalesOrderSentEventHandler(
    ISalesOrderService salesOrderService,
    ILoggerFactory loggerFactory) 
    : DomainEventHandlerBaseAsync<SalesOrderSent>(loggerFactory)
{
    public override async Task HandleAsync(SalesOrderSent @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();

        await salesOrderService.SendSalesOrderAsync((SalesOrderId) @event.AggregateId, @event.SalesOrderDeliveryDate, cancellationToken);
    }
}