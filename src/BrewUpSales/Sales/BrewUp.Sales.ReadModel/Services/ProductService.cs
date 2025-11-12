using BrewUp.Sales.Entities.Dtos;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.Exceptions;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.Services;

internal sealed class ProductService(
    IBrewUpPersister<Product> productRepository,
    ILoggerFactory loggerFactory) : IProductService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ProductService>();
    
    public async Task CreateProductReadModelAsync(ProductId productId, ProductName productName, ProductDescription productDescription,
        ProductType productType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Product entity = Product.Create(productId, productName, productDescription, productType);
            await productRepository.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            UtilitiesService.LogError(ex, _logger);
        }
    }
}