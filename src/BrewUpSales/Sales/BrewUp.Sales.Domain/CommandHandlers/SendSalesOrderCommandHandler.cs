using BrewUp.Sales.Domain.Entities;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using Microsoft.Extensions.Logging;
using Muflone.Persistence;

namespace BrewUp.Sales.Domain.CommandHandlers;

public class SendSalesOrderCommandHandler(IRepository repository,
    ILoggerFactory loggerFactory) : CommandHandlerBaseAsync<SendSalesOrder>(repository, loggerFactory)
{
    protected override async Task ProcessCommand(SendSalesOrder command, CancellationToken cancellationToken = default)
    {
        SalesOrder? aggregate = await Repository.GetByIdAsync<SalesOrder>(command.AggregateId, cancellationToken);
        aggregate!.SendOrder(command.SalesOrderDeliveryDate, command.MessageId);
        await Repository.SaveAsync(aggregate, Guid.NewGuid(), cancellationToken);
    }
}