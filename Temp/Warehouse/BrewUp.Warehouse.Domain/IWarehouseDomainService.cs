using BrewUp.Shared.ExternalContracts;
using BrewUp.Warehouse.SharedKernel.CustomTypes;

namespace BrewUp.Warehouse.Domain;

public interface IWarehouseDomainService
{
    Task<string> CreateProductAsync(ProductId productId, ProductName productName, ProductDescription productDescription,
        ProductType productType, IEnumerable<AvailabilityJson> availabilities, CancellationToken cancellationToken);
}