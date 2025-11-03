using BrewUp.Sales.Domain.Entities;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using Microsoft.Extensions.Logging;
using Muflone.Persistence;

namespace BrewUp.Sales.Domain.CommandHandlers;

public sealed class CreateSalesOrderCommandHandler(IRepository repository,
    ILoggerFactory loggerFactory) : CommandHandlerBaseAsync<CreateSalesOrder>(repository, loggerFactory)
{
    public override async Task ProcessCommand(CreateSalesOrder command, CancellationToken cancellationToken = default)
    {
        SalesOrder aggregate = SalesOrder.CreateSalesOrder((SalesOrderId) command.AggregateId, command.SalesOrderNumber,
            command.SalesOrderDate, command.CustomerId, command.CustomerName, command.SalesOrderDeliveryDate,
            command.Rows, command.MessageId);
        await repository.SaveAsync(aggregate, Guid.NewGuid(), cancellationToken);
    }
}