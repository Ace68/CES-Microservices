using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.Domain;
using BrewUp.Shared.ExternalContracts;

namespace BrewUp.Sales.Entities.Dtos;

public class Product : BrewUpAggregateRoot
{
    public string ProductName { get; private set; } = string.Empty;
    public string ProductDescription { get; private set; } = string.Empty;
    public string ProductType { get; private set; } = string.Empty;

    public virtual ICollection<Availability> Availabilities { get; private set; } = [];
    
    protected Product()
    {}

    public static Product Create(ProductId productId, ProductName productName, ProductDescription productDescription,
        ProductType productType)
    {
        return new Product(productId, productName, productDescription, productType);
    }

    private Product(ProductId productId, ProductName productName, ProductDescription productDescription,
        ProductType productType)
    {
        Id = productId.Value;
        ProductName = productName.Value;
        ProductDescription = productDescription.Value;
        ProductType = productType.Value;
    }
    
    public void AddAvailability(AvailabilityJson newAvailability)
    {
        Availability availability = Availability.Create(
            new ProductId(Id),
            new WarehouseReference(newAvailability.Reference),
            new ProductQuantity(newAvailability.Quantity, "Bottles"));
        Availabilities.Add(availability);
    }
}