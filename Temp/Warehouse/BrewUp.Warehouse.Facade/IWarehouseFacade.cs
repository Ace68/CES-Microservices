using BrewUp.Shared.ExternalContracts;

namespace BrewUp.Warehouse.Facade;

public interface IWarehouseFacade
{
    Task<string> CreateProductAsync(CreateProductJson body, CancellationToken cancellationToken);
}