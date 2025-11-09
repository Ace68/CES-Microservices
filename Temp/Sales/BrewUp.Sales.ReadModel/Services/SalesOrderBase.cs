using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.Services;

public abstract class SalesOrderBase
{
    protected readonly IPersister Persister;
    protected readonly ILogger Logger;

    protected SalesOrderBase(ILoggerFactory loggerFactory, 
        [FromKeyedServices("sales")] IPersister persister)
    {
        Persister = persister;
        Logger = loggerFactory.CreateLogger(GetType());
    }
}