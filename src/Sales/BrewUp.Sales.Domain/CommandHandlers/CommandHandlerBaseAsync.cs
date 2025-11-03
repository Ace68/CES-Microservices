using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.Persistence;

namespace BrewUp.Sales.Domain.CommandHandlers;

public abstract class CommandHandlerBaseAsync<TCommand>(IRepository repository, ILoggerFactory loggerFactory)
    : CommandHandlerAsync<TCommand>(repository, loggerFactory)
    where TCommand : class, ICommand
{
    public override async Task HandleAsync(TCommand command, CancellationToken cancellationToken = new())
    {
        try
        {
            Logger.LogInformation(
                $"Processing command: {command.GetType()} - Aggregate: {command.AggregateId} - CommandId : {command.MessageId}");
            await ProcessCommand(command, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(
                $"Error processing command: {command.GetType()} - Aggregate: {command.AggregateId} - CommandId : {command.MessageId} - Messagge: {e.Message} - Stack Trace {e.StackTrace}");
            throw;
        }
    }

    public abstract Task ProcessCommand(TCommand command, CancellationToken cancellationToken = default);
}