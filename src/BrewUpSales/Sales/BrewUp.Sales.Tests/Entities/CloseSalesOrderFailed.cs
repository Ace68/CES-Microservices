using BrewUp.Sales.Domain.CommandHandlers;
using BrewUp.Sales.Domain.Entities;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using BrewUp.Sales.SharedKernel.Messages.Events;
using BrewUp.Shared.Exceptions;
using BrewUp.Shared.ExternalContracts;
using Microsoft.Extensions.Logging.Abstractions;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.SpecificationTests;

namespace BrewUp.Sales.Tests.Entities;

public sealed class CloseSalesOrderFailed : CommandSpecification<CloseSalesOrder>
{
    private readonly SalesOrderId _salesOrderId = SalesOrderId.New();
    private readonly SalesOrderNumber _salesOrderNumber = new("SO-001");
    private readonly SalesOrderDate _salesOrderDate = new(DateTime.UtcNow);
    private readonly CustomerId _customerId = CustomerId.New();
    private readonly CustomerName _customerName = new("Il Grottino del Muflone");
    private readonly SalesOrderDeliveryDate _salesOrderDeliveryDate = new(DateTime.UtcNow.AddDays(7));
    private readonly IEnumerable<SalesOrderRowJson> _rows = [];
    
    private readonly Guid _correlationId = Guid.NewGuid();
    
    protected override IEnumerable<DomainEvent> Given()
    {
        yield return new SalesOrderCreated(_salesOrderId, _salesOrderNumber, _salesOrderDate, _customerId,
            _customerName, _salesOrderDeliveryDate, _rows.ToArray(), _correlationId);
        yield return new SalesOrderClosed(_salesOrderId, _salesOrderDeliveryDate, _correlationId);
    }
    
    protected override CloseSalesOrder When() => new (_salesOrderId, _salesOrderNumber,
        _salesOrderDeliveryDate, _rows, _correlationId);

    protected override ICommandHandlerAsync<CloseSalesOrder> OnHandler() =>
        new CloseSalesOrderCommandHandler(Repository, new NullLoggerFactory());

    protected override IEnumerable<DomainEvent> Expect()
    {
        yield return new SalesOrderExceptionRaised(_salesOrderId,
            new BrewUpAggregateException(_salesOrderId.Value, typeof(SalesOrder).FullName!, "Order Already Closed!"),
            _correlationId);
    }
}