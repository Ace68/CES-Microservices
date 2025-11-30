using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using BrewUp.Shared.ExternalContracts;
using Muflone.Persistence;

namespace BrewUp.Sales.Domain;

internal class SalesDomainService(IServiceBus serviceBus) : ISalesDomainService
{
    public async Task<string> CreateSalesOrderAsync(CreateSalesOrderJson body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var salesOrderId = Guid.NewGuid().ToString();
        
        CreateSalesOrder command = new CreateSalesOrder(new SalesOrderId(salesOrderId),
            new SalesOrderNumber(body.OrderNumber),
            new SalesOrderDate(DateTime.UtcNow),
            new CustomerId(body.CustomerId),
            new CustomerName(body.CustomerName),
            new SalesOrderDeliveryDate(body.DeliveryDate),
            body.Rows, Guid.NewGuid());

        await serviceBus.SendAsync(command, cancellationToken);
        
        return salesOrderId;
    }

    public async Task UpdateSalesOrderAsync(string orderId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CloseSalesOrder command = new(new SalesOrderId(orderId), new SalesOrderNumber("123"),
            new SalesOrderDeliveryDate(DateTime.UtcNow), [], Guid.NewGuid());
        await serviceBus.SendAsync(command, cancellationToken);
    }

    public async Task SendSalesOrderAsync(string orderId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SendSalesOrder command = new(new SalesOrderId(orderId),
            new SalesOrderDeliveryDate(DateTime.UtcNow), Guid.NewGuid());
        await serviceBus.SendAsync(command, cancellationToken);
    }
}