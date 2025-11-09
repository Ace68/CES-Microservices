using BrewUp.Sales.ReadModel.Services;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.EventHandlers;

public sealed class ProductCreatedEventHandler(IProductService productService, 
    ILoggerFactory loggerFactory)
    : IntegrationEventHandlerBaseAsync<ProductCreated>(loggerFactory)
{
    public override async Task HandleAsync(ProductCreated @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();

        await productService.CreateProductReadModelAsync((ProductId) @event.AggregateId, @event.ProductName,
            @event.ProductDescription, @event.ProductType, cancellationToken);
    }
}