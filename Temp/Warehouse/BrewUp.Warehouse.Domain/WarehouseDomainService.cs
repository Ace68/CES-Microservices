using BrewUp.Shared.Domain;
using BrewUp.Shared.Exceptions;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Warehouse.Entities.Entities;
using BrewUp.Warehouse.SharedKernel.CustomTypes;
using Microsoft.Extensions.Logging;

namespace BrewUp.Warehouse.Domain;

internal sealed class WarehouseDomainService(IBrewUpRepository<Product> productRepository,
    ILoggerFactory loggerFactory) : IWarehouseDomainService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<WarehouseDomainService>();

    public async Task<string> CreateProductAsync(ProductId productId, ProductName productName,
        ProductDescription productDescription, ProductType productType, IEnumerable<AvailabilityJson> availabilities,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var product = Product.Create(productId, productName, productDescription, productType);
            
            foreach (var availability in availabilities)
            {
                product.AddAvailability(availability);
            }
            
            await productRepository.AddAsync(product, cancellationToken);
            
            return product.Id;
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
            return string.Empty;
        }
    }
}