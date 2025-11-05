using BrewUp.Shared.ExternalContracts;
using BrewUp.Warehouse.Domain;

namespace BrewUp.Warehouse.Facade;

internal class WarehouseFacade(IWarehouseDomainService warehouseDomainService) : IWarehouseFacade
{
    public async Task<string> CreateProductAsync(CreateProductJson body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        return await warehouseDomainService.CreateProductAsync(
            new SharedKernel.CustomTypes.ProductId(Guid.NewGuid().ToString()),
            new SharedKernel.CustomTypes.ProductName(body.ProductName),
            new SharedKernel.CustomTypes.ProductDescription(body.ProductDescription),
            new SharedKernel.CustomTypes.ProductType(body.ProductType),
            body.Availabilities,
            cancellationToken);
    }
}