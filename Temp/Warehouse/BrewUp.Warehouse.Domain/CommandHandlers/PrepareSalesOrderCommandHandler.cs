using BrewUp.Warehouse.Entities.Entities;
using BrewUp.Warehouse.SharedKernel.CustomTypes;
using BrewUp.Warehouse.SharedKernel.Messages.Commands;
using Microsoft.Extensions.Logging;
using Muflone.Persistence;

namespace BrewUp.Warehouse.Domain.CommandHandlers;

public sealed class PrepareSalesOrderCommandHandler(IRepository repository, 
    ILoggerFactory loggerFactory) : CommandHandlerBaseAsync<PrepareSalesOrder>(repository, loggerFactory)
{
    public override async Task HandleAsync(PrepareSalesOrder command, CancellationToken cancellationToken = new ())
    {
        // Product aggregate = await repository.GetByIdAsync(command.AggregateId, cancellationToken);
        // aggregate.PrepareSalesOrder((SalesOrderId) command.AggregateId, command.SalesOrderNumber, command.Rows,
        //     command.MessageId);
        // await repository.SaveAsync(aggregate, Guid.NewGuid(), cancellationToken);
    }
}