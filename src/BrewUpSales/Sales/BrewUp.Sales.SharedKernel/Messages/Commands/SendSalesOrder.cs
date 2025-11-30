using BrewUp.Sales.SharedKernel.CustomTypes;
using Muflone.Messages.Commands;

namespace BrewUp.Sales.SharedKernel.Messages.Commands;

public sealed class SendSalesOrder(
    SalesOrderId aggregateId,
    SalesOrderDeliveryDate salesOrderDeliveryDate,
    Guid correlationId) : Command(aggregateId, correlationId)
{
    public SalesOrderDeliveryDate SalesOrderDeliveryDate { get; private set; } = salesOrderDeliveryDate;
}
 