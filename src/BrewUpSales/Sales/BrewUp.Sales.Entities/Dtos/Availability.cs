using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Sales.Entities.Dtos;

public class Availability : DtoBase
{
    public string ProductId { get; private set; } = string.Empty;
    public string WarehouseReference { get; private set; } = string.Empty;
    public double Quantity { get; private set; }
    
    public virtual Product Product { get; private set; } = null!;
    
    protected Availability()
    {}

    public static Availability Create(ProductId productId, WarehouseReference warehouseReference, ProductQuantity quantity)
    {
        return new Availability(productId, warehouseReference, quantity);
    }
    
    private Availability(ProductId productId, WarehouseReference warehouseReference, ProductQuantity quantity)
    {
        Id = Guid.NewGuid().ToString();
        
        ProductId = productId.Value;
        WarehouseReference = warehouseReference.Value;
        Quantity = quantity.Quantity;
    }
    
    internal void UpdateAvailability(ProductQuantity quantity)
    {
        Quantity = quantity.Quantity;
    }
}