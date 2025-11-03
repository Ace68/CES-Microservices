using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using BrewUp.Shared.Domain;
using Microsoft.Extensions.Logging;
using Muflone.Persistence;

namespace BrewUp.Sales.Domain.CommandHandlers;

public class CloseSalesOrderCommandHandler(IRepository repository,
    ILoggerFactory loggerFactory) : CommandHandlerBaseAsync<CloseSalesOrder>(repository, loggerFactory)
{
    public override Task ProcessCommand(CloseSalesOrder command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}