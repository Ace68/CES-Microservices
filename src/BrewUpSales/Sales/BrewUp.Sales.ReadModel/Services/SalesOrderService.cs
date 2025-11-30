using BrewUp.Sales.Entities.Dtos;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.Exceptions;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.Services;

internal sealed class SalesOrderService(IQueries<SalesOrder> salesOrderQuery,
    IBrewUpPersister<SalesOrder> salesOrderRepository,
    ILoggerFactory loggerFactory) : ISalesOrderService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SalesOrderService>();
    
    public async Task<PagedResult<SalesOrderJson>> GetSalesOrdersAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        PagedResult<SalesOrder> orderResult = await salesOrderQuery.GetByFilterAsync(null, page, pageSize, cancellationToken);

        return orderResult.TotalRecords > 0
            ? new PagedResult<SalesOrderJson>(
                orderResult.Results.Select(s => s.ToJson()),
                orderResult.Page, orderResult.PageSize, orderResult.TotalRecords)
            : new PagedResult<SalesOrderJson>(
                new List<SalesOrderJson>(), 0, 0, 0);
    }

    public async Task CreateSalesOrderReadModelAsync(SalesOrderId salesOrderId, SalesOrderNumber salesOrderNumber,
        SalesOrderDate salesOrderDate, CustomerId customerId, CustomerName customerName,
        SalesOrderDeliveryDate salesOrderDeliveryDate, IEnumerable<SalesOrderRowJson> rows, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            await salesOrderRepository.AddAsync(SalesOrder.Create(salesOrderId, salesOrderNumber,
                salesOrderDate, customerId, customerName, salesOrderDeliveryDate, rows,
                Guid.NewGuid()), cancellationToken);
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
        }
    }

    public async Task SendSalesOrderAsync(SalesOrderId salesOrderId, SalesOrderDeliveryDate salesOrderDeliveryDate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var salesOrder = await salesOrderQuery.GetByIdAsync(salesOrderId.ToString(), cancellationToken);
            if (salesOrder == null || salesOrder.Id == string.Empty)
                throw new Exception($"Sales order with id {salesOrderId} not found in read model");
            
            salesOrder.SendSalesOrder(salesOrderDeliveryDate);
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
        }
    }
}