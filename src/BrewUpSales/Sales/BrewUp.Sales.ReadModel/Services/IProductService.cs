using BrewUp.Sales.SharedKernel.CustomTypes;

namespace BrewUp.Sales.ReadModel.Services;

public interface IProductService
{
    Task CreateProductReadModelAsync(ProductId productId, ProductName productName,
        ProductDescription productDescription, ProductType productType, CancellationToken cancellationToken);
}