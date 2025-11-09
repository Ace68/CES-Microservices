using BrewUp.Sales.Domain.Entities;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using Microsoft.Extensions.Logging;
using Muflone.Persistence;

namespace BrewUp.Sales.Domain.CommandHandlers;

public class CloseSalesOrderCommandHandler(IRepository repository,
    ILoggerFactory loggerFactory) : CommandHandlerBaseAsync<CloseSalesOrder>(repository, loggerFactory)
{
    protected override async Task ProcessCommand(CloseSalesOrder command, CancellationToken cancellationToken = default)
    {
        SalesOrder? aggregate = await Repository.GetByIdAsync<SalesOrder>(command.AggregateId, cancellationToken);
        aggregate!.CloseOrder(command.SalesOrderDeliveryDate, command.MessageId);
        await Repository.SaveAsync(aggregate, Guid.NewGuid(), cancellationToken);
    }
}