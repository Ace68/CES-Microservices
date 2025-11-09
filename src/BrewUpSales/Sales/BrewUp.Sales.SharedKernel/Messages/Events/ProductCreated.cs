using BrewUp.Sales.SharedKernel.CustomTypes;
using Muflone.Messages.Events;

namespace BrewUp.Sales.SharedKernel.Messages.Events;

public sealed class ProductCreated(ProductId aggregateId, 
    ProductName productName,
    ProductDescription productDescription,
    ProductType productType) : IntegrationEvent(aggregateId)
{
    public ProductName ProductName { get; private set; } = productName;
    public ProductDescription ProductDescription { get; private set; } = productDescription;
    public ProductType ProductType { get; private set; } = productType;
}