using BrewUp.Sales.ReadModel.Services;
using BrewUp.Sales.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.EventHandlers;

public sealed class SalesOrderCreatedForProductsEventHandler(
    ISalesOrderService salesOrderService,
    ILoggerFactory loggerFactory) 
    : DomainEventHandlerBaseAsync<SalesOrderCreated>(loggerFactory)
{
    public override Task HandleAsync(SalesOrderCreated @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // Update SalesFroProduct read model
        
        return Task.CompletedTask;
    }
}