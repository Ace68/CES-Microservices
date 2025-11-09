using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Sales.ReadModel.Services;

public interface ISalesOrderService
{
    Task<PagedResult<SalesOrderJson>> GetSalesOrdersAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task CreateSalesOrderReadModelAsync(SalesOrderId salesOrderId, SalesOrderNumber salesOrderNumber,
        SalesOrderDate salesOrderDate, CustomerId customerId, CustomerName customerName,
        SalesOrderDeliveryDate salesOrderDeliveryDate, IEnumerable<SalesOrderRowJson> rows,
        CancellationToken cancellationToken);
}